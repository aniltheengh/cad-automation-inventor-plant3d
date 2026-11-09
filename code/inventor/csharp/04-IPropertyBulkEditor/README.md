# 04-IPropertyBulkEditor — iProperties ⇄ Excel (C#, ClosedXML)

```
IPropertyBulkEditor export <folder> <out.xlsx> [--recurse]
IPropertyBulkEditor import <in.xlsx> [--dry-run]
```

1. **export** writes one row per file: path + Part Number, Description, Revision, Stock Number, Vendor, Title, Designer, Project, Material, plus any extra column you add.
2. Edit in Excel (do not change column A).
3. **import** opens each file, writes changed values only, saves. `--dry-run` prints what would change.

- Unknown column headers become **custom** iProperties (`Inventor User Defined Properties`).
- `IProps.cs` (in `../Shared`) maps friendly names → property set + name, e.g. `Part Number` → *Design Tracking Properties*.
- ClosedXML writes .xlsx **without Excel installed** (pure .NET). NuGet: `ClosedXML 0.105.0`.
- Vault: files checked in are read-only → check out first or use Vault's *Edit Properties* / Data Standard.

Typical engineer uses: fix "Description" on 500 purchased parts before a BOM release; set `Project = P-2411` on a whole folder; copy ERP article numbers into `Stock Number`.
