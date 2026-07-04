using NUnit.Framework;

namespace KidzDev.Unity.Profanity.Tests {
    sealed class NormalizedFilterTests {
        NormalizedProfanityFilter _filter;

        [SetUp]
        public void SetUp() {
            _filter = new NormalizedProfanityFilter();
            _filter.SetWordList(ProfanityLanguage.English, new[] { "shit", "fuck", "ass" });
        }

        // ── Detection ────────────────────────────────────────────────────────────────

        [Test] public void DetectsPlainWord()         => Assert.IsTrue(_filter.Contains("shit", ProfanityLanguage.English));
        [Test] public void DetectsLeetspeak()         => Assert.IsTrue(_filter.Contains("$h1t", ProfanityLanguage.English));
        [Test] public void DetectsSeparated()         => Assert.IsTrue(_filter.Contains("s.h.i.t", ProfanityLanguage.English));
        [Test] public void DetectsRepeated()          => Assert.IsTrue(_filter.Contains("shiiit", ProfanityLanguage.English));
        [Test] public void DetectsCombined()          => Assert.IsTrue(_filter.Contains("$h1t", ProfanityLanguage.English));
        [Test] public void DetectsUppercase()         => Assert.IsTrue(_filter.Contains("SHIT", ProfanityLanguage.English));
        [Test] public void DetectsInSentence()        => Assert.IsTrue(_filter.Contains("what the shit man", ProfanityLanguage.English));

        // ── Scunthorpe guard ─────────────────────────────────────────────────────────

        [Test]
        public void DoesNotFlagScunthorpe() {
            var allow = new AllowList(new[] { "scunthorpe", "assassin", "class", "bass" });
            Assert.IsFalse(_filter.Contains("scunthorpe", ProfanityLanguage.English, allow));
            Assert.IsFalse(_filter.Contains("assassin",   ProfanityLanguage.English, allow));
            Assert.IsFalse(_filter.Contains("class",      ProfanityLanguage.English, allow));
        }

        [Test]
        public void StillFlagsStandaloneWord() {
            var allow = new AllowList(new[] { "class" });
            Assert.IsTrue(_filter.Contains("you are an ass", ProfanityLanguage.English, allow));
        }

        // ── Censoring ────────────────────────────────────────────────────────────────

        [Test]
        public void CensorReplacesSpan() {
            var result = _filter.Censor("what the shit man", ProfanityLanguage.English, '*');
            Assert.AreEqual("what the **** man", result);
        }

        [Test]
        public void CensorObfuscatedPreservesLength() {
            var input  = "what the $h1t man";
            var result = _filter.Censor(input, ProfanityLanguage.English, '*');
            Assert.AreEqual(input.Length, result.Length);
            StringAssert.DoesNotContain("$h1t", result);
        }

        [Test]
        public void CensorSeparated() {
            var result = _filter.Censor("s.h.i.t happens", ProfanityLanguage.English, '*');
            StringAssert.DoesNotContain("s.h.i.t", result);
        }

        // ── FindAll ──────────────────────────────────────────────────────────────────

        [Test]
        public void FindAllReturnsMultipleMatches() {
            var matches = _filter.FindAll("shit and fuck", ProfanityLanguage.English);
            Assert.AreEqual(2, matches.Count);
        }

        [Test]
        public void CleanTextReturnsEmpty() {
            var matches = _filter.FindAll("hello world", ProfanityLanguage.English);
            Assert.AreEqual(0, matches.Count);
        }

        // ── Edge cases ───────────────────────────────────────────────────────────────

        [Test] public void NullInputReturnsFalse()   => Assert.IsFalse(_filter.Contains(null,  ProfanityLanguage.English));
        [Test] public void EmptyInputReturnsFalse()  => Assert.IsFalse(_filter.Contains("",    ProfanityLanguage.English));
        [Test] public void UnknownLanguageReturnsFalse() => Assert.IsFalse(_filter.Contains("shit", ProfanityLanguage.Thai));
    }
}
