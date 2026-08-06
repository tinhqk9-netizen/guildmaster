# Phase 5A — Quarters Dialog takeover report

Date: 2026-08-04

## Scope

This takeover was limited to the Quarters dialog layout. No Tavern, Phase 5B, service, model, SaveData, formula, or gameplay code was changed.

## State at takeover

- The previous Claude edit was present before this session.
- `UpgradeSection` already had a `ContentSizeFitter` with vertical `PreferredSize`.
- The dialog root already had `VerticalLayoutGroup` and `ContentSizeFitter` with vertical `PreferredSize`.
- Both `UpgradeButton` and `CloseButton` were already being added by the builder and existed in the generated prefab.
- The runtime backend flow in `QuartersDialog.cs` was retained unchanged.
- `Phase5Tester` was not present under `Assets`; the existing `QuartersDialogWirer` is an editor wiring helper and was retained.

## Root cause

The nested layout had fit-to-content, but its children were not height-controlled by the `VerticalLayoutGroup`, and the outer dialog still retained a fixed design-time height (`720 x 620`). That combination allowed the nested section's runtime preferred height to disagree with the parent layout calculation. The result was the lower Close action being laid out into the same visual area as the Upgrade section.

Evidence from the pre-fix builder/prefab:

- `UpgradeSection` had `childControlHeight: false` and `childForceExpandHeight: true`.
- The root dialog had `childControlHeight: false`.
- The dialog retained a fixed vertical `sizeDelta` of 620 while also using `ContentSizeFitter`.

## Exact fix

In `QuartersDialogBuilder.cs`:

1. Let the root `ContentSizeFitter` own the vertical size by changing the root height to zero in the builder's initial `sizeDelta`.
2. Set the root `VerticalLayoutGroup.childControlHeight = true`.
3. Set `UpgradeSection.childControlHeight = true`.
4. Set `UpgradeSection.childForceExpandHeight = false`.
5. Keep `UpgradeSection.ContentSizeFitter.verticalFit = PreferredSize`.

The prefab was rebuilt through the existing menu:

`Tools/Guild Master/Legacy UI/Build Quarters Dialog`

## Files modified

- `Assets/_Game/Scripts/Editor/UI/Legacy/QuartersDialogBuilder.cs`
- `Assets/_Game/Prefabs/UI/Headquarters/QuartersDialog.prefab` (rebuilt by the builder)
- `Docs/Legacy_Audit/phase_5a_quarters_dialog_report.md`

Not modified:

- `Assets/_Game/Scripts/Runtime/UI/Headquarters/QuartersDialog.cs`
- `Assets/_Game/Prefabs/UI/Legacy/LegacyCurrencyView.prefab`
- service/model/SaveData/formula files
- scene, HUD, card, or App Shell runtime code

## Audit confirmation after rebuild

- Root dialog has `ContentSizeFitter` and `VerticalLayoutGroup`.
- `UpgradeSection` has `VerticalLayoutGroup`, `LayoutElement`, and `ContentSizeFitter`.
- `UpgradeButton` and `CloseButton` each have one `UnityEngine.UI.Button` component.
- The prefab contains one `Upgrade` label; no duplicate `UPGRADE` text was found.
- No temporary runtime debug log was added. The builder's existing build log remains an editor build status log.
- `LegacyCurrencyView.prefab` retains its existing `HorizontalLayoutGroup` and `ContentSizeFitter`.

## Compile result

Unity recompile result: **0 warnings, 0 errors**.

The generated prefab rebuild completed with:

`[QuartersDialogBuilder] QuartersDialog + LegacyCurrencyView prefabs rebuilt (Phase 5A redesign).`

## Fresh Play Mode result at 1080×1920

A fresh Play Mode session was started after stopping the previous session and loading `Assets/_Game/Scenes/Main.unity`.

Runtime hierarchy evidence after opening Quarters:

- `Popup_quarters` is active under `PopupRoot`.
- Root has `ContentSizeFitter` and `VerticalLayoutGroup`.
- `UpgradeSection` has `VerticalLayoutGroup` and `ContentSizeFitter`.
- `UpgradeButton` runtime rect: height 84, centered at y 823.
- `CloseButton` runtime rect: height 68, centered at y 659.
- The two rects are separated; no overlap was observed.
- Close flow log: `IsPopupOpen=False, orphanExists=False`.
- After close, `PopupRoot` contains only the inactive `PopupBackdrop`.

### Upgrade limitation in this test save

The loaded save has 20 copper and the Quarters upgrade price is 75 copper. The Upgrade button is therefore correctly disabled by the existing affordability check. No SaveData or backend mutation was used to manufacture currency.

Therefore:

- Upgrade success / money decrease / capacity increase: **not executed in this natural save state**.
- Disabled affordability state: **verified**.
- Close and popup cleanup: **verified**.
- Backend flow was not changed and remains wired through `GetQuartersCapacity()`, `GetUpgradeQuartersPrice()`, and `UpgradeQuarters()`.

The existing automated flow logged unchanged values (`Current Capacity: 3`, card `0/3`) because the affordability guard correctly prevented the upgrade.

## Screenshots

- [Before upgrade attempt](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5a_quarters_before_upgrade.png)
- [After upgrade attempt](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5a_quarters_after_upgrade.png) — affordability-disabled state; no upgrade mutation occurred.
- [Disabled state](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5a_quarters_disabled.png)

## Regression test status

- Quarters popup open: passed.
- Layout overlap check: passed.
- Close button: passed.
- Popup destroy/orphan check: passed.
- Disabled affordability state: passed.
- Upgrade success and post-upgrade persistence: **blocked by the natural test save having insufficient currency**.
- Full automated Unity verification was started, but the MCP test-run request timed out before a reliable final result was returned; it is not reported as passed.

One pre-existing scene-transition error was observed during bootstrap:

`Some objects were not cleaned up when closing the scene ... Main Camera, Canvas`

It was not introduced by the Quarters layout edit and is recorded separately from the Quarters result.

## Rollback

Restore the backed-up files from:

`D:\Tinh\Backups\Legacy_UI_Phase_5A_Quarters_Claude_Redesign\`

The backup contains the pre-takeover builder, prefab, runtime Quarters files, currency prefab/runtime file, wiring helper, and the pre-change status record. After restoration, reimport/rebuild through Unity and recompile scripts.

Phase 5A stops here. No Phase 5B work was started.
