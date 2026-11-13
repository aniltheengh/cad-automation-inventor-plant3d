using System.Collections.Specialized;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.ProcessPower.PartsRepository.Specification;
using Autodesk.ProcessPower.PnP3dObjects;

namespace Plant3dSdkSamples
{
    /// <summary>
    /// Read parts from a piping spec.
    /// Namespaces and types confirmed on Autodesk Community:
    ///   Autodesk.ProcessPower.PartsRepository.Specification
    ///   SpecManager.GetSpecManager(), SelectParts, HasType, SpecPartReader
    ///   Autodesk.ProcessPower.PnP3dObjects.SpecPart (PropNames, PropValue, NominalDiameter)
    ///   PipePartSpecification.OpenSpecification(path) for the .pspx file
    /// There is no Spec Editor SDK (Autodesk Community). Do not treat this as a spec writer.
    ///
    /// To PLACE a pipe after you have a SpecPart, start from the official SDK samples:
    ///   Samples\Piping\CreatePipeline
    ///   Samples\Piping\PipeRouting
    /// Published pattern (confirm overloads in plantsdk_ref.chm and those samples):
    ///   DataLinksManager3d dlm3d = DataLinksManager3d.Get3dManager(dlm);
    ///   using (PipingObjectAdder adder = new PipingObjectAdder(dlm3d, db))
    ///       adder.Add(specPart, pipe);
    /// </summary>
    public class Commands_Specs
    {
        [CommandMethod("P3DSPECLIST")]
        public static void ListSpecParts()
        {
            Editor ed = PlantHelpers.Ed;
            if (PlantHelpers.RequireProject(ed) == null)
                return;

            PromptResult specRes = ed.GetString("\nSpec name (the name as used in the project, for example CS150 or CS300): ");
            if (specRes.Status != PromptStatus.OK)
                return;

            PromptResult typeRes = ed.GetString("\nPart type (for example Pipe, Elbow, Flange, GateValve): ");
            if (typeRes.Status != PromptStatus.OK)
                return;

            string specName = specRes.StringResult;
            string partType = typeRes.StringResult;

            try
            {
                SpecManager specMgr = SpecManager.GetSpecManager();
                if (!specMgr.HasType(specName, partType))
                {
                    ed.WriteMessage("\nSpec '" + specName + "' has no type '" + partType + "' (HasType returned false).");
                    return;
                }

                PromptResult sizeRes = ed.GetString("\nOptional NominalDiameter filter (empty = all sizes): ");
                string sizeFilter = sizeRes.Status == PromptStatus.OK ? sizeRes.StringResult : "";

                SpecPartReader reader;
                if (!string.IsNullOrWhiteSpace(sizeFilter))
                {
                    StringCollection names = new StringCollection();
                    StringCollection values = new StringCollection();
                    names.Add("NominalDiameter");
                    values.Add(sizeFilter);
                    reader = specMgr.SelectParts(specName, partType, names, values);
                }
                else
                {
                    reader = specMgr.SelectParts(specName, partType);
                }

                int count = 0;
                while (reader.Next())
                {
                    SpecPart part = reader.Current;
                    count++;
                    ed.WriteMessage("\n--- " + part.Name);
                    if (part.NominalDiameter != null)
                        ed.WriteMessage("\n  NominalDiameter = " + part.NominalDiameter.Value);
                    foreach (string propName in part.PropNames)
                    {
                        object? value = part.PropValue(propName);
                        if (value != null)
                            ed.WriteMessage("\n  " + propName + " = " + value);
                    }
                }

                ed.WriteMessage("\nListed " + count + " part(s).");
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nP3DSPECLIST error: " + ex.Message);
                ed.WriteMessage("\nIf the Parts Repository DLL name is wrong, fix the HintPath in the .csproj.");
            }
        }
    }
}
