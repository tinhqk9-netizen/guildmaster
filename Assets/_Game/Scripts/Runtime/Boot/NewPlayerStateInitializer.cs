using System;
using GuildMaster.Runtime.Services;

namespace GuildMaster.Runtime.Boot
{
    /// <summary>
    /// Production first-run initialization. It runs only when SaveService reports a genuinely
    /// missing save and uses the existing Tavern -> Character -> Equipment ownership pipeline.
    /// </summary>
    public static class NewPlayerStateInitializer
    {
        public static bool TryInitialize(ServiceContainer services, out string error)
        {
            error = null;
            if (services == null || services.Save?.CurrentData == null)
            {
                error = "Service container or fresh SaveData is null.";
                return false;
            }

            var data = services.Save.CurrentData;
            if (data.Characters.Count != 0 || data.TavernGuests.Count != 0)
            {
                error = "Fresh-save initializer received a non-empty character or Tavern state.";
                return false;
            }

            data.Money = 100;

            if (!services.Tavern.CreateInitialStartingHero(out var hero) || hero == null)
            {
                error = "Could not create the starting Footman through Tavern recruitment.";
                return false;
            }

            if (!string.Equals(hero.Definition?.id, "footman", StringComparison.OrdinalIgnoreCase))
            {
                error = $"Starting hero pipeline returned '{hero.Definition?.id ?? "null"}' instead of Footman.";
                return false;
            }

            if (hero.Weapon == null || string.IsNullOrEmpty(hero.Weapon.InstanceId))
            {
                error = "Starting Footman has no equipped starter weapon.";
                return false;
            }

            if (!services.Party.AddToParty(hero.InstanceId))
            {
                error = "Could not add the starting Footman to the first expedition party.";
                return false;
            }

            services.Tavern.GenerateInitialVisitor();
            if (data.TavernGuests.Count != 1 ||
                string.Equals(data.TavernGuests[0]?.DefinitionId, "footman", StringComparison.OrdinalIgnoreCase))
            {
                error = "Initial Tavern visitor is missing or is Footman.";
                return false;
            }

            data.Money = 100;
            data.NextTavernVisit = services.Tavern.GetVisitorIntervalSeconds();

            if (!services.Save.Save(out var saveError))
            {
                error = $"Could not persist fresh player state: {saveError?.Message ?? "unknown save error"}";
                return false;
            }

            return true;
        }
    }
}
