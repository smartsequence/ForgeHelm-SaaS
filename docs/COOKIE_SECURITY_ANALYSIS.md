# Cookie 安全性分析

## 概述

Cookie 的安全性取決於**如何配置**。正確配置的 Cookie 比 localStorage **更安全**，但錯誤配置的 Cookie 可能帶來風險。

## Cookie vs localStorage 安全性比較

| 特性 | localStorage | Cookie（正確配置） | Cookie（錯誤配置） |
|------|-------------|-------------------|-------------------|
| **XSS 防護** | ❌ 完全暴露 | ✅ HttpOnly 可防護 | ❌ 暴露給 JavaScript |
| **CSRF 防護** | ✅ 不自動發送 | ✅ SameSite 可防護 | ❌ 可能被跨站請求利用 |
| **傳輸安全** | ❌ 不加密 | ✅ Secure 標誌可強制 HTTPS | ❌ 可能通過 HTTP 傳輸 |
| **服務端控制** | ❌ 無法控制 | ✅ 服務端可設置 HttpOnly | ⚠️ 部分控制 |
| **過期管理** | ❌ 手動管理 | ✅ 自動過期 | ⚠️ 需手動管理 |

## 我們目前的實現

### 1. Session Cookie（最安全）✅

在 `Program.cs` 中配置：

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;        // ✅ 防止 XSS 攻擊
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // ✅ HTTPS 環境使用 Secure
    options.Cookie.SameSite = SameSiteMode.Strict; // ✅ CSRF 防護
    options.Cookie.Name = ".ForgeHelm.Session";
});
```

**安全性評級：⭐⭐⭐⭐⭐（最高）**

- ✅ **HttpOnly**: JavaScript 無法訪問，防止 XSS 攻擊
- ✅ **Secure**: HTTPS 環境自動使用 Secure 標誌
- ✅ **SameSite=Strict**: 防止 CSRF 攻擊
- ✅ **自動過期**: 30 分鐘無活動自動過期

### 2. 語言偏好 Cookie（中等安全）⚠️

在 `i18n.js` 中設置（客戶端）：

```javascript
function setCookie(name, value, days = 365) {
    const expires = new Date();
    expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
    document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/;SameSite=Strict`;
}
```

**安全性評級：⭐⭐⭐（中等）**

- ✅ **SameSite=Strict**: 防止 CSRF 攻擊
- ❌ **無 HttpOnly**: JavaScript 可以訪問（但語言偏好不是敏感數據）
- ❌ **無 Secure**: 在 HTTP 環境下可能被竊聽（但語言偏好不是敏感數據）

在 `HomeController.cs` 中設置（服務端）：

```csharp
var cookieOptions = new CookieOptions
{
    HttpOnly = true,              // ✅ 防止 XSS
    Secure = Request.IsHttps,     // ✅ HTTPS 環境使用 Secure
    SameSite = SameSiteMode.Strict, // ✅ CSRF 防護
    Expires = DateTimeOffset.UtcNow.AddYears(1),
    Path = "/"
};
Response.Cookies.Append("lang", lang, cookieOptions);
```

**安全性評級：⭐⭐⭐⭐⭐（最高）**

- ✅ **HttpOnly**: JavaScript 無法訪問
- ✅ **Secure**: HTTPS 環境使用 Secure
- ✅ **SameSite=Strict**: 防止 CSRF

## 安全建議

### 當前實現的改進建議

1. **語言偏好 Cookie（客戶端設置）**
   - 建議：移除客戶端 Cookie 設置，完全依賴服務端 HttpOnly Cookie
   - 原因：客戶端設置的 Cookie 無法設置 HttpOnly，存在 XSS 風險

2. **生產環境配置**
   - 建議：強制使用 HTTPS（`Secure = true`）
   - 建議：設置 `SameSite = SameSiteMode.Strict`（已實現）

### 最佳實踐

1. **敏感數據**：使用 HttpOnly Cookie + Secure + SameSite=Strict
2. **非敏感數據**（如語言偏好）：可以使用客戶端 Cookie，但建議仍使用 HttpOnly
3. **Session 數據**：完全使用服務端 Session（已實現 ✅）

## 結論

### Cookie 是否安全？

**答案：取決於配置**

- ✅ **正確配置的 Cookie（HttpOnly + Secure + SameSite）**：比 localStorage **更安全**
- ⚠️ **部分配置的 Cookie**：安全性介於 localStorage 和正確配置的 Cookie 之間
- ❌ **錯誤配置的 Cookie**：可能比 localStorage **更危險**（因為會自動發送到服務端）

### 我們目前的實現

- ✅ **Session Cookie**：完全安全（HttpOnly + Secure + SameSite）
- ✅ **語言偏好（服務端）**：完全安全（HttpOnly + Secure + SameSite）
- ⚠️ **語言偏好（客戶端）**：中等安全（只有 SameSite），建議改進

### 建議改進

移除客戶端 Cookie 設置，完全依賴服務端 HttpOnly Cookie 來保存語言偏好。
