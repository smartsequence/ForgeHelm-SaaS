# 評估問卷功能系統設計文件 (System Design Document)

**文件版本：** v1.0  
**最後更新：** 2024-12-XX  
**作者：** ForgeHelm Development Team  
**功能名稱：** 系統風險評估問卷 (Risk Assessment Questionnaire)

---

## 1. 功能概述 (Feature Overview)

### 1.1 功能描述
評估問卷功能允許使用者填寫系統風險評估問卷，包含5個成熟度指標(M1-M5)和3個開放式問題。填寫完成後，系統將引導使用者進行支付(NT$2,990)，支付成功後即可查看專屬的風險評估報告。

### 1.2 主要特性
- ✅ 5個成熟度指標滑桿評估（0-10分）
- ✅ 3個開放式問題輸入
- ✅ 系統名稱輸入
- ✅ 即時分數描述顯示
- ✅ 自動儲存功能（localStorage）
- ✅ 多語言支援（繁體中文/English）
- ✅ 與綠界支付整合
- ✅ 響應式設計（支援桌面與行動裝置）

### 1.3 使用者角色
- **一般使用者**：填寫問卷、進行支付、查看報告

---

## 2. 系統架構 (System Architecture)

### 2.1 整體架構圖

```
┌─────────────────────────────────────────────────────────────┐
│                      使用者瀏覽器 (Browser)                    │
├─────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │          Assessment.cshtml (前端視圖)                 │   │
│  │  - HTML 表單結構                                      │   │
│  │  - JavaScript 邏輯 (表單驗證、數據保存)                 │   │
│  │  - CSS 樣式 (響應式設計)                              │   │
│  └─────────────────────────────────────────────────────┘   │
│                         │                                    │
│                         │ HTTP Request                       │
│                         ▼                                    │
│  ┌─────────────────────────────────────────────────────┐   │
│  │       ASP.NET Core MVC (後端)                        │   │
│  │                                                       │   │
│  │  ┌─────────────────────────────────────────────┐   │   │
│  │  │  HomeController                              │   │   │
│  │  │  - Assessment() [GET]                        │   │   │
│  │  │  - GetEcpayFormData() [POST]                 │   │   │
│  │  │  - EcpayReturn() [GET]                       │   │   │
│  │  └─────────────────────────────────────────────┘   │   │
│  └─────────────────────────────────────────────────────┘   │
│                         │                                    │
│                         │ Payment API Call                   │
│                         ▼                                    │
│  ┌─────────────────────────────────────────────────────┐   │
│  │          綠界支付 (ECPay Payment Gateway)             │   │
│  │  - 測試環境: payment-stage.ecpay.com.tw             │   │
│  └─────────────────────────────────────────────────────┘   │
│                                                               │
│  ┌─────────────────────────────────────────────────────┐   │
│  │          Browser LocalStorage                        │   │
│  │  - survey_data (JSON)                                │   │
│  │  - survey_timestamp                                  │   │
│  │  - survey_systemName                                 │   │
│  │  - survey_M1 ~ survey_M5                            │   │
│  │  - survey_open1 ~ survey_open3                      │   │
│  └─────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 技術棧 (Technology Stack)

#### 前端
- **框架**: ASP.NET Core MVC (Razor Views)
- **樣式**: CSS3 (自定義樣式)
- **JavaScript**: Vanilla JavaScript (ES6+)
- **第三方庫**:
  - i18n.js (多語言支援)

#### 後端
- **框架**: ASP.NET Core MVC
- **語言**: C#
- **支付整合**: 綠界支付 (ECPay)

#### 數據儲存
- **客戶端**: Browser LocalStorage
- **未來擴展**: 可整合資料庫儲存

---

## 3. 資料流程 (Data Flow)

### 3.1 問卷填寫流程

```
┌──────────┐
│  使用者   │
└────┬─────┘
     │
     │ 1. 訪問 /Home/Assessment
     ▼
┌─────────────────┐
│ Assessment View │
│ - 載入頁面      │
│ - 檢查 localStorage
│ - 恢復已保存數據 │
└────┬────────────┘
     │
     │ 2. 使用者填寫表單
     ▼
┌─────────────────┐
│ 輸入事件觸發     │
│ - Slider 改變   │
│ - Textarea 輸入 │
│ - System Name 輸入│
└────┬────────────┘
     │
     │ 3. saveToLocalStorage()
     ▼
