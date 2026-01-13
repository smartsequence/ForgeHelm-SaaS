# Git 分支策略 - DocEngine

## 分支架構

### 主要分支

1. **`main`** - 已發佈的穩定版本（無 Agent）
   - 對應 Production 環境
   - 可接受客戶意見修改
   - 所有提交都應該經過測試
   - 受保護分支（建議在 GitHub 設置）

2. **`develop-agent`** - Agent 開發分支（私有開發）
   - 包含 Agent 相關功能
   - 從 `main` 分支創建
   - 定期同步 `main` 的更新（避免分叉過遠）
   - **不推送到公開倉庫**（或使用私有倉庫）

### 輔助分支

3. **`hotfix/*`** - 生產環境緊急修復
   - 從 `main` 創建
   - 修復後合併回 `main` 和 `develop-agent`
   - 命名：`hotfix/issue-description`

4. **`feature/*`** - 功能開發分支
   - 從對應的主分支創建
   - 完成後合併回對應主分支
   - 命名：`feature/feature-name`

## 工作流程

### 場景 1：Production 版本需要修復（客戶意見）

```bash
# 1. 從 main 創建 hotfix 分支
git checkout main
git pull origin main
git checkout -b hotfix/fix-description

# 2. 進行修復
# ... 修改代碼 ...

# 3. 提交並推送到遠程
git add .
git commit -m "fix: 修復描述"
git push origin hotfix/fix-description

# 4. 創建 Pull Request 合併到 main
# （在 GitHub 上操作）

# 5. 合併後，同步到 develop-agent
git checkout develop-agent
git pull origin main  # 或 git merge main
git push origin develop-agent
```

### 場景 2：開發 Agent 功能

```bash
# 1. 從 develop-agent 創建功能分支
git checkout develop-agent
git pull origin develop-agent
git checkout -b feature/agent-communication

# 2. 開發功能
# ... 開發代碼 ...

# 3. 提交並推送到遠程（私有倉庫或私有分支）
git add .
git commit -m "feat: 添加 Agent 通訊功能"
git push origin feature/agent-communication

# 4. 創建 Pull Request 合併到 develop-agent
# （在 GitHub 上操作，確保倉庫是私有的）
```

### 場景 3：定期同步 main 的更新到 develop-agent

```bash
# 1. 確保 develop-agent 是最新的
git checkout develop-agent
git pull origin develop-agent

# 2. 合併 main 的更新
git merge main
# 或使用 rebase（更乾淨的歷史）
# git rebase main

# 3. 解決衝突（如果有）
# ... 解決衝突 ...

# 4. 推送到遠程
git push origin develop-agent
```

### 場景 4：Agent 版本準備發佈（融合）

```bash
# 1. 確保 main 和 develop-agent 都是最新的
git checkout main
git pull origin main

git checkout develop-agent
git pull origin develop-agent

# 2. 合併 develop-agent 到 main
git checkout main
git merge develop-agent --no-ff -m "merge: 合併 Agent 功能到主分支"

# 3. 解決衝突（如果有）
# ... 解決衝突 ...

# 4. 測試
# ... 運行測試 ...

# 5. 推送到遠程
git push origin main

# 6. 標記版本
git tag -a v2.0.0 -m "Release: 包含 Agent 功能的版本"
git push origin v2.0.0
```

## GitHub 設置建議

### 1. 分支保護規則（Branch Protection Rules）

為 `main` 分支設置保護：
- ✅ Require pull request reviews before merging
- ✅ Require status checks to pass before merging
- ✅ Require branches to be up to date before merging
- ✅ Include administrators

### 2. 私有倉庫選項

**選項 A：使用私有倉庫（推薦）**
- 創建一個新的私有倉庫 `DocEngine-Agent`（或 `DocEngine-Private`）
- 將 `develop-agent` 分支推送到私有倉庫
- 這樣可以完全保密 Agent 開發

**選項 B：使用私有分支（較簡單）**
- 在同一個倉庫中，`develop-agent` 分支不設置為默認分支
- 不在 README 或文檔中提及此分支
- 限制協作者權限（只有開發團隊可以訪問）

### 3. 協作權限

- **Maintainer**：可以合併到 `main` 和 `develop-agent`
- **Developer**：可以推送到 `feature/*` 和 `hotfix/*`
- **Read-only**：只能查看 `main` 分支

## 命名規範

### 提交訊息（Commit Messages）

使用 Conventional Commits 格式：
- `feat:` - 新功能
- `fix:` - 修復
- `docs:` - 文檔
- `style:` - 格式（不影響代碼）
- `refactor:` - 重構
- `test:` - 測試
- `chore:` - 構建過程或輔助工具的變動

範例：
```
feat(agent): 添加 SignalR 通訊功能
fix(payment): 修復支付回調 Session 丟失問題
docs: 更新 Git 分支策略文檔
```

### 分支命名

- `feature/agent-signalr` - Agent SignalR 功能
- `hotfix/payment-session` - 支付 Session 修復
- `develop-agent` - Agent 開發主分支

## 最佳實踐

1. **定期同步**：每週至少一次將 `main` 的更新同步到 `develop-agent`
2. **小步提交**：頻繁提交，每次提交都有明確的目的
3. **代碼審查**：所有合併到 `main` 的 PR 都需要審查
4. **測試優先**：合併前確保所有測試通過
5. **文檔更新**：重要變更時更新相關文檔

## 應急情況

### 如果 main 和 develop-agent 分叉太遠

```bash
# 使用 rebase 重新整理歷史（謹慎使用）
git checkout develop-agent
git rebase main

# 如果有衝突，解決後繼續
git rebase --continue

# 強制推送（僅在私有分支）
git push origin develop-agent --force-with-lease
```

### 如果需要在兩個分支間共享代碼

```bash
# 使用 cherry-pick 選擇性合併特定提交
git checkout develop-agent
git cherry-pick <commit-hash-from-main>
```

## 版本標記（Tags）

```bash
# 創建版本標記
git tag -a v1.0.0 -m "Release: 初始版本（無 Agent）"
git tag -a v2.0.0 -m "Release: 包含 Agent 功能"

# 推送標記
git push origin v1.0.0
git push origin --tags
```

## 常見問題

### Q: 如果客戶要求在 Production 版本中添加功能，但這個功能在 Agent 版本中已經有了怎麼辦？

A: 有兩種選擇：
1. **從 develop-agent cherry-pick**：將相關提交選擇性合併到 main
2. **在 main 重新實現**：如果代碼差異太大，在 main 重新實現

### Q: 如何確保 Agent 功能不會意外洩露？

A: 
1. 使用私有倉庫存放 `develop-agent` 分支
2. 設置分支保護規則
3. 限制協作者權限
4. 代碼審查時特別注意

### Q: 兩個分支的配置文件（如 appsettings.json）如何管理？

A: 
1. 使用環境變數覆蓋敏感配置
2. 將配置範本提交到版本控制（如 `appsettings.example.json`）
3. 實際配置通過環境變數或 CI/CD 注入
