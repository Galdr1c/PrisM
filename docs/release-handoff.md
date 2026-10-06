# PrisM pre-release teslimi

Bu teslim Google Play Internal testing için hazırlanmış sürüm adayıdır. Üretim yayını öncesinde gerçek Android cihaz ve Play pre-launch testi gerekir.

## Çıktılar

- Windows deneme paketi: `Builds/Windows/PrisM.exe` ve yanındaki veri/DLL klasörleri.
- Android: `Builds/Android/PrisM-0.9.1-91.aab`; paket `com.prismstudio.lightworkshop`, API 36, min API 26, ARM64, IL2CPP.
- Mağaza ikonu ve feature graphic: `Builds/StoreAssets`.
- Sekiz 1080 × 1920 gerçek oyun görüntüsü: `Builds/StoreAssets/screens-20261006-120749` (spectrum, mastery, water/glass, reflection, symphony, map, menu, settings).
- TR/EN metinler: `docs/store-listing.md`; deklarasyon rehberi: `docs/play-console-declarations.md`.
- Gizlilik politikası kaynağı: `docs/privacy-policy.md`. Play hesabında gerçek destek adresi ve herkese açık HTTPS politika URL'si tamamlanmalıdır.

## İmza anahtarı

Yerel upload anahtarı `Builds/Secrets/prism-upload.p12`, alias `prism-upload`. Parola `Builds/Secrets/upload-password.txt` içinde bulunur. Bu klasör Git'e dahil edilmez; anahtar ve parola güvenli biçimde yedeklenmelidir. Parola hiçbir commit veya log mesajında paylaşılmamalıdır.

## Doğrulama

Çekirdek testleri, Unity Windows derlemesi, 100 çözüm/reset smoke ve mağaza görüntüsü boyut/RGB kontrolleri geçti. Android bundle, imza ve native alignment sonuçları son paket üretiminden sonra bu belgenin altına kaydedilir.

29 Eylül final Android sonucu: 27.646.479 byte AAB üretildi; bundletool validation exit 0, PAGE_ALIGNMENT_16K ve altı ARM64 ELF LOAD alignment kontrolü PASS. Manifest yalnız VIBRATE izni içerir. JAR imzası doğrulanır; self-signed upload sertifikası kullanılır.

30 Eylül görsel ve bölüm düzeni: sabit tuğla duvarlar artırıldı, son 20 bölümde en az 5 ve final ünitesinde en az 6 aktif ayna kullanılır. Genişleyen ışık hüzmesi, ön plandaki optik parçalar, belirgin döndürme yörüngesi ve sıcak/yuvarlak UI Windows'da derlenip 100 bölüm smoke testinden geçti. Yeni `LevelCatalog.asset` açılış için 100 bölümü önceden içerir.

Son doğrulanmış signed AAB SHA256: `EECF92584EB4A6062DA2BCCFA2110E722AD38C5752DE69CDB8DD17C65A7885D2` (29.591.245 byte; bundletool validation ve 16 KB kontrolü PASS). Manifest API 36/min API 26'dır. Kaynakta VIBRATE izni eklendi; bu manifest-only değişiklikten sonra AAB yeniden paketlenmelidir.

06 Ekim 0.9.1/91 kaynak teslimi: kullanıcı tarafından sağlanan uygulama/mağaza ikonu, Android 13 monochrome adaptive layer, ortak kalınlıklı fiziksel duvarlar, gate/corridor topolojisi, mixed-optics mastery odaları, UI gesture/undo/back düzeltmeleri ve 468 core kontrolü içerir. VIBRATE manifest ekinden sonra yeni AAB üretimi bekliyor.

05 Ekim sunum geçişi: Canvas/TMP + Sora sunum katmanı, yeni PRISM glyph/icon seti, obsidiyen segment duvarlar, drag tepsisi, takımyıldız haritası, sheet modal akışı, üç aşamalı ipucu, accessibility ayarları ve final ışık sekansı kaynakta tamamlandı. Undo/reset geçişleri sonuç hesaplamasını kilitler; gesture iptali ekran/pause/focus değişimlerinde temizlenir; ilerleme dosyası atomik replace ve `.bak`/`.tmp` fallback kullanır. Son doğrulanmış AAB yukarıdaki hash'tir; bu son kaynak değişikliklerinden sonra Unity lisans kanalına erişim gerektiren yeniden paketleme bu makine oturumunda otomatik onay kotasına takıldı ve cihaz/Play pre-launch testleri hâlâ ayrıdır.

Kalan cihaz kontrolleri: farklı ekran/safe-area, touch/rotate, ünite geri dönüşü, Android Back, müzik/SFX ayarları, pause/resume, düşük/orta/yüksek kalite, ısınma ve uzun oturum. Bunlar masaüstü testleriyle doğrulanmış sayılmaz.
