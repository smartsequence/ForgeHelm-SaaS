# localStorage 使用分析與替代方案

## 當前 localStorage 使用情況

### 1. **語言偏好** (`wwwroot/js/i18n.js`)
- **用途**: 保存用戶選擇的語言（zh-TW / en-US）
- **使用頻率**: 每次切換語言時
- **是否必要**: ❌ 不必要
- **替代方案**: 使用 HttpOnly Cookie 或 Session

### 2. **問卷表單自動保存** (`Views/Home/Assessment.cshtml`)
- **用途**: 
  - 自動保存表單輸入（專案名稱、系統名稱、系統代號、評估人角色、M1-M5 分數、開放式問題）
  - 頁面重新載入時恢復表單狀態
- **使用頻率**: 每次輸入時自動保存
- **是否必要**: ⚠️ 部分必要（用戶體驗）
- **替代方案**: 
  - 移除自動保存功能（用戶需一次性提交）
  - 或改用定期提交到後端 Session

### 3. **報告數據讀取** (`Views/Home/Report.cshtml`)
- **用途**: 從 localStorage 讀取問卷數據來生成報告
- **使用頻率**: 每次載入報告頁面時
- **是否必要**: ❌ **不必要**（已有 Session 備份）
- **替代方案**: 完全使用 Session（`/Home/GetSurveyData` API）

### 4. **報告編號** (`Views/Home/Report.cshtml`)
- **用途**: 保存生成的報告編號
- **使用頻率**: 生成報告時
- **是否必要**: ❌ 不必要（可以從後端重新生成）
- **替代方案**: 從後端 Session 或資料庫讀取

## 資安風險評估

### localStorage 的資安問題：
1. **XSS 攻擊風險**: 如果網站有 XSS 漏洞，攻擊者可以讀取 localStorage 中的所有數據
2. **數據持久化**: 數據會一直保存在瀏覽器中，即使用戶登出
3. **跨標籤頁共享**: 同一網站的所有標籤頁都可以訪問 localStorage
4. **無 HttpOnly 保護**: JavaScript 可以直接訪問，無法像 Cookie 那樣設置 HttpOnly

### 對比 Session 的優勢：
1. **服務端控制**: 數據存儲在服務端，客戶端無法直接訪問
2. **HttpOnly Cookie**: Session ID 可以設置為 HttpOnly，防止 XSS 攻擊
3. **自動過期**: Session 可以設置過期時間，自動清理
4. **更安全**: 符合資安最佳實踐

## 替代方案

### 方案 A：完全移除 localStorage（最安全）

**優點**:
- ✅ 完全符合資安要求
- ✅ 無 XSS 風險
- ✅ 數據完全由服務端控制

**缺點**:
- ❌ 失去表單自動保存功能（用戶體驗稍差）
- ❌ 語言偏好每次都要重新選擇（可用 Cookie 解決）

**實作方式**:
1. 移除所有 `localStorage.getItem()` 和 `localStorage.setItem()`
2. 語言偏好改用 HttpOnly Cookie
3. 表單數據完全依賴 Session
4. 報告編號從後端 Session 讀取

### 方案 B：混合方案（平衡安全與體驗）

**優點**:
- ✅ 保留部分用戶體驗（語言偏好）
- ✅ 問卷數據使用 Session（安全）
- ✅ 符合大部分資安要求

**缺點**:
- ⚠️ 語言偏好仍使用 localStorage（風險極低，因為只是語言設定）

**實作方式**:
1. 語言偏好保留 localStorage（或改用 Cookie）
2. 問卷數據完全使用 Session
3. 報告編號從後端讀取

## 建議

**對於資安要求嚴格的客戶，建議使用「方案 A：完全移除 localStorage」**

原因：
1. 問卷數據已經有 Session 備份機制（`SubmitSurvey` 和 `GetSurveyData`）
2. 語言偏好可以用 Cookie 替代（HttpOnly, Secure）
3. 表單自動保存是「便利功能」，不是「必要功能」
4. 完全符合資安審查要求

## 功能影響評估

如果完全移除 localStorage：

| 功能 | 當前依賴 | 移除後影響 | 解決方案 |
|------|---------|-----------|---------|
| 語言偏好 | localStorage | 每次都要重新選擇 | 改用 HttpOnly Cookie |
| 表單自動保存 | localStorage | 失去自動保存 | 移除功能或改用定期提交到 Session |
| 報告數據讀取 | localStorage (後備) | 無影響 | 已優先使用 Session |
| 報告編號 | localStorage | 無影響 | 從後端 Session 讀取 |

**結論**: 完全移除 localStorage **不會**導致功能壞掉，只是會失去一些便利功能。
