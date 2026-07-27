# S6.5A Stage 4 — Character / Stat / Equipment / Skill Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage4_Character_20260727_114200/` (261 files)

---

## Executive Summary

Stage 4 cập nhật và chuẩn hoá công thức tính chỉ số `GetTotalStat` theo đúng 100% recovered rules từ decode:
- **`DecodeMath.Round` (truncate `+0.0001`):** Sử dụng chuẩn ở bước cuối cùng của `GetTotalStat`.
- **Ascended Multiplier (1.5x):** Chỉ nhân cho CON, INT, DEX, MAX_HP. **DEF và MDEF KHÔNG nhân 1.5x**.
- **Potion Index Mapping:** CON -> `potions[0]`, INT -> `potions[2]`, DEX -> `potions[1]`, HP -> `potions[3] * 5`, DEF -> `potions[4]`, MDEF -> `potions[5]`. (INT và DEX bị đảo chỉ số đúng theo bytecode).
- **Trait Multiplier Table:** Hỗ trợ đầy đủ các trait `BRUTE`, `STOUT`, `BOOKWORM`, `FERAL`, `NIMBLE`, `KEEN_EYED`.

---

## Files Changed

| File | Thay đổi |
|---|---|
| `Runtime/Save/SaveData.cs` | Thêm `IsAscended`, `Trait`, `PotionsDrank` (int[6]) vào `CharacterSaveData` + normalize |
| `Runtime/Models/CharacterRuntime.cs` | Thêm `IsAscended`, `Trait`, `PotionsDrank` (int[6]) |
| `Runtime/Services/CharacterService.cs` | Viết lại `GetTotalStat` chuẩn 100% theo recovered rules & potion mapping & trait multiplier & `DecodeMath.Round` |
| `Tests/EditMode/S6_5A_Stage4_CharacterTests.cs` | **MỚI** — 3 EditMode tests kiểm tra Ascended multiplier (DEF/MDEF k nhân), Potion index mapping và Trait table |

---

## Status
# `STAGE4_IMPLEMENTED_READY_FOR_STAGE5`
