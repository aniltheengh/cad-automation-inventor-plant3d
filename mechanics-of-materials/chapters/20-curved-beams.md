# 20 — Curved beams, hooks and rings

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
In a beam curved before loading, equal radial distances do not imply equal strain changes. The neutral axis shifts away from the centroid and stress varies nonlinearly. A straight-beam $Mc/I$ estimate can underpredict the inner-fiber stress of a tightly curved hook.

## Equations
For a homogeneous curved beam in pure bending, measure radius $r$ from the center of curvature:
$$
r_n=\frac{A}{\int_A dA/r},\quad
r_c=\frac{\int_A r\,dA}{A},\quad e=r_c-r_n.
$$
Define positive bending moment as producing inner-fiber tension. Then
$$
\sigma_\theta(r)=\frac{M}{Ae}\left(\frac{r_n}{r}-1\right).
$$
With this convention $M=-\int_A(r-r_c)\sigma_\theta\,dA$. A centric section force can add $N/A$ with tension positive. For constant width $b$ between inner $r_i$ and outer $r_o$:
$$
A=b(r_o-r_i),\quad
r_c=\frac{r_i+r_o}{2},\quad
r_n=\frac{r_o-r_i}{\ln(r_o/r_i)}.
$$

## Worked example
Let $b=20$ mm, $r_i=40$ mm, $r_o=80$ mm and $M=100000$ N·mm, with no direct force. Then $A=800$ mm², $r_c=60$ mm, $r_n=57.708$ mm and $e=2.292$ mm. Inner stress is about +24.141 MPa and outer stress −15.196 MPa.

## Practice and answer
Where is the zero-stress radius? **Answer:** $r=r_n$, not $r_c$.

## Transition and limitations
This is curved-beam theory, not a complete hook design. Loads near contact zones, transverse shear, ring constraints and three-dimensional effects require further analysis. For a hook, calculate both direct force and bending at the cut. Avoid subtractive roundoff when $r_c$ and $r_n$ nearly coincide in a very shallow curved section.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 19: Unsymmetric bending, eccentric loads and shear center](19-unsymmetric-shear-center.md) · [21: Thick cylinders and compound-cylinder ideas →](21-thick-cylinders.md)
