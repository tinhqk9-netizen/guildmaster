# Phạm Vi MVP Đề Xuất (Minimum Viable Product)

Để đảm bảo việc chuyển ngữ từ Java sang Unity thành công mà không bị ngộp bởi lượng dữ liệu khổng lồ (hơn 600 items, 100+ enemies), MVP (Phiên bản khả dụng tối thiểu) của dự án sẽ bị giới hạn nghiêm ngặt ở những tính năng cốt lõi nhất.

## 1. Giới hạn Content (Content Scope)
- **Số Item (Vật phẩm/Trang bị):** 10-15 items cơ bản (ví dụ: Gỗ, Vải, Kiếm gỗ, Giáp da, Bình máu nhỏ).
- **Số Adventurer (Nhân vật):** 2-3 class cơ bản (Fighter, Archer, Mage). Không sử dụng hệ thống Doctrine nâng cao ở MVP.
- **Số Enemy (Quái vật):** 3-5 loại (Slime, Goblin, Wolf).
- **Số Dungeon (Khu vực):** 1 dungeon duy nhất (Ví dụ: The Green Forest) với 5-10 tầng cơ bản.
- **Số Quest (Nhiệm vụ):** 2-3 nhiệm vụ hướng dẫn cơ bản (Giết quái, Nhặt đồ).
- **Số Pet:** 0 (Cắt bỏ hoàn toàn khỏi MVP).

## 2. Những hệ thống BẮT BUỘC CÓ (Must-have)
- **Boot & Data Load:** Hệ thống khởi tạo Game, nạp ScriptableObjects vào bộ nhớ.
- **Save/Load:** Serialize trạng thái người chơi thành JSON và lưu xuống thiết bị. Load lên khi mở game.
- **Inventory System:** Thêm/bớt/đếm số lượng item. Giới hạn ô chứa.
- **Adventurer System:** Quản lý danh sách anh hùng, trang bị đồ (Equip/Unequip), tăng kinh nghiệm/lên cấp.
- **Dungeon & Combat (Auto):** Hệ thống gửi anh hùng vào hầm ngục, tự động mô phỏng đánh quái (trên logic, update lên UI) và nhặt đồ (Reward).
- **Offline Progress:** Tính toán lượng vàng/kinh nghiệm/đồ nhặt được khi người chơi tắt game và mở lại dựa trên hiệu suất của đội hình đang ở trong hầm ngục.
- **UI Navigation:** Chuyển đổi mượt mà giữa các màn hình chính (Thành phố, Đội hình, Hầm ngục) bằng UGUI.

## 3. Những hệ thống HOÃN LẠI (Deferred)
*Các hệ thống này sẽ không được code trong giai đoạn MVP, mà để dành cho các phase mở rộng sau này:*
- Chợ đen (Market/Merchant).
- Quán rượu (Tavern) và hệ thống quay tướng ngẫu nhiên (chỉ tặng tướng mặc định ở MVP).
- Phó bản khó (Raids) và Boss thế giới.
- Chế tạo trang bị nâng cao (Workshop/Recipes) - có thể chỉ làm mock-up.
- Quảng cáo (Ads), IAP và Cloud Save.
- Thú cưng (Pets).
- Hệ thống hiệu ứng trạng thái (Status Effects) phức tạp (Stun, Poison) - Combat ban đầu chỉ tập trung sát thương vật lý thuần túy.
