Tôi đang gặp context compaction nghiêm trọng (content bị compress liên tục). Tôi đã audit xong code và xác định gaps. Đang chuẩn bị implement fixes.

**Các gaps đã xác định:**
1. **Không có nhân vật ban đầu** — player mở game lần đầu có 0 nhân vật
2. **Không có tavern guest khởi đầu** — cần auto-sinh guest
3. **Không có item/gold khởi đầu** — inventory rỗng
4. **Không có dungeon selection UI** — OnClickStartFirst lấy dungeon đầu tiên từ DB
5. **Không có party formation UI** — không chọn được nhân vật nào đi dungeon

**Fix plan:** Thêm `InitializeFirstTime()` trong ServiceContainer, gọi từ UIRuntimeBootstrap sau khi services được tạo.

Tôi sẽ implement ngay!