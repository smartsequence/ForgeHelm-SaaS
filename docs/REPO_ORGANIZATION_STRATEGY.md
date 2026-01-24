# 倉庫組織策略 - Agent 與 SaaS 分離開發

**文件版本：** v1.0  
**最後更新：** 2025-01-XX  
**作者：** DocEngine Development Team

---

## 一、問題分析

### 1.1 核心挑戰

1. **Agent 不是網站程式**：Agent 是獨立的 Console Application，應該分開管理
2. **需要通信**：Agent 和 SaaS 需要透過 SignalR 和 REST API 通信
3. **版本同步**：兩個版本需要共享通信協議，但又要獨立演進
4. **保密需求**：Agent 開發不能提前曝光

### 1.2 設計目標

- ✅ Agent 和 SaaS 分離開發，互不干擾
- ✅ 共享通信協議，確保兼容性
- ✅ 版本獨立管理，但可以同步更新
- ✅ Agent 代碼完全保密（私有倉庫）

---

## 二、推薦方案：分離倉庫 + 共享協議庫

### 2.1 倉庫結構

```
DocEngine-SaaS (公開/私有，SaaS 主倉庫)
├── main (已發佈版本，無 Agent)
├── with-agent (Agent 開發分支，私有)
└── hotfix/* (緊急修復)

DocEngine-Agent (私有倉庫，Agent 專案)
├── main (Agent 主分支)
├── feature/* (Agent 功能分支)
└── develop (Agent 開發分支)

DocEngine-Contracts (私有倉庫，共享協議庫)
├── main (協議定義主分支)
└── version/* (版本標記)
```

### 2.2 方案說明

#### 倉庫 1：DocEngine-SaaS (SaaS 主倉庫)
- **用途**：SaaS 平台代碼
- **可見性**：公開或私有（根據需求）
- **GitHub**：`https://github.com/smartsequence/DocEngine-SaaS`
- **分支**：
  - `main`：已發佈版本（無 Agent）
  - `with-agent`：包含 Agent 整合的開發分支（私有）
  - `hotfix/*`：緊急修復

#### 倉庫 2：DocEngine-Agent (Agent 專案)
- **用途**：Agent 應用程式代碼
- **可見性**：**完全私有**
- **GitHub**：`https://github.com/smartsequence/DocEngine-Agent`
- **分支**：
  - `main`：Agent 主分支
  - `feature/*`：Agent 功能分支

#### 倉庫 3：DocEngine-Contracts (共享協議庫)
- **用途**：定義 Agent 和 SaaS 之間的通信協議
- **可見性**：**私有**（包含 API 契約）
- **GitHub**：`https://github.com/smartsequence/DocEngine-Contracts`
- **內容**：
  - DTO（Data Transfer Objects）
  - API 介面定義
  - SignalR Hub 契約
  - 版本號定義

---

## 三、共享協議庫設計

### 3.1 協議庫結構

```
DocEngine-Contracts/
├── src/
│   ├── DocEngine.Contracts/
│   │   ├── Models/
│   │   │   ├── AnalysisTask.cs
│   │   │   ├── AnalysisResult.cs
│   │   │   ├── AgentStatus.cs
│   │   │   └── ...
│   │   ├── Api/
│   │   │   ├── IAgentApi.cs (REST API 介面)
│   │   │   └── ...
│   │   ├── SignalR/
│   │   │   ├── IAgentHub.cs (SignalR Hub 介面)
│   │   │   └── ...
│   │   └── DocEngine.Contracts.csproj
│   └── DocEngine.Contracts.Client/
│       └── (Agent 端使用的客戶端實現)
├── tests/
│   └── DocEngine.Contracts.Tests/
└── README.md
```

### 3.2 協議庫內容範例

#### Models/AnalysisTask.cs
```csharp
namespace DocEngine.Contracts.Models;

public class AnalysisTask
{
    public string TaskId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectPath { get; set; }
    public string DatabaseConnectionString { get; set; }
    public AnalysisType AnalysisType { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum AnalysisType
{
    Full,
    Incremental
}
```

#### Models/AnalysisResult.cs
```csharp
namespace DocEngine.Contracts.Models;

public class AnalysisResult
{
    public string TaskId { get; set; }
    public string ProjectId { get; set; }
    public List<CodeElement> CodeElements { get; set; }
    public List<DatabaseObject> DatabaseObjects { get; set; }
    public AnalysisMetadata Metadata { get; set; }
}
```

