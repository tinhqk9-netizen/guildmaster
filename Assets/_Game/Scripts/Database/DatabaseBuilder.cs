using System;
using System.Collections.Generic;
using UnityEngine;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Loaders.DTOs;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    public class DatabaseBuilder
    {
        private readonly IGameDataProvider _dataProvider;
        private readonly IJsonSerializer _serializer;
        private readonly GameDatabase _database;

        private delegate void CategoryLoaderDelegate(string jsonContent, ManifestFileEntry fileEntry, DatabaseBuildReport report);
        private readonly Dictionary<string, CategoryLoaderDelegate> _categoryLoaders;

        public DatabaseBuilder(IGameDataProvider dataProvider, IJsonSerializer serializer, GameDatabase database)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _database = database ?? throw new ArgumentNullException(nameof(database));
            
            _categoryLoaders = new Dictionary<string, CategoryLoaderDelegate>(StringComparer.OrdinalIgnoreCase)
            {
                { "items", LoadCategory<ItemDefinition> },
                { "enemies", LoadCategory<EnemyDefinition> },
                { "skills", LoadCategory<SkillDefinition> },
                { "status_effects", LoadCategory<StatusEffectDefinition> },
                { "adventurers", LoadCategory<AdventurerDefinition> },
                { "pets", LoadCategory<PetDefinition> },
                { "recipes", LoadCategory<RecipeDefinition> },
                { "quests", LoadCategory<QuestDefinition> },
                { "dungeons", LoadCategory<DungeonDefinition> },
                { "raids", LoadCategory<RaidDefinition> }
            };
        }

        public DatabaseBuildReport Build()
        {
            var report = new DatabaseBuildReport
            {
                providerName = _dataProvider.ProviderName
            };

            string manifestPath = "manifest.json";
            if (!_dataProvider.Exists(manifestPath))
            {
                report.errors.Add($"Manifest file not found at path: {manifestPath}");
                return report;
            }

            string manifestJson = _dataProvider.ReadText(manifestPath);
            ManifestDefinition manifest;
            try
            {
                manifest = _serializer.Deserialize<ManifestDefinition>(manifestJson);
                report.manifestLoaded = true;
            }
            catch (Exception ex)
            {
                report.errors.Add($"Failed to deserialize manifest.json: {ex.Message}");
                return report;
            }

            if (manifest.files == null)
            {
                report.warnings.Add("Manifest contains no files array.");
                return report;
            }

            report.expectedFiles = manifest.files.Count;

            foreach (var fileEntry in manifest.files)
            {
                string category = fileEntry.category;
                string filename = fileEntry.filename;

                if (string.Equals(category, "localization", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(category, "assets", StringComparison.OrdinalIgnoreCase))
                {
                    // Handled by other services
                    report.skippedFiles++;
                    continue;
                }

                if (!_categoryLoaders.TryGetValue(category, out var loaderDelegate))
                {
                    report.unsupportedCategories.Add(category);
                    report.warnings.Add($"Unsupported category '{category}' in manifest.");
                    report.skippedFiles++;
                    continue;
                }

                if (!_dataProvider.Exists(filename))
                {
                    report.errors.Add($"File '{filename}' defined in manifest does not exist.");
                    continue;
                }

                try
                {
                    string jsonContent = _dataProvider.ReadText(filename);
                    loaderDelegate(jsonContent, fileEntry, report);
                    report.loadedFiles++;
                }
                catch (Exception ex)
                {
                    report.errors.Add($"Error loading or deserializing '{filename}': {ex.Message}");
                }
            }

            return report;
        }

        private void LoadCategory<T>(string jsonContent, ManifestFileEntry fileEntry, DatabaseBuildReport report) where T : DefinitionBase
        {
            var definitionFile = _serializer.Deserialize<DefinitionFile<T>>(jsonContent);
            if (definitionFile == null || definitionFile.data == null)
            {
                report.errors.Add($"Deserialization of {fileEntry.filename} returned null data.");
                return;
            }

            var list = definitionFile.data;
            if (list.Count != fileEntry.recordCount)
            {
                report.recordCountMismatches.Add($"Category {fileEntry.category}: expected {fileEntry.recordCount}, loaded {list.Count}");
            }

            // Record missing/duplicate IDs just for the report before injecting to database
            var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in list)
            {
                if (string.IsNullOrEmpty(item.id))
                {
                    // Database ignores it and logs a warning
                }
                else if (!seenIds.Add(item.id))
                {
                    report.duplicateIds.Add($"[{fileEntry.category}] {item.id}");
                }
            }

            // Drop tables are JSON dictionaries, which JsonUtility skips entirely, so they are
            // parsed straight from the raw text and attached before the definitions go in.
            if (typeof(T) == typeof(EnemyDefinition))
            {
                var enemies = new List<EnemyDefinition>(list.Count);
                foreach (var item in list) enemies.Add((EnemyDefinition)(object)item);

                int withDrops = EnemyDropTableLoader.Apply(jsonContent, enemies);
                report.loadedRecordsByCategory["enemy_drop_tables"] = withDrops;
            }

            // ── Item enrichment ──────────────────────────────────────────────────────
            // items.json is extracted from the APK and only carries `parentClass`.
            // Unity's JsonUtility cannot infer Category or ItemType from that, so we
            // derive them here from the Java class hierarchy (proven from source).
            //
            // Java hierarchy (classes3.dex):
            //   Weapon → Sword, Dagger, Staff, Bow
            //   Armor  → LightArmor, HeavyArmor, MediumArmor
            //   Accessory
            //   Consumable → Food, Potion, Egg
            //   Upgrade, Item → Material
            //
            // ItemType must match AdventurerDefinition.WeaponType / ArmorType
            // which come from R.string values mapped in EnrichAdventurerDefinition().
            if (typeof(T) == typeof(ItemDefinition))
            {
                var items = new List<ItemDefinition>(list.Count);
                foreach (var raw in list) items.Add((ItemDefinition)(object)raw);

                int withFields = ItemFieldsLoader.Apply(jsonContent, items);
                report.loadedRecordsByCategory["item_fields"] = withFields;

                foreach (var raw in list)
                {
                    var def = (ItemDefinition)(object)raw;
                    EnrichItemDefinition(def);
                }
            }

            // ── Adventurer enrichment ────────────────────────────────────────────────
            // adventurers.json is also APK-extracted. WeaponType and ArmorType are
            // stored as R.string resource IDs in Java (e.g. R.string.type_sword) and
            // are not present in the JSON. We map them from parentClass + id here.
            if (typeof(T) == typeof(AdventurerDefinition))
            {
                foreach (var raw in list)
                {
                    var def = (AdventurerDefinition)(object)raw;
                    EnrichAdventurerDefinition(def);
                }
            }

            _database.RegisterCollection(list);
            report.loadedRecordsByCategory[fileEntry.category] = list.Count;
        }

        // ── Item enrichment helper ───────────────────────────────────────────────────
        /// <summary>
        /// Maps <c>parentClass</c> (Java class name from APK) to <c>Category</c> and
        /// <c>ItemType</c> on an <see cref="ItemDefinition"/>.
        /// Source of truth: Java abstract class hierarchy in classes3.dex.
        /// </summary>
        private static void EnrichItemDefinition(ItemDefinition def)
        {
            if (def == null) return;

            // Already populated (e.g. hand-authored entries) — don't overwrite.
            if (def.Category != ItemCategory.None) return;

            switch (def.parentClass)
            {
                // ── Weapons ──────────────────────────────────────────────
                case "Sword":
                    def.Category = ItemCategory.Weapon;
                    def.ItemType  = "Sword";
                    break;
                case "Dagger":
                    def.Category = ItemCategory.Weapon;
                    def.ItemType  = "Dagger";
                    break;
                case "Staff":
                    def.Category = ItemCategory.Weapon;
                    def.ItemType  = "Staff";
                    break;
                case "Bow":
                    def.Category = ItemCategory.Weapon;
                    def.ItemType  = "Bow";
                    break;
                case "Weapon":          // abstract base — generic weapon slot
                    def.Category = ItemCategory.Weapon;
                    break;

                // ── Armors ───────────────────────────────────────────────
                case "LightArmor":
                    def.Category = ItemCategory.Armor;
                    def.ItemType  = "LightArmor";
                    break;
                case "HeavyArmor":
                    def.Category = ItemCategory.Armor;
                    def.ItemType  = "HeavyArmor";
                    break;
                case "MediumArmor":
                    def.Category = ItemCategory.Armor;
                    def.ItemType  = "MediumArmor";
                    break;
                case "Armor":           // abstract base — generic armor slot
                    def.Category = ItemCategory.Armor;
                    break;

                // ── Accessories ──────────────────────────────────────────
                case "Accessory":
                    def.Category = ItemCategory.Accessory;
                    break;

                // ── Consumables ──────────────────────────────────────────
                case "Food":
                case "Potion":
                case "Egg":
                case "Consumable":
                    def.Category = ItemCategory.Consumable;
                    break;

                // ── Materials / Upgrades ─────────────────────────────────
                case "Upgrade":
                case "Item":            // base Java class — treat as material
                default:
                    def.Category = ItemCategory.Material;
                    break;
            }
        }

        // ── Adventurer enrichment helper ─────────────────────────────────────────────
        /// <summary>
        /// Maps Java <c>R.string.type_*</c> weapon/armor type IDs to the string values
        /// that <see cref="GuildMaster.Runtime.Services.EquipmentService.CanEquip"/> compares
        /// against <see cref="ItemDefinition.ItemType"/>.
        /// Mapping derived from each adventurer's <c>configureStatistics()</c> in the APK.
        /// </summary>
        private static void EnrichAdventurerDefinition(AdventurerDefinition def)
        {
            if (def == null) return;

            // Already populated — don't overwrite.
            if (!string.IsNullOrEmpty(def.WeaponType)) return;

            // Java stores weaponType as R.string resource IDs such as "type_sword".
            // The JSON extractor writes these as the resource name string.
            // We map them to the same strings used in EnrichItemDefinition above.
            //
            // Full mapping from every unit's configureStatistics() (classes3.dex):
            //   type_sword        → Sword   (Footman, Guard, Knight, Warrior, …)
            //   type_dagger       → Dagger  (Rogue, Thief, Assassin, …)
            //   type_bow          → Bow     (Archer, Marksman, …)
            //   type_staff        → Staff   (Apprentice, LightDisciple, Cleric, …)
            //   type_armor_heavy  → HeavyArmor
            //   type_armor_medium → MediumArmor
            //   type_armor_light  → LightArmor

            def.WeaponType = MapAdventurerWeaponType(def.id);
            def.ArmorType  = MapAdventurerArmorType(def.id);
        }

        // Weapon type per adventurer id (from Java configureStatistics, proven)
        private static string MapAdventurerWeaponType(string id)
        {
            switch (id)
            {
                // ── Sword users ──────────────────────────────────────────
                case "footman": case "guard": case "knight": case "warrior":
                case "angel_of_war": case "black_regent": case "bone_horror":
                case "bone_hydra": case "bone_nightmare": case "dark_knight":
                case "death_knight": case "divine_champion": case "divine_duelist":
                case "eternal_fortress": case "holy_knight": case "inquisitor":
                case "iron_defender": case "iron_warden": case "juggernaut":
                case "justiciar": case "kings_hand": case "overlord": case "paladin":
                case "royal_captain": case "royal_guard": case "royal_swordsman":
                case "scourge": case "skeleton": case "templar": case "titan":
                case "tyrant": case "undying_bastion": case "zombie":
                    return "Sword";

                // ── Dagger users ─────────────────────────────────────────
                case "rogue": case "thief": case "assassin": case "bard":
                case "cutthroat": case "eidolon": case "heavenly_cantor":
                case "hellish_sculptor": case "lorekeeper": case "meat_carver":
                case "minstrel": case "night_blade": case "night_lament":
                case "night_specter": case "night_terror": case "night_veil":
                case "red_stalker": case "shadow_crawler": case "shadow_dancer":
                case "silver_tongue": case "spire_acolyte": case "spire_initiate":
                case "spire_leader": case "spire_sage": case "spirit_engraver":
                case "trickster": case "whisper": case "wounds_weaver":
                    return "Dagger";

                // ── Bow users ────────────────────────────────────────────
                case "archer": case "alchemist": case "blight": case "celestial_rain":
                case "corrosive_wraith": case "drake_rider": case "eldritch_alchemist":
                case "elemental_alchemist": case "esoteric_alchemist": case "fury":
                case "golden_rider": case "hailstorm": case "horse_rider":
                case "huntress": case "hurricane": case "marksman": case "plague_spreader":
                case "poison_bow": case "spitfang_rider": case "sureshot":
                case "tempest": case "toxic_stalker": case "wolf_rider":
                case "worg_rider": case "wraith": case "wyrm_rider":
                    return "Bow";

                // ── Staff users ──────────────────────────────────────────
                case "apprentice": case "adept": case "ancient_lich": case "angel":
                case "archangel": case "balrog": case "black_idol": case "cleric":
                case "dark_sorcerer": case "demilich": case "demon":
                case "fire_wizard": case "infernal_lord": case "infernal_prince":
                case "inferno": case "lich": case "light_disciple": case "lord_of_decay":
                case "melting_elder": case "necromancer": case "radiant_elder":
                case "red_archmage": case "red_elder": case "red_mage":
                case "scorching_elder": case "unchained": case "white_archmage":
                case "white_elder": case "white_mage":
                    return "Staff";

                default:
                    return "Generic"; // allows any weapon
            }
        }

        // Armor type per adventurer id (from Java configureStatistics, proven)
        private static string MapAdventurerArmorType(string id)
        {
            switch (id)
            {
                // ── Heavy armor ──────────────────────────────────────────
                case "footman": case "guard": case "knight": case "warrior":
                case "angel_of_war": case "black_regent": case "bone_horror":
                case "bone_hydra": case "bone_nightmare": case "dark_knight":
                case "death_knight": case "divine_champion": case "divine_duelist":
                case "eternal_fortress": case "holy_knight": case "inquisitor":
                case "iron_defender": case "iron_warden": case "juggernaut":
                case "justiciar": case "kings_hand": case "overlord": case "paladin":
                case "royal_captain": case "royal_guard": case "royal_swordsman":
                case "scourge": case "skeleton": case "templar": case "titan":
                case "tyrant": case "undying_bastion": case "zombie":
                    return "HeavyArmor";

                // ── Medium armor ─────────────────────────────────────────
                case "rogue": case "thief": case "assassin": case "alchemist":
                case "archer": case "bard": case "blight": case "celestial_rain":
                case "corrosive_wraith": case "cutthroat": case "drake_rider":
                case "eidolon": case "eldritch_alchemist": case "elemental_alchemist":
                case "esoteric_alchemist": case "fury": case "golden_rider":
                case "hailstorm": case "heavenly_cantor": case "hellish_sculptor":
                case "horse_rider": case "huntress": case "hurricane":
                case "lorekeeper": case "marksman": case "meat_carver": case "minstrel":
                case "night_blade": case "night_lament": case "night_specter":
                case "night_terror": case "night_veil": case "plague_spreader":
                case "poison_bow": case "red_stalker": case "shadow_crawler":
                case "shadow_dancer": case "silver_tongue": case "spire_acolyte":
                case "spire_initiate": case "spire_leader": case "spire_sage":
                case "spirit_engraver": case "spitfang_rider": case "sureshot":
                case "tempest": case "trickster": case "toxic_stalker": case "whisper":
                case "wolf_rider": case "worg_rider": case "wounds_weaver":
                case "wraith": case "wyrm_rider":
                    return "MediumArmor";

                // ── Light armor ──────────────────────────────────────────
                case "apprentice": case "adept": case "ancient_lich": case "angel":
                case "archangel": case "balrog": case "black_idol": case "cleric":
                case "dark_sorcerer": case "demilich": case "demon":
                case "fire_wizard": case "infernal_lord": case "infernal_prince":
                case "inferno": case "lich": case "light_disciple": case "lord_of_decay":
                case "melting_elder": case "necromancer": case "radiant_elder":
                case "red_archmage": case "red_elder": case "red_mage":
                case "scorching_elder": case "unchained": case "white_archmage":
                case "white_elder": case "white_mage":
                    return "LightArmor";

                default:
                    return string.Empty;
            }
        }
    }
}
