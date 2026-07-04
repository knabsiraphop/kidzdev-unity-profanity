using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace KidzDev.Unity.Profanity.Tests {
    sealed class ProfanitySystemPlayModeTests {
        ProfanityManager _previous;

        [SetUp]
        public void SetUp() {
            _previous = ProfanitySystem.Default;
            ProfanitySystem.Default = new ProfanityManager();
        }

        [TearDown]
        public void TearDown() {
            ProfanitySystem.Release();
            ProfanitySystem.Default = _previous;
        }

        [UnityTest]
        public IEnumerator AddSource_LoadAsync_Censor_GoldenPath() =>
            UniTask.ToCoroutine(async () => {
                ProfanitySystem.AddSource("en", new StubLoader(new[] { "shit" }, ProfanityLanguage.English), "");
                await ProfanitySystem.LoadAsync();
                var result = ProfanitySystem.Censor("what the shit!", ProfanityLanguage.English);
                StringAssert.DoesNotContain("shit", result);
            });

        [UnityTest]
        public IEnumerator Censor_ObfuscationVariant() =>
            UniTask.ToCoroutine(async () => {
                ProfanitySystem.AddSource("en", new StubLoader(new[] { "shit" }, ProfanityLanguage.English), "");
                await ProfanitySystem.LoadAsync();
                var result = ProfanitySystem.Censor("$h1t happens", ProfanityLanguage.English);
                StringAssert.DoesNotContain("$h1t", result);
            });

        [UnityTest]
        public IEnumerator Default_IsSwappable() =>
            UniTask.ToCoroutine(async () => {
                var custom = new ProfanityManager(new RegexProfanityFilter());
                ProfanitySystem.Default = custom;
                ProfanitySystem.AddSource("en", new StubLoader(new[] { "fuck" }, ProfanityLanguage.English), "");
                await ProfanitySystem.LoadAsync();
                Assert.IsTrue(ProfanitySystem.Contains("fuck", ProfanityLanguage.English));
            });

        // ── Stub ─────────────────────────────────────────────────────────────────────

        sealed class StubLoader : IProfanityDataLoader {
            readonly string[]          _words;
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
