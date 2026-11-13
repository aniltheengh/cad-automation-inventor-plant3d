# 10 — Transverse shear, shear flow and built-up beams

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
Bending moment varies along a loaded beam. Adjacent cross-sections therefore require different longitudinal stresses; shear transfers force between them. Beam shear stress is generally not uniform across the depth.

## Equations
For a slender beam in elastic bending:

```math
\tau=\frac{VQ}{Ib},\qquad q=\frac{VQ}{I}.
```

$`Q=\int_{A'}y\,dA`$ is the first moment of the portion above or below the level being checked, about the neutral axis. $`b`$ is the local width carrying shear, not always the full flange width. Shear flow $`q`$ is force per length.

For a solid rectangle, $`\tau_{\max}=3V/(2A)`$ at the neutral axis and zero at the top/bottom free surfaces. For a solid circle the maximum transverse shear is $`4V/(3A)`$. For built-up members with connector spacing $`s`$, the ideal transferred force per connector station is $`F=q s`$; divide by the number of genuinely sharing connector lines as appropriate.

## Worked example
A 40 × 80 mm rectangular beam carries $`V=10`$ kN. Its area is 3200 mm². Average shear is 3.125 MPa and maximum shear is 4.6875 MPa.

## Practice and answer
An interface has $`q=120`$ N/mm and connector stations 50 mm apart. **Answer:** each station transfers 6000 N. If two equal parallel connectors truly share each station load, each carries 3000 N.

## Transition
For an I-section, calculate $`Q`$ and local web thickness at the web level. A flange-web junction transfers longitudinal shear even though it may look “horizontal.” Thin-walled unsymmetric sections can twist under transverse load; chapter 19 introduces the shear center.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 9: Elastic bending stress and section sizing](09-bending-stress.md) · [11: Beam slopes, deflections and superposition →](11-beam-deflection.md)
