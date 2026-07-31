# 08 — Global UI Design System

Grounded in the actually-existing `UITemporaryTheme.cs` and `UICardFactory.cs` (keep and extend, do not discard — the existing constants are a real, if minimal, starting design system). All values below are placeholder-visual-language, not final art, per task constraints.

## Layout grid / safe area
Reference resolution is portrait 1080×1920, `ScaleMode=ScaleWithScreenSize`, `MatchWidthOrHeight=0.5` (confirmed `Main.unity:18576-18582`). `SafeArea.cs` already applies `Screen.safeArea` to a root RectTransform every `Update()` — keep this, but throttle to only re-apply on an actual `Screen.safeArea` change event or a coarser poll (currently every frame with an equality check, acceptable but note as an easy optimization). All screen content must live inside the existing `SafeArea` root; HUD nav buttons currently use fixed `anchoredPosition` y-stacking (750→-300 in `UIWiringGenerator.cs`) rather than a layout group — replace with a `HorizontalLayoutGroup`/`GridLayoutGroup` bottom nav bar in Phase 0 to remove the aspect-ratio fragility.

## Typography hierarchy
Font is `Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")` (legacy Text component, not TextMeshPro) used everywhere — a real migration candidate (see `15_Scene_Prefab_Migration_Strategy.md`) but out of scope to change without a UI style decision (`17_User_Review_Decisions.md`). Sizes currently: `TitleFontSize`(34, used for H1 AND incorrectly reused for Dungeon's Active-panel sub-header — fix), `BodyFontSize`(22). Recommend adding: H2(28, for sub-headers like "Active Dungeon Title"), Caption(18, for card subtitles/timestamps), and enforcing the H1 size is never reused below the top screen title.

## Button hierarchy
Existing: `ButtonPrimary`/`ButtonSecondary` colors used by `UIScreenLayoutBuilder`-built ActionBar buttons; HUD nav buttons use raw `Color.gray`/`Color.black` from `UIWiringGenerator.cs:216,221` — this is a confirmed **visual inconsistency** (nav row doesn't match themed action buttons) to fix in Phase 0. Recommend 3 tiers: Primary (main CTA per screen — Craft, Recruit, Start Dungeon), Secondary (Upgrade/Sell/Claim), Tertiary/text (Cancel, Back, tab selectors).

## Cards
`UICardFactory.CreateCard` (Image+HorizontalLayoutGroup+Icon+TextColumn) is the one reusable primitive already shared by all 7 list-driven screens — keep as the base card component. Required extension: an interactable-state visual distinct from a "display-only, no listener" card (`isInteractable:false` currently renders indistinguishable from a real disabled button — a real accessibility/clarity gap). Icon is always `PlaceholderIcon` flat color (`UITemporaryTheme.cs:44`) — acceptable for placeholder visual language, but each card type (item/character/enemy/quest/recipe/market/pet) should use a distinct flat color or shape mnemonic, not one identical gray square everywhere.

## List rows / progress bars / timers
`UICardFactory.CreateProgressBar` exists but **is called by zero screens** — every percentage in the app (quest %, craft/sell timers, HP) is rendered as plain text. Phase 1 must actually wire this existing method into Quest/Craft/Merchant/Dungeon rather than build a new one. Timers (Tavern visitor countdown, Craft/Market queue, Dungeon combat) must not be purely animation-based per accessibility requirements — always pair a bar with a numeric "MM:SS remaining" text, and must live-refresh (today only Dungeon's Auto-Battle coroutine live-refreshes; every other timer is a stale snapshot until the next manual `Refresh()`).

## Badges / rarity / currency
No rarity-color system exists yet despite `ItemDefinition.Rarity` (int) being read from data — add a 5-tier rarity color badge (placeholder flat colors, e.g. gray/green/blue/purple/gold) applied consistently to item, adventurer-trait, and quest cards. Currency: Money and Gems need distinct icon-color pairing (currently text-only, no icon glyph assigned per `UICardFactory` notes above).

## Disabled/locked presentation
Currently disabled buttons just set `interactable=false` (grayed by Unity's default `ColorBlock`), with **no reason text anywhere** (confirmed: no screen shows "Need 500 gold" or "Requires Level 5" next to a disabled button). Add a locked-state pattern: dimmed card/button + a small lock icon + one-line reason text, reusable across Dungeon (chain-gate), Promotion (level-gate), Merchant (afford-gate).

## Success/warning/error states
Existing `UICardFactory.CreateFeedbackLabel` supports Success/Warning/Failure colors but usage is inconsistent: 3 screens (Dungeon, Tavern) use Success/Failure, 5 screens (Character, Craft, Inventory, Merchant, Quest) use Success/Warning, and several screens show unconditional "success" text regardless of the actual backend return value (`05_UI_Control_Binding_Audit.md`). Phase 0 must standardize on one 3-color feedback contract (Success/Warning/Error) applied only when the backend result is actually checked.

## Tooltip / confirm popup / empty state / loading state
No tooltip system exists anywhere. No dedicated Yes/No confirm popup exists (`PopupScreen` is OK-only) — build `ConfirmPopup` in Phase 0 (see `07_Information_Architecture.md`). Empty states exist inconsistently: Character shows "No adventurers recruited yet..." text but no CTA link to Tavern; most other screens' empty-list behavior is unverified without PlayMode (`UICardFactory.ClearContainer` on a null/empty list just leaves an empty container — RUNTIME_VERIFICATION_REQUIRED whether this ever null-refs on first launch with zero items in a category). No loading-state exists at all (`UIScreenId.Loading` has no class).

## Offline reward summary
No component exists (`OfflineProgressResult` model exists at the backend layer with `Success/DeltaSeconds/DispatchDeferred` fields but nothing on the UI side consumes it) — new component required, see `10_Screen_Design_Specifications.md`.

## Card specs (placeholder visual language only)
| Card type | Icon | Title | Subtitle | Badge | Interactable |
|---|---|---|---|---|---|
| Inventory item | flat-color square by category | Definition.id | Stack ×N | rarity color + locked icon | yes |
| Character/Adventurer | flat-color square by class | Definition.id, Level | HP/Trait | equipped-gear pips | yes |
| Enemy | flat-color square (red-tinted) | Definition.id | HP | none | no (display only — must look visually distinct from a real button, not just non-clickable) |
| Quest | flat-color square by doctrine | Definition.id | Progress bar + text | rarity | yes when completed |
| Recipe | flat-color square by output category | Output item name | Ingredient count | availability badge (accurate, not hardcoded) | yes |
| Market listing | flat-color square | Item name | Price/timer bar | sold/pending state | yes when claimable |
| Pet | flat-color square by pet type | PetName, Level | Equipped-to (or "Unassigned") | none | yes |

## Accessibility requirements
Readability: minimum 4.5:1 text contrast against the flat-color card backgrounds (verify each `UITemporaryTheme` color pair — RUNTIME_VERIFICATION_REQUIRED, cannot compute exact contrast ratios without rendering). No color-only signaling: every rarity/locked/success/failure state must pair color with an icon or text label, not color alone (current Success/Warning/Failure text-color-only pattern fails this — add icon glyphs in Phase 0). Touch target minimum 88×88px at reference resolution (current `HudButtonWidth/Height`=340×160 and `CardWidth/Height`=310×140 both comfortably exceed this — good, preserve). Clear selected state: card selection currently relies on... (RUNTIME_VERIFICATION_REQUIRED — `UICardFactory` was not confirmed to apply a distinct selected-state visual in the static read; flag for Phase 0 to add an explicit selection outline/background if absent). Timers must always show numeric text, never rely on animation alone (see above). Mobile support: touch-only interactions confirmed (no hover-dependent UI found in any of the 9 screens). No text clipping under longer localized strings: current fixed-width card layout (`CardWidth=310`) with `LegacyRuntime.ttf` has no confirmed text-overflow handling (RUNTIME_VERIFICATION_REQUIRED) — recommend auto-size or ellipsis truncation policy in Phase 0 given localization.json is currently empty and any future translated strings may be longer than English.
