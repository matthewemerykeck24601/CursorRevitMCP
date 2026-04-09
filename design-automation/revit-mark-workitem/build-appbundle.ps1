#Requires -Version 5.1
<#
  Build RevitMarkWorkitem and produce RevitMarkWorkitem-AppBundle.zip for APS Design Automation.
  Prereq: Revit 2024 (or set REVIT_INSTALL_PATH / -RevitInstallPath to folder containing RevitAPI.dll).

  Usage:
    .\build-appbundle.ps1
    .\build-appbundle.ps1 -RevitInstallPath "C:\Program Files\Autodesk\Revit 2025"
#>
param(
  [string]$Configuration = "Release",
  [string]$RevitInstallPath = $env:REVIT_INSTALL_PATH
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

if (-not $RevitInstallPath -or -not (Test-Path $RevitInstallPath)) {
  $RevitInstallPath = "C:\Program Files\Autodesk\Revit 2024"
}
if (-not (Test-Path (Join-Path $RevitInstallPath "RevitAPI.dll"))) {
  Write-Error "RevitAPI.dll not found under: $RevitInstallPath`nInstall Revit or set REVIT_INSTALL_PATH / -RevitInstallPath."
}

Write-Host "Building with RevitInstallPath=$RevitInstallPath ..."
dotnet build (Join-Path $root "RevitMarkWorkitem.csproj") -c $Configuration /p:RevitInstallPath="$RevitInstallPath"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$outDir = Join-Path $root "bin\$Configuration\net48"
$dll = Join-Path $outDir "RevitMarkWorkitem.dll"
if (-not (Test-Path $dll)) {
  Write-Error "Missing build output: $dll"
}

$staging = Join-Path $root "bundle-staging"
Remove-Item -Recurse -Force $staging -ErrorAction SilentlyContinue
$contents = Join-Path $staging "Contents"
New-Item -ItemType Directory -Path $contents -Force | Out-Null

Copy-Item $dll $contents -Force
$nj = Join-Path $outDir "Newtonsoft.Json.dll"
if (Test-Path $nj) {
  Copy-Item $nj $contents -Force
} else {
  Write-Warning "Newtonsoft.Json.dll not in output; ensure PackageReference copies (CopyLocalLockFileAssemblies)."
}

Copy-Item (Join-Path $root "PackageContents.xml") $staging -Force

$zip = Join-Path $root "RevitMarkWorkitem-AppBundle.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $staging "PackageContents.xml"), $contents -DestinationPath $zip -Force

Write-Host "OK: $zip ($((Get-Item $zip).Length) bytes)"
Write-Host "Next: upload with .\publish-appbundle.ps1 (see script header for env vars)."
