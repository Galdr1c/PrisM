$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$unityEditor = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$buildLog = Join-Path $projectRoot 'unity-build.log'
$buildArgs = @('-batchmode', '-quit', '-licensingIpc', ('Unity-LicenseClient-' + $env:USERNAME), '-projectPath', ('"' + $projectRoot + '"'), '-executeMethod', 'Prism.Editor.BuildProject.Build', '-logFile', ('"' + $buildLog + '"'))
$buildProcess = Start-Process -FilePath $unityEditor -ArgumentList $buildArgs -WindowStyle Hidden -Wait -PassThru
if ($buildProcess.ExitCode -ne 0) { throw "Unity build failed ($($buildProcess.ExitCode)). See $buildLog" }
Write-Output (Join-Path $projectRoot 'Builds\Windows\PrisM.exe')
