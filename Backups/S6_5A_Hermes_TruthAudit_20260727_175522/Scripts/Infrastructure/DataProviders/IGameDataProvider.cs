using System.Collections.Generic;

namespace GuildMaster.Infrastructure.DataProviders
{
    public interface IGameDataProvider
    {
        string ProviderName { get; }
        bool Exists(string relativePath);
        string ReadText(string relativePath);
        IEnumerable<string> EnumerateFiles();
    }
}
