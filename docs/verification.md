# PrisM doğrulama durumu

29 Eylül 2026

## Son yerel doğrulama
- Birleştirilmiş Windows sürümü Unity 6000.6.0f1 ile başarıyla derlendi.
- `TestResults/smoke-20260929-131704` çalışması 100 çözüm ve 100 reset kontrolünü geçti.
- Altı gerçek oyun görüntüsü `Builds/StoreAssets/screens-20260929-131720` altında 1080 × 1920 RGB PNG olarak üretildi; bölüm haritasındaki metin çakışması düzeltilip tekrar yakalandı.
- Ünite haritasından geri dönüşte negatif indeks hatası giderildi. Bölüm sayacı geçersiz indekslere karşı korunuyor.
- Üç özgün laboratuvar dokusu, fiziksel optik obje detayları, 48 saniyelik özgün ambient BGM ve altı WAV SFX oyuna entegre edildi.
- Merge-marker ve Unity metadata GUID kontrolleri çekirdek testlerine eklendi; çekirdek testleri geçti.
- Yeni görsel/ses sürümünün imzalı ARM64 AAB'si üretildi. Bundletool validation ve 16 KB bundle/altı ELF kütüphane kontrolü geçti. Final manifest API 36/min API 26 ile yalnız VIBRATE izni içerir; gereksiz INTERNET izni release manifest birleştirmesinden çıkarıldı.
- Müzik/SFX birbirinden bağımsız; ses kesilmesi ve devam etmesi uygulama focus/pause akışında uygulanmış. Kaynak WAV clipping/loop kontrolleri geçti; telefon hoparlöründe işitsel değerlendirme henüz yapılmadı.

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
- Signed Android AAB build otomasyonu, 16 KB page-size doğrulaması, Play store asset generatorü ve Play Console deklarasyon cevap sayfası.

## Önceden doğrulanmış baseline
15 Eylül 2026 baseline kaynaklarında Unity 6000.6.0f1 StandaloneWindows64 development build ve o tarihteki 8-level smoke başarıyla çalıştırılmıştı. Shader/color/build baseline bu sonuçlara dayanır.

## Cihaz ve Play Console üzerinde kalan doğrulamalar
Aşağıdaki maddeler masaüstü smoke tarafından doğrulanmaz:
- Google Play Internal testing upload/install.
- Gerçek telefon safe-area, touch, Android back, haptic, audio, suspend/resume.
- GPU/CPU/frame-time/thermal profiling.
- Play Console pre-launch report.

Tek komut release makinesi doğrulaması:
`Tools/PreRelease-Check.ps1 -RequireAndroid`

Gerçek cihaz ve Play pre-launch sonuçları çalıştırıldıktan sonra eklenmelidir. Otomatik çözüm kontrolü, oyuncularla zorluk/çeşitlilik değerlendirmesinin yerine geçmez.
