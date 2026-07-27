# S6-004 Save Integration Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/S6_003_004_PreImplementation_20260725_153634/`

## Save Flow Before
- **Load status:** Hoạt động — `SaveService.Load()` được gọi trong `UIRuntimeBootstrap.Start()`, fallback về `SaveData` rỗng nếu chưa có `save.json`.
- **Save call status:** **KHÔNG** — `grep "\.Save(out"` trên toàn bộ `Assets/_Game/Scripts` (trừ Tests) trước khi sửa → 0 kết quả. Không có `OnApplicationQuit`/`OnApplicationPause` nào gọi Save. Chưa từng có `save.json` sinh ra trên máy (đã kiểm tra `AppData/LocalLow/.../save.json` → không tồn tại).

## Save Flow After
| Trigger | Method | Status | Notes |
|---|---|---|---|
| App tạm dừng (mobile background) | `UIRuntimeBootstrap.OnApplicationPause(true)` → `PersistSave("OnApplicationPause")` → `_save.Save(out error)` | Đã thêm | Chỉ save khi `pauseStatus == true` (vào background), không save khi quay lại foreground |
| App thoát / Stop Play trong Editor | `UIRuntimeBootstrap.OnApplicationQuit()` → `PersistSave("OnApplicationQuit")` | Đã thêm | Theo tài liệu Unity, `OnApplicationQuit` cũng được gọi khi user bấm **Stop** trong Play Mode ở Editor — dùng được để verify ngay mà không cần build thật |
| Thủ công (nút Save trong Settings) | Không thêm | Không làm | Settings hiện là placeholder trắng theo đúng S5, không có nút nào — không tự thêm UI mới ngoài scope "cải thiện integration an toàn" |
| Auto-save theo chu kỳ | Không thêm | Không làm | User yêu cầu rõ "không tạo autosave quá dày" — chỉ dùng 2 trigger tự nhiên (pause/quit), không thêm timer |

**Implementation:** Mở rộng `UIRuntimeBootstrap.cs` (không tạo class/API save mới):
- Thêm field `private ISaveService _save;`, gán `_save = save;` ngay sau khi `SaveService` được tạo trong `Start()`.
- Thêm `PersistSave(string reason)` — gọi đúng `ISaveService.Save(out Exception error)` sẵn có, log kết quả (thành công/lỗi), không tạo format save mới, không đổi `SaveData` schema.
- Thêm `OnApplicationPause(bool)` và `OnApplicationQuit()` gọi `PersistSave(...)`.

**Không fake:** Không có chỗ nào tự cộng tiền/item/character để "test có gì đó trong save". Nếu save trống, `save.json` sinh ra sẽ chỉ chứa `Metadata` + toàn bộ field mặc định (Money=0, Gems=0, các List rỗng) — đúng như audit đã ghi nhận là hành vi đúng.

## Save File Verification
| File | Exists | Valid JSON | Notes |
|---|---|---|---|
| `C:\Users\gifft\AppData\LocalLow\DefaultCompany\Rebuild_GuildMaster\save.json` | ✅ (942 bytes, tạo lúc 2026-07-25 16:12) | ✅ Parse thành công bằng `json.load` | Đúng đủ toàn bộ field theo `SaveData` schema: `Metadata` (SaveVersion=1, SaveTimeUnix, GameVersion="1.0"), `Money=0`, `Gems=0`, `Items/Characters/Quests/Dungeons/Skills` đều `[]`, `ActiveDungeon` object rỗng đúng cấu trúc — **đúng như audit dự đoán, không có dữ liệu giả nào bị chèn vào** |
| Editor.log evidence | ✅ | — | `[UIRuntimeBootstrap] Save written (OnApplicationQuit).` xuất hiện đúng 1 lần khớp với thao tác Stop Play của user; stack trace dẫn đúng `PersistSave` → `OnApplicationQuit` tại `UIRuntimeBootstrap.cs:103`/`:118` |
| Compile | ✅ 0 lỗi CS | — | `grep "error CS"` toàn log → 0 kết quả |
| Exception | ✅ 0 | — | `grep "Exception"` toàn log → 0 kết quả |

## Scope Check
- **No fake data:** Đúng — không có dòng code nào gán giá trị Money/Gems/Item/Character mẫu.
- **No fake rewards/items/currency:** Đúng — không đụng vào bất kỳ gameplay action nào.
- **No gameplay logic added:** Đúng — `PersistSave` chỉ gọi `Save()` đã có sẵn trong `SaveService`, không thêm rule tính toán nào.
- **Không đổi SaveData schema:** Đúng — dùng nguyên `ISaveService.Save(out Exception)` đã tồn tại từ S2/S3.

## Decision
# `S6_004_SAVE_INTEGRATION_DONE`

**Lý do:** User đã Play rồi Stop; `save.json` được sinh ra đúng vị trí, đúng schema, JSON hợp lệ, không có dữ liệu giả. Editor.log xác nhận `Save written (OnApplicationQuit)` đúng 1 lần, 0 lỗi compile, 0 exception.
