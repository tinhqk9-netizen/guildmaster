# Báo cáo S0-003: Data Contract & Conversion Strategy

## File đã tạo
Toàn bộ các file trong \Docs/Data/\, \Docs/Data/Schemas\, \Docs/Decisions/\ đã hoàn tất (tổng cộng 20+ files).

## Schema đã chốt
Tách bạch Static Definition (Item, Enemy) và Runtime State (Player Inventory, Quest Progress). Mọi liên kết giữa chúng được khóa bằng chuỗi \id\ theo chuẩn \snake_case\.

## Rủi ro & Blocker
- Rủi ro lớn nhất là bộ \Formulas.java\ chứa các rule đặc thù (hardcode if-else cho từng item). Tool JSON converter không thể bóc tách logic này, bắt buộc phải code thủ công trong C#.
- Blocker: Hiện chưa có blocker nào. Mọi cấu trúc data đã sẵn sàng để viết Converter.

## Đề xuất cho S0-004
Bước tiếp theo S0-004 nên tập trung vào việc **viết Tool Java-to-JSON Converter**.

*Cam kết: Tuyệt đối không thay đổi, ghi đè file Java decode hay code Unity Scene.*
