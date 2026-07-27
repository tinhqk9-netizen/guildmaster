# Asset Import Presets Report

**Ngày:** 2026-07-24 · **Phase:** Import Settings cho asset tham khảo
**Script:** `Assets/_Game/Scripts/Editor/AssetImportPresets.cs`
**Menu:** `GuildMaster → Assets → Apply Reference Asset Import Presets`

> ❌ Không sửa gameplay/backend · ❌ Không sửa source decode · ❌ Không sửa Production JSON
> ❌ Không gọi Higgsfield · ❌ Không generate asset · ❌ Không map scene/prefab · ❌ Không slice hàng loạt · ❌ Không bắt đầu S5

---

## 🎨 STYLE DIRECTIVE (bắt buộc, áp dụng toàn dự án)

# `STYLE_SOURCE_OF_TRUTH = Assets-tham-khao / FantasyDungeon pack`

Rebuild_GuildMaster **chưa có asset production riêng**, nên pack FantasyDungeon **chính là chuẩn style của toàn bộ game**.

- ✅ Giữ nguyên style gốc của pack (fantasy dungeon, pixel/chunky, đá–gỗ, palette dark slate)
- ❌ Không tự đổi sang style khác
- ❌ **Không trộn drawable gốc (decompile) làm production style** — drawable chỉ để tra ID/concept/mapping
- ❌ Không dùng asset generate lệch tông
- Khi sau này Higgsfield generate phần còn thiếu: **prompt phải lấy style/palette/chất liệu từ pack**, output phải khớp asset pack đã import; **lệch style → reject/retry, không cố map vào game**

---

## Import Preset Summary

| Group | Folder | Count | Texture Type | Filter | Compression | PPU | Notes |
|---|---|---|---|---|---|---|---|
| Icons | `Art/Icons` | 224 | Sprite (2D and UI) | **Point** | **None** | **64** | Icon native 64×64 → 1 unit = 1 icon |
| Tilesets | `Art/Tilesets` (+`environment`) | 65 | Sprite (2D and UI) | Point | None | **128** | Tile native 128×128 → 1 unit = 1 tile |
| Characters | `Art/Characters` (+`hero_skins`) | 20 | Sprite (2D and UI) | Point | None | **256** | Sheet 1024² = 4×4 cell 256px → 1 unit = 1 cell |
| Enemies | `Art/Enemies` | 90 | Sprite (2D and UI) | Point | None | **256** | Như trên |
| Portraits | `Art/Portraits` | 8 | Sprite (2D and UI) | Point | None | **256** | Portrait native 256×256 |
| UI | `Art/UI` | 2 | Sprite (2D and UI) | Point | None | **100** | **Lựa chọn có chủ đích:** UI được scale bởi `CanvasScaler` (reference 1080×1920), không phải bởi PPU → giữ 100 (mặc định Unity) để không lệch layout |
| VFX | `Art/VFX` | 7 | Sprite (2D and UI) | Point | None | **256** | **Provisional** — frame không đều, chốt lại ở slice plan |
| | | **416** | | | | | |

**Cài đặt chung áp cho tất cả:**
- `Texture Type = Sprite (2D and UI)`
- `Filter Mode = Point (no filter)` — theo yêu cầu README pack
- `Compression = None (Uncompressed)` — cả default lẫn platform settings
- `Generate Mip Maps = Off`
- `Wrap Mode = Clamp`
- `Alpha Is Transparency = On` **chỉ khi** ảnh thật sự có alpha (`DoesSourceTextureHaveAlpha()`)
- `Sprite Mesh Type = Full Rect` (an toàn cho UI + tránh méo icon)
- `Sprite Alignment = Center`
- `Max Size` = power-of-two nhỏ nhất **≥ kích thước gốc** → **không bao giờ giảm chất lượng**

**Lý do bắt buộc có preset** — kiểm chứng thực tế trên `.meta` sau khi Unity auto-import:
```
filterMode: 1            → Bilinear   ❌ (làm mờ pixel-art)
textureCompression: 1    → Compressed ❌ (gây nhiễu/banding)
spritePixelsToUnits: 100 → sai cho icon 64px
```
→ Mặc định Unity **làm hỏng toàn bộ 416 asset pixel-art**. Script này sửa đúng vấn đề đó.

**Quyết định có chủ đích:** `Sprite Mode` giữ **Single** cho *tất cả*, kể cả sheet.
Lý do: set `Multiple` mà chưa có slice data sẽ khiến sheet **không sinh ra sprite nào** → tệ hơn hiện tại. Việc chuyển sang Multiple + slice 4×4 thuộc **slice plan**, đúng ràng buộc "không slice hàng loạt nếu chưa report rõ quy tắc".

---

## Files Processed

