@echo off
setlocal

echo Stopping DocEngine SaaS and Agent...
powershell -NoExit -ExecutionPolicy Bypass -File "%~dp0stop-all.ps1"

endlocal
