# KidzDev Unity Profanity

Production-grade multilingual profanity filter for Unity. Obfuscation-aware engine (leetspeak, separators, repeat-collapse), allowlist for false-positive prevention, plug-in data loader (Resources default, optional Addressables adapter). Supports English and Thai out of the box.

## Installation

Open **Package Manager → + → Add package from git URL** and enter:

```
https://github.com/knabsiraphop/kidzdev-unity-profanity.git#v1.0.0
```

Or add to `Packages/manifest.json`:

```json
"com.kidzdev.unity.profanity": "https://github.com/knabsiraphop/kidzdev-unity-profanity.git#v1.0.0"
```

> **UniTask** (`com.cysharp.unitask`) is a required dependency — add the OpenUPM scoped registry if it is not already present.

## Quick start

### 1 — Configure once (e.g. in a bootstrap MonoBehaviour)

```csharp
using KidzDev.Unity.Profanity;

// Auto-loads Resources/ProfanitySettings.asset (or built-in defaults if absent),
// then registers Resources/Profanity/ProfanityData.json as the data source.
ProfanitySystem.Configure();
await ProfanitySystem.LoadAsync(destroyCancellationToken);
```

### 2 — Censor or check user input

```csharp
// Replace profanity with '*'
string clean = ProfanitySystem.Censor(userInput, ProfanityLanguage.English);

// Check only (no allocation)
bool hasProfanity = ProfanitySystem.Contains(userInput, ProfanityLanguage.English);

// Check all registered languages at once
bool anyLanguage = ProfanitySystem.ContainsAny(userInput);

// Find all matches with source positions
IReadOnlyList<ProfanityMatch> matches = ProfanitySystem.FindAll(userInput, ProfanityLanguage.English);
```

### AllowList (Scunthorpe guard)

Words added to the AllowList are never flagged even if they contain a profane substring.

```csharp
// AllowList is live — add any time, even before loading.
ProfanitySystem.AllowList.Add("classic");
ProfanitySystem.AllowList.Add("scunthorpe");
// You can also include allow words directly in your data JSON/ScriptableObject.
```

### Custom replacement character

```csharp
ProfanitySystem.ReplacementCharacter = '#';
```

## Data format (JSON)

Place a `.json` file in a `Resources/` folder and reference its path (without extension) in `ProfanitySettings`:

```json
{
  "replacementCharacter": "*",
  "languages": [
    {
      "language": "English",
      "words": ["badword1", "badword2"]
    },
    {
      "language": "Thai",
      "words": ["คำหยาบ1", "คำหยาบ2"]
    }
  ],
  "allowWords": ["scunthorpe", "classic", "assassin"]
}
```

## Settings asset

Create a **ProfanitySettings** asset via **KidzDev → Profanity → Create Settings** and place it at `Resources/ProfanitySettings.asset`. Configure:

| Field | Default | Description |
| --- | --- | --- |
| Loader Type | `Json` | `Json` = JSON TextAsset; `Asset` = `ProfanityDataSet` ScriptableObject |
| Data Path | `Profanity/ProfanityData` | Resources path (no extension) |
| Match Mode | `Normalized` | `Normalized` = obfuscation-aware; `Basic` = fast regex |
| Replacement Character | `*` | Character used to replace flagged text |
| Collapse Repeats | `true` | Fold repeated chars (`shiiit` → `shit`) |
| Strip Separators | `true` | Remove separators (`s.h.i.t` → `shit`) |

## Samples

| Sample | Description |
| --- | --- |
| **Demo** | Ready-to-run scene: EN+TH word lists loaded from Resources, live censor input field, allowlist demonstration |
| **Addressables Loader** | `IProfanityDataLoader` backed by Unity Addressables — load word-list JSON from remote with label `profanity` |

Import samples via **Package Manager → KidzDev Unity Profanity → Samples**.

## License

MIT — see [LICENSE.md](LICENSE.md).
