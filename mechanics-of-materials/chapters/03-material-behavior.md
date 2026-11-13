# 03 — Material behavior and elastic constants

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
Stiffness and strength are different. Young's modulus describes elastic resistance to strain; yield strength describes the onset of a chosen permanent-deformation criterion. Ultimate tensile strength is the peak engineering tensile stress. A stronger steel need not have a much larger elastic modulus.

## Equations
For small, homogeneous isotropic linear elasticity, the first equation is the **uniaxial** mechanical-stress/strain relation; the shear equation uses engineering shear strain. The Poisson ratio is measured under uniaxial loading. For multiaxial or thermal loading use chapter 14:

```math
\sigma=E\epsilon,\quad \tau=G\gamma,\quad
\nu=-\frac{\epsilon_{\rm lateral}}{\epsilon_{\rm axial}},\quad
G=\frac{E}{2(1+\nu)},\quad K=\frac{E}{3(1-2\nu)}
```

$`E,G,K`$ are Young's, shear and bulk moduli; $`\nu`$ is Poisson's ratio. The stable isotropic elastic range is $`-1<\nu<0.5`$; near incompressibility needs care.

Engineering measures use original dimensions: $`\sigma_e=P/A_0`$, $`\epsilon_e=(L-L_0)/L_0`$. True measures use current area and logarithmic strain: $`\sigma_t=P/A`$, $`\epsilon_t=\ln(L/L_0)`$. For uniform deformation with approximately constant volume, $`\sigma_t=\sigma_e(1+\epsilon_e)`$; do not apply that conversion blindly after necking.

Under uniaxial linear-elastic loading, stored elastic energy per volume is $`w_e=\sigma^2/(2E)`$. The ideal uniaxial modulus of resilience is $`S_y^2/(2E)`$.

## Worked example
With $`E=200`$ GPa and $`\nu=0.30`$, $`G=76.923`$ GPa and $`K=166.667`$ GPa. At uniaxial stress 100 MPa, axial strain is 0.0005 and lateral strain is −0.00015.

## Practice and answer
If $`S_y=250`$ MPa and $`E=200000`$ MPa, find resilience. **Answer:** 0.15625 MPa = 156250 J/m³.

## Transition
A nominal tensile yield factor is $`n=S_y/|\sigma|`$ for a uniaxial check. Material grade, temperature, processing and loading rate affect properties; example values here are teaching inputs.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 2: Stress, strain and connection stresses](02-stress-strain.md) · [4: Axial deformation, self-weight and temperature →](04-axial-thermal.md)
