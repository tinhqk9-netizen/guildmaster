using System;
using System.Collections.Generic;

namespace GuildMaster.Runtime.Services
{
    public interface IPartyService
    {
        int MaxPartySize { get; }
        int PartyCount { get; }

        /// <summary>Index of the party currently being viewed/edited in UI (0, 1, or 2).</summary>
        int ActivePartyIndex { get; }
        void SetActivePartyIndex(int index);

        // ── Multi-party API ────────────────────────────────────────────
        IReadOnlyList<string> GetPartyMembers(int partyIndex);
        bool AddToParty(string characterId, int partyIndex);
        bool RemoveFromParty(string characterId, int partyIndex);
        void ClearParty(int partyIndex);

        /// <summary>Checks if a character is in any of the 3 parties.</summary>
        bool IsInAnyParty(string characterId);

        /// <summary>Returns the party index (0, 1, 2) containing the character, or -1.</summary>
        int GetPartyIndexOf(string characterId);

        // ── Legacy single-party API (operates on ActivePartyIndex) ──
        IReadOnlyList<string> GetPartyMembers();
        bool AddToParty(string characterId);
        bool RemoveFromParty(string characterId);
        void ClearParty();
        bool IsInParty(string characterId);

        event Action OnPartyChanged;
    }
}
