#!/usr/bin/env python3
"""Compile-check iLogic rules: wrap each .iLogicVb in a VB class that inherits the RuleBase stub,
then `dotnet build` tools/ilogic-check against Autodesk.Inventor.Interop.dll.

    python tools/check_ilogic.py --inventor-bin "C:/Program Files/Autodesk/Inventor 2026/Bin"

What it proves : VB syntax, types, and every Inventor API member used (real interop).
What it doesn't: iLogic-only members are hand stubs (tools/ilogic-check/ILogicStubs.vb), and nothing runs.
"""
import argparse, pathlib, re, shutil, subprocess, sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
RULES = ROOT / "code" / "inventor" / "ilogic"
PROJ = ROOT / "tools" / "ilogic-check"


def wrap(rule: pathlib.Path) -> str:
    lines = rule.read_text(encoding="utf-8").splitlines()
    imports = [l for l in lines if re.match(r"\s*Imports\s", l)]
    body = [l for l in lines if not re.match(r"\s*(Imports|AddReference)\s", l)]
    cls = "Rule_" + re.sub(r"\W", "_", rule.stem)
    return "\n".join(imports + [f"Public Class {cls}", "    Inherits RuleBase"] + body + ["End Class", ""])


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--inventor-bin", required=True)
    a = ap.parse_args()
    gen = PROJ / "_generated"
    shutil.rmtree(gen, ignore_errors=True)
    gen.mkdir()
    rules = sorted(RULES.glob("*.iLogicVb"))
    for r in rules:
        (gen / (r.stem + ".vb")).write_text(wrap(r), encoding="utf-8")
    def build(strict: str):
        p = subprocess.run(["dotnet", "build", str(PROJ), "-nologo", "-v", "q", "--no-incremental",
                            f"-p:InventorBin={a.inventor_bin}", f"-p:OptionStrict={strict}"],
                           capture_output=True, text=True)
        out = p.stdout + p.stderr
        return p.returncode, sorted({l.strip().replace(str(gen) + "/", "").replace(str(gen) + "\\", "")
                                     for l in out.splitlines() if ": error " in l})

    # Pass 1 - iLogic's real setting (Option Strict Off): syntax, types, Inventor API names on typed objects
    rc, errs = build("Off")
    for e in errs:
        print("ERROR", e)
    # Pass 2 - Option Strict On, keep only BC30574 "late binding". VB treats Inventor's COM interfaces as
    # late-bindable, so with Strict Off a MISSPELT API member compiles and only fails when the rule runs.
    # Every late-bound call is listed so a human can confirm it is intentional (e.g. calls on Object).
    _, strict_errs = build("On")
    late = [e for e in strict_errs if "BC30574" in e]
    for e in late:
        print("LATE-BOUND (review)", e)
    print(f"{len(rules)} rules, {len(errs)} errors, {len(late)} late-bound call sites to review")
    return 0 if rc == 0 and not errs else 1


if __name__ == "__main__":
    sys.exit(main())
