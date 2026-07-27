# UI Hierarchy Cleanup Plan

**Ngày:** 2026-07-25 · **Backup:** `Backups/UI_Hierarchy_Function_Audit_20260725_183927/`

## Ràng buộc binding — kết quả rà soát toàn bộ code

### Runtime (`UIRuntimeBootstrap.cs`) — chạy mỗi lần Play
| Dòng | Cách tìm | Phụ thuộc |
|---|---|---|
| 61 | `FindObjectsByType<UIScreen>(FindObjectsInactive.Include, ...)` | **COMPONENT** — không phụ thuộc tên/path ✅ |
| 76 | `FindFirstObjectByType<HUDController>` | **COMPONENT** ✅ |
| 79 | `FindFirstObjectByType<InventoryScreen>` | **COMPONENT** ✅ |
| 82 | `FindFirstObjectByType<CharacterScreen>` | **COMPONENT** ✅ |
| 155 | `screen.transform.Find("Btn_Back")` | ⚠️ **TÊN `Btn_Back` + phải là con TRỰC TIẾP của screen** |

`HUDController` bind `Text`/`Button` qua `[SerializeField]` → lưu bằng **fileID reference**, không phụ thuộc tên hay path ✅

### Editor (`UIWiringGenerator.cs`) — chỉ chạy khi user bấm menu `Wire UI Scene`
| Dòng | Cách tìm | Phụ thuộc |
|---|---|---|
| 23 | `GameObject.Find("UICanvas")` | ⚠️ tên `UICanvas` (quét toàn scene — **không** phụ thuộc parent) |
| 39 | `canvas.transform.Find("SafeArea/HudRoot")` | ⚠️ **path tương đối từ UICanvas** |
| 40 | `canvas.transform.Find("SafeArea/ScreenRoot")` | ⚠️ path tương đối |
| 41 | `canvas.transform.Find("SafeArea/PopupRoot")` | ⚠️ path tương đối |
| 138 | `GameObject.Find("UIRuntimeBootstrap")` | ⚠️ tên (quét toàn scene) |

**Hệ quả then chốt:** vì `GameObject.Find()` quét **toàn scene theo tên** chứ không theo path, nên **gom object vào parent mới ở cấp root là an toàn** — generator vẫn tìm thấy. Ngược lại, **đổi tên `HudRoot`/`ScreenRoot`/`PopupRoot` sẽ làm hỏng generator**, vì nó dùng path tương đối cứng.

## Trả lời 6 câu hỏi bắt buộc

**1. Có thể rename/move object nào an toàn?**
- ✅ **Move an toàn:** `Main Camera`, `Global Light 2D` → nhóm `Cameras`; `UIRuntimeBootstrap` → nhóm `Systems`; `UICanvas`, `EventSystem` → nhóm `UI`. Cả 5 object đều được tìm bằng tên toàn scene hoặc tag/singleton, không phụ thuộc parent.
- ✅ **Move an toàn:** 6 screen bên trong `ScreenRoot` (đổi thứ tự sibling) — runtime tìm bằng component.
- ⚠️ **Rename:** không rename object nào ở giai đoạn này (xem câu 2).

**2. Object nào KHÔNG nên đổi tên vì runtime/editor tìm theo tên?**
| Object | Ai tìm | Hậu quả nếu đổi tên |
|---|---|---|
| `Btn_Back` (×6) | **Runtime** `UIRuntimeBootstrap:155` | **Nút Back chết ngay** trong Play — nghiêm trọng nhất |
| `UICanvas` | Editor generator | `Wire UI Scene` báo lỗi không tìm thấy canvas |
| `HudRoot`, `ScreenRoot`, `PopupRoot` | Editor generator (path tương đối) | Generator không tìm thấy root → tạo lại/lỗi |
| `UIRuntimeBootstrap` | Editor generator | Generator không gắn được component |

→ **Kết luận: KHÔNG đổi tên bất kỳ object nào.** Muốn hierarchy đọc theo chức năng thì dùng **parent group mới**, không đụng tên object cũ. Nếu sau này thực sự muốn đổi tên root, phải sửa `UIWiringGenerator` **trước**, và đó là việc riêng cần verify lại.

