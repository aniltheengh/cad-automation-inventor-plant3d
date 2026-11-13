# 26 — Elasticity and finite-element analysis: the transition

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
Simple formulas are valuable because their assumptions are visible. Finite-element analysis divides a structure into connected elements and approximates displacement. More elements do not automatically fix incorrect supports, loads or material behavior.

## Governing relationships
Small-strain elasticity combines:

```math
\nabla\cdot\boldsymbol\sigma+\mathbf b=0,\qquad
\boldsymbol\epsilon=\tfrac12[\nabla\mathbf u+(\nabla\mathbf u)^{T}],\qquad
\boldsymbol\sigma=\mathbf C:\boldsymbol\epsilon.
```

$`\boldsymbol\sigma=\mathbf C:\boldsymbol\epsilon`$ here assumes zero initial/thermal strain. With uniform isotropic thermal strain, use $`\boldsymbol\sigma=\mathbf C:(\boldsymbol\epsilon-\alpha\Delta T\mathbf I)`$. $`\mathbf I`$ is the identity tensor, and $`\mathbf b`$ is body force per volume, $`\mathbf u`$ displacement and $`\mathbf C`$ the constitutive tensor. Prescribed displacements and surface tractions complete the boundary-value problem.

A two-node axial bar element has

```math
\mathbf k_e=\frac{EA}{L}
\begin{bmatrix}1&-1\\-1&1\end{bmatrix},\qquad
\mathbf K\mathbf u=\mathbf F
```

after assembly and appropriate boundary conditions. Element strain is $`(u_2-u_1)/L`$. Without enough constraints, rigid-body motion makes the stiffness system singular.

## Worked example
One bar has $`E=200000`$ MPa, $`A=100`$ mm² and $`L=1000`$ mm. Fix node 1 and apply 10000 N at node 2. The reduced stiffness is 20000 N/mm, so $`u_2=0.5`$ mm. Strain is 0.0005, stress is 100 MPa and the support reaction balances −10000 N.

## Practice and answer
If the area doubles at the same load, **answer:** displacement and stress both halve. Confirm that result with the chapter 4 formula before trusting a mesh model.

## Intermediate checks
Check units, reaction equilibrium, deformed shape, material assignment and energy. Refine the mesh and compare a chosen output rather than only looking at stress colors. Point loads, sharp re-entrant corners and ideal restraints can produce nonconvergent peak stresses; choose physically meaningful evaluation locations.

## Scope
This chapter introduces the bridge to elasticity/FEA. Full plate/shell theory, nonlinear contact, large deformation, fracture simulation and advanced anisotropic failure require further study. The two-course chapter sequence is a broad learning syllabus, not a promise to cover every specialist formula.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 25: Composite beams and introductory anisotropy](25-composite-beams-materials.md) · [Formula reference →](../formula-reference.md)
