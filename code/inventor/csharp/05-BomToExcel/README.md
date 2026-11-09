# 05-BomToExcel — assembly BOM to a formatted .xlsx (C#)

```
BomToExcel [out.xlsx] [--structured|--parts] [--native]
```

- Reads the **active assembly** BOM: `AssemblyComponentDefinition.BOM` → enable the view → walk `BOMRowsEnumerator` recursively (`row.ChildRows`).
- Columns: Level, Item, Part Number, Description, Qty, Unit, Material, Mass (kg), BOM Structure, File.
- `--parts` = Parts Only view (flat, quantities rolled up). Default = Structured, all levels.
- `--native` also runs Inventor's own `BOMView.Export(path, kMicrosoftExcelFormat)` for comparison.
- Mass comes from `MassProperties.Mass` (kg) — Inventor recalculates if not up to date (slow on big assemblies).

## Gotchas
| Symptom | Reason |
|---|---|
| `E_FAIL` on `BOMViews["Parts Only"]` | view not enabled: set `bom.PartsOnlyViewEnabled = true` first |
| Phantom/Reference rows missing | that is BOM Structure working as designed |
| Quantity "12.5 m" | `ItemQuantity` vs `TotalQuantity` — cut-length / Frame Generator parts use base quantity |
| View names in Japanese UI | use `BOMView.ViewType` (enum), not the localized name — this project does |
