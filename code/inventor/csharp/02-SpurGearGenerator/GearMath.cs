// GearMath.cs - pure involute spur-gear geometry (no Inventor dependency, unit-testable).
// Same formulas as tools/gear_math.py; tools/test_tools.py checks both agree.
// All lengths in mm, angles in radians. External spur gear, standard full-depth teeth
// (addendum = m(1+x), dedendum = m(1.25-x)). No root fillet (trochoid) is modelled.
using System;
using System.Collections.Generic;

namespace CadAutomation.Gears
{
    public sealed record GearSpec(double Module, int Teeth, double PressureAngleDeg = 20.0,
                                  double FaceWidth = 20.0, double Bore = 0.0, double ProfileShift = 0.0);

    public sealed class GearGeometry
    {
        public double PitchRadius, BaseRadius, TipRadius, RootRadius, Alpha;
        public double HalfToothAngleAtPitch;      // psi_p
        public double CircularPitch;
        public double ToothThicknessAtPitch;       // arc thickness
    }

    public static class GearMath
    {
        public static double Inv(double a) => Math.Tan(a) - a;

        public static GearGeometry Compute(GearSpec s)
        {
            if (s.Module <= 0) throw new ArgumentOutOfRangeException(nameof(s.Module));
            if (s.Teeth < 6) throw new ArgumentOutOfRangeException(nameof(s.Teeth), "need >= 6 teeth");
            double a = s.PressureAngleDeg * Math.PI / 180.0;
            var g = new GearGeometry
            {
                Alpha = a,
                PitchRadius = s.Module * s.Teeth / 2.0,
            };
            g.BaseRadius = g.PitchRadius * Math.Cos(a);
            g.TipRadius  = g.PitchRadius + s.Module * (1.0 + s.ProfileShift);
            g.RootRadius = g.PitchRadius - s.Module * (1.25 - s.ProfileShift);
            g.HalfToothAngleAtPitch = Math.PI / (2.0 * s.Teeth) + 2.0 * s.ProfileShift * Math.Tan(a) / s.Teeth;
            g.CircularPitch = Math.PI * s.Module;
            g.ToothThicknessAtPitch = 2.0 * g.HalfToothAngleAtPitch * g.PitchRadius;
            if (s.Bore > 0 && s.Bore / 2.0 >= g.RootRadius * 0.8)
                throw new ArgumentException("Bore too large for this gear (keep bore < 0.8 x root diameter).");
            return g;
        }

        /// <summary>Polar angle of the tooth flank (tooth centred on +X) at radius r (r >= base radius).</summary>
        public static double FlankAngle(GearGeometry g, double r)
        {
            double rr = Math.Max(r, g.BaseRadius);
            double ar = Math.Acos(g.BaseRadius / rr);
            return g.HalfToothAngleAtPitch + Inv(g.Alpha) - Inv(ar);
        }

        /// <summary>Tooth thickness (chordal-free arc length) at radius r.</summary>
        public static double ThicknessAt(GearGeometry g, double r) => 2.0 * FlankAngle(g, r) * r;

        /// <summary>
        /// Points (x,y) of one involute flank from max(base, root) to tip, at polar angle +theta (upper flank).
        /// </summary>
        public static List<(double X, double Y)> UpperFlank(GearGeometry g, int n = 12)
        {
            var pts = new List<(double, double)>(n);
            double r0 = Math.Max(g.BaseRadius, g.RootRadius);
            for (int i = 0; i < n; i++)
            {
                double r = r0 + (g.TipRadius - r0) * i / (n - 1);
                double th = FlankAngle(g, r);
                pts.Add((r * Math.Cos(th), r * Math.Sin(th)));
            }
            return pts;
        }

        /// <summary>Tooth is pointed (tip thickness <= 0) - happens for small z or big positive shift.</summary>
        public static bool IsPointed(GearGeometry g) => FlankAngle(g, g.TipRadius) <= 0;

        /// <summary>Minimum teeth without undercut for a standard rack (x=0): z_min = 2 / sin^2(alpha).</summary>
        public static double MinTeethNoUndercut(double pressureAngleDeg)
        {
            double a = pressureAngleDeg * Math.PI / 180.0;
            return 2.0 / Math.Pow(Math.Sin(a), 2);
        }
    }
}
