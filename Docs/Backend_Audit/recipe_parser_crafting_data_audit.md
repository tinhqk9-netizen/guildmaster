# 🧠 Báo cáo Audit: Recipe Parser & Crafting Data Pipeline

**Dự án**: D:\Tinh\Rebuild_GuildMaster
**Ngày Audit**: 2026-08-06

---

## 1. Recipe Output Mapping
Phát hiện lỗi format ID hàng loạt khi parse từ source Java sang JSON.

### 🔴 Thống kê
- **Tổng số Recipe:** 321
- **Output khớp ItemDefinition (Valid):** 12 recipe (Vd: `cloth`, `glass`, `cloth_robe`).
- **Output bị Orphan (Sai ID):** 309 recipe (96%).

### 🔎 Root Cause
Trình import (`legacy_import_tool`) parse tên class Java dạng PascalCase nhưng chỉ chuyển thành **lowercase viết liền** thay vì **snake_case**. 
- VD ở Java: `AbsoluteZero` → Json Output: `absolutezero`. 
- Nhưng trong `items.json` nó lại là `absolute_zero`. 
- Kết quả: `RecipeDefinition` trỏ tới một Item ID không tồn tại.

---

## 2. Ingredient Parsing
Trình parse regex của recipe hoàn toàn sụp đổ khi gặp công thức có từ 2 nguyên liệu trở lên.

### 🔴 Thống kê
- **Recipe rỗng nguyên liệu (0 Ingredients):** 243 / 321 recipe.
- **Recipe có nguyên liệu:** 78 / 321 recipe.
- **Ingredient bị Orphan:** 50 ID nguyên liệu không tồn tại.

### 🔎 Root Cause
- **Lỗi tách chuỗi (Comma Split Bug):** Trình parse cắt chuỗi argument theo dấu phẩy (`,`) NHƯNG không bỏ qua dấu phẩy nằm trong ngoặc kép hoặc hàm.
  - VD: `Item.getInstance("IceCage", 1), Item.getInstance("FlakeOfInfinity", 8)` bị cắt thành `Item.getInstance("IceCage", 1` → Lỗi cú pháp → Trả về mảng nguyên liệu rỗng `[]`.
- **Lỗi PascalCase:** Giống hệt lỗi Output, các nguyên liệu như `AncientMembrane` bị parse thành `ancientmembrane` thay vì `ancient_membrane`, tạo ra 50 ingredient ID mồ côi.

### 📋 Mẫu đối chiếu (Java → JSON)
1. `Cloth(Item.getInstance("PlantFiber", 4))` → Output: `cloth` (Valid) / Ingredient: `plantfiber` (Orphan).
2. `AbsoluteZero(Item.getInstance("IceCage", 1), Item.getInstance("FlakeOfInfinity", 8))` → Output: `absolutezero` (Orphan) / Ingredient: `[]` (Empty do dấu phẩy).
3. `AbyssalCompendium(Item.getInstance("MissingPage", 50))` → Output: `abyssalcompendium` (Orphan) / Ingredient: `missingpage` (Orphan).
*(Lỗi lặp lại tương tự trên 300 mẫu khác)*

---

## 3. Data Model và Loader
- `RecipeDefinition.cs` chỉ chứa đúng 2 biến `OutputItemId` và `Ingredients`.
- Các trường siêu dữ liệu trong `recipes.json` như `parseStatus`, `rawArgs`, `manualRuleRequired` hoàn toàn bị `JsonUtility` bỏ qua lúc load vào C# do không có property tương ứng. 
- **Kết luận:** Dữ liệu JSON không bị mất do Loader, mà các trường bị bỏ qua chỉ là metadata không ảnh hưởng runtime.

---

## 4. Crafting Reference Integrity
Tính toàn vẹn dữ liệu gần như bằng không do kết hợp cả lỗi Output ID và Ingredient ID.

### 🔴 Thống kê Tàn Khốc
- Tổng số Recipe có thể craft thành công trong game: **Đúng 2 Recipe** (`cloth_robe` và `glass`).
- 319 recipe còn lại hoàn toàn không thể sử dụng do thiếu nguyên liệu hoặc ID không hợp lệ.
- **Output Quantity:** JSON không có trường số lượng output, nhưng điều này LÀ ĐÚNG so với source gốc. Trong Java, hàm `Item.getInstance(name())` luôn mặc định trả về số lượng 1.

---

## 5. CraftService Data Consumption & Rủi ro Logic
Dù data có đúng, `CraftService.cs` vẫn đang chứa 3 lỗi logic đặc biệt nghiêm trọng có thể phá hỏng file Save.

### 🔴 Rủi ro 1: Kẹt hàng vĩnh viễn (Deadlock Queue)
- **Flow:** Khi bấm craft một món đồ lỗi (VD: `absolutezero`), game vẫn ghi nó vào hàng đợi `ItemActionSaveData` với `DefinitionId = "absolutezero"`. 
- **Bug:** Lúc đồ craft xong, hàm `ClaimCompletedCraft` sẽ tìm `absolutezero` trong Database. Vì tìm không thấy (do thiếu dấu gạch dưới), nó `return false;`. Món đồ này sẽ **NẰM VĨNH VIỄN** trong danh sách `CompletedWorkshopItems`, không thể nhận cũng không thể xóa, gây kẹt UI và hỏng Save.

### 🔴 Rủi ro 2: Nuốt trang bị (IsLocked Ignored)
- Giống như lỗi ở Market, `TryStartCraft` gọi hàm `ConsumeByDefinitionId`. Hàm này quét và tiêu hủy nguyên liệu nhưng **không kiểm tra `IsLocked`**, dẫn đến việc người chơi có thể lấy trang bị đang mặc trên người nhân vật (hoặc đồ quý đã khóa) ra làm nguyên liệu craft.

### 🔴 Rủi ro 3: Transaction không nguyên tử (Atomic Failure)
- Nếu recipe yêu cầu 2 nguyên liệu. Nguyên liệu 1 tiêu thụ thành công, nhưng nguyên liệu 2 bị lỗi (do lệch đồng bộ hoặc bug). Hàm sẽ báo `CraftFailureReason.MissingIngredients` và thoát. Tuy nhiên, nó **KHÔNG HOÀN TRẢ** nguyên liệu 1 đã bị trừ trước đó (Data Loss).

---