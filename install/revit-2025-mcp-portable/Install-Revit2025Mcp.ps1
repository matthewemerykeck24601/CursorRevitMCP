#Requires -Version 5.1
<#
.SYNOPSIS
  Installs Revit 2025 MCP bridge add-in and local MCP server for Cursor / Claude Desktop.

.DESCRIPTION
  - Builds (or uses bundled) RevitPublicMCPBridge for Revit 2025
  - Deploys versioned DLL + .addin manifest to the user Addins folder
  - Copies MCP server to %LOCALAPPDATA%\Revit2025Mcp\bridge-mcp and runs npm install
  - Writes bridge settings on port 8767 (avoids conflict with Revit 2026 on 8766)
  - Optionally merges MCP config into Claude Desktop and/or a Cursor project

.PARAMETER SkipBuild
  Use artifacts\RevitPublicMCPBridge.dll instead of running dotnet build.

.PARAMETER ConfigureClaude
  Merge revit-2025-bridge-mcp into %APPDATA%\Claude\claude_desktop_config.json

.PARAMETER ConfigureCursor
  Merge revit-2025-bridge-mcp into .cursor\mcp.json under -CursorProjectPath

.PARAMETER CursorProjectPath
  Path to a Cursor workspace (defaults to repo root when run from this repository).
#>
[CmdletBinding()]
param(
    [switch] $SkipBuild,
    [switch] $ConfigureClaude,
    [switch] $ConfigureCursor,
    [string] $CursorProjectPath = "",
    [string] $RepoRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Write-Step([string] $Message) {
    Write-Host ""
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Get-NextVersionedDllName([string] $DeployDir) {
    $max = 0
    if (Test-Path $DeployDir) {
        Get-ChildItem $DeployDir -Filter "RevitPublicMCPBridge.net8.v*.dll" -ErrorAction SilentlyContinue | ForEach-Object {
            if ($_.BaseName -match '\.v(\d+)$') {
                $n = [int]$Matches[1]
                if ($n -gt $max) { $max = $n }
            }
        }
    }
    "RevitPublicMCPBridge.net8.v$($max + 1).dll"
}

function Merge-McpServerConfig([string] $ConfigPath, [object] $ServerBlock) {
    $dir = Split-Path $ConfigPath -Parent
    if (-not (Test-Path $dir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }

    $root = if (Test-Path $ConfigPath) {
        Get-Content $ConfigPath -Raw | ConvertFrom-Json
    } else {
        New-Object PSObject
    }

    if (-not ($root.PSObject.Properties.Name -contains "mcpServers")) {
        $root | Add-Member -MemberType NoteProperty -Name mcpServers -Value (New-Object PSObject)
    }

    $root.mcpServers | Add-Member -MemberType NoteProperty -Name "revit-2025-bridge-mcp" -Value $ServerBlock -Force
    ($root | ConvertTo-Json -Depth 12) | Set-Content -Path $ConfigPath -Encoding UTF8
}

function Resolve-InstallPaths([string] $ScriptDir, [string] $RepoRoot) {
    $portableMcp = Join-Path $ScriptDir "revit-2025-community-bridge-mcp"
    $repoMcp = Join-Path $RepoRoot "revit-2025-community-bridge-mcp"
    $portableArtifacts = Join-Path $ScriptDir "artifacts\RevitPublicMCPBridge.dll"
    $usePortable = (Test-Path $portableMcp) -and (Test-Path $portableArtifacts)

    [pscustomobject]@{
        RepoRoot = if ($usePortable) { $ScriptDir } else { $RepoRoot }
        McpSourceDir = if (Test-Path $portableMcp) { $portableMcp } else { $repoMcp }
        ArtifactsDll = $portableArtifacts
        UsePortable = $usePortable
        BridgeProject = Join-Path $(if ($usePortable) { $ScriptDir } else { $RepoRoot }) "RevitPublicMCPBridge\RevitPublicMCPBridge.csproj"
    }
}

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $ScriptDir "..\..")).Path
}

