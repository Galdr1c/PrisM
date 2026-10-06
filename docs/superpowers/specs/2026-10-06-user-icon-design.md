# User-supplied PRISM icon

Use the player's supplied `artifacts/logo.png` exclusively as full-color app icon and store art. Preserve its square composition and original file. Replace the optical-fold artwork in full-color generated launcher/store assets. Keep the existing in-game logo, package ID and public PRISM wordmark. This scope incorporates the user's explicit correction that the supplied art is not an in-game logo.

Generate 512 px Play and 1024 px launcher art by bilinear sampling the supplied square PNG, with smaller exports at 32–192 px for actual-size inspection. Adaptive foreground carries the full illustration at a mask-safe inset over the existing dark background; the prism and receiver must remain visible in circle/squircle masks. Monochrome themed icon is a code-native triangle, incoming beam and downward output silhouette derived from the subject, rather than the old Z glyph.

The 1024×500 feature graphic uses the supplied square illustration centered at its original aspect ratio on a dark brand background. Generation must not write into Resources/Brand/OpticalGlyph.png; menus, splash and finale continue using their existing in-game glyph. Release generation must fail clearly if the source image is missing, invalid or not square; no silent fallback to the previous launcher art.

Verify Unity import and Windows/Android compilation, inspect runtime menu and icon exports, run the existing campaign/input regression smoke, validate the signed AAB and its native 16 KB alignment, then preserve all changes in one normal commit and push. Pasted wall/settings critique is retained as review context; structural level-design changes require their own implementation scope.
