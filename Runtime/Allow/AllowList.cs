using System;
using System.Collections.Generic;

namespace KidzDev.Unity.Profanity {
    public sealed class AllowList : IAllowList {
        readonly HashSet<string> _allowed;

        public AllowList() => _allowed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public AllowList(IEnumerable<string> words) =>
            _allowed = new HashSet<string>(words, StringComparer.OrdinalIgnoreCase);

        public void Add(string word) {
            if (!string.IsNullOrEmpty(word)) _allowed.Add(word);
        }

        public void AddRange(IEnumerable<string> words) {
            foreach (var w in words) Add(w);
        }

        public bool Remove(string word) => _allowed.Remove(word);

        public void Clear() => _allowed.Clear();

        public bool IsAllowed(string word) =>
            !string.IsNullOrEmpty(word) && _allowed.Contains(word);

        /// <summary>
        /// Checks whether any allowed word in the source text fully contains or equals
        /// the matched span, making it a false positive (Scunthorpe guard).
        /// </summary>
        public bool IsFalsePositive(string sourceText, int matchIndex, int matchLength) {
            if (string.IsNullOrEmpty(sourceText)) return false;
            foreach (var allowed in _allowed) {
                var idx = sourceText.IndexOf(allowed, StringComparison.OrdinalIgnoreCase);
                while (idx >= 0) {
                    // match span [matchIndex, matchIndex+matchLength) is inside allowed span
                    if (idx <= matchIndex && idx + allowed.Length >= matchIndex + matchLength)
                        return true;
                    idx = sourceText.IndexOf(allowed, idx + 1, StringComparison.OrdinalIgnoreCase);
                }
            }
            return false;
        }
    }
}
