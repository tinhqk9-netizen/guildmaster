# S6.5A Stage 9 — Quest & Doctrine Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage9_Quest_20260727_115200/` (272 files)

---

## Executive Summary

Stage 9 hoàn thiện luồng Quest và nhận thưởng Doctrine/Gems chuẩn recovered rules:
- **Phần thưởng theo Rarity (`rewardFromRarity`):**
  - Rarity 1: 1 LP (Doctrine) / 10 Gems
  - Rarity 2: 2 LP / 20 Gems
  - Rarity 3: 3 LP / 40 Gems
  - Rarity 4: 5 LP / 100 Gems
- **Claim Quest & Nối Doctrine:** Khi nhận thưởng quest không phải gems, điểm thưởng được cộng trực tiếp vào tiến độ Doctrine (`DoctrineService.AddProgress(doctrineName, amount)`), tính toán Level Up dựa theo công thức `Formulas.TotalStarsToNextLp(level)`.
- **Placeholder UI:** `QuestScreen.cs` hiển thị danh sách nhiệm vụ và nút Claim.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Services/IQuestService.cs` | Bổ sung `ClaimReward`, `GetRewardAmount`, `GetActiveQuests` |
| `Runtime/Services/QuestService.cs` | Nối `IDoctrineService`, triển khai `GetRewardAmount` và `ClaimReward` |
| `Runtime/Services/ServiceContainer.cs` | Khởi tạo `DoctrineService` trước và truyền vào `QuestService` |
| `Runtime/UI/Quest/QuestScreen.cs` | **MỚI** — Placeholder UI hiển thị danh sách Quest & claim button |
| `Tests/EditMode/S6_5A_Stage9_QuestTests.cs` | **MỚI** — 2 EditMode tests kiểm tra rewardFromRarity và ClaimReward -> Doctrine progress |

---

## Status
# `STAGE9_IMPLEMENTED_READY_FOR_STAGE10`