Script quét `t:Texture2D` trong 7 folder dưới `Assets/_Game/Art` và áp preset theo folder:

| Folder | File |
|---|---|
| `Art/Icons` | 224 |
| `Art/Tilesets` (+ `environment`) | 65 |
| `Art/Enemies` (15 subfolder) | 90 |
| `Art/Characters` (hero, merchant, villager, hero_skins) | 20 |
| `Art/Portraits` | 8 |
| `Art/VFX` | 7 |
| `Art/UI` | 2 |
| **Tổng** | **416** |

Sau khi chạy: `AssetDatabase.StartAssetEditing()` → áp preset → `SaveAndReimport()` → `StopAssetEditing()` → `AssetDatabase.Refresh()` → log tổng hợp theo nhóm ra Console.

---

## Skipped / Needs Manual Review

Các asset **vẫn được áp import preset** (chúng vẫn cần Point/No-compression), nhưng **còn khiếm khuyết cần người xử lý** — script tự log ra Console:

| Asset | Vấn đề | Việc cần làm |
|---|---|---|
| `UI/ui_kit.png` | Atlas tổng hợp, nền slate đặc (0% alpha) | Cắt element rời + set **9-slice border** thủ công |
| `UI/ui_dialog.png` | **Mockup có chữ tiếng Anh bake cứng** | Chỉ thu hoạch element rời, không dùng nguyên tấm |
| `Characters/hero_skins/crimson.png` | **Nền magenta chroma-key chưa tách** (alpha 2.2%) | Remove background |
| `Characters/hero_skins/mage.png` | **Nền magenta chưa tách** (alpha 0.1%) | Remove background |
| `Characters/hero_skins/darkknight.png` | **Nền magenta chưa tách** (alpha 21%) | Remove background |
| `Characters/hero_skins/paladin.png` | **Viền hào quang trắng bẩn** | Làm sạch matte |
| `VFX/vfx_heal.png` | **Bake nền tile đá** dưới hiệu ứng | Crop bỏ nền |
| `VFX/vfx_levelup.png` | **Bake nền tile đá + chữ "LEVEL UP!"** | Crop bỏ nền, tránh frame có chữ |
| `Portraits/*.png` (8) | **RGB, không alpha** — nền phẳng | Tách nền *nếu* cần alpha; nếu dùng trong khung card thì giữ nguyên |

---

## Asset Suitability Review

Đánh giá theo 5 tiêu chí (Gameplay fit · Style fit · Mobile readability · Technical usability · Mapping confidence).
**Không asset nào được coi là production-ready chỉ vì nó tồn tại trong pack.**

