# Agent 功能開發總結

**開發日期：** 2026-01-16  
**分支：** develop-agent  
**狀態：** ✅ 已完成核心功能與 UI 擴充

---

## 一、已完成功能

### 1. SignalR 通訊基礎設施

#### ✅ AgentHub (Hubs/AgentHub.cs)
- SignalR Hub 用於 Agent 與 SaaS 平台之間的雙向通訊
- 功能：
  - Agent 連接時自動註冊
  - Agent 斷線時自動移除
  - 接收 Agent 的進度回報
  - 接收 Agent 的任務完成通知
  - 接收 Agent 的分析結果

#### ✅ AgentService (Services/AgentService.cs)
- Agent 管理服務，負責管理所有連接的 Agent
- 功能：
  - 註冊/移除 Agent
  - 更新 Agent 狀態
  - 查詢所有 Agent 或空閒 Agent
  - 發送任務到指定 Agent 或第一個空閒 Agent

### 2. Agent 管理 API

#### ✅ AgentController (Controllers/AgentController.cs)
- RESTful API 端點：
  - `GET /Agent` - Agent 管理頁面
  - `GET /api/agents` - 取得所有已連接的 Agent
  - `POST /api/agents/{agentId}/trigger` - 觸發指定 Agent 執行任務
  - `POST /api/agents/trigger-idle` - 觸發第一個空閒的 Agent 執行任務

### 3. Agent 管理 UI

#### ✅ Agent 管理頁面 (Views/Agent/Index.cshtml)
- 功能：
  - 顯示所有已連接的 Agent
  - 顯示 Agent 狀態（已連接、空閒、工作中、已斷線）
  - 顯示 Agent 資訊（ID、名稱、機器名稱、連接時間）
  - 觸發 Agent 執行分析任務
  - 任務參數輸入（任務類型、專案路徑、資料庫連接字串）
  - 動態更新 Agent 設定（settingsUpdates）
  - 即時更新 Agent 狀態（透過 SignalR）
  - 任務進度監控

### 4. 配置與整合

#### ✅ Program.cs 配置
- 添加 SignalR 服務
- 配置 AgentHub 路由 (`/agentHub`)
- 註冊 AgentService 為 Singleton

#### ✅ 套件依賴
- SignalR 已內建於 .NET 9（無需額外套件參考）

---

## 二、工作流程

### Agent 連接流程

```
1. Agent 啟動
   ↓
2. Agent 連接到 SaaS 平台的 SignalR Hub (/agentHub)
   ↓
3. AgentHub.OnConnectedAsync() 被調用
   ↓
4. Agent 資訊被註冊到 AgentService
   ↓
5. Agent 出現在 Agent 管理頁面
```

### 任務觸發流程

```
1. 使用者在 Agent 管理頁面點擊「觸發分析」
   ↓
2. 前端發送 POST 請求到 /api/agents/{agentId}/trigger
   ↓
3. AgentController.TriggerAgent() 處理請求
   ↓
4. AgentService.SendTaskToAgentAsync() 發送任務
   ↓
5. 透過 SignalR Hub 發送 "ExecuteTask" 訊息到 Agent
   ↓
6. Agent 接收任務並開始執行
   ↓
7. Agent 透過 SignalR 回報進度 (ReportProgress)
   ↓
8. Agent 完成任務後回報結果 (ReportTaskCompleted)
   ↓
9. 前端透過 SignalR 接收更新並更新 UI
```

---

## 三、技術架構

### SignalR 通訊協議

#### Server → Agent (SaaS 發送到 Agent)
- `ExecuteTask(taskId, taskData)` - 執行任務指令

#### Agent → Server (Agent 發送到 SaaS)
- `ReportProgress(taskId, progress, message)` - 回報進度
- `ReportTaskCompleted(taskId, success, result, error)` - 回報任務完成
- `ReportAnalysisResult(taskId, analysisResult)` - 回報分析結果

#### Server → Client (SaaS 廣播到所有 Web 客戶端)
- `TaskProgressUpdated(taskId, progress, message)` - 任務進度更新
- `TaskCompleted(taskId, success, result, error)` - 任務完成通知
- `AnalysisResultReceived(taskId, analysisResult)` - 分析結果通知

