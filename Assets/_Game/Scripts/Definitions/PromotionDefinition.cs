using System;

namespace GuildMaster.Definitions
{
    /// <summary>
    /// Phase 1 audit finding: Java has NO data table for promotion at all (no promotions.json
    /// ever existed in this project either — GetAll&lt;PromotionDefinition&gt;() always returned an
    /// empty list). A "promotion" in Legacy is just picking one of the current
    /// AdventurerDefinition's own <see cref="AdventurerDefinition.NextClasses"/> once
    /// <see cref="AdventurerDefinition.MaxLevel"/> is reached (DialogEntityDetail.
    /// dialogAdventurerPromotion/promote/ascend, DialogPromotionChoices.java) — no item is
    /// consumed and no separate "tier" record exists. RequiredItemId/RequiredItemCount/
    /// StatMultiplier/TierName/TierIndex (removed here) were fabricated; see
    /// Docs/Backend_Audit/phase1_audit_report.md. This class is kept as an unused, empty shell
    /// (rather than deleted) purely to avoid touching anything that might still reference the
    /// type name; PromotionService.cs no longer reads from it.
    /// </summary>
    [Serializable]
    public class PromotionDefinition : DefinitionBase
    {
    }
}
