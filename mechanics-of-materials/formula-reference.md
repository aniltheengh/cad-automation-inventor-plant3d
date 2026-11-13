# Formula reference — Mechanics of Materials I and II

**Prefer typeset equations? [Open the formula book PDF](formula-book/Mechanics_of_Materials_Formula_Book.pdf) or [download the offline HTML book](formula-book/Mechanics_of_Materials_Formula_Book.html).**

[Section home](README.md) · [Units and symbols](symbols-and-units.md)

This is the consolidated reference for the 26-chapter syllabus. It is **not every formula in all of solid mechanics**. Use the linked chapter for meanings, assumptions and examples. Unless a formula says otherwise, use small deformation, linear elasticity, consistent units and the stated ideal support conditions. Positive normal stress is tensile. Never choose an equation only because its symbols match your inputs.

## I-1. Equilibrium and distributed loads

```math
\sum\mathbf F=0,\quad \sum\mathbf M=0,\quad \mathbf M=\mathbf r\times\mathbf F,\quad
W=\int w\,dx,\quad \bar x=\frac{\int xw\,dx}{W}
```

[Chapter 1](chapters/01-units-statics.md). A uniform load has $`W=wL`$; a triangular load has $`W=w_{\max}L/2`$ and acts at $`L/3`$ from its larger end.

## I-2. Average stress and strain

```math
\sigma=\frac NA,\quad \tau_{\rm avg}=\frac V{A_s},\quad
\tau_{\rm pin}=\frac P{m\pi d^2/4},\quad
\sigma_{\rm bearing}=\frac P{td}
```

```math
\epsilon=\frac{\delta}{L},\quad \gamma\approx\frac{\Delta u}{h},\quad
A_{\rm net}=(b-d)t
```

[Chapter 2](chapters/02-stress-strain.md). Average stress is not the local peak at a hole or contact.

## I-3. Material laws

```math
\sigma=E\epsilon,\quad \tau=G\gamma,\quad
G=\frac E{2(1+\nu)},\quad K=\frac E{3(1-2\nu)},\quad
\nu=-\frac{\epsilon_{\rm lateral}}{\epsilon_{\rm axial}}
```

```math
w_e=\frac{\sigma^2}{2E},\quad w_r=\frac{S_y^2}{2E},\quad
\sigma_t=\frac P{A_{\rm current}},\quad\epsilon_t=\ln\frac L{L_0}
```

The scalar normal-stress law and energy-density formula above are uniaxial; multiaxial elasticity is in II-14. Under uniform constant-volume deformation, $`\sigma_t=\sigma_e(1+\epsilon_e)`$. [Chapter 3](chapters/03-material-behavior.md).

Engineering measures use the original area and length:

```math
\sigma_e=\frac{P}{A_0},\qquad \epsilon_e=\frac{L-L_0}{L_0}.
```

The elastic Poisson range is $`-1<\nu<0.5`$. The shear and bulk modulus conversions require an isotropic material.
## I-4. Axial and thermal deformation

```math
\delta=\int\frac N{EA}\,dx,\quad
\delta=\sum_i\frac{N_iL_i}{E_iA_i},\quad
k_{\rm axial}=\frac{EA}{L}
```

```math
\epsilon_T=\alpha\Delta T,\quad \delta_T=\alpha L\Delta T,\quad
\sigma_{\rm fully\ restrained}=-E\alpha\Delta T
```

For a uniform hanging bar under self-weight:

```math
\sigma_{\rm top}=\rho gL,\quad \delta=\frac{\rho gL^2}{2E}.
```

[Chapter 4](chapters/04-axial-thermal.md).

## I-5. Compatibility
Parallel engaged bars: $`\delta=P/\sum k_i`$, $`P_i=k_i\delta`$. Series compliance: $`1/k_{\rm eq}=\sum1/k_i`$.
For two bonded equal-length thermally loaded bars:

```math
A_1\sigma_1+A_2\sigma_2=P,\quad
\frac{\sigma_1}{E_1}+\alpha_1\Delta T
=\frac{\sigma_2}{E_2}+\alpha_2\Delta T.
```