### Agent 狀態管理

```csharp
public enum AgentStatus
{
    Connected,      // 已連接
    Idle,          // 空閒
    Working,       // 工作中
    Disconnected   // 已斷線
}
```

### Agent 資訊結構

```csharp
public class AgentInfo
{
    public string AgentId { get; set; }
    public string ConnectionId { get; set; }
    public string AgentName { get; set; }
    public string MachineName { get; set; }
    public DateTime ConnectedAt { get; set; }
    public AgentStatus Status { get; set; }
    public string? CurrentTaskId { get; set; }
}
```

---

## 四、使用說明

### 1. 啟動 SaaS 平台

```bash
dotnet run
```

SaaS 平台會啟動在 `http://localhost:5163`，SignalR Hub 端點為 `/agentHub`

### 2. Agent 連接

Agent 需要連接到 SignalR Hub，連接 URL：
```
ws://localhost:5163/agentHub?agentId={agentId}&agentName={agentName}&machineName={machineName}
```

### 3. 訪問 Agent 管理頁面

在瀏覽器中訪問：
```
http://localhost:5163/Agent
```

### 4. 觸發任務

1. 在 Agent 管理頁面中，找到要觸發的 Agent
2. 點擊「觸發分析」按鈕
3. 填寫任務資訊（任務類型、專案路徑、資料庫連接字串、可選 settingsUpdates）
4. 點擊「確認觸發」
5. 任務會透過 SignalR 發送到 Agent
6. Agent 執行任務並回報進度

---

## 五、近期更新補充

- 新增 `settingsUpdates`：可由 SaaS UI 動態更新 Agent 的 `appsettings.json`
- 新增一鍵啟動/停止腳本：`scripts/run-all.*`、`scripts/stop-all.*`
- `launchSettings.json` 新增：`SaaS+Agent`、`Stop SaaS+Agent`

---

## 六、後續開發建議

### 1. Agent 端開發
- [x] 實作 SignalR 客戶端連接
- [x] 實作任務接收和執行邏輯
- [x] 實作進度回報機制
- [x] 實作程式碼分析功能（C#/VB/ASPX）
- [x] 實作資料庫分析功能

### 2. SaaS 端增強
- [ ] 添加任務歷史記錄
- [ ] 添加任務排程功能
- [ ] 添加 Agent 健康檢查
- [ ] 添加任務結果儲存
- [ ] 添加任務結果查看頁面

### 3. 安全性
- [ ] 添加 Agent 認證機制
- [ ] 添加任務授權檢查
- [ ] 添加通訊加密

### 4. 錯誤處理
- [ ] 添加任務失敗重試機制
- [ ] 添加 Agent 斷線處理
- [ ] 添加任務超時處理

---

## 六、檔案結構

```
DocEngine/
├── Hubs/
│   └── AgentHub.cs              # SignalR Hub
├── Services/
│   └── AgentService.cs          # Agent 管理服務
├── Controllers/
│   └── AgentController.cs       # Agent 管理控制器
├── Views/
│   └── Agent/
│       └── Index.cshtml         # Agent 管理頁面
├── Program.cs                   # SignalR 配置
└── DocEngine.csproj            # 套件依賴
```

---

## 七、測試建議

### 1. 單元測試
- AgentService 的註冊/移除邏輯
- AgentController 的 API 端點

### 2. 整合測試
- SignalR 連接和斷線
- 任務發送和接收
- 進度回報機制

### 3. 端到端測試
- 完整的 Agent 連接 → 任務觸發 → 執行 → 完成流程

---

## 八、注意事項

1. **SignalR 版本**：使用 `Microsoft.AspNetCore.SignalR` Version 8.0.0（與 .NET 9.0 相容）

2. **JavaScript 客戶端**：使用 CDN 提供的 SignalR JavaScript 客戶端（`@@microsoft/signalr@7.0.0`）

3. **AgentService 生命週期**：使用 Singleton，因為需要維護全局的 Agent 狀態

4. **並發安全**：使用 `ConcurrentDictionary` 來確保線程安全

5. **連接管理**：Agent 斷線時會自動從 AgentService 中移除

---

**開發完成時間：** 2024-12-XX  
**下一步：** 開發 Agent 端程式（DocEngine-Agent）
