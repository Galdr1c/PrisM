$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$gamePath = Join-Path $projectRoot 'Builds\Windows\PrisM.exe'
if (-not (Test-Path -LiteralPath $gamePath)) {
    throw "Windows build missing: $gamePath. Run Tools\Build-Windows.ps1 first."
}

$output = Join-Path $projectRoot ('Builds\StoreAssets\screens-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
New-Item -ItemType Directory -Path $output -Force | Out-Null
$log = Join-Path $output 'player.log'
$args = @(
    '-prismStoreCapture',
    '-captureDir', ('"' + $output + '"'),
    '-screen-width', '1080',
    '-screen-height', '1920',
    '-screen-fullscreen', '0',
    '-logFile', ('"' + $log + '"')
)
$p = Start-Process -FilePath $gamePath -ArgumentList $args -WindowStyle Normal -PassThru
if (-not $p.WaitForExit(120000)) {
    Stop-Process -Id $p.Id -Force
    throw "Store screenshot capture timed out. Inspect $log"
}
if ($p.ExitCode -ne 0) { throw "Store screenshot capture failed. Inspect $log" }

$marker = Join-Path $output 'capture-complete.txt'
if (-not (Test-Path -LiteralPath $marker)) { throw "Capture marker missing. Inspect $log" }

$shots = @(Get-ChildItem -LiteralPath $output -Filter '*.png')
if ($shots.Count -lt 6) { throw "Expected at least 6 real gameplay screenshots; got $($shots.Count)." }

Write-Output "Play Store gameplay screenshots:"
$shots | Sort-Object Name | ForEach-Object { Write-Output $_.FullName }
