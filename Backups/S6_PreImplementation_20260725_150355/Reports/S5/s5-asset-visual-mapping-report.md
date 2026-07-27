# S5 — Asset Visual Mapping Report (Batch 1)

**Ngày:** 2026-07-24 · **Phase:** S5 Batch 1 — Asset Visual Mapping
**Cơ sở:** inspect trực quan ảnh thật (contact sheet đã dựng ở phase trước), **không map theo tên file đơn thuần**.

---

## Asset Source
- **STYLE_SOURCE_OF_TRUTH = FantasyDungeon pack** (`Assets/_Game/Art/*`), pixel/chunky stone-wood.
- ❌ Không Higgsfield · ❌ Không generate asset mới.
- 🔵 Drawable decompile = **reference only** (tra ID/concept), không production.

### Bối cảnh hệ thống (đã đọc code, ảnh hưởng quyết định map)
- `ItemDefinition`, `EnemyDefinition`, `AdventurerDefinition` **KHÔNG có field art/icon/sprite** → mapping phải qua **catalog ngoài** (theo `ItemCategory`/`ItemType`/visual group), không gắn trực tiếp vào definition.
- **Chưa có game data JSON** → không có item/enemy instance runtime → map ở mức **loại/nhóm**, không map theo ID cụ thể.
- Đã có sẵn `AssetManifestService` (đọc `assets_manifest.json` id→path) nhưng file manifest **chưa tồn tại**.
- UI screens hiện là **text-only placeholder**; Main scene **chưa có UI** (sinh bằng Editor generator).
- → Batch 1 map theo **enum thật** của game: `ItemCategory {Weapon, Armor, Accessory, Consumable, Material}`, `EquipmentSlot`, enemy folder = visual group.

---

## Icon Mapping
Map theo **hình dạng quan sát được**, gắn vào `ItemCategory`/`ItemType` (không có ID cụ thể để map).

| Target Type | Target Id/Name | Selected Asset | Visual Reason | Confidence | Decision |
|---|---|---|---|---|---|
| Weapon | ItemType "Sword"/"GreatSword" | `Icons/icons_swords_00..15` | 16 lưỡi kiếm rõ chuôi/lưỡi | Cao | APPLY |
| Weapon | ItemType "Blunt"/"Mace"/"Hammer" | `Icons/icons_blunt_00..15` | Đầu chùy/búa nặng | Cao | APPLY |
| Weapon | ItemType "Bow"/"Ranged" | `Icons/icons_ranged_00..15` + `bow_v1` | Cung + mũi tên | Cao | APPLY |
| Weapon | ItemType "Staff"/"Wand"/"MagicWeapon" | `Icons/icons_magic_wpn_00..15` | Trượng đầu ngọc phát sáng | Cao | APPLY |
| Weapon/Offhand | ItemType "Shield" | `Icons/icons_shields_00..15` + `shield` | Khiên tròn/huy hiệu | Trung bình | NEEDS_REVIEW *(game chỉ có slot Weapon/Armor/Accessory, không có Shield riêng)* |
| Armor | ItemType "Helmet" | `Icons/helmet`, `icons_*` mũ | Mũ giáp | Cao | APPLY |
| Armor | ItemType "Boots" | `Icons/boots_v1` | Giày giáp | Cao | APPLY |
| Armor | ItemType "Gloves" | `Icons/gloves_v1` | Găng | Cao | APPLY |
| Accessory | ItemType "Amulet"/"Necklace" | `Icons/amulet_v1` | Bùa cổ | Cao | APPLY |
| Accessory | ItemType "Ring" | `Icons/ring` | Nhẫn có ngọc | Cao | APPLY |
| Consumable | Category Consumable (potion) | `Icons/icons_potions_00..15`, `potion_red/blue` | Bình thuốc nút bần | Cao | APPLY |
| Consumable | Category Consumable (food) | `Icons/bread`, `meat` | Bánh mì, thịt | Cao | APPLY |
| Consumable | Category Consumable (misc) | `Icons/icons_consum_00..15` | Vật phẩm tiêu hao đa dạng | Trung bình | APPLY |
| Material | gem/resource | `Icons/icons_gems_00..15`, `gem_v1` | Đá quý cắt cạnh | Cao | APPLY |
| Material | key | `Icons/icons_keys_jewel_00..15`, `key` | Chìa khoá nạm ngọc | Cao | APPLY |
| Skill/Spell | Skill (elemental) | `Icons/spell_elemental_00..15` | Cầu lửa/băng/sét | Cao | APPLY |
| Skill/Spell | Skill (holy/dark) | `Icons/spell_holy_dark_00..15` | Thánh giá sáng / sọ tối | Cao | APPLY |
| Skill/Spell | Skill (nature/arcane) | `Icons/spell_nature_arc_00..15` | Lá/mắt/xoáy arcane | Cao | APPLY |
| Scroll | ItemType "Scroll" / skill scroll | `Icons/icons_scrolls_00..15`, `scroll_v1` | Cuộn giấy có dấu niêm | Trung bình | NEEDS_REVIEW *(scroll = skill hay consumable tuỳ ngữ cảnh game)* |
| Currency (Money) | HUD money | `Icons/coin` | Đồng vàng có ký hiệu | Cao | APPLY |
| Currency (Gems) | HUD gems | `Icons/gem_v1` (hoặc `icons_gems_00`) | Đá quý xanh | Cao | APPLY |

