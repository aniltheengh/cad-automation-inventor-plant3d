#!/usr/bin/env python3
"""test_tools.py - offline tests for everything that can be tested without Inventor / AutoCAD.

    python tools/test_tools.py          (from the repo root; dotnet optional for the C# cross-check)
"""
import json
import math
import pathlib
import shutil
import subprocess
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / "code" / "inventor" / "python"))
sys.path.insert(0, str(ROOT / "tools"))

import gear_math as gm  # noqa: E402
import lisp_check  # noqa: E402

passed = failed = 0


def check(name: str, cond: bool, detail: str = "") -> None:
    global passed, failed
    if cond:
        passed += 1
        print("PASS", name)
    else:
        failed += 1
        print("FAIL", name, detail)


def close(a, b, tol=1e-9):
    return abs(a - b) <= tol * max(1.0, abs(a), abs(b))


# --- gear math: textbook values -------------------------------------------------
g = gm.Gear(2, 40)
check("m2 z40 pitch d = 80", close(2 * g.rp, 80))
check("m2 z40 tip da = 84", close(2 * g.ra, 84))
check("m2 z40 root df = 75", close(2 * g.rf, 75))
check("m2 z40 base db = 80 cos20 = 75.175", close(2 * g.rb, 80 * math.cos(math.radians(20))) and abs(2 * g.rb - 75.175) < 5e-4)
check("tooth thickness at pitch = pi m / 2 (x=0)", close(g.thickness_at(g.rp), math.pi * 2 / 2))
check("z_min(20 deg) = 17.10", abs(gm.min_teeth_no_undercut(20) - 17.097) < 1e-3)
check("z_min(25 deg) = 11.20", abs(gm.min_teeth_no_undercut(25) - 11.198) < 1e-3)
gs = gm.Gear(3, 12, 20, 0.3)
check("profile shift +0.3 thickens tooth at pitch by 2 x m tan(a)",
      close(gs.thickness_at(gs.rp), math.pi * 3 / 2 + 2 * 0.3 * 3 * math.tan(math.radians(20))))
# involute property: tooth thickness formula s2 = 2 r2 (s1/(2 r1) + inv a1 - inv a2)
r2 = (g.rb + g.ra) / 2
a2 = math.acos(g.rb / r2)
s2 = 2 * r2 * (math.pi * 2 / 2 / (2 * g.rp) + gm.inv(math.radians(20)) - gm.inv(a2))
check("thickness at arbitrary radius matches standard formula", close(g.thickness_at(r2), s2))
flank = g.upper_flank(12)
check("flank starts on max(rb, rf) and ends on ra",
      close(math.hypot(*flank[0]), max(g.rb, g.rf)) and close(math.hypot(*flank[-1]), g.ra))
check("small pinion z=6 m=1 x=0.6 becomes pointed", gm.Gear(1, 6, 20, 0.6).is_pointed())
# tooth-space sketch used by 02-SpurGearGenerator must be a valid (non-crossing) loop:
# flank A (tooth 0) must stay below the space centre angle pi/z at every radius it spans.
for (m_, z_, a_, x_) in [(2, 40, 20, 0), (1.5, 17, 20, 0), (3, 12, 20, 0.3), (1, 100, 20, 0), (5, 60, 25, 0)]:
    gg = gm.Gear(m_, z_, a_, x_)
    r0 = max(gg.rb, gg.rf)
    ok = all(0 < gg.flank_angle(r0 + (gg.ra - r0) * k / 20) < math.pi / z_ for k in range(21))
    check(f"tooth-space loop valid m={m_} z={z_}", ok)
check("center distance m(z1+z2)/2", close(gm.center_distance(2, 20, 40), 60))

# --- C# GearMath gives identical numbers ----------------------------------------
dotnet = shutil.which("dotnet")
if dotnet:
    p = subprocess.run([dotnet, "run", "--project", str(ROOT / "tools" / "gearmath-check"), "-v", "q"],
                       capture_output=True, text=True)
    try:
        data = json.loads(p.stdout[p.stdout.index("["):])
    except ValueError:
        data = []
        check("C# GearMath ran", False, p.stdout[-500:] + p.stderr[-500:])
    for c in data:
        py = gm.Gear(c["m"], c["z"], c["alpha"], c["x"])
        ok = all(close(getattr(py, k), c[k], 1e-12) for k in ("rp", "rb", "ra", "rf"))
        ok &= close(py.thickness_at(py.ra), c["tip"], 1e-12)
        ok &= all(close(a, b, 1e-12) for pt_py, pt_cs in zip(py.upper_flank(5), c["flank"]) for a, b in zip(pt_py, pt_cs))
        check(f"C# == Python geometry m={c['m']} z={c['z']} a={c['alpha']} x={c['x']}", ok)
