# S6.5A Decode Logic Coverage Audit Summary

**Ngày:** 2026-07-25
**Decode source:** `D:\Tinh\Guild Master - Idle Dungeons\sources\it\paranoidsquirrels\idleguildmaster` (1150 file `.java`)
**Unity project:** `D:\Tinh\Rebuild_GuildMaster`

## Executive Summary

### Câu hỏi: "Dự án hiện tại đã lấy đủ source backend, chức năng, logic game từ decode sang Unity chưa?"

# ❌ CHƯA ĐỦ.

Trả lời thẳng: dự án đã port **hạ tầng rất tốt** (data loading, save I/O, UI framework, boot flow) nhưng **phần logic gameplay cốt lõi mới port khoảng 10–15%**. Việc mở được 8 panel và có 1531 record trong database **không đồng nghĩa chức năng game đã có**.

**Bằng chứng định lượng cụ thể:**

| Đối chiếu | Decode | Unity | Tỷ lệ |
|---|---|---|---|
| Formula (`Formulas.java`) | **20** method | **6** method + 1 stub rỗng | **30%** |
| Save state (`Data.java`) | **140** field | **17** field | **12%** |
| Combat/Dungeon engine (`Area.java`) | **3164** dòng | `CombatService` 139 + `DungeonService` 202 = **341** dòng | **~11%** |
| Quest (`QuestsManager.java`) | **468** dòng | `QuestService` **87** dòng | **19%** |
| Service được khởi tạo runtime | — | **3/14** | **21%** |
| Service có test | — | **4/14** | **29%** |
| Player action làm đổi được game state | — | **0** | **0%** |

### Tỷ lệ coverage ước lượng theo nhóm

| Nhóm | Coverage | Căn cứ |
|---|---|---|
| **Data** | **~85%** | 1531 record / 10 category load thật OK. Thiếu: doctrine JSON, localization (rỗng), assets_manifest (rỗng) |
| **Backend logic** | **~15%** | 17 backend gap trong đó 5 Critical. `Area.java` (nặng nhất) mới ~11%. Damage formula = stub rỗng |
| **Runtime integration** | **~21%** | 3/14 service wired. Không có dungeon-run orchestrator |
| **UI** | **~35%** | 8 panel tồn tại + framework tốt, nhưng 4 panel là vỏ rỗng, 2 panel read-only, thiếu QuestScreen |
| **Player-facing** | **~10%** | 3 chức năng thật (mở game, Back, save on quit) + 4 read-only. **0 hành động mutate được game state** |

**Không dám nói con số nào là 100%** — mọi tỷ lệ trên đều dẫn được về số dòng/số field/số method đếm trực tiếp.

## Coverage Matrix

| Domain | Decode Exists | Unity Data | Unity Backend | Runtime Wired | UI Exposed | Player Usable | Coverage |
|---|---|---|---|---|---|---|---|
| Data loading / GameDatabase | ✅ | ✅ 1531 | ✅ | ✅ | — | — | **FULL** |
| UI Framework (UIService/UIScreen) | ✅ | — | ✅ | ✅ | ✅ | ✅ | **FULL** |
| HUD (currency + navigation) | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ read-only | **FULL** (phạm vi hiện có) |
| Save I/O | ✅ | — | ✅ ghi file thật | ✅ | ⚠️ | ✅ | **PARTIAL** (17/140 field) |
| Formulas | ✅ 20 | — | ⚠️ 6/20 | ✅ | ❌ | ❌ | **PARTIAL** |
| Item | ✅ | ✅ 607 | ✅ | ✅ | ⚠️ text | ⚠️ | **PARTIAL** |
| Inventory | ✅ | ✅ | ✅ | ✅ | ⚠️ **read-only** | ⚠️ | **PARTIAL** |
| Character / Adventurer | ✅ | ✅ 129 | ⚠️ stat multiplier SAI | ✅ | ⚠️ **read-only** | ⚠️ | **PARTIAL + MANUAL_RULE_REQUIRED** |
| Equipment | ✅ | ✅ | ⚠️ 3 TODO | ❌ | ❌ | ❌ | **BACKEND_ONLY** |
| Enemy | ✅ (1 class/enemy) | ✅ 122 | ⚠️ chỉ factory | ❌ | ❌ | ❌ | **BACKEND_ONLY** |
| Skill | ✅ | ✅ 227 | ⚠️ 18 dòng | ❌ | ❌ | ❌ | **BACKEND_ONLY** |
| StatusEffect | ✅ | ✅ 25 | ⚠️ thiếu resolve loop | ❌ | ❌ | ❌ | **BACKEND_ONLY** |
| **Dungeon** | ✅ 3164 dòng | ✅ 11 | ⚠️ 202 dòng | ❌ | ❌ **panel trắng** | ❌ | **UI_PLACEHOLDER_ONLY** |
| **Combat** | ✅ | ✅ | ⚠️ 139 dòng, damage = stub | ❌ | ❌ | ❌ | **PARTIAL + MANUAL_RULE_REQUIRED** |
| Loot / Drop | ✅ | ⚠️ | ⚠️ 79 dòng | ❌ | ❌ | ❌ | **BACKEND_ONLY** |
| Quest | ✅ 468 dòng | ✅ 56 | ⚠️ 87 dòng | ❌ | ❌ **không có screen** | ❌ | **BACKEND_ONLY** |
| **Craft** | ✅ | ✅ 321 | ⚠️ trả `ManualRuleRequired` | ❌ | ❌ **panel trắng** | ❌ | **UI_PLACEHOLDER_ONLY** |
| **Merchant** | ✅ | ⚠️ | ⚠️ trả `Deferred...Rule` | ❌ | ❌ **panel trắng** | ❌ | **UI_PLACEHOLDER_ONLY** |
| **Settings** | ✅ 9 setting | ❌ | ❌ | ❌ | ❌ **panel trắng** | ❌ | **UI_PLACEHOLDER_ONLY** |
| Offline progress | ✅ | — | ⚠️ thiếu `lastAccess` | ❌ | ❌ | ❌ | **BACKEND_ONLY** |
| Pet | ✅ 290 dòng | ✅ 21 | ❌ | ❌ | ❌ | ❌ | **DATA_ONLY** |
| Raid | ✅ | ✅ 12 | ❌ | ❌ | ❌ | ❌ | **DATA_ONLY** |
| **Doctrine** | ✅ 8 instance | ❌ | ❌ | ❌ | ❌ | ❌ | **NOT_PORTED** |
| **Tavern / Quarters / Shelter** | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | **NOT_PORTED** |
| Achievements / IAP | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ | **NOT_PORTED** |
| Localization | ✅ (`R.string`) | ❌ rỗng | ⚠️ có `GetText` | ❌ | ❌ | ❌ | **BACKEND_ONLY** |
| Popup / Dialog | ✅ | — | ✅ | ✅ | ⚠️ **0 caller** | ❌ | **BACKEND_ONLY** |

