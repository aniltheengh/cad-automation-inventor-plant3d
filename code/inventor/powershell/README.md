# PowerShell scripts — Inventor COM without Visual Studio

| Script | Does |
|---|---|
| `Get-InventorInfo.ps1` | attach to the running Inventor (P/Invoke `GetActiveObject` via `Add-Type`), print version, documents, parameters; `-SetParameter Length=150` |
| `Export-InventorPdf.ps1` | start a hidden Inventor, export every `.idw` in a folder to PDF |

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned      # once, if scripts are blocked
.\Get-InventorInfo.ps1
.\Export-InventorPdf.ps1 -Folder C:\Work\Drawings -Out C:\Work\PDF
```

Rules learned the hard way (all in the scripts):
- Enum constants are not available by name → use the numbers (`13059` = `kFileBrowseIOMechanism`). Values in [docs/07](../../../docs/07-python-powershell.md).
- Translator options: `HasSaveCopyAsOptions` pre-fills the `NameValueMap` with defaults → overwrite with `$opts.Value("x") = 1` (PowerShell can assign COM parameterized properties, the same idiom as Excel's `$ws.Cells.Item(1,1) = 'x'`); the script falls back to `.Add(name, value)` if the key is missing. VERIFY on your machine — not executed on the build box.
- `New-Object -ComObject Inventor.Application` starts a **new** Inventor; it does not attach.
- Parsed with the real PowerShell 7.4 parser in CI (`tools/check_powershell.ps1`). Not executed (no Inventor on the build box).
