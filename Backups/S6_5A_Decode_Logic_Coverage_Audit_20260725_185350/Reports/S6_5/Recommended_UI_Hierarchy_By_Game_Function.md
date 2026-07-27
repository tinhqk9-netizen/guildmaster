# Recommended UI Hierarchy By Game Function

**Ngày:** 2026-07-25
**Cơ sở đề xuất:** Dữ liệu decode thật (1531 record / 10 category) + 14 service đã code sẵn trong `Runtime/Services` + trạng thái wiring thực tế từ báo cáo S6.

## Cây hierarchy đề xuất

```
Main.unity
├── Systems
│   └── UIRuntimeBootstrap
├── Cameras
│   ├── Main Camera
│   └── Global Light 2D
├── UI
│   ├── UICanvas
│   │   └── SafeArea
│   │       ├── HUD                        (= HudRoot đổi tên)
│   │       │   └── HUDVisual
│   │       │       ├── Currency           (nhóm mới: MoneyText+Icon, GemsText+Icon)
│   │       │       └── Navigation         (nhóm mới: 6 Btn_*)
│   │       ├── Screens                    (= ScreenRoot đổi tên)
│   │       │   ├── CharacterAndEquipment  → CharacterScreen
│   │       │   ├── InventoryAndItems      → InventoryScreen
│   │       │   ├── DungeonAndCombat       → DungeonScreen
│   │       │   ├── Crafting               → CraftScreen
│   │       │   ├── Merchant               → MerchantScreen
│   │       │   └── Settings               → SettingsScreen
│   │       ├── Popups                     (= PopupRoot đổi tên) → PopupScreen
│   │       └── Overlay                    (= OverlayRoot đổi tên, hiện rỗng)
│   └── EventSystem
```

## Bảng nhóm chức năng

| Game Function Group | Should Contain | Current Objects | Missing Objects | Notes |
|---|---|---|---|---|
| **Systems** | Composition root, service bootstrap, save hook | `UIRuntimeBootstrap` | — | Đủ. Sau này nếu tách DI container thì thêm vào đây |
| **Cameras** | Camera + lighting | `Main Camera`, `Global Light 2D` | — | Đủ cho scene UI-only. Khi có dungeon/combat visual sẽ cần thêm camera riêng hoặc render layer |
| **HUD / Currency** | Hiển thị mọi loại tiền tệ | `MoneyText`+`MoneyIcon`, `GemsText`+`GemsIcon` | Nhóm cha `Currency`; icon nên tách khỏi Text thành sibling | ✅ **Real** — đọc `SaveService.Money/Gems`. Chưa có gameplay nào sinh/tiêu tiền |
| **HUD / Navigation** | Nút vào từng module | 6 nút: `Btn_Inventory`, `Btn_Character`, `Btn_Dungeon`, `Btn_Craft`, `Btn_Merchant`, `Btn_Settings` | Nhóm cha `Navigation`; badge/notification dot (sau) | ✅ **Real** — cả 6 nút bind `UIService.ShowScreen` hoạt động thật |
| **Screens / CharacterAndEquipment** | Danh sách nhân vật, chỉ số, **equipment slot**, **skill** | `CharacterScreen` + `ListContent` (text) + `Btn_Back` | **Equipment slot UI** (Weapon/Armor/Accessory), **stat panel** (HP/CON/INT/DEX/DEF/MDEF), **skill list**, portrait, level/exp bar, nút level-up | ⚠️ **Real function một phần**: `CharacterService` đã wired và có `GetTotalStat()`, `GainExperience()`, `LevelUp()` — **nhưng UI chưa dùng gì ngoài text list**. `EquipmentService` **chưa wire**. Data sẵn: **129 adventurer**, **227 skill** |
| **Screens / InventoryAndItems** | Danh sách item, phân loại, thao tác item | `InventoryScreen` + `ListContent` (text) + `Btn_Back` | **Item grid + icon**, filter theo category, chi tiết item, nút equip/use/sell, hiển thị capacity | ⚠️ **Real function một phần**: `InventoryService` wired (add/remove/stack/capacity đầy đủ) — **UI mới ở mức text**. Data sẵn: **607 item**. Kho rỗng vì **chưa có nguồn sinh item** (loot/craft/mua đều chưa chạy) |
| **Screens / DungeonAndCombat** | Chọn dungeon, xem enemy, chạy combat, nhận loot | `DungeonScreen` (Image nền + `Title` + `Message` tĩnh + `Btn_Back`) | **Toàn bộ**: danh sách dungeon, thông tin enemy, party select, combat view/log, loot result, progress/clear count | ❌ **PLACEHOLDER — 0% gameplay.** Data sẵn: **11 dungeon, 122 enemy, 12 raid**. Service sẵn: `DungeonService`, `CombatService`, `LootService`, `EnemyService`, `StatusEffectService`, `SkillService` — **cả 6 đều chưa khởi tạo runtime**. Cần cả **orchestrator "dungeon run"** (hiện chưa tồn tại) |
| **Screens / Crafting** | Recipe, nguyên liệu, hàng đợi craft, claim | `CraftScreen` (Image + Title + Message + Btn_Back) | **Toàn bộ**: danh sách recipe, hiển thị ingredient đủ/thiếu, nút craft, workshop queue + timer, nút claim | ❌ **PLACEHOLDER — 0% gameplay.** Data sẵn: **321 recipe**. `CraftService` đã code (có `SaveData.WorkshopQueue`/`CompletedWorkshopItems`) **nhưng chưa wire**. Timer/claim rule cần đối chiếu decode trước khi dựng |
| **Screens / Merchant** | Mua/bán, listing, restock | `MerchantScreen` (Image + Title + Message + Btn_Back) | **Toàn bộ**: danh sách hàng bán, giá, nút buy/sell, market listing của người chơi, timer restock | ❌ **PLACEHOLDER — 0% gameplay.** `MerchantService` đã code (`SaveData.MarketListings`/`SoldMarketItems`) **chưa wire**. Giá/restock rule phải lấy từ decode, **tuyệt đối không tự đặt số** |
| **Screens / Settings** | Tùy chọn, save, version, debug | `SettingsScreen` (Image + Title + Message + Btn_Back) | **Toàn bộ**: nút Save thủ công, âm lượng, ngôn ngữ, hiện version, xóa save, (tùy chọn) debug info | ❌ **PLACEHOLDER — 0% chức năng.** Không có service tương ứng. **Đây là nhóm dễ làm thật nhất** vì `SaveService` đã có sẵn `Save()`/`DeleteSave()`/`HasSaveFile()` |
| **Popups** | Confirm / result / error dialog | `PopupScreen` (`Title`, `Message`, `Btn_OK`) | Nút Cancel (confirm 2 lựa chọn), loot/result popup, error popup có mã lỗi | ⚠️ **Real function nhưng chưa dùng**: `ShowMessage()`, `ShowDeferred()`, `ShowInfo()`, `ShowError()` đều đã có trong `UIService` — **chưa có nơi nào gọi**. Đây là "quả ngọt dễ hái": chỉ cần gọi `ShowDeferred()` từ 4 nút placeholder là user có phản hồi rõ ràng |
| **Overlay** | Loading, transition, toast, tooltip | `OverlayRoot` (**rỗng**) | Loading screen, fade transition, toast thông báo, tooltip item | ❌ **Rỗng hoàn toàn.** Chưa dùng. `UIScreenId.Loading` đã có trong enum nhưng chưa có object nào |

