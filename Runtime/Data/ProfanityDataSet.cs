using System;
using System.Collections.Generic;
using UnityEngine;

namespace KidzDev.Unity.Profanity {
    [CreateAssetMenu(menuName = "KidzDev/Profanity/Data Set", fileName = "ProfanityDataSet")]
    public sealed class ProfanityDataSet : ScriptableObject {
        [Tooltip("Character used to replace profanity. Default: '*'")]
        [SerializeField] char _replacementCharacter = '*';

        [SerializeField] List<ProfanityLanguageList> _languages = new();

        [Tooltip("Words that should never be flagged (Scunthorpe guard).")]
        [SerializeField] List<string> _allowWords = new();

        public char ReplacementCharacter => _replacementCharacter;
        public IReadOnlyList<ProfanityLanguageList> Languages => _languages;
        public IReadOnlyList<string> AllowWords => _allowWords;
    }
}
