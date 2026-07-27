namespace GuildMaster.Runtime.Models
{
    public enum CraftFailureReason
    {
        None,
        InvalidRecipeId,
        RecipeNotFound,
        ManualRuleRequired,
        InvalidOutputItem,
        InvalidIngredients,
        MissingIngredients,
        OutputStackUnknown,
        Deferred
    }

    public class CraftResult
    {
        public bool Success { get; set; }
        public CraftFailureReason FailureReason { get; set; }

        public static CraftResult Ok() => new CraftResult { Success = true, FailureReason = CraftFailureReason.None };
        public static CraftResult Fail(CraftFailureReason reason) => new CraftResult { Success = false, FailureReason = reason };
    }
}
