# 03 — iLogic: rules, forms, external rules, configurators

iLogic is **VB.NET running inside Inventor**, plus a library of shortcut objects (`Parameter`,
`iProperties`, `Feature`, `Component`, `GoExcel`, ...). It is the first automation tool every Inventor
engineer should master: zero install, and the rule can live inside the model it controls.

## Contents
- [Mental model](#mental-model)
- [Rule types](#rule-types)
- [The iLogic objects you use every day](#the-ilogic-objects-you-use-every-day)
- [Parameters inside rules: the units trap](#parameters-inside-rules-the-units-trap)
- [Your first configurator, step by step](#your-first-configurator-step-by-step)
- [Forms](#forms)
- [External rules and a shared rule library](#external-rules-and-a-shared-rule-library)
- [Event triggers](#event-triggers)
- [Excel-driven design (GoExcel)](#excel-driven-design-goexcel)
- [Assemblies: components, suppression, model states](#assemblies-components-suppression-model-states)
- [Raw Inventor API from iLogic](#raw-inventor-api-from-ilogic)
- [Debugging iLogic](#debugging-ilogic)
- [Calling iLogic from C# and C# from iLogic](#calling-ilogic-from-c-and-c-from-ilogic)
- [The 10 rules in this repo](#the-10-rules-in-this-repo)
- [When to stop using iLogic](#when-to-stop-using-ilogic)

## Mental model

```
Template part / assembly (.ipt/.iam)
 ├── User parameters (inputs)    L, W, HeadType, P, S ...
 ├── Model (sketches, features)  driven by parameters / expressions
 ├── iLogic rules (logic)        IF HeadType = ... THEN suppress ..., compute t_req, set iProperties
 └── iLogic form (UI)            shows chosen parameters as text boxes, drop-downs, check boxes
```

- An **internal rule** runs automatically when a parameter it *reads by name* changes (unless you turn that off with *Don't run automatically*). That is the magic: change `HeadType` on the form → the rule fires → features switch.
- Parameters referenced as `Parameter("x")` or bare `x` (blue text in the editor) create that dependency. Bare names are convenient but make the rule fire on every change of that parameter.

## Rule types

| Type | Stored | Runs | Typical use |
|---|---|---|---|
| **Internal rule** | inside the document | manually, on parameter change, on triggers | configurator logic of one template |
| **External rule** | `.iLogicVb`/`.vb`/`.txt` file in a folder | manually from *iLogic Browser → External Rules*, triggers, other rules | company-wide tools: export, numbering, checks |
| **Global form** | `.xml` in the external rule folder (*Global Forms*) | from the iLogic Browser | buttons for external rules |
| **Event trigger** | assignment in the document (or in *This Document* / *All Documents* templates) | on events | before save checks, after open updates |

## The iLogic objects you use every day

| Object | Example | Notes |
|---|---|---|
| `Parameter` | `Parameter("Length") = 150` ; `Parameter("Part1:1", "d0") = 20` | value in **document units**; second form reaches into a component |
| `iProperties` | `iProperties.Value("Project", "Part Number")` ; `iProperties.Material = "Steel"` | tab name: Summary, Project, Custom, Status |
| `ThisDoc` | `ThisDoc.Path`, `ThisDoc.FileName(False)`, `ThisDoc.PathAndFileName(True)`, `ThisDoc.Document` | `FileName(False)` = without extension |
| `ThisApplication` | `ThisApplication.ActiveDocument` | the full Inventor `Application` → everything in [docs/02](02-inventor-api-object-model.md) |
| `Feature` | `Feature.IsActive("Fillet1") = False` | suppress/unsuppress by browser name |
| `Component` | `Component.IsActive("Leg:1") = False`, `Component.Visible(...)` | in assemblies |
| `MultiValue` | `MultiValue.SetList("HeadType", "A", "B")`, `MultiValue.List("Size") = list` | drop-down options |
| `GoExcel` | `GoExcel.FindRow(file, sheet, "Size", "=", "DN50")`, `GoExcel.CurrentRowValue("OD")` | Excel tables |
| `iLogicVb` | `iLogicVb.RunRule("Rule0")`, `iLogicVb.RunExternalRule("Export")`, `iLogicVb.UpdateWhenDone = True` | rule control |
| `InventorVb` | `InventorVb.DocumentUpdate()` | update now |
| `iLogicForm` | `iLogicForm.Show("Tank")`, `iLogicForm.ShowGlobal("Export")` | open forms from code |
| `SharedVariable` | `SharedVariable("Job") = "P-2411"` | pass values between rules in a session |
| `Logger` | `Logger.Info`, `Logger.Warn`, `Logger.Error` | shows in the *iLogic Log* window (View → User Interface → iLogic Log) |
| `MessageBox`, `InputBox`, `InputListBox` | `InputListBox("Pick", MultiValue.List("Size"), Size, "Title", "Sizes")` | quick UI |
| `Measure` | `Measure.MinimumDistance("Face1", "Face2")` | needs named entities |
| `ThisAssembly` / `Components.Add` | add/replace components, constraints (iLogic *assembly* API, 2019+) | VERIFY exact snippet signatures in *System Snippets → Components* |

Find more: the **System Snippets** tree in the rule editor. Double-click inserts a correct call —
faster and safer than typing from memory.

## Parameters inside rules: the units trap

```vb
' Document units are mm:
Parameter("Length") = 150              ' -> 150 mm  (iLogic converts)
Dim L As Double = Parameter("Length")  ' -> 150

' Raw API is ALWAYS cm:
Dim p As Inventor.Parameter = ThisDoc.Document.ComponentDefinition.Parameters("Length")
Dim v As Double = p.Value              ' -> 15   (cm!)
p.Expression = "150 mm"                ' safest: units in text
```

Rule of thumb: **iLogic shortcuts → document units; anything from `ThisApplication` / `ThisDoc.Document`
→ cm/rad.** Mixing them is the most common iLogic bug after typos.

Other quirks:
- Text parameters: `Parameter("HeadType")` returns `Object` → `CStr(...)` before `Select Case`.
- Boolean parameters: `CBool(Parameter("HasShelf"))`.
- After changing parameters that drive geometry, set `iLogicVb.UpdateWhenDone = True` (update once at end) instead of `InventorVb.DocumentUpdate()` in a loop.
- `RuleParametersOutput()` pushes parameter values to the model immediately (when later code needs the updated geometry in the same rule).

## Your first configurator, step by step

Using `02_Tank_Configurator.iLogicVb` (vertical tank, ASME VIII-1 thickness check):

1. **Model a tank part** with a revolved shell + three alternative head features named in the browser `Head_Ellipsoidal`, `Head_Torispherical`, `Head_Hemispherical`, and a nozzle circular pattern `Nozzle_Pattern`.
2. **User parameters** (*Manage → Parameters → Add Numeric / Add Text*):

   | Name | Units | Example | Meaning |
   |---|---|---|---|
   | `D_i` | mm | 1200 | inside diameter |
   | `L_ss` | mm | 2400 | straight shell |
   | `P` | ul | 1.0 | design pressure MPa |
   | `S` | ul | 138 | allowable stress MPa |
   | `E` | ul | 0.85 | joint efficiency |
   | `CA` | mm | 1.5 | corrosion allowance |
   | `t_n` | mm | 10 | chosen plate |
   | `HeadType` | Text | 2:1 Ellipsoidal | multi-value |
   | `N_Nozzles` | ul | 4 | nozzles |

3. **Add rule** → paste the file. What it does:
   - `MultiValue.SetList("HeadType", ...)` → drop-down choices.
   - Shell: t = P·R / (S·E − 0.6·P) + CA (UG-27(c)(1)).
   - Head: 2:1 ellipsoidal t = P·D / (2·S·E − 0.2·P); torispherical (L = D, r = 0.06 L) t = 0.885·P·L / (S·E − 0.1·P); hemispherical t = P·L / (2·S·E − 0.2·P), L = R (UG-32).
   - Suppresses the two heads not chosen (`Feature.IsActive`), toggles the nozzle pattern.
   - Capacity in litres → custom iProperty `Capacity_L`; Description built from inputs.
   - Warns if `t_n` is below the requirement.
4. **Form** → see [Forms](#forms). Drag `HeadType`, `D_i`, `L_ss`, `P`, `t_n`, `N_Nozzles` in.
5. **Save as template** (e.g. `Templates\Tank.ipt`). New tank = *Copy Design* or *Save As*, open, change the form.

Engineering caution: this is a sizing aid. The code says it too: formulas are inside-dimension forms, no
external pressure, no nozzle reinforcement, no forming thinning — the companion repo
*pressure-vessel-fea-inventor-nastran* covers those.

## Forms

*iLogic Browser → Forms tab → right-click → Add Form.*

- Drag parameters, iProperties, rules from the left into the form tree. Rules appear as **buttons**.
- Each parameter control: text box, slider, drop-down (multi-value), check box (boolean), image, picture-by-value.
- *Behaviour*: *OK/Cancel* vs. *Done*; *Modal* or not; predefined *Apply* updates live.
- **Global forms** live in the external rules folder (shared XML) → company toolbox with buttons for external rules.
- Forms cannot do everything (no grid editors, no validation code beyond rules). For that: WinForms in an add-in (`07-MyFirstAddin/Commands/ParameterEditorCommand.cs`).

## External rules and a shared rule library

1. Create a folder on a shared drive or in your Git repo, e.g. `C:\CAD-Automation\code\inventor\ilogic`.
2. *Tools → Options → iLogic Configuration* (or *Manage → iLogic → iLogic Configuration*) → **External Rule Directories → Add**.
3. *iLogic Browser → External Rules* lists the files. Right-click → *Run*, or assign to triggers.

Good practice:
- External rules for **anything used in more than one file** (export, numbering, checks). Fixing one file fixes everyone.
- Keep them in **Git**; users point their iLogic configuration at a read-only synced copy.
- Use `AddVbFile "Common.vb"` (first line of a rule) to share helper functions between external rules.
- Use `AddReference "MyCompany.Tools.dll"` to call your own compiled .NET code from rules (the DLL must be in iLogic's trusted *iLogic Addin DLLs Directory*, configured in the same dialog).

## Event triggers

*Manage → iLogic → Event Triggers.* Drag rules onto events:

| Event | Use |
|---|---|
| *After Open Document* | refresh title block fields, check references |
| *New Document* | fill default iProperties (designer, date, project) |
| *Before Save Document* | `07_iProps_Description_Builder`, `10_Event_Rules_Examples.StampSaveInfo` |
| *Any Model Parameter Change* | recompute derived values |
| *iProperty Change*, *Material Change*, *Part Geometry Change*, *Feature Suppression Change* | data consistency |
| *Close Document* | cleanup |

Set triggers for **all documents** via *This Document* tab in a template, or for all documents of a type
through the iLogic Event Trigger *All Documents / Parts / Assemblies / Drawings* tabs (stored in the
iLogic configuration, not in files) — choose deliberately: per-template triggers travel with the file,
global ones only work on PCs configured for them.

## Excel-driven design (GoExcel)

`06_Excel_Size_Table.iLogicVb` drives a flange from `Sizes.xlsx`:

```vb
Dim xls As String = ThisDoc.Path & "\Sizes.xlsx"
Dim sheet As String = "Flanges"
MultiValue.List("Size") = GoExcel.CellValues(xls, sheet, "A2", "A200")
Dim row As Integer = GoExcel.FindRow(xls, sheet, "Size", "=", CStr(Parameter("Size")))
If row < 0 Then MessageBox.Show("Size not in table") : Return
Parameter("OD") = GoExcel.CurrentRowValue("OD")
Parameter("Thk") = GoExcel.CurrentRowValue("Thk")
GoExcel.Close()
```

- `FindRow` uses row 1 as **headers** (`Size`, `OD`, …).
- **Embed** the workbook (*Manage → Link → Embed*) for a self-contained template: file name becomes `"3rd Party:Embedding 1"`.
- GoExcel uses Excel or its own reader depending on the *iLogic Configuration → Excel Options*; on a PC without Excel set it to the built-in option (VERIFY wording in your release).
- Alternatives: **iParts** (table inside the part, members as files) for catalogue items — see [docs/09](09-drawings-bom-export.md#iparts-and-iassemblies). GoExcel = live calculation; iPart = fixed family.

## Assemblies: components, suppression, model states

`04_Frame_Assembly_Driver.iLogicVb` pushes values from the assembly down to parts:

```vb
Parameter("Leg:1", "Length") = H            ' set the Leg part parameter through an occurrence
Component.IsActive("Shelf:1") = CBool(Parameter("HasShelf"))
```

- `Parameter("Occ:1", "name")` changes the **part file** (all instances!). Two different leg lengths need two part files (or a Model State / iPart member).
- `Component.IsActive = False` suppresses (2022+: in the active Model State). Suppressed = not in BOM.
- Occurrence names must be stable: rename them in the browser (`Leg:1`), and do not let users rename.
- **Model States** (2022+) store suppression, parameter values and iProperties per state — an alternative to iParts for configurations within one file. iLogic works in the active model state.

## Raw Inventor API from iLogic

Everything in [docs/02](02-inventor-api-object-model.md) works in a rule. `03_Shaft_Generator.iLogicVb`
builds geometry with no parameters at all:

```vb
Dim doc As PartDocument = ThisDoc.Document
Dim def As PartComponentDefinition = doc.ComponentDefinition
Dim tg As TransientGeometry = ThisApplication.TransientGeometry
Dim sk As PlanarSketch = def.Sketches.Add(def.WorkPlanes.Item(2))     ' XZ plane
' half profile in cm: x along shaft, y = radius
Dim axisLine As SketchLine = sk.SketchLines.AddByTwoPoints(tg.CreatePoint2d(0, 0), tg.CreatePoint2d(12.0, 0))
' ... outline lines: up the left face, along each step, down the right face ...
Dim prof As Profile = sk.Profiles.AddForSolid()
Dim rev As RevolveFeature = def.Features.RevolveFeatures.AddFull(prof, axisLine, PartFeatureOperationEnum.kNewBodyOperation)
```

The rule header can import namespaces (`Imports System.IO`) and set options. iLogic rules are compiled
with **Option Strict Off**: `doc.ComponentDefinitoin` (typo) is *not* a compile error — it fails at run
time with *"Public member ... not found"*. Our checker ([tools/README](../tools/README.md)) catches this
with a second Option-Strict-On pass. In your own rules: declare types (`Dim def As PartComponentDefinition`)
so IntelliSense helps.

## Debugging iLogic

| Technique | How |
|---|---|
| `Logger.Info/Warn/Error` | iLogic Log window; set log level in iLogic Configuration |
| `MessageBox.Show(value)` | quick; annoying in loops |
| `Try ... Catch ex As Exception ... MessageBox.Show(ex.ToString())` | full stack trace, line numbers |
| Rule *Error* dialog → *More Info* | shows the exception; the line number refers to the rule text |
| Visual Studio attach | *Debug → Attach to Process → Inventor.exe* (Managed (.NET Core)) + `System.Diagnostics.Debugger.Break()` in the rule — works for rules compiled with debug info; VERIFY in 2026 (`iLogic Configuration → Debug` setting) |
| Move logic to a DLL | complex logic → class library, unit-test it, call it with `AddReference` |

## Calling iLogic from C# and C# from iLogic

From an add-in or EXE — run a rule by name through the iLogic add-in's automation object:

```csharp
const string ILogicAddinGuid = "{3BDD8D79-2179-4B11-8A5A-257B1C0263AC}";   // iLogic add-in ClientId (VERIFY in Tools > Add-ins)
ApplicationAddIn ilogic = app.ApplicationAddIns.ItemById(ILogicAddinGuid);
dynamic auto = ilogic.Automation;                    // Autodesk.iLogic.Interfaces.IiLogicAutomation
auto.RunRule(doc, "Rule0");                          // internal rule
auto.RunExternalRule(doc, "05_Batch_Export_PDF_DXF");
```

For type-safe calls reference `Autodesk.iLogic.Interfaces.dll` from the Inventor `Bin` folder.

From iLogic — call your DLL:

```vb
AddReference "CadAutomation.Tools.dll"
Imports CadAutomation.Tools
Sub Main()
    Dim t As Double = VesselMath.ShellThickness(Parameter("P"), Parameter("D_i") / 2, Parameter("S"), Parameter("E"))
End Sub
```

## The 10 rules in this repo

| Rule | Read it for |
|---|---|
| [`01_SpurGear_Params`](../code/inventor/ilogic/01_SpurGear_Params.iLogicVb) | validation logic, `Logger.Warn`, engineering formulas in VB |
| [`02_Tank_Configurator`](../code/inventor/ilogic/02_Tank_Configurator.iLogicVb) | full configurator: multi-value, suppression, computed iProperties |
| [`03_Shaft_Generator`](../code/inventor/ilogic/03_Shaft_Generator.iLogicVb) | raw API geometry from a rule |
| [`04_Frame_Assembly_Driver`](../code/inventor/ilogic/04_Frame_Assembly_Driver.iLogicVb) | assembly → part parameter push, component suppression |
| [`05_Batch_Export_PDF_DXF`](../code/inventor/ilogic/05_Batch_Export_PDF_DXF.iLogicVb) | translators, folder mode, error logging |
| [`06_Excel_Size_Table`](../code/inventor/ilogic/06_Excel_Size_Table.iLogicVb) | GoExcel |
| [`07_iProps_Description_Builder`](../code/inventor/ilogic/07_iProps_Description_Builder.iLogicVb) | before-save trigger, sheet-metal extents |
| [`08_Drawing_Auto_Create`](../code/inventor/ilogic/08_Drawing_Auto_Create.iLogicVb) | drawing views, dimension retrieval, parts list |
| [`09_BOM_To_Excel`](../code/inventor/ilogic/09_BOM_To_Excel.iLogicVb) | BOM views + Inventor's own export |
| [`10_Event_Rules_Examples`](../code/inventor/ilogic/10_Event_Rules_Examples.iLogicVb) | small checks for triggers |

## When to stop using iLogic

Move to an add-in or EXE when **two or more** apply:
- the rule is > 300 lines or has many helper functions,
- you copy-paste the same internal rule into many templates,
- you need a real UI (grids, trees, validation, progress bars),
- you need to debug step-by-step regularly,
- it must run without a user (overnight, server),
- performance matters on large assemblies.

Middle way: keep the iLogic rule as a thin "glue" layer and put the logic in a DLL (`AddReference`).

Next: [04 — VBA](04-vba.md).
