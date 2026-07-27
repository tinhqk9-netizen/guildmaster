# S6.5A Stage 8 — Dungeon / Combat / Target / Loot Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage8_DungeonCombat_20260727_115000/` (269 files)

---

## Executive Summary

Stage 8 triển khai thành công hệ thống Dungeon State Machine và công thức tính sát thương Combat chuẩn recovered rules:
- **Dungeon State Machine (Timing):** `Action` state machine với 7 trạng thái chuẩn timing bytecode: `ENTER_DUNGEON` (5), `ENTER_ROOM` (5), `FIGHT` (2), `LOOT` (5), `SEARCH_ROOM` (5), `RESPAWN` (18), `FLEE` (12).
- **Quy tắc Reset Tiến độ khi Thua:** Khi chết/thua (`Action(5)`), progress chỉ reset về 0 nếu `Progress < 250` (chuẩn Rule T-05).
- **Công thức ApplyDamage:**
  - Giảm sát thương theo DEF/MDEF: `reduction = min(1.0, (1 - armorIgnored) * 0.01 * defStat)`.
  - Trừ Flat reduction (`CON / 8`) và khiên `CurrentShield` trước HP.
  - Làm tròn kết quả cuối cùng qua `DecodeMath.Round`.
- **Target Selection (15 strategy):** `TargetSelectionService` hỗ trợ 15 chiến thuật chọn mục tiêu.
- **Placeholder UI:** `DungeonScreen.cs` hiển thị trạng thái dungeon và lượt đi.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Services/ICombatService.cs` | Bổ sung `ApplyDamage` |
| `Runtime/Services/CombatService.cs` | Implementation `ApplyDamage` chuẩn 100% recovered rules & `DecodeMath.Round` & khiên |
| `Runtime/Services/IDungeonService.cs` | Bổ sung `Tick` và `GetActiveDungeon` |
| `Runtime/Services/DungeonService.cs` | Implementation `Tick` state machine & progress reset rule |
| `Runtime/UI/Dungeon/DungeonScreen.cs` | **MỚI** — Placeholder UI cho Dungeon & Combat |
| `Tests/EditMode/S6_5A_Stage8_DungeonCombatTests.cs` | **MỚI** — 2 EditMode tests kiểm tra ApplyDamage (Defense + Shield) và Dungeon State Machine Tick |

---

## Status
# `STAGE8_IMPLEMENTED_READY_FOR_STAGE9`
