# S6-007 Bug Fixing Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/S6_005_008_PreImplementation_20260725_172659/` (đã bao gồm `Runtime/Boot/UIRuntimeBootstrap.cs` trước khi sửa)
**Phạm vi:** Chỉ fix bug có ID trong Bug List của S6-005/S6-006. Không làm feature mới.

## Bugs Addressed
| Bug ID | Severity | Fix | Files Changed | Verification |
|---|---|---|---|---|
| **P-02** | Moderate | `UIRuntimeBootstrap` trước đây gọi `new DatabaseBuilder(...).Build();` và **vứt bỏ kết quả** → data load fail sẽ im lặng hoàn toàn. Nay bọc qua `LogDatabaseBuild(report, provider)`: log tên provider + số file + tổng record khi thành công; `Debug.LogError` kèm danh sách lỗi khi `report.hasFatalErrors`; `Debug.LogWarning` nếu có `recordCountMismatches` | `Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs` (1 dòng đổi + 1 method `private static` mới) | Xem mục Verification bên dưới |

**Chi tiết fix (tối thiểu, không đổi gameplay):**
- Dòng 46: `new DatabaseBuilder(provider, serializer, db).Build();` → `LogDatabaseBuild(new DatabaseBuilder(provider, serializer, db).Build(), provider);`
- Thêm method `private static void LogDatabaseBuild(DatabaseBuildReport report, IGameDataProvider provider)` — **thuần logging**, không thay đổi luồng khởi tạo, không throw, không chặn boot, không đụng vào bất kỳ service/data/gameplay rule nào.
- **Không thêm using mới:** `GuildMaster.Database` (chứa `DatabaseBuildReport`) và `GuildMaster.Infrastructure.DataProviders` (chứa `IGameDataProvider`) đều đã có sẵn trong file từ trước.

**Vì sao chọn "log" chứ không phải "halt boot khi lỗi":** Nếu ném exception/dừng boot khi database rỗng, sẽ làm **hỏng trạng thái hiện tại** — hiện tại UI được thiết kế để hiển thị đúng khi database/save rỗng (Inventory "empty", Character "No characters available."). Halt boot sẽ là thay đổi hành vi, vượt scope "fix nhỏ nhất có thể". Log là đủ để chẩn đoán mà không rủi ro.

## Bugs Deferred
| Bug ID | Reason | Safe Behavior |
|---|---|---|
| **P-01** (Critical cho S7) | Android StreamingAssets throw `NotSupportedException`. Fix đúng cách cần chuyển `IGameDataProvider` sang **async** (`UnityWebRequest`) — là thay đổi kiến trúc, không phải fix nhỏ. Hard rule cấm rõ "Không tự ý làm S7 Android Build/APK trong prompt này" | Windows Standalone **không bị ảnh hưởng** (dùng nhánh `File.ReadAllText` bình thường). Với fix P-02 vừa áp, nếu sau này build Android chạy thật, log sẽ **hiện rõ lỗi database build FAILED** thay vì im lặng — giúp chẩn đoán ngay |
| **P-03** | 2 class `Bootstrapper` dead code cùng tên. Hard rule cấm rewrite architecture; cả hai đều không gắn scene nên **không chạy, không tốn runtime** | Không hoạt động, không gây hại. Chỉ là rủi ro nhầm lẫn cho người sửa sau — đã ghi rõ trong report |
| **P-04** | `localization.json`/`assets_manifest.json` rỗng `[]` trong StreamingAssets, không nằm trong `manifest.json` | Không bao giờ được load (DatabaseBuilder chỉ đọc file liệt kê trong manifest). Tốn 4 bytes. Nguồn decode thật cũng rỗng → **không tự bịa nội dung** |
| **P-05** | Memory risk Android (416 sprite Compression None, Art 97 MB). Cần đo thật trên thiết bị; hard rule cấm "tối ưu texture bằng cách nén/resize bừa" | Import settings S5 giữ nguyên 100%, visual không đổi |
| **P-06** | Unity Hub chưa cài Android Build Support module | Thông tin môi trường, không phải bug code |

## Verification
| Check | Result | Evidence |
|---|---|---|
| Cú pháp/kiểu dữ liệu | ✅ Đã rà soát | `DatabaseBuildReport` có đủ field dùng trong method (`hasFatalErrors`, `manifestLoaded`, `loadedFiles`, `expectedFiles`, `errors`, `loadedRecordsByCategory`, `recordCountMismatches`) — đối chiếu trực tiếp `Database/DatabaseBuildReport.cs`. `IGameDataProvider.ProviderName` tồn tại — đối chiếu `IGameDataProvider.cs` |
| Using statements | ✅ Đủ, không cần thêm | `GuildMaster.Database` (dòng 4), `GuildMaster.Infrastructure.DataProviders` (dòng 5) đã có sẵn |
| Không đổi gameplay | ✅ | Method chỉ gọi `Debug.Log/LogWarning/LogError`, không mutate state, không throw |
| Không phá S5 UI | ✅ | Không đụng scene, không đụng UI script, luồng wiring giữ nguyên 100% |
| **Compile thật bởi Unity** | ⚠️ **Chưa verify** | Không có MCP Unity trong phiên này. Rủi ro thấp (thay đổi cơ học, đã đối chiếu từng field/property với source thật), nhưng chưa có bằng chứng compile từ chính Unity cho thay đổi này |

## Decision
# `S6_007_BUG_FIXING_DONE`

**Lý do:** Issue Moderate duy nhất nằm trong scope S6 (**P-02**) đã được fix bằng thay đổi tối thiểu, thuần logging, không đổi gameplay, không phá S5 UI. Các bug còn lại đều được defer có lý do rõ ràng và đều có safe behavior đã xác nhận (không gây hại ở trạng thái hiện tại). Cần 1 lần Unity reimport + Play test để xác nhận compile — xem "Manual Test Needed" trong Sprint Review.
