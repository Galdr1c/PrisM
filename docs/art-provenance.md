# Özgün görsel assetler

Arka planlar built-in imagegen ile bu proje için üretildi; üçüncü taraf oyun görseli kullanılmadı. Kaynak PNG'ler `Assets/Prism/Resources/Art` içinde tutulur ve mobilde 1024 maksimum boyut/ASTC sıkıştırma ayarlarıyla içeri alınır.

## Prompt seti

LabSurface: “Square background surface texture for a premium mobile light-optics puzzle game. Seamless-looking top-down dark optical laboratory work surface, restrained deep navy and blue-black anodized metal with extremely subtle fine grain, faint frosted-glass tonal variation, soft cool edge lighting. Flat orthographic full-bleed surface, quiet low-contrast center. No text, logo, symbols, grid, lines, UI, prisms, tools, stars or bright glow. Keep luminance low so gameplay light remains readable.”

WaterSurface: “Square low-contrast background texture for water and refraction chapters. Flat top-down dark blue-green laboratory surface under frosted glass, extremely subtle blurred water caustic patterns and fine matte grain, cool teal edges, almost black quiet center. No objects, UI, grid, lettering, symbols, stars, vivid streaks or bright highlights. Orthographic full-bleed surface.”

MasterySurface: “Square background texture for advanced geometry chapters. Flat orthographic dark indigo-blue optical bench, ultra fine brushed metal grain with faint smoked-glass tonal layering, subtle violet cool edge light and blue-black center. No objects, lettering, numbers, symbols, grid, UI, stars, neon streaks, triangles or rays. Very restrained luminance.”

Optik obje çerçeveleri, ışık demetleri, hedef ve kaynak gövdeleri kod/mesh/shader ile çizilir; bu grafikler çözüm geometrisini değiştirmez.
