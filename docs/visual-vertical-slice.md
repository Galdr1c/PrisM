# Phase 2 — Işık ve optik görsel vertical slice

## Amaç
Fizik solver'ını değiştirmeden Prism Rules benzeri parlak, okunabilir ve mobilde ölçeklenebilir bir ışık dili oluşturmak.

## Bu dilimde uygulananlar
- Tahta/grid artık yüzlerce disk mesh'i yerine tek quad üzerinde prosedürel shader ile çizilir.
- Işık yolları fizik sonucundan ayrı `Light field` mesh katmanında çizilir.
- Beam shader additive HDR çıkış, yumuşak merkez profili, beyaza yaklaşan core ve hafif temporal pulse kullanır.
- URP post-processing çalışma zamanında etkinleştirilir; düşük maliyetli Bloom + ACES tonemapping + hafif color adjustments uygulanır.
- Prizma, lens ve cam küre ayrı optical-glass shader katmanına taşınmıştır.
- Su bölgeleri ayrı procedural shader katmanında hareketli highlight/caustic-benzeri desen üretir.
- Hedef ve kaynak çevresindeki parıltılar additive light-field katmanına alınmıştır.

## Bilinçli sınırlar
Bu vertical slice gerçek screen-space background refraction, fiziksel Fresnel enerji ayrımı veya ray-traced caustics yapmaz. Solver gerçek ışık yolunu belirler; GPU katmanı görsel sunumu üretir. Mobil hedef nedeniyle ilk iterasyonda texture-free/procedural shaderlar tercih edilmiştir.

## Mobil performans ilkeleri
- Grid tek quad/draw katmanıdır; eski 19×19 disk geometrisi kaldırılmıştır.
- Bloom high-quality filtering kapalıdır.
- Beam geometri katmanı solver segmentlerinden üretilir ve üç profil (halo/glow/core) kullanır.
- Kamera gölgeleri kapalıdır; görsel bütçe ışık ve post-process'e ayrılır.
- Mobile URP render scale 0.8 korunur.

## Doğrulama
1. `dotnet run --project Tests/CoreTests.csproj` ile optik sonuçlarının değişmediğini doğrula.
2. Unity 6000.6.0f1 ile projeyi aç; shader import/compile hatası olmadığını kontrol et.
3. Bölüm 2'de prizma dispersion ışıklarını, bölüm 4'te lens odaklamayı, bölüm 7'de su + cam küreyi ve bölüm 8'de birleşik sahneyi görsel olarak kontrol et.
4. Windows smoke capture'larını önceki görüntülerle karşılaştır; fizik rotaları aynı kalmalı, sadece sunum değişmeli.
5. İlk Android cihaz build'inde GPU frame time, overdraw ve thermal davranışı ölçülmeden değerleri final kabul etme.

## Sonraki iterasyon
- Beam genişliği/yoğunluğunu güç değerine ve kalite katmanına bağlama.
- Cam için daha iyi rim/Fresnel ve kontrollü chromatic dispersion.
- Water için screen-space distortion seçeneğini sadece orta/yüksek kalite cihazlarda açma.
- Low/Mid/High kalite presetleri ve thermal fallback.
- UI'ı IMGUI'den safe-area destekli kalıcı mobil UI'a taşıma.
