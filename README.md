# CAD Automation for Inventor and Plant 3D — in many languages

A practical, engineer-to-engineer guide with **working code** for automating **Autodesk Inventor 2026**
(iLogic, VBA, C#, VB.NET, Python, PowerShell, Apprentice, Design Automation) and **AutoCAD Plant 3D 2026**
(AutoCAD .NET, Plant SDK, AutoLISP, scripts, Core Console, pyautocad, custom part scripts).

Written for a mechanical engineer who is new to C# and Visual Studio. Bullets, tables and code, not essays.
Companion to *pressure-vessel-fea-inventor-nastran*.

> **Target setup:** Inventor 2026 · Visual Studio 2022 Community 17.14 (.NET desktop workload) · .NET SDK 9
> (builds .NET 8 targets) · Anaconda Python · Plant 3D via trial/education/company licence ([how](docs/00-getting-started.md#getting-autocad-plant-3d)).

## What is inside

| | Count |
|---|---|
| Docs | 19 chapters (`docs/00`–`18`) |
| Projects | 13 complete projects ([docs/15](docs/15-projects.md)) |
| C# / VB.NET projects | 12 (8 Inventor C#, 1 VB.NET, 1 Design Automation, 1 AutoCAD, 1 Plant 3D) |
| iLogic rules | 10 |
| Python scripts | 10 (pywin32, pyautocad, openpyxl, Plant 3D custom part) |
| Other | 3 VBA modules, 3 PowerShell scripts (+2 in tools), 2 AutoLISP files, 2 `.scr` + `.bat` |
| Checks | compile, iLogic two-pass, unit tests (C# ⇄ Python), LISP, PowerShell parse, links ([tools/](tools/README.md)) |

## Which language for which job?

| Job | Best choice | Also good | Chapter |
|---|---|---|---|
| Rules inside a template part/assembly (configurator) | **iLogic** | add-in | [03](docs/03-ilogic.md) |
| Explore the API quickly | **VBA** (Object Browser, Watch) | C# debugger | [04](docs/04-vba.md) |
| Batch export / batch iProperties / integrations | **C# EXE** | Python, iLogic external rule | [05](docs/05-csharp-vbnet-exe.md), [09](docs/09-drawings-bom-export.md) |
| Ribbon button / events / team tool | **C# add-in** | iLogic global form | [06](docs/06-inventor-addins.md) |
| Engineering calc + Excel/pandas driving Inventor | **Python (pywin32)** | C# EXE | [07](docs/07-python-powershell.md) |
| Admin script on a locked-down PC | **PowerShell** | Python | [07](docs/07-python-powershell.md#powershell) |
| iProperties of 10 000 files, no Inventor seat | **Apprentice** | — | [08](docs/08-apprentice-design-automation-vault.md#apprentice-server) |
| Web configurator, no Inventor seats | **Design Automation (APS)** | — | [08](docs/08-apprentice-design-automation-vault.md#design-automation-for-inventor-aps) |
| Plant 3D properties, line list, MTO, P&ID tags | **Plant SDK (.NET)** | Report Creator (no code) | [11](docs/11-plant3d-sdk.md) |
| Title blocks, layers, attributes on DWGs | **AutoLISP / AutoCAD .NET** | pyautocad | [10](docs/10-autocad-dotnet-basics.md), [12](docs/12-autolisp-scripts-coreconsole-python.md) |
| Same job on 500 DWGs unattended | **Core Console + .scr** | ScriptPro | [12](docs/12-autolisp-scripts-coreconsole-python.md#core-console) |
| New Plant 3D catalogue part geometry | **Python custom part script** | blocks | [11](docs/11-plant3d-sdk.md#custom-part-scripts-python-inside-plant-3d) |
| Visual programming for Plant 3D | **Dynamo is not available** | .NET + Python | [11](docs/11-plant3d-sdk.md#dynamo-for-plant-3d-not-available) |

Full comparison and decision tree: [docs/01](docs/01-choosing-a-language.md).

## Plant 3D add-in development — C# and Plant 3D API

A separate track for building add-ins that run inside Plant 3D:

- **[Chapter 18: Plant 3D add-in development with C#](docs/18-plant3d-addin-development.md)** — environment and SDK versions, first command, lifecycle, loading/debugging, project data, UI, deployment and verification.
- **[Plant3DDataTools examples](code/plant3d/csharp/Plant3DDataTools/README.md)** — inspect properties, export line lists/MTO, work with P&ID tags and explore property updates.
- **[Plant SDK reference](docs/11-plant3d-sdk.md)** — project parts, DataLinksManager, database rows and relationships.

Start with a read-only command in a disposable project. Match the SDK and .NET target to the installed Plant 3D build; the new chapter explains the runtime/update distinction.

## Learning path (beginner)

| Step | Do | Read | Code |
|---|---|---|---|
| 1 | Run your first iLogic rule (no Visual Studio) | 00, 02, 03 | `ilogic/03_Shaft_Generator` |
| 2 | Build a configurator template | 03 | `ilogic/02_Tank_Configurator`, `06_Excel_Size_Table` |
| 3 | First C# EXE + the debugger | 05 | `csharp/01-HelloInventor`, `02-SpurGearGenerator` |
| 4 | Batch export and Excel round trips | 09, 07 | `03-BatchExport`, `04-IPropertyBulkEditor`, `python/` |
| 5 | Your own ribbon button | 06 | `07-MyFirstAddin` |
| 6 | AutoCAD .NET + LISP + Core Console (plain AutoCAD is enough) | 10, 12 | `autocad/AttributeTools`, `plant3d/autolisp` |
| 7 | Plant 3D trial: SDK, line list, MTO, custom part | 11 | `plant3d/csharp/Plant3DDataTools`, `custom-part-scripts` |
| 8 | Cloud: Design Automation | 08 | `design-automation/GearDAPlugin` |
| always | When stuck | 13 | — |
| before sharing | Version control, tests, deployment | 14 | `tools/` |

## Table of contents

| # | Chapter |
|---|---|
| 00 | [Getting started: install, versions, first build, getting Plant 3D](docs/00-getting-started.md) |
| 01 | [Choosing a language and hosting model](docs/01-choosing-a-language.md) |
| 02 | [Inventor API object model](docs/02-inventor-api-object-model.md) |
| 03 | [iLogic: rules, forms, external rules, configurators](docs/03-ilogic.md) |
| 04 | [VBA](docs/04-vba.md) |
| 05 | [C# and VB.NET EXEs](docs/05-csharp-vbnet-exe.md) |
| 06 | [Add-ins: manifest, ribbon, deployment, debugging](docs/06-inventor-addins.md) |
| 07 | [Python (pywin32) and PowerShell](docs/07-python-powershell.md) |
| 08 | [Apprentice, Design Automation, Vault](docs/08-apprentice-design-automation-vault.md) |
| 09 | [Drawings, BOM, PDF/DXF/STEP export, Content Center, iParts, Excel](docs/09-drawings-bom-export.md) |
| 10 | [AutoCAD .NET basics](docs/10-autocad-dotnet-basics.md) |
| 11 | [Plant 3D: SDK, DataLinks, PnPDatabase, piping, P&ID, specs, Isogen, custom parts](docs/11-plant3d-sdk.md) |
| 12 | [AutoLISP, scripts, Core Console, pyautocad](docs/12-autolisp-scripts-coreconsole-python.md) |
| 13 | [Errors and fixes](docs/13-errors-and-fixes.md) |
| 14 | [Best practices: Git, testing, deployment, bundles, performance](docs/14-best-practices.md) |
| 15 | [Projects (13) with exercises](docs/15-projects.md) |
| 16 | [Glossary (English / 日本語)](docs/16-glossary.md) |
| 17 | [References](docs/17-references.md) |
| 18 | [Plant 3D add-in development: C#, Plant API, commands, debugging and deployment](docs/18-plant3d-addin-development.md) |

Code map: [code/README.md](code/README.md) · Checks: [tools/README.md](tools/README.md)

## Quick start

```powershell
git clone https://github.com/aniltheengh/cad-automation-inventor-plant3d.git C:\CAD-Automation
cd C:\CAD-Automation
# 1) check InventorBin in code\inventor\Directory.Build.props (default: Inventor 2026)
# 2) build + check everything
pwsh -File tools\build_all.ps1          # Windows PowerShell 5.1 works too: powershell -File ...
# 3) open a part in Inventor, then:
dotnet run --project code\inventor\csharp\01-HelloInventor
```

## Honesty notes

- Code was **compiled** against the real Inventor interop and the real AutoCAD NuGet packages, but **not
  executed** against Inventor/AutoCAD (the build machine has neither). Treat first runs as tests; use test copies of files.
- The interop on the build machine was the Inventor **2023** DLL; every member used exists there. Build
  once against your 2026 interop.
- Plant 3D SDK code compiles against **stubs** written from Autodesk's public samples. Build against the
  real Plant 3D 2026 SDK before use.
- Items marked **VERIFY** in the docs/code are not confirmed from a primary source. They are collected in
  [docs/13](docs/13-errors-and-fixes.md) and the per-project READMEs.
- Engineering formulas (ASME, gears) are textbook forms for sizing and learning — not a substitute for the code of construction or a design review.

## Licence

MIT for the code in this repository. Autodesk product names are trademarks of Autodesk, Inc. No Autodesk
binaries are included.
