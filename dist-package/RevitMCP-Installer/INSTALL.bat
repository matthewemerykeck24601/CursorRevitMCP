@echo off
setlocal enableextensions enabledelayedexpansion

rem ============================================================================
rem  Revit 2025 MCP Bridge - Installer
rem  Installs the MCP server (portable, node dist/index.js) and the Revit 2025
rem  gateway add-in into per-user locations, then wires Claude Desktop up.
rem
rem  Self-contained: if Node.js 18+ is not already present, a portable copy is
rem  downloaded silently to %LOCALAPPDATA%\RevitMCP\nodejs (no admin, no manual
rem  prerequisite). An internet connection is needed only for that first run.
rem
rem  Usage:  INSTALL.bat            (interactive - pauses at the end)
rem          INSTALL.bat /nopause   (no final pause - for scripted runs)
rem ============================================================================

rem --- Fixed locations -------------------------------------------------------
set "PKG_DIR=%~dp0"
set "INSTALL_ROOT=%LOCALAPPDATA%\RevitMCP"
set "DEST_SERVER=%INSTALL_ROOT%\mcp-server"
set "SERVER_ENTRY=%DEST_SERVER%\dist\index.js"
set "REVIT_ADDINS=%APPDATA%\Autodesk\Revit\Addins\2025"
set "DEPLOY_DIR=%REVIT_ADDINS%\RevitPublicMCPBridge-v2"
set "DLL_NAME=RevitPublicMCPBridge.net8.v17.dll"
set "PDB_NAME=RevitPublicMCPBridge.net8.v17.pdb"
set "DEST_DLL=%DEPLOY_DIR%\%DLL_NAME%"
set "MANIFEST=%REVIT_ADDINS%\RevitPublicMCPBridge.addin"
set "LOGFILE=%TEMP%\RevitMCP-Install.log"

rem --- Node.js resolution (system PATH or portable) --------------------------
set "PORTABLE_DIR=%INSTALL_ROOT%\nodejs"
set "PORTABLE_NODE=%PORTABLE_DIR%\node.exe"
set "NODE_EXE="
set "NODE_SRC="
rem Pinned portable Node.js 20 LTS (win-x64).
set "NODE_URL=https://nodejs.org/dist/v20.19.0/node-v20.19.0-win-x64.zip"
set "NODE_INNER=node-v20.19.0-win-x64"

set "NOPAUSE=0"
if /i "%~1"=="/nopause" set "NOPAUSE=1"

rem --- Fresh log -------------------------------------------------------------
> "%LOGFILE%" echo Revit 2025 MCP Bridge install log
>> "%LOGFILE%" echo Package source: %PKG_DIR%
>> "%LOGFILE%" echo Install root:   %INSTALL_ROOT%

echo.
echo  ==========================================================
echo            Revit 2025 MCP Bridge Installer
echo  ==========================================================
echo.
echo   Server : %DEST_SERVER%
echo   Add-in : %DEPLOY_DIR%
echo   Config : %APPDATA%\Claude\claude_desktop_config.json
echo.

rem ===========================================================================
rem  [1/5] Resolve Node.js 18+ (use system if present, else portable download)
rem ===========================================================================
call :say "[1/5] Resolving Node.js (18 or later)..."

rem (a) system node on PATH, version 18+
where node >nul 2>nul
if not errorlevel 1 (
    call :verok node
    if "!VEROK!"=="1" (
        set "NODE_EXE=node"
        set "NODE_SRC=system PATH (Node.js !VER_FOUND!)"
    )
)

rem (b) a portable copy we downloaded on a previous run, version 18+
if not defined NODE_EXE if exist "%PORTABLE_NODE%" (
    call :verok "%PORTABLE_NODE%"
    if "!VEROK!"=="1" (
        set "NODE_EXE=%PORTABLE_NODE%"
        set "NODE_SRC=portable (Node.js !VER_FOUND!)"
    )
)

rem (c) nothing usable - download a portable copy
if not defined NODE_EXE call :install_portable_node
if not defined NODE_EXE (
    call :say "  ERROR: Node.js auto-install failed. Please install Node.js 18 or later from https://nodejs.org and re-run this installer."
    goto :fail
)

call :say "  Node.js ready: !NODE_SRC!"
call :say "    Command: %NODE_EXE%"