┌─────────────────┐
│ LocalStorage    │
│ - 即時保存所有欄位│
│ - 保存完整 JSON  │
│ - 保存時間戳     │
└────┬────────────┘
     │
     │ 4. 點擊「生成報告」按鈕
     ▼
┌─────────────────┐
│ goToPayment()   │
│ - 再次保存數據  │
│ - 調用支付 API  │
└────┬────────────┘
     │
     │ 5. POST /Home/GetEcpayFormData
     ▼
┌─────────────────┐
│ HomeController  │
│ - 生成訂單號    │
│ - 計算 CheckMacValue│
│ - 返回表單數據  │
└────┬────────────┘
     │
     │ 6. 動態創建表單
     ▼
┌─────────────────┐
│ 提交到綠界支付   │
│ - 新窗口打開    │
│ - 進行支付      │
└────┬────────────┘
     │
     │ 7. 支付成功後跳轉
     ▼
┌─────────────────┐
│ /Home/EcpayReturn│
│ → /Home/Report  │
│ - 顯示報告頁面  │
└─────────────────┘
```

### 3.2 數據結構

#### LocalStorage 數據結構

```javascript
// 個別欄位（用於表單恢復）
localStorage.setItem('survey_systemName', '系統名稱');
localStorage.setItem('survey_M1', '5');
localStorage.setItem('survey_M2', '7');
// ... M3, M4, M5
localStorage.setItem('survey_open1', '挑戰描述...');
localStorage.setItem('survey_open2', '改善領域...');
localStorage.setItem('survey_open3', '其他資訊...');

// 完整數據（JSON格式，用於 Report 頁面）
localStorage.setItem('survey_data', JSON.stringify({
    systemName: '系統名稱',
    M1: '5',
    M2: '7',
    M3: '6',
    M4: '8',
    M5: '5',
    open1: '挑戰描述...',
    open2: '改善領域...',
    open3: '其他資訊...'
}));

// 時間戳
localStorage.setItem('survey_timestamp', '2024-12-XXTXX:XX:XX.XXXZ');
```

---

## 4. API 設計 (API Design)

### 4.1 路由定義

| 方法 | 路由 | 說明 | 參數 |
|------|------|------|------|
| GET | `/Home/Assessment` | 顯示評估問卷頁面 | 無 |
| POST | `/Home/GetEcpayFormData` | 獲取綠界支付表單資料 | 無 |
| GET | `/Home/EcpayReturn` | 綠界支付完成後跳轉頁面 | 無 |

### 4.2 API 詳細說明

#### 4.2.1 GET `/Home/Assessment`

**描述**: 顯示評估問卷頁面

**回應**: HTML View (Assessment.cshtml)

**行為**:
- 渲染評估問卷表單
- 前端 JavaScript 會自動檢查 localStorage 並恢復已保存的數據

---

#### 4.2.2 POST `/Home/GetEcpayFormData`

**描述**: 獲取綠界支付所需的表單資料和簽名

**請求標頭**:
```
Content-Type: application/json
```

**請求體**: 無（目前未使用請求體，未來可傳入訂單資訊）

**回應格式**: JSON

**回應範例**:
```json
{
  "MerchantID": "2000132",
  "MerchantTradeNo": "DOC20241201123456",
  "MerchantTradeDate": "2024/12/01 12:34:56",
  "PaymentType": "aio",
  "TotalAmount": "2990",
  "TradeDesc": "Doc Engine Report",
  "ItemName": "Risk Assessment Report",
  "ReturnURL": "http://localhost:5163/Home/EcpayReturn",
  "ChoosePayment": "Credit",
  "EncryptType": "1",
  "ClientBackURL": "http://localhost:5163/Home/Report",
  "OrderResultURL": "http://localhost:5163/Home/EcpayReturn",
  "NeedExtraPaidInfo": "N",
  "CheckMacValue": "A1B2C3D4E5F6..."
}
```

**實作細節**:
1. 生成唯一訂單號：`DOC + DateTime.Now.ToString("yyyyMMddHHmmss")`
2. 設定訂單資訊（金額、描述等）
3. 使用 `GenCheckMacValue()` 方法計算簽名
4. 返回完整的表單資料

**CheckMacValue 計算邏輯**:
```
1. 排除 CheckMacValue 欄位
2. 按照 Key 進行排序
3. 組合字串：HashKey={hashKey}&{sortedParams}&HashIV={hashIV}
4. URL Encode 並轉為小寫
5. SHA256 加密
6. 轉為大寫十六進制字串
```

---

#### 4.2.3 GET `/Home/EcpayReturn`

**描述**: 綠界支付完成後的回調頁面

**回應**: HTML View (Report.cshtml)

**行為**:
- 顯示報告頁面
- 前端 JavaScript 會從 localStorage 讀取 `survey_data` 並渲染報告內容

---

## 5. 前端實作細節 (Frontend Implementation)

### 5.1 視圖結構 (Assessment.cshtml)

#### 5.1.1 主要區塊

```html
<!-- 系統名稱輸入 -->
<div class="question-card system-name-card">
    <input type="text" id="systemName" name="systemName" />
