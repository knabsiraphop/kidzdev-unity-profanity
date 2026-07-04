using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KidzDev.Unity.Profanity {
    /// <summary>Loads profanity data from a <see cref="ProfanityDataSet"/> ScriptableObject in Resources.</summary>
    public sealed class ResourcesAssetProfanityLoader : IProfanityDataLoader {
        public UniTask<ProfanityData> LoadAsync(string path, CancellationToken ct = default) =>
            UniTask.FromResult(Load(path));

        static ProfanityData Load(string path) {
            var asset = Resources.Load<ProfanityDataSet>(path);
            if (asset == null) {
                Debug.LogWarning($"[Profanity] ProfanityDataSet not found at Resources/{path}");
                return new ProfanityData();
            }
            var data = new ProfanityData { ReplacementCharacter = asset.ReplacementCharacter };
            foreach (var entry in asset.Languages) {
                if (string.IsNullOrEmpty(entry.language) || entry.words == null) continue;
                if (Enum.TryParse<ProfanityLanguage>(entry.language, ignoreCase: true, out var lang))
                    data.WordLists[lang] = entry.words;
                else
                    Debug.LogWarning($"[Profanity] Unknown language '{entry.language}' in DataSet.");
            }
            foreach (var w in asset.AllowWords)
                if (!string.IsNullOrEmpty(w)) data.AllowWords.Add(w);
            return data;
        }
    }
}
