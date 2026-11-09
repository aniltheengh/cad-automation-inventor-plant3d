# AttributeTools — AutoCAD .NET plug-in (works in Plant 3D, Core Console)

| Command | Does |
|---|---|
| `ATTEXPORT` | every attribute in the drawing → `<dwg>_attributes.csv` (Handle, Block, Tag, Value, Layout), UTF-8 with BOM so Excel shows Japanese correctly |
| `ATTIMPORT` | CSV (Handle, Tag, NewValue) → updates attributes by handle |
| `ATTREPLACE` | Block, Tag, Value prompts → set on every insert |

## Build & load
```
dotnet build -c Release            (or Visual Studio)
AutoCAD / Plant 3D: NETLOAD → bin\Release\net8.0-windows\AttributeTools.dll → ATTEXPORT
```
- References come from NuGet **`AutoCAD.NET.Core` 25.1.0** (= AutoCAD 2026 API, net8.0) with `ExcludeAssets="runtime"`.
  25.1.1 targets net10 only (for AutoCAD 2026.1.2+) — keep 25.1.0 if you build with the .NET 8/9 SDK.
- Only `accoremgd`/`acdbmgd` APIs → also loads in `accoreconsole.exe` for batch jobs (no `acmgd`, no UI).
- The folder must be trusted when `SECURELOAD = 1` (default): add it to `TRUSTEDPATHS` or load from a `.bundle` in an ApplicationPlugins folder.
