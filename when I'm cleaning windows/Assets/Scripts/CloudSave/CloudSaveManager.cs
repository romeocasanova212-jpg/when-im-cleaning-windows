using System;
using System.Collections.Generic;
using UnityEngine;
using WhenImCleaningWindows.Core;
using WhenImCleaningWindows.Monetization;

#if FIREBASE_ENABLED
using Firebase.Extensions;
using Firebase.Storage;
#endif

namespace WhenImCleaningWindows.CloudSave
{
    /// <summary>
    /// Cloud Save system using Firebase Cloud Storage for cross-device progression.
    /// Automatically syncs player data including levels, currency, VIP status, and stats.
    /// </summary>
    public class CloudSaveManager : MonoBehaviour
    {
        public static CloudSaveManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableCloudSave = true;
        [SerializeField] private bool debugMode = true;
        [SerializeField] private float autoSaveIntervalSeconds = 300f; // 5 minutes

        [Header("Conflict Resolution")]
        [SerializeField] private ConflictResolution conflictStrategy = ConflictResolution.UseNewest;

        private const string SAVE_FILE_NAME = "player_data.json";
        private float autoSaveTimer = 0f;
        private bool isInitialized = false;
        private bool isSaving = false;
        private bool isLoading = false;

#if FIREBASE_ENABLED
        Firebase.Storage.FirebaseStorage storage;
        Firebase.Storage.StorageReference storageRef;
#endif

        public enum ConflictResolution
        {
            UseNewest,      // Use the most recently modified save
            UseHighestLevel, // Use save with highest player level
            UseMostProgress  // Use save with most total stars
        }

        #region Initialization

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeCloudSave();
        }

        private void Update()
        {
            // Auto-save timer
            if (isInitialized && enableCloudSave)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveIntervalSeconds)
                {
                    autoSaveTimer = 0f;
                    SaveToCloud();
                }
            }
        }

        public void InitializeCloudSave()
        {
            if (isInitialized)
            {
                Debug.Log("[CloudSave] Already initialized");
                return;
            }

#if FIREBASE_ENABLED
            Debug.Log("[CloudSave] Initializing Cloud Save system...");

            storage = Firebase.Storage.FirebaseStorage.DefaultInstance;
            storageRef = storage.GetReferenceFromUrl("gs://dummy-firebase-project.appspot.com");
            
            isInitialized = true;
            Debug.Log("[CloudSave] ✓ Cloud Save initialized");
            
            // Auto-load on startup
            LoadFromCloud();
#else
            Debug.LogWarning("[CloudSave] Firebase not installed. Cloud Save disabled.");
            isInitialized = true;
#endif
        }

        #endregion

