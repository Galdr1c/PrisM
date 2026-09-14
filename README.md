# PrisM Işık Atölyesi

Unity ile geliştirilen, beş özgün bölümlük mobil optik bulmaca prototipi.

## Mevcut durum
Optik çekirdek ve beş bölümün bilinen çözümleri bağımsız C# testlerinden geçiyor. Unity arayüzü, dokunmatik/fare girişi, envanter, açı kontrolü, geri alma, sıfırlama, ipuçları ve yerel ilerleme kaydı kaynak kodda mevcut.

Unity Hub'daki Personal lisansı mevcut. İlk komut satırı denemesinde editör yanlış lisans IPC kanalına bağlanıyordu; derleme aracına Hub'ın `Unity-LicenseClient-<kullanıcı>` kanalı eklenerek erişim düzeltildi. Windows derleme ve ekran doğrulaması sürüyor. Android/iOS modülleri bu bilgisayarda kurulu değil; telefon performansı doğrulanmış değildir.

## Açma
1. Unity Hub'ı mevcut Personal lisansının bağlı olduğu hesapla açık tut.
2. Add project from disk ile `D:\PrisM` klasörünü ekle; Unity 6000.6.0f1 ile aç.
3. Paketler yüklendikten sonra `Assets/Scenes/Prism.unity` sahnesini aç.
4. Game görünümünü 900 × 1340 veya benzer dikey oran yap ve Play'e bas.

## Kontroller
- Envanterden parça seç, oyun alanına dokunarak yerleştir.
- Parçanın ortasından sürükle; seçili parçanın çevresindeki halkayı kullanarak döndür.
- Alt bardaki ±1° / ±15° düğmeleriyle ince ayar yap. Fare tekerleği de 1° ayarlar.
- Kaldır, parçayı envantere geri verir. Geri al son düzenlemeyi geri getirir.
- Hedeflerin tamamı yeterli ışığı 0,65 saniye alınca bölüm tamamlanır.
- Bölüm düğmeleri prototipte serbesttir; tamamlanan bölümler yerel olarak kaydedilir.

## Test ve derleme
`dotnet run --project Tests/CoreTests.csproj` (.NET 10 SDK; harici NuGet paketi yok).

`powershell -ExecutionPolicy Bypass -File Tools/Build-Windows.ps1`

Başarılı derleme `Builds/Windows/PrisM.exe` üretir. Windows paketi taşınırken yanındaki veri klasörü ve DLL dosyaları da birlikte taşınmalıdır.

`Tools/Smoke-Windows.ps1` geliştirme paketini açar; beş çözümü, stok yerleşimini ve sıfırlamayı doğrular. Ekran görüntüleri ile sonuçları `TestResults/` altına yazar. Bu otomasyon gerçek dokunmatik giriş testi yerine geçmez.

## Sınırlar
Lens paraxial ince lens yaklaşımıdır; prizma doğrudan kırılma ve tam iç yansıma içerir, Fresnel ikincil yansımaları henüz yoktur. Yedi spektral bant ve kaynak genişliği boyunca 13 örnek kullanılır. Grafikler prosedürel şerit meshlerdir; referanstaki hacimsel görünüm ve gelişmiş bloom henüz hedeflenmemiştir. Unity IMGUI arayüzü ilk prototip içindir; mağaza sürümü öncesinde safe-area destekli kalıcı UI, erişilebilirlik ve gerçek telefon testleri gerekir. Su, kara delik, ses ve haptik eklenmedi.

`Assets/Prism/Core` motor bağımsız hesaplar; `Runtime` etkileşim; `Rendering` prosedürel çizim; `Editor` sahne/derleme otomasyonu içerir.
