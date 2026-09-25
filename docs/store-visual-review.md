# Store visual review

Generate the two listing images with `PrisM/Release/Generate Store Assets` in the Unity Editor, or with `Tools/Generate-Store-Assets.ps1`. The output is under `Builds/StoreAssets`.

| Asset | Expected format | Visual check |
| --- | --- | --- |
| `play-icon-512.png` | 512 × 512, 32-bit PNG with alpha, at most 1024 KB | The full square contains the dark teal background. The beam, prism, and seven spectral rays remain legible at small sizes; no corner radius or outer shadow is baked in. |
| `feature-graphic-1024x500.png` | 1024 × 500, 24-bit PNG without alpha | The prism is central, with the incoming beam and seven output rays visible. Check a narrow crop and a preview-video overlay because Play may crop or cover the edges. |

The Android build separately generates `Assets/Prism/Generated/AppIcon.png` and two adaptive launcher layers. Check the adaptive icon on an Android 8+ device under round and squircle launcher masks: the foreground artwork should remain within [Android's central 66/108 safe area](https://developer.android.com/develop/ui/compose/system/icon_design_adaptive) while the background fills the mask. Check the icon again at launcher thumbnail scale.

Suggested Play Console alt text:

- Icon: “A white beam passes through a glass prism and becomes seven colored rays on a dark teal background.”
- Feature graphic: “A bright beam enters a triangular prism and splits into a rainbow of seven rays across a dark teal field.”

Before upload, inspect the generated PNG files and confirm their actual dimensions, color mode, and file size. Google Play currently requires the [listing icon](https://support.google.com/googleplay/android-developer/answer/9866151) to be 512 × 512, 32-bit PNG with alpha and no more than 1024 KB; the [feature graphic](https://support.google.com/googleplay/android-developer/answer/9866151) must be 1024 × 500 in JPEG or 24-bit PNG without alpha. Google Play applies the [icon corner mask and shadow](https://developer.android.com/distribute/google-play/resources/icon-design-specifications) after upload.

For gameplay screenshots, capture the current build directly on a phone. Include a level with a clear seven-band prism split and a more complex optical chain, and check that Turkish labels, targets, and beams remain readable at Play Store thumbnail size. Do not substitute the generated illustration for gameplay footage.
