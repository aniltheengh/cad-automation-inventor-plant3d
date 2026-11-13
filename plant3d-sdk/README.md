# AutoCAD Plant 3D SDK — what, how, and samples

A beginner-friendly, code-heavy guide to the **AutoCAD Plant 3D ObjectARX / Plant SDK** (.NET).

- Written for Plant 3D **2026** (AutoCAD 2025+ family).
- API names below were checked against Autodesk Platform Services pages, Autodesk Help, Autodesk Support articles, the ADN [Plant3DTraining](https://github.com/ADN-DevTech/Plant3DTraining) labs, and Autodesk Community threads.
- **Do not invent or guess** method names from this file alone. Confirm signatures in the SDK help (`plantsdk_ref.chm`) and in `Samples\` before you ship a tool.
- This folder does **not** include Autodesk binaries, Udemy material, or Google Drive links.
- Samples are meant to be built on a PC that has Plant 3D + the matching SDK. They were not executed against Plant 3D in this repository.

Related material already in this repo: [docs/11](../docs/11-plant3d-sdk.md), [docs/18](../docs/18-plant3d-addin-development.md), [code/plant3d](../code/plant3d/).

---

## What the Plant 3D SDK is

- The **AutoCAD Plant 3D ObjectARX SDK** (Plant SDK) extends the **ObjectARX SDK for AutoCAD**.
- Official overview: [AutoCAD Plant 3D and P&ID API](https://aps.autodesk.com/developer/overview/autocad-plant-3d-and-pid).
- It lets you:
  - Query and control **P&ID** schematic and project information.
  - Control the **Plant 3D model** (piping, equipment, project data).
- The **AutoCAD P&ID API is a subset** of the Plant SDK. For a P&ID-only program, do not reference Plant 3D-only interfaces (they are flagged in `plantsdk_ref.chm`).
- You need **both** SDKs: ObjectARX for AutoCAD **and** the Plant 3D SDK. You should already know the AutoCAD .NET API.
- The Plant SDK is laid out like ObjectARX, so you can **overlay** it on a pre-installed ObjectARX SDK.
- Official samples ship in the SDK `Samples` folder (`PnID`, `Piping`, `Common`).
- Official help ships in the SDK `Docs` folder:
  - `plantsdk_dev.chm` — developer guide
  - `plantsdk_ref.chm` — class/method reference
- There is a separate Autodesk documentation pack covering Plant SDK **2017–2022** on the [Plant SDK downloads](https://aps.autodesk.com/developer/overview/autocad-plant-3d-objectarx-sdk-downloads) page.

### Main managed components (DLL → typical use)

Reference only what you need. Set **Copy Local = False** (`<Private>false</Private>`).

| DLL | Typical namespaces / types | What it is for |
|---|---|---|
| `PnPProjectManagerMgd.dll` | `Autodesk.ProcessPower.PlantInstance` (`PlantApplication`), `Autodesk.ProcessPower.ProjectManager` (`PlantProject`, `Project`, `PnPProjectDrawing`) | Current project, project parts, drawing list |
| `PnPDataLinks.dll` | `Autodesk.ProcessPower.DataLinks` (`DataLinksManager`, `DataLinksManager3d`, `PpObjectId`) | Link a DWG object to a project-database row; get/set properties |
| `PnPDataObjects.dll` | `Autodesk.ProcessPower.DataObjects` (`PnPDatabase`, `PnPTable`, `PnPRow`) | Project / spec database tables and rows |
| `PnP3dProjectPartsMgd.dll` | `Autodesk.ProcessPower.P3dProjectParts` (`PipingProject`, `IsoProject`) | 3D piping project settings and the `Piping` part |
| `PnP3dObjectsMgd.dll` | `Autodesk.ProcessPower.PnP3dObjects` (`Part`, `Pipe`, `Port`, `PortCollection`, `ConnectionManager`, `ConnectionIterator`, `Pair`, `SpecPart`, `PipingObjectAdder`, `Equipment`) | 3D parts, ports, connectivity, placing parts |
| `PnIdProjectPartsMgd.dll` | `PnIdProject` (confirm the namespace in `plantsdk_ref.chm`) | P&ID project settings. ADN LAB 1 notes this DLL is sometimes **missing from the SDK** and must be taken from the `acad.exe` folder |
| `PnIDMgd.dll` | `Autodesk.ProcessPower.PnIDObjects` (`LineSegment`, `Asset`, `LineGroupManager`, `InlineEntity`) | P&ID line segments, assets, line groups |
| `PnPCommonDbxMgd.dll` | `Autodesk.ProcessPower.PnPCommonDbx` (`FormatStringUtils`); `Autodesk.ProcessPower.Styles` (`LineStyle`, `Style`) | Format strings / annotation; P&ID styles |
| `PnPCommonMgd.dll` | shared Plant types | Common helpers (include if the compiler asks for it) |
| Parts Repository assembly (often `PnP3dPartsRepository.dll` — **confirm the filename** in your `Inc-x64` folder) | `Autodesk.ProcessPower.PartsRepository.Specification` (`SpecManager`, `PipePartSpecification`, `SpecPartReader`); `Autodesk.ProcessPower.PartsRepository.Catalog` | Read / query piping specs and catalogs |
| `PnP3dOrthoProjectPart.dll` | `OrthoProject` (mentioned in ADN LAB 1 comments) | Orthographic project part — confirm in your SDK |

AutoCAD managed DLLs (also **Copy Local = False**):

- `accoremgd.dll`, `acdbmgd.dll`, `acmgd.dll`
- From the AutoCAD / Plant 3D install folder, the ObjectARX `inc-x64` folder, or NuGet `AutoCAD.NET` **25.1.0** with `ExcludeAssets="runtime"` (AutoCAD 2026 / .NET 8).

### What you can automate

| Area | What the API is good for | Limits to know |
|---|---|---|
| **Project / Project Manager data** | Current project, part names (`Piping`, `PnId` / `PnID`, `Iso`, `Ortho`), drawing lists, some settings (`GapWidth`, `MinimumPipeLength`, …) | ADN LAB 1: **no APIs for Project Manager UI** operations |
| **Data Manager / properties** | `GetAllProperties` / `GetProperties` / `SetProperties`; export to CSV | Broken DWG↔row links throw; some properties are calculated or read-only |
| **3D piping** | Walk `Part` ports; `ConnectionManager.IsConnected`; `ConnectionIterator` | Creating / routing pipe is supported in SDK samples (`CreatePipeline`, `PipeRouting`) but is easy to get wrong — start from those samples |
| **Equipment** | Community-documented helpers such as `EquipmentHelper`, `NozzleInfo`, `PipingObjectAdder` | Confirm types in `plantsdk_ref.chm` and SDK samples |
| **P&ID** | Read line segments, styles, line groups, assets, tags, properties | Tool Palette has **no insert API**. A plain `BlockReference` is only graphics until it is classed and linked (Autodesk Support article, Jul 2026) |
| **Specs / catalogs** | `SpecManager.SelectParts`, `PipePartSpecification.OpenSpecification`, read tables | Autodesk Community: there is **no Spec Editor SDK**. Do not write `.pspx` / `.pcat` as raw SQLite |
| **Isometrics** | Project part `Iso` / `IsoProject`, iso config, PCF / result DWGs after the fact | Community answer (2025): **generating an iso drawing is not done through the .NET API**; use the Iso server / product commands |
| **Orthographics** | Autodesk Support: updating Plant ortho views requires the **Plant 3D SDK**, not the plain AutoCAD or ACC API | The Support article does not name the update method — look it up in `plantsdk_ref.chm` |
| **Reports / MTO** | Property export from the API; or use **Report Creator** (no code) | Report Creator remains the supported path for issued MTOs with cut lengths |

---

## Version notes (Plant 3D 2026 / AutoCAD family)

Official AutoCAD compatibility tables ([Managed .NET](https://help.autodesk.com/cloudhelp/2026/ENU/AutoCAD-Customization/files/GUID-A6C680F2-DE2E-418A-A182-E4884073338A.htm), [ObjectARX](https://help.autodesk.com/cloudhelp/2026/ENU/AutoCAD-Customization/files/GUID-C21B8F00-C7DE-4E44-8006-D5DC99199F31.htm)):

| Product year | Release number (Series) | .NET target | ObjectARX / AutoCAD.NET |
|---|---|---|---|
| AutoCAD / Plant 3D **2025** | 25.0 (`R25.0`) | **.NET 8** | AutoCAD 2025 SDK |
| AutoCAD / Plant 3D **2026** | 25.1 (`R25.1`) | **.NET 8** through 2026 Update **1.1**; **.NET 10** from 2026 Update **1.2** | AutoCAD 2026 or 2025 SDK |
| AutoCAD **2027** | 26.0 (`R26.0`) | **.NET 10** | ObjectARX 2027; Visual Studio 2026 |

Practical rules for **Plant 3D 2026**:

- Start with a **.NET 8** class library (`net8.0-windows`), **x64**, Visual Studio **2022**.
- If your installed Plant 3D 2026 is Update **1.2 or later**, Autodesk moved the product to **.NET 10**. A .NET 8 plugin may fail to load; retarget or keep a separate .NET 10 build.
- Match the **year** of the Plant SDK to the Plant 3D year you run. Do not mix 2025 DLLs into a 2026-only install unless Autodesk's compatibility table says that SDK is supported.
- AutoCAD 2024 and earlier use **.NET Framework 4.8**, not .NET 8. Those projects are a different template (`Class Library (.NET Framework)`).
- Plant vertical DLLs (`PnP*.dll`) come from the **Plant install or Plant SDK**, not from the AutoCAD.NET NuGet package.

---

## Where to download

1. **ObjectARX for AutoCAD SDK** (install this first):
   - [ObjectARX for AutoCAD SDK](https://aps.autodesk.com/developer/overview/objectarx-autocad-sdk)
   - Accept the ObjectARX license, then download the year that matches Plant 3D.
2. **Plant 3D ObjectARX SDK**:
   - [Plant 3D ObjectARX SDK downloads](https://aps.autodesk.com/developer/overview/autocad-plant-3d-objectarx-sdk-downloads)
   - License page (required before install): [Plant 3D ObjectARX licensing](https://aps.autodesk.com/developer/overview/autocad-plant-3d-objectarx-licensing)
   - Covers Plant 3D 2020 through 2026 on that license page (and later years as Autodesk updates it).
3. You also need a licensed **AutoCAD Plant 3D** install (the SDK is not the product).

Older ADN landing page (may redirect): `https://www.autodesk.com/developer-network/platform-technologies/autocad-p-id-and-plant-3d`.

---

## Install locations

Exact folder names vary by installer. On a typical Windows PC:

- Plant 3D / AutoCAD itself:
  - `C:\Program Files\Autodesk\AutoCAD 2026\`
  - `C:\Program Files\Autodesk\AutoCAD 2026\PLNT3D\`
- Plant SDK inc folder (Autodesk Community, 2025):
  - `..\AutoCAD Plant 3D 2026 SDK\Inc-x64`
  - Example: `C:\Program Files\Autodesk\AutoCAD Plant 3D 2026 SDK\Inc-x64` **or** a folder you chose when unzipping, such as `C:\Autodesk\AutoCAD Plant 3D 2026 SDK\Inc-x64`
- ObjectARX inc folder (typical):
  - `C:\ObjectARX 2026\inc-x64` (confirm after you install)
- Content / custom Python part scripts (product content, not the SDK):
  - `C:\AutoCAD Plant 3D 2026 Content\CPak Common\CustomScripts\`
  - `C:\AutoCAD Plant 3D 2026 Content\CPak Common\equipment\`

**What to do:** browse those folders and point `AcadDir` / `PlantSdkInc` in `samples/Directory.Build.props` at the folders that actually contain the DLLs.

---

## Visual Studio project setup (beginner)

You want a **Class Library** that Plant 3D can `NETLOAD`. You are not making a `.exe`.

1. Install **Visual Studio 2022** with the **.NET desktop development** workload, plus the **.NET 8** SDK.
2. Install ObjectARX, then the Plant 3D SDK (same year as your Plant 3D).
3. In Visual Studio: **File → New → Project**.
4. Choose **Class Library** that targets **.NET** (Windows).  
   **Do not** choose **Class Library (.NET Framework)** for Plant 3D 2025/2026.
5. Framework: **.NET 8.0**. After the project is created, set the target to `net8.0-windows` (Project Properties → Application, or edit the `.csproj`).
6. Set **Platform target** to **x64** (not AnyCPU).
7. Add a framework reference to Windows desktop (needed because AutoCAD's managed API uses WinForms):

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.WindowsDesktop.App" />
</ItemGroup>
```

8. Add references to `accoremgd`, `acdbmgd`, `acmgd` and the Plant DLLs in the table above.
9. For every AutoCAD / Plant reference: select the reference → Properties → **Copy Local = False**. In an SDK-style `.csproj` that is `<Private>false</Private>`.
10. Optional: NuGet `AutoCAD.NET` version **25.1.0**, with `ExcludeAssets="runtime"`, instead of HintPath references to the three AutoCAD DLLs. You still must HintPath-reference the Plant `PnP*.dll` files.
11. Build (**Build → Build Solution**). The output is a `.dll` under `bin\Debug\net8.0-windows\`.
12. Start **Plant 3D** (not vanilla AutoCAD). Open a **project** in Project Manager, then open a drawing.
13. Command line: `NETLOAD` → pick your DLL → run a command such as `P3DPROJECT`.

A complete `.csproj` is in [`samples/Plant3dSdkSamples.csproj`](samples/Plant3dSdkSamples.csproj). Edit the paths in [`samples/Directory.Build.props`](samples/Directory.Build.props) first.

Debug tips:

- Plant 3D is `acad.exe` started with `/product PLNT3D` (copy the target from the Plant 3D desktop shortcut).
- In Visual Studio: **Debug → Attach to Process** → `acad.exe`, or set that exe as the debug start program.
- After you change code, close Plant 3D (a .NET plugin usually cannot be unloaded), rebuild, then start Plant 3D again.

---

## Load with NETLOAD or an autoloader bundle

### NETLOAD (first tests)

- Command: `NETLOAD`
- Browse to `samples\bin\Debug\net8.0-windows\Plant3dSdkSamples.dll`
- Then type a command name, for example `P3DPROJECT`

### Autoloader bundle (keeps the plugin loaded)

Folder layout:

```
%APPDATA%\Autodesk\ApplicationPlugins\Plant3dSdkSamples.bundle\
  PackageContents.xml
  Contents\
    Plant3dSdkSamples.dll
    Plant3dSdkSamples.deps.json
```

- Per-user path: `%APPDATA%\Autodesk\ApplicationPlugins\`
- All-users path: `%PROGRAMDATA%\Autodesk\ApplicationPlugins\`
- A starter `PackageContents.xml` is in [`samples/PackageContents.xml`](samples/PackageContents.xml).
- Use a **new GUID** for `ProductCode` if you copy the file.
- `Platform="Plant3D"` is the value used in Autodesk Community Plant 3D examples.
- `SeriesMin` / `SeriesMax`:
  - Plant 3D 2025: `R25.0`
  - Plant 3D 2026: `R25.1`
- Autodesk recommends setting **both** `SeriesMin` and `SeriesMax` for .NET plugins from AutoCAD 2025 onward, so a newer release does not load an incompatible DLL.
- `LoadOnCommandInvocation="True"` loads the DLL when the user first types one of your commands (good for samples). `LoadOnAutoCADStartup="True"` loads every time Plant 3D starts.
- Copy the built DLL (and `.deps.json` if present) into `Contents\`, then restart Plant 3D.

---

## How Plant data is structured (short)

- The DWG holds geometry. Tags, size, spec, service, and most Data Manager fields live in a **project database**.
- Local projects use SQLite files such as `ProcessPower.dcf` (P&ID), `Piping.dcf` (3D), `Iso.dcf`, `Ortho.dcf`, plus `Project.xml`.
- Collaboration projects can use SQL Server or Autodesk Docs. Your .NET code still goes through `DataLinksManager`; do not edit those files by hand.
- Entry chain (ADN labs):

```csharp
PlantProject plant = PlantApplication.CurrentProject;   // null if no project is open
Project piping = plant.ProjectParts["Piping"];
Project pid    = plant.ProjectParts["PnId"];            // labs also use "PnID"
DataLinksManager dlm = piping.DataLinksManager;
PnPDatabase pdb = dlm.GetPnPDatabase();
```

- `PlantProject.ProjectFolderPath` is the project folder (Autodesk Community samples).
- `Project.ProjectDirectory` and `Project.GetPnPDrawingFiles()` are the part folder and drawing list (ADN LAB 1).
- Three IDs you will see:
  - AutoCAD `ObjectId` — this session, this drawing
  - Row id (`int`, often called PnPID) — the stable database row
  - `PpObjectId` — Plant's cross-drawing id
- Convert with `FindAcPpRowId`, `MakeAcPpObjectId`, `FindAcPpObjectIds`, `MakeAcDbObjectId`. One row can map to more than one DWG object (for example a symbol and its annotation).

---

## Sample commands in this folder

Build and `NETLOAD` [`samples/Plant3dSdkSamples.csproj`](samples/Plant3dSdkSamples.csproj), with a Plant project open.

| Command | File | What it does |
|---|---|---|
| `P3DPROJECT` | `Commands_Project.cs` | Lists project parts, folders, and drawing files |
| `P3DSETTINGS` | `Commands_Project.cs` | Reads a few `PnIdProject` / `PipingProject` settings |
| `P3DPROPS` | `Commands_Properties.cs` | Pick a Plant object; print all properties |
| `P3DSETPROP` | `Commands_Properties.cs` | Pick an object; set one existing property |
| `P3DPIPEWALK` | `Commands_Piping.cs` | Ports, connection state, connected row ids |
| `P3DLINEGROUP` | `Commands_Pid.cs` | Walk a P&ID `LineSegment` and its line group |
| `P3DEXPORTCSV` | `Commands_ExportCsv.cs` | Export model-space Plant properties to a CSV |
| `P3DSPECLIST` | `Commands_Specs.cs` | List parts of one type from a named spec |

Creating new intelligent objects:

- **3D pipe / fittings:** use the official SDK samples `Samples\Piping\CreatePipeline` and `Samples\Piping\PipeRouting`. The published pattern is `SpecManager.SelectParts` → `SpecPart` → `new Pipe()` → `new PipingObjectAdder(DataLinksManager3d.Get3dManager(dlm), db).Add(...)`. See comments in `Commands_Specs.cs`. Do not ship a placer until you have compared it with the SDK sample on your year.
- **P&ID symbols:** Autodesk Support (Jul 2026) says: the Tool Palette has no API; insert + assign the P&ID class + assign properties + register/link to the project database. The article does **not** publish the exact method names — use `plantsdk_ref.chm` and the SDK `Samples\PnID` folder.

---

## Other languages (what is actually supported)

- **C# / VB.NET Plant SDK** — the supported way to read and write Plant properties and to place intelligent Plant objects.
- **AutoLISP / scripts** — fine for AutoCAD-level work (layers, title blocks, simple commands). Autodesk Community: you can read only a **limited** amount of Plant data (much of it is not in the DWG), and you **cannot write Plant properties** via LISP.
- **VBA / COM / pyautocad** — same limit: AutoCAD COM sees drawing objects, not the Plant project database. Do not plan a pyautocad Data Manager editor.
- **Python custom part scripts** — a different Python. These are scripts Plant 3D's content engine runs to build **catalogue / equipment geometry** (`varmain.primitiv`, `@activate`, …), not a general automation API. Official series: [Custom Python Scripts for AutoCAD Plant 3D – Part 1](https://blog.autodesk.io/custom-python-scripts-for-autocad-plant-3d-part-1/). This repo also has examples under `code/plant3d/custom-part-scripts/`.
- **Your Anaconda / CPython** — useful *after* a .NET command has written a CSV (Excel reports, pandas). It is not a substitute for the Plant SDK.
- **Dynamo** — not available in Plant 3D (AutoCAD-based; no Dynamo host).
- **Core Console** — does not run Plant 3D commands.

---

## Troubleshooting

- **`PlantApplication.CurrentProject` is null** — no Plant project is open. Open one in Project Manager first.
- **Command is unknown after NETLOAD** — you loaded the DLL in vanilla AutoCAD, or the build is AnyCPU / x86, or the class is not `public` / missing `[CommandMethod]`.
- **`FileLoadException` / mixed-mode / wrong framework** — you built .NET Framework 4.8 for a 2025+ product, or .NET 8 for a 2026 Update 1.2+ (.NET 10) install, or x86 instead of x64.
- **`Could not load file or assembly 'PnP…'`** — Copy Local was left **true**, or the DLL lives under `PLNT3D\` and .NET did not probe that folder. Set Copy Local = False. If needed, resolve from the AutoCAD and `PLNT3D` folders in `IExtensionApplication.Initialize` (see `PluginApp.cs`).
- **`Autodesk.ProcessPower.DataLinks.DLException: ObjectDoesNotHaveLink`** — the entity is not a Plant-linked object, or you used the wrong project part (`Piping` vs `PnId`). Catch the exception; do not call `FindAcPpRowId` on every model-space entity without a guard. Community code also uses `dlm.HasLinks(objectId)` — confirm that helper in your `plantsdk_ref.chm`.
- **`FailedToReturnProperties`** — the DWG handle ↔ row link is broken (can happen after AUDIT / RECOVER). `PLANTAUDIT` may delete broken items. Autodesk Community discusses this around `GetAllProperties`.
- **Wrong properties / empty list** — you asked the P&ID `DataLinksManager` about a 3D part, or the other way around. Labs use `"Piping"` and `"PnId"` / `"PnID"`.
- **SetProperties seems to do nothing** — the property is calculated or read-only (for example many size / catalog description fields); or the value is not in the Project Setup list; or you forgot to save. Prefer `SetProperties` over raw `PnPRow` edits so annotations stay consistent.
- **Plugin does not update after rebuild** — .NET assemblies stay loaded. Close Plant 3D, rebuild, reopen.
- **Bundle does not load** — check `Platform="Plant3D"`, SeriesMin/Max vs your release, `ModuleName` path, and that the `.bundle` folder is under `ApplicationPlugins`. Autodesk Community has many Plant 3D `PackageContents.xml` threads.
- **Creating objects "works" but Data Manager is empty** — you inserted a plain AutoCAD block, not an intelligent Plant object.

---

## Official links

- [Plant 3D and P&ID API overview](https://aps.autodesk.com/developer/overview/autocad-plant-3d-and-pid)
- [Plant 3D ObjectARX SDK downloads](https://aps.autodesk.com/developer/overview/autocad-plant-3d-objectarx-sdk-downloads)
- [Plant 3D ObjectARX license](https://aps.autodesk.com/developer/overview/autocad-plant-3d-objectarx-licensing)
- [ObjectARX for AutoCAD SDK](https://aps.autodesk.com/developer/overview/objectarx-autocad-sdk)
- [Create a New AutoCAD .NET Project](https://help.autodesk.com/cloudhelp/2025/ENU/OARX-DevGuide-Managed/files/GUID-43564EB9-F843-4771-823C-573495EE23E0.htm) (use the ENU path if the language in the URL differs)
- [Managed .NET compatibility (release ↔ .NET)](https://help.autodesk.com/cloudhelp/2026/ENU/AutoCAD-Customization/files/GUID-A6C680F2-DE2E-418A-A182-E4884073338A.htm)
- [ObjectARX compatibility](https://help.autodesk.com/cloudhelp/2026/ENU/AutoCAD-Customization/files/GUID-C21B8F00-C7DE-4E44-8006-D5DC99199F31.htm)
- [PackageContents.xml format](https://help.autodesk.com/cloudhelp/2026/ENU/AutoCAD-Customization/files/GUID-BC76355D-682B-46ED-B9B7-66C95EEF2BD0.htm)
- [Inserting P&ID symbols with C#](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Autodesk-AutoCAD-Plant-3D--How-to-Insert-P-ID-Tool-Palette-Symbols-Using-C-Sharp.html)
- [Ortho updates need the Plant SDK](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Is-it-possible-to-update-AutoCAD-Plant-3D-ortho-views-by-the-API-of-AutoCAD-or-ACC-API.html)
- [Customization options (LISP, .NET, scripts, APS)](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/Autodesk-AutoCAD-Plant-3D-Customization-and-Automation-Options-for-Engineering-Workflows.html)
- [ADN Plant3DTraining labs (C#)](https://github.com/ADN-DevTech/Plant3DTraining)
- [ADN DevBlog — Plant 3D](https://adndevblog.typepad.com/autocad/plant3d/)
- [Custom Python part scripts, part 1](https://blog.autodesk.io/custom-python-scripts-for-autocad-plant-3d-part-1/)
- Forums: [AutoCAD Plant 3D](https://forums.autodesk.com/t5/autocad-plant-3d-forum/bd-p/330) · [AutoCAD .NET](https://forums.autodesk.com/t5/net/bd-p/152)

---

## Could not fully verify (check on your install)

These are documented as uncertain on purpose:

- The exact default unzip path of the 2026 Plant SDK (only the `AutoCAD Plant 3D 202x SDK\Inc-x64` pattern is consistently reported).
- The exact filename of the Parts Repository / Spec assembly (namespace `Autodesk.ProcessPower.PartsRepository.Specification` is confirmed; the `.dll` name can differ).
- The namespace of `PnIdProject` (the class and `PnIdProjectPartsMgd.dll` are confirmed).
- The precise meaning of the `bool` argument to `GetAllProperties` (ADN LAB 3 calls it the "current version" flag).
- Exact `PipingObjectAdder.Add` overloads and the required `Pipe` properties (start/end, diameter). Use `Samples\Piping\CreatePipeline`.
- Exact .NET method names for "assign P&ID class and link to the project database".
- Exact .NET method names to update or create ortho views, or to create an isometric drawing (community: isos go through the Iso server, not a public create-iso API).
- Whether `DataLinksManager.HasLinks` is in every year's SDK (widely used on Autodesk Community).
