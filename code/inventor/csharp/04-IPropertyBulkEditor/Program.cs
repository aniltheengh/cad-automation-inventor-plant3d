// 04-IPropertyBulkEditor (C# EXE) - round-trip iProperties through Excel.
//
//   IPropertyBulkEditor export <folder> <out.xlsx> [--recurse]
//   IPropertyBulkEditor import <in.xlsx> [--dry-run]
//
// Workflow: export -> edit the yellow cells in Excel -> import. Column A = full file path (key).
// Any column header that is not a standard iProperty name becomes a CUSTOM iProperty.
// Files that are checked-in to Vault are read-only: check them out first (or use Vault's own tools).
using ClosedXML.Excel;
using CadAutomation.Shared;
using Inventor;
using IO = System.IO;

internal static class Program
{
    private static readonly string[] DefaultCols =
        { "Part Number", "Description", "Revision Number", "Stock Number", "Vendor", "Title", "Designer", "Project", "Material" };

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length < 2) { Usage(); return 2; }
        return args[0].ToLowerInvariant() switch
        {
            "export" when args.Length >= 3 => Export(args[1], args[2], args.Contains("--recurse")),
            "import" => Import(args[1], args.Contains("--dry-run")),
            _ => Usage()
        };
    }

    private static int Usage()
    {
        Console.WriteLine("IPropertyBulkEditor export <folder> <out.xlsx> [--recurse]\nIPropertyBulkEditor import <in.xlsx> [--dry-run]");
        return 2;
    }

    private static IEnumerable<string> InventorFiles(string folder, bool recurse) =>
        IO.Directory.EnumerateFiles(folder, "*.*", recurse ? IO.SearchOption.AllDirectories : IO.SearchOption.TopDirectoryOnly)
          .Where(f => new[] { ".ipt", ".iam", ".idw", ".ipn" }.Contains(IO.Path.GetExtension(f).ToLowerInvariant()))
          .Where(f => !f.Contains(@"\OldVersions\", StringComparison.OrdinalIgnoreCase));

    private static int Export(string folder, string xlsx, bool recurse)
    {
        Inventor.Application app = InventorConnector.Connect(true, visible: false);
        app.SilentOperation = true;
        var rows = new List<(string file, Dictionary<string, string> vals)>();
        var customCols = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (string f in InventorFiles(folder, recurse))
            {
                Document doc = app.Documents.Open(f, false);
                try
                {
                    var vals = DefaultCols.ToDictionary(c => c, c => IProps.Get(doc, c));
                    foreach (string cn in IProps.CustomNames(doc)) { customCols.Add(cn); vals[cn] = IProps.Get(doc, cn); }
                    rows.Add((f, vals));
                    Console.WriteLine("read " + IO.Path.GetFileName(f));
                }
                finally { doc.Close(true); }
            }
        }
        finally { app.SilentOperation = false; }

        using var wb = new XLWorkbook();
        IXLWorksheet ws = wb.AddWorksheet("iProperties");
        var cols = new List<string> { "File" }; cols.AddRange(DefaultCols); cols.AddRange(customCols);
        for (int c = 0; c < cols.Count; c++) ws.Cell(1, c + 1).Value = cols[c];
        for (int r = 0; r < rows.Count; r++)
        {
            ws.Cell(r + 2, 1).Value = rows[r].file;
            for (int c = 1; c < cols.Count; c++)
                ws.Cell(r + 2, c + 1).Value = rows[r].vals.TryGetValue(cols[c], out var v) ? v : "";
        }
        ws.Row(1).Style.Font.Bold = true;
        ws.Range(2, 2, Math.Max(2, rows.Count + 1), cols.Count).Style.Fill.BackgroundColor = XLColor.LightYellow;
        ws.Column(cols.IndexOf("Material") + 1).Style.Fill.BackgroundColor = XLColor.LightGray; // read-only
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents(1, 200, 8, 60);
        wb.SaveAs(xlsx);
        Console.WriteLine($"{rows.Count} files -> {xlsx}");
        return 0;
    }

    private static int Import(string xlsx, bool dryRun)
    {
        using var wb = new XLWorkbook(xlsx);
        IXLWorksheet ws = wb.Worksheet(1);
        var header = ws.Row(1).CellsUsed().Select(c => c.GetString()).ToList();
        Inventor.Application app = InventorConnector.Connect(true, visible: false);
        app.SilentOperation = true;
        int changedFiles = 0;
        try
        {
            foreach (IXLRow row in ws.RowsUsed().Skip(1))
            {
                string file = row.Cell(1).GetString();
                if (!IO.File.Exists(file)) { Console.WriteLine("missing: " + file); continue; }
                if (new IO.FileInfo(file).IsReadOnly) { Console.WriteLine("read-only (Vault?): " + file); continue; }
                Document doc = app.Documents.Open(file, false);
                try
                {
                    var changes = new List<string>();
                    for (int c = 1; c < header.Count; c++)
                    {
                        string col = header[c], val = row.Cell(c + 1).GetString();
                        string old = IProps.Get(doc, col);
                        if (old == val) continue;
                        if (string.IsNullOrEmpty(val) && !IProps.Standard.ContainsKey(col)) continue; // don't create empty customs
                        if (!dryRun && IProps.Set(doc, col, val)) changes.Add($"{col}: '{old}' -> '{val}'");
                        else if (dryRun) changes.Add($"{col}: '{old}' -> '{val}' (dry-run)");
                    }
                    if (changes.Count > 0)
                    {
                        Console.WriteLine(IO.Path.GetFileName(file)); changes.ForEach(x => Console.WriteLine("   " + x));
                        if (!dryRun) { doc.Save(); changedFiles++; }
                    }
                }
                finally { doc.Close(true); }
            }
        }
        finally { app.SilentOperation = false; }
        Console.WriteLine($"{changedFiles} files saved{(dryRun ? " (dry run: nothing saved)" : "")}");
        return 0;
    }
}
