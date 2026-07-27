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

    [Serializable]
    public class DungeonDefinition : DefinitionBase
    {
        public List<MerchantOfferData> RegularMerchantOffers;
        public List<MerchantOfferData> SpecialMerchantOffers;

        /// <summary>
        /// Enemies this area can spawn, recovered from the area's <c>listEnemies()</c> override
        /// in the APK. Ids match <c>enemies.json</c>.
        ///
        /// Evidence: Reports/S6_5A/S6_5A_Claude_DungeonCombatLoot_Audit_Report.md
        /// </summary>
        public List<string> EnemyIds;

        public string SourceClass;
    }
}
