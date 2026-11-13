# 24 — Plastic bending, residual stress and limit analysis

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
After the first outer fibers yield, the rest of a ductile cross-section may still carry increased moment. Plastic-section theory idealizes this redistribution. It does not imply unlimited deformation or guaranteed stability.

## Equations
For an elastic-perfectly plastic material with symmetric tensile/compressive yield strength:

```math
M_y=S_y Z_e,\qquad M_p=S_y Z_p,\qquad
Z_p=\int_A |y-y_p|\,dA.
```

The plastic neutral axis $`y_p`$ divides tensile and compressive force areas equally for pure bending of a homogeneous section with symmetric yield. For a rectangle:

```math
Z_e=\frac{bh^2}{6},\quad Z_p=\frac{bh^2}{4},\quad
\frac{M_p}{M_y}=1.5.
```

With axial force, use force equilibrium to locate the plastic neutral axis; equal areas no longer generally apply. For an ideal collapse mechanism, equate external work to $`\sum M_p|\Delta\theta|`$ at plastic hinges, provided a kinematically admissible mechanism exists.

## Worked example
A 40 × 80 mm rectangle with $`S_y=250`$ MPa has $`Z_e=42666.7`$ mm³ and $`Z_p=64000`$ mm³. First-yield moment is 10.667 kN·m; fully plastic moment is 16 kN·m.

For a simply supported span with a central point force, the ideal bending collapse load is $`P_c=4M_p/L`$ if shear and instability do not intervene.

## Practice and answer
For $`M_p=16`$ kN·m and $`L=4`$ m, **answer:** ideal $`P_c=16`$ kN.

## Transition
Unloading after partial yielding can leave residual stresses and permanent curvature. Local buckling, lateral-torsional buckling, brittle fracture or inadequate rotation capacity can prevent development of $`M_p`$. A nominal plastic hinge is a model of a yielded region, not a physical pin installed in the beam.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 23: Fatigue, fracture, creep and stress concentrations](23-fatigue-fracture-creep.md) · [25: Composite beams and introductory anisotropy →](25-composite-beams-materials.md)
