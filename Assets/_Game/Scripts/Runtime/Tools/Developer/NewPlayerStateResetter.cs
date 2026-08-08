#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GuildMaster.Database;
using GuildMaster.Runtime.Save;
using GuildMaster.Runtime.Services;
using UnityEngine;
using SystemRandom = System.Random;

namespace GuildMaster.Tools.Developer
{
    /// <summary>
    /// Editor-only reset pipeline for a clean onboarding test state. It resets the existing
    /// SaveData instance from SaveData.CreateDefault(), then uses TavernService and
    /// CharacterService to create the hero and first visitor with normal ownership flow.
    /// </summary>
    public static class NewPlayerStateResetter
    {
        public const string StartingHeroId = "footman";

        public static ServiceContainer ResetToNewPlayerState(
            GameDatabase database,
            ISaveService saveService,
            SystemRandom random = null)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (saveService?.CurrentData == null) throw new ArgumentNullException(nameof(saveService));

            ResetSaveDataInPlace(saveService.CurrentData);
            random ??= new SystemRandom();

            var services = new ServiceContainer(database, saveService);
            if (!(services.Tavern is TavernService tavern))
                throw new InvalidOperationException("ServiceContainer did not provide TavernService.");

            // Generate and recruit the starting hero through the same visitor pipeline used by
            // the real Tavern, so the starter weapon gets one stable equipped ownership record.
            tavern.GenerateVisitorForDeveloper(StartingHeroId);
            if (!tavern.RecruitGuest(0, out var hero) || hero == null ||
                !string.Equals(hero.Definition?.id, StartingHeroId, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Could not create the starting Footman through Tavern recruitment.");

            if (!services.Party.AddToParty(hero.InstanceId))
                throw new InvalidOperationException("Could not place the starting Footman in the first party.");

            var visitorPool = tavern.GetDeveloperVisitorClassPool()
                .Where(id => !string.Equals(id, StartingHeroId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (visitorPool.Count == 0)
                throw new InvalidOperationException("The normal Tavern class pool has no non-Footman visitor.");

            // Equal probability across every normal visitor class except Footman.
            string visitorClassId = visitorPool[random.Next(visitorPool.Count)];
            tavern.GenerateVisitorForDeveloper(visitorClassId);

            saveService.CurrentData.Money = 100;
            saveService.CurrentData.TutorialStep = 0;
            saveService.CurrentData.NextTavernVisit = services.Tavern.GetVisitorIntervalSeconds();

            if (!saveService.Save(out var error))
                throw new InvalidOperationException($"New player state could not be saved: {error?.Message ?? "unknown error"}");

            return services;
        }

        private static void ResetSaveDataInPlace(SaveData data)
        {
            // Use the canonical default object and Unity's serializer so every current and
            // future serialized field resets without hand-maintaining a partial field list.
            var defaults = SaveData.CreateDefault();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(defaults), data);
            foreach (var field in typeof(SaveData).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (typeof(IList).IsAssignableFrom(field.FieldType) && field.GetValue(data) is IList list)
                    list.Clear();
            }
            // Unity's JSON serializer omits null fields, so explicitly clear nullable active
            // runtime state that a dirty save may already hold.
            data.ActiveDungeon = null;
            data.ActiveRaid = null;
            data.NormalizeAfterLoad();
            data.ActiveExpeditions.Clear();
            data.Money = 100;
            data.TutorialStep = 0;
        }
    }
}
#endif
