using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KidzDev.Unity.Profanity.Samples {
    /// <summary>
    /// IProfanityDataLoader backed by Unity Addressables.
    /// Loads a JSON TextAsset tagged with the configured Addressables label/key.
    ///
    /// Usage:
    /// <code>
    /// ProfanitySystem.AddSource("remote", new AddressableProfanityDataLoader(), "profanity");
    /// await ProfanitySystem.LoadAsync();
    /// </code>
    /// Tag your profanity data JSON asset with the Addressables label "profanity"
    /// (or whatever key you pass to AddSource).
    /// </summary>
    public sealed class AddressableProfanityDataLoader : IProfanityDataLoader {
        AsyncOperationHandle<TextAsset> _handle;
        bool _loaded;

        public async UniTask<ProfanityData> LoadAsync(string key, CancellationToken ct = default) {
            if (_loaded) {
                Addressables.Release(_handle);
                _loaded = false;
            }

            _handle = Addressables.LoadAssetAsync<TextAsset>(key);

            // Poll until done — avoids dependency on UniTask.Addressables extension package.
            try {
                await UniTask.WaitUntil(() => _handle.IsDone, cancellationToken: ct);
            } catch (OperationCanceledException) {
                Addressables.Release(_handle);
                throw;
            }

            if (_handle.Status != AsyncOperationStatus.Succeeded) {
                var ex = _handle.OperationException;
                Addressables.Release(_handle);
                Debug.LogError($"[Profanity] Failed to load Addressable '{key}': {ex?.Message}");
                return new ProfanityData();
            }

            var asset = _handle.Result;
            _loaded = true;

            if (asset == null) {
                Debug.LogWarning($"[Profanity] Addressable TextAsset '{key}' is null.");
                return new ProfanityData();
            }

            return ParseJson(asset.text);
        }

        public void Release() {
            if (!_loaded) return;
            Addressables.Release(_handle);
            _loaded = false;
        }

        static ProfanityData ParseJson(string json) {
            var root = JsonUtility.FromJson<JsonRoot>(json) ?? new JsonRoot();
            var data = new ProfanityData();
            if (root.replacementCharacter?.Length == 1)
                data.ReplacementCharacter = root.replacementCharacter[0];
            foreach (var entry in root.languages) {
                if (string.IsNullOrEmpty(entry.language) || entry.words == null) continue;
                if (Enum.TryParse<ProfanityLanguage>(entry.language, ignoreCase: true, out var lang))
                    data.WordLists[lang] = entry.words;
            }
            foreach (var w in root.allowWords)
                if (!string.IsNullOrEmpty(w)) data.AllowWords.Add(w);
            return data;
        }

        [Serializable]
        sealed class JsonRoot {
            public string                    replacementCharacter = "*";
            public ProfanityLanguageList[]   languages            = Array.Empty<ProfanityLanguageList>();
            public string[]                  allowWords           = Array.Empty<string>();
        }
    }
}
