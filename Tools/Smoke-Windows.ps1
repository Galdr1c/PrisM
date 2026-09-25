$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$gamePath = Join-Path $projectRoot 'Builds\Windows\PrisM.exe'
$capturePath = Join-Path $projectRoot ('TestResults\smoke-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $capturePath -Force | Out-Null
$gameArgs = @('-prismSmoke', '-captureDir', ('"' + $capturePath + '"'), '-logFile', ('"' + (Join-Path $capturePath 'player.log') + '"'))
$gameProcess = Start-Process -FilePath $gamePath -ArgumentList $gameArgs -WindowStyle Normal -PassThru
if (-not $gameProcess.WaitForExit(180000)) { Stop-Process -Id $gameProcess.Id; throw 'Smoke test timed out. Inspect player.log.' }
if ($gameProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath (Join-Path $capturePath 'runtime-smoke.txt'))) { throw 'Smoke test failed. Inspect player.log.' }
$resultFile = Join-Path $capturePath 'runtime-smoke.txt'
$lines = Get-Content -LiteralPath $resultFile
$passes = @($lines | Where-Object { $_ -like 'PASS *' }).Count
if ($passes -ne 200) { throw "Expected 200 runtime PASS lines (solve + reset for 100 levels); got $passes. Inspect $resultFile" }

$logPath = Join-Path $capturePath 'player.log'
if (Test-Path -LiteralPath $logPath) {
    $bad = Select-String -LiteralPath $logPath -Pattern 'Exception:|NullReferenceException|MissingReferenceException|Assertion failed' -SimpleMatch:$false
    if ($bad) { throw "Smoke log contains runtime errors. Inspect $logPath" }
}

$lines
