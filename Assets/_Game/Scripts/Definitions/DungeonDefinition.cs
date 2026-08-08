using System;

using System.Collections.Generic;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class MerchantOfferData
    {
        public string ItemId;
        public int StackCount;
        public int Weight;
    }

    /// <summary>
    /// One weighted enemy group restored from Java's <c>rollEnemies()</c> (e.g.
    /// "1 Wurm + 1 SandVulture, weight 100"). Groups are loaded from dungeon data and consumed
    /// by <see cref="GuildMaster.Runtime.Services.DungeonService"/>.
    /// </summary>
    [Serializable]
    public class EncounterGroupData
    {
        public List<string> EnemyIds;
        public double Weight;
    }

    [Serializable]
    public class DungeonDefinition : DefinitionBase
    {
        public List<MerchantOfferData> RegularMerchantOffers;
        public List<MerchantOfferData> SpecialMerchantOffers;

        /// <summary>
        /// Enemies this area can spawn, recovered from the area's <c>listEnemies()</c> override
        /// in the APK. Ids match <c>enemies.json</c>. This is a FLATTENED view (no weights, no
        /// grouping, no empty-room chance) — kept for backward compatibility with existing
        /// readers. Prefer <see cref="EncounterGroups"/> once populated.
        ///
        /// Evidence: Reports/S6_5A/S6_5A_Claude_DungeonCombatLoot_Audit_Report.md
        /// </summary>
        public List<string> EnemyIds;

        /// <summary>
        /// Java's real <c>rollEnemies()</c> weighted table: 0-6 monster groups per encounter,
        /// plus <see cref="EmptyRoomWeight"/> for "no enemies this room". A missing table remains
        /// supported only as a legacy flat-pool fallback for unconverted data.
        /// </summary>
        public List<EncounterGroupData> EncounterGroups;

        /// <summary>Weight (same scale as EncounterGroups.Weight) for rolling an empty room.</summary>
        public double EmptyRoomWeight;

        /// <summary>
        /// Material rewards from <c>searchRoom()</c> — rolled on the empty-room path (Java's
        /// SEARCH_ROOM action) on a 0-1000 scale, same convention as <see cref="EncounterGroups"/>.
        /// Hand-extracted per area from the decompiled Java (see
        /// Docs/Backend_Audit/dungeon_encounter_data_audit.md and phase2b_completion_report.md);
        /// weights are best-effort where a dungeon's searchRoom() nests its material chain behind
        /// an extra RNG gate (documented per-area in the completion report). searchRoom()'s trap /
        /// heal branches (status-effect application) are intentionally NOT modeled here — see
        /// "known limitations" in the Phase 2B report. Empty/null for areas whose searchRoom() has
        /// no deterministic material chain (e.g. trap-only or single rare-drop areas).
        /// </summary>
        public List<EnemyDropEntry> SearchRoomDrops;

        /// <summary>
        /// Dungeon chain gating (G05): the dungeon that must be cleared before this one unlocks.
        /// Null or empty = no requirement.
        /// </summary>
        public string RequiredClearDungeonId;
        public int RequiredClearProgress;

        /// <summary>
        /// Quest event category this dungeon completion feeds into.
        /// Maps to QuestDefinition.TrueClass for caller wiring.
        /// </summary>
        public string QuestEventCategory;

        public string SourceClass;
    }
}
