using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KidzDev.Unity.Profanity {
    /// <summary>
    /// Fast basic filter: per-language word-boundary regex against the raw (non-normalized) input.
    /// Select via <see cref="MatchMode.Basic"/> in ProfanitySettings.
    /// </summary>
    public sealed class RegexProfanityFilter : IProfanityFilter {
        readonly Dictionary<ProfanityLanguage, Regex> _regexes = new();

        static readonly HashSet<ProfanityLanguage> NoBoundaryLanguages = new() {
            ProfanityLanguage.Thai,
        };

        public void SetWordList(ProfanityLanguage language, string[] words) {
            if (words == null || words.Length == 0) { RemoveWordList(language); return; }
            var pattern = BuildPattern(language, words);
            var opts    = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;
            _regexes[language] = new Regex(pattern, opts);
        }

        public void RemoveWordList(ProfanityLanguage language) => _regexes.Remove(language);

        public void Clear() => _regexes.Clear();

        public bool Contains(string input, ProfanityLanguage language, IAllowList allowList = null) =>
            FindAll(input, language, allowList).Count > 0;

        public string Censor(string input, ProfanityLanguage language, char replacement, IAllowList allowList = null) {
            var matches = FindAll(input, language, allowList);
            if (matches.Count == 0) return input;
            var sb = new StringBuilder(input);
            for (var i = matches.Count - 1; i >= 0; i--) {
                var m = matches[i];
                for (var j = 0; j < m.Length; j++)
                    sb[m.Index + j] = replacement;
            }
            return sb.ToString();
        }

        public IReadOnlyList<ProfanityMatch> FindAll(string input, ProfanityLanguage language, IAllowList allowList = null) {
            if (string.IsNullOrEmpty(input) || !_regexes.TryGetValue(language, out var regex))
                return Array.Empty<ProfanityMatch>();

            var matches = new List<ProfanityMatch>();
            foreach (Match m in regex.Matches(input)) {
                if (allowList != null && allowList.IsFalsePositive(input, m.Index, m.Length)) continue;
                matches.Add(new ProfanityMatch(m.Index, m.Length, m.Value, language));
            }
            return matches;
        }

        static string BuildPattern(ProfanityLanguage language, string[] words) {
            var useBoundary = !NoBoundaryLanguages.Contains(language);
            var sb = new StringBuilder();
            for (var i = 0; i < words.Length; i++) {
                if (string.IsNullOrWhiteSpace(words[i])) continue;
                if (i > 0) sb.Append('|');
                if (useBoundary) sb.Append(@"\b");
                sb.Append(Regex.Escape(words[i].Trim()));
                if (useBoundary) sb.Append(@"\b");
            }
            return sb.ToString();
        }
    }
}
