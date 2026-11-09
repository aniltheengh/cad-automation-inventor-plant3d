# 01 — Choosing a language (and a hosting model)

The real choice is not "which language" but **where the code runs** and **who starts it**. The
language follows from that.

## Contents
- [Four hosting models](#four-hosting-models)
- [Inventor: language vs. use case](#inventor-language-vs-use-case)
- [AutoCAD / Plant 3D: language vs. use case](#autocad--plant-3d-language-vs-use-case)
- [Decision tree](#decision-tree)
- [Same task, six languages](#same-task-six-languages)
- [Speed: in-process vs. out-of-process](#speed-in-process-vs-out-of-process)
- [iLogic vs. add-in vs. EXE vs. Python vs. cloud — detailed](#ilogic-vs-add-in-vs-exe-vs-python-vs-cloud--detailed)
- [Recommended path for you](#recommended-path-for-you)

## Four hosting models

| Model | Runs | Started by | Examples |
|---|---|---|---|
| **In-process, inside the product** | in `Inventor.exe` / `acad.exe` memory | user (button, rule, trigger, command) | iLogic, VBA, Inventor add-in DLL, AutoCAD .NET DLL, AutoLISP |
| **Out-of-process, on the same PC** | your own process talking COM to the product | you (double-click, Task Scheduler, another app) | C#/VB.NET EXE, Python + pywin32, PowerShell, pyautocad |
| **Headless, on the same PC** | product engine without UI | script / scheduler | Inventor Apprentice (read-mostly), AutoCAD Core Console, InventorCoreConsole (DA local test) |
| **Cloud** | Autodesk servers | HTTP request | APS Design Automation for Inventor / AutoCAD |

Two rules fall out of this:
1. **In-process is 10–100× faster** for many small API calls (no cross-process marshalling).
2. **Out-of-process is easier to debug and to combine with other tools** (Excel, databases, ERP, pandas), and a crash does not take Inventor down.

## Inventor: language vs. use case

| Use case | iLogic | VBA | C#/VB.NET EXE | C#/VB.NET add-in | Python (pywin32) | PowerShell | Apprentice | Design Automation |
|---|---|---|---|---|---|---|---|---|
| Rule inside a template part (configurator) | ★★★ | ★ | – | ★★ | – | – | – | – |
| Form/dialog for users | ★★ (iLogic Forms) | ★ (UserForm) | ★★ (WinForms/WPF) | ★★★ | ★ (tkinter, slow) | – | – | web front end |
| Ribbon button for the team | ★ (Global Form with rule buttons — limited) | – | – | ★★★ | – | – | – | – |
| Events (on save, on open, on change) | ★★ (Event Triggers) | ★ | – | ★★★ | – | – | – | – |
| Batch export 1000 drawings to PDF | ★★ | ★ | ★★★ | ★★ | ★★ | ★★ | – (no export) | ★★ (cloud) |
| iProperties of 10 000 files | ★ | ★ | ★★ | ★ | ★★ | ★ | ★★★ (no Inventor needed) | – |
| Geometry generator (gear, frame, shaft) | ★★ | ★ | ★★★ | ★★★ | ★★ | – | – | ★★★ (if on web) |
| Data work: Excel/ERP/SQL/optimisation | ★ (GoExcel) | ★ | ★★★ | ★★ | ★★★ (pandas, SciPy) | ★ | ★★ | ★★ |
| Customer configurator on a website | – | – | – | – | – | – | – | ★★★ |
| Learn the API / prototype | ★★ | ★★★ (Object Browser) | ★★★ (debugger) | ★★ | ★★ | ★ | – | – |
| Deploy to 20 colleagues | ★★ (shared external rules folder) | ★ (.ivb copy) | ★★ (copy EXE) | ★★★ (installer / bundle) | ★ (Python on every PC) | ★★ | ★★ | ★★★ (nothing to install) |
| Needs Visual Studio | no | no | yes | yes | no | no | yes | yes |
| Needs an Inventor seat | yes | yes | yes | yes | yes | yes | **no** | **no** (cloud credits) |

★★★ = best tool, ★★ = good, ★ = possible but awkward, – = no / unsuitable.

## AutoCAD / Plant 3D: language vs. use case

| Use case | AutoLISP | .scr script | AutoCAD .NET (C#) | Plant SDK (.NET) | Core Console | pyautocad / COM | Python custom part script |
|---|---|---|---|---|---|---|---|
| Title-block attributes on 500 DWGs | ★★ | ★★ (calls LISP) | ★★★ | – | ★★★ (batch host) | ★ | – |
| Layer standards, purge, audit | ★★★ | ★★ | ★★ | – | ★★★ | ★ | – |
| Read pipe line numbers, sizes, specs | – | – | – | ★★★ | – (Plant apps don't load) | – | – |
| Line list / MTO / valve list to Excel | – | – | – | ★★★ (+ Python for the xlsx) | – | – | – |
| Set Plant properties in bulk (Status, Insulation) | – | – | – | ★★★ | – | – | – |
| P&ID tag report | – | – | – | ★★★ | – | – | – |
| New catalogue part geometry (strainer, blind) | – | – | – | – | – | – | ★★★ |
| Project setup (folders, Project.xml, specs copy) | – | – | ★ | ★★ | – | – | – |
| Isometrics (Isogen) styles | – | – | – | ★ | – | – | – (config files) |
| Quick personal command | ★★★ | ★ | ★ | ★ | – | ★ | – |
| Dynamo | **not available for Plant 3D** — see [docs/11](11-plant3d-sdk.md#dynamo-for-plant-3d-not-available) |||||||

## Decision tree

```
Does it need to run without anyone opening Inventor?
├─ yes ─ Does it need to model geometry?
│        ├─ yes ─ Web users / no Inventor seat? ── Design Automation (cloud)        [docs/08]
│        │        └─ Office PC at night? ───────── C# EXE + Task Scheduler           [docs/05]
│        └─ no (read/write iProperties, references) ── Apprentice                   [docs/08]
└─ no ── Is it tied to ONE template/product family?
         ├─ yes ─ iLogic internal rules + iLogic Form                               [docs/03]
         └─ no ── Should it be a button for everyone, or react to events?
                  ├─ yes ─ C# add-in                                                [docs/06]
                  └─ no ── Is the hard part data (Excel, ERP, maths)?
                           ├─ yes ─ Python (pywin32) or C# EXE                      [docs/07, 05]
                           └─ no ── External iLogic rule (shared folder)            [docs/03]
```

For Plant 3D the tree is shorter: **Plant data → .NET Plant SDK. Drawing/DWG work → LISP or AutoCAD .NET.
Batch on many DWGs → Core Console. Part geometry → Python custom script.**

## Same task, six languages

Task: *get the active document's Part Number iProperty.*

**iLogic**
```vb
Dim pn As String = iProperties.Value("Project", "Part Number")
```

**VBA**
```vb
Dim pn As String
pn = ThisApplication.ActiveDocument.PropertySets.Item("Design Tracking Properties").Item("Part Number").Value
```

**C# (EXE or add-in)**
```csharp
Document doc = app.ActiveDocument;
string pn = (string)doc.PropertySets["Design Tracking Properties"]["Part Number"].Value;
```

**VB.NET**
```vb
Dim pn As String = CStr(app.ActiveDocument.PropertySets("Design Tracking Properties")("Part Number").Value)
```

**Python (pywin32)**
```python
pn = inv.ActiveDocument.PropertySets.Item("Design Tracking Properties").Item("Part Number").Value
```

**PowerShell**
```powershell
$pn = $inv.ActiveDocument.PropertySets.Item("Design Tracking Properties").Item("Part Number").Value
```

Notice: the **API is the same object model** in every language. iLogic adds shortcuts (`iProperties.Value`)
whose UI name "Project" maps to the internal set *Design Tracking Properties*. Learn the object model once
([docs/02](02-inventor-api-object-model.md)) and every language is just syntax.

## Speed: in-process vs. out-of-process

Order-of-magnitude figures from typical experience (not a benchmark of this repo — measure your own case):

| Operation | In-process (iLogic/add-in) | Out-of-process (EXE/Python) |
|---|---|---|
| Read 1 parameter | µs | ~0.1–1 ms (one COM round trip) |
| Loop over 10 000 edges and read a property | < 1 s | 10 s – minutes |
| Open a drawing, export PDF | seconds (Inventor work dominates) | same seconds — overhead negligible |
| Batch 500 files | dominated by file open/close | same |

**So:** for batch jobs over *files*, out-of-process costs nothing noticeable. For loops over *geometry*
(thousands of faces, edges, occurrences), run in-process or minimise calls (get a collection once, cache
values in .NET/Python lists).

## iLogic vs. add-in vs. EXE vs. Python vs. cloud — detailed

### iLogic
- **Pros:** in every Inventor seat, no install, rules live *with the model* (internal) or on a shared drive (external), Forms without code, Event Triggers, full VB.NET + full Inventor API available inside rules.
- **Cons:** editor is basic (no real debugger, `Logger` + `MessageBox` only), rules embedded in many files are hard to update (*one* bug, 300 files), Option Strict Off hides typos until run time, slow-ish rule recompiles.
- **Choose when:** knowledge belongs to a product family ("this bracket's hole pattern depends on width"), the users are designers, change frequency is high.
- **Avoid when:** you need a polished UI, background work, or the logic is shared across many product families → move the logic to an external rule or an add-in DLL that iLogic calls (`AddVbFile`, or reference a DLL with `AddReference`).

### VBA
- **Pros:** Object Browser + Watch on live objects, instant run, no build.
- **Cons:** separate download, legacy language, `.ivb` binary files (bad for Git), security prompts.
- **Choose when:** exploring the API. **Do not** deploy new tools in VBA.

### C# / VB.NET EXE
- **Pros:** full debugger, NuGet (Excel, JSON, SQL, HTTP), separate process (crash-safe), easy to schedule, works with a hidden Inventor.
- **Cons:** COM overhead, Inventor must be installed and licensed on the PC, elevation must match.
- **Choose when:** batch processing, integrations, overnight jobs, tools you run, not your colleagues.

### C# / VB.NET add-in
- **Pros:** fastest, ribbon buttons, events, dockable windows, interaction (selection, preview graphics), professional deployment.
- **Cons:** most code to write, restart Inventor to reload, a crash in your add-in can crash Inventor, needs packaging per Inventor version.
- **Choose when:** a team tool used daily, or something iLogic cannot do (custom browser panes, client graphics, interactive commands).

### Python (pywin32)
- **Pros:** you already have Anaconda; pandas/numpy/scipy/matplotlib; quick scripts; great for "Excel → Inventor" and engineering calculations.
- **Cons:** out-of-process COM speed, late-binding surprises (`cast()` needed), no events/UI in Inventor, every user needs Python.
- **Choose when:** the engineering/data part dominates and Inventor is a geometry back end.

### PowerShell
- **Pros:** on every Windows PC, good for IT-style tasks (file moves + export), scheduled tasks.
- **Cons:** clumsy COM (no enum names, parameterized properties), little tooling.
- **Choose when:** a 30-line admin script; otherwise use Python or C#.

### Apprentice
- **Pros:** no Inventor licence, no UI, fast; iProperties, references, BOM structure, read-only geometry.
- **Cons:** no modelling, no export translators, no drawings updates; 2026 needs a separate installer.
- **Choose when:** reports and mass iProperty fixes across a file server or Vault workspace.

### Design Automation (APS cloud)
- **Pros:** Inventor engine on demand, no seats, scales, web/mobile front ends, sales configurators.
- **Cons:** cloud credits cost money, upload/download overhead, no UI, debug locally first, more moving parts (OAuth, buckets, signed URLs).
- **Choose when:** non-CAD users must generate models/drawings (sales, customers), or nightly jobs at scale.

## Recommended path for you

You are a mechanical engineer, beginner in C#, with Inventor 2026 now and Plant 3D later:

| Week | Focus | Docs | Projects |
|---|---|---|---|
| 1 | iLogic: parameters, iProperties, a configurator form | 02, 03 | iLogic 01, 02, 06 |
| 2 | C# basics through Inventor EXEs + debugger | 05 | C# 01, 02 |
| 3 | Batch work: export + Excel round trips | 09, 07 | C# 03, 04, 05; Python batch_export |
| 4 | First add-in on the ribbon | 06 | C# 07 |
| 5 | AutoCAD .NET + LISP + Core Console (no Plant licence needed) | 10, 12 | AttributeTools, att_tools.lsp |
| 6 | Plant 3D trial: SDK, line list, MTO, custom part | 11 | Plant3DDataTools, CPNOZZLE_STUB |
| 7+ | Cloud: Design Automation; harden deployment | 08, 14 | GearDAPlugin |

Next: [02 — Inventor API object model](02-inventor-api-object-model.md).
