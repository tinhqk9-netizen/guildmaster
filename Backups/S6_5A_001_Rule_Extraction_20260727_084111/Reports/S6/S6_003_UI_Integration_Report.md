# S6-003 UI Integration Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/S6_003_004_PreImplementation_20260725_153634/`

## CharacterService API Audit
`CharacterService` (`Assets/_Game/Scripts/Runtime/Services/CharacterService.cs`) có constructor:
```
CharacterService(ISaveService saveService, IFormulaService formulaService, GameDatabase registry, RuntimeFactory runtimeFactory, IInventoryService inventoryService)
```
Cả 5 dependency này **đã có sẵn instance đang sống** trong `UIRuntimeBootstrap.Start()` (`save`, `formula`, `db`, `factory`, `inventory`) — không cần tạo thêm bất kỳ dependency mới nào, không có tham số nào yêu cầu rule/data chưa rõ. `GetAllCharacters()` trả `IReadOnlyList<CharacterRuntime>` với đủ field cần cho hiển thị (`Definition.id`, `Level`, `CurrentHp`, `Experience`, `Weapon/Armor/Accessory` instance id). Kết luận: **AN TOÀN để wire**, không cần deferred.

**Lưu ý đã ghi nhận (không sửa, không phải bug chặn):** `CharacterService.LoadFromSave()` có comment TODO nội bộ về việc equipment (Weapon/Armor/Accessory) chưa có rule khôi phục đầy đủ từ save khi item đó không nằm trong danh sách inventory chung. Vì hiện tại `SaveData.Characters` luôn rỗng (chưa có tạo nhân vật ở đâu), phần này không có gì để lộ ra sai lệch — giữ nguyên, không tự suy đoán rule.

## UI Integration Changes
| Screen | Before | After | Data Source | Status |
|---|---|---|---|---|
| Character | Đọc thẳng `ISaveService.CurrentData.Characters` (raw save data), bỏ qua tầng service | Đọc qua `ICharacterService.GetAllCharacters()` | `CharacterService` (dùng chung `save/formula/db/factory/inventory` đã có trong `UIRuntimeBootstrap`) | **FUNCTIONAL** (kiến trúc đúng tầng, vẫn hiển thị "No characters available." vì save rỗng — không fake) |
| Inventory | `IInventoryService.GetAllItems()` | Không đổi | `InventoryService` | FUNCTIONAL (không đổi) |
| HUD | `ISaveService.CurrentData.Money/Gems` | Không đổi | `SaveService` | FUNCTIONAL (không đổi) |
| Dungeon / Craft / Merchant / Settings | Placeholder trắng + Back | Không đổi | Không có | PLACEHOLDER_PANEL (giữ nguyên đúng S5) |
| Popup | Registered, chưa ai gọi | Không đổi | — | FUNCTIONAL, chưa có caller (không đổi) |

## Files Changed
| File | Change |
|---|---|
| `Runtime/UI/Character/CharacterScreen.cs` | `Initialize(ISaveService)` → `Initialize(ICharacterService)`; `Refresh()` đọc `GetAllCharacters()` thay vì `CurrentData.Characters` |
| `Runtime/Boot/UIRuntimeBootstrap.cs` | Thêm `var characterService = new CharacterService(save, formula, db, factory, inventory);`; đổi `chr.Initialize(save)` → `chr.Initialize(characterService)` |

## Service Wiring
| Service | Used By UI | Wired? | Reason |
|---|---|---|---|
| ItemService | Gián tiếp (qua Inventory) | ✅ (không đổi) | Đã wire từ S5 |
| InventoryService | ✅ InventoryScreen | ✅ (không đổi) | Đã wire từ S5 |
| **CharacterService** | ✅ CharacterScreen | **✅ MỚI wire ở S6-003** | Dependency đầy đủ, an toàn, đúng kiến trúc |
| EquipmentService | ❌ | ❌ | Không có UI nào cần hiển thị equipment riêng lúc này (Character screen mới chỉ show text tối giản); để dành khi có UI Equipment thật |
| EnemyService, SkillService, StatusEffectService, DungeonService, CombatService, LootService, QuestService, CraftService, MerchantService, OfflineProgressService | ❌ | ❌ | Không có UI thật nào cần (Dungeon/Craft/Merchant/Settings vẫn placeholder theo đúng scope S6-003: "không dựng full UI cho 4 màn này") |

## Deferred Areas
| Area | Reason | Safe Behavior |
|---|---|---|
| Dungeon/Craft/Merchant/Settings UI thật | Ngoài phạm vi S6-003 (chỉ đọc/hiển thị an toàn, không dựng UI mới) | Giữ nguyên placeholder trắng + Back, không crash |
| Character equipment display (Weapon/Armor/Accessory tên thật thay vì instance id) | Cần UI phong phú hơn (icon, tên item) — ngoài scope "cải thiện data source", không phải rule thiếu | Giữ hiển thị instance id thô như cũ (không đổi hành vi hiển thị, chỉ đổi nguồn data) |
| EquipmentService wiring | Không có UI nào gọi tới ở bước này | DEFERRED tới khi có UI Equipment |

## Verification
- **Compile:** Chưa thể tự chạy Unity compiler trong phiên này (không có MCP Unity) — cần user để Unity lấy focus reimport. Đã tự rà soát bằng mắt: `using GuildMaster.Runtime.Services;` đã có sẵn trong `UIRuntimeBootstrap.cs` (không cần thêm using); `CharacterScreen.cs` đổi `using GuildMaster.Runtime.Save;` → `using GuildMaster.Runtime.Services;` (đúng, vì không còn dùng `ISaveService` trực tiếp nữa); tất cả tham số constructor `CharacterService(...)` khớp đúng thứ tự/kiểu với các biến cục bộ đã có.
- **Play/click/navigation/Console:** Gộp chung với verify S6-004 bên dưới (cùng 1 lần Play test), vì cả 2 thay đổi nằm trong cùng file `UIRuntimeBootstrap.cs` và cần user Play 1 lần để xác nhận cả hai.

## Decision
# `S6_003_UI_INTEGRATION_DONE`

**Lý do:** Thay đổi nhỏ, cơ học, đúng kiểu/đúng using, không có gameplay logic mới, không có dependency thiếu. Xác nhận compile/runtime thật cuối cùng sẽ gộp chung 1 lần Play test với S6-004 (cùng đụng vào `UIRuntimeBootstrap.cs`) — xem "Manual Test Needed" ở report tổng hợp.

Tiếp tục S6-004.