rem ===========================================================================
rem  [2/5] Install MCP server (copy mcp-server\, includes prebuilt node_modules)
rem ===========================================================================
call :say "[2/5] Installing MCP server..."
if not exist "%INSTALL_ROOT%" mkdir "%INSTALL_ROOT%" >>"%LOGFILE%" 2>&1
robocopy "%PKG_DIR%mcp-server" "%DEST_SERVER%" /E /R:1 /W:1 /NFL /NDL /NJH /NJS /NP >>"%LOGFILE%" 2>&1
if errorlevel 8 (
    call :say "  ERROR: Failed to copy MCP server files (robocopy code !errorlevel!). See log."
    goto :fail
)
if not exist "%SERVER_ENTRY%" (
    call :say "  ERROR: Expected server entry not found after copy: %SERVER_ENTRY%"
    goto :fail
)
call :say "  Server installed to %DEST_SERVER%"

rem ===========================================================================
rem  [3/5] Deploy Revit 2025 add-in DLL
rem ===========================================================================
call :say "[3/5] Deploying Revit 2025 add-in..."
if not exist "%DEPLOY_DIR%" mkdir "%DEPLOY_DIR%" >>"%LOGFILE%" 2>&1
copy /y "%PKG_DIR%revit-addin\%DLL_NAME%" "%DEST_DLL%" >>"%LOGFILE%" 2>&1
if errorlevel 1 (
    call :say "  ERROR: Failed to copy the add-in DLL to %DEST_DLL%."
    call :say "  If Revit 2025 is running, close it and re-run this installer."
    goto :fail
)
if exist "%PKG_DIR%revit-addin\%PDB_NAME%" copy /y "%PKG_DIR%revit-addin\%PDB_NAME%" "%DEPLOY_DIR%\%PDB_NAME%" >>"%LOGFILE%" 2>&1
call :say "  DLL deployed to %DEST_DLL%"

rem ===========================================================================
rem  [4/5] Write the .addin manifest with the absolute installed DLL path
rem  (Revit does NOT expand environment variables in the manifest, so the
rem   placeholder token is patched to the resolved absolute path here.)
rem ===========================================================================
call :say "[4/5] Writing add-in manifest..."
if not exist "%REVIT_ADDINS%" mkdir "%REVIT_ADDINS%" >>"%LOGFILE%" 2>&1
set "TARGET_DLL=%DEST_DLL%"
powershell -NoProfile -ExecutionPolicy Bypass -Command "(Get-Content -Raw -LiteralPath '%PKG_DIR%revit-addin\RevitPublicMCPBridge.addin').Replace('@@ASSEMBLY_PATH@@', $env:TARGET_DLL) | Set-Content -LiteralPath '%MANIFEST%' -Encoding UTF8" >>"%LOGFILE%" 2>&1
if errorlevel 1 (
    call :say "  ERROR: Failed to write the add-in manifest at %MANIFEST%."
    goto :fail
)
if not exist "%MANIFEST%" (
    call :say "  ERROR: Manifest was not created at %MANIFEST%."
    goto :fail
)
call :say "  Manifest written: %MANIFEST%"
call :say "       Assembly: %DEST_DLL%"

rem ===========================================================================
rem  [5/5] Configure Claude Desktop (Node merges JSON; other servers preserved)
rem  The resolved Node command (system "node" or the portable node.exe path) is
rem  passed through so it is what Claude Desktop launches the server with.
rem ===========================================================================
call :say "[5/5] Configuring Claude Desktop..."
"%NODE_EXE%" "%PKG_DIR%configure_claude.js" "%NODE_EXE%" >>"%LOGFILE%" 2>&1
if errorlevel 1 (
    call :say "  ERROR: Could not update Claude Desktop config automatically."
    call :say "  Add this entry by hand under \"mcpServers\" in:"
    call :say "    %APPDATA%\Claude\claude_desktop_config.json"
    echo       "revit-2025-bridge-mcp": { "command": "%NODE_EXE%", "args": ["%SERVER_ENTRY%"], "env": {} }
    call :say "  See the log for details: %LOGFILE%"
    goto :fail
)
call :say "  Claude Desktop configured."

rem ===========================================================================
rem  Summary + next steps
rem ===========================================================================
call :say ""
call :say "  =========================================================="
call :say "   [OK] Node.js     : !NODE_SRC!"
call :say "   [OK] MCP server  : %DEST_SERVER%"
call :say "   [OK] Add-in DLL  : %DEST_DLL%"
call :say "   [OK] Manifest    : %MANIFEST%"
call :say "   [OK] Claude config updated"
call :say "  =========================================================="
call :say ""
call :say "  Installation complete. Please restart Revit 2025 and Claude Desktop."
call :say ""
call :say "  (Optional) run VERIFY.bat to confirm every piece is in place."
call :say "  Log: %LOGFILE%"
if "%NOPAUSE%"=="0" ( echo. & echo  Press any key to close. & pause >nul )
endlocal
exit /b 0

