# 02-SpurGearGenerator — real involute gear from three numbers (C#)

**Goal:** a complete "generator" EXE: engineering math in plain C#, geometry through the API.

```
dotnet run -- <module> <teeth> [pressureAngleDeg=20] [faceWidth=20] [bore=0] [outFolder]
dotnet run -- 2 40 20 20 12 C:\Temp\Gears        -> C:\Temp\Gears\Gear_m2_z40.ipt
```

## Files
| File | Content |
|---|---|
| `GearMath.cs` | pure math, **no Inventor reference**: radii, involute function, tooth thickness at radius, flank points, undercut check. Unit-tested against Python in `tools/test_tools.py` (identical to 1e-12). |
| `Program.cs` | blank extrude → sketch one tooth space (2 involute splines + root arc + tip arc) → cut through all → circular pattern ×z; user parameters `m`, `z`, `alpha`, `b`, `d_pitch = m * z`; iProperties; one `Transaction` so a single Undo removes it all. |

## Engineering notes
- Standard full-depth teeth: addendum = m, dedendum = 1.25 m (ISO 53 profile A-like). Root fillet is not modelled — use Design Accelerator for production gears.
- Below z ≈ 17 (α = 20°) the tool warns about undercut; a profile shift `x` is supported in `GearMath` (`GearSpec.X`).
- Checked: m = 2, z = 40 → d = 80, da = 84, df = 75, db = 75.175 mm.

## API points you will reuse
- `TransientGeometry.CreatePoint2d` in **cm**; `SketchSplines.Add(ObjectCollection of SketchPoints)`.
- `PartFeatures.ExtrudeFeatures.CreateExtrudeDefinition(profile, kCutOperation)` + `SetThroughAllExtent`.
- `CircularPatternFeatures.Add(collection, axis, true, "z", "360 deg")` — the count is a **parameter expression**, so changing `z` later re-patterns.
