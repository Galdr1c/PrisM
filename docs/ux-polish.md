# PrisM visual / UX polish — 0.9.0

This pass turns the current functional pre-release interface into a calmer, more intentional mobile game flow without changing the deterministic optics rules.

## Implemented

- Rebuilt the gameplay HUD hierarchy around chapter, experiment, objective and difficulty context.
- Reworked the piece inspector with larger touch targets, clearer selection state and explicit precision rotation controls.
- Reworked the optical inventory into large stateful cards with remaining-count feedback.
- Replaced the flat bottom toolbar with a consistent action dock and a dedicated solved-state dock.
- Added contextual invalid-placement feedback instead of relying on audio alone.
- Rebuilt the chapter / level map with progress bars, current-state emphasis, completion states and two-stage navigation.
- Rebuilt settings into separated interaction rows plus explicit Auto / Low / Medium / High quality choices.
- Added a cinematic completion sequence with:
  - spectral reveal strip,
  - animated result-card entrance,
  - goal / active-piece / difficulty summary,
  - campaign progress,
  - chapter-complete and campaign-complete variants,
  - “inspect solution” flow instead of immediately forcing the next level.
- Added a temporary HDR/bloom lift and beam shimmer during completion, then returns to the normal visual profile.
- Added separate completion and chapter-milestone audio motifs.
- Refined board contrast, grid hierarchy, board-corner framing, target-complete glow and selected-piece rings.

## UX principles

1. The optical board remains the visual priority; interface chrome is dark and quiet.
2. Gold is reserved for progression / primary actions, cyan for interaction / active state, green for completion.
3. Every persistent control is sized for touch use in the 900 × 1540 design space.
4. Modal states block background interaction.
5. Completion is a reward moment, but the player can dismiss it and inspect the solved light path.
6. No score or star system was introduced: the game communicates success without penalizing experimentation.

## Device validation gate

Before merging this pass into a store candidate, run:

`Tools/PreRelease-Check.ps1 -RequireAndroid`

Then inspect at minimum on one lower/mid-range and one modern Android device:

- 20:9 and tall punch-hole layouts,
- level title wrapping,
- all piece-inventory combinations,
- selected piece inspector,
- invalid placement toast,
- hint modal,
- chapter overview and 10-level view,
- settings quality selector,
- regular completion,
- level 10 chapter completion,
- level 100 campaign completion,
- completion bloom / thermal cost,
- Android back behavior from each modal and solved-state view.

Store screenshots must be regenerated after this pass.
