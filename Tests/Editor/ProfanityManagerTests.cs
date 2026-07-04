using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace KidzDev.Unity.Profanity.Tests {
    sealed class ProfanityManagerTests {
        ProfanityManager _manager;

        [SetUp]
        public void SetUp() => _manager = new ProfanityManager();

        [TearDown]
        public void TearDown() => _manager.Dispose();

        [Test]
        public void AddSource_ThenDirectSetWordList_WorksWithoutLoad() {
            _manager.AddSource("a", new StubLoader(new[] { "shit" }, ProfanityLanguage.English), "");
            // Without calling LoadAsync, word lists aren't applied yet → Contains = false.
            Assert.IsFalse(_manager.Contains("shit", ProfanityLanguage.English));
        }

        [UnityTest]
        public System.Collections.IEnumerator LoadAsync_MergesTwoSources() =>
            UniTask.ToCoroutine(async () => {
                _manager.AddSource("en", new StubLoader(new[] { "shit" },  ProfanityLanguage.English), "");
                _manager.AddSource("th", new StubLoader(new[] { "หยาบ" }, ProfanityLanguage.Thai),    "");
                await _manager.LoadAsync();
                Assert.IsTrue(_manager.Contains("shit",  ProfanityLanguage.English));
                Assert.IsTrue(_manager.Contains("หยาบ", ProfanityLanguage.Thai));
            });

        [UnityTest]
        public System.Collections.IEnumerator LoadAsync_JoinsInFlight() =>
            UniTask.ToCoroutine(async () => {
                _manager.AddSource("en", new StubLoader(new[] { "shit" }, ProfanityLanguage.English), "");
                var t1 = _manager.LoadAsync();
                var t2 = _manager.LoadAsync();
                await UniTask.WhenAll(t1, t2);
                Assert.IsTrue(_manager.Contains("shit", ProfanityLanguage.English));
            });

        [UnityTest]
        public System.Collections.IEnumerator Release_ClearsWordLists() =>
            UniTask.ToCoroutine(async () => {
                _manager.AddSource("en", new StubLoader(new[] { "shit" }, ProfanityLanguage.English), "");
                await _manager.LoadAsync();
                _manager.Release();
                Assert.IsFalse(_manager.Contains("shit", ProfanityLanguage.English));
            });

        [UnityTest]
        public System.Collections.IEnumerator ContainsAny_MatchesAcrossLanguages() =>
            UniTask.ToCoroutine(async () => {
                _manager.AddSource("en", new StubLoader(new[] { "shit" }, ProfanityLanguage.English), "");
                await _manager.LoadAsync();
                Assert.IsTrue(_manager.ContainsAny("shit"));
            });

        // ── Stub ─────────────────────────────────────────────────────────────────────

        sealed class StubLoader : IProfanityDataLoader {
            readonly string[]         _words;
            readonly ProfanityLanguage _lang;
            public StubLoader(string[] words, ProfanityLanguage lang) { _words = words; _lang = lang; }
            public UniTask<ProfanityData> LoadAsync(string path, CancellationToken ct = default) {
                var data = new ProfanityData();
                data.WordLists[_lang] = _words;
                return UniTask.FromResult(data);
            }
        }
    }
}
