param(
    [Parameter(Mandatory=$true)][string]$PayloadPath,
    [Parameter(Mandatory=$true)][string]$OutputPath
)
$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
$tools = Join-Path $root '.tools'
$archive = Join-Path $tools 'tcc-win64.zip'
$compiler = Join-Path $tools 'tinycc\tcc\tcc.exe'
$expectedHash = '34a721949a2583fdff725312da092fa0f5f1f284b702e6f811c6954714faabb2'
New-Item -ItemType Directory -Path $tools -Force | Out-Null
if (!(Test-Path -LiteralPath $archive)) {
    $urls = @(
        'https://download.savannah.gnu.org/releases/tinycc/tcc-0.9.27-win64-bin.zip',
        'https://github.com/phoenixthrush/Tiny-C-Compiler/releases/download/0.9.27-win64/tcc-0.9.27-win64-bin.zip'
    )
    $downloaded = $false
    foreach ($url in $urls) {
        try {
            Invoke-WebRequest -Uri $url -OutFile ($archive+'.download') -TimeoutSec 25 -UseBasicParsing
            if ((Get-FileHash -LiteralPath ($archive+'.download')).Hash.ToLowerInvariant() -ne $expectedHash) { throw 'Compiler archive checksum mismatch.' }
            Move-Item -LiteralPath ($archive+'.download') -Destination $archive -Force
            $downloaded = $true
            break
        } catch { Write-Warning 'Compiler source unavailable or checksum invalid; trying the next verified mirror.' }
    }
    if (!$downloaded) { throw 'Cannot download the checksum-pinned native compiler.' }
}
if ((Get-FileHash -LiteralPath $archive).Hash.ToLowerInvariant() -ne $expectedHash) { throw 'Native compiler archive checksum mismatch.' }
if (!(Test-Path -LiteralPath $compiler)) { Expand-Archive -LiteralPath $archive -DestinationPath (Join-Path $tools 'tinycc') -Force }
$outDir = Split-Path -Parent ([IO.Path]::GetFullPath($OutputPath))
New-Item -ItemType Directory -Path $outDir -Force | Out-Null
$native = Join-Path $outDir 'GAMES.Native.exe'
$bridge = Join-Path $outDir 'BootstrapBridge.dll'
$frameworkCompiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
& $frameworkCompiler /nologo /target:library /platform:x64 /optimize+ "/out:$bridge" (Join-Path $PSScriptRoot 'Bridge.cs')
if ($LASTEXITCODE -ne 0) { throw 'Managed entry bridge compilation failed.' }
& $compiler -nostdlib '-Wl,-subsystem=windows' -o $native (Join-Path $PSScriptRoot 'Launcher.c') -lkernel32 -luser32 -lmsvcrt
if ($LASTEXITCODE -ne 0) { throw 'Native entry point compilation failed.' }

if (!('GamesNativeResources' -as [type])) {
    Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class GamesNativeResources {
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)] public static extern IntPtr BeginUpdateResource(string file, bool delete);
    [DllImport("kernel32.dll", EntryPoint="UpdateResourceW", SetLastError=true)] public static extern bool UpdateResource(IntPtr update, IntPtr type, IntPtr name, ushort language, byte[] data, uint size);
    [DllImport("kernel32.dll", SetLastError=true)] public static extern bool EndUpdateResource(IntPtr update, bool discard);
}
'@
}
$update = [GamesNativeResources]::BeginUpdateResource($native,$false)
if ($update -eq [IntPtr]::Zero) { throw 'Cannot begin native icon/resource update.' }
$committed = $false
try {
    $icon = [IO.File]::ReadAllBytes((Join-Path $root 'app.ico'))
    $count = [BitConverter]::ToUInt16($icon,4)
    $group = New-Object byte[] (6+14*$count)
    [Buffer]::BlockCopy($icon,0,$group,0,6)
    for ($i=0;$i -lt $count;$i++) {
        $entry=6+16*$i
        $size=[BitConverter]::ToInt32($icon,$entry+8)
        $offset=[BitConverter]::ToInt32($icon,$entry+12)
        $image=New-Object byte[] $size
        [Buffer]::BlockCopy($icon,$offset,$image,0,$size)
        if (![GamesNativeResources]::UpdateResource($update,[IntPtr]3,[IntPtr]($i+1),0,$image,$image.Length)) { throw 'Cannot write native icon image.' }
        [Buffer]::BlockCopy($icon,$entry,$group,6+14*$i,12)
        [Buffer]::BlockCopy([BitConverter]::GetBytes([UInt16]($i+1)),0,$group,6+14*$i+12,2)
    }
    if (![GamesNativeResources]::UpdateResource($update,[IntPtr]14,[IntPtr]1,0,$group,$group.Length)) { throw 'Cannot write native icon group.' }
    $bridgeBytes=[IO.File]::ReadAllBytes($bridge)
    if (![GamesNativeResources]::UpdateResource($update,[IntPtr]10,[IntPtr]101,0,$bridgeBytes,$bridgeBytes.Length)) { throw 'Cannot embed the CLR entry bridge.' }
    $manifest=[Text.Encoding]::UTF8.GetBytes('<?xml version="1.0" encoding="UTF-8" standalone="yes"?><assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0"><assemblyIdentity version="1.0.0.0" processorArchitecture="amd64" name="GAMES.NativeLauncher" type="win32"/><trustInfo xmlns="urn:schemas-microsoft-com:asm.v3"><security><requestedPrivileges><requestedExecutionLevel level="asInvoker" uiAccess="false"/></requestedPrivileges></security></trustInfo></assembly>')
    if (![GamesNativeResources]::UpdateResource($update,[IntPtr]24,[IntPtr]1,0,$manifest,$manifest.Length)) { throw 'Cannot write native application manifest.' }
    $committed=[GamesNativeResources]::EndUpdateResource($update,$false)
    if (!$committed) { throw 'Cannot commit native resources.' }
} finally { if (!$committed) { [GamesNativeResources]::EndUpdateResource($update,$true) | Out-Null } }

$payload=[IO.File]::ReadAllBytes([IO.Path]::GetFullPath($PayloadPath))
if ($payload.Length -lt 128 -or $payload.Length -gt 64MB) { throw 'Invalid managed payload size.' }
$pe=[BitConverter]::ToInt32($payload,0x3c)
$optional=$pe+24
$directories=$optional+$(if([BitConverter]::ToUInt16($payload,$optional) -eq 0x20b){112}else{96})
if ([BitConverter]::ToUInt32($payload,$directories+14*8) -eq 0) { throw 'The payload must be a managed executable, not another bootstrapper.' }
$sha=[Security.Cryptography.SHA256]::Create()
try { $digest=$sha.ComputeHash($payload) } finally { $sha.Dispose() }
Copy-Item -LiteralPath $native -Destination $OutputPath -Force
$stream=[IO.File]::Open($OutputPath,[IO.FileMode]::Append,[IO.FileAccess]::Write,[IO.FileShare]::None)
$writer=New-Object IO.BinaryWriter($stream)
try {
    $offset=$stream.Position
    $writer.Write($payload)
    $writer.Write([Text.Encoding]::ASCII.GetBytes('GAMESPK1'))
    $writer.Write([UInt32]1)
    $writer.Write([UInt64]$offset)
    $writer.Write([UInt64]$payload.Length)
    $writer.Write($digest)
} finally { $writer.Dispose() }
Write-Output "Native single-file launcher: $OutputPath"
