# 00 — Getting started: install, versions, first build

## Add-in learning levels

Start with [the from-zero workbook](19-addin-learning-from-zero.md). Treat the machine/version tables below as the repository's recorded setup, not an inspection of your computer.

### Beginner — identify the tools
Your first task is to locate Visual Studio, your Plant 3D installation and the downloaded repository. Write down the product/update shown in Plant's About dialog. Find a .cs source file and a .csproj project file. Read workbook B1–B2 before changing SDK paths. **Checkpoint:** explain which program edits code and which program runs the add-in.

### Transition — build one project
Open Plant3DDataTools.csproj, resolve references for your actual host using [chapter 18](18-plant3d-addin-development.md), and build just that project. Find its DLL in the build output and record its full path. **Checkpoint:** distinguish a compiler error from a DLL-loading error. You do not need to build every Inventor project to begin Plant development.

### Intermediate — make setup reproducible
Record the tested product/update, target framework, reference locations and build command. Repeat the build and NETLOAD in a fresh Plant session. **Exercise:** explain what another learner would have to change on a different computer. **Answer:** machine paths and possibly host/SDK versions, not arbitrary source-code changes.


> Goal of this page: in one evening, go from "Inventor 2026 + Visual Studio installed" to "I built and
> ran three different kinds of automation". Plant 3D setup (you do not have it yet) is at the end.

