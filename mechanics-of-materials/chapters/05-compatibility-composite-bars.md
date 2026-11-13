# 05 — Compatibility and statically indeterminate bars

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
Equilibrium alone cannot determine every support reaction in a redundant system. Add a deformation condition, called compatibility. Bonded members between rigid plates can have equal extension but different stresses.

## Equations and workflow
An axial member has stiffness $k=EA/L$. Parallel members with common extension share a total load as
$$
P_i=k_i\delta,\quad \delta=\frac{P}{\sum k_i},\quad
P_i=\frac{k_i}{\sum k_i}P.
$$
Members in series carry the same axial force if there are no intermediate loads; their total compliance is $\sum L_i/(E_iA_i)$.

For two bonded materials of equal length at uniform temperature change, with no external axial load:
$$
A_1\sigma_1+A_2\sigma_2=0,\qquad
\frac{\sigma_1}{E_1}+\alpha_1\Delta T
=\frac{\sigma_2}{E_2}+\alpha_2\Delta T.
$$
These equations express force balance and equal total strain. A known support movement becomes the right-hand side of a displacement compatibility equation.

## Worked example
Two equal-length bars support 60 kN through rigid end plates. Bar 1 has $E_1=200000$ MPa, $A_1=200$ mm²; bar 2 has $E_2=70000$ MPa, $A_2=400$ mm². Their stiffness ratio is $40:28$. Thus $P_1=35.294$ kN and $P_2=24.706$ kN. Their stresses are 176.471 and 61.765 MPa.

## Practice and answer
Two identical parallel bars carry 10 kN. **Answer:** 5 kN each, provided both engage at the same time and their end displacements are equal.

## Transition and limitations
Write geometry and compatibility before substituting numbers. Equal force, equal stress and equal strain are different statements. Initial gaps, different lengths, slip, temperature and plasticity can invalidate simple load-sharing fractions.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 4: Axial deformation, self-weight and temperature](04-axial-thermal.md) · [6: Centroids and second moments of area →](06-section-properties.md)