| Asset / Group | Intended Use | Gameplay Fit | Style Fit | Mobile Readability | Technical Usability | Mapping Confidence | Decision |
|---|---|---|---|---|---|---|---|
| **Icons — vũ khí/giáp/khiên** (~160) | Item & equipment icon | ✅ Đúng vai trò | ✅ Chuẩn pack | ✅ Silhouette rõ ở cỡ nhỏ | ✅ Alpha sạch, 64px, không cần xử lý | ~95% (slot UI chưa tồn tại) | **USE_NOW** |
| **Icons — spell** (48) | Skill icon | ✅ Khớp `SkillDefinition` | ✅ | ✅ | ✅ | ~90% (chưa map skill ID) | **USE_NOW** |
| **Icons — coin/gem/key** | Currency & reward | ✅ | ✅ | ✅ | ✅ | ~95% | **USE_NOW** |
| **Tilesets** (65) | Dungeon/area scene | ✅ | ✅ | ✅ | 🟡 Vài file là sheet gộp cần slice | ~85% (chưa có scene dungeon) | **PROCESS_THEN_USE** |
| **Enemy sheets** (90) | Enemy combat sprite | ✅ Khớp `EnemyDefinition` | ✅ | ✅ | 🟡 Cần slice 4×4; idle game có thể chỉ cần hàng "down" | ~80% | **PROCESS_THEN_USE** |
| **Character sheets** (12) | Hero/NPC sprite | ✅ | ✅ | ✅ | 🟡 Cần slice 4×4 | ~80% | **PROCESS_THEN_USE** |
| **hero_skins — assassin, frost, ranger, royal** | Class/skin adventurer | ✅ | ✅ | ✅ | 🟡 2048² quá lớn, cần crop/downscale | ~75% | **PROCESS_THEN_USE** |
| **hero_skins — crimson, mage, darkknight** | Class/skin adventurer | ✅ | ✅ | ✅ | ❌ **Nền magenta chưa tách** | Thấp | **NEEDS_MANUAL_REVIEW** |
| **hero_skins — paladin** | Class/skin adventurer | ✅ | ✅ | ✅ | ❌ Viền trắng bẩn | Thấp | **NEEDS_MANUAL_REVIEW** |
| **Portraits** (8) | Portrait character/enemy/NPC | ✅ | ✅ | ✅ | 🟡 Không alpha (nền phẳng) | ~70% | **READY_TO_MAP** |
| **UI kit** (`ui_kit.png`) | Panel/button/slot/HUD/9-slice | ✅ **Nguồn UI chrome chính** | ✅ Chuẩn tuyệt đối (đây là style gốc) | ✅ | ❌ Cần cắt element + tách nền + 9-slice | Thấp khi chưa cắt | **NEEDS_MANUAL_REVIEW** |
| **UI dialog** (`ui_dialog.png`) | Tham chiếu layout | 🟡 Chỉ tham chiếu | ✅ | ❌ **Text bake cứng** → không dùng runtime | ❌ Phải cắt element | Thấp | **NEEDS_MANUAL_REVIEW** |
| **VFX — fire, hit, lightning, slash, glow** | Combat effect | ✅ | ✅ | ✅ | 🟡 Cần slice strip (frame không đều) | ~70% | **PROCESS_THEN_USE** |
| **VFX — heal, levelup** | Combat effect | ✅ | 🟡 | ❌ Dính chữ "LEVEL UP!" | ❌ Bake nền tile + chữ | Thấp | **NEEDS_MANUAL_REVIEW** |
| **Pet art** | Pet display | — | — | — | ❌ **Pack có 0 file** | — | **HIGGSFIELD_LATER** |
| **Buildings** | Guild hall/shop/forge | — | — | — | ❌ **Pack có 0 file** | — | **HIGGSFIELD_LATER** |
| **Area background hoàn chỉnh** | Nền area/dungeon | — | — | — | ❌ Pack chỉ có tile rời | — | **HIGGSFIELD_LATER** |
| **Splash / loading / title / menu bg** | Presentation | — | — | — | ❌ Không có trong pack | — | **HIGGSFIELD_LATER** |
| **Rarity frame 5 bậc** | Item rarity | ✅ | ✅ | ✅ | 🟡 Thử cắt từ ui_kit trước | — | **HIGGSFIELD_LATER** (chỉ nếu ui_kit không đủ) |
| **Audio** | SFX/BGM | — | — | — | ❌ Không có trong pack | — | **HIGGSFIELD_LATER** |
| **Drawable gốc (decompile)** | — | ❌ | ❌ **Vi phạm style directive** | — | — | — | **DO_NOT_USE** (chỉ tra ID/concept) |

---

## Unity Compile

- **UNITY_COMPILE:** ✅ `PASS`
- **Total errors:** `0`
- **Errors caused by `AssetImportPresets.cs`:** `0` — không có exception, không có warning

Nguồn verify: `C:\Users\EDITNGOCMINH\AppData\Local\Unity\Editor\Editor.log`
```
grep "error CS[0-9]+"                     → 0 kết quả
grep "ArgumentException|NullReference|Failed to import" → 0 kết quả
```
> Ghi chú: MCP Unity vẫn timeout tại thời điểm verify (main thread bận), nên đã verify bằng cách **đọc trực tiếp `Editor.log` + parse `.meta` trên đĩa** — nguồn dữ liệu chính xác hơn cả Console UI.

**Log thực tế script xuất ra:**
```
[AssetImportPresets] Applied pixel-art import presets:
  Icons         224 file(s)   PPU 64    — icons are 64x64 native
  Tilesets       65 file(s)   PPU 128   — tiles are 128x128 native
  Characters     20 file(s)   PPU 256   — 1024x1024 sheet = 4x4 grid of 256px cells
  Enemies        90 file(s)   PPU 256   — 1024x1024 sheet = 4x4 grid of 256px cells
  Portraits       8 file(s)   PPU 256   — portraits are 256x256 native
  UI              2 file(s)   PPU 100   — UI is scaled by CanvasScaler, not PPU
  VFX             7 file(s)   PPU 256   — provisional
  TOTAL         416 file(s)
  Needs manual review (8): [crimson, darkknight, mage, paladin, ui_dialog, ui_kit, vfx_heal, vfx_levelup]
```
→ **416/416 xử lý, 0 skipped, 8 flagged đúng như dự kiến.**

---

## Preset Application Verification

Đọc trực tiếp `.meta` sau khi apply (1 file mẫu/nhóm):