## Critical Findings

1. **Placeholder panel KHÔNG đồng nghĩa chức năng xong** — 4 màn `Dungeon`/`Craft`/`Merchant`/`Settings` chỉ là `Image` + `Title` + `Message` + `Btn_Back`, **0% gameplay** phía sau. Đúng như user nhấn mạnh: không được tính là DONE.

2. **`FormulaService.CalculateDamage_ManualPortRequired()` là stub rỗng** — công thức damage **chưa port một dòng nào**. Đây là rào chặn cứng cho toàn bộ combat. Rule thật nằm ở `Area.dealDamage()` (dòng 2213), **đọc được**, chưa port.

3. **`CharacterService.GetTotalStat()` hardcode `levelMultiplier = 1.0f`** — chỉ số nhân vật **hiện đang SAI** so với decode. Nguy hiểm vì code chạy không lỗi nên dễ bị hiểu nhầm là đã xong.

4. **11/14 service chưa được khởi tạo runtime** — code có, compile sạch, nhưng chưa từng `new` lên ngoài test. Riêng 10 service **chưa có test nào**.

5. **Không tồn tại dungeon-run orchestrator** — không có lớp nào tương đương `Area.tick()` để điều phối một lượt chạy dungeon.

6. **Chuỗi gameplay bị chặn ngay từ khâu đầu:** không có Tavern → không có adventurer → Character rỗng → không lập party → không combat → không loot → Inventory rỗng → Money/Gems = 0 → `save.json` luôn toàn giá trị mặc định. **Đây là lý do save file chỉ có số 0 — đúng, không fake, nhưng chứng minh chưa có vòng gameplay nào chạy.**

7. **Doctrine system hoàn toàn vắng mặt** (`grep -ri "doctrine"` trên Unity → **0 kết quả**), dù decode có 8 doctrine + 16 save field + damage modifier + ảnh hưởng quest.

8. **Save schema thiếu `lastAccess`** → offline progress về mặt kỹ thuật **chưa thể hoạt động**.

9. **Chưa test được save mutation** — chỉ chứng minh "ghi file hoạt động", **chưa chứng minh "ghi đúng nội dung khi state đổi"**, vì chưa có action nào đổi state.

10. **Android StreamingAssets (P-01/G-P01)** — `StreamingAssetsGameDataProvider` throw `NotSupportedException` trên Android → **APK sẽ không load được data nào**. Không ảnh hưởng Editor/Standalone.

11. **Thiếu asset KHÔNG phải nguyên nhân** — chỉ **5/43 gap** là asset-only, và **không gap nào trong đó chặn chức năng**. 28/43 gap là chức năng thật.

## What Is Actually Done

Những thứ **chắc chắn** đã xong, có bằng chứng verify:

