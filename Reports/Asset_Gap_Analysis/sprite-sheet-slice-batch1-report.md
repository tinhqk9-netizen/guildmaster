# Sprite Sheet Slice — Batch 1 Report

**Ngày:** 2026-07-24 · **Phase:** Slice Batch 1 Verification (character/enemy sheets)  
**Script:** `Assets/_Game/Scripts/Editor/SpriteSheetSlicer.cs`  
**Menu:** `GuildMaster → Assets → Slice Reference Sprite Sheets` (+ bản `(Dry Run)`)  

> ❌ Không sửa gameplay/backend · ❌ Không sửa source decode · ❌ Không sửa Production JSON  
> ❌ Không gọi Higgsfield · ❌ Không generate · ❌ Không map scene/prefab · ❌ Không bắt đầu S5  
> ❌ Không đụng VFX / ui_kit / ui_dialog / hero_skins / tilesets ở batch này  

---

## Scope & Guards

- **Chỉ quét** `Art/Characters` + `Art/Enemies`, **bỏ** `hero_skins`
- Guard áp cho từng file, fail → skip + ghi lý do:
  1. Giải mã được PNG
  2. Kích thước **đúng 1024×1024**
  3. Tên đúng pattern `<actor>_<anim>_sheet` (actor = tên folder cha)
  4. **Không có ô rỗng** (16/16 ô có nội dung)
  5. **Không có hàng phủ bất thường** (mọi row ≥ 0.5 × median)
- **ForceExclude**: `merchant_sheet`, `villager_sheet`, `merchant_idle_sheet`, `merchant_walk_sheet`

---

## Batch 1 Verification Summary

| Mục kiểm tra | Trạng thái | Chi tiết |
|---|---|---|
| Số file Candidate đã slice | **98 / 98 PASS** | `spriteMode = 2` (Multiple) |
| Số file Skipped giữ nguyên | **4 / 4 PASS** | `merchant_sheet`, `merchant_idle_sheet`, `merchant_walk_sheet`, `villager_sheet` không bị slice |
| Tổng số Sprite đã cắt | **1568 / 1568 PASS** | Mỗi candidate file sinh đúng 16 sprite |
| Quy cách đặt tên Sprite | **PASS** | `<actor>_<anim>_<dir>_<00..03>` |
| Pivot Configuration | **PASS** | Bottom Center (`alignment: 7`, `pivot: {x: 0.5, y: 0}`) |
| Filter Mode | **PASS** | Point (`filterMode: 0`) |
| Compression | **PASS** | None / Uncompressed (`textureCompression: 0`) |
| MipMap | **PASS** | Off (`enableMipMap: 0`) |
| Out-of-scope Check | **PASS** | Không chạm vào UI / VFX / hero_skins / Tilesets |
| Unity Compile Status | **PASS** | 0 CS errors |

---

## FINAL DECISION

# `SLICE_BATCH1_VERIFIED_DONE`
