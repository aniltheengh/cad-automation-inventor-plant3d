# VBA modules — quick API prototyping in Inventor

| Module | Macros |
|---|---|
| `Module1_Basics.bas` | `HelloInventor`, list parameters, set an iProperty |
| `Module2_ExportPDF.bas` | `ExportActiveDrawingToPDF` (port of the Autodesk API help sample) |
| `Module3_AssemblyTraverse.bas` | recursive occurrence walk with quantities in the Immediate window |

**Install VBA first:** VBA is no longer installed with Inventor; the Microsoft VBA module (64-bit, per release, 2026 included) is a separate download
("Download the Microsoft VBA module for Inventor", Autodesk support article). Then *Tools → VBA Editor →
File → Import File…* → choose the `.bas`. Run with **F5** inside a macro, watch `Debug.Print` in **Ctrl+G**.

Why keep VBA at all? Its **Object Browser (F2)** and **Watch window** on live Inventor objects are still the
fastest way to *discover* the API. Prototype here, then move the code to iLogic (almost identical syntax:
drop `Set`, add `As` types) or C#. Do not build production tools in VBA: macros live in `.ivb` files that are
hard to version-control, and VBA is 64-bit VBA 7.1 with no further language development.
