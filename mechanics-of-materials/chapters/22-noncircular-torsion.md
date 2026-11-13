# 22 — Noncircular and thin-walled torsion

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
A noncircular section warps under torque. Its torsional stiffness uses a torsion constant $`J_t`$, which generally differs from the polar area moment. Closed thin-walled sections carry circulating shear flow and can be much stiffer than slit/open versions.

## Equations
For uniform Saint-Venant torsion, $`\phi'=T/(GJ_t)`$. For a thin rectangular strip of long width $`b`$ and thickness $`t\ll b`$:

```math
J_t\approx\frac{bt^3}{3},\qquad
\tau_{\max}\approx\frac{3T}{bt^2}.
```

For an open section assembled from thin strips, $`J_t\approx\sum b_i t_i^3/3`$, neglecting junction corrections.

For a single closed thin-walled cell, with median-line enclosed area $`A_m`$:

```math
q=\frac{T}{2A_m},\quad \tau(s)=\frac q{t(s)},\quad
\phi'=\frac{T}{4A_m^2}\oint\frac{ds}{G(s)t(s)}.
```

For constant $`G`$, $`J_t=4A_m^2/\oint ds/t`$. Use the area enclosed by the wall **median line**, not material area.

## Worked example
A rectangular single-cell tube has median dimensions 100 × 50 mm and uniform thickness 2 mm. $`A_m=5000`$ mm² and median perimeter 300 mm. At $`T=10^6`$ N·mm, $`q=100`$ N/mm and $`\tau=50`$ MPa. $`J_t=666666.7`$ mm⁴. For $`G=80000`$ MPa and $`L=1000`$ mm, twist is 0.01875 rad.

## Practice and answer
Halve thickness without changing median geometry and torque. **Answer:** shear stress doubles and torsional stiffness approximately halves.

## Transition
Multiple closed cells need torque equilibrium and equal compatible twist, with differences of cell flows on common walls. Restrained warping adds longitudinal normal stresses and cannot be captured by free-warping Saint-Venant torsion alone.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 21: Thick cylinders and compound-cylinder ideas](21-thick-cylinders.md) · [23: Fatigue, fracture, creep and stress concentrations →](23-fatigue-fracture-creep.md)
