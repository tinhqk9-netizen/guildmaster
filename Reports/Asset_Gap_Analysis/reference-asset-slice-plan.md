# Reference Asset Slice Plan

**Ngày:** 2026-07-24 · **Phase:** Slice Planning (chưa slice thật)
**Style source of truth:** `Assets-tham-khao / FantasyDungeon pack`
**Target:** mobile portrait 1080×1920

> ❌ Không slice ngay · ❌ Không map scene/prefab · ❌ Không generate · ❌ Không gọi Higgsfield
> ❌ Không sửa gameplay/backend · ❌ Không bắt đầu S5
> ✅ Toàn bộ số liệu dưới đây **đo trực tiếp từ file thật**, không suy đoán

---

## Slice Scope

| Asset group | Source files | Slice needed | Reason |
|---|---|---|---|
| Icons | 224 × 64×64 | **NO** | 1 file = 1 icon, alpha sạch, dùng thẳng |
| Portraits | 8 × 256×256 | **NO** | 1 file = 1 portrait |
| Tileset — tile đơn | **57** × 128×128 | **NO** | 1 file = 1 tile |
| Tileset — sheet gộp | **8** × 1024×1024 | **YES** | Grid đều **8×8 @128px** |
| Character sheets | 12 × 1024×1024 | **YES** | Grid đều **4×4 @256px** |
| Enemy sheets | 90 × 1024×1024 | **YES** | Grid đều **4×4 @256px** |
| hero_skins | 8 × 2048×2048 | **NO** (nhưng cần crop) | Ảnh đơn, không phải sheet; 4 file lỗi nền |
| VFX — `glow_warm` | 1 × 128×128 | **NO** | 1 frame duy nhất (bbox 6,6→123,123) |
| VFX — strip | 6 × 2048×2048 | **YES (khó)** | Frame **không đều** → không grid-slice được |
| UI kit / dialog | 2 × 2048×2048 | **YES (thủ công)** | Nền đặc, bbox = full canvas → **auto-slice bất khả thi** |

---

## Character / Enemy Sheet Plan

### Xác nhận bằng đo đạc
- ✅ **102/102 file đúng 1024×1024** (không có ngoại lệ nào)
- ✅ **Grid 4×4, cell 256×256** — xác nhận bằng phân tích độ phủ alpha từng ô
- ✅ **Cả 16 ô đều có nội dung** (không ô rỗng)

### Hàng nào là hướng nào (xác nhận trực quan trên `hero_walk_sheet.png`)
| Row | Hướng | Dấu hiệu nhìn thấy |
|---|---|---|
| **row 0** | **DOWN / front** | Nhìn thẳng vào camera, thấy mặt qua khe mũ |
| **row 1** | **UP / back** | Thấy gáy + lưng áo choàng, không thấy mặt |
| **row 2** | **LEFT** | Nghiêng trái, áo choàng đổ về bên phải sprite |
| **row 3** | **RIGHT** | Nghiêng phải, áo choàng đổ về bên trái sprite |

Cột = frame animation, trái → phải.

### Frame thực tế (đã kiểm bằng so sánh pixel)
- `*_idle_sheet`: 4 frame, **1 frame lặp** (kiểu bob A-B-A-C) → vẫn là animation thật
- `*_walk_sheet`, `*_attack_sheet`: **4 frame khác nhau hoàn toàn**
- ⚠️ `merchant_sheet.png` **row3 độ phủ chỉ 10.7–11.5%** so với 23–27% các hàng khác → nội dung khác thường, cần nhìn tay trước khi dùng
- ⚠️ `merchant` và `villager` chỉ có **3 sheet** (`sheet`, `idle`, `walk`), không đủ 6 như các nhân vật khác

### Đề xuất: dùng đủ 4 hướng hay chỉ hàng down?

**⚠️ Đính chính quan trọng so với nhận định trước của tôi:** slice ít ô hơn **KHÔNG tiết kiệm bộ nhớ**. Sprite trong Unity chỉ là metadata hình chữ nhật trỏ vào texture; texture 1024×1024 vẫn nạp nguyên vẹn dù bạn định nghĩa 4 hay 16 sprite. Ý "bỏ 75% sprite thừa" trước đó là **sai về mặt bộ nhớ**.

