# BÁO CÁO NGHIỆM THU DỰ ÁN GUILD MASTER (ACCEPTANCE REPORT)
**Mã Báo Cáo:** `GuildMaster_Acceptance_20260729`  
**Ngày Thực Hiện:** 29/07/2026  
**Trạng Thái:** CORRECTIVE PASS IMPLEMENTED & VERIFIED  

---

## I. TỔNG QUAN HỆ THỐNG & KẾT QUẢ KIỂM THỬ TĨNH (SMOKE TESTS)
Toàn bộ dự án đã biên dịch thành công 100% trên môi trường Unity 6000.3.17f1. Unity Log xác nhận không có bất kỳ compile error nào. Hai bộ kiểm thử tự động chính:
- **Asset Database Verification:** `Output written to verify-result.txt` -> **PASS**
- **Editor Smoke Test Runner:** `Smoke Test Completed Successfully.` -> **PASS**

---

## II. CHI TIẾT CORRECTIVE CHANGES (CÁC SỬA ĐỔI CHÍNH)

### 1. Khắc Phục Lỗi HP = 0 của Nhân Vật Mới Tạo (Fresh-save HP issue)
*   **Vấn đề:** Khi tạo game mới hoặc load save thô, các Doctrine Level của Guild (như Fortitude, Ruin) bằng 0, dẫn đến công thức tính HP quy ra kết quả rất bé hoặc 0, khiến nhân vật Footman mặc định bị coi là đã chết.
*   **Giải pháp an toàn:**
    - Khôi phục `SaveData.CreateDefault()` về trạng thái sạch sẽ ban đầu, không còn hardcode `WarLevel = 1`, `FortitudeLevel = 1`... (để các chỉ số này về 0 mặc định).
    - Thêm flag `public bool IsHpInitialized;` trong `CharacterSaveData` nhằm tương thích ngược và phân biệt giữa nhân vật mới tạo (chưa gán HP) và nhân vật đã thực sự chết trong Dungeon (HP = 0).
    - Cập nhật `CharacterService.LoadFromSave()` và `RecruitCharacter()`: 
      - Nếu `IsHpInitialized == false`, hệ thống tự động tính toán chỉ số `MaxHp` thực tế bằng `GetTotalStat(charRuntime, StatType.MaxHp)` và gán `CurrentHp = MaxHp`.
      - Đặt `IsHpInitialized = true` để lưu lại trạng thái này.
    - Cập nhật `CharacterService.CreateCharacter()` để gán đầy HP tương ứng `MaxHp` thực tế ngay khi tạo mới thay vì 100 HP tượng trưng.

### 2. Nâng Cấp Hợp Đồng và An Toàn Pet System (Pet integration contract)
*   **Vấn đề:** Các hàm tính chỉ số bonus từ Pet trong `PetService.cs` gọi hàm `_database.GetRequired<PetDefinition>()` trực tiếp. Nếu save data của người chơi chứa Pet ID không tồn tại hoặc lỗi timing/partial-init, game sẽ crash tức thì với lỗi `KeyNotFoundException`.
*   **Giải pháp phòng vệ:**
    - Overwrite `PetService.cs` thay thế toàn bộ lệnh `GetRequired<T>` thành `_database.TryGet<PetDefinition>(..., out var def)`.
    - Thêm các kiểm tra null-guard an toàn trên `_saveService.CurrentData` và `CurrentData.Pets` để tránh null pointer exception lúc khởi động.
    - Duy trì sự liên kết chỉ số Pet với Adventurer Stats (HP, Defense, Speed) và Combat (Attack) hoàn toàn độc lập và an toàn ở backend.

### 3. Phòng Vệ An Toàn Workshop Craft (Craft completed claim safety)
*   **Vấn đề:** Giao diện UI `CraftScreen.cs` khi claim item hoàn thành gọi `ClaimCompletedCraft(...)` một cách mù quáng (blind success) mà không kiểm tra kết quả `bool` trả về, dẫn đến việc UI báo thành công ngay cả khi hòm đồ đầy hoặc claim lỗi.
*   **Giải pháp:**
    - Thay thế logic `OnClickClaimSelected()` trong `CraftScreen.cs` để nhận và đánh giá kết quả trả về từ `ClaimCompletedCraft()`.
    - Trả về thông báo lỗi đỏ `"Failed to claim. Inventory might be full."` khi thất bại, giải phóng selected index và Refresh UI mượt mà khi thành công.
    - Đồng thời, tự động lưu game xuống đĩa cứng bằng cách đặt `_saveService.Save(out _)` ngay khi claim thành công trong `CraftService.cs`.

### 4. Tiêu Diệt Vòng Lặp Lỗi Doctrine Level (Doctrine Progression Loop Guard)
*   **Vấn đề:** Lượng điểm kinh nghiệm lớn được trao thưởng có thể thăng nhiều level liên tiếp. Tuy nhiên, nếu công thức stars cần thiết (`TotalStarsToNextLp`) trả về `<= 0` do đạt max cấp hoặc lỗi data, vòng lặp `while` sẽ chạy vô tận và treo Unity.
*   **Giải pháp:**
    - Thêm bảo vệ `starsNeeded <= 0` trong vòng lặp của `DoctrineService.AddProgress()`.
    - Ngăn chặn triệt để hiện tượng infinite loop treo Unity, tự động ngắt và bảo toàn state của người chơi khi đạt giới hạn.

---

## III. MÀN HÌNH KIỂM CHỨNG TỰ ĐỘNG: `GuildMasterAcceptanceWindow`
Chúng tôi đã xây dựng màn hình nghiệm nghiệm thu tự động tích hợp trực tiếp trong Unity Editor tại `Window -> Guild Master -> Acceptance Suite`. Màn hình này chạy 4 bài tests độc lập dưới dạng Edit-mode để xác thực logic:

1.  **Test 1: Database Typo & Staging Verification (Cloth Robe Sync)**
    - Tìm kiếm và xác thực không còn bất kỳ typo `"clothrobe"` hay `"recipe_clothrobe"` nào trong live data configs (`items.json`, `recipes.json`, `dungeons.json`) cũng như staging build.
2.  **Test 2: Doctrine Multi-level progression & Loop Guard Check**
    - Chạy thử nghiệm cộng progress Doctrine thăng cấp hàng loạt (cộng 20 sao đưa level từ 1 lên 3 dư 3 sao) và đưa input lỗi để xác nhận vòng lặp infinite loop được ngăn chặn thành công.
3.  **Test 3: Save Data Default Doctrine Levels Alignment Check**
    - Đảm bảo save data mặc định có cấp độ Doctrine là 0, không hardcode.
4.  **Test 4: Newly Created Adventurer HP Safety Check**
    - Xác nghiệm một nhân vật Footman mới được tạo với HP là 0 và flag `IsHpInitialized = false` thì sau khi khởi chạy sẽ tự động được hồi đầy HP khớp với MaxHP thực tế (100 HP) và gán `IsHpInitialized = true`.

---

## IV. BẢN CẮT GIẢM GIT DIFF THỰC TẾ (SELECTED DIFF EVIDENCE)

```diff
diff --git a/Assets/_Game/Scripts/Runtime/Save/SaveData.cs b/Assets/_Game/Scripts/Runtime/Save/SaveData.cs
--- a/Assets/_Game/Scripts/Runtime/Save/SaveData.cs
+++ b/Assets/_Game/Scripts/Runtime/Save/SaveData.cs
@@ -38,6 +38,7 @@
         public int Level;
         public int Exp;
         public float CurrentHp;
+        public bool IsHpInitialized;
 
         public string WeaponInstanceId;
 
@@ -287,16 +287,6 @@
             data.SettingsSound = true;
             data.SettingsMusic = true;
-            data.WarLevel = 1;
-            data.FortitudeLevel = 1;
-            data.RuinLevel = 1;
 
             data.NormalizeAfterLoad();
             data.Characters.Add(new CharacterSaveData { DefinitionId = "Footman", Level = 1, InstanceId = System.Guid.NewGuid().ToString() });

diff --git a/Assets/_Game/Scripts/Runtime/Services/CharacterService.cs b/Assets/_Game/Scripts/Runtime/Services/CharacterService.cs
--- a/Assets/_Game/Scripts/Runtime/Services/CharacterService.cs
+++ b/Assets/_Game/Scripts/Runtime/Services/CharacterService.cs
@@ -64,6 +63,18 @@
                         charRuntime.Armor = _inventoryService.GetItem(saveData.ArmorInstanceId);
                     if (!string.IsNullOrEmpty(saveData.AccessoryInstanceId))
                         charRuntime.Accessory = _inventoryService.GetItem(saveData.AccessoryInstanceId);
+
+                    if (!saveData.IsHpInitialized)
+                    {
+                        int maxHp = GetTotalStat(charRuntime, StatType.MaxHp);
+                        charRuntime.CurrentHp = maxHp;
+                        saveData.IsHpInitialized = true;
+                        saveData.CurrentHp = maxHp;
+                    }
+                    else
+                    {
+                        charRuntime.CurrentHp = (int)saveData.CurrentHp;
+                    }
 
                     _characters.Add(charRuntime);

diff --git a/Assets/_Game/Scripts/Runtime/UI/Craft/CraftScreen.cs b/Assets/_Game/Scripts/Runtime/UI/Craft/CraftScreen.cs
--- a/Assets/_Game/Scripts/Runtime/UI/Craft/CraftScreen.cs
+++ b/Assets/_Game/Scripts/Runtime/UI/Craft/CraftScreen.cs
@@ -262,8 +262,17 @@
             if (completed == null || completed.Count == 0) return;
             _selectedCompletedIndex = Mathf.Clamp(_selectedCompletedIndex, 0, completed.Count - 1);
             var item = completed[_selectedCompletedIndex];
-            _craftService.ClaimCompletedCraft(item.InstanceId);
-            ShowFeedback($"Claimed {item.DefinitionId} x{item.StackCount}.", true);
+            
+            bool success = _craftService.ClaimCompletedCraft(item.InstanceId);
+            if (success)
+            {
+                ShowFeedback($"Claimed {item.DefinitionId} x{item.StackCount}.", true);
+                _selectedCompletedIndex = 0;
+            }
+            else
+            {
+                ShowFeedback($"Failed to claim {item.DefinitionId}. Inventory might be full.", false);
+            }
             Refresh();

diff --git a/Assets/_Game/Scripts/Runtime/Services/DoctrineService.cs b/Assets/_Game/Scripts/Runtime/Services/DoctrineService.cs
--- a/Assets/_Game/Scripts/Runtime/Services/DoctrineService.cs
+++ b/Assets/_Game/Scripts/Runtime/Services/DoctrineService.cs
@@ -65,7 +64,20 @@
 
             while (remainingAmount > 0)
             {
-                int neededToNext = _formulaService.TotalStarsToNextLp(newLevel) - currentProgress;
+                int starsNeeded = _formulaService.TotalStarsToNextLp(newLevel);
+                if (starsNeeded <= 0)
+                {
+                    break;
+                }
+
+                int neededToNext = starsNeeded - currentProgress;
+                if (neededToNext <= 0)
+                {
+                    newLevel++;
+                    currentProgress = 0;
+                    continue;
+                }
```

---
*Báo cáo được chuẩn bị và kiểm chứng bởi Antigravity AI Engine.*
