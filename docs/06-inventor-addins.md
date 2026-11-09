# 06 — Inventor add-ins (C# / VB.NET): manifest, ribbon, deployment, debugging

An add-in is a .NET class library that **Inventor loads into its own process** at start-up. It is how
professional tools put buttons on the ribbon, react to events and run at full API speed.

## Contents
- [How Inventor loads an add-in](#how-inventor-loads-an-add-in)
- [Create the project](#create-the-project)
- [The .addin manifest, line by line](#the-addin-manifest-line-by-line)
- [Where to put the files (2026)](#where-to-put-the-files-2026)
- [StandardAddInServer: Activate and Deactivate](#standardaddinserver-activate-and-deactivate)
- [Commands and buttons](#commands-and-buttons)
- [Ribbons, tabs and panels](#ribbons-tabs-and-panels)
- [Icons in .NET 8](#icons-in-net-8)
- [Windows and dialogs](#windows-and-dialogs)
- [Events in an add-in](#events-in-an-add-in)
- [Debugging with F5](#debugging-with-f5)
- [Dependencies and DLL conflicts](#dependencies-and-dll-conflicts)
- [Porting a 2024 (.NET Framework) add-in to 2026](#porting-a-2024-net-framework-add-in-to-2026)
- [Packaging and deployment](#packaging-and-deployment)
- [Checklist before you give it to colleagues](#checklist-before-you-give-it-to-colleagues)

## How Inventor loads an add-in

```
Inventor start
 └─ scans the add-in folders for *.addin  (XML manifests)
     └─ for each manifest that matches this Inventor version (SupportedSoftwareVersion...)
         ├─ loads <Assembly> (your DLL, .NET 8)
         ├─ finds the class whose [Guid] == <ClassId>
         ├─ creates it, calls Activate(site, firstTime)
         │    → you create ButtonDefinitions, ribbon controls, subscribe to events
         └─ on exit/unload → Deactivate()  → you clean up
```

Since **Inventor 2024, only registry-free add-ins exist** — no `regasm`, no registry keys. Registry add-ins
were hidden from the Add-In Manager from 2022 and fully retired in 2024 (Autodesk developer blog,
*Inventor 2026 API* article).

## Create the project

Option A — **Autodesk template** (recommended first time): run `DeveloperTools.msi` from
`C:\Users\Public\Documents\Autodesk\Inventor 2026\SDK\` (close VS first) → VS → *New Project* → search
"Inventor" → *Autodesk Inventor AddIn* (C# or VB). It generates `StandardAddInServer`, a `.addin` file
and a GUID.

Option B — **this repo's** `07-MyFirstAddin` (no template needed). Its `.csproj`:

```xml
<PropertyGroup>
  <TargetFramework>net8.0-windows</TargetFramework>
  <OutputType>Library</OutputType>
  <UseWindowsForms>true</UseWindowsForms>
  <ComVisible>true</ComVisible>
  <DeployDir>$(APPDATA)\Autodesk\ApplicationPlugins\MyFirstAddin</DeployDir>
</PropertyGroup>
<ItemGroup>
  <Reference Include="Autodesk.Inventor.Interop">
    <HintPath>$(InventorBin)\Autodesk.Inventor.Interop.dll</HintPath>
    <EmbedInteropTypes>false</EmbedInteropTypes>
    <Private>false</Private>            <!-- Inventor has it loaded already -->
  </Reference>
</ItemGroup>
<ItemGroup>
  <None Update="MyFirstAddin.addin"><CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory></None>
</ItemGroup>
<Target Name="DeployToInventor" AfterTargets="Build" Condition="'$(OS)'=='Windows_NT' And '$(SkipDeploy)'!='true'">
  <MakeDir Directories="$(DeployDir)" />
  <Copy SourceFiles="$(TargetPath);$(OutDir)MyFirstAddin.addin" DestinationFolder="$(DeployDir)" />
</Target>
```

Notes:
- **Copy Local of the interop:** this repo uses `Private=false` (Inventor already has its interop loaded). Several community guides (e.g. Jelte de Jong's 2025 migration guide) use *Copy Local = True*. Both work in practice; if Inventor reports it cannot load `Autodesk.Inventor.Interop`, switch to True.
- .NET 8 SDK builds into `bin\Debug\net8.0-windows\` — an extra folder. Don't point the output *directly* at an add-in folder; **copy** after build (the `DeployToInventor` target).

## The .addin manifest, line by line

```xml
<Addin Type="Standard">
  <ClassId>{2ECD787D-3623-49EC-8830-C0052D21C63A}</ClassId>      <!-- = [Guid] on StandardAddInServer -->
  <ClientId>{2ECD787D-3623-49EC-8830-C0052D21C63A}</ClientId>    <!-- id used for UI objects; same GUID is fine -->
  <DisplayName>My First Add-in (CAD Automation guide)</DisplayName>
  <Description>Ribbon tab with Export PDF, Part Info and Parameter Editor commands.</Description>
  <Assembly>MyFirstAddin.dll</Assembly>                          <!-- relative to the .addin file, or absolute -->
  <LoadOnStartUp>1</LoadOnStartUp>
  <UserUnloadable>1</UserUnloadable>
  <Hidden>0</Hidden>
  <SupportedSoftwareVersionGreaterThan>28..</SupportedSoftwareVersionGreaterThan>
  <DataVersion>1</DataVersion>
  <UserInterfaceVersion>1</UserInterfaceVersion>
</Addin>
```

| Element | Meaning | Tip |
|---|---|---|
| `Type` | `Standard` (desktop) / `Plugin` (Design Automation) / `Translator` | |
| `ClassId` | GUID of the server class | **must** equal `[Guid("...")]` — `tools/test_tools.py` checks this |
| `ClientId` | GUID passed as `ClientId` when creating buttons/tabs | |
| `Assembly` | DLL path | relative path = portable |
| `LoadOnStartUp` | 1 = load at start | 0 = load on demand from the Add-In Manager |
| `SupportedSoftwareVersionGreaterThan` / `LessThan` / `EqualTo` | Inventor **major** version numbers: 2024 = 28, **2025 = 29, 2026 = 30, 2027 = 31** | `28..` = 2025 and newer = the .NET 8 generation |
| `UserInterfaceVersion` | increase when you change your ribbon layout | forces Inventor to rebuild your UI instead of using its saved customisation |
| `Hidden` | hide from Add-In Manager | keep 0 while learning |

## Where to put the files (2026)

| Scope | Folder | Admin rights? |
|---|---|---|
| Per user, version **independent** | `%APPDATA%\Autodesk\ApplicationPlugins\<YourAddin>\` | no — **use this while developing** |
| Per user, version dependent | `%APPDATA%\Autodesk\Inventor 2026\Addins\` | no |
| All users, version independent | `%ALLUSERSPROFILE%\Autodesk\Inventor Addins\` (= `C:\ProgramData\Autodesk\Inventor Addins\`) | yes (installers) |
| All users, version dependent | `%PROGRAMFILES%\Autodesk\Inventor 2026\Bin\Addins\` | yes — **changed in 2024** (was `%ALLUSERSPROFILE%\Autodesk\Inventor 20xx\Addins\`) |

Sources: Autodesk developer blog (*Inventor 2026 API* update, location change since 2024) and Jelte de
Jong, *Migration addins to .Net core 8*. The `.addin` and DLL can sit in a sub-folder per add-in.

"Version independent" + `SupportedSoftwareVersionGreaterThan 28..` = one install that loads in 2025,
2026 and 2027 — but **not** 2024 (which needs a .NET Framework build).

## StandardAddInServer: Activate and Deactivate

```csharp
[Guid("2ECD787D-3623-49EC-8830-C0052D21C63A")]
[ComVisible(true)]
public class StandardAddInServer : ApplicationAddInServer
{
    internal static Inventor.Application App = null!;

    public void Activate(ApplicationAddInSite site, bool firstTime)
    {
        App = site.Application;
        try
        {
            // create ButtonDefinitions (every load), ribbon controls, events
        }
        catch (Exception ex) { MessageBox.Show("MyFirstAddin failed to load:\n" + ex); }
    }

    public void Deactivate()
    {
        // unsubscribe events, delete definitions, release COM
        Marshal.ReleaseComObject(App); App = null!;
        GC.Collect(); GC.WaitForPendingFinalizers();
    }

    public void ExecuteCommand(int commandID) { }      // obsolete, keep empty
    public object? Automation => null;                 // expose an API to other programs/iLogic if you want
}
```

- `firstTime` = true the first time the add-in loads with this `UserInterfaceVersion` → create ribbon controls *persistently*. This repo rebuilds controls on every load and on *Reset Ribbon* instead — simpler and robust.
- Wrap everything in `try/catch`: an exception escaping `Activate` disables the add-in, and escaping an event handler can crash Inventor.
- `Automation`: return an object (COM-visible class) to let iLogic or EXEs call your add-in (`ApplicationAddIns.ItemById(guid).Automation`).

## Commands and buttons

`Commands/AddinCommand.cs` is a small base class; each command is ~30 lines:

```csharp
Definition = app.CommandManager.ControlDefinitions.AddButtonDefinition(
    DisplayName,                 // "Export PDF"
    InternalName,                // "CadAuto_ExportPdf" — unique across ALL add-ins: prefix it
    CommandTypesEnum.kQueryOnlyCmdType,
    StandardAddInServer.ClientId,
    Tooltip, Tooltip,            // description, tooltip
    small, large,                // IPictureDisp 16x16 and 32x32
    ButtonDisplayEnum.kAlwaysDisplayText);
Definition.OnExecute += ctx => { try { Execute(); } catch (Exception ex) { MessageBox.Show(ex.Message); } };
```

- `CommandTypesEnum`: `kQueryOnlyCmdType` (reads), `kShapeEditCmdType` (modifies geometry), `kFilePropertyEditCmdType`, ... Inventor uses this for undo grouping and enabling.
- Enable/disable: `Definition.Enabled = false` (e.g. no document open).
- Long-running commands: show progress via `App.StatusBarText` or a `ProgressBar` (`App.CreateProgressBar(...)`).

## Ribbons, tabs and panels

| Ribbon name | Shown when |
|---|---|
| `ZeroDoc` | no document open |
| `Part` | part (incl. sheet metal) |
| `Assembly` | assembly |
| `Drawing` | drawing |
| `Presentation` | .ipn |

```csharp
Ribbon ribbon = App.UserInterfaceManager.Ribbons["Part"];
RibbonTab tab = ribbon.RibbonTabs.Add("CAD Automation", "id_Tab_CadAutomation", ClientId, "", false, false);
RibbonPanel panel = tab.RibbonPanels.Add("Tools", "id_Panel_CadAutomation_Tools", ClientId, "", false);
panel.CommandControls.AddButton(Definition, true /*large*/, true /*show text*/, "", false);
```

Add to an **existing** tab instead: `ribbon.RibbonTabs["id_TabTools"]` (internal names of Inventor tabs:
find them by looping `RibbonTabs` and printing `InternalName`).

*Reset Ribbon* (user right-click) removes add-in controls → handle `UserInterfaceEvents.OnResetRibbonInterface` and rebuild (`StandardAddInServer.OnResetRibbon`).

## Icons in .NET 8

`AddButtonDefinition` wants `stdole.IPictureDisp`. The old trick (`Microsoft.VisualBasic.Compatibility.VB6.Support`) does not exist in .NET 8. Options:

1. **AxHost helper** (this repo): a tiny subclass of `System.Windows.Forms.AxHost` exposes the protected static `GetIPictureDispFromPicture(Image)`. Works with `<UseWindowsForms>true</UseWindowsForms>`.
2. `OleCreatePictureIndirect` P/Invoke (oleaut32).
3. Reference `stdole.dll` from the Inventor `Bin` folder when you need the `IPictureDisp` type itself.

Icons: 16×16 and 32×32 PNG as embedded resources (`<EmbeddedResource Include="Resources\*.png" />`),
loaded with `Assembly.GetManifestResourceStream`. This sample draws letter icons at runtime so it has no image files.

## Windows and dialogs

- **Modal dialog:** `form.ShowDialog(new WindowWrapper(App.MainFrameHWND))` — parent = Inventor's main window, so the dialog stays on top and centred (`ParameterEditorCommand.cs`).
- **Modeless:** `form.Show(owner)` — keep a reference; COM calls still happen on Inventor's thread (Inventor's UI thread runs your event handlers).
- **Dockable window:** `App.UserInterfaceManager.DockableWindows.Add(ClientId, "id_MyPane", "My Pane")` → `AddChild(hwnd)` of a WinForms `UserControl`. Good for property panels; more work.
- Do not block Inventor for minutes: process in chunks, update `App.StatusBarText`, or offload pure computation (not COM) to a background task.

## Events in an add-in

```csharp
_appEvents = App.ApplicationEvents;                  // keep a field reference or the GC may drop the sink
_appEvents.OnSaveDocument += OnSave;

private void OnSave(_Document doc, EventTimingEnum timing, NameValueMap ctx, out HandlingCodeEnum handling)
{
    handling = HandlingCodeEnum.kEventNotHandled;
    if (timing != EventTimingEnum.kBefore) return;   // kBefore = 4097, kAfter = 4098
    try { StampIProps(doc); } catch { /* never throw from an event */ }
}
```

- Store the events object in a **field**; unsubscribe in `Deactivate`.
- `kBefore` handlers can cancel (`kEventCanceled`) — use with care.
- Events fire for **every** document, including those opened invisibly by other tools. Filter by type/path.

## Debugging with F5

1. `Properties/launchSettings.json` (or *Debug → <project> Debug Properties → New profile → Executable*):
   ```json
   { "profiles": { "Inventor 2026": {
       "commandName": "Executable",
       "executablePath": "C:\\Program Files\\Autodesk\\Inventor 2026\\Bin\\Inventor.exe" } } }
   ```
2. **F5** → build → copy to `ApplicationPlugins` → Inventor starts with the debugger attached → breakpoints in `Activate` and commands hit.
3. First start after a new GUID: Inventor may show *"add-in blocked/untrusted"* → *Tools → Add-ins* → select → untick *Block*, set *Load automatically*.
4. **Stop debugging kills Inventor** — save your work first.
5. Edit-and-Continue for small changes works for .NET 8 in many cases; otherwise close Inventor, rebuild (DLL is locked while loaded).
6. Attach instead: start Inventor normally → VS *Debug → Attach to Process* → `Inventor.exe` → code type **Managed (.NET Core, .NET 5+)**.

## Dependencies and DLL conflicts

Your add-in shares Inventor's process with every other add-in. If you reference `Newtonsoft.Json 13.0.3`
and Inventor (or another add-in) already loaded 12.0.1, you may get `FileLoadException` or odd behaviour.

| Strategy | Effort |
|---|---|
| Use what .NET 8 gives you (`System.Text.Json`, `System.Net.Http`) | none — preferred |
| Keep dependencies few and common | low |
| Copy dependencies next to your DLL; avoid strong-name conflicts | low |
| Isolate with a custom `AssemblyLoadContext` (see Tyler Warner, *AssemblyLoadContext for Inventor addins*) | medium — for big dependency trees |
| Move heavy work out of process (EXE/service) and talk via files/pipes | medium |

`BinaryFormatter` is gone in .NET 9+/Inventor 2027 — never use it for settings; use JSON.

## Porting a 2024 (.NET Framework) add-in to 2026

1. VS 17.8+; install *.NET Upgrade Assistant* extension (optional) or create a new SDK-style project and copy code.
2. `TargetFramework` → `net8.0-windows`, `UseWindowsForms`/`UseWPF` as needed.
3. Re-add references: `Autodesk.Inventor.Interop.dll` from `...\Inventor 2026\Bin\`, `stdole.dll` if used; iLogic: `Autodesk.iLogic.Interfaces.dll`, `Autodesk.iLogic.Runtime.dll`.
4. NuGet `System.Drawing.Common` if you use `System.Drawing` outside WinForms.
5. Replace `Marshal.GetActiveObject` (only EXEs), `Microsoft.VisualBasic.Compatibility` icon code, `BinaryFormatter`, `AppDomain` tricks, `System.Web`.
6. Remove `regasm` post-build steps and registry code; make sure the `.addin` is copied.
7. `SupportedSoftwareVersionGreaterThan` → `28..`.
8. Re-create the debug launch profile (the upgrade assistant drops it).

Official guide: Inventor API Help → *Port .Net Framework-based projects to .Net*.

## Packaging and deployment

| Method | For | How |
|---|---|---|
| **Copy folder** | you, 2–5 colleagues | `MyFirstAddin\` with DLL + .addin → `%APPDATA%\Autodesk\ApplicationPlugins\` (a `.bat` or PowerShell script does it) |
| **Network share** | team | `.addin` with absolute `<Assembly>` path to `\\server\tools\...` — VERIFY your IT allows loading .NET from shares; trusting remote code is a policy decision |
| **Installer (MSI/Inno Setup/WiX)** | company | install to `%PROGRAMFILES%\...` + `.addin` into an all-users folder |
| **Autodesk App Store `.bundle`** | public | `MyAddin.bundle\PackageContents.xml` + `Contents\` — the App Store installer places it under `ApplicationPlugins` |

A per-user install script:

```powershell
$dst = "$env:APPDATA\Autodesk\ApplicationPlugins\MyFirstAddin"
New-Item -ItemType Directory -Force $dst | Out-Null
Copy-Item .\MyFirstAddin.dll, .\MyFirstAddin.addin $dst -Force
Get-ChildItem $dst | Unblock-File        # files from e-mail/zip are "blocked" by Windows
```

`Unblock-File` matters: a DLL downloaded in a zip carries the *Mark of the Web*, and .NET may refuse to
load it ("operation is not supported" / `NotSupportedException`).

## Checklist before you give it to colleagues

- [ ] `ClassId` = `[Guid]`; new GUID per add-in (never copy the sample's GUID into a real product).
- [ ] Internal names prefixed (`MyCo_ExportPdf`).
- [ ] Every event handler and `Execute` wrapped in try/catch.
- [ ] `Deactivate` unsubscribes and releases.
- [ ] Works with no document open (`ZeroDoc`), with a drawing, with a sheet-metal part, in a Japanese/English UI (use internal names, not display names).
- [ ] Version in `AssemblyInformationalVersion` and shown in the Add-In Manager description.
- [ ] Tested by loading from the *deployed* folder, not only from F5.
- [ ] Git tag for the release ([docs/14](14-best-practices.md)).

Next: [07 — Python and PowerShell](07-python-powershell.md).
