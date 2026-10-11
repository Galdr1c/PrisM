# 11 Ekim UI/UX denetimi uygulaması

Kullanıcının denetimi 10 Ekim geliştirme dalına aitti. Bulgular son main ile doğrulandı; 11 Ekim görsel tasarımı ve v3 katalog korundu.

## Uygulanan davranışlar

- Yarım kalan deneyler bölüm/katalog kimliği ve parça türü, konum, açı ile atomik kaydedilir. Yaklaşık 500 ms aralıklı kayıt; bölüm değişimi, pause/focus kaybı ve çıkışta flush. Bozuk/uyumsuz/geometrik olarak geçersiz snapshot yüklenmez. Başlangıç öncesi lifecycle olayları kayıt yazmaz.
- Devam et mevcut deneyi ve önceden tamamlanmış bölümde yarım kalan tekrar denemesini korur. Aktif yarım deney yoksa, son bölüm tamamlanmışsa sıradaki açılmış bölüme; kampanya tamamlanmışsa finale gider. Uygulama yeniden açıldığında undo geçmişi yeniden başlar; öğretici dönüşü aynı Session ve geçmişi korur.
- Boş dokunuş ve 360° eşdeğer dönüş undo adımı oluşturmaz; geçmiş yoksa Geri al pasiftir.
- Sabit basılı tutmak mevcut parçayı hareket ettirmez. Dönüş tutamacı 48 tasarım pikseli hedef alır. Normal dönüş 15° yakınında yapışır; hassas dönüş serbesttir. Açı alanından ±1°/15°/45° seçenekleri açılır.
- Geçersiz deneme, denenen pozda ayrı kırmızı hayaletle görünür; gerçek parça ve ışık son geçerli pozda kalır. Kullanılabilir merkez sınırı sürüklemede çizilir. Bölüm tamamlanması jest bitene kadar bekler.
- Çözümü inceleme ekranı kalıcı ileri/tekrar/harita eylemleri içerir. Gizli tamamlanma HUD'u normal ve native etkileşimden çıkarılır; animasyon dokunma/Back ile geçilir, azaltılmış hareketle kısa tutulur.
- Harita geldiği ekrana döner ve kaydırma konumunu korur. Kilitli düğme önceki gerekli bölümü açıklar. Öğretici bittiğinde/Back ile kapandığında orijinal deneye dönülür.

## Öğrenme ve erişilebilirlik

- Yeşil seçici tek çizgili altıgen; kırmızı seçici çift çizgili elmas. Dünya ve tepsi aynı kimliği kullanır. Geri al ve sıfırla farklı simgelerdir.
- İlk bölümler yerleşim/seçim/hedef durumuna göre görev metni gösterir. Tanıtım gerçek mekaniği anlatır; keşif kaydı metin göründükten sonra yazılır. Duraklatma menüsündeki Optik parçalar açıklamaları tekrar açar.
- Hedef HUD'unda tahtayla aynı sembol, yüzde ve tamamlanma durumu; hedefe dokununca gereken bant ve yeterli/yetersiz ışık açıklaması bulunur.
- Kilitli bilgi metinleri okunabilir opaklıktadır; renk sembolleri yeni kullanıcıda varsayılan açık. Büyük yazı tercihi kalıcıdır; uzun bölüm önizleme adları iki satır/otomatik boyut kullanır.
- Yerleşik accessibility modülü etkinleştirildi. Menü/pencere metinleri, düğme, toggle ve scroll gerçek native hiyerarşiye bağlanır; modal açıkken alttaki HUD dışarıda kalır. Dayanak: [Unity AccessibilityNode belgesi](https://docs.unity.com/en-us/engine/6000.5/script-reference/unityengine/accessibility/accessibilitynode).

İpucu 1→2→3 ve ikinci aşama prizma yön gizleme önceki teslimde zaten düzeltilmişti. Tamamen görmeden uzamsal bulmacayı oynama, fiziksel TalkBack/Narrator QA, kontrollü yakınlaştırma ve bölüm çeşitliliği için kullanıcı araştırması bu kaynak doğrulamasına dahil değildir; tamamlandıkları iddia edilmez.

## Doğrulama

500 çekirdek kontrolü no-op undo, stok/poz doğrulaması ve 100 çözümün snapshot restorasyonunu kapsar. Unity smoke JSON üzerinden yeniden açılma, üç aynanın 37° pozları, 75. bölüm öğretici dönüşü, harita geri eylemi, çözümden ileri geçiş, gizli HUD/native hiyerarşi, sabit basılı tutma, tutamaç hedefi ve hayaletin fiziği değiştirmemesini denetler. Paket ve koşu sonuçları release-handoff belgesine kaydedilir.
