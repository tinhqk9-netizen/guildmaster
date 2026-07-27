using System;
using GuildMaster.Definitions.Enums;

namespace GuildMaster.Runtime.Models
{
    public class StatusEffectRuntime
    {
        public StatusEffectType Type { get; set; }
        public string SourceInstanceId { get; set; }
        public int TurnsLeft { get; set; }
        
        // Probability is used during application, maybe not needed here unless it's a decaying effect, but Java stores it.
        // For now, minimal.
        public double Probability { get; set; }
    }
}
