# 17 — Indeterminate beams and compatibility methods

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
An extra support reduces movement but introduces an extra reaction. Compatibility supplies the missing equation. Choose a redundant reaction, remove its support to obtain a solvable primary structure, and require the final displacement to match the original support condition.

## Equations
For one redundant $`R`$:

```math
\delta_{\rm loads}+fR=\delta_{\rm support}.
```

$`f`$ is displacement per unit redundant force, with a stated positive direction. For several redundants:

```math
\sum_j f_{ij}R_j=\Delta_i-\delta_i^{(0)},\qquad
f_{ij}=\int\frac{m_i m_j}{EI}\,dx
```

for bending-only linear analysis. Add other deformation contributions when needed. Symmetry $`f_{ij}=f_{ji}`$ is a useful reciprocity check.

## Worked example
A propped cantilever of length $`L`$ carries full-span downward UDL $`w`$. Remove the prop. Cantilever tip deflection magnitude from $`w`$ is $`wL^4/(8EI)`$; an upward prop force $`R_B`$ produces opposite deflection $`R_BL^3/(3EI)`$. Zero final tip displacement gives $`R_B=3wL/8`$. Vertical equilibrium gives $`R_A=5wL/8`$; the fixed-end internal bending moment is hogging with magnitude $`wL^2/8`$.

For $`w=2`$ kN/m and $`L=4`$ m, $`R_B=3`$ kN, $`R_A=5`$ kN and the hogging end-moment magnitude is 4 kN·m.

## Practice and answer
If a prop settles downward, is zero displacement still the correct compatibility condition? **Answer:** no; set the final displacement equal to the specified settlement with its sign.

## Transition
Slope-deflection, moment distribution and stiffness methods are alternative ways of enforcing equilibrium and compatible rotations/displacements. Before using any method, count independent redundants and confirm the released structure remains stable. Temperature and support settlement can cause reactions without externally applied force.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 16: Strain energy, Castigliano, unit loads and impact](16-energy-impact.md) · [18: Column buckling and eccentric compression →](18-columns-buckling.md)
