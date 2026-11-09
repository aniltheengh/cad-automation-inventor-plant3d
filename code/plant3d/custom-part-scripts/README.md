# Plant 3D custom part script — nozzle stub (Python inside Plant 3D)

Plant 3D builds many catalogue parts from **Python geometry scripts** (the stock ones live in the
"CPak Common" content). You can add your own for special parts (strainers, spectacle blinds, nozzle stubs).

`CPNOZZLE_STUB.py` = hollow cylinder with two ports, parameters D (OD), L (length), T (wall).

```python
from aqa.math import *
from varmain.primitiv import *
from varmain.custom import *

@activate(Group="Pipe", TooltipShort="Stub nozzle", TooltipLong="Straight hollow stub, two ports",
          LengthUnit="mm", Ports=2)
@group("MainDimensions")
@param(D=LENGTH, TooltipShort="Outside diameter")
@param(L=LENGTH, TooltipLong="Length")
@param(T=LENGTH, TooltipLong="Wall thickness")
def CPNOZZLE_STUB(s, D=114.3, L=150.0, T=6.0, **kw):
    body = CYLINDER(s, R=D / 2.0, H=L, O=D / 2.0 - T).rotateY(90.0)   # O = inner radius
    s.setPoint((0.0, 0.0, 0.0), (-1.0, 0.0, 0.0))
    s.setPoint((L, 0.0, 0.0), (1.0, 0.0, 0.0))
```

## Install / test (VERIFY paths for your content location)
1. Copy the `.py` to the shared content folder `...\CPak Common\CustomScripts\`.
2. In Plant 3D: `PLANTREGISTERCUSTOMSCRIPTS` → creates/updates the script registry.
3. Restart Plant 3D. Quick test: `(arxload "PnP3dACPAdapter")` then `(testacpscript "CPNOZZLE_STUB" "D" "114.3" "L" "150" "T" "6")` → the part is drawn at 0,0,0.
4. Spec Editor → Catalog Builder → new part using the script.

Every call above (`CYLINDER(s, R, H, O)`, `rotateY`, `setPoint(pos, dir)`, decorators) was checked against
the **stock Autodesk scripts** (e.g. `CSGD001` uses `CYLINDER(s, R=D/2, H=L+IL, O=DI/2)`). Autodesk's
official documentation of this API is thin — see [docs/11](../../../docs/11-plant3d-sdk.md#custom-part-scripts-python-inside-plant-3d).
