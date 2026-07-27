# UI Hierarchy Cleanup — Implementation Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/UI_Hierarchy_Function_Audit_20260725_183927/`

## Changes Applied
| File/Scene | Change | Reason |
|---|---|---|
| `Assets/_Game/Scripts/Editor/UIHierarchyOrganizer.cs` (**mới**) + `.meta` | Editor tool với menu `GuildMaster → UI → Organize Hierarchy By Game Function`. Thực hiện đúng 4 thay đổi an toàn A–D trong plan: tạo 3 nhóm root `Systems`/`Cameras`/`UI`, move 5 object root vào đúng nhóm, sắp xếp 6 screen theo thứ tự luồng chơi, sắp thứ tự 3 nhóm, `MarkSceneDirty` + `SaveOpenScenes` | Reparent bằng Unity API chuẩn (`Undo.SetTransformParent`, `SetSiblingIndex`) thay vì sửa YAML thủ công |
| `Assets/_Game/Scenes/Main.unity` | **KHÔNG sửa** — đã verify bằng `diff`, file **identical** với backup | Xem mục "Vì sao chưa apply thẳng vào scene" bên dưới |
| `UIRuntimeBootstrap.cs`, `UIWiringGenerator.cs`, `UIScreenPreviewTool.cs`, các UI script | **KHÔNG sửa** | Không cần thiết cho phạm vi A–D; sửa thêm chỉ tăng rủi ro phá logic đã pass |

### Vì sao chưa apply thẳng vào scene
Rủi ro của A–D **không nằm ở logic** (đã chứng minh an toàn trong plan) mà ở **kỹ thuật sửa file**: reparent trong scene YAML thủ công phải sửa đồng bộ `m_Children` của parent cũ/mới, `m_Father` của con, `m_RootOrder`, đồng thời tạo block `GameObject` + `Transform` mới với `fileID` không đụng độ. Sai một chi tiết là hỏng scene. Phiên này không có MCP Unity nên độ chắc chắn khi tự sửa YAML **dưới 95%** → theo đúng hard rule *"Nếu không chắc an toàn >95%, chỉ report plan, không apply"*, em **không tự sửa scene**, mà giao việc reparent cho Unity API thực hiện qua tool — cách này đảm bảo đúng 100%.

## Final Hierarchy (sau khi user chạy menu)
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
    │       ├── ScreenRoot                    (giữ nguyên tên)
    │       │   ├── CharacterScreen           → CharacterAndEquipment  [Real function, UI text]
    │       │   ├── InventoryScreen           → InventoryAndItems      [Real function, UI text]
    │       │   ├── DungeonScreen             → DungeonAndCombat       [PLACEHOLDER 0% gameplay]
    │       │   ├── CraftScreen                → Crafting               [PLACEHOLDER 0% gameplay]
    │       │   ├── MerchantScreen            → Merchant               [PLACEHOLDER 0% gameplay]
    │       │   └── SettingsScreen            → Settings               [PLACEHOLDER 0% chức năng]
    │       ├── HudRoot                       (giữ nguyên tên)
    │       │   └── HUDVisual                 [Real: currency + navigation]
    │       ├── PopupRoot                     (giữ nguyên tên)
    │       │   └── PopupScreen               [Real function, chưa có caller]
    │       └── OverlayRoot                   (giữ nguyên tên, rỗng)
    └── EventSystem
