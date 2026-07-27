# Bản Đồ UI (UI Map)

*Bản đồ này loại trừ các XML sinh ra tự động bởi thư viện Android (như `abc_*`, `design_*`, `material_*`, `m3_*`). Chỉ tập trung vào giao diện thực tế của game.*

| XML layout | Màn hình/chức năng | Unity View/Prefab đề xuất | Ưu tiên MVP |
|---|---|---|---|
| `activity_main.xml` | Khung sườn chính của App (chứa Navigation) | `MainScreenView` (Canvas root) | Cao |
| `fragment_headquarters.xml` | Màn hình Thành chính (Tavern, Quarters, Workshop) | `HeadquartersView` | Cao |
| `fragment_adventurers.xml` | Màn hình Quản lý đội hình | `RosterView` | Cao |
| `fragment_dungeons.xml` | Màn hình chọn Ngục tối | `DungeonSelectionView` | Cao |
| `fragment_raids.xml` | Màn hình chọn Phó bản khó | `RaidSelectionView` | Thấp |
| `dialog_settings.xml` | Cài đặt game | `SettingsDialog` | Cao |
| `dialog_shop.xml` | Cửa hàng IAP | `ShopDialog` | Thấp |
| `dialog_market.xml` / `dialog_merchant.xml` | Chợ đen / Thương gia | `MarketDialog` | Trung bình |
| `dialog_craft.xml` / `dialog_recipes.xml` | Chế tạo trang bị | `CraftingDialog` | Cao |
| `dialog_quests.xml` | Danh sách nhiệm vụ | `QuestDialog` | Trung bình |
| `dialog_item_detail.xml` | Chi tiết vật phẩm | `ItemDetailPopup` | Cao |
| `dialog_entity_detail.xml` | Chi tiết nhân vật / quái vật | `EntityDetailPopup` | Cao |
| `dialog_idle_progress.xml` | Tổng kết offline progress | `OfflineReportDialog` | Cao |
| `layout_adventurer.xml` | Card hiển thị 1 nhân vật trong list | `AdventurerCard` (Prefab) | Cao |
| `layout_item.xml` | Ô hiển thị 1 vật phẩm | `ItemSlot` (Prefab) | Cao |
| `layout_dungeon.xml` | Card hiển thị 1 hầm ngục | `DungeonCard` (Prefab) | Cao |
| `layout_quest.xml` | Dòng hiển thị 1 quest | `QuestEntry` (Prefab) | Trung bình |
| `layout_entity_fighting.xml` | Thanh máu/trạng thái trong combat log | `CombatEntityBar` (Prefab) | Cao |

**Lưu ý:** Trong Unity, thay vì dùng Fragment như Android, chúng ta sẽ dùng kiến trúc **UI Panels** chuyển đổi lẫn nhau trên cùng một Canvas, hoặc các **Prefabs** được instantiate (tạo ra) vào các ScrollRect.
