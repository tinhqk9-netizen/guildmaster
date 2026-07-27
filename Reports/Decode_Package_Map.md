# Bản Đồ Package (Decode Package Map)

Bảng dưới đây ánh xạ cấu trúc thư mục của mã nguồn Java sang các module hệ thống Unity tương ứng để phục vụ quá trình chuyển ngữ.

| Package/Folder | Mục đích | File chính | Unity module tương ứng | Ưu tiên |
|---|---|---|---|---|
| `core` / `app` (Gốc) | Core Loop, Setup, Utils | `MainActivity.java`, `Formulas.java`, `Utils.java` | `Scripts/Core` | Cao |
| `storage/save` | I/O, Serialization, Auto-save | `SaveManager.java`, `FileManager.java` | `Scripts/Save` | Cao |
| `storage/data` | Chứa dữ liệu gốc (State) | `Data.java` | `Scripts/Data/Runtime` | Cao |
| `storage/data/items` | Item Definitions & Recipes | `Item.java`, `Recipes.java`, thư mục `instances` | `Scripts/Equipment` & `Scripts/Inventory` | Cao |
| `storage/data/entities/adventurers`| Dữ liệu anh hùng | `Adventurer.java`, thư mục `units`, `doctrines` | `Scripts/Characters` | Cao |
| `storage/data/entities/enemies`| Dữ liệu quái vật, Boss | thư mục `units` | `Scripts/Enemies` | Cao |
| `storage/data/entities`| Base Entities, Combat logic liên quan | `Entity.java`, `Skills.java`, `StatusEffectType.java` | `Scripts/Combat` | Cao |
| `storage/data/places/dungeons`| Điểm thám hiểm cơ bản | Thư mục `dungeons` | `Scripts/Areas` | Trung bình |
| `storage/data/places/raids`| Phó bản khó | Thư mục `raids` | `Scripts/Areas` | Trung bình |
| `storage/data/quests` | Logic nhiệm vụ | `Quest.java`, thư mục `instances` | `Scripts/Quests` | Thấp |
| `storage/data/pets` | Thú cưng đi kèm | `Pet.java`, thư mục `instances` | `Scripts/Pets` | Thấp |
| `ui` | Quản lý màn hình chính | `MainActivity.java` (một phần), các class View | `Scripts/UI` | Cao |
| `ui/dialogs` | Các popup con (Settings, Item Detail)| `DialogItem.java`, `DialogMerchant.java` | `Scripts/UI/Dialogs` | Trung bình |
| `ui/components` | UI Helpers, Custom Views | Các custom adapter | `Scripts/UI/Components` | Thấp |
| `Ads/IAP/Cloud` | Không tách rõ thành package độc lập, bị gắn vào MainActivity | `MainActivity.java` (billing client, play games) | Tích hợp thư viện Unity (IAP, Google Play) sau | Thấp (Defer) |
