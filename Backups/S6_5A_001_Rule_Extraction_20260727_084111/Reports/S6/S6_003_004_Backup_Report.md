# S6-003/S6-004 Pre-Implementation Backup Report

**Timestamp:** 2026-07-25 15:36:34
**Backup folder:** `D:\Tinh\Rebuild_GuildMaster\Backups\S6_003_004_PreImplementation_20260725_153634\`
**Reason:** Chuẩn bị sửa UI integration (S6-003: CharacterScreen/CharacterService, có thể UIRuntimeBootstrap) và Save integration (S6-004: thêm điểm gọi `SaveService.Save()`). Backup toàn bộ nhóm file có khả năng bị đụng trước khi sửa, theo đúng rule bắt buộc.

## Files/Folders Backed Up (86 file)
| Nhóm | Backup Path | Note |
|---|---|---|
| `ProjectSettings/EditorBuildSettings.asset` | `.../ProjectSettings/` | Đã DONE ở S6-002, backup lại phòng ngừa |
| `Assets/_Game/Scenes/*.unity` (4 scene + meta) | `.../Assets/_Game/Scenes/` | Bao gồm Boot/Main thật và 2 bản rỗng trùng lặp |
| `Assets/StreamingAssets/GameData/*` (13 JSON + meta) | `.../Assets/StreamingAssets/` | Data đã đổ vào ở S6-001 |
| `Runtime/Boot/*.cs` (Bootstrapper, BootSceneLoader, UIRuntimeBootstrap) | `.../Scripts/Runtime/Boot/` | UIRuntimeBootstrap dự kiến có thể sửa ở S6-004 (thêm save trigger) |
| `Runtime/UI/Character/CharacterScreen.cs` | `.../Scripts/Runtime/UI/Character/` | Dự kiến sửa ở S6-003 nếu CharacterService an toàn để wire |
| `Runtime/UI/Inventory/InventoryScreen.cs` | `.../Scripts/Runtime/UI/Inventory/` | Không dự kiến sửa, backup phòng ngừa |
| `Runtime/UI/HUD/HUDController.cs` | `.../Scripts/Runtime/UI/HUD/` | Không dự kiến sửa, backup phòng ngừa |
| `Runtime/UI/Core/*.cs` (IUIService, SafeArea, UIScreen, UIService) | `.../Scripts/Runtime/UI/Core/` | Backup phòng ngừa |
| `Runtime/UI/Popup/PopupScreen.cs` | `.../Scripts/Runtime/UI/Popup/` | Backup phòng ngừa |
| `Runtime/Services/*.cs` (14 service + interface, 24 file) | `.../Scripts/Runtime/Services/` | `CharacterService.cs` là trọng tâm audit S6-003 |
| `Runtime/Save/*.cs` (6 file: SaveService, SaveData, ISaveService...) | `.../Scripts/Runtime/Save/` | Trọng tâm S6-004 |
| `Reports/S6/*.md` (5 report hiện có) | `.../Reports/S6/` | Trạng thái trước khi cập nhật tiếp |

## Restore Instruction
1. Copy đè `Backups/S6_003_004_PreImplementation_20260725_153634/ProjectSettings/EditorBuildSettings.asset` → vị trí gốc
2. Copy đè toàn bộ `Backups/.../Assets/_Game/Scenes/*` → `Assets/_Game/Scenes/`
3. Copy đè toàn bộ `Backups/.../Assets/StreamingAssets/*` → `Assets/StreamingAssets/`
4. Copy đè các file trong `Backups/.../Scripts/**` về đúng vị trí gốc tương ứng trong `Assets/_Game/Scripts/`
5. Mở Unity, chờ compile lại, verify Console 0 lỗi

## Status
Backup hoàn tất, không lỗi (86 file). Tiếp tục Step 0 verify S6-002 (đã DONE — xem `S6_002_Runtime_BuildSettings_Integration_Report.md`), sau đó S6-003.
