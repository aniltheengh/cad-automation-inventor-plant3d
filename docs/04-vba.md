# 04 — VBA in Inventor: the API explorer you should still use

VBA is legacy for *deployment* but still the best **API discovery tool** Inventor has: live objects,
Watch window, Object Browser, no build step.

## Contents
- [Install and open](#install-and-open)
- [Where macros live](#where-macros-live)
- [Five-minute tour](#five-minute-tour)
- [VBA → iLogic → C# translation table](#vba--ilogic--c-translation-table)
- [The three modules in this repo](#the-three-modules-in-this-repo)
- [Why not to deploy VBA](#why-not-to-deploy-vba)

## Install and open

1. Download **Microsoft VBA module for Inventor 2026** (Autodesk support article *"Download the Microsoft VBA module for Inventor"*). It is not installed with Inventor. Without it: *"Microsoft Visual Basic for Applications software (VBA) is no longer installed with Inventor."*
2. Close Inventor, run the installer, start Inventor.
3. *Tools → Options → Macros* (security level) → then *Tools → VBA Editor* (Alt+F11).

## Where macros live

| Project | File | Scope |
|---|---|---|
| *ApplicationProject* | `Default.ivb` (path in *Tools → Application Options → File → Default VBA project*) | every session |
| *DocumentProject* | inside an `.ipt/.iam/.idw` | that document (security risk; avoid) |
| User projects | any `.ivb` loaded via *Tools → Macros* | when loaded |

## Five-minute tour

1. Open a part. VBA Editor → *Insert → Module*.
2. Type and run (F5):
   ```vb
   Sub Explore()
       Dim doc As PartDocument
       Set doc = ThisApplication.ActiveDocument
       Stop                                ' execution pauses here
   End Sub
   ```
3. When it stops: *View → Watch Window*, right-click → *Add Watch* → `doc`. Expand: `ComponentDefinition → Features → ExtrudeFeatures → Item 1 → Definition → Extent` — you are browsing the live model.
4. **F2** = Object Browser → library *Inventor* → search `ExtrudeDefinition` → every method with arguments.
5. **Ctrl+G** Immediate window: `? doc.ComponentDefinition.Parameters.Count`.

This is the quickest way to answer "what is the API object for this feature I made by hand?".

## VBA → iLogic → C# translation table

| VBA | iLogic / VB.NET | C# |
|---|---|---|
| `Dim d As PartDocument` / `Set d = ThisApplication.ActiveDocument` | `Dim d As PartDocument = ThisApplication.ActiveDocument` | `var d = (PartDocument)app.ActiveDocument;` |
| `Set` for objects | no `Set` | — |
| `Debug.Print x` | `Logger.Info(x)` | `Console.WriteLine(x);` |
| `MsgBox "Hi"` | `MessageBox.Show("Hi")` | `MessageBox.Show("Hi");` |
| `On Error Resume Next` | `Try ... Catch ... End Try` | `try { } catch { }` |
| `Dim i As Long` | `Dim i As Integer` (32-bit) | `int i;` |
| `For i = 1 To col.Count` | same | `for (int i = 1; i <= col.Count; i++)` |
| `col.Item(1)` | `col.Item(1)` or `col(1)` | `col[1]` |
| `kPartDocumentObject` | `DocumentTypeEnum.kPartDocumentObject` (or bare in iLogic) | `DocumentTypeEnum.kPartDocumentObject` |
| optional args by position | same + named `name:=value` | named `name: value` |
| `Nothing` | `Nothing` | `null` |

## The three modules in this repo

| Module | Macro | Shows |
|---|---|---|
| [`Module1_Basics.bas`](../code/inventor/vba/Module1_Basics.bas) | `HelloInventor`, parameter list, iProperty set | `ThisApplication`, collections, property sets |
| [`Module2_ExportPDF.bas`](../code/inventor/vba/Module2_ExportPDF.bas) | `ExportActiveDrawingToPDF` | translator add-in pattern (same GUIDs as everywhere in this repo) |
| [`Module3_AssemblyTraverse.bas`](../code/inventor/vba/Module3_AssemblyTraverse.bas) | `TraverseActiveAssembly` | recursion over `SubOccurrences` |

Import: VBA Editor → *File → Import File...* → `.bas`.

## Why not to deploy VBA

- Separate install on every PC; security prompts; corporate policies increasingly block VBA.
- `.ivb` is binary → no Git diff, no code review. (Export modules as `.bas` text files — this repo does.)
- No NuGet, no modern language features, poor error handling.
- VBA runs in-process but marshals through the VBA runtime — not faster than iLogic.

Prototype in VBA → move to **iLogic** (copy, remove `Set`, add types) or **C#**.

Next: [05 — C# and VB.NET EXEs](05-csharp-vbnet-exe.md).
