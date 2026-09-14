# Doğrulama durumu

14 Eylül 2026

- `dotnet run --project Tests/CoreTests.csproj`: 30 kontrol geçti.
- Beş bölümün bilinen çözümleri bütün hedefleri etkinleştirdi; başlangıçlar çözülmemiş.
- Yansıma, normal girişte kırılma, tam iç yansıma, spektral indis sıralaması, duvar engellemesi, stok, geri alma, enerji normalizasyonu, renk seçimi ve döngü bütçesi kontrol edildi.
- Bağımsız kod incelemesinde bulunan shader paketleme, açılış sahnesi ve ipucu panelinin giriş engellemesi sorunları düzeltildi.
- İlk Unity denemesi yanlış lisans IPC kanalından dolayı başarısızdı; Hub'daki Personal lisansı mevcut. Doğru `-licensingIpc` kanalı ile yetkiler çözümlendi, editör paket ve C# derlemesine geçti. Derleme sonucu henüz bekleniyor.
- Windows paket çalıştırma ve ekran görüntüsü doğrulaması bekliyor. `Tools/Build-Windows.ps1` ve `Tools/Smoke-Windows.ps1` hazır.
- Android/iOS modülleri kurulu değil; gerçek telefon testleri yapılmadı.
