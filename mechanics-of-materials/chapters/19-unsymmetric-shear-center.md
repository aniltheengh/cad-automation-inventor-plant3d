# 19 — Unsymmetric bending, eccentric loads and shear center

[Section home](../README.md) · [Formula reference](../formula-reference.md) · [Symbols](../symbols-and-units.md)

**Course:** Mechanics of Materials II — intermediate applications.

## Physical meaning
An unsymmetric section may bend in a direction different from the applied load. Use centroidal principal axes to simplify bending. A transverse load applied away from the shear center can cause twist even if it passes through the centroid.

## Equations
For centroidal coordinates y,z define $I_y=\int z^2dA$, $I_z=\int y^2dA$, $I_{yz}=\int yz\,dA$. Principal second moments are
$$
I_{1,2}=\frac{I_y+I_z}{2}\pm
\sqrt{\left(\frac{I_y-I_z}{2}\right)^2+I_{yz}^2}.
$$
On principal axes $u,v$, define moment resultants by
$M_u=\int v\sigma_xdA$ and $M_v=-\int u\sigma_xdA$. Then
$$
\sigma_x=\frac NA+\frac{M_uv}{I_u}-\frac{M_vu}{I_v}.
$$
These explicit resultant definitions prevent ambiguity in signs. Under eccentric axial force, find the applied moment from $\mathbf r\times\mathbf F$.

For a rectangular compression section of depth $h$ under single-axis eccentricity $e$, no tensile normal stress occurs in the simple elastic model if $|e|\le h/6$. This middle-third rule is not a buckling check.

## Worked example
A 100 × 200 mm rectangular section carries 100 kN compression at $e=20$ mm along the depth. Direct compressive stress is 5 MPa. Bending stress magnitude is $Pe/Z=3$ MPa. Edge stresses are −8 and −2 MPa, both compressive. Since $20<200/6$, the result agrees with the middle-third rule.

## Practice and answer
Increase eccentricity to 50 mm. **Answer:** edge stresses become −12.5 and +2.5 MPa.

## Transition
For thin-walled open sections, integrate shear flows and balance their moment to locate the shear center. A doubly symmetric section has its shear center at the symmetry intersection; a channel generally does not have it at its centroid.

## Further reading

Use the textbook/topic map in [Books and sources](../books/README.md). The notes and worked values on this page are a concise study companion; the books supply longer derivations and additional problems.

---

[← 18: Column buckling and eccentric compression](18-columns-buckling.md) · [20: Curved beams, hooks and rings →](20-curved-beams.md)