- ✅ **Data pipeline**: 1531 record / 10 category load thật, count khớp 100% manifest, có `DatabaseTests` (5 test) + smoke test tự động 8/8 pass
- ✅ **Boot → Main flow**: `Boot.unity` → `BootSceneLoader` → `Main.unity`, user Play test xác nhận
- ✅ **UI framework**: `UIService` (Register/Show/Hide/Back/Popup stack) + `UIScreen` + `SafeArea` hoạt động, `Wired 8 screen(s)` trong log
- ✅ **EventSystem**: `InputSystemUIInputModule` đúng, click/navigation hoạt động
- ✅ **HUD**: hiện money/gems từ `SaveService` thật, 6 nav button điều hướng hoạt động
- ✅ **Save I/O**: `save.json` 942 bytes ghi thật, JSON hợp lệ, đúng schema, có backup file, trigger `OnApplicationQuit`/`OnApplicationPause`
- ✅ **Inventory/Character đọc qua đúng tầng service** (không đọc raw save data)
- ✅ **Scene editable**: 66/66 object active, user tự chỉnh UI offline được
- ✅ **Import settings S5**: 416/416 sprite giữ Point / MipMap Off / Compression None
- ✅ **Compile sạch**: 0 `error CS`, 0 exception
- ✅ **Không có dữ liệu giả nào** trong toàn bộ dự án

## What Is Not Done Yet

- ❌ **Công thức damage** — stub rỗng
- ❌ **Level stat multiplier** — hardcode sai
- ❌ **14/20 formula** chưa port
- ❌ **~123/140 save field** chưa port (bao gồm `lastAccess` chặn offline, 9 setting, 8 doctrine)
- ❌ **11/14 service chưa wire runtime**
- ❌ **Dungeon run orchestrator** — không tồn tại
- ❌ **Combat engine** — thiếu dodge/retaliate/heal/cast/resolveStatus/selectTargets(7 strategy)/decideTurnsOrder/reanimate/healingNova/pet actions
- ❌ **Tavern/Quarters/Shelter** — chưa port (nút thắt chặn cả chuỗi)
- ❌ **Doctrine** — chưa port gì
- ❌ **Pet** — chưa có service (data có 21 pet)
- ❌ **Raid** — chưa có service (data có 12 raid)
- ❌ **Quest UI** — không có screen
- ❌ **Inventory actions** — read-only, không equip/use/sell
- ❌ **Character stat/equipment/skill UI** — read-only text
- ❌ **Craft/Merchant/Settings function** — panel trắng
- ❌ **Offline progress** — chưa hoạt động được
- ❌ **Save mutation test** — chưa test được
- ❌ **Achievements/IAP** — chưa port (cần xác nhận scope)
- ❌ **Android data loading** — sẽ fail trên APK
- ❌ **Standalone build** — chưa từng thử

## Recommended Next Step

# `S6.5A_Function_Completion_With_Placeholder_Assets`

Kế hoạch chi tiết 13 task: `S6_5A_Function_Completion_Plan.md`

**Bắt đầu bằng `S6.5A-001 Rule Extraction`** — bóc chính xác 5 rule đang thiếu (damage formula, level stat multiplier, craft timer/claim, merchant price/restock, doctrine) từ decode. Task này **chỉ đọc và viết tài liệu, không code**, nhưng 6 task sau phụ thuộc vào nó.

**Không bắt đầu S6.5B (asset/visual/Higgsfield) trước khi S6.5A xong** — làm đẹp UI trước sẽ phải dựng lại khi thêm chức năng.

**Cần user quyết định 2 điểm scope:**
1. **Raid** (12 record data đã có) — có thuộc scope rebuild không?
2. **Achievements / IAP** — có thuộc scope rebuild không?

## Final Decision

# `S6_5A_AUDIT_FOUND_CRITICAL_MISSING_LOGIC`

**Lý do:** Audit hoàn tất đầy đủ với bằng chứng định lượng cho từng domain (đếm dòng/field/method trực tiếp từ cả decode và Unity, không suy đoán). Kết quả cho thấy **9 gap mức Critical**, trong đó có 3 gap chặn cứng toàn bộ gameplay: công thức damage là stub rỗng (**G-B02**), level stat multiplier hardcode sai (**G-B03**), và Tavern chưa port khiến không thể có adventurer nào (**G-B14**). Ngoài ra 11/14 service chưa khởi tạo runtime và không tồn tại dungeon-run orchestrator. Vì vậy **không thể kết luận "đã lấy đủ logic từ decode"** — chọn `AUDIT_FOUND_CRITICAL_MISSING_LOGIC` thay vì `DONE_READY_FOR_FUNCTION_COMPLETION`, để phản ánh trung thực rằng khối lượng còn lại là đáng kể chứ không phải vài chỗ nhỏ. Không có scope violation, không sửa code/data/scene/gameplay nào trong phase audit này.
