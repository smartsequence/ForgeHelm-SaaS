using Microsoft.AspNetCore.SignalR;
using DocEngine.Services;
using System.Collections.Concurrent;

namespace DocEngine.Hubs;

/// <summary>
/// SignalR Hub 用於 Agent 與 SaaS 平台之間的雙向通訊
/// </summary>
public class AgentHub : Hub
{
    private readonly ILogger<AgentHub> _logger;
    private readonly AgentService _agentService;

    public AgentHub(ILogger<AgentHub> logger, AgentService agentService)
    {
        _logger = logger;
        _agentService = agentService;
    }

    /// <summary>
    /// Agent 連接時調用，註冊 Agent
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var agentId = Context.GetHttpContext()?.Request.Query["agentId"].ToString();
        var agentName = Context.GetHttpContext()?.Request.Query["agentName"].ToString() ?? "Unknown";
        var machineName = Context.GetHttpContext()?.Request.Query["machineName"].ToString() ?? Environment.MachineName;

        if (string.IsNullOrEmpty(agentId))
        {
            agentId = Context.ConnectionId;
        }

        var agentInfo = new AgentInfo
        {
            AgentId = agentId,
            ConnectionId = Context.ConnectionId,
            AgentName = agentName,
            MachineName = machineName,
            ConnectedAt = DateTime.UtcNow,
            Status = AgentStatus.Connected
        };

        await _agentService.RegisterAgentAsync(agentInfo);
        _logger.LogInformation("Agent 已連接: {AgentId} ({AgentName}) on {MachineName}, ConnectionId: {ConnectionId}", 
            agentId, agentName, machineName, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Agent 斷線時調用，移除 Agent 註冊
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await _agentService.UnregisterAgentAsync(Context.ConnectionId);
        _logger.LogInformation("Agent 已斷線: ConnectionId: {ConnectionId}, Exception: {Exception}", 
            Context.ConnectionId, exception?.Message ?? "正常斷線");

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Agent 回報任務進度
    /// </summary>
    public async Task ReportProgress(string taskId, int progress, string message)
    {
        _logger.LogInformation("收到 Agent 進度回報: TaskId: {TaskId}, Progress: {Progress}%, Message: {Message}", 
            taskId, progress, message);

        // 通知所有客戶端（包括 Web UI）任務進度更新
        await Clients.All.SendAsync("TaskProgressUpdated", taskId, progress, message);
    }

    /// <summary>
    /// Agent 回報任務完成
    /// </summary>
    public async Task ReportTaskCompleted(string taskId, bool success, string? result = null, string? error = null)
    {
        _logger.LogInformation("收到 Agent 任務完成: TaskId: {TaskId}, Success: {Success}", taskId, success);

        // 通知所有客戶端任務完成
        await Clients.All.SendAsync("TaskCompleted", taskId, success, result, error);
        
        // 更新 Agent 狀態為空閒
        await _agentService.UpdateAgentStatusAsync(Context.ConnectionId, AgentStatus.Idle);
    }

    /// <summary>
    /// Agent 回報分析結果
    /// </summary>
    public async Task ReportAnalysisResult(string taskId, object analysisResult)
    {
        _logger.LogInformation("收到 Agent 分析結果: TaskId: {TaskId}", taskId);

        // 通知所有客戶端分析結果
        await Clients.All.SendAsync("AnalysisResultReceived", taskId, analysisResult);
    }
}
