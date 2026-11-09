# 05 — C# and VB.NET standalone EXEs for Inventor

An EXE is your own program that **drives** Inventor over COM. It is the best way to learn C# with
Inventor: full debugger, fast edit-run cycle, no Inventor restart.

## Contents
- [Project setup from zero](#project-setup-from-zero)
- [Connecting to Inventor from .NET 8](#connecting-to-inventor-from-net-8)
- [C# in 15 minutes for Inventor users](#c-in-15-minutes-for-inventor-users)
- [Pattern: open, work, close — safely](#pattern-open-work-close--safely)
- [Hidden Inventor and batch jobs](#hidden-inventor-and-batch-jobs)
- [Excel without Excel: ClosedXML](#excel-without-excel-closedxml)
- [User interface: console vs. WinForms](#user-interface-console-vs-winforms)
- [VB.NET version](#vbnet-version)
- [Scheduling: run at night](#scheduling-run-at-night)
- [The projects](#the-projects)

## Project setup from zero

Visual Studio → *Create a new project* → **Console App** (C#, *not* ".NET Framework") → .NET 8.0.
Then edit the `.csproj` (double-click the project):

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>   <!-- -windows: COM + WinForms available -->
    <PlatformTarget>x64</PlatformTarget>                <!-- Inventor is 64-bit -->
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="Autodesk.Inventor.Interop">
      <HintPath>C:\Program Files\Autodesk\Inventor 2026\Bin\Autodesk.Inventor.Interop.dll</HintPath>
      <EmbedInteropTypes>false</EmbedInteropTypes>
    </Reference>
    <Compile Include="..\Shared\InventorConnector.cs" Link="InventorConnector.cs" />
  </ItemGroup>
</Project>
```

In this repo the `HintPath` comes from `code/inventor/Directory.Build.props` (`$(InventorBin)`), so the
projects work for any Inventor version by changing one line.

Key points:
- Interop DLL location for 2025/2026: **`...\Inventor 2026\Bin\Autodesk.Inventor.Interop.dll`**. Older guides mention `Bin\Public Assemblies\` — that was the .NET Framework layout.
- `EmbedInteropTypes=false` (Autodesk recommendation for .NET 8 projects). For an EXE, *Copy Local* may stay default (true): the interop DLL is copied next to your EXE, which is fine — it is only the type definitions, not Inventor.
- **Do not** add Inventor through *Add COM Reference*: that generates a new interop assembly that conflicts with the official one.

## Connecting to Inventor from .NET 8

.NET Framework had `Marshal.GetActiveObject("Inventor.Application")`. **.NET 5+ removed it.** The fix
(Autodesk developer blog, *"Marshal.GetActiveObject Replacement in .NET 8+"*) is to call the two Win32
functions it wrapped:

```csharp
[DllImport("ole32.dll")]
static extern int CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

[DllImport("oleaut32.dll")]
static extern int GetActiveObject([MarshalAs(UnmanagedType.LPStruct)] Guid rclsid, IntPtr reserved,
                                  [MarshalAs(UnmanagedType.IUnknown)] out object ppunk);
```

`code/inventor/csharp/Shared/InventorConnector.cs` wraps this:

```csharp
Inventor.Application app = InventorConnector.Connect(startIfNotRunning: true, visible: false);
```

- Running Inventor → attaches.
- Not running → `Activator.CreateInstance(Type.GetTypeFromProgID("Inventor.Application"))` starts one (takes 20–60 s).
- **Elevation must match.** If Visual Studio runs *as administrator* and Inventor does not (or the other way round), the Running Object Table is separate → "not running". Run both normally.
- Two Inventors open → you get one of them (the first registered). Close the extra one.

## C# in 15 minutes for Inventor users

```csharp
// variables: type name = value;
double module = 2.0;            // mm
int teeth = 40;
string name = $"Gear_m{module}_z{teeth}";    // string interpolation

// cast the general Document to the specific type
if (app.ActiveDocument is PartDocument part)          // pattern matching: test + cast in one
{
    PartComponentDefinition def = part.ComponentDefinition;
    foreach (Parameter p in def.Parameters)           // COM collections work with foreach
        Console.WriteLine($"{p.Name,-12} = {p.Expression}");
}

// methods
static double PitchDiameter(double m, int z) => m * z;   // "=>" = return this expression

// lists and LINQ (filter/sort like Excel)
var big = files.Where(f => new FileInfo(f).Length > 10_000_000).OrderBy(f => f).ToList();

// errors
try { doc.Save(); }
catch (COMException ex) { Console.WriteLine($"Save failed: 0x{ex.HResult:X8} {ex.Message}"); }
```

| Concept | Engineer translation |
|---|---|
| `namespace` | folder for class names |
| `class` | a drawing template: defines what an object has and can do |
| object | one drawing made from the template |
| `static` | belongs to the template, not one drawing — e.g. `GearMath.Compute(...)` |
| `interface` (`PartDocument`) | a contract: "anything that is a part has these members" — Inventor gives you interfaces |
| `record` | a small data row, e.g. `record BomLine(int Level, string Item, ...)` in `05-BomToExcel` |
| `?` after a type (`string?`) | may be empty (null) — the compiler warns you to check |
| `var` | "the compiler knows the type" — fine when the right side shows it |

Recommended reading order of this repo's C#: `01-HelloInventor` → `02-SpurGearGenerator/GearMath.cs`
(pure C#, no Inventor) → `02/Program.cs` → `03-BatchExport`.

## Pattern: open, work, close — safely

```csharp
Inventor.Application app = InventorConnector.Connect();
bool oldSilent = app.SilentOperation;
app.SilentOperation = true;                         // no dialogs (e.g. "can't find reference")
try
{
    foreach (string path in Directory.EnumerateFiles(folder, "*.idw", SearchOption.AllDirectories))
    {
        Document? doc = null;
        try
        {
            doc = app.Documents.Open(path, false);   // invisible
            Work(doc);
            log.WriteLine($"{path},ok");
        }
        catch (Exception ex)
        {
            log.WriteLine($"{path},error,{ex.Message.Replace(',', ';')}");   // one bad file never stops the batch
        }
        finally
        {
            doc?.Close(true);                        // ALWAYS close; true = skip save prompt
        }
    }
}
finally { app.SilentOperation = oldSilent; }
```

Extras for long batches:
- `app.Documents.CloseAll(true)` every N files if you open references (sub-assemblies stay in memory).
- `app.ScreenUpdating = false` with a visible Inventor.
- Log to CSV → open in Excel → filter errors. `03-BatchExport` writes `export_log.csv`.

## Hidden Inventor and batch jobs

```csharp
var app = InventorConnector.Connect(startIfNotRunning: true, visible: false);
try { /* work */ }
finally { if (startedByUs) app.Quit(); }
```

- A hidden Inventor still consumes a licence.
- If your EXE crashes, the hidden Inventor keeps running → Task Manager → end `Inventor.exe`. Always `Quit()` in `finally` if you started it.
- Project file (.ipj) matters for references: `app.DesignProjectManager.DesignProjects.AddExisting(@"C:\Vault\Designs.ipj").Activate(true)` before opening files (cannot switch with documents open; `AddExisting` returns the already-registered project if it is known — VERIFY behaviour on your setup).

## Excel without Excel: ClosedXML

COM-automating Excel from a COM-automated Inventor = two slow, fragile processes. Use a .NET library:

```xml
<PackageReference Include="ClosedXML" Version="0.105.0" />
```

```csharp
using var wb = new XLWorkbook();
var ws = wb.AddWorksheet("BOM");
ws.Cell(1, 1).Value = "Item";
ws.Row(1).Style.Font.Bold = true;
ws.Columns().AdjustToContents();
ws.SheetView.FreezeRows(1);
wb.SaveAs(outFile);
```

Used in `04-IPropertyBulkEditor` (read + write) and `05-BomToExcel` (formatted export). Python's
equivalent is **openpyxl** ([docs/07](07-python-powershell.md)).

## User interface: console vs. WinForms

| Need | Use |
|---|---|
| batch tool, logs | Console app (this repo's 01–05, 08) |
| a few inputs + button | WinForms (`06-FrameConfigurator`) — set `<UseWindowsForms>true</UseWindowsForms>` and `[STAThread]` |
| modern look, data binding | WPF (`<UseWPF>true</UseWPF>`) — more to learn |

`06-FrameConfigurator/MainForm.cs` builds the form **in code** (no designer file): easy to read in Git
and to modify. The Inventor work runs on the UI (STA) thread — COM objects expect that — and progress is
reported through `Progress<string>` into a log text box. Do not move Inventor COM calls to `Task.Run`
background threads: every call is then marshalled back anyway, and some objects fail outright.

## VB.NET version

`code/inventor/vbnet/BatchExportVB` is the PDF exporter in VB.NET. Why it matters: iLogic is VB.NET, so VB
code moves between an iLogic rule and a compiled project with minimal changes:

```vb
<STAThread>
Function Main(args As String()) As Integer
    Dim app = InventorConnector.Connect()
    Dim pdf = CType(app.ApplicationAddIns.ItemById(PdfAddinId), TranslatorAddIn)
    app.SilentOperation = True
    For Each f In IO.Directory.GetFiles(src, "*.idw")
        Dim doc As Document = Nothing
        Try
            doc = app.Documents.Open(f, False)
            ' ... TranslationContext, NameValueMap, DataMedium, pdf.SaveCopyAs(doc, ctx, opts, dm)
        Catch ex As Exception
            Console.WriteLine("FAIL " & f & ": " & ex.Message)
        Finally
            If doc IsNot Nothing Then doc.Close(True)
        End Try
    Next
End Function
```

VB `.vbproj` uses the same `Directory.Build.props`, `net8.0-windows`, interop reference.

## Scheduling: run at night

```powershell
$act = New-ScheduledTaskAction -Execute "C:\Tools\BatchExport\BatchExport.exe" `
       -Argument '"D:\Projects\P-2411" --pdf --recurse --out "D:\Export\P-2411"'
$trg = New-ScheduledTaskTrigger -Daily -At 22:00
Register-ScheduledTask -TaskName "Inventor PDF export" -Action $act -Trigger $trg -RunLevel Limited
```

- *Run only when user is logged on* — Inventor needs an interactive desktop session; running as a service / without login is unsupported and usually fails.
- Make sure no modal dialog can appear (`SilentOperation`), and log everything.

## The projects

| Project | Learn |
|---|---|
| [01-HelloInventor](../code/inventor/csharp/01-HelloInventor/README.md) | connect, iterate, read |
| [02-SpurGearGenerator](../code/inventor/csharp/02-SpurGearGenerator/README.md) | modelling API, splines, patterns, parameters, transactions |
| [03-BatchExport](../code/inventor/csharp/03-BatchExport/README.md) | translators, robust batch loop |
| [04-IPropertyBulkEditor](../code/inventor/csharp/04-IPropertyBulkEditor/README.md) | property sets, Excel round trip |
| [05-BomToExcel](../code/inventor/csharp/05-BomToExcel/README.md) | BOM API, recursion |
| [06-FrameConfigurator](../code/inventor/csharp/06-FrameConfigurator/README.md) | WinForms, part creation, matrices |
| [08-ApprenticeReader](../code/inventor/csharp/08-ApprenticeReader/README.md) | Apprentice (no Inventor) |
| [BatchExportVB](../code/inventor/vbnet/BatchExportVB/README.md) | same in VB.NET |

Next: [06 — add-ins](06-inventor-addins.md).
