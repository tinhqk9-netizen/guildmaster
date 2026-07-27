namespace GuildMaster.Runtime.Models
{
    public class OfflineProgressResult
    {
        public bool Success { get; set; }
        public long DeltaSeconds { get; set; }
        public bool DispatchDeferred { get; set; }
    }
}
