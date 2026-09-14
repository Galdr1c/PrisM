# Doğrulama durumu

14 Eylül 2026

- `dotnet run --project Tests/CoreTests.csproj`: 30 kontrol geçti.
- Beş bölümün bilinen çözümleri bütün hedefleri etkinleştirdi; başlangıçlar çözülmemiş.
- Yansıma, normal girişte kırılma, tam iç yansıma, spektral indis sıralaması, duvar engellemesi, stok, geri alma, enerji normalizasyonu, renk seçimi ve döngü bütçesi kontrol edildi.
- Bağımsız kod incelemesinde bulunan shader paketleme, açılış sahnesi ve ipucu panelinin giriş engellemesi sorunları düzeltildi.
- Unity 6000.6.0f1 StandaloneWindows64 derlemesi `Tools/Build-Windows.ps1` ile başarıyla tamamlandı (`Builds/Windows/PrisM.exe`).
- `Tools/Smoke-Windows.ps1` ile 5 bölümün çalışma zamanı yüklemesi, çözümü ve sıfırlaması (10 kontrol) otomatik olarak doğrulandı.
- Kamera ölçekleme/hizalama hatası ve OnGUI dizi sınır kontrolü düzeltildi; oyun tahtası ekran çerçevesiyle birebir piksel hizasına oturtuldu.
- Bölüm 1'den 5'e kadar çözülmüş durum ekran görüntüleri (`TestResults/smoke-*/level-*-solved.png`), prizma renk kırılması, odaklama ve tamamlama diyalogları görsel olarak doğrulandı.
- Android/iOS modülleri kurulu değil; gerçek telefon testleri yapılmadı.