</div>

<!-- 5個成熟度指標 (M1-M5) -->
<div class="question-card">
    <label>M1 系統交接</label>
    <input type="range" class="slider" id="M1" name="M1" min="0" max="10" value="5" step="1" />
    <div class="score-description" id="desc-M1">
        <span class="score-value-inline" id="value-M1">5</span>
        <span class="score-text">描述文字...</span>
    </div>
</div>

<!-- 3個開放式問題 -->
<div class="question-card">
    <textarea id="open1" name="open1" rows="2"></textarea>
    <!-- open2, open3 類似 -->
</div>

<!-- 提交按鈕 -->
<button type="button" onclick="goToPayment()">生成報告 NT$2,990</button>
```

### 5.2 JavaScript 核心功能

#### 5.2.1 分數描述系統

**功能**: 根據滑桿數值（0-10）顯示對應的描述文字

**實作**:
- 使用 `scoreDescriptions` 物件儲存中英文描述
- 支援動態切換語言
- 使用 `updateScoreDescription(sliderId, value)` 更新顯示

**描述結構**:
```javascript
const scoreDescriptions = {
    'M1': {
        0: '極差：新人需要3個月以上才能上手',
        1: '很差：新人需要2-3個月才能上手',
        // ... 2-10
    },
    // M2, M3, M4, M5 類似
};
```

#### 5.2.2 自動儲存機制

**觸發時機**:
- Slider 值改變時（`input` 事件）
- Textarea 內容改變時（`input` 事件）
- System Name 輸入改變時（`input` 事件）

**儲存內容**:
1. **個別欄位**：用於表單恢復
   ```javascript
   localStorage.setItem(`survey_${fieldId}`, value);
   ```

2. **完整 JSON**：用於 Report 頁面
   ```javascript
   localStorage.setItem('survey_data', JSON.stringify(data));
   ```

3. **時間戳**：記錄填寫時間
   ```javascript
   localStorage.setItem('survey_timestamp', new Date().toISOString());
   ```

#### 5.2.3 數據恢復機制

**頁面載入時**:
1. 檢查 localStorage 中的個別欄位值
2. 恢復 Slider 位置和顯示值
3. 恢復 Textarea 內容
4. 恢復 System Name
5. 更新分數描述顯示

**實作邏輯**:
```javascript
// 載入 Slider 值
const savedValue = localStorage.getItem(`survey_${slider.id}`);
if (savedValue !== null) {
    slider.value = savedValue;
    updateScoreDescription(slider.id, parseInt(savedValue));
}

