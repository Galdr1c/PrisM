# PrisM — Işık Atölyesi

Unity 6000.6.0f1 + URP ile geliştirilen, mobil odaklı optik bulmaca oyunu.

## Pre-release kapsamı
- **100 bölüm / 10 ünite**: yansıma, spektrum, seçici renk, odak, birleşik optik, kırılma, su/cam, senfoni, geometri ve ustalık.
- İlk sekiz özgün mekanik seed'in stabil ID'leri korunur; mevcut ilerleme kayıtları bozulmadan 100 bölümlük kampanyaya geçebilir.
- Son 20 bölümde 3–6 aynalı geometri zincirleri vardır. Ustalık bölümleri stoktaki bütün parçaların gerçekten ışık yolunda aktif olmasını zorunlu kılar.
- Çekirdek optik: ayna, prizma, renk seçici yüzeyler, ince lens, cam küre, su, duvarlar, renk hedefleri; 7 spektral bant ve kaynak genişliği örneklemesi.
- Safe-area uyumlu dikey mobil layout, 10×10 bölüm haritası, sıralı kilit açma, ipucu, undo/reset, ayarlar, ses/titreşim ve Android geri davranışı.
- Katmanlı URP renderer: prosedürel board, additive HDR ışın, Bloom + ACES, cam ve su shader'ları.
- Auto / Düşük / Orta / Yüksek cihaz kalite profilleri. Grafik kalitesi optik çözümü değiştirmez.
- Yerel, versioned JSON ilerleme kaydı; eski PlayerPrefs completion verisi otomatik migrate edilir.
- Reklam, analytics, hesap, IAP veya zorunlu internet SDK'sı yoktur.

## İçerik doğrulaması
`dotnet run --project Tests/CoreTests.csproj`

CI bütün 100 bölüm için benzersiz ID, başlangıçta çözülmemiş durum, kayıtlı çözüm, solver etkileşim bütçesi, placement kuralları, chapter/difficulty yapısı ve mastery active-piece koşullarını doğrular.

## Unity / Windows preflight
```powershell
.\Tools\PreRelease-Check.ps1
```

Preflight sırasıyla core testleri, Windows build, **100 çözüm + 100 reset** runtime smoke ve Play Store grafik üretimini çalıştırır. Android imza environment variable'ları mevcutsa signed AAB de üretir.

Tekil komutlar:
```powershell
.\Tools\Build-Windows.ps1
.\Tools\Smoke-Windows.ps1
.\Tools\Generate-Store-Assets.ps1
.\Tools\Build-Android-AAB.ps1
```

Unity yolu farklıysa `PRISM_UNITY_EDITOR` ile belirtilebilir.

## Android / Google Play
Release build sözleşmesi:
- package: `com.prismstudio.lightworkshop`
- sürüm: `0.9.0`, versionCode `90` (environment variable ile override edilebilir)
- signed `.aab`
- target API 36, min API 26
- ARM64
- IL2CPP
- portrait
- optimized frame pacing
- edge-to-edge render + safe-area UI
- release keystore yalnız environment variable üzerinden; parola/keystore repo'ya yazılmaz

Ayrıntılı teslim sırası: `docs/google-play-release.md`  
Store metinleri: `docs/store-listing.md`  
Gizlilik politikası kaynağı: `docs/privacy-policy.md`

## Level authoring
`ILevelProvider` üzerinde ScriptableObject `LevelCatalog` / `LevelDefinition` authoring katmanı vardır. Unity Editor:

`PrisM/Authoring/Create or Refresh Default Level Catalog`

Runtime yalnız 100 bölüm sayısını ve tam catalog validasyonunu geçen authored catalog'u kabul eder; eski/bozuk asset bulunursa deterministik `Levels.Create()` kampanyasına fallback yapar.

## Mimari
- `Assets/Prism/Core`: Unity bağımsız geometri, optik solver, curriculum, placement/session.
- `Assets/Prism/Runtime`: input, progress, UI, feedback, catalog ve kalite profili.
- `Assets/Prism/Rendering`: prosedürel HDR board/beam/glass/water renderer.
- `Assets/Prism/Editor`: authoring, branding ve Windows/Android release otomasyonu.
- `Tests`: Unity bağımsız regresyon ve 100-level doğrulama.

## Doğrulama sınırı
GitHub Core CI Unity Editor'ü derlemez. Signed AAB üretimi, gerçek Android cihaz touch/cutout/thermal testi ve Play Console yüklemesi Unity Android Build Support ile release makinesinde yapılmalıdır. Bu nedenle repo pre-release teslim aşamasına hazırlanmıştır; gerçek cihaz ve signed-store artefact sonucu `docs/verification.md` içinde ancak çalıştırıldıktan sonra doğrulanmış sayılır.
