CHCP 65001
@echo off
color 0e

:: 自动请求管理员权限
fltmc >nul 2>&1 || (
  echo 正在请求管理员权限...
  PowerShell "Start-Process '%~f0' -Verb RunAs"
  exit /b
)

@echo ==================================
@echo 安全提示：正在以管理员权限运行
@echo ==================================
@echo Start Uninstall EasyTunnelClient

Net Stop EasyTunnelClient 2>nul
sc delete EasyTunnelClient 2>nul && (
  echo 状态：服务已彻底移除
) || (
  echo 状态：服务不存在或删除失败
)

pause