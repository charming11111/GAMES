param([switch]$Test)
$ErrorActionPreference = 'Stop'
if ($env:OS -ne 'Windows_NT') { throw 'GAMES requires Windows.' }
$framework = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319'
$compiler = Join-Path $framework 'csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'Install .NET Framework 4.8 before building.' }
$outDir = Join-Path $PSScriptRoot 'out'
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$exe = Join-Path $outDir 'GAMES.exe'
$managedExe = Join-Path $outDir 'GAMES.Core.exe'
$refs = @('System.dll','System.Core.dll','System.Xaml.dll','System.Web.Extensions.dll','System.Windows.Forms.dll','System.Drawing.dll')
$refs += @('WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll') | ForEach-Object { Join-Path (Join-Path $framework 'WPF') $_ }
$sources = @('Shell.cs','Visuals.cs','Menus.cs','Controllers.cs','Metadata.cs','Tests.cs','Navigation.cs','TechDesign.cs','Audio.cs','Cache.cs','Tray.cs','LaunchWatch.cs','CompactHeader.cs','Performance.cs','ContinuousScroll.cs','MotionProfile.cs','Discovery.cs','YuzuArtwork.cs')
$buildArgs = @('/nologo','/target:winexe','/platform:x64','/optimize+',"/out:$managedExe","/win32icon:$PSScriptRoot\app.ico")
$buildArgs += $refs | ForEach-Object { '/reference:' + $_ }
$buildArgs += @("/resource:$PSScriptRoot\SDL2.dll,SDL2.dll","/resource:$PSScriptRoot\SDL-NOTICE.txt,SDL-NOTICE.txt","/resource:$PSScriptRoot\entry-profiles.json,entry-profiles.json","/resource:$PSScriptRoot\yuzu-cover.png,yuzu-cover.png","/resource:$PSScriptRoot\app.ico,app.ico")
$buildArgs += $sources | ForEach-Object { Join-Path $PSScriptRoot $_ }
& $compiler $buildArgs
if ($LASTEXITCODE -ne 0) { throw 'Compilation failed.' }
& (Join-Path $PSScriptRoot 'bootstrap\build-native.ps1') -PayloadPath $managedExe -OutputPath $exe
Write-Output "Built $exe"
if ($Test) {
    & (Join-Path $PSScriptRoot 'bootstrap\test-native.ps1') -Executable $exe
}
