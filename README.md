# KidzDev Unity Profanity

Production-grade, multilingual, obfuscation-aware profanity filter for Unity. Built to defeat
common evasion tricks (leetspeak, separators, letter-repeats) while guarding against false
positives (the "Scunthorpe problem"), with a fully substitutable data source, filter engine, and
allowlist.

## Features

- **Obfuscation-aware matching** — normalizes text before matching to catch `$h1t`, `s.h.i.t`,
  `shiiiit` and similar evasions, then maps the match back to the exact position in the original
  string so censoring never distorts unrelated characters.
- **Two selectable filter engines** — `Normalized` (obfuscation-aware, default) and `Basic` (fast
  exact/word-boundary regex, no normalization overhead) via `ProfanitySettings.MatchMode`.
- **False-positive guard (Scunthorpe problem)** — an `AllowList` seam lets you protect words like
  `class`, `assassin`, `Scunthorpe`, `cockpit` from being flagged just because they contain a
  banned substring.
- **Multilingual out of the box** — English (word-boundary matching) and Thai (contiguous
  matching, since Thai script has no inter-word spaces) via your own `ProfanityLanguage` enum,
  easily extended with more languages.
- **Facade + seam + default-impl architecture** — `ProfanitySystem` (static facade) →
  `ProfanityManager` (instance, `IDisposable`) → swappable `IProfanityFilter` / `IAllowList` /
  `IProfanityDataLoader`. Swap any piece without touching call sites.
- **Named, mergeable data sources** — register multiple sources (e.g. a built-in list plus a
  live-updated remote list) and `LoadAsync` merges them into one filter; in-flight loads are
  joined so concurrent calls never race.
- **Pluggable data loading** — ships with `Resources`-based JSON and ScriptableObject loaders;
  an optional Addressables-backed loader is available as a sample for remote/CDN word lists,
  keeping the core package dependency-free of Addressables.
- **Works with zero setup** — `ProfanitySystem.Configure()` auto-loads a built-in default word
  list (`Runtime/Resources/ProfanityData.json`, ~100 English + 35+ Thai entries) when no
  `ProfanitySettings` asset exists, so the package is useful immediately after installation.
- **Zero-allocation queries where it matters** — `Contains`/`ContainsAny` for boolean checks,
  `Censor` for replacement, `FindAll` for full match metadata (index, length, matched value,
  language) when you need more than a yes/no answer.
- **UniTask-only core dependency** — no Newtonsoft, no Addressables required; `JsonUtility` is
  used for data parsing.

## Installation

Open **Package Manager → + → Add package from git URL** and enter:

```
https://github.com/knabsiraphop/kidzdev-unity-profanity.git#v1.0.2
```

Or add to `Packages/manifest.json`:

```json
"com.kidzdev.unity.profanity": "https://github.com/knabsiraphop/kidzdev-unity-profanity.git#v1.0.2"
```

> **UniTask** (`com.cysharp.unitask`) is a required dependency — add the OpenUPM scoped registry if it is not already present.

## Quick start

### 1 — Configure once (e.g. in a bootstrap MonoBehaviour)

```csharp
using KidzDev.Unity.Profanity;

// Auto-loads Resources/ProfanitySettings.asset if present; otherwise falls back to the
// package's built-in default word list — no setup required to get started.
ProfanitySystem.Configure();
await ProfanitySystem.LoadAsync(destroyCancellationToken);
```

### 2 — Censor or check user input

```csharp
// Replace profanity with the configured replacement character ('*' by default)
string clean = ProfanitySystem.Censor(userInput, ProfanityLanguage.English);

// Check only (boolean)
bool hasProfanity = ProfanitySystem.Contains(userInput, ProfanityLanguage.English);

// Check across every registered language at once
bool anyLanguage = ProfanitySystem.ContainsAny(userInput);

// Full match metadata: index, length, matched (source) text, language
IReadOnlyList<ProfanityMatch> matches = ProfanitySystem.FindAll(userInput, ProfanityLanguage.English);
```

### AllowList (Scunthorpe guard)

Words added to the AllowList are never flagged even when they contain a banned substring.

