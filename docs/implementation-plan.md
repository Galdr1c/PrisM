# PrisM uygulama durumu — pre-release 0.9.0

## Tamamlanan ürün altyapısı
1. Unity bağımsız optik solver ve sözleşme/regresyon testleri.
2. 100 bölüm / 10 ünite deterministik kampanya; eski sekiz seed'in stabil ID uyumluluğu.
3. Son 20 bölümde 3–6 elemanlı açıortay/yansıma geometrisi ve tüm parçaların aktif kullanım koşulu.
4. Kaynak/hedef/duvar/parça çakışmalarını engelleyen ortak placement kuralları.
5. Versioned JSON ilerleme, eski PlayerPrefs migrasyonu ve atomik temp-file save akışı.
6. 10×10 bölüm haritası, sıralı unlock, chapter/difficulty gösterimi, hint/undo/reset.
7. Screen.safeArea kullanan dikey mobil layout ve Android back akışı.
8. Prosedürel click/error/completion sesi, aç/kapat ses ve titreşim tercihleri.
9. Layered HDR renderer: board / geometry / water / beam / glass, Bloom + ACES.
10. Auto / Low / Medium / High görsel kalite profilleri ve kaliteye göre geometri/Bloom maliyeti.
11. ScriptableObject LevelCatalog authoring, catalog validasyonu ve stale-catalog fallback.
12. Windows 100-level smoke otomasyonu.
13. Android release pipeline: signed AAB, API 36, min 26, ARM64, IL2CPP, keystore environment variables.
14. Store icon + feature graphic Unity generatorü.
15. Google Play listing, privacy policy ve release checklist kaynakları.

## Pre-release dış doğrulama kapıları
Bunlar kod deposu içinden sahte şekilde “tamamlandı” sayılamaz; gerçek build ortamı/cihaz/Play hesabı gerekir.

1. Unity 6000.6.0f1 + Android Build Support ile `Tools/PreRelease-Check.ps1 -RequireAndroid` çalıştır.
2. Signed AAB'yi Play Console Internal testing'e yükle.
3. Play-generated install artefact ile en az bir orta/düşük ve bir modern Android cihazda:
   - safe-area / punch-hole,
   - touch drag/rotate,
   - Android back,
   - suspend/resume save,
   - audio/haptic toggles,
   - Low/Medium/High/Auto kalite,
   - bölüm 1, 50, 80, 90, 100,
   - thermal/frame pacing
   doğrula.
4. Gerçek cihaz bulgularından sonra yalnız gerekirse shader yoğunluğu veya kalite eşiklerini ayarla.
5. Store listing support email ve yayınlanmış HTTPS privacy-policy URL'sini gerçek Play hesabı bilgileriyle gir.
6. Internal → Closed testing → Production geçişinde Play Console politika/pre-launch raporlarını temizle.

## Yayın sonrası adaylar
- Cloud save / achievements ancak ürün kararı verilirse.
- Analytics/crash SDK ancak Data Safety + privacy policy güncellenerek.
- Daha gelişmiş screen-space refraction/caustics yalnız gerçek cihaz GPU profili yeterliyse.
