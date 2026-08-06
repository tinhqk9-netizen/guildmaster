# Legacy Game Audit — Summary

> **Source:** `D:\Tinh\Guild Master - Idle Dungeons\`
> **Generated:** 2026-08-04
> **Purpose:** Source of truth for UI/UX reconstruction

---

## 1. Project Identity

| Field | Value | Evidence |
|-------|-------|----------|
| **Package** | `it.paranoidsquirrels.idleguildmaster` | `AndroidManifest.xml` line `package=` |
| **Platform** | Native Android (Java) | 14,482 total `.java`, 0 `.kt` |
| **Game Code** | **1,150 Java files** in game package | `sources/it/paranoidsquirrels/idleguildmaster/` |
| **Library Code** | 13,332 Java files | `com.android.*`, `com.google.*`, `com.instacart.*` |
| **Entry** | `MainActivity` → `HeadquartersFragment` (start) | `mobile_navigation.xml` `app:startDestination` |
| **Night mode** | Force dark (`AppCompatDelegate.setDefaultNightMode(2)`) | `MainActivity.java` line 209 |
| **UI hidden** | `UIUtils.hideUI(getWindow())` + `getSupportActionBar().hide()` | `MainActivity.java` lines 211-212 |

## 2. Game Code Breakdown

| Package | Files | Purpose |
|---------|-------|---------|
| Root | 12 | `MainActivity`, `Formulas`, `UIUtils`, `Utils`, `R`, `BuildConfig`, etc. |
| `databinding/` | 78 | Auto-generated view binding classes |
| `storage/` | 1,010 | Data models, save/load, entities, items, pets, places, quests |
| `ui/` | 50 | 4 Fragments + 46 Dialogs + 1 Custom View |

## 3. Asset Census

### Image Files

| Source | Count | Note |
|--------|-------|------|
| `res/drawable/` (PNG) | 1,035 | Main game sprites, icons, UI |
| `res/drawable-hdpi/` | 1 | `notification_oversize_large_icon_bg.png` (system) |
| `res/drawable/` (WebP) | 0 | WebP files are in mipmap only |
| **Total Image Assets** | **1,036** | |
| **Referenced (Java/XML)** | **1,023** (98.7%) | Via `R.drawable.*` or `@drawable/` |
| **Unreferenced** | **13** (1.3%) | Likely unused or dynamic-only |

### By Category

| Category | Count |
|----------|-------|
| Uncategorized (units, items not matched by heuristic) | 685 |
| Items & Equipment | 125 |
| Misc System (shop, quest, tavern, merchant, etc.) | 90 |
| Icons (navigation) | 40 |
| Characters (named adventurer classes) | 26 |
| Icons (UI elements) | 25 |
| Resources & Currency | 21 |
| Pets | 21 |
| Dungeon/Place | 3 |

### Other Resource Types

| Type | Folder | Count | Note |
|------|--------|-------|------|
| Layout XML | `res/layout/` | 201 | ~80 game-specific, rest are Android/library |
| Anim XML | `res/anim/` | 36 | Entry/exit animations |
| Animator XML | `res/animator/` | 36 | Property animations |
| Color XML | `res/color/` | 165 | Selectors for states |
| Interpolator | `res/interpolator/` | 11 | Animation timing |
| Navigation | `res/navigation/` | 1 | `mobile_navigation.xml` |
| Menu | `res/menu/` | 2 | bottom_nav + drawer |
| Values | `res/values/` | Many | `colors.xml`, `strings.xml` (486KB!), `styles.xml` (445KB), `dimens.xml` (39KB) |
| Font | `res/font/` | **NOT FOUND** | Game uses system font |
| Raw | `res/raw/` | 2 | Unknown content |
| XML config | `res/xml/` | Unknown | Preferences, file providers |

## 4. Canvas/Custom Drawing Audit

**Result: NO custom Canvas drawing found in game code.**

| Pattern | Searched In | Result |
|---------|-------------|--------|
| `onDraw` | All game Java | Only `R.java` resource IDs (false positive: `endIconDrawable`, etc.) |
| `Canvas` | All game Java | NOT FOUND |
| `Paint` | All game Java | NOT FOUND |
| `drawBitmap` | All game Java | NOT FOUND |
| `drawText` | All game Java | NOT FOUND |
| `drawRect` | All game Java | NOT FOUND |
| `onTouchEvent` | All game Java | NOT FOUND |
| `SurfaceView` | All game Java | NOT FOUND |

**Conclusion:** Game is 100% XML-layout-based. `NonScrollableGridView` is a simple `GridView` subclass (no custom drawing). All visual elements are standard Android Views (TextView, ImageView, ConstraintLayout).

## 5. Runtime Capture

**Status: `RUNTIME CAPTURE BLOCKED`**

- ADB: Not installed
- Android Emulator: No running emulator processes detected
- BlueStacks/Nox/LDPlayer/MEmu: Not found

## 6. Contact Sheets Generated

| File | Category | Assets |
|------|----------|--------|
| `contact_character.png` | Characters | 26 |
| `contact_dungeon_place.png` | Dungeon/Place | 3 |
| `contact_icon_nav.png` | Navigation Icons | 40 |
| `contact_icon_ui.png` | UI Icons | 25 |
| `contact_item_equipment.png` | Items/Equipment | 125 |
| `contact_misc_system.png` | System/Feature | 90 |
| `contact_pet.png` | Pets | 21 |
| `contact_resource_currency.png` | Currency/Resources | 21 |
| `contact_uncategorized.png` | Uncategorized (units etc.) | 685 |
| `contact_ALL_REFERENCED.png` | All referenced | 1,023 |
| `contact_ALL_UNREFERENCED.png` | All unreferenced | 13 |

All stored in: `Docs/Legacy_Audit/Asset_Gallery/`
