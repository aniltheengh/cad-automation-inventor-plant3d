# 12 — Thin pressure vessels and helical springs

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Thin vessels
A thin wall carries membrane stress approximately uniform through its thickness. For differential internal pressure $`p`$, mean radius $`r`$ and thickness $`t`$:

```math
\sigma_h=\frac{pr}{t},\quad
\sigma_z=\frac{pr}{2t}\quad\text{(closed-end cylinder)},\qquad
\sigma_{\rm sphere}=\frac{pr}{2t}.
```

The cylindrical axial expression comes from pressure thrust on closed ends. Open-ended tubes with separately supported end loads need their actual axial loading. In a closed thin elastic cylinder, $`\epsilon_h=(\sigma_h-\nu\sigma_z)/E`$ and $`\epsilon_z=(\sigma_z-\nu\sigma_h)/E`$.

A common first thin-wall check is $`r/t\gtrsim10`$, but accuracy requirements and codes govern applicability. External-pressure buckling is not predicted by these membrane equations.

**Example:** $`p=2`$ MPa, $`r=100`$ mm, $`t=5`$ mm gives hoop stress 40 MPa and closed-end axial stress 20 MPa.

## Close-coiled round-wire springs
For axial force $`P`$, mean coil diameter $`D`$, wire diameter $`d`$, active turns $`n_a`$ and shear modulus $`G`$:

```math
\delta=\frac{8PD^3n_a}{Gd^4},\quad
k=\frac{Gd^4}{8D^3n_a},\quad
\tau_{\rm nominal}=\frac{8PD}{\pi d^3}.
```

With spring index $`C=D/d`$, a commonly used Wahl correction is

```math
K_w=\frac{4C-1}{4C-4}+\frac{0.615}{C},\qquad
\tau_{\max}\approx K_w\tau_{\rm nominal}.
```

These are close-coiled elastic approximations. End details, active turns, solid height, buckling and fatigue need separate checks.

**Example:** $`P=100`$ N, $`D=40`$ mm, $`d=5`$ mm, $`n_a=8`$, $`G=80000`$ MPa gives $`\delta=8.192`$ mm and $`k=12.207`$ N/mm.

## Practice and answer
Double vessel thickness at unchanged pressure/radius. **Answer:** membrane stress halves. Double spring wire diameter with other inputs fixed. **Answer:** spring stiffness increases sixteenfold within the model.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 11: Beam slopes, deflections and superposition](11-beam-deflection.md) · [13: Plane stress, principal stresses and Mohr's circle →](13-stress-transformation.md)