## 🔴 Kết luận bắt buộc ghi rõ (theo yêu cầu user)

**Panel nào hiện CHỈ là placeholder visual (không được coi là chức năng đã xong):**
- `DungeonScreen` — 0% gameplay
- `CraftScreen` — 0% gameplay
- `MerchantScreen` — 0% gameplay
- `SettingsScreen` — 0% chức năng

**Panel nào cần S6.5A Gameplay Function Completion:**

| Ưu tiên | Nhóm | Việc cần làm | Độ khó | Rule từ decode đã đủ chưa? |
|---|---|---|---|---|
| 1 | **Settings** | Wire `SaveService` vào nút Save/Delete + hiện version | Thấp | ✅ Đủ — không cần rule gameplay |
| 2 | **Popups** | Gọi `ShowDeferred()`/`ShowInfo()` từ các nút chưa có chức năng | Thấp | ✅ Đủ |
| 3 | **InventoryAndItems** | Wire `EquipmentService`, hiện item theo category + icon từ AssetCatalog | Trung bình | ✅ Data 607 item đủ; rule equip/unequip đã có trong `EquipmentService` |
| 4 | **CharacterAndEquipment** | Hiện chỉ số qua `GetTotalStat()`, equipment slot, skill | Trung bình | ⚠️ Một phần — `GetTotalStat()` có TODO `manualRuleRequired` cho **level stat multiplier** (đang hardcode 1.0f). Cần bóc rule từ decode |
| 5 | **Crafting** | Wire `CraftService`, recipe list, queue, claim | Cao | ⚠️ Cần xác nhận **rule timer + claim** từ decode |
| 6 | **Merchant** | Wire `MerchantService`, buy/sell/listing | Cao | ⚠️ Cần xác nhận **rule giá + restock** từ decode |
| 7 | **DungeonAndCombat** | Wire 6 service + viết **dungeon run orchestrator** (chưa tồn tại) | **Rất cao** | ⚠️ Cần xác nhận rule **combat result, damage, loot table** — đây là phần dễ "fake" nhất nên phải cẩn thận nhất |

**Nguyên tắc áp dụng cho S6.5A:** Thiếu asset → dùng placeholder sprite. **Thiếu rule gameplay → KHÔNG được tự bịa số.** Nếu rule chưa rõ từ decode thì đánh dấu `MANUAL_RULE_REQUIRED` và để nguyên trạng, không đoán.
