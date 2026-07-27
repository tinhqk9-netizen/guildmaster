namespace GuildMaster.Runtime.Core
{
    public interface IInstanceIdGenerator
    {
        string GenerateInstanceId(string prefix);
    }
}
