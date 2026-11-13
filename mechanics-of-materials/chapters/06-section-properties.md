# 06 — Centroids and second moments of area

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Readable formula book:** [PDF](../formula-book/Mechanics_of_Materials_Formula_Book.pdf) · [Offline HTML](../formula-book/Mechanics_of_Materials_Formula_Book.html)

**Course:** Mechanics of Materials I — foundations.

## Physical meaning
Where material sits relative to a bending axis matters. Moving material away from that axis increases bending stiffness. The second moment of area $`I`$ is a geometric quantity, not a mass moment of inertia.

## Equations

```math
\bar y=\frac{\sum A_i y_i}{\sum A_i},\quad
I_x=\int y^2\,dA,\quad I_y=\int x^2\,dA,\quad
I_{xy}=\int xy\,dA
```

For an axis parallel to a centroidal axis, $`I=I_c+Ad^2`$. Treat holes as negative areas with their own centroid locations.

| Shape and centroidal axis | Area | Second moment |
|---|---|---|
| Rectangle, width $`b`$, vertical depth $`h`$ | $`bh`$ | $`I_x=bh^3/12`$ |
| Solid circle, diameter $`d`$ | $`\pi d^2/4`$ | $`I_x=I_y=\pi d^4/64`$ |
| Concentric annulus, outer $`D`$, inner $`d`$ | $`\pi(D^2-d^2)/4`$ | $`\pi(D^4-d^4)/64`$ |

The polar area moment is $`J_p=I_x+I_y`$. For circular shafts this equals the torsion constant, but that equality does not hold for arbitrary shapes. Section modulus is $`Z=I/c`$, with extreme-fiber distance $`c`$. Radius of gyration is $`r_g=\sqrt{I/A}`$.

## Worked example
A rectangle has $`b=40`$ mm and $`h=80`$ mm. $`I_x=1.706667\times10^6`$ mm⁴ and $`Z_x=42666.7`$ mm³. Rotating it through 90° gives $`I_x=426666.7`$ mm⁴, one quarter as large.

## Practice and answer
Find the centroidal $`I`$ of a 20 mm solid circle. **Answer:** 7853.982 mm⁴. Its polar moment is 15707.963 mm⁴.

## Transition
Always sketch the actual bending axis. A catalog's strong-axis property cannot be used for loading about the weak axis. For composite shapes find the centroid first, then apply the parallel-axis theorem.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 5: Compatibility and statically indeterminate bars](05-compatibility-composite-bars.md) · [7: Torsion of circular shafts and power transmission →](07-circular-torsion.md)
