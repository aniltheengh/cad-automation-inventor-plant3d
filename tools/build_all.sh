#!/usr/bin/env bash
# build_all.sh - compile-check every project WITHOUT Inventor/AutoCAD installed (Linux/macOS/CI).
#   INVENTOR_INTEROP_DIR=/path/with/Autodesk.Inventor.Interop.dll ./tools/build_all.sh
# Inventor projects need the interop DLL (copy it from an Inventor PC: ...\Inventor 20xx\Bin\).
# AutoCAD refs come from NuGet; Plant 3D refs are replaced by tools/plant-sdk-stubs (syntax only).
set -u
cd "$(dirname "$0")/.."
INV="${INVENTOR_INTEROP_DIR:?set INVENTOR_INTEROP_DIR}"
fail=0
build() { echo "== $1"; if dotnet build "$1" -nologo -v q -p:InventorBin="$INV" -p:SkipDeploy=true $2 2>&1 | grep -E " error |Build succeeded" | sort -u; then :; fi
          dotnet build "$1" -nologo -v q -p:InventorBin="$INV" -p:SkipDeploy=true $2 >/dev/null 2>&1 || fail=1; }
for p in code/inventor/csharp/0*/ code/inventor/vbnet/*/ code/inventor/design-automation/*/ code/autocad/csharp/*/; do build "$p" ""; done
build code/plant3d/csharp/Plant3DDataTools/ "-p:PlantStubs=true"
python3 tools/check_ilogic.py --inventor-bin "$INV" || fail=1
python3 -m py_compile $(find code tools -name "*.py" -not -path "*/obj/*") && echo "py_compile OK" || fail=1
python3 tools/lisp_check.py || fail=1
python3 tools/test_tools.py >/dev/null && echo "test_tools OK" || { echo "test_tools FAILED"; fail=1; }
command -v pwsh >/dev/null && { pwsh -NoProfile -File tools/check_powershell.ps1 || fail=1; }
python3 tools/check_links.py || fail=1
exit $fail
