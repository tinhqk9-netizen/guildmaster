# Phase 5E — Shelter Dialog report

Date: 2026-08-05

## Scope

Implemented the Shelter dialog + Pet Detail overlay only. Quarters, Tavern, Storage, Workshop, App Shell, Headquarters Hub layout, services, models, SaveData, and formulas were not redesigned or changed. The only existing runtime controller change is the additive Shelter prefab branch in `HeadquartersHubController` and its refresh wiring — the same pattern already used for Quarters/Tavern/Storage/Workshop.

## API audit — source of truth

Audited (read-only, no edits):

- `Assets/_Game/Scripts/Runtime/Services/PetService.cs` (contains both `IPetService` and `PetService` — no separate interface file exists)
- `Assets/_Game/Scripts/Definitions/PetDefinition.cs`
- `Assets/_Game/Scripts/Runtime/Save/SaveData.cs` (`PetSaveData`)
- `Assets/_Game/Scripts/Runtime/Formulas/FormulaService.cs`, `IFormulaService.cs` (`ShelterCapacity`, `GetShelterPrice`, `GetShelterAutofeedPrice`)
- `Assets/StreamingAssets/GameData/pets.json` (raw data, to verify which `PetDefinition` fields are actually populated)

### `IPetService` — confirmed real API surface

```csharp
IReadOnlyList<PetSaveData> GetAllPets();
PetSaveData CreatePet(string definitionId, string ownerCharacterId);
void AddExp(string instanceId, long amount);
bool LevelUp(string instanceId);
bool EquipToCharacter(string petInstanceId, string characterInstanceId);
bool UnequipFromCharacter(string petInstanceId);
IReadOnlyList<PetSaveData> GetCharacterPets(string characterInstanceId);
bool HasPetEquipped(string characterInstanceId);
float GetAttackBonus/DefenseBonus/HpBonus/SpeedBonus(string characterInstanceId);
```

**No capacity getter.** `FormulaService.ShelterCapacity(levelShelter, upgradeShelter)` is a real, populated formula, but `IPetService` never calls or exposes it — confirmed by grepping the full interface/implementation. This matches the finding already on record in the Phase 4 report ("IPetService exposes no capacity getter — count-only, no denominator invented"), carried forward here.

**No Favourite field/API anywhere.** Grepped the entire runtime tree for "Favourite"/"Favorite" — zero matches. Not a UI gap; the concept does not exist in this backend at all.

**No Autofeed per-pet state.** There is a *global* `SaveData.LevelShelterAutofeed` field and a real `FormulaService.GetShelterAutofeedPrice(levelShelterAutofeed)` formula (an all-or-nothing shelter-wide unlock, not a per-pet toggle), but **no `IPetService` (or any other service) method ever reads or writes `LevelShelterAutofeed`** — grepped for any assignment or service call, found none. The formula and field exist but are completely unwired, mirroring Workshop's unwired `GetSecondsToCraft`/`UpgradeWorkshopTime` situation from Phase 5D.

**No Feed action.** No method named anything like `Feed`/`FeedPet` exists anywhere in the codebase.

**No capacity-upgrade flow.** `FormulaService.GetShelterPrice(levelShelter)` is a real cost formula, but no service method increments `SaveData.LevelShelter`. Per instruction 6 ("Upgrade capacity nếu backend có flow hoàn chỉnh"), the flow is incomplete — not built.

**No Breeding API.** Grepped for "breed"/"Breeding" — zero matches anywhere in the runtime. Not built, per instruction 6 ("Breeding chỉ dựng nếu API và data flow thật sự đầy đủ").

