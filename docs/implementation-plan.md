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

## Sonraki adımlar
1. `ILevelProvider` için ScriptableObject tabanlı `LevelCatalog`/`LevelDefinition` authoring katmanı ve özel Unity level editor.
2. Tek bir referans bölümde HDR/additive beam shader, bloom, Fresnel glass, dispersion edge, water distortion ve caustics vertical slice.
3. IMGUI yerine safe-area uyumlu mobil UI ve gerçek telefon touch/haptic doğrulaması.
4. Solver/renderer allocation ölçümü, mesh/grid optimizasyonu ve cihaz bazlı Low/Mid/High kalite profilleri.
5. Android/iOS mağaza build ayarlarının gerçek build modülleriyle doğrulanması.
