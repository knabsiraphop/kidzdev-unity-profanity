using System;
using System.Collections.Generic;

namespace KidzDev.Unity.Profanity {
    /// <summary>Runtime data merged from all loaded sources.</summary>
    public sealed class ProfanityData {
        public char ReplacementCharacter { get; set; } = '*';
        public Dictionary<ProfanityLanguage, string[]> WordLists { get; } = new();
        public HashSet<string> AllowWords { get; } = new(StringComparer.OrdinalIgnoreCase);

        public void MergeFrom(ProfanityData other) {
            if (other == null) return;
            foreach (var (lang, words) in other.WordLists) {
                if (WordLists.TryGetValue(lang, out var existing)) {
                    var merged = new string[existing.Length + words.Length];
                    existing.CopyTo(merged, 0);
                    words.CopyTo(merged, existing.Length);
                    WordLists[lang] = merged;
                } else {
                    WordLists[lang] = words;
                }
            }
            foreach (var w in other.AllowWords) AllowWords.Add(w);
        }

        public void Clear() {
            WordLists.Clear();
            AllowWords.Clear();
        }
    }
}