**Khuyến nghị:**
- **Slice đủ 16 ô** — chi phí bằng 0, làm một lần, giữ nguyên tùy chọn về sau
- **Chỉ tạo AnimationClip cho hướng thật sự dùng.** Guild Master là **idle RPG portrait**, nhiều khả năng chỉ cần:
  - **row0 (down/front)** → card nhân vật, roster, portrait, danh sách adventurer
  - **row3 (right)** hoặc **row2 (left)** → combat side-view nếu có
- Nếu thật sự cần giảm dung lượng texture, cách đúng là **repack** riêng các hướng cần dùng thành atlas mới (việc lớn hơn, để sau), **không phải** slice ít đi.

### Naming convention sau slice
Unity mặc định đặt `hero_walk_sheet_0 … _15` — vô nghĩa. Đề xuất đổi thành:

```
<actor>_<anim>_<dir>_<frame>

hero_walk_down_00   hero_walk_down_01   hero_walk_down_02   hero_walk_down_03
hero_walk_up_00     …
hero_walk_left_00   …
hero_walk_right_00  …
```
Ánh xạ index → tên: `index = row*4 + col`, `dir = [down, up, left, right][row]`, `frame = col`.

**Pivot đề xuất:** `Bottom` cho character/enemy (chân chạm đất, tránh giật khi đổi frame). Nếu chỉ dùng trong UI card thì `Center` cũng được — nhưng nên thống nhất **Bottom** để dùng được cả 2 nơi.

---

## UI Kit Slice Plan

### Ràng buộc kỹ thuật đã đo
`ui_kit.png` và `ui_dialog.png` đều 2048×2048, **bbox = (0,0,2048,2048)** vì nền slate đặc (0% pixel trong suốt).
→ **Unity auto-slice (Automatic/Grid) sẽ thất bại hoàn toàn.** Bắt buộc **cắt tay** + tách nền.

### `ui_kit.png` — element nên cắt

| Element | Dùng 9-slice? | Ghi chú |
|---|---|---|
| Panel INVENTORY (khung ngoài + title bar) | ✅ **CÓ** | Khung chính cho popup/screen |
| Panel ITEMS (lưới) | ✅ CÓ | |
| **Bộ mảnh 9-SLICE PANELS** (3 biến thể: trơn, gạch, tối) | ✅ **CÓ — ưu tiên cao nhất** | Pack đã cắt sẵn góc/cạnh/giữa → nguồn 9-slice chuẩn nhất |
| Khung **TITLE / PANEL / FRAME** (3 cái) | ✅ CÓ | Header gỗ / header đá / khung trơn |
| **Ô slot** (ô lưới đá trong panel ITEMS) | ❌ Không | Kích thước cố định → sprite đơn |
| Thanh **HP (đỏ) / MP (xanh) / XP (vàng)** + đầu bịt | ✅ CÓ (phần fill) | Tách riêng: khung bar, phần fill, icon đầu (tim/ngọc/XP) |
| Nút icon (kiếm, khiên, potion, túi, bánh răng) | ❌ Không | Sprite đơn |
| Nút chữ **OK / CANCEL / EQUIP / USE** | ✅ CÓ | ⚠️ **có chữ bake** → chỉ lấy **nền nút** 4 màu (lục/đỏ/lam/xám), chữ để runtime |
| Nút mũi tên ◄ ► ▲ ▼ | ❌ Không | Sprite đơn |
| Thanh ngang dài (khung gem xanh) | ✅ CÓ | Tooltip / thanh thông báo |
| Thanh nhỏ (gỗ, vàng, gem) 4 cái | ✅ CÓ | Divider / progress |
| Divider hoa văn | ❌ Không | Trang trí |
| **Banner** (lam / đỏ / lục) | ❌ Không | Huy hiệu guild/faction |
| Khung octagon (skull, gem) | ❌ Không | Khung avatar/rarity — **ứng viên cho rarity frame** |
| Rương, kiếm-khiên bắt chéo, cửa, cầu thang, nước, sàn | ❌ Không | Icon trang trí |

