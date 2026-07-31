using System;
using System.Collections.Generic;
using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public class PartyService : IPartyService
    {
        public int MaxPartySize => 4;
        public int PartyCount => 3;

        public event Action OnPartyChanged;

        private readonly ISaveService _saveService;
        private int _activePartyIndex;

        public int ActivePartyIndex => _activePartyIndex;

        public PartyService(ISaveService saveService)
        {
            _saveService = saveService ?? throw new ArgumentNullException(nameof(saveService));
            _activePartyIndex = 0;
        }

        public void SetActivePartyIndex(int index)
        {
            _activePartyIndex = Math.Max(0, Math.Min(index, PartyCount - 1));
        }

        // ── Core multi-party helpers ────────────────────────────────────

        private List<List<string>> AllParties
        {
            get
            {
                var data = _saveService.CurrentData;
                if (data == null) return new List<List<string>> { new(), new(), new() };
                if (data.ExpeditionParties == null)
                    data.ExpeditionParties = new List<List<string>>();
                while (data.ExpeditionParties.Count < PartyCount)
                    data.ExpeditionParties.Add(new List<string>());
                return data.ExpeditionParties;
            }
        }

        private List<string> GetPartyList(int partyIndex)
        {
            int idx = Math.Max(0, Math.Min(partyIndex, PartyCount - 1));
            return AllParties[idx];
        }

        // ── Multi-party API ─────────────────────────────────────────────

        public IReadOnlyList<string> GetPartyMembers(int partyIndex)
        {
            return GetPartyList(partyIndex).AsReadOnly();
        }

        public bool AddToParty(string characterId, int partyIndex)
        {
            if (string.IsNullOrEmpty(characterId)) return false;

            // Exclusivity: character can only be in one party
            if (IsInAnyParty(characterId)) return false;

            var party = GetPartyList(partyIndex);
            if (party.Count >= MaxPartySize) return false;

            party.Add(characterId);
            SyncLegacyParty();
            OnPartyChanged?.Invoke();
            return true;
        }

        public bool RemoveFromParty(string characterId, int partyIndex)
        {
            var party = GetPartyList(partyIndex);
            if (party.Remove(characterId))
            {
                SyncLegacyParty();
                OnPartyChanged?.Invoke();
                return true;
            }
            return false;
        }

        public void ClearParty(int partyIndex)
        {
            var party = GetPartyList(partyIndex);
            if (party.Count > 0)
            {
                party.Clear();
                SyncLegacyParty();
                OnPartyChanged?.Invoke();
            }
        }

        public bool IsInAnyParty(string characterId)
        {
            return GetPartyIndexOf(characterId) >= 0;
        }

        public int GetPartyIndexOf(string characterId)
        {
            var parties = AllParties;
            for (int i = 0; i < parties.Count; i++)
            {
                if (parties[i].Contains(characterId))
                    return i;
            }
            return -1;
        }

        // ── Legacy single-party API (delegates to ActivePartyIndex) ──

        public IReadOnlyList<string> GetPartyMembers()
        {
            return GetPartyMembers(_activePartyIndex);
        }

        public bool AddToParty(string characterId)
        {
            return AddToParty(characterId, _activePartyIndex);
        }

        public bool RemoveFromParty(string characterId)
        {
            return RemoveFromParty(characterId, _activePartyIndex);
        }

        public void ClearParty()
        {
            ClearParty(_activePartyIndex);
        }

        public bool IsInParty(string characterId)
        {
            return GetPartyList(_activePartyIndex).Contains(characterId);
        }

        // ── Sync legacy CurrentParty field for backward compatibility ──

        private void SyncLegacyParty()
        {
            var data = _saveService.CurrentData;
            if (data == null) return;
            data.CurrentParty = new List<string>(GetPartyList(0));
        }
    }
}
