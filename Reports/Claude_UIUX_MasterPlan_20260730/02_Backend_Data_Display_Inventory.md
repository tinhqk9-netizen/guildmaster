# 02 — Backend Data Display Inventory

Classification: MUST_SHOW (player needs this to make a decision), SHOULD_SHOW (useful context), DETAIL_ONLY (secondary panel/drill-in), DEBUG_ONLY (decode-audit metadata, never player-facing), NOT_PLAYER_FACING.

| System | Backend field/value | Meaning | Player relevance | Class. | Real-time refresh needed | Current UI coverage | Evidence |
|---|---|---|---|---|---|---|---|
| Currency | SaveData.Money | Soft currency balance | Spend gating everywhere | MUST_SHOW | Yes (after every spend/earn) | HUD text, refreshed only on HUD Show() — **goes stale while other screens change it** | HUDController.cs:47-63 |
| Currency | SaveData.Gems | Premium currency balance | Same | MUST_SHOW | Yes | HUD text, same staleness issue | HUDController.cs:47-63 |
| Adventurer | CharacterRuntime.Level/Experience | Progression | Core stat | MUST_SHOW | On level-up | CharacterScreen detail text | CharacterScreen.cs:114-115 |
| Adventurer | CharacterRuntime.CurrentHp / max (via GetTotalStat) | Survivability | Core | MUST_SHOW | During combat | CharacterScreen shows CurrentHp only, no max-HP comparison shown | CharacterScreen.cs:114 |
| Adventurer | GetTotalStat(Constitution/Dexterity/Defense/Intelligence/MagicDefense/ImmunityToStatus) | Full stat block incl. doctrine/pet/equipment/trait bonuses | Core build info | MUST_SHOW | On equip/level/promote | CharacterScreen shows all 6 as plain text list, no breakdown of bonus sources | CharacterScreen.cs:121-126 |
| Adventurer | Weapon/Armor/Accessory (ItemRuntime or "(none)") | Equipped gear | Core | MUST_SHOW | On equip/unequip | CharacterScreen text row | CharacterScreen.cs:130-132 |
| Adventurer | Trait (string) | Rolled trait | Player wants to know effect | SHOULD_SHOW | rare (assigned once at recruit) | Shown as name only; **7/10 traits have no effect to describe** (would mislead if effect text is added without backend fix) | CharacterService.cs:270-290 |
| Adventurer | IsAscended/AscensionLevel | Promotion tier | Progression flex | SHOULD_SHOW | on promote (currently unreachable — data gap) | Not shown anywhere in CharacterScreen | CharacterRuntime |
| Inventory item | Definition.id, StackCount, IsLocked | Identity/qty/lock state | Core | MUST_SHOW | On any inventory mutation | InventoryScreen card + detail | InventoryScreen.cs:103-121 |
| Inventory item | "Equipped" state | Whether item is currently worn | Prevents accidental sell of equipped gear | MUST_SHOW | On equip/unequip | **Stub always returns false — never shown** (real gap) | InventoryScreen.cs:97-101 |
| Inventory | GetCapacity()/current count | Storage pressure | Core | MUST_SHOW | On add/remove | InventoryScreen summary text | InventoryService.cs:64-70 |
| Tavern guest | DefinitionId, Level, InstanceId | Recruit candidate identity | Core | MUST_SHOW | On visitor roll | TavernScreen card | TavernScreen.cs:99-121 |
| Tavern | NextTavernVisit countdown | Time to next guest | Core | MUST_SHOW | **Live** (currently snapshot-only, no ticking) | TavernScreen timer text, stale while idle | TavernScreen.cs:60-68 |
| Tavern | GetTavernCapacity/GetQuartersCapacity | Roster caps | Core | MUST_SHOW | On upgrade | TavernScreen summary | TavernService.cs:33-43 |
| Dungeon | DungeonRuntime.State/ClearCount/BestTimeSeconds | Progress record | Player achievement tracking | SHOULD_SHOW | On completion (never fires — dead write) | DungeonScreen select panel shows dungeon id only, not clear count/best time | DungeonRuntime.cs; DungeonService.cs (no Completed write) |
| Dungeon | Progress/MaxProgress | Run advancement | Core (but MaxProgress always 0 — placeholder) | MUST_SHOW (currently meaningless) | Live during run | Not shown as numeric or bar anywhere | DungeonRuntime; DungeonService.cs:75 |
| Dungeon combat | Enemy.CurrentHp, Party HP | Fight state | MUST_SHOW during Active panel | Live | Text only ("HP: {e.CurrentHp}"), no bar, no max reference | DungeonScreen.cs:188-197 |
| Loot | PendingDrops list | What's in the chest | MUST_SHOW before collect | On roll | DungeonScreen Loot panel cards | DungeonService.cs:448-495 |
| Quest | Progress/TargetProgress/State | Completion state | MUST_SHOW | On increment | QuestScreen text ("Progress: X/Y (Z%)"), **no progress bar** (CreateProgressBar unused) | QuestScreen.cs:87-124; UICardFactory.cs:149-195 |
| Doctrine | 8× Level/Progress | Long-term meta-progression | SHOULD_SHOW | rare | **Not shown anywhere** — only a claim-target cycler exists | DoctrineService.cs; QuestScreen.cs:29 |
| Craft | Recipe ingredient availability | Can I craft this now | MUST_SHOW, must be accurate | On inventory change | **Hardcoded to always show available (999)** — actively misleading | CraftScreen.cs:214-218 |
| Craft queue | ItemActionSaveData.SecondsPassed vs duration | Time remaining | MUST_SHOW, live | Live | Text status only, no live countdown, no bar | CraftScreen.cs:260 |
| Craft | GetQueueCapacity/level/upgrade price | Queue slots | SHOULD_SHOW | On upgrade | CraftScreen summary | CraftService.cs:33-195 |
| Merchant stock | Regular/Special offers | What's buyable | MUST_SHOW | On roll (never populated — see gap register) | MerchantScreen Buy tab (will render empty on fresh save) | MerchantService.cs:27-32 |
| Market listing | Item, price, sell timer | Sell-in-progress state | MUST_SHOW, live | Live | Text only, no live timer | MerchantScreen Listings tab |
| Pets | Level/Exp/EquippedToCharacterId, stat bonuses | Pet roster & assignment | MUST_SHOW | on level/equip | **No UI at all** | PetService.cs |
| Settings | 5 toggles + language + version | Player prefs | MUST_SHOW | on toggle | SettingsScreen (4 of 5 toggles interactive; Cloud is display-only) | SettingsScreen.cs:61-132 |
| Decode-audit metadata | className, parentClass, recordHash, parseStatus, manualRuleRequired, sourcePath, parseReasons | Build/QA provenance of decoded data | Never player-facing | DEBUG_ONLY | n/a | Correctly never surfaced in any screen (confirmed no UI reads these fields) | DefinitionBase.cs:7-17 |

Full per-screen field reads are cited in `evidence/ui_trace_raw.md` (screen-by-screen section) and cross-referenced against `01_Backend_System_Inventory.md`.
