# Guild Master - Unity Rebuild Project

Dự án game Guild Master được tái cấu trúc và phát triển trên Unity 6 LTS.

---

## 🛠️ Yêu Cầu Hệ Thống & Môi Trường (Prerequisites)

- **Unity Editor Version:** `6000.3.17f1` (Unity 6 LTS)
- **Render Pipeline:** Universal Render Pipeline (URP 17.3.0)
- **Target Platform:** Mobile (Android / iOS) & PC
- **Git LFS:** Khuyên dùng Git LFS (`git lfs install`) trước khi clone repo để tải đầy đủ các file binary/asset lớn.

---

## 🚀 Hướng Dẫn Tiếp Quản & Khởi Chạy Project

### 1. Clone Repository
```bash
git clone https://github.com/tinhqk9-netizen/guildmaster.git
cd guildmaster
git lfs pull
```

### 2. Mở Dự Án Trong Unity
1. Mở **Unity Hub**.
2. Chọn **Add** -> **Add project from disk**.
3. Trỏ tới thư mục vừa clone.
4. Mở bằng Unity phiên bản `6000.3.17f1`.
5. Đợi Unity tự động rebuild lại thư mục `Library/` và load các Packages.

---

## 📁 Cấu Trúc Thư Mục Dự Án (Repository Structure)

```text
├── Assets/                 # Mã nguồn C#, Prefabs, Scenes, Textures, Audio & UI
│   └── _Game/              # Cấu trúc thư mục chính của dự án GuildMaster
│       ├── Scripts/        # Các script C# hệ thống (Runtime, UI, Services, Data)
│       ├── Scenes/         # Các Unity Scene chính của game
│       ├── Prefabs/        # Các UI & Gameplay Prefab
│       └── Tests/          # Bộ test tự động (EditMode & PlayMode)
├── Packages/               # Quản lý Unity Packages (bao gồm manifest.json)
├── ProjectSettings/        # Cấu hình dự án Unity (Tags, Layers, Input, Graphics...)
├── Docs/                   # Tài liệu kiến trúc & thiết kế hệ thống
├── Reports/                # Báo cáo audit, kiểm thử và lộ trình phát triển
├── Tools/                  # Các script helper & công cụ hỗ trợ dev
└── Backups/                # Các bản lưu trữ snapshot qua các giai đoạn nâng cấp
```

---

## 🧪 Chạy Bộ Kiểm Thử (Running Tests)

Dự án bao gồm bộ kiểm thử tự động toàn diện (EditMode & PlayMode):

1. Trong Unity Editor, mở window **Test Runner**: `Window -> General -> Test Runner`.
2. Chọn tab **EditMode** hoặc **PlayMode**.
3. Nhấn **Run All** để chạy toàn bộ unit tests và integration tests.

---

## 📑 Tài Liệu Tham Khảo (Documentation)

- `GuildMaster_Unity_Rebuild_Phased_Plan.md`: Lộ trình tổng thể tái cấu trúc dự án.
- `Docs/`: Chứa các tài liệu chi tiết về hệ thống Service, UI Bridge, Data Loaders...
- `Reports/`: Chứa lịch sử audit và báo cáo sửa lỗi/biên dịch.

---
*Dự án được bàn giao đầy đủ mã nguồn, cấu hình scene, asset metadata (.meta files) và bộ test.*
