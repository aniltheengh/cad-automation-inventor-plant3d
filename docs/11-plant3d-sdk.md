# 11 — AutoCAD Plant 3D: SDK, project data, piping, P&ID, specs, Isogen, custom parts

## Add-in learning levels

### Beginner — connect a drawing object to its information
Think of a valve's drawing object and its project-data record as linked pieces of information. First inspect a known object manually in Data Manager. Write down one property name and value. **Checkpoint:** distinguish geometry from project properties. Read [workbook B1 and T3](19-addin-learning-from-zero.md) before the API tables below.

### Transition — trace project, part, manager and row
Read PlantHelpers and PIDPICK. Follow current project → project part → DataLinksManager → row lookup → property values. Identify where no project, an ordinary entity or a missing property could interrupt the sequence. **Exercise:** explain why a piping manager should not be assumed correct for a P&ID object. **Answer:** each project part has its own data context. Existing samples need explicit error handling before being used as robust tools.

### Intermediate — define scope before queries or changes
Specify whether the operation covers one selection, one drawing or the project database. Reconcile an export against a small known project. Add validation and a preview before writing properties; test actual persistence after save/reopen. **Checkpoint:** distinguish one row per line group from one row per component, and piece counts from pipe length. Routing, connectivity edits, catalogs and isometrics are later specialist topics, not prerequisites for a first property-reporting add-in.


