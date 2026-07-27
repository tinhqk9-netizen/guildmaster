# Decode Converter

Bộ công cụ Python giúp chuyển đổi mã nguồn Java của game Guild Master sang JSON format.
Tuân thủ nghiêm ngặt nguyên tắc READ ONLY với thư mục nguồn.

## Yêu cầu
- Python 3.8+
- Không yêu cầu third-party package (không dùng pip install).

## Cấu trúc
- \config/\: Chứa file JSON cấu hình và alias.
- \src/\: Lõi converter, parser, exporter, validator.
- \parsers/\: Các parser xử lý từng hệ thống cụ thể (Items, Enum, Strings).
- \	ests/\: Unit tests (dùng \unittest\ mặc định).
- \output/\: Kết quả export.

## Cách chạy
\\\ash
# Chạy demo (export 3 item và 20 strings)
python run_converter.py all-sample

# Chạy unit tests
python -m unittest discover -s tests
\\\

## Các Lệnh (Commands)
- \scan\: Quét và liệt kê file Java, strings, images.
- \alidate\: Kiểm tra chéo toàn bộ dữ liệu (Không ghi JSON).
- \sample-items\: Parse và xuất JSON cho 3 vật phẩm mẫu.
- \sample-localization\: Parse và xuất 20 string vào JSON.
- \eport\: Tạo validation report.
- \ll-sample\: Chạy các lệnh sample và report.

## Cờ (Flags)
- \--config\: Trỏ đến file config tùy chỉnh.
- \--verbose\: Bật log DEBUG.
- \--fail-on-fatal\: Thoát tiến trình ngay nếu gặp lỗi FATAL (exit code 1).

## Giới hạn
- Hiện tại Java parser chỉ sử dụng regex và tokenizer đơn giản, không dựng cây AST đầy đủ, do vậy logic trong các method phức tạp (\Formulas.java\) sẽ không được convert tự động.

## Troubleshooting
Nếu không có quyền ghi \output/\, dùng quyền Admin hoặc thiết lập lại permissions cho thư mục Tool.
