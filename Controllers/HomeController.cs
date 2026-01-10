using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using DocEngine.Models;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace DocEngine.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private const int PORT = 5163; //定義Port號

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult Report()
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
        // Log the survey data
        _logger.LogInformation("Survey submitted: {Data}", System.Text.Json.JsonSerializer.Serialize(data));
        
        // Return success response
        return Json(new { success = true, message = "Survey submitted successfully" });
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
        
        // ✅ URL Encode
        checkValue = System.Web.HttpUtility.UrlEncode(checkValue).ToLower();
        
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
}
