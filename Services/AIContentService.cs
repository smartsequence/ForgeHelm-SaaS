using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ForgeHelm.SaaS.Services;

/// <summary>
/// AI 內容生成服務，統一處理「先生成英文，再翻譯成繁體中文」的流程
/// </summary>
public class AIContentService
{
    private readonly OpenAIService _aiService;
    private readonly TranslationService _translationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AIContentService> _logger;

    public AIContentService(
        OpenAIService aiService,
        TranslationService translationService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AIContentService> logger)
    {
        _aiService = aiService;
        _translationService = translationService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 生成內容（統一流程：先生成英文，需要時翻譯成繁體中文）
    /// </summary>
    /// <param name="cacheKey">Session 緩存鍵（例如 "PersonalizedAdvice"）</param>
    /// <param name="systemPrompt">系統 Prompt（英文）</param>
    /// <param name="userPrompt">用戶 Prompt（英文）</param>
    /// <param name="targetLang">目標語言（"zh-TW" 或 "en-US"）</param>
    /// <param name="forceRegenerate">是否強制重新生成</param>
    /// <param name="temperature">溫度參數（預設 0.7）</param>
    /// <param name="maxTokens">最大 token 數（預設 500）</param>
    /// <param name="additionalTranslationInstructions">額外的翻譯指示（用於特殊格式要求）</param>
    /// <returns>生成或翻譯後的內容</returns>
    public async Task<string> GenerateContentAsync(
        string cacheKey,
        string systemPrompt,
        string userPrompt,
        string targetLang = "zh-TW",
        bool forceRegenerate = false,
        double temperature = 0.7,
        int maxTokens = 500,
        string? additionalTranslationInstructions = null)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HttpContext is not available");

        var isEnglish = targetLang == "en-US";
        var englishCacheKey = $"{cacheKey}_en-US";
        var chineseCacheKey = $"{cacheKey}_zh-TW";

        // 如果強制重新生成，清除緩存
        if (forceRegenerate)
        {
            httpContext.Session.Remove(englishCacheKey);
            httpContext.Session.Remove(chineseCacheKey);
            _logger.LogInformation("強制重新生成，已清除緩存: {CacheKey}", cacheKey);
        }

        // 檢查是否有英文版本緩存
        var cachedEnglish = httpContext.Session.GetString(englishCacheKey);
        if (!string.IsNullOrEmpty(cachedEnglish) && !forceRegenerate)
        {
            _logger.LogInformation("發現已存在的英文版本: {CacheKey}", cacheKey);
            if (isEnglish)
            {
                return cachedEnglish;
            }
            else
            {
                // 翻譯為繁體中文
                var translated = await _translationService.TranslateToTraditionalChineseAsync(cachedEnglish);
                return translated;
            }
        }

        // 生成英文版本
        _logger.LogInformation("生成英文版本: {CacheKey}", cacheKey);
        var englishContent = await _aiService.GenerateContentAsync(systemPrompt, userPrompt, temperature, maxTokens);

        if (string.IsNullOrEmpty(englishContent))
        {
            throw new InvalidOperationException("Failed to generate English content");
        }

        // 存儲英文版本到 Session
        httpContext.Session.SetString(englishCacheKey, englishContent);
        _logger.LogInformation("已將英文版本存儲到 Session: {CacheKey}", cacheKey);

        // 根據目標語言返回
        if (isEnglish)
        {
            return englishContent;
        }
        else
        {
            // 翻譯為繁體中文
            var translated = await _translationService.TranslateToTraditionalChineseAsync(englishContent, additionalTranslationInstructions);
            return translated;
        }
    }

    /// <summary>
    /// 生成結構化 JSON 內容（用於行動清單等）
    /// </summary>
    public async Task<Dictionary<string, string>?> GenerateJsonContentAsync(
        string cacheKey,
        string systemPrompt,
        string userPrompt,
        string[] requiredKeys,
        string targetLang = "zh-TW",
        bool forceRegenerate = false,
        double temperature = 0.7,
        int maxTokens = 800)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
            throw new InvalidOperationException("HttpContext is not available");

        var isEnglish = targetLang == "en-US";
        var englishCacheKey = $"{cacheKey}_en-US";

        // 如果強制重新生成，清除緩存
        if (forceRegenerate)
        {
            httpContext.Session.Remove(englishCacheKey);
            _logger.LogInformation("強制重新生成，已清除緩存: {CacheKey}", cacheKey);
        }

        // 檢查是否有英文版本緩存
        Dictionary<string, string>? englishJson = null;
        var cachedEnglishJson = httpContext.Session.GetString(englishCacheKey);
        if (!string.IsNullOrEmpty(cachedEnglishJson) && !forceRegenerate)
        {
            try
            {
                englishJson = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(cachedEnglishJson);
                if (englishJson != null && requiredKeys.All(k => englishJson.ContainsKey(k)))
                {
                    _logger.LogInformation("發現已存在的英文版本 JSON: {CacheKey}", cacheKey);
                    if (isEnglish)
                    {
                        return englishJson;
                    }
                    else
                    {
                        // 翻譯為繁體中文
                        var translated = await _translationService.TranslateJsonToTraditionalChineseAsync(englishJson);
                        return translated ?? englishJson;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "無法解析緩存的 JSON，將重新生成");
            }
        }

        // 生成英文版本
        _logger.LogInformation("生成英文版本 JSON: {CacheKey}", cacheKey);
        var englishContent = await _aiService.GenerateContentAsync(systemPrompt, userPrompt, temperature, maxTokens);

        if (string.IsNullOrEmpty(englishContent))
        {
            throw new InvalidOperationException("Failed to generate English content");
        }

        // 清理並解析 JSON
        var cleanedContent = OpenAIService.CleanMarkdownCodeBlocks(englishContent);
        englishJson = OpenAIService.ParseJsonToDictionary(cleanedContent);

        // 如果直接解析失敗，嘗試提取 JSON
        if (englishJson == null || !requiredKeys.All(k => englishJson.ContainsKey(k)))
        {
            var extractedJson = OpenAIService.ExtractJsonFromText(cleanedContent);
            if (extractedJson != null)
            {
                englishJson = OpenAIService.ParseJsonToDictionary(extractedJson);
            }
        }

        if (englishJson == null || !requiredKeys.All(k => englishJson.ContainsKey(k)))
        {
            _logger.LogError("無法解析生成的 JSON，缺少必要欄位: {RequiredKeys}", string.Join(", ", requiredKeys));
            throw new InvalidOperationException("Failed to parse generated JSON");
        }

        // 存儲英文版本到 Session
        var jsonString = System.Text.Json.JsonSerializer.Serialize(englishJson);
        httpContext.Session.SetString(englishCacheKey, jsonString);
        _logger.LogInformation("已將英文版本 JSON 存儲到 Session: {CacheKey}", cacheKey);

        // 根據目標語言返回
        if (isEnglish)
        {
            return englishJson;
        }
        else
        {
            // 翻譯為繁體中文
            var translated = await _translationService.TranslateJsonToTraditionalChineseAsync(englishJson);
            return translated ?? englishJson;
        }
    }
}
