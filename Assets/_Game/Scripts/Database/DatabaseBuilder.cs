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

            // Phase 2A: Doctrine catalog has no JSON source (no doctrines.json anywhere in the
            // decoded data — see Docs/Backend_Audit/phase2a_audit_report.md). Registered directly
            // from the hand-transcribed DoctrineCatalog (real Java DoctrineAbilityType /
            // DoctrineOf* data, not fabricated) so DoctrineDefinition flows through GameDatabase
            // exactly like every JSON-sourced category.
            RegisterDoctrineCatalog(report);

            var raidDefinitions = new List<RaidDefinition>(_database.GetAll<RaidDefinition>() ?? Array.Empty<RaidDefinition>());
            RaidContentCatalog.Apply(raidDefinitions, _database.GetAll<EnemyDefinition>());
            int raidRoomsRestored = 0;
            foreach (var raid in raidDefinitions) raidRoomsRestored += raid.Rooms?.Count ?? 0;
            report.loadedRecordsByCategory["raid_rooms_restored"] = raidRoomsRestored;

            ResolveRecipeItemIds(report);

            return report;
        }

        private void ResolveRecipeItemIds(DatabaseBuildReport report)
        {
            var items = _database.GetAll<ItemDefinition>();
            var recipes = new List<RecipeDefinition>(_database.GetAll<RecipeDefinition>());
            var resolver = new CanonicalItemIdResolver(items);
            int outputResolved = 0;
            int ingredientResolved = 0;
            int invalidRecipes = 0;

            report.loadedRecordsByCategory["recipes_before_item_id_resolution"] = recipes.Count;

            foreach (var recipe in recipes)
            {
                bool valid = true;

                if (!resolver.TryResolve(recipe.OutputItemId, out var canonicalOutput, out var outputFailure))
                {
                    valid = false;
                    string message =
                        $"Recipe '{recipe.id}' has unresolved output id '{recipe.OutputItemId}': {outputFailure}.";
                    report.errors.Add(message);
                    UnityEngine.Debug.LogError($"[DatabaseBuilder] {message}");
                }
                else
                {
                    if (!string.Equals(recipe.OutputItemId, canonicalOutput, StringComparison.Ordinal))
                    {
                        recipe.OutputItemId = canonicalOutput;
                    }
                    outputResolved++;
                }

                if (recipe.Ingredients != null)
                {
                    foreach (var ingredient in recipe.Ingredients)
                    {
                        if (ingredient == null) continue;

                        if (!resolver.TryResolve(ingredient.ItemId, out var canonicalIngredient, out var ingredientFailure))
                        {
                            valid = false;
                            string message =
                                $"Recipe '{recipe.id}' has unresolved ingredient id '{ingredient.ItemId}': {ingredientFailure}.";
                            report.errors.Add(message);
                            UnityEngine.Debug.LogError($"[DatabaseBuilder] {message}");
                            continue;
                        }

                        if (!string.Equals(ingredient.ItemId, canonicalIngredient, StringComparison.Ordinal))
                            ingredient.ItemId = canonicalIngredient;
                        ingredientResolved++;
                    }
                }

                if (!valid)
                {
                    invalidRecipes++;
                    recipe.OutputItemId = null;
                }
            }

            // Re-register the same recipe objects after canonicalization. Invalid records are
            // excluded from the runtime registry so UI/services cannot execute them.
            recipes.RemoveAll(recipe => string.IsNullOrEmpty(recipe.OutputItemId));
            _database.RegisterCollection(recipes);

            report.loadedRecordsByCategory["recipes"] = recipes.Count;
            report.loadedRecordsByCategory["recipe_outputs_resolved"] = outputResolved;
            report.loadedRecordsByCategory["recipe_ingredients_resolved"] = ingredientResolved;
            report.loadedRecordsByCategory["recipe_invalid_removed"] = invalidRecipes;
        }

        private void RegisterDoctrineCatalog(DatabaseBuildReport report)
        {
            try
            {
                var doctrines = DoctrineCatalog.BuildDefinitions();
                _database.RegisterCollection(doctrines);
                report.loadedFiles++;
            }
            catch (Exception ex)
            {
                report.errors.Add($"Failed to register DoctrineCatalog: {ex.Message}");
            }
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
                // Phase 1 audit finding: the DecodeConverter's regex parser walks the whole
                // "entities/adventurers" package tree, so adventurers.json also picked up 13
                // non-hero records that merely live under that Java package: the abstract
                // Adventurer base class itself, PotionsDrank, and the 11 Doctrine classes
                // (Doctrine + 8 DoctrineOf* + DoctrineAbility + EmptyDoctrine). Every real
                // playable hero class extends Adventurer directly, so parentClass == "Adventurer"
                // is an exact, verified filter — 129 raw records -> 116 real hero classes
                // (confirmed against the 116 *.java files under
                // storage/data/entities/adventurers/units in the Legacy source).
                list.RemoveAll(raw => ((AdventurerDefinition)(object)raw).parentClass != "Adventurer");

                foreach (var raw in list)
                {
                    var def = (AdventurerDefinition)(object)raw;
                    EnrichAdventurerDefinition(def);
                }
            }

            // ── Quest metadata enrichment ────────────────────────────────────────────
            // quests.json (manifest-driven "quests" category) only carries id/className.
            // quest_metadata.json has the real defaultRarity/targetProgressValues but isn't
            // listed in manifest.json, so it's read directly here. See phase0_schema_mapping.md §8.
            if (typeof(T) == typeof(QuestDefinition))
            {
                var quests = new List<QuestDefinition>(list.Count);
                foreach (var item in list) quests.Add((QuestDefinition)(object)item);

                int withMetadata = QuestMetadataLoader.Apply(_dataProvider, quests);
                report.loadedRecordsByCategory["quest_metadata"] = withMetadata;
                EnrichQuestPoolMembership(quests);
            }

            if (typeof(T) == typeof(PetDefinition))
            {
                var pets = new List<PetDefinition>(list.Count);
                foreach (var item in list) pets.Add((PetDefinition)(object)item);
                EnrichPetDefinitions(pets);
            }

            _database.RegisterCollection(list);
            report.loadedRecordsByCategory[fileEntry.category] = list.Count;
        }

        /// <summary>
        /// Restores QuestsManager's nine static lists. The decoded JSON contains only the
        /// record identity, so this mapping is deliberately kept at the data boundary and is
        /// transcribed from QuestsManager.setupAccessibleQuests().
        /// </summary>
        private static void EnrichQuestPoolMembership(List<QuestDefinition> quests)
        {
            var doctrinePools = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "affliction", new[] { "vampiric_thirst", "falling_apart", "the_end", "innocence", "soft_and_fluffy", "tormentor", "delirious" } },
                { "control", new[] { "smoking_hot", "shocking", "slow_burn", "ice_breaker", "regicide", "crystal_clear", "laroxian_power" } },
                { "fortitude", new[] { "heavy_armor", "spiky", "protector", "speedy_hare", "clash_of_titans", "unscathed", "god_feared" } },
                { "grace", new[] { "medic", "light_bringer", "soothing_remedy", "psychiatrist", "and_stay_dead", "miracle", "darkness_within" } },
                { "illusion", new[] { "hit_or_miss", "lucky_roll", "its_a_trap", "nice_try", "eldritch_horror", "active_deterrent", "marathon" } },
                { "knowledge", new[] { "student", "myopia", "paleontologist", "master_crafter", "from_hell", "fast_learner", "exorcism" } },
                { "ruin", new[] { "annihilator", "smart_fighter", "critical_hit", "coup_d_etat", "botched_ritual", "pulverization", "thalassophobia" } },
                { "war", new[] { "expert_duelist", "warrior", "long_march", "conqueror", "endless_agony", "tabula_rasa", "raging_volcano" } }
            };

            var poolById = new Dictionary<string, (string Pool, string Doctrine)>(StringComparer.OrdinalIgnoreCase);
            foreach (var pool in doctrinePools)
                foreach (var id in pool.Value)
                    poolById[id] = ("Doctrine", pool.Key);

            foreach (var quest in quests)
            {
                if (quest == null) continue;
                if (poolById.TryGetValue(quest.id ?? string.Empty, out var membership))
                {
                    quest.PoolType = membership.Pool;
                    quest.DoctrineId = membership.Doctrine;
                }
                else
                {
                    // QuestsManager.accessibleQuests is the Kings/general pool. This also
                    // covers the six decoded records that are not in a doctrine list.
                    quest.PoolType = "Kings";
                    quest.DoctrineId = string.Empty;
                }
            }
        }

        /// <summary>Maps the 21 Java pet classes to family, tier and guaranteed ability.</summary>
        private static void EnrichPetDefinitions(List<PetDefinition> pets)
        {
            var families = new Dictionary<string, (string Family, int Tier, string Ability)>(StringComparer.OrdinalIgnoreCase)
            {
                { "dove", ("avian", 1, "DECOY") }, { "owl", ("avian", 2, "DECOY") }, { "eagle", ("avian", 3, "DECOY") },
                { "rockling", ("construct", 1, "MAGIC") }, { "golem", ("construct", 2, "MAGIC") }, { "tesseract", ("construct", 3, "MAGIC") },
                { "floating_eye", ("esoteric", 1, "EXPERIENCE") }, { "tentacle_tangle", ("esoteric", 2, "EXPERIENCE") }, { "thing_from_the_abyss", ("esoteric", 3, "EXPERIENCE") },
                { "mosquito", ("insect", 1, "FIGHTER") }, { "beetle", ("insect", 2, "FIGHTER") }, { "tarantula", ("insect", 3, "FIGHTER") },
                { "lizard", ("reptile", 1, "SAVAGE") }, { "tree_frog", ("reptile", 2, "SAVAGE") }, { "crocodile", ("reptile", 3, "SAVAGE") },
                { "rat", ("wild", 1, "LIFESTEAL") }, { "squirrel", ("wild", 2, "LIFESTEAL") }, { "red_wolf", ("wild", 3, "LIFESTEAL") },
                { "floating_seed", ("wooden", 1, "REGENERATION") }, { "walking_bush", ("wooden", 2, "REGENERATION") }, { "holy_tree", ("wooden", 3, "REGENERATION") }
            };
            foreach (var pet in pets)
            {
                if (pet == null || !families.TryGetValue(pet.id ?? string.Empty, out var data)) continue;
                pet.PetFamily = data.Family;
                pet.PetTier = data.Tier;
                pet.IdName = "pet_" + pet.id + "_name";
                pet.IdImage = "pet_" + pet.id;
                pet.GuaranteedFirstAbility = data.Ability;
                pet.AbilityNumber = data.Tier + 1;
            }
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
