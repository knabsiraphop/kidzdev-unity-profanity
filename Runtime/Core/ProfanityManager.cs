using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KidzDev.Unity.Profanity {
    /// <summary>
    /// Owns profanity state: named data sources, merged word lists, filter, and allowlist.
    /// Use <see cref="ProfanitySystem"/> for the default singleton instance.
    /// </summary>
    public sealed class ProfanityManager : IDisposable {
        readonly struct Source {
            public readonly IProfanityDataLoader Loader;
            public readonly string Path;
            public Source(IProfanityDataLoader loader, string path) { Loader = loader; Path = path; }
        }

        readonly Dictionary<string, Source> _sources = new();
        readonly IProfanityFilter           _filter;

        UniTask              _loadTask;
        bool                 _isLoading;
        CancellationTokenSource _loadCts;

        public char        ReplacementCharacter { get; set; } = '*';
        public IAllowList  AllowList            { get; }

        public ProfanityManager(IProfanityFilter filter = null, IAllowList allowList = null) {
            _filter   = filter    ?? new NormalizedProfanityFilter();
            AllowList = allowList ?? new AllowList();
        }

        // ── Sources ──────────────────────────────────────────────────────────────────

        public void AddSource(string name, IProfanityDataLoader loader, string path) {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
            if (loader == null)             throw new ArgumentNullException(nameof(loader));
            _sources[name] = new Source(loader, path ?? string.Empty);
        }

        public bool RemoveSource(string name) => _sources.Remove(name);

        // ── Loading ──────────────────────────────────────────────────────────────────

        public async UniTask LoadAsync(CancellationToken ct = default) {
            if (_isLoading) { await _loadTask; return; }

            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            _isLoading = true;
            _loadTask  = LoadAsyncCore(_loadCts.Token);
            try   { await _loadTask; }
            finally { _isLoading = false; }
        }

        async UniTask LoadAsyncCore(CancellationToken ct) {
            var tasks = new List<UniTask<ProfanityData>>(_sources.Count);
            foreach (var (_, src) in _sources)
                tasks.Add(src.Loader.LoadAsync(src.Path, ct));

            var results = await UniTask.WhenAll(tasks);
            ct.ThrowIfCancellationRequested();

            _filter.Clear();
            if (AllowList is AllowList al) al.Clear();

            var merged = new ProfanityData();
            foreach (var d in results) merged.MergeFrom(d);

            foreach (var (lang, words) in merged.WordLists)
                _filter.SetWordList(lang, words);

            if (AllowList is AllowList al2) al2.AddRange(merged.AllowWords);

            ReplacementCharacter = merged.ReplacementCharacter;
        }

        // ── Query ────────────────────────────────────────────────────────────────────

        public bool Contains(string input, ProfanityLanguage language) =>
            _filter.Contains(input, language, AllowList);

        public bool ContainsAny(string input) {
            foreach (ProfanityLanguage lang in Enum.GetValues(typeof(ProfanityLanguage)))
                if (_filter.Contains(input, lang, AllowList)) return true;
            return false;
        }

        public string Censor(string input, ProfanityLanguage language) =>
            _filter.Censor(input, language, ReplacementCharacter, AllowList);

        public IReadOnlyList<ProfanityMatch> FindAll(string input, ProfanityLanguage language) =>
            _filter.FindAll(input, language, AllowList);

        // ── Settings ─────────────────────────────────────────────────────────────────

        public void Apply(ProfanitySettings settings) {
            if (settings == null) return;
            ReplacementCharacter = settings.ReplacementChar;

            if (_filter is NormalizedProfanityFilter npf) {
                npf.Normalizer.CollapseRepeats = settings.CollapseRepeats;
                npf.Normalizer.StripSeparators = settings.StripSeparators;
            }

            IProfanityDataLoader loader = settings.LoaderType switch {
                ProfanityLoaderType.Asset => new ResourcesAssetProfanityLoader(),
                _                        => new ResourcesJsonProfanityLoader(),
            };
            AddSource("default", loader, settings.DataPath);
        }

        // ── Release ───────────────────────────────────────────────────────────────────

        public void Release() {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts   = null;
            _isLoading = false;
            _filter.Clear();
            _sources.Clear();
            if (AllowList is AllowList al) al.Clear();
        }

        public void Dispose() => Release();
    }
}
