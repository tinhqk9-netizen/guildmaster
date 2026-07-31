namespace GuildMaster.Runtime.Services
{
    public interface IGameLoopService
    {
        void Initialize();
        void ProcessOfflineCatchup();
        void TickRuntime();
    }
}
