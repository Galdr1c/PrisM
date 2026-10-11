# Optics, walls and mobile UX branch integration

Reviewed source: `origin/fix/optics-walls-mobile-ux-20261010` at `60c71a3`. Integration base: main `6928f16`. Changes are consolidated into one commit on main.

The branch normalizes rendered spectral intensity, keeps beam widths constant, draws emitter apertures from physical source width, and validates rotated optic footprints against free-standing walls. Invalid tray ghosts retain the committed optical path. Tap and drag use matching initial angles; wheel, handle and precision rotation reject invalid poses. HUD target indicators are larger, quality changes redraw the board, and hints progress through region, silhouette and exact angle. The shipped 100-level wall catalog is regenerated at revision `2026-10-10-spatial-optics-v3`.

Integration review fixed a Unity compiler variable-scope conflict in the source renderer, made catalog export preserve LF/CRLF line endings, updated the old hint-stage smoke expectation, and aligned hint copy/counters with the three reveals. Independent review also found and fixed wide-window layout overflow and premature prism orientation disclosure at the silhouette stage. Canvas and board now use a matching scale constrained by screen width and height.

Additional runtime regressions cover board viewport containment, mouse-held optical refresh before release, and invalid tray previews preserving the committed beam path. Existing campaign solution/reset, lifecycle, navigation, undo/reset synchronization and input checks remain.

Compatibility limit: legacy mastery gates retain their documented center-clearance exception. Rotated full-footprint rejection applies to free-standing barriers; tightening those authored gate rooms requires a separate geometry revision. Stable level IDs and saved progress remain compatible.

The user-supplied illustration remains restricted to app/store icons. No public package identity or signing key is changed. Desktop tests do not replace Android touch, system Back, haptic or thermal testing on a physical device.

Validation: 477 core checks passed; Unity Windows and signed Android builds succeeded. Both 360×800 and 1280×720 smoke runs produced 213 PASS entries, including 100 solve/100 reset checks. The signed AAB passes bundletool and six native-library 16 KB alignment checks; final manifest includes VIBRATE and excludes INTERNET. Current store captures are under `Builds/StoreAssets/screens-20261011-114945`.