[Chapter 5](chapters/05-compatibility-composite-bars.md).

## I-6. Section geometry

```math
\bar y=\frac{\sum A_iy_i}{\sum A_i},\quad
I_x=\int y^2dA,\quad I_y=\int x^2dA,\quad I_{xy}=\int xy\,dA
```

```math
I=I_c+Ad^2,\quad J_p=I_x+I_y,\quad Z=\frac Ic,\quad r_g=\sqrt{\frac IA}
```

| Section | Area | Centroidal $`I`$ | Circular torsion $`J`$ |
|---|---|---|---|
| Rectangle $`b\times h`$ | $`bh`$ | $`bh^3/12`$ about horizontal axis | Do not use $`I_x+I_y`$ |
| Circle diameter $`d`$ | $`\pi d^2/4`$ | $`\pi d^4/64`$ | $`\pi d^4/32`$ |
| Annulus $`D,d`$ | $`\pi(D^2-d^2)/4`$ | $`\pi(D^4-d^4)/64`$ | $`\pi(D^4-d^4)/32`$ |

Triangle of base $`b`$, height $`h`$: $`A=bh/2`$, centroid $`h/3`$ from base, centroidal $`I=bh^3/36`$ about an axis parallel to the base. [Chapter 6](chapters/06-section-properties.md).

## I-7. Circular torsion and power

```math
\tau=\frac{Tr}{J},\quad \tau_{\max,\rm solid}=\frac{16T}{\pi d^3},\quad
\phi=\int\frac T{GJ}\,dx,\quad \mathcal P=T\omega,\quad \omega=\frac{2\pi n}{60}
```

[Chapter 7](chapters/07-circular-torsion.md). Angles are radians; $`n`$ is rpm.

## I-8. Beam internal loads

```math
V'=-w,\quad M'=V
```

for downward-positive $`w`$ and sagging-positive $`M`$. [Chapter 8](chapters/08-shear-moment-diagrams.md).

| Load case | Maximum moment magnitude |
|---|---|
| Cantilever, tip force $`P`$ | $`PL`$ at fixed end |
| Cantilever, full-span UDL $`w`$ | $`wL^2/2`$ at fixed end |
| Simply supported, central $`P`$ | $`PL/4`$ |
| Simply supported, point $`P`$ at distances $`a,b`$ from supports, $`a+b=L`$ | $`Pab/L`$ |
| Simply supported, full-span UDL $`w`$ | $`wL^2/8`$ |

For the off-center point load, left reaction $`Pb/L`$ and right reaction $`Pa/L`$.

## I-9. Elastic bending

```math
\sigma_x=-\frac{My}{I},\quad |\sigma_{\max}|=\frac{|M|}{Z},\quad
Z_{\rm required}=\frac{|M|}{\sigma_{\rm allow}},\quad
\sigma_{\rm axial+bending}=\frac NA-\frac{My}{I}
```

[Chapter 9](chapters/09-bending-stress.md). Use centroidal principal axes for the simple form.

## I-10. Beam shear and shear flow

```math
Q=\int_{A'}y\,dA,\quad \tau=\frac{VQ}{Ib},\quad q=\frac{VQ}{I},\quad
F_{\rm station}=qs
```

Rectangle: $`\tau_{\max}=3V/(2A)`$. Solid circle: $`\tau_{\max}=4V/(3A)`$. [Chapter 10](chapters/10-transverse-shear.md).

## I-11. Elastic deflection
For upward-positive $`v`$: $`EI v''=M`$ and $`(EI v'')''=-w`$.

| Case, constant $`EI`$ | Maximum deflection magnitude | Slope magnitude |
|---|---|---|
| Cantilever tip $`P`$ | $`PL^3/(3EI)`$ | $`PL^2/(2EI)`$ at tip |
| Cantilever full UDL $`w`$ | $`wL^4/(8EI)`$ | $`wL^3/(6EI)`$ at tip |
| Simply supported central $`P`$ | $`PL^3/(48EI)`$ | $`PL^2/(16EI)`$ at support |
| Simply supported full UDL $`w`$ | $`5wL^4/(384EI)`$ | $`wL^3/(24EI)`$ at support |

