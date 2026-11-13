# 25 — Composite beams and introductory anisotropy

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
Bonded materials share deformation but carry different stress. Fiber composites also depend on loading direction: a single isotropic Young's modulus may be inadequate.

## Bonded elastic beams
For materials with moduli $`E_i`$, areas $`A_i`$ and centroids $`y_i`$, perfect bond and plane-section kinematics give

```math
y_E=\frac{\sum E_iA_iy_i}{\sum E_iA_i},\quad
(EI)_{\rm eff}=\sum E_i[I_{ci}+A_i(y_i-y_E)^2],
```

```math
\kappa=\frac{M}{(EI)_{\rm eff}},\quad
\sigma_i=-E_i\kappa(y-y_E).
```

Alternatively transform areas by modular ratio $`n_i=E_i/E_{\rm ref}`$, but convert transformed stresses back to the actual material.

## Fiber-composite introductory estimates
For aligned fibers with volume fraction $`V_f`$ and matrix fraction $`V_m=1-V_f`$:

```math
E_L\approx V_fE_f+V_mE_m,\qquad
\frac1{E_T}\approx\frac{V_f}{E_f}+\frac{V_m}{E_m}.
```

These idealized iso-strain/iso-stress estimates do not describe all real microstructures. An orthotropic plane-stress lamina uses directional moduli:

```math
\epsilon_1=\frac{\sigma_1}{E_1}-\frac{\nu_{21}}{E_2}\sigma_2,\quad
\epsilon_2=-\frac{\nu_{12}}{E_1}\sigma_1+\frac{\sigma_2}{E_2},\quad
\gamma_{12}=\frac{\tau_{12}}{G_{12}},
```

with reciprocal relation $`\nu_{12}/E_1=\nu_{21}/E_2`$.

## Worked example
For $`V_f=0.6`$, $`E_f=200`$ GPa and $`E_m=3`$ GPa, the longitudinal estimate is 121.2 GPa. The reciprocal transverse estimate is 7.335 GPa. Direction therefore matters enormously even with identical constituents.

## Practice and answer
Two bonded layers have equal area and centroid distances 0 and 10 mm, but modulus ratio 2:1. **Answer:** elastic neutral-axis coordinate is 3.333 mm from the first centroid.

## Transition
Layered laminates may couple extension, bending and twist. Slip, delamination, matrix cracking and directional strengths require additional models. Perfect-bond transformed-section calculations cannot detect interface failure by themselves.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 24: Plastic bending, residual stress and limit analysis](24-plastic-bending.md) · [26: Elasticity and finite-element analysis: the transition →](26-elasticity-fea.md)
