# Spatial puzzle revision — 2026-10-06

Campaign revision: `2026-10-06-spatial-optics-v2`. The 100 legacy IDs remain unchanged, preserving progress keys. The baked catalog stores wall thickness, purpose and cluster alongside endpoints. Missing/zero legacy thickness resolves to 0.36 world units. Both authoring import buttons stamp the current revision.

## Shared physical geometry

`Wall(A, B, thickness = .36, purpose = "", cluster = 0)` is an oriented rectangle with flat end caps at A and B. `WallGeometry.Vertices(wall)` returns four perimeter corners: A+normal, A-normal, B-normal, B+normal. `Thickness(wall)` applies the legacy default. `Distance(point, wall)` measures distance to the solid rectangle, including flat end corners. Placement adds each optic's clearance outside that face. `Raycast(origin, unitDirection, wall, out distance, out normal)` intersects the nearest physical face/end cap without managed allocation; `Blocks(wall, a, b)` checks a finite segment. The solver and renderer consume this shared geometry.

## Curriculum grammar

The introductory 80 levels retain their optical lessons. Existing seed baffles are validated; new long 1.6–3.2 unit occluders must block a concrete source/optic-to-receiver shortcut while preserving a valid, placeable known solution. The generator no longer scores decorative bars by distance from solved beams. It removes any wall lacking a counterfactual shortcut witness. Wall count is a ceiling, not a quota: filling unused space is intentionally not a progression metric.

Levels 81–100 author the room constraints first: three full-board vertical gates with alternating opening heights, followed by a fourth exit gate in 91–100. Sources and optics are then instantiated into that channel. Gate opening sizes account for oblique rays passing through the entire finite wall thickness. They span 6 or 8 wall segments, grouped into 3 or 4 clusters.

The final twenty combine three mirrors, a green/red selector, a necessary lens/sphere, normal-incidence water and two colored receivers. Four spectral chamber variants instead combine a red split at the entrance, four reflections, a prism at the exit and three spectral receivers. Broad focusing variants use a 0.4 source width and a small receiver; removing the lens/sphere fails receiver energy even without the all-active completion rule. Spectral chambers use width 0.08 and two small separated prism-band receivers. Every optic must participate. Alternating heights, selector band, focus optic, spectral chamber and room symmetry vary independently; this is no longer difficulty measured only by mirror count.

Water in these rooms is intentionally normal-incidence reinforcement; it does not independently require angular refraction. Chapters 6 and 8 retain the angled refraction and compound spectral lessons. This revision does not claim an exhaustive search proving a unique solution or globally minimal par.

## Validation and runtime

`SpatialValidation.TryWitness` returns an actual finite segment freed by removing the selected wall/cluster. Occluders use source/optic/receiver shortcuts. Gates additionally require a solved beam crossing their opening and two collinear barriers spanning the board; a blocked transverse route through a closed part proves the pair's constraint. Witness validity is checked geometrically, rather than accepting a purpose label. This detects decorative isolated bars; it does not prove every wall is individually indispensable to every possible solution.

The core test harness checks physical face hits, diagonal walls, flat end caps, rectangle corner distance, thickness-aware placement, zero raycast allocation, all 100 known solutions and placement/gameplay rules, wall removal witnesses, mixed late inventories and ablation of every late optic (without the inventory completion rule). Authoring catalog validation also checks placement, witnesses and optical solutions. Runtime checks structural catalog integrity and revision, then loads baked definitions without rerunning 100 optical solutions or procedural authoring. Development fallback generation remains available when the resource is absent or mismatched.

Measured on the desktop .NET harness: generation ~724 ms; chapter 8 mean optical solve ~2.02 ms. Meaningful segment totals by chapter: 9, 10, 10, 0, 10, 10, 20, 30, 60, 80. Chapter 4 remains an open focus lesson rather than adding nonfunctional geometry. These figures are not Android benchmarks.
