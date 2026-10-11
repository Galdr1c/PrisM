# 11 Ekim PrisM arayüz uygulaması

Kullanıcının `PrisM Arayüz Tasarımı.html`, `home.svg`, `1tt.svg`, `2tt.svg`, `3tt.svg` ve `4tt.svg` tasarımları Unity Canvas/TMP sunum katmanına uyarlandı. Kaynak dosyalar kullanıcıya aittir ve değiştirilmedi.

## Uygulanan görünüm ve akış

- Gece moru arka plan, lavanta taş yüzeyler, Sora tipografi, derinlikli yuvarlak butonlar ve kısa basma hareketi.
- Ana menüde kodla çizilen kaynak/prizma/spektrum/alıcı sahnesi. Güvenli alan küçüldüğünde sahne ölçeklenir; başlık ve ana eylem ayrı kalır.
- Düğümler arasında uçları kesilmiş ışık yollarıyla ünite haritası; üç sütunlu bölüm haritasında seçim, bölüm bilgisi ve ayrı Oyna eylemi.
- Küçük parça tepsisi, seçili parçaya bağlı açı/kaldırma kontrolleri, hedef ilerlemesi ve okunabilir alt eylemler. Yerleştirilmiş parça Tahtada etiketi alır.
- Duvarların fiziksel kalınlığıyla eşleşen mor tuğla yüzeyler, belirgin kenarlar, üç yüzlü prizma, kaynak/alıcı çerçeveleri ve düşük yoğunlukta nokta zemini.
- Sinematik ton eşleme kaldırıldı: koyu mor paletin siyaha ezilmesi giderildi; ışık halesi bloom üzerinden korunur. Yüksek kontrastta nokta ve obje kenarları ayrıca güçlenir.

Uygulama/store ikonu `artifacts/logo.png` menü illüstrasyonu olarak kullanılmaz. Yeni çizimler yerel vektör mesh kullanır; indirilen üçüncü taraf asset eklenmedi. Tanımlı puanlama olmadığı için yıldız/para sistemi eklenmedi.

## Oyun ve doğrulama

100 bölüm, v3 katalog kimlikleri, çözümler ve optik hesaplama aynı kalır. Yeni bölüm haritası önizleme akışı ve seçim çubuğunun oyun alanıyla çakışmaması gerçek oyuncuda smoke kontrolüne dahildir. Son doğrulama yolları ve signed AAB bilgileri `docs/release-handoff.md` içinde kaydedilir.

Sürüm: 0.9.2 / Android versionCode 92. Masaüstü doğrulaması Android cihaz testi yerine geçmez; fiziksel cihaz/safe-area, touch, performans ve Play pre-launch kontrolleri yayın öncesinde tamamlanmalıdır.
