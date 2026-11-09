# 09 — Drawings, BOM, export (PDF/DXF/STEP), Content Center, iParts/iAssemblies, Excel

The daily bread of Inventor automation. Most companies save more hours here than with any generator.

## Contents
- [Translator add-ins: the export pattern](#translator-add-ins-the-export-pattern)
- [PDF options](#pdf-options)
- [DXF/DWG from drawings](#dxfdwg-from-drawings)
- [STEP (and other 3D formats)](#step-and-other-3d-formats)
- [Sheet-metal flat pattern DXF](#sheet-metal-flat-pattern-dxf)
- [Drawing automation](#drawing-automation)
- [BOM: views, rows, structure](#bom-views-rows-structure)
- [Content Center](#content-center)
- [iParts and iAssemblies](#iparts-and-iassemblies)
- [Excel driving: four ways](#excel-driving-four-ways)
- [Export recipes: which project does what](#export-recipes-which-project-does-what)

## Translator add-ins: the export pattern

Every file export except the flat pattern goes through a **TranslatorAddIn**, identified by GUID:

| Format | Translator GUID |
|---|---|
| PDF | `{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}` |
| DXF | `{C24E3AC4-122E-11D5-8E91-0010B541CD80}` |
| DWG (AutoCAD) | `{C24E3AC2-122E-11D5-8E91-0010B541CD80}` |
| STEP | `{90AF7F40-0C01-11D5-8E83-0010B541CD80}` |

List all on your machine: loop `app.ApplicationAddIns`, print those that are `TranslatorAddIn` with
`DisplayName` and `ClassIdString` — IGES, Parasolid, JT, STL, 3D PDF, OBJ… are there too.

The pattern (C#, from `code/inventor/csharp/03-BatchExport/Exporter.cs`):

```csharp
var pdf = (TranslatorAddIn)app.ApplicationAddIns.ItemById("{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}");
TranslationContext ctx = app.TransientObjects.CreateTranslationContext();
ctx.Type = IOMechanismEnum.kFileBrowseIOMechanism;
NameValueMap opts = app.TransientObjects.CreateNameValueMap();
DataMedium dm = app.TransientObjects.CreateDataMedium();
if (pdf.HasSaveCopyAsOptions[doc, ctx, opts])        // fills opts with the defaults
{
    opts.Value["All_Color_AS_Black"] = 0;
    opts.Value["Sheet_Range"] = PrintRangeEnum.kPrintAllSheets;
}
dm.FileName = @"C:\Export\Bracket.pdf";
pdf.SaveCopyAs(doc, ctx, opts, dm);
```

- `HasSaveCopyAsOptions` is a **property with arguments** in C# → square brackets `[doc, ctx, opts]`. In VB/iLogic/Python it is a normal call.
- The translator add-in must be **activated** (normally is). If `ItemById` returns an inactive add-in: `addin.Activate()`.

## PDF options

| Option | Values | Meaning |
|---|---|---|
| `All_Color_AS_Black` | 0/1 | monochrome output |
| `Remove_Line_Weights` | 0/1 | 1 = thin lines everywhere |
| `Vector_Resolution` | e.g. 400, 720 | DPI for vector content |
| `Sheet_Range` | `kPrintAllSheets` (14082), `kPrintCurrentSheet`, `kPrintSheetRange` | which sheets |
| `Custom_Begin_Sheet` / `Custom_End_Sheet` | int | with `kPrintSheetRange` |

Option names come from the API help sample *"Export to PDF"* and are case-sensitive. To see every
option your version supports, print the map after `HasSaveCopyAsOptions`:

```csharp
for (int i = 1; i <= opts.Count; i++) Console.WriteLine($"{opts.Name[i]} = {opts.Value[opts.Name[i]]}");
```

## DXF/DWG from drawings

- The DXF/DWG translators read an **export configuration .ini**: in Inventor *Save As → Save Copy As → DXF → Options → (set layers, version, scaling) → Save Configuration*. Then:
  `opts.Value["Export_Acad_IniFile"] = @"C:\CAD-Automation\config\dxf_2018.ini";`
- Multi-sheet drawings → one file per sheet with a `_Sheet_N` style suffix (depends on ini settings).
- For model-space-only DXF of a *part* (laser cutting), use the **flat pattern** route or a face export (`DataIO` on a face) — not the drawing translator.

## STEP (and other 3D formats)

```csharp
opts.Value["ApplicationProtocolType"] = 3;     // 2 = AP203, 3 = AP214, 5 = AP242 (forum-confirmed; VERIFY in your release)
opts.Value["Author"] = Environment.UserName;
opts.Value["Organization"] = "My Company";
```

- Assemblies export as one STEP with structure. To export each part separately, walk `ReferencedDocuments`.
- Model States: the export uses the **active** model state; switch first if needed (`ComponentDefinition.ModelStates[name].Activate()`).

## Sheet-metal flat pattern DXF

No translator — `DataIO.WriteDataToFile` with a **format string**:

```csharp
var smcd = (SheetMetalComponentDefinition)part.ComponentDefinition;
if (!smcd.HasFlatPattern) { smcd.Unfold(); smcd.FlatPattern.ExitEdit(); }
string fmt = "FLAT PATTERN DXF?AcadVersion=2018"
           + "&OuterProfileLayer=OUTER&InteriorProfilesLayer=INNER"
           + "&InvisibleLayers=IV_TANGENT;IV_BEND;IV_BEND_DOWN;IV_ARC_CENTERS";
smcd.DataIO.WriteDataToFile(fmt, @"C:\Export\Plate.dxf");
```

- Other keys (from API help / forum samples): `BendUpLayer`, `BendDownLayer`, `SimplifySplines=True`, `SplineTolerance`, `MergeProfilesIntoPolyline=True`, `RebaseGeometry=True`. **VERIFY** each against your release's *DataIO* help page — they are string keys, so typos fail silently.
- Laser shops usually want: outer + inner contours only, polylines merged, splines simplified, no bend lines (or bend lines on a separate layer for press-brake).
- Engineering check: the flat length depends on the **K-factor / bend table** in the sheet-metal style. Automating export does not validate that — check one part per material/thickness with a test bend.

## Drawing automation

`08_Drawing_Auto_Create.iLogicVb` creates a drawing from a part/assembly:

```vb
Dim tpl As String = ThisApplication.FileManager.GetTemplateFile(DocumentTypeEnum.kDrawingDocumentObject)
Dim dwg As DrawingDocument = ThisApplication.Documents.Add(DocumentTypeEnum.kDrawingDocumentObject, tpl, True)
Dim sh As Sheet = dwg.ActiveSheet
Dim front As DrawingView = sh.DrawingViews.AddBaseView(model, tg.CreatePoint2d(x, y), scale,
        ViewOrientationTypeEnum.kFrontViewOrientation, DrawingViewStyleEnum.kHiddenLineRemovedDrawingViewStyle)
sh.DrawingViews.AddProjectedView(front, tg.CreatePoint2d(x, y2), DrawingViewStyleEnum.kFromBaseDrawingViewStyle)
sh.DrawingDimensions.GeneralDimensions.Retrieve(front)       ' model dimensions into the view
sh.PartsLists.Add(front, tg.CreatePoint2d(sh.Width - 1, sh.Height - 1))
```

| Task | API |
|---|---|
| base/projected/section/detail views | `DrawingViews.AddBaseView / AddProjectedView / AddSectionView / AddDetailView` |
| view scale & label | `DrawingView.Scale`, `.ShowLabel`, `.Label.FormattedText` |
| retrieve model dimensions | `GeneralDimensions.Retrieve(view)` |
| add dimensions yourself | `GeneralDimensions.AddLinear(textPos, intent1, intent2)` with `Sheet.CreateGeometryIntent(curve)` — curves from `view.DrawingCurves(modelEntity)` |
| parts list | `Sheet.PartsLists.Add(view, point)` → `PartsList.PartsListRows` |
| balloons | `Sheet.Balloons.Add(leaderPoints, ...)`; *auto-balloon* API varies by release — VERIFY |
| title block fields | prompted entries: `TitleBlock.GetResultText` / `SetPromptResultText`; better: link fields to **iProperties** in the title block definition so no code is needed |
| sheet size/format | `Sheets.AddUsingSheetFormat(format, model)` — sheet formats in the template give consistent layouts with zero geometry code |
| revision table | `Sheet.RevisionTables` |

Biggest win with least code: **good templates + sheet formats + iProperty-linked title blocks**, then
code only for what varies.

`NiceScale()` snaps to ISO 5455 scales (1:1, 1:2, 1:5, 1:10, 2:1, 5:1…). Sheet units are **cm** like everything else.

## BOM: views, rows, structure

```csharp
BOM bom = asm.ComponentDefinition.BOM;
bom.StructuredViewEnabled = true;
bom.StructuredViewFirstLevelOnly = false;          // all levels
BOMView view = bom.BOMViews.Cast<BOMView>().First(v => v.ViewType == BOMViewTypeEnum.kStructuredBOMViewType);
Walk(view.BOMRows, 0);

void Walk(BOMRowsEnumerator rows, int level)
{
    foreach (BOMRow row in rows)
    {
        ComponentDefinition cd = row.ComponentDefinitions[1];
        var doc = (Document)cd.Document;
        Console.WriteLine($"{new string(' ', level * 2)}{row.ItemNumber}  {row.ItemQuantity}  {doc.DisplayName}  {row.BOMStructure}");
        if (row.ChildRows != null) Walk(row.ChildRows, level + 1);
    }
}
```

| BOM concept | Meaning |
|---|---|
| Model Data | every component (internal) — not usually exported |
| **Structured** view | tree (first level or all levels) — for assembly drawings |
| **Parts Only** view | flat, quantities summed — for purchasing/cutting lists |
| `BOMStructure` | Normal, Phantom (dissolved into parent), Reference (excluded), Purchased (children hidden), Inseparable |
| `ItemQuantity` vs `TotalQuantity` | per parent vs. string with units (Frame Generator lengths: `"1250 mm"`) |
| View names | localized in the UI ("部品のみ" in Japanese) → find views by `ViewType`, never by name |

`BOMView.Export(file, FileFormatEnum.kMicrosoftExcelFormat)` = Inventor's own export
(`09_BOM_To_Excel.iLogicVb`). It needs Excel/ACE drivers and gives little formatting control →
`05-BomToExcel` (ClosedXML) and `bom_to_excel.py` (openpyxl) write formatted files without Excel.

Mass per row: `MassProperties.Mass` of the definition (kg) — slow on big assemblies if mass is not up
to date (*Update Mass Properties* first, or read the iProperty `Mass` with care).

## Content Center

Content Center = libraries of standard parts (ISO/JIS/ANSI bolts, nuts, profiles…) stored in Vault or
*Desktop Content* (`.idcl` libraries).

| Task | How |
|---|---|
| place a standard part from code | `ContentCenter.TreeViewTopNode` → navigate categories → `ContentFamily` → `CreateMember(row, ...)` returns the member file path → `Occurrences.Add(path, matrix)` |
| find a family by name | iterate `ContentTreeViewNode.Families`; match `ContentFamily.DisplayName` / `InternalName` |
| row selection | `ContentFamily.TableRows` + `ContentTableColumns` (e.g. *Nominal Diameter*, *Nominal Length*) |
| JIS parts for a Japanese company | make sure the JIS libraries are in the project's library list (*Projects → Configure Content Center Libraries*) |
| check "is this a CC part?" | `PartComponentDefinition.IsContentMember` |

Content Center API is verbose and slow (each `CreateMember` may generate a file). Cache: build a
dictionary *(family, size) → member path* once per session. In iLogic the snippet *Components →
Add Content Center Part* (2019+) wraps this — try it before writing raw API. **VERIFY** exact
signatures of `CreateMember` (it has several `out` parameters) in the API help of your release.

## iParts and iAssemblies

| | iPart | iAssembly | Model States (2022+) | iLogic configurator |
|---|---|---|---|---|
| Stored | factory + table inside the part | factory + table inside the assembly | states inside the file | rules + parameters |
| Members | **generated files** (one per row) | generated files | no extra files | Save-As copies |
| Best for | catalogue families (sizes of one design) | assemblies of standard sizes | variants that never leave the file (simplified, manufacturing states) | engineered-to-order variants |
| Automation | `iPartFactory.TableRows`, `iPartMember`, `Occurrences.AddiPartMember(factoryPath, matrix, row)` | `iAssemblyFactory`, `AddiAssemblyMember` | `ModelStates.Add`, `.Activate()` | rules |

```csharp
var factory = ((PartDocument)doc).ComponentDefinition.iPartFactory;
foreach (iPartTableRow row in factory.TableRows)
    Console.WriteLine($"{row.Index}: {row.MemberName}");
// place size row 3 in an assembly:
ComponentOccurrence occ = asmDef.Occurrences.AddiPartMember(@"C:\Lib\Spacer.ipt", matrix, 3);
```

- iPart tables can be **edited in Excel** (*Edit via Spreadsheet*) — the simplest form of Excel driving.
- Choose iPart for "the size list is fixed and comes from a standard"; iLogic for "dimensions are calculated".

## Excel driving: four ways

| Way | Code? | Live link? | Typical |
|---|---|---|---|
| **Parameter link** (*Manage → Parameters → Link*) to an `.xlsx` | none | yes, parameters update from cells | calculation sheet drives dimensions |
| **iPart table** via spreadsheet | none | table only | size families |
| **GoExcel** in iLogic | small | when rule runs | size tables, lookup by key (`06_Excel_Size_Table`) |
| **External program** (ClosedXML / openpyxl) | yes | batch | iProperties round trip (`04-IPropertyBulkEditor`, `iprops_excel.py`), BOM export |

Parameter link tips: named cells must be in columns A/B (name/value[/unit]) from the start cell you
choose; units in column C; save the Excel file before Inventor updates. Keep the workbook next to the
model (relative path) and in the same Vault folder.

## Export recipes: which project does what

| I need… | Use |
|---|---|
| PDF of one drawing, on a button | `07-MyFirstAddin` *Export PDF* |
| PDF+DXF of a whole folder now | `03-BatchExport --pdf --dxf --recurse` or `05_Batch_Export_PDF_DXF` (iLogic external) |
| STEP of all parts for a supplier | `03-BatchExport --step` or `python batch_export.py --step` |
| laser DXF of all sheet-metal parts | `03-BatchExport --flat` |
| BOM for purchasing | `05-BomToExcel --parts` |
| fix descriptions in 300 files | `04-IPropertyBulkEditor` (or `ApprenticeReader --set` without Inventor) |
| drawing for each new part | `08_Drawing_Auto_Create` |
| everything automatically on release | Vault Job Processor ([docs/08](08-apprentice-design-automation-vault.md#vault-what-to-know-as-an-automation-engineer)) |

Next: [10 — AutoCAD .NET basics](10-autocad-dotnet-basics.md).
