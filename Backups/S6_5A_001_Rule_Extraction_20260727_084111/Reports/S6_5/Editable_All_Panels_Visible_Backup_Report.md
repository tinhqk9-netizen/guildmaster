# Editable All Panels Visible — Backup Report

**Timestamp:** 2026-07-25 18:28:32
**Backup folder:** `D:\Tinh\Rebuild_GuildMaster\Backups\Editable_All_Panels_Visible_20260725_182832\`
**Reason:** User yêu cầu bật sẵn toàn bộ panel/screen trong `Main.unity` để edit offline (chấp nhận panel chồng lên nhau, tự tắt trong Hierarchy khi cần). Cần sửa trực tiếp `Main.unity` + bổ sung menu mới vào `UIScreenPreviewTool.cs` → backup trước.

## Files Backed Up (13 file)
| # | File | Backup Path | Note |
|---|---|---|---|
| 1 | `Assets/_Game/Scenes/Main.unity` (+ `.meta`) | `.../Assets/_Game/Scenes/` | **Đối tượng sửa chính** — đổi `m_IsActive` của 7 screen |
| 2 | `Assets/_Game/Scenes/Boot.unity` (+ `.meta`) | `.../Assets/_Game/Scenes/` | Không sửa, backup phòng ngừa |
| 3 | `Assets/_Game/Scripts/Editor/UIScreenPreviewTool.cs` (+ `.meta`) | `.../Assets/_Game/Scripts/Editor/` | Bổ sung menu `Make All Panels Visible In Scene` |
| 4 | `Assets/_Game/Scripts/Editor/UIWiringGenerator.cs` (+ `.meta`) | `.../Assets/_Game/Scripts/Editor/` | Không sửa, backup phòng ngừa |
| 5 | `Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs` (+ `.meta`) | `.../Assets/_Game/Scripts/Runtime/Boot/` | Không sửa, backup phòng ngừa |
| 6 | `Reports/S6_5/*.md` (3 report) | `.../Reports/S6_5/` | Trạng thái báo cáo trước thay đổi |

## Restore Instruction
1. Copy đè `Backups/Editable_All_Panels_Visible_20260725_182832/Assets/_Game/Scenes/Main.unity` → `Assets/_Game/Scenes/Main.unity` (trả 7 screen về trạng thái tắt)
2. Copy đè `.../Assets/_Game/Scripts/Editor/UIScreenPreviewTool.cs` → vị trí gốc (gỡ menu mới)
3. Các file còn lại không bị sửa — chỉ restore nếu cần
4. Mở Unity, chờ reimport, verify Console 0 lỗi

**Cách restore nhanh không cần backup:** chạy `GuildMaster → UI → Preview: Reset (Hide All Screens)` rồi Ctrl+S — trả toàn bộ screen về trạng thái tắt như cũ.

## Status
Backup hoàn tất, không lỗi (13 file). Tiếp tục thực hiện thay đổi.
