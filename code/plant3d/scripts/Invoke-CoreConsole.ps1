<#
.SYNOPSIS  Run an AutoCAD script on many DWGs in parallel with accoreconsole.exe.
.EXAMPLE   .\Invoke-CoreConsole.ps1 -Folder D:\Project\Ortho -Script .\titleblock_rev.scr -Parallel 4
.NOTES     Core Console = AutoCAD without the UI: no ribbon, no dialogs, only accoremgd-based .NET
           plug-ins (NETLOAD works if the DLL does not reference acmgd.dll). Exit code per file is logged.
#>
param(
  [Parameter(Mandatory)][string]$Folder,
  [Parameter(Mandatory)][string]$Script,
  [string]$CoreConsole = "C:\Program Files\Autodesk\AutoCAD 2026\accoreconsole.exe",
  [string]$Product = "PLNT3D",        # VERIFY: product switch for Plant 3D objects; omit for plain AutoCAD
  [int]$Parallel = 2,
  [switch]$Recurse
)
if (-not (Test-Path $CoreConsole)) { throw "accoreconsole.exe not found: $CoreConsole" }
$Script = (Resolve-Path $Script).Path
$files = Get-ChildItem -Path $Folder -Filter *.dwg -Recurse:$Recurse | Where-Object { $_.FullName -notmatch '\\_backup\\' }
$log = Join-Path $Folder ("coreconsole_{0:yyyyMMdd_HHmm}.csv" -f (Get-Date))
"File,ExitCode,Seconds" | Set-Content $log -Encoding UTF8

$jobs = @()
foreach ($f in $files) {
  while (@($jobs | Where-Object State -eq 'Running').Count -ge $Parallel) { Start-Sleep -Milliseconds 300 }
  $jobs += Start-Job -ArgumentList $CoreConsole, $f.FullName, $Script, $Product -ScriptBlock {
    param($acc, $dwg, $scr, $prod)
    $sw = [Diagnostics.Stopwatch]::StartNew()
    $argList = @('/i', "`"$dwg`"", '/s', "`"$scr`"", '/l', 'en-US')
    if ($prod) { $argList += @('/product', $prod) }
    $p = Start-Process -FilePath $acc -ArgumentList $argList -NoNewWindow -Wait -PassThru
    [pscustomobject]@{ File = $dwg; ExitCode = $p.ExitCode; Seconds = [math]::Round($sw.Elapsed.TotalSeconds, 1) }
  }
}
$jobs | Wait-Job | Receive-Job | ForEach-Object { "{0},{1},{2}" -f $_.File, $_.ExitCode, $_.Seconds | Add-Content $log; $_ }
$jobs | Remove-Job
"Log: $log"
