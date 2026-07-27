using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class DungeonService : IDungeonService
    {
        private DungeonRuntime _activeDungeon;
        private readonly ISaveService _saveService;
        private readonly GameDatabase _registry;

        public DungeonService(ISaveService saveService, GameDatabase registry)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _registry = registry ?? throw new ArgumentNullException(nameof(registry));
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
            
            SaveDungeonState();
        }

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

        public void Tick()
        {
            if (_activeDungeon == null) return;

            _activeDungeon.ActionTurnsPassed++;

            int requiredTurns = GetActionDuration(_activeDungeon.ActionType);
            if (_activeDungeon.ActionTurnsPassed >= requiredTurns)
            {
                // Transition state (Recovered state machine rules T-02)
                _activeDungeon.ActionTurnsPassed = 0;
                switch (_activeDungeon.ActionType)
                {
                    case 0: // ENTER_DUNGEON -> ENTER_ROOM
                        _activeDungeon.ActionType = 1;
                        break;
                    case 1: // ENTER_ROOM -> FIGHT
                        _activeDungeon.ActionType = 2;
                        break;
                    case 2: // FIGHT -> LOOT
                        _activeDungeon.Progress++;
                        _activeDungeon.ActionType = 3;
                        break;
                    case 3: // LOOT -> SEARCH_ROOM
                        _activeDungeon.ActionType = 4;
                        break;
                    case 4: // SEARCH_ROOM -> ENTER_ROOM
                        _activeDungeon.ActionType = 1;
                        break;
                    case 5: // RESPAWN / DEFEAT -> ENTER_ROOM (reset progress if < 250 - Rule T-05)
                        if (_activeDungeon.Progress < 250) _activeDungeon.Progress = 0;
                        _activeDungeon.ActionType = 1;
                        break;
                    case 6: // FLEE -> ENTER_ROOM
                        _activeDungeon.ActionType = 1;
                        break;
                    default:
                        _activeDungeon.ActionType = 1;
                        break;
                }
            }

            SaveDungeonState();
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
