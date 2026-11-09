# BatchExportVB — the PDF batch exporter in VB.NET

```
BatchExportVB <folder> [outFolder]
```

Same API calls as `csharp/03-BatchExport`, written in VB.NET so you can compare syntax side by side.
VB.NET matters to an Inventor engineer because **iLogic is VB.NET**: code you write here can be pasted
into a rule with small changes (no `Sub Main` wrapper needed in iLogic, `ThisApplication` already exists).

| C# | VB.NET / iLogic |
|---|---|
| `var doc = (DrawingDocument)app.Documents.Open(p, false);` | `Dim doc As DrawingDocument = app.Documents.Open(p, False)` |
| `foreach (Sheet s in doc.Sheets)` | `For Each s As Sheet In doc.Sheets` |
| `try { } finally { doc.Close(true); }` | `Try ... Finally doc.Close(True) End Try` |
| `addin.SaveCopyAs(doc, ctx, opts, dm);` | `addin.SaveCopyAs(doc, ctx, opts, dm)` |
| `$"{a} {b}"` | `$"{a} {b}"` (VB 14+) |

`InventorConnector.vb` is the VB copy of the .NET 8 `GetActiveObject` P/Invoke helper.
