# ForgeHelm 部署架構設計文件

**文件版本：** v1.0  
**最後更新：** 2024-12-XX  
**作者：** ForgeHelm Development Team

---

## 一、部署架構概述

### 1.1 架構選擇：混合式架構（支援內網部署）

採用 **SaaS 平台 + 輕量級 Agent** 的混合式架構，平衡安全性、便利性和功能完整性。

**重要說明**：根據實際部署環境，SaaS 平台可以部署在：
- **雲端**：適用於可連外網的環境
- **客戶內網**：適用於完全內網隔離的環境（**更常見**）

### 1.2 部署場景分類

#### 場景 A：內網部署（推薦，更常見）
```
客戶內網環境
├─ SaaS 平台（部署在客戶內網伺服器）
│  ├─ ForgeHelm Web App
│  ├─ PostgreSQL
│  ├─ 內網 AI Server
│  └─ Git Repository
├─ Agent（安裝在開發人員電腦）
└─ 客戶專案和資料庫（內網）
```

#### 場景 B：雲端部署
```
雲端環境
├─ SaaS 平台（雲端）
│  ├─ ForgeHelm Web App
│  ├─ PostgreSQL
│  ├─ OpenAI API 或 內網 AI Server（透過 VPN）
│  └─ Git Repository
└─ Agent（安裝在客戶端，透過 VPN 或專線連接）
```

### 1.3 架構圖（內網部署場景）

```
┌─────────────────────────────────────────────────────────────┐
│              SaaS 平台 (內網部署 - 客戶內網伺服器)            │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  ForgeHelm Web Application (ASP.NET Core)          │   │
│  │  - 專案管理 UI                                       │   │
│  │  - 文件管理 UI                                       │   │
│  │  - 任務排程                                          │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  PostgreSQL 資料庫                                   │   │
│  │  - 專案元資料                                         │   │
│  │  - 程式碼元素                                         │   │
│  │  - 資料庫物件                                         │   │
│  │  - 文件版本記錄                                       │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  AI 服務整合層                                        │   │
│  │  - 內網 AI Server (HTTP API)                         │   │
│  │  - OpenAI API (可選，需 VPN 或代理)                  │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  文件版控系統                                         │   │
│  │  - Git Repository                                    │   │
│  │  - 版本管理                                           │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  SaaS Document API 整合                              │   │
│  │  - 文件列表 API                                       │   │
│  │  - 版本查詢 API                                       │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────┬───────────────────────────────────────┘
                       │
                       │ HTTP/HTTPS REST API (內網)
                       │ SignalR WebSocket (雙向通訊)
                       │ (認證：API Key / OAuth)
                       │
┌──────────────────────▼───────────────────────────────────────┐
│              客戶端 Agent (ForgeHelm.Agent.exe)              │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  程式碼分析器 (Code Analyzer)                         │   │
│  │  - Roslyn 分析 C# 程式碼                             │   │
│  │  - 解析 ASP.NET WebForm 專案                          │   │
│  │  - 提取類別、方法、驗證規則                           │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  資料庫分析器 (Database Analyzer)                    │   │
│  │  - 連接 Oracle 資料庫                                 │   │
│  │  - 提取表結構、約束、索引                             │   │
│  │  - 分析預存程序、觸發器                               │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  本地快取與暫存                                       │   │
│  │  - SQLite 本地資料庫                                  │   │
│  │  - 分析結果暫存                                       │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  SaaS 平台通訊模組                                    │   │
│  │  - REST API Client (上傳分析結果)                    │   │
│  │  - SignalR Client (接收任務指令)                      │   │
│  │  - 本地 HTTP 服務 (接收 SaaS 觸發請求)                │   │
│  └──────────────────────────────────────────────────────┘   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐   │
│  │  本地 HTTP 服務 (Local HTTP Server)                  │   │
│  │  - 監聽 localhost:8888                               │   │
│  │  - 接收 SaaS 平台的觸發請求                           │   │
│  │  - 啟動分析任務                                       │   │
│  └──────────────────────────────────────────────────────┘   │
└──────────────────────┬───────────────────────────────────────┘
                       │
         ┌─────────────┴─────────────┐
         │                           │
         ▼                           ▼
┌──────────────┐          ┌──────────────┐
│ 客戶專案程式碼 │          │ Oracle 資料庫 │
│ (ASP.NET      │          │ (內網環境)    │
│  WebForm)     │          │              │
└──────────────┘          └──────────────┘
```

