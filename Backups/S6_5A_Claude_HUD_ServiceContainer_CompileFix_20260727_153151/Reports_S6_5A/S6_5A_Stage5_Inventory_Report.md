# S6.5A Stage 5 — Inventory Actions Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage5_Inventory_20260727_114400/` (262 files)

---

## Executive Summary

Stage 5 bổ sung đầy đủ các thao tác kho đồ theo decode:
- **Phân loại (Filtering):** API `GetItemsByCategory(ItemCategory)` lọc vật phẩm theo danh mục.
- **Khóa vật phẩm (Locking):** API `ToggleLockItem(instanceId)` hỗ trợ bật/tắt trạng thái khóa vật phẩm và đồng bộ save.
- **Sử dụng vật phẩm (Consumable usage):** API `UseConsumable(instanceId, character)` áp dụng hiệu ứng bình máu/thuốc và trừ số lượng stack.
- **Đồng bộ Save:** Mọi thao tác đều đồng bộ trực tiếp vào `SaveData.Items`.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Services/IInventoryService.cs` | Bổ sung `GetItemsByCategory`, `ToggleLockItem`, `UseConsumable` |
| `Runtime/Services/InventoryService.cs` | Implementation các API phân loại, khóa đồ và dùng consumable |
| `Tests/EditMode/S6_5A_Stage5_InventoryTests.cs` | **MỚI** — 3 EditMode tests kiểm tra Category filtering, Toggle lock và Consumable usage |

---

## Status
# `STAGE5_IMPLEMENTED_READY_FOR_STAGE6`
