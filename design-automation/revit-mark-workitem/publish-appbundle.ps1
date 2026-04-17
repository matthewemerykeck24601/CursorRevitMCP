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
    or DA_APPBUNDLE_ID_<YEAR> (e.g. DA_APPBUNDLE_ID_2025)

  Optional:
    DA_REGION          us-east | emea | apac   (default: us-east)
    APS_CLIENT_ID      (same as Forge app)

  OAuth scope: code:all (two-legged client credentials)

  Usage (after .\build-appbundle.ps1):
    $env:DA_APPBUNDLE_ID = "your.fully.qualified.bundle.id"
    .\publish-appbundle.ps1

  First-time bundle (no id in APS yet): create the AppBundle once in APS portal or POST /appbundles
  per tutorial, then use this script only for new versions.

  By default, this script also publishes a new Activity version + alias
  when ActivityId is provided (param or env DA_ACTIVITY_ID / DA_ACTIVITY_ID_<YEAR>).
  Use -SkipActivityPublish for upload-only mode.
#>
param(
  [int]$RevitYear = 2024,
  [string]$ZipPath = "",
  [string]$BundleId = "",
  [string]$Engine = "",
  [string]$EnvFile = "",
  [switch]$InitializeBundle,
  [string]$ActivityId = "",
  [string]$ActivityAlias = "",
  [ValidateSet("mark", "create_model")]
  [string]$ActivityProfile = "mark",
  [switch]$InitializeActivity,
  [switch]$SkipActivityPublish
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

function Resolve-ActivityReference {
  param(
    [string]$Reference,
    [string]$FallbackAlias
  )
  $r = ""
  if ($null -ne $Reference) { $r = $Reference.Trim() }
  if (-not $r) {
    return @{
      ActivityId = ""
      Alias = $(if ($null -ne $FallbackAlias) { $FallbackAlias.Trim() } else { "" })
    }
  }
  $plusIdx = $r.IndexOf("+")
  if ($plusIdx -gt 0 -and $plusIdx -lt ($r.Length - 1)) {
    return @{
      ActivityId = $r.Substring(0, $plusIdx).Trim()
      Alias = $r.Substring($plusIdx + 1).Trim()
    }
  }
  return @{
    ActivityId = $r
    Alias = $(if ($null -ne $FallbackAlias) { $FallbackAlias.Trim() } else { "" })
  }
}

if (-not $EnvFile) {
  $EnvFile = Join-Path (Join-Path $root "..\..\aps-ai-web") ".env.local"
}
Import-DotEnvFile -Path $EnvFile

if (-not $ZipPath) {
  $ZipPath = Join-Path $root "RevitMarkWorkitem-AppBundle-$RevitYear.zip"
}
if (-not (Test-Path $ZipPath)) {
  Write-Error "Zip not found: $ZipPath`nRun .\build-appbundle.ps1 first."
}

$cid = $env:APS_CLIENT_ID
$sec = $env:APS_CLIENT_SECRET
$bundleIdByYearName = "DA_APPBUNDLE_ID_$RevitYear"
$bundleIdByYear = (Get-Item -Path "Env:$bundleIdByYearName" -ErrorAction SilentlyContinue).Value
if (-not $BundleId) {
  if ($bundleIdByYear) { $BundleId = $bundleIdByYear }
  else { $BundleId = $env:DA_APPBUNDLE_ID }
}
if (-not $cid -or -not $sec) { Write-Error "Set APS_CLIENT_ID and APS_CLIENT_SECRET" }
if (-not $BundleId -and -not $InitializeBundle) { Write-Error "Set -BundleId, DA_APPBUNDLE_ID, or DA_APPBUNDLE_ID_$RevitYear (existing AppBundle nickname)." }

$region = $env:DA_REGION
if (-not $region) { $region = "us-east" }
$region = $region.ToLowerInvariant()
$daBase = switch ($region) {
  "emea" { "https://developer.api.autodesk.com/da/eu/v3" }
  "apac" { "https://developer.api.autodesk.com/da/apac/v3" }
  "asia" { "https://developer.api.autodesk.com/da/apac/v3" }
  default { "https://developer.api.autodesk.com/da/us-east/v3" }
}

$activityByYearName = "DA_ACTIVITY_ID_$RevitYear"
$activityByYear = (Get-Item -Path "Env:$activityByYearName" -ErrorAction SilentlyContinue).Value
if (-not $ActivityId) {
  if ($activityByYear) { $ActivityId = $activityByYear }
  else { $ActivityId = $env:DA_ACTIVITY_ID }
}
if (-not $ActivityAlias) {
  $ActivityAlias = $env:DA_ACTIVITY_ALIAS
}
$resolved = Resolve-ActivityReference -Reference $ActivityId -FallbackAlias $ActivityAlias
$resolvedActivityId = $resolved.ActivityId
$resolvedActivityAlias = $resolved.Alias
if (-not $resolvedActivityAlias) { $resolvedActivityAlias = "prod" }

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
  if (-not $Engine) { $Engine = $env:DA_ENGINE }
  if (-not $Engine) { $Engine = "Autodesk.Revit+$RevitYear" }
  $newId = $env:DA_APPBUNDLE_ID
  if (-not $newId) { $newId = $BundleId }
  if (-not $newId) { Write-Error "For -InitializeBundle set -BundleId or DA_APPBUNDLE_ID / DA_APPBUNDLE_ID_$RevitYear to the new bundle id" }
  $createBody = @{
    id          = $newId
    engine      = $Engine
    description = "RevitMarkWorkitem - mark + parameterPatches"
  } | ConvertTo-Json
  $reg = Invoke-RestMethod -Method Post -Uri "$daBase/appbundles" -Headers $headers `
    -ContentType "application/json" -Body $createBody
} else {
  Write-Host "POST $daBase/appbundles/$BundleId/versions (new version) ..."
  $verUrl = "$daBase/appbundles/$BundleId/versions"
  if (-not $Engine) { $Engine = $env:DA_ENGINE }
  if (-not $Engine) { $Engine = "Autodesk.Revit+$RevitYear" }
  $versionBody = @{ engine = $Engine } | ConvertTo-Json
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

if (-not $SkipActivityPublish -and $resolvedActivityId) {
  Write-Host ""
  Write-Host "Publishing activity version for $resolvedActivityId (profile=$ActivityProfile) ..."
  $activityCommandLine = @()
  $activityParameters = @{}
  $activityDescription = ""
  if ($ActivityProfile -eq "create_model") {
    $activityCommandLine = @("`"`$(engine.path)\\revitcoreconsole.exe`" /i `"`$(args[inputFile].path)`" /al `"`$(appbundles[$BundleId].path)`"")
    $activityParameters = @{
      inputFile = @{
        verb = "get"
        description = "Bootstrap RVT used to launch RevitCoreConsole for create_model workflow"
        required = $true
        zip = $false
        localName = "input.rvt"
      }
      revitmodel = @{
        verb = "get"
        description = "Cloud model tool config (region/project/model/toolName/save)"
        required = $true
        zip = $false
        localName = "revitmodel.json"
      }
      toolinputs = @{
        verb = "get"
        description = "Create-model inputs (account/project/folder/worksharing/modelName)"
        required = $true
        zip = $false
        localName = "toolinputs.json"
      }
      result = @{
        verb = "put"
        description = "Optional activity result"
        required = $false
        zip = $false
        localName = "result.json"
      }
    }
    $activityDescription = "Revit cloud model create_model workflow"
  } else {
    $activityCommandLine = @("`"`$(engine.path)\\revitcoreconsole.exe`" /i `"`$(args[inputFile].path)`" /al `"`$(appbundles[$BundleId].path)`"")
    $activityParameters = @{
      inputFile = @{
        verb = "get"
        description = "Input Revit model (RVT)"
        required = $true
        zip = $false
      }
      payload = @{
        verb = "get"
        description = "JSON payload for mark + parameter updates"
        required = $true
        zip = $false
      }
      adsk3LeggedToken = @{
        verb = "get"
        description = "Autodesk 3-legged token for cloud operations"
        required = $false
        zip = $false
      }
      result = @{
        verb = "put"
        description = "Optional activity result"
        required = $false
        zip = $false
        localName = "result.json"
      }
    }
    $activityDescription = "RevitMarkWorkitem - mark + parameterPatches"
  }

  $activityBodyObject = @{
    commandLine = $activityCommandLine
    engine = $Engine
    appbundles = @("$BundleId+$($reg.version)")
    description = $activityDescription
    parameters = $activityParameters
  }
  $activityBody = $activityBodyObject | ConvertTo-Json -Depth 12

  if ($InitializeActivity) {
    Write-Host "POST $daBase/activities (initialize activity id) ..."
    $createActivityBodyObject = @{
      id = $resolvedActivityId
      commandLine = $activityCommandLine
      engine = $Engine
      appbundles = @("$BundleId+$($reg.version)")
      description = $activityDescription
      parameters = $activityParameters
    }
    $createActivityBody = $createActivityBodyObject | ConvertTo-Json -Depth 12
    $activityReg = Invoke-RestMethod -Method Post -Uri "$daBase/activities" -Headers $headers `
      -ContentType "application/json" -Body $createActivityBody
  } else {
    Write-Host "POST $daBase/activities/$resolvedActivityId/versions (new activity version) ..."
    $activityReg = Invoke-RestMethod -Method Post -Uri "$daBase/activities/$resolvedActivityId/versions" -Headers $headers `
      -ContentType "application/json" -Body $activityBody
  }

  $activityVersion = $activityReg.version
  if (-not $activityVersion) {
    Write-Error "Activity publish did not return a version: $($activityReg | ConvertTo-Json -Depth 10)"
  }

  $aliasCreateBody = @{
    id = $resolvedActivityAlias
    version = $activityVersion
  } | ConvertTo-Json
  try {
    Write-Host "POST $daBase/activities/$resolvedActivityId/aliases (create alias) ..."
    $null = Invoke-RestMethod -Method Post -Uri "$daBase/activities/$resolvedActivityId/aliases" -Headers $headers `
      -ContentType "application/json" -Body $aliasCreateBody
  } catch {
    Write-Host "Alias exists; patching to version $activityVersion ..."
    $aliasPatchBody = @{ version = $activityVersion } | ConvertTo-Json
    $null = Invoke-RestMethod -Method Patch -Uri "$daBase/activities/$resolvedActivityId/aliases/$resolvedActivityAlias" -Headers $headers `
      -ContentType "application/json" -Body $aliasPatchBody
  }

  Write-Host "Activity version published:" $activityVersion
  Write-Host "Resolved activity alias:" "$resolvedActivityId+$resolvedActivityAlias"
  Write-Host "Set in env:" "$activityByYearName=$resolvedActivityId+$resolvedActivityAlias"
} elseif (-not $SkipActivityPublish -and -not $resolvedActivityId) {
  Write-Host "Activity publish skipped: no ActivityId provided."
  Write-Host "Set -ActivityId or env DA_ACTIVITY_ID / $activityByYearName."
}

Write-Host "Reference: https://aps.autodesk.com/en/docs/design-automation/v3/tutorials/revit/step4-publish-appbundle/"
