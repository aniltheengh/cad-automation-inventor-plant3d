"""list_params.py - print / set parameters of the active Inventor part or assembly.

    python list_params.py                       # list
    python list_params.py --set Length=150mm --set Width="Length/2"
"""
import argparse

from inv_connect import connect, cast


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("--set", action="append", default=[], metavar="NAME=EXPR")
    a = ap.parse_args()
    inv = connect(start=False)
    doc = cast(inv.ActiveDocument)
    params = doc.ComponentDefinition.Parameters
    for p in params:
        # p.Value is in database units (cm, rad) - Expression is what the user typed
        print(f"{p.Name:<20} {p.Expression:<25} value(db)={p.Value}")
    for item in a.set:
        name, expr = item.split("=", 1)
        params.Item(name).Expression = expr
        print("set", name, "=", expr)
    if a.set:
        doc.Update()


if __name__ == "__main__":
    main()
