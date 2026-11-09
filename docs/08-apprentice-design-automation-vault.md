# 08 — Apprentice, Design Automation (cloud), and Vault

Three ways to work with Inventor data **without** a person sitting in Inventor.

## Contents
- [Apprentice Server](#apprentice-server)
- [Design Automation for Inventor (APS)](#design-automation-for-inventor-aps)
- [Design Automation: the five objects](#design-automation-the-five-objects)
- [Design Automation: step-by-step with the gear plug-in](#design-automation-step-by-step-with-the-gear-plug-in)
- [Design Automation: debugging and cost](#design-automation-debugging-and-cost)
- [Vault: what to know as an automation engineer](#vault-what-to-know-as-an-automation-engineer)

## Apprentice Server

**What:** a free, in-process subset of the Inventor API. No UI, no Inventor licence.

| Can | Cannot |
|---|---|
| read all iProperties; **write** iProperties (`FlushToFile`) | create/modify geometry |
| read file references; repair them (`FileDescriptor.ReplaceReference`) | export PDF/DXF/STEP (no translators) |
| *Save As* a copy with changed references (copy design) | update drawings |
| read BOM-like structure, occurrences, mass props (as stored), geometry (B-rep read-only) | run iLogic |
| thumbnails | |

**2026 change:** Autodesk made Apprentice **registry-free** in 2026 and it is now a separate installer,
*Inventor Apprentice Server 2026*. Inventor View (which used to bring Apprentice to PCs without Inventor)
was discontinued after 2023. Source: Autodesk developer blog, *Inventor 2026 API関連情報* (May 2025).

```csharp
var apprentice = new ApprenticeServerComponent();
ApprenticeServerDocument doc = apprentice.Open(@"D:\Vault\Designs\Frame.iam");
foreach (PropertySet set in doc.PropertySets)
    foreach (Property p in set)
        Console.WriteLine($"{set.DisplayName} | {p.DisplayName} = {p.Value}");

doc.PropertySets["Design Tracking Properties"]["Revision Number"].Value = "B";
doc.PropertySets.FlushToFile();                // write iProperties back
doc.Close();
apprentice.Close();
```

Project `08-ApprenticeReader` adds a recursive reference tree (`ReferencedFileDescriptors`) — the
quickest way to find *missing files* in a project folder.

Practical rules:
- **x64 process.** Apprentice is 64-bit only.
- Set the Inventor **project** before opening: `apprentice.DesignProjectManager...` (same objects as Inventor) — otherwise library/content paths do not resolve.
- Do not `FlushToFile` on files open in Inventor for edit, or on Vault-checked-in (read-only) files.
- VERIFY in 2026: with the registry-free server, `new ApprenticeServerComponent()` depends on how the 2026 installer exposes the COM class. If it fails with `REGDB_E_CLASSNOTREG`, follow the Apprentice section of the 2026 SDK help (activation via manifest).

## Design Automation for Inventor (APS)

**What:** Autodesk runs Inventor's engine (InventorCoreConsole) in the cloud. You upload a plug-in, send
JSON + input files, get output files back. Part of **Autodesk Platform Services** (APS, formerly Forge).

| Topic | Fact |
|---|---|
| Engine for 2026 | `Autodesk.Inventor+2026` — **.NET 8 only** (Autodesk blog: *Inventor Design Automation 2026 Engine Moves to .NET 8*) |
| Older engines | `+2025` (.NET Framework) and `+2025_Net8` |
| List engines | `GET https://developer.api.autodesk.com/da/us-east/v3/engines` (paginated) |
| Plug-in | a normal .NET 8 class library + `.addin` with `Type="Plugin"`, zipped as a `.bundle` |
| Auth | APS app (client id/secret) → 2-legged OAuth token, scope `code:all` (+ `data:read data:write` for OSS) |
| Storage | OSS buckets with signed URLs, or any HTTPS URL you control |
| Billing | Flex tokens / cloud credits per processing time — check the current APS pricing page |
| No UI | no dialogs, no `MessageBox`, no `Application` object — you get `InventorServer` |

Use it when: sales staff or customers configure products in a browser; an ERP needs drawings generated
overnight; you want to sell a configurator without Inventor seats.

## Design Automation: the five objects

```
AppBundle   = your zipped plug-in (versions + alias "prod")
Activity    = "how to run": engine, command line, parameters (inputs/outputs), which AppBundle
WorkItem    = one job: Activity + actual URLs for each parameter
Engine      = Autodesk.Inventor+2026
OSS bucket  = where input/output files live (optional; any signed URL works)
```

Files in `code/inventor/design-automation/GearDAPlugin/`:

| File | Content |
|---|---|
| `PluginServer.cs` | `ApplicationAddInServer`; takes `InventorServer` from `addInSiteObject.InventorServer`; `Automation` returns `GearAutomation` |
| `GearAutomation.cs` | `Run(Document)` and `RunWithArguments(Document, NameValueMap)`; reads `params.json`, models the gear blank, saves `gear.ipt` |
| `GearDAPlugin.Inventor.addin` | `<Addin Type="Plugin">` |
| `bundle/PackageContents.xml` | `RuntimeRequirements OS="Win64" Platform="Inventor"`, `ModuleName="./Contents/GearDAPlugin.Inventor.addin"` |
| `bundle/activity.json` | engine, command line, `params` (get) + `OutputIpt` (put) |
| `bundle/workitem.json` | inline JSON input via `data:application/json,...`, output to a signed URL |

Activity command line:

```text
$(engine.path)\InventorCoreConsole.exe /al "$(appbundles[GearDAPlugin].path)" "$(args[params].path)"
```

- `/al` = load this app bundle. `/i <file>` would open an input document first (then `Run(doc)` gets it).
- Extra positional arguments are delivered to `RunWithArguments` in the `NameValueMap`; this sample expects the first as `_1` and falls back to `params.json` in the working folder. **VERIFY** the key convention against the current APS docs/template — it is the least documented part.

## Design Automation: step-by-step with the gear plug-in

1. **APS app:** aps.autodesk.com → *My Apps* → create app → note client id/secret (store in an environment variable or a secret store — never in Git).
2. **Token:** `POST https://developer.api.autodesk.com/authentication/v2/token` (client credentials, scope `code:all`).
3. **Nickname** (optional): `PATCH /da/us-east/v3/forgeapps/me` — makes ids `nickname.GearDAPlugin+prod` instead of `<clientId>.GearDAPlugin+prod`.
4. **Build + zip** the bundle:
   ```
   GearDAPlugin.bundle/
     PackageContents.xml
     Contents/GearDAPlugin.dll, GearDAPlugin.Inventor.addin
   ```
   Zip the **folder** (`GearDAPlugin.zip` containing `GearDAPlugin.bundle/...`).
5. **Test locally** with InventorCoreConsole from your Inventor 2026 `Bin` folder and the same command line. 90 % of DA problems show up here, for free.
6. **AppBundle:** `POST /da/us-east/v3/appbundles` `{ "id": "GearDAPlugin", "engine": "Autodesk.Inventor+2026" }` → response contains an upload form → upload the zip → `POST /appbundles/GearDAPlugin/aliases` `{ "id": "prod", "version": 1 }`.
7. **Activity:** `POST /da/us-east/v3/activities` with `activity.json` → alias `prod`.
8. **Output URL:** create an OSS bucket + signed upload URL (`/oss/v2/buckets/{bucket}/objects/{object}/signeds3upload`), or use your own storage.
9. **WorkItem:** `POST /da/us-east/v3/workitems` with `workitem.json` → returns `id`.
10. **Poll** `GET /da/us-east/v3/workitems/{id}` until `status` = `success` / `failed*` → download `reportUrl` (the console log, including your `Console.WriteLine`) and the output file.

Tools to send the requests: Postman collections from APS, the official .NET SDK (`Autodesk.Forge.DesignAutomation` NuGet), or the APS *Design Automation tools* VS Code extension. Each new feature you add = new AppBundle **version** + move the alias.

## Design Automation: debugging and cost

| Problem | Where to look |
|---|---|
| `failedInstructions` | report log: command line wrong, bundle not found, exception in plug-in |
| plug-in not loaded | `PackageContents.xml` `ModuleName` path, `.addin` `Type="Plugin"`, `ClassId` = `[Guid]` |
| `.NET` errors on the 2026 engine | built for net48? 2026 engine is .NET 8 only |
| works locally, fails in cloud | absolute paths, fonts, content center/library files not present in the cloud — upload them as inputs or a zip |
| timeouts | work items have a processing time limit; batch less per item |

Cost control: test locally; keep work items short; reuse one AppBundle version per release.

## Vault: what to know as an automation engineer

Most companies with Inventor use **Autodesk Vault** (PDM). Automation meets Vault in four places:

| Situation | What happens | Do this |
|---|---|---|
| your tool edits iProperties | checked-in files are **read-only** in the workspace | check out first (Vault add-in in Inventor), or use Vault's own property editing / Data Standard |
| batch export of drawings | Vault already has a **Job Processor** (PDF/DWG/STEP on lifecycle change) | prefer a Job Processor job over your own nightly EXE |
| file renames/moves | breaks references outside Vault | rename/move **in Vault** (it fixes references) |
| BOM to ERP | Vault Professional has an *ERP integration* path / item master | export from items, not from files |

Programming Vault:
- **Vault SDK** (free, per release: `Autodesk Vault 2026 SDK`) → .NET API (`Autodesk.Connectivity.WebServices`, Vault Development Framework `Autodesk.DataManagement.Client.Framework.Vault`). Explorer extensions, custom jobs, event handlers.
- **Vault Data Standard** — PowerShell + XAML dialogs for file naming and property entry in Inventor/AutoCAD/Vault. Low-code; good first step.
- Community: **coolOrange powerVault / powerJobs** (PowerShell modules; commercial with free tiers) — widely used for custom jobs.
- Vault 2026 also runs on .NET 8 for extensions — VERIFY with the Vault 2026 SDK readme before building.

This repo does not include Vault code (you would need a Vault server to test it). If your company uses
Vault, start with: *Vault SDK → `HelloWorld` sample*, then a custom Job Processor job that calls the same
`Exporter` class as `03-BatchExport`.

Next: [09 — drawings, BOM, export, Content Center, iParts](09-drawings-bom-export.md).
