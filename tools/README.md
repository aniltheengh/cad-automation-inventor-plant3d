# tools/ — how this repo checks itself without Inventor or AutoCAD

Neither Inventor nor AutoCAD runs on Linux CI, and you cannot unit-test a COM call without the product.
So the repo checks what *can* be checked and says clearly what was not.

| Tool | Checks | Needs |
|---|---|---|
| `build_all.sh` / `build_all.ps1` | builds every C#/VB.NET project, then runs all checks below | .NET 8+ SDK; Inventor interop DLL path |
| `check_ilogic.py` + `ilogic-check/` | each `.iLogicVb` rule compiles as VB.NET against the **real** Inventor interop + iLogic **stubs**; 2nd pass Option Strict On lists late-bound calls | .NET SDK, interop DLL |
| `plant-sdk-stubs/` | stub assembly so `Plant3DDataTools` compiles without the Plant SDK (`-p:PlantStubs=true`) | .NET SDK |
| `test_tools.py` | gear math vs. textbook values; C# `GearMath` == Python `gear_math` (1e-12); tooth-space sketch validity; LISP checker; add-in GUID match | Python 3.10+, dotnet optional |
| `gearmath-check/` | tiny console that runs the C# gear math and prints JSON (used by `test_tools.py`) | .NET SDK |
| `lisp_check.py` | AutoLISP parens/strings/comments, duplicate `c:` commands, final `(princ)` | Python |
| `check_powershell.ps1` | every `.ps1` parses with the real PowerShell parser | PowerShell 7 |
| `check_links.py` | internal Markdown links + `#anchors`; `--external` also HTTP-checks URLs | Python |

```bash
# Linux / macOS / CI
INVENTOR_INTEROP_DIR=/path/to/dir/with/Autodesk.Inventor.Interop.dll ./tools/build_all.sh
```
```powershell
# Windows with Inventor 2026
pwsh -File tools\build_all.ps1
```

## What "passed" means — and does not
- **Compiles against the interop** = every Inventor type/member name and signature used exists. It does not
  prove the call succeeds at run time (wrong document type, missing feature, locked file...).
- The interop used on the build box was from **Inventor 2023** (v27). Members used here exist since long before
  2023; anything newer would have failed. Build against your 2026 interop once — that is the real test.
- **iLogic stubs** and **Plant SDK stubs** are hand-written from public samples. They prove syntax and *our*
  understanding of signatures, not Autodesk's. Marked as such in every README.
