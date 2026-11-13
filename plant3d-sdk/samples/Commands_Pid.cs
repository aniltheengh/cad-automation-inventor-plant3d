using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.PnIDObjects;
using Autodesk.ProcessPower.Styles;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Plant3dSdkSamples
{
    /// <summary>
    /// ADN Plant3DTraining LAB 2 — read a P&amp;ID LineSegment, its LineStyle, and its line group.
    /// This sample does not create symbols. For creation, see Samples\PnID in the Plant SDK
    /// and the Autodesk Support article on inserting P&amp;ID symbols (no Tool Palette API).
    /// </summary>
    public class Commands_Pid
    {
        [CommandMethod("P3DLINEGROUP")]
        public static void WalkLineGroup()
        {
            Editor ed = PlantHelpers.Ed;
            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;

            PromptEntityOptions opts = new PromptEntityOptions("\nSelect a P&ID line segment (SLINE): ");
            PromptEntityResult pick = ed.GetEntity(opts);
            if (pick.Status != PromptStatus.OK)
                return;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                try
                {
                    LineSegment sline = (LineSegment)tr.GetObject(pick.ObjectId, OpenMode.ForRead);
                    LineStyle lineStyle = (LineStyle)tr.GetObject(sline.StyleID, OpenMode.ForRead, true);
                    ed.WriteMessage("\nLineStyle name = " + lineStyle.Name);
                    ed.WriteMessage("\n  FlagType = " + lineStyle.FlagType);
                    ed.WriteMessage("\n  GapPriority = " + lineStyle.GapPriority);
                    ed.WriteMessage("\n  ShowFlowDirection = " + lineStyle.ShowFlowDirection);

                    LineGroupManager groupMgr = new LineGroupManager();
                    int groupId = groupMgr.GroupId(pick.ObjectId);
                    GroupType type = groupMgr.Type(groupId);
                    ed.WriteMessage("\nLineGroup id = " + groupId + " (" + type + ")");
                    ed.WriteMessage("\n  Segment count = " + groupMgr.Count(groupId));

                    foreach (ObjectId lsId in groupMgr.LineDbIds(groupId))
                    {
                        LineSegment ls = (LineSegment)tr.GetObject(lsId, OpenMode.ForRead);
                        ed.WriteMessage("\n  LineSegment class = " + ls.ClassName + "  vertex0 = " + ls.Vertices[0]);
                    }

                    // Optional: assets sitting on the picked segment (LAB 2 LineObjects).
                    LineObjectCollection objects = sline.GetLineObjectCollection(null);
                    foreach (LineObject lo in objects)
                    {
                        if (lo is InlineEntity inlineEntity)
                        {
                            Entity ent = (Entity)tr.GetObject(inlineEntity.ObjectId, OpenMode.ForRead);
                            if (ent is Asset asset)
                            {
                                ed.WriteMessage("\n  Inline asset class = " + asset.ClassName);
                            }
                        }
                    }

                    tr.Commit();
                }
                catch (System.Exception ex)
                {
                    ed.WriteMessage("\nP3DLINEGROUP error: " + ex.Message);
                    ed.WriteMessage("\nThe pick must be a P&ID LineSegment in a P&ID drawing.");
                }
            }
        }
    }
}
