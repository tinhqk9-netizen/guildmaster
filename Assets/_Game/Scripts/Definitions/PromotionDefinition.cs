using System;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class PromotionDefinition : DefinitionBase
    {
        /// <summary>Stat requirement to qualify for this promotion tier.</summary>
        public int RequiredLevel;
        /// <summary>Item ID consumed on promotion.</summary>
        public string RequiredItemId;
        /// <summary>How many of the required item are consumed.</summary>
        public int RequiredItemCount = 1;
        /// <summary>Stat multiplier applied to this character after promotion.</summary>
        public float StatMultiplier = 1.1f;
        /// <summary>Display name of this tier (e.g. "Bronze", "Silver", "Gold").</summary>
        public string TierName = "Promotion";
        /// <summary>Visual index for tier badge/sprites.</summary>
        public int TierIndex;
    }
}
