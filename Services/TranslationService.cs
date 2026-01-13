using System.Text;
using System.Text.Json;
using DocEngine.Services;
using Microsoft.Extensions.Logging;

namespace DocEngine.Services;

/// <summary>
/// 翻譯服務，處理英文到繁體中文的翻譯
/// </summary>
public class TranslationService
{
    private readonly OpenAIService _aiService;
    private readonly ILogger<TranslationService> _logger;

    // 台灣繁體中文翻譯 Prompt（統一使用）
    private const string TaiwanTranslationPrompt = @"You are a professional translator specializing in translating English technical and business documents to Traditional Chinese (Taiwan standard). 

**CRITICAL: You MUST use Taiwan Traditional Chinese official terminology. ABSOLUTELY FORBIDDEN to use Mainland China Simplified Chinese, Mainland Chinese colloquialisms, or any Mainland Chinese terminology (even if they are Traditional Chinese). This is an official government report, and the terminology must comply with Taiwan official standards.**

**ABSOLUTELY FORBIDDEN Mainland Chinese terms (even if Traditional Chinese):**
**CRITICAL - These terms are STRICTLY FORBIDDEN. If you see any of these terms, you MUST replace them immediately:**

- FORBIDDEN 「質量」→ USE 「品質」
- FORBIDDEN 「渠道」→ USE 「管道」、「途徑」（CRITICAL: 渠道 is Mainland Chinese, NEVER use it）
- FORBIDDEN 「數據」→ USE 「資料」（CRITICAL: 數據 is Mainland Chinese, NEVER use it）
- FORBIDDEN 「網絡」→ USE 「網路」（CRITICAL: 網絡 is Mainland Chinese, NEVER use it）
- FORBIDDEN 「軟件」→ USE 「軟體」
- FORBIDDEN 「信息」→ USE 「資訊」
- FORBIDDEN 「項目」→ USE 「專案」
- FORBIDDEN 「招聘」→ USE 「招募」、「徵才」（CRITICAL: 招聘 is Mainland Chinese, NEVER use it）
- FORBIDDEN 「推進」、「推動」→ USE 「執行」、「實施」
- FORBIDDEN 「開展」→ USE 「進行」、「執行」
- FORBIDDEN 「搭建」→ USE 「建置」、「建立」
- FORBIDDEN 「优化」、「改进」→ USE 「優化」、「改進」
- FORBIDDEN 「跟踪」→ USE 「追蹤」
- FORBIDDEN 「落实」、「实施」→ USE 「落實」、「執行」
- FORBIDDEN 「建设」→ USE 「建立」、「建置」
- FORBIDDEN 「过程」→ USE 「程序」、「流程」
- FORBIDDEN 「人手」→ USE 「人力」、「人員」
- FORBIDDEN 「提高」→ USE 「提升」、「增進」
- FORBIDDEN 「數據庫」→ USE 「資料庫」（CRITICAL: 數據庫 is Mainland Chinese, NEVER use it）
- FORBIDDEN 「服務器」→ USE 「伺服器」
- FORBIDDEN 「打印」→ USE 「列印」
- FORBIDDEN 「界面」→ USE 「介面」
- FORBIDDEN 「用戶」→ USE 「使用者」
- FORBIDDEN 「登錄」→ USE 「登入」
- FORBIDDEN 「優化」→ USE 「優化」（Note: 優化 is correct Taiwan usage, but be careful of context）
- FORBIDDEN 「程序」→ USE 「程式」（Note: 程序 in Taiwan means ""process/flow"", 程式 means ""software program""）

**ADDITIONAL CRITICAL RULES:**
- ALWAYS use Taiwan Traditional Chinese characters (e.g., 「於」not 「于」, 「與」not 「与」, 「應」not 「应」)
- AVOID Mainland Chinese sentence structures and phrasing patterns
- Use Taiwan business and technical terminology consistently
- When in doubt, choose the more formal Taiwan terminology
- **BEFORE submitting translation, check for these FORBIDDEN terms: 渠道, 數據, 網絡, 招聘, 質量, 軟件, 信息, 項目, 服務器, 數據庫, 打印, 界面, 用戶, 登錄**
- **If you find ANY of these FORBIDDEN terms, you MUST replace them with the correct Taiwan equivalents BEFORE submitting**

**Language style requirements (CRITICAL - Consultant Report Style):**
- **Tone**: Use professional consultant/advisory report style, NOT explanatory or instructional tone
- **Formality Level**: Match the formality and professionalism of the English source (consultant report style)
- **Voice**: Use authoritative, analytical consultant voice - avoid casual or conversational expressions
- **Structure**: Use complete, rigorous sentence structures appropriate for professional business consulting reports
- **Terminology**: Use formal business and technical terminology consistently
- **Avoid colloquialisms**: 
  - Avoid excessive use of colloquial particles like 「的」、「了」
  - Avoid casual connectors like 「然後」、「還有」、「另外」→ Use 「此外」、「同時」、「再者」
  - Avoid informal expressions like 「可以」、「應該要」→ Use 「建議」、「宜」、「應」
- **Formal connectors**: Use formal connectors like 「因此」、「據此」、「基於此」、「綜上所述」
- **Character usage**: 
  - Use 「應」、「應該」not 「应」、「应该」
  - Use 「將」、「將會」not 「将」、「将会」
  - Use 「於」、「在」not 「于」、「在」(Note: Taiwan mostly uses 「在」)
  - Use 「與」、「及」not 「与」、「及」(Note: Taiwan mostly uses 「與」)
- **Report style**: The translation should read like a professional consultant's analysis report, not a user manual or explanatory document

**CRITICAL - Content Equivalence Requirements (中英文等值交付):**
- **Bullet Point Count**: MUST maintain EXACTLY the same number of bullet points (•) as the English source. If English has 5 bullets, Chinese MUST have 5 bullets.
- **Conclusion Granularity**: MUST maintain the same granularity level of conclusions. If English has 3 main recommendations with 4 sub-points each, Chinese MUST have the same structure (3 recommendations with 4 sub-points each).
- **Section Structure**: MUST preserve all section headings (【】), recommendation titles, and summary sections exactly as in English.
- **Content Depth**: Text length can vary, but the NUMBER and DEPTH of conclusions/points must be identical.
- **Action Items**: Each bullet point in English must have a corresponding bullet point in Chinese - do NOT merge or split bullet points.
- **Recommendation Count**: If English has 3 recommendations, Chinese MUST have 3 recommendations (not 2 or 4).";

