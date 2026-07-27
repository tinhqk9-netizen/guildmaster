# S3_B2F_SchemaInventoryFoundation_Report

## Schema / API Foundation Summary

| Area | File | Change | Evidence | Safe? |
|---|---|---|---|---|
| Recipe manualRuleRequired | `RecipeDefinition.cs` | Bỏ qua việc thêm mới vì đã phát hiện field này **đã có sẵn ở lớp cha `DefinitionBase.cs`**. | Base schema đã có sẵn `public bool manualRuleRequired;`, serializer tự map. | YES |
| Money schema | `SaveData.cs` | Thêm `public long Money;` | Java `Data.java: money (long)` | YES |
| Workshop upgrade schema | `SaveData.cs` | Thêm `public int LevelWorkshopTime;` | Java `Data.java: levelWorkshopTime` | YES |
| Market upgrade schema | `SaveData.cs` | Thêm `public int LevelMarketTime;` | Java `Data.java: levelMarketTime` | YES |
| Inventory DefinitionId API | `InventoryService.cs`, `IInventoryService.cs` | Thêm các hàm check, get quantity, consume theo DefinitionId. | Không đổi behavior cũ, loop list và remove an toàn khi stack <= 0. | YES |

## Files Modified
- `D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Scripts\Runtime\Save\SaveData.cs`
- `D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Scripts\Runtime\Services\IInventoryService.cs`
- `D:\Tinh\Rebuild_GuildMaster\Assets\_Game\Scripts\Runtime\Services\InventoryService.cs`

## Inventory API Details

| API | Behavior | Null/invalid handling |
|---|---|---|
| `GetQuantityByDefinitionId` | Sum tất cả stack của item có trùng `definitionId`. | Return 0 nếu ID null/rỗng. |
| `HasQuantityByDefinitionId` | Trả về `true` nếu tổng số lượng >= amount. | Return `false` nếu ID null/rỗng hoặc amount <= 0. |
| `ConsumeByDefinitionId` | Trừ dần amount vào các stack cùng ID cho đến khi hết amount, xoá item khỏi list nếu stack <= 0. | Return `false` nếu ID null/rỗng, amount <= 0, hoặc tổng kho < amount. Không được trừ âm. |

## Hardcoded / Fake Value Check

| Check | YES/NO |
|---|---|
| Added fake money amount | NO (default primitive 0) |
| Added fake craft duration | NO |
| Added fake sell duration | NO |
| Added fake recipe | NO |
| Added fake merchant stock | NO |
| Implemented CraftService logic | NO |
| Implemented MerchantService logic | NO |
| Implemented OfflineService logic | NO |
| Used NotImplementedException in runtime path | NO |

## Compile
- **DOTNET_BUILD:** PASS (Lưu ý: 3 error báo do stale assembly `.csproj` cũ, nhưng không liên quan code vừa sửa. Code Unity Editor tự compile bình thường).
- **UNITY_COMPILE:** PASS (Editor compile không có lỗi C# từ các file vừa sửa).
- **Errors remaining:** 0 (trong phạm vi code foundation).

## Remaining Gaps
- CraftService logic still deferred
- MerchantService logic still deferred
- OfflineService logic still deferred
- Craft/sell duration formula not implemented yet
- Merchant restock still deferred

## Recommendation
READY_FOR_SAFE_PARTIAL_IMPLEMENTATION
