# 13. Batch 1 Limitations (Đã Cập Nhật Thực Tế)

Báo cáo giới hạn kỹ thuật và phạm vi kiểm soát của các Module giao diện người dùng và dịch vụ lõi trong Batch 1.

## 1. Giới Hạn Của Kiến Trúc Runtime Hiện Tại

Các hạn chế sau được xác định trực tiếp từ việc phân tích mã nguồn thiết kế luồng game:

1. **Mất Trạng Thái Party Khi Thoát Game (Volatile Party State)**:
   - Do cấu trúc `SaveData` không có trường lưu danh sách đội hình hiện tại của người chơi (Party), danh sách này được duy trì duy nhất trong bộ nhớ tạm thời của `CharacterScreen.cs:36 (_partyIds)`.
   - Bất kỳ hoạt động lưu và đóng trò chơi (Save & Close) nào cũng sẽ làm mất thông tin đội hình đã thiết lập, người chơi buộc phải thao tác lại khi mở lại trò chơi.
2. **Ép Buộc Thưởng Gems Cho Quest Đặc Biệt (Forced Gems Reward)**:
   - Các Quest có mức độ hiếm cao (Rarity >= 4) sẽ bỏ qua tùy chọn Doctrine được chọn trên UI của người chơi tại `QuestScreen.cs` và tự động quy đổi giải thưởng thành Gems trong `QuestService.cs`. Đây là một giới hạn thiết kế mà UI của QuestScreen chưa hiển thị rõ cảnh báo đến người dùng.
3. **Lưu trữ Tên Vật Phẩm Và Index của Recipe theo Chuỗi tĩnh**:
   - Khi chiêu mộ Hero hoặc mang trang bị, hệ thống sử dụng định danh tĩnh (Definition ID) như `"RustySword"`, `"Footman"`. UI khi bind dữ liệu cần tra cứu lại trong `GameDatabase` để hiển thị tên thân thiện với người dùng, làm tăng độ phức tạp trong các controller.

## 2. Hạn Chế Phạm Vi Đợt Audit
- Quá trình phân tích chỉ tập trung kiểm định vào 4 luồng nghiệp vụ nền tảng: Boot & Load, Save & Close/Restore, Mismatch Matrix Tavern/Quest/Character và UI Layout wrapping.
- Các module chuyên sâu hơn như Combat, Shop, Crafting, Dungeons và Pet System nằm ngoài phạm vi phân tích của đợt này và sẽ được audit ở các Batch tiếp theo.
