#!/usr/bin/env python3
"""lisp_check.py - static sanity check for AutoLISP files (no AutoCAD needed).

Checks: balanced parentheses (string- and comment-aware), unterminated strings, duplicate
(defun c:NAME) commands, and that every file ends with (princ) for a quiet load.
    python tools/lisp_check.py code/plant3d/autolisp/*.lsp
"""
import pathlib
import re
import sys


def check(path: pathlib.Path) -> list[str]:
    src = path.read_text(encoding="utf-8")
    errs, depth, i, line, in_str = [], 0, 0, 1, False
    stack = []
    while i < len(src):
        ch = src[i]
        if ch == "\n":
            line += 1
        if in_str:
            if ch == "\\":
                i += 2
                continue
            if ch == '"':
                in_str = False
        elif ch == ";":
            if src.startswith(";|", i):                     # block comment ;| ... |;
                end = src.find("|;", i + 2)
                line += src.count("\n", i, end if end > 0 else len(src))
                i = end + 2 if end > 0 else len(src)
                continue
            while i < len(src) and src[i] != "\n":
                i += 1
            continue
        elif ch == '"':
            in_str = True
            str_line = line
        elif ch == "(":
            depth += 1
            stack.append(line)
        elif ch == ")":
            depth -= 1
            if depth < 0:
                errs.append(f"{path.name}:{line}: unexpected ')'")
                depth = 0
            else:
                stack.pop()
        i += 1
    if in_str:
        errs.append(f"{path.name}:{str_line}: unterminated string")
    if depth > 0:
        errs.append(f"{path.name}: {depth} unclosed '(' (first opened at line {stack[0]})")
    cmds = re.findall(r"\(defun\s+c:([\w-]+)", src, re.I)
    dups = {c for c in cmds if cmds.count(c) > 1}
    if dups:
        errs.append(f"{path.name}: duplicate commands {sorted(dups)}")
    if not re.search(r"\(princ\)\s*$", src.strip()):
        errs.append(f"{path.name}: should end with (princ)")
    return errs


def main(argv: list[str]) -> int:
    files = [pathlib.Path(a) for a in argv] or sorted(pathlib.Path(__file__).resolve().parents[1].glob("code/**/*.lsp"))
    all_errs = []
    for f in files:
        e = check(f)
        all_errs += e
        print(("OK   " if not e else "FAIL ") + str(f))
    for e in all_errs:
        print("   ", e)
    return 1 if all_errs else 0


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