## Contents
- [Your machine, checked](#your-machine-checked)
- [.NET versions in one table](#net-versions-in-one-table)
- [Install checklist](#install-checklist)
- [Clone this repo and set two paths](#clone-this-repo-and-set-two-paths)
- [First hour: three wins](#first-hour-three-wins)
- [Visual Studio for a mechanical engineer](#visual-studio-for-a-mechanical-engineer)
- [Getting AutoCAD Plant 3D](#getting-autocad-plant-3d)
- [Help files and where the truth lives](#help-files-and-where-the-truth-lives)

## Your machine, checked

| Item | You have | Needed for | OK? |
|---|---|---|---|
| Inventor | 2026 | everything in Inventor | ✔ |
| Visual Studio | 2022 Community 17.14, *.NET desktop development* | C#/VB.NET EXE + add-ins | ✔ (17.8+ needed for .NET 8) |
| .NET SDK | 9 | building | ✔ — SDK 9 builds `net8.0-windows` targets fine |
| .NET 8 **runtime** (Desktop) | installed by Inventor 2026 | running EXEs targeting net8.0 | check: `dotnet --list-runtimes` shows `Microsoft.WindowsDesktop.App 8.x` |
| Python | Anaconda | pywin32 scripts, reports | ✔ — make a separate env |
| PowerShell | 5.1 (Windows) | `.ps1` samples | ✔ — PowerShell 7 optional |
| AutoCAD Plant 3D | **not installed** | Plant 3D chapters | see [Getting AutoCAD Plant 3D](#getting-autocad-plant-3d) |
| Excel | optional | nothing in this repo *requires* Excel | ClosedXML / openpyxl write .xlsx without Excel |

Check in a terminal (*Windows Terminal* or *Developer PowerShell for VS 2022*):

```powershell
dotnet --info                       # SDKs and runtimes
dotnet --list-runtimes              # need Microsoft.WindowsDesktop.App 8.0.x
& "C:\Program Files\Autodesk\Inventor 2026\Bin\Inventor.exe" /?   # just proves the path exists
conda --version
```

## .NET versions in one table

The single most confusing thing for a beginner in 2026: **which .NET?**

| Product | Runtime inside the product | Build your DLL/EXE as | Notes |
|---|---|---|---|
| Inventor 2023, 2024 | .NET Framework 4.8 | `net48` | old tutorials (most of YouTube) are this |
| **Inventor 2025, 2026** | **.NET 8** | **`net8.0-windows`** | add-ins *must* be .NET 8; Framework 4.8 add-ins do not load |
| Inventor 2026.4 update (Aug 2026) | .NET 10 | `net8.0-windows` generally still loads | runtime moved, API unchanged; test your add-ins, recompile only if hit by a .NET 10 breaking change (Autodesk blog *Desktop Products 2025/2026: .NET 10 Updates*) |
| Inventor 2027 | .NET 10 | `net8.0-windows` or `net10.0-windows` | binary compatible with .NET 8 add-ins; `BinaryFormatter` removed |
| AutoCAD / Plant 3D 2025, 2026 | .NET 8 | `net8.0-windows` | AutoCAD 2026.1.2 update moves to .NET 10 |
| AutoCAD 2027 | .NET 10 | `net10.0-windows` (NuGet `AutoCAD.NET` 26.x) | |
| Inventor Design Automation (cloud) 2026 engine | .NET 8 only | `net8.0` | 2025 had both a Framework and a `_Net8` engine |

Practical rules for this repo:
- Everything targets **`net8.0-windows`**: loads in Inventor 2025/2026/2027 and AutoCAD 2025/2026.
- .NET 8 and .NET 9 *themselves* reach end of Microsoft support on **10 Nov 2026**. Autodesk products ship their own runtime, so your add-in keeps working; plan to retarget to `net10.0-windows` when you move to 2027 products.
- Your **SDK** (9) is only the compiler. It can build for 8. You do not need SDK 8 installed.
- `Marshal.GetActiveObject` **does not exist** in .NET 8 → use `code/inventor/csharp/Shared/InventorConnector.cs` ([docs/05](05-csharp-vbnet-exe.md#connecting-to-inventor-from-net-8)).

## Install checklist

Do these once, in order.

1. **Visual Studio Installer → Modify** → workloads: *.NET desktop development* (have it). Individual components: *.NET 8.0 Runtime* (if missing). Optional: *Git for Windows*.
2. **Inventor SDK developer tools.** Inventor installs the SDK files here:
   `C:\Users\Public\Documents\Autodesk\Inventor 2026\SDK\`
   - Close Visual Studio, run **`DeveloperTools.msi`** → installs the *Inventor Add-In* project templates (C#, VB) into `Documents\Visual Studio 2022\Templates\ProjectTemplates` plus API samples.
   - `UserTools.msi` (same folder) installs small utilities and samples for end users — optional.
3. **VBA module** (only if you want VBA): VBA is no longer installed with Inventor — download *Microsoft VBA module for Inventor 2026* from Autodesk (support article "Download the Microsoft VBA module for Inventor"), close Inventor, install.
4. **Inventor Apprentice Server 2026** (only for `08-ApprenticeReader`): since 2026 it is a separate download and registry-free ([docs/08](08-apprentice-design-automation-vault.md#apprentice-server)).
5. **Python env** (Anaconda Prompt):
   ```bat
   conda create -n cad python=3.12 pywin32 openpyxl -y
   conda activate cad
   pip install pyautocad
   python -c "import win32com.client, openpyxl; print('ok')"
   ```
6. **Git** (`winget install Git.Git`) and a GitHub account — this repo is meant to live in version control ([docs/14](14-best-practices.md#version-control-for-cad-code)).

## Clone this repo and set two paths

```powershell
cd C:\
git clone https://github.com/aniltheengh/cad-automation-inventor-plant3d.git CAD-Automation
cd CAD-Automation
```

Two MSBuild properties control every .NET project (set once, in one file each):

| File | Property | Default | Change if |
|---|---|---|---|
| `code/inventor/Directory.Build.props` | `InventorVersion` / `InventorBin` | `2026` / `C:\Program Files\Autodesk\Inventor 2026\Bin` | other version or drive |
| `code/plant3d/Directory.Build.props` | `PlantSdkInc` | `C:\Autodesk\Plant3D 2026 SDK\inc-x64` | where you unzipped the Plant SDK |

`Directory.Build.props` is a feature of MSBuild: every project file *below* that folder imports it
automatically. So upgrading to Inventor 2027 = change **one number**.

Build everything:

```powershell
pwsh -File tools\build_all.ps1          # or: Windows PowerShell 5.1 → powershell -File ...
```

## First hour: three wins

| # | What | Language | Time | Proves |
|---|---|---|---|---|
| 1 | Run an iLogic rule | iLogic (VB.NET) | 10 min | you can automate *inside* Inventor with zero setup |
| 2 | Run `01-HelloInventor` | C# EXE | 20 min | VS + .NET 8 + COM works |
| 3 | Load `07-MyFirstAddin` | C# add-in | 30 min | you can put your own button on the ribbon |

**Win 1 — iLogic (no Visual Studio at all):**
1. New part (metric template). *Manage → iLogic → Add Rule* → name `Shaft`.
2. Paste `code/inventor/ilogic/03_Shaft_Generator.iLogicVb` → **Save & Run**.
3. A stepped shaft appears. Edit the diameter/length table at the top, run again.

**Win 2 — C# EXE:**
1. Open a part in Inventor.
2. VS → *File → Open → Project/Solution* → `code\inventor\csharp\01-HelloInventor\HelloInventor.csproj`.
3. **F5**. The console lists parameters and iProperties.
4. Put a breakpoint (F9) on a line in the loop, F5 again, hover over variables. This is how you *learn* the API.

**Win 3 — add-in:**
1. Close Inventor. Open `07-MyFirstAddin\MyFirstAddin.csproj`. **F5** — Visual Studio builds, copies files to `%APPDATA%\Autodesk\ApplicationPlugins\MyFirstAddin\`, starts Inventor.
2. If Inventor asks whether to load a blocked add-in: *Tools → Add-ins* → select → untick *Block*, tick *Loaded*.
3. Open a drawing → **CAD Automation** tab → *Export PDF*.

## Visual Studio for a mechanical engineer

You do not need to learn all of Visual Studio. Learn these ten things:

| Key / place | Use |
|---|---|
| **F5** / Shift+F5 | start with debugger / stop (stopping kills Inventor if it was started by F5 — save first) |
| **F9** | breakpoint on the current line |
| **F10 / F11** | step over / step into |
| Hover, **Locals**, **Watch** | inspect live Inventor objects; expand `Parameters`, `PropertySets`… |
| **Immediate window** (Ctrl+Alt+I) | evaluate `doc.DisplayName` while paused |
| **Ctrl+.** | quick fix: add `using`, implement interface |
| **F12** | go to definition — on an Inventor type this opens the interop metadata: every member and its signature |
| **Object Browser** (Ctrl+Alt+J) | browse `Autodesk.Inventor.Interop` like a catalogue |
| **Error List** | read the first error only, fix, rebuild; later errors are often consequences |
| `launchSettings.json` / *Debug properties* | "start external program" = `Inventor.exe` for add-in debugging |

Reading a C# error as an engineer: `CS0266 Cannot implicitly convert type 'Inventor.Document' to 'Inventor.PartDocument'` means "you have the generic thing, cast to the specific thing" — `(PartDocument)doc` — exactly like knowing a drawing view is a *section* view before asking for its section line.

## Getting AutoCAD Plant 3D

Plant 3D is **not a separate product to buy** any more. It is one of the specialised toolsets in
**"AutoCAD — including specialized toolsets"** (AutoCAD, Architecture, Electrical, Mechanical, MEP, Map 3D,
Plant 3D, Raster Design). Options:

| Route | Who | How | Notes |
|---|---|---|---|
| **Free trial** | anyone | autodesk.com → AutoCAD → *Free trial* (the AutoCAD trial includes the toolsets) → Autodesk Access / installer → pick *AutoCAD Plant 3D* | normally 30 days; one trial per product per account |
| **Education licence** | students/teachers of a qualifying institution | autodesk.com/education → verify → download AutoCAD Plant 3D | 1 year, renewable; not for commercial work |
| **Company subscription** | your employer has *AutoCAD including specialized toolsets* or the *AEC Collection* | Autodesk Account → *Products and services* → AutoCAD Plant 3D | ask your BIM/CAD manager to assign it |
| **P&ID only** | | AutoCAD P&ID is inside Plant 3D (same install) | Plant 3D includes P&ID |

**Version warning:** AutoCAD 2027 (with the *Plant 3D toolset 2027*) is already released, and a trial
normally installs the **newest** release. 2027 runs on **.NET 10**. This repo targets 2026 (.NET 8) by
default; for a 2027 trial build the AutoCAD/Plant projects with `dotnet build -p:AcadRelease=2027`
(switches to `net10.0-windows` + NuGet `AutoCAD.NET` 26.0.0) — that needs the **.NET 10 SDK**
(`winget install Microsoft.DotNet.SDK.10`) alongside your SDK 9. Also download the matching *Plant 3D 2027 SDK*.
Subscription and education users can usually pick previous versions (2026) in their Autodesk Account.

Plan your trial: 30 days goes fast. Do **docs 10 and 12** (plain AutoCAD .NET, LISP, scripts — the trial
or even your company AutoCAD covers those) *before* starting the Plant 3D trial, then use the trial days
for [docs/11](11-plant3d-sdk.md) only.

**Plant 3D SDK** (for the .NET Plant API): free download from Autodesk (Autodesk Platform Services /
ADN *Plant 3D SDK* page, per release). It contains the `inc-x64` reference DLLs, docs (`.chm`), and
samples. The ADN training labs on GitHub (`ADN-DevTech/Plant3DTraining`) are the best learning material.

## Help files and where the truth lives

| Need | Source | Remark |
|---|---|---|
| Inventor API object reference | Inventor → *Help (?) dropdown → Community Resources → Programming/API Help*, or the online API help | the object model diagram PDF is in the SDK folder |
| Inventor API in Japanese | "日本語版 Inventor 2026 API プログラミング用ヘルプ" (Autodesk Japan developer blog) | good when English is slow |
| What changed in 2026 | API help → *What's New in the Inventor API* | read once per release |
| iLogic snippets | iLogic editor → *System Snippets* (left pane), right-click → *Help* | |
| AutoCAD .NET | *AutoCAD .NET Developer's Guide* (help.autodesk.com) | |
| Plant 3D API | SDK `.chm` + `ADN-DevTech/Plant3DTraining` labs + Autodesk forum "AutoCAD Plant 3D Customization" | **thin**; expect to read samples, not docs |
| Forums | forums.autodesk.com → *Inventor Programming – iLogic, Macros, AddIns & Apprentice* | search first; most beginner questions are answered |

Full link list: [17-references.md](17-references.md). Next: [01 — choosing a language](01-choosing-a-language.md).
