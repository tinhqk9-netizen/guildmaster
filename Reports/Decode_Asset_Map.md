# Bản Đồ Asset (Asset Map)

*Bản đồ này loại trừ các asset hệ thống của Android/Google Play. Tập trung vào tài nguyên đồ họa Game-specific trong thư mục `drawable` và `mipmap`.*

| Phân loại Asset | Định dạng tên file gốc (Regex/Pattern) | Mục đích sử dụng | Xử lý trong Unity |
|---|---|---|---|
| **App Icons** | `mipmap/ic_launcher*.png` | Icon ngoài màn hình chính của thiết bị. | Đưa vào Project Settings -> Player -> Icon. |
| **Item Icons** | Tên tiếng anh viết thường (vd: `abyssal_cutlass.png`, `apple.png`) | Icon hiển thị trong túi đồ, chợ, phần thưởng. | Đóng gói thành Sprite Atlas (ItemAtlas). |
| **Adventurer Icons** | `unit_*.png` (vd: `unit_archer.png`) | Chân dung anh hùng. | Sprite Atlas (CharacterAtlas). |
| **Enemy Icons** | `unit_*.png` (Dùng chung tiền tố với adventurer) | Chân dung quái vật, Boss. | Sprite Atlas (CharacterAtlas). |
| **Dungeon/Raid Images** | `area_*.png` (vd: `area_the_desert.png`) | Background hoặc banner của khu vực thám hiểm. | Đưa vào thư mục Art/Areas (Textures riêng lẻ). |
| **Status/Skill Icons** | `icon_effect_*.png`, `doctrine_ability_*.png` | Icon buff/debuff và kỹ năng. | Sprite Atlas (UIAttributeAtlas). |
| **Currency Icons** | Không có tiền tố chung (vd: `coin.png`, `gem.png`) | Hiển thị tiền tệ. | Sprite Atlas (UIAttributeAtlas). |
| **UI Backgrounds & Buttons** | `button_*.xml`, `bg_*.xml`, hoặc PNG viền | Đồ họa giao diện, khung gỗ, nút bấm. | Cắt 9-slice cho UGUI (Image). (Lưu ý: Các file XML Shape của Android phải được vẽ lại bằng file ảnh PNG/SVG hoặc dùng Unity UI Shape). |

**Ghi chú:** Có tổng cộng khoảng 1035 file PNG trong thư mục drawable. Do không có animation frame-by-frame phức tạp (game idle text-based), việc import sang Unity rất đơn giản: Đổi toàn bộ Texture Type sang `Sprite (2D and UI)` và dùng tính năng Sprite Atlas để tối ưu draw call.
