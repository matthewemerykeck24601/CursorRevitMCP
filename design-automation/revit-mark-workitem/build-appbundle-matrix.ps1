#Requires -Version 5.1
<#
  Build year-specific AppBundle zips for multiple Revit versions.

  Examples:
    .\build-appbundle-matrix.ps1
    .\build-appbundle-matrix.ps1 -Years 2024,2025,2026,2027
#>
param(
  [string]$Configuration = "Release",
  [int[]]$Years = @(2024, 2025, 2026, 2027)
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

foreach ($year in $Years) {
  Write-Host ""
  Write-Host "=== Building AppBundle for Revit $year ==="
  & (Join-Path $root "build-appbundle.ps1") -Configuration $Configuration -RevitYear $year
  if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host ""
Write-Host "All requested AppBundle builds completed."
