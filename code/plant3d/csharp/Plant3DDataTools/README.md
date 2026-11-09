# Plant3DDataTools — Plant 3D SDK (.NET) commands

| Command | Part | Does |
|---|---|---|
| `P3DTABLES` | Piping | list PnPDatabase tables and row counts → command line |
| `P3DDUMP` | Piping | pick object → all Plant properties (`DataLinksManager.GetAllProperties`) |
| `P3DLINELIST` | Piping | `P3dLineGroup` rows → `linelist.csv` (line number, size, spec, service, ...) |
| `P3DMTO` | Piping | parts related to each line group → `mto.csv` (simple MTO) |
| `P3DSETPROPS` | Piping | pick objects → set a property (e.g. `Status=IFC`) via `SetProperties` |
| `PIDPICK` | P&ID | pick symbol → class + properties |
| `PIDTAGS` | P&ID | all tagged P&ID objects → `pid_tags.csv` |

## Build
- **Real build (Windows, Plant 3D 2026 + SDK):** download the *Plant 3D 2026 SDK* (Autodesk Platform Services → Plant 3D / ADN), set `PlantSdkInc` in `code/plant3d/Directory.Build.props`, `dotnet build`.
- **Syntax check anywhere:** `dotnet build -p:PlantStubs=true` compiles against `tools/plant-sdk-stubs` — hand-written stubs that contain only members seen in Autodesk's public Plant 3D training labs. Passing proves C# syntax and our use of *those* members; it does **not** prove the real SDK signatures. Build once against the real SDK before trusting it.
- All Plant DLLs: `Private=false` (Copy Local off). They are loaded by Plant 3D.
- VERIFY: the `PnPDataObjects` reference name; if your SDK names it differently, the compiler tells you which type is missing.

## Load
`NETLOAD` inside **Plant 3D** with a project open (the commands need `PlantApplication.CurrentProject`). In
plain AutoCAD they fail at `CurrentProject` — by design.

See [docs/11-plant3d-sdk.md](../../../../docs/11-plant3d-sdk.md).
