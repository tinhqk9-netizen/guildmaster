# GameData File Layout

Thư mục xuất file dự kiến: \Assets/StreamingAssets/GameData/\

## Abstraction: GameDataProvider
Lưu ý: \StreamingAssets\ chỉ là provider mặc định của MVP, không phải dependency cố định của gameplay. Mọi data consumer phải làm việc qua \GameDataProvider\ abstraction.
Nguồn dữ liệu hiện tại có thể là:
- StreamingAssets
- local file
- Addressables
- remote source
- encrypted source

## Cấu trúc chuẩn
- encoding UTF-8
- Toàn bộ mảng gốc JSON được bọc trong object \{ "data": [...] }\.
- Deterministic sorting: Các items trong JSON bắt buộc phải sort theo \id\ a->z.
