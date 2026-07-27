# Current UI Hierarchy — Position & Function Report

**Ngày:** 2026-07-25 · **Backup:** `Backups/UI_Hierarchy_Function_Audit_20260725_183927/`
**Phương pháp:** Parse trực tiếp `Main.unity` (YAML) — trích RectTransform (anchorMin/Max, pivot, anchoredPosition, sizeDelta) và toàn bộ component của từng GameObject; resolve GUID script → tên class thật; đối chiếu với `UIRuntimeBootstrap.cs`, `UIService.cs`, các screen script và trạng thái service từ báo cáo S6.

**Ghi chú đọc bảng:** `Real Function` = có logic gameplay thật chạy phía sau. `Placeholder Visual` = chỉ là panel trống + chữ, **không có chức năng gameplay nào**, và **không được tính là đã xong**.

## 1. Root / Scene

| Object | Current Hierarchy Path | RectTransform Summary | Component/Script | Current Function | Data/Service | Real Function Or Placeholder | Recommended Game Group |
|---|---|---|---|---|---|---|---|
| Main Camera | `Main Camera` | (không có RectTransform) | `Camera`, `AudioListener`, URP `UniversalAdditionalCameraData` | Render scene, orthographic | — | **Real** | `Cameras` |
| Global Light 2D | `Global Light 2D` | (không có RectTransform) | `Light2D` (URP 2D) | Ánh sáng 2D toàn cục | — | **Real** | `Cameras` |
| UICanvas | `UICanvas` | anchor (0,0)-(0,0) · pivot (0,0) · pos (0,0) · size (0,0) | `Canvas` (ScreenSpaceOverlay), `CanvasScaler` (1080×1920, ScaleWithScreenSize, match 0.5), `GraphicRaycaster` | Gốc toàn bộ UI, scale theo màn hình dọc | — | **Real** | `UI` |
| SafeArea | `UICanvas/SafeArea` | anchor (0,0)-(1,1) · pivot (0.5,0.5) · pos (0,0) · size (0,0) — **stretch full** | `SafeArea` | Co UI tránh notch/tai thỏ thiết bị | — | **Real** | `UI` |
| EventSystem | `EventSystem` | (không có RectTransform) | `EventSystem`, `InputSystemUIInputModule` | Xử lý input chuột/chạm cho UI | Input System package | **Real** | `UI` |
| UIRuntimeBootstrap | `UIRuntimeBootstrap` | (không có RectTransform) | `UIRuntimeBootstrap` | Composition root: dựng `GameDatabase`+`SaveService`+`ItemService`+`InventoryService`+`CharacterService`, register screen, bind Back, `ShowScreen(MainHUD)`, save khi Quit/Pause | GameDatabase (1531 record), SaveService, 3 service | **Real** | `Systems` |

## 2. UI Roots

| Object | Current Hierarchy Path | RectTransform Summary | Component/Script | Current Function | Data/Service | Real Function Or Placeholder | Recommended Game Group |
|---|---|---|---|---|---|---|---|
| ScreenRoot | `UICanvas/SafeArea/ScreenRoot` | anchor (0,0)-(1,1) · pivot (0.5,0.5) · pos (0,0) · size (0,0) — stretch full | (không có) | Container chứa 6 màn full-screen | — | **Real** (container) | `UI/Screens` |
| HudRoot | `UICanvas/SafeArea/HudRoot` | anchor (0,0)-(1,1) · pos (0,0) · size (0,0) — stretch full | (không có) | Container chứa HUD | — | **Real** (container) | `UI/HUD` |
| PopupRoot | `UICanvas/SafeArea/PopupRoot` | anchor (0,0)-(1,1) · pos (0,0) · size (0,0) — stretch full | (không có) | Container chứa dialog | — | **Real** (container) | `UI/Popups` |
| OverlayRoot | `UICanvas/SafeArea/OverlayRoot` | anchor (0,0)-(1,1) · pos (0,0) · size (0,0) — stretch full | (không có) | **Rỗng hoàn toàn**, chưa dùng | — | **Chưa dùng** | `UI/Overlay` |

## 3. HUD

