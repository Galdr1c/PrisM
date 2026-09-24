# PrisM Işık Atölyesi

Unity ile geliştirilen, sekiz özgün bölümlük mobil optik bulmaca prototipi.

## Mevcut durum
Optik çekirdek ve sekiz bölümün bilinen çözümleri bağımsız C# testlerinden geçiyor. Unity arayüzü, dokunmatik/fare girişi, envanter, açı kontrolü, geri alma, sıfırlama, ipuçları ve yerel ilerleme kaydı kaynak kodda mevcut.

İlerleme artık sabit bölüm kimlikleriyle versioned JSON olarak `Application.persistentDataPath/progress.json` altında tutulur. Eski `PlayerPrefs` bit mask kaydı varsa ilk açılışta otomatik olarak yeni formata taşınır. Böylece bölüm sayısı 32 ile sınırlı değildir ve bölüm sırası değişse bile tamamlanma bilgisi korunabilir.

Unity Hub'daki Personal lisansı mevcut. Windows derleme ve smoke doğrulaması geliştirme aracıyla yapılabiliyor. Android/iOS modülleri bu bilgisayarda kurulu değil; gerçek telefon performansı doğrulanmış değildir.

Güncel Windows geliştirme paketi sekiz bölümün tamamında otomatik çözüm/reset testinden geçti. Render katmanı tahta, geometri, su, ışın ve cam için ayrılmış shader/mesh katmanları kullanır; ışınlar enerjiye bağlı additive HDR çizilir ve runtime URP Bloom + ACES tonemapping ile işlenir. Koyu tahta ve ışık renkleri Linear color space için düzeltilmiştir.

## Açma
1. Unity Hub'ı mevcut Personal lisansının bağlı olduğu hesapla açık tut.
2. Add project from disk ile proje klasörünü ekle; Unity 6000.6.0f1 ile aç.
3. Paketler yüklendikten sonra `Assets/Scenes/Prism.unity` sahnesini aç.
4. Game görünümünü 900 × 1340 veya benzer dikey oran yap ve Play'e bas.

## Kontroller
- Envanterden parça seç, oyun alanına dokunarak yerleştir. Kaynak, hedef, duvar veya başka bir optik parçayla çakışan konumlar kabul edilmez.
- Parçanın ortasından sürükle; döndürülebilir parçalarda seçili parçanın çevresindeki halkayı kullanarak döndür.
- Alt bardaki ±1° / ±15° düğmeleriyle ince ayar yap. Fare tekerleği de döndürülebilir parçalarda 1° ayarlar.
- Cam küre dönel simetriktir; açı kontrolü gösterilmez.
- Kaldır, parçayı envantere geri verir. Geri al son düzenlemeyi geri getirir.
- Hedeflerin tamamı yeterli ışığı 0,65 saniye alınca bölüm tamamlanır.
- Bölüm düğmeleri prototipte serbesttir; görünür bölüm sayfası dinamik olarak ilerler.

## Optik kapsam
- Düz ayna yansıması
- Üçgen prizma, spektral kırılma ve tam iç yansıma
- Yeşil/kırmızı seçici aynalar
- Paraksiyal ince lens
- Cam küre kırılması ve odaklama
- Su bölgelerinde Snell kırılması
- Duvar engelleri ve renk hedefleri
- Yedi spektral bant, kaynak genişliği boyunca 13 örnek ve en fazla 24 etkileşim

## Mimari
- `Assets/Prism/Core`: Unity bağımsız matematik, optik solver, parça metadata/capability bilgisi ve built-in level provider.
- `Assets/Prism/Runtime`: giriş, oturum, UI, ilerleme ve oyun akışı.
- `Assets/Prism/Rendering`: prosedürel tahta ve ışık şeridi çizimi.
- `Assets/Prism/Editor`: sahne/derleme otomasyonu.
- `Tests`: Unity'den bağımsız çekirdek doğrulama.

`ILevelProvider` artık ScriptableObject tabanlı `LevelCatalog`/`LevelDefinition` authoring katmanıyla beslenebilir. Runtime `Resources/LevelCatalog` asset'ini tercih eder; asset henüz oluşturulmamışsa mevcut built-in kataloğa fallback yapar. Unity Editor'daki `PrisM/Authoring/Create or Refresh Default Level Catalog` komutu sekiz built-in bölümü authoring catalog'una aktarır ve temel validasyonları çalıştırır.

## Test ve derleme
`dotnet run --project Tests/CoreTests.csproj` (.NET 10 SDK; harici NuGet paketi yok).

`powershell -ExecutionPolicy Bypass -File Tools/Build-Windows.ps1`

Başarılı derleme `Builds/Windows/PrisM.exe` üretir. Windows paketi taşınırken yanındaki veri klasörü ve DLL dosyaları da birlikte taşınmalıdır.

`Tools/Smoke-Windows.ps1` geliştirme paketini açar; bütün built-in çözümleri, stok yerleşimini ve sıfırlamayı doğrular. Ekran görüntüleri ile sonuçları `TestResults/` altına yazar. Bu otomasyon gerçek dokunmatik giriş testi yerine geçmez.

## Bilinen sınırlar
Lens paraxial ince lens yaklaşımıdır; Fresnel ikincil yansımaları henüz yoktur. Phase 2 görsel katmanda additive HDR ışın, Bloom/ACES, prosedürel su ve cam shader'ları vardır; gerçek hacimsel ışık, sahne tabanlı refraction/caustics ve cihaz bazlı kalite ölçekleme hâlâ geliştirme konusudur. Unity IMGUI arayüzü ilk prototip içindir; mağaza sürümü öncesinde safe-area destekli kalıcı UI, erişilebilirlik ve gerçek telefon testleri gerekir. Kara delik, gelişmiş diffraction, ses ve haptik henüz eklenmedi.
