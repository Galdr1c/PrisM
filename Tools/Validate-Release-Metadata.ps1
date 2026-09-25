$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw $Message }
}

function Extract-Field([string]$Text, [string]$Heading, [string]$NextHeading) {
    $pattern = '(?ms)^\*\*' + [regex]::Escape($Heading) + '\*\*\s*\r?\n(.+?)(?=\r?\n\r?\n\*\*' + [regex]::Escape($NextHeading) + '\*\*)'
    $match = [regex]::Match($Text, $pattern)
    if (-not $match.Success) { throw "Store listing field '$Heading' could not be parsed." }
    return $match.Groups[1].Value.Trim()
}

Write-Host '== Release source contract =='
$buildPath = Join-Path $projectRoot 'Assets\Prism\Editor\BuildProject.cs'
$build = Get-Content -LiteralPath $buildPath -Raw
Assert-True ($build.Contains('AndroidApiLevel36')) 'Android release must target API 36.'
Assert-True ($build.Contains('AndroidArchitecture.ARM64')) 'Android release must include ARM64.'
Assert-True ($build.Contains('ScriptingImplementation.IL2CPP')) 'Android release must use IL2CPP.'
Assert-True ($build.Contains('buildAppBundle=true')) 'Android release must build an AAB.'
Assert-True ($build.Contains('PRISM_KEYSTORE_PATH')) 'Release signing must use environment-provided keystore data.'

$projectSettings = Get-Content -LiteralPath (Join-Path $projectRoot 'ProjectSettings\ProjectSettings.asset') -Raw
Assert-True ($projectSettings -match 'AndroidTargetSdkVersion:\s*36') 'ProjectSettings Android target SDK must be 36.'
Assert-True ($projectSettings -match 'AndroidBundleVersionCode:\s*90') 'ProjectSettings pre-release versionCode must be 90.'

Write-Host '== PowerShell syntax =='
Get-ChildItem -LiteralPath $PSScriptRoot -Filter '*.ps1' | ForEach-Object {
    $tokens = $null
    $errors = $null
    [System.Management.Automation.Language.Parser]::ParseFile($_.FullName, [ref]$tokens, [ref]$errors) | Out-Null
    if ($errors.Count -gt 0) {
        $detail = ($errors | ForEach-Object { $_.Message }) -join '; '
        throw "PowerShell syntax error in $($_.Name): $detail"
    }
}
Write-Host 'PASS PowerShell syntax'

Write-Host '== Store listing constraints =='
$listing = Get-Content -LiteralPath (Join-Path $projectRoot 'docs\store-listing.md') -Raw
$trStart = $listing.IndexOf('## Turkish')
$enStart = $listing.IndexOf('## English')
Assert-True ($trStart -ge 0 -and $enStart -gt $trStart) 'Store listing must contain Turkish and English sections.'
$tr = $listing.Substring($trStart, $enStart-$trStart)
$en = $listing.Substring($enStart)

$trName = Extract-Field $tr 'App name' 'Short description'
$trShort = Extract-Field $tr 'Short description' 'Full description'
$enName = Extract-Field $en 'App name' 'Short description'
$enShort = Extract-Field $en 'Short description' 'Full description'

Assert-True ($trName.Length -le 30) "Turkish Play app name exceeds 30 characters: $($trName.Length)"
Assert-True ($enName.Length -le 30) "English Play app name exceeds 30 characters: $($enName.Length)"
Assert-True ($trShort.Length -le 80) "Turkish short description exceeds 80 characters: $($trShort.Length)"
Assert-True ($enShort.Length -le 80) "English short description exceeds 80 characters: $($enShort.Length)"
Assert-True ($listing.Contains('100')) 'Store listing must accurately describe the 100-level campaign.'

Write-Host '== Required release documents =='
$requiredDocs = @(
    'docs\google-play-release.md',
    'docs\privacy-policy.md',
    'docs\store-listing.md',
    'docs\verification.md'
)
foreach ($relative in $requiredDocs) {
    Assert-True (Test-Path -LiteralPath (Join-Path $projectRoot $relative)) "Missing release document: $relative"
}

Write-Host ''
Write-Host 'RELEASE METADATA VALIDATION PASSED'
