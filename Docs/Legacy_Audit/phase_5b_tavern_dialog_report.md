# Phase 5B — Tavern Dialog report

Date: 2026-08-05

## Scope

Implemented the Tavern dialog only. Quarters, Storage, Workshop, Shelter, Market, Adventurers, Dungeons, Raids, services, models, SaveData, and formulas were not redesigned or changed. The only existing runtime controller change is the additive Tavern prefab branch and its refresh wiring.

## API audit — source of truth

Audited:

- `Assets/_Game/Scripts/Runtime/Services/ITavernService.cs`
- `Assets/_Game/Scripts/Runtime/Services/TavernService.cs`
- `Assets/_Game/Scripts/Runtime/UI/Tavern/TavernScreen.cs`
- `Assets/_Game/Scripts/Runtime/UI/Shell/HeadquartersHubController.cs`
- `Assets/_Game/Scripts/Runtime/Save/SaveData.cs`
- `Assets/_Game/Scripts/Definitions/AdventurerDefinition.cs`

Confirmed API semantics:

- Guest data is `CharacterSaveData`.
- Guest list is `GetGuests()`.
- Tavern capacity is `GetTavernCapacity()`.
- Quarters capacity is `GetQuartersCapacity()`.
- Timer is `GetNextVisitorTimerSeconds()`.
- Recruitment gate is `CanRecruit()` plus `RecruitGuest(index, out CharacterRuntime)`.
- `RecruitGuest` has no price argument and `ITavernService` exposes no recruit-price method. The current backend recruitment is therefore rendered as `Recruit: Free`; the UI does not subtract currency.
- `CharacterSaveData` has `DefinitionId`, `Level`, `Trait`, and weapon/save fields, but no display-name or rarity field. The UI formats `DefinitionId` as the guest name/class and displays `Trait` when present. Rarity is not invented.
- `TavernService.CanRecruit()` checks owned character count against Quarters capacity. The dialog disables Recruit when that backend gate is false or the guest definition is invalid.

## Implementation

Created:

- `Assets/_Game/Scripts/Runtime/UI/Headquarters/TavernDialog.cs`
- `Assets/_Game/Scripts/Editor/UI/Legacy/TavernDialogBuilder.cs`
- `Assets/_Game/Prefabs/UI/Headquarters/TavernDialog.prefab`
- Generated `.meta` files for the new script/prefab.

Modified:

- `Assets/_Game/Scripts/Runtime/UI/Shell/HeadquartersHubController.cs`
  - Added `_tavernDialogPrefab`.
  - Added Tavern popup branch through `OpenPopup`.
  - Close calls `ClosePopup`, clears active popup state.
  - Successful recruitment callback calls `RefreshCards()` and `RefreshHud()`.
- `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellScreenshotTool.cs`
  - Added a Phase 5B runtime verification menu and screenshot flow.
  - Added delayed capture frames because `ScreenCapture.CaptureScreenshot` is asynchronous.
- `Assets/_Game/Scenes/Main.unity`
  - Builder serialized the Tavern prefab reference into `HeadquartersHubController`.

The builder is idempotent and was executed in Unity batchmode. The guest viewport uses `RectMask2D` and `ScrollRect` for reliable mobile uGUI clipping.

## Visual hierarchy

`TavernDialog`

- Title
- Summary: Guests count/capacity + Quarters occupancy/capacity
- Visitor timer/full state
- `GuestScroll`
  - `Viewport` + `RectMask2D`
  - `GuestContent` + `VerticalLayoutGroup` + `ContentSizeFitter`
  - Runtime guest cards
    - legacy frame
    - unit portrait from `LegacySpriteRegistry.GetUnitSprite(...)`
    - name/class, level, traits, recruit state
    - Recruit button
- Close button

## Runtime behavior tested

Fresh Play Mode at 1080×1920, Main scene:

- Tavern card opens `Popup_tavern` through PopupRoot.
- Guest list rendered with real `unit_footman` portrait and real guest data.
- Guest card is visible inside the scroll viewport.
- Current natural save state: `Guests 1/1`, `Quarters 5/5`.
- Recruit button disabled because Quarters is full.
- No currency was modified by the UI.
- Close result: `IsPopupOpen=False`, `orphanExists=False`.
- App Shell bootstrap and database build completed successfully.

Recruit success was not tested because the natural save has full Quarters. No SaveData hack was used. The service success path is wired to `RecruitGuest` and the post-success refresh callback, but requires a naturally available Quarters slot for runtime acceptance.

The existing bootstrap scene-transition error was observed:

`Some objects were not cleaned up when closing the scene ... Canvas / Main Camera`

It predates the Tavern implementation and was not caused by the dialog code.

## Compile and test result

- Unity batchmode builder: completed successfully.
- Unity script recompile: **0 warnings, 0 errors**.
- Runtime Tavern disabled/full-capacity flow: passed.
- Popup close/destroy cleanup: passed.
- Portrait and guest card render: passed after forcing the first runtime canvas/layout pass and using `RectMask2D`.
- Automated B2 EditMode command was launched, but Unity exited after asset/domain initialization without producing a test-results XML; it is therefore not reported as passed.
- Quarters backend/runtime code was not modified. Phase 5A's user-verified upgrade remains the source-of-truth regression baseline.

## Screenshots

- [Tavern dialog](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5b_tavern.png)
- [Tavern guest card](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5b_tavern_guest.png)
- [Tavern disabled/full Quarters state](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5b_tavern_disabled.png)
- `phase_5b_tavern_after_recruit.png` was not created because the natural save could not recruit while Quarters was full.

## Known limitations

- Backend has no recruit-price API; UI correctly shows Free rather than inventing a cost.
- Backend has no separate guest display-name or rarity fields; UI uses formatted `DefinitionId` and real `Trait`.
- Full success-path recruitment and post-recruit HUD/card delta remain pending a natural save with an available Quarters slot.
- The pre-existing scene-transition cleanup error remains outside Phase 5B.

## Rollback

Restore the pre-Phase-5B files from:

`D:\Tinh\Backups\Legacy_UI_Phase_5B_Tavern\`

Then remove the Tavern prefab/script references and recompile. The backup includes the previous `HeadquartersHubController`, Main scene, Quarters prefab/runtime files, LegacyCurrencyView prefab, and Quarters builder baseline.

Phase 5B stops here. No Storage or later phase was started.
