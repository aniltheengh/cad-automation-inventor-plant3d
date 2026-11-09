"""bom_to_excel.py - structured BOM of the active assembly -> .xlsx (openpyxl, no Excel needed).

    python bom_to_excel.py [out.xlsx] [--parts]
"""
import argparse
import pathlib

from openpyxl import Workbook
from openpyxl.styles import Font

from inv_connect import connect, cast, const


def walk(rows, level, out):
    for i in range(1, rows.Count + 1):            # COM collections are 1-based
        row = rows.Item(i)
        cd = row.ComponentDefinitions.Item(1)
        doc = cast(cd.Document)
        dtp = doc.PropertySets.Item("Design Tracking Properties")
        mat = ""
        if int(doc.DocumentType) == const("kPartDocumentObject"):
            mat = doc.ActiveMaterial.DisplayName
        out.append([level, row.ItemNumber, "   " * level + str(dtp.Item("Part Number").Value),
                    str(dtp.Item("Description").Value), row.TotalQuantity, mat,
                    round(cd.MassProperties.Mass, 3), doc.FullFileName])
        children = row.ChildRows
        if children is not None and children.Count > 0:
            walk(children, level + 1, out)


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("out", nargs="?")
    ap.add_argument("--parts", action="store_true")
    a = ap.parse_args()
    inv = connect(start=False)
    asm = cast(inv.ActiveDocument)
    if int(asm.DocumentType) != const("kAssemblyDocumentObject"):
        raise SystemExit("Activate an assembly")
    bom = asm.ComponentDefinition.BOM
    want = const("kPartsOnlyBOMViewType") if a.parts else const("kStructuredBOMViewType")
    if a.parts:
        bom.PartsOnlyViewEnabled = True
    else:
        bom.StructuredViewEnabled = True
        bom.StructuredViewFirstLevelOnly = False
    view = next(v for v in bom.BOMViews if v.ViewType == want)
    lines = []
    walk(view.BOMRows, 0, lines)
    wb = Workbook()
    ws = wb.active
    ws.title = "BOM"
    ws.append(["Level", "Item", "Part Number", "Description", "Qty", "Material", "Mass each (kg)", "File"])
    for c in ws[1]:
        c.font = Font(bold=True)
    for line in lines:
        ws.append(line)
    out = a.out or str(pathlib.Path(asm.FullFileName).with_suffix(".bom.xlsx"))
    wb.save(out)
    print(len(lines), "lines ->", out)


if __name__ == "__main__":
    main()
