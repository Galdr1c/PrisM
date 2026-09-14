$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$gamePath = Join-Path $projectRoot 'Builds\Windows\PrisM.exe'
$capturePath = Join-Path $projectRoot ('TestResults\smoke-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $capturePath -Force | Out-Null
$gameArgs = @('-prismSmoke', '-captureDir', ('"' + $capturePath + '"'), '-logFile', ('"' + (Join-Path $capturePath 'player.log') + '"'))
$gameProcess = Start-Process -FilePath $gamePath -ArgumentList $gameArgs -WindowStyle Hidden -PassThru
if (-not $gameProcess.WaitForExit(60000)) { Stop-Process -Id $gameProcess.Id; throw 'Smoke test timed out. Inspect player.log.' }
if ($gameProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath (Join-Path $capturePath 'runtime-smoke.txt'))) { throw 'Smoke test failed. Inspect player.log.' }
Get-Content -LiteralPath (Join-Path $capturePath 'runtime-smoke.txt')
