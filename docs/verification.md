# Doğrulama durumu

15 Eylül 2026

## Otomatik çekirdek doğrulaması
- `Tests/CoreTests.csproj` artık 45 sözleşme/regresyon kontrolü içerir.
- Yansıma, normal girişte kırılma, tam iç yansıma, spektral indis sıralaması, cam küre odaklaması ve su kırılması kontrol edilir.
- Sekiz built-in level'in başlangıçta çözülmemiş olduğu, bilinen çözümlerinin bütün hedefleri etkinleştirdiği ve solver bounce bütçesini aşmadığı kontrol edilir.
- Level ID'lerinin boş olmadığı ve benzersiz olduğu doğrulanır.
- Parça capability metadata'sında cam kürenin ayrı adı ve döndürülemez davranışı, aynanın ise döndürülebilir davranışı kontrol edilir.
- Duvar engellemesi, stok, yerleştirme, geri alma, enerji normalizasyonu, renk seçimi ve sonsuz yansıma döngüsü bütçesi kontrol edilir.
- `.github/workflows/core-tests.yml` pull request ve `main` push'larında aynı test projesini .NET 10 ile çalıştırır.

## Runtime doğrulaması
- Smoke otomasyonu sabit 5 yerine `levels.Length` üzerinden bütün built-in level'leri yükler, bilinen çözümleri yerleştirir ve reset davranışını kontrol eder.
- Normal oyuncu akışı da bölüm sayısını `levels.Length` üzerinden kullanır; 7. ve 8. bölümler sayfalı selector ve completion akışından erişilebilir.
- Cam küre UI'da `Cam küre` olarak görünür ve gereksiz açı kontrolleri gösterilmez.
- İlerleme kaydı stabil level ID'leriyle versioned JSON'a taşınmıştır; eski `PlayerPrefs` completion mask'i destekleniyorsa ilk normal açılışta migrate edilir. Smoke modu kullanıcı ilerlemesini değiştirmez.

## Unity ve Windows doğrulaması
- Unity 6000.6.0f1 StandaloneWindows64 geliştirme derlemesi güncel kaynaklarla `Tools/Build-Windows.ps1` üzerinden başarıyla tamamlandı.
- Güncel 8-level runtime smoke bütün built-in çözümleri ve her bölümde reset davranışını doğruladı.
- Başlangıç, her çözülmüş bölüm ve tamamlanma paneli için ekran görüntüsü üretildi ve görsel olarak incelendi.
- Linear color space içinde doğrudan kullanılan vertex renklerinin tahtayı gereğinden açık göstermesi düzeltildi; shader sRGB değerlerini lineara çeviriyor.
- Spektral bantlar additive birleşerek beyaz ışığı oluşturuyor. Her çizimin parlaklığı solver enerji değerine bağlandı; prizma sonrası renk ayrışması korunuyor.
- Smoke oyuncu günlüğünde hata, exception veya warning bulunmadı.
- Build otomasyonu artık mevcut oyun sahnesini açıyor; her build sırasında sahneyi yeniden üretip dosya kimliklerini değiştirmiyor.

## Hâlâ gerekli doğrulamalar
- Android/iOS modülleri ve gerçek telefon cihazlarıyla build, safe-area, touch, thermal ve GPU/CPU profiling henüz yapılmadı.
- Üretim kalitesindeki HDR beam/bloom/refraction görsel katmanı ayrı vertical-slice aşamasıdır.