---

## 二、核心組件說明

### 2.1 SaaS 平台組件

#### 2.1.1 ForgeHelm Web Application
- **技術棧**: ASP.NET Core MVC (現有專案擴展)
- **功能**:
  - 專案管理介面
  - 文件生成任務管理
  - 文件版本瀏覽
  - 與 SaaS Document 功能整合
- **部署方式**: 
  - IIS / Azure App Service
  - Docker Container (可選)

#### 2.1.2 PostgreSQL 資料庫
- **用途**: 儲存分析結果的結構化資料
- **主要資料表**:
  - `projects` - 專案資訊
  - `code_elements` - 程式碼元素
  - `database_objects` - 資料庫物件
  - `documents` - 文件記錄
  - `document_versions` - 文件版本
  - `analysis_tasks` - 分析任務

#### 2.1.3 AI 服務整合層
- **策略模式實作**: 自動偵測環境，切換 AI 服務
- **外網環境**: OpenAI API
- **內網環境**: 內網 AI Server (HTTP API)

#### 2.1.4 文件版控系統
- **技術**: Git Repository (LibGit2Sharp)
- **功能**: 
  - 自動提交文件版本
  - 版本標籤管理
  - 變更記錄

### 2.2 客戶端 Agent 組件

#### 2.2.1 ForgeHelm.Agent.exe
- **技術棧**: .NET 9.0 Console Application
- **執行模式**:
  - **模式一**: 手動執行（一次性分析）
  - **模式二**: 排程執行（Windows Task Scheduler）
  - **模式三**: 常駐服務（可選，Windows Service）

#### 2.2.2 程式碼分析器
- **技術**: Microsoft.CodeAnalysis.CSharp (Roslyn)
- **功能**:
  - 解析 .csproj 檔案
  - 分析 .aspx, .cs, .ascx 檔案
  - 提取類別、方法、屬性
  - 識別驗證規則（DataAnnotations, Custom Validation）
  - 分析業務邏輯流程

#### 2.2.3 資料庫分析器
- **技術**: Oracle.ManagedDataAccess.Core
- **功能**:
  - 連接 Oracle 資料庫
  - 查詢系統表（ALL_TABLES, ALL_COLUMNS 等）
  - 提取表結構、主鍵、外鍵
  - 分析索引、觸發器、預存程序

#### 2.2.4 本地快取
- **技術**: SQLite
- **用途**: 
  - 暫存分析結果
  - 增量分析支援
  - 離線分析支援

---

## 三、工作流程

### 3.1 完整分析流程

#### 流程 A：從 SaaS 按鈕觸發（推薦）

