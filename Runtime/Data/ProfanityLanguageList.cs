using System;

namespace KidzDev.Unity.Profanity {
    [Serializable]
    public sealed class ProfanityLanguageList {
        public string language = "English";
        public string[] words  = Array.Empty<string>();
    }
}
