# DocEngine-SaaS

DocEngine-SaaS 是一個以 ASP.NET Core MVC (.NET 9) 開發的 Web 應用程式，負責專案管理、文件生成與（未來）Agent 整合。

## 開發環境啟動

```bash
dotnet restore
dotnet run
```

應用程式預設會在 `http://localhost:5163`（或 `launchSettings.json` 設定的 URL）啟動。

## 設定檔與連線字串

- `appsettings.json`
  - 共用設定（含 `ConnectionStrings:Postgres` 預留鍵值）。
- `appsettings.Development.json`
  - 開發環境專用設定，可在這裡放 Postgres 開發用連線字串。

在 Azure / Container 環境中建議使用環境變數覆寫連線字串，例如：

- `ConnectionStrings__Postgres`：PostgreSQL 連線字串
- `OpenAI__ApiKey`：OpenAI API Key

## Container 部署說明（Azure / On-Prem 共用）

專案根目錄已提供多階段建置的 `Dockerfile`：

- 建置階段：使用 `mcr.microsoft.com/dotnet/sdk:9.0` 還原與發佈
- 執行階段：使用 `mcr.microsoft.com/dotnet/aspnet:9.0` 作為 runtime
- 服務對外監聽埠：`8080`（`ASPNETCORE_URLS=http://+:8080`）

### 本機或 On-Prem 簡易示例（若環境有 Docker）

```bash
docker build -t docengine-saas .
docker run -d -p 8080:8080 \
  -e ConnectionStrings__Postgres="Host=...;Port=...;Database=...;Username=...;Password=..." \
  -e OpenAI__ApiKey="your-key" \
  --name docengine-saas \
  docengine-saas
```

### Azure 容器部署（搭配 GitHub Actions）

Repo 內的 `.github/workflows/azure-container-deploy.yml` 會在 push 到 `main` 時：

1. 使用 .NET 9 建置與測試專案
2. 透過 `Dockerfile` 建置映像並推送到 Azure Container Registry (ACR)
3. 使用 `azure/webapps-deploy` 將最新映像佈署到 Azure App Service（Container 模式）

你需要在 GitHub Repo 的 Secrets 中設定：

- `ACR_LOGIN_SERVER`：例如 `acrdocenginedev.azurecr.io`
- `ACR_USERNAME` / `ACR_PASSWORD`：對應 ACR 的登入帳號與密碼（或 Service Principal）
- `AZURE_WEBAPP_PUBLISH_PROFILE`：從 Azure Portal 匯出的 App Service 發佈設定檔內容

並在 workflow 檔案中將：

- `AZURE_WEBAPP_NAME` 改成實際 App Service 名稱
- `DOCKER_IMAGE_NAME` 改成你希望在 ACR 中使用的映像名稱

