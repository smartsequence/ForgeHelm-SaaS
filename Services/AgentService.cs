using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace DocEngine.Services;

/// <summary>
/// Agent 狀態
/// </summary>
public enum AgentStatus
{
    Connected,  // 已連接
    Idle,       // 空閒
    Working,    // 工作中
    Disconnected // 已斷線
}

/// <summary>
/// Agent 資訊
/// </summary>
public class AgentInfo
{
    public string AgentId { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
    public AgentStatus Status { get; set; }
    public string? CurrentTaskId { get; set; }
}

/// <summary>
/// Agent 管理服務，負責管理所有連接的 Agent
/// </summary>
public class AgentService
{
    private readonly ConcurrentDictionary<string, AgentInfo> _agents = new();
    private readonly ConcurrentDictionary<string, string> _connectionIdToAgentId = new();
    private readonly ILogger<AgentService> _logger;
    private readonly IHubContext<Hubs.AgentHub>? _hubContext;

    public AgentService(ILogger<AgentService> logger, IHubContext<Hubs.AgentHub>? hubContext = null)
    {
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// 註冊 Agent
    /// </summary>
    public Task RegisterAgentAsync(AgentInfo agentInfo)
    {
        _agents.AddOrUpdate(agentInfo.ConnectionId, agentInfo, (key, oldValue) => agentInfo);
        _connectionIdToAgentId.AddOrUpdate(agentInfo.ConnectionId, agentInfo.AgentId, (key, oldValue) => agentInfo.AgentId);
        
        _logger.LogInformation("Agent 已註冊: {AgentId} ({AgentName}), 總數: {Count}", 
            agentInfo.AgentId, agentInfo.AgentName, _agents.Count);
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 移除 Agent 註冊
    /// </summary>
    public Task UnregisterAgentAsync(string connectionId)
    {
        if (_agents.TryRemove(connectionId, out var agentInfo))
        {
            _connectionIdToAgentId.TryRemove(connectionId, out _);
            _logger.LogInformation("Agent 已移除: {AgentId}, 剩餘: {Count}", agentInfo.AgentId, _agents.Count);
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新 Agent 狀態
    /// </summary>
    public Task UpdateAgentStatusAsync(string connectionId, AgentStatus status, string? taskId = null)
    {
        if (_agents.TryGetValue(connectionId, out var agentInfo))
        {
            agentInfo.Status = status;
            if (taskId != null)
            {
                agentInfo.CurrentTaskId = taskId;
            }
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 取得所有已連接的 Agent
    /// </summary>
    public IEnumerable<AgentInfo> GetAllAgents()
    {
        return _agents.Values.Where(a => a.Status != AgentStatus.Disconnected).ToList();
    }

    /// <summary>
    /// 根據 AgentId 取得 Agent 資訊
    /// </summary>
    public AgentInfo? GetAgentById(string agentId)
    {
        return _agents.Values.FirstOrDefault(a => a.AgentId == agentId);
    }

    /// <summary>
    /// 根據 ConnectionId 取得 Agent 資訊
    /// </summary>
    public AgentInfo? GetAgentByConnectionId(string connectionId)
    {
        _agents.TryGetValue(connectionId, out var agentInfo);
        return agentInfo;
    }

    /// <summary>
    /// 取得空閒的 Agent
    /// </summary>
    public IEnumerable<AgentInfo> GetIdleAgents()
    {
        return _agents.Values.Where(a => a.Status == AgentStatus.Idle || a.Status == AgentStatus.Connected).ToList();
    }

    /// <summary>
    /// 發送任務到指定的 Agent
    /// </summary>
    public async Task<bool> SendTaskToAgentAsync(string agentId, string taskId, object taskData)
    {
        var agent = GetAgentById(agentId);
        if (agent == null || _hubContext == null)
        {
            _logger.LogWarning("無法找到 Agent: {AgentId} 或 HubContext 未初始化", agentId);
            return false;
        }

        try
        {
            await _hubContext.Clients.Client(agent.ConnectionId).SendAsync("ExecuteTask", taskId, taskData);
            await UpdateAgentStatusAsync(agent.ConnectionId, AgentStatus.Working, taskId);
            
            _logger.LogInformation("任務已發送到 Agent: {AgentId}, TaskId: {TaskId}", agentId, taskId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發送任務到 Agent 失敗: {AgentId}, TaskId: {TaskId}", agentId, taskId);
            return false;
        }
    }

    /// <summary>
    /// 發送任務到第一個空閒的 Agent
    /// </summary>
    public async Task<bool> SendTaskToIdleAgentAsync(string taskId, object taskData)
    {
        var idleAgents = GetIdleAgents().ToList();
        if (!idleAgents.Any())
        {
            _logger.LogWarning("沒有可用的空閒 Agent");
            return false;
        }

        // 選擇第一個空閒的 Agent
        var agent = idleAgents.First();
        return await SendTaskToAgentAsync(agent.AgentId, taskId, taskData);
    }
}
