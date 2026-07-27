# Data Validation Rules

## Mức độ lỗi (Severity)
- **INFO:** Thông tin log phụ.
- **WARNING:** Thiếu icon, missing localization text (Sẽ fallback sang text ID). Vẫn cho phép build JSON.
- **ERROR:** Lỗi logic nhỏ, reference sai với các tính năng không bắt buộc.
- **FATAL:** Lỗi nghiêm trọng, block export JSON. Trò chơi sẽ crash nếu nạp data này.

## FATAL Rules
- **Duplicate ID:** Hai entity trùng ID (VD: 2 Item cùng ID \wood\).
- **Missing Required Field:** Thiếu \id\, \
ameKey\.
- **Missing Referenced ID:** Recipe cần \"abyssal_cutlass"\ nhưng item này không tồn tại trong DB. Dungeon thả quái vật \"ghost"\ nhưng quái vật bị xóa.
- **Circular Reference:** Item A cần Item B để chế, Item B cần Item A.
- **Negative Value:** Máu quái vật < 0, giá tiền nâng cấp < 0.
- **Invalid Drop Weight:** Tổng Drop Rate của hầm ngục = 0.
