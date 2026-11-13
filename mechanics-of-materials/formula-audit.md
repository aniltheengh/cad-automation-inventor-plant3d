# Formula review — 13 November 2026

This review covers the 26 Mechanics of Materials I–II chapter notes, symbols, consolidated formula reference and the generated formula book. It does not claim independent engineering certification or cover every possible engineering formula.

## Corrections and clarifications

- Replaced display delimiters with GitHub math fences and protected inline mathematical expressions for reliable repository rendering.
- Added a self-contained HTML book and a vector-equation PDF. Each topic has its own page, symbol guide, worked check and applicability notes.
- Added missing reference relations for engineering measures, vessel strains, principal strains, energy, column factors, cylinder displacement, fatigue, plastic work, composites and thermal elasticity.
- Distinguished uniaxial stress–strain/energy relations from general multiaxial behavior; distinguished mechanical strain from total strain in thermal problems.
- Distinguished in-plane principal stresses from the three sorted principal stresses used for failure assessment.
- Clarified the Johnson/Euler transition and the compression/eccentricity assumptions for the secant formula.
- Distinguished pressure-closed cylinder ends, specified axial stress and zero axial strain. Clarified radial displacement notation and separated it from strain-energy density.
- Corrected the tensor-gradient transpose notation and included thermal strain in the elasticity relation.
- Stated that the explicit critical crack-size equation assumes constant geometry factor; otherwise solve implicitly.

## Verification

- The build parsed 975 mathematical expressions across book and chapter sources with no MathJax errors. Repeated expressions are included in that count.
- The book contains 151 displayed equation blocks and 259 inline expressions, rendered as embedded vector graphics with no external font or math service dependency.
- All 30 PDF pages were rendered and visually inspected; the revision removes sparse continuation pages and orphan links.
- 45 numerical regression checks passed. They cover examples across the 26 topics plus stress invariants, hydrostatic/pure-shear failure cases, the Johnson/Euler junction, curved-beam resultants, cylinder pressure boundaries/end balance and the finite-element support reaction. Run the included checker for the current result.
- Local Markdown links and whitespace are checked before publication. External reading links remain subject to change.

## Reading formats

- [Formula book PDF](formula-book/Mechanics_of_Materials_Formula_Book.pdf)
- [Offline HTML](formula-book/Mechanics_of_Materials_Formula_Book.html) — download and open in a browser; GitHub displays HTML source.
- [Editable formula reference](formula-reference.md)
- [Build and check instructions](tools/README.md)

The MIT textbook in the books folder remains a separate publication with its own attribution and licence. These concise sheets supplement the chapters and textbook; they do not replace the full derivations or design standards.
