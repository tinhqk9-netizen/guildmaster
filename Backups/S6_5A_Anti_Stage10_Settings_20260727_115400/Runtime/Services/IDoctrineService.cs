using GuildMaster.Runtime.Save;

namespace GuildMaster.Runtime.Services
{
    public interface IDoctrineService
    {
        int GetLevel(string doctrineName);
        int GetProgress(string doctrineName);
        void AddProgress(string doctrineName, int amount);
        bool IsMaxed();
    }
}
