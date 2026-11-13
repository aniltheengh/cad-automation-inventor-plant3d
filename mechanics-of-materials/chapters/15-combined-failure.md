# 15 — Combined loading and static failure criteria

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials II — intermediate applications.

## Method
Choose a **point** on a critical section. Add the signed normal stresses and matching shear components at that same point, then form its stress tensor. Maxima from different points must not be combined as if simultaneous.

## Equations
For axial load plus single-axis bending:
$$
\sigma_x=\frac NA-\frac{My}{I}.
$$
Add circular-shaft torsional shear $Tr/J$ and transverse shear where appropriate. For isotropic ductile yielding:
$$
\sigma_{\rm VM}=\sqrt{\frac{(\sigma_1-\sigma_2)^2+
(\sigma_2-\sigma_3)^2+(\sigma_3-\sigma_1)^2}{2}}.
$$
Under plane stress:
$$
\sigma_{\rm VM}=\sqrt{\sigma_x^2-\sigma_x\sigma_y+\sigma_y^2+3\tau_{xy}^2}.
$$
Tresca equivalent stress is $\sigma_{\rm Tr}=\max|\sigma_i-\sigma_j|$. Compare either equivalent stress with the chosen yield strength $S_y$; the corresponding nominal yield factor is $S_y/\sigma_{\rm eq}$.

Maximum principal stress checks can be introductory models for brittle materials, using distinct tensile/compressive strengths. They do not replace fracture mechanics or material-specific criteria.

## Worked example
At a shaft surface point, $\sigma_x=100$ MPa and $\tau_{xy}=50$ MPa, with other plane-stress components zero. Von Mises stress is 132.288 MPa. The principal stresses are 120.711, −20.711 and 0 MPa; Tresca equivalent stress is 141.421 MPa. If $S_y=250$ MPa, yield factors are 1.890 and 1.768 respectively.

## Practice and answer
For pure shear, von Mises predicts first yield at $\tau=S_y/\sqrt3$; Tresca predicts $\tau=S_y/2$.

## Transition
Static yielding is only one failure mode. Fatigue, buckling, fracture and deflection may govern even with a high calculated yield factor. A stress concentration factor depends on geometry and the definition of nominal stress.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 14: Strain transformation, rosettes and 3D elasticity](14-strain-three-dimensional.md) · [16: Strain energy, Castigliano, unit loads and impact →](16-energy-impact.md)
