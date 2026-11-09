// 02-SpurGearGenerator (C# EXE) - builds a real involute spur gear part in Inventor.
//
//   dotnet run -- <module> <teeth> [pressureAngleDeg=20] [faceWidth=20] [bore=0] [outFolder]
//   dotnet run -- 2 40 20 20 12 C:\Temp\Gears
//
// Method (robust, easy to debug):
//   1. Blank   : extrude a circle at the tip diameter (+ bore circle) by the face width.
//   2. Space   : sketch ONE tooth space (two involute splines, two radial lines, root arc, outer arc).
//   3. Cut     : extrude-cut the space through all.
//   4. Pattern : circular pattern the cut z times about the Z axis.
// Inventor API units are ALWAYS cm and radians -> divide mm by 10.
// For production gears use Design Accelerator > Spur Gear (has root fillet, tolerances, strength calc).
using CadAutomation.Gears;
using CadAutomation.Shared;
using Inventor;
using IO = System.IO;

internal static class Program
{
    private const double MM = 0.1; // mm -> cm

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("usage: SpurGearGenerator <module> <teeth> [alphaDeg=20] [faceWidth=20] [bore=0] [outFolder]");
            return 2;
        }
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var spec = new GearSpec(
            Module: double.Parse(args[0], inv),
            Teeth: int.Parse(args[1], inv),
            PressureAngleDeg: args.Length > 2 ? double.Parse(args[2], inv) : 20,
            FaceWidth: args.Length > 3 ? double.Parse(args[3], inv) : 20,
            Bore: args.Length > 4 ? double.Parse(args[4], inv) : 0);
        string outFolder = args.Length > 5 ? args[5] : IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "Gears");

        GearGeometry g = GearMath.Compute(spec);
        Console.WriteLine($"d={2 * g.PitchRadius:F3} da={2 * g.TipRadius:F3} df={2 * g.RootRadius:F3} db={2 * g.BaseRadius:F3} mm");
        if (GearMath.IsPointed(g)) { Console.Error.WriteLine("Teeth are pointed - reduce profile shift or increase z."); return 3; }
        if (spec.Teeth < GearMath.MinTeethNoUndercut(spec.PressureAngleDeg))
            Console.WriteLine($"Warning: z < {GearMath.MinTeethNoUndercut(spec.PressureAngleDeg):F1} -> real cutting would undercut; model shows ideal involute only.");

        Inventor.Application app = InventorConnector.Connect(startIfNotRunning: true);
        app.SilentOperation = true; // no dialogs while we build
        try
        {
            string tpl = app.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject,
                                                         SystemOfMeasureEnum.kMetricSystemOfMeasure,
                                                         DraftingStandardEnum.kISO_DraftingStandard);
            var part = (PartDocument)app.Documents.Add(DocumentTypeEnum.kPartDocumentObject, tpl, true);
            PartComponentDefinition cd = part.ComponentDefinition;
            TransientGeometry tg = app.TransientGeometry;

            // --- user parameters (documentation + drive extrude depth / pattern count) ---
            UserParameters up = cd.Parameters.UserParameters;
            up.AddByExpression("m", spec.Module.ToString(inv) + " mm", "mm");
            up.AddByValue("z", spec.Teeth, "ul");
            up.AddByExpression("alpha", spec.PressureAngleDeg.ToString(inv) + " deg", "deg");
            up.AddByExpression("b", spec.FaceWidth.ToString(inv) + " mm", "mm");
            up.AddByExpression("d_pitch", "m * z", "mm");

            // Undo wrapper: one Ctrl+Z removes the whole gear build
            Transaction tx = app.TransactionManager.StartTransaction((_Document)part, "Create spur gear");

            // 1. Blank
            PlanarSketch sk1 = cd.Sketches.Add(cd.WorkPlanes[3]); // 1=YZ, 2=XZ, 3=XY
            sk1.SketchCircles.AddByCenterRadius(tg.CreatePoint2d(0, 0), g.TipRadius * MM);
            if (spec.Bore > 0) sk1.SketchCircles.AddByCenterRadius(tg.CreatePoint2d(0, 0), spec.Bore / 2 * MM);
            Profile blankProfile = sk1.Profiles.AddForSolid();
            ExtrudeDefinition exDef = cd.Features.ExtrudeFeatures.CreateExtrudeDefinition(blankProfile, PartFeatureOperationEnum.kNewBodyOperation);
            exDef.SetDistanceExtent("b", PartFeatureExtentDirectionEnum.kPositiveExtentDirection);
            ExtrudeFeature blank = cd.Features.ExtrudeFeatures.Add(exDef);
            blank.Name = "GearBlank";

            // 2. One tooth space between tooth 0 (centred on +X) and tooth 1 (centred at 2*pi/z)
            PlanarSketch sk2 = cd.Sketches.Add(cd.WorkPlanes[3]);
            sk2.Name = "ToothSpace";
            double pitchAng = 2 * Math.PI / spec.Teeth;
            double rOut = g.TipRadius + 0.5 * spec.Module;  // cutter overshoots the blank
            double rLow = Math.Max(g.BaseRadius, g.RootRadius);
            var flank = GearMath.UpperFlank(g, 15);          // upper flank of tooth 0 (angle +theta)

            Point2d P(double r, double ang) => tg.CreatePoint2d(r * Math.Cos(ang) * MM, r * Math.Sin(ang) * MM);
            double thLow = GearMath.FlankAngle(g, rLow), thTip = GearMath.FlankAngle(g, g.TipRadius);

            // key points, side A = tooth 0 upper flank, side B = tooth 1 lower flank (mirror about space centre)
            SketchPoints sp = sk2.SketchPoints;
            SketchPoint aRoot = sp.Add(P(g.RootRadius, thLow));
            SketchPoint aLow  = rLow > g.RootRadius ? sp.Add(P(rLow, thLow)) : aRoot;
            SketchPoint aTip  = sp.Add(P(g.TipRadius, thTip));
            SketchPoint aOut  = sp.Add(P(rOut, thTip));
            SketchPoint bRoot = sp.Add(P(g.RootRadius, pitchAng - thLow));
            SketchPoint bLow  = rLow > g.RootRadius ? sp.Add(P(rLow, pitchAng - thLow)) : bRoot;
            SketchPoint bTip  = sp.Add(P(g.TipRadius, pitchAng - thTip));
            SketchPoint bOut  = sp.Add(P(rOut, pitchAng - thTip));

            ObjectCollection fitA = app.TransientObjects.CreateObjectCollection();
            ObjectCollection fitB = app.TransientObjects.CreateObjectCollection();
            fitA.Add(aLow); fitB.Add(bLow);
            for (int i = 1; i < flank.Count - 1; i++)
            {
                var (x, y) = flank[i];
                double r = Math.Sqrt(x * x + y * y), th = Math.Atan2(y, x);
                fitA.Add(P(r, th));
                fitB.Add(P(r, pitchAng - th));
            }
            fitA.Add(aTip); fitB.Add(bTip);
            sk2.SketchSplines.Add(fitA);
            sk2.SketchSplines.Add(fitB);
            if (rLow > g.RootRadius)
            {
                sk2.SketchLines.AddByTwoPoints(aRoot, aLow);   // radial below base circle
                sk2.SketchLines.AddByTwoPoints(bRoot, bLow);
            }
            sk2.SketchLines.AddByTwoPoints(aTip, aOut);
            sk2.SketchLines.AddByTwoPoints(bTip, bOut);
            Point2d c = tg.CreatePoint2d(0, 0);
            sk2.SketchArcs.AddByCenterStartEndPoint(c, aRoot, bRoot, true);  // CCW from A to B
            sk2.SketchArcs.AddByCenterStartEndPoint(c, aOut, bOut, true);

            // 3. Cut
            Profile spaceProfile = sk2.Profiles.AddForSolid();
            ExtrudeDefinition cutDef = cd.Features.ExtrudeFeatures.CreateExtrudeDefinition(spaceProfile, PartFeatureOperationEnum.kCutOperation);
            cutDef.SetThroughAllExtent(PartFeatureExtentDirectionEnum.kSymmetricExtentDirection);
            ExtrudeFeature cut = cd.Features.ExtrudeFeatures.Add(cutDef);
            cut.Name = "ToothSpaceCut";

            // 4. Pattern z times about Z
            ObjectCollection parents = app.TransientObjects.CreateObjectCollection();
            parents.Add(cut);
            CircularPatternFeatureDefinition patDef = cd.Features.CircularPatternFeatures.CreateDefinition(
                parents, cd.WorkAxes[3], true, "z", "360 deg", true);
            CircularPatternFeature pat = cd.Features.CircularPatternFeatures.AddByDefinition(patDef);
            pat.Name = "Teeth";

            tx.End();

            // iProperties
            string pn = $"GEAR-M{spec.Module:0.##}-Z{spec.Teeth}";
            part.PropertySets["Design Tracking Properties"]["Part Number"].Value = pn;
            part.PropertySets["Design Tracking Properties"]["Description"].Value =
                $"Spur gear m={spec.Module} z={spec.Teeth} a={spec.PressureAngleDeg}deg b={spec.FaceWidth}";

            IO.Directory.CreateDirectory(outFolder);
            string file = IO.Path.Combine(outFolder, pn + ".ipt");
            part.SaveAs(file, false);
            app.ActiveView.Fit(true);
            Console.WriteLine("Saved " + file);
            return 0;
        }
        finally
        {
            app.SilentOperation = false;
        }
    }
}
