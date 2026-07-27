# Decode → Unity Gap Classification Report

**Ngày:** 2026-07-25

## Bảng phân loại gap

| Gap ID | Domain | Gap Type | Description | Severity | Blocks Gameplay Function? | Blocks Assets? | Recommended Phase |
|---|---|---|---|---|---|---|---|
| **G-D01** | Localization | Data gap | `localization.json` = `[]` rỗng. Decode dùng `R.string` ID (int) trong `resources/`, converter chưa bóc được | Medium | Không (text hard-code tạm được) | Không | `Deferred_ManualRuleRequired` |
| **G-D02** | Assets manifest | Data gap | `assets_manifest.json` = `[]` rỗng | Low | Không | Không (S5 đã có AssetCatalog riêng) | `Deferred_ManualRuleRequired` |
| **G-D03** | Doctrine | Data gap | **Không có `doctrines.json`** — decode có `Doctrine.java` + 8 instance nhưng converter chưa bóc | **High** | **CÓ** — doctrine ảnh hưởng damage modifier + quest amount | Không | `Deferred_ManualRuleRequired` (cần chạy lại converter hoặc bóc tay) |
| **G-D04** | Save schema | Data gap | `SaveData` có **17 field** vs `Data.java` **140 field**. Thiếu: `lastAccess` (chặn offline), 8 doctrine level/progress, 20 area state, 9 setting, thống kê, 4 mốc thời gian, IAP, tutorialStep, các cặp level/upgrade Tavern/Quarters/Shelter | **High** | **CÓ** — thiếu `lastAccess` là chặn cứng offline progress | Không | `S6.5A_Function_Completion` |
| **G-B01** | Formulas | Backend logic gap | `FormulaService` port **6/20** method. Thiếu 14: TavernTimePrice, MarketListingsPrice, MarketTimePrice, WorkshopQueuePrice, WorkshopTimePrice, ShelterPrice, ShelterAutofeedPrice, QuartersCapacity, TavernVisitorInterval, TavernCapacity, marketListings, workshopQueue, shelterCapacity, totalStarsToNextLp | **High** | **CÓ** — chặn HQ upgrade, Tavern, Shelter, Market/Workshop capacity | Không | `S6.5A_Function_Completion` (rule có sẵn trong `Formulas.java`, port trực tiếp được) |
| **G-B02** | Combat / damage | Backend logic gap — **missing rule** | **`FormulaService.CalculateDamage_ManualPortRequired()` là stub rỗng.** Công thức damage chưa port dòng nào | **Critical** | **CÓ — chặn toàn bộ combat** | Không | `S6.5A_Function_Completion` (rule nằm ở `Area.dealDamage()` dòng 2213 — **đọc được**, cần port cẩn thận) |
| **G-B03** | Character stats | Backend logic gap — **risky assumption** | `CharacterService.GetTotalStat()` **hardcode `levelMultiplier = 1.0f`** kèm TODO manualRuleRequired → chỉ số theo level **hiện đang SAI** so với decode. Code chạy không lỗi nên dễ bị hiểu nhầm là đã xong | **Critical** | **CÓ** — mọi số liệu nhân vật/combat đều lệch | Không | `S6.5A_Function_Completion` (cần bóc rule từ `Adventurer.java`/`Entity.calculate*`) |
| **G-B04** | Combat engine | Backend logic gap | `CombatService` **139 dòng** vs `Area.java` **3164 dòng**. Thiếu: `dodge()`, `retaliate()`, `heal()`, `cast()`, `increaseMana()`, `calculateCriticalMultiplier()`, `selectTargets()` (7 strategy), `decideTurnsOrder()`, `reanimate()`, `healingNova()`, `applyOnDeathStatusEffects()`, pet actions, inner class `Skill` (8 modifier) | **Critical** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-B05** | StatusEffect | Backend logic gap | Thiếu `resolveStatus()` — ~180 dòng logic resolve mỗi lượt trong `Area.java:990-1171`; thiếu `EndOfTurnAction`, immunity, on-death effect | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-B06** | Skill | Backend logic gap | `SkillService` chỉ **18 dòng** (1 method tạo runtime). Chưa port `Area.Skill` với targetSelectionMode/criticalAmplification/healing/statusEffect/damageAmplification/executionThreshold/recastOnKill/reviveProbability | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-B07** | Enemy | Backend logic gap | Decode có **1 class riêng mỗi enemy** override `calculate*`; Unity chỉ có factory tạo từ definition → hành vi riêng của từng enemy chưa port | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-B08** | Dungeon | Backend logic gap | `DungeonService` 202 dòng vs `Area.java` 3164. Thiếu `tick()`, `enterRoom()`, `respawn()`, `incrementProgress()`, unlock chain (`listAreasUnlocked()`), darkness, `AdventureRecap` | **Critical** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-B09** | Craft | Backend logic gap — **missing rule** | `CraftService.CanCraft()` **trả về `ManualRuleRequired`**. Rule timer/claim/`maxCraftableAmount` chưa xác nhận | **High** | **CÓ** | Không | `Deferred_ManualRuleRequired` → sau khi bóc rule thì `S6.5A_Function_Completion` |
| **G-B10** | Merchant | Backend logic gap — **missing rule** | `MerchantService.BuyItem()` **trả `DeferredPriceOrCurrencyRule`**. Rule giá (`truncatePrice`), restock, roll offer chưa xác nhận | **High** | **CÓ** | Không | `Deferred_ManualRuleRequired` → rồi `S6.5A_Function_Completion` |
| **G-B11** | Quest | Backend logic gap | `QuestService` 87 dòng vs `QuestsManager.java` 468. Thiếu `extractQuests`, `calculateDifficulty`, `rollRarity`, `setupDoctrineAmounts`, `setupAccessibleQuests`, `realignQuests` | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-B12** | Pet | Backend logic gap | **Không có `PetService`**. Decode `Pet.java` 290 dòng + `PetAbility` + `rollPetAbility` + 4 pet action trong combat | Medium | **CÓ** (pet tham chiến combat) | Không | `S6.5A_Function_Completion` |
| **G-B13** | Doctrine | Backend logic gap | **Hoàn toàn không tồn tại trong Unity** (`grep -ri "doctrine"` → 0 kết quả) | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-B14** | Tavern / Quarters / Shelter | Backend logic gap | Không có service nào. Thiếu `newTavernVisitor()`, visitor interval, capacity, autofeed, `checkDismissedAdventurersExpiration` | **Critical** | **CÓ — đây là nút thắt chặn cả chuỗi gameplay** (không tuyển được adventurer) | Không | `S6.5A_Function_Completion` |
| **G-B15** | Offline progress | Backend logic gap | `SaveData` **thiếu `lastAccess`** → không tính được delta. Thiếu `tick60()`, `nextTimeTick()`, `refreshCooldowns()`, 4 mốc thời gian, cap delta (`MainActivity:878-880`) | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-B16** | Raid | Backend logic gap | Không có service; 12 raid trong JSON không dùng | Medium | **CÓ** | Không | `Future_Out_Of_Scope` (cần user xác nhận có trong scope) |
| **G-B17** | Achievements / IAP | Backend logic gap | Chưa port `AchievementsUtils.java`, `IAPWrapper.java` | Low | Không (không phải core loop) | Không | `Future_Out_Of_Scope` (cần user xác nhận) |
| **G-R01** | 11 service | Runtime integration gap | **11/14 service chưa được `new` lên ngoài test**: Equipment, Enemy, Skill, StatusEffect, Dungeon, Combat, Loot, Quest, Craft, Merchant, OfflineProgress | **Critical** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-R02** | Dungeon run orchestrator | Runtime integration gap | **Không tồn tại lớp nào điều phối 1 lượt chạy dungeon** (tương đương `Area.tick()`) | **Critical** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-R03** | Bootstrapper trùng lặp | Runtime integration gap | 2 class `Bootstrapper` cùng tên, cả 2 không gắn scene, mỗi cái tự tạo `GameDatabase` riêng | Low | Không | Không | `S6.5A_Function_Completion` (dọn khi wire service) |
| **G-R04** | Popup | Runtime integration gap | `ShowDeferred()`/`ShowInfo()`/`ShowError()` có sẵn nhưng **0 caller** | Low | Không | Không | `S6.5A_Function_Completion` (fix rất nhanh) |
| **G-U01** | Dungeon UI | UI/function gap | Panel trắng + chữ tĩnh. Không có list/chọn/start/result | **Critical** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-U02** | Craft UI | UI/function gap | Panel trắng. Không có recipe list/craft/queue/claim | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-U03** | Merchant UI | UI/function gap | Panel trắng. Không có buy/sell/listing | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-U04** | Settings UI | UI/function gap | Panel trắng. Không có nút Save/Delete/version. `SaveData` chưa có 9 setting field | Medium | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-U05** | Inventory UI | UI/function gap | **Read-only text list**. Không có grid/icon/filter/equip/use/sell, không hiện capacity | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-U06** | Character UI | UI/function gap | **Read-only text list**. Không có stat panel, equipment slot, skill, level/exp bar | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-U07** | Quest UI | UI/function gap | **Không có màn quest nào** (chỉ 6 nav button, thiếu Quest) | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-U08** | Overlay | UI/function gap | `OverlayRoot` rỗng; `UIScreenId.Loading` chưa có screen | Low | Không | Không | `S6.5B_Asset_Finalization` |
| **G-S01** | Save mutation | Save/progression gap | **Chưa có action gameplay nào mutate save** → `save.json` luôn toàn giá trị mặc định. Chỉ chứng minh được "ghi file hoạt động", **chưa chứng minh "ghi đúng nội dung thay đổi"** | **High** | **CÓ** | Không | `S6.5A_Function_Completion` |
| **G-S02** | Save round-trip test | Save/progression gap | Chưa có test: action → save → load → verify giá trị đổi | **High** | Không (nhưng chặn tin cậy) | Không | `S6.5A_Function_Completion` |
| **G-S03** | Setting persistence | Save/progression gap | 9 setting của decode không có field tương ứng trong `SaveData` | Medium | **CÓ** (Settings UI cần) | Không | `S6.5A_Function_Completion` |
| **G-A01** | Item/enemy/skill icon | Asset-only gap | Chưa map icon cho 607 item / 122 enemy / 227 skill ra UI (AssetCatalog S5 đã có sprite nhưng chưa dùng ở screen nào) | Medium | **KHÔNG** — dùng ô vuông xám placeholder được | **CÓ** | `S6.5B_Asset_Finalization` |
| **G-A02** | UI skin | Asset-only gap | Panel/button đang là màu trơn, chưa dùng `ui_kit.png`/`ui_dialog.png` | Low | **KHÔNG** | **CÓ** | `S6.5B_Asset_Finalization` |
| **G-A03** | Animation / VFX / Audio | Asset-only gap | Chưa dùng sprite animation, VFX (16 MB có sẵn), chưa có audio | Low | **KHÔNG** | **CÓ** | `S6.5B_Asset_Finalization` |
| **G-A04** | Character portrait | Asset-only gap | 8 portrait trong AssetCatalog chưa hiện ở Character screen | Low | **KHÔNG** | **CÓ** | `S6.5B_Asset_Finalization` |
| **G-P01** | Android StreamingAssets | Android/build gap | `StreamingAssetsGameDataProvider` throw `NotSupportedException` khi `UNITY_ANDROID && !UNITY_EDITOR` → **APK không load được data nào** | **Critical** (cho Android) | Không (Editor/Standalone OK) | Không | `S7_Android_Build_Fix` |
| **G-P02** | Android Build Support | Android/build gap | Unity Hub chỉ cài WindowsStandaloneSupport module | Medium | Không | Không | `S7_Android_Build_Fix` |
| **G-P03** | Standalone smoke test | Android/build gap | Chưa từng build Standalone → nhánh non-Editor chưa được kiểm chứng | Medium | Không | Không | `S7_Android_Build_Fix` |