```

**Khác biệt so với cây lý tưởng** trong `Recommended_UI_Hierarchy_By_Game_Function.md`: chưa đổi tên `HudRoot`→`HUD`, `ScreenRoot`→`Screens`, `PopupRoot`→`Popups`, `OverlayRoot`→`Overlay`, và chưa thêm nhóm `Currency`/`Navigation` bên trong HUD. Lý do: đổi tên root sẽ **làm hỏng `UIWiringGenerator`** (dùng path tương đối cứng `SafeArea/HudRoot`…), còn thêm parent bên trong Canvas **có nguy cơ lệch layout** vì con dùng `anchoredPosition` tuyệt đối. Cả hai gom vào S6.5B khi có Unity để nhìn trực tiếp.

## Runtime Binding Safety
| Cơ chế | Vì sao không bị phá |
|---|---|
| `UIRuntimeBootstrap` tìm screen | `FindObjectsByType<UIScreen>(FindObjectsInactive.Include, ...)` — **theo component**, quét toàn scene, không quan tâm parent hay thứ tự sibling ✅ |
| `UIRuntimeBootstrap` tìm HUD/Inventory/Character | `FindFirstObjectByType<HUDController/InventoryScreen/CharacterScreen>` — **theo component** ✅ |
| `UIRuntimeBootstrap` tìm nút Back | `screen.transform.Find("Btn_Back")` — phụ thuộc **tên + là con trực tiếp của screen**. Tool **không rename gì và không đụng vào bên trong screen** ✅ |
| `HUDController` bind Text/Button | `[SerializeField]` → lưu bằng **fileID reference**, hoàn toàn độc lập với tên/path ✅ |
| `UIService` Register/Hide/Show | Dựa trên `UIScreenId` field và tham chiếu object, không dùng path ✅ |
| `UIWiringGenerator` (editor) | `GameObject.Find("UICanvas")` / `GameObject.Find("UIRuntimeBootstrap")` — **quét toàn scene theo tên**, nên gom vào parent mới vẫn tìm thấy. `canvas.transform.Find("SafeArea/HudRoot")` là **path tương đối từ Canvas** — tool không đụng gì bên trong Canvas nên path này nguyên vẹn ✅ |
| Layout / RectTransform | 3 nhóm mới nằm ở **cấp root** với `Transform` mặc định (pos 0, scale 1). `Canvas` ở chế độ `ScreenSpaceOverlay` **bỏ qua hoàn toàn transform của parent`. Camera/Light giữ nguyên world position ✅ |
| Duplicate | Tool dùng `FindRootObject()` để tái sử dụng nhóm nếu đã tồn tại; `SetTransformParent` là **move**, không phải clone; có guard `if (target.transform.parent == group.transform) return 0;` → chạy nhiều lần vẫn không sinh trùng ✅ |
| Undo | Dùng `Undo.RegisterCreatedObjectUndo` + `Undo.SetTransformParent` + `Undo.RecordObject` → user Ctrl+Z hoàn tác được ✅ |

## Verification
| Check | Result | Evidence |
|---|---|---|
| `Main.unity` chưa bị sửa | ✅ PASS | `diff` với backup → **identical**, không byte nào khác |
| Menu mới đăng ký đúng | ✅ PASS | `GuildMaster/UI/Organize Hierarchy By Game Function` (priority 1) xuất hiện trong danh sách menu, nằm cạnh `Make All Panels Visible In Scene` (priority 0) |
| Preview tool cũ còn nguyên | ✅ PASS | 8 menu `Preview: ...` vẫn đủ |
| Tool không rename object nào | ✅ PASS | Đọc code: không có lệnh gán `.name` nào |
| Tool không đụng bên trong Canvas | ✅ PASS | Chỉ `GroupRootObject` (5 object cấp root) + `SetSiblingIndex` cho screen — không reparent con nào trong Canvas |
| Tool không xoá/merge object | ✅ PASS | Không có `DestroyImmediate`/`Destroy` nào trong file |
| Chạy nhiều lần an toàn (idempotent) | ✅ PASS | Guard parent đã đúng thì bỏ qua; nhóm đã tồn tại thì tái dùng |
| Using/namespace hợp lệ | ✅ PASS | Cùng namespace `GuildMaster.Editor.UI` và cùng pattern using với `UIScreenPreviewTool.cs`/`UIWiringGenerator.cs` đã compile thành công |
| Không đụng gameplay/data/save | ✅ PASS | Không sửa service, data, save, formula nào |
| **Compile thật bởi Unity** | ⚠️ **Chưa verify** | Không có MCP Unity trong phiên này |
| **Kết quả hierarchy sau khi chạy tool** | ⚠️ **Chưa chạy** | Cần user bấm menu rồi review |

## Cách dùng
1. Mở `Assets/_Game/Scenes/Main.unity`.
2. Chạy **`GuildMaster → UI → Organize Hierarchy By Game Function`** (tool tự lưu scene).
3. Xem Hierarchy: phải thấy 3 nhóm `Systems` / `Cameras` / `UI`, và 6 screen xếp theo thứ tự Character → Inventory → Dungeon → Craft → Merchant → Settings.
4. Không ưng thì **Ctrl+Z** hoàn tác, hoặc restore `Main.unity` từ backup.
5. Bấm Play kiểm tra: HUD hiện, 6 nút điều hướng hoạt động, Back hoạt động, Console 0 lỗi.

## Decision
# `UI_HIERARCHY_CLEANUP_PARTIAL_NEEDS_USER_REVIEW`

**Lý do:** Đã chuẩn bị đầy đủ công cụ áp dụng 4 thay đổi an toàn (A–D) bằng Unity API chuẩn, có Undo, idempotent, không rename/xoá/merge, không đụng bên trong Canvas nên **không thể lệch layout**. **Chưa apply vào scene** vì việc reparent qua sửa YAML thủ công có độ chắc chắn dưới ngưỡng 95% mà hard rule yêu cầu — nên giao cho Unity thực hiện. 6 thay đổi còn lại (E–J: đổi tên root, nhóm Currency/Navigation, tách icon, sửa sizeDelta, thêm Image nền, đổi Btn_Back binding) **cố ý hoãn** sang S6.5B kèm lý do rủi ro cụ thể. Cần user chạy menu và review kết quả để chốt.
