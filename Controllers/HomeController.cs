using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DocEngine.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;  // ✅ appsettings
using System;

namespace DocEngine.Controllers;

public class HomeController : Controller
{
    private const int PORT = 5163; //定義Port號
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60); // 設置 60 秒超時
        // ASP.NET Core 配置系統會自動從環境變數讀取（優先順序：環境變數 > appsettings.json）
        // 環境變數名稱：OpenAI__ApiKey（雙底線 __ 會被轉換為配置鍵的冒號 :）
        var apiKey = _configuration["OpenAI:ApiKey"]?.Trim();
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var keyPreview = apiKey.Length > 10 ? $"{apiKey.Substring(0, 7)}...{apiKey.Substring(apiKey.Length - 4)}" : "***";
            _logger.LogInformation("API Key 已設置到 HttpClient（長度: {Length}, 預覽: {Preview}）", apiKey.Length, keyPreview);
        }
        else
        {
            _logger.LogError("API Key 未設置！請設置環境變數 OpenAI__ApiKey 或在 appsettings.json 中配置");
        }
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Assessment()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SubmitSurvey([FromBody] Dictionary<string, string> data)
    {
        try
        {
            // 將問卷數據存儲在 Session 中（服務端存儲，更安全）
            HttpContext.Session.SetString("SurveyData", JsonSerializer.Serialize(data));
            
            // 同時存儲時間戳
            HttpContext.Session.SetString("SurveyTimestamp", DateTime.Now.ToString("O"));
            
            _logger.LogInformation("Survey data saved to session: {SessionId}", HttpContext.Session.Id);
            
            return Json(new { success = true, message = "Survey submitted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save survey data");
            return Json(new { success = false, message = "Failed to save survey data" });
        }
    }

    // 獲取問卷數據（從 Session）
    [HttpGet]
    public IActionResult GetSurveyData()
    {
        try
        {
            var surveyDataJson = HttpContext.Session.GetString("SurveyData");
            var timestamp = HttpContext.Session.GetString("SurveyTimestamp");
            
            if (string.IsNullOrEmpty(surveyDataJson))
            {
                return Json(new { success = false, message = "No survey data found" });
            }
            
            var surveyData = JsonSerializer.Deserialize<Dictionary<string, string>>(surveyDataJson);
            
            return Json(new 
            { 
                success = true, 
                data = surveyData,
                timestamp = timestamp
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve survey data");
            return Json(new { success = false, message = "Failed to retrieve survey data" });
        }
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    public IActionResult GetEcpayFormData()
    {
        var baseUrl = $"http://localhost:{PORT}";  // ✅ 組成基礎網址
        
        var formData = new Dictionary<string, string>
        {
            ["MerchantID"] = "2000132",
            ["MerchantTradeNo"] = "DOC" + DateTime.Now.ToString("yyyyMMddHHmmss"),
            ["MerchantTradeDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
            ["PaymentType"] = "aio",
            ["TotalAmount"] = "2990",
            ["TradeDesc"] = "Doc Engine Report",
            ["ItemName"] = "Risk Assessment Report",
            ["ReturnURL"] = $"{baseUrl}/Home/EcpayReturn",  // ✅ 使用變數
            ["ChoosePayment"] = "Credit",
            ["EncryptType"] = "1",
            ["ClientBackURL"] = $"{baseUrl}/Home/Report",  // ✅ 使用變數
            ["OrderResultURL"] = $"{baseUrl}/Home/EcpayReturn",  // ✅ 使用變數
            ["NeedExtraPaidInfo"] = "N"
        };

        string hashKey = "5294y06JbISpM5x9";
        string hashIV = "v77hoKGq4kWxNNIS";
        
        formData["CheckMacValue"] = GenCheckMacValue(formData, hashKey, hashIV);
        
        return Json(formData);
    }

    private string GenCheckMacValue(Dictionary<string, string> parameters, string hashKey, string hashIV)
    {
        // ✅ 排除 CheckMacValue 欄位
        var sortedParams = parameters
            .Where(x => x.Key != "CheckMacValue")
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key}={x.Value}");
        
        // ✅ 組合字串
        var checkValue = $"HashKey={hashKey}&{string.Join("&", sortedParams)}&HashIV={hashIV}";
        
        // ✅ URL Encode (使用 ASP.NET Core 的 WebUtility)
        checkValue = System.Net.WebUtility.UrlEncode(checkValue).ToLower();
        
        // ✅ SHA256 加密
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(checkValue));
        return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
    }

    public IActionResult EcpayReturn()
    {
        // 綠界跳回成功頁
        return View("Report");
    }

    // 風險改善建議 API（基於 M1-M5 分數）
    [HttpPost]
    public async Task<IActionResult> GeneratePersonalizedAdvice([FromBody] Dictionary<string, string> data)
    {
        try
        {
            var lang = data?.GetValueOrDefault("lang") ?? "zh-TW";
            var forceRegenerate = data?.GetValueOrDefault("forceRegenerate") == "true";
            var m1 = data?.GetValueOrDefault("m1") ?? data?.GetValueOrDefault("M1") ?? "0";
            var m2 = data?.GetValueOrDefault("m2") ?? data?.GetValueOrDefault("M2") ?? "0";
            var m3 = data?.GetValueOrDefault("m3") ?? data?.GetValueOrDefault("M3") ?? "0";
            var m4 = data?.GetValueOrDefault("m4") ?? data?.GetValueOrDefault("M4") ?? "0";
            var m5 = data?.GetValueOrDefault("m5") ?? data?.GetValueOrDefault("M5") ?? "0";

            var isEnglish = lang == "en-US";
            
            // 如果強制重新生成，清除Session中的緩存（包括舊的中文版本，確保使用先英後中的邏輯）
            if (forceRegenerate)
            {
                HttpContext.Session.Remove("PersonalizedAdvice_en-US");
                HttpContext.Session.Remove("PersonalizedAdvice_zh-TW");
                _logger.LogInformation("強制重新生成，已清除Session緩存");
            }
            
            // 優先生成英文版本作為基礎（避免大陸用語問題），確保內容一致性
            // 檢查 Session 中是否已有英文版本
            var cachedEnglishAdvice = HttpContext.Session.GetString("PersonalizedAdvice_en-US");
            if (!string.IsNullOrEmpty(cachedEnglishAdvice) && !forceRegenerate)
            {
                _logger.LogInformation("發現已存在的英文版本風險改善建議");
                if (isEnglish)
                {
                    // 直接返回英文版本
                    return Json(new { advice = cachedEnglishAdvice });
                }
                else
                {
                    // 翻譯英文版本為繁體中文
                    _logger.LogInformation("將英文版本翻譯為繁體中文");
                    var translatedAdvice = await TranslateEnglishToTraditionalChinese(cachedEnglishAdvice);
                    return Json(new { advice = translatedAdvice });
                }
            }
            
            // 如果沒有英文版本，檢查是否有中文版本（向後兼容，但不強制重新生成時才使用）
            if (isEnglish && !forceRegenerate)
            {
                var chineseAdvice = HttpContext.Session.GetString("PersonalizedAdvice_zh-TW");
                if (!string.IsNullOrEmpty(chineseAdvice))
                {
                    // 有中文版本，翻譯為英文（保留原有邏輯）
                    _logger.LogInformation("發現中文版本的風險改善建議，翻譯為英文");
                    var translatedAdvice = await TranslateAdviceToEnglish(chineseAdvice);
                    // 同時存儲英文版本，確保一致性
                    HttpContext.Session.SetString("PersonalizedAdvice_en-US", translatedAdvice);
                    return Json(new { advice = translatedAdvice });
                }
            }

            // 生成英文版本（作為基礎，避免大陸用語）
            var systemPrompt = @"You are a senior project management and documentation risk assessment expert. Based on the user's risk assessment scores, provide actionable risk improvement recommendations.

Analysis requirements:
1. Identify the weakest areas based on M1-M5 maturity scores (0-10 points)
2. Provide specific, actionable improvement recommendations
3. Focus on the most critical issues

Output format:
Provide 1-2 concise, actionable recommendations in professional business English.";

            var userPrompt = $@"Risk Assessment Scores:
• M1 Handover: {m1} points
• M2 Traceability: {m2} points
• M3 Change: {m3} points
• M4 Acceptance: {m4} points
• M5 Communication: {m5} points

Please provide risk improvement recommendations.";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.8,
                max_tokens = 200
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            
            var englishAdvice = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // 存儲英文版本到 Session（作為基礎）
            if (!string.IsNullOrEmpty(englishAdvice))
            {
                HttpContext.Session.SetString("PersonalizedAdvice_en-US", englishAdvice);
                _logger.LogInformation("已將英文版本的風險改善建議存儲到 Session");
                
                if (isEnglish)
                {
                    // 直接返回英文版本
                    return Json(new { advice = englishAdvice });
                }
                else
                {
                    // 翻譯英文版本為繁體中文（避免大陸用語）
                    var translatedAdvice = await TranslateEnglishToTraditionalChinese(englishAdvice);
                    return Json(new { advice = translatedAdvice });
                }
            }
            
            return Json(new { advice = englishAdvice });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "風險改善建議生成失敗");
            var lang = data?.GetValueOrDefault("lang") ?? "zh-TW";
            var errorMsg = lang == "en-US" 
                ? $"Failed to generate risk improvement recommendations: {ex.Message}"
                : $"風險改善建議生成失敗：{ex.Message}";
            return Json(new { advice = errorMsg });
        }
    }

    // 翻譯英文建議為台灣繁體中文（使用明確的台灣用詞指南，避免大陸用語）
    private async Task<string> TranslateEnglishToTraditionalChinese(string englishAdvice)
    {
        try
        {
            var translationPrompt = @"You are a professional translator specializing in translating English technical and business documents to Traditional Chinese (Taiwan standard). 

**CRITICAL: You MUST use Taiwan Traditional Chinese official terminology. ABSOLUTELY FORBIDDEN to use Mainland China Simplified Chinese or Mainland Chinese colloquialisms (even if they are Traditional Chinese). This is an official government report, and the terminology must comply with Taiwan official standards.**

**ABSOLUTELY FORBIDDEN Mainland Chinese terms (even if Traditional Chinese):**
- FORBIDDEN 「質量」→ USE 「品質」
- FORBIDDEN 「渠道」→ USE 「管道」、「途徑」
- FORBIDDEN 「數據」→ USE 「資料」
- FORBIDDEN 「網絡」→ USE 「網路」
- FORBIDDEN 「軟件」→ USE 「軟體」
- FORBIDDEN 「信息」→ USE 「資訊」
- FORBIDDEN 「項目」→ USE 「專案」
- FORBIDDEN 「招聘」→ USE 「招募」、「徵才」
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

**Language style requirements:**
- Use formal official terminology, avoid colloquial expressions
- Avoid excessive use of colloquial particles like 「的」、「了」
- Use complete, rigorous sentence structures
- Comply with Taiwan government official document style
- Use 「應」、「應該」not 「应」、「应该」
- Use 「將」、「將會」not 「将」、「将会」
- Use 「於」、「在」not 「于」、「在」(Note: Taiwan mostly uses 「在」)
- Use 「與」、「及」not 「与」、「及」(Note: Taiwan mostly uses 「與」)

Translate the following English text to Taiwan Traditional Chinese while maintaining:
1. The exact same meaning and tone
2. Professional business language appropriate for government and enterprise contexts
3. Keep it concise (1-2 sentences as the original)
4. Use Taiwan Traditional Chinese official terminology only

Translate the following text:";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = translationPrompt },
                    new { role = "user", content = englishAdvice }
                },
                temperature = 0.3,  // 較低溫度，確保翻譯一致性和用詞準確性
                max_tokens = 500  // 增加token數量，避免截斷
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            
            var translatedText = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            
            if (string.IsNullOrEmpty(translatedText))
            {
                _logger.LogWarning("翻譯結果為空，返回原始英文版本");
                return englishAdvice;
            }
            
            _logger.LogInformation("成功將英文風險改善建議翻譯為台灣繁體中文");
            return translatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "翻譯失敗，返回原始英文版本");
            // 如果翻譯失敗，返回英文版本（前端可以處理）
            return englishAdvice;
        }
    }

    // 翻譯中文建議為英文（保持格式和結構一致）- 保留作為向後兼容
    private async Task<string> TranslateAdviceToEnglish(string chineseAdvice)
    {
        try
        {
            var translationPrompt = @"You are a professional translator specializing in technical and business documents. Translate the following Traditional Chinese text to English while maintaining:

1. The exact same meaning and tone
2. Professional business English appropriate for government and enterprise contexts
3. Keep it concise (1-2 sentences as the original)

Translate the following text:";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = translationPrompt },
                    new { role = "user", content = chineseAdvice }
                },
                temperature = 0.3,  // 較低溫度，確保翻譯一致性
                max_tokens = 300
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            
            var translatedText = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            
            if (string.IsNullOrEmpty(translatedText))
            {
                _logger.LogWarning("翻譯結果為空，返回原始中文版本");
                return chineseAdvice;
            }
            
            _logger.LogInformation("成功將中文風險改善建議翻譯為英文");
            return translatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "翻譯失敗，返回原始中文版本");
            // 如果翻譯失敗，返回中文版本（前端可以處理）
            return chineseAdvice;
        }
    }

    // M6-M8 AI摘要API
    [HttpPost]
    public async Task<IActionResult> GenerateInsights([FromBody] Dictionary<string, string> m678)
    {
        try
        {
            var lang = m678?.GetValueOrDefault("lang") ?? "zh-TW";
            var forceRegenerate = m678?.GetValueOrDefault("forceRegenerate") == "true";
            // 從請求體中提取 M1-M5 分數和 M6-M8 開放式問題
            var m1 = m678?.GetValueOrDefault("m1") ?? m678?.GetValueOrDefault("M1") ?? "0";
            var m2 = m678?.GetValueOrDefault("m2") ?? m678?.GetValueOrDefault("M2") ?? "0";
            var m3 = m678?.GetValueOrDefault("m3") ?? m678?.GetValueOrDefault("M3") ?? "0";
            var m4 = m678?.GetValueOrDefault("m4") ?? m678?.GetValueOrDefault("M4") ?? "0";
            var m5 = m678?.GetValueOrDefault("m5") ?? m678?.GetValueOrDefault("M5") ?? "0";
            var m6 = m678?.GetValueOrDefault("m6") ?? m678?.GetValueOrDefault("M6") ?? "";
            var m7 = m678?.GetValueOrDefault("m7") ?? m678?.GetValueOrDefault("M7") ?? "";
            var m8 = m678?.GetValueOrDefault("m8") ?? m678?.GetValueOrDefault("M8") ?? "";

            var isEnglish = lang == "en-US";

            // 如果沒有開放式問題數據，返回提示
            if (string.IsNullOrWhiteSpace(m6) && string.IsNullOrWhiteSpace(m7) && string.IsNullOrWhiteSpace(m8))
            {
                var noDataMsg = isEnglish 
                    ? "No open-ended question data provided"
                    : "未提供開放式問題數據";
                return Json(new { insights = noDataMsg });
            }

            // 如果強制重新生成，清除Session中的緩存
            if (forceRegenerate)
            {
                HttpContext.Session.Remove("AIInsights_en-US");
                HttpContext.Session.Remove("AIInsights_zh-TW");
                _logger.LogInformation("強制重新生成，已清除Session緩存");
            }

            // 優先生成英文版本作為基礎（避免大陸用語問題），確保內容一致性
            // 檢查 Session 中是否已有英文版本
            var cachedEnglishInsights = HttpContext.Session.GetString("AIInsights_en-US");
            if (!string.IsNullOrEmpty(cachedEnglishInsights) && !forceRegenerate)
            {
                _logger.LogInformation("發現已存在的英文版本 AI 洞察");
                if (isEnglish)
                {
                    // 直接返回英文版本
                    return Json(new { insights = cachedEnglishInsights });
                }
                else
                {
                    // 翻譯英文版本為繁體中文
                    _logger.LogInformation("將英文版本翻譯為繁體中文");
                    var translatedInsights = await TranslateEnglishToTraditionalChineseForInsights(cachedEnglishInsights);
                    return Json(new { insights = translatedInsights });
                }
            }
            
            // 如果沒有英文版本，檢查是否有中文版本（向後兼容，但不強制重新生成時才使用）
            if (isEnglish && !forceRegenerate)
            {
                var chineseInsights = HttpContext.Session.GetString("AIInsights_zh-TW");
                if (!string.IsNullOrEmpty(chineseInsights))
                {
                    // 有中文版本，翻譯為英文（保留原有邏輯）
                    _logger.LogInformation("發現中文版本的 AI 洞察，翻譯為英文");
                    var translatedInsights = await TranslateToEnglish(chineseInsights);
                    // 同時存儲英文版本，確保一致性
                    HttpContext.Session.SetString("AIInsights_en-US", translatedInsights);
                    return Json(new { insights = translatedInsights });
                }
            }

            // 生成英文版本（作為基礎，避免大陸用語）
            // 構建 OpenAI API 請求 - 統一使用英文 prompt
            var systemPrompt = @"You are a senior project management and documentation risk assessment expert. Deeply analyze the user's risk assessment results, combining quantitative scores and qualitative descriptions to identify root causes and provide specific, actionable recommendations.

Important terminology clarification:
- PG = Programmer (程式設計師/程式開發人員), NOT Project Manager
- SA = System Analyst (系統分析師)
- PM = Project Manager (專案經理)
- PL = Project Leader (專案負責人)
- SD = System Designer (系統設計師)
- SE = System Engineer (系統工程師)
- BA = Business Analyst (業務分析師)
- QA = Quality Assurance (品質保證)
- DBA = Database Administrator (資料庫管理員)
- DevOps = Development Operations (開發運維工程師)
- SRE = Site Reliability Engineering (站點可靠性工程師)

Analysis requirements:
1. Combine M1-M5 maturity scores (0-10) and M6-M8 text descriptions for in-depth analysis
2. Identify root causes, not surface symptoms
3. Provide 3-5 prioritized specific improvement recommendations, each containing multiple actionable steps
4. Each recommendation must be actionable with bulleted specific action items
5. Total word count should be approximately 600-800 words, ensuring detailed and complete content
6. Use bulleted list format to make recommendations clear and easy to read
7. Ensure complete output without truncation - use full sentences and finish all thoughts

Output format (in English):
【Core Issue】
Summarize the most critical problem in 2-3 sentences, explaining the root cause.

【Recommendation 1】Title of highest priority recommendation
• Specific action item one
• Specific action item two
• Specific action item three
(Add more action items as needed)

【Recommendation 2】Title of second priority recommendation
• Specific action item one
• Specific action item two
• Specific action item three

【Recommendation 3】Other important improvement directions (if needed)
• Specific action item one
• Specific action item two

【Summary】
Summarize the expected effects after implementing these recommendations in 1-2 sentences.

IMPORTANT: Complete your response fully within the specified length. Use bulleted lists for all action items. Do not cut off mid-sentence.";

            var indicatorLabels = new { M1 = "M1 Handover", M2 = "M2 Traceability", M3 = "M3 Change", M4 = "M4 Acceptance", M5 = "M5 Communication", Q6 = "Question 6 - Main Challenges", Q7 = "Question 7 - Areas to Improve", Q8 = "Question 8 - Other Information" };

            var userPrompt = $@"Risk Assessment Results:

Quantitative Indicators (Maturity Score 0-10):
• {indicatorLabels.M1}: {m1} points
• {indicatorLabels.M2}: {m2} points
• {indicatorLabels.M3}: {m3} points
• {indicatorLabels.M4}: {m4} points
• {indicatorLabels.M5}: {m5} points

Qualitative Descriptions (Open-ended Questions):
• {indicatorLabels.Q6}: {m6}
• {indicatorLabels.Q7}: {m7}
• {indicatorLabels.Q8}: {m8}

Please conduct a deep analysis and provide improvement recommendations.";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt }
                },
                temperature = 0.8,
                max_tokens = 1200  // 設定為 1200，支援更長篇且詳實的條列式內容（AI 會控制在 600-800 字左右）
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("發送 OpenAI API 請求，語言: {Lang}", lang);
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
            
            var englishInsights = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // 存儲英文版本到 Session（作為基礎）
            if (!string.IsNullOrEmpty(englishInsights))
            {
                HttpContext.Session.SetString("AIInsights_en-US", englishInsights);
                _logger.LogInformation("已將英文版本的 AI 洞察存儲到 Session");
                
                if (isEnglish)
                {
                    // 直接返回英文版本
                    _logger.LogInformation("AI 洞察生成成功（英文），長度: {Length}", englishInsights.Length);
                    return Json(new { insights = englishInsights });
                }
                else
                {
                    // 翻譯英文版本為繁體中文（避免大陸用語）
                    var translatedInsights = await TranslateEnglishToTraditionalChineseForInsights(englishInsights);
                    _logger.LogInformation("AI 洞察生成成功（繁體中文），長度: {Length}", translatedInsights?.Length ?? 0);
                    return Json(new { insights = translatedInsights });
                }
            }
            
            _logger.LogInformation("AI 洞察生成成功，長度: {Length}", englishInsights?.Length ?? 0);
            return Json(new { insights = englishInsights });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AI 洞察生成失敗");
            var lang = m678?.GetValueOrDefault("lang") ?? "zh-TW";
            var isEnglish = lang == "en-US";
            var errorMsg = isEnglish
                ? $"AI analysis failed: {ex.Message}"
                : $"AI 分析失敗：{ex.Message}";
            return Json(new { insights = errorMsg });
        }
    }

    // 翻譯英文洞察為台灣繁體中文（使用明確的台灣用詞指南，避免大陸用語）
    private async Task<string> TranslateEnglishToTraditionalChineseForInsights(string englishInsights)
    {
        try
        {
            var translationPrompt = @"You are a professional translator specializing in translating English technical and business documents to Traditional Chinese (Taiwan standard). 

**CRITICAL: You MUST use Taiwan Traditional Chinese official terminology. ABSOLUTELY FORBIDDEN to use Mainland China Simplified Chinese or Mainland Chinese colloquialisms (even if they are Traditional Chinese). This is an official government report, and the terminology must comply with Taiwan official standards.**

**ABSOLUTELY FORBIDDEN Mainland Chinese terms (even if Traditional Chinese):**
- FORBIDDEN 「質量」→ USE 「品質」
- FORBIDDEN 「渠道」→ USE 「管道」、「途徑」
- FORBIDDEN 「數據」→ USE 「資料」
- FORBIDDEN 「網絡」→ USE 「網路」
- FORBIDDEN 「軟件」→ USE 「軟體」
- FORBIDDEN 「信息」→ USE 「資訊」
- FORBIDDEN 「項目」→ USE 「專案」
- FORBIDDEN 「招聘」→ USE 「招募」、「徵才」
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

**Language style requirements:**
- Use formal official terminology, avoid colloquial expressions
- Avoid excessive use of colloquial particles like 「的」、「了」
- Use complete, rigorous sentence structures
- Comply with Taiwan government official document style
- Use 「應」、「應該」not 「应」、「应该」
- Use 「將」、「將會」not 「将」、「将会」
- Use 「於」、「在」not 「于」、「在」(Note: Taiwan mostly uses 「在」)
- Use 「與」、「及」not 「与」、「及」(Note: Taiwan mostly uses 「與」)

Translate the following English text to Taiwan Traditional Chinese while maintaining:
1. The exact same structure and formatting (including section headings with 【】, bullet points, etc.)
2. The same number of recommendations and action items
3. Professional business language appropriate for government and enterprise contexts
4. All technical terminology (PG, SA, PM, PL, etc.) correctly translated
5. Use Taiwan Traditional Chinese official terminology only

Translate the following text:";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = translationPrompt },
                    new { role = "user", content = englishInsights }
                },
                temperature = 0.3,  // 較低溫度，確保翻譯一致性和用詞準確性
                max_tokens = 1500
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            
            var translatedText = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            
            if (string.IsNullOrEmpty(translatedText))
            {
                _logger.LogWarning("翻譯結果為空，返回原始英文版本");
                return englishInsights;
            }
            
            _logger.LogInformation("成功將英文 AI 洞察翻譯為台灣繁體中文");
            return translatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "翻譯失敗，返回原始英文版本");
            // 如果翻譯失敗，返回英文版本（前端可以處理）
            return englishInsights;
        }
    }

    // 翻譯中文洞察為英文（保持格式和結構一致）- 保留作為向後兼容
    private async Task<string> TranslateToEnglish(string chineseInsights)
    {
        try
        {
            var translationPrompt = @"You are a professional translator specializing in technical and business documents. Translate the following Traditional Chinese text to English while maintaining:

1. The exact same structure and formatting (including section headings with 【】, bullet points, etc.)
2. The same number of recommendations and action items
3. Professional business English appropriate for government and enterprise contexts
4. All technical terminology (PG, SA, PM, PL, etc.) correctly translated

Translate the following text:";

            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "system", content = translationPrompt },
                    new { role = "user", content = chineseInsights }
                },
                temperature = 0.3,  // 較低溫度，確保翻譯一致性
                max_tokens = 1500
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseContent);
            
            var translatedText = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();
            
            if (string.IsNullOrEmpty(translatedText))
            {
                _logger.LogWarning("翻譯結果為空，返回原始中文版本");
                return chineseInsights;
            }
            
            _logger.LogInformation("成功將中文洞察翻譯為英文");
            return translatedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "翻譯失敗，返回原始中文版本");
            // 如果翻譯失敗，返回中文版本（前端可以處理）
            return chineseInsights;
        }
    }

    // Report頁使用
    public IActionResult Report()
    {
        // 前端通過 localStorage 和 AJAX 獲取數據，這裡不需要讀取表單
        // 如果需要從後端傳遞數據，可以在 ViewBag 中設置
        ViewBag.AIEInsights = "【建議】等待AI分析...";
        return View();
    }    
}