**3. Object nào có thể gom vào parent mới?**
- `Systems` ← `UIRuntimeBootstrap`
- `Cameras` ← `Main Camera`, `Global Light 2D`
- `UI` ← `UICanvas`, `EventSystem`
- (Tùy chọn, rủi ro cao hơn) `Currency` / `Navigation` bên trong `HUDVisual` — **hoãn**, xem câu 4.

**4. Thêm parent mới có làm đổi RectTransform/layout không?**
| Trường hợp | Ảnh hưởng layout |
|---|---|
| Parent mới ở **cấp root** (`Systems`, `Cameras`, `UI`) là empty GameObject với `Transform` mặc định (pos 0, rot 0, scale 1) | ❌ **Không đổi gì.** `Canvas` ở chế độ `ScreenSpaceOverlay` **bỏ qua hoàn toàn transform của parent** — Unity tự render theo screen space. Camera/Light cũng giữ nguyên world position |
| Parent mới **bên trong Canvas** (VD `Currency`, `Navigation` trong `HUDVisual`) | ⚠️ **CÓ THỂ đổi.** Con đang dùng anchor center + `anchoredPosition` tuyệt đối; chèn parent mới sẽ đổi hệ quy chiếu trừ khi parent đó được set stretch full (anchor 0,0–1,1, offset 0). **Rủi ro lệch layout → hoãn sang S6.5B khi làm visual** |

**5. Có cần sửa `UIRuntimeBootstrap`/`UIWiringGenerator` để find theo component thay vì path/name không?**
- `UIRuntimeBootstrap`: **gần như đã đúng rồi** — 4/5 chỗ dùng component. Chỉ còn `transform.Find("Btn_Back")` dùng tên. **Đề xuất (chưa làm ngay):** đổi sang `GetComponentsInChildren<Button>()` lọc theo tên hoặc thêm field `[SerializeField] Button _backButton` trên `UIScreen`. Lợi ích: hết phụ thuộc tên. Nhưng đây là **sửa runtime đã pass** → cần verify lại → **hoãn**, không thuộc phạm vi task này.
- `UIWiringGenerator`: nếu muốn đổi tên root sau này thì bắt buộc sửa. Hiện **không đổi tên** nên **không cần sửa**.

**6. Rủi ro từng thay đổi** → xem bảng dưới.

## Bảng đề xuất thay đổi

| Proposed Change | Objects Affected | Benefit | Risk | Safe To Apply Now |
|---|---|---|---|---|
| **A. Thêm parent `Systems`** và move `UIRuntimeBootstrap` vào | 1 object move + 1 object mới | Tách rõ tầng hệ thống khỏi UI | **Rất thấp** — không có RectTransform, `GameObject.Find` quét toàn scene | ✅ **CÓ** |
| **B. Thêm parent `Cameras`** và move `Main Camera` + `Global Light 2D` | 2 object move + 1 mới | Gom render/lighting | **Rất thấp** — `Camera.main` tìm theo tag, không theo parent | ✅ **CÓ** |
| **C. Thêm parent `UI`** và move `UICanvas` + `EventSystem` | 2 object move + 1 mới | Gom toàn bộ tầng UI | **Thấp** — Canvas ScreenSpaceOverlay bỏ qua transform parent; `EventSystem.current` là singleton toàn cục; `GameObject.Find("UICanvas")` vẫn thấy | ✅ **CÓ** |
| **D. Sắp xếp lại thứ tự sibling** của 6 screen trong `ScreenRoot` theo nhóm chức năng (Character → Inventory → Dungeon → Craft → Merchant → Settings) | 6 screen | Hierarchy đọc theo luồng chơi | **Rất thấp** — runtime tìm theo component, thứ tự chỉ ảnh hưởng thứ tự vẽ (các screen vốn không hiện cùng lúc lúc Play) | ✅ **CÓ** |
| **E. Đổi tên `HudRoot`→`HUD`, `ScreenRoot`→`Screens`, `PopupRoot`→`Popups`, `OverlayRoot`→`Overlay`** | 4 root | Tên đọc rõ nghĩa hơn | **CAO** — làm hỏng `UIWiringGenerator` (path tương đối cứng ở dòng 39–41) | ❌ **KHÔNG** — cần sửa generator trước |
| **F. Thêm parent `Currency` + `Navigation`** trong `HUDVisual` | 4 + 6 object | Nhóm HUD theo chức năng | **TRUNG BÌNH–CAO** — con dùng `anchoredPosition` tuyệt đối, dễ lệch layout | ❌ **KHÔNG** — hoãn sang S6.5B |
| **G. Tách `MoneyIcon`/`GemsIcon` ra khỏi `MoneyText`/`GemsText`** thành sibling | 2 icon | Icon không bị ảnh hưởng khi chỉnh alignment Text | **TRUNG BÌNH** — đổi hệ quy chiếu vị trí icon | ❌ **KHÔNG** — hoãn sang S6.5B |
| **H. Sửa `sizeDelta (100,100)`** trên `HUDVisual`/`InventoryScreen`/`CharacterScreen`/`PopupScreen` thành stretch full | 4 container | Layout chuẩn, con không tràn khung | **TRUNG BÌNH** — đổi RectTransform, cần nhìn mắt để xác nhận | ❌ **KHÔNG** — hoãn sang S6.5B |
| **I. Thêm `Image` nền cho `InventoryScreen`/`CharacterScreen`** | 2 screen | Hết xuyên thấu xuống HUD | **THẤP–TRUNG BÌNH** — đổi visual | ❌ **KHÔNG** — thuộc S6.5B visual |
| **J. Đổi `Btn_Back` binding sang component thay vì tên** | `UIRuntimeBootstrap` + `UIScreen` | Hết phụ thuộc tên object | **TRUNG BÌNH** — sửa runtime đã pass, cần Play test lại | ❌ **KHÔNG** — hoãn |

