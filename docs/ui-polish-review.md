# UI polish review · 2026-10-06

## Presentation decisions

The main settings sheet now puts music, sound effects, haptics, quality, reduced motion and color support first. High contrast, precision rotation and tutorial replay remain available. Beam/bloom tuning lives in Advanced display. Quality opens an explicit four-option list with a selected checkmark rather than cycling. Advanced display offers Beam: Yumuşak / Dengeli / Parlak and Bloom: Kapalı / Dengeli / Güçlü. The small live beam is drawn inside the sheet above its world dimmer. It is a UI illustration of width and halo, not a capture of the post-processing camera.

Small presentation labels have a 12-design-pixel minimum. Button hit areas are at least 48 × 48 design pixels. Toggle transitions, press motion, map pulses and optical ambience honor reduced motion. Empty tray stock keeps its glyph at low opacity. An armed optic moves upward with a small halo and quieter count; the tray lowers during pickup.

## Optical motif and ambience

Home, introduction and campaign finale use an independent Canvas-native prism motif: one incoming ray, a closed triangular refractive body, then a broad spectrum. This avoids a folded Z/letter silhouette and stays independent of the user-provided app icon. `artifacts/logo.png` is reserved for app/store identity. No photograph or external raster is used in the home ambience. Low-contrast optical curves provide a calm material field; chapter glyphs now vary by optical mechanism.

## Runtime corrections

Hints reveal the region first (renderer stage 1), then exact orientation (renderer stage 3). The final action repeats orientation; it does not promise an unsupported beam trace. Each request redraws immediately. InputAction handles the mapped Escape/back control without requiring Keyboard.current. RequestBackNavigation is also a queued controller entry point. Android predictive/system back still requires real-device validation against the Unity activity and manifest configuration.

Undo/reset animations independently solve every visual piece snapshot. Logical completion continues to read only the final session result, and transition state resets its settlement timer. Gesture cancellation remains active on modal/screen changes, focus loss and pause. Undo/reset also finish an active edit before changing history.

Tray preview and placement share a physical 26-design-pixel finger clearance converted through the camera. Existing piece drags introduce the same clearance after 120 ms, blending it over another 120 ms. Renderer lift stays zero so displayed geometry, solved beams and dropped pieces agree. Pointer anchor/tether and interaction state redraw on release; lift does not replace or reset the beam result.

## Validation

Runtime Smoke now covers queued InputSystem Escape through the actual action callback, region-before-orientation hint requests, tray clearance and preview/drop equality, independent visual beam endpoints during undo/reset, cancellation of pending completion and aborted tray cleanup. Existing all-100-level runtime placement/solve/reset checks remain. Additional screenshots cover home, settings, quality selection and advanced preview.

Compilation, runtime Smoke and visual screenshot review are performed by the coordinating agent after integration. This document does not claim an Android physical-device check.