```
1. 使用者在 SaaS 平台建立專案
   └─> 產生專案 ID 和 API Key

2. 客戶端啟動 ForgeHelm.Agent.exe（作為本地服務）
   ├─> Agent 啟動本地 HTTP 服務（localhost:8888）
   ├─> Agent 連接到 SaaS 平台的 SignalR Hub
   ├─> Agent 註冊到 SaaS 平台（告知 Agent 已就緒）
   └─> Agent 等待任務指令

3. 使用者在 SaaS 平台點擊「開始分析」按鈕
   ├─> SaaS 平台透過 SignalR 發送任務到 Agent
   │   └─> 或透過 HTTP POST 到 Agent 本地服務
   └─> Agent 收到任務，開始分析

4. Agent 執行分析
   ├─> 程式碼分析器掃描專案
   ├─> 資料庫分析器連接 Oracle
   ├─> 透過 SignalR 即時回報進度到 SaaS
   └─> 將結果暫存到本地 SQLite

5. Agent 上傳分析結果到 SaaS 平台
   ├─> POST /api/analysis/upload
   ├─> 傳送專案 ID、分析結果 JSON
   └─> SaaS 平台儲存到 PostgreSQL

6. SaaS 平台觸發文件生成任務
   ├─> 從 PostgreSQL 讀取分析結果
   ├─> 呼叫內網 AI Server 生成文件內容
   ├─> 使用 DocumentFormat.OpenXml 生成 Word
   └─> 提交到 Git 版控

7. 文件版本同步到 SaaS Document
   └─> 透過 API 更新文件列表
```

#### 流程 B：手動執行 Agent（備用方案）

```
1. 使用者在 SaaS 平台建立專案
   └─> 產生專案 ID 和 API Key

2. 客戶端手動執行 ForgeHelm.Agent.exe
   ├─> 輸入專案 ID 和 API Key
   ├─> 指定專案路徑和資料庫連線字串
   └─> 開始分析

3-7. 同流程 A 的步驟 4-7
```

### 3.2 Agent 執行模式

#### 模式一：本地服務模式（推薦，支援 SaaS 觸發）

Agent 作為本地 HTTP 服務運行，同時連接 SignalR，等待 SaaS 平台觸發。

```bash
# 啟動 Agent 作為本地服務
ForgeHelm.Agent.exe --mode service --port 8888 --saas-url "http://internal-docengine:5000"
```

**工作流程**：
1. Agent 啟動本地 HTTP 服務（`http://localhost:8888`）
2. Agent 連接到 SaaS 平台的 SignalR Hub
3. Agent 註冊到 SaaS 平台，告知 Agent 已就緒
4. 使用者在 SaaS 平台點擊「開始分析」
5. SaaS 透過 SignalR 或 HTTP POST 觸發 Agent
6. Agent 執行分析並回報進度

#### 模式二：手動執行（一次性分析）
```bash
ForgeHelm.Agent.exe --project-id "xxx" --api-key "yyy" --project-path "C:\Projects\MyApp" --db-connection "Data Source=..."
```

#### 模式三：排程執行
```xml
<!-- Windows Task Scheduler XML -->
<Task>
  <Triggers>
    <CalendarTrigger>
      <ScheduleByDay>
        <DaysInterval>1</DaysInterval>
      </ScheduleByDay>
    </CalendarTrigger>
  </Triggers>
  <Actions>
    <Exec>
      <Command>ForgeHelm.Agent.exe</Command>
      <Arguments>--project-id "xxx" --api-key "yyy" ...</Arguments>
    </Exec>
  </Actions>
</Task>
```

#### 模式四：Windows Service（可選）
- 安裝為 Windows Service
- 常駐運行，自動連接 SignalR
- 收到任務後執行分析

---

## 四、API 設計

### 4.1 SaaS 平台觸發 Agent 的機制

#### 4.1.1 方案 A：SignalR 雙向通訊（推薦）

**優點**：
- 即時雙向通訊
- 自動重連機制
- 支援進度回報

**實作**：
```csharp
// SaaS 平台端：SignalR Hub
public class AgentHub : Hub
{
    public async Task SendAnalysisTask(string agentId, AnalysisTask task)
    {
        await Clients.Client(agentId).SendAsync("StartAnalysis", task);
    }
}

// Agent 端：SignalR Client
var connection = new HubConnectionBuilder()
    .WithUrl("http://internal-docengine:5000/agentHub")
    .Build();

connection.On<AnalysisTask>("StartAnalysis", async (task) => {
    await ExecuteAnalysis(task);
});

await connection.StartAsync();
```

#### 4.1.2 方案 B：Agent 本地 HTTP 服務