| Object | Current Hierarchy Path | RectTransform Summary | Component/Script | Current Function | Data/Service | Real Function Or Placeholder | Recommended Game Group |
|---|---|---|---|---|---|---|---|
| HUDVisual | `.../SafeArea/HudRoot/HUDVisual` | anchor center · pos (0,0) · **size (100,100)** ⚠️ | `HUDController` (kế thừa `UIScreen`, ScreenId=`MainHUD`) | Màn chính: hiện tiền + 6 nút điều hướng | `SaveService`, `UIService` | **Real** | `UI/HUD` |
| MoneyText | `.../HUDVisual/MoneyText` | anchor center · pos (-200, 800) · size (300,100) | `Text` | Hiện số vàng | `SaveService.CurrentData.Money` (đang = 0) | **Real** (giá trị thật, chưa có gameplay sinh tiền) | `UI/HUD/Currency` |
| MoneyIcon | `.../MoneyText/MoneyIcon` ⚠️ *(con của MoneyText, không phải sibling)* | anchor center · pos (-110, 0) · size (64,64) | `Image` (sprite `coin` từ S5) | Icon vàng | AssetCatalog (S5) | **Real** (visual) | `UI/HUD/Currency` |
| GemsText | `.../HUDVisual/GemsText` | anchor center · pos (200, 800) · size (300,100) | `Text` | Hiện số gem | `SaveService.CurrentData.Gems` (đang = 0) | **Real** | `UI/HUD/Currency` |
| GemsIcon | `.../GemsText/GemsIcon` ⚠️ *(con của GemsText)* | anchor center · pos (-110, 0) · size (64,64) | `Image` (sprite `gem_v1`) | Icon gem | AssetCatalog (S5) | **Real** (visual) | `UI/HUD/Currency` |
| Btn_Inventory | `.../HUDVisual/Btn_Inventory` | anchor center · pos (0, 600) · size (400,100) | `Button`, `Image` + con `Text`, `Icon` | Mở InventoryScreen | `UIService.ShowScreen(Inventory)` | **Real** | `UI/HUD/Navigation` |
| Btn_Character | `.../HUDVisual/Btn_Character` | anchor center · pos (0, 450) · size (400,100) | `Button`, `Image` + `Text`, `Icon` | Mở CharacterScreen | `UIService.ShowScreen(Character)` | **Real** | `UI/HUD/Navigation` |
| Btn_Dungeon | `.../HUDVisual/Btn_Dungeon` | anchor center · pos (0, 300) · size (400,100) | `Button`, `Image` + `Text`, `Icon` | Mở DungeonScreen (**panel rỗng**) | `UIService.ShowScreen(Dungeon)` | **Real** (nút hoạt động) nhưng **dẫn tới placeholder** | `UI/HUD/Navigation` |
| Btn_Craft | `.../HUDVisual/Btn_Craft` | anchor center · pos (0, 150) · size (400,100) | `Button`, `Image` + `Text`, `Icon` | Mở CraftScreen (**panel rỗng**) | `UIService.ShowScreen(Craft)` | Như trên | `UI/HUD/Navigation` |
| Btn_Merchant | `.../HUDVisual/Btn_Merchant` | anchor center · pos (0, 0) · size (400,100) | `Button`, `Image` + `Text`, `Icon` | Mở MerchantScreen (**panel rỗng**) | `UIService.ShowScreen(Merchant)` | Như trên | `UI/HUD/Navigation` |
| Btn_Settings | `.../HUDVisual/Btn_Settings` | anchor center · pos (0, -150) · size (400,100) | `Button`, `Image` + `Text`, `Icon` | Mở SettingsScreen (**panel rỗng**) | `UIService.ShowScreen(Settings)` | Như trên | `UI/HUD/Navigation` |

## 4. Screens

