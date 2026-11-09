# 15 — Projects: 13 complete projects in 8 languages

Each project is complete code in `code/`, with its own README. Here: what you learn, in which order,
and **exercises** to extend it (the real learning happens in the exercises).

| # | Project | Product | Languages | Level |
|---|---|---|---|---|
| P1 | [Spur gear generator](#p1-spur-gear-generator) | Inventor | C#, Python, iLogic | ★★ |
| P2 | [Batch export PDF/DXF/STEP/flat](#p2-batch-export) | Inventor | C#, VB.NET, iLogic, Python, PowerShell, VBA | ★ |
| P3 | [iProperty bulk editor](#p3-iproperty-bulk-editor) | Inventor | C#, Python, C# Apprentice | ★ |
| P4 | [BOM to Excel](#p4-bom-to-excel) | Inventor | C#, Python, iLogic | ★ |
| P5 | [Frame configurator](#p5-frame-configurator) | Inventor | C# WinForms, iLogic | ★★ |
| P6 | [Tank configurator (ASME check)](#p6-tank-configurator) | Inventor | iLogic | ★★ |
| P7 | [My first add-in](#p7-my-first-add-in) | Inventor | C# | ★★ |
| P8 | [Shaft generator + auto drawing](#p8-shaft-generator-and-auto-drawing) | Inventor | iLogic | ★★ |
| P9 | [Excel size table (flanges)](#p9-excel-size-table) | Inventor | iLogic | ★ |
| P10 | [Gear on the cloud](#p10-gear-on-the-cloud) | Inventor DA | C#, JSON | ★★★ |
| P11 | [Attribute tools](#p11-attribute-tools) | AutoCAD / Plant 3D | C#, AutoLISP, .scr, PowerShell, Python | ★ |
| P12 | [Plant 3D data tools: line list, MTO, tags](#p12-plant-3d-data-tools) | Plant 3D | C# (Plant SDK), Python | ★★★ |
| P13 | [Plant 3D custom part script](#p13-plant-3d-custom-part-script) | Plant 3D | Python (embedded) | ★★ |

★ = first week, ★★ = after the basics, ★★★ = needs SDK/cloud setup.

## P1 Spur gear generator

**Files:** `code/inventor/csharp/02-SpurGearGenerator/` (GearMath.cs, Program.cs),
`code/inventor/python/gear_math.py`, `drive_gear_template.py`, `code/inventor/ilogic/01_SpurGear_Params.iLogicVb`.

**Engineering:** involute geometry — pitch radius r = m·z/2, base radius r_b = r·cos α, tip r_a = r + m,
root r_f = r − 1.25 m; tooth thickness at radius r_y: s_y = 2 r_y (s/(2r) + inv α − inv α_y), inv α = tan α − α;
undercut limit z_min = 2/sin² α (≈ 17.1 for 20°).

**API:** user parameters, sketch splines through points, extrude cut through all, circular pattern with
an expression count, iProperties, transactions.

**Steps:** run `python gear_math.py 2 40` (check numbers) → `dotnet run -- 2 40 20 20 12 C:\Temp\Gears` → open the part, change `z` → see the pattern follow.

**Exercises:**
1. Add a keyway to the bore (DIN 6885 table in a dictionary).
2. Add root fillet radius 0.38 m (sketch fillet between flank and root arc).
3. Generate a gear *pair* and an assembly at centre distance a = m(z1+z2)/2.
4. Compare your mass with Design Accelerator's spur gear for the same data.

## P2 Batch export

**Files:** `03-BatchExport` (C#), `vbnet/BatchExportVB`, `ilogic/05_Batch_Export_PDF_DXF`,
`python/batch_export.py`, `powershell/Export-InventorPdf.ps1`, `vba/Module2_ExportPDF.bas`.

**Learn:** translator add-ins, option maps, robust loops (`try/finally Close`), CSV logs. Comparing six
languages doing the same thing is the fastest way to see that the **API is the constant**.

**Exercises:**
1. Name PDFs `<PartNumber>_<Rev>.pdf` from iProperties instead of the file name.
2. Skip drawings whose PDF is newer than the `.idw` (incremental export).
3. Add a `--zip` option: one zip per folder for the supplier.
4. Schedule it nightly (Task Scheduler, [docs/05](05-csharp-vbnet-exe.md#scheduling-run-at-night)).

## P3 iProperty bulk editor

**Files:** `04-IPropertyBulkEditor` (C# + ClosedXML), `python/iprops_excel.py`, `08-ApprenticeReader` (no Inventor).

**Learn:** property sets, custom properties (`Add(value, name)`), Excel round trips, dry-run mode.

**Exercises:**
1. Colour cells that differ between Excel and file (preview before import).
2. Validate Part Number against a regex (`^[A-Z]{2}-\d{5}$`) before writing.
3. Re-implement `import` with Apprentice only — and measure the speed difference.

## P4 BOM to Excel

**Files:** `05-BomToExcel` (C#), `python/bom_to_excel.py`, `ilogic/09_BOM_To_Excel`.

**Learn:** BOM views, recursion over `ChildRows`, BOM structure, mass per row, formatted xlsx.

**Exercises:**
1. Add a "total mass per row" column (qty × mass) and a sum row.
2. Group purchased parts on a separate sheet for the buyer.
3. Add the part thumbnail (Apprentice `Thumbnail`) as a picture in Excel.

## P5 Frame configurator

**Files:** `06-FrameConfigurator` (C# WinForms: MainForm.cs, FrameBuilder.cs), `ilogic/04_Frame_Assembly_Driver`.

**Learn:** WinForms UI in code, creating parts from scratch, reusing parts per length, transformation
matrices (and their order!), iProperties for a clean BOM. The iLogic version shows the template-driven
alternative — compare effort and flexibility.

**Exercises:**
1. Add a middle shelf at height H/2 as an option.
2. Write a cut list (member, length, qty, total metres) to CSV.
3. Replace the plain tube with a Content Center steel profile (JIS square pipe).

## P6 Tank configurator

**Files:** `ilogic/02_Tank_Configurator.iLogicVb`.

**Learn:** multi-value parameters, feature suppression, engineering formulas in a rule, computed iProperties,
an iLogic Form as UI. Formulas: ASME VIII-1 UG-27(c)(1) shell, UG-32 heads (ellipsoidal, torispherical,
hemispherical), capacity.

**Exercises:**
1. Add the longitudinal-stress check UG-27(c)(2): t = P·R/(2·S·E + 0.4·P).
2. Choose the next standard plate thickness automatically (6, 8, 10, 12, 14, 16 mm) and write `t_n`.
3. Add hydrotest pressure 1.3·P·(S_test/S_design) to the description.

## P7 My first add-in

**Files:** `07-MyFirstAddin` (StandardAddInServer, three commands, `.addin`, launch settings).

**Learn:** registry-free add-ins, ribbon UI, icons in .NET 8, modal dialogs parented to Inventor, F5 debugging, deployment folder.

**Exercises:**
1. Add a 4th button that runs `05_Batch_Export_PDF_DXF` via the iLogic automation object.
2. Add an `OnSaveDocument` (before) handler that fills Designer + date.
3. Make the PDF command remember the last output folder in `%APPDATA%\CadAutomation\settings.json`.

## P8 Shaft generator and auto drawing

**Files:** `ilogic/03_Shaft_Generator`, `ilogic/08_Drawing_Auto_Create`.

**Learn:** raw API from iLogic (sketch → revolve → chamfer), then a drawing with base/projected views,
retrieved dimensions, ISO scale snapping.

**Exercises:**
1. Read the step table from Excel instead of the rule text.
2. Add keyway and circlip grooves as optional features.
3. Add a section view through the keyway and a detail view of the groove.

## P9 Excel size table

**Files:** `ilogic/06_Excel_Size_Table` (+ your `Sizes.xlsx`).

**Learn:** GoExcel lookups, multi-value list from Excel, embedding the workbook.

**Exercises:**
1. Build the JIS B 2220 10K flange table for DN15–DN200 (OD, PCD, holes, bolt size, thickness).
2. Write the size into Part Number and Description.
3. Convert the family to an **iPart** and compare (files vs. live rule).

## P10 Gear on the cloud

**Files:** `design-automation/GearDAPlugin/` (+ `bundle/` JSON).

**Learn:** Inventor Server API (`InventorServer`), `Type="Plugin"` add-ins, AppBundles/Activities/WorkItems,
OAuth, signed URLs, local testing with InventorCoreConsole.

**Exercises:**
1. Port the full tooth geometry from P1 into the plug-in (share `GearMath.cs` — already linked).
2. Also output a PDF drawing (second output parameter).
3. A tiny web page (or Excel VBA / Python) that posts a WorkItem and downloads the result.

## P11 Attribute tools

**Files:** `code/autocad/csharp/AttributeTools` (C#), `code/plant3d/autolisp/att_tools.lsp`,
`code/plant3d/scripts/*`, `code/plant3d/python/pyautocad_attributes.py`.

**Learn:** AutoCAD transactions, block references and attributes, handles for round trips, LISP/ActiveX,
scripts, Core Console batch in parallel.

**Exercises:**
1. `ATTEXPORT` for **all** DWGs of a folder via Core Console → one merged CSV (Python).
2. Add layout name filtering to `ATTIMPORT`.
3. LISP: `ATTRENUM` with a "skip existing numbers" option.

## P12 Plant 3D data tools

**Files:** `code/plant3d/csharp/Plant3DDataTools` (PlantHelpers, PipingCommands, PidCommands),
`code/plant3d/python/csv_to_excel_report.py`.

**Learn:** Plant SDK entry chain, DataLinksManager, PnPDatabase tables, relationships, writing properties,
P&ID tag export; then Python for the Excel report.

**Steps:** Plant 3D trial + sample project (Plant 3D ships a sample project) → `NETLOAD` → `P3DTABLES`
→ `P3DLINELIST` → `P3DMTO` → `PIDTAGS` on a P&ID → `python csv_to_excel_report.py "<project>\*.csv" -o Report.xlsx`.

**Exercises:**
1. Line list: add design pressure/temperature columns (custom properties in Project Setup) and check they are filled.
2. MTO: add pipe length per line (Pipe table length column — find its name with `P3DDUMP Pipe`).
3. P&ID vs 3D: list line numbers present in P&ID but missing in 3D (two `DataLinksManager`s).

## P13 Plant 3D custom part script

**Files:** `code/plant3d/custom-part-scripts/CPNOZZLE_STUB.py`.

**Learn:** embedded Python geometry scripts, decorators, ports, registration and testing.

**Exercises:**
1. Add a weld-neck shoulder: second, larger hollow cylinder at one end, joined with `uniteWith`.
2. Add a parameter `B` (bevel) shown in Catalog Builder.
3. Build a spectacle blind (two discs + web) — check port directions carefully.

## Suggested 8-week plan

| Week | Projects |
|---|---|
| 1 | P2 (iLogic + C# versions), P9 |
| 2 | P1, P3 |
| 3 | P4, P6 |
| 4 | P7 |
| 5 | P5, P8 |
| 6 | P11 (plain AutoCAD is enough) |
| 7 | P12, P13 (Plant 3D trial) |
| 8 | P10 (APS account) |

Next: [16 — glossary](16-glossary.md).
