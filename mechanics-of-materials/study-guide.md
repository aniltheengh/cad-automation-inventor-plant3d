# Study guide — from beginner to intermediate

[Section home](README.md) · [Formula reference](formula-reference.md)

## Beginner: learn one physical idea at a time

You need arithmetic, rearranging equations and basic geometry to start. Review a topic only when a chapter needs it. Trigonometry helps with inclined forces; derivatives/integrals explain changing loads and deformation. Chapter 1 comes before stress calculations.

Use this six-line problem record:

1. **Given:** loads, dimensions and material properties with units.
2. **Find:** the requested stress, deformation, reaction or failure estimate.
3. **Sketch:** free-body diagram and coordinate axes.
4. **Model:** assumptions and why the formula applies.
5. **Calculate:** symbolic equation first, then numbers with units.
6. **Check:** equilibrium, dimensions, signs and expected behavior.

For chapters 1–6, focus on understanding force versus stress, movement versus strain, and stiffness versus strength. Do not try to memorize all equations at once.

## Transition: explain where the equation comes from

Connect three ingredients: **equilibrium** (forces balance), **compatibility** (the movements fit the supports and connected parts), and **material law** (stress relates to strain).

Example: an axial bar gives stress N/A from equilibrium, strain stress/E from the material law, and extension strain × length from geometry. Together these give NL/(EA). For redundant structures, the same ingredients are used in a different order.

In chapters 7–12, draw torque/shear/moment diagrams before computing stress. State the bending axis and distinguish a strength check from a deflection check.

## Intermediate: choose and verify a model

Chapters 13–26 combine loads, directions, instability, time dependence and more complex geometry. Start each problem by deciding which mechanism is relevant. A yield calculation does not replace a buckling check, and a fatigue formula requires fatigue data.

Your checkpoint is being able to explain both **why an equation applies** and **when it stops applying**.

## Before moving from Course I to Course II

You should be able to:

- Find reactions and internal forces from a free-body diagram.
- Convert consistently between N–m–Pa and N–mm–MPa.
- Calculate axial extension, circular-shaft twist and elementary beam stress.
- Identify the correct centroid/second moment and construct a shear/moment diagram.
- Calculate a beam deflection and explain the support conditions used.
- Recognize when compatibility is needed.

## Formula selection by question

| Question | Start with |
|---|---|
| What internal force does the section carry? | Chapter 1 or 8 |
| How much does a bar extend? | 4–5 |
| How much does a shaft twist? | 7 or 22 |
| Where are beam stresses largest? | 8–10 |
| How far does a beam move? | 11, 16–17 |
| What happens under multiple loads? | 13–15, 19 |
| Will a compressed member buckle? | 18 |
| Is the wall or initial curvature important? | 12, 20–21 |
| Will repeated loading or cracks matter? | 23 |
| Has yielding spread across the section? | 24 |
| Are multiple materials bonded together? | 5 or 25 |
| How do I benchmark FEA? | 26, plus the closest simple model |

## How to use the attached textbook

Use these chapter notes as a route map, then read the corresponding full textbook module listed in [Books and sources](books/README.md). The textbook's module page numbers restart, so use its PDF search for a topic title. The Virginia Tech online book is an additional introductory companion.

Each repository chapter contains a worked example and a separate practice question/answer. Cover the answers while solving. If you are stuck, check the sketch and units before searching for a different formula.
