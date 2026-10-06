# PRISM brand presentation

**6 October 2026 update:** the user supplied an illustration exclusively for app icon/store art. [Icon integration](app-icon-integration.md) documents its source and exports. The old optical fold below is historical: [UI polish](ui-polish-review.md) replaces its in-game use with an independent Canvas-native prism motif; the supplied illustration never appears as the in-game logo.

Implemented from `PrisM_Marka_UIUX_Yeniden_Tasarim_Spesifikasyonu.docx` (4 October 2026), especially sections 3, 13, 21 and 22. **PRISM is the temporary public name.** No final naming or clearance decision is made; Rayfold and Lightfold remain excluded from product branding. Namespace, save identifiers and package identity remain independent of the public name.

## Optical fold

The original symbol is a thick ivory incoming beam, two angular changes of direction and one continuous spectrum wedge. The fold remains recognizable as a white silhouette. The background is `#090812`; glass triangles, outlines and seven fine rainbow rays have been removed. The spectrum appears in the brand glyph rather than ordinary controls.

`BrandAssets.cs` is the code-native source. `PRISM > Release > Generate Store Assets` regenerates the 512 px Play icon, 1024 × 500 feature graphic, runtime glyph, adaptive monochrome layer and small-size exports. `PrepareAndroidBranding()` generates legacy and adaptive layers and assigns Android slots. The adaptive art uses a 0.64 scale within the central mask-safe area; its optional third layer is monochrome.

Concrete outputs:

- `Assets/Prism/Generated/AppIcon.png` (1024 × 1024)
- `Assets/Prism/Generated/AdaptiveForeground.png`, `AdaptiveBackground.png`, `MonochromeIcon.png`
- `Assets/Prism/Resources/Brand/OpticalGlyph.png` (transparent 512 × 512 sprite)
- `Builds/StoreAssets/play-icon-512.png` and `feature-graphic-1024x500.png`
- `Builds/StoreAssets/icon-{32,48,64,128,192}.png` and monochrome counterparts
- `Builds/StoreAssets/brand-review.png` (actual-size glyph review sheet)

The feature graphic expands the same optical fold, preserving the brand's beam and wedge language. It intentionally makes no claim that a new final game name has been chosen.

## Typography and provenance

Sora Regular and SemiBold are unmodified static TTFs from the [official Sora project](https://github.com/sora-xor/sora-font), pinned to revision `7f9a9c5d0ccd1c099cfac420aa27133df1c5fdc4`. Source URLs:

- `https://raw.githubusercontent.com/sora-xor/sora-font/7f9a9c5d0ccd1c099cfac420aa27133df1c5fdc4/fonts/ttf/Sora-Regular.ttf`
- `https://raw.githubusercontent.com/sora-xor/sora-font/7f9a9c5d0ccd1c099cfac420aa27133df1c5fdc4/fonts/ttf/Sora-SemiBold.ttf`
- `https://raw.githubusercontent.com/sora-xor/sora-font/7f9a9c5d0ccd1c099cfac420aa27133df1c5fdc4/OFL.txt`

Copyright 2019 The Sora Project Authors. The complete SIL Open Font License 1.1 accompanies the fonts as `Assets/Prism/Resources/Brand/Fonts/OFL.txt`.

SHA-256:

| Font | Hash |
| --- | --- |
| Sora Regular | `517e945dedbeeb8d700ccae77d189a6ef2a01f6dcc95ba5d032ef9a30f7f0de9` |
| Sora SemiBold | `b02621d4009da9a18eb8a5cbe277f084eb9c3f065ba08a6e77769e81863becf2` |

The Unicode cmap was parsed directly for both fonts: **Ç Ğ İ Ö Ş Ü ç ğ ı ö ş ü are present**, with no missing glyphs. The machine-readable result and hashes are in `glyph-validation.json` beside the font files.

## Unity presentation resources

`PrismTheme` exposes `Font(bool bold = false)`, `Glyph`, `Background`, `Surface`, `Ivory`, `Muted`, `Accent` and `Success`. Font assets are cached dynamic TextMeshPro SDF atlases (64 pt sampling, 8 px padding, 1024 px atlas, multiple atlas support). Both source font data and the mobile SDF shader are included in Resources so runtime font construction works in player builds. The text alphabet, digits, Turkish letters and common UI punctuation are warmed on creation; additional characters are added dynamically.

`TMP_SDF-Mobile.shader` and its includes are unmodified extracts from Unity 6000.6.0f1's bundled `com.unity.ugui/Package Resources/TMP Essential Resources.unitypackage`. The corresponding Unity Companion License notice is in `Shaders/UNITY-LICENSE.md`. `TMP Settings.asset` uses that same bundled template, with references to optional sample fonts, sprites, styles and line-breaking assets cleared. The first generated Sora asset becomes the TMP default font at runtime.

`BrandImporter` applies only to these brand resources and generated icon textures: sprite import for the runtime glyph, uncompressed/no-mipmap/clamped textures, and included dynamic font data for Sora. Existing unrelated fonts and textures are not affected.

## Validation

The concrete color and monochrome PNGs were visually inspected at 32, 48, 64, 128 and 192 px. The two-turn white beam and triangular exit remain legible at 32–48 px. The icons and feature graphic have the expected pixel dimensions. Both Sora font cmap checks pass Turkish coverage. The integration pass compiled the Canvas/TMP shaders and rasterized Sora in the Windows player. Android branding forces Texture2D import shape so the monochrome glyph cannot be auto-imported as a cubemap. Physical Android launcher masks remain a device QA item.

![Color and monochrome review](../Builds/StoreAssets/brand-review.png)
