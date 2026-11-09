"""batch_export.py - export Inventor drawings to PDF/DXF and models to STEP (pywin32).

    python batch_export.py "C:/Work/Project" --pdf --dxf --step --recurse --out C:/Work/Export

Same translator GUIDs as the C# and iLogic versions (docs/09).
"""
import argparse
import csv
import pathlib
import time

from inv_connect import connect, const

PDF = "{0AC6FD96-2F4D-42CE-8BE0-8AEA580399E4}"
DXF = "{C24E3AC4-122E-11D5-8E91-0010B541CD80}"
STEP = "{90AF7F40-0C01-11D5-8E83-0010B541CD80}"


def set_option(opts, name: str, value) -> None:
    """NameValueMap.Value(name) = value. HasSaveCopyAsOptions pre-fills the map with defaults, so the
    key normally exists and must be overwritten; makepy exposes the parameterized property put as
    SetValue(). Fall back to Add() for keys that are not there yet."""
    try:
        opts.SetValue(name, value)
    except Exception:  # noqa: BLE001 - COM error type differs between late/early binding
        opts.Add(name, value)


def translate(inv, addin_id: str, doc, out_file: pathlib.Path, options: dict | None = None) -> None:
    addin = inv.ApplicationAddIns.ItemById(addin_id)
    ctx = inv.TransientObjects.CreateTranslationContext()
    ctx.Type = const("kFileBrowseIOMechanism")
    opts = inv.TransientObjects.CreateNameValueMap()
    dm = inv.TransientObjects.CreateDataMedium()
    if options and addin.HasSaveCopyAsOptions(doc, ctx, opts):
        for k, v in options.items():
            set_option(opts, k, v)
    dm.FileName = str(out_file)
    addin.SaveCopyAs(doc, ctx, opts, dm)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("folder")
    ap.add_argument("--out")
    ap.add_argument("--pdf", action="store_true")
    ap.add_argument("--dxf", action="store_true")
    ap.add_argument("--step", action="store_true")
    ap.add_argument("--recurse", action="store_true")
    a = ap.parse_args()
    if not (a.pdf or a.dxf or a.step):
        a.pdf = True
    src = pathlib.Path(a.folder).resolve()
    out = pathlib.Path(a.out) if a.out else src / "_export"
    out.mkdir(parents=True, exist_ok=True)
    pattern = "**/*" if a.recurse else "*"
    files = [p for p in src.glob(pattern) if p.suffix.lower() in (".idw", ".ipt", ".iam")
             and "OldVersions" not in p.parts]

    inv = connect(start=True, visible=False)
    inv.SilentOperation = True
    log_rows = []
    t0 = time.time()
    try:
        for f in files:
            is_dwg = f.suffix.lower() == ".idw"
            if is_dwg and not (a.pdf or a.dxf):
                continue
            if not is_dwg and not a.step:
                continue
            doc = inv.Documents.Open(str(f), False)
            try:
                jobs = []
                if is_dwg and a.pdf:
                    jobs.append((PDF, out / (f.stem + ".pdf"), {"All_Color_AS_Black": 0, "Sheet_Range": const("kPrintAllSheets")}))
                if is_dwg and a.dxf:
                    jobs.append((DXF, out / (f.stem + ".dxf"), None))
                if not is_dwg and a.step:
                    jobs.append((STEP, out / (f.stem + ".stp"), {"ApplicationProtocolType": 3}))
                for addin, target, opts in jobs:
                    try:
                        translate(inv, addin, doc, target, opts)
                        log_rows.append([str(f), target.name, "OK", ""])
                        print("OK  ", target.name)
                    except Exception as e:  # pywintypes.com_error
                        log_rows.append([str(f), target.name, "FAIL", str(e)])
                        print("FAIL", target.name, e)
            finally:
                doc.Close(True)
    finally:
        inv.SilentOperation = False
    with open(out / "export_log.csv", "w", newline="", encoding="utf-8") as fh:
        csv.writer(fh).writerows([["file", "output", "status", "message"], *log_rows])
    print(f"{len(log_rows)} exports in {time.time() - t0:.0f} s -> {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
