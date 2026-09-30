# PrisM pre-release teslimi

Bu teslim Google Play Internal testing için hazırlanmış sürüm adayıdır. Üretim yayını öncesinde gerçek Android cihaz ve Play pre-launch testi gerekir.

## Çıktılar

- Windows deneme paketi: `Builds/Windows/PrisM.exe` ve yanındaki veri/DLL klasörleri.
- Android: `Builds/Android/PrisM-0.9.0-90.aab`; paket `com.prismstudio.lightworkshop`, API 36, min API 26, ARM64, IL2CPP.
- Mağaza ikonu ve feature graphic: `Builds/StoreAssets`.
- Altı 1080 × 1920 gerçek oyun görüntüsü: `Builds/StoreAssets/screens-20260930-130943`.
- TR/EN metinler: `docs/store-listing.md`; deklarasyon rehberi: `docs/play-console-declarations.md`.
- Gizlilik politikası kaynağı: `docs/privacy-policy.md`. Play hesabında gerçek destek adresi ve herkese açık HTTPS politika URL'si tamamlanmalıdır.

## İmza anahtarı

Yerel upload anahtarı `Builds/Secrets/prism-upload.p12`, alias `prism-upload`. Parola `Builds/Secrets/upload-password.txt` içinde bulunur. Bu klasör Git'e dahil edilmez; anahtar ve parola güvenli biçimde yedeklenmelidir. Parola hiçbir commit veya log mesajında paylaşılmamalıdır.

## Doğrulama

Çekirdek testleri, Unity Windows derlemesi, 100 çözüm/reset smoke ve mağaza görüntüsü boyut/RGB kontrolleri geçti. Android bundle, imza ve native alignment sonuçları son paket üretiminden sonra bu belgenin altına kaydedilir.

29 Eylül final Android sonucu: 27.646.479 byte AAB üretildi; bundletool validation exit 0, PAGE_ALIGNMENT_16K ve altı ARM64 ELF LOAD alignment kontrolü PASS. Manifest yalnız VIBRATE izni içerir. JAR imzası doğrulanır; self-signed upload sertifikası kullanılır.

30 Eylül görsel ve bölüm düzeni: sabit tuğla duvarlar artırıldı, son 20 bölümde en az 5 ve final ünitesinde en az 6 aktif ayna kullanılır. Genişleyen ışık hüzmesi, ön plandaki optik parçalar, belirgin döndürme yörüngesi ve sıcak/yuvarlak UI Windows'da derlenip 100 bölüm smoke testinden geçti. Yeni `LevelCatalog.asset` açılış için 100 bölümü önceden içerir.

Güncel signed AAB SHA256: `FBC6D20DEBC159C2CA31DE1668F823CA3E21534AE75520E1507784A2A83C2AAA` (27.676.757 byte; bundletool validation ve 16 KB kontrolü PASS). Final manifest API 36/min API 26 ve yalnız VIBRATE iznini içerir.

Kalan cihaz kontrolleri: farklı ekran/safe-area, touch/rotate, ünite geri dönüşü, Android Back, müzik/SFX ayarları, pause/resume, düşük/orta/yüksek kalite, ısınma ve uzun oturum. Bunlar masaüstü testleriyle doğrulanmış sayılmaz.
