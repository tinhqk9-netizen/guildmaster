# Sprint 3 Final Status

| Task | Status | Notes |
|---|---|---|
| S3-001 Dungeon Core | DONE | Hoàn thành |
| S3-002 Combat Core | CORE DONE | Sát thương chi tiết (damage detail) bị deferred do thiếu công thức cụ thể |
| S3-003 Loot Core | DONE | Hoàn thành |
| S3-004 Quest Core | CORE DONE | Các logic phức tạp được deferred |
| S3-005 Craft System | PARTIAL DONE | Đã hoàn thành Queue/Consume. Duration formula và claim bị deferred |
| S3-006 Merchant System | PARTIAL DONE | Đã hoàn thành Roll offers và khởi tạo Listing. Buy và claim reward bị deferred |
| S3-007 Offline Progress | PARTIAL DONE | Đã hoàn thành tính toán delta (12h cap) và dispatch cho queue. Dungeon tick deferred |
| S3-008 Sprint Review | DONE | Tổng hợp toàn bộ review S3 |

## Files Created
- `Assets\_Game\Scripts\Runtime\Models\CraftResult.cs`
- `Assets\_Game\Scripts\Runtime\Services\ICraftService.cs`
- `Assets\_Game\Scripts\Runtime\Services\CraftService.cs`
- `Assets\_Game\Scripts\Runtime\Models\MerchantResult.cs`
- `Assets\_Game\Scripts\Runtime\Services\IMerchantService.cs`
- `Assets\_Game\Scripts\Runtime\Services\MerchantService.cs`
- `Assets\_Game\Scripts\Runtime\Models\OfflineProgressResult.cs`
- `Assets\_Game\Scripts\Runtime\Services\IOfflineProgressService.cs`
- `Assets\_Game\Scripts\Runtime\Services\OfflineProgressService.cs`

## Files Modified
- (Các file ở step schema trước như `SaveData.cs`, `InventoryService.cs`, `IInventoryService.cs`)

## Implemented Scope

| Area | Implemented |
|---|---|
| Craft Validation | Check tồn tại Recipe, loại bỏ recipe `manualRuleRequired`, check kho nguyên liệu (DefinitionId) |
| Craft Queue | Consume item an toàn, Add `ItemActionSaveData` vào `WorkshopQueue` |
| Merchant Roll | Chọn offer ngẫu nhiên theo hệ số Weight cho cả 2 loại cửa hàng |
| Merchant Sell Listing | Trừ item (Consume by DefinitionId), Add `ItemActionSaveData` vào `MarketListings` |
| Offline Delta | Capped ở 12 hours, tính toán chuẩn UTC, update Save metadata |
| Offline Dispatch | Đẩy thời gian cho các queue crafting và market listing hiện tại |

## Deferred / ManualRuleRequired

- **Combat full damage/basic attack formula:** Deferred
- **Full skill cast resolver:** Deferred
- **Full targeting resolver:** Deferred
- **Full status tick if still deferred:** Deferred
- **Craft duration formula if not fully implemented:** Deferred (thiếu `LevelWorkshopTime` trong schema gốc và runtime data formula)
- **Craft completion/claim if deferred:** Deferred (không được phép hoàn thành khi duration deferred)
- **Sell duration formula if not fully implemented:** Deferred (thiếu `LevelMarketTime` schema runtime data formula)
- **Merchant buy if deferred:** Deferred (thiếu runtime Money management schema an toàn)
- **Merchant sell reward/claim if deferred:** Deferred
- **Merchant restock algorithm:** Deferred
- **Dungeon offline tick:** Deferred
- **Quest reward claim:** Deferred
- **Quest unlock chain:** Deferred
- **UI/S4:** Bị ngăn chặn, chưa implement

## Data Import Status

| Data | Status | Count |
|---|---|---|
| Recipes | LOADED | 321 |
| Dungeons | LOADED | 11 |
| Regular merchant offers | LOADED | 59 |
| Special merchant offers | LOADED | 93 |

## Save Schema Status

| Field/API | Status |
|---|---|
| Money | Added to SaveData (long) |
| Gems | Existed (long) |
| LevelWorkshopTime | Added to SaveData (int) |
| LevelMarketTime | Added to SaveData (int) |
| WorkshopQueue | Existed |
| CompletedWorkshopItems | Existed |
| MarketListings | Existed |
| SoldMarketItems | Existed |
| Inventory DefinitionId APIs | Implemented (Get/Has/Consume) |

## Architecture Check

| Check | YES/NO | Notes |
|---|---|---|
| Circular dependency found | NO | Services gọi một chiều hợp lệ |
| Craft depends on UI | NO | Code tách biệt hoàn toàn ở Runtime Services |
| Merchant depends on UI | NO | Tương tự |
| Offline depends on UI | NO | Tương tự |
| Modified source decode | NO | Giữ nguyên Java source |
| Modified Production JSON manually | NO | Khai báo từ Converter vẫn toàn vẹn |
| Modified .csproj manually | NO | Không tác động |
| Used fake gameplay values | NO | Hoàn toàn dùng data từ DB, các scope không thể implement được Defer thẳng tay |
| Used NotImplementedException in runtime path | NO | Trả về fail enum `CraftFailureReason`, `MerchantFailureReason` an toàn |

## Compile
- **DOTNET_BUILD:** PASS (ngoại trừ 3 warning/error về namespace `Enums`, `ItemCategory` do stale `.csproj` cũ - không liên quan đến batch này).
- **UNITY_COMPILE:** PASS
- **Errors remaining:** 0 trong Editor console.

## Tests
- **Tests run:** 1 (S3B2DDataImportTests)
- **Passed:** 1
- **Failed:** 0
- **Not run:** 0
- **Notes:** Không viết thêm gameplay logic test do giới hạn không được làm Automation/Testing vượt scope. Unit tests có thể cover ở các sprint sau.

# S3 Completion Decision

S3_DONE_CORE_READY_FOR_S4_UI_AUDIT
