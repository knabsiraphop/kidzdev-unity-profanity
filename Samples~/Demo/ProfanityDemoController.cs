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

        bool _ready;

        IEnumerator Start() {
            _statusText.text = "Loading…";

            ProfanitySystem.AddSource("demo", new ResourcesJsonProfanityLoader(), "ProfanityData");
            yield return ProfanitySystem.LoadAsync(destroyCancellationToken).ToCoroutine();

            _ready           = true;
            _statusText.text = "Ready";
            _filterEnButton.onClick.AddListener(() => Filter(ProfanityLanguage.English));
            _filterThButton.onClick.AddListener(() => Filter(ProfanityLanguage.Thai));
            _inputField.onValueChanged.AddListener(_ => Filter(ProfanityLanguage.English));
        }

        void OnDestroy() => ProfanitySystem.Release();

        void Filter(ProfanityLanguage lang) {
            if (!_ready) return;
            var raw     = _inputField.text;
            var censored = ProfanitySystem.Censor(raw, lang);
            var matches  = ProfanitySystem.FindAll(raw, lang);
            _outputText.text = censored;
            _statusText.text = matches.Count == 0
                ? "Clean"
                : $"Found {matches.Count} match(es): {string.Join(", ", System.Linq.Enumerable.Select(matches, m => m.Value))}";
        }
    }
}
