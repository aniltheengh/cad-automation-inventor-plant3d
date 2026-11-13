# 23 — Fatigue, fracture, creep and stress concentrations

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
Repeated loading can crack a part below its static yield strength. An existing crack can grow unstably. Sustained stress at elevated temperature can cause time-dependent deformation. These are different mechanisms and require appropriate material data.

## Equations and limits
$$
\sigma_a=\frac{\sigma_{\max}-\sigma_{\min}}2,\quad
\sigma_m=\frac{\sigma_{\max}+\sigma_{\min}}2,\quad
R_\sigma=\frac{\sigma_{\min}}{\sigma_{\max}}.
$$
A simple Goodman screening relation for tensile mean stress is
$\sigma_a/S_e+\sigma_m/S_{ut}\le1/n$; $S_e$ must be appropriate to the component and target life, not an assumed universal value. Some materials have no true endurance limit. A simple stress-life fit is $\sigma_a=C N_f^b$; its fitted cycle definition and range must be stated.

Miner's approximate damage sum is $\mathcal D=\sum n_i/N_i$; $\mathcal D=1$ is an idealized failure estimate, not an exact rule. Elastic concentration $K_t=\sigma_{\rm local}/\sigma_{\rm nominal}$ differs from fatigue factor $K_f=1+q_n(K_t-1)$, with notch sensitivity $q_n$.

For linear-elastic mode-I fracture:
$$
K_I=Y\sigma\sqrt{\pi a},\quad
a_c=\frac1\pi\left(\frac{K_{IC}}{Y\sigma}\right)^2.
$$
$Y$ is geometry-dependent; $a$ must use the corresponding crack definition. Paris growth is $da/dN=C_P(\Delta K)^m$ in its fitted mid-growth regime. A possible steady creep fit is $\dot\epsilon=A_c\sigma^n\exp[-Q_c/(RT)]$, with temperature in kelvin.

## Worked example
With $\sigma_{\max}=120$ MPa and $\sigma_{\min}=20$ MPa, amplitude is 50 MPa and mean is 70 MPa. For assumed component data $S_e=200$ MPa and $S_{ut}=500$ MPa, Goodman utilization is $0.25+0.14=0.39$, giving $n=2.564$ in that model.

## Practice and answer
With $Y=1$, $\sigma=100$ MPa and $K_{IC}=50$ MPa√m, **answer:** ideal $a_c=0.07958$ m. The assumed geometry and small-scale yielding must be checked before accepting it.

## Transition
Do not confuse stress intensity $K_I$ with concentration $K_t$. Static formulas cannot establish fatigue life without load history and suitable material/environment data.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 22: Noncircular and thin-walled torsion](22-noncircular-torsion.md) · [24: Plastic bending, residual stress and limit analysis →](24-plastic-bending.md)
