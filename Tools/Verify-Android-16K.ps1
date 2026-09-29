param(
    [string]$AabPath
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$defaultUnity = 'C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe'
$unityEditor = if ($env:PRISM_UNITY_EDITOR) { $env:PRISM_UNITY_EDITOR } else { $defaultUnity }

if (-not (Test-Path -LiteralPath $unityEditor)) {
    throw "Unity editor not found at '$unityEditor'. Set PRISM_UNITY_EDITOR."
}

if ([string]::IsNullOrWhiteSpace($AabPath)) {
    $latest = Get-ChildItem -LiteralPath (Join-Path $projectRoot 'Builds\Android') -Filter '*.aab' -ErrorAction SilentlyContinue |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1
    if (-not $latest) { throw 'No Android App Bundle found under Builds\Android.' }
    $AabPath = $latest.FullName
} else {
    $AabPath = [IO.Path]::GetFullPath($AabPath)
}
if (-not (Test-Path -LiteralPath $AabPath)) { throw "AAB not found: $AabPath" }

$editorDir = Split-Path -Parent $unityEditor
$androidRoot = Join-Path $editorDir 'Data\PlaybackEngines\AndroidPlayer'
$java = Join-Path $androidRoot 'OpenJDK\bin\java.exe'
if ($env:PRISM_JAVA_PATH) { $java = $env:PRISM_JAVA_PATH }
if (-not (Test-Path -LiteralPath $java)) {
    $externalJava = Get-Command java.exe -ErrorAction SilentlyContinue
    if ($externalJava) { $java = $externalJava.Source }
}
if (-not (Test-Path -LiteralPath $java)) { throw 'Java was not found. Set PRISM_JAVA_PATH.' }

$bundletool = $env:PRISM_BUNDLETOOL_JAR
if ([string]::IsNullOrWhiteSpace($bundletool)) {
    $candidate = Get-ChildItem -LiteralPath $androidRoot -Recurse -Filter 'bundletool*.jar' -ErrorAction SilentlyContinue |
        Sort-Object FullName |
        Select-Object -First 1
    if ($candidate) { $bundletool = $candidate.FullName }
}
if ([string]::IsNullOrWhiteSpace($bundletool) -or -not (Test-Path -LiteralPath $bundletool)) {
    throw 'bundletool JAR is required for 16 KB AAB validation. Set PRISM_BUNDLETOOL_JAR to an official bundletool .jar.'
}

$ndkRoot = if ($env:PRISM_NDK_PATH) { $env:PRISM_NDK_PATH } else { Join-Path $androidRoot 'NDK' }
$readelf = Get-ChildItem -LiteralPath $ndkRoot -Recurse -Filter 'llvm-readelf.exe' -ErrorAction SilentlyContinue |
    Select-Object -First 1
if (-not $readelf) { throw 'llvm-readelf.exe was not found in the Unity Android NDK.' }

Write-Host '== AAB page-alignment request =='
$config = & $java -jar $bundletool dump config --bundle="$AabPath" 2>&1
if ($LASTEXITCODE -ne 0) { throw "bundletool dump config failed:
$($config -join [Environment]::NewLine)" }
$configText = $config -join [Environment]::NewLine
if ($configText -notmatch 'PAGE_ALIGNMENT_16K') {
    throw 'AAB does not declare PAGE_ALIGNMENT_16K. Do not upload this bundle to Google Play.'
}
Write-Host 'PASS PAGE_ALIGNMENT_16K'

Write-Host '== Native ELF LOAD alignment =='
Add-Type -AssemblyName System.IO.Compression.FileSystem
$temp = Join-Path ([IO.Path]::GetTempPath()) ('prism-16k-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temp -Force | Out-Null

try {
    $zip = [IO.Compression.ZipFile]::OpenRead($AabPath)
    try {
        $entries = @($zip.Entries | Where-Object { $_.FullName -match '^base/lib/arm64-v8a/.+\.so$' })
        if ($entries.Count -eq 0) { throw 'No ARM64 native libraries found in the AAB; expected IL2CPP ARM64 libraries.' }

        foreach ($entry in $entries) {
            $safeName = ($entry.FullName -replace '[^A-Za-z0-9_.-]','_')
            $dest = Join-Path $temp $safeName
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entry,$dest,$true)

            $elf = & $readelf.FullName -lW $dest 2>&1
            if ($LASTEXITCODE -ne 0) { throw "llvm-readelf failed for $($entry.FullName)." }

            $loadLines = @($elf | Where-Object { $_ -match '^\s*LOAD\s+' })
            if ($loadLines.Count -eq 0) { throw "No ELF LOAD segments found in $($entry.FullName)." }

            foreach ($line in $loadLines) {
                $parts = @($line.Trim() -split '\s+')
                $alignToken = $parts[-1]
                if ($alignToken -notmatch '^0x[0-9A-Fa-f]+$') {
                    throw "Could not parse ELF alignment in $($entry.FullName): $line"
                }
                $align = [Convert]::ToInt64($alignToken.Substring(2),16)
                if ($align -lt 16384) {
                    throw "$($entry.FullName) has a LOAD segment aligned to $alignToken (< 0x4000). Do not upload this bundle."
                }
            }
            Write-Host ("PASS {0}" -f $entry.FullName)
        }
    } finally {
        $zip.Dispose()
    }
} finally {
    $resolvedTemp = [IO.Path]::GetFullPath($temp)
    $expectedParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath())
    if ($resolvedTemp.StartsWith($expectedParent,[StringComparison]::OrdinalIgnoreCase) -and
        (Split-Path -Leaf $resolvedTemp) -like 'prism-16k-*') {
        Remove-Item -LiteralPath $resolvedTemp -Recurse -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ''
Write-Host 'ANDROID 16 KB PAGE-SIZE VALIDATION PASSED'
Write-Output $AabPath
