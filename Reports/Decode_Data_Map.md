# Bản Đồ Dữ Liệu (Data Map)

| Java class | Vai trò | Dữ liệu chính | Unity class đề xuất | Ghi chú |
|---|---|---|---|---|
| `Data.java` | Runtime State & Save Data Root | Tiền vàng, danh sách Adventurers đang sở hữu, Inventory, Dungeons đã mở, Quests đang làm... | `GameData` (POCO) | Gốc của cây dữ liệu được serialize bằng JSON. |
| `SaveManager.java` | System | Auto-save timer | `SaveSystem` (MonoBehaviour hoặc C# Task) | Quản lý vòng đời lưu dữ liệu. |
| `FileManager.java` | System | GSON Serializer/Deserializer | `FileHandler` (C# Static) | Dùng Newtonsoft.Json thay cho Gson. |
| `Formulas.java` | Formula | Công thức tính sát thương, kinh nghiệm, tốc độ đào tạo, drop rate. | `Formulas` (Static Class) | Chứa toàn bộ logic toán học cốt lõi. Cần port cẩn thận. |
| `Utils.java` | Helper | Các hàm tiện ích (random, format số). | `Utils` (Static Class) | Dễ dàng chuyển sang C#. |
| `Entity.java` | Definition (Base) | HP, MP, Attributes (STR, AGI, INT), Level. Các logic gây sát thương (Damage/Heal). | `EntityData` (POCO/Model) + `EntityController` | Là class cha của `Adventurer` và `Enemy`. |
| `Adventurer.java` | Runtime State | Kinh nghiệm, Trang bị (Vũ khí, Giáp), Nhánh kỹ năng. | `AdventurerData` (POCO) | Được lưu trong `Data.java`. Cần tách phần definition tĩnh ra ScriptableObject. |
| `Item.java` | Definition & Reference | Tên (ID file), số lượng, R.string (tên hiển thị), R.drawable (icon). | `ItemDefinition` (ScriptableObject) + `ItemInstance` (POCO) | Trong Java, `Item` vừa đóng vai trò định nghĩa vừa bọc số lượng qua `.getInstance(name, quantity)`. Unity nên bóc tách rõ. |
| `Area.java` | Definition | Drop tables (Danh sách rơi đồ), Requirements (Điều kiện cấp độ/vật phẩm để vào). R.string, R.drawable. | `AreaDefinition` (ScriptableObject) | Class cha của Dungeons và Raids. |
| `Quest.java` | Runtime & Definition | Điều kiện hoàn thành, tiến độ, phần thưởng (Reward list). | `QuestDefinition` (ScriptableObject) + `QuestState` (POCO) | Unity cần tách tiến độ (lưu Save) và nội dung Quest (ScriptableObject). |
| `Pet.java` | Runtime & Definition | Buff stats, Icon, Tên. | `PetDefinition` (SO) + `PetInstance` | Tương tự Item. |
| `Recipes.java` | Definition tĩnh | Danh sách nguyên liệu cần thiết (List of Items) và Item kết quả. | `RecipeDefinition` (ScriptableObject) | Đang được thiết kế dạng Java Enum, dễ dàng chuyển sang SO array. |

**Đặc điểm phân loại cấu trúc mã nguồn gốc:**
- **IDs:** Thường được truyền qua biến String (tên Class) hoặc thông qua tên của file Class (ví dụ `Item.getInstance("CopperSword")`).
- **R.string / R.drawable:** Được map cứng vào các thuộc tính của object (thường ở constructor của class con).
- **List / Drop Tables:** Thường sử dụng `ArrayList`, `LinkedHashMap` để chứa tỷ lệ rơi đồ hoặc thành phần chế tạo. Được khai báo ở cấp class.
