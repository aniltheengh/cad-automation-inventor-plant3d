// FrameBuilder.cs - generates a welded-style table frame: 4 legs + 2 long rails + 2 short rails,
// each a square-tube part, placed in a new assembly with transformation matrices.
// Shows: creating parts from scratch, user parameters driving features, iProperties,
// saving, ComponentOccurrences.Add with a Matrix, grounding, and a parameter-only rebuild path.
using Inventor;
using IO = System.IO;

namespace FrameConfigurator
{
    public sealed record FrameSpec(double Length, double Width, double Height, double Tube, double Wall, string Folder, string Name);

    public sealed class FrameBuilder
    {
        private readonly Inventor.Application _app;
        private readonly TransientGeometry _tg;
        private const double MM = 0.1;   // API length unit is cm

        public FrameBuilder(Inventor.Application app) { _app = app; _tg = app.TransientGeometry; }

        public string Build(FrameSpec s, IProgress<string>? log = null)
        {
            if (s.Wall * 2 >= s.Tube) throw new ArgumentException("Wall too thick for tube size.");
            if (s.Length <= 2 * s.Tube || s.Width <= 2 * s.Tube) throw new ArgumentException("Frame smaller than two tubes.");
            IO.Directory.CreateDirectory(s.Folder);

            double railL = s.Length - 2 * s.Tube, railW = s.Width - 2 * s.Tube;
            string leg = MakeTube(s, "LEG", s.Height, log);
            string rl = MakeTube(s, "RAIL-L", railL, log);
            string rw = MakeTube(s, "RAIL-W", railW, log);

            string tpl = _app.FileManager.GetTemplateFile(DocumentTypeEnum.kAssemblyDocumentObject,
                SystemOfMeasureEnum.kMetricSystemOfMeasure, DraftingStandardEnum.kISO_DraftingStandard);
            var asm = (AssemblyDocument)_app.Documents.Add(DocumentTypeEnum.kAssemblyDocumentObject, tpl, true);
            ComponentOccurrences occs = asm.ComponentDefinition.Occurrences;

            // Tubes are modelled along +Z from the origin, centred on X/Y (see MakeTube).
            double h = s.Tube / 2, top = s.Height - s.Tube / 2;
            // 4 legs at the corners
            foreach (var (x, y) in new[] { (h, h), (s.Length - h, h), (s.Length - h, s.Width - h), (h, s.Width - h) })
                Place(occs, leg, Translate(x, y, 0));
            // long rails: rotate tube axis Z -> X (rotate +90 deg about Y)
            foreach (double y in new[] { h, s.Width - h })
                Place(occs, rl, Compose(RotY(Math.PI / 2), Translate(s.Tube, y, top)));
            // short rails: rotate Z -> Y (rotate -90 deg about X)
            foreach (double x in new[] { h, s.Length - h })
                Place(occs, rw, Compose(RotX(-Math.PI / 2), Translate(x, s.Tube, top)));

            asm.PropertySets["Design Tracking Properties"]["Part Number"].Value = s.Name;
            asm.PropertySets["Design Tracking Properties"]["Description"].Value =
                $"Frame {s.Length}x{s.Width}x{s.Height} tube {s.Tube}x{s.Wall}";
            string file = IO.Path.Combine(s.Folder, s.Name + ".iam");
            asm.SaveAs(file, false);
            _app.ActiveView.Fit(true);
            log?.Report("saved " + file);
            return file;
        }

        /// <summary>Square tube along +Z, length driven by user parameter "L".</summary>
        private string MakeTube(FrameSpec s, string suffix, double length, IProgress<string>? log)
        {
            string tpl = _app.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject,
                SystemOfMeasureEnum.kMetricSystemOfMeasure, DraftingStandardEnum.kISO_DraftingStandard);
            var part = (PartDocument)_app.Documents.Add(DocumentTypeEnum.kPartDocumentObject, tpl, false); // invisible
            PartComponentDefinition cd = part.ComponentDefinition;
            var inv = System.Globalization.CultureInfo.InvariantCulture;
            UserParameters up = cd.Parameters.UserParameters;
            up.AddByExpression("A", s.Tube.ToString(inv) + " mm", "mm");
            up.AddByExpression("t", s.Wall.ToString(inv) + " mm", "mm");
            up.AddByExpression("L", length.ToString(inv) + " mm", "mm");

            PlanarSketch sk = cd.Sketches.Add(cd.WorkPlanes[3]);
            double a = s.Tube / 2 * MM, b = (s.Tube / 2 - s.Wall) * MM;
            sk.SketchLines.AddAsTwoPointCenteredRectangle(_tg.CreatePoint2d(0, 0), _tg.CreatePoint2d(a, a));
            sk.SketchLines.AddAsTwoPointCenteredRectangle(_tg.CreatePoint2d(0, 0), _tg.CreatePoint2d(b, b));
            Profile prof = sk.Profiles.AddForSolid();   // two nested loops -> hollow section
            ExtrudeDefinition def = cd.Features.ExtrudeFeatures.CreateExtrudeDefinition(prof, PartFeatureOperationEnum.kJoinOperation);
            def.SetDistanceExtent("L", PartFeatureExtentDirectionEnum.kPositiveExtentDirection);
            cd.Features.ExtrudeFeatures.Add(def);

            string pn = $"{s.Name}-{suffix}";
            part.PropertySets["Design Tracking Properties"]["Part Number"].Value = pn;
            part.PropertySets["Design Tracking Properties"]["Description"].Value =
                $"SHS {s.Tube}x{s.Tube}x{s.Wall} L={length:0.#}";
            part.PropertySets["Design Tracking Properties"]["Stock Number"].Value = $"SHS{s.Tube}x{s.Wall}";
            string file = IO.Path.Combine(s.Folder, pn + ".ipt");
            part.SaveAs(file, false);
            part.Close(true);
            log?.Report("part " + pn);
            return file;
        }

        private void Place(ComponentOccurrences occs, string file, Matrix m)
        {
            ComponentOccurrence o = occs.Add(file, m);
            o.Grounded = true;   // fixed in place; for a "real" configurator use iMates/constraints instead
        }

        // ---- matrix helpers (positions in mm, converted to cm) ----
        private Matrix Translate(double x, double y, double z)
        {
            Matrix m = _tg.CreateMatrix();
            m.SetTranslation(_tg.CreateVector(x * MM, y * MM, z * MM));
            return m;
        }
        private Matrix RotX(double ang) { Matrix m = _tg.CreateMatrix(); m.SetToRotation(ang, _tg.CreateVector(1, 0, 0), _tg.CreatePoint(0, 0, 0)); return m; }
        private Matrix RotY(double ang) { Matrix m = _tg.CreateMatrix(); m.SetToRotation(ang, _tg.CreateVector(0, 1, 0), _tg.CreatePoint(0, 0, 0)); return m; }
        /// <summary>first apply 'a', then 'b' (b * a).</summary>
        private static Matrix Compose(Matrix a, Matrix b) { Matrix r = a.Copy(); r.TransformBy(b); return r; }  // TransformBy = "this, followed by b"
    }
}
