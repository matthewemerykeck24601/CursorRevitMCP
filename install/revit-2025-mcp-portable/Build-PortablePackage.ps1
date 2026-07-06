#Requires -Version 5.1
<#
.SYNOPSIS
  Builds the Revit add-in and assembles a zip-ready portable package folder.

.DESCRIPTION
  Run from the repository on a machine with .NET SDK and Revit 2025 API installed.
  Output: install/revit-2025-mcp-portable/dist/Revit2025-MCP-Portable/
#>
[CmdletBinding()]
param(
    [string] $RepoRoot = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = (Resolve-Path (Join-Path $ScriptDir "..\..")).Path
}

$DistRoot = Join-Path $ScriptDir "dist\Revit2025-MCP-Portable"
$BridgeProject = Join-Path $RepoRoot "RevitPublicMCPBridge\RevitPublicMCPBridge.csproj"
$ArtifactsDir = Join-Path $ScriptDir "artifacts"

Write-Host "Building RevitPublicMCPBridge for Revit 2025..."
& dotnet build $BridgeProject -c Release -p:RevitYear=2025
if ($LASTEXITCODE -ne 0) { throw "Build failed." }

$builtDll = Join-Path $RepoRoot "RevitPublicMCPBridge\bin\Release\net8.0-windows\RevitPublicMCPBridge.dll"
New-Item -ItemType Directory -Force -Path $ArtifactsDir | Out-Null
Copy-Item $builtDll (Join-Path $ArtifactsDir "RevitPublicMCPBridge.dll") -Force

if (Test-Path $DistRoot) {
    Remove-Item $DistRoot -Recurse -Force
}
New-Item -ItemType Directory -Force -Path $DistRoot | Out-Null

$copyItems = @(
    "Install-Revit2025Mcp.ps1",
    "README.md",
    "templates"
)
foreach ($item in $copyItems) {
    Copy-Item (Join-Path $ScriptDir $item) (Join-Path $DistRoot $item) -Recurse -Force
}

Copy-Item $ArtifactsDir (Join-Path $DistRoot "artifacts") -Recurse -Force

$mcpDest = Join-Path $DistRoot "revit-2025-community-bridge-mcp"
Copy-Item (Join-Path $RepoRoot "revit-2025-community-bridge-mcp") $mcpDest -Recurse -Force
Get-ChildItem $mcpDest -Directory -Filter "node_modules" -Recurse -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force

# Minimal repo layout so installer finds bridge sources
$bridgeSrc = Join-Path $DistRoot "RevitPublicMCPBridge"
New-Item -ItemType Directory -Force -Path $bridgeSrc | Out-Null
Copy-Item $BridgeProject $bridgeSrc
# Portable installs use artifacts; stub csproj-only folder satisfies path checks when using -SkipBuild

@"
Portable package built: $DistRoot

To install on a target PC:
  1. Copy or extract Revit2025-MCP-Portable.zip to any folder on any drive.
  2. Close Revit 2025, then run Install-Revit2025Mcp.ps1 -SkipBuild from that folder.

Recommended: extract anywhere (any drive), then run:

  cd E:\AnyFolder\Revit2025-MCP-Portable
  .\Install-Revit2025Mcp.ps1 -SkipBuild -ConfigureClaude -ConfigureCursor -CursorProjectPath D:\YourCursorProject

Zip for distribution:
  Compress-Archive -Path '$DistRoot\*' -DestinationPath '$($DistRoot).zip' -Force
"@ | Write-Host -ForegroundColor Green
