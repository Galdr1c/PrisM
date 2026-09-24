# Uygulama sırası

## Tamamlanan foundation
1. Unity bağımsız C# test koşucusu ve optik sözleşme testleri.
2. Geometrik optik, demet örnekleme, hedef değerlendirme.
3. Sekiz özgün bölüm ve her birinin bilinen çözümünü otomatik doğrulama.
4. Unity URP sahnesi, demet ve obje çizimi, dokunmatik/fare etkileşimi.
5. Envanter, açı kontrolü, undo/reset, bölüm seçimi ve ipucu.
6. Stabil level ID'leri, `PieceInfo` capability metadata'sı ve `ILevelProvider` ayrım noktası.
7. 32-bit completion mask yerine versioned JSON ilerleme kaydı ve eski kaydın otomatik migrasyonu.
8. Runtime'daki sabit 5-level varsayımlarının kaldırılması ve 8 bölümün oyuncu akışına açılması.
9. Generated build loglarının source control'dan çıkarılması.
10. Kaynak/hedef/duvar/parça çakışmalarını engelleyen ortak yerleştirme kuralları ve regresyon testleri.
11. Katmanlı HDR renderer: additive beam, Bloom + ACES, ayrı board/geometry/water/glass katmanları ve prosedürel grid optimizasyonu.

## Devam eden authoring geçişi
- `LevelCatalog` ScriptableObject ve serializable `LevelDefinition` veri modeli eklendi.
- Runtime, `Resources/LevelCatalog` asset'i varsa authored catalog'u kullanır; asset yoksa mevcut `Levels.Create()` kataloğuna güvenli fallback yapar.
- Unity Editor'da `PrisM/Authoring/Create or Refresh Default Level Catalog` komutu built-in sekiz bölümü catalog asset'ine aktarır ve ID/direction/goal validasyonu yapar.
- Geçiş tamamlanana kadar hard-coded built-in katalog regresyon/fallback kaynağı olarak korunur.

## Sonraki adımlar
1. Unity'de default `LevelCatalog.asset` üretip smoke parity doğrulaması yapmak; ardından yeni bölümleri yalnız authoring catalog üzerinden üretmeye başlamak.
2. IMGUI yerine safe-area uyumlu mobil UI ve gerçek telefon touch/haptic doğrulaması.
3. Solver/renderer allocation ölçümü, mesh optimizasyonu ve cihaz bazlı Low/Mid/High kalite profilleri.
4. Android/iOS mağaza build ayarlarının gerçek build modülleriyle doğrulanması.
5. Görsel kalite için gerçek cihaz profiling sonrasında sahne tabanlı refraction/caustics ve gerekirse ek HDR kalite katmanları.
