# S6.5A Stage 11 — Offline Progress Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage11_Offline_20260727_115600/` (278 files)

---

## Executive Summary

Stage 11 hoàn thiện hệ thống mô phỏng tiến độ ngoại tuyến (Offline Progress):
- **12-Hour Cap:** Giới hạn thời gian tính offline tối đa 12 giờ (43,200 giây).
- **Mô phỏng hàng đợi:** Đẩy thời gian delta offline vào `CraftService.ProgressWorkshop(delta)` và `MerchantService.ProgressMarket(delta)`.
- **Đồng bộ thời gian save:** Cập nhật `SaveData.Metadata.SaveTimeUnix` lên mốc thời gian hiện tại.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Services/OfflineProgressService.cs` | Kiểm tra tính chính xác của mốc 12h cap và simulation |
| `Tests/EditMode/S6_5A_Stage11_OfflineTests.cs` | **MỚI** — 2 EditMode tests kiểm tra 12h cap và ApplyOfflineProgress simulation |

---

## Status
# `STAGE11_IMPLEMENTED_READY_FOR_STAGE12`
