# 09 — Elastic bending stress and section sizing

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning and derivation
For a slender straight beam, plane sections approximately remain plane. Fibers on one side lengthen and those on the other shorten. With small curvature $`\kappa`$, strain varies linearly as $`\epsilon_x=-\kappa y`$. Hooke's law and moment equilibrium give the flexure relation.

## Equations
For homogeneous linear-elastic bending about a centroidal principal axis:

```math
\sigma_x(y)=-\frac{My}{I},\quad
|\sigma_{\max}|=\frac{|M|c}{I}=\frac{|M|}{Z},\quad
\kappa=\frac{M}{EI}
```

The last relation uses a compatible curvature sign. With centric axial loading, add $`N/A`$. For an allowable stress magnitude $`\sigma_{\rm allow}`$, $`Z_{\rm required}=|M|/\sigma_{\rm allow}`$. Check both extreme fibers if distances from the neutral axis differ.

## Worked example
A rectangular beam is 40 mm wide and 80 mm deep with $`M=2`$ kN·m. Chapter 6 gives $`I=1.706667\times10^6`$ mm⁴ and $`c=40`$ mm. The extreme stress magnitude is 46.875 MPa. One face is in tension and the other in compression.

## Practice and answer
At $`M=3`$ kN·m and $`\sigma_{\rm allow}=100`$ MPa, **answer:** required section modulus is 30000 mm³. This does not establish acceptable deflection or buckling resistance.

## Transition and limitations
The ordinary neutral axis passes through the centroid only under the stated homogeneous elastic assumptions. Different bonded materials require an elastic-modulus-weighted section analysis. Deep beams, curved beams, local load application, yielding and lateral-torsional buckling require additional models. Do not treat a peak near a sharp notch as the nominal $`Mc/I`$ value.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 8: Shear-force and bending-moment diagrams](08-shear-moment-diagrams.md) · [10: Transverse shear, shear flow and built-up beams →](10-transverse-shear.md)
