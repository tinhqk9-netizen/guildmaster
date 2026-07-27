# Asset Gap Report — Rebuild_GuildMaster (v2)

**Ngày:** 2026-07-24 · **Phase:** A (audit only — chưa generate, chưa tiêu credit, chưa sửa Unity)

> **Hướng đã chỉnh:** Nguồn art CHÍNH cho bản rebuild mới là **`Assets-tham-khao` (FantasyDungeon pack)**.
> Art gốc trong `resources/res/drawable` **KHÔNG** phải nguồn production — chỉ dùng để tra tên/ID/concept/mapping (Nhóm D).

---

## 1. Trạng thái project

Code-first, chưa có content asset. `_Game/Art`, `Audio`, `Prefabs`, `Data`, `StreamingAssets/GameData` **rỗng**. Có 92 script hệ thống + định nghĩa content: **Adventurer, Dungeon, Enemy, Item, Pet, Quest, Raid, Recipe, Skill, StatusEffect**. → Mọi art phải reuse từ pack hoặc generate.

---

## 2. Nguồn art CHÍNH — FantasyDungeon pack (`Assets-tham-khao`)

Style: **pixel-art 2D fantasy dungeon**. Tổng 416 file + `FantasyDungeon_v1.3_Unity.unitypackage` (bản Unity có sẵn slicing).

| Nhóm file | Số lượng | Nội dung |
|---|---|---|
| `characters/` | 102 | 17 bộ sprite-sheet (idle/walk/run/attack/hurt/death): hero, archer, elitearcher, necromancer + 12 quái (bat, bloodskeleton, boss, fireorc, frostslime, goblin, orc, shadowgoblin, skeleton, slime, slimeking, toxicbat) + merchant, villager |
| `hero_skins/` | 8 | Skin class: assassin, crimson, darkknight, frost, mage, paladin, ranger, royal |
| `icons/` | 224 | Item icon theo nhóm (swords, blunt, ranged, shields, magic_wpn, potions, consum, gems, keys_jewel, scrolls) + **spell icons** (elemental, holy_dark, nature_arc) + lẻ (coin, gem, ring, key...) |
| `portraits/` | 8 | boss, goblin, hero, merchant, orc, skeleton, slime, villager |
| `tileset/` (+environment) | 65 | Tile dungeon, tường/sàn, cửa, bẫy, rương, props |
| `ui/` | 2 | `ui_kit.png` (2048²), `ui_dialog.png` |
| `vfx/` | 7 | fire, heal, hit, levelup, lightning, slash, glow |

---

## 3. Đối chiếu: game CẦN gì vs pack CÓ gì

| Nhu cầu (theo Definitions) | Pack có? | Đánh giá |
|---|---|---|
| Enemy (quái combat) | ✅ 13 bộ sprite-sheet | Đủ dùng (generic) |
| Adventurer / class | 🟡 hero + 8 hero_skins + archer/necromancer | Có nhân vật; **thiếu portrait riêng từng class** |
| Item icons | ✅ 224 | Đủ (generic, không khớp tên item gốc) |
| Skill / spell icons | ✅ spell_* (~48) | Đủ dùng |
| VFX cơ bản | ✅ 7 | Đủ tối thiểu |
| Tileset môi trường | ✅ 65 | Đủ để ghép cảnh |
| **Pet** | ❌ **0** | **Thiếu hẳn** → generate |
| **Area/Dungeon background** (cảnh nền hoàn chỉnh) | ❌ chỉ có tile lẻ | **Thiếu** → generate/ghép |
| **UI chrome** (panel/button/slot/frame/card/bar/dialog) | ❌ chỉ ui_kit + ui_dialog | **Thiếu nhiều nhất** → generate |
| **Portrait** (class/pet/nhiều enemy) | 🟡 chỉ 8 generic | **Thiếu** → generate |
| **Splash / loading / title / menu bg** | ❌ | **Thiếu** → generate |
| StatusEffect icon riêng | ❌ (tạm mượn vfx/spell) | Thiếu nhẹ |
| Audio | ❌ | Thiếu (batch riêng, không ưu tiên) |

---

## 4. Phân loại 4 nhóm

### 🟢 Nhóm A — Asset trong pack dùng được ngay
- Item icons (224), spell/skill icons (~48), vfx (7), tileset (65).
- (Các sprite-sheet enemy nếu import qua `.unitypackage` đã có slicing → cũng gần A.)

### 🟡 Nhóm B — Có trong pack nhưng cần chỉnh
- **Sprite-sheet** (102 char) → cần **slice** thành animation trong Unity (theo `README_IMPORT.txt`); ưu tiên import `.unitypackage` để có sẵn config.
- **hero_skins** → tên file lỗi `.png.png` → **rename**.
- **ui_kit.png** → cần **cắt / 9-slice** nếu tận dụng làm UI chrome.
- Kiểm tra kích thước/pivot/PPU đồng nhất cho pixel-art.

### 🔴 Nhóm C — Thiếu, cần Higgsfield GENERATE (giữ style đồng nhất với pack)
Ưu tiên cao → thấp:
1. **UI chrome:** panel/window bg, buttons (normal/pressed/disabled), item/equipment **slot**, **card frames** (adventurer/pet/enemy/raid), rarity frames (common→legendary), tab/bottom-nav bar, dialog & popup bg, progress/HP/XP bars, badge/ribbon/tooltip.
2. **Pet:** bộ pet art (pack không có) — số lượng theo dataset pet của game.
3. **Area/Dungeon background:** cảnh nền hoàn chỉnh cho các area (ghép từ tileset hoặc generate mới).
4. **Portrait:** portrait từng class adventurer + pet + bổ sung enemy.
5. **Presentation:** splash, loading, title, menu bg, reward/chest popup.
6. **(Tùy)** status-effect icon riêng; **(batch riêng)** audio.

### 🔵 Nhóm D — Original drawable (`resources/res/drawable`) — CHỈ tham khảo
~1040 file art gốc. **Không dùng làm production.** Chỉ để:
- Tra **tên/ID/concept** item, skill, enemy, area, pet của game gốc.
- Đối chiếu nội dung & **mapping** khi cần.
- Mọi trường hợp muốn dùng 1 file drawable làm asset thật → **phải bạn duyệt riêng**.

---

## 5. Điểm cần quyết định trước khi generate (STYLE)

Pack là **pixel-art**. Toàn bộ Nhóm C (UI, pet, background, portrait, splash) nếu muốn **đồng nhất** thì phải generate theo đúng pixel-art — trong khi các model Higgsfield (`recraft_v4_1`, `soul_cast`, `soul_location`) mạnh ở vector/painterly/photoreal, **khớp pixel-art chặt là điểm yếu**. → Cần bạn chốt định hướng style (mục dưới) trước khi mình khóa spec + chọn model cho Nhóm C.

Xem: [asset-generation-plan.md](asset-generation-plan.md)
