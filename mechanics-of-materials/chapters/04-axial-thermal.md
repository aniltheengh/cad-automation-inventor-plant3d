# 04 — Axial deformation, self-weight and temperature

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
A longer rod stretches more; a larger area or modulus reduces its extension. Temperature adds strain even without an applied force. Restraint converts otherwise free expansion into stress.

## Equations and derivation
Combining $\sigma=N/A$, $\epsilon=\sigma/E$ and $d\delta=\epsilon\,dx$ gives
$$
\delta=\int_0^L\frac{N(x)}{E(x)A(x)}\,dx,\qquad
\delta=\sum_i\frac{N_iL_i}{E_iA_i}
$$
Use the sum for piecewise-constant segments. Thermal strain and free movement are
$$
\epsilon_T=\alpha\Delta T,\quad
\delta_T=\alpha L\Delta T,\quad
\delta_{\rm total}=\delta_{\rm mechanical}+\delta_T.
$$
For uniform heating of a completely restrained, uniform elastic bar, $\sigma=-E\alpha\Delta T$ with tension positive. A free bar has zero thermal stress.

A uniform vertical hanging bar under its own weight has maximum stress $\rho gL$ and extension $\rho gL^2/(2E)$. The internal force decreases toward the bottom; using the full weight over the whole length doubles the extension incorrectly.

## Worked example
For $N=20$ kN, $L=1000$ mm, $A=200$ mm² and $E=200000$ MPa, mechanical extension is 0.5 mm. If $\alpha=12\times10^{-6}$/K and $\Delta T=40$ K, free thermal extension is 0.48 mm. With the same applied load and free thermal movement, total extension is 0.98 mm.

## Practice and answer
The same material is fully restrained and heated 40 K without that applied force. **Answer:** stress is −96 MPa. Check it remains in the elastic range.

## Transition
For varying temperature use $\int\alpha(x)\Delta T(x)\,dx$. Gaps and yielding change the active constraints; determine whether contact occurs before imposing full restraint.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 3: Material behavior and elastic constants](03-material-behavior.md) · [5: Compatibility and statically indeterminate bars →](05-compatibility-composite-bars.md)
