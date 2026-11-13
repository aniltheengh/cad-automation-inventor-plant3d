# 11c — Plant 3D code examples: connect, active document, components, properties

> **What you get.** Ten short examples, in the order you need them: connect → active document → loop over the
> components → read their properties → ports, connections, line numbers → CSV.
> Every block marked *From `File.cs`* is copied from the working project
> [`code/plant3d/csharp/Plant3DAddinStarter`](../code/plant3d/csharp/Plant3DAddinStarter/); a test keeps the
> copies identical to the code. All examples **read only**.
> **Honesty.** Compiled against stand-ins for the AutoCAD and Plant 3D assemblies, not run inside Plant 3D
> (see [tools](../tools/README.md)). Source tags **[T] [B] [S] [F]** as in [11a → Sources](11a-plant3d-api-architecture.md#sources).

Before this chapter: the API map in [11a](11a-plant3d-api-architecture.md), a loadable DLL from [11b](11b-plant3d-first-addin.md).

## Contents
- [How to run the examples](#how-to-run-the-examples)
- [The chain in one picture](#the-chain-in-one-picture)
- [Example 1 — Connect to the application](#example-1--connect-to-the-application)
- [Example 2 — The active document and its project part](#example-2--the-active-document-and-its-project-part)
- [Example 3 — Iterate through the components of a drawing](#example-3--iterate-through-the-components-of-a-drawing)
- [Example 4 — Extract the property data of one component](#example-4--extract-the-property-data-of-one-component)
- [Example 5 — The typed 3D object: class and ports](#example-5--the-typed-3d-object-class-and-ports)
- [Example 6 — From a part to its line number](#example-6--from-a-part-to-its-line-number)
- [Example 7 — Walk the connections](#example-7--walk-the-connections)
- [Example 8 — From database rows back to drawing objects](#example-8--from-database-rows-back-to-drawing-objects)
- [Example 9 — Write the result to CSV](#example-9--write-the-result-to-csv)
- [Example 10 — See the real class hierarchy](#example-10--see-the-real-class-hierarchy)
- [Orthographic and isometric drawings](#orthographic-and-isometric-drawings)
- [Common mistakes](#common-mistakes)
- [Sources](#sources)

## How to run the examples

1. Build the starter and load it: [11b, steps 6–8](11b-plant3d-first-addin.md#step-6--build), or the [project README](../code/plant3d/csharp/Plant3DAddinStarter/README.md).
2. Open a **copy** of a project, then a 3D model drawing (or a P&ID drawing).
3. Type a command:

| Command | Shows | Examples | Code |
|---|---|---|---|
| `P3DHELLO` | application, active document, project, project parts, loaded Plant 3D DLLs | 1, 2 | `HelloCommands.cs` |
| `P3DCOMPONENTS` | every Plant object of the drawing: counts per class + CSV with all properties | 3, 4, 9 | `ComponentCommands.cs`, `ComponentReader.cs` |
| `P3DINSPECT` | pick one object: classes, row id, line number, all properties, ports | 4, 5, 6 | same |
| `P3DWALK` | pick one 3D part: everything connected to it | 7 | same |
| `P3DAPIMAP` | the class hierarchy of the Plant 3D API on **your** installation → Markdown file | 10 | `ApiMapCommands.cs` |
| `P3DREGISTER`, `P3DUNREGISTER` | write / remove the demand-loading registry key | [11b, step 8](11b-plant3d-first-addin.md#step-8--register-the-dll-so-it-loads-by-itself) | `RegisterCommands.cs` |

Files go to `Documents\Plant3DAddinStarter\` — never into the project folder.

## The chain in one picture

```
AutoCAD    Application ── DocumentManager.MdiActiveDocument ── Document ──┬─ Database   (the DWG content)
                                                                          └─ Editor     (command line, picking)

Plant 3D   PlantApplication.CurrentProject ── PlantProject ── ProjectParts ── Project (one part: "Piping", "PnId"…)
                                                                                 └─ DataLinksManager ── PnPDatabase ── PnPTable ── PnPRow

Bridge     DataLinksManager:   ObjectId (object in the DWG)   ⇄   row id "PnPID" (row in the project database)
```

Every example is one walk along this chain. The helper class
[`PlantContext`](../code/plant3d/csharp/Plant3DAddinStarter/PlantContext.cs) walks it once and keeps the results:

| Property | Type | Is |
|---|---|---|
| `Doc`, `Db`, `Ed` | `Document`, `Database`, `Editor` | the active drawing |
| `Project` | `PlantProject` | the project that is current in Project Manager |
| `Part`, `PartName` | `Project`, `string` | the project part this drawing belongs to |
| `Dlm` | `DataLinksManager` | the bridge of that part |
| `Data` | `PnPDatabase` | the project database of that part |

## Example 1 — Connect to the application

There is nothing to connect to. An add-in runs **inside** Plant 3D, so the application is already there:

| API | "Connect" means | Process |
|---|---|---|
| Inventor from an EXE ([docs/05](05-csharp-vbnet-exe.md#connecting-to-inventor-from-net-8)) | find or start the program through COM | your EXE, outside |
| **AutoCAD / Plant 3D .NET** | use the static classes `Application` and `PlantApplication` | your DLL, inside `acad.exe` |

The Plant 3D .NET API cannot be used from a separate EXE — other routes: [Ways to automate Plant 3D](11-plant3d-sdk.md#ways-to-automate-plant-3d).

<!-- excerpt: HelloCommands.cs -->
*From `HelloCommands.cs` — the AutoCAD side:*

```csharp
Document? doc = AcApp.DocumentManager.MdiActiveDocument;
if (doc == null) return;                    // commands need a drawing window for their output
Editor ed = doc.Editor;

// ---- 1. the application: plain AutoCAD API, works in every AutoCAD-based product
ed.WriteMessage($"\n--- Application");
ed.WriteMessage($"\n  AutoCAD release : {AcApp.GetSystemVariable("ACADVER")}");
ed.WriteMessage($"\n  Registry key    : HKCU\\{HostApplicationServices.Current.UserRegistryProductRootKey}");
ed.WriteMessage($"\n  Program         : {Environment.ProcessPath}");
ed.WriteMessage($"\n  .NET runtime    : {RuntimeInformation.FrameworkDescription}");
```

<!-- excerpt: HelloCommands.cs -->
*From `HelloCommands.cs` — the Plant 3D side:*

```csharp
// PlantApplication is the Plant 3D counterpart of AutoCAD's Application class.
PlantProject project = PlantApplication.CurrentProject
    ?? throw new InvalidOperationException("No Plant 3D project is open (Project Manager).");
// ...
// A Plant project is a set of parts (P&ID, 3D piping, orthos, isos...), each with its own database.
ed.WriteMessage($"\n--- Project parts");
foreach (Project part in project.ProjectParts)
{
    int drawings = part.GetPnPDrawingFiles()?.Count ?? 0;
    ed.WriteMessage($"\n  {project.ProjectPartName(part),-10} {drawings,4} drawing(s)   {part.ProjectDirectory}");
}
```

| Line | Point |
|---|---|
| `AcApp` | alias written at the top of the file: `using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;` — the bare name `Application` clashes with Windows Forms and WPF |
| `MdiActiveDocument` | `null` when no drawing is open → test it |
| `UserRegistryProductRootKey` | tells you *which* product is running: `…\R25.1\ACAD-9117:409` is Plant 3D 2026, English ([11b](11b-plant3d-first-addin.md#way-b--demand-loading-registry-key)) |
| `PlantApplication.CurrentProject` | `null` when no project is open → test it |
| `PlantProject` | the whole project: `Name`, `ProjectFolderPath`, `ProjectParts` [T] [B] |
| `Project` | **one part** of it: `ProjectDirectory`, `DataLinksManager`, `GetPnPDrawingFiles()` [T] |

"Is this Plant 3D at all?" — in plain AutoCAD the Plant assemblies are missing, and the first method that uses
them fails to load. `Report.Run` turns that into one line: *Run this in AutoCAD Plant 3D, not in plain AutoCAD*
([11b, step 5](11b-plant3d-first-addin.md#step-5--add-the-first-command)).

`P3DHELLO` prints:

| Block | Content |
|---|---|
| Application | release (`ACADVER`), registry key, path of `acad.exe`, .NET runtime |
| Active document | name, DWG file |
| Plant project | name, folder, type of the active drawing |
| Project parts | per part: name, number of drawings, folder |
| Loaded Plant 3D assemblies | name, version, **folder** — the DLLs to reference in your own project |

## Example 2 — The active document and its project part

A Plant project has several parts, each with its own database and its own `DataLinksManager`.
Data of a drawing object is found only through the part that the drawing belongs to.

<!-- excerpt: PlantContext.cs -->
*From `PlantContext.cs`:*

```csharp
public static PlantContext FromActiveDrawing()
{
    // 1) the active document (null when no drawing is open)
    Document doc = AcApp.DocumentManager.MdiActiveDocument
        ?? throw new InvalidOperationException("No drawing is open.");

    // 2) the Plant project that is current in Project Manager (null when none is open)
    PlantProject project = PlantApplication.CurrentProject
        ?? throw new InvalidOperationException("No Plant 3D project is open (Project Manager).");

    // 3) which part of the project does this drawing belong to?  "PnID", "Piping", ...
    string docType = PnPProjectUtils.GetActiveDocumentType() ?? "";
    Project? part = FindPart(project, docType);
    // ...
    return new PlantContext(doc, project, project.ProjectPartName(part), part);
}
```

<!-- excerpt: PlantContext.cs -->
*and its constructor takes the last two steps of the chain:*

```csharp
Dlm = part.DataLinksManager;
Data = Dlm.GetPnPDatabase();
```

| Object | Is | You use it for |
|---|---|---|
| `Document` | one open drawing window | `Name`, `Editor`, `Database`; `LockDocument()` when your code is **not** a command (palette button) |
| `Database` | the content of the DWG | transactions, model space, objects |
| `Editor` | command line of that window | `WriteMessage`, `GetEntity`, `GetString` |
| `Project` (part) | P&ID / 3D piping / ortho / iso part of the project | `DataLinksManager`, drawings of that part |

Other ways to get the part, all seen in public code:

| Way | Code | Source |
|---|---|---|
| by name, when you know what you want | `project.ProjectParts["Piping"]`, `project.ProjectParts["PnId"]` | [T] |
| from any drawing database | `DataLinksManager.GetManager(db)` | [S] [F] |
| test a part against a database | `part.DataLinksManager.GetDrawingId(db)` — `-1` means "not a drawing of this part" | [F] |

Part names in a standard project: `PnId`, `Piping`, `Ortho`, `Iso` [T]. `P3DHELLO` lists yours.

## Example 3 — Iterate through the components of a drawing

The core loop. Model space holds everything: pipes, fittings, valves, joints, equipment, supports — and plain
lines, text and xrefs. `HasLinks` separates the two groups.

<!-- excerpt: ComponentReader.cs -->
*From `ComponentReader.cs`:*

```csharp
public static List<Component> ReadAll(PlantContext ctx)
{
    var result = new List<Component>();

    // Every read of a DWG goes through a transaction. The using block closes all opened objects.
    using (Transaction tr = ctx.Db.TransactionManager.StartTransaction())
    {
        // Model space is a block table record; looping over it gives the ObjectId of each entity.
        var modelSpace = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(ctx.Db), OpenMode.ForRead);

        foreach (ObjectId id in modelSpace)
        {
            // 1) Plant object or plain AutoCAD geometry? Only Plant objects have a row in the project database.
            if (!ctx.TryGetRowId(id, out int rowId)) continue;

            // 2) the DWG side: open the object to see its .NET class, layer and ports
            DBObject obj = tr.GetObject(id, OpenMode.ForRead);

            // 3) the data side: project class and all properties come from the project database
            string plantClass = ctx.Dlm.GetObjectClassname(id) ?? "";

            result.Add(new Component
            {
                Handle = id.Handle.ToString(),
                Layer = (obj as Entity)?.Layer ?? "",
                NetClass = obj.GetType().Name,
                Kind = Kind(obj),
                PlantClass = plantClass,
                ClassPath = ctx.ClassPath(plantClass),
                RowId = rowId,
                Ports = obj is Part part ? PortCount(part) : 0,
                Properties = ctx.Properties(rowId),
            });
        }
        tr.Commit();        // nothing was changed; Commit is simply the cheapest way to end a transaction
    }
    return result;
}
```

| Step | API | Point |
|---|---|---|
| open the drawing for reading | `Transaction` | the same pattern as in [docs/10](10-autocad-dotnet-basics.md#the-five-core-ideas) |
| loop | `foreach (ObjectId id in modelSpace)` | ids only — nothing is opened yet, so the loop is cheap |
| filter | `HasLinks(id)` → `FindAcPpRowId(id)` | no row id = not a Plant object [S] [T] |
| DWG side | `tr.GetObject(id, ForRead)` | .NET class, layer, handle, ports |
| data side | `GetObjectClassname(id)`, `GetAllProperties(rowId, true)` | project class and properties [S] [T] |
| keep | plain `Component` objects | DWG objects are dead after the transaction — copy what you need |

Scope: this loop sees **one drawing** — the active one. For the whole project without opening drawings, read the
tables instead (route A in [11a](11a-plant3d-api-architecture.md#routes-through-the-data); commands `P3DMTO`,
`P3DLINELIST` in [Plant3DDataTools](../code/plant3d/csharp/Plant3DDataTools/)).

`P3DCOMPONENTS` runs this loop, prints one line per project class with its count and .NET class, and writes the CSV of
[Example 9](#example-9--write-the-result-to-csv).

## Example 4 — Extract the property data of one component

Three questions per object: does it have a row? what are its properties? which class is it?

<!-- excerpt: PlantContext.cs -->
*From `PlantContext.cs` — the row id:*

```csharp
public bool TryGetRowId(ObjectId id, out int rowId)
{
    rowId = 0;
    try
    {
        if (!Dlm.HasLinks(id)) return false;
        rowId = Dlm.FindAcPpRowId(id);
    }
    catch (System.Exception)
    {
        return false;
    }
    return rowId > 0;
}
```

<!-- excerpt: PlantContext.cs -->
*the properties:*

```csharp
public Dictionary<string, string> Properties(int rowId)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    foreach (KeyValuePair<string, string> p in Dlm.GetAllProperties(rowId, true))
        if (p.Key != null) result[p.Key] = p.Value ?? "";
    return result;
}
```

<!-- excerpt: PlantContext.cs -->
*the class and its parents:*

```csharp
public string ClassPath(string className)
{
    var levels = new List<string>();
    string? current = className;
    while (!string.IsNullOrWhiteSpace(current) &&
           !string.Equals(current, "PnPBase", StringComparison.OrdinalIgnoreCase) &&
           levels.Count < 20)                               // guard against a circular definition
    {
        levels.Add(current);
        current = Data.Tables.Contains(current) ? Data.Tables[current].BaseTableName : null;
    }
    levels.Reverse();
    return string.Join(" > ", levels);
}
```

| Fact | Consequence |
|---|---|
| `GetAllProperties` returns name/value pairs, **all values as text** [T] | convert yourself; test for empty text first |
| property names come from the project class, including the ones your company added | never guess a name: run `P3DINSPECT` on a real object and copy it |
| the row id (`PnPID`) is permanent, the `ObjectId` is not | store row ids and handles, never `ObjectId`s ([three kinds of ids](11-plant3d-sdk.md#three-kinds-of-ids)) |
| a project class is a **table**; `BaseTableName` leads to its parent, the root is `PnPBase` [S] | `ClassPath` answers "is this any kind of valve?" without a list of valve classes |
| one value, typed, from the table | `Data.GetRow(rowId)[column]` returns an `object`; guard with `row.Table.Columns.Contains(column)` |

`P3DINSPECT` shows all of it for one picked object:

| Line | From |
|---|---|
| `.NET class` | `obj.GetType()` and its base types — the real inheritance chain |
| `ObjectARX class` | `id.ObjectClass.Name` |
| `Handle` | `id.Handle` |
| `Project class` | `GetObjectClassname` + `ClassPath` |
| `Row id (PnPID)` | `FindAcPpRowId` |
| `Line group` | relationship, [Example 6](#example-6--from-a-part-to-its-line-number) |
| properties | `GetAllProperties`, sorted by name |
| ports | [Example 5](#example-5--the-typed-3d-object-class-and-ports) |

Writing a property back is one call (`SetProperties`), but read [Writing data safely](11-plant3d-sdk.md#writing-data-safely) first.

## Example 5 — The typed 3D object: class and ports

A handful of .NET classes covers the whole 3D model. **What** a part is — elbow, gate valve, reducer — is data
(project class and properties), not a .NET class. The .NET class tells you what you can **ask** the object.

<!-- excerpt: ComponentReader.cs -->
*From `ComponentReader.cs`:*

```csharp
public static string Kind(DBObject obj) => obj switch
{
    Pipe => "pipe",
    PipeInlineAsset => "inline part (fitting, valve, instrument...)",
    Connector => "joint between two ports",
    Equipment => "equipment",
    Part => "other 3D part",
    _ => "P&ID or other Plant object",
};
```

<!-- excerpt: ComponentReader.cs -->
*Ports — where things connect:*

```csharp
var connections = new ConnectionManager();
foreach (Port port in part.GetPorts(PortType.All))
{
    // A Pair = one object + one of its ports = one side of a connection.
    var here = new Pair { ObjectId = part.ObjectId, Port = port };
    string state = "open end";
    if (connections.IsConnected(here))
    {
        // The other side of the joint. "is Pair" also filters an empty answer.
        state = connections.GetConnectedPairAt(here) is Pair there && !there.ObjectId.IsNull
            ? "connected to " + Label(ctx, there.ObjectId)
            : "connected";
    }
    lines.Add($"{port.Name,-6} ({port.Position.X:0.##}, {port.Position.Y:0.##}, {port.Position.Z:0.##})  {state}");
}
```

| API | Point | Source |
|---|---|---|
| `obj is Part part` | every 3D model object derives from `Part`; test before you cast | [T] |
| `GetPorts(PortType.All)` | a pipe has two ports, a tee three; each has `Name`, `Position`, `Direction` | [T] [B] |
| `Pair` | object + port = one **end** | [T] [B] |
| `IsConnected(pair)` | `false` = open end — a quick model check | [T] [B] |
| `GetConnectedPairAt(pair)` | the other end. Between two parts there is always a `Connector` (the joint) | [B] |
| needs | reference `PnP3dObjectsMgd.dll` ([11b, step 3](11b-plant3d-first-addin.md#step-3--reference-the-autocad-and-plant-3d-assemblies)) | [T] |

Pipe supports, steel and insulation have no entry in `Kind`: the public samples do not name their classes.
`P3DCOMPONENTS` still lists them, with their real .NET class in the `NetClass` column.

## Example 6 — From a part to its line number

In the 3D model the line number is not a property of the pipe. It is a row of its own (class `P3dLineGroup`),
linked to its parts by a **relationship** [S].

<!-- excerpt: PlantContext.cs -->
*From `PlantContext.cs`:*

```csharp
public int LineGroupRowId(int partRowId)
{
    try
    {
        foreach (int id in Dlm.GetRelatedRowIds("P3dLineGroupPartRelationship", "Part", partRowId, "LineGroup"))
            return id;                                      // a part belongs to one line group
    }
    catch (System.Exception)
    {
        // P&ID drawing, or a class that takes no part in this relationship
    }
    return 0;
}
```

- Read the call as: *in relationship X, my row plays the role `Part` — give me the rows that play `LineGroup`.*
- Swap the roles to go the other way: all parts of a line.
- More relationship names and roles: [11a](11a-plant3d-api-architecture.md#relationships). A P&ID line uses `PipeLineGroupRelationship`; see `P3DLINELIST` in [Plant3DDataTools](../code/plant3d/csharp/Plant3DDataTools/).
- `P3DINSPECT` prints the result as `Line group : row …, Tag '…', Number '…'` (columns that do not exist in your project stay empty).

## Example 7 — Walk the connections

<!-- excerpt: ComponentReader.cs -->
*From `ComponentReader.cs`:*

```csharp
public static List<ObjectId> Walk(ObjectId start, int max)
{
    var ids = new List<ObjectId>();
    ConnectionIterator walker = ConnectionIterator.NewIterator(start);
    for (; !walker.Done() && ids.Count < max; walker.Next())
        ids.Add(walker.ObjectId);
    return ids;
}
```

- `ConnectionIterator` moves from part to joint to part, starting at one object [T].
- Always give a loop like this an upper limit (`max`): a model can be large, and you do not control it.
- `P3DWALK` prints each stop as `project class [handle] Tag '…'`. Use it to trace a line or to see where a run ends.
- The order of the stops is the iterator's — the public material does not document it (**VERIFY** on a small model before you rely on it).

## Example 8 — From database rows back to drawing objects

The other direction: you have rows (a report, a filter) and want the objects in the drawing.
Not a command of the starter — a method to copy. Pattern from ADN LAB 3 [T].

```csharp
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;

namespace Plant3DAddinStarter
{
    internal static class RowsToObjects
    {
        /// <summary>All objects of one project class ("Valve", "Pipe"...) that are in the ACTIVE drawing.</summary>
        public static List<ObjectId> ObjectsOfClass(PlantContext ctx, string className)
        {
            var result = new List<ObjectId>();
            if (!ctx.Data.Tables.Contains(className)) return result;       // no such class in this project part

            foreach (PnPRow row in ctx.Data.Tables[className].Select())    // one row = one object of the project
            {
                // One row can have several DWG objects (a P&ID symbol and its annotation).
                foreach (PpObjectId plantId in ctx.Dlm.FindAcPpObjectIds(row.RowId))
                {
                    ObjectId id = ctx.Dlm.MakeAcDbObjectId(plantId);       // back to an AutoCAD ObjectId
                    if (!id.IsNull && id.IsValid && id.Database == ctx.Db) // keep what is in the active drawing
                        result.Add(id);
                }
            }
            return result;
        }
    }
}
```

- `PnPTable.Select()` returns the rows of the **whole project part**, not of one drawing — hence the last test.
- What `MakeAcDbObjectId` returns for an object in a drawing that is not open is not documented in the public material (**VERIFY**); the three tests in the `if` cover a null, an invalid and a foreign id.
- With the ids: open each in a transaction, or hand them to the editor (`ctx.Ed.SetImpliedSelection(result.ToArray())`) to select them.
- The three ids and their conversions: [chapter 11](11-plant3d-sdk.md#three-kinds-of-ids).

## Example 9 — Write the result to CSV

Different classes have different properties. So: collect every property name that occurs, sort, and write one
column per name. Objects without that property get an empty cell.

<!-- excerpt: ComponentCommands.cs -->
*From `ComponentCommands.cs`:*

```csharp
// ---- CSV: fixed columns first, then every property name that occurs, in alphabetical order
var names = new SortedSet<string>(components.SelectMany(c => c.Properties.Keys), StringComparer.OrdinalIgnoreCase);
var sb = new StringBuilder();
sb.AppendLine(Report.CsvLine(new[] { "Handle", "Layer", "NetClass", "Kind", "PlantClass", "ClassPath", "PnPID", "Ports" }.Concat(names)));
foreach (Component c in components)
{
    var cells = new List<string?> { c.Handle, c.Layer, c.NetClass, c.Kind, c.PlantClass, c.ClassPath, c.RowId.ToString(), c.Ports.ToString() };
    foreach (string name in names) cells.Add(c.Properties.TryGetValue(name, out string? value) ? value : "");
    sb.AppendLine(Report.CsvLine(cells));
}
string path = Report.OutputFile(ctx.Doc.Name, "components", "csv");
Report.WriteText(path, sb.ToString());
```

<!-- excerpt: Report.cs -->
*From `Report.cs`:*

```csharp
/// <summary>One CSV field: always quoted, quotes doubled.</summary>
public static string Csv(string? value) => "\"" + (value ?? "").Replace("\"", "\"\"") + "\"";

public static string CsvLine(IEnumerable<string?> values) => string.Join(",", values.Select(Csv));

/// <summary>UTF-8 with BOM: Excel then shows Japanese and other non-ASCII text correctly.</summary>
public static void WriteText(string path, string text) => File.WriteAllText(path, text, new UTF8Encoding(true));
```

| Column | Content |
|---|---|
| `Handle` | id of the object inside its DWG — stable between sessions |
| `Layer` | AutoCAD layer |
| `NetClass`, `Kind` | .NET class and what it means |
| `PlantClass`, `ClassPath` | project class, alone and with its parents |
| `PnPID` | row id in the project database |
| `Ports` | number of ports (0 for P&ID objects) |
| the rest | one column per property name |

- Quote every field: sizes such as `4"` and descriptions with commas break a naive CSV.
- A real Excel file with formats: ClosedXML, as in [docs/05](05-csharp-vbnet-exe.md#excel-without-excel-closedxml) — the same library works here.

## Example 10 — See the real class hierarchy

Autodesk publishes no online class reference for the Plant SDK. `P3DAPIMAP` builds one from the assemblies that
**your** Plant 3D has loaded — with .NET reflection, so it is always right for your release:

| Output | Use |
|---|---|
| assembly → namespace → class `: base class` | find the class and the DLL for a job |
| inheritance tree of all drawing-object classes | the full version of the diagrams in [11a](11a-plant3d-api-architecture.md#class-hierarchy-2-3d-model-entities) — including supports and steel |
| optional: all public members of one class you name | check a method name or signature before you write code |

The file is `Documents\Plant3DAddinStarter\Plant3D_apimap_<date>.md`. Code:
[`ApiMapCommands.cs`](../code/plant3d/csharp/Plant3DAddinStarter/ApiMapCommands.cs) — it uses no Plant 3D type, only `System.Reflection`.

Use it to settle every **VERIFY** in chapters 11–11c.

## Orthographic and isometric drawings

| Drawing type | What model space holds | Read plant data from |
|---|---|---|
| 3D model | Plant objects (`Part` and its subclasses) | the drawing itself — Examples 3–7 |
| P&ID | Plant objects (`Asset`, `LineSegment`…) | the drawing itself — Examples 3–4 |
| Orthographic | generated 2D views; a forum report says these objects carry no row ids [F] | the **3D model** drawing, or the `Piping` part by rows (Example 8, route A) |
| Isometric | generated by Isogen | the `Piping` part by rows; isometrics: [chapter 11](11-plant3d-sdk.md#isometrics-isogen-and-pcf) |

So `P3DCOMPONENTS` in an ortho drawing reports *No Plant objects* — that is expected. From an ortho command, get the
model data with `PlantContext.FindPart(project, "Piping")` and its `DataLinksManager`.

## Common mistakes

| Mistake | Result | Do instead |
|---|---|---|
| keep an `ObjectId` in a file or a static field | wrong object next session | handle + drawing, or the row id |
| ask the P&ID part about a 3D object (or the reverse) | no row, or an exception | the part of the active drawing (Example 2) |
| call `FindAcPpRowId` on every model-space object | exceptions on lines, text, xrefs | `HasLinks` first (Example 4) |
| hard-code property or column names | works in one project only | `P3DINSPECT`; `Columns.Contains` before reading |
| use a DWG object after its transaction ended | crash | copy the values inside the transaction (Example 3) |
| keep `PlantContext` in a static field | stale after the user switches project or drawing | build it at the start of every command |
| loop over one drawing for a project-wide report | you get one drawing | read the tables (route A) |
| unlimited connection walk | very long run on a big model | a `max` (Example 7) |
| test on the live project | a bug costs real work | a **copy** of the project |

## Sources

Tags and links: [11a → Sources](11a-plant3d-api-architecture.md#sources). The ones this chapter leans on most:

| Tag | Used for |
|---|---|
| [T] | ADN [Plant3DTraining](https://github.com/ADN-DevTech/Plant3DTraining): LAB 3 (ids, properties), LAB 4 (ports, `ConnectionManager`, `ConnectionIterator`) |
| [B] | Autodesk Developer Blog — [pipe port connections](https://blog.autodesk.io/retrieving-iso-arrow-symbols-and-pipe-port-connections/) (`GetConnectedPairAt`, `Port.Direction`) |
| [S] | [speckle-sharp-connectors](https://github.com/specklesystems/speckle-sharp-connectors), `Converters/Plant3d`: `HasLinks`, `GetObjectClassname`, `BaseTableName`, `P3dLineGroupPartRelationship`, `GetActiveDocumentType` |
| [F] | Autodesk Community — [plant data in ortho drawings](https://forums.autodesk.com/t5/net-forum/accessing-plant-3d-support-properties-in-ortho-drawings-via-net/td-p/13823041), [drawing type of a database](https://forums.autodesk.com/t5/autocad-plant-3d-forum/api-how-to-read-and-change-drawing-properties-defined-at-the/td-p/12914063) |

Next: [12 — AutoLISP, scripts, Core Console, Python](12-autolisp-scripts-coreconsole-python.md).
