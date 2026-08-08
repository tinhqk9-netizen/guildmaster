using System;
using System.Collections.Generic;
using System.Linq;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    /// <summary>
    /// Room/boss payload transcribed from storage/data/places/raids/*.java. The source uses
    /// Java class names; this catalog resolves those names to the canonical EnemyDefinition id.
    /// No generated encounter or reward is added here.
    /// </summary>
    public static class RaidContentCatalog
    {
        private static RaidRoomData Room(bool boss, params string[] enemies) => new RaidRoomData
        {
            EnemySourceClasses = enemies?.ToList() ?? new List<string>(),
            IsBossRoom = boss
        };

        private static RaidEncounterData Encounter(int progress, bool boss, params string[] enemies) => new RaidEncounterData
        {
            LegacyProgress = progress,
            IsBossRoom = boss,
            EnemyIds = enemies?.ToList() ?? new List<string>()
        };

        private static RaidEncounterData UniqueEncounter(int progress, string reward, params string[] enemies) => new RaidEncounterData
        {
            LegacyProgress = progress,
            IsBossRoom = true,
            UniqueRewardItemId = reward,
            EnemyIds = enemies?.ToList() ?? new List<string>()
        };

        private static readonly Dictionary<string, List<RaidRoomData>> Rooms = new Dictionary<string, List<RaidRoomData>>(StringComparer.OrdinalIgnoreCase)
        {
            { "ancient_grave_digging", new List<RaidRoomData> {
                Room(false, "Undead", "Undead", "UndeadWarlord", "Undead", "Undead"),
                Room(false, "Undead", "UndeadWarlord", "Abomination", "UndeadWarlord", "Undead"),
                Room(false, "UndeadArcher", "UndeadWarlord", "UndeadGeneral", "UndeadWarlord", "UndeadArcher"),
                Room(true, "Necrolith", "KabarTheRotten", "Necrolith"),
                Room(false, "DeathHound", "UndeadWarlord", "UndeadWarlord", "UndeadWarlord", "DeathHound"),
                Room(false, "UndeadWarlord", "UndeadWarlord", "UndeadGeneral", "UndeadWarlord", "UndeadWarlord") } },
            { "celestial_mothership", new List<RaidRoomData> {
                Room(false), Room(false, "Oculus"), Room(false, "Oculus", "CelestialLancer", "CelestialLancer", "CelestialLancer", "Oculus"),
                Room(false, "CelestialLancer", "CelestialLancer", "CelestialLancer", "CelestialLancer", "CelestialLancer"),
                Room(false, "CelestialLancer", "CelestialLancer", "CelestialDestroyer", "CelestialLancer", "CelestialLancer"),
                Room(false, "CelestialDestroyer", "CelestialLancer", "CelestialLancer", "CelestialLancer", "CelestialDestroyer"),
                Room(false, "Gcss", "ReinforcedDoor", "Gcss"), Room(false), Room(true, "LegateHadrian") } },
            { "divine_archeology", new List<RaidRoomData> {
                Room(false), Room(false, "ShahuriWarrior", "ShahuriArcher", "ShahuriMage", "ShahuriArcher", "ShahuriWarrior"),
                Room(true, "ShaKireFirstSwordsman"), Room(false), Room(true, "ShaTheHiddenGod"),
                Room(false, "SandDemon", "SandDemon", "SandDemon", "SandDemon", "SandDemon") } },
            { "imperial_rescue", new List<RaidRoomData> {
                Room(false), Room(false, "InsaneCitizen", "InsaneCitizen", "CityWarden", "InsaneCitizen", "InsaneCitizen"),
                Room(false, "InsaneCitizen", "CityWarden", "InsaneMerchant", "CityWarden", "InsaneCitizen"),
                Room(false, "CityWarden", "InsaneCitizen", "ImperialGuard", "InsaneCitizen", "CityWarden"),
                Room(false, "ImperialGuard", "ImperialGuard", "ImperialGuard", "ImperialGuard", "ImperialGuard"),
                Room(false, "ImperialGuard", "ImperialMage", "ImperialGuard", "ImperialMage", "ImperialGuard"),
                Room(false, "InsaneCitizen", "InsaneMerchant", "InsaneCitizen", "InsaneMerchant", "InsaneCitizen"),
                Room(false, "ImperialGuard", "ImperialMage", "ImperialGuard", "ImperialMage", "ImperialGuard"), Room(true, "EmperorClovisXXVIII") } },
            { "kaunis", new List<RaidRoomData> {
                Room(false, "Necrobot", "Necrobot", "Necrobot"), Room(false, "Necrobot", "Necrobot", "Enforcer", "Necrobot", "Necrobot"),
                Room(false, "Phantasm"), Room(false, "Necrobot", "Enforcer", "Enforcer", "Necrobot"),
                Room(false, "Necrobot", "Necrobot", "Cerebrum", "Necrobot", "Necrobot"), Room(false, "Necrobot", "Phantasm", "Necrobot"), Room(false) } },
            { "sleeping_planet", new List<RaidRoomData> {
                Room(false, "DreamwroughtBeast", "DreamwroughtBeast", "DreamwroughtBeast"), Room(false, "DreamwroughtBeast", "DreamwroughtDragon", "DreamwroughtBeast"),
                Room(false, "DreamwroughtBeast", "DreamwroughtSwarm", "DreamwroughtBeast"), Room(false, "DreamwroughtBeast", "DreamwroughtForge", "DreamwroughtBeast"),
                Room(true, "Singularity"), Room(false) } },
            { "the_cultist_rebels", new List<RaidRoomData> { Room(false), Room(true, "Claris", "Thorvus"), Room(true, "PrimordialTitan"), Room(false, "LesserTitan"), Room(false, "Crusader", "Crusader", "Crusader", "Crusader", "Crusader") } },
            { "the_dire_descent", new List<RaidRoomData> { Room(false), Room(false), Room(true, "HeraldXavi", "HeraldMaya", "HeraldShoran") } },
            { "the_dreadful_ascent", new List<RaidRoomData> { Room(false), Room(false, "EtherealSoul", "EtherealSoul", "EtherealSoul"), Room(false, "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul"), Room(false, "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul"), Room(true, "KasimirTheSeer"), Room(false), Room(true, "HeraldKali") } },
            { "the_lost_expedition", new List<RaidRoomData> { Room(false, "LostMiner"), Room(false, "LostMiner", "LostMiner", "LostMiner", "LostMiner", "LostMiner"), Room(false, "BleakDisciple", "EldritchHound", "BleakDisciple"), Room(false, "EldritchHound", "EldritchHound", "BleakDisciple", "EldritchHound", "EldritchHound"), Room(false, "EldritchHound", "BleakDisciple", "BleakDeacon", "BleakDisciple", "EldritchHound"), Room(false, "BleakDisciple", "AvatarOfTheAncient", "BleakDisciple"), Room(false, "LostMiner", "LostMiner"), Room(true, "LostMiner", "LostMiner", "TekeliLiFirstApostle", "LostMiner", "LostMiner") } },
            { "the_slime_pond", new List<RaidRoomData> { Room(true, "SlimeKing"), Room(false), Room(false, "Slime", "FireSlime", "ElectricSlime", "FrozenSlime", "VoidSlime") } },
            { "the_tower", new List<RaidRoomData> { Room(true, "Lazarus"), Room(true, "Phoenix"), Room(true, "HeadlessKnight"), Room(true, "Ultraslime"), Room(true, "TheExiled"), Room(true, "TheAncient"), Room(true, "TheMachine") } }
        };

        private static readonly Dictionary<string, string> UniqueRewards = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "celestial_mothership", "evo23_vial" },
            { "divine_archeology", "eyes_of_the_swordsman" },
            { "imperial_rescue", "skeleton_key" },
            { "the_dire_descent", "serpent_lunge" },
            { "the_dreadful_ascent", "serpent_staff" }
        };

        // Direct transcription of each Java rollEnemies() fixed encounter. Random/event-driven
        // branches remain in RaidService because they depend on the persisted Event state.
        private static readonly Dictionary<string, List<RaidEncounterData>> FixedEncounters = new Dictionary<string, List<RaidEncounterData>>(StringComparer.OrdinalIgnoreCase)
        {
            { "ancient_grave_digging", new List<RaidEncounterData> {
                Encounter(3, false, "Undead", "Undead", "UndeadWarlord", "Undead", "Undead"),
                Encounter(4, false, "Undead", "UndeadWarlord", "Abomination", "UndeadWarlord", "Undead"),
                Encounter(6, false, "UndeadArcher", "UndeadWarlord", "UndeadGeneral", "UndeadWarlord", "UndeadArcher"),
                Encounter(8, false, "DeathHound", "UndeadWarlord", "UndeadWarlord", "UndeadWarlord", "DeathHound"),
                Encounter(9, false, "UndeadWarlord", "UndeadWarlord", "UndeadGeneral", "UndeadWarlord", "UndeadWarlord"),
                Encounter(11, true, "Necrolith", "KabarTheRotten", "Necrolith") } },
            { "celestial_mothership", new List<RaidEncounterData> {
                Encounter(2, false, "Oculus"), Encounter(3, false, "Oculus", "CelestialLancer", "CelestialLancer", "CelestialLancer", "Oculus"),
                Encounter(4, false, "CelestialLancer", "CelestialLancer", "CelestialLancer", "CelestialLancer", "CelestialLancer"),
                Encounter(5, false, "CelestialLancer", "CelestialLancer", "CelestialDestroyer", "CelestialLancer", "CelestialLancer"),
                Encounter(6, false, "CelestialDestroyer", "CelestialLancer", "CelestialLancer", "CelestialLancer", "CelestialDestroyer"),
                Encounter(8, false, "CelestialLancer", "CelestialLancer", "CelestialDestroyer", "CelestialLancer", "CelestialLancer"),
                Encounter(9, false, "CelestialDestroyer", "CelestialLancer", "CelestialLancer", "CelestialLancer", "CelestialDestroyer"),
                Encounter(12, false, "Gcss", "ReinforcedDoor", "Gcss"), Encounter(15, false, "Gcss", "ReinforcedDoor", "Gcss"),
                UniqueEncounter(17, "evo_23_vial", "LegateHadrian") } },
            { "divine_archeology", new List<RaidEncounterData> {
                Encounter(2, false, "ShahuriWarrior", "ShahuriArcher", "ShahuriMage", "ShahuriArcher", "ShahuriWarrior"),
                Encounter(4, false, "SandDemon", "SandDemon", "SandDemon", "SandDemon", "SandDemon"),
                Encounter(5, false, "SandDemon", "SandDemon", "SandDemon", "SandDemon", "SandDemon"),
                Encounter(6, false, "SandDemon", "SandDemon", "SandDemon", "SandDemon", "SandDemon"),
                UniqueEncounter(9, "eyes_of_the_swordsman", "ShaKireFirstSwordsman"),
                UniqueEncounter(12, "divine_zygote", "ShaTheHiddenGod") } },
            { "imperial_rescue", new List<RaidEncounterData> {
                Encounter(1, false, "InsaneCitizen", "InsaneCitizen", "CityWarden", "InsaneCitizen", "InsaneCitizen"),
                Encounter(2, false, "InsaneCitizen", "CityWarden", "InsaneMerchant", "CityWarden", "InsaneCitizen"),
                Encounter(3, false, "CityWarden", "InsaneCitizen", "ImperialGuard", "InsaneCitizen", "CityWarden"),
                Encounter(6, false, "ImperialGuard", "ImperialGuard", "ImperialGuard", "ImperialGuard", "ImperialGuard"),
                Encounter(7, false, "ImperialGuard", "ImperialGuard", "ImperialMage", "ImperialGuard", "ImperialGuard"),
                Encounter(9, false, "InsaneCitizen", "InsaneMerchant", "InsaneCitizen", "InsaneMerchant", "InsaneCitizen"),
                Encounter(11, false, "ImperialGuard", "ImperialMage", "ImperialGuard", "ImperialMage", "ImperialGuard"),
                UniqueEncounter(14, "skeleton_key", "EmperorClovisXXVIII") } },
            { "kaunis", new List<RaidEncounterData> {
                Encounter(1, false, "Necrobot", "Necrobot", "Necrobot"), Encounter(6, false, "Necrobot", "Necrobot", "Enforcer", "Necrobot", "Necrobot"),
                Encounter(9, false, "Phantasm"), Encounter(10, false, "Necrobot", "Enforcer", "Enforcer", "Necrobot"),
                Encounter(11, false, "Necrobot", "Necrobot", "Cerebrum", "Necrobot", "Necrobot"),
                Encounter(12, false, "Necrobot", "Phantasm", "Necrobot"), Encounter(16, true, "ChiefScientistAva", "KingAino", "FirstMinisterAtos") } },
            { "sleeping_planet", new List<RaidEncounterData> {
                Encounter(5, false, "DreamwroughtBeast", "DreamwroughtBeast", "DreamwroughtBeast"),
                Encounter(8, false, "DreamwroughtBeast", "DreamwroughtDragon", "DreamwroughtBeast"),
                Encounter(10, false, "DreamwroughtBeast", "DreamwroughtSwarm", "DreamwroughtBeast"),
                Encounter(12, false, "DreamwroughtBeast", "DreamwroughtForge", "DreamwroughtBeast"), Encounter(14, true, "Singularity") } },
            { "the_dire_descent", new List<RaidEncounterData> { UniqueEncounter(5, "serpent_lunge", "HeraldXavi", "HeraldMaya", "HeraldShoran") } },
            { "the_dreadful_ascent", new List<RaidEncounterData> {
                Encounter(2, false, "EtherealSoul", "EtherealSoul", "EtherealSoul"), Encounter(3, false, "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul"),
                Encounter(4, false, "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul"),
                Encounter(5, false, "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul"),
                Encounter(8, false, "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul", "EtherealSoul"),
                Encounter(10, false, "KasimirTheSeer"), UniqueEncounter(11, "serpent_staff", "HeraldKali") } },
            { "the_slime_pond", new List<RaidEncounterData> { Encounter(6, true, "SlimeKing") } },
            { "the_tower", new List<RaidEncounterData> {
                Encounter(8, true, "Lazarus"), Encounter(12, true, "Phoenix"), Encounter(16, true, "HeadlessKnight"),
                Encounter(22, true, "Ultraslime"), Encounter(26, true, "TheExiled"), Encounter(31, true, "TheAncient"), Encounter(35, true, "TheMachine") } }
        };

        private static readonly Dictionary<string, (int party, int max, int darkness, bool eventDriven, string[] events)> Metadata =
            new Dictionary<string, (int, int, int, bool, string[])>(StringComparer.OrdinalIgnoreCase)
        {
            { "ancient_grave_digging", (5, 12, 0, false, Array.Empty<string>()) },
            { "celestial_mothership", (5, 19, 0, false, Array.Empty<string>()) },
            { "divine_archeology", (5, 13, 0, false, new[] { "pyramid_door_open" }) },
            { "imperial_rescue", (5, 15, 0, false, Array.Empty<string>()) },
            { "kaunis", (5, 16, 0, false, Array.Empty<string>()) },
            { "sleeping_planet", (5, 15, 0, false, Array.Empty<string>()) },
            { "the_cultist_rebels", (8, 14, 0, true, new[] { "halls_exploration", "halls_skeleton_door" }) },
            { "the_dire_descent", (5, 8, 0, false, Array.Empty<string>()) },
            { "the_dreadful_ascent", (5, 13, 0, false, Array.Empty<string>()) },
            { "the_lost_expedition", (8, 15, 100, true, new[] { "lost_expedition_trapdoor" }) },
            { "the_slime_pond", (5, 7, 0, false, Array.Empty<string>()) },
            { "the_tower", (14, 39, 0, false, Array.Empty<string>()) }
        };

        public static void Apply(IEnumerable<RaidDefinition> raids, IEnumerable<EnemyDefinition> enemies)
        {
            var enemyByClass = enemies?.Where(e => e != null).ToDictionary(e => e.className, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, EnemyDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (var raid in raids ?? Enumerable.Empty<RaidDefinition>())
            {
                if (!Rooms.TryGetValue(raid.id ?? string.Empty, out var sourceRooms)) continue;
                raid.Rooms = sourceRooms.Select(room => new RaidRoomData
                {
                    EventKey = room.EventKey,
                    IsBossRoom = room.IsBossRoom,
                    LegacyProgress = room.LegacyProgress,
                    EnemySourceClasses = room.EnemySourceClasses
                        .Where(enemyByClass.ContainsKey)
                        .Select(source => enemyByClass[source].id).ToList()
                }).ToList();
                if (FixedEncounters.TryGetValue(raid.id ?? string.Empty, out var fixedEncounters))
                {
                    raid.LegacyEncounters = fixedEncounters.Select(encounter => new RaidEncounterData
                    {
                        LegacyProgress = encounter.LegacyProgress,
                        IsBossRoom = encounter.IsBossRoom,
                        UniqueRewardItemId = encounter.UniqueRewardItemId,
                        EnemyIds = encounter.EnemyIds.Where(enemyByClass.ContainsKey)
                            .Select(source => enemyByClass[source].id).ToList()
                    }).ToList();
                }
                if (Metadata.TryGetValue(raid.id ?? string.Empty, out var metadata))
                {
                    raid.LegacyPartySize = metadata.party;
                    raid.LegacyMaxProgress = metadata.max;
                    raid.LegacyDarkness = metadata.darkness;
                    raid.IsEventDriven = metadata.eventDriven;
                    raid.LegacyEventKeys = metadata.events.ToList();
                }
                raid.EnemyIds = raid.Rooms.SelectMany(room => room.EnemySourceClasses).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (UniqueRewards.TryGetValue(raid.id ?? string.Empty, out var reward)) raid.UniqueRewardItemId = reward;
            }
        }
    }
}
