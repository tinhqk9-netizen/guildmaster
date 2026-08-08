using System.Linq;
using NUnit.Framework;
using GuildMaster.Database;
using GuildMaster.Definitions;
using GuildMaster.Infrastructure.DataProviders;
using GuildMaster.Infrastructure.Serialization;
using GuildMaster.Runtime.Models;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using GuildMaster.Runtime.UI.Headquarters;
using GuildMaster.Runtime.UI.Legacy;
using UnityEngine;

namespace GuildMaster.Tests.EditMode
{
    [TestFixture]
    public sealed class Phase4_ContentRestorationTests
    {
        private GameDatabase _database;
        private ServiceContainer _services;

        [SetUp]
        public void SetUp()
        {
            _database = new GameDatabase();
            new DatabaseBuilder(new EditorExternalGameDataProvider(), new UnityJsonSerializer(), _database).Build();
            _services = new ServiceContainer(_database);
            for (int i = 0; i < _services.Dungeon.MaxExpeditions; i++)
                _services.Dungeon.StopExpedition(i);
        }

        [Test]
        public void ContentCatalog_LoadsLegacyCountsAndMappings()
        {
            Assert.AreEqual(21, _database.GetAll<PetDefinition>().Count);
            Assert.AreEqual(12, _database.GetAll<RaidDefinition>().Count);
            Assert.AreEqual(56, _database.GetAll<QuestDefinition>().Count);
            Assert.IsTrue(_database.GetAll<PetDefinition>().All(p => !string.IsNullOrEmpty(p.PetFamily) && p.PetTier > 0));
            Assert.IsTrue(_database.GetAll<RaidDefinition>().All(r => r.Rooms != null && r.Rooms.Count > 0));
            Assert.IsTrue(_database.GetAll<QuestDefinition>().All(q => !string.IsNullOrEmpty(q.PoolType)));
        }

        [Test]
        public void Pet_HatchAssignFeedAndUnassign_UsesRealInventoryAndDungeonState()
        {
            var egg = _database.GetAll<ItemDefinition>().First(i => i.id == "avian_egg");
            var food = _database.GetAll<ItemDefinition>().First(i => i.parentClass == "Food" && i.FeedPower > 0);
            int foodBefore = _services.Inventory.GetQuantityByDefinitionId(food.id);
            _services.Inventory.AddItem(new ItemRuntime("phase4_egg", egg, 1));
            _services.Inventory.AddItem(new ItemRuntime("phase4_food", food, 1));

            var pet = _services.Pet.HatchEgg(egg.id);
            Assert.IsNotNull(pet);
            Assert.AreEqual(0, _services.Inventory.GetQuantityByDefinitionId(egg.id));

            var dungeon = _database.GetAll<DungeonDefinition>().First(d => _services.Dungeon.IsDungeonUnlocked(d.id));
            Assert.IsTrue(_services.Pet.AssignToDungeon(pet.InstanceId, dungeon.id));
            Assert.IsTrue(_services.Pet.GetDungeonPets(dungeon.id).Any(p => p.InstanceId == pet.InstanceId));

            Assert.IsTrue(_services.Pet.FeedWithItem(pet.InstanceId, "phase4_food", 1));
            Assert.AreEqual(foodBefore, _services.Inventory.GetQuantityByDefinitionId(food.id));
            Assert.IsTrue(_services.Pet.UnassignFromDungeon(pet.InstanceId));
            Assert.IsFalse(_services.Pet.GetDungeonPets(dungeon.id).Any(p => p.InstanceId == pet.InstanceId));
        }

