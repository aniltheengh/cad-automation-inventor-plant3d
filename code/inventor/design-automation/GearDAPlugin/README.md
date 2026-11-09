# GearDAPlugin — the gear generator on Autodesk's cloud (Design Automation for Inventor)

Runs `GearMath` + Inventor modelling **on APS servers** — no Inventor licence on the caller's side. A web
form / Excel / ERP sends `{ "module": 2, "teeth": 40, "faceWidth": 20 }`, gets back `gear.ipt`.

## Pieces
| File | Role |
|---|---|
| `GearDAPlugin.csproj` | .NET 8 class library (the 2026 DA engine is .NET 8 only) |
| `PluginServer.cs` | `ApplicationAddInServer`; gets `InventorServer` from `addInSiteObject.InventorServer`, exposes `Automation` |
| `GearAutomation.cs` | `Run(Document)` / `RunWithArguments(Document, NameValueMap)` — reads `params.json`, builds a **gear blank** (tip-diameter disc + bore, Part Number `GEAR-M2-Z40`), saves `gear.ipt`. Deliberately minimal: paste the tooth-space + pattern code from `02-SpurGearGenerator/Program.cs` for full teeth. |
| `GearDAPlugin.Inventor.addin` | manifest with `Type="Plugin"` |
| `bundle/PackageContents.xml` | AppBundle description (`ModuleName` → the .addin) |
| `bundle/activity.json` | Activity: engine `Autodesk.Inventor+2026`, command line, parameters |
| `bundle/workitem.json` | WorkItem: input JSON + signed output URL |

## Steps (summary — full walkthrough in [docs/08](../../../../docs/08-apprentice-design-automation-vault.md))
1. Build → zip `GearDAPlugin.bundle/` (PackageContents.xml + Contents/*.dll + .addin).
2. Test **locally** first, same command line as the Activity: `"C:\Program Files\Autodesk\Inventor 2026\Bin\InventorCoreConsole.exe" /al "C:\...\GearDAPlugin.bundle" "C:\...\params.json"` (InventorCoreConsole ships with Inventor; VERIFY it is in your `Bin` folder and how a document-less run calls `Run`/`RunWithArguments`).
3. Create AppBundle + alias, upload zip; create Activity + alias; post WorkItem; poll status; download report + output.

VERIFY before production: engine id (call `GET /da/us-east/v3/engines`), the exact key under which extra
arguments arrive in `NameValueMap` (this sample tries `_1` then falls back to `params.json` in the working folder).
