# CPNOZZLE_STUB.py - Plant 3D custom part script (content authoring), simple stub-end nozzle.
# --------------------------------------------------------------------------------------------
# This is NOT CPython run by you: Plant 3D's embedded Python engine executes it when the part
# is built from the spec/catalog. Workflow (Autodesk DevBlog "Custom Python Scripts for AutoCAD
# Plant 3D" parts 1-4, David Wolfe):
#   1. put this file in   <Shared Content>\CPak Common\CustomScripts\   (create the folder)
#   2. in Plant 3D:  PLANTREGISTERCUSTOMSCRIPTS   (compiles + registers -> ScriptGroup.xml / .pyc)
#   3. restart Plant 3D, test from the command line:
#        (arxload "PnP3dACPAdapter")
#        (testacpscript "CPNOZZLE_STUB" "D" "114.3" "L" "150" "T" "6")
#   4. Spec Editor / Catalog Builder: create a component using this script.
# The function name MUST equal the file name. Helper functions go ABOVE it.
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
    # body: hollow cylinder. CYLINDER's O = inner radius (0 = solid) - pattern used by Autodesk's
    # own scripts (e.g. CSGD001 dummy leg). Primitives are built along Z; rotateY(90) lays it on X.
    body = CYLINDER(s, R=D / 2.0, H=L, O=D / 2.0 - T).rotateY(90.0)
    # ports: position + direction vector (port 1 at origin pointing -X, port 2 at L pointing +X)
    s.setPoint((0.0, 0.0, 0.0), (-1.0, 0.0, 0.0))
    s.setPoint((L, 0.0, 0.0), (1.0, 0.0, 0.0))
