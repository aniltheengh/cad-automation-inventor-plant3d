using System.Runtime.InteropServices;
using System.Text.Json;
using CadAutomation.Gears;
using Inventor;
using IO = System.IO;

namespace GearDAPlugin
{
    [ComVisible(true)]
    public class GearAutomation
    {
        private readonly InventorServer _server;
        public GearAutomation(InventorServer server) => _server = server;

        /// <summary>Called when the work item passes an input document with /i.</summary>
        public void Run(Document doc) => RunWithArguments(doc, _server.TransientObjects.CreateNameValueMap());

        /// <summary>
        /// Called by DA. Extra command-line arguments arrive in the map; the usual convention
        /// (verify against the current APS docs / VS template) is keys "_1", "_2"... in order.
        /// Here: _1 = path of params.json  {"module":2,"teeth":40,"faceWidth":20,"bore":12}
        /// Output: gear.ipt in the working folder (activity parameter "OutputIpt", localName "gear.ipt").
        /// </summary>
        public void RunWithArguments(Document? doc, NameValueMap map)
        {
            string paramsFile = map.Count > 0 ? Convert.ToString(map.Value["_1"]) ?? "params.json" : "params.json";
            var p = JsonSerializer.Deserialize<GearParams>(IO.File.ReadAllText(paramsFile),
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? throw new InvalidOperationException("params.json is empty");
            GearGeometry g = GearMath.Compute(new GearSpec(p.Module, p.Teeth, 20, p.FaceWidth, p.Bore));

            // InventorServer has Documents.Add like the desktop app; template path comes from the engine's defaults.
            string tpl = _server.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject,
                SystemOfMeasureEnum.kMetricSystemOfMeasure, DraftingStandardEnum.kISO_DraftingStandard);
            var part = (PartDocument)_server.Documents.Add(DocumentTypeEnum.kPartDocumentObject, tpl, true);
            PartComponentDefinition cd = part.ComponentDefinition;
            TransientGeometry tg = _server.TransientGeometry;

            // Minimal geometry: tip-diameter blank with bore (full involute: reuse 02-SpurGearGenerator code).
            PlanarSketch sk = cd.Sketches.Add(cd.WorkPlanes[3]);
            sk.SketchCircles.AddByCenterRadius(tg.CreatePoint2d(0, 0), g.TipRadius / 10.0);
            if (p.Bore > 0) sk.SketchCircles.AddByCenterRadius(tg.CreatePoint2d(0, 0), p.Bore / 20.0);
            ExtrudeDefinition def = cd.Features.ExtrudeFeatures.CreateExtrudeDefinition(sk.Profiles.AddForSolid(), PartFeatureOperationEnum.kNewBodyOperation);
            def.SetDistanceExtent(p.FaceWidth / 10.0, PartFeatureExtentDirectionEnum.kPositiveExtentDirection);
            cd.Features.ExtrudeFeatures.Add(def);
            part.PropertySets["Design Tracking Properties"]["Part Number"].Value = $"GEAR-M{p.Module}-Z{p.Teeth}";

            string outFile = IO.Path.Combine(IO.Directory.GetCurrentDirectory(), "gear.ipt");
            part.SaveAs(outFile, false);
            Console.WriteLine($"Saved {outFile}  da={2 * g.TipRadius:F2} mm");   // appears in the work item report
        }

        private sealed record GearParams(double Module, int Teeth, double FaceWidth = 20, double Bore = 0);
    }
}
