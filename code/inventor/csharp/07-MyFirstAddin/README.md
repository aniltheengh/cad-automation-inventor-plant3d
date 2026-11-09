# 07-MyFirstAddin — a registry-free Inventor add-in with a ribbon (C#)

Adds a **CAD Automation** tab to the Part, Assembly and Drawing ribbons with three buttons:

| Button | Ribbon | Does |
|---|---|---|
| Export PDF | Drawing | PDF of all sheets next to the drawing (re-uses `03-BatchExport/Exporter.cs`) |
| Part Info | Part, Assembly | mass, volume, area, bounding box, material in a message box |
| Parameters | Part, Assembly | grid editor for user parameters, one Undo step (`Transaction`) |

## Build, deploy, debug (Visual Studio 2022 17.8+)
1. Open `MyFirstAddin.csproj`. Check `InventorBin` in `code/inventor/Directory.Build.props`.
2. **Build** → the `DeployToInventor` target copies `MyFirstAddin.dll` + `MyFirstAddin.addin` to
   `%APPDATA%\Autodesk\ApplicationPlugins\MyFirstAddin\`.
3. **F5** uses `Properties/launchSettings.json` → starts `Inventor.exe`; breakpoints in `Activate` and the commands hit.
4. Close Inventor before rebuilding (the DLL is locked while loaded).

## Files
| File | Purpose |
|---|---|
| `MyFirstAddin.addin` | manifest: ClassId = `[Guid]` of `StandardAddInServer`, relative `<Assembly>`, `SupportedSoftwareVersionGreaterThan 28..` |
| `StandardAddInServer.cs` | `Activate` / `Deactivate`, ribbon tab + panel, rebuild on *Reset Ribbon* |
| `Commands/AddinCommand.cs` | base class: `ButtonDefinition`, icons drawn at runtime → `IPictureDisp` |
| `Commands/*.cs` | the three commands |

## Not loading? (in order)
1. *Tools → Add-ins*: listed? If not, the `.addin` is not in a scanned folder or the XML is invalid.
2. Listed but not loaded → select it, read the message; tick *Loaded/Unloaded*.
3. `Could not load file or assembly` → wrong `<Assembly>` path, or DLL blocked (*Properties → Unblock*) after download.
4. Loads in 2026 but not 2024 → expected: 2024 is .NET Framework 4.8, this DLL is .NET 8.

Details: [docs/06-inventor-addins.md](../../../../docs/06-inventor-addins.md).
