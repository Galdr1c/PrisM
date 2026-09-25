$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$gamePath = Join-Path $projectRoot 'Builds\Windows\PrisM.exe'
if (-not (Test-Path -LiteralPath $gamePath)) {
    throw "Windows build missing: $gamePath. Run Tools\Build-Windows.ps1 first."
}

function Get-PngInfo([string]$Path) {
    $bytes = [IO.File]::ReadAllBytes($Path)
    if ($bytes.Length -lt 26 -or $bytes[0] -ne 137 -or $bytes[1] -ne 80 -or $bytes[2] -ne 78 -or $bytes[3] -ne 71) {
        throw "Invalid PNG: $Path"
    }
    $width = ($bytes[16] -shl 24) -bor ($bytes[17] -shl 16) -bor ($bytes[18] -shl 8) -bor $bytes[19]
    $height = ($bytes[20] -shl 24) -bor ($bytes[21] -shl 16) -bor ($bytes[22] -shl 8) -bor $bytes[23]
    [PSCustomObject]@{ Width = $width; Height = $height; ColorType = $bytes[25] }
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

foreach ($shot in $shots) {
    $info = Get-PngInfo $shot.FullName
    if ($info.Width -lt 1080 -or $info.Height -lt 1920) {
        throw "$($shot.Name) is too small for the store capture contract: $($info.Width)x$($info.Height)"
    }
    $ratio = $info.Width / [double]$info.Height
    if ([Math]::Abs($ratio - (9.0/16.0)) -gt 0.01) {
        throw "$($shot.Name) is not 9:16: $($info.Width)x$($info.Height)"
    }
    if ($info.ColorType -ne 2) {
        throw "$($shot.Name) is not RGB PNG without alpha (PNG color type $($info.ColorType))."
    }
}

Write-Output "Play Store gameplay screenshots:"
$shots | Sort-Object Name | ForEach-Object { Write-Output $_.FullName }
