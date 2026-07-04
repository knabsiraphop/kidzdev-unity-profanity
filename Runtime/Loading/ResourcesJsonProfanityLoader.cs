using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KidzDev.Unity.Profanity {
    /// <summary>Loads profanity data from a JSON TextAsset in Resources.</summary>
    public sealed class ResourcesJsonProfanityLoader : IProfanityDataLoader {
        [Serializable]
        sealed class JsonRoot {
            public string replacementCharacter = "*";
            public ProfanityLanguageList[] languages = Array.Empty<ProfanityLanguageList>();
            public string[] allowWords = Array.Empty<string>();
        }

        public UniTask<ProfanityData> LoadAsync(string path, CancellationToken ct = default) =>
            UniTask.FromResult(Load(path));

        static ProfanityData Load(string path) {
            var asset = Resources.Load<TextAsset>(path);
            if (asset == null) {
                Debug.LogWarning($"[Profanity] JSON not found at Resources/{path}");
                return new ProfanityData();
            }
            return Parse(asset.text);
        }

        static ProfanityData Parse(string json) {
            var root = JsonUtility.FromJson<JsonRoot>(json) ?? new JsonRoot();
            var data = new ProfanityData();
            if (root.replacementCharacter?.Length == 1)
                data.ReplacementCharacter = root.replacementCharacter[0];

            foreach (var entry in root.languages) {
                if (string.IsNullOrEmpty(entry.language) || entry.words == null) continue;
                if (Enum.TryParse<ProfanityLanguage>(entry.language, ignoreCase: true, out var lang))
                    data.WordLists[lang] = entry.words;
                else
                    Debug.LogWarning($"[Profanity] Unknown language '{entry.language}' in data.");
            }
            foreach (var w in root.allowWords)
                if (!string.IsNullOrEmpty(w)) data.AllowWords.Add(w);
            return data;
        }
    }
}