**Dedicated add-in track:** [Chapter 18 — Plant 3D add-in development with C#](18-plant3d-addin-development.md) covers commands, lifecycle, debugging and deployment. Read its version-matching guidance before using the original .NET 8 setup below.

> **Honesty first.** The Plant 3D API is real and powerful, but its public documentation is **thin**.
> The SDK help (`.chm`) lists classes with little explanation; the best learning material is the ADN
> *Plant3DTraining* labs (written for Plant 3D 2011–2015 and still valid in structure), forum threads and
> developer-blog posts. In this chapter every member name was taken from those public samples; anything
> not seen there is marked **VERIFY**. Build against the real SDK before trusting a signature.
>
> **New to the API?** Read [11a — architecture and class hierarchy](11a-plant3d-api-architecture.md),
> [11b — first add-in, step by step](11b-plant3d-first-addin.md) and [11c — code examples](11c-plant3d-code-examples.md) first;
> this chapter is the reference for project data, specs, isos and custom parts.

## Contents
- [How Plant 3D stores data](#how-plant-3d-stores-data)
- [Ways to automate Plant 3D](#ways-to-automate-plant-3d)
- [Plant SDK setup (.NET 8)](#plant-sdk-setup-net-8)
- [The entry chain: project → part → DataLinksManager](#the-entry-chain-project--part--datalinksmanager)
- [Three kinds of IDs](#three-kinds-of-ids)
- [DataLinksManager members (seen in public samples)](#datalinksmanager-members-seen-in-public-samples)
- [PnPDatabase: tables, rows, queries](#pnpdatabase-tables-rows-queries)
- [Piping: lines, parts, connections](#piping-lines-parts-connections)
- [P&ID](#pid)
- [Writing data safely](#writing-data-safely)
- [Line list and MTO to Excel](#line-list-and-mto-to-excel)
- [Specs and catalogs](#specs-and-catalogs)
- [Isometrics: Isogen and PCF](#isometrics-isogen-and-pcf)
- [Project setup automation](#project-setup-automation)
- [Custom part scripts (Python inside Plant 3D)](#custom-part-scripts-python-inside-plant-3d)
- [Dynamo for Plant 3D: not available](#dynamo-for-plant-3d-not-available)
- [Reading project data without Plant 3D](#reading-project-data-without-plant-3d)
- [What Autodesk officially says](#what-autodesk-officially-says)

## How Plant 3D stores data

```
MyProject\
  Project.xml                ← project definition (parts, paths, settings)
  ProcessPower.dcf           ← P&ID project database   (SQLite by default)
  Piping.dcf                 ← 3D piping database       (SQLite)
  Ortho.dcf, Iso.dcf         ← orthographic / isometric databases
  P&ID DWG\  Plant 3D Models\  Orthos\  Isos\  Spec Sheets\  Related Files\ ...
  Spec Sheets\*.pspx         ← piping specs   (SQLite)
  (catalogues *.pcat in the shared content folder) (SQLite)
```

- A Plant object in a DWG (pipe, valve, P&ID symbol) is linked to a **row** in a project database table. The DWG holds geometry; the **database holds properties** (tag, size, spec, service, insulation, status…).
- *Data Manager* (`DATAMANAGER`) in Plant 3D is the UI view of these tables.
- Collaboration projects: the same model with **SQL Server** or **Autodesk Docs (ACC/BIM 360)** instead of local SQLite. Your API code is the same; never touch the files directly in those setups.
- Every class (Pipe, Elbow, Valve, Equipment, P&ID `HandValve`…) is a **table**; class inheritance (e.g. `PipeRunComponent` → `Pipe`, `Elbow`, …) means a parent table lists all children's common columns.

## Ways to automate Plant 3D

| Need | Tool | Doc |
|---|---|---|
| read/write **Plant properties**, line lists, MTO, tags, relationships | **.NET Plant SDK** (this chapter) | here |
| general DWG work: attributes, layers, title blocks, purge | AutoLISP / AutoCAD .NET | [10](10-autocad-dotnet-basics.md), [12](12-autolisp-scripts-coreconsole-python.md) |
| batch the above over many DWGs | Core Console (Plant commands **not** available there) | [12](12-autolisp-scripts-coreconsole-python.md#core-console) |
| new catalogue geometry (special parts) | Python custom part scripts | [below](#custom-part-scripts-python-inside-plant-3d) |
| reports without code | Data Manager export, **Report Creator** (ships with Plant 3D) | — |
| isometric styles | Iso style configuration files (no code) | [below](#isometrics-isogen-and-pcf) |
| Excel report formatting | Python (openpyxl) on CSVs from .NET | [12](12-autolisp-scripts-coreconsole-python.md#python-for-autocad-and-plant-3d) |
| visual programming | **Dynamo: not available** | [below](#dynamo-for-plant-3d-not-available) |

## Plant SDK setup (.NET 8)

0. Match the release: Plant 3D **2026** = .NET 8 (default here); Plant 3D **2027** = .NET 10 → `-p:AcadRelease=2027` ([docs/00](00-getting-started.md#getting-autocad-plant-3d)). 2026 also introduced a redesigned Spec Editor; 2027 adds multi-sheet isometrics (Autodesk Plant 3D toolset page).
1. Download the **AutoCAD Plant 3D 2026 SDK** (Autodesk developer site / APS → *Plant 3D*; ADN members via the ADN portal). Unzip, e.g. `C:\Autodesk\Plant3D 2026 SDK\`.
2. Reference DLLs (from the SDK `inc-x64` folder, or the install folders: Autodesk's support article puts `PnPDataLinks.dll` and `PnPDataObjects.dll` in `C:\Program Files\Autodesk\AutoCAD 2026\` and `PnPProjectManagerMgd.dll` in its `PLNT3D\` subfolder — for the others, `P3DHELLO` prints the folder of every loaded Plant DLL), all with **Copy Local = False**:

   | DLL | Namespaces you use | For |
   |---|---|---|
   | `PnPProjectManagerMgd.dll` | `Autodesk.ProcessPower.ProjectManager`, `Autodesk.ProcessPower.PlantInstance` | `PlantApplication`, `PlantProject`, `Project` |
   | `PnPDataLinks.dll` | `Autodesk.ProcessPower.DataLinks` | `DataLinksManager` |
   | `PnPDataObjects.dll` | `Autodesk.ProcessPower.DataObjects` | `PnPDatabase`, `PnPTable`, `PnPRow` |
   | `PnP3dProjectPartsMgd.dll` | `Autodesk.ProcessPower.P3dProjectParts` | `PipingProject` |
   | `PnP3dObjectsMgd.dll` | `Autodesk.ProcessPower.PnP3dObjects` | `Part`, `Pipe`, `ConnectionIterator` |
   | `PnIdProjectPartsMgd.dll` | P&ID project part | `PnIdProject` |
   | `PnIDMgd.dll` | `Autodesk.ProcessPower.PnIDObjects`, `...Styles` | `LineSegment`, `Asset`, styles |
   | `PnPCommonDbxMgd.dll` | `Autodesk.ProcessPower.PnPCommonDbx`, `Autodesk.ProcessPower.Styles` | `FormatStringUtils`, `Style`, `LineStyle`, `AssetStyle` |
   | `PnPCommonMgd.dll` | — (no `using` needed) | base class of the table/column collections; without it: CS0012 |

   (DLL and namespace names from the ADN *Plant3DTraining* lab projects.)
3. AutoCAD references: NuGet `AutoCAD.NET` 25.1.0 with `ExcludeAssets="runtime"` ([docs/10](10-autocad-dotnet-basics.md#project-setup-with-the-official-nuget-package)).
4. `code/plant3d/Directory.Build.props` → set `PlantSdkInc`. `Plant3DDataTools.csproj` then builds with the real SDK; `-p:PlantStubs=true` builds against `tools/plant-sdk-stubs` (syntax check only).
5. Debug: start `acad.exe` with `/product PLNT3D` (copy the arguments from the Plant 3D desktop shortcut), open a project in Project Manager, `NETLOAD` your DLL.

Loader problem you will meet: *"Could not load file or assembly 'PnP3dObjectsMgd'"* when your command
runs → the DLL lives in a folder .NET does not probe. Fix with a resolve handler in an
`IExtensionApplication.Initialize`:

```csharp
AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
{
    string name = new AssemblyName(e.Name).Name + ".dll";
    foreach (string dir in new[] { @"C:\Program Files\Autodesk\AutoCAD 2026\PLNT3D", @"C:\Program Files\Autodesk\AutoCAD 2026" })
        if (File.Exists(Path.Combine(dir, name))) return Assembly.LoadFrom(Path.Combine(dir, name));
    return null;
};
```

## The entry chain: project → part → DataLinksManager

```csharp
using Autodesk.ProcessPower.PlantInstance;      // PlantApplication
using Autodesk.ProcessPower.ProjectManager;     // PlantProject, Project
using Autodesk.ProcessPower.DataLinks;          // DataLinksManager
using Autodesk.ProcessPower.DataObjects;        // PnPDatabase

PlantProject prj = PlantApplication.CurrentProject;            // null if no project open
Project piping = prj.ProjectParts["Piping"];                   // 3D
Project pid    = prj.ProjectParts["PnId"];                     // P&ID  (labs use both "PnId" and "PnID")
DataLinksManager dlm = piping.DataLinksManager;
PnPDatabase db = dlm.GetPnPDatabase();
string folder = prj.ProjectFolderPath;                         // the project; a PART has ProjectDirectory
```

Project part names come from `Project.xml`: typically `Piping`, `PnId`, `Ortho`, `Iso`. Typed versions:
`(PipingProject)prj.ProjectParts["Piping"]`, `(PnIdProject)prj.ProjectParts["PnID"]`.

## Three kinds of IDs

| ID | Type | Lives | Convert |
|---|---|---|---|
| AutoCAD `ObjectId` | `ObjectId` | this session, this DWG | `dlm.FindAcPpRowId(objectId)` → row id |
| **Row id** (`PnPID`) | `int` | project database, permanent | `dlm.FindAcPpObjectIds(rowId)` → `PpObjectId`s → `dlm.MakeAcDbObjectId(ppId)` |
| `PpObjectId` | Plant object id (DWG id + handle) | cross-drawing link | `dlm.MakeAcPpObjectId(objectId)` |

The row id is what you store in CSVs and use in `GetAllProperties` / `SetProperties`. One row can map
to several DWG objects (e.g. a P&ID symbol and its tag annotation) — `PIDTAGS` de-duplicates by row id.

## DataLinksManager members (seen in public samples)

| Member | Does |
|---|---|
| static `GetManager(Database db)` | the manager of the part a drawing belongs to ([11c, Example 2](11c-plant3d-code-examples.md#example-2--the-active-document-and-its-project-part)) |
| `GetPnPDatabase()` | the project-part database |
| `HasLinks(ObjectId)` | cheap test: is this a Plant object? — use before `FindAcPpRowId` |
| `GetObjectClassname(ObjectId)` | project class (= table name) of a DWG object |
| `FindAcPpRowId(ObjectId)` | row id of a DWG object (≤ 0 / exception if not a Plant object) |
| `MakeAcPpObjectId(ObjectId)` / `FindAcPpObjectIds(int rowId)` / `MakeAcDbObjectId(PpObjectId)` | ID conversions |
| `GetAllProperties(int rowId, bool)` — the labs pass `true` (meaning of the flag: VERIFY) | `List<KeyValuePair<string,string>>` of every property |
| `SetProperties(int rowId, StringCollection names, StringCollection values)` | write properties (parallel collections) |
| `GetRelatedRowIds(relationshipType, role1, rowId, role2)` | follow a relationship, e.g. `("P3dLineGroupPartRelationship", "LineGroup", lineRowId, "Part")` |
| `Relate(...)` / `Unrelate(...)` | create/remove relationships (VERIFY signatures in SDK help) |
| `GetDrawingId(Database db)` | id of a drawing in this part's database; `-1` = not a drawing of this part (forum) |
| event `DataLinkOperationOccurred` | react to data changes |

Our `Plant3DDataTools` uses exactly: `GetPnPDatabase`, `FindAcPpRowId`, `GetAllProperties`, `SetProperties`, `GetRelatedRowIds`.

## PnPDatabase: tables, rows, queries

```csharp
PnPDatabase db = dlm.GetPnPDatabase();
foreach (PnPTable t in db.Tables) ed.WriteMessage($"\n{t.Name}");          // P3DTABLES does this
PnPTable lines = db.Tables["P3dLineGroup"];
PnPRow[] rows = lines.Select("Tag = '10\"-P-1001-CS150'");                // filter = SQL-like WHERE
foreach (PnPRow r in lines.Select())
{
    int id = r.RowId;
    string tag = Convert.ToString(r["Tag"]);                              // indexer by column name
}
string cls = db.GetRowTableName(rowId);                                     // "Pipe", "Elbow", "GateValve"...
```

| Table (piping) | Content |
|---|---|
| `P3dLineGroup` | line numbers (one row per line) |
| `PipeRunComponent` | parent of all inline parts: pipe, fittings, valves… columns like `PartFamilyLongDesc`, `PartSizeLongDesc`, `ShortDescription`, `LineNumberTag`, `Spec`, `Size` |
| `Pipe`, `Elbow`, `Tee`, `Reducer`, `Flange`, `Valve` subclasses … | per class |
| `Equipment` and subclasses | equipment |
| `PnPDataLinks` | DWG-object ↔ row links (internal; read only) |

Column names differ by project (custom properties added in *Project Setup*). Always **discover**:
`P3DTABLES` lists tables; `P3DDUMP` writes a chosen table to CSV; `PIDPICK` prints properties of a
picked object. Then code against real names.

Write access through rows (`BeginEdit` / set indexer / `EndEdit`) exists in the labs; for **user-visible
properties prefer `dlm.SetProperties`** so Plant keeps DWG annotations and history consistent.

## Piping: lines, parts, connections

- **Line number** = `P3dLineGroup` row; parts belong to it through `P3dLineGroupPartRelationship` (roles `LineGroup` ↔ `Part`).
- Walking the physical connectivity (pipe → elbow → flange…): `ConnectionIterator.NewIterator(part.ObjectId)` from `PnP3dObjectsMgd` (ADN LAB 4 *PipeWalk*). Use it for "trace from this nozzle to the end of the line" tools.
- Geometry: Plant parts are AutoCAD custom entities (`Autodesk.ProcessPower.PnP3dObjects.Part` and subclasses) with ports (`Port` positions/directions). Reading is fine; **creating** routed piping from code is possible but complex and fragile — the SDK has routing-related classes, but public samples are few. VERIFY before promising "automatic routing" to anyone.

## P&ID

```csharp
Project pidPart = PlantApplication.CurrentProject.ProjectParts["PnId"];
DataLinksManager dlm = pidPart.DataLinksManager;
int rowId = dlm.FindAcPpRowId(pickedId);
foreach (var kv in dlm.GetAllProperties(rowId, true)) ed.WriteMessage($"\n{kv.Key} = {kv.Value}");
```

- P&ID objects: `Autodesk.ProcessPower.PnIDObjects` — `LineSegment` (process/signal lines), `Asset` (equipment, valves, instruments as symbols), styles in `Autodesk.ProcessPower.Styles` (`LineStyle`, symbol styles). ADN LAB 2 creates a line group and line objects.
- **P&ID ↔ 3D validation** is built in (*Validate*, *P&ID Line List* in the 3D ship-from-P&ID workflow) — automate around it rather than re-implementing.
- Tags: formatted by tag format definitions in Project Setup. `FormatStringUtils.Evaluate("#(Project.General.Project_Name)", oid)` (LAB 3) evaluates format strings — handy for title blocks from project data.

`PIDTAGS` (in `PidCommands.cs`) exports every tagged object in the current P&ID drawing with **all**
property columns → `<dwg>_tags.csv` → instrument index / valve list / equipment list in Excel.

## Writing data safely

```csharp
var names = new StringCollection { "Status", "InsulationType" };
var values = new StringCollection { "IFC", "HC" };
dlm.SetProperties(rowId, names, values);
```

Rules:
- Project databases are shared: **check-out/lock** rules apply in collaboration projects; run bulk writes when nobody else edits those drawings.
- Back up the project (zip the folder or rely on server backups) before the first bulk write.
- Some properties are **read-only / calculated** (size, spec-derived descriptions). Change those by changing the spec or the part, not by writing the row.
- Write property **values that exist** in the property's value list (acceptable values in Project Setup) — otherwise validation flags them.
- After writing, annotations update on regen/reopen.

`P3DSETPROPS` reads `PnPID,Property,Value` rows from a CSV (exported via `P3DDUMP`/`P3DLINELIST`, edited in Excel) and groups them per row id.

## Line list and MTO to Excel

```
Plant 3D: P3DLINELIST → LineList_yyyyMMdd_HHmm.csv   (all P3dLineGroup columns + part counts)
          P3DMTO      → MTO_yyyyMMdd_HHmm.csv        (count per line / description / size)
          PIDTAGS     → <dwg>_tags.csv
PC:       python code/plant3d/python/csv_to_excel_report.py "<project>\*.csv" -o PlantReport.xlsx
```

Limits of the sample MTO (stated in the code): it counts **pieces**; pipe **length** needs the pipe
length column or Plant's own **Report Creator** (which knows cut lengths and is the supported way for an
official MTO). Use the code version for quick checks and custom groupings; use Report Creator /
isometric BOMs for issued documents.

## Specs and catalogs

| File | What | Edit with |
|---|---|---|
| `*.pcat` | catalogue: every possible part (sizes, end types, geometry script or block) | **Spec Editor → Catalog Builder** |
| `*.pspx` | spec: the parts allowed for a service class (e.g. `CS150`), size ranges, branch table | **Spec Editor** |
| `*.pcat`/`*.pspx` format | SQLite databases | read-only queries are possible; **never write directly** |

Automation reality:
- Spec creation is mostly a **UI job** (Spec Editor). The SDK contains spec-related classes, but public samples are rare — VERIFY before planning a code-driven spec generator.
- Practical automation: **report** what's in a spec (read the `.pspx` with SQLite in Python, read-only, on a *copy*), compare two specs, check that every size in the line list exists in the spec.
- Custom geometry for catalogue parts → custom Python part scripts (next sections).

## Isometrics: Isogen and PCF

- Plant 3D produces isometrics with **Isogen** (Hexagon/Alias technology licensed by Autodesk). *Quick Iso* / *Production Iso* from the ribbon.
- Isometric **styles** (title block, BOM, dimensions, weld numbering, split rules) live in the project's iso configuration folders and are edited in *Project Setup → Isometric DWG Settings*. That is configuration, not programming.
- **PCF export** (Piping Component File, a plain-text Isogen input format) is available from Plant 3D → a strong automation lever: export PCFs, then process with Python (weld counts, check SKEYs, feed stress tools).
- No public API to "drive Isogen" step by step from .NET is documented — VERIFY; use the UI/batch iso creation and post-process the results (DWG attributes via `ATTEXPORT`, PCF via Python).

## Project setup automation

What engineers want: *"new project in 5 minutes with our standard specs, title blocks, tag formats, iso styles"*.

| Approach | How | Notes |
|---|---|---|
| **Template project** (recommended) | build one perfect project; *New Project* wizard → copy settings from it | supported, no code |
| folder script | PowerShell: copy template folder, rename, fix paths in `Project.xml` | VERIFY each `Project.xml` path element; Autodesk does not document editing it — test on a copy |
| .NET after creation | open project → `SetProperties`/project properties for client, site, job number | ADN LAB 1 `ProjectSettings` reads project settings |
| collaboration | ACC/BIM 360 project setup is admin UI | — |

Keep the template project in version control (specs, title blocks, `.dwt`, Project Setup XMLs) so changes are reviewed.

## Custom part scripts (Python inside Plant 3D)

Plant 3D builds parametric catalogue parts with **Python scripts** run by its embedded engine (not your
Anaconda Python). Autodesk's own content uses them; you can add yours.

```python
from aqa.math import *
from varmain.primitiv import *
from varmain.custom import *

@activate(Group="Pipe", TooltipShort="Stub nozzle", TooltipLong="Straight hollow stub, two ports",
          LengthUnit="mm", Ports=2)
@group("MainDimensions")
@param(D=LENGTH, TooltipShort="Outside diameter")
@param(L=LENGTH, TooltipLong="Length")
@param(T=LENGTH, TooltipLong="Wall thickness")
def CPNOZZLE_STUB(s, D=114.3, L=150.0, T=6.0, **kw):
    body = CYLINDER(s, R=D / 2.0, H=L, O=D / 2.0 - T).rotateY(90.0)   # O = inner radius
    s.setPoint((0.0, 0.0, 0.0), (-1.0, 0.0, 0.0))                       # port 1: position, direction
    s.setPoint((L, 0.0, 0.0), (1.0, 0.0, 0.0))                          # port 2
```

| Element | Meaning (checked against stock Autodesk scripts) |
|---|---|
| `aqa.math`, `varmain.primitiv`, `varmain.custom` | the script libraries |
| `@activate(Group=..., Ports=...)` | registers the script and its port count |
| `@group`, `@param(NAME=LENGTH, ...)`, `@enum` | parameter UI in Catalog Builder |
| `CYLINDER(s, R=, H=, O=)` | cylinder along Z; `O` = inner radius (0 = solid) |
| other primitives | `BOX`, `CONE`, `TORUS`, `SPHERE`… (VERIFY each signature in the stock scripts) |
| `.translate((x,y,z))`, `.rotateX/Y/Z(deg)`, `.uniteWith(b)`, `.subtractFrom(b)`, `.erase()` | solid operations |
| `s.setPoint(position, direction[, angle])` | port definition |
| function name == file name | required |

Install & test: copy to `<shared content>\CPak Common\CustomScripts\` → `PLANTREGISTERCUSTOMSCRIPTS` →
restart → `(arxload "PnP3dACPAdapter")` → `(testacpscript "CPNOZZLE_STUB" "D" "114.3" "L" "150" "T" "6")`
→ Catalog Builder → new component using the script. Source: Autodesk developer-blog series *Custom
Python Scripts for AutoCAD Plant 3D* and AU 2011 handouts *Scripting Components for AutoCAD Plant 3D*.

Documentation reality: the primitive library is documented only in old AU handouts and by reading the
**stock scripts** shipped with the content (decompile the `.pyc` in the content folder or read the
supplied `.py`). That is how every call in `CPNOZZLE_STUB.py` was checked.

## Dynamo for Plant 3D: not available

- Autodesk support article *"Does AutoCAD Products support Dynamo?"* (Jun 2026): *"Dynamo cannot be run directly inside standard AutoCAD. If you need to use Dynamo for AutoCAD-based civil design workflows, use Dynamo for Civil 3D."* Plant 3D is AutoCAD-based and has no Dynamo integration (Dynamo hosts are Revit, Civil 3D, Advance Steel, FormIt and the standalone sandbox).
- Autodesk Community *Plant 3D Ideas*: "Dynamo for AutoCAD Plant 3D" is an **open idea** (request), not a product.
- Community experiments exist (third-party nodes reading Plant data), but nothing supported.

So: for "visual" or parametric work in Plant 3D use .NET + Python scripts; for P&ID/3D data use the SDK.

## Reading project data without Plant 3D

ADN LAB 6 shows a **standalone EXE** opening a project database with `PnPDatabase.Open(@"...\ProcessPower.dcf")`
(references `PnPDataObjects.dll` + `PnPSQLiteEngine.dll`). That pattern dates from older releases; whether it
works from a .NET 8 EXE outside Plant 3D 2026 is **VERIFY** (native dependencies).

Fallback for **read-only** reporting: `.dcf` files are SQLite → open a **copy** with Python's built-in
`sqlite3` and query tables. Unsupported by Autodesk, schema may change between releases, and **never write**
— the links between DWGs and rows would break. Fine for a nightly dashboard; not for changes.

## What Autodesk officially says

Autodesk support article *"Autodesk AutoCAD Plant 3D Customization and Automation Options for Engineering
Workflows"* (Oct 2026) lists: AutoLISP for AutoCAD-level automation; the **.NET API and Plant 3D APIs**
("capabilities depend on version and available interfaces"); AutoCAD scripts for batch; APS Automation API
(confirm compatibility for Plant operations); AutoLISP Extension for VS Code; ADN for advanced resources —
and that Autodesk support does not debug custom code. That matches this chapter: .NET for data, LISP/scripts
for drawings, no Dynamo.

Next: [11a — Plant 3D .NET API architecture](11a-plant3d-api-architecture.md).
