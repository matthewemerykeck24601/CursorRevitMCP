#Requires -Version 5.1
<#
  Build RevitMarkWorkitem and produce RevitMarkWorkitem-AppBundle.zip for APS Design Automation.
  Prereq: matching Revit desktop install (or set REVIT_INSTALL_PATH / -RevitInstallPath to folder containing RevitAPI.dll).

  Usage:
    .\build-appbundle.ps1
    .\build-appbundle.ps1 -RevitYear 2025
    .\build-appbundle.ps1 -RevitInstallPath "C:\Program Files\Autodesk\Revit 2025"
#>
param(
  [string]$Configuration = "Release",
  [int]$RevitYear = 2024,
  [string]$TargetFramework = "",
  [string]$ForgePackageVersion = "",
  [string]$RevitInstallPath = $env:REVIT_INSTALL_PATH,
  [string]$EnvFile = ""
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

function Import-DotEnvFile {
  param([string]$Path)
  if (-not $Path -or -not (Test-Path $Path)) { return }
  Get-Content $Path | ForEach-Object {
    $line = $_.Trim()
    if (-not $line -or $line.StartsWith("#")) { return }
    $idx = $line.IndexOf("=")
    if ($idx -le 0) { return }
    $name = $line.Substring(0, $idx).Trim()
    $value = $line.Substring($idx + 1).Trim()
    [Environment]::SetEnvironmentVariable($name, $value, "Process")
  }
}

if (-not $EnvFile) {
  $EnvFile = Join-Path (Join-Path $root "..\..\aps-ai-web") ".env.local"
}
Import-DotEnvFile -Path $EnvFile
if (-not $PSBoundParameters.ContainsKey("RevitInstallPath") -and $env:REVIT_INSTALL_PATH) {
  $RevitInstallPath = $env:REVIT_INSTALL_PATH
}

if ($RevitYear -lt 2024) {
  Write-Error "RevitYear must be 2024 or newer."
}
if (-not $TargetFramework) {
  if ($RevitYear -le 2024) { $TargetFramework = "net48" }
  elseif ($RevitYear -ge 2027) { $TargetFramework = "net10.0-windows" }
  else { $TargetFramework = "net8.0-windows" }
}
if (-not $ForgePackageVersion) {
  if ($RevitYear -eq 2024) { $ForgePackageVersion = "2024.0.2" }
  elseif ($RevitYear -eq 2025) { $ForgePackageVersion = "2025.0.1" }
  else { $ForgePackageVersion = "$RevitYear.0.0" }
}
if (-not $RevitInstallPath -or -not (Test-Path $RevitInstallPath)) {
  $RevitInstallPath = "C:\Program Files\Autodesk\Revit $RevitYear"
}
if (-not (Test-Path (Join-Path $RevitInstallPath "RevitAPI.dll"))) {
  Write-Error "RevitAPI.dll not found under: $RevitInstallPath`nInstall Revit or set REVIT_INSTALL_PATH / -RevitInstallPath."
}

Write-Host "Building year=$RevitYear framework=$TargetFramework package=$ForgePackageVersion"
Write-Host "RevitInstallPath=$RevitInstallPath"
dotnet build (Join-Path $root "RevitMarkWorkitem.csproj") -c $Configuration `
  /p:RevitYear="$RevitYear" `
  /p:DaTargetFramework="$TargetFramework" `
  /p:DaForgePackageVersion="$ForgePackageVersion" `
  /p:RestoreIgnoreFailedSources=true `
  /p:RevitInstallPath="$RevitInstallPath"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$outDir = Join-Path $root "bin\$Configuration\$TargetFramework"
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

$packageXml = @"
<?xml version="1.0" encoding="utf-8"?>
<ApplicationPackage
  SchemaVersion="1.0"
  AutodeskProduct="Revit"
  Name="RevitMarkWorkitem"
  Description="run_revit_automation: generic parameter ops + optional mark pass from JSON payload (Design Automation)."
  AppVersion="1.0.0"
  FriendlyVersion="1.0.0"
  ProductType="Application"
  SupportedLocales="Enu"
  AppNameSpace="appstore.autodesk.com">
  <CompanyDetails Name="Metromont" Url="" Email="" />
  <Components Description="Revit $RevitYear">
    <RuntimeRequirements SeriesMax="R$RevitYear" SeriesMin="R$RevitYear" OS="Win64" Platform="Revit" />
    <ComponentEntry AppName="RevitMarkWorkitem" Version="1.0.0" ModuleName="./Contents/RevitMarkWorkitem.addin" AppDescription="Revit mark workitem runner" LoadOnCommandInvocation="False" LoadOnRevitStartup="True" />
  </Components>
</ApplicationPackage>
"@
Set-Content -Path (Join-Path $bundleDir "PackageContents.xml") -Value $packageXml -Encoding UTF8

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

$zip = Join-Path $root "RevitMarkWorkitem-AppBundle-$RevitYear.zip"
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path $bundleDir -DestinationPath $zip -Force

Write-Host "OK: $zip ($((Get-Item $zip).Length) bytes)"
Write-Host "Next: upload with .\publish-appbundle.ps1 -RevitYear $RevitYear (see script header for env vars)."
