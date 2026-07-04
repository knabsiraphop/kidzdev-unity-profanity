using UnityEditor;
using UnityEngine;

namespace KidzDev.Unity.Profanity.Editor {
    [CustomEditor(typeof(ProfanityDataSet))]
    sealed class ProfanityDataSetEditor : UnityEditor.Editor {
        SerializedProperty _replacementCharacter;
        SerializedProperty _languages;
        SerializedProperty _allowWords;

        void OnEnable() {
            _replacementCharacter = serializedObject.FindProperty("_replacementCharacter");
            _languages            = serializedObject.FindProperty("_languages");
            _allowWords           = serializedObject.FindProperty("_allowWords");
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_replacementCharacter);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Word Lists", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_languages, includeChildren: true);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Allow Words (Scunthorpe guard)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_allowWords, includeChildren: true);

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(
                "Word lists stay in your project — only the loader ships with the package.",
                MessageType.Info);
        }
    }
}
