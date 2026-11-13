// HelloCommands.cs
//   P3DHELLO : "am I connected?" - prints the application, the active document, the Plant project and its
//              parts, and which Plant 3D assemblies are loaded from which folder. Reads only; changes nothing.
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using AcApp = Autodesk.AutoCAD.ApplicationServices.Application;

[assembly: CommandClass(typeof(Plant3DAddinStarter.HelloCommands))]

namespace Plant3DAddinStarter
{
    public class HelloCommands
    {
        [CommandMethod("P3DHELLO")]
        public void Hello()
        {
            Document? doc = AcApp.DocumentManager.MdiActiveDocument;
            if (doc == null) return;                    // commands need a drawing window for their output
            Editor ed = doc.Editor;

            // ---- 1. the application: plain AutoCAD API, works in every AutoCAD-based product
            ed.WriteMessage($"\n--- Application");
            ed.WriteMessage($"\n  AutoCAD release : {AcApp.GetSystemVariable("ACADVER")}");
            ed.WriteMessage($"\n  Registry key    : HKCU\\{HostApplicationServices.Current.UserRegistryProductRootKey}");
            ed.WriteMessage($"\n  Program         : {Environment.ProcessPath}");
            ed.WriteMessage($"\n  .NET runtime    : {RuntimeInformation.FrameworkDescription}");

            // ---- 2. the active document: Document = window + Editor, Database = the DWG content
            ed.WriteMessage($"\n--- Active document");
            ed.WriteMessage($"\n  Name            : {doc.Name}");
            ed.WriteMessage($"\n  DWG file        : {doc.Database.Filename}");

            // ---- 3. Plant 3D: a separate method, see Report.Run
            Report.Run("P3DHELLO", () => PlantInfo(ed));
            ed.WriteMessage("\n");
        }

        private static void PlantInfo(Editor ed)
        {
            // PlantApplication is the Plant 3D counterpart of AutoCAD's Application class.
            PlantProject project = PlantApplication.CurrentProject
                ?? throw new InvalidOperationException("No Plant 3D project is open (Project Manager).");

            ed.WriteMessage($"\n--- Plant project");
            ed.WriteMessage($"\n  Name            : {project.Name}");
            ed.WriteMessage($"\n  Folder          : {project.ProjectFolderPath}");
            ed.WriteMessage($"\n  Active drawing  : type '{PnPProjectUtils.GetActiveDocumentType()}'");

            // A Plant project is a set of parts (P&ID, 3D piping, orthos, isos...), each with its own database.
            ed.WriteMessage($"\n--- Project parts");
            foreach (Project part in project.ProjectParts)
            {
                int drawings = part.GetPnPDrawingFiles()?.Count ?? 0;
                ed.WriteMessage($"\n  {project.ProjectPartName(part),-10} {drawings,4} drawing(s)   {part.ProjectDirectory}");
            }

            // Which Plant 3D .NET assemblies are in memory, and from which folder? Use this list to find the
            // DLLs to reference in your own project.
            ed.WriteMessage($"\n--- Loaded Plant 3D assemblies");
            foreach (System.Reflection.Assembly asm in ApiMapCommands.PlantAssemblies())
            {
                System.Reflection.AssemblyName name = asm.GetName();
                ed.WriteMessage($"\n  {name.Name,-28} {name.Version,-16} {Path.GetDirectoryName(asm.Location)}");
            }
        }
    }
}
