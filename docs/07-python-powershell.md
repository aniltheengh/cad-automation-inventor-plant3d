# 07 — Python (pywin32) and PowerShell for Inventor

Both talk to Inventor through **COM from outside the process**. Same object model as everything else;
different syntax, different traps.

## Contents
- [Python setup with Anaconda](#python-setup-with-anaconda)
- [Connecting, casting, constants](#connecting-casting-constants)
- [pywin32 rules you must know](#pywin32-rules-you-must-know)
- [Enum values table](#enum-values-table)
- [Pattern: Python does the engineering, Inventor does the geometry](#pattern-python-does-the-engineering-inventor-does-the-geometry)
- [The Python scripts](#the-python-scripts)
- [Limitations (be honest with yourself)](#limitations-be-honest-with-yourself)
- [Alternatives: pythonnet, comtypes, Inventor-hosted Python](#alternatives-pythonnet-comtypes-inventor-hosted-python)
- [PowerShell](#powershell)

## Python setup with Anaconda

```bat
:: Anaconda Prompt
conda create -n cad python=3.12 pywin32 openpyxl pandas -y
conda activate cad
python -c "import win32com.client as w; print(w.__file__)"
```

- Use a **dedicated env**; don't install into `base`.
- If `import win32com` fails with *DLL load failed*: `python %CONDA_PREFIX%\Scripts\pywin32_postinstall.py -install` (when installed with pip) or reinstall via conda.
- VS Code with the Python extension → select the `cad` interpreter → F5 debugging works like in Visual Studio.

## Connecting, casting, constants

`code/inventor/python/inv_connect.py`:

```python
import win32com.client
from win32com.client import gencache

app = win32com.client.GetActiveObject("Inventor.Application")   # running Inventor
app = gencache.EnsureDispatch(app)                               # early binding: typed members + constants
# or start one:  app = gencache.EnsureDispatch("Inventor.Application"); app.Visible = True

doc = app.ActiveDocument                       # comes back as the BASE interface 'Document'
part = win32com.client.CastTo(doc, "PartDocument")               # now .ComponentDefinition exists
k = win32com.client.constants.kPartDocumentObject                # 12290
```

Unlike .NET 8, pywin32 *does* have `GetActiveObject` — the elevation rule still applies (Python and
Inventor both normal user).

## pywin32 rules you must know

| Rule | Example |
|---|---|
| **Cast** derived interfaces | `CastTo(doc, "AssemblyDocument")` — otherwise `AttributeError: '<win32com.gen_py...Document>' object has no attribute 'ComponentDefinition'` |
| Collections are 1-based; `for x in coll` works (pywin32 enumerates) | `for i in range(1, coll.Count + 1): coll.Item(i)` |
| Default indexer = `.Item(...)` | `doc.PropertySets.Item("Design Tracking Properties").Item("Part Number").Value` |
| Parameterized property **put** → `SetXxx` | `opts.SetValue("Sheet_Range", 14082)` (makepy generates `SetValue`) |
| `out` parameters come back as a **tuple** | `ok, proxy = occ.CreateGeometryProxy(face)` (shape differs per method; print it) |
| Optional COM args: pass by position or omit at the end | `docs.Open(path, False)` |
| Enum constants exist only after `EnsureDispatch` | `win32com.client.constants.kFileBrowseIOMechanism` |
| `gen_py` cache can go stale | delete `%LOCALAPPDATA%\Temp\gen_py` after an Inventor upgrade |
| Strings with Japanese characters | fine (COM uses UTF-16); write CSV with `encoding="utf-8-sig"` so Excel shows them |
| Units: cm and radians | `mm_to_cm()` helper in `inv_connect.py` |

## Enum values table

Read from `Autodesk.Inventor.Interop.dll` by reflection (useful in PowerShell, or when `constants` is unavailable):

| Enum member | Value |
|---|---|
| `kPartDocumentObject` / `kAssemblyDocumentObject` / `kDrawingDocumentObject` / `kPresentationDocumentObject` | 12290 / 12291 / 12292 / 12293 |
| `kFileBrowseIOMechanism` (TranslationContext.Type) | 13059 |
| `kPrintAllSheets` (PDF Sheet_Range) | 14082 |
| `kJoinOperation` / `kCutOperation` | 20481 / 20482 |
| `kPositiveExtentDirection` / `kSymmetricExtentDirection` | 20993 / 20995 |
| `kStructuredBOMViewType` / `kPartsOnlyBOMViewType` | 62466 / 62467 |
| `kNormalBOMStructure` / `kPhantomBOMStructure` / `kReferenceBOMStructure` / `kPurchasedBOMStructure` | 51970 / 51971 / 51972 / 51973 |
| `kMicrosoftExcelFormat` (BOMView.Export) | 74498 |
| `kUnitlessUnits` / `kCentimeterLengthUnits` / `kMillimeterLengthUnits` | 11265 / 11268 / 11269 |
| `kTextUnits` / `kBooleanUnits` | 11346 / 11347 |
| `kFrontViewOrientation` / `kIsoTopRightViewOrientation` | 10764 / 10759 |
| `kHiddenLineDrawingViewStyle` | 32257 |
| `kBefore` / `kAfter` (EventTimingEnum) | 4097 / 4098 |

(The interop on the build box was the 2023 version; enum values are stable across releases.)

## Pattern: Python does the engineering, Inventor does the geometry

This is where Python shines for a mechanical engineer. `drive_gear_template.py`:

```python
from gear_math import Gear, min_teeth_no_undercut
from inv_connect import connect, cast

g = Gear(m, z)                              # pure Python involute maths, unit-tested
if z < min_teeth_no_undercut(20):
    print("warning: undercut")
inv = connect()
doc = cast(inv.Documents.Open(template, True))
params = doc.ComponentDefinition.Parameters
params.Item("m").Expression = f"{m} mm"
params.Item("z").Expression = f"{z}"
doc.Update()
doc.SaveAs(str(out_file), True)            # True = Save Copy As
```

Extend it with pandas: read a design table of 50 gears from Excel → loop → generate parts + a CSV
summary with masses. Or SciPy: optimise a bracket thickness by calling Inventor for mass and a simple
stress formula (keep the loop count small — every iteration is an Inventor rebuild).

## The Python scripts

| Script | Command |
|---|---|
| `list_params.py` | `python list_params.py --set Length=150mm --set Width="Length/2"` |
| `batch_export.py` | `python batch_export.py C:/Work/Project --pdf --dxf --step --recurse --out C:/Work/Export` |
| `iprops_excel.py` | `python iprops_excel.py export C:/Work/Project props.xlsx --recurse` → edit → `import props.xlsx` |
| `bom_to_excel.py` | `python bom_to_excel.py bom.xlsx --parts` |
| `gear_math.py` | `python gear_math.py 2 40` → d = 80, da = 84, df = 75, db = 75.175 mm |
| `drive_gear_template.py` | `python drive_gear_template.py GearTemplate.ipt 2 40 --b 20 --out C:/Work/Gears` |

## Limitations (be honest with yourself)

| Limitation | Consequence | Workaround |
|---|---|---|
| out-of-process COM | slow loops over geometry | do loops in iLogic/add-in; fetch data once |
| no Inventor events in practice | can't react to "before save" | iLogic trigger or add-in |
| no ribbon/UI inside Inventor | users run scripts from a terminal | small tkinter launcher, or call Python from an iLogic rule via `Process.Start` |
| pywin32 late-binding surprises | `AttributeError` until you cast | `cast()` everything you get from `Documents` |
| needs Python on each PC | deployment friction | PyInstaller one-file EXE, or keep Python for your own tools |
| Inventor shows a dialog | script hangs | `inv.SilentOperation = True` |
| Anaconda vs. corporate IT | blocked installs | Miniconda in user folder |

## Alternatives: pythonnet, comtypes, Inventor-hosted Python

- **pythonnet** (`import clr`) can load `Autodesk.Inventor.Interop.dll` and use .NET types — still out of process for Inventor; adds complexity. Rarely worth it over pywin32.
- **comtypes** — what pyautocad uses; works for Inventor too, similar rules.
- **Inside Inventor**: Inventor has no official embedded Python. Community add-ins exist; not covered here. For in-process logic use iLogic or C#.

## PowerShell

Good for admin scripts on locked-down PCs (PowerShell is always there). Files: `code/inventor/powershell/`.

```powershell
# attach to running Inventor: PowerShell 7 is .NET 8 → no [Marshal]::GetActiveObject → tiny C# helper
Add-Type -TypeDefinition @"
using System; using System.Runtime.InteropServices;
public static class ComRot {
  [DllImport("ole32.dll")] static extern int CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string p, out Guid c);
  [DllImport("oleaut32.dll")] static extern int GetActiveObject([MarshalAs(UnmanagedType.LPStruct)] Guid c, IntPtr r, [MarshalAs(UnmanagedType.IUnknown)] out object o);
  public static object Get(string progId) { ... }
}
"@
$inv = [ComRot]::Get("Inventor.Application")
$doc = $inv.ActiveDocument
foreach ($p in $doc.ComponentDefinition.Parameters) { "{0,-15} {1}" -f $p.Name, $p.Expression }
```

PowerShell COM rules:
- Enum names don't exist → **numbers** from the table above.
- `New-Object -ComObject Inventor.Application` **starts** a new Inventor (does not attach).
- Derived interfaces are resolved by `IDispatch` at run time — no cast needed (unlike pywin32 early binding).
- Parameterized property set: `$opts.Value("Sheet_Range") = 14082` (as in Excel `Cells.Item(r,c) = v`); fall back to `.Add()` for missing keys.
- Release: `[System.Runtime.InteropServices.Marshal]::ReleaseComObject($inv) | Out-Null` at the end of long scripts.
- Execution policy: `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned`, and `Unblock-File` on downloaded scripts.

Next: [08 — Apprentice, Design Automation, Vault](08-apprentice-design-automation-vault.md).
