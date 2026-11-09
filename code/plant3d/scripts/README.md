# Scripts (.scr) and Core Console batch

| File | Use |
|---|---|
| `titleblock_rev.scr` | load `att_tools.lsp`, set REV and DATE on the title block, `QSAVE` |
| `export_pdf_all_layouts.scr` | full AutoCAD: `-EXPORT PDF All` (VERIFY prompts in your version) |
| `batch_coreconsole.bat` | run a `.scr` on every DWG in a folder with `accoreconsole.exe` |
| `Invoke-CoreConsole.ps1` | same, in parallel jobs, with a CSV log of exit codes and times |

```bat
batch_coreconsole.bat "D:\Project\Ortho" titleblock_rev.scr
pwsh -File Invoke-CoreConsole.ps1 -Folder D:\Project\Ortho -Script .\titleblock_rev.scr -Parallel 4
```

## Core Console and Plant 3D — what really works
- Core Console = AutoCAD **without UI**. Plain DWG work (attributes, layers, purge, LISP without dialogs, `accoremgd` .NET DLLs) works and is fast.
- Plant 3D: Autodesk's developer blog states the Plant 3D **object enablers** load (`arx L PnP3dObjects.dbx`), but the Plant environment ARX apps do not, so **no Plant commands** are available ([source](https://blog.autodesk.io/can-i-run-autocad-pidplant-3d-in-console-mode/)). P&ID has no usable enabler.
- `/product PLNT3D` is commonly used to start Core Console with the Plant profile — VERIFY on your install (`accoreconsole.exe /?`). Without it, Plant objects show as proxies but attributes on normal blocks still edit fine.
- Scripts: every blank line is ENTER; keep `.scr` files free of trailing spaces; paths with spaces in "quotes"; use `_` and `.` prefixes (`_.QSAVE`) so commands work in every language version (Japanese AutoCAD included).