else:
    print("SKIP C# cross-check (dotnet not found)")

# --- AutoLISP static check -------------------------------------------------------
for f in sorted((ROOT / "code").rglob("*.lsp")):
    e = lisp_check.check(f)
    check(f"lisp {f.name}", not e, "; ".join(e))
# a deliberately broken snippet must be caught
tmp = ROOT / "tools" / "_broken.lsp"
tmp.write_text('(defun c:X ( / a) (setq a "x)\n(princ)\n', encoding="utf-8")
check("lisp checker catches unterminated string", bool(lisp_check.check(tmp)))
tmp.unlink()

# --- files every project README promises exist ----------------------------------
for rel in ["code/inventor/csharp/07-MyFirstAddin/MyFirstAddin.addin",
            "code/inventor/csharp/07-MyFirstAddin/StandardAddInServer.cs",
            "code/plant3d/csharp/Plant3DDataTools/PipingCommands.cs",
            "code/plant3d/csharp/Plant3DAddinStarter/PlantAddin.cs",
            "code/plant3d/csharp/Plant3DAddinStarter/bundle/PackageContents.xml",
            "code/plant3d/csharp/Plant3DAddinStarter/Properties/launchSettings.json",
            "code/inventor/design-automation/GearDAPlugin/bundle/PackageContents.xml"]:
    check("exists " + rel, (ROOT / rel).exists())

# --- .addin GUID == [Guid] on the server class -----------------------------------
import re  # noqa: E402
addin = (ROOT / "code/inventor/csharp/07-MyFirstAddin/MyFirstAddin.addin").read_text()
srv = (ROOT / "code/inventor/csharp/07-MyFirstAddin/StandardAddInServer.cs").read_text()
gid = re.search(r"<ClassId>\{([0-9A-F-]+)\}</ClassId>", addin).group(1)
check("add-in ClassId matches [Guid] attribute", f'[Guid("{gid}")]' in srv and f"{{{gid}}}" in srv)

# --- Plant3DAddinStarter: commands in the code == commands in the bundle == commands in the docs --------
starter = ROOT / "code/plant3d/csharp/Plant3DAddinStarter"
in_code = set()
for cs in starter.glob("*.cs"):
    in_code |= set(re.findall(r'\[CommandMethod\("([A-Z0-9]+)"', cs.read_text(encoding="utf-8")))
bundle = (starter / "bundle/PackageContents.xml").read_text(encoding="utf-8")
in_bundle = set(re.findall(r'<Command Global="([A-Z0-9]+)"', bundle))
check("starter add-in has commands", len(in_code) >= 5, str(in_code))
check("bundle command list matches [CommandMethod] names", in_code == in_bundle, f"{sorted(in_code ^ in_bundle)}")
check("bundle is restricted to Plant 3D", 'Platform="Plant3D"' in bundle)
check("bundle module name matches the assembly name",
      "./Contents/Plant3DAddinStarter.dll" in bundle
      and "<AssemblyName>Plant3DAddinStarter</AssemblyName>" in (starter / "Plant3DAddinStarter.csproj").read_text(encoding="utf-8"))
for doc in ["docs/11c-plant3d-code-examples.md", "code/plant3d/csharp/Plant3DAddinStarter/README.md"]:
    text = (ROOT / doc).read_text(encoding="utf-8") if (ROOT / doc).exists() else ""
    missing = sorted(c for c in in_code if c not in text)
    check(f"every starter command is described in {doc}", not missing, str(missing))
# docs/11c quotes the starter: every line of a block marked <!-- excerpt: File.cs --> must exist in that file
doc11c = (ROOT / "docs/11c-plant3d-code-examples.md").read_text(encoding="utf-8")
excerpts = re.findall(r"<!-- excerpt: ([\w.]+) -->\n.*?```csharp\n(.*?)```", doc11c, flags=re.S)
stale = []
for file, block in excerpts:
    src = {l.strip() for l in (starter / file).read_text(encoding="utf-8").splitlines()}
    stale += [f"{file}: {l.strip()}" for l in block.splitlines() if l.strip() not in ("", "// ...") and l.strip() not in src]
check("docs/11c has code excerpts", len(excerpts) >= 10, str(len(excerpts)))
check("docs/11c excerpts match the starter source", not stale, "; ".join(stale[:3]))
json.loads((starter / "Properties/launchSettings.json").read_text(encoding="utf-8"))
check("launchSettings.json is valid JSON", True)

print(f"\n{passed} passed, {failed} failed")
sys.exit(1 if failed else 0)