// 載入 Textarea 值
const savedText = localStorage.getItem(`survey_${textarea.id}`);
if (savedText !== null) {
    textarea.value = savedText;
}
```

#### 5.2.4 支付流程整合

**函數**: `goToPayment()`

**流程**:
1. 呼叫 `saveToLocalStorage()` 確保數據已保存
2. 發送 POST 請求到 `/Home/GetEcpayFormData`
3. 獲取表單資料（JSON）
4. 動態創建 `<form>` 元素
5. 添加所有隱藏欄位
6. 提交表單到綠界支付（新窗口）

**實作範例**:
```javascript
async function goToPayment() {
    saveToLocalStorage();
    
    const response = await fetch('/Home/GetEcpayFormData', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    });
    
    const formData = await response.json();
    
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = 'https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5';
    form.target = '_blank';
    
    for (const [key, value] of Object.entries(formData)) {
        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = key;
        input.value = value;
        form.appendChild(input);
    }
    
    document.body.appendChild(form);
    form.submit();
}
```

### 5.3 多語言支援

**實作方式**:
- 使用 `data-i18n` 屬性標記需要翻譯的元素
- 使用 `data-placeholder-i18n` 標記 placeholder
- 在 `i18n.js` 中定義翻譯鍵值
- 支援繁體中文 (`zh-TW`) 和英文 (`en-US`)

**分數描述語言切換**:
- 使用 `loadScoreDescriptions()` 函數載入對應語言的描述
- 監聽 `languageChanged` 事件，重新載入描述

**翻譯鍵值範例**:
```javascript
'Assessment.Title': {
    'zh-TW': '風險評估問卷',
    'en-US': 'Risk Assessment Questionnaire'
},
'Assessment.M1': {
    'zh-TW': 'M1 系統交接',
    'en-US': 'M1 System Handover'
}
```

---

## 6. 後端實作細節 (Backend Implementation)

### 6.1 Controller 結構

#### 6.1.1 HomeController.cs

**類別**: `HomeController`

**依賴注入**:
- `ILogger<HomeController>`: 用於日誌記錄

**常數定義**:
- `PORT = 5163`: 應用程式 Port 號（用於建構回調 URL）

### 6.2 主要方法實作

#### 6.2.1 Assessment() [GET]

```csharp
public IActionResult Assessment()
{
    return View();
}
```

**說明**: 返回評估問卷視圖，無需額外邏輯。

---

#### 6.2.2 GetEcpayFormData() [POST]

```csharp
[HttpPost]
public IActionResult GetEcpayFormData()
{
    var baseUrl = $"http://localhost:{PORT}";
    
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
    
    return Json(formData);
}
```

**參數說明**:
- `MerchantID`: 綠界測試環境特店編號（固定值：2000132）
- `MerchantTradeNo`: 唯一訂單號（格式：DOC + yyyyMMddHHmmss）
- `MerchantTradeDate`: 訂單建立時間
- `TotalAmount`: 訂單金額（單位：新台幣）
- `ReturnURL`: 綠界付款完成後跳轉 URL（Server 端回調）
- `ClientBackURL`: 使用者付款完成後返回 URL（Browser 端跳轉）
- `OrderResultURL`: 訂單結果通知 URL（目前與 ReturnURL 相同）

**安全性**:
- 使用 `CheckMacValue` 確保資料完整性
- HashKey 和 HashIV 應在正式環境中移至配置檔或環境變數

---

#### 6.2.3 GenCheckMacValue() [Private]

```csharp
private string GenCheckMacValue(Dictionary<string, string> parameters, string hashKey, string hashIV)
{
    // 排除 CheckMacValue 欄位
    var sortedParams = parameters
        .Where(x => x.Key != "CheckMacValue")
        .OrderBy(x => x.Key)
        .Select(x => $"{x.Key}={x.Value}");
    
    // 組合字串
    var checkValue = $"HashKey={hashKey}&{string.Join("&", sortedParams)}&HashIV={hashIV}";
    
    // URL Encode
    checkValue = System.Web.HttpUtility.UrlEncode(checkValue).ToLower();
    
    // SHA256 加密
    using var sha256 = SHA256.Create();
    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(checkValue));
    return BitConverter.ToString(bytes).Replace("-", "").ToUpper();
}
```

**計算步驟**:
1. 過濾掉 `CheckMacValue` 欄位本身
2. 按照 Key 的字母順序排序
3. 組合為 `Key=Value` 格式，用 `&` 連接
4. 前後加上 `HashKey=` 和 `&HashIV=`
5. 進行 URL Encode 並轉為小寫
6. 使用 SHA256 加密
7. 轉為大寫十六進制字串

**範例計算過程**:
```
原始參數:
- ChoosePayment=Credit
- MerchantID=2000132
- TotalAmount=2990

排序後: ChoosePayment=Credit&MerchantID=2000132&TotalAmount=2990

加上 HashKey 和 HashIV:
HashKey=5294y06JbISpM5x9&ChoosePayment=Credit&MerchantID=2000132&TotalAmount=2990&HashIV=v77hoKGq4kWxNNIS