$paths = Resolve-InstallPaths $ScriptDir $RepoRoot
$RepoRoot = $paths.RepoRoot
$BridgeProject = $paths.BridgeProject
$McpSourceDir = $paths.McpSourceDir
$ArtifactsDll = $paths.ArtifactsDll

if ($paths.UsePortable) {
    $SkipBuild = $true
    Write-Host "Portable package detected (artifacts + MCP bundled)." -ForegroundColor DarkGray
}
$InstallRoot = Join-Path $env:LOCALAPPDATA "Revit2025Mcp"
$McpInstallDir = Join-Path $InstallRoot "bridge-mcp"
$RevitAddins2025 = Join-Path $env:APPDATA "Autodesk\Revit\Addins\2025"
$BridgeDeployDir = Join-Path $RevitAddins2025 "RevitPublicMCPBridge-v2"
$BridgeSettingsDir = Join-Path $env:APPDATA "RevitPublicMCPBridge"
$AddinManifest = Join-Path $RevitAddins2025 "RevitPublicMCPBridge.addin"
$GatewayPort = 8767

Write-Host ""
Write-Host "Revit 2025 MCP Portable Installer" -ForegroundColor Green
Write-Host "Repository root: $RepoRoot"

Write-Step "Checking prerequisites"
$revit2025 = "${env:ProgramFiles}\Autodesk\Revit 2025\Revit.exe"
if (-not (Test-Path $revit2025)) {
    throw "Revit 2025 not found at '$revit2025'. Install Revit 2025 before continuing."
}
Write-Host "  Revit 2025: OK"

try {
    $nodeVersion = (node --version 2>$null)
    if (-not $nodeVersion) { throw "node missing" }
    Write-Host "  Node.js: $nodeVersion"
} catch {
    throw "Node.js is required (https://nodejs.org). Install Node 20+ and re-run."
}

if (-not (Test-Path $McpSourceDir)) {
    throw "MCP source not found at '$McpSourceDir'. Run this installer from the full repository or portable package."
}

Write-Step "Building Revit add-in (Release, RevitYear=2025)"
$builtDll = Join-Path $RepoRoot "RevitPublicMCPBridge\bin\Release\net8.0-windows\RevitPublicMCPBridge.dll"

if ($SkipBuild -and (Test-Path $ArtifactsDll)) {
    $builtDll = $ArtifactsDll
    Write-Host "  Using bundled DLL: $ArtifactsDll"
} else {
    if (-not (Test-Path $BridgeProject)) {
        throw "Bridge project not found at '$BridgeProject'."
    }
    & dotnet build $BridgeProject -c Release -p:RevitYear=2025 | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed."
    }
    if (-not (Test-Path $builtDll)) {
        throw "Expected build output not found: $builtDll"
    }
    Write-Host "  Built: $builtDll"
}

Write-Step "Deploying Revit add-in (close Revit 2025 first if deploy fails)"
New-Item -ItemType Directory -Force -Path $BridgeDeployDir | Out-Null
New-Item -ItemType Directory -Force -Path $RevitAddins2025 | Out-Null

$dllName = Get-NextVersionedDllName $BridgeDeployDir
$deployDll = Join-Path $BridgeDeployDir $dllName
Copy-Item $builtDll $deployDll -Force
$pdb = [System.IO.Path]::ChangeExtension($builtDll, ".pdb")
if (Test-Path $pdb) {
    Copy-Item $pdb (Join-Path $BridgeDeployDir ([System.IO.Path]::GetFileNameWithoutExtension($dllName) + ".pdb")) -Force -ErrorAction SilentlyContinue
}

$addinXml = @"
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="Application">
    <Name>RevitPublicMCPBridge</Name>
    <Assembly>$deployDll</Assembly>
    <AddInId>4C880AAC-7E16-4AE7-BD4D-27E641620A71</AddInId>
    <FullClassName>RevitPublicMCPBridge.RevitPublicMCPBridge</FullClassName>
    <VendorId>MSTRI</VendorId>
    <VendorDescription>Bridge UI for Autodesk Revit Public MCP Server.</VendorDescription>
  </AddIn>
