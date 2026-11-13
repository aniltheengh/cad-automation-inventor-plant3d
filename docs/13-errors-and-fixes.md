# 13 — Errors and fixes (Inventor, AutoCAD, Plant 3D)

## Add-in learning levels

### Beginner — identify when it failed
Use three questions: did it fail while building, while loading the DLL, or after typing the command? Write down the exact first error and the action that caused it. Work through [workbook B5](19-addin-learning-from-zero.md). **Checkpoint:** fix a deliberate missing semicolon and explain why rebuilding was necessary.

### Transition — inspect rather than guess
Set a breakpoint before the failing operation, run the command and inspect the document, project, selection status or property value. Change one thing and repeat the same steps. **Exercise:** a greeting still shows old text after editing source. **Suggested diagnosis:** check whether you rebuilt, loaded the intended DLL and restarted the host. Do not assume the new code actually ran.

### Intermediate — reproduce and report
Reduce a failure to the smallest test drawing and command sequence. Record host/update, build target, exception type/message, failing method and expected versus actual result. Distinguish “no findings” from “inspection failed.” **Checkpoint:** someone else can reproduce the issue from your notes without receiving a production project. Catch exceptions where useful context can be added, rather than hiding them with a success message.


Organised by **where** the error shows up. Each row: message (or symptom) → cause → fix. Many rows
come from building this repo; they are marked ★.

