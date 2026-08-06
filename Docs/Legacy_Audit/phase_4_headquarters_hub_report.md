# Phase 4 — Headquarters Hub Report

**Date:** 2026-08-04
**Scope:** Replace the Headquarters tab placeholder with a ScrollView of 6 building cards (Quarters/Tavern/Storage/Market/Workshop/Shelter), bound to live backend data, each opening a minimal placeholder popup on click.

---

## 1. Files created / modified

**Created:**
- `Assets/_Game/Scripts/Runtime/UI/Shell/BuildingCardView.cs` — one card's view (title/status/sign icon/NEW indicator)
- `Assets/_Game/Scripts/Runtime/UI/Shell/SimplePopupPanel.cs` — minimal popup (title + Close), reused for all 6 buildings
- `Assets/_Game/Scripts/Runtime/UI/Shell/HeadquartersHubController.cs` — reads backend, populates cards, opens popups with singleton guard
- `Assets/_Game/Scripts/Editor/UI/Legacy/HeadquartersHubBuilder.cs` — idempotent builder (`Tools/Guild Master/Legacy UI/Build Headquarters Hub`)
- `Docs/Legacy_Audit/Asset_Gallery/phase_4_headquarters_hub.png`, `phase_4_headquarters_scrolled.png`, `phase_4_building_popup.png`
- `Docs/Legacy_Audit/phase_4_headquarters_hub_report.md` — this report
- `D:\Tinh\Backups\Legacy_UI_Phase_4_Headquarters_Hub\` — backup (git state + `Main.unity.bak` + `AppShellController.cs.bak`)

**Modified:**
- `Assets/_Game/Scenes/Main.unity` — `Tab_Headquarters` gained a `HeadquartersHubRoot` (ScrollView + 6 cards) and a `PopupTemplate` child; the Phase 3 placeholder `Label` is hidden (`SetActive(false)`), not deleted
- `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs` — added one `[SerializeField] HeadquartersHubController` field and a 4-line call to `Initialize()` at the end of `AppShellController.Initialize()`. No existing line changed.
- `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellScreenshotTool.cs` — added 3 more menu items (05/06/07) for this phase's screenshots, reusing Phase 3's `DelayFrames` capture pattern

**Not modified:** every service/model/SaveData, Adventurers/Dungeons/Raids tabs, Drawer, any existing screen from before Phase 3.

---

## 2. Layout (matches legacy P2-01)

Each card: 140px tall, `StandardBackground` fill + `DimWhite` outline border (from `LegacyUITheme`, same as Phase 2/3), sign icon overlaid left, title (bold, centered), status text below title, NEW indicator (hidden — see §3). All 6 sit inside a `VerticalLayoutGroup` + `ContentSizeFitter` under a `ScrollRect`/`Viewport`/`Content`, so the hub scrolls if content ever exceeds the visible area.

---

## 3. Backend bindings

| Card | Current | Capacity/Max | Source |
|---|---|---|---|
| Quarters | `Character.GetAllCharacters().Count` | `Tavern.GetQuartersCapacity()` | Live: **0/3** |
| Tavern | `Tavern.GetGuests().Count` | `Tavern.GetTavernCapacity()` | Live: **1/1** |
| Storage | `Inventory.GetAllItems().Count` | `Inventory.GetCapacity()` | Live: **1/36** |
| Workshop | `Craft.GetQueue().Count` | `Craft.GetQueueCapacity()` | Live: **0/1** |
| Shelter | `Pet.GetAllPets().Count` | — (no capacity getter exposed by `IPetService`) | Live: **"0 pet(s)"**, count-only, no denominator invented |
| Market | — | — | No `MarketService` exists — card shows **"Coming soon"**, click still opens the same placeholder popup, no functionality implied |

**NEW indicator:** built into `BuildingCardView` (a hideable "NEW" label) but never shown — no backend service among the 6 exposes an "unseen/new" boolean (checked `ITavernService`, `IInventoryService`, `ICraftService`, `IPetService`; none has one). Per instruction 2 ("NEW indicator nếu backend có trạng thái") this is correctly omitted, not faked.

---

## 4. Click behavior

- Each card's `Button` → `HeadquartersHubController.OpenBuildingPopup(featureId)` → clones the shared `PopupTemplate`, sets its title to the building name, calls `AppShellController.OpenPopup()` (Phase 3's existing blocking/duplicate-prevention logic — unchanged).
- Popup content in this phase: title + `(placeholder — Phase 5+ will build real content here)` + Close button. No Tavern/Storage/Workshop/Shelter dialog logic was built (per instruction 6).
- **Singleton guard:** `HeadquartersHubController` tracks the currently-open `featureId`; clicking the same card again while its popup is open is refused (logged, no duplicate). Clicking a *different* card while one is open is also blocked — by Phase 3's `AppShellController.OpenPopup()`, which refuses any second popup regardless of source.
- Close button → `AppShellController.ClosePopup()` (same backdrop-hide/blocking-release path as Phase 3).

---

## 5. Compile / runtime verification

| Check | Result |
|---|---|
| Unity compile | **0 errors, 0 warnings** (one hiccup: newly created `.cs` files had no `.meta` yet after creation — `recompile_scripts` alone didn't pick them up; fixed by running `Assets/Refresh` first, then recompiling clean) |
| Play Mode entry | Successful, same `[UIRuntimeBootstrap] Phase 3 App Shell found` log as before, no new errors from Phase 4 code |
| 6 cards render | Confirmed — `phase_4_headquarters_hub.png` shows all 6 with correct titles, real sign icons, live status text |
| Scroll | `ScrollRect`/`Viewport`/`Content` present and functional; **with only 6 cards at 140px + spacing, content (~1000px) fits entirely within the visible area (~1580px) — nothing to actually scroll.** `phase_4_headquarters_scrolled.png` looks identical to the top screenshot for this reason (verified by setting `verticalNormalizedPosition` to 0 vs 1), not a bug. |
| Click → popup | Confirmed — clicked `Card_Tavern`'s real `Button.onClick` (same code path as a tap), popup opened correctly above the cards (`phase_4_building_popup.png`) |
| Popup blocks background | Confirmed — same `PopupBackdrop` mechanism from Phase 3, reused unchanged |
| No duplicate popup | Confirmed by code path — `OpenBuildingPopup` checks `_shell.IsPopupOpen` before creating a new instance |
| 1080×1920 fit | No clipping/overflow in any of the 3 screenshots |

**Same pre-existing error as Phase 3, still unrelated:** `"Some objects were not cleaned up when closing the scene... Canvas, Main Camera"` fires on every Play Mode entry, before any Phase 3/4 log line — confirmed again this run, not caused by Headquarters Hub code (no object Phase 4 creates is named `Canvas` or `Main Camera`).

**Scene-reload gotcha (same as Phase 3, noted for future phases):** the Unity Editor's active scene reverts to `LegacyShapeTest.unity` after every script recompile/domain reload. Each time before running a scene-modifying menu item or entering Play Mode, `Main.unity` had to be explicitly reloaded via `load_scene` and the change re-verified with `grep` on the saved `.unity` file before proceeding — otherwise edits silently land in (or are lost from) the wrong scene, as happened once in Phase 3.

---

## 6. Known limitations (honest gaps, not silently patched)

- **Card fill color reads brighter/more opaque than the legacy game's subtle translucent panels.** `StandardBackground` (`#1fffffff`, ~12% alpha) combined with the UI `Outline` component renders more solid gray than intended in this Editor/Player combination. Functionally correct (uses the exact theme color, no invented color), but a cosmetic mismatch worth revisiting alongside the Phase 3 popup-contrast note.
- **Market card is fully inert** beyond opening the same generic placeholder popup — accurately reflects "no MarketService exists yet," not a bug.
- **Shelter card has no capacity denominator** — `IPetService` doesn't expose one; showing a fabricated number was avoided per instruction 6 (no backend changes).