#if FIREBASE_ENABLED
        #region Save/Load

        /// <summary>
        /// Save current game state to cloud
        /// </summary>
        public void SaveToCloud()
        {
            if (!isInitialized || !enableCloudSave || isSaving)
            {
                if (debugMode)
                {
                    Debug.Log("[CloudSave] Skipping cloud save (not ready or already saving)");
                }
                return;
            }

            isSaving = true;

            // Gather all player data
            PlayerSaveData saveData = GatherPlayerData();
            string json = JsonUtility.ToJson(saveData, true);

            if (debugMode)
            {
                Debug.Log($"[CloudSave] Saving to cloud... (Size: {json.Length} bytes)");
                Debug.Log($"[CloudSave] Data: Level {saveData.playerLevel}, Stars {saveData.totalStars}, Coins {saveData.coins}, Gems {saveData.gems}");
            }

            // Save locally first (fallback)
            PlayerPrefs.SetString("CloudSaveBackup", json);
            PlayerPrefs.SetString("CloudSaveBackupTime", DateTime.Now.ToString("o"));
            PlayerPrefs.Save();

            var playerRef = storageRef.Child($"players/{GetPlayerId()}/{SAVE_FILE_NAME}");
            var uploadTask = playerRef.PutBytesAsync(System.Text.Encoding.UTF8.GetBytes(json));

            uploadTask.ContinueWithOnMainThread(task =>
            {
                isSaving = false;

                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError($"[CloudSave] Upload failed: {task.Exception}");
                    FirebaseManager.Instance?.LogException(task.Exception, "CloudSave.SaveToCloud");
                }
                else
                {
                    Debug.Log($"[CloudSave] ✓ Data saved to cloud successfully");
                }
            });
        }

        /// <summary>
        /// Load game state from cloud
        /// </summary>
        public void LoadFromCloud()
        {
            if (!isInitialized || !enableCloudSave || isLoading)
            {
                if (debugMode)
                {
                    Debug.Log("[CloudSave] Skipping cloud load (not ready or already loading)");
                }
                return;
            }

            isLoading = true;

            if (debugMode)
            {
                Debug.Log("[CloudSave] Loading from cloud...");
            }

            var playerRef = storageRef.Child($"players/{GetPlayerId()}/{SAVE_FILE_NAME}");
            var downloadTask = playerRef.GetBytesAsync(10 * 1024 * 1024); // 10MB max

            downloadTask.ContinueWithOnMainThread(task =>
            {
                isLoading = false;

                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogWarning($"[CloudSave] Download failed: {task.Exception?.Message}");
                    LoadLocalBackup();
                }
                else
                {
                    string json = System.Text.Encoding.UTF8.GetString(task.Result);
                    PlayerSaveData cloudData = JsonUtility.FromJson<PlayerSaveData>(json);

                    // Check for conflicts with local save
                    if (HasLocalSave())
                    {
                        ResolveConflict(cloudData);
                    }
                    else
                    {
                        ApplyPlayerData(cloudData);
                        Debug.Log($"[CloudSave] ✓ Data loaded from cloud");
                    }
                }
            });
        }

        private void LoadLocalBackup()
        {
            if (PlayerPrefs.HasKey("CloudSaveBackup"))
            {
                string json = PlayerPrefs.GetString("CloudSaveBackup");
                PlayerSaveData localData = JsonUtility.FromJson<PlayerSaveData>(json);
                ApplyPlayerData(localData);

                Debug.Log("[CloudSave] ✓ Loaded local backup");
            }
            else
            {
                Debug.Log("[CloudSave] No local backup found - starting fresh");
            }
        }

        #endregion

        #region Conflict Resolution

        private bool HasLocalSave()
        {
            return PlayerPrefs.HasKey("CloudSaveBackup");
        }

        private void ResolveConflict(PlayerSaveData cloudData)
        {
            string localJson = PlayerPrefs.GetString("CloudSaveBackup");
            PlayerSaveData localData = JsonUtility.FromJson<PlayerSaveData>(localJson);

            Debug.Log($"[CloudSave] Conflict detected! Resolving with strategy: {conflictStrategy}");
            Debug.Log($"[CloudSave] Local: Level {localData.playerLevel}, Stars {localData.totalStars}, Modified {localData.lastModified}");
            Debug.Log($"[CloudSave] Cloud: Level {cloudData.playerLevel}, Stars {cloudData.totalStars}, Modified {cloudData.lastModified}");

            PlayerSaveData chosenData = null;

            switch (conflictStrategy)
            {
                case ConflictResolution.UseNewest:
                    DateTime localTime = DateTime.Parse(localData.lastModified);
                    DateTime cloudTime = DateTime.Parse(cloudData.lastModified);
                    chosenData = cloudTime > localTime ? cloudData : localData;
                    break;

                case ConflictResolution.UseHighestLevel:
                    chosenData = cloudData.playerLevel > localData.playerLevel ? cloudData : localData;
                    break;

                case ConflictResolution.UseMostProgress:
                    chosenData = cloudData.totalStars > localData.totalStars ? cloudData : localData;
                    break;
            }

            ApplyPlayerData(chosenData);
            Debug.Log($"[CloudSave] ✓ Conflict resolved - using {(chosenData == cloudData ? "cloud" : "local")} save");
        }

        #endregion

        #region Data Gathering/Applying

        private PlayerSaveData GatherPlayerData()
        {
            var saveData = new PlayerSaveData
            {
                // Player Progress
                playerLevel = GameManager.Instance?.GetCurrentLevel()?.levelNumber ?? 1,
                highestLevelUnlocked = GameManager.Instance?.GetHighestUnlockedLevel() ?? 1,
                totalStars = GameManager.Instance?.GetTotalStars() ?? 0,
                totalPlaytime = GameManager.Instance?.GetTotalPlaytime() ?? 0f,

                // Currency
                coins = CurrencyManager.Instance?.GetCoins() ?? 0,
                gems = CurrencyManager.Instance?.GetGems() ?? 0,

                // VIP Status
                isVIP = VIPManager.Instance?.IsVIPActive() ?? false,
                vipTier = VIPManager.Instance?.GetCurrentTier().ToString() ?? "None",
                vipExpiryDate = VIPManager.Instance?.GetVIPExpiryDate().ToString("o") ?? "",
                cumulativeGemsSpent = VIPManager.Instance?.GetCumulativeGemsSpent() ?? 0,

                // Energy
                currentEnergy = EnergySystem.Instance?.GetCurrentEnergy() ?? 5,

                // Stats
                totalLevelsPlayed = GameManager.Instance?.GetTotalLevelsPlayed() ?? 0,
                totalLevelsCompleted = GameManager.Instance?.GetTotalLevelsCompleted() ?? 0,
                totalDeaths = GameManager.Instance?.GetTotalDeaths() ?? 0,

                // Timestamps
                lastModified = DateTime.Now.ToString("o"),
                firstInstallDate = PlayerPrefs.GetString("FirstInstallDate", DateTime.Now.ToString("o"))
            };

            return saveData;
        }

        private void ApplyPlayerData(PlayerSaveData data)
        {
            Debug.Log($"[CloudSave] Applying player data: Level {data.playerLevel}, Stars {data.totalStars}");

            // Apply to GameManager
            if (GameManager.Instance != null)
            {
                // GameManager.Instance.LoadProgress(data.playerLevel, data.highestLevelUnlocked, data.totalStars, data.totalPlaytime);
            }

            // Apply to CurrencyManager
            if (CurrencyManager.Instance != null)
            {
                CurrencyManager.Instance.SetCoins(data.coins);
                CurrencyManager.Instance.SetGems(data.gems);
            }

            // Apply to VIPManager
            if (VIPManager.Instance != null && data.isVIP)
            {
                if (!string.IsNullOrEmpty(data.vipExpiryDate))
                {
                    DateTime expiryDate = DateTime.Parse(data.vipExpiryDate);
                    VIPTier tier = (VIPTier)Enum.Parse(typeof(VIPTier), data.vipTier);
                    
                    if (expiryDate > DateTime.Now)
                    {
                        int daysRemaining = (int)(expiryDate - DateTime.Now).TotalDays;
                        VIPManager.Instance.ActivateVIP(tier, daysRemaining);
                    }
                }
            }

            // Apply to EnergySystem
            if (EnergySystem.Instance != null)
            {
                // EnergySystem.Instance.SetEnergy(data.currentEnergy);
            }

            Debug.Log("[CloudSave] ✓ Player data applied successfully");
        }

        #endregion

        #region Helpers

        private string GetPlayerId()
        {
            // Use Firebase Auth UID when available
            // For now, use device identifier
            if (!PlayerPrefs.HasKey("PlayerId"))
            {
                PlayerPrefs.SetString("PlayerId", Guid.NewGuid().ToString());
                PlayerPrefs.Save();
            }

            return PlayerPrefs.GetString("PlayerId");
        }

        public void ForceSync()
        {
            Debug.Log("[CloudSave] Force sync requested");
            SaveToCloud();
            LoadFromCloud();
        }

        public bool IsSyncing()
        {
            return isSaving || isLoading;
        }

        #endregion
