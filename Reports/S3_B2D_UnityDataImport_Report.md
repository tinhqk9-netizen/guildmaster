## Unity Data Import Wiring Summary

| Area | Status | Notes |
|---|---|---|
| RecipeDefinition loading | DONE | `DatabaseBuilder` đã support `"recipes"`. JSON từ converter map tự động với `IngredientData` (chứa `ItemId`, `Amount`). |
| DungeonDefinition merchant fields | DONE | JSON mảng `RegularMerchantOffers` và `SpecialMerchantOffers` được deserialize thẳng vào List của `DungeonDefinition`. Các object trong mảng map chuẩn với `MerchantOfferData` (chứa `ItemId`, `StackCount`, `Weight`). |
| Data files placement | DONE | Do `EditorExternalGameDataProvider` hỗ trợ đọc thẳng từ converter staging, em đã cập nhật lại relative path để Editor tự link sang `D:\Tinh\Game Decode Converter\output\production_staging` mà không cần copy thủ công. |
| Compile | DONE | Đã tạo test script `S3B2DDataImportTests.cs` tại `Assets\_Game\Scripts\Tests\Editor\`. Unity tự động compile sạch không báo lỗi. |

## Files Modified

- `Assets\_Game\Scripts\Infrastructure\DataProviders\EditorExternalGameDataProvider.cs`: Sửa path `../../Tools/DecodeConverter` thành `../../Game Decode Converter` để Unity Editor tìm thấy thư mục xuất của python converter.
- `Assets\_Game\Scripts\Tests\Editor\S3B2DDataImportTests.cs`: Tạo mới bài Test trong NUnit EditMode để in log và Assert loaded data counts.

## Files Copied

| Source | Destination |
|---|---|
| N/A | N/A (Dùng EditorExternalGameDataProvider đọc trực tiếp data từ source gốc) |

## Loaded Data Validation

| Check | Result |
|---|---|
| Recipe count loaded | 321 |
| Dungeon count loaded | 11 |
| Total regular merchant offers loaded | 59 |
| Total special merchant offers loaded | 93 |
| First recipe sample | `recipe_absolutezero`, Output: `absolutezero`, Ingredients count: 0 |
| First merchant offer sample | Từ `blackwater_port`: `ghostwoodboard x2` (weight: 170) |

## Hardcoded Value Check

| Check | YES/NO |
|---|---|
| Added fake recipe | NO |
| Added fake merchant stock | NO |
| Added fake price | NO |
| Added fake craft time | NO |
| Added fake sell time | NO |
| Implemented CraftService logic | NO |
| Implemented MerchantService logic | NO |
| Implemented OfflineService logic | NO |

## Remaining Gaps

- CraftService logic deferred.
- MerchantService logic deferred.
- OfflineService integration deferred.
- Recipes with `manualRuleRequired` must not be craftable until parser improved/manual rule resolved (hiện tại `Ingredients` count = 0, cần filter ở logic Craft sau này).

## Recommendation

READY_FOR_S3_BATCH2_CORE_LOGIC_PLANNING
