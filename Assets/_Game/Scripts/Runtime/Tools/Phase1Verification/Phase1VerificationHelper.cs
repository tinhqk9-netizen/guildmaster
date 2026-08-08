using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Runtime.Boot;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Character;

namespace GuildMaster.Tools.Phase1Verification
{
    /// <summary>
    /// Dev Verification Helper for Phase 1 Character Progression.
    /// Provides runtime test hero creation, double trait verification, promotion test setup,
    /// showcase hero generation, and clean data deletion without corrupting real save data.
    /// </summary>
    public static class Phase1VerificationHelper
    {
        public const string TestIdPrefix = "DEV_TEST_";

        public static ServiceContainer GetRuntimeServices()
        {
            if (!Application.isPlaying) return null;
            var boot = UnityEngine.Object.FindFirstObjectByType<UIRuntimeBootstrap>();
            return boot?.Services;
        }

        /// <summary>
        /// 1. [Spawn Basic Hero]
        /// Spawns a normal test hero using current pipeline (RecruitCharacter).
        /// Tagged with DEV_TEST_ prefix.
        /// </summary>
        public static CharacterSaveData SpawnBasicHero(ServiceContainer services = null)
        {
            services ??= GetRuntimeServices();
            if (services?.Character == null)
            {
                Debug.LogError("[Phase1Verification] Services or CharacterService not available. Ensure Game is running in Play Mode.");
                return null;
            }

            string instanceId = TestIdPrefix + "BASIC_" + Guid.NewGuid().ToString().Substring(0, 8);
            var saveData = new CharacterSaveData
            {
                InstanceId = instanceId,
                DefinitionId = "footman",
                Level = 1,
                Exp = 0,
                IsHpInitialized = false,
                TraitCommon = "BRUTE",
                TraitRare = string.Empty,
                Trait = "BRUTE"
            };

            var character = services.Character.RecruitCharacter(saveData);
            services.Save.Save(out _);
            RefreshUI(services, character);
            Debug.Log($"[Phase1Verification] Spawned Basic Hero: {character.Definition.id} (ID: {instanceId})");
            return saveData;
        }

        /// <summary>
        /// 2. [Spawn Double Trait Hero]
        /// Spawns a hero possessing BOTH TraitCommon and TraitRare.
        /// Tests SaveData persistence and Character Detail UI display.
        /// </summary>
        public static CharacterSaveData SpawnDoubleTraitHero(ServiceContainer services = null)
        {
            services ??= GetRuntimeServices();
            if (services?.Character == null)
            {
                Debug.LogError("[Phase1Verification] Services or CharacterService not available.");
                return null;
            }

            string instanceId = TestIdPrefix + "DOUBLE_" + Guid.NewGuid().ToString().Substring(0, 8);
            var saveData = new CharacterSaveData
            {
                InstanceId = instanceId,
                DefinitionId = "archer",
                Level = 5,
                Exp = 0,
                IsHpInitialized = false,
                TraitCommon = "FERAL",
                TraitRare = "DRAGON_BLOOD",
                Trait = "FERAL"
            };

            var character = services.Character.RecruitCharacter(saveData);
            services.Save.Save(out _);
            RefreshUI(services, character);
            Debug.Log($"[Phase1Verification] Spawned Double Trait Hero: {character.Definition.id} (Common: {saveData.TraitCommon}, Rare: {saveData.TraitRare})");
            return saveData;
        }

        /// <summary>
        /// 3. [Spawn Promotion Test Hero]
        /// Spawns an Apprentice at MaxLevel (Level 20) to make it eligible for Promotion.
        /// Enables inspecting NextClasses, promoting, and verifying class/skill changes.
        /// </summary>
        public static CharacterSaveData SpawnPromotionTestHero(ServiceContainer services = null)
        {
            services ??= GetRuntimeServices();
            if (services?.Character == null || services?.Database == null)
            {
                Debug.LogError("[Phase1Verification] Services or Database not available.");
                return null;
            }

            string defId = "apprentice";
            if (!services.Database.TryGet<AdventurerDefinition>(defId, out var def))
            {
                Debug.LogError($"[Phase1Verification] AdventurerDefinition '{defId}' not found.");
                return null;
            }

            string instanceId = TestIdPrefix + "PROMO_" + Guid.NewGuid().ToString().Substring(0, 8);
            var saveData = new CharacterSaveData
            {
                InstanceId = instanceId,
                DefinitionId = defId,
                Level = def.MaxLevel, // Level 20
                Exp = 0,
                IsHpInitialized = false,
                TraitCommon = "BOOKWORM",
                TraitRare = string.Empty,
                Trait = "BOOKWORM"
            };

            var character = services.Character.RecruitCharacter(saveData);
            services.Save.Save(out _);
            RefreshUI(services, character);

            if (services.Promotion != null)
            {
                var choices = services.Promotion.GetPromotionChoices(saveData);
                string choicesList = choices.Count > 0 ? string.Join(", ", choices.Select(c => c.id)) : "None";
                Debug.Log($"[Phase1Verification] Spawned Promotion Test Hero: {def.id} Lv.{def.MaxLevel}. Promotion choices available: {choices.Count} ({choicesList})");
            }

            return saveData;
        }

