# 10 — AutoCAD .NET basics (the foundation for Plant 3D)

**Plant 3D *is* AutoCAD** with extra object types, a project database and the Plant SDK on top. Every
Plant 3D plug-in is first an AutoCAD .NET plug-in. Learn this layer with plain AutoCAD (or the Plant 3D
trial), then add the Plant API ([docs/11](11-plant3d-sdk.md)).

## Contents
- [Inventor API vs. AutoCAD API: different worlds](#inventor-api-vs-autocad-api-different-worlds)
- [Project setup with the official NuGet package](#project-setup-with-the-official-nuget-package)
- [The five core ideas](#the-five-core-ideas)
- [A command, line by line](#a-command-line-by-line)
- [Reading user input](#reading-user-input)
- [Block attributes: the AttributeTools project](#block-attributes-the-attributetools-project)
- [Working on DWGs that are not open](#working-on-dwgs-that-are-not-open)
- [Loading: NETLOAD, autoload bundles, SECURELOAD](#loading-netload-autoload-bundles-secureload)
- [Debugging AutoCAD plug-ins](#debugging-autocad-plug-ins)
- [Japanese and multilingual tips](#japanese-and-multilingual-tips)

## Inventor API vs. AutoCAD API: different worlds

| | Inventor | AutoCAD |
|---|---|---|
| Technology | **COM** (interop DLL) | **native .NET** wrappers (`acdbmgd`, `acmgd`, `accoremgd`) + COM (ActiveX) |
| Entry point | `Application` object | static `Application.DocumentManager`, `[CommandMethod]` attributes |
| Data access | object properties directly | open objects inside a **Transaction** by `ObjectId` |
| Units | cm/rad internally | drawing units (whatever the DWG uses — usually mm in plant work) |
| Out-of-process | normal (EXE) | possible via COM only; **.NET API is in-process only** |
| Language choices | iLogic, VBA, .NET, Python… | AutoLISP, .scr, .NET, VBA (separate), COM from Python, ObjectARX (C++) |

## Project setup with the official NuGet package

Autodesk publishes reference packages on nuget.org:

| Package | Contains | Use for |
|---|---|---|
| `AutoCAD.NET.Core` | `accoremgd`, `acdbmgd` | commands that also run in **Core Console** (no UI) |
| `AutoCAD.NET` | + `acmgd` (UI, ribbon, palettes) | full desktop plug-ins |
| `AutoCAD.NET.Model` | `acdbmgd` only | database-only libraries |

| Package version | AutoCAD | Target |
|---|---|---|
| 25.0.x | 2025 | net8.0 |
| **25.1.0** | 2026 | net8.0 — **used in this repo** |
| 25.1.1 | 2026.1.2 update and later | net10.0 only |
| 26.0.0 | 2027 | net10.0 |

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-windows</TargetFramework>
    <PlatformTarget>x64</PlatformTarget>
    <OutputType>Library</OutputType>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="AutoCAD.NET.Core" Version="25.1.0" ExcludeAssets="runtime" />
  </ItemGroup>
</Project>
```

`ExcludeAssets="runtime"` = compile against the DLLs but **never copy** them to your output (AutoCAD
loads its own; a copied `acdbmgd.dll` next to your DLL causes load failures). Alternative without NuGet:
reference `C:\Program Files\Autodesk\AutoCAD 2026\acdbmgd.dll` etc. with *Copy Local = No*. For Plant 3D the
AutoCAD DLLs are in the same AutoCAD 2026 install folder (Plant 3D is installed *into* AutoCAD).

## The five core ideas

1. **Database** (`Database`) = the DWG content. **Document** (`Document`) = an open drawing window with an `Editor` (command line) and a `Database`.
2. **ObjectId** = a handle to an object *in the current session*. **Handle** = permanent ID saved in the DWG (use for CSV round trips).
3. **Transaction**: `db.TransactionManager.StartTransaction()` → `tr.GetObject(id, OpenMode.ForRead/ForWrite)` → change → `tr.Commit()`. Not committed = rolled back. Always `using`.
4. **Symbol tables**: `BlockTable` (block definitions, incl. `*Model_Space`, `*Paper_Space`), `LayerTable`, `TextStyleTable`… Each record is a `...TableRecord`.
5. **Commands** are methods with `[CommandMethod("NAME")]` in a class; `[assembly: CommandClass(typeof(...))]` makes loading faster.

## A command, line by line

```csharp
using Autodesk.AutoCAD.ApplicationServices;        // Document
using Autodesk.AutoCAD.ApplicationServices.Core;   // Application: Core = works in Core Console too
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(Demo.Commands))]
namespace Demo;

public class Commands
{
    [CommandMethod("COUNTBLOCKS")]
    public void CountBlocks()
    {
        Document doc = Application.DocumentManager.MdiActiveDocument;
        Database db = doc.Database;
        var counts = new Dictionary<string, int>();
        using (Transaction tr = db.TransactionManager.StartTransaction())
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            var ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);
            foreach (ObjectId id in ms)
            {
                if (id.ObjectClass.DxfName != "INSERT") continue;              // cheap filter, no open
                var br = (BlockReference)tr.GetObject(id, OpenMode.ForRead);
                string name = br.IsDynamicBlock
                    ? ((BlockTableRecord)tr.GetObject(br.DynamicBlockTableRecord, OpenMode.ForRead)).Name
                    : br.Name;                                                  // dynamic blocks are "*U12" otherwise
                counts[name] = counts.GetValueOrDefault(name) + 1;
            }
            tr.Commit();
        }
        foreach (var kv in counts.OrderBy(k => k.Key))
            doc.Editor.WriteMessage($"\n{kv.Key,-30} {kv.Value}");
    }
}
```

Add an entity (e.g. a circle at 0,0 with R50 on layer `P-NOZZLE`):

```csharp
using var tr = db.TransactionManager.StartTransaction();
var ms = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForWrite);
var c = new Circle(Point3d.Origin, Vector3d.ZAxis, 50.0) { Layer = "P-NOZZLE" };   // layer must exist
ms.AppendEntity(c);
tr.AddNewlyCreatedDBObject(c, true);
tr.Commit();
```

## Reading user input

| Need | Method |
|---|---|
| text | `ed.GetString(new PromptStringOptions("\nTag: ") { AllowSpaces = true })` |
| number | `ed.GetDouble`, `ed.GetInteger` |
| point | `ed.GetPoint("\nPick point: ")` |
| one entity | `ed.GetEntity(new PromptEntityOptions("\nPick pipe: "))` |
| selection set with filter | `ed.GetSelection(new SelectionFilter(new[] { new TypedValue((int)DxfCode.Start, "INSERT") }))` |
| all objects of a type without picking | `ed.SelectAll(filter)` |
| keywords | `PromptKeywordOptions` with `Keywords.Add("Yes")` |

Always check `result.Status == PromptStatus.OK` (user may press Esc).

## Block attributes: the AttributeTools project

`code/autocad/csharp/AttributeTools/AttributeCommands.cs` — useful on every Plant 3D project for title
blocks, equipment tags on general-arrangement drawings, and valve labels:

| Command | Detail |
|---|---|
| `ATTEXPORT` | walks model + all layouts, every `BlockReference.AttributeCollection` → CSV `Handle,Block,Tag,Value,Layout` (UTF-8 **with BOM** — Excel then shows Japanese correctly) |
| `ATTIMPORT` | CSV `Handle,Tag,NewValue` → `db.TryGetObjectId(new Handle(h), out id)` → update only if changed |
| `ATTREPLACE` | set one tag on every insert of a block (dynamic blocks matched by effective name) |

Typical workflow: `ATTEXPORT` on the title-block drawings → fix revisions/dates/titles in Excel → save as
CSV (Handle, Tag, NewValue) → `ATTIMPORT`. With Core Console the same DLL runs on 200 DWGs unattended
([docs/12](12-autolisp-scripts-coreconsole-python.md#core-console)).

Attribute gotchas:
- After changing a block **definition** attribute, existing inserts do not update → `ATTSYNC` (or code: re-create `AttributeReference`s).
- Multiline attributes: `att.IsMTextAttribute` → `att.MTextAttribute.Contents`.
- Field-driven attributes (`%<\AcVar ...>%`) show evaluated text in `TextString`; writing replaces the field.
- `att.AdjustAlignment(db)` after changing text of aligned attributes, otherwise position may jump on regen.

## Working on DWGs that are not open

Side database — fast, no window:

```csharp
using var side = new Database(false, true);
side.ReadDwgFile(path, FileOpenMode.OpenForReadAndAllShare, true, "");
using (var tr = side.TransactionManager.StartTransaction()) { /* ... */ tr.Commit(); }
side.SaveAs(path, DwgVersion.Current);
```

- Set `HostApplicationServices.WorkingDatabase = side` temporarily if you use APIs that depend on it (attribute alignment, text).
- Plant 3D objects in a side database: the Plant object enablers must be loaded (they are in Plant 3D; not in plain AutoCAD → proxies).
- For many files: side databases inside one AutoCAD session, or Core Console per file (robust, parallel).

## Loading: NETLOAD, autoload bundles, SECURELOAD

| Method | When |
|---|---|
| `NETLOAD` → pick DLL | development |
| `(command "NETLOAD" "C:/Tools/AttributeTools.dll")` in `acaddoc.lsp` | quick team setup |
| **Autoloader `.bundle`** | proper deployment (below) |
| Registry demand-loading | legacy; avoid |

Autoloader bundle:

```
AttributeTools.bundle\
  PackageContents.xml
  Contents\AttributeTools.dll
```

```xml
<ApplicationPackage SchemaVersion="1.0" AutodeskProduct="AutoCAD" Name="AttributeTools"
                    AppVersion="1.0.0" ProductCode="{NEW-GUID}" UpgradeCode="{NEW-GUID}">
  <Components>
    <RuntimeRequirements OS="Win64" Platform="AutoCAD*" SeriesMin="R25.0" SeriesMax="R25.1" />
    <ComponentEntry AppName="AttributeTools" ModuleName="./Contents/AttributeTools.dll"
                    LoadOnCommandInvocation="True" LoadOnAutoCADStartup="False">
      <Commands GroupName="ATTRTOOLS">
        <Command Global="ATTEXPORT" Local="ATTEXPORT" />
        <Command Global="ATTIMPORT" Local="ATTIMPORT" />
        <Command Global="ATTREPLACE" Local="ATTREPLACE" />
      </Commands>
    </ComponentEntry>
  </Components>
</ApplicationPackage>
```

- `SeriesMin/SeriesMax`: R25.0 = AutoCAD 2025, R25.1 = 2026 (2027 = R26.0). AutoCAD 2025+ requires these for .NET 8 bundles, so a .NET Framework bundle for 2024 and a .NET 8 bundle for 2025/26 can coexist.
- `Platform="AutoCAD*"` = all AutoCAD-based products (Plant 3D included). Use `PLNT3D` to restrict to Plant 3D (VERIFY the exact product code string in your PackageContents examples).
- Bundle folders in **2026**: `%APPDATA%\Autodesk\ApplicationPlugins\` (per user) or `C:\Program Files\Autodesk\ApplicationPlugins\` (all users). AutoCAD 2026 made the old `C:\ProgramData\Autodesk\ApplicationPlugins` location **obsolete** for security.
- **SECURELOAD** (default 1): AutoCAD only loads code from *trusted locations* without asking. Add your folder to `TRUSTEDPATHS` (*Options → Files → Trusted Locations*) — the ApplicationPlugins folders are trusted by default. Never set `SECURELOAD 0` company-wide.

## Debugging AutoCAD plug-ins

1. Project → *Debug properties* → *Executable* `C:\Program Files\Autodesk\AutoCAD 2026\acad.exe`. For Plant 3D add command-line arguments: `/product PLNT3D /language en-US` (that is how the Plant 3D desktop shortcut starts AutoCAD — check the shortcut's *Target* field on your PC).
2. F5 → AutoCAD starts → `NETLOAD` your `bin\Debug\net8.0-windows\*.dll` → run the command → breakpoints hit.
3. Unloading a .NET DLL is impossible → restart AutoCAD after each rebuild (or use the community *Hot reload* tricks — not covered).
4. `doc.Editor.WriteMessage` is your `Console.WriteLine`. For Core Console runs it goes to stdout/log.

## Japanese and multilingual tips

- Use **global command names** with `_` in scripts and LISP (`_.LINE`) — works in Japanese AutoCAD.
- Layer/block names in Japanese are fine in .NET (UTF-16). In CSV use UTF-8 **with BOM** for Excel.
- `.scr` and `.lsp` files: save as UTF-8? AutoCAD's LISP reader historically expects the system code page (Shift-JIS on Japanese Windows) for non-ASCII string literals — VERIFY with your version; keep non-ASCII text out of scripts or test first.

Next: [11 — Plant 3D SDK](11-plant3d-sdk.md).
