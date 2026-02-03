using UnityEngine;
using System;
using WhenImCleaningWindows.Monetization;
using WhenImCleaningWindows.Procedural;
using WhenImCleaningWindows.Mechanics;
using WhenImCleaningWindows.Gameplay;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Core
{
    /// <summary>
    /// Game state enumeration.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        LevelSelect,
        Playing,
        Paused,
        LevelComplete,
        LevelFailed,
        Shop,
        Settings
    }
    
    /// <summary>
    /// Player progression data.
    /// </summary>
    [Serializable]
    public class PlayerProgress
    {
        public int currentLevel = 1;
        public int highestLevelUnlocked = 1;
        public int totalStars = 0;
        public int perfectLevels = 0;  // 3-star count
        public float totalPlayTime = 0f;  // minutes
        
        // World progress
        public int currentWorld = 1;
        public int currentFloor = 1;
        public int currentRoom = 1;
        
        // Stats
        public int totalLevelsCompleted = 0;
        public int totalSwipes = 0;
        public int totalCircleScrubs = 0;
        public int totalPowerUpsUsed = 0;
        
        // Current level stats
        public int currentLevelRetries = 0;
        
        // Achievements
        public bool[] achievements = new bool[50];  // 50 achievements
    }
    
    /// <summary>
    /// Level completion result.
    /// </summary>
    public class LevelResult
    {
        public int levelNumber;
        public int stars;  // 0-3
        public float timeRemaining;
        public float cleanPercentage;
        public int swipesUsed;
        public bool wasElegant;
        public int coinsEarned;
        public int gemsEarned;
    }
    
    /// <summary>
    /// Game Manager - Central controller for all systems.
    /// Manages game state, player progression, and system integration.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        
        [Header("Game State")]
        [SerializeField] private GameState currentState = GameState.MainMenu;
        [SerializeField] private PlayerProgress playerProgress = new PlayerProgress();
        
        [Header("Current Level")]
        [SerializeField] private LevelData currentLevel;
        [SerializeField] private float levelStartTime;
        [SerializeField] private bool isLevelActive = false;
        
        // Events
        public static event Action<GameState> OnGameStateChanged;
        public static event Action<LevelData> OnLevelStarted;
        public static event Action<LevelResult> OnLevelCompleted;
        public static event Action OnLevelFailed;
        public static event Action<int> OnLevelUnlocked;
        
        // System references
        private EnergySystem energySystem;
        private CurrencyManager currencyManager;
        private IAPManager iapManager;
        private VIPManager vipManager;
        private PersonalizationEngine personalizationEngine;
        private LevelGenerator levelGenerator;
        private HazardSystem hazardSystem;
        private TimerSystem timerSystem;
        private CleaningController cleaningController;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadPlayerProgress();
        }
        
        private void Start()
        {
            // Defer initialization to next frame to ensure all singletons are created
            StartCoroutine(DeferredInitialization());
        }
        
        private System.Collections.IEnumerator DeferredInitialization()
        {
            // Wait one frame for Bootstrapper to complete initialization
            yield return null;
            
            InitializeSystems();
            ChangeState(GameState.MainMenu);
        }
        
        private void Update()
        {
            // Track play time
            if (currentState == GameState.Playing)
            {
                playerProgress.totalPlayTime += Time.deltaTime / 60f;  // Convert to minutes
            }
            
            // Check level completion during gameplay
            if (isLevelActive && currentState == GameState.Playing && cleaningController != null)
            {
                float cleanPercentage = cleaningController.GetCurrentCleanPercentage();
                
                float threshold = ConfigProvider.GameConfig != null
                    ? ConfigProvider.GameConfig.levelCompleteThreshold
                    : 95f;

                // Level complete when threshold or more clean
                if (cleanPercentage >= threshold)
                {
                    CompleteLevel(cleanPercentage, 0, false);
                }
            }
        }
        
        /// <summary>
        /// Initialize all game systems.
        /// </summary>
        private void InitializeSystems()
        {
            energySystem = EnergySystem.Instance;
            currencyManager = CurrencyManager.Instance;
            iapManager = IAPManager.Instance;
            vipManager = VIPManager.Instance;
            personalizationEngine = PersonalizationEngine.Instance;
            levelGenerator = LevelGenerator.Instance;
            hazardSystem = HazardSystem.Instance;
            timerSystem = FindFirstObjectByType<TimerSystem>();
            cleaningController = FindFirstObjectByType<CleaningController>();
            
            // Subscribe to system events
            if (timerSystem != null)
            {
                timerSystem.OnTimerEnd.AddListener(OnTimerExpired);
            }
            
            UnityEngine.Debug.Log("GameManager: All systems initialized");
        }
        
        /// <summary>
        /// Change game state.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            currentState = newState;
            OnGameStateChanged?.Invoke(newState);
            
            UnityEngine.Debug.Log($"Game State: {newState}");
        }
        
        /// <summary>
        /// Start a level (checks energy, loads level data).
        /// </summary>
        public bool StartLevel(int levelNumber)
        {
            // Check if level is unlocked
            if (levelNumber > playerProgress.highestLevelUnlocked)
            {
                UnityEngine.Debug.Log("Level locked!");
                return false;
            }
            
            // Check energy (unless VIP)
            if (!vipManager.HasUnlimitedEnergy())
            {
                if (!energySystem.TryConsumeEnergy())
                {
                    UnityEngine.Debug.Log("Not enough energy!");
                    // In production: Show energy refill popup
                    return false;
                }
            }
            
            // Generate level
            currentLevel = levelGenerator.GenerateLevel(levelNumber);
            
            // Reset retry counter when starting a new level (not retrying same level)
            if (playerProgress.currentLevel != levelNumber)
            {
                playerProgress.currentLevelRetries = 0;
            }
            
            playerProgress.currentLevel = levelNumber;
            
            // Update world/floor/room
            playerProgress.currentWorld = currentLevel.worldNumber;
            playerProgress.currentFloor = currentLevel.floorNumber;
            playerProgress.currentRoom = currentLevel.roomNumber;
            
            // Start timer
            if (timerSystem != null)
            {
                timerSystem.SetWorldLevel(currentLevel.worldNumber, currentLevel.levelNumber);
                timerSystem.StartTimer();
            }
            
            // Start cleaning controller
            if (cleaningController != null)
            {
                cleaningController.StartLevel(currentLevel);
            }
            
            levelStartTime = Time.time;
            isLevelActive = true;
            
            OnLevelStarted?.Invoke(currentLevel);
            ChangeState(GameState.Playing);
            
            UnityEngine.Debug.Log($"Level {levelNumber} started! ({currentLevel.hazardCount} hazards, {currentLevel.timerSeconds}s)");
            
            return true;
        }
        
        /// <summary>
        /// Complete level (calculate stars, rewards).
        /// </summary>
        public void CompleteLevel(float cleanPercentage, int swipesUsed, bool wasElegant)
        {
            if (!isLevelActive) return;
            
            isLevelActive = false;
            
            // Get actual clean percentage from gameplay
            if (cleaningController != null)
            {
                cleanPercentage = cleaningController.GetCurrentCleanPercentage();
            }
            
            float timeElapsed = Time.time - levelStartTime;
            float timeRemaining = currentLevel.timerSeconds - timeElapsed;
            
            // Calculate stars (based on time remaining %)
            int stars = CalculateStars(timeRemaining, currentLevel.timerSeconds);
            
            // Create result
            LevelResult result = new LevelResult
            {
                levelNumber = currentLevel.levelNumber,
                stars = stars,
                timeRemaining = timeRemaining,
                cleanPercentage = cleanPercentage,
                swipesUsed = swipesUsed,
                wasElegant = wasElegant
            };
            
            // Award rewards
            float timeBonus = timeRemaining / currentLevel.timerSeconds;
            float eleganceBonus = wasElegant ? 1.0f : 0f;
            bool isVIP = vipManager.IsVIPActive();
            
            currencyManager.AwardLevelRewards(stars, timeBonus, eleganceBonus, isVIP);
            
            GameConfig gameConfig = ConfigProvider.GameConfig;
            int baseCoins = gameConfig != null ? gameConfig.baseCoinsPerLevel : 5;
            int coinsPerStar = gameConfig != null ? gameConfig.coinsPerStar : 5;
            float gemChance = gameConfig != null ? gameConfig.gemDropChance : 0.05f;
            int minGem = gameConfig != null ? gameConfig.minGemDrop : 2;
            int maxGem = gameConfig != null ? gameConfig.maxGemDrop : 10;

            result.coinsEarned = baseCoins + (stars * coinsPerStar);
            result.gemsEarned = (stars == 3 && UnityEngine.Random.value < gemChance)
                ? UnityEngine.Random.Range(minGem, maxGem + 1)
                : 0;
            
            // Update progress
            playerProgress.totalLevelsCompleted++;
            playerProgress.totalStars += stars;
            if (stars == 3) playerProgress.perfectLevels++;
            playerProgress.totalSwipes += swipesUsed;
            
            // Unlock next level
            int nextLevel = currentLevel.levelNumber + 1;
            if (nextLevel > playerProgress.highestLevelUnlocked)
            {
                playerProgress.highestLevelUnlocked = nextLevel;
                OnLevelUnlocked?.Invoke(nextLevel);
            }
            
            // Track for ML personalization
            personalizationEngine.OnLevelComplete(stars, playerProgress.currentLevelRetries);
            
            SavePlayerProgress();
            
            OnLevelCompleted?.Invoke(result);
            ChangeState(GameState.LevelComplete);
            
            UnityEngine.Debug.Log($"Level {currentLevel.levelNumber} complete! {stars} stars, {result.coinsEarned} coins");
        }
        
        /// <summary>
        /// Fail level (out of time or energy).
        /// </summary>
        public void FailLevel()
        {
            if (!isLevelActive) return;
            
            isLevelActive = false;
            
            // Track for ML personalization
            personalizationEngine.OnLevelFailed();
            
            OnLevelFailed?.Invoke();
            ChangeState(GameState.LevelFailed);
            
            UnityEngine.Debug.Log($"Level {currentLevel.levelNumber} failed");
        }
        
        /// <summary>
        /// Calculate stars based on time remaining percentage.
        /// </summary>
        private int CalculateStars(float timeRemaining, float totalTime)
        {
            float percentage = (timeRemaining / totalTime) * 100f;

            LevelConfig levelConfig = ConfigProvider.LevelConfig;
            float twoStarThreshold = levelConfig != null ? levelConfig.twoStar_TimePercent : 25f;
            float threeStarThreshold = levelConfig != null ? levelConfig.threeStar_TimePercent : 50f;

            if (percentage >= threeStarThreshold) return 3;
            if (percentage >= twoStarThreshold) return 2;
            if (percentage > 0f) return 1;
            return 0;
        }
        
        /// <summary>
        /// Timer expired callback.
        /// </summary>
        private void OnTimerExpired()
        {
            FailLevel();
        }
        
        /// <summary>
        /// Retry current level.
        /// </summary>
        public void RetryLevel()
        {
            playerProgress.currentLevelRetries++;
            StartLevel(currentLevel.levelNumber);
        }
        
        /// <summary>
        /// Go to next level.
        /// </summary>
        public void NextLevel()
        {
            int nextLevelNum = currentLevel.levelNumber + 1;
            if (nextLevelNum <= playerProgress.highestLevelUnlocked)
            {
                StartLevel(nextLevelNum);
            }
        }
        
        /// <summary>
        /// Get player progress.
        /// </summary>
        public PlayerProgress GetProgress()
        {
            return playerProgress;
        }
        
        /// <summary>
        /// Get current level data.
        /// </summary>
        public LevelData GetCurrentLevel()
        {
            return currentLevel;
        }
        
        /// <summary>
        /// Get game state.
        /// </summary>
        public GameState GetState()
        {
            return currentState;
        }
        
        /// <summary>
        /// Save player progress to PlayerPrefs.
        /// </summary>
        private void SavePlayerProgress()
        {
            PlayerPrefs.SetInt("Progress_CurrentLevel", playerProgress.currentLevel);
            PlayerPrefs.SetInt("Progress_HighestUnlocked", playerProgress.highestLevelUnlocked);
            PlayerPrefs.SetInt("Progress_TotalStars", playerProgress.totalStars);
            PlayerPrefs.SetInt("Progress_PerfectLevels", playerProgress.perfectLevels);
            PlayerPrefs.SetFloat("Progress_PlayTime", playerProgress.totalPlayTime);
            PlayerPrefs.SetInt("Progress_LevelsCompleted", playerProgress.totalLevelsCompleted);
            PlayerPrefs.SetInt("Progress_Swipes", playerProgress.totalSwipes);
            PlayerPrefs.Save();
            
            UnityEngine.Debug.Log($"Progress saved: Level {playerProgress.currentLevel}, {playerProgress.totalStars} stars");
        }
        
        /// <summary>
        /// Load player progress from PlayerPrefs.
        /// </summary>
        private void LoadPlayerProgress()
        {
            playerProgress.currentLevel = PlayerPrefs.GetInt("Progress_CurrentLevel", 1);
            playerProgress.highestLevelUnlocked = PlayerPrefs.GetInt("Progress_HighestUnlocked", 1);
            playerProgress.totalStars = PlayerPrefs.GetInt("Progress_TotalStars", 0);
            playerProgress.perfectLevels = PlayerPrefs.GetInt("Progress_PerfectLevels", 0);
            playerProgress.totalPlayTime = PlayerPrefs.GetFloat("Progress_PlayTime", 0f);
            playerProgress.totalLevelsCompleted = PlayerPrefs.GetInt("Progress_LevelsCompleted", 0);
            playerProgress.totalSwipes = PlayerPrefs.GetInt("Progress_Swipes", 0);
            
            UnityEngine.Debug.Log($"Progress loaded: Level {playerProgress.currentLevel}, {playerProgress.totalStars} stars");
        }
        
        /// <summary>
        /// Track play time (call once per second).
        /// </summary>
        
        // === DEBUG FUNCTIONS ===
        
        [ContextMenu("Start Level 1")]
        public void DEBUG_StartLevel1()
        {
            StartLevel(1);
        }
        
        [ContextMenu("Complete Current Level (3 Stars)")]
        public void DEBUG_CompleteLevel()
        {
            CompleteLevel(100f, 50, true);
        }
        
        [ContextMenu("Unlock All Levels")]
        public void DEBUG_UnlockAll()
        {
            playerProgress.highestLevelUnlocked = 10000;
            SavePlayerProgress();
            UnityEngine.Debug.Log("All levels unlocked!");
        }
        
        [ContextMenu("Reset Progress")]
        public void DEBUG_ResetProgress()
        {
            playerProgress = new PlayerProgress();
            SavePlayerProgress();
            UnityEngine.Debug.Log("Progress reset!");
        }
        
        public int GetHighestUnlockedLevel() => playerProgress.highestLevelUnlocked;
        public int GetTotalStars() => playerProgress.totalStars;
        public float GetTotalPlaytime() => playerProgress.totalPlayTime;
        public int GetTotalLevelsPlayed() => playerProgress.totalLevelsCompleted;
        public int GetTotalLevelsCompleted() => playerProgress.totalLevelsCompleted;
        public int GetTotalDeaths() => 0;
    }
}