URL Encode + 小寫:
hashkey%3d5294y06jbispm5x9%26choosepayment%3dcredit%26...

SHA256 加密:
A1B2C3D4E5F6...
```

---

#### 6.2.4 EcpayReturn() [GET]

```csharp
public IActionResult EcpayReturn()
{
    // 綠界跳回成功頁
    return View("Report");
}
```

**說明**: 
- 綠界支付完成後會跳轉到此頁面
- 目前直接顯示 Report 頁面
- 未來可在此處理支付驗證邏輯

---

## 7. 支付整合 (Payment Integration)

### 7.1 綠界支付 (ECPay) 整合

#### 7.1.1 測試環境設定

**測試特店編號**:
- MerchantID: `2000132`
- HashKey: `5294y06JbISpM5x9`
- HashIV: `v77hoKGq4kWxNNIS`

**支付網址**:
- 測試環境: `https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5`
- 正式環境: `https://payment.ecpay.com.tw/Cashier/AioCheckOut/V5`

#### 7.1.2 測試流程

1. 訪問評估問卷頁面
2. 填寫問卷並點擊「生成報告」
3. 系統自動開啟新窗口跳轉至綠界支付頁面
4. 使用測試卡號進行支付：
   - 信用卡號: `4561 1111 1111 1111`
   - 到期日: `12/30`
   - CVV: `123`
   - 姓名: `TEST`
   - 生日: `85/01/01`
   - 手機: `0912345678`
5. 支付成功後自動跳轉回 `/Home/Report`

#### 7.1.3 支付流程圖

```
使用者點擊「生成報告」
        │
        ▼
前端調用 GetEcpayFormData API
        │
        ▼
後端生成訂單資訊並計算簽名
        │
        ▼
前端動態創建表單並提交到綠界
        │
        ▼
使用者在新窗口完成支付
        │
        ├─ 支付成功 ──→ 綠界跳轉到 EcpayReturn ──→ 顯示 Report 頁面
        │
        └─ 支付失敗 ──→ 使用者可關閉窗口，數據仍保存在 localStorage
```

### 7.2 正式環境遷移建議

**配置管理**:
- 將 HashKey、HashIV、MerchantID 移至 `appsettings.json` 或環境變數
- 根據環境（Development/Production）自動切換支付網址
- 實作配置驗證，確保必要參數存在

**建議實作**:
```csharp
// appsettings.json
{
  "EcpaySettings": {
    "MerchantID": "2000132",
    "HashKey": "5294y06JbISpM5x9",
    "HashIV": "v77hoKGq4kWxNNIS",
    "PaymentUrl": "https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5"
  }
}
```

---

## 8. 錯誤處理 (Error Handling)

### 8.1 前端錯誤處理

#### 8.1.1 支付流程錯誤

**情境**: 無法獲取支付表單資料

**處理方式**:
```javascript
try {
    const response = await fetch('/Home/GetEcpayFormData', { ... });
    if (!response.ok) {
        throw new Error('獲取支付資料失敗');
    }
    // ... 處理成功情況
} catch (error) {
    console.error('支付流程錯誤:', error);
    alert('啟動支付流程失敗，請稍後再試');
}
```

**使用者體驗**:
- 顯示錯誤訊息
- 數據仍保存在 localStorage，使用者可重新嘗試
- 記錄錯誤到 Console（開發階段）

#### 8.1.2 LocalStorage 錯誤

**情境**: LocalStorage 已滿或不可用

**處理方式**:
- 使用 `try-catch` 包裝所有 localStorage 操作
- 提供降級方案（例如：提示使用者啟用 LocalStorage）
- 可考慮實作 Cookie 備援機制

**建議實作**:
```javascript
function saveToLocalStorage() {
    try {
        localStorage.setItem('survey_data', JSON.stringify(data));
    } catch (e) {
        if (e.name === 'QuotaExceededError') {
            alert('儲存空間已滿，請清除瀏覽器快取後重試');
        } else {
            console.error('LocalStorage 錯誤:', e);
        }
    }
}
```

### 8.2 後端錯誤處理

#### 8.2.1 CheckMacValue 計算錯誤

**情境**: 參數格式錯誤或加密失敗

**處理方式**:
- 使用 `try-catch` 包裝計算邏輯
- 記錄詳細錯誤日誌
- 返回適當的 HTTP 狀態碼和錯誤訊息