**優點**：
- 簡單直接
- 不需要 WebSocket 支援
- 防火牆友善

**實作**：
```csharp
// Agent 端：本地 HTTP 服務
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/analysis/start", async (AnalysisTask task) => {
    await ExecuteAnalysis(task);
    return Results.Ok(new { status = "started" });
});

app.Run("http://localhost:8888");
```

**SaaS 平台觸發**：
```csharp
// SaaS 平台端：觸發 Agent
var agentUrl = $"http://{agentIp}:8888/api/analysis/start";
var response = await httpClient.PostAsJsonAsync(agentUrl, task);
```

#### 4.1.3 方案 C：混合方案（最佳）

結合 SignalR 和本地 HTTP 服務：
- SignalR：用於即時通訊和進度回報
- 本地 HTTP：作為備用觸發機制

### 4.2 SaaS 平台 API

#### 4.2.1 觸發 Agent 分析任務
```
POST /api/agents/{agentId}/trigger
Headers:
  Authorization: Bearer {API_KEY}
  Content-Type: application/json

Body:
{
  "projectId": "guid",
  "projectPath": "C:\\Projects\\MyApp",
  "databaseConnection": "Data Source=...",
  "analysisType": "full|incremental"
}

Response:
{
  "success": true,
  "taskId": "guid",
  "message": "分析任務已發送到 Agent"
}
```

#### 4.2.2 上傳分析結果
```
POST /api/analysis/upload
Headers:
  Authorization: Bearer {API_KEY}
  Content-Type: application/json

Body:
{
  "projectId": "guid",
  "codeElements": [...],
  "databaseObjects": [...],
  "metadata": {
    "analyzedAt": "2024-12-01T10:00:00Z",
    "agentVersion": "1.0.0"
  }
}

Response:
{
  "success": true,
  "taskId": "guid",
  "message": "分析結果已接收，文件生成任務已建立"
}
```

#### 4.2.3 查詢任務狀態
```
GET /api/tasks/{taskId}
Headers:
  Authorization: Bearer {API_KEY}

Response:
{
  "taskId": "guid",
  "status": "completed|processing|failed",
  "progress": 75,
  "documentId": "guid",
  "documentVersion": "v1.0.0"
}
```

#### 4.2.4 下載文件
```
GET /api/documents/{documentId}/download
Headers:
  Authorization: Bearer {API_KEY}

Response: Word 檔案 (application/vnd.openxmlformats-officedocument.wordprocessingml.document)
```

### 4.3 Agent 本地 HTTP API

#### 4.3.1 接收分析任務
```
POST http://localhost:8888/api/analysis/start
Content-Type: application/json

Body:
{
  "taskId": "guid",
  "projectId": "guid",
  "projectPath": "C:\\Projects\\MyApp",
  "databaseConnection": "Data Source=...",
  "analysisType": "full|incremental"
}

Response:
{
  "success": true,
  "message": "分析任務已啟動"
}
```

#### 4.3.2 查詢 Agent 狀態
```
GET http://localhost:8888/api/status

Response:
{
  "status": "idle|analyzing|uploading",
  "currentTask": "guid",
  "progress": 50,
  "version": "1.0.0"
}
```

### 4.4 Agent 配置檔案

```json
{
  "SaaS": {
    "BaseUrl": "http://internal-docengine:5000",
    "ApiKey": "your-api-key-here",
    "ProjectId": "project-guid-here",
    "SignalRHub": "/agentHub"
  },
  "Agent": {
    "Mode": "service",
    "LocalHttpPort": 8888,
    "AutoStart": true,
    "AutoReconnect": true
  },
  "Analysis": {
    "ProjectPath": "C:\\Projects\\MyApp",
    "Database": {
      "ConnectionString": "Data Source=...;User Id=...;Password=...;",
      "Schema": "MY_SCHEMA"
    }
  },
  "LocalCache": {
    "DatabasePath": "%AppData%\\ForgeHelm\\cache.db"
  }
}
```

