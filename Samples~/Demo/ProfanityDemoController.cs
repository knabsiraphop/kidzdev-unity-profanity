using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KidzDev.Unity.Profanity.Samples {
    public sealed class ProfanityDemoController : MonoBehaviour {
        [SerializeField] TMP_InputField _inputField;
        [SerializeField] TMP_Text       _outputText;
        [SerializeField] TMP_Text       _statusText;
        [SerializeField] Button         _filterEnButton;
        [SerializeField] Button         _filterThButton;
        [SerializeField] Toggle         _normalizedToggle;

        bool             _ready;
        ProfanityLanguage _currentLang = ProfanityLanguage.English;

        IEnumerator Start() {
            _statusText.text = "Loading…";

            ProfanitySystem.AddSource("demo", new ResourcesJsonProfanityLoader(), "ProfanityData");
            yield return ProfanitySystem.LoadAsync(destroyCancellationToken).ToCoroutine();

            _ready           = true;
            _statusText.text = "Ready — type to filter";

            _filterEnButton.onClick.AddListener(() => { _currentLang = ProfanityLanguage.English; Filter(); });
            _filterThButton.onClick.AddListener(() => { _currentLang = ProfanityLanguage.Thai;    Filter(); });
            _inputField.onValueChanged.AddListener(_ => Filter());

            _normalizedToggle.isOn = true;
            _normalizedToggle.onValueChanged.AddListener(OnModeToggled);
        }

        void OnDestroy() => ProfanitySystem.Release();

        void OnModeToggled(bool normalized) {
            if (!_ready) return;
            _statusText.text = "Reloading…";
            ReloadWithMode(normalized).Forget();
        }

        async UniTaskVoid ReloadWithMode(bool normalized) {
            IProfanityFilter filter = normalized
                ? new NormalizedProfanityFilter()
                : (IProfanityFilter)new RegexProfanityFilter();

            ProfanitySystem.Default = new ProfanityManager(filter);
            ProfanitySystem.AddSource("demo", new ResourcesJsonProfanityLoader(), "ProfanityData");
            await ProfanitySystem.LoadAsync(destroyCancellationToken);

            _ready           = true;
            _statusText.text = normalized ? "Mode: Normalized (obfuscation-aware)" : "Mode: Basic (exact match)";
            Filter();
        }

        void Filter() {
            if (!_ready) return;
            var raw      = _inputField.text;
            var censored = ProfanitySystem.Censor(raw, _currentLang);
            var matches  = ProfanitySystem.FindAll(raw, _currentLang);
            _outputText.text = censored;
            _statusText.text = matches.Count == 0
                ? "Clean"
                : $"Found {matches.Count} match(es): {string.Join(", ", System.Linq.Enumerable.Select(matches, m => m.Value))}";
        }
    }
}
