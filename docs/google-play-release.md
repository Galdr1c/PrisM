# Google Play pre-release / upload checklist

Status target: **0.9.0 (versionCode 90)** — internal/closed testing candidate.

## Build contract
- Package ID: `com.prismstudio.lightworkshop`
- Unity: `6000.6.0f1`
- Format: signed Android App Bundle (`.aab`)
- Android target: API 36 / Android 16
- Minimum Android: API 26 / Android 8.0
- Architecture: ARM64
- Scripting backend: IL2CPP
- Orientation: portrait
- Gameplay: offline; no account required
- Current codebase: no ads, analytics, IAP or network SDK

Google Play requires new apps and updates submitted after 31 August 2026 to target API 36 or newer:
https://support.google.com/googleplay/android-developer/answer/11926878

New Google Play apps publish with Android App Bundles:
https://developer.android.com/guide/app-bundle

Google Play apps must support 64-bit architectures:
https://developer.android.com/games/optimize/64-bit

## Signed AAB
Never commit a keystore or passwords. Set these environment variables:

```powershell
$env:PRISM_KEYSTORE_PATH="D:\secure\prism-upload.keystore"
$env:PRISM_KEYSTORE_PASS="..."
$env:PRISM_KEY_ALIAS="prism-upload"
$env:PRISM_KEY_ALIAS_PASS="..."
$env:PRISM_VERSION_NAME="0.9.0"
$env:PRISM_VERSION_CODE="90"
.\Tools\Build-Android-AAB.ps1
```

Expected artifact:
`Builds/Android/PrisM-0.9.0-90.aab`

The build fails before packaging if the 100-level known-solution release validation fails.

## Before Play Console upload
1. Install Android Build Support, SDK/NDK/OpenJDK for Unity 6000.6.0f1.
2. Generate and securely back up the upload keystore.
3. Run `dotnet run --project Tests/CoreTests.csproj`.
4. Run Windows development build and `Tools/Smoke-Windows.ps1`.
5. Run the signed AAB build.
6. Install a Play-generated test APK from an internal testing release on at least one low/mid and one modern Android device.
7. Verify notch/punch-hole safe area, touch dragging/rotation, audio/haptic toggles, suspend/resume save, all quality tiers and final mastery levels.
8. Upload the AAB to Internal testing first, then Closed testing before Production.

## Play Console declarations for the current build
- Ads: **No**
- App access / login: **No restricted access; no login**
- Data collection: **No personal data collected by the app code**
- Data sharing: **No**
- Location: **No**
- Purchases: **No**
- User-generated content: **No**
- Network-dependent functionality: **No**
- Target audience recommendation: general puzzle audience, **13+** unless the product decision is changed to intentionally target children.
- Privacy policy source: `docs/privacy-policy.md`; publish it at a stable public HTTPS URL before production submission.
- Support email: use the real Play Console support address; do not invent or commit credentials/contact data here.

Any future analytics, crash SDK, ads, cloud save, social login or IAP changes require revisiting Data Safety and the privacy policy.
