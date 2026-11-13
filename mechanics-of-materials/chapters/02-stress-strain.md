# 02 — Stress, strain and connection stresses

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
Stress measures internal force per area. Normal stress pulls or pushes perpendicular to a section; shear stress acts along it. Strain measures relative deformation, so it is dimensionless. Small elastic strain can matter even when movement is hard to see.

## Equations
For a centric force and an approximately uniform section stress:

```math
\sigma_{\rm avg}=\frac{N}{A},\quad
\tau_{\rm avg}=\frac{V}{A_s},\quad
\epsilon=\frac{\delta}{L},\quad
\gamma\approx\frac{\Delta u}{h}
```

Here $`A_s`$ is the actual shear area, $`\delta`$ the extension, and $`\gamma`$ the small shear angle in radians. For $`m`$ equally loaded shear planes in a pin, $`\tau_{\rm avg}=P/(mA_{\rm pin})`$. Average plate bearing stress is $`\sigma_b=P/(td)`$, with plate thickness $`t`$ and pin diameter $`d`$.

For a plate of width $`b`$ with a hole diameter $`d`$, the idealized net tension area is $`(b-d)t`$. This gives average net stress, not the peak stress at the hole. Choose the area required by the failure mechanism.

## Worked example
A 20 mm diameter rod carries 30 kN tension. Its area is $`\pi20^2/4=314.159`$ mm², giving $`\sigma=95.493`$ MPa. If a 500 mm gauge length extends 0.25 mm, $`\epsilon=0.0005=500`$ microstrain.

## Practice and answer
A 10 mm pin carries 8 kN in ideal double shear. **Answer:** each shear plane carries 4 kN and $`\tau_{\rm avg}=50.930`$ MPa.

## Transition and limitations
Actual connection stresses are nonuniform. Bearing, net-section tension, shear-out, pin bending and joint geometry can all matter. The average equations are first models, not complete joint design rules.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 1: Units, forces and equilibrium](01-units-statics.md) · [3: Material behavior and elastic constants →](03-material-behavior.md)
