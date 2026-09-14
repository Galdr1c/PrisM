# PrisM ilk oynanabilir sürüm

Onaylanan yön: Unity, C#, URP ile mobil optik bulmaca. Referans oynanış incelendi; ilk prototip artık sekiz özgün bölüm içeriyor.

## Kapsam
Kaynak, ayna, üçgen prizma, renk seçici ayna, ince lens, cam küre, su bölgesi, engel ve renk hedefleri. Sınırlı envanterden yerleştirme, sürükleme, yalnızca döndürülebilir parçalarda açı kontrolü, geri alma, sıfırlama, ipucu ve yerel ilerleme kaydı. Fare ve tek parmak aynı etkileşimi kullanır. Çevrimdışı.

## Mimari
Unity bağımsız C# geometri/optik çekirdeği ışık yolları ve hedef enerjilerini üretir. Kaynak genişliği ve yedi spektral bant örneklenir. Prizmada ve su/kürede Snell kırılması ve tam iç yansıma, aynada yansıma, renk seçicide bant ayrımı; lenste paraxial ince lens yaklaşımı kullanılır. Görsel katman hesap sonucunu renkli şerit meshlerle çizer. Bulmaca eşiği tüm grafik ayarlarında aynıdır. Nesne değişince yeniden hesaplama yapılır.

Parça adı, döndürülebilirlik ve seçim yarıçapı gibi etkileşim metadata'sı `PieceInfo` üzerinden tek noktada tutulur. Runtime level kaynağı `ILevelProvider` arayüzüne bağlanacak şekilde ayrılmıştır; mevcut `BuiltInLevelProvider` hard-coded kataloğu kullanır. Sonraki authoring adımı ScriptableObject tabanlı level catalog/editor'dür.

## İlerleme modeli
Her level sıralamadan bağımsız stabil bir `Id` taşır. Tamamlanan level kimlikleri ve son oynanan level `Application.persistentDataPath/progress.json` dosyasında versioned JSON olarak saklanır. Eski `PlayerPrefs` bit-mask kaydı ilk çalıştırmada otomatik taşınır. Bu yapı 32 bölüm sınırını kaldırır ve yaklaşık 70+ level hedefi için uygundur.

## Bölüm ve doğrulama
Her bölüm başlangıç verisi, stok, hedefler ve bilinen çözüm içerir. Testler bilinen çözümlerin bütün hedefleri etkinleştirdiğini, başlangıçların çözülmüş olmadığını, level ID'lerinin benzersiz olduğunu ve optik solver'ın bounce bütçesini aşmadığını doğrular. Çözümün kısa süre sabit kalması tamamlanma sayılır.

## Sonraki görsel aşama
Bu foundation değişikliği görsel renderer'ı bilinçli olarak dönüştürmez. Sonraki ayrı vertical-slice aşamasında HDR/additive beam shader, bloom, Fresnel cam, dispersion edge, su distortion/caustics ve mobil kalite katmanları tek bir referans level üzerinde çözülmelidir.
