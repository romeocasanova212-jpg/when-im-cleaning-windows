using UnityEngine;

namespace WhenImCleaningWindows.Config
{
    /// <summary>
    /// ConfigProvider - Loads ScriptableObject configs from Resources/Config.
    /// Falls back to null if assets are missing.
    /// </summary>
    public static class ConfigProvider
    {
        private const string GameConfigPath = "Config/GameConfig";
        private const string LevelConfigPath = "Config/LevelConfig";
        private const string AudioConfigPath = "Config/AudioConfig";

        private static GameConfig gameConfig;
        private static LevelConfig levelConfig;
        private static AudioConfig audioConfig;

        public static GameConfig GameConfig => gameConfig ??= LoadConfig<GameConfig>(GameConfigPath);
        public static LevelConfig LevelConfig => levelConfig ??= LoadConfig<LevelConfig>(LevelConfigPath);
        public static AudioConfig AudioConfig => audioConfig ??= LoadConfig<AudioConfig>(AudioConfigPath);

        private static T LoadConfig<T>(string resourcesPath) where T : ScriptableObject
        {
            T config = Resources.Load<T>(resourcesPath);
            if (config == null)
            {
                Debug.LogWarning($"[ConfigProvider] Missing config at Resources/{resourcesPath}.asset");
            }
            return config;
        }
    }
}