## Thống kê gap

| Gap Type | Số lượng | Trong đó Critical |
|---|---|---|
| Data gap | 4 | 0 (2 High) |
| Backend logic gap | 17 | **5** (G-B02 damage, G-B03 stat, G-B04 combat, G-B08 dungeon, G-B14 tavern) |
| Runtime integration gap | 4 | **2** (G-R01 11 service, G-R02 orchestrator) |
| UI/function gap | 8 | **1** (G-U01 dungeon) |
| Save/progression gap | 3 | 0 (2 High) |
| Asset-only gap | 4 | 0 |
| Android/build gap | 3 | **1** (G-P01, chỉ với Android) |
| **TỔNG** | **43** | **9** |

## Phân bổ theo phase

| Recommended Phase | Số gap | Ghi chú |
|---|---|---|
| `S6.5A_Function_Completion` | **28** | Phần lớn khối lượng — đây là phase quan trọng nhất |
| `S6.5B_Asset_Finalization` | 5 | Chỉ thuần visual, **không chặn chức năng** |
| `S7_Android_Build_Fix` | 3 | |
| `Deferred_ManualRuleRequired` | 5 | G-D01, G-D02, G-D03, G-B09 (craft rule), G-B10 (merchant rule) |
| `Future_Out_Of_Scope` | 2 | G-B16 (Raid), G-B17 (Achievements/IAP) — **cần user xác nhận** |

## Kết luận về asset vs chức năng
**Chỉ 5/43 gap là asset-only** (G-A01…G-A04 + G-U08) và **không gap nào trong số đó chặn chức năng gameplay**. Nghĩa là: **thiếu asset KHÔNG phải nguyên nhân game chưa chơi được.** Nguyên nhân là 28 gap chức năng thật.

Toàn bộ 28 gap chức năng đó **đều có thể triển khai với placeholder asset** (ô vuông xám, text label, button trơn) — trừ 5 gap cần bóc rule từ decode trước (`Deferred_ManualRuleRequired`).
