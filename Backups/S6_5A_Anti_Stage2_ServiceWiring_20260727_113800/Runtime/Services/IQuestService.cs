namespace GuildMaster.Runtime.Services
{
    public interface IQuestService
    {
        void Increment(string questInstanceId, long amount);
        void IncrementToValue(string questInstanceId, long newValue);
    }
}
