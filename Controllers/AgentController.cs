using Microsoft.AspNetCore.Mvc;
using DocEngine.Services;
using Microsoft.AspNetCore.SignalR;
using DocEngine.Hubs;

namespace DocEngine.Controllers;

/// <summary>
/// Agent 管理控制器
/// </summary>
public class AgentController : Controller
{
    private readonly ILogger<AgentController> _logger;
    private readonly AgentService _agentService;
    private readonly IHubContext<AgentHub> _hubContext;

    public AgentController(
        ILogger<AgentController> logger,
        AgentService agentService,
        IHubContext<AgentHub> hubContext)
    {
        _logger = logger;
        _agentService = agentService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Agent 管理頁面
    /// </summary>
    public IActionResult Index()
    {
        var agents = _agentService.GetAllAgents().ToList();
        ViewData["Agents"] = agents;
        return View();
    }

    /// <summary>
    /// 取得所有 Agent 的 API
    /// </summary>
    [HttpGet]
    [Route("api/agents")]
    public IActionResult GetAgents()
    {
        var agents = _agentService.GetAllAgents().Select(a => new
        {
            a.AgentId,
            a.AgentName,
            a.MachineName,
            a.Status,
            a.ConnectedAt,
            a.CurrentTaskId
        }).ToList();

        return Json(agents);
    }

    /// <summary>
    /// 觸發 Agent 執行分析任務
    /// </summary>
    [HttpPost]
    [Route("api/agents/{agentId}/trigger")]
    public async Task<IActionResult> TriggerAgent(string agentId, [FromBody] TriggerTaskRequest request)
    {
        if (string.IsNullOrEmpty(agentId))
        {
            return BadRequest(new { error = "AgentId 不能為空" });
        }

        var agent = _agentService.GetAgentById(agentId);
        if (agent == null)
        {
            return NotFound(new { error = $"找不到 Agent: {agentId}" });
        }

        if (agent.Status == AgentStatus.Working)
        {
            return BadRequest(new { error = $"Agent {agentId} 正在執行任務中" });
        }

        // 生成任務 ID
        var taskId = Guid.NewGuid().ToString();

        // 構建任務數據
        var taskData = new
        {
            taskId = taskId,
            taskType = request.TaskType ?? "analyze",
            projectPath = request.ProjectPath,
            databaseConnectionString = request.DatabaseConnectionString,
            options = request.Options,
            settingsUpdates = request.SettingsUpdates
        };

        // 發送任務到 Agent
        var success = await _agentService.SendTaskToAgentAsync(agentId, taskId, taskData);

        if (success)
        {
            _logger.LogInformation("任務已觸發: AgentId: {AgentId}, TaskId: {TaskId}", agentId, taskId);
            return Ok(new { taskId, message = "任務已成功發送到 Agent" });
        }
        else
        {
            _logger.LogError("任務觸發失敗: AgentId: {AgentId}, TaskId: {TaskId}", agentId, taskId);
            return StatusCode(500, new { error = "任務發送失敗" });
        }
    }

    /// <summary>
    /// 觸發第一個空閒的 Agent 執行任務
    /// </summary>
    [HttpPost]
    [Route("api/agents/trigger-idle")]
    public async Task<IActionResult> TriggerIdleAgent([FromBody] TriggerTaskRequest request)
    {
        // 生成任務 ID
        var taskId = Guid.NewGuid().ToString();

        // 構建任務數據
        var taskData = new
        {
            taskId = taskId,
            taskType = request.TaskType ?? "analyze",
            projectPath = request.ProjectPath,
            databaseConnectionString = request.DatabaseConnectionString,
            options = request.Options,
            settingsUpdates = request.SettingsUpdates
        };

        // 發送任務到第一個空閒的 Agent
        var success = await _agentService.SendTaskToIdleAgentAsync(taskId, taskData);

        if (success)
        {
            _logger.LogInformation("任務已觸發到空閒 Agent: TaskId: {TaskId}", taskId);
            return Ok(new { taskId, message = "任務已成功發送到空閒的 Agent" });
        }
        else
        {
            _logger.LogWarning("沒有可用的空閒 Agent: TaskId: {TaskId}", taskId);
            return StatusCode(503, new { error = "目前沒有可用的空閒 Agent" });
        }
    }
}

/// <summary>
/// 觸發任務請求
/// </summary>
public class TriggerTaskRequest
{
    public string? TaskType { get; set; }
    public string? ProjectPath { get; set; }
    public string? DatabaseConnectionString { get; set; }
    public Dictionary<string, object>? Options { get; set; }
    
    /// <summary>
    /// 配置更新參數（用於動態更新 Agent 的 appsettings.json）
    /// 格式：{ "Analysis.SourceCodePath": "C:\\path", "Database.ConnectionString": "..." }
    /// </summary>
    public Dictionary<string, object>? SettingsUpdates { get; set; }
}