### `ui_dialog.png` — phân loại
| Nhóm | Quyết định |
|---|---|
| Toàn bộ layout (dialog, Merchant, Quests, HUD) | **Chỉ làm reference layout** — **KHÔNG dùng**, vì chữ tiếng Anh bake cứng ("Merchant", "Quests", "Health Potion"…) |
| **9 icon nav tròn** (Character, Inventory, Map, Quests, Skills, Options, Save, Load, Quit) | ✅ **Cắt dùng được** — nhưng chỉ lấy **phần icon tròn**, bỏ chữ label bên dưới |
| Nút **X** đóng (đỏ) | ✅ Cắt dùng |
| Tab **Active / Completed** | ✅ Cắt **nền tab** (bỏ chữ) |
| Mũi tên scrollbar ▲▼ + rãnh | ✅ Cắt dùng |
| Ô **hotbar** (8 ô có số) | ✅ Cắt **ô trống** (bỏ số) |
| Khung portrait (vòm đá) | ✅ Cắt dùng |
| Thanh HP/MP/Stamina + khung level | ✅ Cắt (bỏ số) |

### Folder output đề xuất
```
Assets/_Game/Art/UI/Elements/
├── Panels/     ui_panel_window, ui_panel_inventory, ui_panel_9slice_stone, …
├── Buttons/    ui_btn_primary_normal, ui_btn_danger_normal, ui_btn_arrow_left, ui_btn_close_x, …
├── Slots/      ui_slot_empty, ui_slot_hotbar, ui_frame_octagon, …
├── Bars/       ui_bar_frame, ui_bar_fill_hp, ui_bar_fill_mp, ui_bar_fill_xp, ui_bar_icon_heart, …
├── Frames/     ui_frame_title_wood, ui_frame_portrait_arch, ui_tab_bg, …
└── NavIcons/   ui_nav_character, ui_nav_inventory, ui_nav_map, …
```
Giữ `ui_kit.png` / `ui_dialog.png` nguyên bản làm **source of truth**, không sửa đè.

---

## Tileset / Environment Slice Plan

### Tile đơn — **57 file**, không cần slice
Toàn bộ đúng 128×128: tường (`wall_a/b`, `wall_crack`, `wall_moss`, `wall_torch`, `wall_pillar`, `wall_spikes`, `skull_wall`, `coffin_wall`, `crystal_wall`, `cave_wall`), sàn (`floor_plain/cracked/mossy/sand/plank/light`, `bone_floor`, `dirt_floor`, `cave_floor`), cửa (`door`, `door_wood_open/closed`, `gate_iron`, `portcullis_open/closed`, `archway`), bẫy (`spikes_0/1/2`, `bear_trap`, `arrow_trap`, `pressure_plate`), `lever_up/down`, `stairs`, `stairs_down`, `lava`, `water`, `under_water`, `brazier_big`, props lẻ (`barrel`, `crate`, `pot`, `candle`, `chest_open`, `cobweb`, `blood`, `bones`, `skull`, `moss`, `rubble`, `banner`).

### Sheet gộp — **8 file**, grid **8×8 @128px** (đo được, hoàn toàn đều)
| File | Grid | Nội dung |
|---|---|---|
| `environment/props.png` | 8×8 @128 | Props tổng hợp |
| `environment/props2.png` | 8×8 @128 | Props bổ sung |
| `environment/props2b.png` | 8×8 @128 | Props biến thể |
| `environment/traps.png` | 8×8 @128 | Bẫy các loại |
| `environment/animated_tiles.png` | 8×8 @128 | Tile động |
| `environment/water_anim.png` | 8×8 @128 | Nước động |
| `environment/brazier_anim.png` | 8×8 @128 | Lửa động |
| `environment/deco_shadows.png` | 8×8 @128 | Bóng đổ |

→ **Đây là nhóm dễ slice nhất**: `Grid By Cell Size 128×128`, hoàn toàn xác định.
⚠️ Ô rỗng sẽ sinh sprite trống → bật **"Trim"**/xoá thủ công sprite rỗng sau slice.

### Naming / folder
```
Art/Tilesets/                      (57 tile đơn — giữ nguyên tên)
Art/Tilesets/environment/          (8 sheet gốc — giữ nguyên)
→ sau slice: props_00 … props_63   (đổi tên theo ngữ nghĩa khi đã nhìn rõ từng ô)
```
Pivot: `Center` cho tile; PPU đã set 128 → **1 tile = 1 unit**.

---

## VFX Slice Plan

