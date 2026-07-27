# S6.5A Stage 6 — Craft / Workshop Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage6_Craft_20260727_114600/` (263 files)

---

## Executive Summary

Stage 6 hoàn thiện toàn bộ luồng chế tạo Workshop/Craft theo decode:
- **Công thức & Nguyên liệu:** `CanCraft` và `GetMaxCraftable` tính số lượng chế tạo tối đa dựa vào nguyên liệu trong kho.
- **Hàng đợi Chế tạo (Queue):** `TryStartCraft` trừ nguyên liệu, đẩy vật phẩm vào `WorkshopQueue` với giới hạn dung lượng `WorkshopQueue(level, upgrade)`.
- **Tiến độ (Progress):** `ProgressWorkshop(deltaSeconds)` tăng thời gian `SecondsPassed` của item đầu hàng đợi. Khi hoàn thành, chuyển item sang `CompletedWorkshopItems`.
- **Nhận vật phẩm (Claim):** `ClaimCompletedCraft(instanceId)` đưa sản phẩm hoàn thành vào Kho đồ (`InventoryService`).
- **Placeholder UI:** `CraftScreen.cs` hiển thị danh sách hàng đợi và danh sách sản phẩm chờ nhận.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Services/ICraftService.cs` | Bổ sung `GetQueueCapacity`, `GetQueue`, `GetCompletedItems`, `GetMaxCraftable`, `ClaimCompletedCraft` |
| `Runtime/Services/CraftService.cs` | Implementation đầy đủ luồng chế tạo, tính max craftable, tiến độ và claim sản phẩm |
| `Runtime/UI/Craft/CraftScreen.cs` | **MỚI** — Placeholder UI hiển thị queue & nút claim |
| `Tests/EditMode/S6_5A_Stage6_CraftTests.cs` | **MỚI** — 2 EditMode tests kiểm tra max craftable calculation và toàn bộ vòng đời Craft -> Progress -> Claim |

---

## Status
# `STAGE6_IMPLEMENTED_READY_FOR_STAGE7`
