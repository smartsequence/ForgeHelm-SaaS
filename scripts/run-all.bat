@echo off
setlocal

set "SaaSPath=%~1"
if "%SaaSPath%"=="" set "SaaSPath=C:\charleen\DocEngine"

set "AgentPath=%~2"
if "%AgentPath%"=="" set "AgentPath=C:\charleen\DocEngine-Agent"

echo Starting DocEngine SaaS and Agent in one window...
powershell -NoExit -ExecutionPolicy Bypass -File "%~dp0run-all.ps1" -SaaSPath "%SaaSPath%" -AgentPath "%AgentPath%"

endlocal
