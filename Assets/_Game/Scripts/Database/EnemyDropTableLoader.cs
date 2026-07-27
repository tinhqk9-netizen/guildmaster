using System.Collections.Generic;
using System.Text.RegularExpressions;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    /// <summary>
    /// Fills <see cref="EnemyDefinition.DropTable"/> from the raw enemies JSON.
    ///
    /// JsonUtility cannot deserialize a dictionary, and the drop tables in the decode are
    /// exactly that shape — <c>"Drops": { "bone_fragment": 946, ... }</c> plus a parallel
    /// <c>"DropStacks"</c>. Rather than reshaping the data file (which would drift from what
    /// the converter produces), the two objects are parsed out here and zipped into a list.
    ///
    /// Weights stay per-mille on the 1000 scale used by <c>Utils.rollFromWeightedMap</c>.
    /// </summary>
    public static class EnemyDropTableLoader
    {
        // Matches one enemy record's "id", "Drops" and "DropStacks" blocks.
        private static readonly Regex RecordRegex = new Regex(
            "\"id\"\\s*:\\s*\"(?<id>[^\"]+)\"", RegexOptions.Compiled);

        private static readonly Regex DropsBlockRegex = new Regex(
            "\"Drops\"\\s*:\\s*\\{(?<body>[^}]*)\\}", RegexOptions.Compiled);

        private static readonly Regex StacksBlockRegex = new Regex(
            "\"DropStacks\"\\s*:\\s*\\{(?<body>[^}]*)\\}", RegexOptions.Compiled);

        private static readonly Regex PairRegex = new Regex(
            "\"(?<key>[^\"]+)\"\\s*:\\s*(?<value>-?\\d+)", RegexOptions.Compiled);

        /// <summary>
        /// Parses <paramref name="rawJson"/> and attaches drop tables to the matching
        /// definitions. Enemies whose <c>listDrops()</c> is empty simply keep an empty table.
        /// </summary>
        /// <returns>How many definitions received a non-empty drop table.</returns>
        public static int Apply(string rawJson, IEnumerable<EnemyDefinition> definitions)
        {
            if (string.IsNullOrEmpty(rawJson) || definitions == null) return 0;

            Dictionary<string, List<EnemyDropEntry>> byId = Parse(rawJson);
            int applied = 0;

            foreach (EnemyDefinition def in definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.id)) continue;
                if (def.DropTable == null) def.DropTable = new List<EnemyDropEntry>();

                if (byId.TryGetValue(def.id, out List<EnemyDropEntry> table) && table.Count > 0)
                {
                    def.DropTable = table;
                    applied++;
                }
            }

            return applied;
        }

        private static Dictionary<string, List<EnemyDropEntry>> Parse(string rawJson)
        {
            var result = new Dictionary<string, List<EnemyDropEntry>>();

            // Records are separated by their "id" key; slice between consecutive ids so a
            // record's Drops block cannot be attributed to its neighbour.
            MatchCollection ids = RecordRegex.Matches(rawJson);
            for (int i = 0; i < ids.Count; i++)
            {
                string id = ids[i].Groups["id"].Value;
                int start = ids[i].Index;
                int end = (i + 1 < ids.Count) ? ids[i + 1].Index : rawJson.Length;
                string slice = rawJson.Substring(start, end - start);

                Match drops = DropsBlockRegex.Match(slice);
                if (!drops.Success) continue;

                Dictionary<string, int> weights = ReadPairs(drops.Groups["body"].Value);
                if (weights.Count == 0) continue;

                Match stacks = StacksBlockRegex.Match(slice);
                Dictionary<string, int> stackCounts = stacks.Success
                    ? ReadPairs(stacks.Groups["body"].Value)
                    : new Dictionary<string, int>();

                var table = new List<EnemyDropEntry>(weights.Count);
                foreach (KeyValuePair<string, int> pair in weights)
                {
                    table.Add(new EnemyDropEntry
                    {
                        ItemId = pair.Key,
                        Weight = pair.Value,
                        StackCount = stackCounts.TryGetValue(pair.Key, out int s) && s > 0 ? s : 1
                    });
                }

                result[id] = table;
            }

            return result;
        }

        private static Dictionary<string, int> ReadPairs(string body)
        {
            var map = new Dictionary<string, int>();
            foreach (Match m in PairRegex.Matches(body))
            {
                map[m.Groups["key"].Value] = int.Parse(m.Groups["value"].Value);
            }
            return map;
        }
    }
}