---

## 7. Rollback steps

1. Delete `Assets/_Game/Scripts/Runtime/UI/Shell/BuildingCardView.cs`, `SimplePopupPanel.cs`, `HeadquartersHubController.cs`
2. Delete `Assets/_Game/Scripts/Editor/UI/Legacy/HeadquartersHubBuilder.cs`
3. In `AppShellController.cs`, remove the `_headquartersHub` field and the 4-line `if (_headquartersHub != null)` block — or restore from `D:\Tinh\Backups\Legacy_UI_Phase_4_Headquarters_Hub\AppShellController.cs.bak`
4. In `AppShellScreenshotTool.cs`, remove the 3 added menu items (05/06/07) — optional, harmless if left
5. Restore `Assets/_Game/Scenes/Main.unity` from `D:\Tinh\Backups\Legacy_UI_Phase_4_Headquarters_Hub\Main.unity.bak`, or manually delete `HeadquartersHubRoot` + `PopupTemplate` under `Tab_Headquarters` and re-enable its `Label` child

No backend/service/model/save file was touched — rollback is purely deleting/reverting the files above.

---

## 9. CHANGES REQUESTED fix — Popup Close Fix (2026-08-04, same day)

### Bug report
All 6 building cards opened a popup correctly, but the Close button did nothing — 100% reproducible, all 6 cards, at 1080×1920 Play Mode.

