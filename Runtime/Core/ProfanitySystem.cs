using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace KidzDev.Unity.Profanity {
    /// <summary>
    /// Static facade over <see cref="ProfanityManager.Default"/>.
    /// Swap <see cref="Default"/> for testing or multi-manager setups.
    /// </summary>
    public static class ProfanitySystem {
        static ProfanityManager _default;

        public static ProfanityManager Default {
            get => _default ??= new ProfanityManager();
            set => _default = value;
        }

        // ── Settings ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Loads <c>Resources/ProfanitySettings</c>, creates the right filter, and applies it.
        /// Safe to call when the asset does not exist (uses built-in defaults).
        /// </summary>
        public static void Configure() => Configure(Resources.Load<ProfanitySettings>("ProfanitySettings"));

        /// <summary>
        /// Applies an explicit <paramref name="settings"/> asset, creating the matching filter.
        /// Replaces <see cref="Default"/> with a new manager configured by the settings.
        /// </summary>
        public static void Configure(ProfanitySettings settings) {
            if (settings == null) { Default = new ProfanityManager(); return; }

            IProfanityFilter filter = settings.MatchMode switch {
                MatchMode.Basic => new RegexProfanityFilter(),
                _               => new NormalizedProfanityFilter(),
            };
            Default = new ProfanityManager(filter);
            Default.Apply(settings);
        }

        // ── Forwarded properties ─────────────────────────────────────────────────────

        public static char ReplacementCharacter {
            get => Default.ReplacementCharacter;
            set => Default.ReplacementCharacter = value;
        }

        public static IAllowList AllowList => Default.AllowList;

        // ── Sources ──────────────────────────────────────────────────────────────────

        public static void AddSource(string name, IProfanityDataLoader loader, string path)
            => Default.AddSource(name, loader, path);

        public static bool RemoveSource(string name) => Default.RemoveSource(name);

        // ── Loading ──────────────────────────────────────────────────────────────────

        public static UniTask LoadAsync(CancellationToken ct = default) => Default.LoadAsync(ct);

        // ── Query ────────────────────────────────────────────────────────────────────

        public static bool Contains(string input, ProfanityLanguage language)
            => Default.Contains(input, language);

        public static bool ContainsAny(string input)
            => Default.ContainsAny(input);

        public static string Censor(string input, ProfanityLanguage language)
            => Default.Censor(input, language);

        public static IReadOnlyList<ProfanityMatch> FindAll(string input, ProfanityLanguage language)
            => Default.FindAll(input, language);

        // ── Release ───────────────────────────────────────────────────────────────────

        public static void Release() => Default.Release();
    }
}
