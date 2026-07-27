# S6.5 Preparation Status Report

**Ngày:** 2026-07-25
**Phạm vi phiên này:** Chỉ audit + sắp xếp hierarchy + giải thích tác dụng panel. **Không** làm gameplay, **không** generate asset, **không** dùng Higgsfield, **không** làm S6.5B visual, **không** nhảy S7.

## 1. Editable Panels Status
✅ **Hoàn tất.** Toàn bộ **66/66 GameObject** trong `Main.unity` đang `active`, **0 object inactive**. Mở scene là thấy đủ mọi panel trong Hierarchy lẫn Scene view, không cần bấm Play.

Công cụ hỗ trợ hiện có (`GuildMaster → UI → ...`):
| Menu | Tác dụng |
|---|---|
| `Make All Panels Visible In Scene` | Bật tất cả panel **và lưu scene** (dùng khi lỡ tắt hết hoặc sau khi chạy lại `Wire UI Scene`) |
| `Organize Hierarchy By Game Function` | **MỚI** — gom root thành `Systems`/`Cameras`/`UI` + xếp screen theo luồng chơi |
| `Preview: Show All Screens` / `Reset` / `Only <screen>` | Bật/tắt tạm để xem riêng từng màn |

**An toàn runtime:** `UIService.RegisterScreen()` gọi `Hide()` ngay khi đăng ký → mỗi lần Play runtime **tự chuẩn hoá lại** trạng thái active, bất kể scene file để bật hay tắt.

## 2. Current Panel Position / Function Summary
Chi tiết đầy đủ (RectTransform, component, data/service từng object): `Current_UI_Hierarchy_Function_Report.md`.

| Nhóm | Chức năng thật | Ghi chú |
|---|---|---|
| HUD — Currency | ✅ **Real** | `MoneyText`/`GemsText` đọc `SaveService.Money/Gems` (đang = 0 vì chưa có gameplay sinh tiền) |
| HUD — Navigation | ✅ **Real** | 6 nút bind `UIService.ShowScreen()` hoạt động thật |
| InventoryScreen | ⚠️ **Real function, UI tối giản** | `InventoryService` đã wired (add/remove/stack/capacity đủ). UI mới ở mức **text list**. Kho rỗng vì **chưa có nguồn sinh item** |
| CharacterScreen | ⚠️ **Real function, UI tối giản** | `CharacterService` wired ở S6-003. **Chưa có equipment slot, chưa có skill, chưa có chỉ số** |
| PopupScreen | ⚠️ **Real function, chưa có caller** | `ShowMessage()`/`ShowDeferred()`/`ShowInfo()`/`ShowError()` đã sẵn sàng, **chưa nơi nào gọi** |
| **DungeonScreen** | ❌ **PLACEHOLDER — 0% gameplay** | Panel trắng + chữ tĩnh. Data sẵn **11 dungeon, 122 enemy, 12 raid**; service `Dungeon/Combat/Loot/Enemy/Skill/StatusEffect` **cả 6 chưa wire**; **chưa có orchestrator dungeon run** |
| **CraftScreen** | ❌ **PLACEHOLDER — 0% gameplay** | Data sẵn **321 recipe**; `CraftService` chưa wire |
| **MerchantScreen** | ❌ **PLACEHOLDER — 0% gameplay** | `MerchantService` chưa wire |
| **SettingsScreen** | ❌ **PLACEHOLDER — 0% chức năng** | Không có service; nhưng `SaveService` đã sẵn `Save()`/`DeleteSave()` → dễ làm thật nhất |
| OverlayRoot | ❌ **Rỗng** | Chưa dùng; `UIScreenId.Loading` đã có trong enum nhưng chưa có object |

**Vấn đề layout ghi nhận (chưa sửa, để S6.5B):** `sizeDelta (100,100)` trên `HUDVisual`/`InventoryScreen`/`CharacterScreen`/`PopupScreen` khiến con tràn khung; `InventoryScreen`/`CharacterScreen` **không có Image nền** nên nhìn xuyên xuống HUD; `MoneyIcon`/`GemsIcon` là **con của Text** thay vì sibling.

## 3. Recommended Hierarchy By Game Function
Chi tiết: `Recommended_UI_Hierarchy_By_Game_Function.md`.

```
Main ├── Systems (UIRuntimeBootstrap)
     ├── Cameras (Main Camera, Global Light 2D)
     └── UI ├── UICanvas/SafeArea
            │   ├── HUD      → Currency, Navigation
            │   ├── Screens  → CharacterAndEquipment, InventoryAndItems,
            │   │              DungeonAndCombat, Crafting, Merchant, Settings
            │   ├── Popups
            │   └── Overlay
            └── EventSystem
```

