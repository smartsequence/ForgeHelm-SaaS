using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ForgeHelm.SaaS.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using System;
using System.Text.RegularExpressions;
using ForgeHelm.SaaS.Services;
using Microsoft.AspNetCore.Hosting;

namespace ForgeHelm.SaaS.Controllers;

public class HomeController : Controller
{
    private const int PORT = 5163; //定義Port號
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient; // 保留用於向後兼容，逐步遷移到服務
    private readonly AIContentService _aiContentService;
    private readonly TranslationService _translationService;
    private readonly ReportIdService _reportIdService;
    private readonly IWebHostEnvironment _environment;

    public HomeController(
        ILogger<HomeController> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        AIContentService aiContentService,
        TranslationService translationService,
        ReportIdService reportIdService,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _configuration = configuration;
        _aiContentService = aiContentService;
        _translationService = translationService;
        _reportIdService = reportIdService;
        _environment = environment;

        // 配置 OpenAI HttpClient（保留用於向後兼容）
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.Timeout = TimeSpan.FromSeconds(60);
        var apiKey = _configuration["OpenAI:ApiKey"]?.Trim();
        
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            var keyPreview = apiKey.Length > 10 ? $"{apiKey.Substring(0, 7)}...{apiKey.Substring(apiKey.Length - 4)}" : "***";
            _logger.LogInformation("API Key 已設置（長度: {Length}, 預覽: {Preview}）", apiKey.Length, keyPreview);
        }
        else
        {
            _logger.LogError("API Key 未設置！請設置環境變數 OpenAI__ApiKey 或在 appsettings.json 中配置");
        }
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
    }

    public IActionResult Index()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Privacy()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Assessment()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Projects()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Documentation()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Risks()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Team()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Analytics()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Security()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        return View();
    }

    public IActionResult Settings()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
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
            
            // 強制保存 Session（確保數據已寫入）
            HttpContext.Session.CommitAsync().Wait();
            
            _logger.LogInformation("Survey data saved to session: {SessionId}, Data keys: {Keys}", 
                HttpContext.Session.Id, string.Join(", ", data.Keys));
            
            return Json(new { success = true, message = "Survey submitted successfully", sessionId = HttpContext.Session.Id });
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
            var sessionId = HttpContext.Session.Id;
            var surveyDataJson = HttpContext.Session.GetString("SurveyData");
            var timestamp = HttpContext.Session.GetString("SurveyTimestamp");
            
            _logger.LogInformation("GetSurveyData 請求，SessionId: {SessionId}, 是否有數據: {HasData}", 
                sessionId, !string.IsNullOrEmpty(surveyDataJson));
            
            if (string.IsNullOrEmpty(surveyDataJson))
            {
                _logger.LogWarning("Session 中沒有問卷數據，SessionId: {SessionId}", sessionId);
                return Json(new { success = false, message = "No survey data found", sessionId = sessionId });
            }
            
            var surveyData = JsonSerializer.Deserialize<Dictionary<string, string>>(surveyDataJson);
            
            // 檢查 deserialize 是否成功
            if (surveyData == null)
            {
                _logger.LogWarning("Session 中的問卷數據無法反序列化，SessionId: {SessionId}, 數據內容: {Data}", 
                    sessionId, surveyDataJson?.Substring(0, Math.Min(200, surveyDataJson?.Length ?? 0)));
                return Json(new { success = false, message = "Failed to deserialize survey data", sessionId = sessionId });
            }
            
            // 檢查是否有 M1-M5 分數
            var hasMValues = surveyData.ContainsKey("M1") || surveyData.ContainsKey("M2") || 
                           surveyData.ContainsKey("M3") || surveyData.ContainsKey("M4") || 
                           surveyData.ContainsKey("M5");
            
            _logger.LogInformation("Session 中有問卷數據，SessionId: {SessionId}, 數據大小: {Size}, 有M值: {HasM}", 
                sessionId, surveyDataJson.Length, hasMValues);
            
            if (!hasMValues)
            {
                _logger.LogWarning("Session 中有數據但沒有 M1-M5 分數，SessionId: {SessionId}, 數據鍵: {Keys}", 
                    sessionId, string.Join(", ", surveyData.Keys));
            }
            
            return Json(new 
            { 
                success = true, 
                data = surveyData,
                timestamp = timestamp,
                sessionId = sessionId
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
        try
        {
            // 動態獲取當前請求的基礎網址
            var scheme = Request.Scheme; // http 或 https
            var host = Request.Host.Value; // localhost:5163 或實際域名
            var baseUrl = $"{scheme}://{host}";
            
            var formData = new Dictionary<string, string>
            {
                ["MerchantID"] = "2000132",
                ["MerchantTradeNo"] = "DOC" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                ["MerchantTradeDate"] = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"),
                ["PaymentType"] = "aio",
                ["TotalAmount"] = "2990",
                ["TradeDesc"] = "Doc Engine Report",
                ["ItemName"] = "Risk Assessment Report",
                ["ReturnURL"] = $"{baseUrl}/Home/EcpayReturn",
                ["ChoosePayment"] = "Credit",
                ["EncryptType"] = "1",
                ["ClientBackURL"] = $"{baseUrl}/Home/Report",
                ["OrderResultURL"] = $"{baseUrl}/Home/EcpayReturn",
                ["NeedExtraPaidInfo"] = "N"
            };

            string hashKey = "5294y06JbISpM5x9";
            string hashIV = "v77hoKGq4kWxNNIS";
            
            formData["CheckMacValue"] = GenCheckMacValue(formData, hashKey, hashIV);
            
            _logger.LogInformation("支付表單資料已生成，BaseUrl: {BaseUrl}", baseUrl);
            return Json(formData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成支付表單資料失敗");
            return Json(new { success = false, message = "生成支付表單資料失敗: " + ex.Message });
        }
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

    [HttpPost]
    [HttpGet]
    public IActionResult EcpayReturn()
    {
        try
        {
            // 綠界支付完成後會 POST 或 GET 回傳結果
            // 檢查 Session 中是否有問卷數據
            var surveyDataJson = HttpContext.Session.GetString("SurveyData");
            var sessionId = HttpContext.Session.Id;
            
            // 記錄所有 Cookie（用於診斷）
            var cookies = Request.Cookies;
            var cookieKeys = cookies.Keys.ToList();
            var sessionCookie = cookies[".ForgeHelm.Session"];
            var cookieInfo = $"共有 {cookieKeys.Count} 個 Cookie: {string.Join(", ", cookieKeys)}";
            if (sessionCookie != null)
            {
                cookieInfo += $", Session Cookie 存在 (長度: {sessionCookie.Length})";
            }
            else
            {
                cookieInfo += ", Session Cookie 不存在";
            }
            _logger.LogInformation("支付回調收到請求，Method: {Method}, SessionId: {SessionId}, 是否有數據: {HasData}, {CookieInfo}", 
                Request.Method, sessionId, !string.IsNullOrEmpty(surveyDataJson), cookieInfo);
            
            if (string.IsNullOrEmpty(surveyDataJson))
            {
                _logger.LogWarning("支付完成但 Session 中沒有問卷數據，SessionId: {SessionId}。可能是 Cookie SameSite 問題或 Session 過期。{CookieInfo}", 
                    sessionId, cookieInfo);
                
                // 嘗試從 Query String 或 Form Data 中獲取 Session ID（如果有的話）
                // 但這不是標準做法，主要還是依賴 Cookie
                
                // 即使沒有數據，也跳轉到 Report 頁面，讓前端處理
            }
            else
            {
                // 標記為已付款
                HttpContext.Session.SetString("PaymentCompleted", "true");
                HttpContext.Session.CommitAsync().Wait(); // 強制保存
                _logger.LogInformation("支付完成，Session 中有問卷數據，SessionId: {SessionId}，數據大小: {Size} bytes", 
                    sessionId, surveyDataJson.Length);
            }
            
            // 跳轉到 Report 頁面
            return RedirectToAction("Report");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "處理支付回調失敗");
            return RedirectToAction("Report");
        }
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

            var advice = await _aiContentService.GenerateContentAsync(
                cacheKey: "PersonalizedAdvice",
                systemPrompt: systemPrompt,
                userPrompt: userPrompt,
                targetLang: lang,
                forceRegenerate: forceRegenerate,
                temperature: 0.8,
                maxTokens: 200
            );

            return Json(new { advice = advice });
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


    // 30 天行動清單 API
    [HttpPost]
    public async Task<IActionResult> GenerateActionPlan([FromBody] Dictionary<string, string> data)
    {
        try
        {
            var lang = data?.GetValueOrDefault("lang") ?? "zh-TW";
            var mKey = data?.GetValueOrDefault("mKey") ?? "";
            var mValue = data?.GetValueOrDefault("mValue") ?? "0";
            var mName = data?.GetValueOrDefault("mName") ?? "";
            var basicDeliverable = data?.GetValueOrDefault("basicDeliverable") ?? "";
            var allDeliverablesStr = data?.GetValueOrDefault("allDeliverables") ?? "[]";
            
            // 解析 allDeliverables（可能是 JSON 字符串或陣列）
            List<string> allDeliverables = new List<string>();
            try
            {
                if (allDeliverablesStr.StartsWith("["))
                {
                    allDeliverables = JsonSerializer.Deserialize<List<string>>(allDeliverablesStr) ?? new List<string>();
                }
                else
                {
                    allDeliverables = new List<string> { allDeliverablesStr };
                }
            }
            catch
            {
                allDeliverables = new List<string>();
            }
            
            var allDeliverablesText = string.Join(", ", allDeliverables);

            if (string.IsNullOrEmpty(mKey) || string.IsNullOrEmpty(basicDeliverable))
            {
                return Json(new { success = false, message = "Missing required parameters" });
            }

            var systemPrompt = @"You are a senior project management expert. Generate a 30-day action plan based on the risk assessment results. The plan should be practical, actionable, and time-bound.";

            var userPrompt = $@"Based on the risk assessment, the highest risk indicator is {mKey} ({mName}) with a score of {mValue} points (0-10 scale, lower is riskier).

The basic deliverable for this indicator is: {basicDeliverable}
All deliverables for this indicator: {allDeliverablesText}

Generate a 30-day action plan with three phases:

1. **Within 7 Days | Establish Foundation**
   - Describe what needs to be done (about the basic deliverable: {basicDeliverable})
   - Explain why this should be done first

2. **Within 14 Days | Form Mechanism**
   - Describe how to make the 7-day deliverable usable
   - Explain the mechanism to strengthen and reduce risk

3. **Within 30 Days | Practice and Verify**
   - Describe how to practice using a real case
   - Explain verification goals and feedback focus

Output format: Return a JSON object with three fields: day7, day14, day30. Each field should contain a concise, actionable description (one sentence or short paragraph).";

            var actionPlan = await _aiContentService.GenerateJsonContentAsync(
                cacheKey: "ActionPlan",
                systemPrompt: systemPrompt,
                userPrompt: userPrompt,
                requiredKeys: new[] { "day7", "day14", "day30" },
                targetLang: lang,
                forceRegenerate: false,
                temperature: 0.7,
                maxTokens: 800
            );

            if (actionPlan == null)
            {
                return Json(new { success = false, message = "Failed to generate action plan" });
            }

            return Json(new
            {
                success = true,
                actionPlan = new
                {
                    day7 = actionPlan["day7"],
                    day14 = actionPlan["day14"],
                    day30 = actionPlan["day30"]
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating action plan");
            return Json(new { success = false, message = ex.Message });
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

**CRITICAL - Structure Requirements (for translation consistency):**
- Use clear, countable bullet points (•) for each action item
- Maintain consistent structure: Each recommendation should have a title followed by bullet points
- Ensure each bullet point is a distinct, separate conclusion/action item (do not merge multiple points into one bullet)
- The structure must be easily parseable for translation to maintain exact bullet point counts

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

            var additionalInstructions = @"**CRITICAL - Content Equivalence Requirements (中英文等值交付):**

1. **Bullet Point Count**: MUST maintain EXACTLY the same number of bullet points (•) as the English source. Count each bullet point carefully - if English has 5 bullets, Chinese MUST have 5 bullets (not 4 or 6).

2. **Conclusion Granularity**: MUST maintain the same granularity level of conclusions. If English has 3 main recommendations with 4 sub-points each, Chinese MUST have the same structure (3 recommendations with 4 sub-points each). Do NOT merge or split recommendations or bullet points.

3. **Section Structure**: MUST preserve all section headings (【】), recommendation titles, and summary sections exactly as in English. Every section in English must appear in Chinese.

4. **Recommendation Count**: If English has 3 recommendations, Chinese MUST have 3 recommendations (not 2 or 4). Count recommendations carefully.

5. **Action Items Mapping**: Each bullet point in English must have a corresponding bullet point in Chinese. Maintain one-to-one mapping of conclusions/action items.

6. **Content Depth**: Text length can vary between languages, but the NUMBER and DEPTH of conclusions/points must be identical.

7. **Language Style**: Use professional consultant/advisory report style (NOT explanatory or instructional tone) - match the formality level of the English source.

8. **Voice**: Use authoritative, analytical consultant voice - avoid casual or conversational expressions.

9. **Terminology**: All technical terminology (PG, SA, PM, PL, etc.) correctly translated. Use Taiwan Traditional Chinese official terminology only.

10. **Connectors**: Use formal connectors (「因此」、「據此」、「基於此」、「綜上所述」) instead of casual ones (「然後」、「還有」).

**Before translating, count:**
- Number of main sections (【】)
- Number of recommendations
- Number of bullet points under each recommendation
- Number of action items

**After translating, verify:**
- All counts match exactly with English source";

            var insights = await _aiContentService.GenerateContentAsync(
                cacheKey: "AIInsights",
                systemPrompt: systemPrompt,
                userPrompt: userPrompt,
                targetLang: lang,
                forceRegenerate: forceRegenerate,
                temperature: 0.8,
                maxTokens: 1200,
                additionalTranslationInstructions: additionalInstructions
            );

            return Json(new { insights = insights });
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

    // 翻譯中文內容為英文（用於開放式問題等用戶輸入內容）
    // 注意：此方法已改用 TranslationService，保留作為向後兼容
    private async Task<string> TranslateToEnglish(string chineseInsights)
    {
        // 此方法已被 TranslationService.TranslateToEnglishAsync 取代
        // 保留此方法以避免編譯錯誤，但實際使用 TranslationService
        return await _translationService.TranslateToEnglishAsync(chineseInsights);
    }

    // 翻譯 M6-M8 開放式問題內容 API
    [HttpPost]
    public async Task<IActionResult> TranslateOpenQuestions([FromBody] Dictionary<string, string> data)
    {
        try
        {
            var lang = data?.GetValueOrDefault("lang") ?? "zh-TW";
            var open1 = data?.GetValueOrDefault("open1") ?? data?.GetValueOrDefault("m6") ?? "";
            var open2 = data?.GetValueOrDefault("open2") ?? data?.GetValueOrDefault("m7") ?? "";
            var open3 = data?.GetValueOrDefault("open3") ?? data?.GetValueOrDefault("m8") ?? "";

            var isEnglish = lang == "en-US";
            
            // 如果語言是中文，直接返回原始內容
            if (!isEnglish)
            {
                return Json(new 
                { 
                    success = true,
                    open1 = open1,
                    open2 = open2,
                    open3 = open3
                });
            }

            // 如果語言是英文，翻譯中文內容為英文
            // 使用翻譯服務翻譯用戶輸入的開放式問題
            var translatedOpen1 = string.IsNullOrWhiteSpace(open1) ? "" : await _translationService.TranslateToEnglishAsync(open1);
            var translatedOpen2 = string.IsNullOrWhiteSpace(open2) ? "" : await _translationService.TranslateToEnglishAsync(open2);
            var translatedOpen3 = string.IsNullOrWhiteSpace(open3) ? "" : await _translationService.TranslateToEnglishAsync(open3);

            return Json(new 
            { 
                success = true,
                open1 = translatedOpen1,
                open2 = translatedOpen2,
                open3 = translatedOpen3
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "翻譯 M6-M8 內容失敗");
            var lang = data?.GetValueOrDefault("lang") ?? "zh-TW";
            var isEnglish = lang == "en-US";
            var errorMsg = isEnglish 
                ? $"Failed to translate open questions: {ex.Message}"
                : $"翻譯開放式問題失敗：{ex.Message}";
            return Json(new { success = false, message = errorMsg });
        }
    }

    // 保存報告編號到 Session
    [HttpPost]
    public IActionResult SaveReportId([FromBody] Dictionary<string, string> data)
    {
        try
        {
            var reportId = data?.GetValueOrDefault("reportId") ?? "";
            if (!string.IsNullOrEmpty(reportId))
            {
                HttpContext.Session.SetString("ReportId", reportId);
                return Json(new { success = true });
            }
            return Json(new { success = false, message = "Report ID is empty" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存報告編號失敗");
            return Json(new { success = false, message = ex.Message });
        }
    }

    // 獲取報告編號（從 Session）
    [HttpGet]
    public IActionResult GetReportId()
    {
        try
        {
            var reportId = HttpContext.Session.GetString("ReportId");
            return Json(new { success = true, reportId = reportId ?? "-" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取報告編號失敗");
            return Json(new { success = false, message = ex.Message });
        }
    }

    // 生成報告編號 API
    [HttpPost]
    public async Task<IActionResult> GenerateReportId([FromBody] Dictionary<string, string> data)
    {
        try
        {
            var systemCode = data?.GetValueOrDefault("systemCode") ?? "";
            
            if (string.IsNullOrWhiteSpace(systemCode))
            {
                return Json(new { success = false, message = "系統代號不能為空" });
            }

            var reportId = await _reportIdService.GenerateReportIdAsync(systemCode);
            
            return Json(new { success = true, reportId = reportId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成報告編號失敗");
            return Json(new { success = false, message = ex.Message });
        }
    }

    // 設置語言偏好（使用 HttpOnly Cookie）
    [HttpPost]
    public IActionResult SetLanguage([FromBody] Dictionary<string, string> data)
    {
        try
        {
            var lang = data?.GetValueOrDefault("lang") ?? "zh-TW";
            
            // CookieRequestCultureProvider 需要的格式：c=zh-TW|uic=zh-TW
            var cookieValue = $"c={lang}|uic={lang}";
            
            // 設置 HttpOnly Cookie（更安全）
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,  // 防止 XSS 攻擊
                Secure = Request.IsHttps,  // HTTPS 環境使用 Secure
                SameSite = SameSiteMode.Lax,  // 使用 Lax 以確保跨頁面導航時 Cookie 可用（仍防止 CSRF POST 攻擊）
                Expires = DateTimeOffset.UtcNow.AddYears(1),  // 1 年有效期
                Path = "/"
            };
            
            // 設置 ASP.NET Core 本地化 Cookie（CookieRequestCultureProvider 使用）
            Response.Cookies.Append(".AspNetCore.Culture", cookieValue, cookieOptions);
            
            // 同時設置簡單的 lang Cookie（前端 i18n.js 使用）
            var simpleCookieOptions = new CookieOptions
            {
                HttpOnly = false,  // 前端需要讀取
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                Path = "/"
            };
            Response.Cookies.Append("lang", lang, simpleCookieOptions);
            
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設置語言 Cookie 失敗");
            return Json(new { success = false, message = ex.Message });
        }
    }

    // Report頁使用
    public IActionResult Report()
    {
        ViewData["IsDevelopment"] = _environment.IsDevelopment();
        ViewData["Environment"] = _environment.EnvironmentName;
        // 前端通過 Session 和 AJAX 獲取數據，這裡不需要讀取表單
        // 如果需要從後端傳遞數據，可以在 ViewBag 中設置
        ViewBag.AIEInsights = "【建議】等待AI分析...";
        return View();
    }    
}