```csharp
// AllowList is live — add any time, even before loading.
ProfanitySystem.AllowList.Add("classic");
ProfanitySystem.AllowList.Add("scunthorpe");
// Or ship allow words directly in your data JSON/ScriptableObject — see "Data format" below.
```

### Multiple named sources

```csharp
// Merge a built-in list with a remote/live one — both are loaded and merged on LoadAsync.
ProfanitySystem.AddSource("builtin", new ResourcesJsonProfanityLoader(), "ProfanityData");
ProfanitySystem.AddSource("remote",  new MyRemoteLoader(),               "https://.../words.json");
await ProfanitySystem.LoadAsync();
```

### Custom replacement character

```csharp
ProfanitySystem.ReplacementCharacter = '#';
```

### Swapping the filter engine at runtime

```csharp
// Basic mode: fast, exact word-boundary regex, no obfuscation normalization.
ProfanitySystem.Default = new ProfanityManager(new RegexProfanityFilter());
ProfanitySystem.AddSource("builtin", new ResourcesJsonProfanityLoader(), "ProfanityData");
await ProfanitySystem.LoadAsync();
```

## Data format (JSON)

Place a `.json` file in a `Resources/` folder and reference its path (without extension) in
`ProfanitySettings` or via `AddSource`:

```json
{
  "replacementCharacter": "*",
  "languages": [
    { "language": "English", "words": ["badword1", "badword2"] },
    { "language": "Thai",    "words": ["คำหยาบ1", "คำหยาบ2"] }
  ],
  "allowWords": ["scunthorpe", "classic", "assassin"]
}
```

The package's built-in default (`Runtime/Resources/ProfanityData.json`) ships with a
production-scale English list (scatological, sexual, and general-insult categories, ~100 entries
with common variants like `fucker`/`fucking`/`motherfucker`) and a Thai list (~35 entries), plus a
matching allowlist of common false-positive words (`class`, `assassin`, `cockpit`, `attitude`,
`arsenal`, etc.). Real production word lists should still live in **your** project — the shipped
list is a solid starting point, not an exhaustive moderation dictionary.

## Settings asset

Create a **ProfanitySettings** asset via **KidzDev → Profanity → Create Settings** and place it at
`Resources/ProfanitySettings.asset`. Configure:

| Field | Default | Description |
| --- | --- | --- |
| Loader Type | `Json` | `Json` = JSON TextAsset; `Asset` = `ProfanityDataSet` ScriptableObject |
| Data Path | `Profanity/ProfanityData` | Resources path (no extension) |
| Match Mode | `Normalized` | `Normalized` = obfuscation-aware; `Basic` = fast regex |
| Replacement Character | `*` | Character used to replace flagged text |
| Collapse Repeats | `true` | Fold repeated chars (`shiiit` → `shit`) |
| Strip Separators | `true` | Remove separators (`s.h.i.t` → `shit`) |

## How obfuscation-aware matching works

`TextNormalizer` runs before matching (when `MatchMode.Normalized` is active):

1. **Lowercasing** — invariant casing.
2. **Leetspeak fold** — `@`/`4`→`a`, `3`→`e`, `1`/`|`→`i`, `0`→`o`, `5`/`$`→`s`, `7`→`t`, `8`→`b`, `9`→`g`.
3. **Separator strip** — `.`, `-`, `_`, `*`, `+`, `,`, `/`, `\`, and zero-width characters are
   removed between letters (spaces are intentionally *not* stripped — removing them would merge
   separate words and break word-boundary detection).
4. **Repeat collapse** — runs of the same character collapse to one (`shiiit` → `shit`).

Every normalized position is mapped back to its exact source index, so `Censor` replaces only the
real offending characters — never more, never less — even after normalization changed the string
length.

## Samples

| Sample | Description |
| --- | --- |
| **Demo** | Ready-to-run 1920×1080 scene: EN+TH word lists loaded from Resources, live censor input field, an Obfuscation-Aware Mode toggle to compare `Normalized` vs `Basic` filtering in real time |
| **Addressables Loader** | `IProfanityDataLoader` backed by Unity Addressables — load word-list JSON from remote with label `profanity` |

Import samples via **Package Manager → KidzDev Unity Profanity → Samples**.

## License

MIT — see [LICENSE.md](LICENSE.md).
