# 13 — Plane stress, principal stresses and Mohr's circle

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
The force on an imagined cut depends on the cut's orientation. Principal planes have zero shear stress. Principal stresses are properties of the stress state at a point, not necessarily the stresses on the original x/y faces.

## Equations
Let $\theta$ rotate the x-axis counterclockwise to x′. Positive $\tau_{xy}$ acts in +y on the +x face.
$$
\sigma_{x'}=\frac{\sigma_x+\sigma_y}{2}
+\frac{\sigma_x-\sigma_y}{2}\cos2\theta+\tau_{xy}\sin2\theta
$$
$$
\tau_{x'y'}=-\frac{\sigma_x-\sigma_y}{2}\sin2\theta+\tau_{xy}\cos2\theta
$$
$$
C=\frac{\sigma_x+\sigma_y}{2},\quad
R=\sqrt{\left(\frac{\sigma_x-\sigma_y}{2}\right)^2+\tau_{xy}^2},\quad
\sigma_{1,2}=C\pm R.
$$
Use $2\theta_p=\operatorname{atan2}(2\tau_{xy},\sigma_x-\sigma_y)$ for one principal orientation and check by substitution. Maximum in-plane shear is $R$.

For plane stress, the third principal stress is zero. The **absolute 3D** maximum shear is $(\max(\sigma_1,\sigma_2,0)-\min(\sigma_1,\sigma_2,0))/2$, which can exceed $R$.

## Worked example
At $\sigma_x=80$, $\sigma_y=20$, $\tau_{xy}=30$ MPa, $C=50$ and $R=42.426$ MPa. The in-plane principal stresses are 92.426 and 7.574 MPa. Absolute maximum shear is 46.213 MPa, not 42.426 MPa, because zero is the third principal stress.

## Practice and answer
Pure shear of 40 MPa gives principal stresses +40, −40 and 0 MPa; absolute maximum shear is 40 MPa.

## Transition
Mohr's circle has center $C$ and radius $R$; physical rotations map to twice the angle. Published plots differ in shear-axis direction. State the plotting convention and verify one transformed stress rather than guessing the rotation direction.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 12: Thin pressure vessels and helical springs](12-thin-vessels-springs.md) · [14: Strain transformation, rosettes and 3D elasticity →](14-strain-three-dimensional.md)