#### SignalR/IAgentHub.cs
```csharp
namespace DocEngine.Contracts.SignalR;

public interface IAgentHub
{
    // Agent 端實現的方法（SaaS 調用）
    Task StartAnalysis(AnalysisTask task);
    Task CancelAnalysis(string taskId);
    
    // SaaS 端實現的方法（Agent 調用）
    Task ReportProgress(string taskId, int progress);
    Task ReportStatus(string taskId, AgentStatus status);
    Task UploadResult(AnalysisResult result);
}
```

#### Api/IAgentApi.cs
```csharp
namespace DocEngine.Contracts.Api;

public interface IAgentApi
{
    Task<ApiResponse> UploadAnalysisResult(AnalysisResult result);
    Task<AgentStatusResponse> GetAgentStatus(string agentId);
}
```

### 3.3 協議庫版本管理

使用語義化版本（Semantic Versioning）：
- `v1.0.0`：初始版本
- `v1.1.0`：新增功能（向後兼容）
- `v2.0.0`：重大變更（可能不兼容）

---

## 四、整合方式

### 4.1 方案 A：NuGet 包（推薦）

#### 優點
- ✅ 標準化依賴管理
- ✅ 版本控制清晰
- ✅ 易於發布和更新
- ✅ 支援私有 NuGet Feed

#### 實作步驟

**1. 建立私有 NuGet Feed**
```bash
# 使用 GitHub Packages 或 Azure Artifacts
# 或自建 NuGet Server
```

**2. 發布協議庫**
```bash
cd DocEngine-Contracts
dotnet pack -c Release
dotnet nuget push DocEngine.Contracts.1.0.0.nupkg --source "私有Feed"
```

**3. SaaS 專案引用**
```xml
<!-- DocEngine-SaaS/DocEngine.csproj -->
<ItemGroup>
  <PackageReference Include="DocEngine.Contracts" Version="1.0.0" />
</ItemGroup>
```

**4. Agent 專案引用**
```xml
<!-- DocEngine-Agent/DocEngine.Agent.csproj -->
<ItemGroup>
  <PackageReference Include="DocEngine.Contracts" Version="1.0.0" />
</ItemGroup>
```

### 4.2 方案 B：Git Submodule

#### 優點
- ✅ 代碼同步即時
- ✅ 不需要發布流程
- ✅ 適合開發階段

#### 缺點
- ⚠️ 需要手動更新
- ⚠️ 版本管理較複雜

#### 實作步驟

**1. 在 SaaS 倉庫中添加 Submodule**
```bash
cd DocEngine-SaaS
git submodule add https://github.com/smartsequence/DocEngine-Contracts.git src/Contracts
git submodule update --init --recursive
```

**2. 在 Agent 倉庫中添加 Submodule**
```bash
cd DocEngine-Agent
git submodule add https://github.com/smartsequence/DocEngine-Contracts.git src/Contracts
git submodule update --init --recursive
```

**3. 更新 Submodule**
```bash
git submodule update --remote
```

### 4.3 方案 C：Git Subtree（不推薦）

Git Subtree 會將代碼複製到主倉庫，失去獨立性，不適合此場景。

---

## 五、工作流程

### 5.1 開發新功能（Agent 端）

```bash
# 1. 在 Agent 倉庫開發
cd DocEngine-Agent
git checkout -b feature/new-analysis-feature

# 2. 如果需要修改協議，先在 Contracts 倉庫修改
cd DocEngine-Contracts
git checkout -b feature/update-contracts
# ... 修改協議 ...
git commit -m "feat: 新增分析結果欄位"
git push origin feature/update-contracts

# 3. 發布新版本協議庫（NuGet）
dotnet pack -c Release
dotnet nuget push --source "私有Feed"

# 4. 在 Agent 專案中更新協議庫版本
cd DocEngine-Agent
# 更新 .csproj 中的版本號
dotnet restore
# ... 使用新協議開發功能 ...

# 5. 提交 Agent 代碼
git commit -m "feat: 使用新協議實現分析功能"
git push origin feature/new-analysis-feature
```

### 5.2 同步協議更新到 SaaS

