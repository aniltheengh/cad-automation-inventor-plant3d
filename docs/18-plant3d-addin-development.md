# 18 — Plant 3D add-in development with C# and the Plant 3D API

[Repository home](../README.md) · [Plant SDK reference](11-plant3d-sdk.md) · [Existing C# project](../code/plant3d/csharp/Plant3DDataTools/README.md)

A dedicated learning path for building a DLL that runs **inside AutoCAD Plant 3D**. Start with a command, learn to read project data, then add validation, UI and deployment.

**Status:** this is a development guide with an illustrative C# extension. The existing Plant examples have a stub-based compilation route; neither that route nor this guide proves compatibility with your installed SDK. Build with real Autodesk references and complete the manual checks below before using a tool on a working project.

## 1. What you are building

| Layer | Responsibility | Examples |
|---|---|---|
| C# / .NET | Your application logic | validation, collections, CSV formatting, error handling |
| AutoCAD managed API | Host and drawing interaction | commands, selection, editor messages, ObjectId, transactions |
| Plant 3D managed API | Plant project semantics | current project, project parts, data links, properties, relationships |
| Your add-in | A focused engineering workflow | inspect a valve, check missing tags, export a line list |

A command-only DLL is already an add-in. A ribbon or palette can be added later. Plant properties require the Plant APIs; editing an ordinary block attribute does not necessarily update Plant project data.

The first deliverable is small: load your DLL, type a command, select a piping component and inspect its properties. Keep Python catalog geometry scripting as a separate workflow; see [chapter 11](11-plant3d-sdk.md).

## 2. Prepare a version-matched development environment

1. In Plant 3D, record the product year and installed update/build from About.
2. Install Visual Studio with the .NET desktop development tools and an SDK that supports your chosen target framework.
3. Obtain the [official Plant 3D SDK](https://aps.autodesk.com/developer/overview/autocad-plant-3d-objectarx-sdk-downloads) for the target release/update. Use its reference assemblies, help and examples.
4. Use a C# class library with an x64 target. AutoCAD loads the resulting DLL; there is no console-program Main entry point.
5. Match the AutoCAD references, Plant references and target framework as one set. Set Copy Local to false for Autodesk host assemblies.
6. Create a disposable Plant project with a model drawing, a pipe or valve, and an ordinary AutoCAD line.

**Runtime note, checked 13 November 2026:** do not infer the runtime from the product year alone. Autodesk's [2025/2026 .NET transition notice](https://blog.autodesk.io/autodesk-desktop-products-2025-2026-net-10-updates/) identifies the AutoCAD 2026.1.2 transition to .NET 10, including specialized toolsets, and explains that existing add-ins still require compatibility testing. Original 2025/2026 .NET 8 instructions describe an earlier baseline.

The repository's [Plant build settings](../code/plant3d/Directory.Build.props) default to a 2026/net8 target and offer a 2027/net10 branch. These are build choices, not a guarantee for every patched installation. A change of framework alone does not make mismatched Autodesk assemblies compatible.

### Start from the existing project

Open [Plant3DDataTools.csproj](../code/plant3d/csharp/Plant3DDataTools/Plant3DDataTools.csproj). It already contains the command classes and Plant references.

From the repository root, for the existing 2026 baseline:

```powershell
dotnet build code/plant3d/csharp/Plant3DDataTools/Plant3DDataTools.csproj -c Debug "-p:PlantSdkInc=C:\Autodesk\Plant3D 2026 SDK\inc-x64"
```

Replace the SDK directory with your actual directory. Inspect unresolved-reference warnings as failures to resolve before loading. For another host build, first align the reference versions and framework in the build settings with its SDK.

Do not deploy a build made with `-p:PlantStubs=true`. Those hand-written types are only for development checks.

## 3. Learn the C# concepts as you use them

| Concept | Where it appears |
|---|---|
| Class and public method | A command handler AutoCAD can discover |
| Attribute | `[CommandMethod("P3DHELLO")]` registers a command name |
| Interface | `IExtensionApplication` supplies load and shutdown hooks |
| Null check | A document or project may not be open |
| Exception handling | A bad selection or missing project part must produce a useful message |
| Collections and foreach | Properties are name/value pairs |
| using and disposal | Drawing transactions and document locks have bounded lifetimes |

Keep UI prompts in command methods. Move reusable validation and data conversion into ordinary C# methods so they can be tested without CAD.

## 4. Add a first command and lifecycle hooks

As an exercise, add `AddinEntry.cs` to the existing Plant3DDataTools project:

```csharp
using Autodesk.AutoCAD.Runtime;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Plant3DDataTools
{
    public sealed class AddinEntry : IExtensionApplication
    {
        public void Initialize()
        {
            // No project access here: loading can happen before a project opens.
        }

        public void Terminate()
        {
            // Detach any event handlers that this add-in subscribed to.
        }

        [CommandMethod("P3DHELLO", CommandFlags.Modal)]
        public void Hello()
        {
            var doc = AcadApp.DocumentManager.MdiActiveDocument;
            if (doc == null)
                return;

            doc.Editor.WriteMessage(
                "\nPlant add-in loaded. Next: run P3DDUMP in a project model drawing.");
        }
    }
}
```

This introductory command uses the AutoCAD API. The next step uses the Plant API. The snippet is an exercise, not an additional source file already included in the project.

AutoCAD discovers public command handlers through their attributes. Lifecycle hooks are optional. If you later add assembly-level `CommandClass` attributes, include **every** class containing commands, including the existing piping and P&ID command classes; otherwise those commands can disappear from discovery. Autodesk documents this behavior in [application initialization](https://help.autodesk.com/cloudhelp/2022/ENU/OARX-DevGuide-Managed/files/GUID-FA3B4125-F7BD-4E89-969F-9DCC90AC6977.htm).

## 5. Load and debug inside Plant 3D

1. Build using real SDK references.
2. Launch the installed **Plant 3D** shortcut and open the disposable project/model drawing.
3. Run `NETLOAD` and select the built `Plant3DDataTools.dll`.
4. Run `P3DHELLO` if you added the exercise, then the existing `P3DDUMP` command.
5. In Visual Studio, attach to the correct `acad.exe` process with the managed debugger, set a breakpoint in the command and run it again. Keep the matching PDB alongside the Debug DLL.
6. Restart Plant 3D after rebuilding an already loaded managed assembly. Normal managed add-ins cannot be manually unloaded.

For launch-from-debugger setup, copy the executable and arguments from your installed Plant 3D shortcut; a plain AutoCAD launch is not equivalent. Use an approved trusted development folder if secure loading blocks your DLL. See Autodesk's [loading a managed assembly](https://help.autodesk.com/cloudhelp/2018/ENU/OARX-DevGuide-Managed/files/GUID-4EB83A6B-9903-4BF7-9F19-767A4D419CE3.htm) for the basic workflow; paths in older examples must be adapted.

## 6. Follow the Plant API data path

Read [PlantHelpers.cs](../code/plant3d/csharp/Plant3DDataTools/PlantHelpers.cs) and [PipingCommands.cs](../code/plant3d/csharp/Plant3DDataTools/PipingCommands.cs) side by side.

```text
Active Plant project
    -> "Piping" project part
    -> DataLinksManager
Selected DWG entity's ObjectId
    -> FindAcPpRowId
    -> project database row ID
    -> GetAllProperties
    -> name/value pairs shown in the editor
```

Autodesk's historical [Plant training lab 3](https://github.com/ADN-DevTech/Plant3DTraining/blob/master/Labs/LABS_Complete/LAB_03.cs) demonstrates the project/data-links/property pattern. Use it to understand the sequence; confirm signatures against your installed SDK.

Before reading properties, check the active document, project, correct project part and selection status. An ordinary line, an unlinked object or an object from another context may have no usable Plant row; handle that outcome without assuming every selected entity is Plant data.

An AutoCAD `ObjectId`, a DWG handle and a Plant database row ID are different identifiers. Do not store an ObjectId in a CSV and expect it to identify the same object after reopening. Include project/drawing context when designing persistent records.

For P&ID, study [PidCommands.cs](../code/plant3d/csharp/Plant3DDataTools/PidCommands.cs) and use the appropriate project part. Do not reuse the piping manager blindly.

## 7. Grow from inspection to useful tools

| Stage | Existing starting point | Exercise and acceptance condition |
|---|---|---|
| Inspect | `P3DDUMP` | Select a pipe and compare printed values with Data Manager |
| Discover schema | `P3DTABLES` | Identify the classes/properties actually present in your test project |
| Report | `P3DLINELIST` | Export and reconcile row count and line tags with the project |
| Materials | `P3DMTO` | Check sample quantities against a small, manually counted model |
| P&ID | `PIDPICK`, `PIDTAGS` | Check tag uniqueness and missing values in a test drawing |
| Edit | `P3DSETPROPS` | Preview a small proposed change, apply it to a test object, reopen and verify |
| Team tool | Command wrapper, then UI | A colleague can install, run and diagnose the tool from its instructions |

A line list and an MTO represent different things: a line can contain many components. Define scope, grouping and units before presenting an export as an engineering deliverable.

## 8. Handle changes, document context and UI deliberately

For property-writing exercises, discover the real property names, check whether values are writable, validate inputs, and show the old and proposed values first. Use the supported Plant data-link APIs rather than direct SQL changes to project files. Keep a test project backup and an explicit record of successful and failed updates.

A DWG transaction does not by itself establish atomic rollback across all Plant project data. Verify the SDK's persistence behavior and the effects of cancel, undo, save and reopen for the exact operation you implement.

Keep AutoCAD/Plant object access on the supported host execution context. A modeless palette must re-check the active document/project when its button is clicked, enter an appropriate command context and obtain a document lock where required. Do not place CAD API calls in Task.Run. Pure formatting or calculations on copied data can be separated from host access.

Once the command is reliable, add a ribbon button that invokes it, or a palette for repeated inspection. Keep initialization light, avoid caching a project manager across project switches, and unsubscribe events when no longer needed.

## 9. Package for deployment

Begin with NETLOAD during development. For repeatable installation, prepare an Autodesk application bundle:

```text
PlantDataTools.bundle/
    PackageContents.xml
    Contents/
        Windows/
            Plant3DDataTools.dll
            ...your own required dependencies...
```

The manifest must point to the actual DLL and declare the tested host/release range. Restrict Plant-dependent components with `RuntimeRequirements` using `OS="Win64"` and `Platform="Plant3D"`; do not declare all AutoCAD products compatible. Consult the official [RuntimeRequirements reference](https://help.autodesk.com/cloudhelp/2024/ENU/AutoCAD-Customization/files/GUID-1591CA01-EF87-48CD-952B-772FE26037F1.htm) when choosing release limits.

Use the user's Autodesk ApplicationPlugins directory or a managed deployment location appropriate to the installation. Include your own dependencies, configuration and installation instructions; do not ship Autodesk SDK binaries or the repository's stub assembly. Test installation and command discovery after a clean host restart. See [deployment practices](14-best-practices.md) for the repository's broader guidance.

## 10. Manual verification and troubleshooting

| Check | Expected outcome |
|---|---|
| Real-reference build | No unresolved Autodesk references |
| NETLOAD in the target Plant build | DLL loads; expected commands are available |
| No open project | Useful explanation, no unhandled exception |
| Cancel selection | Command exits without a change |
| Select an ordinary AutoCAD line | No linked Plant row is handled clearly |
| Select a known piping component | Values agree with Data Manager |
| Switch projects and drawings | Next command uses the new active context |
| Use a P&ID drawing | Correct project part, or a clear unsupported-context message |
| Export a small known model | Correct scope, columns, units and counts |
| Save, close and reopen after an edit | Intended values persist; unrelated objects remain unchanged |
| Install bundle and restart | Only the declared compatible host loads the add-in |

Common diagnoses:

- **Unknown command:** verify the loaded DLL, public command method, CommandMethod spelling and any CommandClass attributes.
- **Missing assembly/type:** check the actual Plant host, reference paths, SDK/release match and deployment contents before adding custom assembly-resolution code.
- **Breakpoint not hit:** check the selected acad.exe process, DLL path, matching PDB and whether the host still has an earlier DLL loaded.
- **Missing property:** inspect the project class/schema instead of assuming a column from another project's example exists.
- **Drawing-lock error:** review whether the operation came from a modal command, session command or modeless UI, and use the required host context.

## 11. Suggested practice sequence

1. Explain each line of the P3DHELLO exercise, then build and run it.
2. Trace P3DDUMP from selection through row lookup to property output.
3. Add a read-only check for a missing property using a name discovered in your test project.
4. Create a report whose counts you can verify manually.
5. Design a change preview before adding any write operation.
6. Package the verified tool and repeat the checks on a clean session.

The milestone is a small, repeatable add-in with understandable failure messages. Advanced routing, spec-driven placement and connectivity edits should follow separate SDK-backed experiments.

Previous reference chapter: [17 — References](17-references.md) · Related: [10 — AutoCAD .NET basics](10-autocad-dotnet-basics.md), [11 — Plant SDK](11-plant3d-sdk.md)
