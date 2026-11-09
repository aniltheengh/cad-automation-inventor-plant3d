using System.Globalization;
using CadAutomation.Gears;

var cases = new[] { (2.0, 40, 20.0, 0.0), (1.5, 17, 20.0, 0.0), (3.0, 12, 20.0, 0.3), (5.0, 60, 25.0, 0.0) };
var inv = CultureInfo.InvariantCulture;
Console.WriteLine("[");
for (int i = 0; i < cases.Length; i++)
{
    var (m, z, a, x) = cases[i];
    var g = GearMath.Compute(new GearSpec(m, z, a, 20, 0, x));
    var fl = GearMath.UpperFlank(g, 5);
    string pts = string.Join(",", fl.Select(p => $"[{p.X.ToString("R", inv)},{p.Y.ToString("R", inv)}]"));
    Console.WriteLine($"{{\"m\":{m.ToString(inv)},\"z\":{z},\"alpha\":{a.ToString(inv)},\"x\":{x.ToString(inv)}," +
        $"\"rp\":{g.PitchRadius.ToString("R", inv)},\"rb\":{g.BaseRadius.ToString("R", inv)},\"ra\":{g.TipRadius.ToString("R", inv)}," +
        $"\"rf\":{g.RootRadius.ToString("R", inv)},\"tip\":{GearMath.ThicknessAt(g, g.TipRadius).ToString("R", inv)}," +
        $"\"flank\":[{pts}]}}{(i < cases.Length - 1 ? "," : "")}");
}
Console.WriteLine("]");
