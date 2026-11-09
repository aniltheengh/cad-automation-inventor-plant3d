"""pyautocad_attributes.py - read/write block attributes in the ACTIVE AutoCAD / Plant 3D drawing over COM.

    pip install pyautocad pywin32
    python pyautocad_attributes.py export out.csv
    python pyautocad_attributes.py set TITLEBLOCK REV B

Notes:
* pyautocad wraps comtypes; it attaches to the running "AutoCAD.Application" (Plant 3D is AutoCAD).
* COM from outside AutoCAD is SLOW (each call crosses processes) - fine for hundreds of blocks,
  use .NET or LISP for tens of thousands.
* Plant 3D smart objects (pipes, P&ID symbols) are NOT accessible as Plant data via COM/pyautocad -
  only as generic AutoCAD entities. Use the .NET Plant SDK (code/plant3d/csharp) for Plant data.
"""
import csv
import sys

try:
    from pyautocad import Autocad
except ImportError:
    Autocad = None


def effective_name(blk) -> str:
    try:
        return blk.EffectiveName
    except Exception:
        return blk.Name


def iter_attribute_blocks(acad):
    for obj in acad.iter_objects("BlockReference"):     # model space of the active document
        if obj.HasAttributes:
            yield obj


def export(path: str) -> None:
    acad = Autocad(create_if_not_exists=False)
    n = 0
    with open(path, "w", newline="", encoding="utf-8-sig") as fh:   # BOM -> Excel shows Japanese fine
        w = csv.writer(fh)
        w.writerow(["Handle", "Block", "Tag", "Value"])
        for blk in iter_attribute_blocks(acad):
            for att in blk.GetAttributes():
                w.writerow([att.Handle, effective_name(blk), att.TagString, att.TextString])
                n += 1
    print(n, "attributes ->", path, "from", acad.doc.Name)


def set_all(block: str, tag: str, value: str) -> None:
    acad = Autocad(create_if_not_exists=False)
    n = 0
    for blk in iter_attribute_blocks(acad):
        if effective_name(blk).upper() != block.upper():
            continue
        for att in blk.GetAttributes():
            if att.TagString.upper() == tag.upper():
                att.TextString = value
                n += 1
    acad.doc.Regen(1)   # acAllViewports
    print(n, "attributes set")


if __name__ == "__main__":
    if Autocad is None:
        sys.exit("pip install pyautocad")
    if len(sys.argv) >= 3 and sys.argv[1] == "export":
        export(sys.argv[2])
    elif len(sys.argv) == 5 and sys.argv[1] == "set":
        set_all(*sys.argv[2:5])
    else:
        print(__doc__)
