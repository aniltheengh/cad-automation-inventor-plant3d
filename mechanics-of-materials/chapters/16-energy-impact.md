# 16 — Strain energy, Castigliano, unit loads and impact

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
Elastic deformation stores energy. Energy methods calculate displacement without finding the full deflected shape. Under a gradually applied force, work is $`P\delta/2`$ for a linear system, not $`P\delta`$.

## Equations
For a slender linear-elastic member:

```math
U=\int\left(\frac{N^2}{2EA}+\frac{M^2}{2EI}+
\frac{T^2}{2GJ_t}+\frac{V^2}{2k_sGA}\right)dx.
```

$`J_t`$ is the appropriate torsion constant and $`k_s`$ a shear correction factor. Terms not relevant to the model may be omitted deliberately. Castigliano's relation is $`\delta_i=\partial U/\partial P_i`$, and rotation is $`\partial U/\partial M_i`$.

With a virtual unit load, $`\delta=\int M m/(EI)\,dx`$ for bending alone, where $`M`$ is the real-load moment and $`m`$ the moment due to the unit load. Include axial, torsion or shear terms if significant.

For a weight $`W`$ dropped through height $`h`$ onto an ideal massless linear spring of stiffness $`k`$, with no energy loss:

```math
W(h+\delta)=\tfrac12k\delta^2,\qquad
\delta=\frac Wk\left(1+\sqrt{1+\frac{2kh}{W}}\right).
```

At $`h=0`$ a suddenly applied load produces twice the static deflection; a gradually applied load does not.

## Worked example
A uniform axial bar has $`P=10000`$ N, $`L=1000`$ mm, $`E=200000`$ MPa, $`A=100`$ mm². $`U=P^2L/(2EA)=2500`$ N·mm = 2.5 J. Differentiating gives $`\delta=PL/(EA)=0.5`$ mm.

## Practice and answer
A suddenly applied 100 N force acts on a 200 N/mm ideal spring. **Answer:** maximum ideal displacement 1 mm, versus 0.5 mm static.

## Transition
Impact results depend strongly on contact, mass distribution, damping and plasticity. Castigliano's simple strain-energy form here assumes linear elasticity; do not treat it as universally valid for nonlinear materials.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 15: Combined loading and static failure criteria](15-combined-failure.md) · [17: Indeterminate beams and compatibility methods →](17-indeterminate-beams.md)
