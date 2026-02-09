using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;

namespace ForgeHelm.SaaS.Services;

/// <summary>
/// 報告編號生成服務
/// 編碼原則：RA-{系統代號}-{YYYYMMDD}-{流水號}
/// 例如：RA-ADM-20260113-003
/// </summary>
public class ReportIdService
{
    private readonly ILogger<ReportIdService> _logger;
    private readonly string _dataDirectory;
    private const string ReportIdFileName = "report_ids.json";

    public ReportIdService(ILogger<ReportIdService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _dataDirectory = Path.Combine(env.ContentRootPath, "Data");
        
        // 確保 Data 目錄存在
        if (!Directory.Exists(_dataDirectory))
        {
            Directory.CreateDirectory(_dataDirectory);
            _logger.LogInformation("創建 Data 目錄: {DataDirectory}", _dataDirectory);
        }
    }

    /// <summary>
    /// 生成報告編號
    /// </summary>
    /// <param name="systemCode">系統代號（如：ADM）</param>
    /// <returns>報告編號（如：RA-ADM-20260113-003）</returns>
    public async Task<string> GenerateReportIdAsync(string systemCode)
    {
        try
        {
            // 標準化系統代號（轉大寫，移除空格）
            var normalizedSystemCode = (systemCode ?? "SYS").Trim().ToUpper();
            if (string.IsNullOrWhiteSpace(normalizedSystemCode))
            {
                normalizedSystemCode = "SYS";
            }

            // 取得今天的日期（YYYYMMDD）
            var today = DateTime.Now.ToString("yyyyMMdd");

            // 讀取或建立流水號記錄
            var reportIds = await LoadReportIdsAsync();
            
            // 檢查今天的記錄是否存在
            var todayKey = $"{normalizedSystemCode}-{today}";
            if (!reportIds.ContainsKey(todayKey))
            {
                reportIds[todayKey] = 0;
            }

            // 增加流水號
            reportIds[todayKey]++;

            // 保存流水號記錄
            await SaveReportIdsAsync(reportIds);

            // 生成報告編號
            var sequenceNumber = reportIds[todayKey].ToString("D3"); // 三位數，不足補零
            var reportId = $"RA-{normalizedSystemCode}-{today}-{sequenceNumber}";

            _logger.LogInformation("生成報告編號: {ReportId} (系統代號: {SystemCode}, 日期: {Date}, 流水號: {Sequence})", 
                reportId, normalizedSystemCode, today, sequenceNumber);

            return reportId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成報告編號時發生錯誤");
            // 如果發生錯誤，返回一個基於時間戳的備用編號
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            return $"RA-{systemCode?.ToUpper() ?? "SYS"}-{timestamp}-ERR";
        }
    }

    /// <summary>
    /// 載入流水號記錄
    /// </summary>
    private async Task<Dictionary<string, int>> LoadReportIdsAsync()
    {
        var filePath = Path.Combine(_dataDirectory, ReportIdFileName);
        
        if (!File.Exists(filePath))
        {
            return new Dictionary<string, int>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath);
            var reportIds = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
            
            // 清理過期的記錄（只保留最近 30 天的記錄）
            var cutoffDate = DateTime.Now.AddDays(-30);
            var cleanedReportIds = new Dictionary<string, int>();
            
            foreach (var kvp in reportIds ?? new Dictionary<string, int>())
            {
                // 嘗試解析日期（格式：系統代號-YYYYMMDD）
                var parts = kvp.Key.Split('-');
                if (parts.Length >= 2)
                {
                    var dateStr = parts[parts.Length - 1]; // 最後一部分是日期
                    if (DateTime.TryParseExact(dateStr, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var recordDate))
                    {
                        if (recordDate >= cutoffDate)
                        {
                            cleanedReportIds[kvp.Key] = kvp.Value;
                        }
                    }
                }
            }

            // 如果有清理，保存清理後的記錄
            if (cleanedReportIds.Count < (reportIds?.Count ?? 0))
            {
                await SaveReportIdsAsync(cleanedReportIds);
            }

            return cleanedReportIds;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "載入報告編號記錄時發生錯誤，將使用空記錄");
            return new Dictionary<string, int>();
        }
    }

    /// <summary>
    /// 保存流水號記錄
    /// </summary>
    private async Task SaveReportIdsAsync(Dictionary<string, int> reportIds)
    {
        var filePath = Path.Combine(_dataDirectory, ReportIdFileName);
        
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var json = JsonSerializer.Serialize(reportIds, options);
            await File.WriteAllTextAsync(filePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存報告編號記錄時發生錯誤");
            throw;
        }
    }
}
