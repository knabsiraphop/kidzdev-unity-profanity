namespace KidzDev.Unity.Profanity {
    public enum MatchMode {
        /// <summary>Obfuscation-aware: leetspeak fold, separator strip, repeat-collapse.</summary>
        Normalized,
        /// <summary>Fast per-language word-boundary regex with no normalization.</summary>
        Basic,
    }
}
