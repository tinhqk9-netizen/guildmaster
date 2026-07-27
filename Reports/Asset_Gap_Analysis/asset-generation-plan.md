# Asset Generation Plan — Rebuild_GuildMaster (v2)

**Ngày:** 2026-07-24 · **Phase:** A (kế hoạch — CHƯA generate) · **Credit:** 1000 (Plus)
**Nguồn chính:** `Assets-tham-khao` (FantasyDungeon, pixel-art). **Drawable gốc = tham khảo ID/concept (Nhóm D), không production.**

> ⚠️ CHẶN: chưa chốt được model/spec Nhóm C vì phụ thuộc **quyết định style** (xem mục 1). Plan này là bản nháp, khóa sau khi bạn chọn style.

---

## 1. Quyết định style (cần bạn chốt trước)

Pack là pixel-art. Higgsfield mạnh vector/painterly, yếu pixel-art chặt. 2 hướng:

- **Hướng 1 — Cam kết pixel-art đồng nhất:** mọi asset generate phải khớp pixel-art của pack. Rủi ro: khó ép model ra pixel-art sạch, có thể phải hậu xử lý (downscale/posterize/pixelate) → tốn công, tỉ lệ retry cao.
- **Hướng 2 — Pack là nền, UI/presentation theo style "clean fantasy" nhất quán riêng:** giữ sprite gameplay pixel-art của pack, nhưng UI chrome/portrait/splash làm theo style painterly/vector sạch (hợp mobile). Rủi ro: hai lớp style khác nhau, cần phối màu/khung cho ăn nhập.

→ Chọn hướng này quyết định model + spec bên dưới.

---

## 2. Model (xác minh lại bằng `models_explore` mỗi batch)

| Nhu cầu | Model | Aspect | Ghi chú |
|---|---|---|---|
| Icon/UI/slot/frame | `recraft_v4_1` (vector/utility) | 1:1, 3:4, 9:16 | đặt `background_color`; hợp Hướng 2 |
| Background/area/splash | `soul_location` | 9:16, 16:9 | cảnh nền |
| Portrait class/pet | `soul_cast` | ⚠️ 16:9 → crop/outpaint | hoặc `recraft` cho khung dọc |
| Nháp style / test pixel | `z_image` | 1:1, 9:16 | rẻ, thử tone (hữu ích cho Hướng 1) |

Nền trong suốt: generate → `remove_background`. Không có `get_cost`/`preflight` → ước tính credit thủ công từ `budget` × số ảnh, báo trước mỗi batch.

---

## 3. Ưu tiên generate (Nhóm C)

- **P1 — UI core:** panel/window, buttons (3 state), slot, card frames (adventurer/pet/enemy/raid), rarity frames (5 bậc), tab/bottom-nav, dialog/popup, progress bars.
- **P2 — Pet & Area:** bộ pet (pack không có); area background hoàn chỉnh.
- **P3 — Portrait & Presentation:** portrait class/pet; splash, loading, title, menu bg, reward popup.
- **P4 — Bổ sung:** status-effect icon; (audio batch riêng).

---

## 4. Batch 01 — Test 3 asset đại diện — **CHỜ STYLE + DUYỆT**

| # | Loại | Asset | Model (sẽ chốt theo style) | Aspect |
|---|---|---|---|---|
| 1 | UI | 1 panel/window bg (9-slice) | recraft utility | 1:1/3:4 |
| 2 | Icon | 1 item slot frame + rarity border | recraft vector | 1:1 |
| 3 | Portrait | 1 adventurer class portrait | recraft/soul_cast | 3:4 |

Quy trình: `models_explore get` → budget → tính credit → báo → chờ "generate Batch 01" → lưu `_Generated_Higgsfield/_Incoming/` → `asset-batch-01.md` → chấm PASS/RETRY/REJECT (đọc rõ 1080×1920, viền sạch, **ăn nhập style pack**, không vỡ scale nhỏ).

---

## 5. Phase B (trước khi generate thật)
1. Import `FantasyDungeon_v1.3_Unity.unitypackage` (thử) → xem slicing/animation sẵn có.
2. Slice thử 1 sprite-sheet + rename hero_skins (`.png.png`) + cắt thử `ui_kit.png`.
3. Xác nhận Nhóm A/B thật sự dùng được → chốt danh sách Nhóm C cuối.
4. Dùng Nhóm D (drawable gốc) để rút danh sách tên **pet / area / class** thật của game → lên số lượng cần generate.

---

**Trạng thái:** ⏸️ Dừng. Chưa generate, chưa tiêu credit. Chờ **quyết định style** + duyệt plan.
