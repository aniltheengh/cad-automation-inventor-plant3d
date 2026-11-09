# 12 — AutoLISP, .scr scripts, Core Console, and Python for AutoCAD / Plant 3D

The "lightweight" tools. Perfect for drawing-level work on Plant 3D orthos, isos, P&IDs and general
arrangement drawings — and you can learn them in plain AutoCAD before your Plant 3D trial starts.

## Contents
- [AutoLISP in 20 minutes](#autolisp-in-20-minutes)
- [Visual LISP / ActiveX from LISP](#visual-lisp--activex-from-lisp)
- [The LISP files in this repo](#the-lisp-files-in-this-repo)
- [Editing and debugging LISP: VS Code](#editing-and-debugging-lisp-vs-code)
- [Script files (.scr)](#script-files-scr)
- [Core Console](#core-console)
- [Batch patterns compared](#batch-patterns-compared)
- [Python for AutoCAD and Plant 3D](#python-for-autocad-and-plant-3d)
- [Security: SECURELOAD and TRUSTEDPATHS](#security-secureload-and-trustedpaths)

## AutoLISP in 20 minutes

LISP = lists in parentheses: `(function arg1 arg2)`.

```lisp
(setq d 114.3)                          ; variable
(setq r (/ d 2.0))                      ; arithmetic: (/ a b) — always use 2.0 for real division
(defun area (r) (* pi r r))             ; function
(defun c:PIPEAREA (/ od)                ; c: = command; after "/" = local variables
  (setq od (getreal "\nPipe OD <114.3>: "))
  (if (null od) (setq od 114.3))
  (princ (strcat "\nArea = " (rtos (area (/ od 2.0)) 2 1) " mm2"))
  (princ))                              ; quiet exit (no "nil" printed)
```

| Need | Function |
|---|---|
| user input | `getstring` (`T` = allow spaces), `getreal`, `getint`, `getpoint`, `entsel` |
| select objects | `(ssget "_X" '((0 . "INSERT") (66 . 1)))` = all blocks with attributes; `sslength`, `ssname` |
| entity data | `(entget e)` → DXF list; `(cdr (assoc 8 ed))` = layer |
| run a command | `(command "_.LAYER" "_T" "P-*" "")` — `_` = English name, `.` = built-in version |
| system variables | `(getvar "DWGNAME")`, `(setvar "CMDECHO" 0)` |
| strings | `strcat`, `substr`, `strcase`, `vl-string-subst`, `rtos`, `itoa`, `atof` |
| lists | `car`, `cdr`, `cons`, `foreach`, `mapcar`, `vl-sort`, `vl-remove-if-not` |
| files | `open`, `write-line`, `read-line`, `close`, `findfile` |

## Visual LISP / ActiveX from LISP

`(vl-load-com)` gives LISP the **ActiveX object model** — attributes, layouts, properties with names
instead of DXF codes:

```lisp
(vl-load-com)
(setq obj (vlax-ename->vla-object (car (entsel))))
(vla-get-Layer obj)
(vlax-property-available-p obj 'EffectiveName)        ; dynamic blocks
(foreach att (vlax-invoke obj 'GetAttributes)
  (princ (strcat "\n" (vla-get-TagString att) " = " (vla-get-TextString att))))
```

## The LISP files in this repo

**`code/plant3d/autolisp/att_tools.lsp`**

| Command / function | Does |
|---|---|
| `ATTLIST` | every attribute → text window + `<dwg>_att.csv` |
| `ATTSET` | block, tag, value prompts → set on all inserts (dynamic blocks by effective name) |
| `ATTRENUM` | renumber a tag by position (left→right, top→bottom), e.g. valve labels `V-101, V-102…` |
| `(att:set-all "TITLEBLOCK" "REV" "B")` | the same as a function → callable from `.scr` scripts and Core Console |

**`code/plant3d/autolisp/plant_helpers.lsp`**

| Command | Does |
|---|---|
| `P3DPROPS` | generic info of a picked object; points to the .NET `PIDPICK` for Plant properties |
| `P3DLAYERS` | freeze/thaw a list of Plant layers (edit the list to your standard) |
| `P3DPURGE` | purge + audit before issue |

Honest limit: Plant 3D exposes very little *Plant data* to LISP. Anything touching Plant properties in
LISP would rely on undocumented functions — the file marks this **VERIFY** and routes you to .NET.

Checked by `tools/lisp_check.py`: parentheses/strings/comments balanced, no duplicate commands, every
file ends with `(princ)`.

## Editing and debugging LISP: VS Code

- Install **AutoCAD AutoLISP Extension** in VS Code (Autodesk's official extension; Autodesk's Plant 3D automation article recommends it).
- `LISPSYS` = 1 in AutoCAD (Unicode LISP, VS Code as editor); `VLISP` command opens VS Code.
- Debug: VS Code *Run → Start Debugging* → *AutoLISP: Attach* to `acad.exe` → breakpoints in `.lsp`.
- Old **VLIDE** still exists with `LISPSYS` = 0 (ANSI). Prefer VS Code.

## Script files (.scr)

A script = keystrokes typed at the command line, one per line:

```text
; titleblock_rev.scr
(load "C:/CAD-Automation/code/plant3d/autolisp/att_tools.lsp")
(att:set-all "TITLEBLOCK" "REV" "B")
(att:set-all "TITLEBLOCK" "DATE" (menucmd "M=$(edtime,$(getvar,date),YYYY-MO-DD)"))
_.QSAVE
```

Rules:
- **Every space and blank line = ENTER.** Trailing spaces break scripts. Keep files tidy; end with a newline.
- Use `_.COMMAND` (international + not redefined).
- Dialog commands do not work → use the `-` command-line versions (`-LAYER`, `-PLOT`, `-EXPORT`, `-PURGE`).
- Set `FILEDIA` 0 if a command would open a file dialog; restore 1 at the end.
- Paths with spaces in "quotes"; forward slashes in LISP strings (or `\\`).
- Run: `SCRIPT` command, drag-drop the `.scr`, **ScriptPro** (Autodesk's batch utility — download separately), or Core Console `/s`.

`export_pdf_all_layouts.scr` uses `-EXPORT` → PDF → all layouts; prompt order varies by release —
VERIFY by typing the command once manually and copying your answers.

## Core Console

`accoreconsole.exe` (in `C:\Program Files\Autodesk\AutoCAD 2026\`) = AutoCAD **without the UI**:
starts in ~1–2 s, runs a script on one DWG, exits. Ideal for batch jobs on hundreds of drawings.

```bat
"C:\Program Files\Autodesk\AutoCAD 2026\accoreconsole.exe" /i "D:\P\Ortho\ORTHO-001.dwg" /s "D:\scripts\titleblock_rev.scr" /l en-US
```

| Switch | Meaning |
|---|---|
| `/i <dwg>` | input drawing |
| `/s <scr>` | script to run |
| `/l en-US` | language pack |
| `/product PLNT3D` | start with the Plant 3D product profile — VERIFY on your install (`accoreconsole /?`) |
| `/isolate <user> <folder>` | separate profile/settings (VERIFY) |

What runs in Core Console:
- ✔ core commands, LISP (no dialogs, no `vla-` UI objects), **.NET DLLs that reference only `accoremgd`/`acdbmgd`** (`NETLOAD` in the script) → our `AttributeTools.dll` works.
- ✘ ribbon, dialogs, `acmgd`-dependent plug-ins, most PDF publishing via UI.
- **Plant 3D:** the object enablers (`PnP3dObjects.dbx`) can be loaded so Plant objects are not proxies, but the Plant environment ARX apps don't load → **no Plant commands** (Autodesk developer blog, *Can I Run AutoCAD P&ID/Plant 3D in Console Mode?*). P&ID has no usable enabler there. So Core Console is for **DWG-level** work on Plant drawings (title blocks, layers, purge), not Plant data.

Scripts in this repo:
- `batch_coreconsole.bat` — simple loop, one log file.
- `Invoke-CoreConsole.ps1` — PowerShell, **parallel** jobs (`-Parallel 4`), CSV log of exit code and seconds per file:
  ```powershell
  pwsh -File code\plant3d\scripts\Invoke-CoreConsole.ps1 -Folder D:\P\Ortho -Script .\titleblock_rev.scr -Parallel 4
  ```
  Parallel = one process per DWG; 4 parallel on an 8-core PC is a sensible start. Watch RAM.

Using a .NET command in a Core Console script:

```text
_.NETLOAD "C:/Tools/AttributeTools/AttributeTools.dll"
ATTREPLACE
TITLEBLOCK
REV
C
_.QSAVE
```

(`ATTREPLACE` prompts for block, tag, value with `AllowSpaces = true` — so a space does **not** end the
answer; each answer must be on its own line, ENTER = newline.)

## Batch patterns compared

| Pattern | Speed | Robustness | Plant data | Effort |
|---|---|---|---|---|
| ScriptPro / `SCRIPT` with full AutoCAD | slow (UI) | medium | yes (Plant 3D running) | low |
| **Core Console + .scr (+ LISP/.NET)** | fast, parallel | high (one process per file) | no | low |
| .NET command looping side databases in one session | fast | medium (one crash stops all) | partly | medium |
| .NET command in Plant 3D on the **project database** | fastest for data | high | **yes** | medium |
| Python COM driving AutoCAD | slow | low-medium | no | low |

## Python for AutoCAD and Plant 3D

Three different "Pythons":

| Python | Runs | Use |
|---|---|---|
| **pyautocad / pywin32** | your Anaconda, out of process via COM/ActiveX | attributes, layers, text on the **active** drawing |
| **openpyxl / pandas** | your Anaconda, no AutoCAD | turn CSVs from .NET/LISP into reports |
| **Plant 3D custom part scripts** | Plant 3D's embedded engine | catalogue geometry ([docs/11](11-plant3d-sdk.md#custom-part-scripts-python-inside-plant-3d)) |

```python
from pyautocad import Autocad
acad = Autocad(create_if_not_exists=False)          # attach to running AutoCAD / Plant 3D
for blk in acad.iter_objects("BlockReference"):
    if blk.HasAttributes:
        for att in blk.GetAttributes():
            print(att.TagString, att.TextString)
```

- `code/plant3d/python/pyautocad_attributes.py` — `export out.csv`, `set TITLEBLOCK REV B`.
- `code/plant3d/python/csv_to_excel_report.py` — merge CSVs (line list, MTO, tags, attributes) into one workbook: a sheet per CSV, bold frozen header, auto-filter, column widths.
- COM is slow (each property = cross-process call): fine for hundreds of blocks, not tens of thousands.
- Plant objects via COM appear as generic/custom entities; **no Plant properties** → use .NET for data.

## Security: SECURELOAD and TRUSTEDPATHS

| Setting | Meaning | Recommendation |
|---|---|---|
| `SECURELOAD` = 1 (default) | warn when loading code from untrusted folders | keep |
| `SECURELOAD` = 2 | load only from trusted folders | good for locked-down companies |
| `SECURELOAD` = 0 | load anything | **never** company-wide |
| `TRUSTEDPATHS` | `;`-separated list of trusted folders (`...\...` suffix = include sub-folders) | add your tools folder, read-only for users |
| ApplicationPlugins folders | trusted by default | deploy as `.bundle` |

```lisp
(setvar "TRUSTEDPATHS" (strcat (getvar "TRUSTEDPATHS") ";C:\\CAD-Automation\\code\\plant3d\\autolisp"))
```

`acad.lsp` / `acaddoc.lsp` in random folders are a classic malware vector — AutoCAD's security
features exist for this reason. Keep startup code in **one** controlled, read-only location.

Next: [13 — errors and fixes](13-errors-and-fixes.md).