</RevitAddIns>
"@
$addinXml | Set-Content -Path $AddinManifest -Encoding UTF8
Write-Host "  DLL: $deployDll"
Write-Host "  Manifest: $AddinManifest"

Write-Step "Writing bridge settings (port $GatewayPort)"
New-Item -ItemType Directory -Force -Path $BridgeSettingsDir | Out-Null
$settingsPath = Join-Path $BridgeSettingsDir "settings-2025.json"
$settingsTemplate = Join-Path $ScriptDir "templates\settings-2025.json"
if (Test-Path $settingsPath) {
    $existing = Get-Content $settingsPath -Raw | ConvertFrom-Json
    $existing.LocalApiGatewayPort = $GatewayPort
    $existing.EnableLocalApiGateway = $true
    ($existing | ConvertTo-Json -Depth 5) | Set-Content $settingsPath -Encoding UTF8
} elseif (Test-Path $settingsTemplate) {
    Copy-Item $settingsTemplate $settingsPath -Force
} else {
    @{
        EnableLocalApiGateway = $true
        LocalApiGatewayPort = $GatewayPort
        EnableProcessControl = $false
        AutoStartOnLaunch = $false
    } | ConvertTo-Json | Set-Content $settingsPath -Encoding UTF8
}
Write-Host "  Settings: $settingsPath"

Write-Step "Installing MCP server to $McpInstallDir"
if (Test-Path $McpInstallDir) {
    Remove-Item $McpInstallDir -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $McpInstallDir | Out-Null
Copy-Item (Join-Path $McpSourceDir "package.json") $McpInstallDir
Copy-Item (Join-Path $McpSourceDir "tsconfig.json") $McpInstallDir
Copy-Item (Join-Path $McpSourceDir "src") (Join-Path $McpInstallDir "src") -Recurse
Push-Location $McpInstallDir
try {
    npm install --omit=dev 2>&1 | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "npm install failed." }
} finally {
    Pop-Location
}
Write-Host "  MCP server installed."

$mcpEntryPath = Join-Path $ScriptDir "templates\mcp-server-entry.json"
$mcpTemplate = Get-Content $mcpEntryPath -Raw
$mcpJsonText = $mcpTemplate.Replace("{{MCP_SERVER_DIR}}", $McpInstallDir.Replace("\", "\\"))
$mcpServerBlock = ($mcpJsonText | ConvertFrom-Json)."revit-2025-bridge-mcp"

if ($ConfigureClaude) {
    Write-Step "Configuring Claude Desktop"
    $claudeConfig = Join-Path $env:APPDATA "Claude\claude_desktop_config.json"
    Merge-McpServerConfig $claudeConfig $mcpServerBlock
    Write-Host "  Updated: $claudeConfig"
    Write-Host "  Quit and restart Claude Desktop completely."
}

if ($ConfigureCursor) {
    Write-Step "Configuring Cursor workspace MCP"
    if ([string]::IsNullOrWhiteSpace($CursorProjectPath)) {
        $CursorProjectPath = $RepoRoot
    }
    $cursorConfig = Join-Path $CursorProjectPath ".cursor\mcp.json"
    Merge-McpServerConfig $cursorConfig $mcpServerBlock
    Write-Host "  Updated: $cursorConfig"
    Write-Host "  Reload MCP servers in Cursor (Settings -> MCP)."
}

Write-Step "Installation complete"
Write-Host @"

NEXT STEPS
----------
1. Start Revit 2025 (add-in loads on startup — restart if Revit was open during install).
2. Open a project or family; confirm MCP ribbon tab appears.
3. Gateway URL: http://127.0.0.1:$GatewayPort/health
4. Add MCP client config if you skipped -ConfigureClaude / -ConfigureCursor:

   Server name: revit-2025-bridge-mcp
   MCP server path: $McpInstallDir\src\index.ts
   Env: REVIT_2025_BRIDGE_URL=http://127.0.0.1:$GatewayPort

5. Test tool: get_family_parameters (Family Editor) or say_hello (any document).

See README.md in this folder for full end-user instructions.
"@ -ForegroundColor Yellow
