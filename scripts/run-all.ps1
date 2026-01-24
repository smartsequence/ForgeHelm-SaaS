param(
    [string]$SaaSPath = "C:\charleen\DocEngine-SaaS",
    [string]$AgentPath = "C:\charleen\DocEngine-Agent"
)

$ErrorActionPreference = "Stop"

Write-Host "Starting DocEngine SaaS and Agent in one window..." -ForegroundColor Cyan

$pidFile = Join-Path $PSScriptRoot "run-all.pids"

$saaSProcess = Start-Process dotnet -ArgumentList @("run") -WorkingDirectory $SaaSPath -PassThru
Start-Sleep -Seconds 2
$agentProcess = Start-Process dotnet -ArgumentList @("run") -WorkingDirectory $AgentPath -PassThru

Set-Content -Path $pidFile -Value @($saaSProcess.Id, $agentProcess.Id)

Write-Host "SaaS PID: $($saaSProcess.Id), Agent PID: $($agentProcess.Id)" -ForegroundColor Green
Write-Host "Press Ctrl+C to stop both." -ForegroundColor Yellow

try {
    Wait-Process -Id $saaSProcess.Id, $agentProcess.Id
}
finally {
    if (Test-Path $pidFile) {
        Remove-Item -Path $pidFile -Force
    }
    if (-not $saaSProcess.HasExited) {
        Write-Host "Stopping SaaS..." -ForegroundColor Yellow
        Stop-Process -Id $saaSProcess.Id -Force
    }
    if (-not $agentProcess.HasExited) {
        Write-Host "Stopping Agent..." -ForegroundColor Yellow
        Stop-Process -Id $agentProcess.Id -Force
    }
    Write-Host "Both processes stopped." -ForegroundColor Green
}
