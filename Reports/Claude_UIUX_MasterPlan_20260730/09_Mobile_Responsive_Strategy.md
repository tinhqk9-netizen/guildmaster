# 09 — Mobile / Responsive Strategy

## Confirmed current settings (read directly, not assumed)
- `CanvasScaler` (`Assets/_Game/Scenes/Main.unity:18576-18582`): `m_UiScaleMode=1` (Scale With Screen Size), `m_ReferenceResolution={1080,1920}` (portrait), `m_ScreenMatchMode=0` (Match Width Or Height), `m_MatchWidthOrHeight=0.5` (50/50 blend).
- `ProjectSettings.asset` (`D:\Tinh\Rebuild_GuildMaster\ProjectSettings\ProjectSettings.asset`): `defaultScreenOrientation: 0` (Auto Rotation), all four `allowedAutorotateTo*` flags = 1 (Portrait, PortraitUpsideDown, LandscapeLeft, LandscapeRight all enabled).
- **Direct conflict found**: reference resolution is explicitly portrait, but the project allows free rotation into landscape. No landscape-specific layout branch exists anywhere in the 9 screen classes or the 2 Editor generator scripts (`GuildMasterUnifiedApply.cs`, `UIWiringGenerator.cs`) — every fixed-pixel value (HUD nav button y-stack 750→-300, Dungeon grid cellSize 310×140, card dimensions) assumes portrait.

## Recommendation (User Review Decision — see 17)
Either (a) lock `defaultScreenOrientation` to Portrait to match the reference resolution and the entirely-portrait-authored layout, or (b) explicitly design and test a landscape variant. Given zero landscape-aware code exists today, (a) is the lower-risk default recommendation, but this is a project-level orientation decision requiring user sign-off, not something to silently change.

## Safe area / notch
`SafeArea.cs` already applies `Screen.safeArea` correctly to a root RectTransform. Keep this mechanism; ensure every new screen/component is parented under `SafeArea`, not directly under `UICanvas`.

## Aspect ratios
`MatchWidthOrHeight=0.5` is a reasonable general-purpose default but should be validated against the narrowest supported phone (e.g. 19.5:9 / 20:9 modern devices vs the 1080:1920 (9:16) reference) — RUNTIME_VERIFICATION_REQUIRED, cannot confirm actual letterboxing/cropping without the Editor's device simulator or a built player.

## Scroll / fixed header-footer
Existing `ContentScroll`/`ScrollRect`/`Viewport`/`Content` pattern (validated by `GuildMasterValidate.cs:129-136` as a required structure on every card-list screen) is sound and should be kept as the standard scroll container. HUD nav bar and each screen's Header/ActionBar should be explicitly fixed (non-scrolling) regions — confirmed current structure already separates Header/ContentScroll/DetailPanel/ActionBar as siblings (`GuildMasterValidate.cs:125-136`), so this is already correctly architected; just needs the HUD nav row converted from fixed-anchored-position to a proper fixed footer/header layout group (see `08_Global_UI_Design_System.md`).

## List virtualization
607 items (Inventory), 129 adventurers (roster), 321 recipes (Craft) are plausible list sizes. `UICardFactory.ClearContainer`+rebuild-from-scratch on every `Refresh()` (confirmed pattern across all 7 card-list screens) means a full inventory or recipe list rebuild happens on every single mutation — with no virtualization, a near-max-capacity Inventory or a mostly-populated recipe browser could visibly hitch on lower-end devices. Recommend evaluating simple virtualization (recycle visible cards only) for Inventory and Craft-Recipes specifically in Phase 3, RUNTIME_VERIFICATION_REQUIRED for actual frame-time impact.

## Modal sizing
No modal system with defined sizing exists yet (only the OK-only `PopupScreen`). New `ConfirmPopup`/reward popups (`08`) must be sized relative to safe area, not fixed pixel dimensions, to survive both portrait and (if adopted) landscape.

## Touch targets
Current button sizes (`HudButtonWidth/Height=340×160`, `CardWidth/Height=310×140`) comfortably exceed the 88×88px minimum at reference resolution — no changes needed here, just enforce the same minimums on any new components.

## One-hand reach / no hover-only UI
Confirmed: no hover-dependent interaction exists in any of the 9 screens (touch-only click handlers throughout). HUD nav row currently spans a large vertical range (y=750 to y=-300) via fixed anchored positions rather than a bottom-anchored bar — on a real portrait phone this places some nav buttons in the upper, harder-to-reach zone. Recommend converting to a bottom tab bar (industry-standard one-hand-reach pattern) in Phase 0's HUD rework.

## Android back button
No explicit `Input.GetKeyDown(KeyCode.Escape)` (Android back button maps to Escape in Unity) handling was found in any of the 9 screens, `UIService`, or `UIRuntimeBootstrap` in the files read — RUNTIME_VERIFICATION_REQUIRED to confirm absence project-wide (only the files explicitly listed for this task were read), but flagged as a likely gap: Android hardware back should invoke the same `UIService.Back()` used by the in-UI Back button, and is not currently wired anywhere found.

## Long-list performance
See "List virtualization" above — same finding, same recommendation.
