namespace KidzDev.Unity.Profanity {
    public interface IAllowList {
        /// <summary>Returns true when <paramref name="word"/> should never be flagged.</summary>
        bool IsAllowed(string word);

        /// <summary>
        /// Returns true when <paramref name="match"/> is a false positive because
        /// it occurs inside an allowed word (e.g. "ass" inside "class").
        /// </summary>
        bool IsFalsePositive(string sourceText, int matchIndex, int matchLength);
    }
}
