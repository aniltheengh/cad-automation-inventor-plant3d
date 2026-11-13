# code/ — every sample, by product and language

All code here was compile-checked (C#, VB.NET, iLogic via stubs), byte-compiled (Python), parsed
(PowerShell) or paren-checked (AutoLISP). See [../tools/README.md](../tools/README.md) for how, and
[../docs/15-projects.md](../docs/15-projects.md) for the guided project walk-through.

```
code/
├── inventor/
│   ├── Directory.Build.props        Inventor version + interop path for ALL .NET projects below
│   ├── csharp/
│   │   ├── Shared/                  InventorConnector.cs (.NET 8 GetActiveObject), IProps.cs
│   │   ├── 01-HelloInventor/        connect, list documents, parameters, iProperties
│   │   ├── 02-SpurGearGenerator/    involute spur gear part from m, z, alpha (pure math + API)
│   │   ├── 03-BatchExport/          folder -> PDF / DXF / STEP / flat-pattern DXF + CSV log
│   │   ├── 04-IPropertyBulkEditor/  iProperties <-> Excel round trip (ClosedXML)
│   │   ├── 05-BomToExcel/           structured / parts-only BOM -> formatted xlsx
│   │   ├── 06-FrameConfigurator/    WinForms UI -> square-tube frame parts + assembly
│   │   ├── 07-MyFirstAddin/         registry-free add-in: ribbon tab, 3 commands, F5 debugging
│   │   └── 08-ApprenticeReader/     read/write iProperties + file tree without Inventor
│   ├── vbnet/BatchExportVB/         the PDF exporter in VB.NET (same API, different syntax)
│   ├── ilogic/                      10 rules: configurators, generators, export, drawings, BOM, events
│   ├── vba/                         3 modules for the built-in VBA editor (legacy but handy)
│   ├── python/                      pywin32 scripts: params, export, iProps<->Excel, BOM, gear driver
│   ├── powershell/                  COM from PowerShell 5.1 / 7
│   └── design-automation/GearDAPlugin/  cloud (APS Design Automation) gear plug-in + bundle + JSON
├── autocad/
│   ├── Directory.Build.props        AutoCAD NuGet version pin
│   └── csharp/AttributeTools/       ATTEXPORT / ATTIMPORT / ATTREPLACE (Core-Console safe)
└── plant3d/
    ├── Directory.Build.props        Plant SDK path + AutoCAD NuGet pin
    ├── csharp/Plant3DAddinStarter/  first add-in: IExtensionApplication, bundle, registry; components + properties
    ├── csharp/Plant3DDataTools/     Plant SDK: tables, line list, MTO, set props, P&ID tags
    ├── autolisp/                    attribute tools + small Plant helpers
    ├── scripts/                     .scr scripts, Core Console batch (.bat + PowerShell parallel)
    ├── python/                      pyautocad attributes, CSV -> Excel report
    └── custom-part-scripts/         Plant 3D Python custom part (nozzle stub) template
```

## Build everything (Windows, Inventor 2026 installed)

```powershell
cd C:\CAD-Automation
pwsh -File tools\build_all.ps1                       # Inventor + AutoCAD; Plant uses stubs
pwsh -File tools\build_all.ps1 -PlantSdkInc "C:\Autodesk\Plant3D 2026 SDK\inc-x64"   # real Plant SDK
```

Or open any `.csproj` in Visual Studio 2022 (17.8 or newer) and press **Ctrl+Shift+B**.

## Rules used in every project

| Rule | Why |
|---|---|
| `net8.0-windows`, x64 | Inventor 2025–2027 and AutoCAD 2025–2027 load .NET 8 assemblies ([docs/00](../docs/00-getting-started.md#net-versions-in-one-table)) |
| Interop referenced from the Inventor `Bin` folder, `EmbedInteropTypes=false`, never committed | licence + version match; one path in `Directory.Build.props` |
| AutoCAD refs from NuGet with `ExcludeAssets="runtime"` | never copy acmgd/acdbmgd into the output |
| Inventor API lengths in **cm**, angles in **radians** | convert once at the boundary (`MM = 0.1`) |
| Always `try/finally` close documents in batch tools | otherwise memory grows and Inventor slows down |
| No passwords, licence files or customer data in the repo | see [docs/14](../docs/14-best-practices.md) |
