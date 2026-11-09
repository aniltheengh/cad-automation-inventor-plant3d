# 08-ApprenticeReader — read Inventor files without Inventor (C#)

```
ApprenticeReader Bracket.ipt                         # all property sets
ApprenticeReader Frame.iam --tree                    # referenced files, indented
ApprenticeReader Bracket.ipt --set "Revision Number=B" --set "Vendor=Misumi"
```

- Uses `ApprenticeServerComponent` (no UI, no licence, in-process, fast).
- `--set` writes iProperties, then `PropertySets.FlushToFile()` — the only "write" besides reference repair and *Save As copy*.
- `--tree` walks `ReferencedFileDescriptors` (resolved or not) — great for "which files are missing?" audits.

## Requirements (2026)
- Inventor 2026 **or** the separate free *Inventor Apprentice Server 2026* installer (from 2026 it is registry-free and no longer bundled with Inventor View, which was discontinued after 2023 — see [docs/08](../../../../docs/08-apprentice-design-automation-vault.md)).
- **x64** process (set in `Directory.Build.props`). A 32-bit host cannot load Apprentice.
- Do not run while Inventor has the file open for editing if you use `--set`.

VERIFY: creating `ApprenticeServerComponent` with `new` relies on the COM class being resolvable on the machine; if the 2026 registry-free server needs a different activation path on your install, see the Apprentice section of the 2026 SDK help.
