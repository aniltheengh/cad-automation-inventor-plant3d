# iLogic rules (10) — VB.NET inside Inventor

| # | Rule | Type | Lives in | Teaches |
|---|---|---|---|---|
| 01 | `01_SpurGear_Params` | internal | gear template part | user-parameter validation, involute math in VB, `Parameter("x") =`, `iLogicVb.UpdateWhenDone` |
| 02 | `02_Tank_Configurator` | internal | tank part / skeleton | ASME VIII-1 UG-27 shell + UG-32 head thickness, feature suppression by head type, capacity, `MultiValue.SetList` |
| 03 | `03_Shaft_Generator` | run once | new empty part | raw API from iLogic: sketch, profile, revolve, chamfer — cm units |
| 04 | `04_Frame_Assembly_Driver` | internal | template assembly | push parameters to parts, `Component.IsActive`, configurator + iLogic Form |
| 05 | `05_Batch_Export_PDF_DXF` | **external** | shared folder | translator add-ins from iLogic, folder browse, active-doc vs. folder mode |
| 06 | `06_Excel_Size_Table` | internal | flange part | `GoExcel.FindRow` / `CurrentRowValue`, embedded workbook |
| 07 | `07_iProps_Description_Builder` | external + trigger | shared folder | Before Save trigger, sheet-metal extents → `PL 10 x 200 x 350, SS400` |
| 08 | `08_Drawing_Auto_Create` | external | shared folder | drawing views, retrieve dimensions, parts list, auto scale |
| 09 | `09_BOM_To_Excel` | external | shared folder | `BOMView.Export` |
| 10 | `10_Event_Rules_Examples` | trigger rules | template | material check, part-number = file name, save stamp |

## How to use a rule file
- **Internal rule:** *Manage → iLogic → Add Rule* → paste the file content → **Save & Run**.
- **External rule:** put the file in a folder, *iLogic Configuration → External Rule Directories* → add the folder → the rule appears under *iLogic Browser → External Rules*. Extension `.iLogicVb` (or `.vb` / `.txt`).
- **Trigger:** *Manage → Event Triggers* → drag the rule onto e.g. *Before Save Document*.
- **Form:** *iLogic Browser → Forms → Add Form* → drag parameters in → a "configurator" dialog with no code.

## Checked how?
`python tools/check_ilogic.py --inventor-bin <dir with Autodesk.Inventor.Interop.dll>` wraps each rule in a
class that inherits **stubs** of the iLogic objects (`tools/ilogic-check/ILogicStubs.vb`) and compiles it
with the real Inventor interop — twice: Option Strict Off (like iLogic) and Strict On to list late-bound
calls (where a misspelt member would only fail at run time). Result: 10 rules, 0 errors, 0 late-bound sites.
The stubs are *our* approximations of iLogic signatures — passing the check proves the Inventor API calls
and VB syntax, not every iLogic snippet signature. Always run the rule once in Inventor.
