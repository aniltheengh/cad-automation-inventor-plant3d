# 01 — Units, forces and equilibrium

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Start here
Mechanics of materials asks two questions: will a part withstand its loading, and how much will it deform? Start by finding the loads. A free-body diagram replaces supports with forces and moments and shows dimensions. A pin in a 2D model usually supplies two force components; a roller one; a fixed support two forces and a moment.

Use one consistent unit system. N, mm and MPa are convenient because $`1\,\mathrm{MPa}=1\,\mathrm{N/mm^2}`$. A moment is force times distance, not a force. Area moments have units of length to the fourth power.

## Equations and method

```math
\sum F_x=0,\quad \sum F_y=0,\quad \sum M_O=0,\qquad
\mathbf M_O=\mathbf r\times\mathbf F
```

A distributed load $`w(x)`$ has resultant $`W=\int w(x)\,dx`$ at $`\bar x=\int xw(x)\,dx/W`$. For uniform $`w`$ over length $`L`$, $`W=wL`$ acts at the midpoint; a triangular load acts one-third of its length from the larger end.

For 3D equilibrium add $`\sum F_z=0`$ and three independent moment equations. An internal cut can expose axial force $`N`$, shear forces $`V`$, torque $`T`$ and bending moments $`M`$.

## Worked example
A simply supported beam has span 4 m and a downward 6 kN load 1 m from its left support. Taking moments about the left support gives $`R_B(4)=6(1)`$, so $`R_B=1.5`$ kN. Vertical equilibrium gives $`R_A=4.5`$ kN. Check that both reactions total 6 kN.

## Practice and answer
Move the same load to midspan. **Answer:** both reactions are 3 kN. Convert 2 kN·m to N·mm. **Answer:** $`2\times10^6`$ N·mm.

## Transition and limitations
Draw a cut before using any stress equation. An incorrect internal load cannot be fixed by a more advanced stress formula. Statics assumes negligible acceleration; dynamics needs additional equations.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← Study guide](../study-guide.md) · [2: Stress, strain and connection stresses →](02-stress-strain.md)
