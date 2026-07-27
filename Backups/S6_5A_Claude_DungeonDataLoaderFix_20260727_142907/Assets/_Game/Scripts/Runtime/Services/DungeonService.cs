using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Formulas;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class DungeonService : IDungeonService
    {
        /// <summary>Adventurers stop fighting after this many turns in a dungeon (Area case 2).</summary>
        private const int DUNGEON_TURN_CAP = 400;

        /// <summary>Losing only wipes progress below this threshold (Area case 5).</summary>
        private const int PROGRESS_KEEP_THRESHOLD = 250;

        private DungeonRuntime _activeDungeon;
        private readonly ISaveService _saveService;
        private readonly GameDatabase _registry;
        private readonly ICombatService _combatService;
        private readonly ILootService _lootService;
        private readonly ICharacterService _characterService;
        private readonly IInventoryService _inventoryService;
        private readonly Random _random;

        /// <summary>Party resolved at StartDungeon; combat mutates these instances directly.</summary>
        private readonly List<CharacterRuntime> _party = new List<CharacterRuntime>();

        public DungeonService(ISaveService saveService, GameDatabase registry,
                              ICombatService combatService = null,
                              ILootService lootService = null,
                              ICharacterService characterService = null,
                              IInventoryService inventoryService = null,
                              Random random = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
            _combatService = combatService;
            _lootService = lootService;
            _characterService = characterService;
            _inventoryService = inventoryService;
            _random = random ?? new Random();
        }

        public void StartDungeon(string dungeonId, List<string> adventurerIds)
        {
            var def = _registry.GetRequired<DungeonDefinition>(dungeonId);
            _activeDungeon = new DungeonRuntime(Guid.NewGuid().ToString(), def)
            {
                State = DungeonState.Unlocked,
                Progress = 0,
                MaxProgress = 0, // Placeholder, endless dungeon logic deferred
                AdventurerInstanceIds = new List<string>(adventurerIds)
            };
            
            ResolveParty();
            SaveDungeonState();
        }

        /// <summary>
        /// Looks the party up from CharacterService so combat mutates the same runtime
        /// instances the rest of the game sees.
        /// </summary>
        private void ResolveParty()
        {
            _party.Clear();
            if (_activeDungeon == null || _characterService == null) return;

            IReadOnlyList<CharacterRuntime> owned = _characterService.GetAllCharacters();
            foreach (string id in _activeDungeon.AdventurerInstanceIds)
            {
                CharacterRuntime match = owned.FirstOrDefault(c => c.InstanceId == id);
                if (match != null) _party.Add(match);
            }
        }

        /// <summary>Party members still standing.</summary>
        private int AdventurersAlive() => _party.Count(c => c.CurrentHp > 0);

        public void StopDungeon()
        {
            _activeDungeon = null;
            if (_saveService.CurrentData != null)
            {
                _saveService.CurrentData.ActiveDungeon = null;
            }
        }

        public void SaveDungeonState()
        {
            if (_activeDungeon == null || _saveService.CurrentData == null) return;

            var activeData = _saveService.CurrentData.ActiveDungeon ?? new ActiveDungeonSaveData();
            
            activeData.DungeonDefinitionId = _activeDungeon.Definition.id;
            activeData.Progress = _activeDungeon.Progress;
            activeData.MaxProgress = _activeDungeon.MaxProgress;
            activeData.LocalDarkness = _activeDungeon.LocalDarkness;
            activeData.AdventurerInstanceIds = new List<string>(_activeDungeon.AdventurerInstanceIds);
            
            activeData.PendingDrops = _activeDungeon.PendingDrops.Select(drop => new ItemSaveData
            {
                DefinitionId = drop.Definition.id,
                InstanceId = drop.InstanceId,
                StackCount = drop.StackCount,
                IsLocked = drop.IsLocked
            }).ToList();

            activeData.EncounterState = new CombatEncounterSaveData
            {
                TurnsFighting = _activeDungeon.TurnsFighting,
                SavedActingEntityId = _activeDungeon.SavedActingEntityId,
                Enemies = MapEnemies(_activeDungeon.Enemies),
                Corpses = MapEnemies(_activeDungeon.Corpses)
            };

            activeData.ActionState = new DungeonActionState
            {
                Type = _activeDungeon.ActionType,
                TurnsPassed = _activeDungeon.ActionTurnsPassed
            };

            _saveService.CurrentData.ActiveDungeon = activeData;
        }

        public void LoadDungeonState()
        {
            if (_saveService.CurrentData?.ActiveDungeon == null)
            {
                _activeDungeon = null;
                return;
            }

            var activeData = _saveService.CurrentData.ActiveDungeon;
            var def = _registry.GetRequired<DungeonDefinition>(activeData.DungeonDefinitionId);
            
            _activeDungeon = new DungeonRuntime(Guid.NewGuid().ToString(), def)
            {
                State = DungeonState.Unlocked,
                Progress = activeData.Progress,
                MaxProgress = activeData.MaxProgress,
                LocalDarkness = activeData.LocalDarkness,
                AdventurerInstanceIds = activeData.AdventurerInstanceIds != null ? new List<string>(activeData.AdventurerInstanceIds) : new List<string>(),
                PendingDrops = new List<ItemRuntime>(),
                ActionType = activeData.ActionState?.Type ?? 0,
                ActionTurnsPassed = activeData.ActionState?.TurnsPassed ?? 0,
                TurnsFighting = activeData.EncounterState?.TurnsFighting ?? 0,
                SavedActingEntityId = activeData.EncounterState?.SavedActingEntityId
            };

            if (activeData.PendingDrops != null)
            {
                foreach (var dropData in activeData.PendingDrops)
                {
                    var itemDef = _registry.GetRequired<ItemDefinition>(dropData.DefinitionId);
                    var itemRuntime = new ItemRuntime(dropData.InstanceId, itemDef)
                    {
                        StackCount = dropData.StackCount,
                        IsLocked = dropData.IsLocked
                    };
                    _activeDungeon.PendingDrops.Add(itemRuntime);
                }
            }

            if (activeData.EncounterState != null)
            {
                _activeDungeon.Enemies = HydrateEnemies(activeData.EncounterState.Enemies);
                _activeDungeon.Corpses = HydrateEnemies(activeData.EncounterState.Corpses);
            }
        }

        public bool IsDungeonActive() => _activeDungeon != null;

        public DungeonRuntime GetActiveDungeon() => _activeDungeon;

        public void AdvanceProgressOneStep()
        {
            if (_activeDungeon != null)
            {
                _activeDungeon.Progress++;
                SaveDungeonState();
            }
        }

        /// <summary>
        /// One second of dungeon time, mirroring <c>Area.tick()</c>: advance the current
        /// action's timer, and when it completes run <c>performAction()</c> to move the state
        /// machine on.
        ///
        /// Evidence: Reports/S6_5A/S6_5A_001D_Remaining_Core_Rules_Report.md (T-01, T-02)
        /// </summary>
        public void Tick()
        {
            if (_activeDungeon == null) return;

            _activeDungeon.ActionTurnsPassed++;

            if (_activeDungeon.ActionTurnsPassed >= GetActionDuration(_activeDungeon.ActionType))
            {
                _activeDungeon.ActionTurnsPassed = 0;
                PerformAction();
            }

            SaveDungeonState();
        }

        /// <summary>
        /// Port of <c>Area.performAction()</c>. Action ids follow the decode:
        /// 0 ENTER_DUNGEON · 1 ENTER_ROOM · 2 FIGHT · 3 LOOT · 4 SEARCH_ROOM ·
        /// 5 DEFEAT/RESPAWN · 6 FLEE.
        /// </summary>
        private void PerformAction()
        {
            switch (_activeDungeon.ActionType)
            {
                case 0: // ENTER_DUNGEON -> ENTER_ROOM
                    _activeDungeon.ActionType = 1;
                    break;

                case 1: // ENTER_ROOM: spawn a room's worth of enemies, then fight or move on
                    EnterRoom();
                    break;

                case 2: // FIGHT: one combat round per completed action
                    FightRound();
                    break;

                case 3: // LOOT: roll the corpses' drop tables into the area chest
                    RunLoot();
                    _activeDungeon.ActionType = 4;
                    break;

                case 4: // SEARCH_ROOM -> ENTER_ROOM, advancing depth
                    if (AdventurersAliveOrNoParty())
                    {
                        _activeDungeon.Progress++;
                        _activeDungeon.ActionType = 1;
                    }
                    else
                    {
                        _activeDungeon.ActionType = 5;
                    }
                    break;

                case 5: // DEFEAT/RESPAWN: progress is only wiped below the keep threshold
                    if (_activeDungeon.Progress < PROGRESS_KEEP_THRESHOLD) _activeDungeon.Progress = 0;
                    RestoreParty();
                    _activeDungeon.TurnsFighting = 0;
                    _activeDungeon.ActionType = 1;
                    break;

                case 6: // FLEE: drop the current encounter and re-enter
                    _activeDungeon.Enemies.Clear();
                    _activeDungeon.TurnsFighting = 0;
                    _activeDungeon.ActionType = 1;
                    break;

                default:
                    _activeDungeon.ActionType = 1;
                    break;
            }
        }

        /// <summary>
        /// A party of zero is treated as alive so a caller that never assigned adventurers can
        /// still walk the state machine; with a real party the usual alive check applies.
        /// </summary>
        private bool AdventurersAliveOrNoParty() => _party.Count == 0 || AdventurersAlive() > 0;

        /// <summary>
        /// <c>Area.enterRoom()</c> + <c>rollEnemies()</c>: populate the encounter from the
        /// area's own enemy list. With no enemies to spawn the run falls through to SEARCH_ROOM,
        /// exactly as the decode does when <c>rollEnemies()</c> comes back empty.
        /// </summary>
        private void EnterRoom()
        {
            if (!AdventurersAliveOrNoParty())
            {
                _activeDungeon.ActionType = 5;
                return;
            }

            _activeDungeon.Enemies = RollEnemies();
            _activeDungeon.TurnsFighting = 0;

            _activeDungeon.ActionType = _activeDungeon.Enemies.Count > 0 ? 2 : 4;
        }

        /// <summary>
        /// Picks one enemy from the area's <c>listEnemies()</c> set. The decode composes rooms
        /// from that same list; a single spawn keeps the encounter shape honest without
        /// inventing a room-size rule that was not recovered.
        /// </summary>
        private List<EnemyRuntime> RollEnemies()
        {
            var spawned = new List<EnemyRuntime>();

            List<string> pool = _activeDungeon.Definition?.EnemyIds;
            if (pool == null || pool.Count == 0) return spawned;

            string enemyId = pool[_random.Next(pool.Count)];
            if (!_registry.TryGet<EnemyDefinition>(enemyId, out EnemyDefinition def)) return spawned;

            spawned.Add(new EnemyRuntime(Guid.NewGuid().ToString(), def)
            {
                CurrentHp = def.BaseMaxHp,
                CurrentMana = 0,
                CurrentShield = 0
            });

            return spawned;
        }

        /// <summary>
        /// <c>Area.fightTurn()</c> wrapped in the case-2 guards: bail to FLEE past the turn cap,
        /// to DEFEAT when the party is wiped, and to LOOT once the room is clear.
        /// </summary>
        private void FightRound()
        {
            if (!AdventurersAliveOrNoParty())
            {
                _activeDungeon.ActionType = 5;
                return;
            }

            if (_activeDungeon.Enemies.Count == 0 || _activeDungeon.Enemies.All(e => e.IsDead))
            {
                MoveCorpsesAndAwardExperience();
                _activeDungeon.ActionType = 3;
                return;
            }

            if (_activeDungeon.TurnsFighting >= DUNGEON_TURN_CAP)
            {
                _activeDungeon.ActionType = 6;
                return;
            }

            _activeDungeon.TurnsFighting++;

            if (_combatService != null && _party.Count > 0)
            {
                string acting;
                _combatService.ProcessTurn(_party, _activeDungeon.Enemies, out acting);
                _activeDungeon.SavedActingEntityId = acting;
            }

            if (_activeDungeon.Enemies.All(e => e.IsDead))
            {
                MoveCorpsesAndAwardExperience();
                _activeDungeon.ActionType = 3;
            }
            else if (!AdventurersAliveOrNoParty())
            {
                _activeDungeon.ActionType = 5;
            }
        }

        /// <summary>
        /// Retires the dead into <c>corpses</c> (the list loot rolls from) and shares their
        /// experience across the survivors, per <c>Area.collectExperience()</c>:
        /// <c>totalExp / adventurersAlive()</c>, rounded with the decode's rounding.
        /// </summary>
        private void MoveCorpsesAndAwardExperience()
        {
            List<EnemyRuntime> dead = _activeDungeon.Enemies.Where(e => e.IsDead).ToList();
            if (dead.Count == 0) return;

            foreach (EnemyRuntime corpse in dead)
            {
                _activeDungeon.Enemies.Remove(corpse);
                _activeDungeon.Corpses.Add(corpse);
            }

            int alive = AdventurersAlive();
            if (alive <= 0 || _characterService == null) return;

            double totalExp = dead.Sum(c => c.Definition != null ? c.Definition.ExpGiven : 0);
            if (totalExp <= 0) return;

            int perHead = DecodeMath.Round(totalExp / alive);
            if (perHead <= 0) return;

            foreach (CharacterRuntime member in _party.Where(c => c.CurrentHp > 0))
            {
                _characterService.GainExperience(member, perHead);
            }
        }

        /// <summary>
        /// <c>Area.loot()</c>: roll every corpse's drop table into the temporary area chest.
        /// Nothing reaches the player's storage here — <see cref="CollectDrops"/> does that.
        /// </summary>
        private void RunLoot()
        {
            if (_lootService == null || _activeDungeon.Corpses.Count == 0) return;

            bool merchantPack = _saveService.CurrentData != null && _saveService.CurrentData.MerchantPackPurchased;
            if (_lootService.IsChestFull(_activeDungeon.PendingDrops, merchantPack)) return;

            foreach (EnemyRuntime corpse in _activeDungeon.Corpses)
            {
                List<DropTableEntry> table = BuildDropTable(corpse);
                if (table.Count == 0) continue;

                ItemRuntime drop = _lootService.RollSingleDrop(table);
                if (drop == null) continue;   // rolled into the table's miss gap

                _lootService.CollectPendingLoot(_activeDungeon.PendingDrops,
                                                new List<ItemRuntime> { drop },
                                                merchantPack);
            }

            _activeDungeon.Corpses.Clear();
        }

        /// <summary>Resolves an enemy's recovered drop table into item definitions.</summary>
        private List<DropTableEntry> BuildDropTable(EnemyRuntime enemy)
        {
            var table = new List<DropTableEntry>();
            if (enemy == null || enemy.Definition == null || enemy.Definition.DropTable == null) return table;

            foreach (EnemyDropEntry entry in enemy.Definition.DropTable)
            {
                if (entry == null || string.IsNullOrEmpty(entry.ItemId)) continue;

                ItemDefinition itemDef;
                if (!_registry.TryGet<ItemDefinition>(entry.ItemId, out itemDef)) continue;

                table.Add(new DropTableEntry
                {
                    Item = itemDef,
                    Weight = entry.Weight,
                    StackCount = entry.StackCount
                });
            }

            return table;
        }

        public int CollectDrops()
        {
            if (_activeDungeon == null || _inventoryService == null) return 0;
            if (_activeDungeon.PendingDrops.Count == 0) return 0;

            var moved = new List<ItemRuntime>(_activeDungeon.PendingDrops);
            int transferred = 0;

            foreach (ItemRuntime drop in moved)
            {
                if (!_inventoryService.CanAddItem(drop.Definition.id)) continue;

                _inventoryService.AddItem(drop);
                _activeDungeon.PendingDrops.Remove(drop);
                transferred++;
            }

            SaveDungeonState();
            return transferred;
        }

        /// <summary>Heals the party back to full, matching respawn after a wipe.</summary>
        private void RestoreParty()
        {
            if (_characterService == null) return;

            foreach (CharacterRuntime member in _party)
            {
                member.CurrentHp = _characterService.GetTotalStat(member, StatType.MaxHp);
                member.CurrentShield = 0;
            }
        }

        private int GetActionDuration(int actionType)
        {
            switch (actionType)
            {
                case 0: return 5;  // ENTER_DUNGEON
                case 1: return 5;  // ENTER_ROOM
                case 2: return 2;  // FIGHT
                case 3: return 5;  // LOOT
                case 4: return 5;  // SEARCH_ROOM
                case 5: return 18; // RESPAWN / DEFEAT
                case 6: return 12; // FLEE
                default: return 5;
            }
        }

        private List<EnemySaveData> MapEnemies(List<EnemyRuntime> runtimes)
        {
            if (runtimes == null) return new List<EnemySaveData>();
            return runtimes.Select(r => new EnemySaveData
            {
                DefinitionId = r.Definition.id,
                CurrentHp = (int)r.CurrentHp,
                CurrentMana = r.CurrentMana,
                CurrentShield = r.CurrentShield,
                PositiveStatusEffects = MapStatusEffects(r.PositiveStatusEffects),
                NegativeStatusEffects = MapStatusEffects(r.NegativeStatusEffects)
            }).ToList();
        }

        private List<EnemyRuntime> HydrateEnemies(List<EnemySaveData> savedData)
        {
            var list = new List<EnemyRuntime>();
            if (savedData == null) return list;
            foreach (var d in savedData)
            {
                var def = _registry.GetRequired<EnemyDefinition>(d.DefinitionId);
                var r = new EnemyRuntime(Guid.NewGuid().ToString(), def)
                {
                    CurrentHp = d.CurrentHp,
                    CurrentMana = d.CurrentMana,
                    CurrentShield = d.CurrentShield,
                    PositiveStatusEffects = HydrateStatusEffects(d.PositiveStatusEffects),
                    NegativeStatusEffects = HydrateStatusEffects(d.NegativeStatusEffects)
                };
                list.Add(r);
            }
            return list;
        }

        private List<StatusEffectSaveData> MapStatusEffects(List<StatusEffectRuntime> runtimes)
        {
            if (runtimes == null) return new List<StatusEffectSaveData>();
            return runtimes.Select(r => new StatusEffectSaveData
            {
                Type = r.Type,
                SourceInstanceId = r.SourceInstanceId,
                TurnsLeft = r.TurnsLeft
            }).ToList();
        }

        private List<StatusEffectRuntime> HydrateStatusEffects(List<StatusEffectSaveData> savedData)
        {
            var list = new List<StatusEffectRuntime>();
            if (savedData == null) return list;
            
            foreach (var d in savedData)
            {
                list.Add(new StatusEffectRuntime
                {
                    Type = d.Type,
                    SourceInstanceId = d.SourceInstanceId,
                    TurnsLeft = d.TurnsLeft
                });
            }
            return list;
        }
    }
}
