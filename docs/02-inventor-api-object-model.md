# 02 — The Inventor API object model (for engineers)

Every Inventor language — iLogic, VBA, C#, VB.NET, Python, PowerShell — talks to the **same COM object
model**. Learn it once. This page is the map; the API Help is the catalogue.

## Contents
- [The tree](#the-tree)
- [Units: cm and radians](#units-cm-and-radians)
- [Documents and their types](#documents-and-their-types)
- [Component definitions: where the model really is](#component-definitions-where-the-model-really-is)
- [Parameters](#parameters)
- [iProperties (property sets)](#iproperties-property-sets)
- [Assemblies: occurrences, proxies, matrices](#assemblies-occurrences-proxies-matrices)
- [Transient objects and transient geometry](#transient-objects-and-transient-geometry)
- [Sketches and features: the modelling pattern](#sketches-and-features-the-modelling-pattern)
- [Collections, enums, and COM habits](#collections-enums-and-com-habits)
- [Transactions (one undo step)](#transactions-one-undo-step)
- [Events](#events)
- [Finding your way: four techniques](#finding-your-way-four-techniques)

## The tree

```
Application                                   (ThisApplication in iLogic/VBA)
├── Documents  ─ Document (base)              (open documents, Open/Add)
│     ├── PartDocument
│     │     └── ComponentDefinition : PartComponentDefinition
│     │           ├── Parameters  (ModelParameters, UserParameters, ReferenceParameters, ...)
│     │           ├── Sketches / Sketches3D
│     │           ├── Features    (ExtrudeFeatures, RevolveFeatures, HoleFeatures, ...)
│     │           ├── WorkPlanes / WorkAxes / WorkPoints
│     │           ├── SurfaceBodies → Faces → Edges → Vertices
│     │           └── MassProperties, RangeBox
│     │     (SheetMetalComponentDefinition when SubType = sheet metal: FlatPattern, Thickness)
│     ├── AssemblyDocument
│     │     └── ComponentDefinition : AssemblyComponentDefinition
│     │           ├── Occurrences → ComponentOccurrence (.Definition, .Transformation, .SubOccurrences)
│     │           ├── Constraints, Joints
│     │           ├── Parameters
│     │           └── BOM → BOMViews → BOMView → BOMRows
│     ├── DrawingDocument
│     │     └── Sheets → Sheet → DrawingViews, PartsLists, DrawingDimensions, TitleBlock, Border
│     └── PresentationDocument
│     (every Document: PropertySets, File, ReferencedDocuments, Save/SaveAs/Close, DocumentType)
├── TransientGeometry      (Point, Point2d, Vector, Matrix, UnitVector — math objects, not in the model)
├── TransientObjects       (ObjectCollection, NameValueMap, TranslationContext, DataMedium)
├── ApplicationAddIns      (incl. translators: PDF, DXF, DWG, STEP, IGES...)
├── CommandManager         (ControlDefinitions, Pick, selection)
├── UserInterfaceManager   (Ribbons → RibbonTabs → RibbonPanels)
├── TransactionManager
├── FileManager / DesignProjectManager (projects, .ipj)
├── UnitsOfMeasure         (conversions, document units)
└── ...Events              (ApplicationEvents, ModelingEvents, FileUIEvents, ...)
```

Engineer analogy: **Application** = the factory; **Document** = a drawing/model file on the desk;
**ComponentDefinition** = the actual design inside; **Occurrence** = a placed instance of a part in an
assembly (the same part file placed 4 times = 4 occurrences, 1 definition).

## Units: cm and radians

The API **always** uses database units, whatever the document shows:

| Quantity | API unit | Conversion from what you think in |
|---|---|---|
| length | **cm** | mm ÷ 10 |
| angle | **radian** | deg × π/180 |
| mass | kg | — |
| area / volume | cm² / cm³ | mm² ÷ 100, mm³ ÷ 1000 |
| density | g/cm³ | (MassProperties) |

- `Parameter.Value` → database units. `Parameter.Expression` → text with units (`"120 mm"`).
- iLogic `Parameter("Length")` → **document units** (convenient, and a trap when you mix it with API calls).
- Safe pattern in code: one constant `const double MM = 0.1;` and multiply every mm value at the API boundary.
- `UnitsOfMeasure.ConvertUnits(value, kMillimeterLengthUnits, kCentimeterLengthUnits)` or `GetValueFromExpression("12 mm", kDefaultDisplayLengthUnits)` when you want Inventor to parse.

## Documents and their types

| Enum `DocumentTypeEnum` | Value | Interface | File |
|---|---|---|---|
| `kPartDocumentObject` | 12290 | `PartDocument` | .ipt |
| `kAssemblyDocumentObject` | 12291 | `AssemblyDocument` | .iam |
| `kDrawingDocumentObject` | 12292 | `DrawingDocument` | .idw, .dwg (Inventor DWG) |
| `kPresentationDocumentObject` | 12293 | `PresentationDocument` | .ipn |

(Values read from the interop DLL; useful in Python/PowerShell where enum names are not available.)

Sheet metal is a **part** with a different `SubType` GUID; check `doc.ComponentDefinition is SheetMetalComponentDefinition` (C#) or `TypeOf ... Is SheetMetalComponentDefinition` (VB).

Opening: `app.Documents.Open(path, visible)`. `visible = false` opens without a window — use it for batch work, **and always `Close`**. Invisible documents count towards memory.

```csharp
var doc = (PartDocument)app.Documents.Open(@"C:\Work\Bracket.ipt", false);
try { /* ... */ doc.Save(); }
finally { doc.Close(true); }   // true = skip save prompt
```

## Component definitions: where the model really is

- `PartDocument.ComponentDefinition` → `PartComponentDefinition`
- `AssemblyDocument.ComponentDefinition` → `AssemblyComponentDefinition`

C# trap (see [docs/13](13-errors-and-fixes.md#interop-type-surprises)): in the interop, `PartComponentDefinition`
and `AssemblyComponentDefinition` do **not** derive from `ComponentDefinition` in a way C# can convert
implicitly. Branch per document type:

```csharp
Parameters ps = doc switch
{
    PartDocument p     => p.ComponentDefinition.Parameters,
    AssemblyDocument a => a.ComponentDefinition.Parameters,
    _ => throw new InvalidOperationException("part or assembly only")
};
```

## Parameters

| Collection | Contains | Can create? |
|---|---|---|
| `Parameters` | all (iterate this to list everything) | — |
| `ModelParameters` | d0, d1… from sketches/features | no (created by features) |
| `UserParameters` | your named values (`Length`, `m`, `z`) | **yes** `AddByExpression`, `AddByValue` |
| `ReferenceParameters` | driven dimensions | no |
| `DerivedParameterTables` / `ParameterTables` | linked from other files / Excel | via `AddExcelTable` |

```csharp
UserParameters up = def.Parameters.UserParameters;
UserParameter m = up.AddByExpression("m", "2 mm", "mm");            // name, expression, units
up.AddByExpression("z", "40", "ul");                                  // "ul" = unitless
def.Parameters["Length"].Expression = "m * z + 10 mm";                // equations allowed
doc.Update();                                                         // rebuild
```

- Text and boolean user parameters: `AddByValue(name, "SS400", UnitsTypeEnum.kTextUnits)` / `kBooleanUnits`.
- Multi-value list: `Parameter.ExpressionList.SetExpressionList(...)` (iLogic: `MultiValue.SetList`).
- Units of an existing parameter: in C# it is the method `get_Units()` (COM property with a setter quirk) — `p.get_Units()`. In VB/iLogic/Python: `p.Units`.
- Export flag (to show as custom iProperty): `Parameter.ExposedAsProperty = true`.

## iProperties (property sets)

The *iProperties* dialog tabs map to four property sets:

| Dialog tab | Internal name (use this in code) | Typical properties |
|---|---|---|
| Summary | `Inventor Summary Information` | Title, Subject, Author, Keywords, Comments |
| Summary (company) | `Inventor Document Summary Information` | Company, Manager, Category |
| Project | `Design Tracking Properties` | Part Number, Description, Revision Number, Stock Number, Vendor, Designer, Project, Cost Center, Material (read-only text) |
| Custom | `Inventor User Defined Properties` | anything you add |

```csharp
PropertySet dtp = doc.PropertySets["Design Tracking Properties"];
dtp["Description"].Value = "PL 10 x 200 x 350, SS400";
PropertySet custom = doc.PropertySets["Inventor User Defined Properties"];
try { custom["ERP_No"].Value = "A-10023"; }
catch (ArgumentException) { custom.Add("A-10023", "ERP_No"); }    // Add(value, name) — value FIRST
```

- Accessing a missing property throws → `IProps.cs` in `code/inventor/csharp/Shared` wraps get/set safely.
- Material: set via `PartDocument.ActiveMaterial = asset` (from a material library), not the "Material" property text.
- Mass/area in iProperties are derived; read `ComponentDefinition.MassProperties` instead.

## Assemblies: occurrences, proxies, matrices

```csharp
AssemblyComponentDefinition ad = asm.ComponentDefinition;
Matrix pos = tg.CreateMatrix();                         // identity
pos.SetTranslation(tg.CreateVector(10, 0, 0));          // 10 cm along X
ComponentOccurrence occ = ad.Occurrences.Add(@"C:\Work\Leg.ipt", pos);
occ.Grounded = true;
```

- `Occurrences` = top level only. Deep walk: `occ.SubOccurrences` recursively, or `ad.Occurrences.AllLeafOccurrences`.
- `occ.Definition` → the part/sub-assembly definition; `occ.Definition.Document` → its document.
- **Proxies:** geometry seen *through* an occurrence (a face of a part placed in the assembly) is a `FaceProxy`. Constraints in an assembly need proxies: `occ.CreateGeometryProxy(face, out object proxy)`.
- **Matrices:** `Matrix.TransformBy(b)` = "this transform, **followed by** b". To rotate about the part origin and then move: `r = rot.Copy(); r.TransformBy(move);` (the bug in [docs/13](13-errors-and-fixes.md#geometry-and-modelling-errors)).
- Suppress / visibility: `occ.Suppress()`, `occ.Visible = false`; iLogic `Component.IsActive("Leg:1") = False`.
- Levels of Detail were replaced by **Model States** (2022+) — `ComponentDefinition.ModelStates`. Many old forum samples about LOD no longer apply.

## Transient objects and transient geometry

*Transient* = exists only in memory, not in the file.

| Object | Create with | Use |
|---|---|---|
| `Point2d`, `Point`, `Vector`, `UnitVector`, `Matrix` | `app.TransientGeometry.Create...` | inputs to sketches, placements |
| `ObjectCollection` | `app.TransientObjects.CreateObjectCollection()` | lists of features for patterns, points for splines |
| `NameValueMap` | `CreateNameValueMap()` | options for translators, DA arguments |
| `TranslationContext`, `DataMedium` | `CreateTranslationContext()`, `CreateDataMedium()` | file export |

## Sketches and features: the modelling pattern

Every generator follows the same four lines of thought — compare `03_Shaft_Generator.iLogicVb` and `02-SpurGearGenerator/Program.cs`:

1. **Plane:** `def.WorkPlanes[3]` (1 = YZ, 2 = XZ, 3 = XY) or a face.
2. **Sketch:** `def.Sketches.Add(plane)` → `SketchLines.AddByTwoPoints`, `SketchCircles.AddByCenterRadius`, `SketchArcs`, `SketchSplines.Add(points)`; add constraints/dimensions if the sketch must stay editable.
3. **Profile:** `sketch.Profiles.AddForSolid()`.
4. **Feature:** `def.Features.ExtrudeFeatures.CreateExtrudeDefinition(profile, PartFeatureOperationEnum.kJoinOperation)` → `SetDistanceExtent("20 mm", kPositiveExtentDirection)` → `ExtrudeFeatures.Add(definition)`.

```csharp
PlanarSketch sk = def.Sketches.Add(def.WorkPlanes[3]);
sk.SketchCircles.AddByCenterRadius(tg.CreatePoint2d(0, 0), 4.2);      // R 42 mm
Profile prof = sk.Profiles.AddForSolid();
ExtrudeDefinition ed = def.Features.ExtrudeFeatures.CreateExtrudeDefinition(prof, PartFeatureOperationEnum.kJoinOperation);
ed.SetDistanceExtent("b", PartFeatureExtentDirectionEnum.kPositiveExtentDirection);  // expression = user parameter b
ExtrudeFeature blank = def.Features.ExtrudeFeatures.Add(ed);
```

Extents accept **expressions**: pass `"b"` or `"20 mm"` and the feature stays linked to the parameter —
much better than passing a number.

## Collections, enums, and COM habits

| Habit | Why |
|---|---|
| Collections are **1-based**: `Item(1)` is the first | COM convention. `foreach` hides it; `for` loops must start at 1 |
| `doc.PropertySets["name"]` in C# = `.Item("name")` in Python/PowerShell | default indexer |
| Enums are numbers underneath (`kFileBrowseIOMechanism` = 13059) | in Python use `const()` helper; in PowerShell use the number |
| Read a collection once, then loop | each `.Count` / `.Item(i)` out-of-process is a round trip |
| Release objects in add-ins (`Marshal.ReleaseComObject` on `Deactivate`) | Inventor may hang on exit if references stay alive |
| Never store `Document` objects in static fields across commands | the user may close the document → `COMException` |

## Transactions (one undo step)

```csharp
Transaction tx = app.TransactionManager.StartTransaction((_Document)doc, "Create spur gear");
try { /* many API calls */ tx.End(); }
catch { tx.Abort(); throw; }
```

The user then sees **one** Undo entry instead of 300. `_Document` cast: `StartTransaction` expects the
underlying `_Document` interface — the cast is required in C#.

## Events

| Event object | From | Examples |
|---|---|---|
| `ApplicationEvents` | `app.ApplicationEvents` | `OnOpenDocument`, `OnSaveDocument` (Before/After via `EventTiming`), `OnActivateDocument`, `OnQuit` |
| `ModelingEvents` | `app.ModelingEvents` | `OnParameterChange`, `OnFeatureChange` |
| `UserInterfaceEvents` | `app.UserInterfaceManager.UserInterfaceEvents` | `OnResetRibbonInterface` (used by `07-MyFirstAddin`) |
| `ButtonDefinition.OnExecute` | your command | button clicked |
| `InteractionEvents` | `CommandManager.CreateInteractionEvents()` | custom picking, preview graphics |

Events belong in **add-ins** (in-process, lifetime = Inventor session). An EXE can subscribe too, but
it must keep running and every callback crosses processes. In iLogic use *Event Triggers* instead.

## Finding your way: four techniques

1. **Do it by hand, then inspect.** Create the feature in the UI, then in VBA *Watch* `ThisApplication.ActiveDocument.ComponentDefinition.Features` — expand and read properties.
2. **F12 / Object Browser** in Visual Studio on the interop: every member, every argument type.
3. **API Help → search the object → "Samples"**. Most objects have a VBA sample; it translates line by line.
4. **The object model diagram** (PDF in `C:\Users\Public\Documents\Autodesk\Inventor 2026\SDK\`, look for the *Inventor Object Model* chart; file name differs per release).
5. Forum search: `site:forums.autodesk.com inventor api <ObjectName>`.

Next: [03 — iLogic](03-ilogic.md).
