# Phase 2 — Layered HDR lighting

This continuation is based on the current `main` commit after the September 15 rendering/build updates.

## Rendering layers

- `PrismBoard`: one full-board quad with procedural dot grid and vignette.
- `PrismGeometry`: normal alpha-blended non-emissive walls, outlines, controls and object accents.
- `PrismWater`: lightweight animated procedural water/caustic appearance.
- `PrismBeam`: additive HDR beam/core/glow rendering driven by solver beam power.
- `PrismGlass`: transparent optical surfaces for prism, lens and sphere.

The existing `PrismUnlit` shader on `main` is intentionally left unchanged. It remains additive as authored in the latest main change; normal geometry uses the separate `PrismGeometry` shader so non-light objects do not become emissive.

## Performance direction

The old CPU-generated 19 × 19 dot grid is removed from `BoardRenderer`. The grid is now evaluated in the board shader from a single quad. Beam/glass/water/base geometry are separate meshes so expensive visual passes can later be quality-scaled independently.

## Beam intensity

`Optics` emits 13 source-width samples. Each sample starts with `1/13` power. The renderer therefore normalizes `beam.Power * 13` before applying HDR glow so bounce attenuation remains visible while a fresh beam still reaches bloom intensity.

## Runtime post-processing

`VisualEnvironment` enables HDR, URP post-processing and a runtime global volume with mobile-conscious Bloom, ACES tonemapping and color adjustments. High-quality Bloom filtering stays disabled.

## Unity validation checklist

1. Open the project in Unity `6000.6.0f1` and confirm all five shaders compile without errors.
2. Verify Level 1 mirror beam remains crisp and not over-bloomed.
3. Verify Level 2 prism keeps seven distinguishable spectral bands.
4. Verify Level 4 lens glass remains readable against the board.
5. Verify Level 7 water animation is subtle and the sphere edge remains visible.
6. Verify Level 8 does not wash spectral colors to white when several beams overlap.
7. Run the existing Windows build/smoke scripts from the real gameplay scene.
8. Profile a real Android ARM64 build before increasing bloom, glass or water quality.
