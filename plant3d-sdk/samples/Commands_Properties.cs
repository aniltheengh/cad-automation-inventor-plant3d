using System.Collections.Generic;
using System.Collections.Specialized;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.DataLinks;
using Autodesk.ProcessPower.PlantInstance;
using Autodesk.ProcessPower.ProjectManager;

namespace Plant3dSdkSamples
{
    /// <summary>
    /// Read and write part properties through DataLinksManager.
    /// Sources: ADN LAB 3 (P&amp;ID) and LAB 4 (piping), plus Autodesk Community threads
    /// on GetAllProperties / SetProperties / GetProperties.
    /// </summary>
    public class Commands_Properties
    {
        [CommandMethod("P3DPROPS")]
        public static void PrintProperties()
        {
            Editor ed = PlantHelpers.Ed;
            PlantProject? plant = PlantHelpers.RequireProject(ed);
            if (plant == null)
                return;

            PromptKeywordOptions partOpts = new PromptKeywordOptions("\nProject part");
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

            PromptEntityResult pick = ed.GetEntity("\nPick a Plant 3D or P&ID object: ");
            if (pick.Status != PromptStatus.OK)
                return;

            try
            {
                int rowId = dlm.FindAcPpRowId(pick.ObjectId);
                ed.WriteMessage("\nRow id (PnPID) = " + rowId);

                // LAB 3: the bool is described as the "current version" flag.
                List<KeyValuePair<string, string>> props = dlm.GetAllProperties(rowId, true);
                foreach (KeyValuePair<string, string> kv in props)
                {
                    ed.WriteMessage("\n" + kv.Key + " = " + kv.Value);
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nP3DPROPS error: " + ex.Message);
                ed.WriteMessage("\nIf this is ObjectDoesNotHaveLink, the entity is not linked in this project part.");
            }
        }

        [CommandMethod("P3DSETPROP")]
        public static void SetOneProperty()
        {
            Editor ed = PlantHelpers.Ed;
            PlantProject? plant = PlantHelpers.RequireProject(ed);
            if (plant == null)
                return;

            PromptKeywordOptions partOpts = new PromptKeywordOptions("\nProject part");
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

            PromptEntityResult pick = ed.GetEntity("\nPick a Plant object to edit: ");
            if (pick.Status != PromptStatus.OK)
                return;

            PromptResult nameRes = ed.GetString("\nProperty name (must already exist, for example Area or Manufacturer): ");
            if (nameRes.Status != PromptStatus.OK)
                return;

            PromptResult valueRes = ed.GetString("\nNew value: ");
            if (valueRes.Status != PromptStatus.OK)
                return;

            try
            {
                int rowId = dlm.FindAcPpRowId(pick.ObjectId);
                List<KeyValuePair<string, string>> existing = dlm.GetAllProperties(rowId, true);
                bool found = false;
                foreach (KeyValuePair<string, string> kv in existing)
                {
                    if (string.Equals(kv.Key, nameRes.StringResult, StringComparison.OrdinalIgnoreCase))
                    {
                        found = true;
                        ed.WriteMessage("\nCurrent " + kv.Key + " = " + kv.Value);
                        break;
                    }
                }

                if (!found)
                {
                    ed.WriteMessage("\nProperty '" + nameRes.StringResult + "' was not in GetAllProperties. No write was done.");
                    return;
                }

                // SetProperties takes two parallel StringCollection lists (LAB 3 / Community).
                // Overloads exist for a row id and for an ObjectId.
                StringCollection names = new StringCollection();
                StringCollection values = new StringCollection();
                names.Add(nameRes.StringResult);
                values.Add(valueRes.StringResult);
                dlm.SetProperties(rowId, names, values);
                ed.WriteMessage("\nUpdated. Save the drawing and check Data Manager. Calculated fields may ignore the write.");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nP3DSETPROP error: " + ex.Message);
            }
        }
    }
}
