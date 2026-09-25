# Google Play Console declarations — PrisM 0.9.0

This file is the release-team answer sheet for the current codebase. Re-check it if any SDK, permission, online service, monetization, account system or telemetry is added.

## App content

- **Ads:** No.
- **App access:** No login, subscription gate, invite code or restricted content.
- **In-app purchases:** No.
- **User-generated content:** No.
- **News / government / financial / health functionality:** No.
- **Location functionality:** No.
- **Target audience:** General puzzle audience, 13+; the product is not intentionally directed to children.
- **Online dependency:** None for gameplay.

## Data Safety

Current application code does not collect or share personal or sensitive user data.

Locally stored only:
- completed level IDs;
- last played level ID;
- audio preference;
- haptic preference;
- visual-quality preference.

These values remain in the app's local persistent storage and are not transmitted by PrisM.

No current SDK for:
- advertising;
- analytics;
- crash reporting;
- attribution;
- social login;
- cloud save;
- payments.

If any of those are added, the Data Safety answers and privacy policy must be reviewed before shipping that build.

## Android permissions

Expected runtime/build permissions from current source:
- `VIBRATE` can be added automatically by Unity because the game uses `Handheld.Vibrate`.
- No location, camera, microphone, contacts, storage or account permission is required by gameplay.
- The release build explicitly disables forced Internet and SD-card permissions.

The final merged manifest must still be inspected from the release AAB because Unity packages or future plugins can alter it.

## Content rating preparation

Based on current game content:
- violence: none;
- sexual content / nudity: none;
- profanity: none;
- drugs / alcohol / tobacco: none;
- gambling: none;
- horror / intense fear: none;
- user communication: none;
- user-generated content: none;
- unrestricted web access: none.

Answer the live IARC questionnaire from the actual build and store listing rather than copying these notes blindly if Play changes the questionnaire.

## Store presence

Prepared repository sources:
- Turkish and English listing: `docs/store-listing.md`
- privacy policy text: `docs/privacy-policy.md`
- icon + feature graphic generator: `Assets/Prism/Editor/BrandAssets.cs`
- real gameplay screenshot capture: `Tools/Capture-Store-Screens.ps1`

Still account-specific and therefore intentionally not hard-coded:
- public support email;
- public HTTPS privacy-policy URL;
- developer name/address/phone required by the Play account;
- countries/regions and pricing;
- testing lists;
- Play App Signing account choices.

## App signing

The upload AAB is signed with the developer upload key. Play App Signing then manages distribution signing for a new Google Play app.

Keep the upload keystore and passwords outside the repository. The build pipeline reads:
- `PRISM_KEYSTORE_PATH`
- `PRISM_KEYSTORE_PASS`
- `PRISM_KEY_ALIAS`
- `PRISM_KEY_ALIAS_PASS`
