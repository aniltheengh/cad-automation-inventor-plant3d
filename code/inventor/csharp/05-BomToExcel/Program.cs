// 05-BomToExcel (C# EXE) - export the BOM of the active assembly to a formatted .xlsx.
//
//   BomToExcel [out.xlsx] [--structured|--parts] [--native]
//
// --structured (default): multi-level, indented item numbers (1, 1.1, 1.2 ...)
// --parts               : flat Parts Only view
// --native              : additionally call BOMView.Export(kMicrosoftExcelFormat) = Inventor's own export,
//                         which needs Excel/ACE drivers; our ClosedXML export does not.
using ClosedXML.Excel;
using CadAutomation.Shared;
using Inventor;
using IO = System.IO;

internal static class Program
{
    private sealed record BomLine(int Level, string Item, string PartNumber, string Description, string Qty,
                                  string Unit, string Material, double MassKg, string Structure, string File);

    [STAThread]
    private static int Main(string[] args)
    {
        Inventor.Application app = InventorConnector.Connect(startIfNotRunning: false);
        if (app.ActiveDocument is not AssemblyDocument asm) { Console.Error.WriteLine("Activate an assembly."); return 1; }
        bool parts = args.Contains("--parts");
        string outFile = args.FirstOrDefault(a => !a.StartsWith("--"))
                         ?? IO.Path.ChangeExtension(asm.FullFileName, parts ? ".parts-bom.xlsx" : ".bom.xlsx");

        BOM bom = asm.ComponentDefinition.BOM;
        // Views are disabled by default; enabling is a document change (Inventor marks it dirty).
        if (parts) { bom.PartsOnlyViewEnabled = true; } else { bom.StructuredViewEnabled = true; bom.StructuredViewFirstLevelOnly = false; }
        BOMView view = parts ? bom.BOMViews["Parts Only"] : bom.BOMViews["Structured"];
        // NOTE: view names are localised in non-English Inventor ("構造化" etc.). Safer: loop and test ViewType:
        foreach (BOMView v in bom.BOMViews)
            if (v.ViewType == (parts ? BOMViewTypeEnum.kPartsOnlyBOMViewType : BOMViewTypeEnum.kStructuredBOMViewType)) view = v;

        var lines = new List<BomLine>();
        Walk(view.BOMRows, 0, lines);

        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("BOM");
        string[] head = { "Level", "Item", "Part Number", "Description", "Qty", "Unit", "Material", "Mass each (kg)", "BOM Structure", "File" };
        for (int i = 0; i < head.Length; i++) ws.Cell(1, i + 1).Value = head[i];
        int r = 2;
        foreach (var l in lines)
        {
            ws.Cell(r, 1).Value = l.Level; ws.Cell(r, 2).Value = l.Item;
            ws.Cell(r, 3).Value = new string(' ', l.Level * 3) + l.PartNumber;
            ws.Cell(r, 4).Value = l.Description; ws.Cell(r, 5).Value = l.Qty; ws.Cell(r, 6).Value = l.Unit;
            ws.Cell(r, 7).Value = l.Material; ws.Cell(r, 8).Value = Math.Round(l.MassKg, 3);
            ws.Cell(r, 9).Value = l.Structure; ws.Cell(r, 10).Value = l.File;
            if (l.Level == 0 && !parts) ws.Row(r).Style.Font.Bold = true;
            r++;
        }
        var table = ws.Range(1, 1, r - 1, head.Length).CreateTable("BOM");
        table.Theme = XLTableTheme.TableStyleMedium2;
        ws.SheetView.FreezeRows(1);
        ws.Columns().AdjustToContents(1, 300, 6, 70);

        var info = wb.AddWorksheet("Info");
        info.Cell(1, 1).Value = "Assembly"; info.Cell(1, 2).Value = asm.FullFileName;
        info.Cell(2, 1).Value = "Exported"; info.Cell(2, 2).Value = DateTime.Now;
        info.Cell(3, 1).Value = "View"; info.Cell(3, 2).Value = parts ? "Parts Only" : "Structured (all levels)";
        info.Cell(4, 1).Value = "Lines"; info.Cell(4, 2).Value = lines.Count;
        wb.SaveAs(outFile);
        Console.WriteLine($"{lines.Count} BOM lines -> {outFile}");

        if (args.Contains("--native"))
        {
            string native = IO.Path.ChangeExtension(outFile, ".native.xls");
            view.Export(native, FileFormatEnum.kMicrosoftExcelFormat);
            Console.WriteLine("native export -> " + native);
        }
        return 0;
    }

    private static void Walk(BOMRowsEnumerator rows, int level, List<BomLine> acc)
    {
        foreach (BOMRow row in rows)
        {
            // A row can merge several definitions (e.g. "merge rows on part number") - [1] is representative.
            ComponentDefinition cd = row.ComponentDefinitions[1];
            Document doc = (Document)cd.Document;
            string mat = "";
            double mass = 0;
            if (doc is PartDocument pd) { mat = pd.ActiveMaterial.DisplayName; mass = pd.ComponentDefinition.MassProperties.Mass; }
            else if (doc is AssemblyDocument ad) { mass = ad.ComponentDefinition.MassProperties.Mass; }
            acc.Add(new BomLine(level, row.ItemNumber,
                IProps.Get(doc, "Part Number"), IProps.Get(doc, "Description"),
                row.TotalQuantity, row.ItemQuantity.ToString(), mat, mass,   // MassProperties.Mass is kg
                row.BOMStructure.ToString().Replace("k", "").Replace("BOMStructure", ""), doc.FullFileName));
            if (row.ChildRows != null && row.ChildRows.Count > 0) Walk(row.ChildRows, level + 1, acc);
        }
    }
}
