$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$defaultUnity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$unityEditor = if ($env:PRISM_UNITY_EDITOR) { $env:PRISM_UNITY_EDITOR } else { $defaultUnity }
if (-not (Test-Path -LiteralPath $unityEditor)) {
    throw "Unity editor not found at '$unityEditor'. Set PRISM_UNITY_EDITOR."
}

$log = Join-Path $projectRoot 'unity-store-assets.log'
$args = @(
    '-batchmode',
    '-quit',
    '-licensingIpc', ('Unity-LicenseClient-' + $env:USERNAME),
    '-projectPath', ('"' + $projectRoot + '"'),
    '-executeMethod', 'Prism.Editor.BrandAssets.GenerateStoreAssets',
    '-logFile', ('"' + $log + '"')
)
$p = Start-Process -FilePath $unityEditor -ArgumentList $args -WindowStyle Hidden -Wait -PassThru
if ($p.ExitCode -ne 0) { throw "Store asset generation failed ($($p.ExitCode)). See $log" }

$icon = Join-Path $projectRoot 'Builds\StoreAssets\play-icon-512.png'
$feature = Join-Path $projectRoot 'Builds\StoreAssets\feature-graphic-1024x500.png'
if (-not (Test-Path -LiteralPath $icon)) { throw "Missing store icon: $icon" }
if (-not (Test-Path -LiteralPath $feature)) { throw "Missing feature graphic: $feature" }
Write-Output $icon
Write-Output $feature