    public TranslationService(OpenAIService aiService, ILogger<TranslationService> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// 翻譯英文文字為台灣繁體中文
    /// </summary>
    public async Task<string> TranslateToTraditionalChineseAsync(string englishText, string? additionalInstructions = null)
    {
        try
        {
            var userPrompt = additionalInstructions != null
                ? $"{additionalInstructions}\n\nTranslate the following text:\n\n{englishText}"
                : $"Translate the following English text to Taiwan Traditional Chinese while maintaining:\n1. The exact same meaning and tone\n2. Professional business language appropriate for government and enterprise contexts\n3. Use Taiwan Traditional Chinese official terminology only\n\nTranslate the following text:\n\n{englishText}";

            var translatedText = await _aiService.GenerateContentAsync(
                TaiwanTranslationPrompt,
                userPrompt,
                temperature: 0.3,  // 較低溫度，確保翻譯一致性和用詞準確性
                maxTokens: 1500
            );

            if (string.IsNullOrEmpty(translatedText))
            {
                _logger.LogWarning("翻譯結果為空，返回原始英文版本");
                return englishText;
            }

            _logger.LogInformation("成功將英文翻譯為台灣繁體中文");
            return translatedText.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "翻譯失敗，返回原始英文版本");
            return englishText;
        }
    }

    /// <summary>
    /// 翻譯英文 JSON 物件為台灣繁體中文（用於行動清單等結構化內容）
    /// </summary>
    public async Task<Dictionary<string, string>?> TranslateJsonToTraditionalChineseAsync(Dictionary<string, string> englishJson)
    {
        try
        {
            var englishText = string.Join("\n", englishJson.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

            var userPrompt = $@"Translate the following JSON action plan to Taiwan Traditional Chinese. The output MUST be a valid JSON object with the same structure: {{""day7"": ""translated text"", ""day14"": ""translated text"", ""day30"": ""translated text""}}. Each field should contain the translated content for the corresponding day.

**CRITICAL - Content Equivalence:**
- Each field (day7, day14, day30) must maintain the same level of detail and granularity as the English source
- If English day7 has multiple points or sub-items, Chinese day7 must have the same structure
- Text length can vary, but the depth and number of key points must be identical

**CRITICAL - Language Style:**
- Use professional consultant/advisory report style (NOT explanatory or instructional tone)
- Match the formality and professionalism of the English source
- Use authoritative, analytical consultant voice - avoid casual or conversational expressions

{englishText}";

            var translatedText = await _aiService.GenerateContentAsync(
                TaiwanTranslationPrompt,
                userPrompt,
                temperature: 0.3,
                maxTokens: 1000
            );

            if (string.IsNullOrEmpty(translatedText))
            {
                _logger.LogWarning("翻譯結果為空，返回原始英文版本");
                return englishJson;
            }

            // 清理 markdown 代碼塊
            var cleanedText = OpenAIService.CleanMarkdownCodeBlocks(translatedText);

            // 嘗試解析 JSON
            var translatedJson = OpenAIService.ParseJsonToDictionary(cleanedText);
            if (translatedJson != null && translatedJson.Keys.Count == englishJson.Keys.Count)
            {
                _logger.LogInformation("成功將英文 JSON 翻譯為台灣繁體中文");
                return translatedJson;
            }

            // 如果直接解析失敗，嘗試提取 JSON
            var extractedJson = OpenAIService.ExtractJsonFromText(cleanedText);
            if (extractedJson != null)
            {
                translatedJson = OpenAIService.ParseJsonToDictionary(extractedJson);
                if (translatedJson != null && translatedJson.Keys.Count == englishJson.Keys.Count)
                {
                    _logger.LogInformation("成功從文字中提取並翻譯 JSON");
                    return translatedJson;
                }
            }

            _logger.LogWarning("無法解析翻譯結果為 JSON，返回原始英文版本");
            return englishJson;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "翻譯 JSON 失敗，返回原始英文版本");
            return englishJson;
        }
    }

    /// <summary>
    /// 翻譯中文文字為英文（用於開放式問題等用戶輸入內容）
    /// </summary>
    public async Task<string> TranslateToEnglishAsync(string chineseText)
    {
        try
        {
            var translationPrompt = @"You are a professional translator specializing in technical and business documents. Translate the following Traditional Chinese text to English while maintaining:
1. The exact same meaning and tone
2. Professional business English appropriate for government and enterprise contexts
3. All technical terminology correctly translated

Translate the following text:";

            var translatedText = await _aiService.GenerateContentAsync(
                translationPrompt,
                $"{translationPrompt}\n\n{chineseText}",
                temperature: 0.3,
                maxTokens: 500
            );

            if (string.IsNullOrEmpty(translatedText))
            {
                _logger.LogWarning("翻譯結果為空，返回原始中文版本");
                return chineseText;
            }

            _logger.LogInformation("成功將中文翻譯為英文");
            return translatedText.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "翻譯失敗，返回原始中文版本");
            return chineseText;
        }
    }
}
