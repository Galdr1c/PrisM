# PrisM doğrulama durumu

24 Eylül 2026

## Doğrulandı — GitHub Core CI
- 100 bölüm üretiliyor.
- Level ID'leri benzersiz ve eski sekiz temel ID korunuyor.
- 10 chapter × 10 level yapısı ve chapter bazlı 1–10 difficulty progression doğrulanıyor.
- Her bölümün başlangıç konfigürasyonu çözülmemiş.
- Her bölümün kayıtlı çözümü hedefleri tamamlıyor.
- Kayıtlı çözümler solver bounce/interaction bütçesini aşmıyor.
- 100 çözüm placement kurallarından geçiyor.
- Mastery bölümleri gameplay completion sırasında bütün stok parçalarını yerleştirmeyi ve ışık yolunda aktif kullanmayı gerektiriyor.
- Yansıma, kırılma, TIR, dispersion index sıralaması, sphere focus, water, wall blocking, inventory, undo, renk seçimi ve loop budget regresyonları korunuyor.
- Son güncel PR koşusu: Core Tests başarılı.

## Kodda uygulanmış — Unity/runtime
- 100-level campaign ve 10×10 level map.
- Sıralı unlock ve eski progress ID uyumluluğu.
- Screen.safeArea tabanlı dikey layout.
- Android back overlay/navigation davranışı.
- Procedural audio + optional haptics.
- Auto/Low/Medium/High visual quality.
- HDR beam, Bloom/ACES, water/glass katmanları.
- 100-level runtime smoke kodu: her bölüm için known solution + reset kontrolü; chapter örnek screenshot'ları.
- ScriptableObject LevelCatalog; stale/invalid catalog güvenli fallback.
- Signed Android AAB build otomasyonu ve Play store asset generatorü.

## Önceden doğrulanmış baseline
15 Eylül 2026 baseline kaynaklarında Unity 6000.6.0f1 StandaloneWindows64 development build ve o tarihteki 8-level smoke başarıyla çalıştırılmıştı. Shader/color/build baseline bu sonuçlara dayanır.

## Bu 100-level pre-release branch'inde henüz gerçek ortamda tekrar çalıştırılması gerekenler
Aşağıdaki maddeler bu GitHub Core CI tarafından doğrulanamaz ve çalıştırılmadan “PASS” olarak kabul edilmemelidir:
- Unity Editor compile/import (runtime + editor scripts).
- Güncel 100-level Windows build/smoke.
- Android Build Support ile signed API 36 AAB üretimi.
- Google Play Internal testing upload/install.
- Gerçek telefon safe-area, touch, Android back, haptic, audio, suspend/resume.
- GPU/CPU/frame-time/thermal profiling.
- Play Console pre-launch report.

Tek komut release makinesi doğrulaması:
`Tools/PreRelease-Check.ps1 -RequireAndroid`

Sonuçlar başarılı olduktan sonra bu belgeye gerçek cihaz/build artefact bilgisi eklenmelidir.