---

## 五、安全性考量

### 5.1 API 認證
- **API Key**: 每個專案有獨立的 API Key
- **OAuth 2.0**: 可選，支援更進階的認證

### 5.2 資料傳輸
- **HTTPS**: 所有 API 通訊使用 HTTPS
- **資料加密**: 敏感資料（如資料庫連線字串）在 Agent 端加密後傳輸

### 5.3 資料儲存
- **客戶端**: 連線字串等敏感資訊加密儲存
- **SaaS 平台**: 不儲存客戶原始程式碼，只儲存分析結果（結構化資料）

### 5.4 網路隔離
- **內網環境**: Agent 可配置代理伺服器
- **外網環境**: 直接連接 SaaS 平台

---

## 六、部署步驟

### 6.1 SaaS 平台部署（內網部署）

#### 步驟 1: 準備內網伺服器環境
- Windows Server 或 Linux Server
- 安裝 .NET 9.0 Runtime
- 安裝 PostgreSQL
- 配置內網 DNS 或使用 IP 位址

#### 步驟 2: 準備 PostgreSQL 資料庫
```sql
-- 建立資料庫
CREATE DATABASE docengine;

-- 執行遷移腳本（後續提供）
```

#### 步驟 3: 部署 Web Application
- 發布到 IIS（Windows）或 Nginx（Linux）
- 配置連線字串（PostgreSQL、內網 AI Server）
- 設定環境變數
- 配置 SignalR（WebSocket 支援）

#### 步驟 4: 設定 Git Repository
- 在內網建立 Git Repository（GitLab、Gitea 等）
- 或使用本地檔案系統作為版控
- 配置 SSH Key 或 Access Token

#### 步驟 5: 配置內網 AI Server
- 確認內網 AI Server 的 API 端點
- 在 appsettings.json 中配置 AI Server URL
- 測試連線

### 6.2 Agent 部署

#### 步驟 1: 建立 Agent 專案
- 建立新的 .NET Console Application
- 加入必要的 NuGet 套件

#### 步驟 2: 打包發行
```bash
dotnet publish -c Release -r win-x64 --self-contained
```

#### 步驟 3: 客戶端安裝
- 解壓縮到客戶指定目錄
- 執行配置精靈或手動編輯 config.json
- （可選）設定 Windows Task Scheduler

---

## 七、技術棧總結

### SaaS 平台
- ASP.NET Core MVC (.NET 9.0)
- ASP.NET Core SignalR (雙向通訊)
- PostgreSQL
- DocumentFormat.OpenXml
- LibGit2Sharp
- OpenAI / 自訂 AI API Client

### Agent
- .NET 9.0 Console Application
- ASP.NET Core (本地 HTTP 服務)
- Microsoft.AspNetCore.SignalR.Client (SignalR Client)
- Microsoft.CodeAnalysis.CSharp (Roslyn)
- Oracle.ManagedDataAccess.Core
- SQLite (System.Data.SQLite)
- HttpClient (REST API)

---

## 八、優缺點分析

### 優點
✅ **安全性**: 客戶程式碼和資料庫不離開客戶環境  
✅ **靈活性**: Agent 可手動執行或排程  
✅ **集中管理**: SaaS 平台統一管理文件生成和版控  
✅ **環境適應**: 支援外網和內網環境  
✅ **離線支援**: Agent 可離線分析，完成後再上傳  

### 缺點
⚠️ **安裝需求**: 客戶需要安裝 Agent  
⚠️ **維護成本**: Agent 需要更新和維護  
⚠️ **網路依賴**: 上傳分析結果需要網路連線  

---

## 九、從 SaaS 觸發 Agent 的技術方案比較

| 方案 | 優點 | 缺點 | 適用場景 |
|------|------|------|----------|
| **SignalR** (推薦) | 即時雙向通訊、自動重連、進度回報 | 需要 WebSocket 支援 | 大多數場景 |
| **本地 HTTP 服務** | 簡單直接、防火牆友善 | 需要知道 Agent IP | 簡單場景、備用方案 |
| **混合方案** | 結合兩者優點 | 實作較複雜 | 企業級應用 |

