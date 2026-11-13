# Plant3DAddinStarter — a complete, read-only Plant 3D add-in

The working project behind [11b — first add-in](../../../../docs/11b-plant3d-first-addin.md) and
[11c — code examples](../../../../docs/11c-plant3d-code-examples.md). Nothing in it changes a drawing or a project.

| Command | Does | Explained in |
|---|---|---|
| `P3DHELLO` | application, registry key, .NET runtime, active document, project, project parts, loaded Plant 3D DLLs and their folders | 11c, Examples 1–2 |
| `P3DCOMPONENTS` | every Plant object in model space → counts per project class + CSV with all properties | 11c, Examples 3, 4, 9 |
| `P3DINSPECT` | pick one object → .NET class chain, ObjectARX class, handle, project class path, row id, line group, all properties, ports | 11c, Examples 4–6 |
| `P3DWALK` | pick a 3D part → every object connected to it (max 500) | 11c, Example 7 |
| `P3DAPIMAP` | class hierarchy of the Plant 3D API on **your** install → Markdown; optional member list of one class | 11c, Example 10 |
| `P3DREGISTER` | write the demand-loading registry key for this DLL (current user, running product) | 11b, step 8 |
| `P3DUNREGISTER` | remove that key | 11b, step 8 |

Output files: `Documents\Plant3DAddinStarter\`.

## Files

| File | Role |
|---|---|
| `PlantAddin.cs` | `IExtensionApplication`: safe `Initialize`, first-idle message, `AssemblyResolve` safety net |
| `PlantContext.cs` | application → active document → project → project part → `DataLinksManager` → `PnPDatabase` |
| `ComponentReader.cs` | the model-space loop, ports, connections |
| `*Commands.cs` | the commands; only talk to the user |
| `Report.cs` | safe command wrapper, output folder, CSV (no Plant types) |
| `bundle/PackageContents.xml` | Autoloader manifest, `Platform="Plant3D"`, 2025–2026 |
| `Properties/launchSettings.json` | F5 starts `acad.exe /product PLNT3D /language en-US` |

## Build

```powershell
# real build: Plant 3D 2026 on this PC; PlantSdkInc = folder with the PnP*.dll files (see docs/11b, step 3)
dotnet build -c Release -p:PlantSdkInc="C:\Autodesk\Plant3D 2026 SDK\inc-x64"

# same, and copy the bundle to %APPDATA%\Autodesk\ApplicationPlugins\Plant3DAddinStarter.bundle
dotnet build -c Release -p:PlantSdkInc="..." -p:DeployBundle=true

# syntax check anywhere (no Autodesk software): compiles against tools/plant-sdk-stubs
dotnet build -p:PlantStubs=true
```

Plant 3D 2027: add `-p:AcadRelease=2027` (`net10.0-windows`) and widen `SeriesMax` in the bundle to `R26.0`.

## Load

| Way | Do |
|---|---|
| once | `NETLOAD` → `bin\Release\net8.0-windows\Plant3DAddinStarter.dll` |
| every start, recommended | build with `-p:DeployBundle=true`, restart Plant 3D |
| every start, registry | `NETLOAD` once, then `P3DREGISTER` |

Chapter 18's greeting exercise also calls its command `P3DHELLO`: if you added it to Plant3DDataTools, do not
load both DLLs in one session (two commands with one name).

Use one automatic way, not both. Details and troubleshooting: [docs/11b](../../../../docs/11b-plant3d-first-addin.md).

## Honesty

- Compiled here against a stand-in for the AutoCAD API and the hand-written Plant SDK stubs
  (`tools/plant-sdk-stubs`). That proves C# syntax and our reading of the public samples — **not** the real
  Autodesk signatures, and nothing was run inside Plant 3D.
- First thing on a Plant 3D PC: build against the real DLLs, run `P3DHELLO` and `P3DAPIMAP`, and settle the
  **VERIFY** items in docs/11a–11c.
- Run it on a **copy** of a project first.
