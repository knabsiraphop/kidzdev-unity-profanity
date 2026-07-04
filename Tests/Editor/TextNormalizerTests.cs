using System.Collections.Generic;
using NUnit.Framework;

namespace KidzDev.Unity.Profanity.Tests {
    sealed class TextNormalizerTests {
        TextNormalizer _n;
        List<int> _map;

        [SetUp]
        public void SetUp() {
            _n   = new TextNormalizer();
            _map = new List<int>();
        }

        [Test]
        public void LowercasesInput() =>
            Assert.AreEqual("world", _n.Normalize("WORLD", _map));

        [Test]
        public void FoldsLeetspeak() =>
            Assert.AreEqual("shit", _n.Normalize("$h1t", _map));

        [Test]
        public void StripsSeparators() =>
            Assert.AreEqual("shit", _n.Normalize("s.h.i.t", _map));

        [Test]
        public void CollapsesRepeats() =>
            Assert.AreEqual("shit", _n.Normalize("shiiit", _map));

        [Test]
        public void CombinedObfuscation() =>
            Assert.AreEqual("shit", _n.Normalize("$h1t", _map));

        [Test]
        public void IndexMapPreservesSourcePositions() {
            var norm = _n.Normalize("s.h.i.t", _map);
            // dot separators stripped → "shit", map = [0,2,4,6]
            Assert.AreEqual("shit", norm);
            Assert.AreEqual(0, _map[0]);
            Assert.AreEqual(2, _map[1]);
            Assert.AreEqual(4, _map[2]);
            Assert.AreEqual(6, _map[3]);
        }

        [Test]
        public void MapToSource_ReturnsCorrectSpan() {
            _n.Normalize("s.h.i.t", _map);
            TextNormalizer.MapToSource(_map, 0, 4, out var start, out var end);
            Assert.AreEqual(0, start);
            Assert.AreEqual(7, end); // 's'=0, last 't'=6 → end = 6+1 = 7
        }

        [Test]
        public void DisabledOptions_DoNotTransform() {
            _n.CollapseRepeats = false;
            _n.StripSeparators = false;
            _n.FoldLeetspeak   = false;
            Assert.AreEqual("s.h.i.t", _n.Normalize("s.h.i.t", _map));
        }
    }
}
