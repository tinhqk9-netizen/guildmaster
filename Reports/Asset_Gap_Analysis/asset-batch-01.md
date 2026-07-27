# Batch 01 — STYLE TEST (preflight) — Rebuild_GuildMaster

**Ngày:** 2026-07-24 · **Trạng thái:** ⏸️ PREFLIGHT — CHƯA generate, chờ duyệt
**Mục tiêu:** so sánh trực quan 2 hướng style trên **cùng 1 loại asset** để chốt style chính cho Nhóm C.

---

## Asset test: **Item Slot Frame** (ô đồ trống trong inventory)
Lý do chọn: đơn vị UI chrome nhỏ nhất, dùng lại nhiều nhất, dễ so sánh 1:1. Style tham chiếu = ô đá bevel trong `ui_kit.png` của pack (viền đá xám khối, lòng ô lõm tối slate).

**Palette bám pack (rút từ ui_kit.png):**
`#33383D` slate nền · `#9AA0A6` đá sáng · `#6E747B` đá vừa · `#4A4F55` đá tối · `#20242A` lòng ô tối · `#C9A24B` trim vàng · `#3F6FB0` accent xanh

---

## Hướng 1 — Clean / painterly-vector (color-matched)
- **Model:** `recraft_v4_1`, `model_type: utility` (phẳng, sạch, front-facing, ổn định — hợp UI)
- **resolution:** `2k` · **aspect:** `1:1`
- **colors:** palette bám pack ở trên · **background_color:** `#33383D` (slate, đồng nền pack)
- **Prompt (dự kiến):**
  > "Game UI inventory item slot frame, single empty square socket, chunky carved-stone border with beveled grey stone blocks, dark recessed slate interior, subtle inner shadow, fantasy RPG dungeon style, clean readable, centered, flat front view, mobile game asset"
- **Rủi ro:** có thể ra hơi "bóng/painterly" hơn pack; cần chỉnh contrast/viền cho ăn khớp. Điểm mạnh của model → tỉ lệ đạt cao.

## Hướng 2 — Pixel-art (cố gắng khớp pack)
- **Model:** `recraft_v4_1`, `model_type: standard` + prompt ép pixel (⚠️ **không có model pixel-art chuyên dụng** trong Higgsfield — đây là điểm yếu, test để xem ra được tới đâu)
- **resolution:** `2k` · **aspect:** `1:1` · **colors:** cùng palette · **background_color:** `#33383D`
- **Prompt (dự kiến):**
  > "16-bit pixel art game UI item slot, empty square socket, blocky pixelated stone border, limited palette, crisp hard pixel edges, no anti-aliasing, dithering shadows, retro fantasy RPG inventory cell, front view"
- **Rủi ro:** Higgsfield dễ ra "faux-pixel" (pixel giả, cạnh vẫn mượt) thay vì pixel thật; có thể phải hậu xử lý posterize/downscale. Đây chính là điều cần thấy trước khi cam kết.

---

## Cost — báo minh bạch
- **Không có tool `get_cost`/`preflight`.** `recraft_v4_1` và `z_image` **không expose tham số cost/budget** trong metadata. Lịch sử giao dịch chỉ có 1 dòng grant 1000 credit → **không có dữ liệu thực nghiệm để tính chính xác trước.**
- **Ước lượng:** 2 ảnh chất lượng tiêu chuẩn — thực tế thường ở mức thấp (vài đến vài chục credit/ảnh). Trên tổng 1000 credit, phơi nhiễm tối đa của test này là **không đáng kể**.
- **Cách honor kỷ luật cost:** nếu bạn muốn con số thật trước khi làm cả 2, mình generate **đúng 1 ảnh trước**, đọc ngay `balance`+`transactions` để lấy chênh lệch credit thật, báo lại, rồi mới làm ảnh thứ 2. (Xem tùy chọn khi duyệt.)

---

## Sau khi generate (nếu duyệt)
- Lưu vào `Assets/Art/_Generated_Higgsfield/_Incoming/`
  - `slot_test_clean.png` (Hướng 1) · `slot_test_pixel.png` (Hướng 2)
- Đọc lại `balance` → ghi credit thực đã tiêu vào file này.
- **KHÔNG** import/map Unity, **KHÔNG** sửa code/scene/prefab.
- Bạn xem 2 ảnh → chọn style chính cho Nhóm C.

**Chờ bạn duyệt:** (a) làm cả 2 ảnh luôn, hay (b) làm 1 ảnh đo cost trước rồi mới làm ảnh 2.

---

## KẾT QUẢ — Hướng 1 (đã generate 1 ảnh, 2026-07-24)

- **Model:** `recraft_v4_1` · `model_type: utility` · `resolution: 2k` · `aspect: 1:1`
- **Prompt đã dùng:** "Game UI inventory item slot frame, single empty square socket, chunky carved-stone border made of beveled grey stone blocks, dark recessed slate interior with subtle inner shadow, fantasy RPG dungeon style, clean and readable, centered, flat front view, mobile game asset"
- **colors:** `#33383D,#9AA0A6,#6E747B,#4A4F55,#20242A,#C9A24B,#3F6FB0` · **background_color:** `#33383D`
- **Output:** `Assets/Art/_Generated_Higgsfield/_Incoming/slot_test_clean.png` — 2048×2048 PNG (RGB, không alpha)
- **Cost:** preflight `get_cost` = **8 credit** · balance **1000 → 992** = **8 credit thực** (khớp)
- **Job id:** `4df92ced-62d2-4ba2-9f40-45ef7cc57c9a`

### Self-review: 🟡 **NEED RETRY** (nhưng HƯỚNG STYLE = ĐẠT)

**✅ Điểm ĐẠT (quan trọng nhất — trả lời câu hỏi style):**
- Style đá-gỗ chunky, bevel, palette xám-slate **khớp rất tốt với ui_kit của pack**. → **Xác nhận Hướng 1 (clean/painterly, recraft utility) bám được style pack. KHÔNG cần test Hướng 2 pixel-art.**

**❌ Lỗi cần sửa ở lần retry (do prompt, không phải do hướng):**
1. **Chữ "EMPTY" bị bake vào ảnh** — text phải là UI label runtime, không được nằm trong sprite.
2. **Ô vuông viền xanh giả ở giữa** — artifact thừa, không mong muốn.
3. **Khung đôi** (viền đá ngoài + rìa đá trong) → giống panel hơn là 1 ô slot gọn; pack slot là ô bevel đơn.
4. **Không có alpha** (nền slate đặc) → hạn chế tái dùng; cần transparent hoặc chạy `remove_background`.
5. Corner đá không đều → **khó 9-slice** ra kích thước tùy ý.

**Hướng retry (prompt tinh chỉnh):** "single compact item slot, one simple beveled stone border, NO text, NO inner square, empty dark interior, transparent background, tileable, uniform corners" + để `background_color: null` và/hoặc `remove_background` sau.

**Trạng thái:** ⏸️ Dừng để bạn review ảnh. Đã tiêu 8/1000 credit. Chưa import Unity, chưa sửa code/scene.
