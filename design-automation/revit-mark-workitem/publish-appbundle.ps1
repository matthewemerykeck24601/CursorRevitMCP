#Requires -Version 5.1
<#
  Upload RevitMarkWorkitem-AppBundle.zip to Autodesk Design Automation (new AppBundle version).

  Docs: https://aps.autodesk.com/en/docs/design-automation/v3/tutorials/revit/step4-publish-appbundle/
       https://aps.autodesk.com/developer/overview/design-automation

  Required env:
    APS_CLIENT_ID
    APS_CLIENT_SECRET

  Required env (your existing registered AppBundle id, e.g. com.yourcompany.revitmarkworkitem):
    DA_APPBUNDLE_ID

  Optional:
    DA_REGION          us-east | emea | apac   (default: us-east)
    APS_CLIENT_ID      (same as Forge app)

  OAuth scope: code:all (two-legged client credentials)

  Usage (after .\build-appbundle.ps1):
    $env:DA_APPBUNDLE_ID = "your.fully.qualified.bundle.id"
    .\publish-appbundle.ps1

  First-time bundle (no id in APS yet): create the AppBundle once in APS portal or POST /appbundles
  per tutorial, then use this script only for new versions.

  After upload, point Activity at the new version (or bump alias) in DA console / API.
#>
param(
  [string]$ZipPath = "",
  [switch]$InitializeBundle
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

if (-not $ZipPath) {
  $ZipPath = Join-Path $root "RevitMarkWorkitem-AppBundle.zip"
}
if (-not (Test-Path $ZipPath)) {
  Write-Error "Zip not found: $ZipPath`nRun .\build-appbundle.ps1 first."
}

$cid = $env:APS_CLIENT_ID
$sec = $env:APS_CLIENT_SECRET
$bundleId = $env:DA_APPBUNDLE_ID
if (-not $cid -or -not $sec) { Write-Error "Set APS_CLIENT_ID and APS_CLIENT_SECRET" }
if (-not $bundleId -and -not $InitializeBundle) { Write-Error "Set DA_APPBUNDLE_ID (existing AppBundle nickname)" }

$region = $env:DA_REGION
if (-not $region) { $region = "us-east" }
$region = $region.ToLowerInvariant()
$daBase = switch ($region) {
  "emea" { "https://developer.api.autodesk.com/da/eu/v3" }
  "apac" { "https://developer.api.autodesk.com/da/apac/v3" }
  "asia" { "https://developer.api.autodesk.com/da/apac/v3" }
  default { "https://developer.api.autodesk.com/da/us-east/v3" }
}

Write-Host "Token (code:all) ..."
$tokenBody = @{
  client_id     = $cid
  client_secret = $sec
  grant_type    = "client_credentials"
  scope         = "code:all"
}
$tokenRes = Invoke-RestMethod -Method Post -Uri "https://developer.api.autodesk.com/authentication/v2/token" `
  -ContentType "application/x-www-form-urlencoded" -Body $tokenBody
$token = $tokenRes.access_token
if (-not $token) { Write-Error "No access_token in response" }

$headers = @{ Authorization = "Bearer $token" }

if ($InitializeBundle) {
  Write-Host "POST $daBase/appbundles (initialize - use unique id) ..."
  $engine = $env:DA_ENGINE
  if (-not $engine) { $engine = "Autodesk.Revit+2024" }
  $newId = $env:DA_APPBUNDLE_ID
  if (-not $newId) { Write-Error "For -InitializeBundle set DA_APPBUNDLE_ID to the new bundle id" }
  $createBody = @{
    id          = $newId
    engine      = $engine
    description = "RevitMarkWorkitem - mark + parameterPatches"
  } | ConvertTo-Json
  $reg = Invoke-RestMethod -Method Post -Uri "$daBase/appbundles" -Headers $headers `
    -ContentType "application/json" -Body $createBody
} else {
  Write-Host "POST $daBase/appbundles/$bundleId/versions (new version) ..."
  $verUrl = "$daBase/appbundles/$bundleId/versions"
  $engine = $env:DA_ENGINE
  if (-not $engine) { $engine = "Autodesk.Revit+2024" }
  $versionBody = @{ engine = $engine } | ConvertTo-Json
  $reg = Invoke-RestMethod -Method Post -Uri $verUrl -Headers $headers `
    -ContentType "application/json" -Body $versionBody
}

$up = $reg.uploadParameters
if (-not $up.endpointURL) { Write-Error "No uploadParameters.endpointURL in response: $($reg | ConvertTo-Json -Depth 6)" }

Write-Host "Uploading zip to signed URL (curl multipart) ..."
$endpoint = $up.endpointURL
$form = $up.formData
if (-not $form) { Write-Error "No uploadParameters.formData" }

$curlArgs = @("-s", "-S", "-X", "POST", $endpoint)
foreach ($prop in $form.PSObject.Properties) {
  $name = $prop.Name
  $val = $prop.Value
  if ($null -eq $val) { continue }
  if ($val -is [System.Array]) {
    foreach ($item in $val) {
      $curlArgs += "-F"
      $curlArgs += "$name=$item"
    }
  } else {
    $curlArgs += "-F"
    $curlArgs += "$name=$val"
  }
}
$curlArgs += "-F"
$curlArgs += "file=@$ZipPath"

$curlExe = Get-Command curl.exe -ErrorAction SilentlyContinue
if (-not $curlExe) { Write-Error "curl.exe not found (Windows 10+ includes it in System32)." }
& curl.exe @curlArgs
if ($LASTEXITCODE -ne 0) { Write-Error "curl upload failed with exit $LASTEXITCODE" }

Write-Host ""
Write-Host "Upload complete. New version:" $reg.version
Write-Host "Update your Activity alias to this version in APS Design Automation, or set DA_ACTIVITY_ID to match."
Write-Host "Reference: https://aps.autodesk.com/en/docs/design-automation/v3/tutorials/revit/step4-publish-appbundle/"
