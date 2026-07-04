using NUnit.Framework;

namespace KidzDev.Unity.Profanity.Tests {
    sealed class RegexFilterTests {
        RegexProfanityFilter _filter;

        [SetUp]
        public void SetUp() {
            _filter = new RegexProfanityFilter();
            _filter.SetWordList(ProfanityLanguage.English, new[] { "shit", "fuck", "ass" });
        }

        [Test] public void DetectsExactWord()      => Assert.IsTrue(_filter.Contains("shit", ProfanityLanguage.English));
        [Test] public void DetectsUppercase()      => Assert.IsTrue(_filter.Contains("SHIT", ProfanityLanguage.English));
        [Test] public void DetectsInSentence()     => Assert.IsTrue(_filter.Contains("what the shit man", ProfanityLanguage.English));
        [Test] public void DoesNotFoldLeetspeak()  => Assert.IsFalse(_filter.Contains("$h1t", ProfanityLanguage.English));
        [Test] public void DoesNotStripSeparators() => Assert.IsFalse(_filter.Contains("s.h.i.t", ProfanityLanguage.English));

        [Test]
        public void CensorReplacesSpan() {
            var result = _filter.Censor("what the shit man", ProfanityLanguage.English, '*');
            Assert.AreEqual("what the **** man", result);
        }

        [Test]
        public void FindAllReturnsMultipleMatches() {
            var matches = _filter.FindAll("shit and fuck", ProfanityLanguage.English);
            Assert.AreEqual(2, matches.Count);
        }

        [Test] public void NullInputReturnsFalse()  => Assert.IsFalse(_filter.Contains(null, ProfanityLanguage.English));
        [Test] public void EmptyInputReturnsFalse() => Assert.IsFalse(_filter.Contains("",   ProfanityLanguage.English));
        [Test] public void UnknownLanguageReturnsFalse() => Assert.IsFalse(_filter.Contains("shit", ProfanityLanguage.Thai));

        // ── Regression: blank entries must not produce an always-match pattern ────────
        // A leading/only blank word used to build a pattern like "|\bshit\b" or "" — both
        // of which match an empty span at every position, flagging any input as profane.

        [Test]
        public void LeadingBlankEntry_DoesNotMatchEverything() {
            var f = new RegexProfanityFilter();
            f.SetWordList(ProfanityLanguage.English, new[] { "", "shit" });
            Assert.IsFalse(f.Contains("hello world", ProfanityLanguage.English));
            Assert.IsTrue(f.Contains("shit", ProfanityLanguage.English));
        }

        [Test]
        public void AllBlankEntries_NeverMatches() {
            var f = new RegexProfanityFilter();
            f.SetWordList(ProfanityLanguage.English, new[] { "", "   ", "" });
            Assert.IsFalse(f.Contains("hello world", ProfanityLanguage.English));
            Assert.IsFalse(f.Contains("", ProfanityLanguage.English));
        }
    }
}
