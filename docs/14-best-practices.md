# 14 — Best practices: version control, testing, deployment, packaging, performance

## Add-in learning levels

### Beginner — keep a recoverable working version
Save your source and keep a brief note of each successful change. Learn that Git tracks text revisions; a build creates output from that text. Use a disposable project for exercises. **Checkpoint:** identify the last version whose behavior you checked. [Workbook's learning record](19-addin-learning-from-zero.md) provides a simple format.

### Transition — separate responsibilities and check results
Keep a command's prompts, data reading and validation understandable as separate steps. Check a normal input, a missing input and cancellation. **Exercise:** does successful compilation prove a report's counts are right? **Answer:** no; compare against known project data. Write installation and command instructions while you still remember the setup.

### Intermediate — release a repeatable tool
Record the tested host/update and dependencies, package only the intended files, restart and verify loading. Test project switches and failure paths, not only a successful demonstration. For writes, verify partial failures and saved/reopened data. **Checkpoint:** another person can follow the instructions and identify both successful output and a failed run. Add UI and performance work after correctness and clear behavior are established.


Automation code is engineering work: it needs revision control, checking and release management like a
drawing. This page is the "how we do it properly" checklist.

## Contents
- [Version control for CAD code](#version-control-for-cad-code)
- [Repository layout](#repository-layout)
- [Separate the maths from the CAD](#separate-the-maths-from-the-cad)
- [Testing without and with the CAD program](#testing-without-and-with-the-cad-program)
- [Code style for engineers](#code-style-for-engineers)
- [Logging and error handling](#logging-and-error-handling)
- [Configuration and secrets](#configuration-and-secrets)
- [Deployment options](#deployment-options)
- [Bundles and PackageContents.xml](#bundles-and-packagecontentsxml)
- [Versioning and release](#versioning-and-release)
- [Performance](#performance)
- [Multi-version support (2025/2026/2027)](#multi-version-support-202520262027)
- [Working with IT and CAD admins](#working-with-it-and-cad-admins)

## Version control for CAD code

- **Git for code, PDM (Vault) for CAD files.** Don't put `.ipt/.iam/.dwg` production data in Git. Small *test models* and *templates* are fine (use Git LFS if they are > a few MB).
- Commit text formats: `.cs`, `.vb`, `.iLogicVb`, `.bas` (export VBA modules), `.lsp`, `.scr`, `.py`, `.ps1`, `.xml`, `.json`.
- `.gitignore` (this repo): `bin/`, `obj/`, `.vs/`, `*.user`, `__pycache__/`, logs, generated check files.
- **Never commit** the Inventor interop DLL, AutoCAD/Plant SDK DLLs (licence terms), passwords, API secrets, customer data.
- Small commits with meaningful messages: `BatchExport: add --flat for sheet metal`, not `update`.
- Branch per feature if two people work on it; otherwise `main` + tags is enough.
- Review your own diff before committing (`git diff`) — it catches debug code and absolute paths.

## Repository layout

```
repo/
  README.md            what, why, how to start
  docs/                knowledge (this kind of material)
  code/<product>/<language>/<project>/   one folder per tool, each with README.md
  tools/               build/check scripts
  config/              .ini export configs, iLogic configuration exports, spec lists
  tests/models/        tiny test parts/drawings (if allowed)
  Directory.Build.props  shared MSBuild settings (product version, paths)
```

## Separate the maths from the CAD

The most important design rule in this repo:

```
GearMath.cs (no Inventor reference)  ←  unit-testable anywhere, reused by EXE + DA plug-in + Python twin
Program.cs  (Inventor API calls only) ←  thin: takes numbers, makes geometry
```

- Engineering logic (thickness formulas, gear geometry, frame member lengths, BOM grouping rules) goes in **plain classes/functions**.
- CAD code only translates results into sketches/features/properties.
- Bonus: the same maths runs in Python (`gear_math.py`) and is cross-checked to 1e-12 by `tools/test_tools.py`.

## Testing without and with the CAD program

| Level | What | How in this repo |
|---|---|---|
| 1. compile | every C#/VB project against the real interop / NuGet | `tools/build_all.sh` / `.ps1` |
| 2. static checks | iLogic rules compile (stubs), Strict-On pass, LISP balance, PowerShell parse, links | `check_ilogic.py`, `lisp_check.py`, `check_powershell.ps1`, `check_links.py` |
| 3. unit tests | pure maths vs. textbook values; C# == Python | `test_tools.py` |
| 4. integration (manual or scripted) | run against **test models** in Inventor/Plant 3D | your PC: keep `tests/models` with expected results (mass, part count, PDF page count) |
| 5. acceptance | an engineer checks the output drawing/BOM | sign-off like a drawing check |

Integration test idea for `03-BatchExport`: folder with 3 drawings + 1 sheet-metal part → run → assert
4 output files exist, PDF page counts right, `export_log.csv` has no `error` rows. That is a 20-line
script and catches most regressions.

## Code style for engineers

- **Units in names**: `lengthMm`, `angleRad`, `massKg`. Convert once (`const double MM = 0.1;`).
- One function = one job; < 50 lines when possible.
- Name things like the engineer would: `ShellThicknessRequired`, not `Calc2`.
- Comments explain **why** (`// UG-27(c)(1) circumferential stress governs for thin shells`), not what.
- No magic numbers: `const string PdfTranslator = "{0AC6FD96-...}";`.
- Fail loudly on bad input (`throw new ArgumentOutOfRangeException(nameof(teeth), "z >= 6")`) — a clear error beats a wrong part.
- Enable `<Nullable>enable</Nullable>` and treat warnings seriously.

## Logging and error handling

- Batch tools: CSV log per run (file, action, status, message, seconds). Engineers can filter it in Excel.
- Add-ins: write to `%LOCALAPPDATA%\<Company>\<Tool>\log.txt` with timestamps; show the user a short message, keep the stack trace in the log.
- Never swallow exceptions silently except in shutdown code; never let them escape event handlers (Inventor/AutoCAD may crash).
- iLogic: `Logger.Info/Warn/Error`; set the log level in iLogic configuration.

## Configuration and secrets

- Paths, standards, layer names → JSON/XML config next to the tool, not hard-coded.
- APS client id/secret (Design Automation) → environment variables, Windows Credential Manager, or a secret store. **Never** in Git, never in the add-in DLL.
- Per-user settings → `%APPDATA%\<Company>\<Tool>\settings.json`.

## Deployment options

| Tool type | Deploy as | Location |
|---|---|---|
| iLogic external rules | shared folder (read-only for users), synced from Git | *iLogic Configuration → External Rule Directories* |
| Inventor add-in | folder with `.addin` + DLLs; script or MSI | `%APPDATA%\Autodesk\ApplicationPlugins\<Tool>\` (per user) or `%PROGRAMFILES%\Autodesk\Inventor 2026\Bin\Addins\` / `%ALLUSERSPROFILE%\Autodesk\Inventor Addins\` (all users) |
| Inventor EXE | folder + shortcut; `dotnet publish -c Release` | `C:\Tools\<Tool>\` |
| AutoCAD/Plant .NET plug-in | `.bundle` | `%APPDATA%\Autodesk\ApplicationPlugins\` or `C:\Program Files\Autodesk\ApplicationPlugins\` (2026) |
| LISP | `.bundle` or startup suite + trusted path | trusted, read-only folder |
| Python | `requirements.txt` / conda `environment.yml`; or PyInstaller EXE | per user |
| Design Automation | AppBundle versions + alias | APS |

`dotnet publish` for EXEs:

```powershell
dotnet publish code\inventor\csharp\03-BatchExport -c Release -r win-x64 --self-contained false -o C:\Tools\BatchExport
```

`--self-contained false` uses the .NET 8 Desktop runtime that Inventor already installed — smaller.

## Bundles and PackageContents.xml

The **autoloader bundle** format (`<Name>.bundle\PackageContents.xml` + `Contents\`) is used by AutoCAD-based
products (incl. Plant 3D), by the Autodesk App Store, and by Design Automation AppBundles. Key elements:

| Element / attribute | Purpose |
|---|---|
| `ApplicationPackage` `ProductCode`, `UpgradeCode`, `AppVersion`, `Name` | identity and version (new `ProductCode` per version, same `UpgradeCode` forever) |
| `RuntimeRequirements OS="Win64" Platform="AutoCAD*" SeriesMin="R25.0" SeriesMax="R25.1"` | which products/releases load it |
| `ComponentEntry ModuleName="./Contents/x.dll"` | the module (.dll, .lsp, .cuix, Inventor `.addin`) |
| `LoadOnAutoCADStartup` / `LoadOnCommandInvocation` + `Commands` | demand loading |

- AutoCAD 2025+: `SeriesMin/SeriesMax` are needed so that .NET Framework (≤ 2024) and .NET 8 (2025+) builds don't load into the wrong release.
- Inventor: App Store packages for Inventor use the same `.bundle` layout with `Platform="Inventor"` and `ModuleName` pointing to the `.addin` (as in this repo's DA bundle). For desktop Inventor deployment, a plain folder with `.addin` + DLL in `ApplicationPlugins` is simpler — VERIFY bundle scanning behaviour of your Inventor release before relying on it.

## Versioning and release

- Semantic version `MAJOR.MINOR.PATCH` in the project (`<Version>1.2.0</Version>`), shown in the tool (About box / log header / Add-In Manager description).
- Git tag per release: `git tag -a v1.2.0 -m "BatchExport: flat pattern DXF"`.
- `CHANGELOG.md` in engineering language: what changed for the user.
- Keep the previous version available for rollback (a folder per version on the share).

## Performance

| Rule | Why |
|---|---|
| In-process for many small calls (iLogic/add-in) | out-of-process COM ~0.1–1 ms per call |
| Read collections once, store values in .NET lists | each `.Item(i)` is a call |
| `app.ScreenUpdating = false`, `SilentOperation = true` in batches | no redraws, no dialogs |
| Open documents invisibly, close them | memory |
| Use `Transaction` for many model edits | one undo step, often faster |
| Defer updates: change all parameters, then `doc.Update()` once | avoid N rebuilds |
| Assemblies: `ComponentOccurrence.Definition` once per unique file; dictionary by `FullFileName` | same part placed 500× |
| AutoCAD: filter by `ObjectId.ObjectClass` before `GetObject` | opening is the expensive part |
| AutoCAD batch: Core Console in parallel | each DWG in its own process |
| Plant 3D: query `PnPDatabase` tables instead of opening DWGs | the project DB has everything |
| Measure: `Stopwatch` around blocks; log seconds per file | optimise what is slow, not what you guess |

## Multi-version support (2025/2026/2027)

- One `Directory.Build.props` per product with the version number → change one line, rebuild.
- `SupportedSoftwareVersionGreaterThan 28..` → one Inventor add-in for 2025–2027 (.NET 8 binaries load in .NET 10 hosts).
- For 2024 or older: separate `net48` build configuration (multi-targeting `<TargetFrameworks>net48;net8.0-windows</TargetFrameworks>` with `#if NET8_0_OR_GREATER` around differences — see Jelte de Jong, *Compile and debug multiple .NET versions*).
- AutoCAD NuGet: 25.1.0 for 2026 on net8; 26.x for 2027 (net10). Bundle `SeriesMin/Max` per build.
- Re-test on each **update** (e.g. Inventor 2026.4 moved to the .NET 10 runtime): run the integration tests.

## Working with IT and CAD admins

- Ask early: Can I deploy to `%APPDATA%`? Is VBA allowed? What are `TRUSTEDPATHS`/`SECURELOAD` policies? Is PowerShell constrained?
- Provide a one-page "what this tool does, where it writes, what it needs" for each tool.
- Sign binaries if your company has a code-signing certificate.
- Prefer per-user installs while piloting; move to all-users installs (MSI) after acceptance.

Next: [15 — projects](15-projects.md).
