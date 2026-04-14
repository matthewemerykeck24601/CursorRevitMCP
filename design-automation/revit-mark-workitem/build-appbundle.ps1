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
$bundleDir = Join-Path $staging "RevitMarkWorkitem.bundle"
$contents = Join-Path $bundleDir "Contents"
New-Item -ItemType Directory -Path $contents -Force | Out-Null

Copy-Item $dll $contents -Force
$nj = Join-Path $outDir "Newtonsoft.Json.dll"
if (Test-Path $nj) {
  Copy-Item $nj $contents -Force
} else {
  Write-Warning "Newtonsoft.Json.dll not in output; ensure PackageReference copies (CopyLocalLockFileAssemblies)."
}

Copy-Item (Join-Path $root "PackageContents.xml") $bundleDir -Force

$bundleAddin = @(
  '<RevitAddIns>',
  '  <AddIn Type="DBApplication">',
  '    <Name>RevitMarkWorkitem</Name>',
  '    <Assembly>.\Contents\RevitMarkWorkitem.dll</Assembly>',
  '    <AddInId>8D83C886-B739-4ACD-A9DB-1BC78F315B2A</AddInId>',
  '    <FullClassName>Metromont.RevitMarkWorkitem.DesignAutomationEntryPoint</FullClassName>',
  '    <VendorId>MMNT</VendorId>',
  '    <VendorDescription>Metromont</VendorDescription>',
  '  </AddIn>',
  '</RevitAddIns>'
) -join "`r`n"
Set-Content -Path (Join-Path $bundleDir "RevitMarkWorkitem.addin") -Value $bundleAddin -Encoding UTF8

$contentsAddin = @(
  '<RevitAddIns>',
  '  <AddIn Type="DBApplication">',
  '    <Name>RevitMarkWorkitem</Name>',
  '    <Assembly>.\RevitMarkWorkitem.dll</Assembly>',
  '    <AddInId>8D83C886-B739-4ACD-A9DB-1BC78F315B2A</AddInId>',
  '    <FullClassName>Metromont.RevitMarkWorkitem.DesignAutomationEntryPoint</FullClassName>',
  '    <VendorId>MMNT</VendorId>',
  '    <VendorDescription>Metromont</VendorDescription>',
  '  </AddIn>',
  '</RevitAddIns>'
) -join "`r`n"
Set-Content -Path (Join-Path $contents "RevitMarkWorkitem.addin") -Value $contentsAddin -Encoding UTF8

$zip = Join-Path $root "RevitMarkWorkitem-AppBundle.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path $bundleDir -DestinationPath $zip -Force

Write-Host "OK: $zip ($((Get-Item $zip).Length) bytes)"
Write-Host "Next: upload with .\publish-appbundle.ps1 (see script header for env vars)."
