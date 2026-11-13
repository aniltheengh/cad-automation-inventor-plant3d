using System.Collections.Generic;
using System.IO;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;
using AcadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Plant3dSdkSamples
{
    /// <summary>
    /// Export Data Manager properties of Plant-linked entities in model space to a CSV.
    /// Uses only DataLinksManager.FindAcPpRowId + GetAllProperties (ADN LAB 3).
    /// Entities with no Plant link are skipped (DLException / ObjectDoesNotHaveLink).
    /// </summary>
    public class Commands_ExportCsv
    {
        [CommandMethod("P3DEXPORTCSV")]
        public static void ExportCsv()
        {
            Editor ed = PlantHelpers.Ed;
            PlantProject? plant = PlantHelpers.RequireProject(ed);
            if (plant == null)
                return;

            PromptKeywordOptions partOpts = new PromptKeywordOptions("\nProject part to read");
            partOpts.Keywords.Add("Piping");
            partOpts.Keywords.Add("PnId");
            partOpts.Keywords.Default = "Piping";
            partOpts.AllowNone = true;
            PromptResult partRes = ed.GetKeywords(partOpts);
            if (partRes.Status != PromptStatus.OK && partRes.Status != PromptStatus.None)
                return;

            string partName = string.IsNullOrEmpty(partRes.StringResult) ? "Piping" : partRes.StringResult;
            string[] aliases = partName.Equals("PnId", StringComparison.OrdinalIgnoreCase)
                ? new[] { "PnId", "PnID" }
                : new[] { partName };

            DataLinksManager? dlm = PlantHelpers.GetManager(plant, ed, aliases);
            if (dlm == null)
                return;

            string defaultPath = Path.Combine(
                plant.ProjectFolderPath,
                "PlantProperties_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");

            PromptSaveFileOptions saveOpts = new PromptSaveFileOptions("\nCSV file")
            {
                Filter = "CSV (*.csv)|*.csv",
                InitialFileName = defaultPath
            };
            PromptFileNameResult saveRes = ed.GetFileNameForSave(saveOpts);
            if (saveRes.Status != PromptStatus.OK)
                return;

            Database db = AcadApp.DocumentManager.MdiActiveDocument.Database;
            int exported = 0;
            int skipped = 0;
            HashSet<int> seenRows = new HashSet<int>();

            try
            {
                using (Transaction tr = db.TransactionManager.StartTransaction())
                using (StreamWriter writer = new StreamWriter(saveRes.StringResult, false, Encoding.UTF8))
                {
                    writer.WriteLine("RowId,Handle,Property,Value");

                    BlockTable bt = (BlockTable)tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord ms = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                    foreach (ObjectId id in ms)
                    {
                        Entity? ent = tr.GetObject(id, OpenMode.ForRead) as Entity;
                        if (ent == null)
                            continue;

                        int rowId;
                        try
                        {
                            rowId = dlm.FindAcPpRowId(ent.ObjectId);
                        }
                        catch (System.Exception)
                        {
                            skipped++;
                            continue;
                        }

                        if (rowId <= 0 || !seenRows.Add(rowId))
                            continue;

                        List<KeyValuePair<string, string>> props;
                        try
                        {
                            props = dlm.GetAllProperties(rowId, true);
                        }
                        catch (System.Exception)
                        {
                            skipped++;
                            continue;
                        }

                        string handle = ent.Handle.ToString();
                        foreach (KeyValuePair<string, string> kv in props)
                        {
                            writer.WriteLine(string.Join(",",
                                Csv(rowId.ToString()),
                                Csv(handle),
                                Csv(kv.Key),
                                Csv(kv.Value)));
                        }
                        exported++;
                    }

                    tr.Commit();
                }

                ed.WriteMessage("\nWrote " + exported + " Plant row(s) to " + saveRes.StringResult);
                ed.WriteMessage("\nSkipped " + skipped + " entity/row(s) with no link or no properties.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nP3DEXPORTCSV error: " + ex.Message);
            }
        }

        private static string Csv(string? value)
        {
            string text = value ?? "";
            if (text.Contains('"') || text.Contains(',') || text.Contains('\n') || text.Contains('\r'))
                return "\"" + text.Replace("\"", "\"\"") + "\"";
            return text;
        }
    }
}