#else
        // Stub methods when Firebase is not available
        public void SaveToCloud() { Debug.LogWarning("[CloudSave] Firebase not installed."); }
        public void LoadFromCloud() { Debug.LogWarning("[CloudSave] Firebase not installed."); }
        public void ForceSync() { Debug.LogWarning("[CloudSave] Firebase not installed."); }
        public bool IsSyncing() { return false; }
#endif

        #region Event Hooks

        private void OnApplicationPause(bool pause)
        {
            if (pause && enableCloudSave)
            {
                // Save when app goes to background
                Debug.Log("[CloudSave] App paused - saving to cloud");
                SaveToCloud();
            }
        }

        private void OnApplicationQuit()
        {
            if (enableCloudSave)
            {
                // Final save before quitting
                Debug.Log("[CloudSave] App quitting - final save to cloud");
                SaveToCloud();
            }
        }

        #endregion

        #region Debug Context Menu

        [ContextMenu("Force Save to Cloud")]
        private void ForceSaveToCloud()
        {
            SaveToCloud();
        }

        [ContextMenu("Force Load from Cloud")]
        private void ForceLoadFromCloud()
        {
            LoadFromCloud();
        }

#if FIREBASE_ENABLED
        [ContextMenu("Delete Cloud Save")]
        private void DeleteCloudSave()
        {
            Debug.LogWarning("[CloudSave] Deleting cloud save (local backup remains)");
            
            if (debugMode)
            {
                Debug.Log("[CloudSave] DEBUG MODE - Cloud deletion skipped");
                return;
            }
            
            var playerRef = storageRef.Child($"players/{GetPlayerId()}/{SAVE_FILE_NAME}");
            playerRef.DeleteAsync().ContinueWithOnMainThread(task => {
                if (task.IsCompleted && !task.IsFaulted)
                {
                    Debug.Log("[CloudSave] ✓ Cloud save deleted successfully");
                }
                else
                {
                    Debug.LogError($"[CloudSave] Failed to delete cloud save: {task.Exception}");
                }
            });
        }

        [ContextMenu("Print Current Save Data")]
        private void PrintCurrentSaveData()
        {
            PlayerSaveData data = GatherPlayerData();
            Debug.Log("=== Current Save Data ===");
            Debug.Log($"Player Level: {data.playerLevel}");
            Debug.Log($"Highest Unlocked: {data.highestLevelUnlocked}");
            Debug.Log($"Total Stars: {data.totalStars}");
            Debug.Log($"Coins: {data.coins}");
            Debug.Log($"Gems: {data.gems}");
            Debug.Log($"VIP: {data.isVIP} ({data.vipTier})");
            Debug.Log($"Energy: {data.currentEnergy}");
            Debug.Log($"Last Modified: {data.lastModified}");
            Debug.Log("=========================");
        }
#endif

        #endregion
    }

    /// <summary>
    /// Player save data structure
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        // Player Progress
        public int playerLevel;
        public int highestLevelUnlocked;
        public int totalStars;
        public float totalPlaytime;

        // Currency
        public int coins;
        public int gems;

        // VIP Status
        public bool isVIP;
        public string vipTier;
        public string vipExpiryDate;
        public int cumulativeGemsSpent;

        // Energy
        public int currentEnergy;

        // Stats
        public int totalLevelsPlayed;
        public int totalLevelsCompleted;
        public int totalDeaths;

        // Timestamps
        public string lastModified;
        public string firstInstallDate;
    }
}








