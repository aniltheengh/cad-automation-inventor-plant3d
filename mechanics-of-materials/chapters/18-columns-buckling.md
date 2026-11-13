# 18 — Column buckling and eccentric compression

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
A slender compressed member can become unstable before the material yields. Elastic stiffness, unsupported length, end restraint and the weaker bending axis control the ideal buckling load.

## Equations
$$
P_{\rm cr}=\frac{\pi^2EI_{\min}}{(KL)^2},\quad
r_g=\sqrt{\frac{I_{\min}}A},\quad
\lambda=\frac{KL}{r_g},\quad
\sigma_{\rm cr}=\frac{\pi^2E}{\lambda^2}.
$$
Ideal effective-length factors: pinned-pinned $K=1$; fixed-free $K=2$; fixed-fixed $K=0.5$; fixed-pinned approximately 0.699. Real frames require a restraint/stability model rather than blindly adopting these numbers.

An introductory intermediate-column approximation is Johnson's parabola:
$$
\sigma_{\rm cr}\approx S_y\left[1-\frac{S_y}{4\pi^2E}\lambda^2\right].
$$
The tangent transition to Euler is $\lambda=\sqrt{2\pi^2E/S_y}$. This is not a replacement for a governing structural design curve.

For an ideal pin-ended column with load eccentricity $e$:
$$
\sigma_{\max}=\frac PA\left[1+\frac{ec}{r_g^2}
\sec\left(\frac L{2r_g}\sqrt{\frac{P}{EA}}\right)\right].
$$
Use the formula only within its elastic, stable, pin-ended idealization.

## Worked example
A pinned column has $E=200000$ MPa, $I_{\min}=10^6$ mm⁴ and $L=2000$ mm. Euler gives $P_{\rm cr}=493480$ N. A fixed-free column with the same data gives one-quarter: 123370 N.

## Practice and answer
Double the unbraced length with unchanged end conditions. **Answer:** ideal Euler load becomes one-quarter.

## Transition
Check elastic applicability, initial crookedness, eccentricity, residual stress, local buckling and lateral restraint. A nominal compressive stress below yield alone does not establish stability.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 17: Indeterminate beams and compatibility methods](17-indeterminate-beams.md) · [19: Unsymmetric bending, eccentric loads and shear center →](19-unsymmetric-shear-center.md)
