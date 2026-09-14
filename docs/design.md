# PrisM ilk oynanabilir sürüm

Onaylanan yön: Unity, C#, URP ile mobil optik bulmaca. Referans oynanış incelendi; özgün beş bölüm oluşturulacak.

## Kapsam
Kaynak, ayna, üçgen prizma, renk seçici ayna, ince lens, engel ve renk hedefleri. Beş bölüm; sınırlı envanterden yerleştirme, sürükleme, seçili parçayı ayrı kontrolle döndürme, geri alma, sıfırlama, ipucu ve yerel ilerleme kaydı. Fare ve tek parmak aynı etkileşimi kullanır. Çevrimdışı.

## Mimari
Unity bağımsız C# geometri/optik çekirdeği ışık yolları ve hedef enerjilerini üretir. Kaynak genişliği ve yedi spektral bant örneklenir. Prizmada Snell kırılması ve tam iç yansıma, aynada yansıma, renk seçicide bant ayrımı; lenste paraxial ince lens yaklaşımı kullanılır. Görsel katman hesap sonucunu renkli şerit meshlerle çizer. Bulmaca eşiği tüm grafik ayarlarında aynıdır. Nesne değişince yeniden hesaplama yapılır.

## Bölüm ve doğrulama
Her bölüm başlangıç verisi, stok, hedefler ve bilinen çözüm içerir. Testler bilinen çözümlerin bütün hedefleri etkinleştirdiğini ve başlangıçların çözülmüş olmadığını doğrular. Çekirdekte kırılma, yansıma, enerji, engel ve döngü bütçesi test edilir. Çözümün kısa süre sabit kalması tamamlanma sayılır.

## İlk teslim sınırı
Kurulu Unity 6000.6.0f1 kullanılacak. Windows çalıştırılabilir prototip ve Unity projesi üretilecek. Android/iOS modülleri kurulu değil; telefon performansı ve mağaza paketleri bu teslimde doğrulanmış sayılmaz. Su, kara delik, gerçek lens yüzeyleri, ses tasarımı ve üretim kalitesinde bloom sonraki aşamadır.
