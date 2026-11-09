"""inv_connect.py - connect Python (pywin32) to Autodesk Inventor over COM.

    from inv_connect import connect, cast, const
    inv = connect()
    doc = cast(inv.ActiveDocument)           # -> PartDocument / AssemblyDocument / DrawingDocument

Key facts (pywin32 + Inventor):
* Inventor's COM objects come back typed as their *base* interface (e.g. Document). Call cast() to get
  PartDocument etc., otherwise .ComponentDefinition is missing.
* gencache.EnsureDispatch builds early-bound wrappers from Inventor's type library once (cached in
  %LOCALAPPDATA%\\Temp\\gen_py). That gives you win32com.client.constants (kPartDocumentObject ...).
  If Inventor is upgraded and things break, delete the gen_py folder.
* Inventor API units are cm and radians regardless of document units.
* Python runs OUT of process: each property access is a cross-process COM call (~0.1-1 ms). Avoid
  touching thousands of objects one by one; use Apprentice / iLogic / an add-in for heavy loops.
"""
from __future__ import annotations

import sys

try:
    import pythoncom
    import win32com.client
    from win32com.client import gencache
except ImportError:  # allows py_compile / docs builds on non-Windows machines
    win32com = None  # type: ignore

PROGID = "Inventor.Application"

_DOC_INTERFACES = {
    12290: "PartDocument",        # kPartDocumentObject
    12291: "AssemblyDocument",    # kAssemblyDocumentObject
    12292: "DrawingDocument",     # kDrawingDocumentObject
    12293: "PresentationDocument",
}


def connect(start: bool = True, visible: bool = True):
    """Return the running Inventor.Application (early bound), starting one if needed."""
    if win32com is None:
        raise RuntimeError("pywin32 is required (Windows only): pip install pywin32")
    try:
        app = win32com.client.GetActiveObject(PROGID)
        app = gencache.EnsureDispatch(app)  # upgrade to early binding (constants, typed members)
    except pythoncom.com_error:
        if not start:
            raise RuntimeError("Inventor is not running")
        app = gencache.EnsureDispatch(PROGID)
        app.Visible = visible
    return app


def cast(doc):
    """Cast a generic Document to its specific interface so its members are visible."""
    name = _DOC_INTERFACES.get(int(doc.DocumentType))
    return win32com.client.CastTo(doc, name) if name else doc


def const(name: str):
    """Inventor enum constant by name, e.g. const('kPartDocumentObject')."""
    return getattr(win32com.client.constants, name)


def cm_to_mm(v: float) -> float:
    return v * 10.0


def mm_to_cm(v: float) -> float:
    return v / 10.0


if __name__ == "__main__":
    inv = connect(start=False)
    print("Inventor", inv.SoftwareVersion.DisplayVersion, "-", inv.Documents.Count, "open documents")
    if inv.ActiveDocument is not None:
        d = cast(inv.ActiveDocument)
        print("Active:", d.FullFileName, type(d).__name__)
    sys.exit(0)
