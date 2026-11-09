# check_powershell.ps1 - parse every .ps1 in the repo with the real PowerShell parser (no execution).
#   pwsh -File tools/check_powershell.ps1
$root = Split-Path -Parent $PSScriptRoot
$bad = 0
Get-ChildItem -Path $root -Recurse -Filter *.ps1 | ForEach-Object {
    $tokens = $null; $errors = $null
    [System.Management.Automation.Language.Parser]::ParseFile($_.FullName, [ref]$tokens, [ref]$errors) | Out-Null
    if ($errors.Count) { $bad++; "FAIL $($_.Name)"; $errors | ForEach-Object { "   line $($_.Extent.StartLineNumber): $($_.Message)" } }
    else { "OK   $($_.FullName.Substring($root.Length + 1))" }
}
if ($bad) { exit 1 }
