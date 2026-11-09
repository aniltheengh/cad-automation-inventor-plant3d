# Python (pywin32) scripts for Inventor

```bat
conda create -n cad python=3.12 pywin32 openpyxl -y
conda activate cad
python -c "import win32com.client"        & rem test
cd code\inventor\python
python list_params.py
```

| Script | Does |
|---|---|
| `inv_connect.py` | `connect()` (attach or start), `cast()` to the derived interface, `const()` for enum values via `gencache.EnsureDispatch` |
| `list_params.py` | list / set parameters of the active document |
| `batch_export.py` | folder → PDF / DXF / STEP (same GUIDs as C#) |
| `iprops_excel.py` | iProperties ⇄ Excel (openpyxl) |
| `bom_to_excel.py` | structured BOM → xlsx |
| `gear_math.py` | pure involute math (no Inventor) — `python gear_math.py 2 40`; tested against the C# version |
| `drive_gear_template.py` | open a template part, set `m`, `z`, `b`, rebuild, save a copy |

## Limits (honest)
- Out-of-process COM: every property access is a cross-process call → **10–100× slower** than an add-in for loops over thousands of objects. Fine for batch jobs over files, bad for touching every edge of a model.
- No events, no ribbon buttons, no in-process UI. Use an add-in for that.
- `gencache` writes wrappers to `%LOCALAPPDATA%\Temp\gen_py`; after an Inventor upgrade, delete that folder if you see `AttributeError` on members that exist.
- Anaconda: install `pywin32` with conda (not pip) inside the env, or run `python Scripts/pywin32_postinstall.py -install` if `import win32com` fails.
- All files pass `py_compile`; `gear_math.py` is unit-tested. COM scripts were not executed (no Inventor on the build box).
