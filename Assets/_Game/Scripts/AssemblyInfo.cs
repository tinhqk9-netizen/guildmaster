using System.Runtime.CompilerServices;

// Grants the EditMode/PlayMode test assemblies access to `internal` members of
// GuildMaster.Runtime. Added in Phase 2B so DungeonService's encounter/loot roll methods can be
// exercised directly and statistically (200-1000 iteration loops) without needing to drive the
// full multi-hundred-tick state machine per sample. See phase2b_completion_report.md.
[assembly: InternalsVisibleTo("GuildMaster.Tests")]
[assembly: InternalsVisibleTo("GuildMaster.Tests.PlayMode")]