---

## Character / Enemy Sprite Mapping
Map theo **visual group** (đã nhìn roster). Sheet đã slice 4×4, dùng **row0 (down/front)** cho card/roster; row2/3 cho combat side-view nếu cần.

| Target | Selected Sprite/Sheet | Direction/Anim Used | Visual Reason | Confidence | Decision |
|---|---|---|---|---|---|
| Undead | `Enemies/skeleton`, `bloodskeleton`, `necromancer` | idle row0 (card), walk/attack cho combat | Xương trắng, robe tối phù thuỷ | Cao | APPLY |
| Goblin/humanoid | `Enemies/goblin`, `shadowgoblin` | idle row0 | Da xanh, dáng nhỏ | Cao | APPLY |
| Archer | `Enemies/archer`, `elitearcher` | idle row0 | Cầm cung, elite recolor | Cao | APPLY |
| Slime | `Enemies/slime`, `frostslime`, `slimeking` | idle row0 | Khối nhầy, king có vương miện | Cao | APPLY |
| Bat/flying | `Enemies/bat`, `toxicbat` | idle row0 | Dơi, bản độc màu xanh | Cao | APPLY |
| Orc | `Enemies/orc`, `fireorc` | idle row0 | To lớn, fireorc ánh đỏ | Cao | APPLY |
| Boss | `Enemies/boss` | idle row0 + full anim | Giáp gai đỏ-đen uy hiếp | Cao | APPLY |
| Player/Adventurer (base) | `Characters/hero` | idle/walk/attack đủ 4 hướng | Hiệp sĩ giáp bạc áo choàng xanh | Cao | APPLY |
| Adventurer class variants | `Characters/hero_skins/` (assassin, frost, ranger, royal) | ảnh đơn | 4 skin sạch alpha | Trung bình | APPLY |
| Adventurer class variants (lỗi) | `hero_skins/crimson, mage, darkknight, paladin` | — | Còn nền magenta / halo | Thấp | NEEDS_REVIEW |
| NPC | `Characters/villager` (idle/walk) | row0 | Dân thường | Trung bình | APPLY |
| Merchant NPC | `Characters/merchant` | — | row3 bất thường, thiếu anim | Thấp | NEEDS_REVIEW |

---

## Portrait Mapping
Chỉ 8 portrait (RGB, **không alpha**, nền phẳng) — dùng trong khung card có nền, không cần cutout.

| Target | Selected Portrait | Reason | Confidence | Decision |
|---|---|---|---|---|
| Boss encounter | `Portraits/boss` | Giáp gai đỏ, khớp `Enemies/boss` | Cao | APPLY |
| Goblin enemy | `Portraits/goblin` | Mặt goblin xanh | Cao | APPLY |
| Skeleton enemy | `Portraits/skeleton` | Sọ + khiên | Cao | APPLY |
| Orc enemy | `Portraits/orc` | Mặt orc xanh cơ bắp | Cao | APPLY |
| Slime enemy | `Portraits/slime` | Khối xanh 2 mắt | Cao | APPLY |
| Player/Knight | `Portraits/hero` | Mũ giáp bạc | Cao | APPLY |
| Merchant NPC | `Portraits/merchant` | Người ria mép tạp dề | Cao | APPLY |
| Generic NPC | `Portraits/villager` | Mặt dân thường | Cao | APPLY |
| Class adventurer khác (mage, archer…) | — | Pack không có portrait class riêng | — | DEFERRED → placeholder sprite từ hero |

---

## Tileset / Dungeon Theme Mapping
Tile đơn 128px + 8 sheet đã slice (512 sprite). Map theo theme dungeon (tên area lấy từ drawable gốc CHỈ để tra concept).

