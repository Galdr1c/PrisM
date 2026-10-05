param([int]$Width=810,[int]$Height=1206)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$gamePath = Join-Path $projectRoot 'Builds\Windows\PrisM.exe'
$capturePath = Join-Path $projectRoot ('TestResults\smoke-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $capturePath -Force | Out-Null
$gameArgs = @('-prismSmoke','-screen-width',$Width,'-screen-height',$Height,'-screen-fullscreen','0', '-captureDir', ('"' + $capturePath + '"'), '-logFile', ('"' + (Join-Path $capturePath 'player.log') + '"'))
$gameProcess = Start-Process -FilePath $gamePath -ArgumentList $gameArgs -WindowStyle Normal -PassThru
if (-not $gameProcess.WaitForExit(180000)) { Stop-Process -Id $gameProcess.Id; throw 'Smoke test timed out. Inspect player.log.' }
if ($gameProcess.ExitCode -ne 0 -or -not (Test-Path -LiteralPath (Join-Path $capturePath 'runtime-smoke.txt'))) { throw 'Smoke test failed. Inspect player.log.' }
$resultFile = Join-Path $capturePath 'runtime-smoke.txt'
$lines = Get-Content -LiteralPath $resultFile
$solves = @($lines | Where-Object { $_ -match '^PASS runtime level [0-9]+$' }).Count
$resets = @($lines | Where-Object { $_ -match '^PASS runtime reset [0-9]+$' }).Count
if ($solves -ne 100 -or $resets -ne 100) { throw "Expected 100 solve and 100 reset checks. Inspect $resultFile" }

$logPath = Join-Path $capturePath 'player.log'
if (Test-Path -LiteralPath $logPath) {
    $bad = Select-String -LiteralPath $logPath -Pattern 'Exception:|NullReferenceException|MissingReferenceException|Assertion failed' -SimpleMatch:$false
    if ($bad) { throw "Smoke log contains runtime errors. Inspect $logPath" }
}

$lines
