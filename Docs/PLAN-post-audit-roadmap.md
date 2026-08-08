# PLAN-post-audit-roadmap

## 🧠 Tóm tắt Hiện trạng (Current State Audit)

Dựa trên các báo cáo hoàn thành gần nhất (Phase 3 & 4 Backend/Content) và tiến độ UI, đây là bức tranh toàn cảnh của Rebuild_GuildMaster hiện tại:

### ✅ Đã hoàn thành (Backend & Content Core)
1. **Dữ liệu & Logic:** Port xong toàn bộ công thức, định nghĩa quái, item, hero (Phase 0, 1, 2).
2. **Kinh tế & Nâng cấp (Phase 3):** Storage, Workshop (Crafting), Market. Đã fix UI cơ bản.
3. **Tính năng nâng cao (Phase 4):** Pet Expedition, Raid System (Restoration), Full Content Restoration.
4. **Các tính năng lẻ:** Dismiss Hero.

### ⏸️ Tạm hoãn (YAGNI / Ponytail Principles)
1. **Tutorial / Onboarding (Legacy):** (Từ `tutorial_audit_report.md`). Không port hệ thống nhắc việc bằng Text Box rườm rà của Legacy. Chờ UI hoàn thiện mới làm hệ thống Masking hiện đại. Đỡ tốn công làm lại 2 lần.
2. **Phase B (Font Foundation):** Đã rollback do gây lỗi layout (theo `GuildMaster_UXUI_Phase_Execution_Plan.md`).

---

## 🚀 Kế hoạch tiếp theo (Streamlined Roadmap)

Theo triết lý **Ponytail (Minimalist, Do Less, Standard UI first)**, chúng ta sẽ không vẽ vời thêm các hệ thống phức tạp mà tập trung vào việc **hoàn thiện vòng lặp game chính (Core Loop)** và **UI/UX hiện đại (Phase C)**.

### Mục tiêu cốt lõi (Core Focus)
Không nhồi nhét feature, ưu tiên 3 việc:
1. Giao diện HUD (Phase C).
2. Tối ưu trải nghiệm (UX) thay vì bôi thêm tính năng backend.
3. Hoàn thiện các cơ chế tương tác bị thiếu trên UI hiện tại.

---

## 📋 Task Breakdown

### Phase C: HUD Visual Prototype (Ưu tiên Cao nhất)
> *Mục đích: Thiết lập bộ khung UI (Clean Modern Fantasy) chuẩn trước khi làm các màn hình chi tiết.*
- [ ] Cập nhật `HUDController.cs` và các node trên `Main.unity`.
- [ ] Dựng 8 nút điều hướng (Tavern, Character, Inventory, Dungeons, Workshop, Storage, Market, Settings).
- [ ] Thêm dải Currency (Gold/Gem) trên cùng với placeholder sprite.
- [ ] *Ponytail Check:* Dùng UGUI tiêu chuẩn, không dùng custom mesh tốn kém (như đã phân tích ở Option A của Phase 2 UI).

### Phase D: Nâng cấp Màn Hình Core Loop (Ưu tiên Trung bình)
> *Mục đích: Đắp UI chuẩn lên các logic backend đã hoàn thành ở Phase 3 & 4.*
- [ ] **Tavern/Gacha:** Dựng màn hình chiêu mộ hero đẹp hơn, hiển thị tỷ lệ roll.
- [ ] **Character Detail:** Hoàn thiện panel stats, equipment, trait, doctrine. Đảm bảo nút "Dismiss" nằm đúng chỗ.
- [ ] **Inventory/Storage:** Tối ưu hóa UI Grid, phân loại item (Tab: Equipment, Material, Consumable).
- [ ] **Dungeon/Raid Select:** Dựng UI chọn ải, hiển thị stamina, tỷ lệ rớt đồ.

### Phase E: Tutorial & Polish (Ưu tiên Thấp - Cuối cùng)
> *Mục đích: Giữ chân người mới.*
- [ ] Triển khai UI Masking (khoét lổ sáng) dựa trên field `TutorialStep` đã có trong `SaveData`.
- [ ] Nối event trigger cho các hành động: Lấy loot đầu tiên, Craft đồ đầu tiên, Mặc đồ đầu tiên.

---

## 🎯 Verification Checklist (Tiêu chí Nghiệm thu)

1. [ ] **HUD:** Người chơi có thể nhảy qua lại giữa 8 tab mượt mà, không giật lag.
2. [ ] **Currency:** Tiền/Gem cập nhật realtime mỗi khi tiêu xài hoặc nhận thưởng.
3. [ ] **Clean UI:** Giao diện không bị bóp méo (distort) ở chuẩn 1080x1920.
4. [ ] **Ponytail Rule:** Không cài thêm thư viện UI bên ngoài nếu UGUI có sẵn giải pháp (`Image` 9-slice).

---

## 🤖 Đề xuất của Project Planner

**Recommendation:** Bỏ qua hoàn toàn việc sửa backend trong 1-2 tuần tới. Toàn lực dồn vào **Phase C — HUD Visual Prototype** để game có hình hài thực sự. 

Anh thấy sao về kế hoạch này? Mình có bắt đầu luôn vào `/create` Phase C không?
