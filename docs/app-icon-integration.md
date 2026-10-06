# Supplied app icon and store art

The player supplied `artifacts/logo.png` on 6 October 2026, then explicitly restricted its use to **app icon and store art**. The original 1254×1254 PNG is preserved unchanged (SHA256 `A92478AFA1D80DCDDDECA3995289763D186F0C2770E14A8073F16F170554609A`). No ownership or third-party licensing claim is inferred from the file.

`BrandAssets.cs` reads this source and produces:

- 1024×1024 Android legacy icon, with platform slots assigned in the signed build;
- 512×512 Play listing icon and 32/48/64/128/192 px review exports;
- adaptive foreground at 0.74 scale on a dark background, keeping prism and receiver inside the Android safe circle;
- code-native white prism/beam/receiver silhouette for the monochrome themed icon;
- 1024×500 feature graphic with the square source centered at its original aspect ratio.

Generation validates the PNG signature, decoder result and square aspect. Missing or invalid source aborts generation explicitly. The full-color illustration is sampled without crop, hue changes or an overlay wordmark. The Play icon's encoded size is checked against 1024 KB.

**In-game logo is separate:** the generator does not write `Assets/Prism/Resources/Brand/OpticalGlyph.png`. Home, intro and campaign finale use a separate Canvas-native prism/ray/spectrum motif described in [UI review](ui-polish-review.md). The full-color supplied PNG is not included in runtime Resources; it lives in the repository only as a build-time source.

The Windows release build refreshes store outputs before building; Android preparation assigns legacy/adaptive icon slots. CI/core content and save identifiers are unaffected. Real launcher/OEM mask appearance remains part of physical-device QA.
