# Reference Asset Import / Conversion Report

**Ngày:** 2026-07-24 · **Phase:** Reference Asset Import / Conversion
**Nguồn:** `D:\Tinh\Guild Master - Idle Dungeons\Assets-tham-khao\FantasyDungeon_v1.3_PNG_Godot_GameMaker`
**Đích:** `D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Art\`

> ❌ Không generate · ❌ Không gọi Higgsfield · ❌ Không tiêu credit · ❌ Không sửa gameplay/backend · ❌ Không map vào scene/prefab
> ✅ Đã **inspect trực quan ảnh thật** từng nhóm (không chỉ đếm tên file)
> ⚖️ **License MIT** — bắt buộc ghi credit: *"Assets by Nika Studio"* (đã copy `FantasyDungeon_LICENSE.txt` vào Art/)

---

## 1. Bảng import

| Source asset/folder | Type | Target Unity folder | Action | Count | Notes |
|---|---|---|---|---|---|
| `icons/` | Item & spell icons 64×64 | `Art/Icons/` | Copy | 224 | RGBA trong suốt thật (24–74%), **dùng ngay** |
| `portraits/` | Bust portrait 256×256 | `Art/Portraits/` | Copy | 8 | **RGB, KHÔNG alpha** — nền phẳng bake sẵn |
| `characters/hero,merchant,villager` | Sprite sheet 1024² | `Art/Characters/<name>/` | Copy | 12 | 4×4 grid, cần slice |
| `characters/` (15 loại còn lại) | Sprite sheet 1024² | `Art/Enemies/<name>/` | Copy | 90 | 4×4 grid, cần slice |
| `hero_skins/` | Skin 2048² | `Art/Characters/hero_skins/` | Copy + **rename** | 8 | 3 file còn nền magenta chưa tách |
| `tileset/` | Tile 128×128 | `Art/Tilesets/` | Copy | 45 | dùng ngay; vài file là sheet nhiều vật thể |
| `tileset/environment/` | Props/decor | `Art/Tilesets/environment/` | Copy | 20 | có sheet cần slice (props, animated_tiles, water_anim) |
| `vfx/` | Effect strip 2048² | `Art/VFX/` | Copy | 7 | strip animation, cần slice |
| `ui/` | UI kit + mockup 2048² | `Art/UI/` | Copy | 2 | **0% alpha** — nền đặc, cần cắt element |
| `LICENSE.txt`, `README_IMPORT.txt` | Legal/spec | `Art/` | Copy | 2 | bắt buộc giữ để credit |
| | | | **TỔNG** | **416 PNG + 2 txt** | |

---

## 2. Visual Inspection Summary

| Asset group | Sample files inspected | Visual quality | Alpha/background | Processing needed | Reuse potential | Notes |
|---|---|---|---|---|---|---|
| **UI kit** | `ui_kit.png` (2048²) | ⭐ Cao — đá/gỗ chunky, bevel sạch | RGBA nhưng **0% trong suốt** (nền slate đặc) | Cắt từng element + tách nền + set 9-slice | **RẤT CAO** | Chứa sẵn: panel INVENTORY/ITEMS, **ô slot lưới**, bar HP/MP/XP, nút OK/CANCEL/EQUIP/USE, nút icon, mũi tên, khung TITLE/PANEL/FRAME, **bộ mảnh 9-slice**, banner, divider, khung gem/skull |
| **UI dialog** | `ui_dialog.png` (2048²) | ⭐ Cao nhưng là **mockup** | 0% trong suốt | Chỉ cắt element rời | Trung bình | **Chữ tiếng Anh bake cứng** ("Merchant", "Quests"...) → không dùng nguyên tấm. Cắt được: nút Buy/Sell, tab Active/Completed, nút X, mũi tên scroll, ô hotbar, 9 icon nav tròn, khung portrait |
| **Icons** | 94/224 (mọi category + singles) | ⭐ Rất cao — silhouette rõ, màu tốt | ✅ RGBA trong suốt thật | Không cần (có thể atlas) | **RẤT CAO — dùng ngay** | Đủ vũ khí/giáp/khiên/potion/gem/key/scroll/food + **48 spell icon**. Đọc rõ ở cỡ nhỏ → hợp mobile |
| **Character sheets** | roster 18 nhân vật | ⭐ Cao, nhất quán | ✅ 77–81% trong suốt | **Slice 4×4 (256px)** | Cao | Đúng spec README: **mỗi HÀNG = 1 hướng** (down/up/left/right). Idle game có thể **chỉ cần hàng "down"** → 3/4 sheet có thể thừa |
| **Enemy sheets** | skeleton, bat, slime, orc, boss... | ⭐ Cao | ✅ trong suốt | Slice 4×4 | Cao | 15 loại quái + boss, đủ cho bestiary sớm |
| **Portraits** | cả 8 | ⭐ Cao (bust pixel-art) | ❌ **RGB, không alpha** — nền xanh-slate phẳng | Tách nền (nền phẳng → dễ) hoặc dùng nguyên trong khung card | Trung bình | Chỉ 8 cái; thiếu portrait cho class adventurer & pet |
| **Hero skins** | cả 8 | Hỗn hợp | ⚠️ **3 file lỗi**: `crimson` (2.2%), `mage` (0.1%), `darkknight` (21%) **còn nền magenta chroma-key**; `paladin` có viền hào quang trắng | **Tách nền magenta** + crop + downscale | Trung bình–Cao sau khi sửa | 2048² nhưng nhân vật rất nhỏ → phí dung lượng, cần crop |
| **Tilesets** | 65 tile | ⭐ Cao | RGBA, tile nền đặc (đúng bản chất tile) | Slice các sheet gộp | Cao | Tường/sàn/cửa/bẫy/rương/thùng/props đầy đủ |
| **VFX** | cả 7 | Khá | ✅ 85–98% trong suốt | **Slice strip** | Trung bình | ⚠️ `vfx_heal` & `vfx_levelup` **bake sẵn nền tile đá**; `vfx_levelup` **bake chữ "LEVEL UP!"** → cần crop bỏ hoặc tránh dùng |

---

## 3. Imported / Copied
- ✅ **416 PNG** vào 7 folder đích (`UI, Icons, Characters, Enemies, Portraits, Tilesets, VFX`)
- ✅ Giữ nguyên source pack (chỉ copy, không di chuyển/sửa file gốc)
- ✅ Giữ cấu trúc subfolder theo nhân vật (`Enemies/goblin/`, `Characters/hero/`…) cho dễ map
- ✅ Copy `FantasyDungeon_LICENSE.txt` + `FantasyDungeon_README_IMPORT.txt` (tuân thủ MIT + giữ spec slice)

## 4. Renamed / Fixed
| Trước | Sau |
|---|---|
| `hero_skins/assassin.png.png` | `Characters/hero_skins/assassin.png` |
| `crimson.png.png` · `darkknight.png.png` · `frost.png.png` | `crimson.png` · `darkknight.png` · `frost.png` |
| `mage.png.png` · `paladin.png.png` · `ranger.png.png` · `royal.png.png` | `mage.png` · `paladin.png` · `ranger.png` · `royal.png` |

→ **8 file** sửa lỗi đuôi kép. Không đổi tên gì khác (giữ naming gốc đã production-friendly, snake_case).

## 5. Needs Slice
| Asset | Grid | Ghi chú |
|---|---|---|
| 102 character/enemy sheet (1024²) | **4 cols × 4 rows, cell 256×256** | Mỗi hàng = 1 hướng; theo README |
| `Tilesets/environment/animated_tiles, water_anim, props, props2, props2b, deco_shadows, traps` | biến thiên | Sheet gộp nhiều vật thể |
| 6 VFX strip (2048²) | frame rải ngang, **không đều** | Cần căn thủ công, không auto-grid được |
| `UI/ui_kit.png`, `UI/ui_dialog.png` | tự do | Cắt element thủ công (không phải grid) |

## 6. Needs Import Preset
Pack là **pixel-art** → README yêu cầu rõ: **NO filtering, NO compression**. Cần preset theo folder:

| Folder | Sprite Mode | PPU | Filter | Compression | Khác |
|---|---|---|---|---|---|
| `Icons` | Single | 64 | **Point** | None | Pivot Center, Max Size 64 |
| `Tilesets` | Single (sheet→Multiple) | 128 | Point | None | Max Size 128 |
| `Characters`/`Enemies` | **Multiple** (4×4, 256px) | 256 | Point | None | Max Size 1024 |
| `Portraits` | Single | 256 | Point | None | Max Size 256 |
| `UI` | Multiple (cắt tay) | 100 | Point | None | **Mesh Full Rect** + 9-slice border |
| `VFX` | Multiple | 256 | Point | None | Max Size 2048 |

Chung: `Alpha Is Transparency = true`, `Generate Mip Maps = off`, `Wrap = Clamp`, Texture Type = **Sprite (2D and UI)**.

> ⚠️ Unity mặc định là **Bilinear + Compressed** → pixel-art sẽ **mờ và nhiễu** nếu không set. Đây là lý do cần preset.
> 📌 **Đề xuất (chờ duyệt):** viết `Assets/_Game/Scripts/Editor/AssetImportPresets.cs` — một `AssetPostprocessor` tự áp preset theo đường dẫn folder. Lý do: 416 file, set tay trong Inspector là không khả thi và dễ sai. **Chưa viết, chờ bạn duyệt.**

## 7. Ready To Map
Chỉ những thứ chắc chắn an toàn (**chưa map, mới ở trạng thái imported + ready**):
- `Art/Icons/*` (224) → item icon & skill icon slot trong UI Inventory/Skill — **an toàn nhất, dùng ngay sau khi set PPU/Point**
- `Art/Tilesets/*` → dungeon/area scene
- `Art/Portraits/*` → khung portrait trong card/dialog
- `Art/UI/ui_kit.png` → nguồn UI chrome sau khi cắt

❌ **Chưa map bất cứ thứ gì vào scene/prefab** — project chưa có prefab/UI script nào (`Prefabs/`, `Scripts/UI/` đang rỗng), nên map lúc này là đoán mò.

## 8. Still Missing After Reference Pack
Sau khi **đã tận dụng tối đa pack** (đặc biệt: UI chrome **đã có sẵn trong ui_kit**, không cần generate):

| Thiếu | Mức độ | Ghi chú |
|---|---|---|
| **Pet art** | 🔴 Thiếu hoàn toàn | Pack có **0** pet. Game có `PetDefinition`. |
| **Buildings** | 🔴 Thiếu hoàn toàn | `Art/Buildings/` rỗng; pack không có công trình/guild hall |
| **Area background hoàn chỉnh** | 🟠 Thiếu | Pack chỉ có **tile rời**, không có tranh nền area trọn vẹn |
| **Splash / loading / title / menu bg** | 🟠 Thiếu | Không có trong pack |
| **Portrait bổ sung** | 🟡 Thiếu một phần | Chỉ 8 portrait; thiếu class adventurer & pet |
| **Rarity frame 5 bậc** | 🟡 Thiếu một phần | ui_kit có vài khung nhưng chưa thành bộ common→legendary |
| **Audio** | 🟠 Thiếu hoàn toàn | Pack không có âm thanh |

> ✅ **Đính chính so với Phase A:** UI chrome **KHÔNG còn nằm trong danh sách thiếu**. Sau khi nhìn `ui_kit.png`, xác nhận nó đã có panel, slot, bar, nút, mũi tên, khung, **bộ mảnh 9-slice**, banner. Chỉ cần **cắt + tách nền**, không cần generate.

## 9. Higgsfield Later Candidates
Chỉ những gì thật sự không có trong pack (xếp theo ưu tiên):
1. **Pet art** (thiếu hẳn)
2. **Buildings** (guild hall, shop, forge…)
3. **Area background** hoàn chỉnh cho từng dungeon
4. **Splash / loading / title / menu background**
5. **Portrait** cho class adventurer + pet
6. **Rarity frame** bộ 5 bậc (nếu cắt từ ui_kit không đủ)
7. *(batch riêng)* **Audio**

---

## 10. Optimization Plan For Reference Assets

| Asset group | Optimization action | Priority | Expected output |
|---|---|---|---|
| Icons | Set Sprite 2D/UI, PPU 64, **Filter Point**, Compression None | 🔴 P1 | 224 sprite dùng ngay, nét căng |
| Tất cả | Viết `AssetImportPresets.cs` (AssetPostprocessor theo folder) | 🔴 P1 | Tự áp preset cho 416 file, không set tay |
| UI kit | Cắt element + tách nền slate → tách file riêng (panel, slot, button, bar, arrow, frame) | 🔴 P1 | Bộ UI chrome production, thay thế nhu cầu generate |
| UI kit 9-slice | Set border 9-slice cho panel/button/frame đã cắt | 🔴 P1 | Panel co giãn mọi kích thước, hợp 1080×1920 |
| Character/Enemy sheets | Slice Multiple **4×4 cell 256px**; cân nhắc **chỉ giữ hàng "down"** cho idle game | 🟠 P2 | Sprite animation; giảm ~75% sprite thừa nếu chỉ cần 1 hướng |
| Hero skins (3 lỗi) | **Tách nền magenta** (`crimson`, `mage`, `darkknight`) + xoá halo `paladin` | 🟠 P2 | 8 skin sạch alpha |
| Hero skins (all) | Crop vùng trống + downscale từ 2048² | 🟠 P2 | Giảm mạnh dung lượng texture |
| Portraits | Tách nền phẳng (nếu cần alpha) hoặc dùng nguyên trong khung card | 🟡 P3 | 8 portrait linh hoạt |
| VFX | Slice strip thủ công; **crop bỏ nền tile đá** ở `vfx_heal`/`vfx_levelup`; **tránh dùng frame có chữ "LEVEL UP!"** | 🟡 P3 | VFX sạch, không dính nền/chữ |
| Tileset sheets | Slice `props`, `animated_tiles`, `water_anim`, `deco_shadows` | 🟡 P3 | Props rời dùng cho scene |
| Toàn bộ | Tạo **catalog mapping** (asset ↔ item/skill/enemy ID) — dùng drawable gốc chỉ để **tra tên/ID** | 🟠 P2 | File map cho DatabaseBuilder |

---

## FINAL DECISION

# `REFERENCE_ASSETS_NEED_PROCESSING_PLAN`

**Lý do:** 416 asset đã được **inspect trực quan** và **copy vào đúng folder Unity**, 8 lỗi tên đã sửa. Nhưng chúng **chưa dùng được ngay** vì cần một pass xử lý đã xác định rõ:
1. **Import preset bắt buộc** (Unity mặc định Bilinear+Compressed sẽ làm mờ toàn bộ pixel-art) → cần duyệt `AssetImportPresets.cs`
2. **Cắt `ui_kit.png`** thành UI chrome rời (đây là nguồn thay thế việc generate UI)
3. **Slice** 102 character sheet + VFX + tileset sheet
4. **Sửa 3 hero skin còn nền magenta** + halo `paladin`

**Đã dừng. Chờ bạn duyệt** — đặc biệt là mục **viết `AssetImportPresets.cs`** (yêu cầu #5 của bạn: phải báo lý do và chờ duyệt).
