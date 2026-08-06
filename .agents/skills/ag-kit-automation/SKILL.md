---
name: ag-kit-automation
description: "Quản lý và sử dụng Antigravity Kit (ag-kit) CLI từ mức global để cập nhật template."
trigger: "Khi người dùng yêu cầu cập nhật bộ kit, khởi tạo toolkit, hoặc dùng lệnh ag-kit."
requires: "Cài đặt npm global cho package @vudovn/ag-kit"
---

# AG-Kit Automation

> **Mục tiêu**: Kỹ năng này cung cấp cho AI nhận thức về sự tồn tại của Antigravity Kit (ag-kit) cũng như cách sử dụng nó dưới tư cách là một lệnh global.

## 1. Khởi tạo (Init) / Cập nhật
- **Lệnh**: `ag-kit init`
- **Chức năng**: Dùng để tải toàn bộ template mới nhất của bộ Kit về thư mục `.agent` tại dự án hiện tại.
- **Vị trí**: Chạy lệnh này tại gốc thư mục dự án. Nó sẽ cài đặt các file `.agent/skills`, `.agent/agents`, `.agent/workflows`, v.v.

## 2. Cập nhật phiên bản Global
- **Lệnh**: `npm install -g @vudovn/ag-kit`
- **Chức năng**: Nâng cấp phiên bản CLI của Antigravity Kit lên bản mới nhất.

## 3. Quy trình thực thi an toàn
Khi user yêu cầu cập nhật nội dung kit:
1. Luôn ưu tiên dùng `ag-kit init`.
2. Do việc chạy `ag-kit init` có thể ghi đè (override) lên một số template hiện có mà user đã tự chỉnh sửa, cần hỏi xác nhận rõ ràng trước khi chạy. Mở cổng Socratic Gate để hỏi user về rủi ro ghi đè file.