        /// <summary>
        /// 4. [Spawn Showcase Hero]
        /// Spawns a fully equipped Mage Lv.15 with Common & Rare Traits, Active/Passive Skills,
        /// and equipped Weapon, Armor, Accessory for inspecting the full Character Detail UI.
        /// </summary>
        public static CharacterSaveData SpawnShowcaseHero(ServiceContainer services = null)
        {
            services ??= GetRuntimeServices();
            if (services?.Character == null || services?.Database == null)
            {
                Debug.LogError("[Phase1Verification] Services or Database not available.");
                return null;
            }

            string defId = "mage";
            if (!services.Database.TryGet<AdventurerDefinition>(defId, out var def))
            {
                defId = "apprentice";
                services.Database.TryGet(defId, out def);
            }

            string instanceId = TestIdPrefix + "SHOWCASE_" + Guid.NewGuid().ToString().Substring(0, 8);
            var saveData = new CharacterSaveData
            {
                InstanceId = instanceId,
                DefinitionId = defId,
                Level = 15,
                Exp = 120,
                IsHpInitialized = false,
                TraitCommon = "BOOKWORM",
                TraitRare = "GIFTED",
                Trait = "BOOKWORM",
                IsAscended = false
            };

            // Equip items if available in Database & Inventory
            if (services.Inventory != null)
            {
                if (services.Database.TryGet<ItemDefinition>("woodenstaff", out var wDef) || services.Database.TryGet<ItemDefinition>("ironsword", out wDef))
                {
                    var weaponItem = new ItemRuntime(Guid.NewGuid().ToString(), wDef, 1) { IsLocked = true };
                    services.Inventory.AddItem(weaponItem);
                    saveData.WeaponInstanceId = weaponItem.InstanceId;
                }

                if (services.Database.TryGet<ItemDefinition>("clothrobe", out var aDef) || services.Database.TryGet<ItemDefinition>("ironarmor", out aDef))
                {
                    var armorItem = new ItemRuntime(Guid.NewGuid().ToString(), aDef, 1) { IsLocked = true };
                    services.Inventory.AddItem(armorItem);
                    saveData.ArmorInstanceId = armorItem.InstanceId;
                }

                if (services.Database.TryGet<ItemDefinition>("silverring", out var accDef))
                {
                    var accItem = new ItemRuntime(Guid.NewGuid().ToString(), accDef, 1) { IsLocked = true };
                    services.Inventory.AddItem(accItem);
                    saveData.AccessoryInstanceId = accItem.InstanceId;
                }
            }

            var character = services.Character.RecruitCharacter(saveData);
            services.Save.Save(out _);
            RefreshUI(services, character);
            Debug.Log($"[Phase1Verification] Spawned Showcase Hero: {defId} Lv.15 with Equipment & Traits (Common: BOOKWORM, Rare: GIFTED)");
            return saveData;
        }

        /// <summary>
        /// 5. [Clear Test Data]
        /// Deletes all test heroes created by this tool (InstanceId starting with DEV_TEST_).
        /// Safely unlocks equipped items and does not affect normal save data.
        /// </summary>
        public static int ClearTestData(ServiceContainer services = null)
        {
            services ??= GetRuntimeServices();
            if (services?.Save?.CurrentData == null || services?.Character == null)
            {
                Debug.LogWarning("[Phase1Verification] Cannot clear test data: Services not running.");
                return 0;
            }

            var allTestChars = services.Character.GetAllCharacters()
                .Where(c => c.InstanceId.StartsWith(TestIdPrefix))
                .ToList();

            int clearedCount = 0;
            foreach (var c in allTestChars)
            {
                if (services.Character.DismissCharacter(c.InstanceId, out _))
                {
                    clearedCount++;
                }
            }

            var saveData = services.Save.CurrentData;
            int removedFromSave = saveData.Characters.RemoveAll(c => c.InstanceId.StartsWith(TestIdPrefix));

            services.Save.Save(out _);
            RefreshUI(services, null);
            Debug.Log($"[Phase1Verification] Cleared {clearedCount} test characters ({removedFromSave} from SaveData). Normal save data untouched.");
            return clearedCount;
        }

        public static void RefreshUI(ServiceContainer services, CharacterRuntime selectCharacter)
        {
            var roster = UnityEngine.Object.FindFirstObjectByType<AdventurersTabController>();
            if (roster != null)
            {
                roster.Refresh();
            }

            if (selectCharacter != null)
            {
                var detailPanel = UnityEngine.Object.FindFirstObjectByType<CharacterDetailPanel>();
                if (detailPanel != null)
                {
                    detailPanel.Open(selectCharacter);
                }
            }
        }
    }
}
