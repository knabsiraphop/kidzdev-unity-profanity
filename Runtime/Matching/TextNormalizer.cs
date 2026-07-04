using System;
using System.Collections.Generic;
using System.Text;

namespace KidzDev.Unity.Profanity {
    /// <summary>
    /// Normalizes text to defeat common obfuscation, while producing a source-index map
    /// so every position in the normalized string maps back to the original.
    /// </summary>
    public sealed class TextNormalizer {
        public bool CollapseRepeats   { get; set; } = true;
        public bool StripSeparators   { get; set; } = true;
        public bool FoldLeetspeak     { get; set; } = true;

        // Maps each normalized char position → original char position.
        readonly List<int> _indexMap = new();
        readonly StringBuilder _sb   = new();

        static readonly Dictionary<char, char> LeetMap = new() {
            ['@'] = 'a', ['4'] = 'a',
            ['3'] = 'e',
            ['1'] = 'i', ['|'] = 'i',
            ['0'] = 'o',
            ['5'] = 's', ['$'] = 's',
            ['7'] = 't',
            ['8'] = 'b',
            ['9'] = 'g',
        };

        // Separators that can be inserted between letters to evade detection.
        // Space is intentionally excluded — stripping spaces merges words and breaks \b matching.
        static readonly HashSet<char> Separators = new() {
            '.', '-', '_', '*', '+', ',', '/', '\\',
            '​', // zero-width space
            '‌', // zero-width non-joiner
            '‍', // zero-width joiner
            '﻿', // zero-width no-break space
        };

        /// <summary>
        /// Returns the normalized form of <paramref name="input"/> and writes the
        /// source-index map into <paramref name="indexMap"/> (caller owns the list).
        /// </summary>
        public string Normalize(string input, List<int> indexMap) {
            if (string.IsNullOrEmpty(input)) {
                indexMap?.Clear();
                return string.Empty;
            }

            _sb.Clear();
            _indexMap.Clear();

            for (var i = 0; i < input.Length; i++) {
                var c = char.ToLowerInvariant(input[i]);

                // Strip combining diacritics (U+0300–U+036F).
                if (c >= '̀' && c <= 'ͯ') continue;

                // Separator between letters — strip if option on.
                if (StripSeparators && Separators.Contains(c)) continue;

                // Leetspeak fold.
                if (FoldLeetspeak && LeetMap.TryGetValue(c, out var folded)) c = folded;

                // Repeat collapse — skip if same as last char emitted.
                if (CollapseRepeats && _sb.Length > 0 && _sb[_sb.Length - 1] == c) continue;

                _sb.Append(c);
                _indexMap.Add(i);
            }

            indexMap?.Clear();
            indexMap?.AddRange(_indexMap);
            return _sb.ToString();
        }

        /// <summary>
        /// Maps a span [normStart, normStart+normLength) in normalized space back to
        /// [sourceStart, sourceEnd) in original source. Returns false if out of range.
        /// </summary>
        public static bool MapToSource(
            List<int> indexMap, int normStart, int normLength,
            out int sourceStart, out int sourceEnd) {

            sourceStart = sourceEnd = 0;
            if (indexMap == null || normStart < 0 || normStart >= indexMap.Count) return false;

            sourceStart = indexMap[normStart];
            var normEnd = normStart + normLength - 1;
            if (normEnd >= indexMap.Count) normEnd = indexMap.Count - 1;
            sourceEnd = indexMap[normEnd] + 1;
            return true;
        }
    }
}
