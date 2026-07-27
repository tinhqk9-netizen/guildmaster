namespace GuildMaster.Runtime.Models
{
    public enum MerchantFailureReason
    {
        None,
        InvalidDungeonId,
        DungeonNotFound,
        NoOffersAvailable,
        DeferredPriceOrCurrencyRule
    }

    public class MerchantResult
    {
        public bool Success { get; set; }
        public MerchantFailureReason FailureReason { get; set; }

        public static MerchantResult Ok() => new MerchantResult { Success = true, FailureReason = MerchantFailureReason.None };
        public static MerchantResult Fail(MerchantFailureReason reason) => new MerchantResult { Success = false, FailureReason = reason };
    }
}