## Contents
- [Reading a COM error](#reading-a-com-error)
- [Build errors (Visual Studio)](#build-errors-visual-studio)
- [Interop type surprises](#interop-type-surprises)
- [Connecting to Inventor](#connecting-to-inventor)
- [Runtime COM errors in Inventor](#runtime-com-errors-in-inventor)
- [Geometry and modelling errors](#geometry-and-modelling-errors)
- [Add-in does not load](#add-in-does-not-load)
- [.NET 8 vs .NET Framework 4.8](#net-8-vs-net-framework-48)
- [iLogic errors](#ilogic-errors)
- [Python / PowerShell errors](#python--powershell-errors)
- [AutoCAD / Plant 3D .NET errors](#autocad--plant-3d-net-errors)
- [Security: blocked files, SECURELOAD, access denied](#security-blocked-files-secureload-access-denied)
- [Export problems](#export-problems)
- [Debugging checklist](#debugging-checklist)

## Reading a COM error

`System.Runtime.InteropServices.COMException (0x80004005): Unspecified error` — the hex **HRESULT** is the useful part:

| HRESULT | Name | Usual meaning in Inventor |
|---|---|---|
| `0x80004005` | `E_FAIL` | "it didn't work": wrong input (open profile, invalid extent, feature failed) |
| `0x80070057` | `E_INVALIDARG` | wrong argument type/value, wrong units, name not found in a collection |
| `0x80020009` | `DISP_E_EXCEPTION` | the call raised an error inside Inventor (see message) |
| `0x8002000B` | `DISP_E_BADINDEX` | index out of range (remember: **1-based**) |
| `0x800401E3` | `MK_E_UNAVAILABLE` | `GetActiveObject`: Inventor not running / different elevation |
| `0x80010001` | `RPC_E_CALL_REJECTED` | Inventor busy (modal dialog, still loading) → retry later |
| `0x80010108` | `RPC_E_DISCONNECTED` | Inventor closed/crashed while you held a reference |
| `0x80040154` | `REGDB_E_CLASSNOTREG` | COM class not registered (Apprentice 2026 not installed, wrong bitness) |
| `0x80070005` | `E_ACCESSDENIED` | file read-only (Vault), folder permission, elevation |

In C#: `catch (COMException ex) { Console.WriteLine($"0x{ex.HResult:X8} {ex.Message}"); }`.

## Build errors (Visual Studio)

| Error | Cause | Fix |
|---|---|---|
| `CS0246 The type or namespace name 'Inventor' could not be found` | interop not referenced / wrong `HintPath` | set `InventorBin` in `Directory.Build.props`; the DLL is in `...\Inventor 2026\Bin\` |
| `MSB3245 Could not resolve this reference` | same | same; check *Dependencies → Assemblies* for a yellow triangle |
| `NETSDK1100 To build a project targeting Windows on this operating system...` | building `-windows` TFM on Linux/macOS | `<EnableWindowsTargeting>true</EnableWindowsTargeting>` (set in this repo's props) |
| `NETSDK1045 The current .NET SDK does not support targeting .NET 10.0` | NuGet package needs a newer SDK (e.g. `AutoCAD.NET.Core 25.1.1` is net10-only) ★ | pin `25.1.0` for AutoCAD 2026 on .NET 8, or install SDK 10 |
| `CS0104 'Environment' is an ambiguous reference between 'Inventor.Environment' and 'System.Environment'` ★ | `using Inventor;` brings Inventor's own `Environment` (and `Color`, `Application`, `File`, `Path`…) | fully qualify `System.Environment`, alias `using IO = System.IO;`, `System.Drawing.Color` |
| `CS0266 Cannot implicitly convert type 'Inventor.Document' to 'Inventor.PartDocument'` | need a cast | `(PartDocument)doc` or `doc is PartDocument p` |
| `CS1061 'Document' does not contain a definition for 'ComponentDefinition'` | base interface | cast to `PartDocument` / `AssemblyDocument` |
| `CS0029 / CS0030 cannot convert PartComponentDefinition to ComponentDefinition` ★ | interop interfaces not related as you expect | branch per document type ([Interop type surprises](#interop-type-surprises)) |
| `CS0246 'BOMRows'` ★ | collection type is `BOMRowsEnumerator` | use `BOMRowsEnumerator` |
| `CS1545 Property ... is not supported by the language; try directly calling accessor method 'get_Units'` | COM property with arguments / unusual accessors | call `p.get_Units()` |
| `CS0012 type 'stdole.IPictureDisp' is defined in an assembly that is not referenced` | icons | reference `stdole.dll` from Inventor `Bin`, or use the AxHost helper (`AddinCommand.cs`) |
| `CS0117 'Marshal' does not contain a definition for 'GetActiveObject'` | .NET 8 removed it | `InventorConnector.cs` ([docs/05](05-csharp-vbnet-exe.md#connecting-to-inventor-from-net-8)) |
| Warning `MSB3270 processor architecture mismatch` | AnyCPU vs x64 | `<PlatformTarget>x64</PlatformTarget>` |
| `The file is locked by: "Autodesk Inventor"` on build | add-in DLL loaded | close Inventor, then build |

## Interop type surprises

★ All found while compiling this repo against the real interop:

- **`PartComponentDefinition` is not usable as `ComponentDefinition`** in C# assignments. Write:
  ```csharp
  Parameters ps = doc switch {
      PartDocument p => p.ComponentDefinition.Parameters,
      AssemblyDocument a => a.ComponentDefinition.Parameters,
      _ => throw new NotSupportedException() };
  ```
- **`ApprenticeServerDocument.SubType`** — not available on the Apprentice document type used; check `DocumentType` instead.
- **Indexed properties** need square brackets in C#: `addin.HasSaveCopyAsOptions[doc, ctx, opts]`, `opts.Value["Sheet_Range"]`.
- **Name clashes** with `using Inventor;`: `Application`, `Environment`, `Color`, `File`, `Path`, `Point`. Use aliases: `using IO = System.IO;`, `using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;`.
- **VB / iLogic** treat Inventor interfaces as late-bindable (Option Strict Off): a misspelt member compiles and fails at run time with *"Public member 'X' on type 'Y' not found"*. Use `Option Strict On` when compiling VB outside iLogic, or our two-pass checker.

## Connecting to Inventor

| Symptom | Cause | Fix |
|---|---|---|
| `Inventor is not running` although it is | elevation mismatch (VS as admin, Inventor not — or reverse) | run both normally |
| | Inventor still starting | wait for the UI, retry |
| | two Windows users (Remote Desktop) | same session only |
| `REGDB_E_CLASSNOTREG` on `Type.GetTypeFromProgID("Inventor.Application")` | Inventor not installed/registered (repair install) | repair Inventor |
| attaches to the "wrong" Inventor | two instances | close one; ROT returns the first |
| EXE hangs at first call | modal dialog in Inventor (licence, "file not found") | close dialogs; `SilentOperation = true` |
| Inventor keeps running after your EXE | you started it and didn't `Quit()` | `try/finally { app.Quit(); }` |

## Runtime COM errors in Inventor

| Where | Symptom | Fix |
|---|---|---|
| `Documents.Open` | `E_FAIL` | path wrong, file from newer Inventor version, or project (.ipj) cannot resolve; check `File.Exists` and the active project |
| `Parameters["x"]` | `E_INVALIDARG` | name doesn't exist (case-sensitive) → loop and print names |
| `PropertySets["..."]["Custom1"]` | `E_INVALIDARG` | custom property missing → `PropertySet.Add(value, name)` (value first!) |
| setting `Parameter.Value` | geometry doesn't change | call `doc.Update()`; value is **cm** |
| `doc.Save()` | `E_ACCESSDENIED` / silently not saved | file read-only (Vault checked in, Windows attribute), or document *not dirty* |
| `Occurrences.Add` | `E_FAIL` | file path wrong; file is the assembly itself (recursion) |
| BOM views | `E_FAIL` on `BOMViews[...]` | enable view first (`StructuredViewEnabled = true`); find by `ViewType`, not localized name |
| `CreateExtrudeDefinition` | `E_FAIL` | profile not closed / self-intersecting; check `sketch.Profiles.Count` |
| any call during an event | `RPC_E_CALL_REJECTED` | don't call back into Inventor from another thread; defer work |
| long batch | memory grows, Inventor slows | `Close(true)` every document; `Documents.CloseAll(true)` periodically; restart Inventor every N hundred files |

## Geometry and modelling errors

| Symptom | Cause | Fix |
|---|---|---|
| everything is 10× too big | mm passed as cm | multiply by 0.1 at the API boundary |
| angles wrong | degrees passed as radians | `deg * Math.PI / 180` |
| ★ parts placed in wrong positions | matrix order: `a.TransformBy(b)` = a **then** b | compose `r = a.Copy(); r.TransformBy(b);` (`FrameBuilder.Compose`) |
| extrude in the wrong direction | `kPositiveExtentDirection` relative to sketch normal | use `kNegativeExtentDirection` or `kSymmetricExtentDirection` |
| circular pattern: "feature could not be created" | tooth/space cut intersects itself at small z | check geometry in 2D first (`tools/test_tools.py` checks the gear tooth-space loop) |
| spline sketch fails | duplicate/near points | remove points closer than 1e-6 cm |
| feature fails after parameter change | geometry impossible (wall thicker than radius) | validate inputs in code before setting (`01_SpurGear_Params` does) |
| work plane index wrong | `WorkPlanes[1..3]` = YZ, XZ, XY | use names (`"XY Plane"`) only in English UI — index is language-safe |

## Add-in does not load

Work down the list:

1. **Is the `.addin` in a scanned folder?** `%APPDATA%\Autodesk\ApplicationPlugins\<name>\` is easiest. All-users version-dependent folder is `%PROGRAMFILES%\Autodesk\Inventor 2026\Bin\Addins\` since 2024.
2. **Is it valid XML?** Open it in a browser; a single `&` breaks it.
3. **Version filter:** `SupportedSoftwareVersionGreaterThan` `28..` means 2025+. A typo (`29.` vs `29..`) can exclude your version.
4. **Assembly path:** relative to the `.addin` file. Does `MyFirstAddin.dll` exist there?
5. **ClassId = `[Guid]`?** `tools/test_tools.py` checks this for the sample.
6. **Blocked:** *Tools → Add-ins* → *Block* checkbox; or Windows *Mark of the Web* → `Unblock-File`.
7. **Wrong framework:** a `net48` DLL in Inventor 2025/2026 → won't load. A `net8.0` DLL in Inventor 2024 → won't load. Build per generation.
8. **Exception in `Activate`:** wrap in try/catch and show it; or attach the debugger and break on all exceptions (*Debug → Windows → Exception Settings → Common Language Runtime Exceptions*).
9. **Dependency missing / conflicting** (`FileNotFoundException` / `FileLoadException`): copy dependencies next to your DLL; see [docs/06](06-inventor-addins.md#dependencies-and-dll-conflicts).
10. **Registry-based add-in from an old tutorial** (uses `regasm`, `[ComRegisterFunction]`): not supported since 2024 → convert to registry-free.

## .NET 8 vs .NET Framework 4.8

| Old (net48) code | Problem in .NET 8 | Replacement |
|---|---|---|
| `Marshal.GetActiveObject` | removed | P/Invoke (`InventorConnector`) |
| `Microsoft.VisualBasic.Compatibility.VB6.Support.ImageToIPictureDisp` | not available | `AxHost.GetIPictureDispFromPicture` helper |
| `BinaryFormatter` | obsolete; removed in .NET 9+ | `System.Text.Json` |
| `AppDomain.CreateDomain` | not supported | `AssemblyLoadContext` |
| `System.Web`, WCF client | not available | `HttpClient`, gRPC/REST |
| `app.config` / `ConfigurationManager` | different model (works via NuGet `System.Configuration.ConfigurationManager`) | JSON settings file |
| `packages.config` | — | `PackageReference` |
| `regasm` post-build | not needed | `.addin` file |
| implicit Windows references | not referenced by default | `-windows` TFM, `UseWindowsForms` / `UseWPF` |

Version map: Inventor 2023/2024 = net48; 2025/2026 = .NET 8 (2026.4 runs .NET 10 but loads net8 add-ins); 2027 = .NET 10. AutoCAD/Plant 3D 2025/2026 = .NET 8; 2027 = .NET 10. .NET 8 and 9 end Microsoft support on 10 Nov 2026 — the products keep shipping their runtime, but plan the move to `net10.0-windows` with 2027.

## iLogic errors

| Message | Cause | Fix |
|---|---|---|
| `Parameter: The parameter named "x" was not found` | name typo / wrong document | `Parameter("Occ:1", "x")` for components |
| `Public member 'X' on type 'Y' not found` | late-bound typo (Option Strict Off) | declare types; run `tools/check_ilogic.py` |
| `Conversion from string "" to type 'Double' is not valid` | empty text parameter / iProperty | `If String.IsNullOrEmpty(...)` before `CDbl` |
| `Object reference not set to an instance of an object` | `Nothing` (e.g. no active document, `ThisDoc.Document` in drawing) | check `Is Nothing` |
| rule runs again and again | rule changes a parameter it also reads → triggers itself | `iLogicVb.UpdateWhenDone`, or disable *Run automatically*, or guard with a flag |
| `GoExcel: file not found` | relative path vs. `ThisDoc.Path` | build full path; or embed the workbook |
| Component suppression does nothing | occurrence name changed by user | stable names; check `Component.IsActive` returns |
| works for you, not colleagues | external rule directory not configured on their PC | deploy iLogic configuration (XML export/import) |

## Python / PowerShell errors

| Message | Fix |
|---|---|
| `pywintypes.com_error: (-2147221021, 'Operation unavailable')` | = `MK_E_UNAVAILABLE` → Inventor not running / elevation |
| `AttributeError: '<win32com.gen_py....Document>' object has no attribute 'ComponentDefinition'` | `CastTo(doc, "PartDocument")` |
| `AttributeError: module 'win32com.gen_py...' has no attribute 'CLSIDToClassMap'` | stale cache → delete `%LOCALAPPDATA%\Temp\gen_py` |
| `ImportError: DLL load failed while importing win32api` (Anaconda) | install pywin32 via conda in the active env; run `pywin32_postinstall.py -install` |
| PowerShell: `Cannot find type [ComRot]` | `Add-Type` failed (C# syntax) → remove `-ErrorAction SilentlyContinue` to see why |
| PowerShell: `...cannot be loaded because running scripts is disabled` | `Set-ExecutionPolicy -Scope CurrentUser RemoteSigned` |
| PowerShell: enum name unknown | use numbers ([docs/07](07-python-powershell.md#enum-values-table)) |

## AutoCAD / Plant 3D .NET errors

| Message / symptom | Cause | Fix |
|---|---|---|
| `Unknown command "ATTEXPORT"` after NETLOAD | no `[CommandMethod]` found: class not public, or `CommandClass` attribute points to the wrong type | public class + public method |
| `FileLoadException: acdbmgd` / commands crash on load | AutoCAD DLLs copied to output | `ExcludeAssets="runtime"` / Copy Local False |
| `eLockViolation` | modifying the DB from a modeless window/event | `using (doc.LockDocument())` |
| `eWasOpenedForRead` / `eNotOpenForWrite` | object opened `ForRead` | `UpgradeOpen()` or open `ForWrite` |
| `eWrongObjectType` | cast to wrong entity type | check `id.ObjectClass.DxfName` first |
| changes lost | transaction not committed | `tr.Commit()` |
| `NullReferenceException` at `PlantApplication.CurrentProject` | no Plant project open, or running in plain AutoCAD | open a project in Project Manager |
| `Could not load file or assembly 'PnP3dObjectsMgd'` | probing path | `AssemblyResolve` handler ([docs/11](11-plant3d-sdk.md#plant-sdk-setup-net-8)) |
| `ProjectParts["PnId"]` returns null | part name spelling (`PnId`/`PnID`) or project has no P&ID part | loop `prj.ProjectParts` and print each part name (VERIFY the collection type in the SDK) / try both spellings |
| `FindAcPpRowId` throws / returns ≤ 0 | not a Plant object (plain line, xref, proxy) | `try/catch`, skip |
| `SetProperties` exception | read-only property, value not allowed, drawing locked by another user | write only editable properties; check project locks |
| Plant objects shown as proxies | object enablers not loaded (plain AutoCAD, Core Console) | open in Plant 3D; in Core Console `arx L PnP3dObjects.dbx` |
| Core Console: `Unknown command` for Plant commands | Plant environment not loaded in Core Console | by design — use full Plant 3D for Plant commands |

## Security: blocked files, SECURELOAD, access denied

| Symptom | Fix |
|---|---|
| Add-in/DLL from a zip won't load (`NotSupportedException`, "operation is not supported", or silently) | `Get-ChildItem -Recurse | Unblock-File` on the folder |
| AutoCAD asks "load this file?" every time | add the folder to `TRUSTEDPATHS`; or deploy to ApplicationPlugins |
| AutoCAD refuses to load anything from a share | `SECURELOAD` = 2 and the share isn't trusted → ask CAD admin |
| AutoCAD 2026: bundle in `C:\ProgramData\Autodesk\ApplicationPlugins` ignored | location obsolete in 2026 → `C:\Program Files\Autodesk\ApplicationPlugins` or `%APPDATA%\Autodesk\ApplicationPlugins` |
| `Access to the path ... is denied` when building | output/deploy folder needs admin (`Program Files`) | deploy to `%APPDATA%` during development |
| Inventor: macros disabled | VBA security level / missing VBA module | install VBA module; *Tools → Options → Macros* |
| iLogic: *"rule cannot be run - untrusted"* or DLL from `AddReference` not found | iLogic security / *iLogic Addin DLLs Directory* | put DLLs in the configured directory |
| corporate antivirus deletes your EXE | new unsigned EXE flagged | sign the binary, or ask IT to whitelist the build folder |

## Export problems

| Symptom | Fix |
|---|---|
| PDF has only the first sheet | `Sheet_Range = kPrintAllSheets` (14082) |
| PDF colours wrong / lines too thick | `All_Color_AS_Black`, `Remove_Line_Weights` |
| DXF layers not as expected | create and pass an export `.ini` (`Export_Acad_IniFile`) |
| flat DXF empty or missing holes | `InteriorProfilesLayer` set, invisible-layer list too broad; check flat pattern in UI |
| STEP is AP203/AP214 but supplier wants AP242 | `ApplicationProtocolType` = 5 (forum-confirmed; verify) |
| export uses old geometry | document needs update → `doc.Update()`; drawing *Defer Updates* off |
| "Translator add-in not found" | GUID typo; or add-in not activated → `addin.Activate()` |

## Debugging checklist

1. Read the **first** error only; fix; rebuild.
2. Reproduce with one file, one part, one command.
3. Print what you have: names in the collection, document type, units, HRESULT.
4. Do it by hand in the UI — does Inventor/AutoCAD allow it?
5. Search the exact message in forums (`site:forums.autodesk.com "<message>"`).
6. Reduce to 10 lines and post on the forum with version + code + full message — people answer good questions fast.

Next: [14 — best practices](14-best-practices.md).