Moment-area relations with compatible signed curvature:

```math
\theta_B-\theta_A=\int_A^B\frac M{EI}\,dx,\quad
v_B-v_A-\theta_A(B-A)=\int_A^B(B-x)\frac M{EI}\,dx.
```

[Chapter 11](chapters/11-beam-deflection.md).

## I-12. Thin vessels and springs

```math
\sigma_h=\frac{pr}{t},\quad \sigma_{z,\rm closed}=\frac{pr}{2t},\quad
\sigma_{\rm sphere}=\frac{pr}{2t}
```

```math
\epsilon_h=\frac{\sigma_h-\nu\sigma_z}{E},\quad
\epsilon_z=\frac{\sigma_z-\nu\sigma_h}{E},\quad
\frac{\Delta V}{V}\approx2\epsilon_h+\epsilon_z
```

The volume relation is for the small strain of a cylindrical volume. Helical spring:

```math
\delta=\frac{8PD^3n_a}{Gd^4},\quad k=\frac{Gd^4}{8D^3n_a},\quad
C_s=\frac Dd,\quad K_w=\frac{4C_s-1}{4C_s-4}+\frac{0.615}{C_s},
\quad \tau_{\max}\approx K_w\frac{8PD}{\pi d^3}
```

[Chapter 12](chapters/12-thin-vessels-springs.md).

For a free thin sphere in an elastic membrane state, tangential strain is $`\epsilon_t=(1-\nu)\sigma_{\rm sphere}/E`$; to first order $`\Delta r/r=\epsilon_t`$ and $`\Delta V/V=3\epsilon_t`$. The cylinder formulas presume pressure thrust carried by closed ends. Spring equations assume a close-coiled round-wire spring; $`n_a`$ counts active turns and $`D`$ is the mean coil diameter.
## II-13. Stress transformation

```math
C=\frac{\sigma_x+\sigma_y}{2},\quad
R=\sqrt{\left(\frac{\sigma_x-\sigma_y}{2}\right)^2+\tau_{xy}^2}
```

```math
\sigma_{x'}=C+\frac{\sigma_x-\sigma_y}{2}\cos2\theta+\tau_{xy}\sin2\theta,\quad
\tau_{x'y'}=-\frac{\sigma_x-\sigma_y}{2}\sin2\theta+\tau_{xy}\cos2\theta
```

```math
\sigma_{1,2}=C\pm R,\quad
2\theta_p=\operatorname{atan2}(2\tau_{xy},\sigma_x-\sigma_y),\quad
\tau_{\max,3D}=\frac{\sigma_{\max}-\sigma_{\min}}2
```

Include the third principal stress, zero for plane stress, in the last expression. [Chapter 13](chapters/13-stress-transformation.md).

The in-plane maximum shear is $`\tau_{\max,\rm in}=R`$; it need not equal the absolute 3D maximum. Re-sort the two in-plane principal stresses together with zero for plane stress.
## II-14. Strain and 3D elasticity

```math
\epsilon_\theta=\frac{\epsilon_x+\epsilon_y}{2}
+\frac{\epsilon_x-\epsilon_y}{2}\cos2\theta+\frac{\gamma_{xy}}2\sin2\theta
```

```math
\epsilon_x=\epsilon_0,\quad \epsilon_y=\epsilon_{90},\quad
\gamma_{xy}=2\epsilon_{45}-\epsilon_0-\epsilon_{90}
```

```math
\epsilon_x=\frac{\sigma_x-\nu(\sigma_y+\sigma_z)}E+\alpha\Delta T,\quad
\gamma_{xy}=\frac{\tau_{xy}}G
```

Use cyclic versions for other directions. For plane stress without thermal strain:

