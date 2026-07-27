# Báo cáo S0-004: Decode Converter Foundation

## 1. Cấu trúc tool đã tạo
Tool Python Converter đã được xây dựng thành công tại `D:\Tinh\Rebuild_GuildMaster\Tools\DecodeConverter` với cấu trúc module hóa rõ ràng:
- `config/`: Chứa JSON settings (converter, alias, migrations).
- `src/`: Core logic gồm Scanner, Parser nền tảng, ID Normalizer, Config, Validator, và Exporter.
- `parsers/`: Các parser logic chuyên biệt như `ItemParser` và `StringsParser`.
- `tests/`: Bộ Unit test chuẩn dùng `unittest`.
- `output/`: Thư mục báo cáo và JSON xuất ra.

## 2. File Python đã tạo
- **Core**: `cli.py`, `config_loader.py`, `exporter.py`, `file_scanner.py`, `id_normalizer.py`, `inheritance_resolver.py`, `java_parser.py`, `java_reader.py`, `models.py`, `reference_resolver.py`, `report_writer.py`, `validator.py`.
- **Parsers**: `base_parser.py`, `enum_parser.py`, `item_parser.py`, `strings_parser.py`.
- **Runners**: `run_converter.py`.
- **Tests**: `test_id_normalizer.py`, `test_java_parser.py`, `test_validator.py`.

## 3. Command có thể chạy
```bash
python run_converter.py scan
python run_converter.py validate
python run_converter.py sample-items
python run_converter.py sample-localization
python run_converter.py report
python run_converter.py all-sample
python -m unittest discover -s tests
```

## 4. Kết quả Unit Test
- **Tổng số test**: 4
- **Pass / Fail**: 4 Pass / 0 Fail. (Bao gồm test chuẩn hóa ID PascalCase, bắt duplicate ID, parse file Java block class cơ bản).

## 5. Sample records đã parse (Item mẫu thật)
Đã lấy trực tiếp từ đường dẫn thật của bản gốc:
- `AbyssalIngot` (Material)
- `AbyssalCutlass` (Equipment)
- `PotionOfHealth` (Consumable)
*(20 Strings mẫu cũng đã được parse ra `localization_sample.json`).*

## 6. Validation issues
- Trong mẫu hiện tại không phát sinh issue FATAL nào. Report được ghi đầy đủ tại `output/reports/summary.md`. Validator framework đã hỗ trợ các check cơ bản như rỗng ID, trùng ID. Validation cross-reference đầy đủ sẽ cần full dataset ở task sau.

## 7. Unsupported Constructs
- Regex Java Parser hiện tại chỉ lấy toàn cục các assignment của `R.string` hay `R.drawable`, dẫn tới có file class nếu tham chiếu nhiều string (như PotionOfHealth lại dính reference của dungeon), nameKey có thể bị map sai thành `dungeon_name_frostbite_peaks`. Cần nâng cấp State Machine parser ở task sau để đọc chính xác từng biến cụ thể (`this.name = ...`).
- Các code block logic bên trong hàm, for loop, if statement (như của `Formulas.java`) là không hỗ trợ.

## 8. Sửa đổi hệ thống
- **Có sửa decode không?** TUYỆT ĐỐI KHÔNG. Không chạm vào D:\Tinh\Guild Master - Idle Dungeons.
- **Có sửa Unity code/scene không?** TUYỆT ĐỐI KHÔNG.
- Các tài liệu kiến trúc như `ADR-001`, `GameData_File_Layout.md`, `ID_Strategy.md` đã được update để tách bạch `GameDataProvider`, bỏ phụ thuộc vào Unity Resources. `Formula_Conversion_Classification.md` đã được tạo để phân loại 20 hàm đầu tiên.

## 9. Blocker
- Không có Blocker. 

## 10. Đề xuất cho S0-005
- **Task S0-005** nên tập trung vào việc **nâng cấp Java Parser State Machine** để trích xuất biến chính xác hơn (vd: phân biệt `this.name = ...` thay vì regex toàn cục) và mở rộng cho Item đầy đủ các thuộc tính. Cần thực hiện "Dry Run" toàn bộ Item Inventory.

## 11. Trạng thái Task
**S0-004 = Review**
