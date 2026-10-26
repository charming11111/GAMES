param([Parameter(Mandatory=$true)][string]$Executable)
$ErrorActionPreference = 'Stop'
$exe=[IO.Path]::GetFullPath($Executable)
$outDir=Split-Path -Parent $exe
$bytes=[IO.File]::ReadAllBytes($exe)
$pe=[BitConverter]::ToInt32($bytes,0x3c)
$optional=$pe+24
if ([BitConverter]::ToUInt16($bytes,$pe+4) -ne 0x8664 -or [BitConverter]::ToUInt16($bytes,$optional) -ne 0x20b) { throw 'Entry point is not native Windows x64.' }
$directories=$optional+112
if ([BitConverter]::ToUInt32($bytes,$directories+14*8) -ne 0) { throw 'Outer executable unexpectedly depends on the CLR loader.' }
$sectionCount=[BitConverter]::ToUInt16($bytes,$pe+6)
$sections=$optional+[BitConverter]::ToUInt16($bytes,$pe+20)
function Get-RawOffset([uint32]$rva) {
    for($i=0;$i -lt $sectionCount;$i++) {
        $s=$sections+$i*40
        $virtual=[BitConverter]::ToUInt32($bytes,$s+12)
        $length=[Math]::Max([BitConverter]::ToUInt32($bytes,$s+8),[BitConverter]::ToUInt32($bytes,$s+16))
        if($rva -ge $virtual -and $rva -lt $virtual+$length){return [int]($rva-$virtual+[BitConverter]::ToUInt32($bytes,$s+20))}
    }
    throw 'Invalid native import RVA.'
}
$import=Get-RawOffset ([BitConverter]::ToUInt32($bytes,$directories+8))
$dlls=@()
while(($nameRva=[BitConverter]::ToUInt32($bytes,$import+12)) -ne 0) {
    $offset=Get-RawOffset $nameRva
    $end=$offset;while($bytes[$end] -ne 0){$end++}
    $dlls += [Text.Encoding]::ASCII.GetString($bytes,$offset,$end-$offset).ToLowerInvariant()
    $import+=20
}
if(@($dlls | Where-Object {$_ -notin @('kernel32.dll','user32.dll','msvcrt.dll')}).Count) { throw 'Unexpected native entry dependency.' }
$report=Join-Path $outDir 'bootstrap-test.json'
$native=Start-Process -FilePath $exe -ArgumentList '--bootstrap-self-test' -RedirectStandardOutput $report -WindowStyle Hidden -Wait -PassThru
$result=Get-Content -LiteralPath $report -Raw | ConvertFrom-Json
if($native.ExitCode -ne 0 -or !$result.native_bootstrap -or !$result.payload_valid -or !$result.checks_passed){throw 'Native runtime/bundle boundary checks failed.'}

# A damaged appended payload must be rejected without attempting CLR startup.
$damaged=Join-Path $outDir 'GAMES.integrity-test.exe'
$bytes[$bytes.Length-1]=$bytes[$bytes.Length-1] -bxor 1
[IO.File]::WriteAllBytes($damaged,$bytes)
$damagedReport=Join-Path $outDir 'bootstrap-integrity-test.json'
$invalid=Start-Process -FilePath $damaged -ArgumentList '--bootstrap-self-test' -RedirectStandardOutput $damagedReport -WindowStyle Hidden -Wait -PassThru
if($invalid.ExitCode -eq 0 -or (Get-Content -LiteralPath $damagedReport -Raw | ConvertFrom-Json).payload_valid){throw 'Damaged bundle was accepted.'}

# Exercise native -> CLR -> WPF argument forwarding from a spaced Unicode path.
$alias=Join-Path $outDir ('native path '+[char]0x6d4b+[char]0x8bd5)
New-Item -ItemType Directory -Path $alias -Force | Out-Null
$aliasExe=Join-Path $alias 'GAMES.exe'
Copy-Item -LiteralPath $exe -Destination $aliasExe -Force
$started=Get-Date
$managed=Start-Process -FilePath $aliasExe -ArgumentList '--self-test' -WindowStyle Hidden -Wait -PassThru
$resultPath=Join-Path $alias 'test-result.txt'
if(!(Test-Path -LiteralPath $resultPath) -or (Get-Item -LiteralPath $resultPath).LastWriteTime -lt $started){throw 'Hosted managed regression result was not produced.'}
$text=Get-Content -LiteralPath $resultPath -Raw
Write-Output $text
if($managed.ExitCode -ne 0 -or !$text.StartsWith('PASS:')){throw 'Hosted WPF regression checks failed.'}
Copy-Item -LiteralPath $resultPath -Destination (Join-Path $outDir 'test-result.txt') -Force
Write-Output 'PASS: native x64/system-DLL-only entry, Framework version boundaries, bundle integrity, tamper rejection, Unicode command-line forwarding.'