### Số liệu đo được
| File | Size | Frame phát hiện | Độ rộng từng frame | Kết luận |
|---|---|---|---|---|
| `glow_warm.png` | 128×128 | **1** | 97 | ✅ Sprite đơn, **không slice** |
| `vfx_fire.png` | 2048² | 6 | 291, 266, 327, 366, 113, 227 | ⚠️ Không đều |
| `vfx_lightning.png` | 2048² | 4 | 220, 343, 382, 470 | ⚠️ Không đều (tăng dần) |
| `vfx_slash.png` | 2048² | 5 | 297, 442, 555, 326, **19** | ⚠️ Frame cuối 19px = mảnh vụn |
| `vfx_hit.png` | 2048² | **9** | 172,233,270,115,114,86,**19,30,59** | ❌ Hạt bắn ra bị tách thành nhiều mảnh giả |
| `vfx_heal.png` | 2048² | 5 | 272, 274, 280, 281, 320 | ❌ **Bake nền tile đá** |
| `vfx_levelup.png` | 2048² | 5 | 280, 288, 295, 296, 321 | ❌ **Bake nền tile đá + chữ "LEVEL UP!"** |

### Kết luận VFX
- ❌ **Không grid-slice được** — độ rộng frame chênh nhau 19–555px
- Phải dùng **Sprite Editor → Automatic slice** rồi **sửa tay**, đặc biệt `vfx_hit` (auto sẽ tách 9+ mảnh thay vì 4–5 frame)
- Canvas 2048² nhưng nội dung chỉ nằm trong dải hẹp (vd `vfx_fire` bbox y = 791→1204) → **rất phí**, nên crop trước
- **Dùng được:** `glow_warm` (ngay), `vfx_fire`, `vfx_lightning`, `vfx_slash` (sau khi sửa tay)
- **Không nên dùng khi chưa cleanup:** `vfx_heal`, `vfx_levelup`

---

## Manual Review / Cleanup Needed

| Asset | Vấn đề | Việc cần làm | Chặn slice? |
|---|---|---|---|
| `UI/ui_kit.png` | Nền đặc, atlas tự do | Cắt tay từng element + tách nền + set 9-slice border | ✅ Có |
| `UI/ui_dialog.png` | Mockup, chữ EN bake cứng | Chỉ thu hoạch element rời; không dùng nguyên tấm | ✅ Có |
| `Characters/hero_skins/crimson.png` | **Nền magenta chưa tách** (alpha 2.2%) | Chroma-key remove | ✅ Có |
| `Characters/hero_skins/mage.png` | **Nền magenta chưa tách** (alpha 0.1%) | Chroma-key remove | ✅ Có |
| `Characters/hero_skins/darkknight.png` | **Nền magenta chưa tách** (alpha 21%) | Chroma-key remove | ✅ Có |
| `Characters/hero_skins/paladin.png` | Viền hào quang trắng | Làm sạch matte | ✅ Có |
| `VFX/vfx_heal.png` | Bake nền tile đá | Crop bỏ nền | ✅ Có |
| `VFX/vfx_levelup.png` | Bake nền tile đá + chữ "LEVEL UP!" | Crop bỏ nền, loại frame có chữ | ✅ Có |
| `Portraits/*.png` (8) | RGB, không alpha | Tách nền **chỉ nếu** cần alpha; dùng trong khung card thì giữ nguyên | ❌ Không |
| `Characters/merchant/merchant_sheet.png` | row3 độ phủ bất thường (10.7%) | Nhìn tay xác định nội dung row3 | ⚠️ Nên xem trước |

---

## Optimization Decision

| Nhóm | Quyết định |
|---|---|
| Icons (224) | **READY_TO_MAP_WITHOUT_SLICE** |
| Portraits (8) | **READY_TO_MAP_WITHOUT_SLICE** *(tách nền chỉ khi cần alpha)* |
| Tileset — tile đơn (57) | **READY_TO_MAP_WITHOUT_SLICE** |
| Tileset — sheet gộp (8) | **SLICE_NOW** *(8×8 @128, xác định tuyệt đối)* |
| Character sheets (12) | **SLICE_NOW** *(4×4 @256)* |
| Enemy sheets (90) | **SLICE_NOW** *(4×4 @256)* |
| `merchant_sheet.png` | **MANUAL_REVIEW_FIRST** *(row3 bất thường)* |
| hero_skins — assassin, frost, ranger, royal | **READY_TO_MAP_WITHOUT_SLICE** *(crop/downscale là tối ưu tùy chọn)* |
| hero_skins — crimson, mage, darkknight, paladin | **MANUAL_REVIEW_FIRST** |
| VFX — `glow_warm` | **READY_TO_MAP_WITHOUT_SLICE** |
| VFX — fire, lightning, slash | **PROCESS_THEN_SLICE** *(auto-slice + sửa tay)* |
| VFX — `vfx_hit` | **PROCESS_THEN_SLICE** *(auto tách sai, phải gộp tay)* |
| VFX — heal, levelup | **MANUAL_REVIEW_FIRST** |
| `ui_kit.png` | **MANUAL_REVIEW_FIRST** *(nguồn UI chrome chính — giá trị cao nhất)* |
| `ui_dialog.png` | **MANUAL_REVIEW_FIRST** *(reference + thu hoạch element)* |
| Drawable gốc (decompile) | **DO_NOT_USE** *(vi phạm style directive)* |
| Pet / Buildings / Area bg / Splash / Audio | **HIGGSFIELD_LATER** |

