# 08 — Shear-force and bending-moment diagrams

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
Before finding beam stress, determine how internal shear and moment vary along its length. Supports react to external loading; an internal cut reveals the forces transmitted across the section.

## Sign convention and equations
Let $`x`$ increase to the right, $`w`$ be positive downward and $`M`$ positive for sagging. Use the matching section-shear convention so

```math
\frac{dV}{dx}=-w,\qquad \frac{dM}{dx}=V.
```

A downward point force produces a negative jump in $`V`$. A concentrated couple produces a jump in $`M`$ whose sign follows the cut convention. On a continuous segment, extrema of $`M`$ occur where $`V=0`$. Also check endpoints and jumps.

## Worked example
For a simply supported span $`L=4`$ m carrying $`w=2`$ kN/m throughout, each reaction is 4 kN.

```math
V(x)=4-2x\ \mathrm{kN},\qquad M(x)=4x-x^2\ \mathrm{kN\,m}.
```

At $`x=2`$ m, $`V=0`$ and $`M_{\max}=4`$ kN·m. At both supports the moment is zero. The area under $`V`$ from 0 to 2 m is 4 kN·m, confirming the moment increase.

## Practice and answer
For the same span with only a 6 kN central point load, reactions are 3 kN, shear is +3 kN on the left half and −3 kN on the right half, and $`M_{\max}=PL/4=6`$ kN·m.

## Transition
Split piecewise problems at every load, support and load-distribution change. Sketch both diagrams with units. A fixed support usually has a nonzero moment; “moment is zero at a support” applies to the appropriate ideal pin/roller condition, not all supports.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 7: Torsion of circular shafts and power transmission](07-circular-torsion.md) · [9: Elastic bending stress and section sizing →](09-bending-stress.md)
