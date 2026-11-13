# 11 — Beam slopes, deflections and superposition

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
A beam can remain below yield and still deflect too much. Bending stiffness is $`EI`$, combining material and geometry. Slope is rotation in radians; deflection is a displacement.

## Equations and boundary conditions
With upward-positive deflection $`v`$ and the sagging-positive convention of chapter 8:

```math
EI\,v''=M,\qquad (EI\,v'')''=-w.
```

For constant $`EI`$, integrate twice and apply conditions. A fixed end imposes $`v=0,v'=0`$; an ideal simple support imposes $`v=0`$ while allowing rotation. At an unloaded free end, $`M=V=0`$.

The table gives **magnitudes**, for constant $`EI`$, slender beams and downward loads:

| Case | Maximum deflection | Relevant slope magnitude |
|---|---|---|
| Cantilever, tip force $`P`$ | $`PL^3/(3EI)`$ at tip | $`PL^2/(2EI)`$ at tip |
| Cantilever, full-span UDL $`w`$ | $`wL^4/(8EI)`$ at tip | $`wL^3/(6EI)`$ at tip |
| Simply supported, central $`P`$ | $`PL^3/(48EI)`$ at center | $`PL^2/(16EI)`$ at supports |
| Simply supported, full UDL | $`5wL^4/(384EI)`$ at center | $`wL^3/(24EI)`$ at supports |

## Worked example
A cantilever has $`P=1000`$ N, $`L=1000`$ mm, $`E=200000`$ MPa and $`I=2\times10^6`$ mm⁴. Tip deflection magnitude is 0.833333 mm and tip slope is 0.00125 rad.

## Practice and answer
Double its length without changing anything else. **Answer:** tip deflection becomes 6.666667 mm, eight times larger.

## Transition
Superpose signed responses only when linearity and unchanged support conditions apply. The moment-area relations integrate $`M/EI`$ to obtain slope change and tangential deviation. Shear deformation matters for short/deep beams; see chapter 16.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 10: Transverse shear, shear flow and built-up beams](10-transverse-shear.md) · [12: Thin pressure vessels and helical springs →](12-thin-vessels-springs.md)
