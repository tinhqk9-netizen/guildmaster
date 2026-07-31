# CORRECTED RUNTIME VERIFICATION REGISTER
## Status labels per Source of Truth rules

---

## Layer 1: Foundation (Boot → Save → Load)

| ID | Verification | File | Method | Status | Notes |
|----|-------------|------|--------|--------|-------|
| RV01 | Bootstrapper calls ServiceContainer.Initialize | Bootstrapper.cs | `Start()` | STATIC_TRACE_CONFIRMED | Line by line read complete |
| RV02 | GameDatabase.LoadAll() reads JSON | GameDatabase.cs | `LoadAll()` | STATIC_TRACE_CONFIRMED | Resources.LoadAll<TextAsset> path verified |
| RV03 | SaveService.LoadGame called on boot | Bootstrapper.cs | `Start()` | STATIC_TRACE_CONFIRMED | `_saveService.LoadGame()` at step 3 |
| RV04 | SaveService.SaveGame writes windoid file | SaveService.cs | `SaveGame()` | STATIC_TRACE_CONFIRMED | BinaryWriter → `save.json` path verified |
| RV05 | LoadGame reads save, handles missing file | SaveService.cs | `LoadGame()` | STATIC_TRACE_CONFIRMED | File.Exists check, falls back to new SaveData |
| RV06 | NormalizeAfterLoad guards null lists | SaveData.cs | `NormalizeAfterLoad()` | STATIC_TRACE_CONFIRMED | 15+ null→new List guards, 3 orphan fields unchanged |
| RV07 | RuntimeFactory.CreateId generates GUID | RuntimeFactory.cs | `CreateId()` | STATIC_TRACE_CONFIRMED | `Guid.NewGuid().ToString()` |
| RV08 | 19 services registered in Container | ServiceContainer.cs | `Initialize()` | STATIC_TRACE_CONFIRMED | 19 `Register<T,I>()` calls counted |
| RV09 | UIService.Initialize called | UIRuntimeBootstrap.cs | `Initialize()` | STATIC_TRACE_CONFIRMED | Registration + first screen show |
| RV10 | Boot order correct (Services → Data → Save → UI) | Bootstrapper.cs | `Start()` | STATIC_TRACE_CONFIRMED |
| RV11 | DataVersion check prevents stale cache | SaveMetadata.cs | `CHECK_DATA_VERSION` | PARTIAL | Version written at save, load comparison unconfirmed |

## Layer 2: SaveData Normalization

