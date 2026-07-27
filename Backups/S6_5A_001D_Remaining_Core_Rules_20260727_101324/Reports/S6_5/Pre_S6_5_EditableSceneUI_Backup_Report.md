# Pre-S6.5 Editable Scene UI — Backup Report

**Timestamp:** 2026-07-25 18:03:37
**Backup folder:** `D:\Tinh\Rebuild_GuildMaster\Backups\Pre_S6_5_EditableSceneUI_20260725_180337\`
**Reason:** Chuẩn bị chuyển workflow UI từ runtime-generated sang edit-time editable scene/prefab. Có khả năng phải sửa `Main.unity`, `UIRuntimeBootstrap.cs`, các editor generator và UI script — backup toàn bộ nhóm này trước khi đụng vào.

## Files/Folders Backed Up (71 file, 484 KB)
| # | Nhóm | Backup Path | Note |
|---|---|---|---|
| 1 | `Assets/_Game/Scenes/*.unity` + meta (4 scene) | `.../Assets/_Game/Scenes/` | **Quan trọng nhất** — `Main.unity` là đối tượng sửa chính |
| 2 | `Assets/_Game/Scripts/Runtime/Boot/*` (BootSceneLoader, Bootstrapper, UIRuntimeBootstrap + meta) | `.../Assets/_Game/Scripts/Runtime/Boot/` | `UIRuntimeBootstrap.cs` dự kiến sửa để ưu tiên find object có sẵn |
| 3 | `Assets/_Game/Scripts/Runtime/UI/**` (Character, Core, HUD, Inventory, Popup, UIScreenId) | `.../Assets/_Game/Scripts/Runtime/UI/` | Toàn bộ UI script |
| 4 | `Assets/_Game/Scripts/Editor/**` (UIFoundationGenerator, UIWiringGenerator, AssetCatalogBuilder, AssetDatabaseVerifier, AssetImportPresets, SpriteSheetSlicer, TilesetSheetSlicer) | `.../Assets/_Game/Scripts/Editor/` | Generator có thể phải cập nhật |
| 5 | `Assets/_Game/Data/AssetCatalog.asset` + meta | `.../Assets/_Game/Data/` | Catalog asset từ S5 |
| 6 | `Assets/_Game/Art/UI/*.meta` (ui_dialog, ui_kit) | `.../Assets/_Game/Art_UI_meta_only/` | Chỉ backup `.meta` (import settings); file PNG gốc không bị sửa |
| 7 | `ProjectSettings/EditorBuildSettings.asset` | `.../ProjectSettings/` | Scene list |
| 8 | `Reports/S6/*.md` (13 report) | `.../Reports/S6/` | Toàn bộ báo cáo S6 |

## Restore Instruction
1. Copy đè `Backups/Pre_S6_5_EditableSceneUI_20260725_180337/Assets/_Game/Scenes/*` → `Assets/_Game/Scenes/` (khôi phục `Main.unity` về trạng thái trước khi bake UI)
2. Copy đè `.../Assets/_Game/Scripts/Runtime/Boot/*` → `Assets/_Game/Scripts/Runtime/Boot/`
3. Copy đè `.../Assets/_Game/Scripts/Runtime/UI/*` → `Assets/_Game/Scripts/Runtime/UI/`
4. Copy đè `.../Assets/_Game/Scripts/Editor/*` → `Assets/_Game/Scripts/Editor/`
5. Copy đè `.../Assets/_Game/Data/*` → `Assets/_Game/Data/`
6. Copy đè `.../ProjectSettings/EditorBuildSettings.asset` → `ProjectSettings/`
7. Nếu import setting Art/UI bị đổi: copy `.../Assets/_Game/Art_UI_meta_only/*.meta` → `Assets/_Game/Art/UI/`
8. Mở Unity, chờ reimport + compile, verify Console 0 lỗi

## Status
Backup hoàn tất, không lỗi. Tiếp tục Task 1 — Audit Editable Scene UI.
