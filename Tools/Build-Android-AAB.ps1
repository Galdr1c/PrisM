$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$defaultUnity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$unityEditor = if ($env:PRISM_UNITY_EDITOR) { $env:PRISM_UNITY_EDITOR } else { $defaultUnity }

if (-not (Test-Path -LiteralPath $unityEditor)) {
    throw "Unity 6000.6.0f1 editor not found at '$unityEditor'. Set PRISM_UNITY_EDITOR to the Unity.exe path."
}

$required = @('PRISM_KEYSTORE_PATH','PRISM_KEYSTORE_PASS','PRISM_KEY_ALIAS','PRISM_KEY_ALIAS_PASS')
foreach ($name in $required) {
    $value = [Environment]::GetEnvironmentVariable($name)
    if ([string]::IsNullOrWhiteSpace($value)) { throw "$name is required for a signed Google Play AAB." }
}
if (-not (Test-Path -LiteralPath $env:PRISM_KEYSTORE_PATH)) { throw "Keystore not found: $env:PRISM_KEYSTORE_PATH" }

$version = if ($env:PRISM_VERSION_NAME) { $env:PRISM_VERSION_NAME } else { '0.9.0' }
$code = if ($env:PRISM_VERSION_CODE) { $env:PRISM_VERSION_CODE } else { '90' }
$buildLog = Join-Path $projectRoot 'unity-android-release.log'

$args = @(
    '-batchmode',
    '-quit',
    '-buildTarget', 'android',
    '-licensingIpc', ('Unity-LicenseClient-' + $env:USERNAME),
    '-projectPath', ('"' + $projectRoot + '"'),
    '-executeMethod', 'Prism.Editor.BuildProject.BuildAndroidRelease',
    '-logFile', ('"' + $buildLog + '"')
)

$process = Start-Process -FilePath $unityEditor -ArgumentList $args -WindowStyle Hidden -Wait -PassThru
if ($process.ExitCode -ne 0) { throw "Unity Android release build failed ($($process.ExitCode)). See $buildLog" }

$output = Join-Path $projectRoot ("Builds\Android\PrisM-$version-$code.aab")
if (-not (Test-Path -LiteralPath $output)) { throw "Expected AAB was not produced: $output" }

Write-Output "Signed Google Play AAB ready:"
Write-Output $output
