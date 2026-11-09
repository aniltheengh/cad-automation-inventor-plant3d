// AttributeCommands.cs - export / batch-edit block attributes (title blocks, tags, valve labels...).
//   NETLOAD AttributeTools.dll, then:
//   ATTEXPORT  -> writes <dwg>_attributes.csv (Handle, Block, Tag, Value, Layout)
//   ATTIMPORT  -> reads a CSV (Handle,Tag,NewValue) and updates matching attributes
//   ATTREPLACE -> prompts Block name, Tag, new value: sets it on every insert of that block
// Only uses accoremgd/acdbmgd -> also runs headless in accoreconsole.exe (see code/plant3d/scripts).
using System.Text;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(AttributeTools.AttributeCommands))]

namespace AttributeTools
{
    public class AttributeCommands
    {
        [CommandMethod("ATTEXPORT")]
        public void Export()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            string csv = Path.ChangeExtension(db.Filename, null) + "_attributes.csv";
            var sb = new StringBuilder("Handle,Block,Tag,Value,Layout\r\n");
            int n = 0;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (var (br, layout) in AllBlockRefs(db, tr))
                {
                    string block = EffectiveName(br, tr);
                    foreach (ObjectId attId in br.AttributeCollection)
                    {
                        var att = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                        sb.Append(string.Join(",", Csv(att.Handle.ToString()), Csv(block), Csv(att.Tag), Csv(att.TextString), Csv(layout))).Append("\r\n");
                        n++;
                    }
                }
                tr.Commit();
            }
            // UTF-8 WITH BOM so Excel opens Japanese / Nepali text correctly
            File.WriteAllText(csv, sb.ToString(), new UTF8Encoding(true));
            doc.Editor.WriteMessage($"\n{n} attributes -> {csv}");
        }

        [CommandMethod("ATTIMPORT")]
        public void Import()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            var pr = ed.GetString(new PromptStringOptions("\nCSV file (Handle,Tag,NewValue): ") { AllowSpaces = true });
            if (pr.Status != PromptStatus.OK || !File.Exists(pr.StringResult.Trim('"'))) return;
            var lines = File.ReadAllLines(pr.StringResult.Trim('"')).Skip(1)
                            .Select(l => SplitCsv(l)).Where(c => c.Length >= 3).ToList();
            int changed = 0;
            Database db = doc.Database;
            using (doc.LockDocument())          // needed when called from a modeless UI; harmless otherwise
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                foreach (var c in lines)
                {
                    if (!long.TryParse(c[0], System.Globalization.NumberStyles.HexNumber, null, out long h)) continue;
                    if (!db.TryGetObjectId(new Handle(h), out ObjectId id) || id.IsErased) continue;
                    if (tr.GetObject(id, OpenMode.ForRead) is not AttributeReference att) continue;
                    if (!att.Tag.Equals(c[1], StringComparison.OrdinalIgnoreCase) || att.TextString == c[2]) continue;
                    att.UpgradeOpen();
                    att.TextString = c[2];
                    changed++;
                }
                tr.Commit();
            }
            ed.WriteMessage($"\n{changed} attributes updated.");
        }

        [CommandMethod("ATTREPLACE")]
        public void Replace()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            string? block = Ask(ed, "\nBlock name: "), tag = Ask(ed, "\nAttribute tag: "), val = Ask(ed, "\nNew value: ");
            if (block == null || tag == null || val == null) return;
            int n = ReplaceAll(doc.Database, block, tag, val);
            ed.WriteMessage($"\n{n} attributes set.");
        }

        /// <summary>Reusable core (also called from scripts / other commands).</summary>
        public static int ReplaceAll(Database db, string block, string tag, string value)
        {
            int n = 0;
            using Transaction tr = db.TransactionManager.StartTransaction();
            foreach (var (br, _) in AllBlockRefs(db, tr))
            {
                if (!EffectiveName(br, tr).Equals(block, StringComparison.OrdinalIgnoreCase)) continue;
                foreach (ObjectId attId in br.AttributeCollection)
                {
                    var att = (AttributeReference)tr.GetObject(attId, OpenMode.ForRead);
                    if (!att.Tag.Equals(tag, StringComparison.OrdinalIgnoreCase)) continue;
                    att.UpgradeOpen();
                    att.TextString = value;
                    n++;
                }
            }
            tr.Commit();
            return n;
        }

        // ---------- helpers ----------
        private static IEnumerable<(BlockReference br, string layout)> AllBlockRefs(Database db, Transaction tr)
        {
            var bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
            foreach (ObjectId btrId in bt)
            {
                var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForRead);
                if (!btr.IsLayout) continue;                  // model space + paper space layouts only
                string layout = ((Layout)tr.GetObject(btr.LayoutId, OpenMode.ForRead)).LayoutName;
                foreach (ObjectId id in btr)
                    if (id.ObjectClass.IsDerivedFrom(RXObject.GetClass(typeof(BlockReference))))
                        yield return ((BlockReference)tr.GetObject(id, OpenMode.ForRead), layout);
            }
        }

        /// <summary>Dynamic blocks insert anonymous "*U12" definitions - get the real name.</summary>
        private static string EffectiveName(BlockReference br, Transaction tr)
        {
            ObjectId id = br.IsDynamicBlock ? br.DynamicBlockTableRecord : br.BlockTableRecord;
            return ((BlockTableRecord)tr.GetObject(id, OpenMode.ForRead)).Name;
        }

        private static string? Ask(Editor ed, string msg)
        {
            var r = ed.GetString(new PromptStringOptions(msg) { AllowSpaces = true });
            return r.Status == PromptStatus.OK && r.StringResult.Length > 0 ? r.StringResult : null;
        }

        private static string Csv(string s) => "\"" + s.Replace("\"", "\"\"") + "\"";

        private static string[] SplitCsv(string line)
        {
            var res = new List<string>(); var cur = new StringBuilder(); bool q = false;
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (q) { if (ch == '"' && i + 1 < line.Length && line[i + 1] == '"') { cur.Append('"'); i++; } else if (ch == '"') q = false; else cur.Append(ch); }
                else if (ch == '"') q = true;
                else if (ch == ',') { res.Add(cur.ToString()); cur.Clear(); }
                else cur.Append(ch);
            }
            res.Add(cur.ToString());
            return res.ToArray();
        }
    }
}