```math
\sigma_x=\frac E{1-\nu^2}(\epsilon_x+\nu\epsilon_y),\quad
\sigma_y=\frac E{1-\nu^2}(\epsilon_y+\nu\epsilon_x).
```

[Chapter 14](chapters/14-strain-three-dimensional.md).

The in-plane principal strains are

```math
\epsilon_{1,2}=\frac{\epsilon_x+\epsilon_y}{2}\pm\sqrt{\left(\frac{\epsilon_x-\epsilon_y}{2}\right)^2+\left(\frac{\gamma_{xy}}2\right)^2}.
```

Engineering shear strain is $`\gamma_{xy}=2\epsilon_{xy}`$. For zero-temperature-change plane strain, $`\sigma_z=\nu(\sigma_x+\sigma_y)`$. For thermally loaded plane strain, $`\sigma_z=\nu(\sigma_x+\sigma_y)-E\alpha\Delta T`$. Do not set both axial strain and axial stress to zero indiscriminately.
## II-15. Failure criteria

```math
\sigma_{\rm VM}=\sqrt{\frac{(\sigma_1-\sigma_2)^2+(\sigma_2-\sigma_3)^2+
(\sigma_3-\sigma_1)^2}{2}}
```

```math
\sigma_{\rm VM,plane}=\sqrt{\sigma_x^2-\sigma_x\sigma_y+\sigma_y^2+3\tau_{xy}^2},
\quad \sigma_{\rm Tr}=\max|\sigma_i-\sigma_j|,\quad n_y=\frac{S_y}{\sigma_{\rm eq}}.
```

[Chapter 15](chapters/15-combined-failure.md).

In pure shear, von Mises gives first yield at $`\tau_y=S_y/\sqrt3`$; Tresca gives $`\tau_y=S_y/2`$. Apply signed stresses at one point before forming the equivalent stress.
## II-16. Energy methods and impact

```math
U=\int\left(\frac{N^2}{2EA}+\frac{M^2}{2EI}+\frac{T^2}{2GJ_t}
+\frac{V^2}{2k_sGA}\right)dx,\quad
\delta_i=\frac{\partial U}{\partial P_i},\quad \theta_i=\frac{\partial U}{\partial M_i}
```

Bending unit-load method: $`\delta=\int M m/(EI)\,dx`$.
Ideal falling weight:

```math
W(h+\delta)=\tfrac12 k\delta^2,\quad
\delta=\frac Wk\left(1+\sqrt{1+\frac{2kh}{W}}\right).
```

[Chapter 16](chapters/16-energy-impact.md).

For a linear spring, $`U=k\delta^2/2=P\delta/2`$. An Euler-Bernoulli bending calculation omits the shear-energy term deliberately; a shear-flexible model must use a section-appropriate $`k_s`$. A common elementary rectangular-section approximation is $`k_s=5/6`$.
## II-17. Indeterminate structures

```math
\delta_0+fR=\Delta_{\rm support},\quad
\sum_j f_{ij}R_j=\Delta_i-\delta_i^{(0)},\quad
f_{ij}=\int\frac{m_i m_j}{EI}\,dx.
```

Propped cantilever under full UDL: $`R_{\rm prop}=3wL/8`$ and fixed-end hogging moment magnitude $`wL^2/8`$. [Chapter 17](chapters/17-indeterminate-beams.md).

## II-18. Buckling

```math
P_{\rm cr}=\frac{\pi^2EI_{\min}}{(KL)^2},\quad
\lambda=\frac{KL}{r_g},\quad
\sigma_{\rm Euler}=\frac{\pi^2E}{\lambda^2}
```

```math
\sigma_{\rm Johnson}\approx S_y\left(1-\frac{S_y\lambda^2}{4\pi^2E}\right),\quad
\lambda_{\rm transition}=\sqrt{\frac{2\pi^2E}{S_y}}.
```

Ideal pin-ended eccentric column:

```math
\sigma_{\max}=\frac PA\left[1+\frac{ec}{r_g^2}
\sec\left(\frac L{2r_g}\sqrt{\frac P{EA}}\right)\right].
```

