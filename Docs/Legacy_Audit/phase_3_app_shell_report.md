# Phase 3 — App Shell Report

**Date:** 2026-08-04
**Scope:** Replace the flat 8-button HUD with a legacy-accurate App Shell (Top HUD + Currency Bar, 4 tabs, Navigation Drawer, PopupRoot), built at 1080×1920, using only Phase 1/2 assets. No feature screens, no new backend.

---

## 1. Files created / modified

**Created:**
- `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs` — tab/drawer/popup runtime controller, HUD refresh
- `Assets/_Game/Scripts/Runtime/UI/Shell/TabPlaceholderView.cs` — placeholder label for the 4 tab panels
- `Assets/_Game/Scripts/Runtime/UI/Shell/LegacyCurrencyAdapter.cs` — Money → Platinum/Gold/Silver/Copper display adapter (UI-only, no backend change)
- `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellBuilder.cs` — idempotent procedural shell builder (`Tools/Guild Master/Legacy UI/Build App Shell`)
- `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellScreenshotTool.cs` — Play Mode screenshot capture (4 menu items), since no screenshot tool exists in the mcp-unity toolset
- `Docs/Legacy_Audit/Asset_Gallery/phase_3_headquarters_shell.png`, `phase_3_adventurers_shell.png`, `phase_3_drawer_open.png`, `phase_3_popup_layer.png`
- `Docs/Legacy_Audit/phase_3_app_shell_report.md` — this report
- `D:\Tinh\Backups\Legacy_UI_Phase_3_App_Shell\` — backup (git state + `Main.unity.bak` + `UIRuntimeBootstrap.cs.bak`, taken before any edit)

**Modified:**
- `Assets/_Game/Scenes/Main.unity` — added `AppShellCanvas` GameObject tree (root #4, alongside the 3 existing roots — nothing removed)
- `Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs` — added an 8-line conditional block after the existing `_ui.ShowScreen(UIScreenId.MainHUD)` call: if an `AppShellController` exists in the scene, initialize it and hide (not destroy) the legacy `HUDController` GameObject. Zero lines removed; if no shell exists, behavior is byte-for-byte unchanged from before Phase 3.

**Not modified:** every other existing screen (Tavern/Inventory/Character/Craft/Merchant/Dungeon/Quest/Settings), every service/model, `SaveData`.

---

## 2. Hierarchy (as built in Main.unity)

```
AppShellCanvas (Canvas sortingOrder=100, CanvasScaler 1080×1920 ScaleWithScreenSize 0.5, GraphicRaycaster)
├── TabContentRoot (stretch, offset for TopHUD/BottomNav)
│   ├── Tab_Headquarters (TabPlaceholderView) — active by default
│   ├── Tab_Adventurers (inactive)
│   ├── Tab_Dungeons (inactive)
│   └── Tab_Raids (inactive)
├── TopHUD (190px tall, top-anchored)
│   ├── MenuButton (☰, text fallback — no vector_menu sprite in Phase 1 set)
│   ├── ScreenTitle (updates per active tab)
│   ├── GemsIcon (gem.png) + GemsText
│   ├── 4× [coin icon + text] — Platinum/Gold/Silver/Copper (coin_platinum/gold/silver/copper.png)
│   └── 4× Tooltip labels (Shop/Msg/Merchant/Quests — text-only, no sprite assets for these exist)
├── BottomNav (150px tall, bottom-anchored)
│   └── 4× NavCell (Headquarters/Adventurers/Dungeons/Raids) — Button + icon + label
├── DrawerRoot (inactive by default)
│   ├── DrawerBackdrop (Button, closes drawer on outside click)
│   └── DrawerPanel (800px wide, left-anchored)
│       └── 10× DrawerItem (Shop, Settings, Recall Adventurers, Messages, FAQ, Bestiary, Achievements, Cloud Save, Redeem Code, Community)
├── PopupRoot (stretch, empty by default, highest sibling index before AppShellController)
│   └── PopupBackdrop (inactive by default, full-screen semi-transparent block)
└── AppShellController (wires everything above)
```

---

## 3. Backend bindings

| HUD element | Source | Method |
|---|---|---|
| Gems | `SaveService.CurrentData.Gems` (long) | Read directly, no adapter needed |
| Platinum/Gold/Silver/Copper | `SaveService.CurrentData.Money` (long) | **`LegacyCurrencyAdapter.FromMoney()`** — implements the recovered legacy formula exactly (`Platinum = money/1,000,000`, `Gold = (money%1,000,000)/10,000`, `Silver = (money%10,000)/100`, `Copper = money%100`, per `06_Decode_Formula_Ledger.md`). Backend only stores a single `Money` long — no `SaveData` field was added or changed. |

No other backend reads. `RefreshHud()` is public — future phases can call it after any money/gem-changing action.

Drawer items (Shop, Settings, Recall Adventurers, Messages, FAQ, Bestiary, Achievements, Cloud Save, Redeem Code, Community) are **inert placeholders** — clicking any of them only closes the drawer. No dialogs exist yet for these (explicitly out of Phase 3 scope per instruction 8), including "Settings" even though `SettingsScreen.cs` already exists in the old system — wiring that crossover is left for Phase 4 to keep this phase's diff minimal and unambiguous.

---

## 4. Navigation behavior (implemented exactly as specified)

| Requirement | Implementation |
|---|---|
| Switch tab via Bottom Nav | `AppShellController.SwitchTab(int)`, wired to each of the 4 `NavCell` buttons |
| All 4 tabs instantiated, only active toggled | All 4 `Tab_*` panels exist permanently under `TabContentRoot`; `SwitchTab` only calls `SetActive` — never destroys/recreates |
| State preserved across tab switches | Direct consequence of the above — panels are never destroyed |
| Drawer opens via hamburger | `MenuButton.onClick → OpenDrawer()` |
| Drawer closes via item click or outside tap | Every `DrawerItem` button and the full-screen `DrawerBackdrop` button both call `CloseDrawer()` |
| Popup blocks interaction below | `PopupBackdrop` (full-screen `Image` + implicit raycast blocking as a sibling drawn above tab content) is activated whenever a popup opens |
| No duplicate popup | `AppShellController.OpenPopup()` checks `_currentPopup != null` and refuses (logs a warning) if one is already open — verified: see `AppShellScreenshotTool.CapturePopupLayer` which calls `OpenPopup` once per run, no double-open path exists in Phase 3 code |

---

## 5. Compile / runtime verification

| Check | Result |
|---|---|
| Unity compile | **0 errors, 0 warnings** (verified 3 times via `recompile_scripts`, after each script batch) |
| Play Mode entry | Successful — `[UIRuntimeBootstrap] Phase 3 App Shell found — set as entry UI, legacy HUD hidden (not deleted).` logged |
| 4 tabs switch | Verified — `SwitchTab(0)` and `SwitchTab(1)` both exercised live in Play Mode (see screenshots), title + active panel + icon tint all update correctly |
| Drawer open/close | Verified — `OpenDrawer()` exercised live, all 10 items rendered correctly, panel occupies left ~74% of screen |
| PopupRoot layer | Verified — a real popup instance opened via `OpenPopup()` renders above HUD + tab content (see `phase_3_popup_layer.png`) |
| HUD at 1080×1920 | No clipping/overflow observed in any of the 4 captured states — Canvas uses the project's existing `ScaleWithScreenSize` convention (same as the rest of the game) |
| Buttons not overlapping | Confirmed visually — 4 bottom nav cells evenly split (270px each), 4 HUD tooltip labels right-aligned with 136px spacing, no visual overlap in any screenshot |

**1 pre-existing error observed, unrelated to Phase 3 (flagging per instructions, not silently fixing):**
```
Some objects were not cleaned up when closing the scene. (Did you spawn new GameObjects from OnDestroy?)
The following scene GameObjects were found:
Canvas
Main Camera
```
This fired on Play Mode entry, **before** any Phase 3 log line, and names generic `Canvas`/`Main Camera` objects — none of which match any object Phase 3 created (all Phase 3 objects are specifically named `AppShellCanvas`, `TopHUD`, etc.). This is very likely a pre-existing `Boot.unity → Main.unity` scene-transition artifact. Recommend a separate investigation outside Phase 3 scope.

---

## 6. Known limitations (honest gaps, not silently patched)

- **Hamburger menu icon, tooltip icons (Shop/Messages/Merchant/Quests), and the Headquarters bottom-nav icon are text-only.** No bitmap sprite exists for `vector_menu`, and no dedicated sprite exists for the 4 tooltip icons or `bottom_nav_castle` — all three are unconverted XML vectors per `unity_legacy_shape_mapping.md` (Phase 1 explicitly did not convert vectors to sprites). Not invented art — plain text labels instead, exactly per "không tự tạo phong cách mới."
- **Active-tab icon tinting is hard to see on the real bottom-nav PNG icons** (`bottom_nav_adventurers/dungeons/raids`) — their source art is already near-black, so multiplying by the brass tint color still reads as dark. The tint logic itself is correct (verified via the placeholder square, which responds to the color swap clearly); this is a source-art contrast limitation, not a logic bug. Worth revisiting in Phase 4 with a background highlight behind the icon instead of/alongside tinting.
- **Popup visual contrast is subtle** — the test popup's panel fill color (`CardviewDarkBackground`) is close to the tab-content background, so the modal reads mainly from its text and backdrop dimming rather than a strong border. Functionally correct (verified topmost + blocking), cosmetic polish deferred.
- **Screenshots were captured via a custom Editor tool** (`ScreenCapture.CaptureScreenshot` driven from Play Mode), not a dedicated mcp-unity screenshot tool — none exists in the current toolset. This exercises real runtime behavior (actual button-equivalent method calls), not a static mockup.

---

## 7. Rollback steps

1. Delete `Assets/_Game/Scripts/Runtime/UI/Shell/` (3 files)
2. Delete `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellBuilder.cs` and `AppShellScreenshotTool.cs`
3. In `Assets/_Game/Scripts/Runtime/Boot/UIRuntimeBootstrap.cs`, remove the block starting with `// Phase 3 (App Shell):` (8 lines) — or simply restore from `D:\Tinh\Backups\Legacy_UI_Phase_3_App_Shell\UIRuntimeBootstrap.cs.bak`
4. Restore `Assets/_Game/Scenes/Main.unity` from `D:\Tinh\Backups\Legacy_UI_Phase_3_App_Shell\Main.unity.bak` (or manually delete the `AppShellCanvas` GameObject in the Unity Editor and re-enable the legacy HUD GameObject if it was left disabled)
5. `Tools > Guild Master/Legacy UI/Import Legacy Assets` and Phase 2 tools are untouched — no further action needed there

Legacy HUD/screens were never deleted — only hidden (`SetActive(false)`) — so re-enabling `HUDController`'s GameObject alone (without any other change) restores the old flat-button entry UI.

---

## 8. Explicitly NOT done (out of Phase 3 scope, per instructions)

- No Tavern, Storage, Character details, Dungeon details, Merchant, Craft, Pet, or Quest screen content was built
- No new backend/service/model was added — `LegacyCurrencyAdapter` is a pure display-layer function, reads one existing field
- No drawer item opens a real dialog yet (all 10 are inert placeholders)
- Phase 4 (Headquarters content) was not started