| ID | Section | Fields | Null Guard | Status | Notes |
|----|---------|--------|-----------|--------|-------|
| RS01 | Metadata | SaveVersion, GameVersion, DataVersion, SaveTimeUnix | `?? new SaveMetadata()` | STATIC_TRACE_CONFIRMED |
| RS02 | Currency | Money, Gems | N/A (int value types) | STATIC_TRACE_CONFIRMED | Default 0 |
| RS03 | Inventory | Items (List) | `?? new List<ItemSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS04 | Characters | Characters (List) | `?? new List<CharacterSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS05 | Dungeon | Dungeons (List) | `?? new List<DungeonSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS06 | ActiveDungeon | ActiveDungeon | `?? new DungeonSaveData()` | STATIC_TRACE_CONFIRMED | DungeonSaveData verified as class (nullable) |
| RS07 | Quests | Quests (List) | `?? new List<QuestSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS08 | Skills | Skills (List) | `?? new List<SkillSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS09 | Workshop | WorkshopQueue (List) | `?? new List<ItemActionSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS10 | Merchant | MerchantRegularStockItems | `?? new List<MerchantStockSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS11 | Tavern | TavernGuests (List) | `?? new List<TavernGuestSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS12 | Settings | Sound, Music, Vibration, Notifications, Language | Default values | STATIC_TRACE_CONFIRMED |
| RS13 | Potions | PotionsDrank (List) | `?? new List<int>()` | STATIC_TRACE_CONFIRMED |
| RS14 | Status Effects | PositiveStatusEffects, NegativeStatusEffects | `?? new List<StatusEffectSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS15 | Traits | Trait | `?? new List<TraitSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS16 | CompletedWorkshopItems | List | `?? new List<ItemActionSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS17 | MarketListings | List | `?? new List<MarketListingSaveData>()` | STATIC_TRACE_CONFIRMED |
| RS18 | UniqueItemsLost | List | `?? new List<string>()` | STATIC_TRACE_CONFIRMED |
| RS19 | Character PotionsDrank | Per-character list (field within CharacterSaveData) | Partial — inside NormalizeAfterLoad? | PARTIAL | May need per-char normalization |

## Layer 3: Game Flow

| ID | Action | Call Chain | Status | Notes |
|----|--------|-----------|--------|-------|
| RF01 | HUD shows Money/Gems | HUDController.Refresh() → SaveData.Money/Gems | STATIC_TRACE_CONFIRMED | UI reads save directly |
| RF02 | Open Inventory | InventoryScreen.Show() → InventoryService.GetAllItems() | STATIC_TRACE_CONFIRMED |
| RF03 | Lock item | InventoryService.ToggleLockItem() → SaveData.Items[].IsLocked | STATIC_TRACE_CONFIRMED |
| RF04 | Use consumable | InventoryService.UseConsumable() → ItemService.ApplyEffect() → RemoveItem | STATIC_TRACE_CONFIRMED |
| RF05 | Open Character | CharacterScreen.Show() → CharacterService.GetAllCharacters() | STATIC_TRACE_CONFIRMED |
| RF06 | Equip item | EquipmentService.Equip() → Set WeaponInstanceId → CharacterService.GetTotalStat() | STATIC_TRACE_CONFIRMED |
| RF07 | Unequip item | EquipmentService.Unequip() → Clear slot → InventoryService.AddItem() | STATIC_TRACE_CONFIRMED |
| RF08 | Enter dungeon | DungeonScreen.StartDungeon() → DungeonService.StartDungeon() | STATIC_TRACE_CONFIRMED |
| RF09 | Dungeon tick | DungeonScreen.Tick() → CombatService.ProcessTurn() | STATIC_TRACE_CONFIRMED | Tick() called from Update() (G09) |
| RF10 | Collect loot | DungeonScreen.CollectDrops() → LootService → InventoryService.AddItem() | STATIC_TRACE_CONFIRMED |
| RF11 | Open Craft | CraftScreen.Show() → CraftService.GetQueue(), GetAvailableRecipes() | STATIC_TRACE_CONFIRMED |
| RF12 | Start craft | CraftService.TryStartCraft() → deduct materials → add to WorkshopQueue | STATIC_TRACE_CONFIRMED |
| RF13 | Claim craft | CraftService.ClaimCompletedCraft() → ItemService.CreateItem() → AddItem() | STATIC_TRACE_CONFIRMED |
| RF14 | Open Merchant | MerchantScreen.Show() → MerchantService.GetRegularStock() | STATIC_TRACE_CONFIRMED |
| RF15 | Buy offer | MerchantService.BuyOffer() → Spend money → AddItem → RemoveStock | STATIC_TRACE_CONFIRMED |
| RF16 | Sell item | MerchantService.SellItem() → RemoveItem → MarketListing | STATIC_TRACE_CONFIRMED |
| RF17 | Claim sold item | MerchantService.ClaimSoldItem() → AddMoney → RemoveListing | STATIC_TRACE_CONFIRMED |
| RF18 | Open Quest | QuestScreen.Show() → QuestService.GetActiveQuests() | STATIC_TRACE_CONFIRMED |
| RF19 | Claim quest reward | QuestService.ClaimReward() → AddGold → DoctrineService.AddProgress() | STATIC_TRACE_CONFIRMED | Doctrine integration exists (but no UI screen) |
| RF20 | Open Tavern | TavernScreen.Show() → TavernService.GetGuests() | STATIC_TRACE_CONFIRMED |
| RF21 | Recruit guest | TavernService.RecruitGuest() → SpendMoney → AddCharacter → RemoveGuest | STATIC_TRACE_CONFIRMED |
| RF22 | Upgrade quarters | TavernService.UpgradeQuarters() → SpendMoney → increase LevelQuarters | STATIC_TRACE_CONFIRMED |
| RF23 | Toggle settings | SettingsScreen → SettingsService → SaveData (settings) | STATIC_TRACE_CONFIRMED |

## Layer 4: Offline

| ID | Offline Action | Method | Status | Notes |
|----|---------------|--------|--------|-------|
| RO01 | Calculate delta | OfflineProgressService.CalculateOfflineDeltaSeconds() | STATIC_TRACE_CONFIRMED | lastAccess → now, caps at MAX_OFFLINE |
| RO02 | Workshop progress | OfflineProgressService → CraftService.ProgressWorkshop() | STATIC_TRACE_CONFIRMED | Deducts offline seconds from craft timers |
| RO03 | Market progress | OfflineProgressService → MerchantService.ProgressMarket() | STATIC_TRACE_CONFIRMED | Auto-refresh, auto-sell tracking |
| RO04 | Tavern progress | OfflineProgressService → TavernService.ProgressVisitorTime() | STATIC_TRACE_CONFIRMED | Regenerates guests |
| RO05 | Update LastAccess | SaveData.LastAccess updated after offline calc | STATIC_TRACE_CONFIRMED |
| RO06 | Active dungeon offline | OfflineProgressService → AutoCompleteDungeon() | PARTIAL | Auto-complete exists but loot reduction unconfirmed |

## Layer 5: Services

| ID | Interface | Implementation | Wired? | Status |
|----|-----------|---------------|--------|--------|
| SI01 | ISaveService | SaveService | ✅ | STATIC_TRACE_CONFIRMED |
| SI02 | ICharacterService | CharacterService | ✅ | STATIC_TRACE_CONFIRMED |
| SI03 | IInventoryService | InventoryService | ✅ | STATIC_TRACE_CONFIRMED |
| SI04 | IItemService | ItemService | ✅ | STATIC_TRACE_CONFIRMED |
| SI05 | IEquipmentService | EquipmentService | ✅ | STATIC_TRACE_CONFIRMED |
| SI06 | IDungeonService | DungeonService | ✅ | STATIC_TRACE_CONFIRMED |
| SI07 | ICombatService | CombatService | ✅ | STATIC_TRACE_CONFIRMED |
| SI08 | ILootService | LootService | ✅ | STATIC_TRACE_CONFIRMED |
| SI09 | ICraftService | CraftService | ✅ | STATIC_TRACE_CONFIRMED |
| SI10 | IMerchantService | MerchantService | ✅ | STATIC_TRACE_CONFIRMED |
| SI11 | IQuestService | QuestService | ✅ | STATIC_TRACE_CONFIRMED |
| SI12 | ITavernService | TavernService | ✅ | STATIC_TRACE_CONFIRMED |
| SI13 | IDoctrineService | DoctrineService | ✅ | STATIC_TRACE_CONFIRMED |
| SI14 | ISkillService | SkillService | ✅ | STATIC_TRACE_CONFIRMED |
| SI15 | IStatusEffectService | StatusEffectService | ✅ | STATIC_TRACE_CONFIRMED |
| SI16 | ISettingsService | SettingsService | ✅ | STATIC_TRACE_CONFIRMED |
| SI17 | IOfflineProgressService | OfflineProgressService | ✅ | STATIC_TRACE_CONFIRMED |
| SI18 | IFormulaService | FormulaService | ✅ | STATIC_TRACE_CONFIRMED |
| SI19 | IPlayerStatsService | PlayerStatsService | ✅ | STATIC_TRACE_CONFIRMED |

---

## Summary

| Status Category | Count |
|----------------|-------|
| STATIC_TRACE_CONFIRMED | 42 |
| PARTIAL | 3 (RS19, RV11, RO06) |
| NOT_RUN (needs Unity runtime) | 0 |
| TEST_VERIFIED | 0 |
| MANUAL_RUNTIME_VERIFIED | 0 |
| **Total** | **45** |

**Key observation:** 42/45 are STATIC_TRACE_CONFIRMED only — not runtime verified. Zero tests, zero runtime confirmations. Do NOT describe these as "verified" or "working" — only "traced statically."
