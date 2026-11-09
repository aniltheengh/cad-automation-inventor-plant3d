# 17 — References (with real links)

All links were checked in November 2026 with `python tools/check_links.py --external`. A few sites
(Stack Overflow, theSwamp, the Autodesk forums, inventor.autodeskapi.dev) block scripts with 402/403, so the
script flags them even though they open fine in a browser; Autodesk support/help pages were opened by hand. Autodesk moves pages often — if a link breaks, search the title.

## Contents
- [Inventor API and add-ins](#inventor-api-and-add-ins)
- [.NET 8 / .NET 10 transition](#net-8--net-10-transition)
- [iLogic and VBA](#ilogic-and-vba)
- [Apprentice and Design Automation](#apprentice-and-design-automation)
- [AutoCAD .NET, LISP, Core Console](#autocad-net-lisp-core-console)
- [Plant 3D](#plant-3d)
- [Libraries used in this repo](#libraries-used-in-this-repo)
- [Engineering standards referenced](#engineering-standards-referenced)
- [Communities](#communities)

## Inventor API and add-ins

| Source | Used for |
|---|---|
| Autodesk Developer Blog — [Inventor 2026 API関連情報](https://blog.autodesk.io/inventor-2026-api-update/) | .NET 8, registry add-ins retired in 2024, all-users add-in folder change, Apprentice 2026 separate & registry-free, Inventor View discontinued |
| Autodesk Developer Blog — [Marshal.GetActiveObject Replacement in .NET 8+](https://blog.autodesk.io/marshalgetactiveobject-replacement-in-net-8-connecting-to-running-autodesk-inventor-instances/) | `InventorConnector.cs` |
| Jelte de Jong — [Migration addins to .Net core 8 (Inventor 2025)](http://hjalte.nl/tutorials/78-migrationinventor2025addins) | interop path `Bin\`, add-in folders (4 locations), debug profile, porting steps |
| Jelte de Jong — [Compile and debug multiple .NET versions](http://hjalte.nl/tutorials/95-compile-and-debug-multiple-net-versions) | multi-targeting net48 + net8 |
| Tyler Warner — [AssemblyLoadContext for Inventor addins](https://tylerwarner.dev/assemblyloadcontext-for-inventor-addins) | dependency isolation |
| Autodesk Inventor API help (online) — [inventor.autodeskapi.dev](https://inventor.autodeskapi.dev/) | object reference, *Port .NET Framework projects to .NET*, *Using VBA in Inventor* |
| Inventor SDK on your PC | `C:\Users\Public\Documents\Autodesk\Inventor 2026\SDK\` (DeveloperTools.msi, samples, object model chart) |
| KETIV — [Connecting Visual Studio with Autodesk Inventor and Creating an Inventor Add-In](https://youtu.be/qm3bnCidJlk) (video) | template install, debug profile, unblocking |
| Autodesk Community — [STP Protocol Options](https://forums.autodesk.com/t5/inventor-programming-forum/stp-protocol-options/td-p/12922008) | STEP `ApplicationProtocolType` (AP242 = 5) |

## .NET 8 / .NET 10 transition

| Source | Used for |
|---|---|
| Autodesk Developer Blog — [Autodesk Desktop Products 2025/2026: .NET 10 Updates](https://blog.autodesk.io/autodesk-desktop-products-2025-2026-net-10-updates/) | 2026.4 / AutoCAD 2026.1.2 runtime move, no API change |
| Autodesk Developer Blog — [.NET Compatibility for Autodesk Inventor 2027 Add-ins](https://blog.autodesk.io/net-compatibility-for-autodesk-inventor-2027-add-ins-what-developers-need-to-know/) | 2027 = .NET 10, .NET 8 add-ins compatible |
| Microsoft — [.NET and .NET Core support policy](https://dotnet.microsoft.com/en-us/platform/support/policy/dotnet-core) | end-of-support dates (.NET 8/9: 10 Nov 2026) |
| Microsoft — [Porting from .NET Framework to .NET](https://learn.microsoft.com/en-us/dotnet/core/porting/) | general porting |

## iLogic and VBA

| Source | Used for |
|---|---|
| Autodesk — [Download the Microsoft VBA module for Inventor](https://www.autodesk.com/support/technical/article/caas/tsarticles/ts/580m5V9igpBgk3WNek5Ydf.html) | VBA is a separate install |
| Inventor help — iLogic (in product: *Help → iLogic*; snippets: rule editor *System Snippets*) | iLogic functions |
| Autodesk Community — [Inventor Programming forum](https://forums.autodesk.com/t5/inventor-programming-forum/bd-p/120) | everyday questions |

## Apprentice and Design Automation

| Source | Used for |
|---|---|
| Autodesk Developer Blog — [Inventor Design Automation 2026 Engine Moves to .NET 8](https://blog.autodesk.io/adapting-to-change-inventor-design-automation-2026-engine-moves-to-net-8/) | engine `Autodesk.Inventor+2026`, .NET 8 only |
| Autodesk Developer Blog — [Design Automation API: obtaining supported engines](https://blog.autodesk.io/design-automation-api-obtaining-supported-engines/) | `GET /engines`, pagination |
| APS — [Inventor Automation API](https://aps.autodesk.com/apis-and-services/inventor-automation-api) | product page, docs entry |
| APS — [Design Automation developer's guide](https://aps.autodesk.com/en/docs/design-automation/v3/developers_guide/overview/) | AppBundle/Activity/WorkItem |
| NuGet — [Autodesk.Forge.DesignAutomation](https://www.nuget.org/packages/Autodesk.Forge.DesignAutomation) | .NET client |

## AutoCAD .NET, LISP, Core Console

| Source | Used for |
|---|---|
| NuGet — [AutoCAD.NET](https://www.nuget.org/packages/AutoCAD.NET) / [AutoCAD.NET.Core](https://www.nuget.org/packages/AutoCAD.NET.Core) | reference packages and version ↔ release mapping |
| Autodesk help — [AutoCAD Developer Center / help](https://help.autodesk.com/view/OARX/2026/ENU/) | AutoCAD .NET developer's guide, PackageContents reference |
| VS Code Marketplace — [AutoCAD AutoLISP Extension](https://marketplace.visualstudio.com/items?itemName=Autodesk.autolispext) | LISP editing/debugging |
| Autodesk Developer Blog — [Can I Run AutoCAD P&ID/Plant 3D in Console Mode?](https://blog.autodesk.io/can-i-run-autocad-pidplant-3d-in-console-mode/) | Core Console + Plant object enablers, no Plant commands |

## Plant 3D

| Source | Used for |
|---|---|
| ADN — [Plant3DTraining labs (GitHub)](https://github.com/ADN-DevTech/Plant3DTraining) | every SDK member used in `Plant3DDataTools`; DLL list |
| Autodesk — [Plant 3D Customization and Automation Options](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Autodesk-AutoCAD-Plant-3D-Customization-and-Automation-Options-for-Engineering-Workflows.html) (Oct 2026) | official list of automation options |
| Autodesk — [Does AutoCAD Products support Dynamo?](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Does-AutoCAD-Products-support-Dynamo.html) | Dynamo not in AutoCAD-based products except Civil 3D |
| Autodesk Community — [Idea: Dynamo for AutoCAD Plant 3D](https://forums.autodesk.com/t5/autocad-plant-3d-p-id-ideas/dynamo-for-autocad-plant-3d/idi-p/11171559) | Dynamo is only a request |
| Autodesk — [AutoCAD including specialized toolsets (Plant 3D)](https://www.autodesk.com/products/autocad/included-toolsets/autocad-plant-3d) | how to get Plant 3D, trial |
| Autodesk — [Education](https://www.autodesk.com/education/home) | education licence |
| Autodesk University 2011 PD1746 / PD4214-L handouts — *Scripting Components for AutoCAD Plant 3D* | custom part script decorators and primitives |
| Autodesk Developer Blog series *Custom Python Scripts for AutoCAD Plant 3D* (David Wolfe) | registration (`PLANTREGISTERCUSTOMSCRIPTS`), `testacpscript` |

## Libraries used in this repo

| Library | Link |
|---|---|
| ClosedXML (C# xlsx) | [nuget.org/packages/ClosedXML](https://www.nuget.org/packages/ClosedXML) |
| pywin32 | [pypi.org/project/pywin32](https://pypi.org/project/pywin32/) |
| openpyxl | [openpyxl.readthedocs.io](https://openpyxl.readthedocs.io/) |
| pyautocad | [pypi.org/project/pyautocad](https://pypi.org/project/pyautocad/) |
| PowerShell 7 | [learn.microsoft.com/powershell](https://learn.microsoft.com/en-us/powershell/) |

## Engineering standards referenced

| Standard | In |
|---|---|
| ASME BPVC Section VIII Div. 1, UG-27 / UG-32 | `02_Tank_Configurator` (sizing aid only) |
| ISO 53 (basic rack), ISO 54 (modules) | gear generator |
| ISO 5455 (drawing scales) | `08_Drawing_Auto_Create` |
| JIS B 2220 (flanges) | exercise P9 |
| DIN 6885 (keys) | exercise P1 |

Standards are paid documents; the samples use well-known textbook forms. Check the current edition for real designs.

## Communities

- [Autodesk Community forums](https://forums.autodesk.com/) — *Inventor Programming*, *AutoCAD Plant 3D*, *.NET (AutoCAD)*, *Visual LISP, AutoLISP and General Customization*.
- [The Swamp](https://www.theswamp.org/) — AutoCAD/LISP/.NET programmers.
- [Stack Overflow — autodesk-inventor tag](https://stackoverflow.com/questions/tagged/autodesk-inventor).

Back to [README](../README.md).
