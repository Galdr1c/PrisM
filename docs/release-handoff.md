# PrisM pre-release teslimi

Bu teslim Google Play Internal testing için hazırlanmış sürüm adayıdır. Üretim yayını öncesinde gerçek Android cihaz ve Play pre-launch testi gerekir.

## Çıktılar

- Windows deneme paketi: `Builds/Windows/PrisM.exe` ve yanındaki veri/DLL klasörleri.
- Android: `Builds/Android/PrisM-0.9.0-90.aab`; paket `com.prismstudio.lightworkshop`, API 36, min API 26, ARM64, IL2CPP.
- Mağaza ikonu ve feature graphic: `Builds/StoreAssets`.
- Altı 1080 × 1920 gerçek oyun görüntüsü: `Builds/StoreAssets/screens-20260929-131720`.
- TR/EN metinler: `docs/store-listing.md`; deklarasyon rehberi: `docs/play-console-declarations.md`.
- Gizlilik politikası kaynağı: `docs/privacy-policy.md`. Play hesabında gerçek destek adresi ve herkese açık HTTPS politika URL'si tamamlanmalıdır.

## İmza anahtarı

Yerel upload anahtarı `Builds/Secrets/prism-upload.p12`, alias `prism-upload`. Parola `Builds/Secrets/upload-password.txt` içinde bulunur. Bu klasör Git'e dahil edilmez; anahtar ve parola güvenli biçimde yedeklenmelidir. Parola hiçbir commit veya log mesajında paylaşılmamalıdır.

## Doğrulama

Çekirdek testleri, Unity Windows derlemesi, 100 çözüm/reset smoke ve mağaza görüntüsü boyut/RGB kontrolleri geçti. Android bundle, imza ve native alignment sonuçları son paket üretiminden sonra bu belgenin altına kaydedilir.

29 Eylül final Android sonucu: 27.646.479 byte AAB üretildi; bundletool validation exit 0, PAGE_ALIGNMENT_16K ve altı ARM64 ELF LOAD alignment kontrolü PASS. Manifest yalnız VIBRATE izni içerir. JAR imzası doğrulanır; self-signed upload sertifikası kullanılır.

SHA256: `DC3B5A02B5298FE17861B47B7BC7F9E7181E3170C7BE5E7389894EAFEFA37567`.

Kalan cihaz kontrolleri: farklı ekran/safe-area, touch/rotate, ünite geri dönüşü, Android Back, müzik/SFX ayarları, pause/resume, düşük/orta/yüksek kalite, ısınma ve uzun oturum. Bunlar masaüstü testleriyle doğrulanmış sayılmaz.