rem ===========================================================================
:fail
call :say ""
call :say "  Installation FAILED. No further steps were run."
call :say "  Log: %LOGFILE%"
if "%NOPAUSE%"=="0" ( echo. & echo  Press any key to close. & pause >nul )
endlocal
exit /b 1

rem ===========================================================================
rem  :verok  - set VEROK=1 if "<cmd> --version" reports major >= 18, else 0.
rem            Also exposes the raw version string in VER_FOUND.
rem            %~1 = node command or full path (re-quoted, so spaces are OK).
rem ===========================================================================
:verok
set "VEROK=0"
set "VER_FOUND="
set "VMAJ="
for /f "usebackq delims=" %%v in (`"%~1" --version 2^>nul`) do set "VER_FOUND=%%v"
if not defined VER_FOUND exit /b 0
set "VNUM=!VER_FOUND:v=!"
for /f "tokens=1 delims=." %%a in ("!VNUM!") do set "VMAJ=%%a"
if not defined VMAJ exit /b 0
if !VMAJ! GEQ 18 set "VEROK=1"
exit /b 0

rem ===========================================================================
rem  :install_portable_node  - download + unpack portable Node.js 20 LTS into
rem  %PORTABLE_DIR% so node.exe lands at %PORTABLE_NODE%. On any failure leaves
rem  NODE_EXE undefined (the caller then prints the error and exits 1).
rem ===========================================================================
:install_portable_node
call :say "  Node.js 18+ not found - downloading portable Node.js 20 LTS (one-time)..."
set "NODE_ZIP=%TEMP%\node-portable.zip"
set "NODE_XDIR=%INSTALL_ROOT%\nodejs-extract"

if not exist "%INSTALL_ROOT%" mkdir "%INSTALL_ROOT%" >>"%LOGFILE%" 2>&1
if exist "%NODE_XDIR%" rd /s /q "%NODE_XDIR%" >>"%LOGFILE%" 2>&1
if exist "%NODE_ZIP%" del /q "%NODE_ZIP%" >>"%LOGFILE%" 2>&1

rem Download + extract via PowerShell (paths/URL passed through env to avoid quoting issues).
powershell -NoProfile -ExecutionPolicy Bypass -Command "$ErrorActionPreference='Stop'; try { [Net.ServicePointManager]::SecurityProtocol=[Net.SecurityProtocolType]::Tls12; Invoke-WebRequest -Uri $env:NODE_URL -OutFile $env:NODE_ZIP -UseBasicParsing; Expand-Archive -LiteralPath $env:NODE_ZIP -DestinationPath $env:NODE_XDIR -Force; exit 0 } catch { Write-Error $_; exit 1 }" >>"%LOGFILE%" 2>&1
if errorlevel 1 (
    call :say "  Download or extraction of portable Node.js failed."
    goto :portable_cleanup
)

rem Move the inner node-vXX-win-x64 folder up so node.exe is at %PORTABLE_NODE%.
if exist "%PORTABLE_DIR%" rd /s /q "%PORTABLE_DIR%" >>"%LOGFILE%" 2>&1
move "%NODE_XDIR%\%NODE_INNER%" "%PORTABLE_DIR%" >>"%LOGFILE%" 2>&1
if errorlevel 1 (
    call :say "  Could not move extracted Node.js into place."
    goto :portable_cleanup
)

:portable_cleanup
if exist "%NODE_ZIP%" del /q "%NODE_ZIP%" >>"%LOGFILE%" 2>&1
if exist "%NODE_XDIR%" rd /s /q "%NODE_XDIR%" >>"%LOGFILE%" 2>&1

if not exist "%PORTABLE_NODE%" exit /b 0
call :verok "%PORTABLE_NODE%"
if not "!VEROK!"=="1" exit /b 0
set "NODE_EXE=%PORTABLE_NODE%"
set "NODE_SRC=portable, downloaded (Node.js !VER_FOUND!)"
call :say "  Portable Node.js installed to %PORTABLE_DIR%"
exit /b 0

rem ===========================================================================
rem  :say  - echo a line to the console and append it to the log file
rem ===========================================================================
:say
echo(%~1
>> "%LOGFILE%" echo(%~1
exit /b 0
