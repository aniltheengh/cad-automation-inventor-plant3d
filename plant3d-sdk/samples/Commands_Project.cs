using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.P3dProjectParts;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;

namespace Plant3dSdkSamples
{
    /// <summary>
    /// ADN Plant3DTraining LAB 1 — project structure and a few project-part settings.
    /// There are no APIs to drive the Project Manager UI (LAB 1 note).
    /// </summary>
    public class Commands_Project
    {
        [CommandMethod("P3DPROJECT")]
        public static void ListProject()
        {
            Editor ed = PlantHelpers.Ed;
            PlantProject? plant = PlantHelpers.RequireProject(ed);
            if (plant == null)
                return;

            try
            {
                ed.WriteMessage("\nProject folder (ProjectFolderPath) = " + plant.ProjectFolderPath);

                foreach (Project proj in plant.ProjectParts)
                {
                    ed.WriteMessage("\n---");
                    ed.WriteMessage("\nProject Name = " + proj.ProjectName);
                    ed.WriteMessage("\nProject Part Name = " + plant.ProjectPartName(proj));
                    ed.WriteMessage("\nProject Directory = " + proj.ProjectDirectory);

                    foreach (PnPProjectDrawing dwg in proj.GetPnPDrawingFiles())
                    {
                        ed.WriteMessage("\n  Drawing = " + dwg.AbsoluteFileName);
                    }
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nP3DPROJECT error: " + ex.Message);
            }
        }

        [CommandMethod("P3DSETTINGS")]
        public static void ShowSettings()
        {
            Editor ed = PlantHelpers.Ed;
            PlantProject? plant = PlantHelpers.RequireProject(ed);
            if (plant == null)
                return;

            try
            {
                // LAB 1 uses the key "PnID" for this typed class.
                // If the compiler cannot find PnIdProject, add the using it suggests
                // (the class lives in PnIdProjectPartsMgd.dll).
                PnIdProject pnid = (PnIdProject)plant.ProjectParts["PnID"];
                ed.WriteMessage("\nP&ID GapWidth = " + pnid.GapWidth);
                ed.WriteMessage("\nP&ID GapLoop = " + pnid.GapLoop());

                PipingProject piping = (PipingProject)plant.ProjectParts["Piping"];
                ed.WriteMessage("\nPiping MinimumPipeLength = " + piping.MinimumPipeLength);
                ed.WriteMessage("\nPiping WeldGapSize = " + piping.WeldGapSize);
                ed.WriteMessage("\nPiping UseWeldGaps = " + piping.UseWeldGaps);
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nP3DSETTINGS error: " + ex.Message);
            }
        }
    }
}
