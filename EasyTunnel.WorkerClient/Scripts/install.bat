@echo off
setlocal enabledelayedexpansion

:: 检查管理员权限
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo 正在请求管理员权限...
    powershell -Command "Start-Process cmd -ArgumentList '/c cd /d ""%~dp0"" && %~0' -Verb RunAs"
    exit /b
)

:: 设置服务名称和路径
set "serviceName=EasyTunnelClient"
set "exePath=%~dp0..\EasyTunnel.WorkerClient.exe"

:: 检查exe文件是否存在
if not exist "%exePath%" (
    echo 错误：找不到文件 "%exePath%"
    pause
    exit /b 1
)

:: 处理路径中的特殊字符（如空格）
set "exePathWithQuotes="%exePath%""

:: 删除旧服务（如果存在）
echo 正在删除旧服务（如果存在）...
sc delete "%serviceName%" >nul 2>&1

:: 创建新服务
echo 正在创建服务...
sc create "%serviceName%" binPath= %exePathWithQuotes% displayname= "%serviceName%" start= auto

if %errorLevel% neq 0 (
    echo 服务创建失败，错误代码：%errorLevel%
    pause
    exit /b 1
)

echo 服务创建成功！

:: 可选：启动服务
echo 是否要立即启动服务？(Y/N)
set /p choice=
if /i "!choice!"=="Y" (
    echo 正在启动服务...
    sc start "%serviceName%"
    if %errorLevel% neq 0 (
        echo 启动服务失败，错误代码：%errorLevel%
    ) else (
        echo 服务启动成功！
    )
)

pause
