# Unity Legacy Shape Mapping — Phase 1

**Scope:** All 117 game-specific XML drawables under `D:\Tinh\Guild Master - Idle Dungeons\resources\res\drawable\` (per `deep_xml_drawables.csv`). None of these were copied into Unity as sprite files — XML shape/vector drawables are not bitmap sprites and must be recreated as native Unity constructs (`Image` + border, or redrawn vector art). This document is the mapping reference for that recreation work in a later phase.

Colors referenced below (`@color/...`) resolve via `Docs/Legacy_Audit/deep_game_colors.json` / `resources/res/values/colors.xml`.

---

## 1. Shape Drawables (36) — border/panel/background rectangles

All of these translate directly to a Unity `Image` component: `solid` → `Image.color`, `stroke` → a thin child/sibling `Image` (or an Outline-style 9-slice sprite generated once), `corners` → a rounded-rect sprite (Unity `Image.Sprite` with border, `Image Type = Sliced`) or a shader-based rounded corner. No painted texture is needed — the legacy game itself only ever uses flat color + stroke + radius.

| File | Fill | Stroke | Corners | State | Unity Equivalent |
|---|---|---|---|---|---|
| `circle_border.xml` | transparent | 1dp `brass_border` | 200dp (fully round) | — | `Image` (transparent fill) + `Outline`/ring sprite, radius = height/2 |
| `dialog_border.xml` | `cardview_dark_background` (#1e1e1e) | 3dp `black` | 10dp all | — | Rounded-rect sliced sprite, used as every dialog panel's root background |
| `m3_tabs_line_indicator.xml` | white | none | none, height=2dp | — | Material framework leftover — **not a game asset**, ignore |
| `navigation_empty_icon.xml` | transparent | none | none | — | Material framework leftover — ignore (empty spacer) |
| `object_background_black_round.xml` | `background_doctrine_level` (#141414) | none | 10dp all | — | Rounded-rect sprite, doctrine ability level pip background |
| `object_border_ascended.xml` | `ascended_background` | 1dp `ascended_unit` (#ffdb7f) | 10dp all | Ascended unit card | Rounded-rect sprite variant, swapped in via `applyAscendedPalette()` logic |
| `object_border_ascended_extra_opaque.xml` | `ascended_background_extra_opaque` | 1dp `ascended_unit` | 10dp all | Ascended, higher-opacity variant | Same sprite, different fill alpha |
| `object_border_black.xml` | `standard_background` | 1dp `border_rarity_none` | 10dp all | Default/no-rarity item border | Rounded-rect sprite |
| `object_border_brass.xml` | `standard_background` (#1fffffff) | 1dp `brass_border` (#befaa03e) | 10dp all | Highlighted / level-up | Rounded-rect sprite — **most common card/button border in the game** |
| `object_border_brass_rounded_right.xml` | `standard_background` | 1dp `brass_border` | 10dp right only | — | Same sprite, right-corners-only variant (portrait+info row joins) |
| `object_border_brass_sharp.xml` | `standard_background` | 1dp `brass_border` | none (square) | — | Plain rect sprite, no rounding |
| `object_border_buy.xml` | `standard_background` | 2dp `brass_border` | 10dp all | Merchant buy button | Rounded-rect sprite, thicker stroke |
| `object_border_dim_white.xml` | `standard_background` | 1dp `dim_white` (#c8c8c8) | 10dp all | Default card | Rounded-rect sprite — **2nd most common border**, used for storage/tavern/quest cards |
| `object_border_dim_white_extra_opaque.xml` | `extra_opaque_background` | 1dp `dim_white` | 10dp all | Dialog section bg | Rounded-rect sprite, higher fill opacity |
| `object_border_dim_white_square_no_border.xml` | `standard_background` | none | none | — | Plain flat-color `Image`, no border needed |
| `object_border_gray.xml` | `standard_background` | 1dp `grey_border` | 10dp all | — | Rounded-rect sprite, gray variant |
| `object_border_no_background.xml` | none | 1dp `dim_white` | 10dp all | Help text box | `Image` with fill alpha=0, stroke-only sprite |
| `object_border_no_background_no_border.xml` | none | none | 10dp all | — | Invisible container, no visual needed |
| `object_border_rounded_down.xml` | `standard_background` | 1dp `dim_white` | 10dp bottom only | — | Rounded-rect sprite, bottom-corners-only |
| `object_border_rounded_left.xml` | `standard_background` | 1dp `dim_white` | 10dp left only | Portrait border | Rounded-rect sprite, left-corners-only |
| `object_border_rounded_left_ascended.xml` | `ascended_background` | 1dp `ascended_unit` | 10dp left only | Ascended portrait | Same as above, ascended palette |
| `object_border_rounded_right.xml` | `standard_background` | 1dp `dim_white` | 10dp right only | — | Rounded-rect sprite, right-corners-only |
| `object_border_rounded_right_ascended.xml` | `ascended_background` | 1dp `ascended_unit` | 10dp right only | Ascended | Same, ascended palette |
| `object_border_unavailable.xml` | `standard_background_unavailable` (#1fff0000) | 1dp `dim_white` | 10dp all | Locked/unaffordable card | Rounded-rect sprite, red-tinted fill |
| `object_border_unavailable_rounded_right.xml` | `standard_background_unavailable` | 1dp `dim_white` | 10dp right only | Locked, right-corners | Same, right-only |
| `object_empty_adventurer.xml` | none | none | none, 60×60dp | Empty party slot | `Image` placeholder / "+" icon container, fixed size 60×60 |
| `object_empty_space_8dp.xml` | none | none | none, 8dp height | Spacer | `LayoutElement` (min height 8) or `VerticalLayoutGroup` spacing |
| `object_empty_space_8dp_horizontal.xml` | none | none | none, 8dp width | Spacer | `LayoutElement` (min width 8) or `HorizontalLayoutGroup` spacing |
| `offline_dialog_background.xml` | `#ffffff` | none | 16dp (uniform `radius`) | Offline progress popup | Rounded-rect sprite, distinct from `dialog_border` (white fill, larger radius) |
| `rarity_border_common.xml` | `standard_background` | 2dp `border_rarity_common` (#80969696) | 10dp all | Item rarity: Common | Rounded-rect sprite — **not currently used** per handoff (`backgroundFromRarity()` returns uniform border), keep for reference only |
| `rarity_border_epic.xml` | `standard_background` | 2dp `border_rarity_epic` (#80ff6400) | 10dp all | Item rarity: Epic | Same, not currently used |
| `rarity_border_legendary.xml` | `standard_background` | 2dp `border_rarity_legendary` (#80ff0000) | 10dp all | Item rarity: Legendary | Same, not currently used |
| `rarity_border_rare.xml` | `standard_background` | 2dp `border_rarity_rare` (#bf3200ff) | 10dp all | Item rarity: Rare | Same, not currently used |
| `rarity_border_uncommon.xml` | `standard_background` | 2dp `border_rarity_uncommon` (#8032ff32) | 10dp all | Item rarity: Uncommon | Same, not currently used |
| `test_custom_background.xml` | black | none | none | — | Test/dev leftover — **not a game asset**, ignore |
| `test_level_drawable.xml` | `primary_dark_material_dark` | none | 10dp uniform | — | Test/dev leftover — **not a game asset**, ignore |

**Practical Phase 2+ note:** only ~6 distinct rounded-rect "shapes" actually appear (10dp-all, 10dp-left, 10dp-right, 10dp-bottom, 10dp-square, 200dp-circle). A single 9-sliced rounded-rect sprite + a circle sprite covers all 36 files; only the `Image.color` (fill) and a thin stroke child change per variant.

---

## 2. Vector Icon Drawables (49) — path-based icons

Android `VectorDrawable` XML cannot be imported into Unity directly (no native VectorDrawable support). Each real game icon below needs either (a) hand-redrawn as a PNG/SVG sprite matching the described path, or (b) exported once from Android Studio's vector preview to PNG and imported as a normal sprite. Framework/Material/Google-Sign-In/AdMob icons are marked **IGNORE** — they belong to bundled libraries, not the game's own art, and have no gameplay UI usage in the rebuild.

| File | Size | Tint | Alpha | Status | Unity Equivalent |
|---|---|---|---|---|---|
| `arrow_down_white.xml` | small | white | 1.0 | **Game icon** | Redraw as sprite — collapse/expand chevron |
| `arrow_left.xml` | small | default | 1.0 | **Game icon** | Redraw as sprite — pagination |
| `arrow_right.xml` | small | default | 1.0 | **Game icon** | Redraw as sprite — pagination |
| `arrow_up_brass.xml` | small | brass_border | 1.0 | **Game icon** | Redraw as sprite — sort ascending |
| `arrow_up_white.xml` | small | white | 1.0 | **Game icon** | Redraw as sprite — sort ascending (alt color) |
| `autofeed_icon.xml` | small | — | 1.0 | **Game icon** | Redraw as sprite — pet autofeed heart |
| `bottom_nav_castle.xml` | 24dp | selector-tinted | — | **Game icon** | Redraw as sprite — Headquarters tab icon |
| `brass_circle.xml` | small | brass_border | 1.0 | **Game icon** | Redraw as sprite — filled dot/bullet |
| `check_brass.xml` | small | brass_border | 1.0 | **Game icon** | Redraw as sprite — checkmark |
| `delete.xml` | small | default | 1.0 | **Game icon** | Redraw as sprite — delete/trash |
| `drawer_icon_cloud.xml` | 24dp | white | 1.0 | **Game icon** | Redraw as sprite — Cloud Save drawer item |
| `drawer_icon_redeem_code.xml` | 24dp | white | 1.0 | **Game icon** | Redraw as sprite — Redeem Code drawer item |
| `drawer_icon_settings.xml` | 24dp | white | 1.0 | **Game icon** | Redraw as sprite — Settings drawer item |
| `help.xml` | small | default | 1.0 | **Game icon** | Redraw as sprite — "?" help button |
| `info.xml` | small | default | 1.0 | **Game icon** | Redraw as sprite — "i" info button |
| `lock_close.xml` | 40dp | default | 1.0 | **Game icon** | Redraw as sprite — locked (opaque) |
| `lock_close_transparent.xml` | 40dp | white | 0.65 | **Game icon** | Redraw as sprite — locked (dimmed, e.g. locked doctrine ability) |
| `lock_open.xml` | 40dp | default | 1.0 | **Game icon** | Redraw as sprite — unlocked |
| `menu_drop.xml` | 36×24dp | white | 0.75 | **Game icon** | Redraw as sprite — collapse chevron (double, down) |
| `menu_lift.xml` | 36×24dp | white | 0.75 | **Game icon** | Redraw as sprite — expand chevron (double, up) |
| `offline_dialog_default_icon_42dp.xml` | 42dp | default | 1.0 | **Game icon** | Redraw as sprite — fallback icon in Offline Progress popup |
| `plus_white_half_alpha.xml` | 48dp | white | 0.50 | **Game icon** | Redraw as sprite — empty-slot "+" |
| `sign_minus.xml` | small | default | 1.0 | **Game icon** | Redraw as sprite — quantity stepper minus |
| `sign_plus.xml` | small | default | 1.0 | **Game icon** | Redraw as sprite — quantity stepper plus |
| `sign_plus_white.xml` | small | white | 1.0 | **Game icon** | Redraw as sprite — quantity stepper plus (alt color) |
| `swap_equipment.xml` | 24dp | brass_border | 0.75 | **Game icon** | Redraw as sprite — circular refresh/swap arrows |
| `vector_menu.xml` | 24dp | white | 1.0 | **Game icon** | Redraw as sprite — hamburger menu |
| `launcher_background.xml` | — | — | — | **App icon** | Not a UI element — app/launcher icon background layer only |
| `launcher_foreground.xml` | — | — | — | **App icon** | Not a UI element — app/launcher icon foreground layer only |
| `_avd_hide_password__0_res_0x7f080000.xml` | — | — | — | IGNORE | AndroidX password-visibility widget, unused (no password field in game) |
| `_avd_show_password__0_res_0x7f080003.xml` | — | — | — | IGNORE | Same as above |
| `admob_close_button_black_circle_white_cross.xml` | — | — | — | IGNORE | AdMob SDK boilerplate — ads are Phase-later scope |
| `admob_close_button_white_circle_black_cross.xml` | — | — | — | IGNORE | AdMob SDK boilerplate |
| `admob_close_button_white_cross.xml` | — | — | — | IGNORE | AdMob SDK boilerplate |
| `btn_checkbox_checked_mtrl.xml` | — | — | — | IGNORE | Material Components framework checkbox, not custom art |
| `btn_checkbox_unchecked_mtrl.xml` | — | — | — | IGNORE | Material Components framework |
| `btn_radio_off_mtrl.xml` | — | — | — | IGNORE | Material Components framework |
| `btn_radio_on_mtrl.xml` | — | — | — | IGNORE | Material Components framework |
| `ic_call_answer.xml` | — | — | — | IGNORE | AndroidX telecom/notification framework icon, unused |
| `ic_call_answer_low.xml` | — | — | — | IGNORE | Same |
| `ic_call_answer_video.xml` | — | — | — | IGNORE | Same |
| `ic_call_answer_video_low.xml` | — | — | — | IGNORE | Same |
| `ic_call_decline.xml` | — | — | — | IGNORE | Same |
| `ic_call_decline_low.xml` | — | — | — | IGNORE | Same |
| `ic_clock_black_24dp.xml` | — | — | — | IGNORE | AndroidX framework icon, unused |
| `ic_keyboard_black_24dp.xml` | — | — | — | IGNORE | AndroidX framework icon, unused |
| `ic_m3_chip_check.xml` | — | — | — | IGNORE | Material3 Chip component framework |
| `ic_m3_chip_checked_circle.xml` | — | — | — | IGNORE | Material3 Chip component framework |
| `ic_m3_chip_close.xml` | — | — | — | IGNORE | Material3 Chip component framework |

**Real game icon count: 27** (+2 app-icon-only layers). Everything else in this list (20 files) is bundled-library boilerplate — safe to permanently ignore for the rebuild.

---

## 3. Layer-List / Selector / Gradient / Animated-Vector / ObjectAnimator / Ripple (32) — all framework, no game art

All 32 remaining game-specific-flagged XML drawables are **Android/Google/Material framework boilerplate**, not custom Guild Master art. None require Unity recreation.

| Type | Files | Reason |
|---|---|---|
| `layer-list` (16) | `common_google_signin_btn_icon_*` (5), `common_google_signin_btn_text_*` (5), `m3_appbar_background.xml`, `m3_popupmenu_background_overlay.xml`, `m3_tabs_background.xml`, `m3_tabs_rounded_line_indicator.xml`, `m3_tabs_transparent_background.xml`, `object_border_no_top.xml` | Google Sign-In SDK button states + Material3 framework chrome. `object_border_no_top.xml` is the one borderline case — a 3-layer variant of the standard border with the top edge omitted; if a screen turns out to need it in Phase 2, recreate as a rounded-rect sprite with top corners removed (same family as `object_border_rounded_down`). |
| `selector` (5) | `bottom_nav_color_selector.xml`, `common_google_signin_btn_icon_dark/light.xml`, `common_google_signin_btn_text_dark/light.xml` | State-list resources. `bottom_nav_color_selector.xml` is the one game-relevant file — defines active/inactive tab tint (brass_border when selected, dim gray when not). In Unity: `Button` states / `Image.color` swap on `Toggle`/tab selection, not a sprite asset. |
| `gradient` (1) | `_launcher_foreground__0_res_0x7f080006.xml` | App launcher icon foreground gradient layer — not in-game UI. |
| `animated-vector` (4) | `btn_checkbox_checked_to_unchecked_mtrl_animation.xml` + 3 more | Material checkbox/radio transition animations — framework only. |
| `objectAnimator` (4) | `_avd_hide_password__*`, `_avd_show_password__*` | AndroidX password-toggle animation frames — unused (no password UI in game). |
| `ripple` (2) | `m3_radiobutton_ripple.xml`, `m3_selection_control_ripple.xml` | Material ripple touch-feedback — Unity has its own `Selectable` transition system, no asset needed. |

---

## Summary

| Category | Count | Action |
|---|---|---|
| Shape (border/panel) | 36 | Documented above — build ~6 reusable rounded-rect sprite variants in Phase 2 |
| Vector — real game icon | 27 | Documented above — redraw each as a Unity sprite in Phase 2 |
| Vector — app icon only | 2 | Not a UI element, skip |
| Vector — framework/SDK boilerplate | 20 | Permanently ignore |
| Layer-list / selector / gradient / animated-vector / objectAnimator / ripple | 32 | Permanently ignore (1 borderline: `bottom_nav_color_selector.xml` tab-tint reference, `object_border_no_top.xml` shape family) |
| **Total game-specific XML audited** | **117** | Matches `deep_xml_drawables.csv` GameSpecific=True count exactly |
