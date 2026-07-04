namespace KidzDev.Unity.Profanity {
    public readonly struct ProfanityMatch {
        public readonly int Index;
        public readonly int Length;
        public readonly string Value;
        public readonly ProfanityLanguage Language;

        public ProfanityMatch(int index, int length, string value, ProfanityLanguage language) {
            Index    = index;
            Length   = length;
            Value    = value;
            Language = language;
        }

        public override string ToString() => $"[{Language}] '{Value}' @{Index}+{Length}";
    }
}
