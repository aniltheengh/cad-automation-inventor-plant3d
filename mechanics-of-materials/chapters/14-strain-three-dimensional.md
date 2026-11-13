# 14 — Strain transformation, rosettes and 3D elasticity

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
A strain gauge measures extension along its own axis. Several directions let you reconstruct the in-plane strain state. Engineering shear strain $`\gamma_{xy}`$ is twice tensor shear strain $`\epsilon_{xy}`$; missing this factor is a common error.

## Equations

```math
\epsilon_\theta=\frac{\epsilon_x+\epsilon_y}{2}
+\frac{\epsilon_x-\epsilon_y}{2}\cos2\theta
+\frac{\gamma_{xy}}2\sin2\theta.
```

For a rectangular 0°/45°/90° rosette:

```math
\epsilon_x=\epsilon_0,\quad \epsilon_y=\epsilon_{90},\quad
\gamma_{xy}=2\epsilon_{45}-\epsilon_0-\epsilon_{90}.
```

Principal in-plane strains are $`(\epsilon_x+\epsilon_y)/2\pm\sqrt{[(\epsilon_x-\epsilon_y)/2]^2+(\gamma_{xy}/2)^2}`$.

For isotropic linear elasticity with uniform temperature change:

```math
\epsilon_x=\frac{\sigma_x-\nu(\sigma_y+\sigma_z)}E+\alpha\Delta T,
\quad \gamma_{xy}=\frac{\tau_{xy}}G
```

with cyclic versions for y/z. Under plane stress and no thermal strain:

```math
\sigma_x=\frac{E}{1-\nu^2}(\epsilon_x+\nu\epsilon_y),\quad
\sigma_y=\frac{E}{1-\nu^2}(\epsilon_y+\nu\epsilon_x).
```

Plane strain imposes $`\epsilon_z=\gamma_{xz}=\gamma_{yz}=0`$; it does **not** impose $`\sigma_z=0`$. Without thermal effects, $`\sigma_z=\nu(\sigma_x+\sigma_y)`$.

## Worked example
Gauge readings are 500, 300 and −100 microstrain at 0°,45°,90°. Then $`\gamma_{xy}=200`$ microstrain. For $`E=200000`$ MPa, $`\nu=0.30`$, plane stress gives $`\sigma_x=103.297`$ MPa, $`\sigma_y=10.989`$ MPa and $`\tau_{xy}=15.385`$ MPa.

## Practice and answer
With zero thermal change, $`\sigma_x=100`$ MPa, $`\sigma_y=0`$ and plane strain at $`\nu=0.3`$, **answer:** $`\sigma_z=30`$ MPa.

## Transition
For a general 3D stress tensor, principal stresses are the eigenvalues of its symmetric matrix. Check tensor shear symmetry and order all three principal values before evaluating a failure criterion.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 13: Plane stress, principal stresses and Mohr's circle](13-stress-transformation.md) · [15: Combined loading and static failure criteria →](15-combined-failure.md)