[Chapter 18](chapters/18-columns-buckling.md). These ideal models are not design-code column curves.

Use Johnson only on its intended branch $`0\le\lambda\le\lambda_{\rm transition}`$ and Euler only in the elastic proportional range. Ideal end factors: pin-pin $`K=1`$, fixed-free $`K=2`$, fixed-fixed $`K=0.5`$, fixed-pin $`K\approx0.699`$. In the secant equation, $`P>0`$ is compression, $`e,c\ge0`$, equal end eccentricities are assumed and $`P<P_{\rm cr}`$.
## II-19. Unsymmetric bending

```math
I_{1,2}=\frac{I_y+I_z}{2}\pm
\sqrt{\left(\frac{I_y-I_z}{2}\right)^2+I_{yz}^2},\quad
\sigma_x=\frac NA+\frac{M_uv}{I_u}-\frac{M_vu}{I_v}
```

Principal-axis moment definitions are in [chapter 19](chapters/19-unsymmetric-shear-center.md). Rectangle middle-third condition: $`|e|\le h/6`$ for single-axis full-compression stress.

## II-20. Curved beams

```math
r_n=\frac A{\int_A dA/r},\quad r_c=\frac{\int_A r\,dA}{A},\quad e=r_c-r_n,
\quad \sigma_\theta=\frac M{Ae}\left(\frac{r_n}{r}-1\right)
```

Positive $`M`$ produces inner-fiber tension under the defined pure-bending convention. For constant width, $`r_n=(r_o-r_i)/\ln(r_o/r_i)`$. [Chapter 20](chapters/20-curved-beams.md).

## II-21. Thick cylinders

```math
C=\frac{p_i a^2-p_o b^2}{b^2-a^2},\quad
D=\frac{a^2b^2(p_i-p_o)}{b^2-a^2},\quad
\sigma_r=C-\frac D{r^2},\quad \sigma_\theta=C+\frac D{r^2}.
```

For pressure-loaded closed ends, $`\sigma_z=C`$. Plane-stress displacement:
$`u_r(r)=[(1-\nu)Cr+(1+\nu)D/r]/E`$. [Chapter 21](chapters/21-thick-cylinders.md).

With a uniform specified axial stress:

```math
u_r(r)=\frac{[(1-\nu)C-\nu\sigma_z]r+(1+\nu)D/r}{E}.
```

Thus closed ends carrying pressure thrust give

```math
u_r(r)=\frac{(1-2\nu)Cr+(1+\nu)D/r}{E}.
```

For zero axial strain with no temperature change, $`\sigma_z=2\nu C`$, a different condition. Always check $`\sigma_r(a)=-p_i`$ and $`\sigma_r(b)=-p_o`$.
## II-22. Noncircular torsion
Thin strip: $`J_t\approx bt^3/3`$, $`\tau_{\max}\approx3T/(bt^2)`$.
Open strips: $`J_t\approx\sum b_it_i^3/3`$.
Single closed cell:

```math
q=\frac T{2A_m},\quad \tau=\frac qt,\quad
\phi'=\frac{T}{4A_m^2}\oint\frac{ds}{Gt},\quad
J_t=\frac{4A_m^2}{\oint ds/t}\quad\text{for constant }G.
```

[Chapter 22](chapters/22-noncircular-torsion.md).

## II-23. Fatigue, fracture and creep

```math
\sigma_a=\frac{\sigma_{\max}-\sigma_{\min}}2,\quad
\sigma_m=\frac{\sigma_{\max}+\sigma_{\min}}2,\quad
\frac{\sigma_a}{S_e}+\frac{\sigma_m}{S_{ut}}\le\frac1n
```

```math
\mathcal D=\sum\frac{n_i}{N_i},\quad K_f=1+q_n(K_t-1),\quad
K_I=Y\sigma\sqrt{\pi a},\quad
a_c=\frac1\pi\left(\frac{K_{IC}}{Y\sigma}\right)^2
```