| Dungeon/Area/Theme | Selected Tiles/Props | Reason | Confidence | Decision |
|---|---|---|---|---|
| Crypt/Undead theme | `bone_floor`, `skull_wall`, `coffin_wall`, `bones`, `skull`, `cobweb` | Xương/sọ | Cao | APPLY |
| Cave theme | `cave_floor`, `cave_floor2`, `cave_wall`, `crystal_wall`, `rubble` | Đá hang, tinh thể | Cao | APPLY |
| Standard dungeon | `wall_a/b`, `floor_plain/cracked/light`, `door`, `stairs`, `archway` | Tường-sàn đá chuẩn | Cao | APPLY |
| Lava/fire theme | `lava`, `wall_torch`, `brazier_big` | Dung nham, đuốc | Cao | APPLY |
| Water/frozen theme | `water`, `under_water`, `water_anim` | Nước động | Cao | APPLY |
| Trap room | `spikes_0/1/2`, `bear_trap`, `arrow_trap`, `pressure_plate`, `traps` (sheet) | Bẫy các loại | Cao | APPLY |
| Props/decor | `barrel`, `crate`, `pot`, `chest_open`, `candle`, `banner`, `props/props2/props2b` (sheet) | Vật trang trí scene | Cao | APPLY |

---

## UI Asset Mapping
⚠️ `ui_kit.png` & `ui_dialog.png` **chưa được cắt element** (nền đặc, cần cắt tay — MANUAL_REVIEW). → Phần lớn UI chrome ở batch này dùng **placeholder panel**, chỉ những gì cắt/dùng nguyên được mới APPLY.

| UI Element | Selected Asset | Reason | Confidence | Decision |
|---|---|---|---|---|
| Main panel / window | `ui_kit` (vùng PANEL/9-slice) | Có sẵn nhưng chưa cắt | Trung bình | PLACEHOLDER *(dùng panel trơn tới khi cắt)* |
| Inventory panel | `ui_kit` (vùng INVENTORY) | Chưa cắt | Trung bình | PLACEHOLDER |
| Character panel | `ui_kit` (vùng FRAME) | Chưa cắt | Trung bình | PLACEHOLDER |
| Button | `ui_kit` (OK/CANCEL/EQUIP/USE) | Có nút nhưng dính chữ bake | Trung bình | PLACEHOLDER *(nút Unity + text runtime)* |
| Item slot | `ui_kit` (ô lưới đá) | Chưa cắt | Trung bình | DEFERRED |
| Title / header | `ui_kit` (thanh gỗ TITLE) | Chưa cắt | Trung bình | PLACEHOLDER |
| **Currency HUD (coin)** | `Icons/coin` | **Icon rời, dùng ngay** | Cao | **APPLY** |
| **Currency HUD (gem)** | `Icons/gem_v1` | **Icon rời, dùng ngay** | Cao | **APPLY** |
| **Nav button icons** | `Icons/` (sword→Dungeon, bag/pouch→Inventory, gear→Settings…) | **Icon rời dùng ngay làm ảnh nút** | Cao | **APPLY** |
| Popup panel | `ui_kit` (panel) | Chưa cắt | Trung bình | PLACEHOLDER |
| HP/MP/XP bar | `ui_kit` (3 thanh) | Chưa cắt; hiện chưa có hệ HP runtime để bind | Thấp | DEFERRED |

---

## Missing After Exhausting Pack
Chỉ liệt kê thứ pack **thật sự không phủ được**.

| Missing Asset | Why Pack Cannot Cover | Suggested Future |
|---|---|---|
| Pet art | Pack có **0** pet | HIGGSFIELD_LATER |
| Buildings (guild hall/shop/forge) | Pack không có công trình | HIGGSFIELD_LATER |
| Area background hoàn chỉnh | Pack chỉ có tile rời, không có tranh nền area | HIGGSFIELD_LATER (hoặc ghép tile thành scene) |
| Splash / loading / title / menu bg | Không có trong pack | HIGGSFIELD_LATER |
| Portrait class adventurer (mage/archer/priest…) | Chỉ có 8 portrait generic | HIGGSFIELD_LATER (tạm dùng `Portraits/hero`) |
| Rarity frame bộ 5 bậc | ui_kit có vài khung, chưa thành bộ | Cắt từ ui_kit trước → nếu thiếu thì HIGGSFIELD_LATER |
| UI chrome production (panel/button/slot đã cắt) | Có trong `ui_kit` nhưng **chưa cắt** | Cắt tay `ui_kit` (không cần Higgsfield) |
| Audio (SFX/BGM) | Pack không có âm thanh | HIGGSFIELD_LATER (batch riêng) |

> "HIGGSFIELD_LATER" chỉ là ghi chú tương lai — **Batch này KHÔNG gọi Higgsfield**.

---

## Batch 1 Result: **PASS**
- ✅ Report tồn tại, map dựa trên **inspect ảnh thật** (không chỉ tên file)
- ✅ Có Confidence + Decision mọi dòng
- ✅ Không đổi gameplay/backend, không generate
- ➡️ Tiếp tục **Batch 2** — Apply Safe Asset Mapping (ưu tiên: AssetCatalog + HUD currency icon + nav button icon; phần chưa cắt UI → placeholder).