| Object | Current Hierarchy Path | RectTransform Summary | Component/Script | Current Function | Data/Service | Real Function Or Placeholder | Recommended Game Group |
|---|---|---|---|---|---|---|---|
| InventoryScreen | `.../ScreenRoot/InventoryScreen` | anchor center · pos (0,0) · **size (100,100)** ⚠️ · **không có Image nền** | `InventoryScreen` (kế thừa `UIScreen`, ScreenId=`Inventory`) | Liệt kê item dạng text | **`InventoryService.GetAllItems()`** — service thật, đang wired | **Real function**, nhưng **UI mới ở mức text list**; hiện rỗng vì chưa có nguồn sinh item | `UI/Screens/InventoryAndItems` |
| ListContent (Inventory) | `.../InventoryScreen/ListContent` | anchor center · pos (0,0) · size (800,1500) — **tràn ra ngoài parent 100×100** | `Text` | Vùng chữ liệt kê item | Gán bởi `InventoryScreen.Refresh()` | **Real** | `UI/Screens/InventoryAndItems` |
| CharacterScreen | `.../ScreenRoot/CharacterScreen` | anchor center · pos (0,0) · **size (100,100)** ⚠️ · **không có Image nền** | `CharacterScreen` (ScreenId=`Character`) | Liệt kê nhân vật dạng text | **`CharacterService.GetAllCharacters()`** — service thật, wired ở S6-003 | **Real function**, UI mức text list; hiện rỗng vì chưa có luồng tạo nhân vật | `UI/Screens/CharacterAndEquipment` |
| ListContent (Character) | `.../CharacterScreen/ListContent` | anchor center · pos (0,0) · size (800,1500) | `Text` | Vùng chữ liệt kê nhân vật | `CharacterScreen.Refresh()` | **Real** | `UI/Screens/CharacterAndEquipment` |
| DungeonScreen | `.../ScreenRoot/DungeonScreen` | anchor center · pos (0,0) · size (900,1400) · **có `Image` nền** | **`UIScreen` (class gốc)** — không có controller riêng | Chỉ hiện chữ "Dungeon UI is not implemented yet." | **KHÔNG có** — `DungeonService`, `CombatService`, `LootService`, `EnemyService` **đều chưa wire** | **PLACEHOLDER VISUAL — chức năng gameplay = 0%** | `UI/Screens/DungeonAndCombat` |
| CraftScreen | `.../ScreenRoot/CraftScreen` | anchor center · pos (0,0) · size (900,1400) · có `Image` | `UIScreen` (class gốc) | Chỉ hiện chữ placeholder | **KHÔNG có** — `CraftService` chưa wire | **PLACEHOLDER VISUAL — 0% gameplay** | `UI/Screens/Crafting` |
| MerchantScreen | `.../ScreenRoot/MerchantScreen` | anchor center · pos (0,0) · size (900,1400) · có `Image` | `UIScreen` (class gốc) | Chỉ hiện chữ placeholder | **KHÔNG có** — `MerchantService` chưa wire | **PLACEHOLDER VISUAL — 0% gameplay** | `UI/Screens/Merchant` |
| SettingsScreen | `.../ScreenRoot/SettingsScreen` | anchor center · pos (0,0) · size (900,1400) · có `Image` | `UIScreen` (class gốc) | Chỉ hiện chữ placeholder | **KHÔNG có** service nào | **PLACEHOLDER VISUAL — 0% chức năng** | `UI/Screens/Settings` |
| PopupScreen | `.../PopupRoot/PopupScreen` | anchor center · pos (0,0) · **size (100,100)** ⚠️ | `PopupScreen` (`IsPopup=true`) | Dialog title/message/OK, có `ShowMessage()`/`ShowDeferred()` | `UIService.RegisterDialogScreen` | **Real function** nhưng **chưa có nơi nào gọi** | `UI/Popups` |

**Chi tiết Title/Message trong 4 placeholder screen:** mỗi screen có `Title` (`Text`, pos (0,550), size 300×100) và `Message` (`Text`, pos (0,0), size 700×300) — đều là chữ tĩnh do `UIWiringGenerator` sinh ra, **không bind data nào**.

## 5. Back buttons

| Object | Current Hierarchy Path | RectTransform Summary | Component/Script | Current Function | Data/Service | Real Function Or Placeholder | Recommended Game Group |
|---|---|---|---|---|---|---|---|
| Btn_Back (Inventory) | `.../InventoryScreen/Btn_Back` | anchor center · pos (0, **-800**) · size (400,100) | `Button`, `Image` + con `Text` | Quay lại màn trước | `UIService.Back()` — bind runtime qua `transform.Find("Btn_Back")` | **Real** | Theo screen cha |
| Btn_Back (Character) | `.../CharacterScreen/Btn_Back` | anchor center · pos (0, **-800**) · size (400,100) | Như trên | Như trên | Như trên | **Real** | Theo screen cha |
| Btn_Back (Dungeon) | `.../DungeonScreen/Btn_Back` | anchor center · pos (0, **-550**) · size (400,100) | Như trên | Như trên | Như trên | **Real** | Theo screen cha |
| Btn_Back (Craft) | `.../CraftScreen/Btn_Back` | anchor center · pos (0, -550) · size (400,100) | Như trên | Như trên | Như trên | **Real** | Theo screen cha |
| Btn_Back (Merchant) | `.../MerchantScreen/Btn_Back` | anchor center · pos (0, -550) · size (400,100) | Như trên | Như trên | Như trên | **Real** | Theo screen cha |
| Btn_Back (Settings) | `.../SettingsScreen/Btn_Back` | anchor center · pos (0, -550) · size (400,100) | Như trên | Như trên | Như trên | **Real** | Theo screen cha |
| Btn_OK (Popup) | `.../PopupScreen/Btn_OK` | anchor center · pos (0, -278) · size (400,100) | `Button`, `Image` + `Text` | Đóng dialog | `PopupScreen` nội bộ | **Real** | `UI/Popups` |

