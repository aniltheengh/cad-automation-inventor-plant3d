# Python for AutoCAD / Plant 3D

| Script | Needs | Does |
|---|---|---|
| `pyautocad_attributes.py` | AutoCAD/Plant 3D running, `pip install pyautocad pywin32` | export attributes to CSV, set a tag on every insert of a block |
| `csv_to_excel_report.py` | only `openpyxl` | merge the CSVs from `P3DLINELIST`, `P3DMTO`, `ATTEXPORT` into one formatted xlsx (one sheet per CSV, bold header, filter, frozen row, widths) |

```bat
python csv_to_excel_report.py "D:\Project\exports\*.csv" -o PlantReport.xlsx
```

Python reaches AutoCAD only through **COM/ActiveX** — generic entities, not Plant data (pipes appear as
custom objects with few properties). The Plant data path is: .NET command writes CSV → Python makes the report.
Also see `custom-part-scripts/` for the *other* Python in Plant 3D: custom part geometry scripts, which run *inside* Plant 3D.
