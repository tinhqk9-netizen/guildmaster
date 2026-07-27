# S5 — Safe Asset Mapping Apply Report (Batch 2)

**Ngày:** 2026-07-24 · **Phase:** S5 Batch 2 — Apply Safe Asset Mapping
**Trạng thái:** ⏸️ **CODE COMPLETE — COMPILE/RUN PENDING** (Unity main thread không phản hồi qua MCP; cần focus Editor)

---

## Files Changed
| File | Purpose | Runtime/Editor |
|---|---|---|
| `Runtime/Assets/AssetCatalog.cs` | ScriptableObject: map hình ảnh pack → khái niệm game (currency, nav, category, enemy group, portrait). Chỉ chứa Sprite reference, **không có gameplay data** | Runtime |
| `Editor/AssetCatalogBuilder.cs` | Menu `GuildMaster/Assets/Build Asset Catalog` — populate catalog từ sprite đã import; skip + log nếu thiếu, không bịa reference | Editor |
| `Editor/UIWiringGenerator.cs` | Chèn **coin/gem icon** cạnh money/gems text + **icon pack lên 6 nav button**; guarded fallback nếu catalog null | Editor |

---

## Applied Mappings
| Target | Asset | Where Applied | Confidence | Status |
|---|---|---|---|---|
| HUD money icon | `Icons/coin` | `HUDVisual/MoneyIcon` cạnh MoneyText (giá trị tiền thật từ SaveService) | Cao | CODE_DONE |
| HUD gems icon | `Icons/gem_v1` | `HUDVisual/GemsIcon` cạnh GemsText | Cao | CODE_DONE |
| Nav Inventory | `Icons/potion_red` | icon trái nút Btn_Inventory | Cao | CODE_DONE |
| Nav Character | `Icons/helmet` | Btn_Character | Cao | CODE_DONE |
| Nav Dungeon | `Icons/icons_swords_0` | Btn_Dungeon | Cao | CODE_DONE |
| Nav Craft | `Icons/icons_blunt_0` | Btn_Craft | Cao | CODE_DONE |
| Nav Merchant | `Icons/coin` | Btn_Merchant | Cao | CODE_DONE |
| Nav Settings | `Icons/gem_v1` | Btn_Settings | Trung bình | CODE_DONE |
| Category/Enemy/Portrait catalog | icons + idle-down-00 + portraits | `AssetCatalog.asset` (chưa hiển thị UI ở batch này — hạ tầng dùng cho sau) | Cao | CODE_DONE |

> "CODE_DONE" = code đã viết; chưa chạy builder/generator vì Unity không phản hồi. Không có mapping nào bịa data.

## Placeholder Panels
| Screen/Feature | Placeholder Type | Reason |
|---|---|---|
| Dungeon / Craft / Merchant / Settings | Cơ chế Deferred sẵn có (`UIService` log "Deferred/Placeholder" + `PopupScreen.ShowDeferred`) | UI thật của các màn này thuộc phạm vi sau; S5 không fake chức năng |

## Deferred / Not Applied
| Target | Reason |
|---|---|
| Item slot / panel / bar chrome từ `ui_kit` | `ui_kit.png` chưa được cắt thành element (MANUAL_REVIEW) → dùng placeholder, không map ở batch này |
| Icon vào Inventory/Character list | 2 màn này là **text-only** và **không có game data** → hiển thị icon sẽ cần dữ liệu item/nhân vật thật (không được fake) |
| HP/MP/XP bar | Chưa có hệ HP runtime để bind |

## Verification
- **Compile:** ⏸️ PENDING — Unity chưa import 2 script mới (`.meta` chưa sinh), MCP `recompile_scripts`/`get_console_logs` đều timeout. Static self-review: namespace/asmdef/using hợp lệ, API dùng đều ổn định; kỳ vọng compile sạch nhưng **chưa xác nhận được**.
- **No fake gameplay:** ✅ Chỉ thêm Sprite/Image; money/gems vẫn lấy từ SaveService thật; không đụng price/reward/combat/quest.
- **No Higgsfield / No generated assets:** ✅
- **No source decode change / No Production JSON change:** ✅
- **Backend intact:** ✅ Không sửa file gameplay nào; chỉ thêm 1 SO runtime + 1 Editor script + sửa 1 Editor generator.

---

## 🔴 Phát hiện kiến trúc quan trọng (định hình lại scope S5)

Khi audit runtime, xác nhận: **UI hiện KHÔNG được wire ở runtime.**
- `HUDController.Initialize()`, `UIService.RegisterScreen()`, mọi `.Initialize(...)` **không được gọi ở bất kỳ đâu**.
- Cả 2 Bootstrapper (`Bootstrap/Bootstrapper.cs`, `Runtime/Boot/Bootstrapper.cs`) chỉ build backend (Database/Save/Formula/Factory) rồi **dừng** — không tạo `UIService`, không đăng ký screen, không chuyển sang Main scene.
- → Ở play mode hiện tại, nút HUD **chưa có listener**, navigation **chưa hoạt động runtime**.

**Kết luận scope:** Việc wire UIService + register screen + Initialize + scene transition chính là **Sprint S6 — System Integration (S6-002 Runtime Integration, S6-003 UI Integration)** theo MasterPlan. Prompt S5 **cấm bắt đầu sprint sau** và cấm thêm hệ thống mới. → **S5 chỉ làm phần asset/visual**: gắn sprite thật vào UI scaffolding do generator dựng + placeholder. "Navigation chạy runtime" **không thuộc S5** và mình **không tự ý viết** (tránh lấn S6 + tránh thay đổi kiến trúc chưa verify được).

Verify Batch 4 vì vậy sẽ ở mức: scene sinh ra có UICanvas/SafeArea/HUD với **icon asset thật hiển thị** (edit-mode), không phải click-navigation runtime.

---

## Batch 2 Result: ⏸️ **BLOCKED — NEEDS USER ACTION (focus Unity)**
Code hoàn tất và low-risk, nhưng **không thể compile-verify / chạy builder+generator** vì Unity main thread không tick (chưa focus). Cần bạn focus Unity 1 lần; sau đó mình chạy toàn bộ qua MCP và verify.
