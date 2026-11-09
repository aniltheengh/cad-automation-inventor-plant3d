"""csv_to_excel_report.py - turn the CSVs from P3DLINELIST / P3DMTO / PIDTAGS into one formatted workbook.

    python csv_to_excel_report.py LineList_*.csv MTO_*.csv -o Plant_Report.xlsx

Pure Python (openpyxl) - runs anywhere, no AutoCAD needed. One sheet per CSV, header bold,
filters on, columns auto-sized, frozen header row.
"""
import argparse
import csv
import glob
import pathlib

from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill
from openpyxl.utils import get_column_letter


def add_sheet(wb, csv_path: pathlib.Path) -> int:
    with open(csv_path, newline="", encoding="utf-8-sig") as fh:
        rows = list(csv.reader(fh))
    ws = wb.create_sheet(csv_path.stem[:31])          # Excel sheet name limit = 31 chars
    for r in rows:
        ws.append([_num(v) for v in r])
    if not rows:
        return 0
    head_fill = PatternFill("solid", fgColor="DDEBF7")
    for c in ws[1]:
        c.font = Font(bold=True)
        c.fill = head_fill
    ws.auto_filter.ref = ws.dimensions
    ws.freeze_panes = "A2"
    for i, col in enumerate(zip(*rows), start=1):
        ws.column_dimensions[get_column_letter(i)].width = min(60, max(8, max(len(str(v)) for v in col) + 2))
    return len(rows) - 1


def _num(v: str):
    try:
        return int(v)
    except ValueError:
        try:
            return float(v)
        except ValueError:
            return v


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("csv", nargs="+")
    ap.add_argument("-o", "--out", default="Plant_Report.xlsx")
    a = ap.parse_args()
    wb = Workbook()
    wb.remove(wb.active)
    files = [pathlib.Path(p) for pat in a.csv for p in glob.glob(pat)]
    for f in files:
        print(f"{f.name}: {add_sheet(wb, f)} rows")
    wb.save(a.out)
    print("->", a.out)


if __name__ == "__main__":
    main()