        [Test]
        public void Pet_FeedAndReleasePersistsThroughRealServices()
        {
            var food = _database.GetAll<ItemDefinition>().First(i => i.parentClass == "Food" && i.FeedPower > 0);
            var pet = _services.Pet.CreatePet("dove");
            Assert.IsNotNull(pet);

            int foodBefore = _services.Inventory.GetQuantityByDefinitionId(food.id);
            int petFoodBefore = pet.Food;
            _services.Inventory.AddItem(new ItemRuntime("phase4_release_food", food, 1));
            Assert.Greater(_services.Pet.GetFoodToNextLevel(pet.InstanceId), 0);
            Assert.IsTrue(_services.Pet.FeedWithItem(pet.InstanceId, "phase4_release_food", 1));
            Assert.AreEqual(foodBefore, _services.Inventory.GetQuantityByDefinitionId(food.id));
            Assert.GreaterOrEqual(pet.Food, 0);
            Assert.IsTrue(pet.Food != petFoodBefore || pet.Level > 1);

            Assert.IsTrue(_services.Pet.ReleasePet(pet.InstanceId));
            Assert.IsFalse(_services.Pet.GetAllPets().Any(saved => saved.InstanceId == pet.InstanceId));

            var reloaded = new SaveService();
            Assert.IsTrue(reloaded.Load(out var error), error?.Message);
            Assert.IsFalse(reloaded.CurrentData.Pets.Any(saved => saved.InstanceId == pet.InstanceId));
        }

        [Test]
        public void PetDefinitions_ResolveLegacyPortraitSprites()
        {
            LegacySpriteRegistry.ClearCache();

            foreach (var pet in _database.GetAll<PetDefinition>())
            {
                Assert.AreEqual("pet_" + pet.id, pet.IdImage, pet.id + " must expose its legacy image key.");
                Assert.IsNotNull(LegacySpriteRegistry.GetPetSprite(pet.IdImage),
                    pet.id + " must resolve a sprite through the pet portrait pipeline.");
            }
        }

        [Test]
        public void PetEggDefinitions_ResolveShelterSpritesThroughGenericResolver()
        {
            LegacySpriteRegistry.ClearCache();
            var eggs = _database.GetAll<ItemDefinition>()
                .Where(item => item != null && item.id.EndsWith("_egg", System.StringComparison.OrdinalIgnoreCase) &&
                               !string.Equals(item.id, "frozen_egg", System.StringComparison.OrdinalIgnoreCase))
                .ToList();

            Assert.Greater(eggs.Count, 0, "No hatchable pet egg definitions were loaded.");
            foreach (var egg in eggs)
            {
                Assert.IsNotNull(ShelterDialog.ResolveEggSprite(egg),
                    $"Shelter could not resolve Image.sprite for egg {egg.id} (ImageId={egg.IdImage}).");

                // Also verify the resolver's canonical-id fallback when an item boundary has no
                // populated ImageId: avian_egg -> egg_avian, etc.
                var canonicalOnly = new ItemDefinition { id = egg.id };
                Assert.IsNotNull(ShelterDialog.ResolveEggSprite(canonicalOnly),
                    $"Canonical egg id fallback did not resolve for {egg.id}.");
            }
        }

        [Test]
        public void Pet_SaveLoadKeepsDefinitionAndPortraitResolution()
        {
            var pet = _services.Pet.CreatePet("dove");
            Assert.IsNotNull(pet);

            var loaded = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(_services.Save.CurrentData));
            loaded.NormalizeAfterLoad();
            var persisted = loaded.Pets.Single(saved => saved.InstanceId == pet.InstanceId);

            Assert.AreEqual(pet.DefinitionId, persisted.DefinitionId);
            Assert.AreEqual(pet.Level, persisted.Level);
            Assert.IsTrue(_database.TryGet<PetDefinition>(persisted.DefinitionId, out var definition));
            Assert.IsNotNull(LegacySpriteRegistry.GetPetSprite(definition.IdImage));
        }

