# S6.5A Stage 7 — Merchant / Market Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage7_Merchant_20260727_114800/` (266 files)

---

## Executive Summary

Stage 7 triển khai luồng mua bán Merchant & chợ (Market) hoàn toàn chuẩn theo recovered rules:
- **Currency Resolution:** Loại tiền thanh toán được quyết định bởi `offer.IsGems` (`Money` nếu false, `Gems` nếu true).
- **Quy tắc Fail No Mutation:** Nếu không đủ kho hoặc không đủ tiền/gems, giao dịch thất bại và **tuyệt đối không trừ tiền/gems, không xóa offer khỏi stock**.
- **Chế độ mua hàng:** Trừ tiền/gems, gỡ offer khỏi `MerchantRegularStockItems` hoặc `MerchantSpecialReserve`, và trao vật phẩm vào `InventoryService`.
- **Luồng chợ (Market Sell/Progress/Claim):** `SellItem` trừ đồ đưa vào `MarketListings`, `ProgressMarket(deltaSeconds)` chuyển đồ bán sang `SoldMarketItems`, và `ClaimSoldItem` cộng tiền vào `SaveData.Money`.
- **Placeholder UI:** `MerchantScreen.cs` hiển thị danh sách stock thường và stock đặc biệt.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Services/IMerchantService.cs` | Bổ sung `BuyOffer`, `GetRegularStock`, `GetSpecialStock`, `ClaimSoldItem` |
| `Runtime/Services/MerchantService.cs` | Implementation đầy đủ luồng Mua hàng (Gold/Gems), Fail No Mutation, Market Progress & Claim |
| `Runtime/UI/Merchant/MerchantScreen.cs` | **MỚI** — Placeholder UI hiển thị danh sách offer regular & special |
| `Tests/EditMode/S6_5A_Stage7_MerchantTests.cs` | **MỚI** — 4 EditMode tests kiểm tra Buy Gold, Buy Fail (no mutation), Buy Gems và luồng Market Sell -> Progress -> Claim |

---

## Status
# `STAGE7_IMPLEMENTED_READY_FOR_STAGE8`
