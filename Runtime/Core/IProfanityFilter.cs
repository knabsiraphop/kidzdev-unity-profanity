using System.Collections.Generic;

namespace KidzDev.Unity.Profanity {
    public interface IProfanityFilter {
        bool Contains(string input, ProfanityLanguage language, IAllowList allowList = null);
        string Censor(string input, ProfanityLanguage language, char replacement, IAllowList allowList = null);
        IReadOnlyList<ProfanityMatch> FindAll(string input, ProfanityLanguage language, IAllowList allowList = null);

        void SetWordList(ProfanityLanguage language, string[] words);
        void RemoveWordList(ProfanityLanguage language);
        void Clear();
    }
}
