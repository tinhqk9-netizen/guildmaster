# Dev Verification Tool Report — Phase 1 Character Progression

**Dự án:** `D:\Tinh\Rebuild_GuildMaster`  
**Tool Path:** `Assets/_Game/Tools/Phase1Verification/`  
**Trạng thái:** Hoàn tất & Đã sẵn sàng trên Unity Editor  

---

## 1. Tổng quan (Overview)

Công cụ **Dev Verification Tool (Phase 1 Character Progression)** được xây dựng nhằm giúp Developer và Tester kiểm tra trực quan các tính năng cốt lõi của Phase 1 trong Unity Editor mà không cần can thiệp hay làm hỏng dữ liệu lưu trữ gốc (SaveData).

### Nguyên tắc thiết kế:
- **Không bypass logic:** Sử dụng trực tiếp `CharacterService`, `PromotionService`, `GameDatabase`, và `ISaveService` hiện có.
- **Không thay đổi data thật:** Toàn bộ Hero test được gắn tag/prefix `DEV_TEST_` để phân biệt và dọn dẹp sạch sẽ chỉ bằng 1 click.
- **Không refactor service:** Đảm bảo 100% tính đóng gói, chỉ bổ sung công cụ hỗ trợ test.

---

## 2. Danh sách File Tạo mới (New Files Created)

### Backup:
- `Backups/PrePhase1Tool_Backup.zip`: Bản lưu trữ dự phòng toàn bộ mã nguồn trước khi tạo tool.

### Mã nguồn Công cụ (Tool Code):
1. **`Assets/_Game/Tools/Phase1Verification/Phase1VerificationHelper.cs`**  
   - Chứa logic backend tĩnh phụ trách sinh Hero test theo các pipeline chuẩn và dọn dẹp dữ liệu test (`DEV_TEST_*`).
2. **`Assets/_Game/Tools/Phase1Verification/Editor/Phase1VerificationWindow.cs`**  
   - Giao diện Editor Tool (EditorWindow) tích hợp trực tiếp vào thanh Menu của Unity Editor (`Tools > GuildMaster > Phase 1 Character Verification Tool`).
3. **`Assets/_Game/Scripts/Tests/EditMode/Phase1VerificationToolTests.cs`**  
   - Bộ unit test EditMode kiểm tra tự động 5 chức năng của tool.

---

## 3. Cách mở Tool trên Unity Editor (How to Open Tool)

1. Mở dự án Unity `Rebuild_GuildMaster`.
2. Trên thanh Menu chính phía trên Unity Editor, chọn:  
   **`Tools` ➔ `GuildMaster` ➔ `Phase 1 Character Verification Tool`**
3. Cửa sổ điều khiển **Phase 1 Verification** sẽ xuất hiện.
4. Bấm **Play** trong Unity Editor để kích hoạt Runtime Services.

---

## 4. Cách Test từng nút chức năng (How to Test Each Button)

| Nút bấm | Hành vi & Quy trình Test | Kết quả mong đợi |
|---|---|---|
| **1. `[Spawn Basic Hero]`** | - Bấm nút khi đang Play Mode.<br>- Tạo một Hero `footman` Lv.1 cơ bản.<br>- Gán ID prefix `DEV_TEST_BASIC_`. | Hero mới xuất hiện trong danh sách Roster (`AdventurersTabController`). Dữ liệu `SaveData` được cập nhật đồng bộ. |
| **2. `[Spawn Double Trait Hero]`** | - Bấm nút khi đang Play Mode.<br>- Tạo một Hero `archer` Lv.5 mang đồng thời **Common Trait** (`FERAL`) và **Rare Trait** (`DRAGON_BLOOD`). | Mở giao diện `CharacterDetailPanel` ➔ Mục **TRAITS** hiển thị đủ cả `Common: Feral` và `Rare: Dragon Blood`. |
| **3. `[Spawn Promotion Test Hero]`** | - Bấm nút khi đang Play Mode.<br>- Tạo một Hero `apprentice` đạt cấp tối đa **Lv.20** (`MaxLevel`).<br>- Tự động mở Character Detail UI. | Mở giao diện `CharacterDetailPanel` ➔ Đủ điều kiện Promotion ➔ Danh sách `NextClasses` hiển thị các nhánh tiến hóa (ví dụ: `Mage`, `Cleric`). Bấm Promote ➔ Class đổi, Level reset về 1, Skills đổi theo class mới. |
| **4. `[Spawn Showcase Hero]`** | - Bấm nút khi đang Play Mode.<br>- Tạo Hero `mage` Lv.15 có đủ Trang bị (Vũ khí, Giáp, Nhẫn), Skills và 2 Traits. | Mở giao diện `CharacterDetailPanel` ➔ Kiểm tra toàn bộ UI: Combat Stats (HP/CON/INT/DEX/DEF/MDEF), HP Bar, Skills, Equipment slots hiển thị chính xác. |
| **5. `[Clear Test Data]`** | - Bấm nút bất kỳ lúc nào.<br>- Tìm tất cả Hero có ID bắt đầu bằng `DEV_TEST_`. | Toàn bộ Hero test bị xóa sạch khỏi Roster và `SaveData`. Trang bị được gỡ bỏ an toàn. Dữ liệu save thật của người chơi không bị ảnh hưởng. |

---

## 5. Các vấn đề phát hiện được qua Tool (Discovered Issues)

1. **Cơ chế Lưu trữ Trait (Double Trait Persistence):**
   - Bộ lưu trữ `CharacterSaveData` và runtime `CharacterRuntime` đã hỗ trợ 2 trường `TraitCommon` và `TraitRare`. 
   - Nút `[Spawn Double Trait Hero]` xác nhận UI `CharacterDetailPanel` có khả năng hiển thị song song 2 Trait chuẩn xác khi dữ liệu có đủ 2 trường này.
2. **Promotion Execution Flow:**
   - Khi thực hiện Promotion thông qua `PromotionService.Promote()`, hệ thống đổi `DefinitionId` sang class mới, reset `Level` về 1 và `Exp` về 0, giữ nguyên Trang bị & Traits. Tool giúp xác nhận quy trình này khớp 100% nguyên mẫu Java gốc.
3. **Equipment Safety on Dismiss/Clear:**
   - Khi bấm `[Clear Test Data]`, nút gọi `CharacterService.DismissCharacter()`, tự động mở khóa (IsLocked = false) cho các trang bị test, tránh rò rỉ hoặc khóa cứng vật phẩm trong rương đồ.