### 9.1 SignalR 實作細節

**SaaS 平台端**：
```csharp
// Program.cs
builder.Services.AddSignalR();

app.MapHub<AgentHub>("/agentHub");

// AgentHub.cs
public class AgentHub : Hub
{
    private readonly IAgentRegistry _agentRegistry;
    
    public override async Task OnConnectedAsync()
    {
        var agentId = Context.ConnectionId;
        await _agentRegistry.RegisterAgent(agentId, Context.UserIdentifier);
        await base.OnConnectedAsync();
    }
    
    public async Task SendAnalysisTask(string agentId, AnalysisTask task)
    {
        await Clients.Client(agentId).SendAsync("StartAnalysis", task);
    }
}
```

**Agent 端**：
```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("http://internal-docengine:5000/agentHub", options => {
        options.AccessTokenProvider = () => Task.FromResult(apiKey);
    })
    .WithAutomaticReconnect()
    .Build();

connection.On<AnalysisTask>("StartAnalysis", async (task) => {
    await ExecuteAnalysis(task);
    await connection.SendAsync("ReportProgress", task.TaskId, progress);
});

await connection.StartAsync();
```

### 9.2 本地 HTTP 服務實作細節

**Agent 端**：
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:8888");

var app = builder.Build();

app.MapPost("/api/analysis/start", async (AnalysisTask task) => {
    _ = Task.Run(() => ExecuteAnalysis(task));
    return Results.Ok(new { status = "started", taskId = task.TaskId });
});

app.MapGet("/api/status", () => {
    return Results.Ok(new { 
        status = _currentStatus,
        progress = _currentProgress 
    });
});

app.Run();
```

**SaaS 平台觸發**：
```csharp
// 需要知道 Agent 的 IP 位址（可透過 SignalR 註冊時取得）
var agentIp = await _agentRegistry.GetAgentIp(agentId);
var agentUrl = $"http://{agentIp}:8888/api/analysis/start";
var response = await httpClient.PostAsJsonAsync(agentUrl, task);
```

## 十、內網部署的優勢

### 10.1 為什麼內網部署更常見？

1. **安全性考量**
   - 客戶程式碼和資料庫不離開內網
   - 符合企業資安政策
   - 不需要 VPN 或專線

2. **效能考量**
   - 內網連線速度快
   - 減少網路延遲
   - AI Server 在內網，回應快速

3. **成本考量**
   - 不需要雲端服務費用
   - 不需要 VPN 或專線費用
   - 一次部署，長期使用

4. **合規性**
   - 符合資料在地化要求
   - 符合政府機關資安規範

### 10.2 內網部署架構建議

```
客戶內網環境
├─ 應用伺服器
│  └─ ForgeHelm Web App (IIS / Nginx)
├─ 資料庫伺服器
│  └─ PostgreSQL
├─ AI 伺服器
│  └─ 內網 AI Server (HTTP API)
├─ 版控伺服器
│  └─ GitLab / Gitea (內網)
└─ 開發人員電腦
   └─ ForgeHelm.Agent.exe
```

## 十一、替代方案比較

| 方案 | 優點 | 缺點 | 適用場景 |
|------|------|------|----------|
| **混合式架構（內網部署）** (推薦) | 安全性高、靈活、符合企業需求 | 需要內網伺服器 | 大多數企業場景 |
| 混合式架構（雲端部署） | 集中管理、易於維護 | 需要 VPN、成本較高 | 多客戶 SaaS 服務 |
| 純 SaaS | 無需安裝 | 需要上傳程式碼 | 小型專案、公開程式碼 |
| Agent 服務 | 即時觸發 | 需要常駐服務 | 大型企業、頻繁更新 |

---

**文件結束**
