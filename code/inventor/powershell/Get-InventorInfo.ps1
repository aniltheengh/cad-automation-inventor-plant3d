<#
.SYNOPSIS  Quick COM automation of Inventor from PowerShell (Windows PowerShell 5.1 or PowerShell 7).
.EXAMPLE   .\Get-InventorInfo.ps1
.EXAMPLE   .\Get-InventorInfo.ps1 -SetParameter Length=150
.NOTES     PowerShell 7 (.NET 8) has no [Marshal]::GetActiveObject either; this script uses a tiny
           C# P/Invoke type (Add-Type) that works in both 5.1 and 7.
#>
param([string]$SetParameter)

Add-Type -TypeDefinition @"
using System; using System.Runtime.InteropServices;
public static class ComRot {
  [DllImport("ole32.dll")] static extern int CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string p, out Guid c);
  [DllImport("oleaut32.dll")] static extern int GetActiveObject([MarshalAs(UnmanagedType.LPStruct)] Guid c, IntPtr r, [MarshalAs(UnmanagedType.IUnknown)] out object o);
  public static object Get(string progId) { Guid c; if (CLSIDFromProgIDEx(progId, out c) < 0) return null; object o; return GetActiveObject(c, IntPtr.Zero, out o) < 0 ? null : o; }
}
"@ -ErrorAction SilentlyContinue

$inv = [ComRot]::Get("Inventor.Application")
if (-not $inv) { Write-Error "Inventor is not running (or is running as a different user / elevation)."; exit 1 }

"Inventor {0}" -f $inv.SoftwareVersion.DisplayVersion
foreach ($d in $inv.Documents) { "  {0}" -f $d.FullFileName }

$doc = $inv.ActiveDocument
if ($doc -and $doc.DocumentType -in 12290, 12291) {   # kPartDocumentObject=12290, kAssemblyDocumentObject=12291
    $params = $doc.ComponentDefinition.Parameters
    foreach ($p in $params.UserParameters) { "  {0,-15} {1}" -f $p.Name, $p.Expression }
    if ($SetParameter) {
        $name, $value = $SetParameter -split '=', 2
        $params.UserParameters.Item($name).Expression = $value
        $doc.Update()
        "Set $name = $value"
    }
}