| Group | Sample file | Filter | Compression | MipMap | PPU | Status |
|---|---|---|---|---|---|---|
| UI | `ui_kit.png` | Point (no filter) | None (Uncompressed) | off | 100 | ✅ PASS |
| Icons | `coin.png` | Point (no filter) | None (Uncompressed) | off | 64 | ✅ PASS |
| Characters | `hero_idle_sheet.png` | Point (no filter) | None (Uncompressed) | off | 256 | ✅ PASS |
| Enemies | `skeleton_attack_sheet.png` | Point (no filter) | None (Uncompressed) | off | 256 | ✅ PASS |
| Portraits | `hero.png` | Point (no filter) | None (Uncompressed) | off | 256 | ✅ PASS |
| Tilesets | `wall_a.png` | Point (no filter) | None (Uncompressed) | off | 128 | ✅ PASS |
| VFX | `vfx_fire.png` | Point (no filter) | None (Uncompressed) | off | 256 | ✅ PASS |

**Chi tiết đầy đủ theo nhóm:**

| Group | Texture Type | Sprite Mode | Alpha Is Transparency | Max Size (Default platform) |
|---|---|---|---|---|
| UI | Sprite (2D and UI) | Single | no *(ui_kit RGBA nhưng 0% trong suốt → Unity báo không có alpha thật)* | 2048 |
| Icons | Sprite (2D and UI) | Single | **yes** | 64 |
| Characters | Sprite (2D and UI) | Single | **yes** | 1024 |
| Enemies | Sprite (2D and UI) | Single | **yes** | 1024 |
| Portraits | Sprite (2D and UI) | Single | no *(nguồn RGB, không có kênh alpha)* | 256 |
| Tilesets | Sprite (2D and UI) | Single | **yes** | 128 |
| VFX | Sprite (2D and UI) | Single | **yes** | 2048 |

→ `Alpha Is Transparency` bật/tắt **đúng theo thực tế từng ảnh** (`DoesSourceTextureHaveAlpha()`), không bật bừa.
→ `Max Size` khớp đúng kích thước gốc từng nhóm → **không hạ chất lượng ảnh nào**.

### Quét toàn bộ 416 file (không chỉ mẫu)

```
total metas scanned      : 416
NOT Point filter         : 0
NOT Uncompressed         : 0
MipMaps still ON         : 0
NOT Sprite(2D and UI)    : 0
>>> ALL CLEAN <<<

PPU distribution:
  Icons {64: 224}   Tilesets {128: 65}   Characters {256: 20}
  Enemies {256: 90} Portraits {256: 8}   VFX {256: 7}   UI {100: 2}
```

### Xác nhận các rule bắt buộc

| Rule | Kết quả |
|---|---|
| Pixel-art không còn Bilinear | ✅ 0/416 file còn Bilinear |
| Không còn Compression cho asset pixel-art | ✅ 0/416 file còn Compressed |
| Mip Maps off | ✅ 0/416 file còn bật mipmap |
| Sprite import usable | ✅ Toàn bộ là `Sprite (2D and UI)` + `Single` → sinh sprite dùng được ngay |
| Không sửa backend | ✅ File gameplay `.cs` mới nhất vẫn là **2026-07-23 09:39**; hôm nay chỉ có `Editor/AssetImportPresets.cs` |
| Không slice / map / generate | ✅ Sprite Mode vẫn `Single`, không đụng scene/prefab, không gọi Higgsfield |

### ⚠️ Một điểm cần theo dõi (chưa phải lỗi)

Trong `.meta` có sẵn block override cho **Standalone** và **Android** với `maxTextureSize: 2048, textureCompression: 1 (Compressed)`.
**Hiện tại vô hại** vì cả hai đều có **`overridden: 0`** → block trơ, `DefaultTexturePlatform` (Point + Uncompressed) mới là setting có hiệu lực.

**Rủi ro tương lai:** dự án target **Android mobile**. Nếu sau này ai đó tick ô override "Android" trong Inspector, toàn bộ pixel-art sẽ bị nén lại. Khi tới bước build, nên bổ sung set `overridden` cho platform Android với Uncompressed — **ghi nhận cho sau, không xử lý ở phase này** (ngoài scope).

---

## FINAL DECISION

# `IMPORT_PRESETS_APPLIED_READY_FOR_SLICE_PLAN`

Preset đã áp **thành công và đã được verify độc lập** trên cả 416 asset:
- ✅ Compile PASS, 0 lỗi, 0 exception
- ✅ 416/416 đúng Point + Uncompressed + Mip off + Sprite (2D and UI)
- ✅ PPU đúng theo native size từng nhóm; Max Size không hạ chất lượng
- ✅ Backend nguyên vẹn, không slice/map/generate
- ✅ 8 asset khiếm khuyết đã được flag rõ để xử lý tay

**Bước hợp lý tiếp theo (chưa làm, chờ bạn quyết):** lập **Slice Plan** — quy tắc slice cho 102 sheet nhân vật (4×4, cell 256px, mỗi hàng = 1 hướng), các sheet gộp trong `Tilesets/environment`, và 6 VFX strip; kèm quyết định *idle game có cần đủ 4 hướng không*.

**Đã dừng.** Không slice, không map, không generate.
