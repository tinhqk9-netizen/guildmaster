# ADR-001: Data-Driven JSON thay vì ScriptableObject làm Data Source chính

## Bối cảnh
Dự án có hàng trăm Items, Enemies, Skills từ bản Java. Việc tạo thủ công ScriptableObject (SO) trong Unity Editor sẽ mất vô số giờ đồng hồ và rủi ro nhập sai dữ liệu.

## Quyết định
Sử dụng **JSON Files** được sinh ra từ tool tự động làm Nguồn Dữ Liệu Tĩnh gốc (Single Source of Truth cho Definition). ScriptableObject chỉ được dùng vào lúc runtime để nạp JSON vào bộ nhớ Unity, hoặc để làm template cho Object.

## Tại sao không dùng 100% ScriptableObject?
Việc sinh ra hàng ngàn file .asset bằng Editor Script làm Project phình to, chậm git, và khó merge conflict. JSON dễ đọc, dễ merge, và dễ update bằng server từ xa (nếu cần).

## Không sử dụng Resources
Không sử dụng \Resources\ làm asset source mặc định. Asset sẽ được resolve qua Sprite Catalog hoặc Addressables abstraction. Chưa cài Addressables trong task hiện tại, vì vậy GameDataProvider sẽ được tạo thành Abstraction layer.
