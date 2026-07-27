# S6-005/006/007/008 Pre-Implementation Backup Report

**Timestamp:** 2026-07-25 17:26:59
**Backup folder:** `D:\Tinh\Rebuild_GuildMaster\Backups\S6_005_008_PreImplementation_20260725_172659\`
**Reason:** Chuẩn bị chạy S6-005 (Performance/Build Readiness audit), S6-006 (Regression Testing), S6-007 (Bug Fixing), S6-008 (Sprint Review). Các task này có thể phải sửa import settings, script runtime/editor, hoặc fix bug phát hiện khi test — backup toàn bộ nhóm file có khả năng bị đụng trước khi bắt đầu.

## Files/Folders Backed Up (644 file, 5.9 MB)
| # | Nhóm | Backup Path | Note |
|---|---|---|---|
| 1 | `ProjectSettings/EditorBuildSettings.asset` | `.../ProjectSettings/` | Scene list đã fix ở S6-002 |
| 2 | `ProjectSettings/ProjectSettings.asset` | `.../ProjectSettings/` | Chứa companyName/productName/build config — có thể bị đụng khi audit build readiness |
| 3 | `Assets/_Game/Scenes/*.unity` + meta (4 scene) | `.../Assets/_Game/Scenes/` | Boot/Main thật + 2 bản rỗng trùng lặp |
| 4 | `Assets/StreamingAssets/GameData/*` (13 JSON + meta) | `.../Assets/StreamingAssets/` | Data 1531 record đã đổ vào ở S6-001 |
| 5 | `Assets/_Game/Scripts/Runtime/**` (toàn bộ: Boot, Core, Formulas, Models, Save, Services, UI, Assets) | `.../Assets/_Game/Scripts/Runtime/` | Nơi có khả năng phải fix bug nhất |
| 6 | `Assets/_Game/Scripts/Editor/**` | `.../Assets/_Game/Scripts/Editor/` | Editor tooling (AssetCatalogBuilder, UIWiringGenerator, UIFoundationGenerator, verifier...) |
| 7 | `Assets/_Game/Data/**` | `.../Assets/_Game/Data/` | `AssetCatalog.asset` + 2 thư mục rỗng |
| 8 | **`Assets/_Game/Art/**/*.meta` (450 file)** | `.../Assets/_Game/Art_meta_only/` (giữ nguyên cấu trúc thư mục) | **Chỉ backup `.meta`, KHÔNG copy file ảnh gốc** — lý do: thư mục Art nặng 97 MB, còn import settings (Point filter / Compression None / MipMap Off từ S5) nằm hoàn toàn trong `.meta`. Nếu S6-005 có đụng import setting thì restore `.meta` là đủ khôi phục, ảnh gốc không bao giờ bị sửa. |
| 9 | `Reports/S6/*.md` (8 report hiện có) | `.../Reports/S6/` | Trạng thái báo cáo trước S6-005 |

**Không backup:** file ảnh gốc trong `Assets/_Game/Art` (97 MB) — xem lý do ở dòng #8. `Library/`, `Temp/`, `obj/` (Unity tự sinh lại). `Tools/DecodeConverter/` (không đụng tới, và audit đã xác định là bản không dùng).

## Restore Instruction
1. Copy đè `Backups/S6_005_008_PreImplementation_20260725_172659/ProjectSettings/*` → `ProjectSettings/`
2. Copy đè `.../Assets/_Game/Scenes/*` → `Assets/_Game/Scenes/`
3. Copy đè `.../Assets/StreamingAssets/*` → `Assets/StreamingAssets/`
4. Copy đè `.../Assets/_Game/Scripts/Runtime/*` → `Assets/_Game/Scripts/Runtime/`
5. Copy đè `.../Assets/_Game/Scripts/Editor/*` → `Assets/_Game/Scripts/Editor/`
6. Copy đè `.../Assets/_Game/Data/*` → `Assets/_Game/Data/`
7. **Nếu import settings bị đổi:** copy đè toàn bộ cây `.../Assets/_Game/Art_meta_only/*` → `Assets/_Game/Art/` (cấu trúc thư mục đã khớp sẵn, chỉ ghi đè file `.meta`, không đụng file ảnh)
8. Mở Unity, chờ reimport + compile lại, verify Console 0 lỗi

## Status
Backup hoàn tất, không lỗi. Tiếp tục S6-005.
