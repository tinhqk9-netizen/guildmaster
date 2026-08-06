# Unresolved Questions — Updated 2026-08-04

> Synced with deep audit. Removed resolved items. Only real blockers remain.

---

## ✅ RESOLVED (removed)

| # | Question | Resolution |
|---|----------|------------|
| 1 | Canvas/custom drawing? | **NO.** 100% XML-based UI. `NonScrollableGridView` extends `GridView`, overrides `onMeasure` only. |
| 2 | How many assets used? | **1,036 PNGs** in `drawable/`, all referenced by Java or XML. Reclassified into 19 categories. |
| 3 | Rarity border colors? | **NO RARITY BORDERS.** `backgroundFromRarity()` always returns `object_border_dim_white` (line 218-220). Uniform border for all items. |
| 4 | Dialog window style? | **Resolved.** `CustomDialog` base class: bg = `dialog_border.xml` (cardview_dark_background fill, 3dp black stroke, 10dp corners). Default layout = `MATCH_PARENT` width, `WRAP_CONTENT` height. |
| 5 | Adapter patterns? | **Resolved.** 9 adapters in `UIUtils.java` (GridAdapter, GridAdapterEnemies, RecipesAdapter, BestiaryAdapter, BestiaryGridAdapter, KingMessagesAdapter, DoctrinesAdapter, FaqAdapter, PetsGridAdapter). All use standard ViewHolder pattern. |
| 6 | Dynamic resource loading? | **Resolved.** 1,193 patterns catalogued. Primary: `getIdentifier("unit_" + id)` for portraits, `R.drawable.{itemName}` for items. |
| 7 | Font system? | **System default only.** No custom fonts. Uses bold/italic/normal styles with sizes 12sp-20sp. |
| 8 | Page system in entity detail? | **Resolved.** 4 pages (stats → secondary → tertiary → potions). Visibility toggled via `setVisibility(VISIBLE/INVISIBLE)`. |
| 9 | Ascended unit palette? | **Resolved.** `applyAscendedPalette()` swaps border drawables to `object_border_ascended` (fill: `ascended_background`, stroke: `ascended_unit` / `#ffdb7f`). |

---

## ❓ REMAINING (2 items — non-blockers)

### Q1: Animation Timing Details
- **What:** 72 animation XML files (36 anim + 36 animator) exist. Individual timing not read.
- **Impact:** LOW. Animations are polish, not layout/UX-critical.
- **Workaround:** Implement with sensible defaults (300ms fade, 200ms slide). Adjust later if needed.
- **Status:** `IMPLEMENT LATER`

### Q2: styles.xml Game-Specific Entries
- **What:** 445KB `styles.xml` is 99% Material library boilerplate. Only 1 game-specific style found: `Theme.IdleGuildMaster` (10 lines).
- **Impact:** NONE for implementation. Game uses direct XML attributes on views, not style references.
- **Workaround:** Use the documented `Theme.IdleGuildMaster` values + per-view attributes.
- **Status:** `RESOLVED — NOT A BLOCKER`

---

## 🟢 VERDICT: NO REMAINING BLOCKERS

All critical implementation data is available in `claude_ui_reconstruction_handoff.md`.
