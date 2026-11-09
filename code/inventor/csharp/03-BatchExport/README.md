# 03-BatchExport — folder to PDF / DXF / STEP / flat-pattern DXF (C#)

```
BatchExport <folder> [--out dir] [--pdf] [--dxf] [--step] [--flat] [--recurse] [--ini dxf.ini] [--black]
BatchExport "D:\Project A" --pdf --flat --recurse --out D:\Export
```

- Drawings (`.idw`, `.dwg`) → PDF (all sheets) and/or DXF (via an export `.ini` from *Save Copy As → DXF → Options → Save Configuration*).
- Parts/assemblies → STEP AP214.
- Sheet-metal parts → flat pattern DXF via `DataIO.WriteDataToFile("FLAT PATTERN DXF?AcadVersion=2018&OuterProfileLayer=OUTER...")`.
- Writes `export_log.csv` (file, action, output, status, message). One bad file never stops the run.

## Files
| File | Content |
|---|---|
| `Exporter.cs` | reusable `Exporter` class: translator add-in GUIDs, option maps, flat pattern. Linked into `07-MyFirstAddin` too. |
| `Program.cs` | argument parsing, folder walk, open **invisible** (`Documents.Open(path, false)`), always `Close(true)` in `finally`. |

## Speed tips (measured patterns, not promises)
- `app.SilentOperation = true` suppresses dialogs (missing references!).
- Open drawings invisibly; Inventor still loads referenced models — a large assembly drawing is slow either way.
- Export from an assembly drawing with `Defer Updates` on? The PDF shows the *saved* state; update and save first if needed.

See [docs/09](../../../../docs/09-drawings-bom-export.md) for every translator option.
