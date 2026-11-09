<#
.SYNOPSIS  Export every .idw in a folder to PDF via Inventor COM.
.EXAMPLE   .\Export-InventorPdf.ps1 -Folder "C:\Work\Drawings" -Out "C:\Work\PDF"
#>
param([Parameter(Mandatory)][string]$Folder, [string]$Out = (Join-Path $Folder "_pdf"))

New-Item -ItemType Directory -Force -Path $Out | Out-Null
# New-Object -ComObject creates a NEW Inventor; reuse a running one if you prefer (see Get-InventorInfo.ps1)
$inv = New-Object -ComObject Inventor.Application
$inv.Visible = $false
$inv.SilentOperation = $true
try {
    $pdf = $inv.ApplicationAddIns.ItemById("{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}")
    foreach ($f in Get-ChildItem -Path $Folder -Filter *.idw) {
        $doc = $inv.Documents.Open($f.FullName, $false)
        try {
            $ctx = $inv.TransientObjects.CreateTranslationContext()
            $ctx.Type = 13059      # kFileBrowseIOMechanism
            $opts = $inv.TransientObjects.CreateNameValueMap()
            $dm = $inv.TransientObjects.CreateDataMedium()
            if ($pdf.HasSaveCopyAsOptions($doc, $ctx, $opts)) {
                # HasSaveCopyAsOptions pre-fills defaults -> overwrite; Add() only if the key is missing.
                # (PowerShell can assign COM parameterized properties, like Excel's $ws.Cells.Item(1,1) = 'x')
                foreach ($kv in @(@("All_Color_AS_Black", 0), @("Sheet_Range", 14082))) {   # 14082 = kPrintAllSheets
                    try { $opts.Value($kv[0]) = $kv[1] } catch { $opts.Add($kv[0], $kv[1]) }
                }
            }
            $dm.FileName = Join-Path $Out ($f.BaseName + ".pdf")
            $pdf.SaveCopyAs($doc, $ctx, $opts, $dm)
            "OK   $($f.Name)"
        } catch { "FAIL $($f.Name): $_" }
        finally { $doc.Close($true) }
    }
} finally {
    $inv.Quit()
    [System.Runtime.InteropServices.Marshal]::ReleaseComObject($inv) | Out-Null
}
