# Phase 1 — Legacy Asset Import Report

**Date:** 2026-08-04
**Scope:** Backup + import original game art from the decompiled source into Unity, prepare it for use, and build a name-based sprite lookup. No backend, gameplay, save data, scene, prefab, or existing UI runtime code was touched. No HUD/tab/drawer/screen was built.

---

## 1. Asset counts

| Metric | Count |
|---|---|
| Total rows in `legacy_asset_inventory_v2.csv` | 1,036 |
| Copied into `Assets/_Game/Art/Legacy/` | **1,032** |
| Skipped (test/framework placeholders) | 4 |
| Copy errors (source file not found) | 0 |
| Duplicate filenames | 0 |
| Sprites with import settings applied | 1,032 |
| Duplicate sprite names (post-import check) | 0 |
| `LegacySpriteCatalog` entries | 1,032 |

### Skipped files (not real game art)
| File | Reason |
|---|---|
| `test_area_image_detail_forest.png` | Dev/test placeholder |
| `test_area_image_summary_forest.png` | Dev/test placeholder |
| `test_unit_not_drawn.png` | Dev/test placeholder |
| `notification_oversize_large_icon_bg.png` | Android system notification icon, not game art |

---

## 2. Folder mapping

| Folder | Count | Source rule |
|---|---|---|
| UI | 5 | CSV category `icon_ui` / `ui_element` |
| Navigation | 40 | CSV category `icon_nav` |
| Currency | 19 | CSV category `resource_currency` |
| Characters | 41 | CSV category `character` / `adventurer_class` |
| Enemies | 188 | CSV category `enemy_other`/`enemy_undead`/`enemy_boss`/`enemy_humanoid`/`enemy_beast` |
| Items | 558 | CSV category `item_misc`/`item_equipment`/`item_material`/`item_weapon_armor`/`item_consumable` |
| **Skills** | **0** | No asset in the decompile references `Skills.java`/`Skill.java` — see §5 |
| Status | 18 | Filename prefix `icon_effect_*` (status effect icons, referenced by `StatusEffectType.java`) |
| Dungeons | 49 | CSV category `dungeon_place`, or filename prefix `area_*`/`summary_*` (area background art) |
| Pets | 21 | CSV category `pet` |
| Doctrines | 48 | Filename prefix `doctrine_*` |
| Quests | 1 | Filename contains `quest` (`quest_marker.png` — the only quest-specific icon found) |
| Misc | 44 | Everything else (CSV category `misc_system`, catch-all) |
| **Total** | **1,032** | |

**Classification rule (priority order, category-column-authoritative):** `doctrine_` prefix → Doctrines; `icon_effect_` prefix → Status; enemy categories → Enemies; character categories → Characters; `pet` → Pets; `dungeon_place`/`area_`/`summary_` → Dungeons; `resource_currency` → Currency; `icon_nav` → Navigation; `quest` substring → Quests; `icon_ui`/`ui_element` → UI; item categories → Items; else → Misc. Original filenames were kept unchanged everywhere — nothing was renamed or merged.

**Note — two classification bugs found and fixed during this run:** the first script draft used the `unit_` filename prefix as a shortcut for "Characters", but `unit_` is shared by both adventurer AND enemy sprites (and a handful of nav/item/currency files) in this game, so it silently mis-sorted 188 enemy sprites into Characters and swept a dozen misc files in with them. Fixed by making the CSV `Category` column authoritative and removing the prefix shortcut entirely; re-verified the final per-folder counts sum to exactly 1,032 (matches total copied) with zero files lost or double-counted.

---

## 3. Import settings applied (via `Tools/Guild Master/Legacy UI/Import Legacy Assets`)

| Setting | Value | Notes |
|---|---|---|
| Texture Type | Sprite (2D and UI) | |
| Sprite Mode | Single | |
| Alpha Is Transparency | true | Set unconditionally per spec |
| Compression | Uncompressed (None) | Preserves full fidelity; can move to Compressed/High Quality later if build size becomes a concern |
| Filter Mode | Bilinear | Source art is painted/photographic-style mobile icons (not pixel art) — Bilinear matches how Android originally rendered them. Point-filtering would look wrong here (unlike a pixel-art asset pack). |
| Max Size | Smallest power-of-two ≥ real resolution | Per-file, computed from actual width/height (e.g. a 37×37 source → Max Size 64) |
| Mipmaps | Off | UI sprites, never viewed at distance/perspective |
| Wrap Mode | Clamp | |
| Sprite Mesh Type | Full Rect | |
| Source file | **Untouched** | Tool only writes `.meta` import settings, never modifies the `.png` itself |

---

## 4. Missing references

None. All 1,032 rows in the CSV that were expected to copy did copy successfully (0 "source file not found" errors). See §1 for the 4 intentionally-skipped non-game files.

