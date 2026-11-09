"""iprops_excel.py - export iProperties of all Inventor files in a folder to Excel, edit, re-import.

    python iprops_excel.py export C:/Work/Project props.xlsx --recurse
    python iprops_excel.py import props.xlsx [--dry-run]

Mirror of code/inventor/csharp/04-IPropertyBulkEditor. Column A = full path. Unknown headers = custom iProperties.
"""
import argparse
import pathlib

from openpyxl import Workbook, load_workbook
from openpyxl.styles import Font, PatternFill

from inv_connect import connect

SETS = {
    "Part Number": ("Design Tracking Properties", "Part Number"),
    "Description": ("Design Tracking Properties", "Description"),
    "Stock Number": ("Design Tracking Properties", "Stock Number"),
    "Vendor": ("Design Tracking Properties", "Vendor"),
    "Designer": ("Design Tracking Properties", "Designer"),
    "Project": ("Design Tracking Properties", "Project"),
    "Material": ("Design Tracking Properties", "Material"),
    "Title": ("Inventor Summary Information", "Title"),
    "Revision Number": ("Inventor Summary Information", "Revision Number"),
    "Author": ("Inventor Summary Information", "Author"),
}
CUSTOM = "Inventor User Defined Properties"
DEFAULT_COLS = ["Part Number", "Description", "Revision Number", "Stock Number", "Vendor", "Title", "Material"]


def get_prop(doc, col: str) -> str:
    try:
        s, n = SETS.get(col, (CUSTOM, col))
        return str(doc.PropertySets.Item(s).Item(n).Value)
    except Exception:
        return ""


def set_prop(doc, col: str, value: str) -> bool:
    if col == "Material" or get_prop(doc, col) == value:
        return False
    s, n = SETS.get(col, (CUSTOM, col))
    ps = doc.PropertySets.Item(s)
    try:
        ps.Item(n).Value = value
    except Exception:
        ps.Add(value, n)          # PropertySet.Add(PropValue, Name) - creates a custom iProperty
    return True


def export(folder: str, xlsx: str, recurse: bool) -> None:
    files = [p for p in pathlib.Path(folder).glob("**/*" if recurse else "*")
             if p.suffix.lower() in (".ipt", ".iam", ".idw") and "OldVersions" not in p.parts]
    inv = connect(visible=False)
    inv.SilentOperation = True
    rows, custom_cols = [], set()
    try:
        for f in files:
            doc = inv.Documents.Open(str(f), False)
            try:
                vals = {c: get_prop(doc, c) for c in DEFAULT_COLS}
                for p in doc.PropertySets.Item(CUSTOM):
                    custom_cols.add(p.Name)
                    vals[p.Name] = str(p.Value)
                rows.append((str(f), vals))
                print("read", f.name)
            finally:
                doc.Close(True)
    finally:
        inv.SilentOperation = False
    cols = ["File", *DEFAULT_COLS, *sorted(custom_cols)]
    wb = Workbook()
    ws = wb.active
    ws.title = "iProperties"
    ws.append(cols)
    for f, vals in rows:
        ws.append([f, *[vals.get(c, "") for c in cols[1:]]])
    for c in ws[1]:
        c.font = Font(bold=True)
    yellow = PatternFill("solid", fgColor="FFFFE0")
    for row in ws.iter_rows(min_row=2, min_col=2):
        for c in row:
            c.fill = yellow
    ws.freeze_panes = "B2"
    wb.save(xlsx)
    print(len(rows), "files ->", xlsx)


def do_import(xlsx: str, dry: bool) -> None:
    ws = load_workbook(xlsx).active
    header = [c.value for c in ws[1]]
    inv = connect(visible=False)
    inv.SilentOperation = True
    saved = 0
    try:
        for row in ws.iter_rows(min_row=2, values_only=True):
            f = row[0]
            if not f or not pathlib.Path(f).exists():
                continue
            doc = inv.Documents.Open(f, False)
            try:
                changes = []
                for col, val in zip(header[1:], row[1:]):
                    val = "" if val is None else str(val)
                    old = get_prop(doc, col)
                    if old != val and (val or col in SETS):
                        changes.append(f"{col}: {old!r} -> {val!r}")
                        if not dry:
                            set_prop(doc, col, val)
                if changes:
                    print(pathlib.Path(f).name, *changes, sep="\n   ")
                    if not dry:
                        doc.Save()
                        saved += 1
            finally:
                doc.Close(True)
    finally:
        inv.SilentOperation = False
    print(saved, "files saved", "(dry run)" if dry else "")


def main() -> None:
    ap = argparse.ArgumentParser()
    sub = ap.add_subparsers(dest="cmd", required=True)
    e = sub.add_parser("export"); e.add_argument("folder"); e.add_argument("xlsx"); e.add_argument("--recurse", action="store_true")
    i = sub.add_parser("import"); i.add_argument("xlsx"); i.add_argument("--dry-run", action="store_true")
    a = ap.parse_args()
    export(a.folder, a.xlsx, a.recurse) if a.cmd == "export" else do_import(a.xlsx, a.dry_run)


if __name__ == "__main__":
    main()
