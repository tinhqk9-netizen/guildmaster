# ID Strategy

## Định dạng ID
- **Quy tắc:** Sử dụng \snake_case\ cho toàn bộ định danh (ID) trong JSON và game logic.
- **Chữ hoa/chữ thường:** Toàn bộ chữ thường (lowercase).

## Cách chuyển Java class name thành ID
- Class gốc: \AbyssalCutlass\ -> ID: \byssal_cutlass\
- Class gốc: \AmanitaObscura\ -> ID: \manita_obscura\
- Tool Converter sẽ tự động chèn dấu \_\ trước các chữ cái in hoa (trừ chữ cái đầu tiên) và chuyển tất cả thành lowercase.

## Xử lý Duplicate & Renamed
- **Duplicate:** Nếu phát hiện trùng ID (do 2 class trùng tên khác package), hệ thống sẽ báo lỗi FATAL. Bắt buộc thêm tiền tố package (VD: \item_abyssal_cutlass\).
- **Renamed Content:** Khi đổi tên một class Java ở bản gốc nhưng muốn giữ ID cũ cho Save Data, cần cấu hình map tay trong file \lias_map.json\.

## Xử lý References
- Mọi quan hệ (như Item cần để craft Recipe, hoặc Enemy trong Dungeon) đều tham chiếu qua chuỗi string ID (\"plant_fiber"\). Không dùng instance memory reference trong JSON.
- **R.string:** Lấy tên resource (VD: \R.string.abyssal_cutlass_name\) bỏ \R.string.\ -> \"abyssal_cutlass_name"\. Unity sẽ dùng key này trong hệ thống Localization.
- **R.drawable:** Tương tự, \R.drawable.abyssal_cutlass\ -> \"abyssal_cutlass"\. Dùng để load Sprite từ Addressables hoặc Resources.
- **Enum:** Java Enum \StatusEffectType.TAUNT\ -> \"taunt"\.
- **Class Inheritance:** Không map inheritance trực tiếp vào JSON. Các class con sẽ được "flatten" (làm phẳng) thành các fields tương ứng.

## Migration & Versioning
- **alias_map.json**: Dùng để xử lý renamed ID hoặc các Alias nếu đổi tên.
- **migration_rules.json**: Ghi nhận removed ID, fallback ID và versioned migration.