---

## Implementation Proposal

### Có cần Editor script slice tự động không?
**CÓ — nhưng chỉ cho phần grid đều.**

| Nhóm | Cách làm | Lý do |
|---|---|---|
| 102 char/enemy sheet + 8 tileset sheet | ✅ **Editor script** (`SpriteSheetSlicer.cs`) | Grid xác định tuyệt đối (4×4@256 và 8×8@128), 110 file — set tay là không khả thi. Script sẽ đặt `spriteImportMode = Multiple`, sinh `SpriteMetaData` theo grid, đặt tên theo convention `<actor>_<anim>_<dir>_<frame>`, pivot Bottom |
| 6 VFX strip | ❌ **Thủ công** trong Sprite Editor | Frame không đều (19–555px), auto-slice tách sai |
| ui_kit / ui_dialog | ❌ **Thủ công** | Nền đặc, layout tự do, cần 9-slice border theo từng element |

### Thứ tự batch đề xuất
1. **Batch 1 — 102 char/enemy sheet** (4×4 @256). Xác định nhất, giá trị cao nhất, không phụ thuộc cleanup.
2. **Batch 2 — 8 tileset sheet** (8×8 @128). Cũng xác định tuyệt đối; dọn sprite rỗng sau khi slice.
3. **Batch 3 — cleanup 4 hero_skins + 2 VFX** (tách nền magenta, crop). Chặn các bước sau.
4. **Batch 4 — VFX strip** slice tay sau khi cleanup.
5. **Batch 5 — cắt `ui_kit`** thành UI Elements + set 9-slice. Công sức lớn nhất nhưng **quyết định chất lượng UI toàn game**.

### Risk nếu slice sai
| Rủi ro | Hậu quả | Giảm thiểu |
|---|---|---|
| Sai cell size | Sprite cắt ngang người | Đã đo và xác nhận 256/128 trên **toàn bộ** file, không suy đoán |
| Sai pivot | Nhân vật giật/lệch chân khi đổi frame | Thống nhất **Bottom** cho character, **Center** cho tile/icon |
| Slice lại đè metadata cũ | Mất tham chiếu sprite đã dùng trong prefab/anim | **Hiện chưa map gì cả → slice bây giờ là an toàn nhất.** Slice TRƯỚC khi map |
| Auto-slice VFX | Tách hạt vụn thành hàng chục sprite rác | Không auto-slice VFX; làm tay |
| Grid-slice ui_kit | Cắt nát UI, vô dụng hoàn toàn | Cấm auto-slice cho UI; chỉ cắt tay |
| Ô rỗng trong tileset sheet | Sinh sprite trống rác | Bật Trim / xoá sprite rỗng sau slice |
| Đặt tên mặc định `_0.._15` | Không tra được sprite nào là hướng nào | Áp naming convention ngay trong script |

---

## FINAL DECISION

# `READY_FOR_SLICE_BATCH1`

**Lý do:** Batch 1 (102 char/enemy sheet) và Batch 2 (8 tileset sheet) đã được **đo và xác nhận trên 100% file**:
- 102/102 sheet đúng 1024×1024, grid 4×4 @256, cả 16 ô có nội dung
- Hướng từng hàng đã **xác nhận trực quan** (down/up/left/right)
- 8/8 tileset sheet đúng 1024×1024, grid 8×8 @128
- Chưa map gì vào prefab/scene → slice lúc này **không phá vỡ tham chiếu nào**

Các nhóm cần cleanup (hero_skins, VFX, ui_kit) **không chặn Batch 1/2** vì nằm ở batch sau.

**Đã dừng — chưa thực hiện slice thật.** Chờ bạn duyệt để mình viết `SpriteSheetSlicer.cs` và chạy Batch 1.
