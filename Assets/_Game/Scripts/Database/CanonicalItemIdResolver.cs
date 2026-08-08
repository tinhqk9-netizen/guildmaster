using System;
using System.Collections.Generic;
using GuildMaster.Definitions;

namespace GuildMaster.Database
{
    /// <summary>
    /// Resolves item identifiers emitted by the legacy decoder to the canonical
    /// ItemDefinition.id used by runtime services. Exact IDs always win; the
    /// normalized fallback only removes non alpha-numeric separators and lowercases
    /// the value. Ambiguous normalized matches are rejected instead of guessed.
    /// </summary>
    public sealed class CanonicalItemIdResolver
    {
        private readonly Dictionary<string, string> _exactIds =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, List<string>> _normalizedIds =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        public CanonicalItemIdResolver(IEnumerable<ItemDefinition> definitions)
        {
            if (definitions == null) return;

            foreach (var definition in definitions)
            {
                if (definition == null || string.IsNullOrWhiteSpace(definition.id)) continue;

                _exactIds[definition.id] = definition.id;

                string normalized = Normalize(definition.id);
                if (!_normalizedIds.TryGetValue(normalized, out var matches))
                {
                    matches = new List<string>();
                    _normalizedIds[normalized] = matches;
                }

                bool alreadyPresent = false;
                foreach (var match in matches)
                {
                    if (string.Equals(match, definition.id, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyPresent = true;
                        break;
                    }
                }

                if (!alreadyPresent) matches.Add(definition.id);
            }
        }

        public bool TryResolve(string rawId, out string canonicalId, out string failureReason)
        {
            canonicalId = null;
            failureReason = null;

            if (string.IsNullOrWhiteSpace(rawId))
            {
                failureReason = "item id is empty";
                return false;
            }

            if (_exactIds.TryGetValue(rawId, out canonicalId)) return true;

            string normalized = Normalize(rawId);
            if (!_normalizedIds.TryGetValue(normalized, out var matches) || matches.Count == 0)
            {
                failureReason = $"no ItemDefinition matches normalized id '{normalized}'";
                return false;
            }

            if (matches.Count > 1)
            {
                failureReason = $"normalized id '{normalized}' is ambiguous: {string.Join(", ", matches)}";
                return false;
            }

            canonicalId = matches[0];
            return true;
        }

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var chars = new List<char>(value.Length);
            foreach (char c in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c)) chars.Add(c);
            }

            return new string(chars.ToArray());
        }
    }
}