**建議實作**:
```csharp
[HttpPost]
public IActionResult GetEcpayFormData()
{
    try {
        // ... 建立 formData ...
        formData["CheckMacValue"] = GenCheckMacValue(formData, hashKey, hashIV);
        return Json(formData);
    } catch (Exception ex) {
        _logger.LogError(ex, "生成支付表單資料失敗");
        return StatusCode(500, new { error = "無法生成支付表單資料" });
    }
}
```

---

## 9. 安全性考量 (Security Considerations)

### 9.1 資料保護

**目前實作**:
- ✅ 問卷資料僅儲存在客戶端 LocalStorage（不涉及敏感資訊）
- ✅ 支付資訊透過 HTTPS 傳輸
- ✅ 使用 CheckMacValue 確保支付資料完整性

**建議改進**:
- ⚠️ **HashKey 和 HashIV 應移至配置檔或環境變數**，避免硬編碼
- ⚠️ **正式環境應使用環境變數**，確保敏感資訊不會進入版本控制
- ⚠️ **實作支付驗證邏輯**，在 `EcpayReturn` 中驗證支付結果的真實性

### 9.2 輸入驗證

**目前實作**:
- HTML5 表單驗證（`min`, `max`, `step`）
- 前端 JavaScript 驗證（數值範圍）

**建議改進**:
- ⚠️ **後端驗證**：雖然目前資料不儲存在伺服器，但未來整合資料庫時應實作後端驗證
- ⚠️ **XSS 防護**：確保所有使用者輸入在顯示前進行適當的編碼
- ⚠️ **CSRF 防護**：對 POST 請求實作 CSRF Token 驗證

### 9.3 支付安全性

**目前實作**:
- 使用綠界官方提供的 CheckMacValue 機制
- 支付流程在新窗口進行，降低主視窗被劫持的風險

**建議改進**:
- ⚠️ **支付結果驗證**：在 `EcpayReturn` 中驗證綠界回傳的支付結果簽名
- ⚠️ **訂單號唯一性**：確保訂單號不會重複（目前使用時間戳，應足夠）
- ⚠️ **金額驗證**：未來可實作伺服器端金額驗證，防止前端被篡改

---

## 10. 多語言支援 (Internationalization)

### 10.1 實作方式

**前端**:
- 使用 `i18n.js` 進行多語言切換
- 透過 `data-i18n` 屬性標記需要翻譯的元素
- 支援動態切換語言（不需重新載入頁面）

**支援語言**:
- 繁體中文 (`zh-TW`) - 預設
- 英文 (`en-US`)

### 10.2 翻譯範圍

**已翻譯元素**:
- ✅ 頁面標題和副標題
- ✅ 所有問題標籤和描述
- ✅ 分數描述（0-10 分，每個指標）
- ✅ 按鈕文字
- ✅ 提示訊息
- ✅ Placeholder 文字

**翻譯鍵值範例**:
```javascript
// i18n.js
'Assessment.Title': {
    'zh-TW': '風險評估問卷',
    'en-US': 'Risk Assessment Questionnaire'
},
'Assessment.SystemNameLabel': {
    'zh-TW': '系統名稱',
    'en-US': 'System Name'
},
'Assessment.M1': {
    'zh-TW': 'M1 系統交接',
    'en-US': 'M1 System Handover'
},
'Assessment.M1Desc': {
    'zh-TW': '新人不需1個月上手？',
    'en-US': 'Can new team members get up to speed within 1 month?'
}
```

### 10.3 語言切換機制

**觸發方式**:
- 點擊右上角語言切換按鈕
- 觸發 `i18n.setLang('zh-TW')` 或 `i18n.setLang('en-US')`
- 自動更新所有標記的元素
- 觸發 `languageChanged` 事件，更新分數描述

**實作邏輯**:
```javascript
// 當語言改變時
window.addEventListener('languageChanged', function() {
    loadScoreDescriptions(); // 重新載入分數描述
    // 更新所有滑桿的描述顯示
    sliders.forEach(slider => {
        updateScoreDescription(slider.id, parseInt(slider.value));
    });
});
```

---

## 11. 使用者體驗 (User Experience)

### 11.1 響應式設計

**桌面版**:
- 固定側邊欄
- 桌面右上角語言切換
- 網格佈局顯示問卷卡片

