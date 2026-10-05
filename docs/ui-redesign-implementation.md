# Marka ve UI yeniden tasarımı — uygulama kaydı

Kaynak: `PrisM_Marka_UIUX_Yeniden_Tasarim_Spesifikasyonu.docx`, 4 Ekim 2026. Kamuya açık çalışma adı **PRISM**; final isim araştırması ayrı tutulur. Optik çekirdek, 100 bölüm, kayıt kimlikleri ve Android paket kimliği korunur.

| Alan | Uygulama |
| --- | --- |
| Sunum mimarisi | IMGUI kaldırıldı; Canvas, GraphicRaycaster, Input System UI ve TextMeshPro. `PrismGame` oyun state'ini, `PrismPresentation` ekranları, `PrismTheme` font/paleti yönetir. Canvas bileşenleri çalışma anında kurulur. |
| Marka | Gece moru, ivory, sınırlı cyan; optik katlama glyph'i; aynı sembolden Play, adaptive ve monokrom ikonlar. Türkçe karakterleri doğrulanmış lisanslı Sora Regular/SemiBold. |
| Gameplay | Tahta baskın, kısa başlık ve hedef noktaları, icon kontroller; stok minyatür tepsisi doğrudan drag/drop ve tek dokunma yerleştirme. |
| Dünya | Mat obsidiyen segmentler yeni plandaki duvar yönünü uygular. Hüzme core/halo, obje separation, seçili objeye yakın halo azaltımı, 130° dönüş yayı ve tutamak. |
| Ekranlar | Optik giriş, ana menü, dikey chapter/10-node takımyıldız haritası; pause/reset/settings/hint/result alt sheet'leri ve modal input engeli. |
| Öğrenme | İlk üç bölümde etkileşimle kaybolan hayalet yönlendirme; sekiz saniye hareketsizlikte kısa yardım; yeni optik tür için keşif kartı. |
| İpucu | Metin, bölge ve hayalet yönelim olmak üzere üç kademe; ekonomi/monetizasyon eklenmedi. |
| Başarı | HUD çekilir, çözüm sonuç panelinden önce izlenir. Finalde ışık yolları tekrar yanar, glyph/wordmark sekansı ve “100 ışık yolu. Tek bir başlangıç.” gösterilir. |
| Game feel | Button press/release, 180 ms undo, 40 ms reset stagger; parça türüne göre yerleştirme tonu ve Android kısa titreşim desenleri. |
| Erişilebilirlik | Hareketi azalt, yüksek kontrast, yedi spektral sembol, hassas dönüş ve ±1° kontroller, beam/bloom ayarları. |
| Dayanıklılık | Undo/reset animasyonu sırasında eski sonuç tamamlanma kaydedemez; kesilen gesture temizlenir; kazanılan çözüm incelemede düzenlenemez; ilerleme atomik replace ve yedek/tmp kurtarma kullanır. |

PNG'ler oyuncudan alınır; ilk üç mağaza görüntüsü spektrum, geometri ve su/cam çeşitliliğidir. Font ve shader lisansları Resources/Brand içinde yer alır.

Masaüstü otomasyonu gerçek cihaz testi yerine geçmez. Android launcher maskeleri, dokunmatik ergonomi, haptic genliği, hoparlör sesi, cutout/safe-area, GPU/thermal ve Play pre-launch değerlendirmesi cihaz/Play Console üzerinde tamamlanmalıdır. Final isim clearance'ı bu değişiklikte kilitlenmez.
