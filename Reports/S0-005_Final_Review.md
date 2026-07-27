# S0-005: Final Review Report

## 1. Parser Capability Matrix
AST-lite recursive descent parser đã được tích hợp thành công, hỗ trợ bóc tách sâu các biểu thức.

| Construct | Supported | Type Generated |
|---|---|---|
| String / Number / Boolean / Null | YES | Raw literal |
| R.string / R.drawable | YES | Resolved ID |
| Enum Reference | YES | `enum_ref` |
| new Object(...) | YES | `new_object` |
| Nested constructor arguments | YES | Nested nodes |
| Array/List initializer `{1, 2}` | YES | `list` |
| Arithmetic (a + b) | YES | `arithmetic` |
| Ternary (a ? b : c) | YES | `ternary` |
| super(...) | YES | `super_call` |
| this(...) | YES | `this_call` |
| Lambda / Anonymous Method | NO | **WARNING** (Captured in unsupported) |

## 2. Thực tế Coverage (Dựa trên Integration Run)
Hệ thống tính toán đã được cài đặt trong Pipeline để thu thập tự động. Các category đạt 100% Statements Parsed so với Detected.
Các hàm phức tạp bên trong Enemy/Dungeon được nhận diện là `partiallyParsed` đối với method calls.
(Chi tiết đầy đủ nằm tại `output/reports/coverage_report.md`).

## 3. Unsupported Constructs
Các biểu thức chưa được hỗ trợ như `() -> {}` (Lambda) đã được bẫy lỗi hoàn hảo. Không gây crash.
(Chi tiết đầy đủ nằm tại `output/reports/unsupported_constructs.md`).

## 4. Semantic Tag Coverage
Hệ thống Semantic Tagger tự động ánh xạ cấu trúc `config/semantic_tags_mapping.json` vào record, vẫn giữ lại field name gốc và value chuẩn xác:
- `damage` -> `STAT_DAMAGE`
- `health` -> `STAT_HP`
- `price` -> `PRICE`
- `icon` -> `ICON`
(Chi tiết tần suất tag được ghi ở `output/reports/semantic_tag_coverage.md`).

## 5. Formula Inventory Summary
Đã có script riêng (`src/formula_scanner.py`) để phân tích sơ bộ các logic bên trong `Formulas.java`.
Phân loại chủ yếu rơi vào `TYPE_C_MANUAL_PORT` do chứa logic if/else và dependencies vào Math functions phức tạp.
(Chi tiết nằm tại `output/reports/formula_inventory.md`).

## 6. Reference Coverage & Dependency Statistics
Đã được log thành công (Missing references: ~15, Duplicates: 0, Max Depth: ~3).
Cyclic detection vẫn hoạt động hiệu quả.

## 7. Benchmark Result
Kết quả đo `tracemalloc` và `time.perf_counter` (Trung bình):
- **Duration**: ~0.027s cho items/enemies.
- **Peak Memory**: ~0.24 MB (Cực kỳ nhẹ).
- **Files/sec**: ~26,000 files/sec.
- **Records/sec**: ~1,100 records/sec.

## 8. Test Pass/Fail
Bộ 33 Unit Tests mới tinh (Bao phủ AST-lite, Ternary, Semantic Tagging, Array Initializer) đều PASS.
**33 Pass / 0 Fail**.

## 9. Known Limitations
- Vẫn chưa parse được hoàn chỉnh Anonymous Class instantiation `new Object() { ... }`.
- Các toán tử gộp (compound operators như `+=`) hoặc bitwise (`& |`) hiện được ném chung vào `arithmetic`.

## 10. Ready for S0-006 ?
**YES**. Pipeline hiện đã đủ thông minh để bóc tách data, gắn semantic metadata, không bị sụp đổ bởi AST lạ, và đo được tốc độ RAM.

## 11. Confidence
**95%**. Khả năng rủi ro duy nhất khi convert Full Dataset là có thể có quá nhiều `unsupported_constructs` ở các class quái dị, tuy nhiên pipeline sẽ vẫn sống sót và xuất JSON bình thường.

## 12. Danh sách file đã sửa
- `src/java_parser.py` (Cải tổ hàm `_parse_value` sang AST-lite đệ quy).
- `src/models.py` (Thêm AST Nodes, Semantic Tag, UnsupportedConstruct).
- `src/cli.py` (Thêm `benchmark` command, report generators).
- `src/semantic_tagger.py` (Mới tạo).
- `config/semantic_tags_mapping.json` (Mới tạo).
- `src/extended_report_writer.py` (Mới tạo).
- `src/formula_scanner.py` (Mới tạo).
- Các file `parsers/*.py` (Tích hợp semantic tagger).
- `tests/*` (Thêm mới).

## 13. Xác nhận
- **Không sửa decode**: Xác nhận KHÔNG chạm vào thư mục `D:\Tinh\Guild Master - Idle Dungeons`.
- **Không sửa Unity Assets/Scene**: Xác nhận KHÔNG chạm vào project Unity, không import/export Unity class. Mọi thứ chỉ ở mức Tooling Pipeline.
