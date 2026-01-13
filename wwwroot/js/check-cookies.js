// 檢查 Cookie 的輔助腳本
// 在瀏覽器控制台（F12 → Console）中執行

// 方法 1: 查看所有 Cookie
console.log('=== 所有 Cookie ===');
console.log(document.cookie);

// 方法 2: 檢查特定 Cookie
function checkCookie(name) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) {
        const cookieValue = parts.pop().split(';').shift();
        console.log(`✓ 找到 Cookie: ${name} = ${cookieValue}`);
        return cookieValue;
    } else {
        console.log(`✗ 未找到 Cookie: ${name}`);
        return null;
    }
}

// 檢查 Session Cookie
console.log('\n=== 檢查 Session Cookie ===');
const sessionCookie = checkCookie('.DocEngine.Session');
if (!sessionCookie) {
    console.warn('⚠️ Session Cookie 不存在！這可能是問題的原因。');
}

// 檢查語言 Cookie
console.log('\n=== 檢查語言 Cookie ===');
checkCookie('lang');

// 方法 3: 列出所有 Cookie 的詳細信息
console.log('\n=== Cookie 詳細列表 ===');
document.cookie.split(';').forEach(cookie => {
    const [name, value] = cookie.trim().split('=');
    console.log(`${name}: ${value || '(空值)'}`);
});

// 方法 4: 清除所有 Cookie（僅用於測試）
function clearAllCookies() {
    document.cookie.split(";").forEach(function(c) { 
        const cookieName = c.trim().split("=")[0];
        document.cookie = cookieName + "=;expires=Thu, 01 Jan 1970 00:00:00 UTC;path=/;";
        console.log(`已清除: ${cookieName}`);
    });
    console.log('✅ 所有 Cookie 已清除，請重新載入頁面');
}

// 使用方式：
// checkCookie('.DocEngine.Session');  // 檢查 Session Cookie
// clearAllCookies();  // 清除所有 Cookie（測試用）
