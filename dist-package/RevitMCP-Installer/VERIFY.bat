@echo off
setlocal enableextensions enabledelayedexpansion

rem ============================================================================
rem  Revit 2025 MCP Bridge - Post-install verifier
rem  Read-only. Prints PASS/FAIL for each installed piece and exits non-zero if
rem  any check fails.   Usage:  VERIFY.bat  [/nopause]
rem ============================================================================

set "INSTALL_ROOT=%LOCALAPPDATA%\RevitMCP"
set "SERVER_ENTRY=%INSTALL_ROOT%\mcp-server\dist\index.js"
set "SERVER_DIST=%INSTALL_ROOT%\mcp-server\dist"
set "PORTABLE_NODE=%INSTALL_ROOT%\nodejs\node.exe"
set "REVIT_ADDINS=%APPDATA%\Autodesk\Revit\Addins\2025"
set "DEST_DLL=%REVIT_ADDINS%\RevitPublicMCPBridge-v2\RevitPublicMCPBridge.net8.v17.dll"
set "MANIFEST=%REVIT_ADDINS%\RevitPublicMCPBridge.addin"
set "CLAUDE_CFG=%APPDATA%\Claude\claude_desktop_config.json"

set "NOPAUSE=0"
if /i "%~1"=="/nopause" set "NOPAUSE=1"
set "FAILED=0"
set "VNODE="

echo.
echo  ==========================================================
echo            Revit 2025 MCP Bridge - Verify
echo  ==========================================================
echo.

rem --- 1. server entry -------------------------------------------------------
if exist "%SERVER_ENTRY%" (
    echo  [PASS] MCP server entry found: %SERVER_ENTRY%
) else (
    echo  [FAIL] MCP server entry MISSING: %SERVER_ENTRY%
    set "FAILED=1"
)

rem --- 2. add-in DLL ---------------------------------------------------------
if exist "%DEST_DLL%" (
    echo  [PASS] Revit add-in DLL found: %DEST_DLL%
) else (
    echo  [FAIL] Revit add-in DLL MISSING: %DEST_DLL%
    set "FAILED=1"
)

rem --- 2b. manifest points at the DLL ---------------------------------------
if exist "%MANIFEST%" (
    findstr /i /c:"RevitPublicMCPBridge.net8.v17.dll" "%MANIFEST%" >nul 2>nul
    if errorlevel 1 (
        echo  [FAIL] Manifest exists but does not reference the v17 DLL: %MANIFEST%
        set "FAILED=1"
    ) else (
        echo  [PASS] Add-in manifest references the v17 DLL: %MANIFEST%
    )
) else (
    echo  [FAIL] Add-in manifest MISSING: %MANIFEST%
    set "FAILED=1"
)

rem --- 3. Claude config exists and contains our server ----------------------
if exist "%CLAUDE_CFG%" (
    findstr /c:"revit-2025-bridge-mcp" "%CLAUDE_CFG%" >nul 2>nul
    if errorlevel 1 (
        echo  [FAIL] Claude config exists but has no revit-2025-bridge-mcp entry: %CLAUDE_CFG%
        set "FAILED=1"
    ) else (
        echo  [PASS] Claude config contains revit-2025-bridge-mcp: %CLAUDE_CFG%
    )
) else (
    echo  [FAIL] Claude config MISSING: %CLAUDE_CFG%
    set "FAILED=1"
)

rem --- 4. Node.js available: system PATH >=18, OR portable node.exe present --
where node >nul 2>nul
if not errorlevel 1 (
    call :verok node
    if "!VEROK!"=="1" set "VNODE=node"
)
if not defined VNODE if exist "%PORTABLE_NODE%" set "VNODE=%PORTABLE_NODE%"

if defined VNODE (
    echo  [PASS] Node.js available: %VNODE%
) else (
    echo  [FAIL] No Node.js 18+ on PATH and no portable Node at %PORTABLE_NODE%
    set "FAILED=1"
)

rem --- 5. Node can load the compiled server (syntax check; does NOT launch
rem        the long-running stdio server, which would block forever) ----------
if defined VNODE (
    set "LOADOK=1"
    call :checkfile index.js
    call :checkfile server.js
    call :checkfile bridgeClient.js
    if "!LOADOK!"=="1" echo  [PASS] Node loaded the compiled server without error.
) else (
    echo  [SKIP] Server load check skipped - no usable Node.js found.
)

echo.
if "%FAILED%"=="0" (
    echo  RESULT: ALL CHECKS PASSED.
    echo  If Claude still does not see the bridge, fully restart Revit 2025 and Claude Desktop.
) else (
    echo  RESULT: ONE OR MORE CHECKS FAILED - see [FAIL] lines above.
)
echo.
if "%NOPAUSE%"=="0" ( echo  Press any key to close. & pause >nul )

endlocal & exit /b %FAILED%

rem ---------------------------------------------------------------------------
rem  :verok  - set VEROK=1 if "<cmd> --version" reports major >= 18, else 0.
rem            %~1 = node command or full path (re-quoted, so spaces are OK).
rem ---------------------------------------------------------------------------
:verok
set "VEROK=0"
set "VFOUND="
set "VMAJ="
for /f "usebackq delims=" %%v in (`"%~1" --version 2^>nul`) do set "VFOUND=%%v"
if not defined VFOUND exit /b 0
set "VNUM=!VFOUND:v=!"
for /f "tokens=1 delims=." %%a in ("!VNUM!") do set "VMAJ=%%a"
if not defined VMAJ exit /b 0
if !VMAJ! GEQ 18 set "VEROK=1"
exit /b 0

rem ---------------------------------------------------------------------------
rem  :checkfile  - <node> --check one compiled file; flags LOADOK/FAILED on error.
rem  (--check validates syntax/load without launching the long-running server.)
rem ---------------------------------------------------------------------------
:checkfile
"%VNODE%" --check "%SERVER_DIST%\%~1" >nul 2>nul
if errorlevel 1 (
    echo  [FAIL] Node could not load %~1 - syntax or load error.
    set "LOADOK=0"
    set "FAILED=1"
)
exit /b 0
