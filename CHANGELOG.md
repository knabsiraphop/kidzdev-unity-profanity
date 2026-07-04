# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.1] - 2026-07-04

### Added
- `Runtime/Resources/ProfanityData.json` — built-in default word list (EN + TH); `ProfanitySystem.Configure()` now auto-loads it when no `ProfanitySettings` asset is found.
- Demo sample: `ProfanityDemo.unity` scene (1920×1080 CanvasScaler reference resolution, dark theme).
- Demo controller: Obfuscation-Aware Mode toggle switches between `NormalizedProfanityFilter` and `RegexProfanityFilter` at runtime.
- Expanded EN and TH word lists in `Samples~/Demo/Resources/ProfanityData.json`.

### Fixed
- `KidzDev.Unity.Profanity.Demo.asmdef` missing `Unity.TextMeshPro` reference — `TMP_InputField`/`TMP_Text` compile errors resolved.
- `KidzDev.Unity.Profanity.AddressablesLoader.asmdef` missing `Unity.ResourceManager` reference — `AsyncOperationHandle<T>` compile error resolved.
- `AddressableProfanityDataLoader`: replaced `ToUniTask()` (requires `UniTask.Addressables`) with `WaitUntil(() => handle.IsDone)` — works with base `UniTask` package only.

## [1.0.0] - 2026-07-04

### Added
- `ProfanitySystem` static facade + `ProfanityManager` instance with named data sources.
- `IProfanityFilter` / `IAllowList` / `IProfanityDataLoader` seams for full substitutability.
- `NormalizedProfanityFilter`: obfuscation-aware engine — leetspeak fold, separator strip, repeat-collapse, with index-map projection back to source spans.
- `RegexProfanityFilter`: fast basic per-language word-boundary regex (selectable via `ProfanitySettings.MatchMode`).
- `AllowList`: explicit allow words + false-positive pair guards (Scunthorpe problem prevention).
- `ResourcesJsonProfanityLoader`: default JSON loader via `JsonUtility` (zero extra deps).
- `ResourcesAssetProfanityLoader`: default ScriptableObject `ProfanityDataSet` loader.
- `ProfanitySettings` `ScriptableObject` with `Configure()` auto-load from `Resources`.
- English and Thai language support via own `ProfanityLanguage` enum.
- Optional `Samples~/AddressablesLoader`: `AddressableProfanityDataLoader` for remote word lists.
- Demo sample with live censor input field.
- EditMode and PlayMode test suites.
