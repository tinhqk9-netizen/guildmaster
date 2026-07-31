using System.Collections.Generic;
using GuildMaster.Runtime.Models;

namespace GuildMaster.Runtime.Services
{
    public interface IDungeonService
    {
        void StartDungeon(string dungeonId, List<string> adventurerIds);
        void StartDungeon(string dungeonId);
        void StopDungeon();
        void SaveDungeonState();
        void LoadDungeonState();
        bool IsDungeonActive();
        void AdvanceProgressOneStep();
        void Tick();
        DungeonRuntime GetActiveDungeon();

        /// <summary>
        /// Moves everything in the area chest into the player's storage, following
        /// <c>Utils.collectDrops</c>. Enemies never drop straight into the inventory — the run
        /// fills a temporary chest first and this is the only way out of it.
        /// </summary>
        /// <returns>Number of item stacks transferred.</returns>
        int CollectDrops();

        // ─── Multi-Expedition API ──────────────────────────────────────────
        int MaxExpeditions { get; }
        bool StartExpedition(int slotIndex, string dungeonId, List<string> adventurerIds, out string error);
        void StopExpedition(int slotIndex);
        ExpeditionRuntime GetExpedition(int slotIndex);
        IReadOnlyList<ExpeditionRuntime> GetAllExpeditions();
        void TickAll();

        /// <summary>
        /// Advances every expedition slot by <paramref name="seconds"/> in-memory ticks and
        /// persists exactly once at the end (instead of once per tick). Used for offline
        /// catch-up to avoid O(seconds) disk saves.
        /// </summary>
        void FastForward(long seconds);
        int CollectDrops(int slotIndex);
        bool IsCharacterOnExpedition(string characterInstanceId);
        bool IsDungeonUnlocked(string dungeonId);
    }
}
