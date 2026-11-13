using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.PnP3dObjects;
using Autodesk.ProcessPower.ProjectManager;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Plant3dSdkSamples
{
    /// <summary>
    /// ADN Plant3DTraining LAB 4 (PipeWalk): ports, ConnectionManager, ConnectionIterator.
    /// Requires a 3D piping drawing, not a P&amp;ID.
    /// </summary>
    public class Commands_Piping
    {
        [CommandMethod("P3DPIPEWALK")]
        public static void WalkPipe()
        {
            Editor ed = PlantHelpers.Ed;
            PlantProject? plant = PlantHelpers.RequireProject(ed);
            if (plant == null)
                return;

            Project? piping = PlantHelpers.GetProjectPart(plant, ed, "Piping");
            if (piping == null)
                return;

            DataLinksManager dlm = piping.DataLinksManager;
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;

            PromptEntityOptions opts = new PromptEntityOptions("\nSelect a 3D Plant part (pipe, fitting, valve): ");
            PromptEntityResult pick = ed.GetEntity(opts);
            if (pick.Status != PromptStatus.OK)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    Part part = (Part)tr.GetObject(pick.ObjectId, OpenMode.ForRead);
                    PortCollection ports = part.GetPorts(PortType.All);
                    ed.WriteMessage("\nPort count = " + ports.Count);

                    ConnectionManager conMgr = new ConnectionManager();
                    bool anyConnected = false;

                    foreach (Port port in ports)
                    {
                        ed.WriteMessage("\nPort name = " + port.Name);
                        ed.WriteMessage("\n  Position X = " + port.Position.X);

                        Pair pair = new Pair();
                        pair.ObjectId = pick.ObjectId;
                        pair.Port = port;

                        if (conMgr.IsConnected(pair))
                        {
                            ed.WriteMessage("\n  Pair is connected");
                            anyConnected = true;
                        }
                        else
                        {
                            ed.WriteMessage("\n  Pair is NOT connected");
                        }
                    }

                    if (anyConnected)
                    {
                        // You can also call ConnectionIterator.NewIterator(part.ObjectId, port)
                        // after you have confirmed that port is connected (LAB 4).
                        ConnectionIterator iter = ConnectionIterator.NewIterator(part.ObjectId);
                        for (; !iter.Done(); iter.Next())
                        {
                            int rowId = dlm.FindAcPpRowId(iter.ObjectId);
                            ed.WriteMessage("\nConnected object row id = " + rowId + "  ObjectId = " + iter.ObjectId);
                        }
                    }

                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("\nP3DPIPEWALK error: " + ex.Message);
                    ed.WriteMessage("\nThe pick must be a Plant 3D Part in a piping drawing.");
                }
            }
        }
    }
}
