using UnityEngine;

namespace KidzDev.Unity.Profanity {
    /// <summary>
    /// Resources-loaded ScriptableObject that configures <see cref="ProfanityManager"/>.
    /// Place one asset at <c>Resources/ProfanitySettings.asset</c> and call
    /// <see cref="ProfanitySystem.Configure()"/> (or pass it to <see cref="ProfanityManager.Apply"/>).
    /// Use <b>KidzDev &gt; Profanity &gt; Create Settings</b> to create it.
    /// </summary>
    [CreateAssetMenu(menuName = "KidzDev/Profanity/Settings", fileName = "ProfanitySettings")]
    public sealed class ProfanitySettings : ScriptableObject {
        [Tooltip("Loader used to read word-list data.")]
        [SerializeField] ProfanityLoaderType _loaderType = ProfanityLoaderType.Json;

        [Tooltip("Resources path to the profanity data asset (no extension).")]
        [SerializeField] string _dataPath = "Profanity/ProfanityData";

        [Tooltip("Matching engine: Normalized (obfuscation-aware) or Basic (fast regex).")]
        [SerializeField] MatchMode _matchMode = MatchMode.Normalized;

        [Tooltip("Character used to replace profanity. Override per-source via the data asset.")]
        [SerializeField] char _replacementCharacter = '*';

        [Tooltip("Collapse repeated characters during normalization (e.g. 'shiiit' → 'shit').")]
        [SerializeField] bool _collapseRepeats = true;

        [Tooltip("Strip separator characters between letters (e.g. 's.h.i.t' → 'shit').")]
        [SerializeField] bool _stripSeparators = true;

        public ProfanityLoaderType LoaderType        => _loaderType;
        public string              DataPath          => _dataPath;
        public MatchMode           MatchMode         => _matchMode;
        public char                ReplacementChar   => _replacementCharacter;
        public bool                CollapseRepeats   => _collapseRepeats;
        public bool                StripSeparators   => _stripSeparators;
    }
}
