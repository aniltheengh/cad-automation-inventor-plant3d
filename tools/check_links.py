#!/usr/bin/env python3
"""check_links.py - verify Markdown links.

    python tools/check_links.py              # internal links + #anchors (offline)
    python tools/check_links.py --external   # also HTTP-check external URLs (slow, needs internet)
"""
import argparse
import pathlib
import re
import sys
import urllib.request

ROOT = pathlib.Path(__file__).resolve().parents[1]
LINK = re.compile(r"(?<!\!)\[[^\]]*\]\(([^)\s]+)(?:\s+\"[^\"]*\")?\)")


def anchors(md: pathlib.Path) -> set[str]:
    out = set()
    in_code = False
    for line in md.read_text(encoding="utf-8").splitlines():
        if line.startswith("```"):
            in_code = not in_code
        if in_code:
            continue
        m = re.match(r"#{1,6}\s+(.*)", line)
        if m:
            t = m.group(1).strip().lower()
            t = re.sub(r"[`*_~]", "", t)
            t = re.sub(r"[^\w\- ]", "", t, flags=re.UNICODE)
            out.add(t.replace(" ", "-"))
    return out


def strip_code(text: str) -> str:
    text = re.sub(r"```.*?```", "", text, flags=re.S)
    return re.sub(r"`[^`\n]*`", "", text)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--external", action="store_true")
    a = ap.parse_args()
    bad, ext = [], set()
    mds = sorted(p for p in ROOT.rglob("*.md") if "bin" not in p.parts and "obj" not in p.parts)
    for md in mds:
        for target in LINK.findall(strip_code(md.read_text(encoding="utf-8"))):
            if target.startswith(("http://", "https://")):
                ext.add(target)
                continue
            if target.startswith("mailto:"):
                continue
            path, _, frag = target.partition("#")
            dest = (md.parent / path).resolve() if path else md
            if not dest.exists():
                bad.append(f"{md.relative_to(ROOT)}: missing {target}")
            elif frag and dest.suffix == ".md" and frag.lower() not in anchors(dest):
                bad.append(f"{md.relative_to(ROOT)}: missing anchor #{frag} in {dest.name}")
    print(f"{len(mds)} markdown files, {len(ext)} external URLs")
    if a.external:
        for url in sorted(ext):
            try:
                req = urllib.request.Request(url, headers={"User-Agent": "Mozilla/5.0 link-check"})
                with urllib.request.urlopen(req, timeout=20) as r:
                    code = r.status
            except Exception as e:  # noqa: BLE001
                code = getattr(e, "code", str(e)[:60])
            if code != 200:
                bad.append(f"external {code}: {url}")
    for b in bad:
        print("  ", b)
    print("OK" if not bad else f"{len(bad)} problems")
    return 1 if bad else 0


if __name__ == "__main__":
    sys.exit(main())
