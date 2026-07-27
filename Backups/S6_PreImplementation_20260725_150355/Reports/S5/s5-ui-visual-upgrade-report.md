# S5 — UI Visual Upgrade Report (Batch 3)

**Ngày:** 2026-07-24 · **Phase:** S5 Batch 3 — UI Visual Upgrade + minimal runtime UI wiring
**Trạng thái:** ⏸️ **CODE COMPLETE — RUN/VERIFY PENDING** (Unity main thread không phản hồi MCP; cần focus Editor)
**Được user cho phép:** viết runtime UI wiring tối thiểu để S5 playable (không full S6, không fake).

---

## UI Screens Updated
| Screen | Visual Change | Asset Used | Function Preserved | Status |
|---|---|---|---|---|
| HUD | Thêm **coin/gem icon** cạnh money/gems text | `Icons/coin`, `Icons/gem_v1` | ✅ money/gems vẫn từ SaveService | CODE_DONE |
| HUD nav buttons (6) | Thêm **icon pack** bên trái mỗi nút | `Icons/` (sword/helmet/potion/blunt/coin/gem) | ✅ onClick giữ nguyên | CODE_DONE |
| Inventory | Thêm **Btn_Back** (về HUD) | — | ✅ text list read-only giữ nguyên | CODE_DONE |
| Character | Thêm **Btn_Back** (về HUD) | — | ✅ text list giữ nguyên | CODE_DONE |
| Popup/Deferred | Không đổi (đã có title/msg/OK) | — | ✅ | NOT_TOUCHED |
| **Runtime wiring** | **UIRuntimeBootstrap** mới: tạo UIService, đăng ký screen, Initialize HUD/Inventory/Character/Popup, wire Back, ShowScreen(MainHUD) | — | ✅ chỉ glue, không gameplay | CODE_DONE |

## Placeholder UI
| Screen | Placeholder Style | Reason |
|---|---|---|
| Dungeon | Panel trắng (alpha 0.12) + title + message + Back | UI thật chưa có; không fake dungeon/combat |
| Craft | Panel trắng + title + message + Back | Không fake craft/claim |
| Merchant | Panel trắng + title + message + Back | Không fake price/restock |
| Settings | Panel trắng + title + message + Back | UI settings thật chưa có |

→ 4 placeholder là **UIScreen thật** (đăng ký với UIService, mở/đóng được), hiển thị message rõ ràng, có Back về HUD. Không chặn game, không fake chức năng.

## Asset Usage
| Asset Group | Used Where |
|---|---|
| `Icons/coin`, `gem_v1` | HUD currency icon + nav Merchant/Settings |
| `Icons/icons_swords_0`, `helmet`, `potion_red`, `icons_blunt_0` | nav button Dungeon/Character/Inventory/Craft |
| `AssetCatalog.asset` | nguồn trung tâm cho toàn bộ icon trên (build bằng menu) |
| Enemy/portrait/category trong catalog | đã map sẵn (hạ tầng), chưa hiển thị UI ở batch này |

## Risk / Manual Review
| Issue | Impact | Recommendation |
|---|---|---|
| Compile lần sửa generator cuối chưa xác nhận | Có thể còn lỗi cú pháp chưa thấy | Chạy recompile qua MCP khi Unity rảnh (Editor.log hiện 0 lỗi CS) |
| Chưa chạy Build Asset Catalog | Nav/currency icon chưa có sprite thật cho tới khi build | Chạy `GuildMaster/Assets/Build Asset Catalog` |
| Chưa Generate+Wire scene | Main scene chưa có UI mới | Chạy `Generate UI Foundation` → `Wire UI Scene` |
| Inventory/Character rỗng | Không có game data | Đúng thực trạng, không fake data |
| UIRuntimeBootstrap tự build DB/service | Nếu DatabaseBuilder lỗi → log error, UI không hiện (try/catch, không crash) | Verify khi play mode |

## Verification
- **Compile:** ⏸️ 3 script mới đã được Unity import (`.meta` OK), `Editor.log` hiện **0 lỗi CS**; lần sửa `UIWiringGenerator` cuối **chưa confirm compile** (Unity đang bận).
- **UI not broken / functions preserved:** ✅ chỉ thêm thành phần hình ảnh + glue; không sửa logic service/định nghĩa.
- **Mobile 1080×1920:** ✅ giữ CanvasScaler + SafeArea (không đổi); icon/nút dùng kích thước lớn hợp cảm ứng.
- **No fake logic / No Higgsfield / No generated assets:** ✅

---

## Batch 3 Result: ⏸️ **CODE COMPLETE — cần Unity focus để run + verify**
Sau khi bạn focus Unity, mình sẽ chạy: recompile → Build Asset Catalog → Generate UI Foundation → Wire UI Scene → play mode + screenshot, rồi hoàn tất Batch 4 (Final Review) và báo cáo tổng hợp cuối.
