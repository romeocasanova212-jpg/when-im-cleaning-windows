using UnityEditor;
using UnityEngine;
using System.IO;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Editor
{
    /// <summary>
    /// Creates default ScriptableObject config assets under Resources/Config.
    /// </summary>
    public static class ConfigAssetCreator
    {
        private const string ResourcesRoot = "Assets/Resources";
        private const string ConfigFolder = "Assets/Resources/Config";

        [MenuItem("Tools/When I'm Cleaning Windows/Create Default Config Assets")]
        public static void CreateDefaultConfigAssets()
        {
            EnsureFolders();

            CreateIfMissing<GameConfig>("GameConfig");
            CreateIfMissing<LevelConfig>("LevelConfig");
            CreateIfMissing<AudioConfig>("AudioConfig");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Config Assets Ready",
                "Default config assets created (if missing) in Assets/Resources/Config.\n\n" +
                "You can now edit GameConfig, LevelConfig, and AudioConfig in the Project view.",
                "OK"
            );
        }

        private static void EnsureFolders()
        {
            if (!Directory.Exists(ResourcesRoot))
            {
                Directory.CreateDirectory(ResourcesRoot);
            }

            if (!Directory.Exists(ConfigFolder))
            {
                Directory.CreateDirectory(ConfigFolder);
            }
        }

        private static void CreateIfMissing<T>(string assetName) where T : ScriptableObject
        {
            string assetPath = Path.Combine(ConfigFolder, $"{assetName}.asset").Replace("\\", "/");

            T existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null) return;

            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, assetPath);
            UnityEngine.Debug.Log($"[ConfigAssetCreator] Created {assetName} at {assetPath}");
        }
    }
}
