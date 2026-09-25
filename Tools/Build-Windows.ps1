$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$defaultUnity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$unityEditor = if ($env:PRISM_UNITY_EDITOR) { $env:PRISM_UNITY_EDITOR } else { $defaultUnity }
if (-not (Test-Path -LiteralPath $unityEditor)) { throw "Unity editor not found at '$unityEditor'. Set PRISM_UNITY_EDITOR." }

$buildLog = Join-Path $projectRoot 'unity-build.log'
$buildArgs = @(
    '-batchmode',
    '-quit',
    '-buildTarget', 'win64',
    '-licensingIpc', ('Unity-LicenseClient-' + $env:USERNAME),
    '-projectPath', ('"' + $projectRoot + '"'),
    '-executeMethod', 'Prism.Editor.BuildProject.BuildWindowsDevelopment',
    '-logFile', ('"' + $buildLog + '"')
)
$buildProcess = Start-Process -FilePath $unityEditor -ArgumentList $buildArgs -WindowStyle Hidden -Wait -PassThru
if ($buildProcess.ExitCode -ne 0) { throw "Unity build failed ($($buildProcess.ExitCode)). See $buildLog" }

$output = Join-Path $projectRoot 'Builds\Windows\PrisM.exe'
if (-not (Test-Path -LiteralPath $output)) { throw "Expected Windows build was not produced: $output" }
Write-Output $output
