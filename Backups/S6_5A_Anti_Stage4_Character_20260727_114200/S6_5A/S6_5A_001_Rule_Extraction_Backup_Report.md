# S6.5A-001 Rule Extraction — Backup Report

**Timestamp:** 2026-07-27 08:41:11
**Backup folder:** `D:\Tinh\Rebuild_GuildMaster\Backups\S6_5A_001_Rule_Extraction_20260727_084111\`
**Reason:** Phase S6.5A-001 chỉ đọc decode + viết tài liệu rule, **không implement code**. Backup theo đúng rule bắt buộc để có điểm hoàn tác cho toàn bộ report và source hiện tại.

## Files Backed Up (210 file, 2.4 MB)
| # | Nhóm | Backup Path |
|---|---|---|
| 1 | `Reports/S6_5A/**` (7 report từ phase audit) | `.../Reports/S6_5A/` |
| 2 | `Reports/S6_5/**` (9 report) | `.../Reports/S6_5/` |
| 3 | `Reports/S6/**` (13 report) | `.../Reports/S6/` |
| 4 | `Assets/_Game/Scripts/Runtime/**` (toàn bộ runtime code) | `.../Assets/_Game/Scripts/Runtime/` |
| 5 | `Assets/_Game/Scripts/Editor/**` (toàn bộ editor tooling) | `.../Assets/_Game/Scripts/Editor/` |
| 6 | `Assets/StreamingAssets/GameData/**` (13 JSON, 1531 record) | `.../Assets/StreamingAssets/GameData/` |
| 7 | `Assets/_Game/Scenes/*.unity` (4 scene) | `.../Assets/_Game/Scenes/` |

**Không backup:** decode source (`D:\Tinh\Guild Master - Idle Dungeons`) — chỉ đọc, tuyệt đối không sửa theo hard rule.

## Restore Instruction
1. Copy đè `.../Assets/_Game/Scripts/Runtime/*` → `Assets/_Game/Scripts/Runtime/`
2. Copy đè `.../Assets/_Game/Scripts/Editor/*` → `Assets/_Game/Scripts/Editor/`
3. Copy đè `.../Assets/StreamingAssets/GameData/*` → `Assets/StreamingAssets/GameData/`
4. Copy đè `.../Assets/_Game/Scenes/*.unity` → `Assets/_Game/Scenes/`
5. Copy đè `.../Reports/*` → `Reports/`
6. Mở Unity, chờ reimport, verify Console 0 lỗi

## Status
Backup hoàn tất, không lỗi. Decode source truy cập được. Tiếp tục Task 1–10.
