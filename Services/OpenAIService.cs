using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DocEngine.Services;

/// <summary>
/// OpenAI API 服務，處理所有 AI 生成請求
/// </summary>
public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OpenAIService> _logger;

    public OpenAIService(HttpClient httpClient, ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// 發送 OpenAI API 請求並返回響應內容
    /// </summary>
    public async Task<string> GenerateContentAsync(string systemPrompt, string userPrompt, double temperature = 0.7, int maxTokens = 500)
    {
        try
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = temperature,
                max_tokens = maxTokens
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("發送 OpenAI API 請求，temperature: {Temperature}, maxTokens: {MaxTokens}", temperature, maxTokens);
            var response = await _httpClient.PostAsync("chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("OpenAI API 錯誤: {StatusCode}, {Error}", response.StatusCode, errorContent);
                throw new HttpRequestException($"OpenAI API 錯誤: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("收到 OpenAI API 回應，長度: {Length}", responseContent.Length);

            var jsonDoc = JsonDocument.Parse(responseContent);
            var contentText = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrEmpty(contentText))
            {
                _logger.LogWarning("AI 回應為空");
                throw new InvalidOperationException("AI response is empty");
            }

            return contentText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI API 請求失敗");
            throw;
        }
    }

    /// <summary>
    /// 清理 AI 回應中的 markdown 代碼塊標記
    /// </summary>
    public static string CleanMarkdownCodeBlocks(string content)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        var cleaned = content.Trim();

        // 移除開頭的 ```json 或 ```
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned.Substring(7).TrimStart();
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned.Substring(3).TrimStart();
        }

        // 移除結尾的 ```
        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned.Substring(0, cleaned.Length - 3).TrimEnd();
        }

        cleaned = cleaned.Trim();

        // 如果還有問題，使用正則表達式
        if (cleaned.StartsWith("`") || cleaned.Contains("```"))
        {
            cleaned = Regex.Replace(
                cleaned,
                @"^```(?:json)?\s*",
                "",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            cleaned = Regex.Replace(
                cleaned,
                @"\s*```$",
                "",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
            cleaned = cleaned.Trim();
        }

        return cleaned;
    }

    /// <summary>
    /// 從文字中提取 JSON 物件
    /// </summary>
    public static string? ExtractJsonFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return null;

        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');
        
        if (jsonStart >= 0 && jsonEnd > jsonStart)
        {
            return text.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        return null;
    }

    /// <summary>
    /// 解析 JSON 為字典
    /// </summary>
    public static Dictionary<string, string>? ParseJsonToDictionary(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