**行動版**:
- 可收合側邊欄（漢堡選單）
- 行動版標題列顯示語言切換
- 單欄佈局，卡片垂直堆疊

**斷點**:
- Mobile: `max-width: 768px`
- Desktop: `min-width: 769px`

### 11.2 即時反饋

**滑桿操作**:
- ✅ 即時顯示數值（0-10）
- ✅ 即時更新分數描述
- ✅ 自動儲存（無需手動保存）

**輸入欄位**:
- ✅ 自動儲存功能
- ✅ 頁面重新載入後自動恢復
- ✅ Placeholder 提示文字

**支付流程**:
- ⚠️ 目前無載入動畫（建議未來加入）
- ✅ 錯誤訊息提示
- ✅ 新窗口開啟支付頁面，保留原視窗狀態

### 11.3 表單驗證

**HTML5 驗證**:
- Slider: `min="0" max="10" step="1"`
- Textarea: 無強制驗證（開放式問題）
- System Name: 文字輸入，無長度限制

**建議改進**:
- ⚠️ 可考慮加入「必填欄位」驗證（例如：System Name）
- ⚠️ 可考慮加入 Textarea 最大長度限制
- ⚠️ 提交前顯示確認對話框（避免誤觸）

---

## 12. 測試建議 (Testing Recommendations)

### 12.1 單元測試

**後端測試**:
- `GenCheckMacValue()` 方法測試
  - 驗證計算結果正確性
  - 驗證參數排序邏輯
  - 驗證特殊字元處理
- `GetEcpayFormData()` 方法測試
  - 驗證訂單號唯一性
  - 驗證所有必要欄位存在
  - 驗證 CheckMacValue 正確生成

**前端測試**:
- `saveToLocalStorage()` 函數測試
  - 驗證數據正確儲存
  - 驗證 JSON 格式正確
- `updateScoreDescription()` 函數測試
  - 驗證描述文字正確顯示
  - 驗證語言切換功能
- `goToPayment()` 函數測試
  - 模擬 API 回應
  - 驗證表單動態創建

### 12.2 整合測試

**支付流程測試**:
- 端對端測試：從填寫問卷到支付完成的完整流程
- 模擬支付成功/失敗情境
- 驗證數據在 localStorage 中的正確性

**跨瀏覽器測試**:
- Chrome、Firefox、Safari、Edge
- 行動瀏覽器（iOS Safari、Chrome Mobile）

**跨裝置測試**:
- 桌面（1920x1080、1366x768）
- 平板（768x1024）
- 手機（375x667、414x896）

### 12.3 使用者接受度測試 (UAT)

**測試場景**:
1. 新使用者首次填寫問卷
2. 使用者中途中斷，重新載入頁面
3. 切換語言並填寫問卷
4. 支付流程測試（使用測試卡）
5. 多個使用者同時填寫問卷（壓力測試）

---

## 13. 未來擴展建議 (Future Enhancements)

### 13.1 資料庫整合

**目前狀態**: 資料僅儲存在 LocalStorage

**建議改進**:
- 實作後端 API 儲存問卷資料到資料庫
- 為使用者建立帳號系統
- 支援多份問卷歷史記錄
- 支援問卷資料的匯出/匯入

**資料庫設計建議**:
```sql
-- 使用者表
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);

-- 問卷表
CREATE TABLE Assessments (
    AssessmentId INT PRIMARY KEY IDENTITY,
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    SystemName NVARCHAR(255),
    M1 INT,
    M2 INT,
    M3 INT,
    M4 INT,
    M5 INT,
    Open1 NVARCHAR(MAX),
    Open2 NVARCHAR(MAX),
    Open3 NVARCHAR(MAX),
    CreatedAt DATETIME2 DEFAULT GETDATE(),
    UpdatedAt DATETIME2 DEFAULT GETDATE()
);

-- 訂單表
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY,
    AssessmentId INT FOREIGN KEY REFERENCES Assessments(AssessmentId),
    MerchantTradeNo NVARCHAR(50) UNIQUE NOT NULL,
    TotalAmount DECIMAL(10, 2),
    PaymentStatus NVARCHAR(50),
    PaidAt DATETIME2,
    CreatedAt DATETIME2 DEFAULT GETDATE()
);
```

