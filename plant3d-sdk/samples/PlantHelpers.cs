using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Plant3dSdkSamples
{
    /// <summary>
    /// Small helpers shared by the sample commands. Pattern from ADN Plant3DTraining labs:
    /// PlantApplication.CurrentProject → ProjectParts[name] → DataLinksManager.
    /// </summary>
    internal static class PlantHelpers
    {
        public static Editor Ed => AcadApp.DocumentManager.MdiActiveDocument.Editor;

        public static Document Doc => AcadApp.DocumentManager.MdiActiveDocument;

        public static PlantProject? RequireProject(Editor ed)
        {
            PlantProject? project = PlantApplication.CurrentProject;
            if (project == null)
            {
                ed.WriteMessage("\nNo Plant 3D project is open. Open a project in Project Manager first.");
                return null;
            }
            return project;
        }

        /// <summary>
        /// Labs use both "PnId" and "PnID". Try both if the first key is missing.
        /// </summary>
        public static Project? GetProjectPart(PlantProject plant, Editor ed, params string[] names)
        {
            foreach (string name in names)
            {
                try
                {
                    Project part = plant.ProjectParts[name];
                    if (part != null)
                        return part;
                }
                catch (System.Exception)
                {
                    // Key not present in this project; try the next alias.
                }
            }

            ed.WriteMessage("\nProject part not found. Tried: " + string.Join(", ", names));
            return null;
        }

        public static DataLinksManager? GetManager(PlantProject plant, Editor ed, params string[] partNames)
        {
            Project? part = GetProjectPart(plant, ed, partNames);
            return part?.DataLinksManager;
        }
    }
}
