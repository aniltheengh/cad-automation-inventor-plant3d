// ComponentReader.cs - the logic behind the commands in ComponentCommands.cs:
//   walk the Plant objects of the active drawing, read class + properties, ports and connections.
// Read-only: nothing here changes the drawing or the project. Explained in docs/11c-plant3d-code-examples.md.
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.ProcessPower.PnP3dObjects;

namespace Plant3DAddinStarter
{
    /// <summary>What the samples read from one Plant object.</summary>
    internal sealed class Component
    {
        public string Handle = "";          // DWG handle: stable id of the object inside its drawing
        public string Layer = "";
        public string NetClass = "";        // .NET class of the DWG object: Pipe, PipeInlineAsset, Connector...
        public string Kind = "";            // the same in words
        public string PlantClass = "";      // project class = table in the project database: Pipe, Elbow, Valve...
        public string ClassPath = "";       // the project class with its parent classes
        public int RowId;                   // PnPID: key of the row in the project database
        public int Ports;
        public Dictionary<string, string> Properties = new Dictionary<string, string>();
    }

    internal static class ComponentReader
    {
        /// <summary>The core loop: model space -> Plant objects -> row id -> class and properties.</summary>
        public static List<Component> ReadAll(PlantContext ctx)
        {
            var result = new List<Component>();

            // Every read of a DWG goes through a transaction. The using block closes all opened objects.
            using (Transaction tr = ctx.Db.TransactionManager.StartTransaction())
            {
                // Model space is a block table record; looping over it gives the ObjectId of each entity.
                var modelSpace = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(ctx.Db), OpenMode.ForRead);

                foreach (ObjectId id in modelSpace)
                {
                    // 1) Plant object or plain AutoCAD geometry? Only Plant objects have a row in the project database.
                    if (!ctx.TryGetRowId(id, out int rowId)) continue;

                    // 2) the DWG side: open the object to see its .NET class, layer and ports
                    DBObject obj = tr.GetObject(id, OpenMode.ForRead);

                    // 3) the data side: project class and all properties come from the project database
                    string plantClass = ctx.Dlm.GetObjectClassname(id) ?? "";

                    result.Add(new Component
                    {
                        Handle = id.Handle.ToString(),
                        Layer = (obj as Entity)?.Layer ?? "",
                        NetClass = obj.GetType().Name,
                        Kind = Kind(obj),
                        PlantClass = plantClass,
                        ClassPath = ctx.ClassPath(plantClass),
                        RowId = rowId,
                        Ports = obj is Part part ? PortCount(part) : 0,
                        Properties = ctx.Properties(rowId),
                    });
                }
                tr.Commit();        // nothing was changed; Commit is simply the cheapest way to end a transaction
            }
            return result;
        }

        /// <summary>
        /// A handful of .NET classes covers the whole 3D model. WHAT a part is (elbow, gate valve, reducer...)
        /// is not a .NET class: it is the project class and the properties in the project database.
        /// </summary>
        public static string Kind(DBObject obj) => obj switch
        {
            Pipe => "pipe",
            PipeInlineAsset => "inline part (fitting, valve, instrument...)",
            Connector => "joint between two ports",
            Equipment => "equipment",
            Part => "other 3D part",
            _ => "P&ID or other Plant object",
        };

        public static int PortCount(Part part)
        {
            try { return part.GetPorts(PortType.All).Count; }
            catch (System.Exception) { return 0; }
        }

        /// <summary>The real inheritance chain of an object, e.g. "Pipe : Segment : Part : Entity : DBObject".</summary>
        public static string TypeChain(DBObject obj)
        {
            var chain = new List<string>();
            for (Type? t = obj.GetType(); t != null; t = t.BaseType)
            {
                chain.Add(t.Name);
                if (t == typeof(DBObject)) break;
            }
            return string.Join(" : ", chain);
        }

        /// <summary>True for every 3D model object: pipes, fittings, valves, joints, equipment.</summary>
        public static bool IsPart(DBObject obj) => obj is Part;

        /// <summary>One line per port of a 3D part: name, position, and what sits on the other side.</summary>
        public static List<string> PortLines(PlantContext ctx, DBObject obj)
        {
            var lines = new List<string>();
            if (obj is not Part part) return lines;         // P&ID objects have no ports

            var connections = new ConnectionManager();
            foreach (Port port in part.GetPorts(PortType.All))
            {
                // A Pair = one object + one of its ports = one side of a connection.
                var here = new Pair { ObjectId = part.ObjectId, Port = port };
                string state = "open end";
                if (connections.IsConnected(here))
                {
                    // The other side of the joint. "is Pair" also filters an empty answer.
                    state = connections.GetConnectedPairAt(here) is Pair there && !there.ObjectId.IsNull
                        ? "connected to " + Label(ctx, there.ObjectId)
                        : "connected";
                }
                lines.Add($"{port.Name,-6} ({port.Position.X:0.##}, {port.Position.Y:0.##}, {port.Position.Z:0.##})  {state}");
            }
            return lines;
        }

        /// <summary>Everything that is connected to the start object, joint by joint (at most <paramref name="max"/>).</summary>
        public static List<ObjectId> Walk(ObjectId start, int max)
        {
            var ids = new List<ObjectId>();
            ConnectionIterator walker = ConnectionIterator.NewIterator(start);
            for (; !walker.Done() && ids.Count < max; walker.Next())
                ids.Add(walker.ObjectId);
            return ids;
        }

        /// <summary>Short text for one object: project class, handle and tag (when it has one).</summary>
        public static string Label(PlantContext ctx, ObjectId id)
        {
            if (!ctx.TryGetRowId(id, out int rowId)) return $"non-Plant object [{id.Handle}]";
            string text = $"{ctx.Dlm.GetObjectClassname(id)} [{id.Handle}]";
            return ctx.Properties(rowId).TryGetValue("Tag", out string? tag) && tag.Length > 0 ? $"{text} Tag '{tag}'" : text;
        }
    }
}
