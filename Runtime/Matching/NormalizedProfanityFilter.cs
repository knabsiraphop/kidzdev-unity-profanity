using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace KidzDev.Unity.Profanity {
    /// <summary>
    /// Obfuscation-aware filter. Normalizes the input before matching, then maps
    /// normalized spans back to source positions for accurate censoring.
    /// </summary>
    public sealed class NormalizedProfanityFilter : IProfanityFilter {
        readonly TextNormalizer _normalizer;
        readonly Dictionary<ProfanityLanguage, Regex> _regexes = new();
        readonly List<int> _indexMap = new();

        // Thai has no Unicode word boundaries; other languages use \b.
        static readonly HashSet<ProfanityLanguage> NoBoundaryLanguages = new() {
            ProfanityLanguage.Thai,
        };

        public TextNormalizer Normalizer => _normalizer;

        public NormalizedProfanityFilter(TextNormalizer normalizer = null) =>
            _normalizer = normalizer ?? new TextNormalizer();

        public void SetWordList(ProfanityLanguage language, string[] words) {
            if (words == null || words.Length == 0) { RemoveWordList(language); return; }
            var pattern = BuildPattern(language, words, _normalizer);
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
            // Process in reverse order to keep earlier indices valid.
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

            var normalized = _normalizer.Normalize(input, _indexMap);
            var matches    = new List<ProfanityMatch>();

            foreach (Match m in regex.Matches(normalized)) {
                if (!TextNormalizer.MapToSource(_indexMap, m.Index, m.Length, out var src, out var end))
                    continue;
                var srcLen = end - src;
                var value  = input.Substring(src, srcLen);

                if (allowList != null && allowList.IsFalsePositive(input, src, srcLen)) continue;

                matches.Add(new ProfanityMatch(src, srcLen, value, language));
            }
            return matches;
        }

        static string BuildPattern(ProfanityLanguage language, string[] words, TextNormalizer normalizer) {
            var useBoundary = !NoBoundaryLanguages.Contains(language);
            var sb = new StringBuilder();
            var any = false;
            for (var i = 0; i < words.Length; i++) {
                if (string.IsNullOrWhiteSpace(words[i])) continue;
                var normalized = normalizer.Normalize(words[i].Trim(), null);
                if (string.IsNullOrEmpty(normalized)) continue;
                if (any) sb.Append('|');
                if (useBoundary) sb.Append(@"\b");
                sb.Append(Regex.Escape(normalized));
                if (useBoundary) sb.Append(@"\b");
                any = true;
            }
            return sb.ToString();
        }
    }
}
