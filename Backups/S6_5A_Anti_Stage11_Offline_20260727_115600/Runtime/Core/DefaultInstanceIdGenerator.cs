using System;
using System.Threading;

namespace GuildMaster.Runtime.Core
{
    public class DefaultInstanceIdGenerator : IInstanceIdGenerator
    {
        private long _instanceCounter = 0;

        public string GenerateInstanceId(string prefix)
        {
            long id = Interlocked.Increment(ref _instanceCounter);
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"{prefix}_{timestamp}_{id}";
        }
    }
}
