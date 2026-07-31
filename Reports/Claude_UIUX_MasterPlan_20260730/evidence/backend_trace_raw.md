# Backend Trace — Rebuild_GuildMaster (Static Analysis, Read-Only)

All paths relative to `D:\Tinh\Rebuild_GuildMaster\`. Line numbers refer to file state at time of read.

---

## 1. Boot / Startup

No dedicated "BootService". Composition root is `Assets/_Game/Scripts/Runtime/Services/ServiceContainer.cs`.
- `ServiceContainer(GameDatabase database, ISaveService saveService = null, IFormulaService formulaService = null, RuntimeFactory factory = null)` (ServiceContainer.cs:42-87) — constructs every service in a fixed order: Item → Inventory → Pet → Character → Equipment → Skill → StatusEffect → Craft → Merchant → Combat → TargetSelection → Loot → Doctrine → Promotion → Quest → Dungeon → Tavern → Settings → OfflineProgress → GameLoop.
- If no `ISaveService` passed, creates `new SaveService()` and calls `Load(out _)` (ServiceContainer.cs:51-56).
- `GameLoopService.Initialize()` (GameLoopService.cs:34-37) calls `ProcessOfflineCatchup()` — this is effectively the "boot" hook for game state.
- `DatabaseBuilder.Build()` (Database/DatabaseBuilder.cs:41-116) is the actual data-loading entry point (see §2), driven by `manifest.json`. Not shown invoked from ServiceContainer — GameDatabase is passed in externally (constructor requires it non-null, ServiceContainer.cs:48), so wiring of DatabaseBuilder→GameDatabase→ServiceContainer happens elsewhere (not in scope of Services/Models/Save/Formulas/Database/Definitions dirs read).

## 2. GameDatabase / Data Loading

- `DatabaseBuilder` class (Database/DatabaseBuilder.cs:11-162).
  - Constructor: `DatabaseBuilder(IGameDataProvider dataProvider, IJsonSerializer serializer, GameDatabase database)` (line 20).
  - `_categoryLoaders` dictionary maps manifest categories → typed loaders (lines 26-39): items→ItemDefinition, enemies→EnemyDefinition, skills→SkillDefinition, status_effects→StatusEffectDefinition, adventurers→AdventurerDefinition, pets→PetDefinition, recipes→RecipeDefinition, quests→QuestDefinition, dungeons→DungeonDefinition, raids→RaidDefinition.
  - `DatabaseBuildReport Build()` (line 41) reads `manifest.json`, iterates `manifest.files`, skips `localization`/`assets` categories explicitly (lines 81-87, "Handled by other services" — but no evidence of those other services in the read scope), loads each category file via its loader delegate, tracks errors/warnings/duplicateIds/recordCountMismatches.
  - `private void LoadCategory<T>(...)` (line 118) — deserializes `DefinitionFile<T>`, checks record count against manifest, flags duplicate ids, and for `T == EnemyDefinition` specifically parses drop tables out of raw JSON via `EnemyDropTableLoader.Apply(jsonContent, enemies)` (line 154) because `JsonUtility` cannot deserialize dictionaries — then calls `_database.RegisterCollection(list)` (line 158).
- `manifest.json` lists 10 categories with recordCounts: adventurers(129), dungeons(11), enemies(122), items(607), pets(21), quests(56), raids(12), recipes(321), skills(227), (truncated at line 80, likely also status_effects). `localization.json` and `assets_manifest.json` are both literally `[]` (empty arrays) — no localization/asset data present despite manifest categories referencing them.
- Every JSON data file's records carry decompile-audit metadata fields inherited from `DefinitionBase` (`id, className, parentClass, recordHash, parseStatus, manualRuleRequired, sourcePath, parseReasons`) — Definitions/DefinitionBase.cs:7-17.

## 3. Save / Load

File: `Assets/_Game/Scripts/Runtime/Save/SaveService.cs`, `SaveData.cs`.
- `SaveService : ISaveService` (SaveService.cs:7-144).
  - `bool HasSaveFile()` (line 22) — checks `File.Exists(_saveFilePath)`.
  - `bool Load(out Exception error)` (line 27) — reads `save.json` from `Application.persistentDataPath`, `JsonUtility.FromJson<SaveData>`, calls `loadedData.NormalizeAfterLoad()`, migrates if `SaveVersion < CurrentSaveVersion(=1)` via `MigrateSave` (line 137, placeholder only — no actual migration logic), falls back to `save_backup.json` on failure, then to `SaveData.CreateDefault()`.
  - `bool Save(out Exception error)` (line 91) — stamps `Metadata.SaveVersion/SaveTimeUnix/GameVersion`, `JsonUtility.ToJson(CurrentData, true)`, copies existing save to backup before overwrite, writes to `save.json`.
  - `void DeleteSave()` (line 124) — deletes both save + backup files, resets `CurrentData` to default.
  - `private void MigrateSave(SaveData oldData)` (line 137) — TODO/placeholder, only bumps version number, no field migration logic implemented.
- `SaveData` class (SaveData.cs:120-344) — the single flat persisted blob. Field groups:
  - Metadata: `SaveMetadata Metadata` (SaveVersion, SaveTimeUnix, GameVersion, DataVersion).
  - Currency: `long Money`, `long Gems`.
  - Storage: `LevelStorage`, `UpgradeStorage`.
  - Collections: `List<ItemSaveData> Items`, `List<CharacterSaveData> Characters`, `List<QuestSaveData> Quests`, `List<DungeonSaveData> Dungeons`, `List<SkillSaveData> Skills`, `List<PetSaveData> Pets`, `List<CharacterSaveData> TavernGuests`.
  - `ActiveDungeonSaveData ActiveDungeon` (referenced but the type itself is defined elsewhere — not in SaveData.cs; used by DungeonService with fields `DungeonDefinitionId, Progress, MaxProgress, LocalDarkness, AdventurerInstanceIds, PendingDrops, EncounterState (CombatEncounterSaveData: TurnsFighting, SavedActingEntityId, Enemies, Corpses), ActionState (DungeonActionState: Type, TurnsPassed)` — inferred from DungeonService.cs:112-193, not physically declared inside SaveData.cs itself, meaning `ActiveDungeonSaveData`/`CombatEncounterSaveData`/`DungeonActionState`/`EnemySaveData` are declared in a different file not read in this pass).
  - Purchase flags (inputs to formulas, not a real store): `StarterPackPurchased, AdventurerPackPurchased, MerchantPackPurchased, ImperialVanguardPurchased, UnholyCrusadePurchased` (lines 163-167).
  - Time markers: `LastAccess, LastHourTriggered, Last24Triggered, LastWeekTriggered` (lines 172-175) — `LastHourTriggered/Last24Triggered/LastWeekTriggered` declared but **no service in the read set writes/reads them** (dead fields).
  - Tavern/Quarters: `NextTavernVisit, TavernLocked, TutorialStep, TavernGuests, LevelQuarters, UpgradeQuarters, LevelTavernCapacity, UpgradeTavernCapacity, LevelTavernTime, UpgradeTavernTime` (lines 178-188). `TavernLocked` is declared but **never read or written by any service in scope** (dead field).
  - Workshop/Market/Shelter levels: `UpgradeWorkshopTime, LevelWorkshopQueue, UpgradeWorkshopQueue, UpgradeMarketTime, LevelMarketListings, UpgradeMarketQueue, LevelShelter, UpgradeShelter, LevelShelterAutofeed` (lines 193-201). **`LevelShelter/UpgradeShelter/LevelShelterAutofeed` have formula support (`IFormulaService.GetShelterPrice`, `GetShelterAutofeedPrice`, `ShelterCapacity`) but NO service class implements Shelter management** — no `ShelterService` exists among the read Services/*.cs files. Same for `LevelMarketListings/UpgradeMarketQueue` — `IFormulaService.MarketListings(...)` exists but no service calls it.
  - Doctrine progression: 8 doctrines × (Level+Progress) pairs (Affliction, Control, Fortitude, Grace, Illusion, Knowledge, Ruin, War) + `DoctrineMaxed` (lines 205-221) — fully backed by `DoctrineService`.
  - Merchant stock: `MerchantRegularStockItems, MerchantSpecialReserve, UniqueItemsLost, NewMerchantRegularItems, NewMerchantSpecialItems` (lines 225-229). `UniqueItemsLost, NewMerchantRegularItems, NewMerchantSpecialItems` are declared but **not referenced by MerchantService** (dead fields in the read scope).
  - Quest bookkeeping: `QuestsSeen, QuestsRefreshed, QuestsCompleted` (lines 232-234). `QuestsSeen`/`QuestsRefreshed` **not referenced by QuestService** (dead fields); `QuestsCompleted` is incremented in `QuestService.ClaimReward` (QuestService.cs:199).
  - Settings: 13 boolean/string toggles (lines 237-250) — fully backed by `SettingsService`.
  - Statistics: `ItemsCrafted, ItemsSold, MaxWealth, MaxAdventurerTier, MaxAdventurersOwned` (lines 253-257) — **none of these are written anywhere in the read Services/*.cs files** (all dead/unwired counters — no service increments ItemsCrafted on craft completion, no service tracks MaxWealth, etc.)
  - `GetPurchaseFlags()` (line 263) maps the 5 purchase bools into `Formulas.PurchaseFlags` struct for formula consumption.
  - `CreateDefault()` (line 282) seeds `Money=500`, `LevelStorage=1`, sound/music on, and one starting `CharacterSaveData{DefinitionId="Footman"}`.
  - `NormalizeAfterLoad()` (line 295) — null-safety pass for all list fields + per-character/per-pet normalization (`NormalizeCharacter`, `NormalizePet`).

## 4. Offline Progress

Two parallel/overlapping implementations exist:
- `GameLoopService.ProcessOfflineCatchup()` (GameLoopService.cs:39-66) — computes `elapsedSeconds` from `data.LastAccess` vs `DateTimeOffset.UtcNow`, caps at `Cap12Hours = 12*3600`, calls `_tavernService.ProgressVisitorTime(jMax)`, `_merchantService.ProgressMarket(jMax)`, `_craftService.ProgressWorkshop(jMax)`, then loops `for (i=0;i<jMax;i++) _dungeonService.Tick()` (line 61) — i.e. **ticks the dungeon once per elapsed second synchronously**, which for a 12-hour cap is up to 43,200 iterations. Sets `data.LastAccess = currentUnix` and saves.
- `OfflineProgressService.ApplyOfflineProgress(long currentUnix)` (OfflineProgressService.cs:31-62) — a **separate, apparently redundant/unused-by-GameLoopService** implementation using `_saveService.CurrentData.Metadata.SaveTimeUnix` (not `LastAccess`) as the reference point, dispatches only to Craft/Merchant (`_craftService.ProgressWorkshop(delta)`, `_merchantService.ProgressMarket(delta)`) and explicitly comments "Dungeon tick deferred: No safe background dungeon loop implemented yet. Combat / Quest offline logic deferred." (lines 50-51). This directly contradicts `GameLoopService.ProcessOfflineCatchup`, which DOES tick the dungeon offline. **No evidence either service calls the other; both are separately registered in `ServiceContainer`** (Container.OfflineProgress and Container.GameLoop both instantiated, ServiceContainer.cs:84-85) — ambiguous which one is the real offline-progress path, or if both run (double-processing risk).
- `IOfflineProgressService` (IOfflineProgressService.cs) exposes `CalculateOfflineDeltaSeconds`, `ApplyOfflineProgress`.
- `IGameLoopService` (IGameLoopService.cs) exposes `Initialize`, `ProcessOfflineCatchup`, `TickRuntime`.
- Model: `OfflineProgressResult` (Models/OfflineProgressResult.cs) — `Success, DeltaSeconds, DispatchDeferred`.
- SaveData fields used: `LastAccess` (GameLoopService path), `Metadata.SaveTimeUnix` (OfflineProgressService path) — **two different time-source fields for what should be one concept**.

## 5. Currency

No dedicated CurrencyService. `Money` (long) and `Gems` (long) live directly on `SaveData` (SaveData.cs:130-131) and are mutated inline by whichever service needs them:
- `CraftService.UpgradeQueueCapacity()` (CraftService.cs:180-192) debits `data.Money`.
- `MerchantService.BuyOffer` (MerchantService.cs:78-116) debits `data.Money` or `data.Gems` based on `offer.IsGems`.
- `MerchantService.ClaimSoldItem` (MerchantService.cs:166-185) credits `data.Money`.
- `TavernService.UpgradeQuarters/UpgradeTavernCapacity/UpgradeTavernTime` (TavernService.cs:217-254) debit `data.Money`.
- `QuestService.ClaimReward` (QuestService.cs:176-202) credits `data.Gems` (rarity ≥ 4) or calls `_doctrineService.AddProgress` otherwise.
- No caps/validation service, no transaction log, no `MaxWealth` stat tracking despite the field existing in SaveData (see §3).

## 6. Headquarters

No "Headquarters" concept/service/definition found anywhere in Services, Models, Definitions, or Save. **No backend representation.**

## 7. Tavern

`TavernService : ITavernService` (TavernService.cs:13-263).
- `int GetTavernCapacity()` (line 33) — reads `_formulaService.GetTavernCapacity(...)`.
- `int GetQuartersCapacity()` (line 39) — reads `_formulaService.GetQuartersCapacity(...)`.
- `long GetVisitorIntervalSeconds()` (line 45).
- `long GetNextVisitorTimerSeconds()` (line 52) — reads `data.NextTavernVisit`.
- `IReadOnlyList<CharacterSaveData> GetGuests()` (line 57) — reads `data.TavernGuests`.
- `bool CanRecruit()` (line 62) — compares owned character count vs `GetQuartersCapacity()`.
- `bool RecruitGuest(int index, out CharacterRuntime newCharacter)` (line 68) — removes guest at index, calls `_characterService.RecruitCharacter(guestData)`.
- `void GenerateVisitor()` (line 120) — rolls class/trait via `RollClass()`/`RollCommonTrait()`/`RollRareTrait()` (private, lines 83-109), special-cases tutorial steps 1/6/7, assigns a default starter weapon (`GetDefaultWeaponId`, line 111), inserts guest at index 0 of `data.TavernGuests`, trims list down to `GetTavernCapacity()` from the tail (also removing their weapon `ItemSaveData` from `data.Items`).
- `void ProgressVisitorTime(long deltaSeconds)` (line 197) — decrements `data.NextTavernVisit`, loops `GenerateVisitor()` while `<=0`.
- `UpgradeQuarters/UpgradeTavernCapacity/UpgradeTavernTime` (lines 217-254) — currency-gated level increments; **note none of these three call `_saveService.Save()`** (unlike Craft/Merchant equivalents), so upgrades persist only via whatever later calls `Save()` elsewhere.
- UI helper getters: `GetUpgradeQuartersPrice/GetUpgradeTavernCapacityPrice/GetUpgradeTavernTimePrice/GetQuartersLevel/GetTavernCapacityLevel/GetTavernTimeLevel` (lines 256-261).
- SaveData fields: `NextTavernVisit, TavernLocked(unused), TutorialStep, TavernGuests, LevelQuarters, UpgradeQuarters, LevelTavernCapacity, UpgradeTavernCapacity, LevelTavernTime, UpgradeTavernTime`.

## 8. Quarters

Folded into Tavern — `GetQuartersCapacity()` (TavernService.cs:39-43) and `LevelQuarters/UpgradeQuarters` fields govern how many adventurers can be owned (checked in `CanRecruit`, TavernService.cs:62-66). No separate Quarters service/definition. Formula: `IFormulaService.GetQuartersCapacity` / `GetQuartersPrice` (FormulaService.cs:61-92, 183-191).

## 9. Adventurers

- Definition: `AdventurerDefinition : DefinitionBase` (Definitions/AdventurerDefinition.cs:13-33) — fields: `MaxLevel, BaseMaxHp, BaseConstitution, BaseIntelligence, BaseDexterity, BaseDefense, BaseMagicDefense, WeaponType, ArmorType, NextClasses[], PassiveSkill, ActiveSkill, ManualRuleRequired_PotionDrinkerType`. Data file `adventurers.json` (129 records per manifest) — raw JSON also carries `baseStats` dict of per-field metadata (semanticTag etc.) which is decode-audit noise, not consumed by the C# model except via the flattened `BaseX` fields.
- Runtime: `CharacterRuntime` (Models/CharacterRuntime.cs:7-44) — `InstanceId, DefinitionId, Definition, Level, Experience, Weapon/Armor/Accessory (ItemRuntime), CurrentHp, CurrentMana, CurrentShield, PositiveStatusEffects, NegativeStatusEffects, IsAscended, AscensionLevel, Trait, PotionsDrank[6], ActiveSkillId, PassiveSkillId`.
- Service: `CharacterService : ICharacterService` (CharacterService.cs:13-326).
  - `CharacterRuntime CreateCharacter(string definitionId)` (line 108) — creates via `RuntimeFactory`, sets HP to max, adds to internal list, syncs to save.
  - `CharacterRuntime RecruitCharacter(CharacterSaveData saveData)` (line 122) — used by TavernService; hydrates equipment refs from InventoryService, initializes HP, adds to save.
  - `int GetTotalStat(CharacterRuntime character, StatType statType)` (line 163) — the central stat-computation method: applies promotion multiplier (`PromotionDefinition.StatMultiplier` matched by `TierIndex == AscensionLevel`, fallback `1.0+AscensionLevel*0.1`), legacy ascended ×1.5, doctrine bonuses (War→CON/DEX, Knowledge→INT, Fortitude/Ruin→HP, Fortitude→DEF, Illusion→MDEF, Grace≥2→double accessory), potion bonuses (`PotionsDrank` index mapping), equipment bonuses, trait multiplier (`GetTraitMultiplier`, line 270), and pet bonuses via `IPetService` (HP/Defense/Speed→Dexterity only — no pet Intelligence/Constitution bonus path).
  - `void GainExperience(CharacterRuntime character, int exp)` (line 292) — adds exp, loops `LevelUp` while true, syncs save.
  - `bool LevelUp(CharacterRuntime character)` (line 303) — checks `_formulaService.ExperienceToNextLevel`, resets Experience to 0 (discards remainder, explicitly commented "Exact Java Parity"), increments Level, heals to full.
  - `IReadOnlyList<CharacterRuntime> GetAllCharacters()` (line 321).
  - Mutates: `_characters` list (in-memory) and `SaveData.Characters` (via `SyncToSave`, line 84-106).
- SaveData: `CharacterSaveData` (SaveData.cs:34-54) — `DefinitionId, InstanceId, Level, Exp, CurrentHp, IsHpInitialized, WeaponInstanceId, ArmorInstanceId, AccessoryInstanceId, IsAscended, AscensionLevel, Trait, PotionsDrank[6], PositiveStatusEffects, NegativeStatusEffects`.
- Gap: `CharacterRuntime` constructor defaults `CurrentHp = 100` (line 41) with comment "will be initialized properly when Combat/Status systems are integrated" — a hardcoded placeholder that is overwritten immediately by callers in practice, but the class itself carries stale logic.

## 10. Character Details / Traits

- Trait is a bare `string` field on `CharacterRuntime.Trait` / `CharacterSaveData.Trait`, no `TraitDefinition` class exists.
- Trait effects are hardcoded in `CharacterService.GetTraitMultiplier(string trait, StatType statType)` (CharacterService.cs:270-290) — switch statement recognizing `BRUTE/STOUT` (+15% CON), `BOOKWORM` (+15% INT), `FERAL/NIMBLE` (+15% DEX), `KEEN_EYED` (+10% DEX, +5% INT), default 1.0. Traits referenced elsewhere but with NO stat-multiplier logic: `EMPATHETIC, GIFTED, INTIMIDATING, FOCUSED, DRAGON_BLOOD, CURSED, REACTIVE` are rolled by `TavernService.RollRareTrait()` (TavernService.cs:98-109) but `GetTraitMultiplier`'s switch does not have cases for any of them — they fall to `default: return 1.0` — i.e. 7 of ~10 rollable rare traits are **backend-inert** (rolled onto characters but produce no stat effect).
- No standalone "character detail" read/query service beyond `GetAllCharacters()` and per-stat `GetTotalStat`.

## 11. Skills

Minimal stub only.
- Definition: `SkillDefinition : DefinitionBase` (Definitions/SkillDefinition.cs:6-13) — `NameKey, DescriptionKey` (properties, not fields — inconsistent with the rest of the definitions which use public fields for JsonUtility compatibility, see AdventurerDefinition/EnemyDefinition comments; this means these two properties will NOT deserialize via `JsonUtility`, which only reads public fields). Comment: "manualRuleRequired: Cooldown, Cost, Level, TargetRule, DamageFormula — deferredToS3Combat".
- Runtime: `SkillRuntime` (Models/SkillRuntime.cs:6-16) — only field is `Id`; comment: "manualRuleRequired: CooldownLeft — deferredToS3Combat".
- Service: `SkillService : ISkillService` (SkillService.cs:11-17) — single method `SkillRuntime CreateSkill(string id, SkillDefinition definition)` (line 13) which **ignores the `definition` parameter entirely** and just returns `new SkillRuntime(id)`.
- `CharacterRuntime.ActiveSkillId/PassiveSkillId` and `EnemyRuntime.ActiveSkillId/PassiveSkillId` exist as string fields referenced by CombatService (mana gating at CombatService.cs:41-51) but no skill-effect resolution exists anywhere in the read scope — skills.json (227 records) is loaded into the database (skill_effects apply nowhere).
- SaveData: `SkillSaveData` (SaveData.cs:78-84: DefinitionId, InstanceId, Level, CurrentCooldown) declared but `SaveData.Skills` list (line 148) is **never populated or read by any service** in scope — fully dead/unwired.

## 12. Status Effects

- Definition: `StatusEffectDefinition : DefinitionBase` (Definitions/StatusEffectDefinition.cs:7-12) — `Type (StatusEffectType), IsNegative, IsSerialized` (all C# **properties**, same JsonUtility-incompatibility issue as SkillDefinition — properties won't deserialize from JSON via JsonUtility since it only reads public fields).
- Runtime: `StatusEffectRuntime` (Models/StatusEffectRuntime.cs:6-15) — `Type, SourceInstanceId, TurnsLeft, Probability`.
- Service: `StatusEffectService : IStatusEffectService` (StatusEffectService.cs:15-79).
  - `void AddStatusEffect(CharacterRuntime character, StatusEffectDefinition definition, string sourceId, int turnsLeft)` (line 69) and enemy overload (line 75) — both delegate to `InternalAddStatusEffect` (line 18-67): routes to positive/negative list per `definition.IsNegative`, BLEED stacks turns additively, all other types take the max of existing vs new turnsLeft (refresh-not-stack).
- **No caller applies status effects anywhere in Services/*.cs** — `CombatService.ProcessTurn`/`ApplyDamage` never call `IStatusEffectService`, and `IStatusEffectService` is not even injected into `CombatService`'s constructor or `ServiceContainer` wiring beyond being instantiated standalone (`StatusEffect = new StatusEffectService();`, ServiceContainer.cs:70) — it is constructed but never passed to anything else. **Fully orphaned service** — no production caller.
- `CombatService.ApplyDamage` has an explicit `[PARTIAL]` comment (CombatService.cs:113-114, 203-205, 339-340) acknowledging status effects (Exalt +5 flat reduction) are not wired into flat damage reduction.
- SaveData: `CharacterSaveData.PositiveStatusEffects/NegativeStatusEffects` and `EnemySaveData.PositiveStatusEffects/NegativeStatusEffects` (referenced in DungeonService.cs:552-553, 569-570, type not in SaveData.cs — declared elsewhere) exist and are round-tripped by DungeonService's enemy hydration, but since nothing ever calls `AddStatusEffect`, these lists stay empty in practice.

## 13. Equipment

- Service: `EquipmentService : IEquipmentService` (EquipmentService.cs:9-126).
  - `bool CanEquip(CharacterRuntime character, ItemRuntime item, EquipmentSlot slot)` (line 20) — checks item category matches slot, and `WeaponType`/`ArmorType` class-restriction (Weapon: `character.Definition.WeaponType != "Generic" && item.Definition.ItemType != character.Definition.WeaponType` → false; Armor: `item.Definition.ItemType != character.Definition.ArmorType` → false). No restriction check implemented for Accessory slot beyond category match.
  - `bool Equip(CharacterRuntime character, string itemInstanceId, EquipmentSlot slot)` (line 48) — looks up item via `_inventoryService.GetItem`, validates via `CanEquip`, unequips current slot occupant first, sets `item.IsLocked = true`, assigns to `character.Weapon/Armor/Accessory`, syncs save. **Two explicit `TODO: manualRuleRequired` comments** (lines 55, 61-62): unclear if class-restriction check is fully correct, and confirms **item is never removed from inventory on equip** — it stays in the inventory list but locked (deliberate design decision per comment, not a bug, but flagged as unconfirmed against source).
  - `bool Unequip(CharacterRuntime character, EquipmentSlot slot)` (line 82) — clears slot reference, unlocks item, syncs save. Comment again confirms item was never removed from inventory so nothing is added back.
  - `private void SyncSave(CharacterRuntime character)` (line 115) — finds matching `CharacterSaveData` by InstanceId, writes `WeaponInstanceId/ArmorInstanceId/AccessoryInstanceId`.
- Interface: `ICanEquip/Equip/Unequip` (IEquipmentService.cs:8-10) — no `GetEquipped`/query method beyond what's on `CharacterRuntime` directly.
- SaveData: `CharacterSaveData.WeaponInstanceId/ArmorInstanceId/AccessoryInstanceId` (SaveData.cs:43-45); also `InventoryService.RemoveItem` explicitly clears these three fields across all characters when the underlying item instance is fully consumed (InventoryService.cs:124-137, tagged "G17").

## 14. Inventory

- Service: `InventoryService : IInventoryService` (InventoryService.cs:13-231).
  - `int GetCapacity()` (line 64) — `_formulaService.StorageSpaces(data.LevelStorage, data.UpgradeStorage, data.GetPurchaseFlags())`.
  - `bool CanAddItem(string definitionId)` (line 70) — stackable categories (Material, Consumable) can always add if an existing stack of that def exists; else needs `_items.Count < GetCapacity()`.
  - `void AddItem(ItemRuntime item)` (line 86) — merges into existing stack if stackable, else appends; throws `InvalidOperationException("Inventory is full")` if over capacity (line 105) — **this throw is not caught by any caller shown** (e.g. `DungeonService.CollectDrops` calls `CanAddItem` first so should be safe, but `MerchantService.BuyOffer` also checks `CanAddItem` first — `CraftService.ClaimCompletedCraft` also checks first, line 172 — so the throw path looks defensively covered, but note `TavernService.GenerateVisitor` inserting starter weapon `data.Items.Add(weaponItem)` bypasses `InventoryService` entirely, writing straight to `SaveData.Items`, which means starter weapons received via tavern recruitment do not go through capacity checks at all).
  - `bool RemoveItem(string instanceId, int amount)` (line 112) — decrements stack, removes if ≤0, also clears character equipment refs pointing at the removed instance (G17 fix, lines 124-137).
  - `bool HasItem/GetItem/GetAllItems/GetItemsByCategory` (lines 144-162).
  - `bool ToggleLockItem(string instanceId)` (line 164).
  - `bool UseConsumable(string instanceId, CharacterRuntime targetCharacter)` (line 173) — hardcoded `+50 HP capped at Definition.BaseMaxHp` (line 182) — **ignores GetTotalStat and any equipment/doctrine/potion bonuses to MaxHp, and ignores the item's actual `ManualRuleRequired_StatusEffects` field** (ItemDefinition.cs:30) — no real potion-type differentiation despite `CharacterRuntime.PotionsDrank[6]` array existing for exactly this purpose elsewhere (CharacterService.cs:203).
  - DefinitionId-based API: `GetQuantityByDefinitionId, HasQuantityByDefinitionId, ConsumeByDefinitionId` (lines 189-229) — used heavily by CraftService/MerchantService for ingredient/sell logic.
- SaveData: `List<ItemSaveData> Items` (SaveData.cs:144) — `DefinitionId, InstanceId, StackCount, IsLocked`.

## 15. Storage

Not a distinct concept/service — "Storage" = Inventory's capacity dimension only (`LevelStorage`/`UpgradeStorage`/`GetCapacity()`/`GetStorageCapacityPrice` in FormulaService). No separate `StorageService`, no separate `StorageDefinition`. Folded entirely into `InventoryService` + `SaveData.LevelStorage/UpgradeStorage`.

## 16. Party

**No dedicated Party model, PartyService, or PartyDefinition.** "Party" is represented ad hoc:
- `DungeonRuntime.AdventurerInstanceIds` (Models/DungeonRuntime.cs:27, `List<string>`) — set once at `DungeonService.StartDungeon(string dungeonId, List<string> adventurerIds)` (DungeonService.cs:53-81).
- `DungeonService._party` (private `List<CharacterRuntime>` field, DungeonService.cs:33) — resolved via `ResolveParty()` (line 87-98) by matching `AdventurerInstanceIds` against `_characterService.GetAllCharacters()`.
- No party size validation, no "assign party" independent of starting a dungeon, no persisted "default party" concept. **Effectively zero standalone backend for Party — it only exists as a byproduct of DungeonService's active-run state.**

## 17. Combat

- Service: `CombatService : ICombatService` (CombatService.cs:10-136), plus embedded `ICombatEntityWrapper`, `AdventurerWrapper`, `EnemyWrapper` (lines 138-352).
  - `CombatResult ProcessTurn(List<CharacterRuntime> adventurers, List<EnemyRuntime> enemies, out string nextActingEntityId)` (line 22) — single-turn state machine: checks defeat/victory, builds wrapper list sorted by `IsInitiative` then `Dexterity` descending, picks the first actor, applies regen + mana gain, selects a target from the opposing team via weighted-threat random pool (`Threat` property: Adventurer=5 fixed, Enemy=1 fixed — line 309, 350), rolls damage via `RollAttackDamage`, applies via `ApplyDamage`.
  - `double RollAttackDamage(ICombatEntityWrapper attacker)` (line 91) — `min + roll*(max-min)`, "rolls three times, take best" if `RollsDamageThreeTimes` (always false for both wrapper types currently — dead code path, no entity sets it true).
  - `int ApplyDamage(ICombatEntityWrapper target, double rawDamage, bool isMagic, int barrier=0, double armorIgnored=0.0)` (line 108) — reduction = `min(1, (1-armorIgnored)*0.01*defStat)`, subtracts `FlatDamageReduction` and barrier, applies to `CurrentShield` first then `CurrentHp`. Comment explicitly flags `[PARTIAL]`: Exalt(+5), DragonBlood(+MaxLevel/5), Ascended(+MaxLevel/10) status/trait bonuses to flat reduction are NOT implemented (lines 113-114, 203-205).
  - `AdventurerWrapper.MinAttackDamage/MaxAttackDamage` (lines 210-304) — weapon-class-based formula (sword/staff/dagger/bow/default), ±20% delta, special-cases weapon id `"serpent_bite"` multiplying by Threat, adds `_petService.GetAttackBonus`.
  - `EnemyWrapper` — all stats sourced straight from `EnemyDefinition`, `FlatDamageReduction` comment flags missing per-enemy overrides (`LegateHadrian +15`, `TheExiled +40`) (lines 339-340).
- Interface `ICombatService`: `ProcessTurn`, `ApplyDamage` (ICombatService.cs:9-10) — note `RollAttackDamage` is public on the class but **not exposed on the interface**, so callers depending on `ICombatService` cannot invoke it directly.
- No `SaveData` fields belong to Combat directly — combat state (HP/mana/shield) lives on `CharacterRuntime`/`EnemyRuntime` and is persisted transitively via `CharacterSaveData.CurrentHp` and `DungeonService`'s `EnemySaveData` mapping.

## 18. Dungeon

- Service: `DungeonService : IDungeonService` (DungeonService.cs:12-605) — the largest single service; implements a 7-state action machine (0 ENTER_DUNGEON, 1 ENTER_ROOM, 2 FIGHT, 3 LOOT, 4 SEARCH_ROOM, 5 DEFEAT/RESPAWN, 6 FLEE) documented at lines 236-238.
  - `void StartDungeon(string dungeonId, List<string> adventurerIds)` (line 53) — gates on `DungeonDefinition.RequiredClearDungeonId` (chain-gating, "G05"), builds new `DungeonRuntime`, resolves party, saves.
  - `void StopDungeon()` (line 103) — clears `_activeDungeon` and `SaveData.ActiveDungeon`.
  - `void SaveDungeonState()/LoadDungeonState()` (lines 112, 149) — full round-trip of active-run state (progress, darkness, party ids, pending drops, encounter enemies/corpses, action state) to `SaveData.ActiveDungeon`.
  - `bool IsDungeonActive()` (line 195), `DungeonRuntime GetActiveDungeon()` (line 197).
  - `void AdvanceProgressOneStep()` (line 199) — manual progress increment + save (**appears to be a debug/test hook — no caller within Services**).
  - `void Tick()` (line 216) — throttled to run every other call (`_tickCounter & 1`), advances `ActionTurnsPassed`, calls `PerformAction()` when the action's duration (`GetActionDuration`, lines 528-541: 5/5/2/5/5/18/12 seconds per state) elapses.
  - `PerformAction()` (line 239) — the state transition switch; notable rules: state 4 (SEARCH_ROOM) increments quest counters `"long_march"` always and `"conqueror"` if dungeon id is `the_desert` (lines 265-268); state 5 only wipes progress if `< PROGRESS_KEEP_THRESHOLD(250)` (line 280).
  - `MoveCorpsesAndAwardExperience()` (line 398) — moves dead enemies to Corpses, fires 6 different quest-id increments based on dungeon/enemy id combos (lines 408-426), splits exp evenly among survivors via `_characterService.GainExperience`.
  - `RunLoot()` (line 448) / `BuildDropTable()` (line 472) — rolls each corpse's `EnemyDefinition.DropTable` via `_lootService`, respects merchant-pack chest cap.
  - `int CollectDrops()` (line 495) — moves `PendingDrops` into `_inventoryService`, skipping any that fail `CanAddItem` (silently dropped if inventory is full — **no report to caller on partial failure**, just returns count transferred).
  - `RestoreParty()` (line 517) — heals party to full HP, resets shield, on defeat/respawn.
- Interface `IDungeonService`: `StartDungeon, StopDungeon, SaveDungeonState, LoadDungeonState, IsDungeonActive, AdvanceProgressOneStep, Tick, GetActiveDungeon, CollectDrops` (IDungeonService.cs:8-24).
- Definition: `DungeonDefinition : DefinitionBase` (Definitions/DungeonDefinition.cs:16-43) — `RegularMerchantOffers, SpecialMerchantOffers (List<MerchantOfferData>), EnemyIds, RequiredClearDungeonId, QuestEventCategory, SourceClass`. Data file `dungeons.json`, 11 records.
- Runtime: `DungeonRuntime` (Models/DungeonRuntime.cs:14-47) — `InstanceId, Definition, State(enum Locked/Unlocked/Completed), ClearCount, BestTimeSeconds, Progress, MaxProgress(always 0 — comment "Placeholder, endless dungeon logic deferred", DungeonService.cs:75), LocalDarkness, AdventurerInstanceIds, PendingDrops, Enemies, Corpses, ActionType, ActionTurnsPassed, SavedActingEntityId, TurnsFighting`.
- SaveData: `List<DungeonSaveData> Dungeons` (SaveData.cs:147: DefinitionId, InstanceId, State, ClearCount, BestTimeSeconds) — **note this list is read in `StartDungeon`'s chain-gate check (`clearedDungeons.Any(...)`, line 60-63) but is never written anywhere** — no service marks a `DungeonSaveData` as `Completed` when a dungeon finishes; the completion write path is missing, meaning `RequiredClearDungeonId` gating can never actually pass once implemented UI calls it (permanent soft-lock for chained dungeons). Also `SaveData.ActiveDungeon` (single `ActiveDungeonSaveData`, not a list — only one dungeon run can be in-flight at a time).

## 19. Loot

- Service: `LootService : ILootService` (LootService.cs:23-101).
  - `ItemRuntime RollSingleDrop(List<DropTableEntry> dropTable)` (line 38) — weighted roll via `DecodeMath.RollFromWeightedMap`; explicitly NOT normalized against table sum, so a table summing <1000 has a real "nothing dropped" chance (documented lines 14-18).
  - `List<ItemRuntime> RollLoot(List<DropTableEntry> dropTable, int count)` (line 53).
  - `bool IsChestFull(List<ItemRuntime> pendingDrops, bool merchantPackPurchased=false)` (line 67) — sums `StackCount`, cap 2000 (`CHEST_CAP`) or 3000 (`CHEST_CAP_MERCHANT_PACK`).
  - `void CollectPendingLoot(List<ItemRuntime> pendingDrops, List<ItemRuntime> newLoot, bool merchantPackPurchased=false)` (line 81) — merges per-drop, stops once chest is full mid-loop (doesn't trim, just stops accepting further items that call).
  - `DropTableEntry` class (ILootService.cs:28-33): `Item(ItemDefinition), Weight, StackCount`.
- Fed entirely by `EnemyDefinition.DropTable` (Definitions/EnemyDefinition.cs:64, `[NonSerialized]`, populated post-deserialize by `EnemyDropTableLoader.Apply` in DatabaseBuilder, since JsonUtility can't read the raw `"Drops"/"DropStacks"` dictionaries seen in enemies.json).
- No persisted "loot log"/history; only the transient `DungeonRuntime.PendingDrops` (chest) round-trips through save via `DungeonService.SaveDungeonState`/`LoadDungeonState`.

## 20. Quest

- Service: `QuestService : IQuestService` (QuestService.cs:27-214).
  - Loads `quest_metadata.json` directly from `Application.streamingAssetsPath` at construction (`LoadMetadata()`, line 48) — **this bypasses `GameDatabase`/`DatabaseBuilder` entirely**, doing its own file read + `JsonUtility.FromJson<QuestFlatMetadataList>` (lines 51-57), building `_metadataMap` keyed by quest id (case-insensitive) mapping to `QuestFlatMetadataEntry{id, className, defaultRarity, targetProgressValues (List<long>, 10 per-rarity values)}`.
  - `long GetTargetProgress(string questId, int rarity)` (line 76) — indexes `targetProgressValues[Clamp(rarity-1,0,9)]`, fallback `rarity*100`.
  - `IReadOnlyList<QuestRuntime> GetActiveQuests()` (line 89).
  - `LoadQuests()/SaveQuests()` (lines 94, 115) — hydrate/persist `_activeQuests` ↔ `SaveData.Quests`.
  - `void Increment(string questInstanceId, long amount)` (line 130) — no-op if not active or already at target; sets `State=Completed` on reaching target.
  - `void IncrementToValue(string questInstanceId, long newValue)` (line 147) — absolute-set variant.
  - `int GetRewardAmount(int rarity, bool isGems)` (line 163) — hardcoded table: rarity1→1/10, 2→2/20, 3→3/40, 4→5/100 (gems only awarded at rarity≥4).
  - `bool ClaimReward(string questInstanceId, string targetDoctrineName="war")` (line 176) — requires `State==Completed`, awards gems OR calls `_doctrineService.AddProgress(targetDoctrineName, amount)`, removes quest from active list, increments `data.QuestsCompleted`.
  - `void IncrementDefinition(string definitionId, long amount)` (line 204) — fan-out increment to every active quest matching a definition id (case-insensitive) — this is the hook DungeonService uses for quest-id-based progress triggers (`"long_march", "conqueror", "active_deterrent", "annihilator", "and_stay_dead", "from_hell", "myopia", "nice_try", "exorcism"`).
- Interface `IQuestService` (IQuestService.cs:6-15) — note `GetTargetProgress` is public on the class but **not on the interface**.
- Definition: `QuestDefinition : DefinitionBase` (Definitions/QuestDefinition.cs:6-11) — `TargetProgress, TrueClass` (both **properties**, same JsonUtility field-vs-property gap noted for Skill/StatusEffect definitions — these two properties will not deserialize from `quests.json` via JsonUtility, meaning `QuestDefinition.TargetProgress`/`TrueClass` are always default/0/null from the database; the actual working `TargetProgress` value in-game comes exclusively from the separately-loaded `quest_metadata.json` map, not from `QuestDefinition` at all).
- Runtime: `QuestRuntime` (Models/QuestRuntime.cs:16-40) — `InstanceId, Definition, State(enum Locked/NotStarted/InProgress/Completed/RewardClaimed), Progress, Rarity, TargetProgress, IsDirty, IsActive⇒State==InProgress`. **Note**: `QuestState.Locked` and `QuestState.RewardClaimed` are declared enum values but no code path in QuestService ever sets a quest to `Locked` or `RewardClaimed` — `ClaimReward` removes the quest from the list entirely instead of transitioning it to `RewardClaimed` (line 198), so that state is unreachable/dead.
- SaveData: `QuestSaveData` (SaveData.cs:56-65: DefinitionId, InstanceId, State, Progress, Rarity, TargetProgress).
- Gap: no service ever **creates/starts** new `QuestRuntime`/`QuestSaveData` entries (no `StartQuest`/`AssignQuest` method anywhere in `IQuestService` or `QuestService`) — the only quest lifecycle operations are Increment/IncrementToValue/ClaimReward/IncrementDefinition, all of which assume the quest already exists in `_activeQuests`. There is no visible mechanism populating `SaveData.Quests` in the first place.

## 21. Doctrine

- Service: `DoctrineService : IDoctrineService` (DoctrineService.cs:7-114).
  - `int GetLevel(string doctrineName)` (line 18) / `int GetProgress(string doctrineName)` (line 36) — string-keyed switch over the 8 doctrines (affliction/control/fortitude/grace/illusion/knowledge/ruin/war), default 0 for unknown names.
  - `void AddProgress(string doctrineName, int amount)` (line 54) — loop-based star-to-level conversion using `_formulaService.TotalStarsToNextLp(level)` (= `level*3+4`), carries remainder across level-ups, writes back level+progress via the same switch pattern.
  - `bool IsMaxed()` (line 109) — reads `data.DoctrineMaxed` flag directly (**no logic ever sets `DoctrineMaxed = true`** anywhere in the read scope — dead write path, flag can never flip from its default `false`).
- Interface (IDoctrineService.cs:5-11).
- SaveData: 8× (Level+Progress) pairs + `DoctrineMaxed` (SaveData.cs:205-221).
- Consumers: `CharacterService.GetTotalStat` reads War/Fortitude/Ruin/Grace/Illusion/Knowledge levels directly from `_saveService.CurrentData` (CharacterService.cs:182-198) rather than through `IDoctrineService.GetLevel` — **bypasses the service's own abstraction**, duplicating the same field-name switch logic that `DoctrineService.GetLevel` already encapsulates.
- No `DoctrineDefinition` class or JSON data file exists — doctrine names/effects are entirely hardcoded strings in C#.

## 22. Raid

- Definition only: `RaidDefinition : DefinitionBase` (Definitions/RaidDefinition.cs:6-9) — **empty class body**, adds zero fields beyond the base decode-audit metadata.
- Data file `raids.json` — 12 records, each record in the JSON shown only carries base fields (className, id, parseReasons, parseStatus, recordHash, sourcePath) — no raid-specific stats present in the file itself either.
- `DatabaseBuilder` registers `raids → RaidDefinition` in `_categoryLoaders` (DatabaseBuilder.cs:37) — so raids ARE loaded into `GameDatabase`.
- **No `RaidService`, `IRaidService`, `RaidRuntime`, or `RaidSaveData` exist anywhere in Services/Models/Save.** No service references `RaidDefinition` or the `raids` category after loading. **Raid has zero gameplay backend — data-only, completely inert.**

## 23. Workshop

Workshop = crafting queue, fully inside `CraftService`:
- `int GetQueueCapacity()` (CraftService.cs:33) — `_formulaService.WorkshopQueue(...)`.
- `IReadOnlyList<ItemActionSaveData> GetQueue()/GetCompletedItems()` (lines 39, 44).
- `int GetMaxCraftable(string recipeId)` (line 49) — min(owned/required) across all ingredients.
- `CraftResult CanCraft(string recipeId)` (line 70) — validates recipe existence, output item, ingredients list, queue-not-full, and ingredient quantities; returns typed `CraftFailureReason`.
- `CraftResult TryStartCraft(string recipeId)` (line 110) — consumes ingredients via `ConsumeByDefinitionId`, enqueues `ItemActionSaveData{StackCount=1 (always 1 — no batch crafting), SecondsPassed=0}`.
- `void ProgressWorkshop(long deltaSeconds)` (line 144) — advances head-of-queue's `SecondsPassed`; completes at fixed `DEFAULT_CRAFT_DURATION_SECONDS=10` (line 19) — **ignores `IFormulaService.GetSecondsToCraft` entirely**, which exists and takes item price/workshop-time-level/purchase-flags into account (FormulaService.cs:252-259) but is never called by CraftService — craft duration is always a flat 10 seconds regardless of recipe/item value or `LevelWorkshopTime`/`UpgradeWorkshopTime`.
- `bool ClaimCompletedCraft(string instanceId)` (line 161) — moves completed item into inventory (respecting `CanAddItem`), calls `_saveService.Save(out _)` directly (inconsistent with most other services that don't self-save).
- `bool UpgradeQueueCapacity()` (line 180) — currency-gated, self-saves.
- `GetUpgradeQueueCapacityPrice()/GetQueueCapacityLevel()` (lines 194-195).
- Interface `ICraftService` (ICraftService.cs:7-22).
- SaveData: `WorkshopQueue, CompletedWorkshopItems` (SaveData.cs:139-140), plus `LevelWorkshopQueue, UpgradeWorkshopQueue` (used) and `LevelWorkshopTime, UpgradeWorkshopTime` (declared, SaveData.cs:134,193, **never read by CraftService** — orphaned given the hardcoded 10s duration above).

## 24. Recipes

- Definition: `RecipeDefinition : DefinitionBase` (Definitions/RecipeDefinition.cs:15-19) — `OutputItemId, List<IngredientData> Ingredients` where `IngredientData{ItemId, Amount}` (lines 8-12).
- Data file `recipes.json` — 321 records. Sample records show many with **empty `Ingredients: []` and `manualRuleRequired: true`** plus `parseReasons: ["MANUAL_RULE_REQUIRED","MISSING_OUTPUT_ITEM","MISSING_INGREDIENTS"]` and a `rawArgs` string showing the original unparsed Java call — i.e. a large fraction of the 321 recipes are decode-incomplete stubs that `CraftService.CanCraft` will reject via `CraftFailureReason.InvalidIngredients` (CraftService.cs:84-87) since `Ingredients.Count==0`. No count given here of how many of 321 are fully populated vs stub (would require reading the full file).
- No standalone `RecipeService` — recipe lookups happen inline in `CraftService` via `_database.TryGet<RecipeDefinition>`.

## 25. Merchant

- Service: `MerchantService : IMerchantService` (MerchantService.cs:12-190).
  - `IReadOnlyList<MerchantOfferSaveData> GetRegularStock()/GetSpecialStock()` (lines 27, 32).
  - `MerchantOfferData RollRegularOffer(string dungeonId)/RollSpecialOffer(string dungeonId)` (lines 37, 45) — pulls `DungeonDefinition.RegularMerchantOffers`/`SpecialMerchantOffers`, weighted roll via `RollWeightedOffer` (line 53, plain linear-scan roll, **not using `DecodeMath.RollFromWeightedMap`** unlike LootService — inconsistent implementation of the same weighted-roll concept across two services).
  - `bool BuyOffer(MerchantOfferSaveData offer, bool isSpecial)` (line 78) — capacity check, currency check (`offer.IsGems` decides Money vs Gems), removes from the appropriate stock list, grants item.
  - `MerchantResult BuyItem(string dungeonId, string itemId)` (line 118) — **stub**: unconditionally returns `MerchantResult.Fail(MerchantFailureReason.DeferredPriceOrCurrencyRule)` — dead/placeholder method, no real implementation, and not called by `BuyOffer` (which is the actual working purchase path) — `BuyItem` looks like an abandoned earlier API surface still exposed on `IMerchantService`? **Actually `BuyItem` is NOT on `IMerchantService`** (checked IMerchantService.cs:8-22 — only `BuyOffer` is exposed) so `BuyItem` is a fully dead/unreachable public method on the concrete class.
  - `MerchantResult SellItem(string definitionId, int stackCount)` (line 123) — consumes from inventory, enqueues into `data.MarketListings` (an `ItemActionSaveData`).
  - `void ProgressMarket(long deltaSeconds)` (line 149) — advances head-of-queue timer, completes at flat `DEFAULT_SELL_TIME_SECONDS=20` (line 18) — **same pattern as CraftService: ignores `IFormulaService.GetSecondsToSell` entirely** despite that formula existing and taking price/stack/market-time-level/purchase-flags into account (FormulaService.cs:261-267).
  - `bool ClaimSoldItem(string instanceId)` (line 166) — computes `itemPrice = itemDef.SellPrice>0 ? SellPrice : 100`, `totalEarned = TruncatePrice(itemPrice*StackCount)`, credits `data.Money`.
  - `GetMarketListings()/GetSoldMarketItems()` (lines 187-188).
- Interface `IMerchantService` (IMerchantService.cs:8-22).
- SaveData: `MarketListings, SoldMarketItems` (used), `MerchantRegularStockItems, MerchantSpecialReserve` (used by GetRegularStock/GetSpecialStock/BuyOffer), `LevelMarketTime, UpgradeMarketTime` (declared, **never read by MerchantService** — same dead-formula-input pattern as Workshop), `LevelMarketListings, UpgradeMarketQueue` (declared, formula `MarketListings()` exists in IFormulaService but **no service calls it** — stock-slot-count formula is entirely unused).
- Gap: **nothing ever populates `MerchantRegularStockItems`/`MerchantSpecialReserve`** with actual `MerchantOfferSaveData` — `RollRegularOffer`/`RollSpecialOffer` return `MerchantOfferData` (a definition-level roll result) but no code converts that roll into a `MerchantOfferSaveData` and appends it to the stock lists; `GetRegularStock`/`BuyOffer` only operate on whatever is already in those lists, and nothing shown writes to them.

## 26. Market

Folded entirely into `MerchantService` — "Market" = the sell-side (`MarketListings`/`SoldMarketItems`/`ProgressMarket`/`ClaimSoldItem`), "Merchant" = the buy-side (regular/special stock, `BuyOffer`). No separate `MarketService`/`MarketDefinition`. See §25.

## 27. Shop

No separate "Shop" concept from Merchant/Market — same service. No `ShopDefinition`/`ShopService`. **Folded into Merchant (§25); if the design intends Shop as a distinct player-facing concept (e.g. IAP/premium currency purchases) that is not represented anywhere** — only `PurchaseFlags` (StarterPack/AdventurerPack/MerchantPack/ImperialVanguard/UnholyCrusade) exist as pre-baked booleans consumed by formulas, with no purchase-flow service to set them.

## 28. Pets

- Definition: `PetDefinition : DefinitionBase` (Definitions/PetDefinition.cs:6-22) — `PetName, BaseAttack, BaseDefense, BaseMaxHp(default 50), BaseSpeed(default 10), AttackMultiplier/DefenseMultiplier/HpMultiplier/SpeedMultiplier(all default 1.0f), ExpToLevel(default 100), SkillDefinitionId, EvolutionDefinitionId, EvolutionLevel, VisualPrefab`. Data file `pets.json`, 21 records (sample records shown carry only base id/className/parse metadata — **no stat overrides visible in the sample rows read**, meaning most/all pets may be running on hardcoded class defaults rather than per-pet-tuned JSON values — would need full-file read to confirm coverage).
- Service: `PetService : IPetService` (PetService.cs:27-201).
  - `IReadOnlyList<PetSaveData> GetAllPets()` (line 38).
  - `PetSaveData CreatePet(string definitionId, string ownerCharacterId)` (line 45) — creates with `Level=1, Exp=0`, appends to `SaveData.Pets`, self-saves.
  - `void AddExp(string instanceId, long amount)` (line 70) — loop-levels while `Exp >= expNeeded` (from `PetDefinition.ExpToLevel`, fallback 100), self-saves.
  - `bool LevelUp(string instanceId)` (line 92) — single-level consume-exp-and-level, self-saves.
  - `bool EquipToCharacter(string petInstanceId, string characterInstanceId)/UnequipFromCharacter(string petInstanceId)` (lines 112, 123) — sets/clears `PetSaveData.EquippedToCharacterId`, self-saves. **No check that a character doesn't already have a pet equipped, and no check that a pet isn't already equipped elsewhere** — `EquipToCharacter` will silently allow multiple pets on one character (per `GetCharacterPets` returning a list, this actually appears to be intentional multi-pet support) and the same pet equipped to two different characters simultaneously (no exclusivity guard — likely a real gap since a physical pet object shouldn't belong to two owners at once).
  - `IReadOnlyList<PetSaveData> GetCharacterPets(string characterInstanceId)` (line 134) / `bool HasPetEquipped(...)` (line 145).
  - `GetAttackBonus/GetDefenseBonus/GetHpBonus/GetSpeedBonus(string characterInstanceId)` (lines 153-199) — each sums `BaseX * Level * XMultiplier` across all equipped pets for that character.
- SaveData: `List<PetSaveData> Pets` (SaveData.cs:149) — `DefinitionId, InstanceId, Level, Exp, EquippedToCharacterId`.
- Consumer: `CharacterService.GetTotalStat` calls `_petService.GetHpBonus/GetDefenseBonus/GetSpeedBonus` (CharacterService.cs:251-265) — note **GetSpeedBonus is added to `StatType.Dexterity`**, and **no pet Intelligence/Constitution bonus path exists** despite CombatService's `AdventurerWrapper.MinAttackDamage/MaxAttackDamage` also calling `_petService.GetAttackBonus` directly (CombatService.cs:250-253, 298-301) — i.e. pet attack bonus is applied twice through two different code paths for different purposes (fine, but worth confirming they don't double-count in one code path — they don't appear to, since GetAttackBonus is only used in weapon damage calc, not in GetTotalStat).

## 29. Shelter

**No `ShelterService`/`IShelterService`/`ShelterDefinition`/`ShelterRuntime` found anywhere in the read Services/Models/Definitions.** Only trace of "Shelter" is unused SaveData fields (`LevelShelter, UpgradeShelter, LevelShelterAutofeed`, SaveData.cs:199-201) and unused formula methods (`GetShelterPrice`, `GetShelterAutofeedPrice`, `ShelterCapacity`, FormulaService.cs:150-178, 234-237) — **no service in the read set calls any of these three formula methods.** Shelter is presumably the pet-housing/pet-feeding system (given `LevelShelterAutofeed` name) but has zero implementation. **No backend beyond dead formula stubs + dead save fields.**

## 30. Promotion

- Service: `PromotionService : IPromotionService` (PromotionService.cs:19-113) — note **interface + implementation are declared in the same file**, unlike every other service which splits into `I*.cs`.
  - `IReadOnlyList<PromotionDefinition> GetAvailablePromotions(CharacterSaveData character)` (line 34) — finds promotions where `promo.TierIndex == character.AscensionLevel+1 && character.Level >= promo.RequiredLevel`.
  - `bool CanPromote(CharacterSaveData character, string promotionId)` (line 53) — same tier/level gating plus checks `_inventoryService` for `RequiredItemId`/`RequiredItemCount` if specified.
  - `bool Promote(CharacterSaveData character, string promotionId)` (line 74) — consumes required item, increments `AscensionLevel`, resets `Level=1, Exp=0`, syncs the live `CharacterRuntime` if found (via `_characterService.GetAllCharacters()`), sets `runtime.IsAscended=true` (note: **`IsAscended` is set true on EVERY promotion**, tier 1 included, and never seems to differentiate a "true ascension" from a lesser promotion — combined with `CharacterService.GetTotalStat`'s `legacyMult = character.IsAscended ? 1.5 : 1.0` (CharacterService.cs:178), a single tier-1 promotion immediately grants the full "ascended" ×1.5 legacy multiplier on top of the tier's own `PromotionDefinition.StatMultiplier`, which may double-count bonus multipliers — worth flagging as a potential balance/stacking bug), self-saves, `Debug.Log`s.
  - `int GetPromotionCount(string characterInstanceId)` (line 107) — reads `AscensionLevel` from `SaveData.Characters` lookup.
- Definition: `PromotionDefinition : DefinitionBase` (Definitions/PromotionDefinition.cs:6-20) — `RequiredLevel, RequiredItemId, RequiredItemCount(default 1), StatMultiplier(default 1.1f), TierName(default "Promotion"), TierIndex`. **No dedicated `promotions.json` data file exists in StreamingAssets/GameData** (not in the directory listing) — `PromotionDefinition` is registered nowhere in `DatabaseBuilder._categoryLoaders` (DatabaseBuilder.cs:26-38 lists items/enemies/skills/status_effects/adventurers/pets/recipes/quests/dungeons/raids only — **no "promotions" entry**) — meaning `_database.GetAll<PromotionDefinition>()`/`GetRequired<PromotionDefinition>` calls in `PromotionService` and `CharacterService.GetTotalStat` (line 170) will always return an empty set, **so promotion tiers can never actually be found/granted in practice** despite the full service logic existing.
- SaveData: reuses `CharacterSaveData.AscensionLevel/Level/Exp` — no separate PromotionSaveData.

## 31. Ascension

Not a distinct system — "Ascension" = `CharacterRuntime.IsAscended`/`AscensionLevel` fields, set exclusively by `PromotionService.Promote` (see §30) and read by `CharacterService.GetTotalStat` (legacy ×1.5 multiplier, line 178) and `PromotionService.GetAvailablePromotions`/`CanPromote` (tier gating). No separate `AscensionService`/`AscensionDefinition`. Folded into Promotion.

## 32. Unlock Progression

No dedicated "UnlockService". Unlock-like gating is scattered:
- Dungeon chain gating: `DungeonDefinition.RequiredClearDungeonId` checked in `DungeonService.StartDungeon` (DungeonService.cs:57-69) — but see §18 gap: nothing ever marks a dungeon `Completed` in `SaveData.Dungeons`, so this gate can never pass.
- Tutorial-step gating: `SaveData.TutorialStep` drives `TavernService.GenerateVisitor`'s special-cased classes at steps ≤1/6/7 (TavernService.cs:126-141) — this is the closest thing to a progression/unlock system, entirely inline in TavernService, not abstracted.
- Promotion tier gating: `PromotionDefinition.RequiredLevel`/`TierIndex` (see §30).
- No generic `IsUnlocked`/achievement-style flags found beyond `SaveData.TavernLocked` (unused, see §7) and per-system level=0 defaults acting as implicit locks.

## 33. Settings

- Service: `SettingsService : ISettingsService` (SettingsService.cs:6-100).
  - `bool GetToggle(string key)` (line 15) / `void SetToggle(string key, bool value)` (line 38) — string-keyed switch over 13 keys: sound, music, vibration, notifications, cloud, colorblind, autoopendetail, confirmretreat, confirmswap, confirmupgrade, craftmax, sellmax, verboselogs. Unknown keys default to `true` on Get, no-op on Set.
  - `string GetLanguage()/void SetLanguage(string lang)` (lines 60, 65) — reads/writes `data.SettingsLanguage`, default `"en"`.
  - `bool SaveCurrentState()` (line 71) — delegates to `_saveService.Save(out _)`.
  - `void ResetToDefault()` (line 76) — resets all 13 toggles + language to hardcoded defaults (matches `SaveData.CreateDefault`'s sound/music=true but explicitly also sets everything else, including several `false` defaults that differ from field declaration defaults in SaveData.cs — e.g. `SettingsNotifications`/`SettingsCloud` declared with no default (`false` by C# default) in SaveData.cs:246-247 but `ResetToDefault` sets them `true`, meaning a *fresh* save and an *explicitly reset* save diverge on these two flags).
  - `string GetGameVersion()` (line 95) — reads `_saveService.CurrentData.Metadata?.GameVersion`, fallback `"2.147"` hardcoded literal.
- Interface (ISettingsService.cs:3-13).
- SaveData: all 13 toggle fields + `SettingsLanguage` (SaveData.cs:237-250).

## 34. Localization

**`localization.json` is a literal empty array `[]`.** No `LocalizationDefinition` class exists in Definitions/. `DatabaseBuilder.Build()` explicitly skips the `localization` category (DatabaseBuilder.cs:81-87, comment "Handled by other services") but **no other service in the read scope handles it** — every definition that references localization keys (`nameKey`, `idName`/`idDescription` in items.json, `NameKey`/`DescriptionKey` on `SkillDefinition`) has nothing to resolve against. `ISettingsService.GetLanguage/SetLanguage` only stores a language code string; no lookup/translation service exists. **No backend for Localization beyond an empty data file and a language-preference string.**

---

## Final Summary — Systems With ZERO Backend Representation

Of the 34 systems audited, the following have **no service, no dedicated runtime model, and no dedicated definition/data class** (data-only entries with an empty class body count as effectively zero, noted separately):

1. **Headquarters** — no trace anywhere (§6).
2. **Party** — no PartyService/PartyDefinition/PartyRuntime; only an ad hoc `List<string> AdventurerInstanceIds` on `DungeonRuntime` and a private `_party` list inside `DungeonService` (§16).
3. **Storage** — not a distinct system; fully folded into `InventoryService`/`SaveData.LevelStorage` (§15). (Folded, not missing outright — noted for completeness.)
4. **Market** — folded entirely into `MerchantService` (§26). (Folded, not missing.)
5. **Shop** — no distinct implementation from Merchant/Market; no IAP/purchase-flow service exists to set `PurchaseFlags` (§27).
6. **Shelter** — SaveData fields and formula methods exist but are **never called by any service**; no `ShelterService` exists at all (§29). Closest to a true zero-backend system among the "has some scaffolding" group.
7. **Ascension** — folded entirely into `PromotionService`/`CharacterRuntime.IsAscended` (§31). (Folded, not missing.)
8. **Raid** — `RaidDefinition` is an empty class, data loads into the database, but **no RaidService/RaidRuntime/RaidSaveData or any consumer exists** — pure inert data (§22).
9. **Localization** — data file is `[]`, no definition class, no service beyond a language-code string in Settings (§34).

Additionally flagged as **backend-present-but-functionally-dead** (service/formula exists but is never invoked or never produces reachable state — not "zero" but effectively non-functional):
- **Promotion/Ascension**: `PromotionDefinition` has no JSON data file and is not registered in `DatabaseBuilder`, so `GetAvailablePromotions`/`CanPromote`/`Promote` can never find a promotion in practice (§30).
- **Skills**: `SkillService.CreateSkill` ignores its `definition` parameter; no skill-effect resolution exists anywhere; `SkillDefinition`'s properties won't deserialize via JsonUtility (§11).
- **Status Effects**: `StatusEffectService` is constructed in `ServiceContainer` but never passed to `CombatService` or any other consumer — orphaned with no caller (§12).
- **Doctrine.IsMaxed**: `DoctrineMaxed` flag is read but never written (§21).
- **Quest creation**: no method exists anywhere to originate a new `QuestRuntime`/`QuestSaveData` — only progress/claim operations on already-existing quests (§20).
- **Dungeon completion**: nothing ever writes `DungeonSaveData.State = Completed`, so `RequiredClearDungeonId` chain-gating can never pass (§18).
- **Merchant stock population**: nothing ever appends rolled offers into `MerchantRegularStockItems`/`MerchantSpecialReserve` (§25).

## Other Cross-Cutting Gaps Worth Flagging

- **Two competing offline-progress implementations** (`GameLoopService.ProcessOfflineCatchup` vs `OfflineProgressService.ApplyOfflineProgress`) using two different save-time reference fields (`SaveData.LastAccess` vs `SaveData.Metadata.SaveTimeUnix`), both registered in `ServiceContainer`, with no evidence of which one is actually the intended/wired call path (§4).
- **JsonUtility property-vs-field bug pattern**: `QuestDefinition.TargetProgress/TrueClass`, `SkillDefinition.NameKey/DescriptionKey`, `StatusEffectDefinition.Type/IsNegative/IsSerialized` are all C# **properties**, which Unity's `JsonUtility` silently ignores (only public fields deserialize) — every other Definition class in the codebase was explicitly fixed to use public fields per comments in `AdventurerDefinition.cs:9-11` and `EnemyDefinition.cs:24-26`, but these three were missed, meaning those specific fields always come back as C# default values from JSON (§11, §12, §20).
- **Flat hardcoded durations bypass their own formula service**: `CraftService.ProgressWorkshop` (10s flat) and `MerchantService.ProgressMarket` (20s flat) both ignore `IFormulaService.GetSecondsToCraft`/`GetSecondsToSell`, which exist and take level/price/purchase-flag inputs (§23, §25).
- **Trait system mostly inert**: 7 of the ~10 rare traits rollable by `TavernService.RollRareTrait` have no corresponding case in `CharacterService.GetTraitMultiplier` (§10).
- **Possible double-multiplier stacking bug**: `PromotionService.Promote` sets `IsAscended=true` on every tier promotion (not just a special "ascension" tier), which stacks with each `PromotionDefinition.StatMultiplier` on top of the flat legacy ×1.5 `IsAscended` bonus in `CharacterService.GetTotalStat` (§30) — moot in practice since promotions can never be found (§30 database gap), but would double-apply if the data-registration gap were fixed without also revisiting this logic.
