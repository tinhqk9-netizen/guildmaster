# S6.5A Decode Logic Coverage Audit — Backup Report

**Timestamp:** 2026-07-25 18:56:38
**Backup folder:** `D:\Tinh\Rebuild_GuildMaster\Backups\S6_5A_Decode_Logic_Coverage_Audit_20260725_185638\`
**Reason:** Chuẩn bị audit đối chiếu logic decode ↔ Unity. Phase này **chỉ đọc/audit, không sửa code/gameplay/data/scene** — backup theo đúng rule bắt buộc để đảm bảo có điểm hoàn tác nếu cần.

## Files Backed Up (208 file, 2.4 MB)
| # | Nhóm | Backup Path |
|---|---|---|
| 1 | `Reports/S6/**` (13 report) | `.../Reports/S6/` |
| 2 | `Reports/S6_5/**` (9 report) | `.../Reports/S6_5/` |
| 3 | `Assets/_Game/Scripts/Runtime/**` (toàn bộ: Assets, Boot, Core, Formulas, Models, Save, Services, UI) | `.../Assets/_Game/Scripts/Runtime/` |
| 4 | `Assets/_Game/Scripts/Editor/**` (toàn bộ editor tooling) | `.../Assets/_Game/Scripts/Editor/` |
| 5 | `Assets/_Game/Scenes/*.unity` + `.meta` (4 scene) | `.../Assets/_Game/Scenes/` |
| 6 | `Assets/StreamingAssets/GameData/**` (13 JSON + meta, 1531 record) | `.../Assets/StreamingAssets/GameData/` |
| 7 | `ProjectSettings/EditorBuildSettings.asset` | `.../ProjectSettings/` |

**Không backup:** decode source (`D:\Tinh\Guild Master - Idle Dungeons`) và converter (`D:\Tinh\Game Decode Converter`) — chỉ đọc, tuyệt đối không sửa theo hard rule "không sửa source decode".

## Restore Instruction
1. Copy đè `.../Assets/_Game/Scripts/Runtime/*` → `Assets/_Game/Scripts/Runtime/`
2. Copy đè `.../Assets/_Game/Scripts/Editor/*` → `Assets/_Game/Scripts/Editor/`
3. Copy đè `.../Assets/_Game/Scenes/*` → `Assets/_Game/Scenes/`
4. Copy đè `.../Assets/StreamingAssets/GameData/*` → `Assets/StreamingAssets/GameData/`
5. Copy đè `.../ProjectSettings/EditorBuildSettings.asset` → `ProjectSettings/`
6. Copy đè `.../Reports/*` → `Reports/` nếu cần khôi phục báo cáo
7. Mở Unity, chờ reimport, verify Console 0 lỗi

## Status
Backup hoàn tất, không lỗi. Decode path đã xác nhận truy cập được (`sources`, `resources`, `Document`). Tiếp tục Task 1.