### Audit performed
Read `SimplePopupPanel.cs`, `HeadquartersHubController.cs`, `AppShellController.OpenPopup()`/`ClosePopup()`, the `PopupRoot`/`PopupBackdrop` hierarchy built by `AppShellBuilder.cs`, the Close button's listener wiring in `HeadquartersHubBuilder.BuildPopupTemplate()`, and `GraphicRaycaster`/`Image.raycastTarget` on every layer between the click and the button.

### Root cause (confirmed exact)
`AppShellController.OpenPopup()`:
```csharp
// BEFORE (bug)
popupInstance.transform.SetAsLastSibling();
popupInstance.SetActive(true);
if (_popupBackdrop != null)
{
    _popupBackdrop.SetActive(true);
    _popupBackdrop.transform.SetSiblingIndex(popupInstance.transform.GetSiblingIndex());
}
```
`SetSiblingIndex(popupInstance.GetSiblingIndex())` moves the **backdrop** to the popup's current index. Under Unity's sibling-list semantics, removing the backdrop from its old slot and reinserting it at the popup's index pushes the popup back one slot — so the **backdrop ends up rendered/hit-tested above the popup**, not below it. `PopupBackdrop`'s `Image.raycastTarget` (default `true`, intentionally — it's supposed to block clicks to the tab content underneath) then intercepts **every** click over the popup area, including the Close button, before it can reach the button. Not a missing listener, not a null callback, not an `interactable=false` issue, not a wrong parent — pure sibling-order/raycast-layering bug, and it affected all 6 cards identically because they all share the exact same `OpenPopup()` code path.

Secondary issue found in the same audit: `ClosePopup()` only called `SetActive(false)`, never destroying the popup instance. Since every popup is a fresh `Instantiate()` per open (`HeadquartersHubController.OpenBuildingPopup`), this left one inactive orphan per building type accumulating under `PopupRoot` forever.

### Exact fix
`Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs` — `OpenPopup()`: reorder — push backdrop to last sibling **first**, then push the popup to last sibling **after**, guaranteeing the popup is always the topmost (and therefore first-hit-tested) sibling regardless of prior state:
```csharp
if (_popupBackdrop != null)
{
    _popupBackdrop.SetActive(true);
    _popupBackdrop.transform.SetAsLastSibling();
}
popupInstance.transform.SetAsLastSibling();
```
`ClosePopup()`: `Destroy(_currentPopup)` instead of `SetActive(false)`, eliminating orphan accumulation.

**Files modified:** `Assets/_Game/Scripts/Runtime/UI/Shell/AppShellController.cs` only (2 methods, both already existed — no new files, no visual/backend/card-layout changes).

### Test result — 6/6 PASS

Added a temporary Editor test menu (`Tools/Guild Master/Legacy UI/Test/Test All 6 Building Popups`, in `AppShellScreenshotTool.cs`) that drives each card's real `Button.onClick` and the resulting popup's real Close `Button.onClick`, in Play Mode, and checks `AppShellController.IsPopupOpen` before/after:

| Card | Opened | Close button found | Closed after click |
|---|---|---|---|
| Quarters | PASS | PASS | PASS |
| Tavern | PASS | PASS | PASS |
| Storage | PASS | PASS | PASS |
| Market | PASS | PASS | PASS |
| Workshop | PASS | PASS | PASS |
| Shelter | PASS | PASS | PASS |
| Reopen a different card immediately after | | | PASS |

**Orphan check:** first same-frame check via `GameObject.Find` initially reported a false negative — `Destroy()` in Unity is deferred to end-of-frame, so an object destroyed this frame is still findable until the frame ends. Re-verified after letting 2 frames pass (via the existing `DelayFrames` screenshot helper): inspected `PopupRoot` directly — **only `PopupBackdrop` remains as a child; all 6 `Popup_*` instances were fully destroyed, zero orphans.**

Compile: 0 errors, 0 warnings. Play Mode: no new red errors (same pre-existing unrelated `Canvas`/`Main Camera` scene-cleanup error noted in §5, confirmed again unrelated).

**TOTAL: 6/6 cards pass the full open → Close → gone → reopen-different cycle.**

---

## 8. Explicitly NOT done (out of Phase 4 scope, per instructions)

- No real Tavern/Storage/Workshop/Shelter dialog content — all 6 open the same placeholder panel
- No backend/service/model/save change — `HeadquartersHubController` only reads existing getters
- Adventurers/Dungeons/Raids tabs untouched
- No existing UI deleted
- Phase 5 not started
