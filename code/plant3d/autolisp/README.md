# AutoLISP tools (AutoCAD, Plant 3D, every toolset)

| File | Commands |
|---|---|
| `att_tools.lsp` | `ATTLIST`, `ATTSET`, `ATTRENUM`, function `(att:set-all block tag value)` for scripts |
| `plant_helpers.lsp` | `P3DPROPS` (generic info + pointer to .NET), `P3DLAYERS`, `P3DPURGE` |

Load: `APPLOAD` → file, or add to the *Startup Suite*, or `(load "C:/CAD-Automation/code/plant3d/autolisp/att_tools.lsp")`.
With `SECURELOAD = 1` the folder must be in `TRUSTEDPATHS` (*Options → Files → Trusted Locations*).

Honest note: Plant 3D exposes almost nothing Plant-specific to LISP. LISP is excellent for **drawing**
work (attributes, layers, title blocks, purge) on Plant 3D ortho/iso/P&ID DWGs; for Plant **data** use .NET.

Checked with `tools/lisp_check.py` (balanced parens, strings, comments, duplicate commands, ends with `(princ)`). Not executed in AutoCAD on the build box.