```math
\frac{da}{dN}=C_P(\Delta K)^m,\qquad
\dot\epsilon=A_c\sigma^n\exp[-Q_c/(RT)].
```

These are specific empirical or idealized models, not universal material properties. [Chapter 23](chapters/23-fatigue-fracture-creep.md).

Stress ratio and a fitted stress-life curve:

```math
R_\sigma=\frac{\sigma_{\min}}{\sigma_{\max}},\qquad \sigma_a=C_fN_f^{b_f}.
```

Do not evaluate the ratio when $`\sigma_{\max}=0`$. The displayed Goodman screening equation uses tensile mean stress, appropriate component fatigue data and the stated safety-factor convention. For constant crack-geometry factor $`Y`$, the explicit $`a_c`$ expression applies; otherwise solve $`Y(a_c)\sigma\sqrt{\pi a_c}=K_{IC}`$. Paris and creep constants are unit-, temperature- and material-dependent.
## II-24. Plastic bending

```math
M_y=S_yZ_e,\quad M_p=S_yZ_p,\quad Z_p=\int_A|y-y_p|dA.
```

Rectangle: $`Z_e=bh^2/6`$, $`Z_p=bh^2/4`$, shape factor 1.5.
Simply supported central-force ideal collapse: $`P_c=4M_p/L`$. [Chapter 24](chapters/24-plastic-bending.md).

Plastic shape factor is $`Z_p/Z_e`$. At an ideal collapse mechanism, external work equals $`\sum M_p|\Delta\theta|`$ over the plastic hinges. Rotation capacity and absence of premature instability are necessary assumptions.
## II-25. Composites

```math
y_E=\frac{\sum E_iA_iy_i}{\sum E_iA_i},\quad
(EI)_{\rm eff}=\sum E_i[I_{ci}+A_i(y_i-y_E)^2],\quad
\sigma_i=-E_i\frac M{(EI)_{\rm eff}}(y-y_E).
```

```math
E_L\approx V_fE_f+V_mE_m,\quad
E_T\approx\left(\frac{V_f}{E_f}+\frac{V_m}{E_m}\right)^{-1},\quad
\frac{\nu_{12}}{E_1}=\frac{\nu_{21}}{E_2}.
```

[Chapter 25](chapters/25-composite-beams-materials.md).

For the ideal orthotropic plane-stress lamina:

```math
\epsilon_1=\frac{\sigma_1}{E_1}-\frac{\nu_{21}\sigma_2}{E_2},\qquad
\epsilon_2=-\frac{\nu_{12}\sigma_1}{E_1}+\frac{\sigma_2}{E_2},\qquad
\gamma_{12}=\frac{\tau_{12}}{G_{12}}.
```

For bonded beams, curvature is $`\kappa=M/(EI)_{\rm eff}`$. Perfect bond and plane-section kinematics are required.
## II-26. Elasticity and bar finite elements

```math
\nabla\cdot\boldsymbol\sigma+\mathbf b=0,\quad
\boldsymbol\epsilon=\tfrac12[\nabla\mathbf u+(\nabla\mathbf u)^{T}],\quad
\boldsymbol\sigma=\mathbf C:\boldsymbol\epsilon
```

```math
\mathbf k_e=\frac{EA}{L}\begin{bmatrix}1&-1\\-1&1\end{bmatrix},\quad
\mathbf K\mathbf u=\mathbf F,\quad
\epsilon_e=\frac{u_2-u_1}{L}.
```

[Chapter 26](chapters/26-elasticity-fea.md).

The stress-strain equation above excludes initial/thermal strain. With isotropic thermal expansion:

```math
\boldsymbol\sigma=\mathbf C:(\boldsymbol\epsilon-\alpha\Delta T\mathbf I).
```

Apply sufficient independent displacement constraints before solving the assembled stiffness system.
## Before accepting a numerical result
Check equilibrium, consistent units, the correct section axis, actual support conditions, material-model applicability and the requested failure mode. Repeat with a simple limiting case. Formula substitution alone is not verification.
