# S6.5A Stage 1 — Foundation (DecodeMath + Formula + SaveData)

**Ngày:** 2026-07-27 · **Backup:** `Backups/S6_5A_Auto_Stage1_Foundation_20260727_104235/` (221 file)

---

## Executive Summary

Stage 1 dựng nền toán học và schema save theo decode. **Phát hiện và sửa 4 bug thật** trong code có sẵn — đây là những lỗi sẽ làm sai lệch toàn bộ số liệu game nếu để nguyên.

| # | Bug phát hiện | Mức | Đã sửa |
|---|---|---|---|
| **B-1** | **`TruncatePrice` sai hoàn toàn** — code cũ dùng bậc thang 4 mức tự chế, decode dùng `j - (j<=1000000 ? j%100 : j%10000)` với ngưỡng bỏ qua `<= 10000` | 🔴 **Nghiêm trọng** | ✅ |
| **B-2** | **`GetStorageCapacityPrice` bậc `>60` dùng `10000`** — decode là **30000** (`DEFAULT_BACKOFF_DELAY_MILLIS`, decompiler thay literal) | 🔴 Nghiêm trọng | ✅ |
| **B-3** | **`GetStorageSpaces(…, additionalBonus)`** nhận bonus từ ngoài — decode tính từ **IAP flags** nội bộ (35/35/70) | 🟡 Vừa | ✅ |
| **B-4** | **`CalculateDamage_ManualPortRequired()` ném `NotImplementedException`** trong runtime path — vi phạm quality gate | 🟡 Vừa | ✅ Gỡ bỏ |