**No Equip/Unequip action wired**, even though `EquipToCharacter`/`UnequipFromCharacter` are real, complete APIs — instruction 6's action checklist (Favourite/Autofeed/Feed/Upgrade capacity/Breeding) does not include pet-to-character equip, and `Equip` would additionally need a character-selection mechanism that doesn't exist in the Shelter/Pet-Detail context (the same gap already documented for Storage's item Equip in Phase 5C). The real, currently-equipped state (`EquippedToCharacterId`) is still shown as read-only info in Pet Detail, since it's a real, populated field.

### Critical finding: `PetDefinition` is almost entirely unpopulated

`Assets/StreamingAssets/GameData/pets.json` has **21 pets**, and every single entry has **only the `DefinitionBase` fields** (`className`, `id`, `parseReasons`, `parseStatus`, `recordHash`, `sourcePath`) — verified by enumerating every key across all 21 records. None of `PetDefinition`'s own fields (`PetName`, `BaseAttack`, `BaseDefense`, `BaseMaxHp`, `BaseSpeed`, the 4 multipliers, `ExpToLevel`, `SkillDefinitionId`, `EvolutionDefinitionId`, `EvolutionLevel`, `VisualPrefab`) has any assignment anywhere in `Database/DatabaseBuilder.cs` (grepped — no enrichment step exists for pets, unlike items/adventurers). **Every pet in the game therefore has identical, default stat/name/skill/evolution data** — `PetName` is always the literal string `"Unnamed Pet"`, `BaseMaxHp` is always `50`, `BaseSpeed` is always `10`, everything else is `0`/empty.

Consequence, per instruction 5 ("Không hiển thị N/A giả. Field nào không có hoặc không được populate thì bỏ."):
- `PetName` is **not shown** (always the same placeholder string for every pet — showing it would misrepresent unpopulated data as real per-pet content). The formatted `DefinitionId` (e.g. `beetle` → "Beetle") is shown instead, the same convention already used for items/guests in prior phases.
- Stats (`BaseAttack`/`BaseDefense`/`BaseMaxHp`/`BaseSpeed`) are shown **only when they differ from their unpopulated C# default** (`0`/`0`/`50`/`10`). With the current data this means they never render for any pet.
- `SkillDefinitionId` and `EvolutionDefinitionId` are shown only when non-empty — currently always empty, so they never render either.
- `Level` and `Exp` (real, on `PetSaveData`) are always shown. The EXP denominator uses `PetDefinition.ExpToLevel` if `> 0`, else falls back to `100` — mirroring `PetService.AddExp`/`LevelUp`'s own real fallback logic (`if (expNeeded <= 0) expNeeded = 100`), not an invented number.

## Shelter Dialog — what's actually shown

- Title ("Shelter")
- Pet count text: `{GetAllPets().Count} pet(s)` — no capacity denominator (see above)
- 5-column pet grid (`GetAllPets()` order): sprite via `LegacySpriteRegistry.GetSprite(DefinitionId)` (falls back to a dim placeholder if missing — the catalog convention established in prior phases), formatted name, `Lv.{Level}`
- No Favourite/Autofeed indicator on the grid — neither concept exists in the backend (see audit)
- Empty state when `GetAllPets().Count == 0`
- Close button

## Pet Detail overlay

Child overlay of `ShelterDialog` (`LayoutElement.ignoreLayout = true`, never a second `AppShellController.OpenPopup` call — instruction 7). Shows:
- Icon (same sprite lookup as the grid)
- Name (formatted `DefinitionId`)
- `Level: {Level}`
- `EXP: {Exp} / {100 or real ExpToLevel}`
- `Stats: ...` — only if any stat differs from its unpopulated default (currently never, per the data finding above)
- `Skill: ...` — only if `SkillDefinitionId` is non-empty (currently never)
- `Evolves into ... at Lv.X` — only if both `EvolutionDefinitionId` and `EvolutionLevel > 0` (currently never)
- `Assigned to adventurer #{id}` / `Not assigned to any adventurer` — real, from `EquippedToCharacterId`
- Close button only — no actions (Favourite/Autofeed/Feed/Breeding/Equip all absent per the audit; nothing was invented to fill the gap)

## Popup lifecycle

Shelter opens through the existing `AppShellController.OpenPopup`/`PopupRoot`, with the same singleton guard, backdrop layering, and destroy-on-close behavior already proven by Quarters/Tavern/Storage/Workshop. The Pet Detail overlay is a child of the Shelter dialog itself, so it can never conflict with the shell's one-popup-at-a-time guard, blocks the grid beneath it (full-cover overlay image), and closing Shelter destroys everything (including any open Pet Detail) in one `Destroy()` call.

## Implementation

Created:

- `Assets/_Game/Scripts/Runtime/UI/Headquarters/ShelterDialog.cs` — dialog controller (pet grid, count text, empty state, Close), plus a small internal `PetFormat` helper for id → display-name formatting.
- `Assets/_Game/Scripts/Runtime/UI/Headquarters/PetDetailPanel.cs` — read-only pet detail overlay (see fields above).
- `Assets/_Game/Scripts/Editor/UI/Legacy/ShelterDialogBuilder.cs` — Editor prefab builder (`Tools/Guild Master/Legacy UI/Build Shelter Dialog`), idempotent, wires `HeadquartersHubController._shelterDialogPrefab` into `Main.unity`.
- `Assets/_Game/Prefabs/UI/Headquarters/ShelterDialog.prefab`.

Modified:

- `Assets/_Game/Scripts/Runtime/UI/Shell/HeadquartersHubController.cs` — added `_shelterDialogPrefab` field and a Shelter popup branch in `OpenBuildingPopup`, following the exact Quarters/Tavern/Storage/Workshop pattern. The existing Shelter card (`RefreshCards`) was not changed — it already read `GetAllPets().Count` with no invented capacity, confirmed still correct.
- `Assets/_Game/Scripts/Editor/UI/Legacy/AppShellScreenshotTool.cs` — added Phase 5E test/verification menu item (`5E Shelter Full Flow`) plus a new shared `RegressionCheck` helper (also used by Phase 5F's test). Purely additive — the existing Storage/Workshop regression-check methods from earlier phases were left untouched, not refactored.
- `Assets/_Game/Scenes/Main.unity` — builder serialized the Shelter prefab reference into `HeadquartersHubController`.

No service, model, `SaveData`, or `FormulaService` file was modified.

## Backend calls / action flow

Read-only dialog: `Refresh()` calls `IPetService.GetAllPets()` and rebuilds the grid; `PetDetailPanel.Show(pet)` resolves the pet's `PetDefinition` via `ServiceContainer.Database.TryGet<PetDefinition>` and renders. **No action exists** (per the audit — no API to wire), so there is no mutation path, no `onStateChanged` trigger beyond the dialog's own `Setup`, and nothing to refresh after an action because no action can be taken.

## Compile and test result

- Unity batchmode recompile: **0 errors, 0 warnings.**
- Prefab build (`ShelterDialogBuilder`): completed successfully, Main scene wiring applied, idempotent.
- Runtime full-flow test (`AppShellScreenshotTool.TestShelterDialogFlow`), fresh Play Mode, `Main.unity`, natural save (never hacked):
  - `Popup_shelter` opened via the Shelter card: **0 pet(s)** (naturally empty in this save — the fresh-game bootstrap in `UIRuntimeBootstrap` only creates starter characters, not pets).
  - Empty state correctly shown and captured.
  - No pets existed, so Pet Detail was not exercised — logged clearly (`"No pets in current save — Pet Detail not exercised."`), not faked.
  - Shelter Close → `IsPopupOpen=False`, `orphanExists=False`.
  - Regression check: Quarters, Tavern, Storage, and Workshop all opened/closed normally.
- No new red console errors. The one pre-existing error (`Some objects were not cleaned up when closing the scene... Canvas / Main Camera`) predates all Phase 5 work and is unrelated.

### Test coverage / not exercised

- **Pet Detail rendering with a real pet** was not exercised — the current save has zero pets. The rendering logic (conditional stat/skill/evolution lines, EXP fallback) was code-reviewed against the real `PetDefinition`/`PetSaveData` field semantics but not visually confirmed with actual pet data.
- No action exists to test (see audit) — this is not a gap in testing, it reflects that no actionable API exists.

## Screenshots

- [Shelter dialog — empty (naturally, save has 0 pets)](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5e_shelter.png)
- [Shelter dialog — empty state explicitly confirmed](D:/Tinh/Rebuild_GuildMaster/Docs/Legacy_Audit/Asset_Gallery/phase_5e_shelter_empty.png)

`phase_5e_pet_detail.png` was not produced — no pet exists in the current save to click. Per instruction 13 ("Không hack SaveData để ép state... ghi rõ nhánh chưa test được"), this is disclosed rather than forced.

## Rollback

Restore the pre-Phase-5E/5F files from:

`D:\Tinh\Backups\Legacy_UI_Phase_5EF_Shelter_Market\`

(contains `Scripts_Before/` and `Scenes_Before/` — full snapshots of `Assets/_Game/Scripts` and `Assets/_Game/Scenes` taken before any Phase 5E/5F edit). To roll back Shelter specifically: restore those two folders over the current ones (this also reverts Market — see that report if only one should be rolled back), delete `Assets/_Game/Prefabs/UI/Headquarters/ShelterDialog.prefab` (+ `.meta`), and recompile.

## Known limitations

- **`PetDefinition` carries almost no real per-pet data** — every field beyond `id` is an unpopulated C# default for all 21 pets (verified against `pets.json`). Name, stats, skill, and evolution info are structurally identical across every pet until the data pipeline is extended with a pet-enrichment step (mirroring `EnrichItemDefinition`/`ItemFieldsLoader` for items). This is a data-source gap, not a UI defect.
- **No capacity denominator shown** — `IPetService` exposes no capacity getter, even though `FormulaService.ShelterCapacity` is real and populated.
- **No Favourite, Autofeed, Feed, capacity-upgrade, or Breeding functionality** — none of these have any backing API in the current backend (see audit for the specific grep evidence for each).
- **No Equip/Unequip pet-to-character action** — the real API exists (`EquipToCharacter`/`UnequipFromCharacter`), but wasn't requested by this phase's action checklist, and `Equip` specifically has no character-selection UI to drive it from Shelter's context. Available for a future phase.
- **Pet Detail was not visually verified against real pet data** — the current save owns no pets.

Phase 5E stops here (paired with Phase 5F in the same request — see `phase_5f_market_dialog_report.md`). No Phase 6 work was started.