**Clarification on the CSV's `IsReferenced` column:** 13 rows are marked `IsReferenced=False` in `legacy_asset_inventory_v2.csv` because that column only checked Java (`.java`) references. 10 of those 13 are real, important assets referenced from XML instead (`bottom_nav_adventurers.png`, `bottom_nav_dungeons.png`, `bottom_nav_raids.png`, and 7 `drawer_icon_*.png` files) — these were copied normally. Only the other 3 (`test_*`, `notification_*`) were correctly excluded.

**Audit finding (not auto-corrected, flagging for review):** a handful of CSV `Category` assignments look questionable and were kept as-is per "don't rename/merge on your own initiative": e.g. `abiotic_core.png` and `alchemic_powder.png` are tagged `icon_nav` but visually resemble crafting material icons, not navigation icons; `crystal_dagger.png` and `diamond.png` are tagged `resource_currency` but look like equipment/gem items rather than currency. These ended up in Navigation/Currency respectively. Recommend a human pass over `Docs/Legacy_Audit/phase_1_copy_manifest.csv` before Phase 2 wiring if exact folder purity matters.

---

## 5. Skills folder is empty — explained, not a bug

Searched the entire 1,036-row inventory for any asset referencing `Skills.java` or `Skill.java`: **zero matches**. Cross-checked against the handoff doc, which itself notes `Skills.java` (23,635 bytes) was only header-read, not fully decoded. The legacy game appears to have no dedicated per-skill icon art — skills likely render using the caster's own portrait or no icon at all. The `Skills/` folder was still created (empty) to match the requested 13-folder structure; populate it later only if a genuine skill-icon asset is found in a deeper audit.

---

## 6. Files created / modified

**Created (new files only — nothing existing was modified):**
- `Assets/_Game/Art/Legacy/{UI,Navigation,Currency,Characters,Enemies,Items,Skills,Status,Dungeons,Pets,Doctrines,Quests,Misc}/` — 1,032 `.png` files + their `.meta` files
- `Assets/_Game/Scripts/Editor/LegacyAssetImporter.cs` — the idempotent import tool
- `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteCatalog.cs` — ScriptableObject data holder
- `Assets/_Game/Scripts/Runtime/UI/Legacy/LegacySpriteRegistry.cs` — runtime lookup API
- `Assets/Resources/LegacySpriteCatalog.asset` — the built catalog (1,032 entries)
- `Docs/Legacy_Audit/unity_legacy_shape_mapping.md` — 117 XML drawable → Unity equivalent mapping
- `Docs/Legacy_Audit/phase_1_copy_manifest.csv` — machine-readable copy manifest (FileName, Folder, OriginalCategory)
- `Docs/Legacy_Audit/Asset_Gallery/phase1_*.png` — 12 contact sheets (one per non-empty folder), proof of import
- `Docs/Legacy_Audit/phase_1_asset_import_report.md` — this report
- `D:\Tinh\Backups\Legacy_UI_Phase_1_Asset_Import\` — backup/rollback manifest (created before any work started)

**Modified:** none. No scene, prefab, HUDController, existing UI screen, service, model, or save data file was touched.

---

## 7. Rollback steps

See `D:\Tinh\Backups\Legacy_UI_Phase_1_Asset_Import\README_ROLLBACK.md` for full detail. Summary: this phase is 100% additive, so rollback = delete the new files listed in §6, then `Assets > Refresh` in Unity. No other cleanup needed since nothing pre-existing was changed.

---

## 8. Verification results

- **Unity compile:** 0 errors, 0 warnings (confirmed via `recompile_scripts` after each of the 3 script additions/edits)
- **Duplicate sprite names:** 0 (checked across all 1,032 files by filename stem, spanning all 13 folders)
- **Registry load test** (`Tools/Guild Master/Legacy UI/Verify Sample Sprite Load`, 16/16 passed):

| Group | Name | Result |
|---|---|---|
| Character | `unit_cleric` | PASS |
| Character | `unit_knight` | PASS |
| Character | `unit_paladin` | PASS |
| Item | `abherrant_fabric` | PASS |
| Item | `absolute_zero` | PASS |
| Item | `abyssal_cutlass` | PASS |
| Enemy | `unit_abomination` | PASS |
| Enemy | `unit_adept` | PASS |
| Enemy | `unit_alchemist` | PASS |
| Currency | `coin_platinum` | PASS |
| Currency | `coin_gold` | PASS |
| Currency | `coin_silver` | PASS |
| Currency | `coin_copper` | PASS |
| Navigation | `bottom_nav_adventurers` | PASS |
| Navigation | `bottom_nav_dungeons` | PASS |
| Navigation | `bottom_nav_raids` | PASS |

`LegacySpriteRegistry.Count = 1032` after the test run, matching the catalog exactly.

---

## 9. Explicitly NOT done (out of Phase 1 scope, per instructions)

- No scene, prefab, HUDController, or existing UI screen was modified
- No new HUD, tab, drawer, or gameplay screen was built
- No backend/service/model/save-data code was touched
- Phase 2 was not started
