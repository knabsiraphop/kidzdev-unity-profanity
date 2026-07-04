# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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
