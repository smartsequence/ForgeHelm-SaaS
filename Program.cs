using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;

// 使用自定義環境變數 DOCENGINE_ENVIRONMENT，避免與同一伺服器上的其他 ASP.NET Core 應用程式衝突
// 如果 DOCENGINE_ENVIRONMENT 不存在，預設為 Production（生產環境）
var docEngineEnv = Environment.GetEnvironmentVariable("DOCENGINE_ENVIRONMENT");
var environmentName = !string.IsNullOrEmpty(docEngineEnv) 
    ? docEngineEnv 
    : "Production"; // 預設為生產環境，確保安全性

// 使用 WebApplicationOptions 直接指定環境名稱，不依賴 ASPNETCORE_ENVIRONMENT
var options = new WebApplicationOptions
{
    EnvironmentName = environmentName
};

var builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// 配置 OpenAI HttpClient
builder.Services.AddHttpClient<DocEngine.Services.OpenAIService>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var apiKey = configuration["OpenAI:ApiKey"]?.Trim();
    
    client.Timeout = TimeSpan.FromSeconds(60);
    client.BaseAddress = new Uri("https://api.openai.com/v1/");
    
    if (!string.IsNullOrEmpty(apiKey))
    {
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
    }
});

// 註冊 AI 相關服務
builder.Services.AddScoped<DocEngine.Services.TranslationService>();
builder.Services.AddScoped<DocEngine.Services.AIContentService>();

// 註冊報告編號服務
builder.Services.AddScoped<DocEngine.Services.ReportIdService>();

// 配置 SignalR
builder.Services.AddSignalR();

// 註冊 Agent 服務（Singleton，因為需要維護全局狀態）
// 注意：需要在 AddSignalR 之後註冊，以便可以注入 IHubContext
builder.Services.AddSingleton<DocEngine.Services.AgentService>(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<DocEngine.Services.AgentService>>();
    var hubContext = serviceProvider.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<DocEngine.Hubs.AgentHub>>();
    return new DocEngine.Services.AgentService(logger, hubContext);
});

// 配置 Session（用於安全存儲問卷數據）
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // 開發環境延長 Session 過期時間（方便測試），生產環境使用 1 小時
    var isDevelopment = builder.Environment.IsDevelopment();
    options.IdleTimeout = isDevelopment ? TimeSpan.FromHours(24) : TimeSpan.FromHours(1);
    
    options.Cookie.HttpOnly = true; // 防止 XSS 攻擊
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS 環境使用 Secure
    options.Cookie.SameSite = SameSiteMode.Lax; // 允許從外部支付網站跳轉回來時攜帶 Cookie（仍防止 CSRF POST 攻擊）
    options.Cookie.Name = ".DocEngine.Session";
});

// 配置本地化服務
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "zh-TW", "en-US" };
    options.SetDefaultCulture("zh-TW")
           .AddSupportedCultures(supportedCultures)
           .AddSupportedUICultures(supportedCultures);
    
    // 優先順序：1. Cookie（用戶最近選擇的語言） 2. URL 參數 3. 預設語言
    // CookieRequestCultureProvider 預設使用 .AspNetCore.Culture Cookie，格式為 c=zh-TW|uic=zh-TW
    options.RequestCultureProviders.Clear();
    options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
    options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// HTTPS 重定向（在開發環境中，如果只使用 HTTP，這個警告是正常的，不影響功能）
app.UseHttpsRedirection();

// 啟用靜態文件服務（必須在 UseRouting() 之前）
app.UseStaticFiles();

app.UseRouting();

// 啟用 Session（必須在 UseRouting() 之後，UseEndpoints() 之前）
app.UseSession();

// 啟用本地化中間件
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// 映射 SignalR Hub
app.MapHub<DocEngine.Hubs.AgentHub>("/agentHub");

app.Run();
