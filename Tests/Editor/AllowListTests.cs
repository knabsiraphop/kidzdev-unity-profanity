using NUnit.Framework;

namespace KidzDev.Unity.Profanity.Tests {
    sealed class AllowListTests {
        [Test]
        public void IsAllowed_ReturnsTrueForAddedWord() {
            var list = new AllowList(new[] { "scunthorpe" });
            Assert.IsTrue(list.IsAllowed("scunthorpe"));
        }

        [Test]
        public void IsAllowed_IsCaseInsensitive() {
            var list = new AllowList(new[] { "Scunthorpe" });
            Assert.IsTrue(list.IsAllowed("SCUNTHORPE"));
        }

        [Test]
        public void IsAllowed_ReturnsFalseForUnknownWord() {
            var list = new AllowList(new[] { "class" });
            Assert.IsFalse(list.IsAllowed("shit"));
        }

        [Test]
        public void IsFalsePositive_MatchInsideAllowedWord() {
            var list  = new AllowList(new[] { "class" });
            // "ass" at index 2, length 3 inside "class" (length 5)
            Assert.IsTrue(list.IsFalsePositive("class", 2, 3));
        }

        [Test]
        public void IsFalsePositive_StandaloneMatchNotCovered() {
            var list  = new AllowList(new[] { "class" });
            Assert.IsFalse(list.IsFalsePositive("you ass!", 4, 3));
        }

        [Test]
        public void Remove_RemovesWord() {
            var list = new AllowList(new[] { "scunthorpe" });
            list.Remove("scunthorpe");
            Assert.IsFalse(list.IsAllowed("scunthorpe"));
        }

        [Test]
        public void Clear_RemovesAll() {
            var list = new AllowList(new[] { "a", "b", "c" });
            list.Clear();
            Assert.IsFalse(list.IsAllowed("a"));
        }
    }
}
