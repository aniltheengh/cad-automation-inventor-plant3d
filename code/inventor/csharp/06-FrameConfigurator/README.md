# 06-FrameConfigurator — WinForms configurator → parts + assembly (C#)

A small product configurator: enter L × W × H, tube size A × t, name → **Build in Inventor**.

- Creates one square-tube part per distinct member length (legs, long rails, short rails) and re-uses it for every member of that length.
- Places them in a new assembly with **transformation matrices** (no constraints = fast and deterministic; add constraints later if the design must flex).
- Sets iProperties (Part Number, Description `SHS 40x40x3 L=1120`, Stock Number) so the BOM is clean.

## Files
| File | Content |
|---|---|
| `MainForm.cs` | UI built in code (no designer file) — easy to read and diff |
| `FrameBuilder.cs` | part creation (sketch rectangle with inner offset → extrude), placement matrices (`Matrix.SetToRotation`, `SetTranslation`, `TransformBy`) |

## Matrix rule you must know
`a.TransformBy(b)` means "apply **a**, then **b**". `FrameBuilder.Compose(a, b)` copies `a` and transforms
by `b`. Getting this backwards puts tubes in the wrong place without any error message.

## Same thing in iLogic
[`code/inventor/ilogic/04_Frame_Assembly_Driver.iLogicVb`](../../ilogic/04_Frame_Assembly_Driver.iLogicVb)
drives a *template* assembly instead (parameters + suppression). Compare both in [docs/01](../../../../docs/01-choosing-a-language.md).
