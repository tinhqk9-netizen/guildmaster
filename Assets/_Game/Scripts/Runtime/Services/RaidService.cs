using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Definitions.Enums;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public interface IRaidService
    {
        RaidRuntime ActiveRaid { get; }
        int CurrentDarkness { get; }
        bool IsUnlocked(RaidDefinition raid);
        bool StartRaid(string raidId, IReadOnlyList<string> partyIds, out string error);
        CombatResult FightCurrentRoom(out string error);
        bool CollectRewards(out string error);
        void AbandonRaid();
    }

    /// <summary>Legacy raid flow with persisted room/event/reward state.</summary>
    public sealed class RaidService : IRaidService
    {
        private readonly ISaveService _saveService;
        private readonly GameDatabase _database;
        private readonly ICombatService _combatService;
        private readonly ILootService _lootService;
        private readonly IInventoryService _inventoryService;
        private readonly ICharacterService _characterService;
        private readonly IQuestService _questService;
        private readonly IBestiaryService _bestiaryService;

        public RaidRuntime ActiveRaid { get; private set; }
        public int CurrentDarkness => ActiveRaid == null
            ? 0
            : (ActiveRaid.Definition.id.Equals("the_tower", StringComparison.OrdinalIgnoreCase) && ActiveRaid.LegacyProgress == 31
                ? 50
                : ActiveRaid.Definition.LegacyDarkness);

        public RaidService(ISaveService saveService, GameDatabase database, ICombatService combatService,
            ILootService lootService, IInventoryService inventoryService, ICharacterService characterService,
            IQuestService questService = null, IBestiaryService bestiaryService = null)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            _combatService = combatService ?? throw new ArgumentNullException(nameof(combatService));
            _lootService = lootService ?? throw new ArgumentNullException(nameof(lootService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _characterService = characterService ?? throw new ArgumentNullException(nameof(characterService));
            _questService = questService;
            _bestiaryService = bestiaryService;
            LoadActiveRaid();
        }

        public bool IsUnlocked(RaidDefinition raid)
        {
            if (raid == null) return false;
            if (string.IsNullOrEmpty(raid.RequiredClearDungeonId)) return true;
            return _saveService.CurrentData?.Dungeons?.Any(d =>
                string.Equals(d.DefinitionId, raid.RequiredClearDungeonId, StringComparison.OrdinalIgnoreCase) &&
                d.MaxProgress >= raid.RequiredClearProgress) == true;
        }

        public bool StartRaid(string raidId, IReadOnlyList<string> partyIds, out string error)
        {
            error = string.Empty;
            if (ActiveRaid != null) { error = "A raid is already active."; return false; }
            if (!_database.TryGet<RaidDefinition>(raidId, out var definition) || definition.Rooms == null || definition.Rooms.Count == 0)
            { error = "Raid encounter data is unavailable."; return false; }
            if (!IsUnlocked(definition)) { error = "Raid is locked."; return false; }

            var ids = partyIds?.Where(id => !string.IsNullOrEmpty(id)).Distinct().ToList() ?? new List<string>();
            if (ids.Count == 0) ids = _saveService.CurrentData?.CurrentParty?.ToList() ?? new List<string>();
            ActiveRaid = new RaidRuntime(definition);
            foreach (var id in ids)
            {
                var character = _characterService.GetAllCharacters().FirstOrDefault(c => c.InstanceId == id);
                if (character != null && character.CurrentHp > 0) ActiveRaid.Party.Add(character);
            }
            if (ActiveRaid.Party.Count == 0) { ActiveRaid = null; error = "Raid requires a living party."; return false; }
            ActiveRaid.AddLog("Raid started: " + definition.id);
            LoadNextRoom();
            SaveActiveRaid();
            return true;
        }

        public CombatResult FightCurrentRoom(out string error)
        {
            error = string.Empty;
            if (ActiveRaid == null || ActiveRaid.IsComplete || ActiveRaid.IsFailed)
            { error = "No active raid combat."; return CombatResult.None; }
            if (ActiveRaid.Enemies.Count == 0) LoadNextRoom();
            if (ActiveRaid.IsComplete) return CombatResult.Victory;
            if (ActiveRaid.Enemies.Count == 0) { error = "Raid room has no encounter."; return CombatResult.None; }

            CombatResult result = CombatResult.None;
            for (int i = 0; i < 400 && result == CombatResult.None; i++)
            {
                result = _combatService.ProcessTurn(ActiveRaid.Party, ActiveRaid.Enemies, out _);
            }
            if (result == CombatResult.Defeat)
            {
                ActiveRaid.IsFailed = true;
                ActiveRaid.AddLog("Raid failed: party defeated.");
                SaveActiveRaid();
                return result;
            }
            if (result != CombatResult.Victory)
            {
                error = "Raid combat reached the turn cap.";
                SaveActiveRaid();
                return result;
            }

            HandleKillCallbacks();
            AddEnemyLoot();
            ActiveRaid.AddLog("Room cleared: " + ActiveRaid.RoomIndex);
            ActiveRaid.RoomIndex++;
            if (ActiveRaid.HasActiveEvent)
                AdvanceEventAfterVictory();
            else
                ActiveRaid.LegacyProgress++;
            LoadNextRoom();
            SaveActiveRaid();
            return result;
        }

        public bool CollectRewards(out string error)
        {
            error = string.Empty;
            if (ActiveRaid == null || !ActiveRaid.IsComplete) { error = "Raid is not complete."; return false; }
            foreach (var reward in ActiveRaid.PendingRewards.ToList())
            {
                if (reward?.Definition == null) continue;
                if (!_inventoryService.CanAddItem(reward.Definition.id)) { error = "Inventory capacity is full."; return false; }
            }
            foreach (var reward in ActiveRaid.PendingRewards)
                if (reward != null) _inventoryService.AddItem(reward);
            _saveService.Save(out _);
            ActiveRaid.PendingRewards.Clear();
            _saveService.CurrentData.ActiveRaid = null;
            _saveService.Save(out _);
            return true;
        }

        public void AbandonRaid()
        {
            ActiveRaid = null;
            _saveService.CurrentData.ActiveRaid = null;
            _saveService.Save(out _);
        }

        private void LoadNextRoom()
        {
            if (ActiveRaid == null) return;
            ActiveRaid.Enemies.Clear();
            for (int guard = 0; guard < 100; guard++)
            {
                if (ActiveRaid.IsComplete || ActiveRaid.IsFailed) return;
                ApplyEnterRoomEffects();
                if (ActiveRaid.IsComplete || ActiveRaid.IsFailed) return;

                var ids = ResolveEncounterIds();
                if (ids != null && ids.Count > 0)
                {
                    foreach (string enemyId in ids)
                        if (_database.TryGet<EnemyDefinition>(enemyId, out var enemy))
                            ActiveRaid.Enemies.Add(new EnemyRuntime(Guid.NewGuid().ToString(), enemy));
                }
                if (ActiveRaid.Enemies.Count > 0)
                {
                    bool boss = ActiveRaid.Definition.LegacyEncounters?.Any(e =>
                        e.LegacyProgress == ActiveRaid.LegacyProgress && e.IsBossRoom) == true;
                    ActiveRaid.AddLog("Entered room " + ActiveRaid.LegacyProgress + (boss ? " (BOSS)" : string.Empty));
                    return;
                }

                ActiveRaid.AddLog("Empty raid room " + ActiveRaid.LegacyProgress);
                if (ActiveRaid.HasActiveEvent)
                    AdvanceEventWithoutEncounter();
                else
                {
                    ActiveRaid.LegacyProgress++;
                    if (ActiveRaid.Definition.IsEventDriven &&
                        ActiveRaid.LegacyProgress > ActiveRaid.Definition.LegacyMaxProgress)
                    {
                        CompleteRaid();
                    }
                }
            }
            ActiveRaid.IsFailed = true;
            ActiveRaid.AddLog("Raid stopped: encounter resolution guard reached.");
            SaveActiveRaid();
        }

        private void CompleteRaid()
        {
            if (ActiveRaid == null) return;
            ActiveRaid.IsComplete = true;
            ActiveRaid.AddLog("Raid cleared.");
            SaveActiveRaid();
        }

        private List<string> ResolveEncounterIds()
        {
            if (ActiveRaid == null) return new List<string>();
            if (ActiveRaid.Definition.IsEventDriven && !ActiveRaid.HasActiveEvent)
                return new List<string>();
            if (ActiveRaid.Definition.IsEventDriven)
                return ResolveEventEncounterIds();

            if (ActiveRaid.Definition.id.Equals("the_slime_pond", StringComparison.OrdinalIgnoreCase) &&
                ActiveRaid.LegacyProgress >= 2 && ActiveRaid.LegacyProgress <= 4)
            {
                int count = ActiveRaid.LegacyProgress + 1;
                var ids = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    double roll = new Random().NextDouble();
                    ids.Add(ResolveClassName(roll < 0.695d ? "Slime" : roll < 0.795d ? "FireSlime" :
                        roll < 0.895d ? "ElectricSlime" : roll < 0.995d ? "FrozenSlime" : "VoidSlime"));
                }
                return ids.Where(id => !string.IsNullOrEmpty(id)).ToList();
            }

            var encounter = ActiveRaid.Definition.LegacyEncounters?.FirstOrDefault(e =>
                e.LegacyProgress == ActiveRaid.LegacyProgress);
            if (encounter == null) return ActiveRaid.LegacyProgress > ActiveRaid.Definition.LegacyMaxProgress
                ? CompleteAndReturnEmpty()
                : new List<string>();
            if (encounter.IsBossRoom && IsUniqueRewardAlreadyOwned(encounter.UniqueRewardItemId)) return new List<string>();
            return encounter.EnemyIds?.ToList() ?? new List<string>();
        }

        private List<string> ResolveEventEncounterIds()
        {
            if (ActiveRaid == null || !ActiveRaid.HasActiveEvent)
                return new List<string>();
            if (ActiveRaid.EventKey.Equals("halls_exploration", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress == 14)
                    return ResolveClasses("Claris", "Thorvus");
                if (new[] { 1, 2, 3, 6, 7, 8, 11, 12, 13 }.Contains(ActiveRaid.EventProgress))
                {
                    double roll = new Random().NextDouble();
                    if (roll < 0.4d) return new List<string>();
                    if (roll < 0.75d) return ResolveClasses("LesserTitan");
                    return ResolveClasses("Crusader", "Crusader", "Crusader", "Crusader", "Crusader");
                }
            }
            else if (ActiveRaid.EventKey.Equals("halls_skeleton_door", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress == 2) return ResolveClasses("PrimordialTitan");
            }
            else if (ActiveRaid.EventKey.Equals("lost_expedition_trapdoor", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress == 5) return ResolveClasses("LostMiner", "LostMiner");
                if (ActiveRaid.EventProgress == 7)
                    return ResolveClasses("LostMiner", "LostMiner", "TekeliLiFirstApostle", "LostMiner", "LostMiner");
            }
            return new List<string>();
        }

        private void ApplyEnterRoomEffects()
        {
            if (ActiveRaid == null) return;
            string id = ActiveRaid.Definition.id;

            if (id.Equals("the_cultist_rebels", StringComparison.OrdinalIgnoreCase) &&
                ActiveRaid.EventKey == null && ActiveRaid.LegacyProgress >= 5)
            {
                ActiveRaid.EventKey = "halls_exploration";
                ActiveRaid.EventProgress = 1;
                ActiveRaid.EventOutcome = "Halls exploration started.";
                ActiveRaid.AddLog(ActiveRaid.EventOutcome);
            }

            if (id.Equals("divine_archeology", StringComparison.OrdinalIgnoreCase) && ActiveRaid.LegacyProgress == 12 && ActiveRaid.EventKey == null)
            {
                int constitution = ActiveRaid.Party.Where(p => p.CurrentHp > 0)
                    .Sum(p => _characterService.GetTotalStat(p, StatType.Constitution));
                if (constitution < 200)
                {
                    ActiveRaid.EventOutcome = $"Pyramid door remained closed (CON {constitution}/200).";
                    ActiveRaid.AddLog(ActiveRaid.EventOutcome);
                    ActiveRaid.IsComplete = true;
                    return;
                }
                ActiveRaid.EventKey = "pyramid_door_open";
                ActiveRaid.EventProgress = 1;
                ActiveRaid.AddLog("Pyramid door opened.");
            }

            if (id.Equals("the_lost_expedition", StringComparison.OrdinalIgnoreCase) &&
                ActiveRaid.LegacyProgress == 11 && ActiveRaid.EventKey == null && new Random().NextDouble() < 0.2d)
            {
                ActiveRaid.EventKey = "lost_expedition_trapdoor";
                ActiveRaid.EventProgress = 1;
                foreach (var hero in ActiveRaid.Party.Where(hero => hero.CurrentHp > 0))
                {
                    hero.CurrentHp = Math.Max(0, hero.CurrentHp - 40);
                }
                ActiveRaid.EventOutcome = "Trapdoor: living heroes took 40 damage.";
                ActiveRaid.AddLog(ActiveRaid.EventOutcome);
                ActiveRaid.EventProgress = 2;
            }

            if (id.Equals("the_tower", StringComparison.OrdinalIgnoreCase) &&
                new[] { 10, 14, 18, 24, 28, 33 }.Contains(ActiveRaid.LegacyProgress))
            {
                CharacterRuntime dead = null;
                foreach (var hero in ActiveRaid.Party)
                {
                    if (hero.CurrentHp <= 0 && dead == null) dead = hero;
                    if (hero.CurrentHp > 0) hero.CurrentHp = _characterService.GetTotalStat(hero, StatType.MaxHp);
                }
                if (dead != null) dead.CurrentHp = _characterService.GetTotalStat(dead, StatType.MaxHp);
                ActiveRaid.EventOutcome = dead == null ? "Tower healing event." : "Tower healing and resurrection event.";
                ActiveRaid.AddLog(ActiveRaid.EventOutcome);
            }
        }

        private void AdvanceEventAfterVictory()
        {
            if (ActiveRaid.EventKey.Equals("halls_exploration", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress == 8 && PartyHasSkeletonKey())
                {
                    ActiveRaid.EventKey = "halls_skeleton_door";
                    ActiveRaid.EventProgress = 0;
                    ActiveRaid.EventOutcome = "Skeleton Key opened the halls door.";
                    ActiveRaid.AddLog(ActiveRaid.EventOutcome);
                    return;
                }
                if (ActiveRaid.EventProgress >= 14) { CompleteRaid(); return; }
                ActiveRaid.EventProgress++;
            }
            else if (ActiveRaid.EventKey.Equals("halls_skeleton_door", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress >= 2) { CompleteRaid(); return; }
                ActiveRaid.EventProgress++;
            }
            else if (ActiveRaid.EventKey.Equals("lost_expedition_trapdoor", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress >= 8) { CompleteRaid(); return; }
                ActiveRaid.EventProgress++;
            }
            else if (ActiveRaid.EventKey.Equals("pyramid_door_open", StringComparison.OrdinalIgnoreCase))
            {
                ActiveRaid.EventKey = null;
                ActiveRaid.EventProgress = 0;
                ActiveRaid.LegacyProgress++;
            }
        }

        private void AdvanceEventWithoutEncounter()
        {
            if (ActiveRaid.EventKey.Equals("halls_exploration", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress >= 14) { CompleteRaid(); return; }
                ActiveRaid.EventProgress++;
            }
            else if (ActiveRaid.EventKey.Equals("halls_skeleton_door", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress >= 2) { CompleteRaid(); return; }
                ActiveRaid.EventProgress++;
            }
            else if (ActiveRaid.EventKey.Equals("lost_expedition_trapdoor", StringComparison.OrdinalIgnoreCase))
            {
                if (ActiveRaid.EventProgress >= 8) { CompleteRaid(); return; }
                ActiveRaid.EventProgress++;
            }
            else
            {
                ActiveRaid.LegacyProgress++;
            }
        }

        private void HandleKillCallbacks()
        {
            bool ritualTargetsDead = false;
            foreach (var enemy in ActiveRaid.Enemies)
            {
                if (enemy == null || !enemy.IsDead) continue;
                if (MatchesClass(enemy, "KabarTheRotten"))
                {
                    foreach (var necrolith in ActiveRaid.Enemies.Where(e => MatchesClass(e, "Necrolith"))) necrolith.CurrentHp = 0;
                    _questService?.IncrementDefinition("and_stay_dead", 1);
                    ActiveRaid.AddLog("Boss callback: Kabar destroyed the Necroliths.");
                }
                else if (MatchesClass(enemy, "ReinforcedDoor"))
                {
                    foreach (var gcss in ActiveRaid.Enemies.Where(e => MatchesClass(e, "Gcss"))) gcss.CurrentHp = 0;
                    _bestiaryService?.MarkSeen(ResolveClassName("Gcss"));
                    _bestiaryService?.MarkSeen(ResolveClassName("ReinforcedDoor"));
                    ActiveRaid.AddLog("Boss callback: Reinforced Door destroyed the Gcss.");
                }
                else if (MatchesClass(enemy, "PrimordialTitan")) _questService?.IncrementDefinition("endless_agony", 1);
                else if (MatchesClass(enemy, "AvatarOfTheAncient")) _questService?.IncrementDefinition("eldritch_horror", 1);
                else if (MatchesClass(enemy, "Claris") || MatchesClass(enemy, "Thorvus")) ritualTargetsDead = true;
                else if (MatchesClass(enemy, "SlimeKing")) _questService?.IncrementDefinition("regicide", 1);
            }
            if (ritualTargetsDead && ActiveRaid.Enemies.All(enemy => enemy == null || enemy.IsDead))
                _questService?.IncrementDefinition("botched_ritual", 1);
        }

        private bool PartyHasSkeletonKey() => ActiveRaid.Party.Any(hero =>
            string.Equals(hero.Accessory?.Definition?.id, "skeleton_key", StringComparison.OrdinalIgnoreCase));

        private bool MatchesClass(EnemyRuntime enemy, string sourceClass) => enemy?.Definition != null &&
            (string.Equals(enemy.Definition.className, sourceClass, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(enemy.Definition.id, ResolveClassName(sourceClass), StringComparison.OrdinalIgnoreCase));

        private string ResolveClassName(string sourceClass) => _database.GetAll<EnemyDefinition>()
            .FirstOrDefault(enemy => string.Equals(enemy.className, sourceClass, StringComparison.OrdinalIgnoreCase))?.id;

        private List<string> ResolveClasses(params string[] sourceClasses) => sourceClasses
            .Select(ResolveClassName).Where(id => !string.IsNullOrEmpty(id)).ToList();

        private List<string> CompleteAndReturnEmpty()
        {
            CompleteRaid();
            return new List<string>();
        }

        private bool IsUniqueRewardAlreadyOwned(string itemId)
        {
            return !string.IsNullOrEmpty(itemId) && _inventoryService.GetQuantityByDefinitionId(itemId) > 0;
        }

        private void AddEnemyLoot()
        {
            foreach (var enemy in ActiveRaid.Enemies)
            {
                var table = enemy.Definition.DropTable?.Select(drop => new DropTableEntry
                {
                    Item = _database.GetAll<ItemDefinition>().FirstOrDefault(item => item.id == drop.ItemId),
                    Weight = drop.Weight,
                    StackCount = drop.StackCount
                }).Where(drop => drop.Item != null).ToList();
                var item = _lootService.RollSingleDrop(table);
                if (item != null) _lootService.CollectPendingLoot(ActiveRaid.PendingRewards, new List<ItemRuntime> { item });
            }
        }

        private void SaveActiveRaid()
        {
            if (ActiveRaid == null) return;
            var saved = new RaidSaveData
            {
                DefinitionId = ActiveRaid.Definition.id,
                RoomIndex = ActiveRaid.RoomIndex,
                LegacyProgress = ActiveRaid.LegacyProgress,
                EventKey = ActiveRaid.EventKey,
                EventProgress = ActiveRaid.EventProgress,
                EventOutcome = ActiveRaid.EventOutcome,
                IsComplete = ActiveRaid.IsComplete,
                IsFailed = ActiveRaid.IsFailed,
                Log = ActiveRaid.Log.ToList(),
                Party = ActiveRaid.Party.Select(hero => new RaidPartyMemberSaveData
                {
                    InstanceId = hero.InstanceId,
                    CurrentHp = hero.CurrentHp,
                    CurrentMana = hero.CurrentMana,
                    CurrentShield = hero.CurrentShield
                }).ToList(),
                Enemies = MapEnemies(ActiveRaid.Enemies),
                PendingRewards = ActiveRaid.PendingRewards.Select(item => new ItemSaveData
                {
                    DefinitionId = item.Definition.id,
                    InstanceId = item.InstanceId,
                    StackCount = item.StackCount,
                    IsLocked = item.IsLocked
                }).ToList()
            };
            _saveService.CurrentData.ActiveRaid = saved;
            _saveService.Save(out _);
        }

        private void LoadActiveRaid()
        {
            var saved = _saveService.CurrentData?.ActiveRaid;
            if (saved == null || string.IsNullOrEmpty(saved.DefinitionId) ||
                !_database.TryGet<RaidDefinition>(saved.DefinitionId, out var definition)) return;
            ActiveRaid = new RaidRuntime(definition)
            {
                RoomIndex = saved.RoomIndex,
                LegacyProgress = saved.LegacyProgress <= 0 ? 1 : saved.LegacyProgress,
                EventKey = saved.EventKey,
                EventProgress = saved.EventProgress,
                EventOutcome = saved.EventOutcome,
                IsComplete = saved.IsComplete,
                IsFailed = saved.IsFailed
            };
            foreach (string line in saved.Log ?? new List<string>()) ActiveRaid.AddLog(line);
            foreach (var member in saved.Party ?? new List<RaidPartyMemberSaveData>())
            {
                var hero = _characterService.GetAllCharacters().FirstOrDefault(character => character.InstanceId == member.InstanceId);
                if (hero == null) continue;
                hero.CurrentHp = member.CurrentHp;
                hero.CurrentMana = member.CurrentMana;
                hero.CurrentShield = member.CurrentShield;
                ActiveRaid.Party.Add(hero);
            }
            foreach (var enemy in HydrateEnemies(saved.Enemies)) ActiveRaid.Enemies.Add(enemy);
            foreach (var item in saved.PendingRewards ?? new List<ItemSaveData>())
            {
                if (_database.TryGet<ItemDefinition>(item.DefinitionId, out var definitionItem))
                    ActiveRaid.PendingRewards.Add(new ItemRuntime(item.InstanceId, definitionItem, item.StackCount) { IsLocked = item.IsLocked });
            }
        }

        private List<EnemySaveData> MapEnemies(List<EnemyRuntime> runtimes) => (runtimes ?? new List<EnemyRuntime>()).Select(enemy => new EnemySaveData
        {
            DefinitionId = enemy.Definition.id,
            CurrentHp = enemy.CurrentHp,
            CurrentMana = enemy.CurrentMana,
            CurrentShield = enemy.CurrentShield,
            PositiveStatusEffects = MapStatusEffects(enemy.PositiveStatusEffects),
            NegativeStatusEffects = MapStatusEffects(enemy.NegativeStatusEffects)
        }).ToList();

        private List<EnemyRuntime> HydrateEnemies(List<EnemySaveData> saved) => (saved ?? new List<EnemySaveData>())
            .Where(data => data != null && _database.TryGet<EnemyDefinition>(data.DefinitionId, out _))
            .Select(data =>
            {
                _database.TryGet<EnemyDefinition>(data.DefinitionId, out var definition);
                return new EnemyRuntime(Guid.NewGuid().ToString(), definition)
                {
                    CurrentHp = data.CurrentHp,
                    CurrentMana = data.CurrentMana,
                    CurrentShield = data.CurrentShield,
                    PositiveStatusEffects = HydrateStatusEffects(data.PositiveStatusEffects),
                    NegativeStatusEffects = HydrateStatusEffects(data.NegativeStatusEffects)
                };
            }).ToList();

        private List<StatusEffectSaveData> MapStatusEffects(List<StatusEffectRuntime> effects) =>
            (effects ?? new List<StatusEffectRuntime>()).Select(effect => new StatusEffectSaveData
            {
                Type = effect.Type, SourceInstanceId = effect.SourceInstanceId, TurnsLeft = effect.TurnsLeft
            }).ToList();

        private List<StatusEffectRuntime> HydrateStatusEffects(List<StatusEffectSaveData> effects) =>
            (effects ?? new List<StatusEffectSaveData>()).Select(effect => new StatusEffectRuntime
            {
                Type = effect.Type, SourceInstanceId = effect.SourceInstanceId, TurnsLeft = effect.TurnsLeft
            }).ToList();
    }
}