## Phạm vi đề xuất áp dụng ngay: **A + B + C + D**

Kết quả hierarchy sau khi áp A–D:
```
Main.unity
├── Systems
│   └── UIRuntimeBootstrap
├── Cameras
│   ├── Main Camera
│   └── Global Light 2D
└── UI
    ├── UICanvas
    │   └── SafeArea
    │       ├── ScreenRoot          (giữ tên — generator phụ thuộc)
    │       │   ├── CharacterScreen     ← CharacterAndEquipment
    │       │   ├── InventoryScreen     ← InventoryAndItems
    │       │   ├── DungeonScreen       ← DungeonAndCombat  [PLACEHOLDER]
    │       │   ├── CraftScreen         ← Crafting          [PLACEHOLDER]
    │       │   ├── MerchantScreen      ← Merchant          [PLACEHOLDER]
    │       │   └── SettingsScreen      ← Settings          [PLACEHOLDER]
    │       ├── HudRoot             (giữ tên)
    │       │   └── HUDVisual
    │       ├── PopupRoot           (giữ tên)
    │       │   └── PopupScreen
    │       └── OverlayRoot         (giữ tên, rỗng)
    └── EventSystem
```

Không đạt 100% cây lý tưởng ở `Recommended_UI_Hierarchy_By_Game_Function.md` (thiếu đổi tên root + nhóm Currency/Navigation), nhưng **đạt được mục tiêu chính**: mở `Main.unity` là thấy 3 nhóm rõ ràng Systems / Cameras / UI, và 6 screen xếp theo đúng thứ tự luồng chơi. Phần còn lại (E, F, G, H, I) gom vào S6.5B khi làm visual — lúc đó có Unity nhìn trực tiếp để xác nhận layout không lệch.

## ⚠️ Ghi chú về cách thực thi
Rủi ro **không nằm ở logic** (A–D đều an toàn về mặt binding) mà nằm ở **kỹ thuật sửa file**: reparent trong scene YAML thủ công đòi hỏi sửa đồng thời `m_Children`, `m_Father`, `m_RootOrder` và tạo block `GameObject`+`Transform` mới với `fileID` không trùng — sai một chỗ là hỏng scene. Vì phiên này không có MCP Unity, **độ chắc chắn khi tự sửa YAML < 95%** → theo đúng hard rule, **không tự sửa YAML**. Thay vào đó cung cấp Editor script để Unity thực hiện reparent bằng API chuẩn (đảm bảo đúng 100%), user chạy 1 menu.

## Decision
# `UI_HIERARCHY_CLEANUP_PLAN_READY`

**Lý do:** Đã rà soát đầy đủ ràng buộc binding (runtime dùng component ở 4/5 chỗ; `Btn_Back` và 4 root name là điểm phụ thuộc tên duy nhất). Xác định được 4 thay đổi an toàn (A–D) và 6 thay đổi cần hoãn (E–J) kèm lý do rủi ro cụ thể. Kế hoạch sẵn sàng áp dụng qua Editor tool.