### 13.2 支付結果驗證

**目前狀態**: 支付完成後直接跳轉到報告頁面，未驗證支付結果

**建議改進**:
- 在 `EcpayReturn` 中接收綠界回傳的支付結果
- 驗證 CheckMacValue 確保資料真實性
- 更新訂單狀態為「已付款」
- 只有已付款的訂單才能查看報告

**實作建議**:
```csharp
[HttpPost]
public IActionResult EcpayReturn([FromForm] Dictionary<string, string> ecpayData)
{
    // 驗證 CheckMacValue
    string receivedCheckMac = ecpayData["CheckMacValue"];
    string calculatedCheckMac = GenCheckMacValue(ecpayData, hashKey, hashIV);
    
    if (receivedCheckMac != calculatedCheckMac) {
        _logger.LogWarning("支付驗證失敗: CheckMacValue 不匹配");
        return BadRequest("支付驗證失敗");
    }
    
    // 檢查支付狀態
    string rtnCode = ecpayData["RtnCode"];
    if (rtnCode == "1") {
        // 支付成功
        // 更新訂單狀態
        // 返回報告頁面
        return View("Report");
    } else {
        // 支付失敗
        return View("PaymentFailed");
    }
}
```

### 13.3 報告存取控制

**目前狀態**: 報告頁面可隨時訪問，只要 localStorage 中有資料

**建議改進**:
- 實作報告存取控制（僅已付款使用者可查看）
- 支援報告連結分享（帶有 Token）
- 支援報告下載權限管理

### 13.4 功能增強

**問卷功能**:
- ⚠️ 支援問卷草稿儲存（伺服器端）
- ⚠️ 支援問卷進度條顯示
- ⚠️ 支援問卷驗證（必填欄位、格式驗證）
- ⚠️ 支援問卷預覽功能

**使用者體驗**:
- ⚠️ 加入載入動畫和進度指示
- ⚠️ 支援鍵盤快捷鍵操作
- ⚠️ 支援問卷列印功能
- ⚠️ 支援問卷 PDF 下載（填寫後）

**分析功能**:
- ⚠️ 問卷填寫統計（平均分數、完成率）
- ⚠️ 歷史問卷對比分析
- ⚠️ 趨勢圖表顯示

---

## 14. 附錄 (Appendix)

### 14.1 相關檔案清單

**前端檔案**:
- `Views/Home/Assessment.cshtml` - 評估問卷視圖
- `wwwroot/css/site.css` - 樣式表（部分）
- `wwwroot/js/i18n.js` - 多語言支援
- `wwwroot/js/site.js` - 共用 JavaScript（如有）

**後端檔案**:
- `Controllers/HomeController.cs` - 控制器
- `Program.cs` - 應用程式入口點
- `appsettings.json` - 設定檔

**資源檔案**:
- `Resources/SharedResources.zh-TW.resx` - 中文資源檔（如有）
- `Resources/SharedResources.en-US.resx` - 英文資源檔（如有）

### 14.2 依賴套件

**NuGet 套件**:
- `Microsoft.AspNetCore.App` (ASP.NET Core MVC)
- `System.Security.Cryptography` (SHA256 加密)

**前端套件**:
- 無第三方 JavaScript 框架（使用 Vanilla JS）
- i18n.js（多語言支援，可能是自定義或外部庫）

### 14.3 環境變數與配置

**目前硬編碼值**（建議移至配置檔）:
- Port: `5163`
- ECPay MerchantID: `2000132`
- ECPay HashKey: `5294y06JbISpM5x9`
- ECPay HashIV: `v77hoKGq4kWxNNIS`
- ECPay Payment URL: `https://payment-stage.ecpay.com.tw/Cashier/AioCheckOut/V5`

### 14.4 參考資料

**綠界支付文件**:
- [綠界金流 API 技術文件](https://developer.ecpay.com.tw/)

**技術文件**:
- [ASP.NET Core MVC 文件](https://docs.microsoft.com/aspnet/core/mvc/)
- [MDN Web Docs - LocalStorage](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage)

---

## 15. 變更紀錄 (Change Log)

| 版本 | 日期 | 作者 | 變更說明 |
|------|------|------|----------|
| v1.0 | 2024-12-XX | Development Team | 初始版本建立 |

---

**文件結束**