```bash
# 1. 在 SaaS 倉庫中更新協議庫版本
cd DocEngine-SaaS
git checkout with-agent

# 2. 更新 NuGet 包版本（或更新 Submodule）
# NuGet 方式：
dotnet add package DocEngine.Contracts --version 1.1.0

# Submodule 方式：
git submodule update --remote
git add src/Contracts
git commit -m "chore: 更新協議庫到 v1.1.0"

# 3. 更新 SaaS 代碼以使用新協議
# ... 修改代碼 ...
git commit -m "feat: 支援新的分析結果格式"
```

### 5.3 融合兩個版本

```bash
# 1. 確保 with-agent 分支包含所有 Agent 整合
cd DocEngine-SaaS
git checkout with-agent
git pull origin with-agent

# 2. 合併到 main
git checkout main
git merge with-agent --no-ff -m "merge: 合併 Agent 功能到主分支"

# 3. 解決衝突（如果有）
# ... 解決衝突 ...

# 4. 測試
# ... 運行測試 ...

# 5. 發布
git tag -a v2.0.0 -m "Release: 包含 Agent 功能的版本"
git push origin main --tags
```

---

## 六、版本兼容性策略

### 6.1 協議版本兼容性

#### 向後兼容（Backward Compatible）
- ✅ 新增可選欄位
- ✅ 新增方法（不影響現有方法）
- ✅ 擴展枚舉（不刪除現有值）

#### 不兼容變更（Breaking Changes）
- ❌ 刪除欄位
- ❌ 修改欄位類型
- ❌ 刪除方法
- ❌ 修改方法簽名

### 6.2 版本號規則

使用語義化版本（SemVer）：
- **主版本號**：不兼容的 API 變更
- **次版本號**：向後兼容的功能新增
- **修訂版本號**：向後兼容的問題修復

範例：
- `v1.0.0` → `v1.0.1`：修復問題（兼容）
- `v1.0.0` → `v1.1.0`：新增功能（兼容）
- `v1.0.0` → `v2.0.0`：重大變更（不兼容）

### 6.3 多版本支援策略

如果需要在過渡期支援多個協議版本：

```csharp
// SaaS 端：支援多個版本
public class AgentHub : Hub, IAgentHub
{
    public async Task StartAnalysis(AnalysisTask task)
    {
        // 根據 Agent 版本選擇處理邏輯
        var agentVersion = await GetAgentVersion(Context.ConnectionId);
        if (agentVersion >= new Version("2.0.0"))
        {
            await HandleV2Task(task);
        }
        else
        {
            await HandleV1Task(task);
        }
    }
}
```

---

## 七、GitHub 設置建議

### 7.1 倉庫權限設置

#### DocEngine-SaaS (SaaS 主倉庫)
- **main 分支**：公開或私有（根據需求）
- **with-agent 分支**：**私有**（不設置為默認分支）
- **協作者**：開發團隊

#### DocEngine-Agent (Agent 專案)
- **所有分支**：**完全私有**
- **協作者**：僅 Agent 開發人員
- **不設置為公開**

#### DocEngine-Contracts (共享協議庫)
- **所有分支**：**私有**（包含 API 契約）
- **協作者**：SaaS 和 Agent 開發人員
- **GitHub Packages**：設置為私有 NuGet Feed

### 7.2 GitHub Packages 設置（NuGet）

**1. 創建 GitHub Personal Access Token**
- 權限：`write:packages`, `read:packages`

**2. 配置 NuGet.config**
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="github" value="https://nuget.pkg.github.com/smartsequence/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <github>
      <add key="Username" value="smartsequence" />
      <add key="ClearTextPassword" value="YOUR_TOKEN" />
    </github>
  </packageSourceCredentials>
</configuration>
```

**3. 發布協議庫**
```bash
dotnet pack -c Release
dotnet nuget push DocEngine.Contracts.1.0.0.nupkg \
  --source "github" \
  --api-key YOUR_TOKEN
