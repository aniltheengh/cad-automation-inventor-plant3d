# Mechanics of Materials / Strength of Materials — I and II

[Repository home](../README.md)

A separate engineering-learning section in this CAD automation repository. **No programming is required.** It begins with force diagrams and units and progresses to stress analysis, deformation, failure and introductory finite-element analysis.

**Start:** [Study guide](study-guide.md) → [01: Units, forces and equilibrium](chapters/01-units-statics.md).

**Quick access:** [All course formulas](formula-reference.md) · [Symbols and units](symbols-and-units.md) · [Textbook PDF](books/Roylance_Modules_in_Mechanics_of_Materials.pdf) · [Book credits and further reading](books/README.md).

## What is included

- 26 chapter notes organized into Mechanics of Materials I and II.
- Physical explanations, equations with assumptions, worked calculations and practice questions with answers in every chapter.
- A consolidated formula reference for every chapter in this syllabus.
- A beginner → transition → intermediate study method.
- One complete, unchanged, openly licensed 359-page textbook PDF: David Roylance's *Modules in Mechanics of Materials*, from MIT OpenCourseWare.
- A link to Virginia Tech's *Strength of Materials* for an additional introductory treatment.

## Scope of “Mechanics I and II”

Here these names mean a broad **two-course Mechanics of Materials / Strength of Materials sequence**, not separate full courses in rigid-body statics and dynamics. Chapter 1 reviews needed statics. University syllabuses differ; the complete coverage of this section is listed below. These are concise study notes, supported by the full textbook, rather than a claim that every specialist solid-mechanics topic or formula is included.

## Mechanics of Materials I — foundations

| Chapter | Topic |
|---|---|
| 01 | [Units, forces and equilibrium](chapters/01-units-statics.md) |
| 02 | [Stress, strain and connection stresses](chapters/02-stress-strain.md) |
| 03 | [Material behavior and elastic constants](chapters/03-material-behavior.md) |
| 04 | [Axial deformation, self-weight and temperature](chapters/04-axial-thermal.md) |
| 05 | [Compatibility and statically indeterminate bars](chapters/05-compatibility-composite-bars.md) |
| 06 | [Centroids and second moments of area](chapters/06-section-properties.md) |
| 07 | [Torsion of circular shafts and power transmission](chapters/07-circular-torsion.md) |
| 08 | [Shear-force and bending-moment diagrams](chapters/08-shear-moment-diagrams.md) |
| 09 | [Elastic bending stress and section sizing](chapters/09-bending-stress.md) |
| 10 | [Transverse shear, shear flow and built-up beams](chapters/10-transverse-shear.md) |
| 11 | [Beam slopes, deflections and superposition](chapters/11-beam-deflection.md) |
| 12 | [Thin pressure vessels and helical springs](chapters/12-thin-vessels-springs.md) |

## Mechanics of Materials II — intermediate applications

| Chapter | Topic |
|---|---|
| 13 | [Plane stress, principal stresses and Mohr's circle](chapters/13-stress-transformation.md) |
| 14 | [Strain transformation, rosettes and 3D elasticity](chapters/14-strain-three-dimensional.md) |
| 15 | [Combined loading and static failure criteria](chapters/15-combined-failure.md) |
| 16 | [Strain energy, Castigliano, unit loads and impact](chapters/16-energy-impact.md) |
| 17 | [Indeterminate beams and compatibility methods](chapters/17-indeterminate-beams.md) |
| 18 | [Column buckling and eccentric compression](chapters/18-columns-buckling.md) |
| 19 | [Unsymmetric bending, eccentric loads and shear center](chapters/19-unsymmetric-shear-center.md) |
| 20 | [Curved beams, hooks and rings](chapters/20-curved-beams.md) |
| 21 | [Thick cylinders and compound-cylinder ideas](chapters/21-thick-cylinders.md) |
| 22 | [Noncircular and thin-walled torsion](chapters/22-noncircular-torsion.md) |
| 23 | [Fatigue, fracture, creep and stress concentrations](chapters/23-fatigue-fracture-creep.md) |
| 24 | [Plastic bending, residual stress and limit analysis](chapters/24-plastic-bending.md) |
| 25 | [Composite beams and introductory anisotropy](chapters/25-composite-beams-materials.md) |
| 26 | [Elasticity and finite-element analysis: the transition](chapters/26-elasticity-fea.md) |

## How to work through a chapter

1. Sketch the part, supports and loads; describe what you expect it to do.
2. Read the physical explanation before choosing a formula.
3. Identify every symbol, unit and assumption.
4. Recalculate the worked example without looking at the answer.
5. Solve the practice question, then compare with the answer.
6. Use the textbook for the derivation and additional problems before continuing.

The calculations use idealized educational models. Material properties in examples are stated inputs, not certified values for your actual part. Real design also needs the applicable standards, load cases and failure checks.

## Relationship to CAD automation

Keep a hand calculation as a benchmark before automating a calculation or comparing with FEA. The original CAD/add-in documentation remains in [docs](../docs/18-plant3d-addin-development.md); this section can be studied independently.

## Licensing

Original repository notes follow the repository licence. The attached third-party textbook has its own **CC BY-NC-SA 4.0** terms and is **not MIT-licensed**. See [book attribution](books/README.md) for the source, licence, checksum and unchanged-file status.
