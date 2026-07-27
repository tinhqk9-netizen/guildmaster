# UI Hierarchy Function Audit — Backup Report

**Timestamp:** 2026-07-25 18:39:27
**Backup folder:** `D:\Tinh\Rebuild_GuildMaster\Backups\UI_Hierarchy_Function_Audit_20260725_183927\`
**Reason:** Chuẩn bị audit vị trí/chức năng từng panel và đề xuất sắp xếp lại hierarchy theo nhóm chức năng game. Có khả năng phải đụng `Main.unity` và các script UI/editor → backup trước.

## Files Backed Up (38 file, 302 KB)
| # | Nhóm | Backup Path |
|---|---|---|
| 1 | `Assets/_Game/Scenes/Main.unity` (+ `.meta`) | `.../Assets/_Game/Scenes/` |
| 2 | `Assets/_Game/Scenes/Boot.unity` (+ `.meta`) | `.../Assets/_Game/Scenes/` |
| 3 | `Assets/_Game/Scripts/Runtime/UI/**` (Character, Core, HUD, Inventory, Popup, UIScreenId — toàn bộ) | `.../Assets/_Game/Scripts/Runtime/UI/` |
| 4 | `Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs` (+ `.meta`) | `.../Assets/_Game/Scripts/Runtime/Boot/` |
| 5 | `Assets/_Game/Scripts/Editor/UIScreenPreviewTool.cs` (+ `.meta`) | `.../Assets/_Game/Scripts/Editor/` |
| 6 | `Assets/_Game/Scripts/Editor/UIWiringGenerator.cs` (+ `.meta`) | `.../Assets/_Game/Scripts/Editor/` |
| 7 | `Reports/S6_5/*.md` (5 report) | `.../Reports/S6_5/` |

## Restore Instruction
1. Copy đè `Backups/UI_Hierarchy_Function_Audit_20260725_183927/Assets/_Game/Scenes/Main.unity` → `Assets/_Game/Scenes/`
2. Copy đè `.../Assets/_Game/Scripts/Runtime/UI/*` → `Assets/_Game/Scripts/Runtime/UI/`
3. Copy đè `.../Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs` → vị trí gốc
4. Copy đè `.../Assets/_Game/Scripts/Editor/*` → `Assets/_Game/Scripts/Editor/`
5. Mở Unity, chờ reimport, verify Console 0 lỗi

## Status
Backup hoàn tất, không lỗi. Tiếp tục Task 1 — Audit.
