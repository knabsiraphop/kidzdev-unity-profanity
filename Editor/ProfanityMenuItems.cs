using UnityEditor;
using UnityEngine;

namespace KidzDev.Unity.Profanity.Editor {
    static class ProfanityMenuItems {
        [MenuItem("KidzDev/Profanity/Create Settings")]
        static void CreateSettings() => CreateAsset<ProfanitySettings>("ProfanitySettings");

        [MenuItem("KidzDev/Profanity/Create Data Set")]
        static void CreateDataSet() => CreateAsset<ProfanityDataSet>("ProfanityDataSet");

        static void CreateAsset<T>(string defaultName) where T : ScriptableObject {
            var asset = ScriptableObject.CreateInstance<T>();
            var path  = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path)) path = "Assets";
            else if (!System.IO.Directory.Exists(path)) path = System.IO.Path.GetDirectoryName(path);
            path = AssetDatabase.GenerateUniqueAssetPath($"{path}/{defaultName}.asset");
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}