        [Test]
        public void Pet_ExpeditionCompanion_SavesReloadsAndUsesSelectedPet()
        {
            var character = _services.Character.CreateCharacter(_database.GetAll<AdventurerDefinition>().First().id);
            var pet = _services.Pet.CreatePet("dove");
            Assert.IsNotNull(character);
            Assert.IsNotNull(pet);

            // This test exercises the real bonus API without inventing a new pet mechanic.
            pet.Ability1 = "EXPERIENCE";
            var dungeon = _database.GetAll<DungeonDefinition>().First(d => _services.Dungeon.IsDungeonUnlocked(d.id));
            Assert.IsTrue(_services.Dungeon.StartExpedition(0, dungeon.id,
                new System.Collections.Generic.List<string> { character.InstanceId }, pet.InstanceId, out var error), error);
            Assert.AreEqual(pet.InstanceId, _services.Dungeon.GetExpedition(0).Dungeon.PetInstanceId);
            Assert.Greater(_services.Pet.GetExperienceBonus(pet.InstanceId), 0d);

            _services.Dungeon.SaveDungeonState();
            var roundTrip = JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(_services.Save.CurrentData));
            roundTrip.NormalizeAfterLoad();
            Assert.AreEqual(pet.InstanceId, roundTrip.ActiveExpeditions.First(e => e.SlotIndex == 0).Dungeon.PetInstanceId);

            _services.Dungeon.LoadDungeonState();
            Assert.AreEqual(pet.InstanceId, _services.Dungeon.GetExpedition(0).Dungeon.PetInstanceId);
        }

        [Test]
        public void Pet_ExpeditionWithoutCompanion_HasNoPetBonus()
        {
            var character = _services.Character.CreateCharacter(_database.GetAll<AdventurerDefinition>().First().id);
            var dungeon = _database.GetAll<DungeonDefinition>().First(d => _services.Dungeon.IsDungeonUnlocked(d.id));

            Assert.IsTrue(_services.Dungeon.StartExpedition(0, dungeon.id,
                new System.Collections.Generic.List<string> { character.InstanceId }, null, out var error), error);
            Assert.IsNull(_services.Dungeon.GetExpedition(0).Dungeon.PetInstanceId);
        }

        [Test]
        public void Pet_RemoveFromExpedition_DisablesCompanionHook()
        {
            var character = _services.Character.CreateCharacter(_database.GetAll<AdventurerDefinition>().First().id);
            var pet = _services.Pet.CreatePet("dove");
            var dungeon = _database.GetAll<DungeonDefinition>().First(d => _services.Dungeon.IsDungeonUnlocked(d.id));

            Assert.IsTrue(_services.Dungeon.StartExpedition(0, dungeon.id,
                new System.Collections.Generic.List<string> { character.InstanceId }, pet.InstanceId, out var error), error);
            _services.Dungeon.StopExpedition(0);
            Assert.IsTrue(_services.Dungeon.StartExpedition(0, dungeon.id,
                new System.Collections.Generic.List<string> { character.InstanceId }, null, out error), error);
            Assert.IsNull(_services.Dungeon.GetExpedition(0).Dungeon.PetInstanceId);
        }

        [Test]
        public void Bestiary_MarksOnlyKnownEnemiesAsSeen()
        {
            var enemy = _database.GetAll<EnemyDefinition>().First();
            _services.Bestiary.MarkSeen(enemy.id);
            Assert.IsTrue(_services.Bestiary.IsSeen(enemy.id));
            Assert.IsTrue(_services.Bestiary.GetSeenEnemyIds().Contains(enemy.id));
            _services.Bestiary.MarkSeen("phase4_unknown_enemy");
            Assert.IsFalse(_services.Bestiary.IsSeen("phase4_unknown_enemy"));
        }

        [Test]
        public void Raid_StartsFromCurrentPartyAndExposesRoomsAndRewards()
        {
            var adventurer = _database.GetAll<AdventurerDefinition>().First();
            var character = _services.Character.CreateCharacter(adventurer.id);
            _services.Save.CurrentData.CurrentParty.Add(character.InstanceId);
            var raid = _database.GetAll<RaidDefinition>().First(r => _services.Raid.IsUnlocked(r));

            Assert.IsTrue(_services.Raid.StartRaid(raid.id, null, out var error), error);
            Assert.IsNotNull(_services.Raid.ActiveRaid);
            Assert.AreEqual(raid.id, _services.Raid.ActiveRaid.Definition.id);
            Assert.Greater(raid.Rooms.Count, 0);
            _services.Raid.AbandonRaid();
        }
    }
}