**Thêm mới quan trọng nhất:** `DecodeMath.Round(d) = (int)(d + 0.0001)`. Đây là hàm mà **toàn bộ** damage / stat / exp / giá tiền đi qua ở bước cuối. Nếu dùng `Math.Round` (banker's rounding) hay `Mathf.RoundToInt` thì `3.5` → `4` thay vì `3` — sai lệch lan ra mọi con số trong game.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Formulas/DecodeMath.cs` | **MỚI** — `Round`, `TruncatePrice`, `RollFromWeightedMap` |
| `Runtime/Formulas/DecodeMath.cs.meta` | **MỚI** |
| `Runtime/Formulas/IFormulaService.cs` | Viết lại: **20 method** + struct `PurchaseFlags`; gỡ `CalculateDamage_ManualPortRequired` |
| `Runtime/Formulas/FormulaService.cs` | Viết lại đủ **20/20 formula**; sửa B-1, B-2, B-3, B-4 |
| `Runtime/Save/SaveData.cs` | Thêm `MerchantOfferSaveData`; **+58 field core**; `NormalizeAfterLoad()`; `GetPurchaseFlags()` |
| `Runtime/Save/SaveService.cs` | Gọi `NormalizeAfterLoad()` ở **cả 4 nhánh** load (mới / bình thường / backup / fallback) |
| `Runtime/Services/InventoryService.cs` | `GetStorageSpaces(...)` → `StorageSpaces(..., data.GetPurchaseFlags())` |
| `Tests/EditMode/S6_5A_Stage1_FoundationTests.cs` | **MỚI** — 30 test |
| `Tests/EditMode/S6_5A_Stage1_FoundationTests.cs.meta` | **MỚI** |

---

## Rule Evidence Used

| Rule | Nguồn |
|---|---|
| `Utils.round(d) = (int)(d + 0.0001)` | `S6_5A_001E_AreaHelpers_DAD.md` · bit `4547007122018943789` = 0.0001 |
| `Utils.truncatePrice` | `Utils.java:889` (JADX) + `S6_5A_001E_Merchant_DAD.md` — **hai nguồn khớp nhau** |
| `Utils.rollFromWeightedMap` thang 1000 | `S6_5A_001E_AreaHelpers_DAD.md` · bit `4652007308841189376` = 1000.0 |
| 20 formula (F-01…F-20) | `S6_5A_001_Rule_Extraction_Report.md` + `Formulas.java` |
| Hằng androidx → literal | `S6_5A_001_Rule_Extraction_Report.md` (10000 / 300000 / 30000) |
| Save field từ `Data.java` | `S6_5A_001_Rule_Extraction_Report.md` (140 field) |
| 3 field merchant mới | `S6_5A_001E_Rule_Cleanup_Report.md` (buy rule) |

---

## Formula Coverage — 20/20

| ID | Method | Trạng thái |
|---|---|---|
| F-01 | `TotalStarsToNextLp` | ✅ **MỚI** (quest reward dùng) |
| F-02 | `GetQuartersPrice` | ✅ Sửa (literal 10000/300000) |
| F-03 | `GetTavernCapacityPrice` | ✅ Giữ + truncate đúng |
| F-04 | `GetTavernTimePrice` | ✅ **MỚI** |
| F-05 | `GetStorageCapacityPrice` | ✅ **Sửa B-2** (30000) + **không truncate** |
| F-06 | `GetMarketListingsPrice` | ✅ **MỚI** |
| F-07 | `GetMarketTimePrice` | ✅ **MỚI** |
| F-08 | `GetWorkshopQueuePrice` | ✅ **MỚI** |
| F-09 | `GetWorkshopTimePrice` | ✅ **MỚI** |
| F-10 | `GetShelterPrice` | ✅ **MỚI** |
| F-11 | `GetShelterAutofeedPrice` | ✅ **MỚI** |
| F-12 | `GetQuartersCapacity` | ✅ **MỚI** (4 IAP flag) |
| F-13 | `GetTavernVisitorInterval` | ✅ **MỚI** (trả **ms**) |
| F-14 | `GetTavernCapacity` | ✅ **MỚI** |
| F-15 | `MarketListings` | ✅ **MỚI** |
| F-16 | `WorkshopQueue` | ✅ **MỚI** |
| F-17 | `StorageSpaces` | ✅ **Sửa B-3** (IAP 35/35/70) |
| F-18 | `ShelterCapacity` | ✅ **MỚI** |
| F-19 | `ExperienceToNextLevel` | ✅ Giữ (đã đúng) |
| F-20 | `FoodToNextLevel` | ✅ Giữ (đã đúng) |

**Trước Stage 1: 6/20 (2 method đúng, 4 method sai hoặc lệch ngữ nghĩa). Sau: 20/20.**

---

## SaveData — field bổ sung

| Nhóm | Field | Ghi chú |
|---|---|---|
| Purchase flags (5) | `StarterPackPurchased`, `AdventurerPackPurchased`, `MerchantPackPurchased`, `ImperialVanguardPurchased`, `UnholyCrusadePurchased` | Mặc định `false`; **không** implement store — chỉ là đầu vào của 7 formula |
| Time (4) | `LastAccess`, `LastHourTriggered`, `Last24Triggered`, `LastWeekTriggered` | `LastAccess` mở khoá offline progress |
| Tavern (10) | `NextTavernVisit`, `TavernLocked`, `TutorialStep`, `TavernGuests`, `LevelQuarters`, `UpgradeQuarters`, `LevelTavernCapacity`, `UpgradeTavernCapacity`, `LevelTavernTime`, `UpgradeTavernTime` | |
| Workshop/Market/Shelter (9) | `UpgradeWorkshopTime`, `LevelWorkshopQueue`, `UpgradeWorkshopQueue`, `UpgradeMarketTime`, `LevelMarketListings`, `UpgradeMarketQueue`, `LevelShelter`, `UpgradeShelter`, `LevelShelterAutofeed` | |
| Doctrine (17) | 8 cặp `<X>Level`/`<X>Progress` + `DoctrineMaxed` | Quest reward ghi vào đây |
| Merchant (5) | `MerchantRegularStockItems`, `MerchantSpecialReserve`, `UniqueItemsLost`, `NewMerchantRegularItems`, `NewMerchantSpecialItems` | Từ buy rule |
| Quest (3) | `QuestsSeen`, `QuestsRefreshed`, `QuestsCompleted` | |
| Settings (9) | 8 toggle + `SettingsLanguage` | |
| Stats (5) | `ItemsCrafted`, `ItemsSold`, `MaxWealth`, `MaxAdventurerTier`, `MaxAdventurersOwned` | |

**Tổng thêm: 58 field** (17 → 75). Các field còn lại của `Data.java` (redeem code, review flag, 20 area instance) **chưa port** — không cần cho core loop, ghi nhận `Deferred`.

### Save migration
`NormalizeAfterLoad()` chạy ở **cả 4 nhánh** của `SaveService.Load()`. JsonUtility để field vắng mặt ở giá trị `default`, biến mọi list mới thành `null` → normalize thay bằng list rỗng, không đụng giá trị đã có.

---

## Tests — 30 test

| Nhóm | Số test | Nội dung đáng chú ý |
|---|---:|---|
| `DecodeMath.Round` | 3 | Chính xác các case `Math.Round` sẽ sai: 2.5→2, **3.5→3**, 2.7→2 |
| `DecodeMath.TruncatePrice` | 3 | Biên `<= 10000` giữ nguyên; 12345→12300; 1234567→1230000 |
| `DecodeMath.RollFromWeightedMap` | 3 | **Có test khoảng trống "không drop"** khi tổng weight < 1000 |
| Formula F-01…F-20 | 17 | Giá trị đối chiếu tính độc lập từ decode |
| SaveData | 6 | Normalize, round-trip, **legacy save không mất data** |

### Tự kiểm chứng trước khi chạy Unity
Em tính lại toàn bộ giá trị kỳ vọng bằng Python độc lập với code C#. **Phát hiện 1 lỗi trong chính test em vừa viết**: `Round(2.9999)` cho **3** chứ không phải 2 — vì `2.9999 + 0.0001 = 3.0`. Đã sửa và thêm case `2.9998 → 2` để phân định rõ ranh giới epsilon.

Kết quả verify (Python vs kỳ vọng test): **khớp 100%** sau khi sửa.

---

## Quality Gates

| Gate | Kết quả | Bằng chứng |
|---|---|---|
| Không còn `NotImplementedException` trong runtime path | ✅ | `grep "CalculateDamage_ManualPortRequired"` → 0 kết quả |
| Không còn call site API cũ | ✅ | `grep "GetStorageSpaces"` → 0 kết quả |
| Không sửa `.csproj` | ✅ | Không đụng |
| Không fake fallback trong core logic | ✅ | `NormalizeAfterLoad` chỉ vá `null` → list rỗng, không bịa giá trị |
| Không dùng `Math.Round`/`Mathf.RoundToInt` cho decode math | ✅ | `DecodeMath.Round` dùng `(int)(d + 0.0001)` |
| Cú pháp cân bằng | ✅ | Ngoặc cân ở cả 3 file chính |
| **Unity compile** | ⚠️ **Chưa verify** | Không có MCP Unity trong phiên này — xem mục dưới |
| **Test chạy thật** | ⚠️ **Chưa chạy** | Cần Unity Test Runner |

---

## ⚠️ Hạn chế trung thực

**Chưa có bằng chứng compile/test từ chính Unity.** Đã làm để giảm rủi ro:
- Rà toàn bộ call site của mọi API bị đổi chữ ký (chỉ 2, cả hai đã sửa hoặc còn hợp lệ)
- Kiểm tra `using`/namespace của mọi file mới
- Kiểm tra cân bằng ngoặc
- Verify độc lập bằng Python toàn bộ giá trị kỳ vọng — **và đã bắt được 1 lỗi thật nhờ đó**

Nhưng **chưa thể khẳng định 0 lỗi CS**. Theo 6 tầng nghiệm thu, Stage 1 hiện đạt:

| Tầng | Trạng thái |
|---|---|
| 1. RULE_EXTRACTED | ✅ |
| 2. IMPLEMENTED | ✅ |
| 3. WIRED_TO_RUNTIME | ⚠️ Một phần (`InventoryService` dùng `StorageSpaces`; các formula mới chưa có caller — sẽ nối ở Stage 3+) |
| 4. EXPOSED_TO_UI | ❌ Chưa (không thuộc phạm vi Stage 1) |
| 5. PLAYER_ACTION_WORKS | ❌ Chưa |
| 6. SAVE_VERIFIED | ⚠️ Có test round-trip + legacy, **chưa chạy thật** |

→ **Status: `PARTIAL_NEEDS_IMPLEMENTATION`**, không phải `DONE_VERIFIED`. Đúng quy tắc: thiếu tầng nào thì không được gọi DONE.

---

## Restore Instruction

1. Copy đè `Backups/S6_5A_Auto_Stage1_Foundation_20260727_104235/Assets/_Game/Scripts/Runtime/*` → `Assets/_Game/Scripts/Runtime/`
2. Xoá 2 file mới: `Runtime/Formulas/DecodeMath.cs(.meta)`, `Tests/EditMode/S6_5A_Stage1_FoundationTests.cs(.meta)`
3. Mở Unity, chờ reimport, verify Console 0 lỗi

---

## Final Status

# `STAGE1_IMPLEMENTED_NEEDS_UNITY_COMPILE_VERIFY`

**Đã làm:** 20/20 formula theo decode · `DecodeMath` với 3 primitive · 58 save field mới · migration an toàn 4 nhánh · 30 test · **sửa 4 bug thật**.

**Còn lại:** cần 1 lần Unity reimport + chạy EditMode test để chốt. Không có blocker logic nào — mọi rule dùng đều ≥95% confidence và truy được về evidence.
