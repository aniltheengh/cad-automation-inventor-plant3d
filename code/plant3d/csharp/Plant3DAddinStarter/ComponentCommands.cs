// ComponentCommands.cs - commands that walk the Plant objects of the ACTIVE drawing. Nothing is changed.
//   P3DCOMPONENTS : every Plant object in model space -> counts per class + CSV with all properties
//   P3DINSPECT    : pick one object -> .NET class, project class, line group, all properties, ports
//   P3DWALK       : pick one 3D part -> follow the connections from part to part
// The commands only talk to the user; the reading is done in ComponentReader.cs and PlantContext.cs.
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

[assembly: CommandClass(typeof(Plant3DAddinStarter.ComponentCommands))]

namespace Plant3DAddinStarter
{
    public class ComponentCommands
    {
        private const int MaxWalk = 500;        // P3DWALK stops after this many objects

        [CommandMethod("P3DCOMPONENTS")]
        public void Components() => Report.Run("P3DCOMPONENTS", () =>
        {
            PlantContext ctx = PlantContext.FromActiveDrawing();
            List<Component> components = ComponentReader.ReadAll(ctx);
            if (components.Count == 0)
                throw new InvalidOperationException("No Plant objects in the model space of this drawing.");

            // ---- summary on the command line: one line per project class
            ctx.Ed.WriteMessage($"\n{components.Count} Plant object(s) in {Path.GetFileName(ctx.Doc.Name)} (project part '{ctx.PartName}'):");
            foreach (IGrouping<string, Component> g in components.GroupBy(c => c.ClassPath).OrderBy(g => g.Key, StringComparer.Ordinal))
                ctx.Ed.WriteMessage($"\n  {g.Count(),5}  {g.Key,-44} .NET: {string.Join(", ", g.Select(c => c.NetClass).Distinct())}");

            // ---- CSV: fixed columns first, then every property name that occurs, in alphabetical order
            var names = new SortedSet<string>(components.SelectMany(c => c.Properties.Keys), StringComparer.OrdinalIgnoreCase);
            var sb = new StringBuilder();
            sb.AppendLine(Report.CsvLine(new[] { "Handle", "Layer", "NetClass", "Kind", "PlantClass", "ClassPath", "PnPID", "Ports" }.Concat(names)));
            foreach (Component c in components)
            {
                var cells = new List<string?> { c.Handle, c.Layer, c.NetClass, c.Kind, c.PlantClass, c.ClassPath, c.RowId.ToString(), c.Ports.ToString() };
                foreach (string name in names) cells.Add(c.Properties.TryGetValue(name, out string? value) ? value : "");
                sb.AppendLine(Report.CsvLine(cells));
            }
            string path = Report.OutputFile(ctx.Doc.Name, "components", "csv");
            Report.WriteText(path, sb.ToString());
            ctx.Ed.WriteMessage($"\n{names.Count} property column(s) -> {path}\n");
        });

        [CommandMethod("P3DINSPECT")]
        public void Inspect() => Report.Run("P3DINSPECT", () =>
        {
            PlantContext ctx = PlantContext.FromActiveDrawing();
            Editor ed = ctx.Ed;
            PromptEntityResult pick = ed.GetEntity("\nPick a Plant 3D or P&ID object: ");
            if (pick.Status != PromptStatus.OK) return;
            ObjectId id = pick.ObjectId;

            using (Transaction tr = ctx.Db.TransactionManager.StartTransaction())
            {
                DBObject obj = tr.GetObject(id, OpenMode.ForRead);

                // ---- the DWG side
                ed.WriteMessage($"\n.NET class      : {ComponentReader.TypeChain(obj)}");
                ed.WriteMessage($"\nObjectARX class : {id.ObjectClass.Name}");
                ed.WriteMessage($"\nHandle          : {id.Handle}");

                // ---- the data side
                if (!ctx.TryGetRowId(id, out int rowId))
                {
                    ed.WriteMessage("\nNot a Plant object: it has no row in the project database.\n");
                    tr.Commit();
                    return;
                }
                ed.WriteMessage($"\nProject class   : {ctx.ClassPath(ctx.Dlm.GetObjectClassname(id) ?? "")}");
                ed.WriteMessage($"\nRow id (PnPID)  : {rowId}");

                // ---- a relationship: part -> line group (3D model drawings)
                string line = ctx.LineGroupLabel(rowId);
                if (line.Length > 0) ed.WriteMessage($"\nLine group      : {line}");

                // ---- every property, sorted by name
                Dictionary<string, string> properties = ctx.Properties(rowId);
                ed.WriteMessage($"\n{properties.Count} properties:");
                foreach (KeyValuePair<string, string> p in properties.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                    ed.WriteMessage($"\n  {p.Key,-30} = {p.Value}");

                // ---- a 3D part also has geometry to ask for: its ports and what is connected to them
                foreach (string portLine in ComponentReader.PortLines(ctx, obj))
                    ed.WriteMessage($"\n  port {portLine}");

                tr.Commit();
            }
            ed.WriteMessage("\n");
        });

        [CommandMethod("P3DWALK")]
        public void Walk() => Report.Run("P3DWALK", () =>
        {
            PlantContext ctx = PlantContext.FromActiveDrawing();
            Editor ed = ctx.Ed;
            PromptEntityResult pick = ed.GetEntity("\nPick a pipe, fitting or equipment to start from: ");
            if (pick.Status != PromptStatus.OK) return;

            using (Transaction tr = ctx.Db.TransactionManager.StartTransaction())
            {
                DBObject start = tr.GetObject(pick.ObjectId, OpenMode.ForRead);
                if (!ComponentReader.IsPart(start))
                {
                    ed.WriteMessage("\nThat is not a Plant 3D part (pipe, fitting, joint, equipment).\n");
                    tr.Commit();
                    return;
                }

                List<ObjectId> connected = ComponentReader.Walk(pick.ObjectId, MaxWalk);
                ed.WriteMessage($"\nConnected to {ComponentReader.Label(ctx, pick.ObjectId)}:");
                for (int i = 0; i < connected.Count; i++)
                    ed.WriteMessage($"\n  {i + 1,4}. {ComponentReader.Label(ctx, connected[i])}");
                ed.WriteMessage(connected.Count >= MaxWalk ? $"\nStopped after {MaxWalk} objects.\n" : $"\n{connected.Count} connected object(s).\n");
                tr.Commit();
            }
        });
    }
}
