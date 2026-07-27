# S6 Pre-Implementation Backup Report

**Timestamp:** 2026-07-25 15:03:55
**Backup folder:** `D:\Tinh\Rebuild_GuildMaster\Backups\S6_PreImplementation_20260725_150355\`
**Reason:** Chuẩn bị sửa data path (S6-001) và Build Settings/scene (S6-002) theo quyết định user: S6 phải build được Standalone/APK thật, không chỉ chạy trong Editor Play Mode. Backup toàn bộ file dự kiến bị đụng tới trước khi sửa.

## Files/Folders Backed Up
| # | Original Path | Backup Path | Note |
|---|---|---|---|
| 1 | `ProjectSettings/EditorBuildSettings.asset` | `Backups/S6_PreImplementation_20260725_150355/ProjectSettings/EditorBuildSettings.asset` | Sẽ sửa scene list ở S6-002 |
| 2 | `Assets/_Game/Scenes/Boot.unity` | `.../Assets/_Game/Scenes/Boot.unity` | Scene thật |
| 3 | `Assets/_Game/Scenes/Boot 1.unity` | `.../Assets/_Game/Scenes/Boot 1.unity` | Scene rỗng trùng lặp |
| 4 | `Assets/_Game/Scenes/Main.unity` | `.../Assets/_Game/Scenes/Main.unity` | Scene thật, đã pass S5 — **không được phá** |
| 5 | `Assets/_Game/Scenes/Main 1.unity` | `.../Assets/_Game/Scenes/Main 1.unity` | Scene rỗng trùng lặp |
| 6 | `Assets/StreamingAssets/` (chỉ có `GameData.meta`, folder `GameData` rỗng) | `.../Assets/StreamingAssets/GameData.meta` | Sẽ đổ JSON thật vào ở S6-001 |
| 7 | `Assets/_Game/Scripts/Infrastructure/DataProviders/*.cs` (3 file) | `.../Scripts/Infrastructure/DataProviders/` | Có thể cần điều chỉnh |
| 8 | `Assets/_Game/Scripts/Database/*.cs` (3 file) | `.../Scripts/Database/` | Tham chiếu, có thể không sửa nhưng backup phòng ngừa |
| 9 | `Assets/_Game/Scripts/Bootstrap/Bootstrapper.cs` | `.../Scripts/Bootstrap/Bootstrapper.cs` | Dead-end class từ S1, audit đã ghi nhận |
| 10 | `Assets/_Game/Scripts/Runtime/Boot/Bootstrapper.cs` | `.../Scripts/Runtime/Boot/Bootstrapper.cs` | Composition root chưa gắn scene |
| 11 | `Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs` | `.../Scripts/Runtime/Boot/UIRuntimeBootstrap.cs` | Composition root S5 đang chạy thật — **không được phá** |
| 12 | `Reports/S5/*.md` (4 file) | `.../Reports/S5/` | Báo cáo S5 hiện có |
| 13 | `Reports/S6/S6_Integration_Audit_Report.md` | `.../Reports/S6/` | Báo cáo audit S6 Batch 1 |

**Không backup** `Assets/_Game/Data/Definitions`, `Assets/_Game/Data/Runtime` — cả hai đang rỗng (0 file), không có gì để mất, và S6-001/002 không dự kiến sửa vào đây.

## Restore Instruction
Nếu cần khôi phục về trạng thái trước S6 Batch 2 (S6-001/S6-002):

1. Copy đè `Backups/S6_PreImplementation_20260725_150355/ProjectSettings/EditorBuildSettings.asset` → `ProjectSettings/EditorBuildSettings.asset`
2. Copy đè toàn bộ `Backups/S6_PreImplementation_20260725_150355/Assets/_Game/Scenes/*` → `Assets/_Game/Scenes/`
3. Xoá nội dung `Assets/StreamingAssets/GameData/` (nếu S6-001 đã đổ JSON vào) để về trạng thái rỗng ban đầu, giữ lại `GameData.meta` như backup
4. Copy đè các file trong `Backups/.../Scripts/**` về đúng vị trí gốc tương ứng trong `Assets/_Game/Scripts/`
5. Mở Unity, chờ compile lại, verify Console 0 lỗi

## Status
Backup hoàn tất, không có lỗi. Tiếp tục S6-001 — Data Integration.
