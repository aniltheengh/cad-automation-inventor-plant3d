# Symbols, units and sign conventions

[Section home](README.md) · [Formula reference](formula-reference.md)

## Units first

| Quantity | SI choice | Convenient small-part choice |
|---|---|---|
| Force | N | N |
| Length | m | mm |
| Stress and elastic modulus | Pa = N/m² | MPa = N/mm² |
| Moment / torque | N·m | N·mm |
| Area | m² | mm² |
| Second moment / torsion constant | m⁴ | mm⁴ |
| Energy | J = N·m | N·mm; divide by 1000 for J |
| Strain | dimensionless | dimensionless or microstrain |
| Rotation | rad | rad; multiply by $`180/\pi`$ for degrees |

$`1`$ kN = $`1000`$ N; $`1`$ GPa = $`1000`$ MPa; $`1`$ m = $`1000`$ mm.
Consequently $`1`$ m² = $`10^6`$ mm² and $`1`$ m⁴ = $`10^{12}`$ mm⁴.
$`1`$ microstrain = $`10^{-6}`$. Temperature **differences** of 1 °C and 1 K are equal, but absolute-temperature equations require kelvin.

## Main symbols

| Symbol | Meaning | Usual dimensions |
|---|---|---|
| $`P,N,V`$ | Applied/axial/shear force; context defines each | force |
| $`M,T`$ | Bending moment, torque | force × length |
| $`w`$ | Distributed transverse load | force / length |
| $`A,A_s`$ | Cross-sectional area, shear area | length² |
| $`A_m`$ | Area enclosed by thin-wall median line | length² |
| $`I,J_p,J_t`$ | Second area moment, polar area moment, torsion constant | length⁴ |
| $`Q`$ | First moment of a partial area | length³ |
| $`Z,Z_e,Z_p`$ | Section modulus, elastic/plastic section modulus | length³ |
| $`E,G,K`$ | Young's, shear and bulk moduli | stress |
| $`\nu`$ | Poisson's ratio | dimensionless |
| $`\sigma,\tau`$ | Normal and shear stress | stress |
| $`\epsilon,\gamma`$ | Normal and engineering shear strain | dimensionless |
| $`\delta,v,u,u_r(r)`$ | Extension, beam/general/radial displacement | length |
| $`w_e,w_r`$ | Uniaxial elastic energy density and modulus of resilience | energy/volume = stress |
| $`\theta,\phi,\kappa`$ | Slope, twist angle, curvature | rad, rad, 1/length |
| $`\alpha,\rho,g`$ | Thermal expansion coefficient, density, gravitational acceleration | 1/K, mass/volume, length/time² |
| $`S_y,S_{ut},S_e`$ | Yield strength, ultimate tensile strength, fatigue strength parameter | stress |
| $`n`$ | Safety factor, or rotational speed when explicitly labelled rpm | context-dependent |
| $`K`$ in buckling | Effective-length factor, not bulk modulus | dimensionless |
| $`K_t,K_f`$ | Elastic stress concentration and fatigue notch factor | dimensionless |
| $`K_I,K_{IC}`$ | Mode-I stress intensity and fracture toughness | stress × √length |
| $`N_f`$ | Cycles to failure | cycle count |

Do not rely on the symbol alone. For example, $`q`$ is shear flow while $`q_n`$ is notch sensitivity; $`C,D`$ in Lamé's equations are constants, while $`D`$ elsewhere can be diameter.

## Sign conventions

- Tensile normal stress and extension are positive; compression is negative.
- A positive engineering shear stress $`\tau_{xy}`$ acts in +y on the +x face.
- In beam chapters, x goes right, distributed load w is positive downward and sagging moment M is positive. The corresponding diagram equations are $`V'=-w`$ and $`M'=V`$.
- For upward-positive beam deflection v, $`EI v''=M`$. Deflection tables explicitly give positive **magnitudes**.
- Curved-beam moment and principal-axis bending conventions are defined in their own chapters.
- A physically positive pressure magnitude produces **negative radial stress** at a pressure-loaded surface.

## A dimensional check

For $`\delta=PL^3/(3EI)`$, the units are

```math
\frac{N\cdot mm^3}{(N/mm^2)\cdot mm^4}=mm.
```

For $`\sigma=Mc/I`$, they are $`(N\cdot mm)\cdot mm/mm^4=N/mm^2`$.
A result with the wrong dimension is wrong even if its numerical value appears reasonable.
