$pidFile = Join-Path $PSScriptRoot "run-all.pids"

if (-not (Test-Path $pidFile)) {
    Write-Host "PID file not found: $pidFile" -ForegroundColor Yellow
    Write-Host "Run scripts/run-all.ps1 first to create PID file." -ForegroundColor Yellow
    exit 1
}

$pids = Get-Content $pidFile | Where-Object { $_ -match '^\d+$' } | ForEach-Object { [int]$_ }

if ($pids.Count -eq 0) {
    Write-Host "No valid PIDs found in $pidFile" -ForegroundColor Yellow
    exit 1
}

foreach ($pid in $pids) {
    try {
        if (Get-Process -Id $pid -ErrorAction SilentlyContinue) {
            Write-Host "Stopping process $pid..." -ForegroundColor Yellow
            Stop-Process -Id $pid -Force
        }
    }
    catch {
        Write-Host "Failed to stop process $pid: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Remove-Item -Path $pidFile -Force
Write-Host "All tracked processes stopped." -ForegroundColor Green
