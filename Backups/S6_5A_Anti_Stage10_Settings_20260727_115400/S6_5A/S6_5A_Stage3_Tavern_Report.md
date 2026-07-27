# S6.5A Stage 3 — Tavern/Quarters/Recruit Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage3_Tavern_20260727_114000/` (258 files)

---

## Executive Summary

Stage 3 hoàn thiện toàn bộ hệ thống Tavern / Quarters / Recruit theo decode evidence:
- **Rule TR-01:** Kiểm tra capacity Quarters (`GetQuartersCapacity() > ownedCharacters`).
- **Rule TR-03:** Recruit hoàn toàn **MIỄN PHÍ** (không trừ money/gems).
- **Rule TR-06:** Khách mới thêm vào đầu danh sách (`Index 0`), tự động cắt tỉa đuôi nếu vượt quá `GetTavernCapacity()`.
- **Visitor Timer:** Đếm ngược `NextTavernVisit` dựa theo `GetTavernVisitorInterval(level, upgrade)`.
- **Upgrades:** Đã nới `UpgradeQuarters`, `UpgradeTavernCapacity`, `UpgradeTavernTime` trừ tiền và tăng level chuẩn formula.
- **Placeholder UI:** `TavernScreen.cs` hiển thị danh sách guest, timer, capacity và nút Recruit.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Services/ITavernService.cs` | Bổ sung `GenerateVisitor`, `UpgradeQuarters`, `UpgradeTavernCapacity`, `UpgradeTavernTime`, `GetVisitorIntervalSeconds`, `GetNextVisitorTimerSeconds` |
| `Runtime/Services/TavernService.cs` | Viết đủ visitor timer, guest generation (index 0 + trim), recruit free (TR-03), upgrade formulas |
| `Runtime/UI/UIScreenId.cs` | Thêm `Tavern` |
| `Runtime/UI/Tavern/TavernScreen.cs` | **MỚI** — Placeholder UI hiển thị danh sách khách & recruit |
| `Tests/EditMode/S6_5A_Stage3_TavernTests.cs` | **MỚI** — 4 EditMode tests kiểm chứng visitor generation, timer, recruit free (TR-03) và upgrades |

---

## Status
# `STAGE3_IMPLEMENTED_READY_FOR_STAGE4`
