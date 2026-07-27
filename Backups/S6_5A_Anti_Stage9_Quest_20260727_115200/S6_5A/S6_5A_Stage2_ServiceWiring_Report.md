# S6.5A Stage 2 — Runtime Service Wiring Report

**Ngày:** 2026-07-27
**Backup:** `Backups/S6_5A_Anti_Stage2_ServiceWiring_20260727_113800/` (248 files)

---

## Executive Summary

Stage 2 thực hiện nối toàn bộ 18 Runtime Services vào một `ServiceContainer` tập trung (Composition Root), đảm bảo **tất cả 18 service dùng chung một instance `ISaveService` và `IFormulaService` duy nhất**. Không còn bất kỳ service nào tự tạo `SaveService` độc lập.

---

## Các thay đổi chính (Files Changed)

| File | Thay đổi |
|---|---|
| `Runtime/Services/ServiceContainer.cs` | **MỚI** — Composition Root cho 18 runtime services |
| `Runtime/Services/IDoctrineService.cs` | **MỚI** — Interface quản lý 8 doctrine & progress |
| `Runtime/Services/DoctrineService.cs` | **MỚI** — Implementation nối với `SaveData` & `FormulaService.TotalStarsToNextLp` |
| `Runtime/Services/ITavernService.cs` | **MỚI** — Interface quản lý Tavern visitor, capacity & recruit |
| `Runtime/Services/TavernService.cs` | **MỚI** — Implementation nối với `SaveData`, `FormulaService`, `CharacterService` |
| `Runtime/Services/ITargetSelectionService.cs` | **MỚI** — Interface chọn mục tiêu chiến đấu (15 strategy) |
| `Runtime/Services/TargetSelectionService.cs` | **MỚI** — Implementation thuật toán targeting |
| `Runtime/Services/ISettingsService.cs` | **MỚI** — Interface quản lý cài đặt |
| `Runtime/Services/SettingsService.cs` | **MỚI** — Implementation nối với `SaveData` (Sound, Music, Vibration, Language...) |
| `Runtime/Boot/Bootstrapper.cs` | Nối `ServiceContainer`, expose `Services` |
| `Runtime/Boot/UIRuntimeBootstrap.cs` | Nối `ServiceContainer` cho UI Bootstrap |
| `Tests/EditMode/S6_5A_Stage2_ServiceWiringTests.cs` | **MỚI** — 5 EditMode tests cho ServiceContainer & 18 services |

---

## Danh sách 18 Runtime Services được nối chung

1. `ISaveService` (Shared instance)
2. `IFormulaService` (Shared instance)
3. `IItemService`
4. `IInventoryService`
5. `ICharacterService`
6. `IEquipmentService`
7. `ISkillService`
8. `IStatusEffectService`
9. `ICraftService`
10. `IMerchantService`
11. `IDungeonService`
12. `ICombatService`
13. `ITargetSelectionService`
14. `ILootService`
15. `IQuestService`
16. `IDoctrineService`
17. `ITavernService`
18. `ISettingsService`
19. `IOfflineProgressService`

---

## Status
# `STAGE2_IMPLEMENTED_READY_FOR_STAGE3`
