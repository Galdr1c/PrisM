param(
    [switch]$RequireAndroid
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
Push-Location $projectRoot
try {
    Write-Host '== PrisM core verification =='
    dotnet run --project Tests/CoreTests.csproj
    if ($LASTEXITCODE -ne 0) { throw 'Core tests failed.' }

    Write-Host '== PrisM Windows build =='
    & (Join-Path $PSScriptRoot 'Build-Windows.ps1')

    Write-Host '== PrisM 100-level runtime smoke =='
    & (Join-Path $PSScriptRoot 'Smoke-Windows.ps1')

    Write-Host '== PrisM store assets =='
    & (Join-Path $PSScriptRoot 'Generate-Store-Assets.ps1')

    Write-Host '== PrisM real gameplay store screenshots =='
    & (Join-Path $PSScriptRoot 'Capture-Store-Screens.ps1')

    $required = @('PRISM_KEYSTORE_PATH','PRISM_KEYSTORE_PASS','PRISM_KEY_ALIAS','PRISM_KEY_ALIAS_PASS')
    $missing = @()
    foreach ($name in $required) {
        if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) { $missing += $name }
    }

    if ($missing.Count -eq 0) {
        Write-Host '== PrisM signed Android AAB =='
        & (Join-Path $PSScriptRoot 'Build-Android-AAB.ps1')
    } elseif ($RequireAndroid) {
        throw ('Android signing variables are required: ' + ($missing -join ', '))
    } else {
        Write-Warning ('Android AAB skipped because signing variables are missing: ' + ($missing -join ', '))
    }

    Write-Host ''
    Write-Host 'PRE-RELEASE PREFLIGHT PASSED'
    Write-Host 'Review docs/google-play-release.md before Play Console upload.'
} finally {
    Pop-Location
}
