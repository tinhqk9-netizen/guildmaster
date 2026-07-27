using System;

using System.Collections.Generic;

namespace GuildMaster.Definitions
{
    [Serializable]
    public class IngredientData
    {
        public string ItemId;
        public int Amount;
    }

    [Serializable]
    public class RecipeDefinition : DefinitionBase
    {
        public string OutputItemId;
        public List<IngredientData> Ingredients;
    }
}
