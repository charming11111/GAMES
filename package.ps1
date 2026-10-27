param([string]$Version = '1.0.9', [switch]$Test)
$ErrorActionPreference = 'Stop'
& (Join-Path $PSScriptRoot 'build.ps1') -Test:$Test
$dist = Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Path $dist -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'out\GAMES.exe') -Destination (Join-Path $dist 'GAMES.exe') -Force
$files = @(@('GAMES.exe') | ForEach-Object { Join-Path $dist $_ })
$files += @('README.md','README.en.md','LICENSE','SDL-LICENSE.txt','SDL-NOTICE.txt','THIRD-PARTY-NOTICES.md') | ForEach-Object { Join-Path $PSScriptRoot $_ }
$zip = Join-Path $dist "GAMES-v$Version-windows-x64.zip"
Compress-Archive -LiteralPath $files -DestinationPath $zip -Force
$hashes = @('GAMES.exe', "GAMES-v$Version-windows-x64.zip") | ForEach-Object {
    $hash = Get-FileHash -LiteralPath (Join-Path $dist $_) -Algorithm SHA256
    "$($hash.Hash.ToLowerInvariant())  $_"
}
[IO.File]::WriteAllLines((Join-Path $dist 'SHA256SUMS.txt'), $hashes, [Text.UTF8Encoding]::new($false))
Write-Output "Release assets: $dist"
