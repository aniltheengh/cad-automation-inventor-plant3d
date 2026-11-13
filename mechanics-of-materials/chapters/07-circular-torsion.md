# 07 — Torsion of circular shafts and power transmission

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
Torque twists a shaft. In a circular elastic shaft, shear stress is zero at the center and largest at the outer radius. The twist angle is a deformation, while shear stress is a strength-related quantity.

## Equations
For a circular shaft in Saint-Venant torsion:
$$
\tau(r)=\frac{Tr}{J},\quad
\phi=\int_0^L\frac{T(x)}{G(x)J(x)}\,dx,\quad
\phi=\frac{TL}{GJ}\ \text{for constants}.
$$
$$
J_{\rm solid}=\frac{\pi d^4}{32},\quad
J_{\rm hollow}=\frac{\pi(D^4-d^4)}{32},\quad
\tau_{\max,\rm solid}=\frac{16T}{\pi d^3}.
$$
Power is $\mathcal P=T\omega$ and $\omega=2\pi n/60$ for speed $n$ in rpm. Use power in W, torque in N·m and angular speed in rad/s. A shaft segment's internal torque comes from a free-body cut.

## Worked example
A solid shaft has $d=40$ mm, $L=1000$ mm and $G=80000$ MPa. At $T=500$ N·m, $J=251327.4$ mm⁴, $\tau_{\max}=39.789$ MPa and $\phi=0.024868$ rad = 1.425°. At 1200 rpm, the transmitted power is 62.832 kW.

## Practice and answer
Double the diameter at the same torque and length. **Answer:** maximum shear becomes one-eighth; twist becomes one-sixteenth.

## Transition and limitations
For fixed-ended shafts, combine torque equilibrium with twist compatibility. These circular-shaft stress equations do not apply to rectangles or open channels. Keyways and shoulders introduce local stress concentrations; bearing reactions and gear loads can add bending.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 6: Centroids and second moments of area](06-section-properties.md) · [8: Shear-force and bending-moment diagrams →](08-shear-moment-diagrams.md)
