"""drive_gear_template.py - open a parametric gear TEMPLATE part, set m/z/b, update, save a copy.

    python drive_gear_template.py GearTemplate.ipt 2 40 --b 20 --out C:/Work/Gears

The template needs user parameters m (mm), z (ul), b (mm) and an iLogic rule (01_SpurGear_Params)
or equations that rebuild the geometry. This is the "Python as a driver" pattern: Python does the
data work (tables, ERP, optimisation), Inventor does the geometry.
"""
import argparse
import pathlib

from gear_math import Gear, min_teeth_no_undercut
from inv_connect import connect, cast


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("template")
    ap.add_argument("m", type=float)
    ap.add_argument("z", type=int)
    ap.add_argument("--b", type=float, default=20.0)
    ap.add_argument("--out", default=".")
    a = ap.parse_args()
    g = Gear(a.m, a.z)
    if a.z < min_teeth_no_undercut():
        print("warning: undercut expected (z < 17 for 20 deg)")
    inv = connect()
    doc = cast(inv.Documents.Open(str(pathlib.Path(a.template).resolve()), True))
    up = doc.ComponentDefinition.Parameters.UserParameters
    up.Item("m").Expression = f"{a.m} mm"
    up.Item("z").Expression = str(a.z)
    up.Item("b").Expression = f"{a.b} mm"
    doc.Update()
    pn = f"GEAR-M{a.m:g}-Z{a.z}"
    doc.PropertySets.Item("Design Tracking Properties").Item("Part Number").Value = pn
    out = pathlib.Path(a.out).resolve() / f"{pn}.ipt"
    out.parent.mkdir(parents=True, exist_ok=True)
    doc.SaveAs(str(out), True)      # True = save a COPY, the template stays open & unchanged on disk
    doc.Close(True)
    print(f"{out}  (d={2 * g.rp:.2f}, da={2 * g.ra:.2f} mm)")


if __name__ == "__main__":
    main()
