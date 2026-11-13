# Plant3DDataTools — Plant 3D SDK (.NET) commands

| Command | Part | Does |
|---|---|---|
| `P3DTABLES` | Piping | list PnPDatabase tables and row counts → command line |
| `P3DDUMP` | Piping | enter table name → all table rows/columns to a timestamped CSV in the project folder |
| `P3DLINELIST` | Piping | `P3dLineGroup` rows and related part counts → timestamped LineList CSV in the project folder |
| `P3DMTO` | Piping | project component rows grouped by available description/size/line fields → timestamped CSV (piece counts, not pipe length) |
| `P3DSETPROPS` | Piping | read CSV (`PnPID,Property,Value`) → bulk-update linked row properties via `SetProperties`; no preview |
| `PIDPICK` | P&ID | pick linked object → class + properties; tries PnId then Piping, with limited error handling |
| `PIDTAGS` | P&ID | linked P&ID model-space objects, deduplicated by row → `<drawing>_tags.csv` (does not filter out empty tags) |

## Build
- **Real build (Windows, Plant 3D 2026 + SDK):** download the *Plant 3D 2026 SDK* (Autodesk Platform Services → Plant 3D / ADN), set `PlantSdkInc` in `code/plant3d/Directory.Build.props`, `dotnet build`.
- **Syntax check anywhere:** `dotnet build -p:PlantStubs=true` compiles against `tools/plant-sdk-stubs` — hand-written stubs that contain only members seen in Autodesk's public Plant 3D training labs. Passing proves C# syntax and our use of *those* members; it does **not** prove the real SDK signatures. Build once against the real SDK before trusting it.
- All Plant DLLs: `Private=false` (Copy Local off). They are loaded by Plant 3D.
- VERIFY: the `PnPDataObjects` reference name; if your SDK names it differently, the compiler tells you which type is missing.

## Load
`NETLOAD` inside **Plant 3D** with a project open (the commands need `PlantApplication.CurrentProject`). In
plain AutoCAD they fail at `CurrentProject` — by design.

See [docs/11-plant3d-sdk.md](../../../../docs/11-plant3d-sdk.md).

## Learn add-in development

Follow [Chapter 18 — Plant 3D add-in development with C#](../../../../docs/18-plant3d-addin-development.md) for a separate step-by-step path through setup, a first command, lifecycle hooks, debugging, Plant project data and deployment. Match references and the target framework to your installed product/update before building.

## Beginner — understand before running
Start with [the from-zero workbook](../../../../docs/19-addin-learning-from-zero.md). Find the project file, a command attribute and its method body. Build with real references and load the DLL in the matching Plant host. Complete the greeting exercise from chapter 18 before project-wide commands.

## Transition — trace read-only behavior
Study PIDPICK for selection/property reading and P3DDUMP for table export. They do different jobs. Match the project part and compare results with Data Manager. The samples do not fully guard every missing-project or data-link exception; add those guards as an exercise. Check the command's scope before running it.

## Intermediate — reporting before editing
Reconcile a small export, define failure messages and validate inputs. P3DSETPROPS is a bulk writer using simple comma splitting: it has no preview and is not a general quoted-CSV importer. Improve those behaviors before using it beyond a disposable test project. P3DMTO counts pieces rather than pipe lengths.

These are learning stages and proposed improvements, not claims that the sample code already implements every check in the workbook.
