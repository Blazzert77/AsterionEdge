@echo off
setlocal EnableExtensions EnableDelayedExpansion
cd /d "%~dp0"
title Asterion Edge - Build and Install

set "ASTERION_VERSION=0.3.0-dev.3"
set "DOTNET_EXE=%ProgramFiles%\dotnet\dotnet.exe"

 echo.
 echo ============================================================
 echo   ASTERION EDGE %ASTERION_VERSION% - ONE CLICK BUILDER
 echo ============================================================
 echo.
 echo This helper is only needed once to create the Windows installer.
 echo After installation, Asterion starts automatically with Windows.
 echo.

rem ------------------------------------------------------------------
rem IMPORTANT: "where dotnet" only proves that the .NET HOST exists.
rem Windows can have dotnet.exe installed with ZERO SDKs. We therefore
rem explicitly verify that a .NET 10 SDK is available.
rem ------------------------------------------------------------------

if not exist "%DOTNET_EXE%" (
  for /f "delims=" %%D in ('where dotnet 2^>nul') do (
    if not defined DOTNET_FALLBACK set "DOTNET_FALLBACK=%%D"
  )
  if defined DOTNET_FALLBACK set "DOTNET_EXE=!DOTNET_FALLBACK!"
)

call :has_dotnet10
if errorlevel 1 (
  echo [1/4] .NET 10 SDK is missing. Installing it with winget...
  where winget >nul 2>nul || goto :no_winget
  winget install --id Microsoft.DotNet.SDK.10 -e --accept-package-agreements --accept-source-agreements
  if errorlevel 1 goto :dotnet_install_failed

  rem Winget normally installs the x64 SDK here. Use the absolute path so
  rem this same CMD session does not depend on a refreshed PATH variable.
  set "DOTNET_EXE=%ProgramFiles%\dotnet\dotnet.exe"
  call :has_dotnet10
  if errorlevel 1 goto :dotnet_not_detected
) else (
  echo [1/4] .NET 10 SDK found.
)

set "ISCC="
where iscc >nul 2>nul && set "ISCC=iscc"
if not defined ISCC if exist "%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles(x86)%\Inno Setup 6\ISCC.exe"
if not defined ISCC if exist "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" set "ISCC=%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe"
if not defined ISCC if exist "%ProgramFiles%\Inno Setup 6\ISCC.exe" set "ISCC=%ProgramFiles%\Inno Setup 6\ISCC.exe"
if not defined ISCC (
  echo [2/4] Installing Inno Setup with winget...
  where winget >nul 2>nul || goto :no_winget
  winget install --id JRSoftware.InnoSetup -e --accept-package-agreements --accept-source-agreements
  if errorlevel 1 goto :failed
) else (
  echo [2/4] Inno Setup found.
)

echo [3/4] Building the self-contained companion and installer...
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\build.ps1" -Installer -DotNetExe "%DOTNET_EXE%"
if errorlevel 1 goto :failed

set "SETUP=%~dp0release\AsterionEdge-Setup-v%ASTERION_VERSION%.exe"
if not exist "%SETUP%" goto :missing_setup

echo [4/4] Build complete. Starting the installer...
start "" "%SETUP%"
exit /b 0

:has_dotnet10
if not exist "%DOTNET_EXE%" exit /b 1
"%DOTNET_EXE%" --list-sdks 2>nul | %SystemRoot%\System32\findstr.exe /R /C:"^10\." >nul
exit /b %ERRORLEVEL%

:no_winget
echo.
echo Winget is not available on this Windows installation.
echo Install the .NET 10 SDK and Inno Setup manually, then run this file again.
pause
exit /b 1

:dotnet_install_failed
echo.
echo ERROR: winget could not install the .NET 10 SDK.
echo Try Windows Update, then run this builder again.
pause
exit /b 1

:dotnet_not_detected
echo.
echo ERROR: .NET 10 SDK installation finished, but the SDK is still not visible at:
echo   %ProgramFiles%\dotnet\dotnet.exe
echo.
echo Restart Windows once, then run BUILD_AND_INSTALL.cmd again.
pause
exit /b 1

:missing_setup
echo.
echo ERROR: the build ended without creating:
echo   %SETUP%
goto :failed

:failed
echo.
echo BUILD FAILED. Nothing was installed by Asterion itself.
echo Scroll up for the exact error message.
pause
exit /b 1