```

---

## 八、最佳實踐

### 8.1 協議設計原則

1. **向後兼容優先**
   - 盡量保持向後兼容
   - 重大變更時升級主版本號

2. **明確的版本標記**
   - 每個協議變更都要標記版本
   - 使用 Git Tag 標記發布版本

3. **文檔同步**
   - 協議變更時更新文檔
   - 提供遷移指南

### 8.2 開發流程建議

1. **協議先行**
   - 新功能開發前，先定義協議
   - 確保 Agent 和 SaaS 都理解協議

2. **版本同步**
   - 定期同步協議庫版本
   - 避免版本分叉過遠

3. **測試優先**
   - 協議變更時，兩端都要測試
   - 使用集成測試驗證兼容性

### 8.3 安全考量

1. **私有倉庫**
   - Agent 代碼完全私有
   - 協議庫也設為私有（包含 API 契約）

2. **訪問控制**
   - 限制協議庫的訪問權限
   - 只有授權人員可以修改協議

3. **版本追蹤**
   - 所有協議變更都要記錄
   - 使用 Git Tag 標記發布版本

---

## 九、實際操作範例

### 9.1 創建三個倉庫

```bash
# 1. 創建 Contracts 倉庫（私有）
gh repo create DocEngine-Contracts --private

# 2. 創建 Agent 倉庫（私有）
gh repo create DocEngine-Agent --private

# 3. SaaS 倉庫已存在（DocEngine）
# 確保 develop-agent 分支是私有的
```

### 9.2 初始化 Contracts 專案

```bash
# 1. Clone Contracts 倉庫
git clone https://github.com/smartsequence/DocEngine-Contracts.git
cd DocEngine-Contracts

# 2. 創建 .NET 類庫專案
dotnet new classlib -n DocEngine.Contracts
cd DocEngine.Contracts

# 3. 添加必要的 NuGet 包
dotnet add package Microsoft.AspNetCore.SignalR

# 4. 創建協議定義（參考上面的範例）
# ... 創建 Models、Api、SignalR 等 ...

# 5. 提交
git add .
git commit -m "feat: 初始協議庫定義"
git push origin main

# 6. 發布第一個版本
git tag -a v1.0.0 -m "Release: 初始協議版本"
git push origin v1.0.0
```

### 9.3 SaaS 專案引用協議庫

```bash
# 1. 在 SaaS 專案中添加 NuGet 源
dotnet nuget add source https://nuget.pkg.github.com/smartsequence/index.json \
  --name github \
  --username smartsequence \
  --password YOUR_TOKEN \
  --store-password-in-clear-text

# 2. 添加協議庫引用
cd DocEngine
dotnet add package DocEngine.Contracts --version 1.0.0 --source github

# 3. 使用協議庫
# ... 在代碼中使用 DocEngine.Contracts ...
```

### 9.4 Agent 專案引用協議庫

```bash
# 1. Clone Agent 倉庫
git clone https://github.com/smartsequence/DocEngine-Agent.git
cd DocEngine-Agent

# 2. 創建 Agent 專案
dotnet new console -n DocEngine.Agent
cd DocEngine.Agent

# 3. 添加協議庫引用
dotnet add package DocEngine.Contracts --version 1.0.0 --source github

# 4. 使用協議庫
# ... 在代碼中使用 DocEngine.Contracts ...
```

---

## 十、常見問題

### Q1: 如果協議變更導致不兼容怎麼辦？

A: 
1. 升級主版本號（如 `v1.0.0` → `v2.0.0`）
2. 在過渡期支援多個版本
3. 提供遷移指南和工具

### Q2: 如何確保 Agent 和 SaaS 使用相同版本的協議？

A: 
1. 使用 NuGet 包管理，版本號明確
2. 在 CI/CD 中檢查版本一致性
3. 協議庫變更時，通知兩端開發人員

### Q3: 協議庫可以公開嗎？

A: 
不建議。協議庫包含 API 契約，可能洩露系統架構。建議設為私有。

### Q4: 如果不想使用 NuGet，有其他方案嗎？

A: 
可以使用 Git Submodule，但需要手動管理版本。NuGet 是更標準的做法。

---

## 十一、總結

### 推薦方案

**分離倉庫 + NuGet 協議庫**：

1. ✅ **DocEngine-SaaS**：SaaS 主倉庫（公開/私有）
2. ✅ **DocEngine-Agent**：Agent 專案（完全私有）
3. ✅ **DocEngine-Contracts**：共享協議庫（私有 NuGet 包）

### 優勢

- ✅ 完全分離：Agent 代碼完全保密
- ✅ 版本管理：清晰的版本控制
- ✅ 易於維護：標準化的依賴管理
- ✅ 靈活擴展：可以獨立演進

### 下一步

1. 創建三個倉庫
2. 初始化協議庫
3. 在 SaaS 和 Agent 專案中引用協議庫
4. 開始開發

---

**文件結束**