⚠️ **Ràng buộc runtime quan trọng:** `UIRuntimeBootstrap.WireBackButton()` tìm bằng `screen.transform.Find("Btn_Back")` → **phải là con TRỰC TIẾP của screen và phải giữ đúng tên `Btn_Back`**. Đây là điểm duy nhất trong toàn bộ UI phụ thuộc vào **tên object**.

## 6. Text/Icon con trong panel
- **Trong mỗi nút** (`Btn_*`): `Text` (size 400×100, pos (0,0)) + `Icon` (`Image`, size 72×72, pos (-150,0)) — sprite gán sẵn từ S5 qua AssetCatalog. Đều là visual thật, edit được.
- **Trong Btn_Back / Btn_OK**: chỉ có `Text`, không có Icon.
- **Trong placeholder screen**: `Title` + `Message` là `Text` tĩnh.
- **Trong Inventory/Character**: `ListContent` là `Text` được runtime ghi đè nội dung.

## ⚠️ Vấn đề layout phát hiện được (chưa sửa, chỉ ghi nhận)
1. **`sizeDelta = (100,100)` trên 4 container**: `HUDVisual`, `InventoryScreen`, `CharacterScreen`, `PopupScreen`. Con của chúng (800×1500, 400×100…) **tràn ra ngoài khung parent**. Hiện **không gây lỗi hiển thị** vì uGUI không clip mặc định, nhưng là layout không chuẩn — nên đổi thành stretch full (anchor 0,0–1,1) khi làm visual ở S6.5B.
2. **`InventoryScreen` và `CharacterScreen` không có `Image` nền**, trong khi 4 placeholder screen lại có. Nên khi mở 2 màn này sẽ thấy xuyên thấu xuống HUD phía dưới.
3. **`MoneyIcon`/`GemsIcon` là con của `MoneyText`/`GemsText`** thay vì sibling — icon sẽ bị ảnh hưởng nếu chỉnh alignment của Text. Nên tách ra ngang hàng khi làm visual.
4. **`OverlayRoot` rỗng hoàn toàn** — chưa có công dụng.

## 📌 Tổng kết chức năng thật vs placeholder
| Nhóm | Trạng thái thật |
|---|---|
| HUD (currency + navigation) | ✅ **Real** — money/gems đọc SaveService, 6 nút điều hướng hoạt động |
| Inventory | ✅ **Real function** (InventoryService wired) — UI mới ở mức text list, chưa có grid/icon/tooltip/dùng item |
| Character | ✅ **Real function** (CharacterService wired) — UI text list, **chưa có equipment, chưa có skill, chưa có chỉ số chi tiết** |
| Popup | ✅ **Real function**, chưa có caller |
| **Dungeon** | ❌ **PLACEHOLDER — 0% gameplay.** Có sẵn data 11 dungeon + 122 enemy + 12 raid, có sẵn `DungeonService`/`CombatService`/`LootService`/`EnemyService` **nhưng chưa wire cái nào** |
| **Craft** | ❌ **PLACEHOLDER — 0% gameplay.** Có 321 recipe + `CraftService` chưa wire |
| **Merchant** | ❌ **PLACEHOLDER — 0% gameplay.** `MerchantService` chưa wire |
| **Settings** | ❌ **PLACEHOLDER — 0% chức năng.** Không có service tương ứng |

**Kết luận thẳng:** 4/7 màn chính hiện **chỉ là panel trắng có chữ**, không có bất kỳ chức năng gameplay nào phía sau — dù dữ liệu decode và service code **đã có sẵn đầy đủ**. Việc panel hiển thị được **không** đồng nghĩa chức năng đã xong.
