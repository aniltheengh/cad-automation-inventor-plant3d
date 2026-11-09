// PidCommands.cs - P&ID data automation (works in a P&ID drawing of a Plant project).
//   PIDTAGS   : every Plant object in the current P&ID DWG -> CSV (class, tag, all properties)
//   PIDPICK   : pick one object, print all its properties (use it to learn property names)
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.DataObjects;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Plant3DDataTools.PidCommands))]

namespace Plant3DDataTools
{
    public class PidCommands
    {
        [CommandMethod("PIDPICK")]
        public void Pick()
        {
            Editor ed = AcApp.DocumentManager.MdiActiveDocument.Editor;
            var res = ed.GetEntity("\nPick a P&ID (or 3D) object: ");
            if (res.Status != PromptStatus.OK) return;
            foreach (string part in new[] { "PnId", "Piping" })
            {
                DataLinksManager dlm = PlantHelpers.Dlm(part);
                int rowId = dlm.FindAcPpRowId(res.ObjectId);
                if (rowId <= 0) continue;
                ed.WriteMessage($"\n[{part}] PnPID={rowId} class={dlm.GetPnPDatabase().GetRowTableName(rowId)}");
                foreach (var kv in dlm.GetAllProperties(rowId, true))
                    ed.WriteMessage($"\n   {kv.Key,-30} = {kv.Value}");
                return;
            }
            ed.WriteMessage("\nNot a Plant object (or not in the current project).");
        }

        [CommandMethod("PIDTAGS")]
        public void Tags()
        {
            var doc = AcApp.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            DataLinksManager dlm = PlantHelpers.Dlm("PnId");
            PnPDatabase pdb = dlm.GetPnPDatabase();
            var rows = new List<(int id, string cls, Dictionary<string, string> props)>();
            var allKeys = new SortedSet<string>();
            using (Transaction tr = doc.Database.TransactionManager.StartTransaction())
            {
                var ms = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(doc.Database), OpenMode.ForRead);
                var seen = new HashSet<int>();
                foreach (ObjectId id in ms)
                {
                    int rowId;
                    try { rowId = dlm.FindAcPpRowId(id); } catch { continue; }
                    if (rowId <= 0 || !seen.Add(rowId)) continue;     // annotation + symbol share one row
                    var props = dlm.GetAllProperties(rowId, true).GroupBy(k => k.Key).ToDictionary(g => g.Key, g => g.First().Value);
                    foreach (var k in props.Keys) allKeys.Add(k);
                    rows.Add((rowId, pdb.GetRowTableName(rowId), props));
                }
                tr.Commit();
            }
            // put the useful columns first
            var first = new[] { "Tag", "Description", "Manufacturer", "Model", "Size", "Spec" }.Where(allKeys.Contains).ToList();
            var cols = first.Concat(allKeys.Except(first)).ToList();
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", new[] { "PnPID", "Class" }.Concat(cols).Select(PlantHelpers.Csv)));
            foreach (var r in rows.OrderBy(r => r.cls))
                sb.AppendLine(string.Join(",", new[] { r.id.ToString(), r.cls }
                    .Concat(cols.Select(c => r.props.TryGetValue(c, out var v) ? v : "")).Select(PlantHelpers.Csv)));
            string csv = Path.ChangeExtension(doc.Database.Filename, null) + "_tags.csv";
            File.WriteAllText(csv, sb.ToString(), new UTF8Encoding(true));
            ed.WriteMessage($"\n{rows.Count} P&ID objects -> {csv}");
        }
    }
}