## 4. Cleanup Status
Plan: `UI_HIERARCHY_Cleanup_Plan.md` → decision `UI_HIERARCHY_CLEANUP_PLAN_READY`
Implementation: `UI_HIERARCHY_Cleanup_Implementation_Report.md` → decision `UI_HIERARCHY_CLEANUP_PARTIAL_NEEDS_USER_REVIEW`

| Thay đổi | Trạng thái |
|---|---|
| A. Nhóm `Systems` ← UIRuntimeBootstrap | 🛠️ Tool sẵn sàng, **chờ user chạy menu** |
| B. Nhóm `Cameras` ← Main Camera, Global Light 2D | 🛠️ Như trên |
| C. Nhóm `UI` ← UICanvas, EventSystem | 🛠️ Như trên |
| D. Xếp 6 screen theo luồng chơi | 🛠️ Như trên |
| E. Đổi tên 4 root (`HudRoot`→`HUD`…) | ⛔ **Hoãn** — làm hỏng `UIWiringGenerator` (path tương đối cứng) |
| F. Nhóm `Currency`/`Navigation` trong HUD | ⛔ **Hoãn S6.5B** — nguy cơ lệch layout |
| G. Tách icon khỏi Text | ⛔ Hoãn S6.5B |
| H. Sửa `sizeDelta (100,100)` | ⛔ Hoãn S6.5B |
| I. Thêm Image nền cho Inventory/Character | ⛔ Hoãn S6.5B |
| J. Đổi `Btn_Back` binding sang component | ⛔ Hoãn — sửa runtime đã pass, cần verify lại |

**`Main.unity` hiện chưa bị sửa** trong phiên này (đã verify `diff` với backup → identical). Việc reparent giao cho Unity API qua menu thay vì sửa YAML thủ công, vì độ chắc chắn khi tự sửa YAML dưới ngưỡng 95%.

## 5. Next Required Phase — S6.5A Gameplay Function Completion

**Nguyên tắc cốt lõi:** Placeholder chỉ thay **asset/visual** còn thiếu. **Chức năng gameplay KHÔNG được coi là xong chỉ vì có panel hiển thị được.** Thiếu rule từ decode → đánh dấu `MANUAL_RULE_REQUIRED`, **tuyệt đối không tự bịa số**.

Thứ tự đề xuất (dễ → khó, đã cân nhắc mức đủ của rule):

| # | Nhóm | Việc | Rule đã đủ? |
|---|---|---|---|
| 1 | **Settings** | Wire `SaveService` vào nút Save/Delete + hiện version | ✅ Đủ, không cần rule gameplay |
| 2 | **Popups** | Gọi `ShowDeferred()` từ các nút chưa có chức năng | ✅ Đủ |
| 3 | **InventoryAndItems** | Wire `EquipmentService`, hiện item theo category + icon từ AssetCatalog | ✅ Đủ (607 item, rule equip có sẵn) |
| 4 | **CharacterAndEquipment** | Chỉ số qua `GetTotalStat()`, equipment slot, skill | ⚠️ `GetTotalStat()` còn TODO `manualRuleRequired` cho **level stat multiplier** (hardcode 1.0f) |
| 5 | **Crafting** | Wire `CraftService`, recipe/queue/claim | ⚠️ Cần xác nhận **rule timer + claim** |
| 6 | **Merchant** | Wire `MerchantService`, buy/sell/listing | ⚠️ Cần xác nhận **rule giá + restock** |
| 7 | **DungeonAndCombat** | Wire 6 service + viết **dungeon run orchestrator** (chưa tồn tại) | ⚠️ Cần rule **combat/damage/loot** — phần dễ "fake" nhất, phải cẩn thận nhất |

## Final Decision
# `S6_5_PREP_NEEDS_USER_REVIEW`

**Lý do:** Toàn bộ phần audit đã hoàn tất và đủ chi tiết để bước sang gameplay function audit — đã lập bản đồ vị trí/RectTransform/component/data-service của cả 66 object, phân định rạch ròi **chức năng thật vs placeholder** (4/7 màn chính là placeholder 0% gameplay, dù data và service code đều đã có sẵn), và đề xuất grouping theo module game kèm danh sách việc còn thiếu cho từng nhóm. Phần sắp xếp hierarchy đã có công cụ an toàn nhưng **chưa apply** — cần user chạy `GuildMaster → UI → Organize Hierarchy By Game Function` và xác nhận kết quả trước khi chốt sang `S6_5_PREP_UI_HIERARCHY_READY_FOR_GAMEPLAY_FUNCTION_AUDIT`.
