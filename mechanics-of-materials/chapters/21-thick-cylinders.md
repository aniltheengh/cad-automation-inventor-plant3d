# 21 — Thick cylinders and compound-cylinder ideas

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
Thick walls develop significant radial stress and nonuniform hoop stress. Use an axisymmetric elastic model away from ends and discontinuities; the thin-wall membrane approximation may be insufficient.

## Lamé equations
For inner radius $a$, outer radius $b$, positive internal pressure $p_i$ and external pressure $p_o$, tensile stress positive:
$$
\sigma_r=C-\frac{D}{r^2},\quad
\sigma_\theta=C+\frac{D}{r^2},
$$
$$
C=\frac{p_i a^2-p_o b^2}{b^2-a^2},\quad
D=\frac{a^2b^2(p_i-p_o)}{b^2-a^2}.
$$
Check $\sigma_r(a)=-p_i$ and $\sigma_r(b)=-p_o$. For closed ends with pressure thrust carried uniformly by the cylinder wall and no additional axial force, $\sigma_z=C$. Different end constraints change axial stress.

Under plane stress, radial displacement is
$$
u(r)=\frac{(1-\nu)Cr+(1+\nu)D/r}{E}.
$$
For a specified nonzero axial stress, the corresponding expression becomes
$u=[(C-\nu C-\nu\sigma_z)r+(1+\nu)D/r]/E$.

## Worked example
$a=50$ mm, $b=100$ mm, $p_i=10$ MPa, $p_o=0$. Then $C=3.333$ MPa, $D=33333.333$ MPa·mm². At the bore, $\sigma_r=-10$ MPa and $\sigma_\theta=16.667$ MPa. At the outside, they are 0 and 6.667 MPa. Closed-end axial stress is 3.333 MPa.

## Practice and answer
For the same radii and internal pressure 20 MPa, all these elastic stresses double.

## Transition
A shrink fit creates contact pressure from radial compatibility: outer-member bore expansion minus inner-member surface expansion equals radial interference. Distinguish radial from diametral interference. Include both parts' elastic response and superpose later pressure loads only while contact and elasticity assumptions remain valid. External-pressure instability needs a separate check.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 20: Curved beams, hooks and rings](20-curved-beams.md) · [22: Noncircular and thin-walled torsion →](22-noncircular-torsion.md)
