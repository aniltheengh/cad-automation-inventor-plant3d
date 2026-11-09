// PipingCommands.cs - Plant 3D piping data automation.
//   P3DLINELIST  : line list (one row per P3dLineGroup = line number) + part counts -> CSV
//   P3DTABLES    : list all tables in the piping database (find the class/table names you need)
//   P3DDUMP      : dump any table (e.g. Pipe, Elbow, Valve, PipeRunComponent) -> CSV
//   P3DMTO       : simple MTO: group PipeRunComponent rows by PartFamilyLongDesc + Size -> CSV
//   P3DSETPROPS  : bulk-write properties from CSV (PnPID,Property,Value) via DataLinksManager
// All work on the PROJECT database (every drawing), not only the open DWG.
using System.Collections.Specialized;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Plant3DDataTools.PipingCommands))]

namespace Plant3DDataTools
{
    public class PipingCommands
    {
        private static Editor Ed => AcApp.DocumentManager.MdiActiveDocument.Editor;

        [CommandMethod("P3DTABLES")]
        public void ListTables()
        {
            PnPDatabase db = PlantHelpers.Dlm("Piping").GetPnPDatabase();
            foreach (PnPTable t in db.Tables)
                Ed.WriteMessage($"\n{t.Name,-40} {t.Select().Length,6} rows");
        }

        [CommandMethod("P3DDUMP")]
        public void DumpTable()
        {
            var r = Ed.GetString("\nTable name <PipeRunComponent>: ");
            string table = r.Status == PromptStatus.OK && r.StringResult.Length > 0 ? r.StringResult : "PipeRunComponent";
            PnPDatabase db = PlantHelpers.Dlm("Piping").GetPnPDatabase();
            string csv = Path.Combine(PlantHelpers.ProjectFolder(), $"{table}_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            int n = PlantHelpers.TableToCsv(db, table, csv);
            Ed.WriteMessage($"\n{n} rows -> {csv}");
        }

        [CommandMethod("P3DLINELIST")]
        public void LineList()
        {
            DataLinksManager dlm = PlantHelpers.Dlm("Piping");
            PnPDatabase db = dlm.GetPnPDatabase();
            PnPTable lines = db.Tables["P3dLineGroup"];
            var cols = lines.Columns.Select(c => c.Name)
                            .Where(c => c is not ("PnPID" or "PnPGuid" or "PnPTimestamp" or "PnPStatus" or "PnPRevision"))
                            .ToList();
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", new[] { "PnPID" }.Concat(cols).Concat(new[] { "Parts", "Pipes", "Fittings", "Valves", "Other" }).Select(PlantHelpers.Csv)));
            int count = 0;
            foreach (PnPRow lg in lines.Select())
            {
                // parts of this line: relationship P3dLineGroupPartRelationship (roles LineGroup / Part)
                var parts = dlm.GetRelatedRowIds("P3dLineGroupPartRelationship", "LineGroup", lg.RowId, "Part");
                int pipes = 0, fittings = 0, valves = 0, other = 0;
                foreach (int partId in parts)
                {
                    string cls = db.GetRowTableName(partId);       // "Pipe", "Elbow", "Valve", "Flange", "Olet", ...
                    if (cls == "Pipe") pipes++;
                    else if (cls.Contains("Valve")) valves++;
                    else if (cls is "Elbow" or "Tee" or "Reducer" or "Flange" or "Coupling" or "Cap" or "Olet" or "Cross") fittings++;
                    else other++;
                }
                var vals = new List<string> { lg.RowId.ToString() };
                vals.AddRange(cols.Select(c => Convert.ToString(lg[c]) ?? ""));
                vals.AddRange(new[] { parts.Count, pipes, fittings, valves, other }.Select(i => i.ToString()));
                sb.AppendLine(string.Join(",", vals.Select(PlantHelpers.Csv)));
                count++;
            }
            string csv = Path.Combine(PlantHelpers.ProjectFolder(), $"LineList_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            File.WriteAllText(csv, sb.ToString(), new UTF8Encoding(true));
            Ed.WriteMessage($"\n{count} lines -> {csv}");
        }

        [CommandMethod("P3DMTO")]
        public void Mto()
        {
            PnPDatabase db = PlantHelpers.Dlm("Piping").GetPnPDatabase();
            PnPTable t = db.Tables["PipeRunComponent"];
            var colNames = t.Columns.Select(c => c.Name).ToHashSet();
            string descCol = colNames.Contains("PartFamilyLongDesc") ? "PartFamilyLongDesc" : "ShortDescription";
            string sizeCol = colNames.Contains("PartSizeLongDesc") ? "PartSizeLongDesc" : "Size";
            string lineCol = colNames.Contains("LineNumberTag") ? "LineNumberTag" : "";
            var groups = t.Select()
                .GroupBy(r => (Desc: Convert.ToString(r[descCol]) ?? "", Size: colNames.Contains(sizeCol) ? Convert.ToString(r[sizeCol]) ?? "" : "",
                               Line: lineCol == "" ? "" : Convert.ToString(r[lineCol]) ?? ""))
                .OrderBy(g => g.Key.Line).ThenBy(g => g.Key.Desc);
            var sb = new StringBuilder("Line,Description,Size,Qty\r\n");
            foreach (var g in groups)
                sb.Append(string.Join(",", new[] { g.Key.Line, g.Key.Desc, g.Key.Size, g.Count().ToString() }.Select(PlantHelpers.Csv))).Append("\r\n");
            // NOTE: pipe is counted per piece here; for pipe LENGTH use the Pipe table length column or
            // Plant's own Report Creator (pipe cut lengths), which is the supported way for an official MTO.
            string csv = Path.Combine(PlantHelpers.ProjectFolder(), $"MTO_{DateTime.Now:yyyyMMdd_HHmm}.csv");
            File.WriteAllText(csv, sb.ToString(), new UTF8Encoding(true));
            Ed.WriteMessage($"\nMTO ({descCol}/{sizeCol}) -> {csv}");
        }

        [CommandMethod("P3DSETPROPS")]
        public void SetProps()
        {
            var r = Ed.GetString(new PromptStringOptions("\nCSV (PnPID,Property,Value): ") { AllowSpaces = true });
            if (r.Status != PromptStatus.OK) return;
            string path = r.StringResult.Trim('"');
            DataLinksManager dlm = PlantHelpers.Dlm("Piping");
            var byRow = File.ReadAllLines(path).Skip(1)
                .Select(l => l.Split(','))
                .Where(c => c.Length >= 3 && int.TryParse(c[0], out _))
                .GroupBy(c => int.Parse(c[0]));
            int rows = 0;
            foreach (var g in byRow)
            {
                // SetProperties takes parallel name/value collections (pattern from ADN Plant3DTraining LAB_03)
                var names = new StringCollection(); var values = new StringCollection();
                foreach (var c in g) { names.Add(c[1].Trim()); values.Add(c[2].Trim()); }
                try { dlm.SetProperties(g.Key, names, values); rows++; }
                catch (System.Exception ex) { Ed.WriteMessage($"\nrow {g.Key}: {ex.Message}"); }
            }
            Ed.WriteMessage($"\n{rows} objects updated. Drawings update their annotation/tag display on next regen/open.");
        }
    }
}
