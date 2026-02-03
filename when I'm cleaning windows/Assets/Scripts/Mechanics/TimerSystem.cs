using UnityEngine;
using UnityEngine.Events;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Mechanics
{
    /// <summary>
    /// Level timer system with dynamic scaling (120s → 40s across difficulty curve).
    /// Manages countdown, time bonuses, and time-based events.
    /// </summary>
    public class TimerSystem : MonoBehaviour
    {
        [Header("Timer Settings")]
        [SerializeField] private float baseTimeLimit = 120f; // Starting time for World 1
        [SerializeField] private float minTimeLimit = 40f; // Final time for World 10
        [SerializeField] private int totalWorldCount = 10;
        [SerializeField] private int currentWorld = 1;
        [SerializeField] private int currentLevel = 1;
        
        [Header("Bonus Time")]
        [SerializeField] private float bucketPowerUpBonus = 10f;
        [SerializeField] private float perfectClearBonus = 5f;
        
        [Header("UI")]
        [SerializeField] private bool showDebugTimer = true;
        
        // Events
        public UnityEvent OnTimerStart;
        public UnityEvent OnTimerEnd;
        public UnityEvent<float> OnTimerUpdate; // Passes remaining time
        public UnityEvent OnTimeWarning; // Triggered at 10 seconds
        
        private float currentTimeRemaining;
        private float currentTimeLimit;
        private bool isTimerRunning = false;
        private bool warningTriggered = false;
        
        private void Start()
        {
            ApplyConfig();
            CalculateTimeLimit();
        }

        private void ApplyConfig()
        {
            LevelConfig config = ConfigProvider.LevelConfig;
            if (config == null) return;

            baseTimeLimit = config.startingTimer;
            minTimeLimit = config.endingTimer;
            totalWorldCount = config.totalWorlds;
        }
        
        private void Update()
        {
            if (isTimerRunning)
            {
                UpdateTimer(Time.deltaTime);
            }
        }
        
        /// <summary>
        /// Calculate time limit based on world/level progression.
        /// </summary>
        private void CalculateTimeLimit()
        {
            // Linear scaling from 120s → 40s across worlds
            float progressRatio = (currentWorld - 1) / (float)(totalWorldCount - 1);
            currentTimeLimit = Mathf.Lerp(baseTimeLimit, minTimeLimit, progressRatio);
            
            // Add slight variation per level (±5s)
            float levelVariation = Mathf.Sin(currentLevel * 0.5f) * 5f;
            currentTimeLimit += levelVariation;
            
            currentTimeLimit = Mathf.Max(minTimeLimit, currentTimeLimit);
        }
        
        /// <summary>
        /// Start the level timer.
        /// </summary>
        public void StartTimer()
        {
            CalculateTimeLimit();
            currentTimeRemaining = currentTimeLimit;
            isTimerRunning = true;
            warningTriggered = false;
            
            OnTimerStart?.Invoke();
            
            if (showDebugTimer)
            {
                Debug.Log($"Timer started: {currentTimeLimit}s for World {currentWorld}, Level {currentLevel}");
            }
        }
        
        /// <summary>
        /// Stop the timer.
        /// </summary>
        public void StopTimer()
        {
            isTimerRunning = false;
            OnTimerEnd?.Invoke();
        }
        
        /// <summary>
        /// Pause the timer.
        /// </summary>
        public void PauseTimer()
        {
            isTimerRunning = false;
        }
        
        /// <summary>
        /// Resume the timer.
        /// </summary>
        public void ResumeTimer()
        {
            isTimerRunning = true;
        }
        
        /// <summary>
        /// Update timer countdown.
        /// </summary>
        private void UpdateTimer(float deltaTime)
        {
            currentTimeRemaining -= deltaTime;
            
            // Check for time warning (10 seconds)
            if (!warningTriggered && currentTimeRemaining <= 10f)
            {
                warningTriggered = true;
                OnTimeWarning?.Invoke();
            }
            
            // Check for time up
            if (currentTimeRemaining <= 0f)
            {
                currentTimeRemaining = 0f;
                StopTimer();
            }
            
            OnTimerUpdate?.Invoke(currentTimeRemaining);
        }
        
        /// <summary>
        /// Add bonus time (power-ups, achievements).
        /// </summary>
        public void AddBonusTime(float seconds)
        {
            currentTimeRemaining += seconds;
            currentTimeRemaining = Mathf.Min(currentTimeRemaining, currentTimeLimit * 1.5f); // Cap at 150%
            
            if (showDebugTimer)
            {
                Debug.Log($"Bonus time added: +{seconds}s");
            }
        }
        
        /// <summary>
        /// Add bucket power-up time bonus.
        /// </summary>
        public void AddBucketBonus()
        {
            AddBonusTime(bucketPowerUpBonus);
        }
        
        /// <summary>
        /// Add perfect clear bonus.
        /// </summary>
        public void AddPerfectClearBonus()
        {
            AddBonusTime(perfectClearBonus);
        }
        
        /// <summary>
        /// Get remaining time.
        /// </summary>
        public float GetRemainingTime()
        {
            return currentTimeRemaining;
        }
        
        /// <summary>
        /// Get remaining time as percentage (0-1).
        /// </summary>
        public float GetRemainingTimePercentage()
        {
            return currentTimeRemaining / currentTimeLimit;
        }
        
        /// <summary>
        /// Get time limit for current level.
        /// </summary>
        public float GetTimeLimit()
        {
            return currentTimeLimit;
        }
        
        /// <summary>
        /// Set current world/level for time calculation.
        /// </summary>
        public void SetWorldLevel(int world, int level)
        {
            currentWorld = world;
            currentLevel = level;
            CalculateTimeLimit();
        }
        
        /// <summary>
        /// Check if timer is running.
        /// </summary>
        public bool IsRunning()
        {
            return isTimerRunning;
        }
        
        /// <summary>
        /// Format time as MM:SS for UI display.
        /// </summary>
        public string GetFormattedTime()
        {
            int minutes = Mathf.FloorToInt(currentTimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(currentTimeRemaining % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        
        /// <summary>
        /// Calculate star rating based on time remaining.
        /// </summary>
        public int CalculateStars()
        {
            float percentage = GetRemainingTimePercentage();
            
            if (percentage >= 0.66f) return 3; // ≥66% time remaining = 3 stars
            if (percentage >= 0.33f) return 2; // ≥33% time remaining = 2 stars
            if (percentage > 0f) return 1;     // >0% time remaining = 1 star
            
            return 0; // Time up = 0 stars (fail)
        }
        
        /// <summary>
        /// Debug visualization.
        /// </summary>
        private void OnGUI()
        {
            if (!Application.isPlaying || !showDebugTimer || !isTimerRunning) return;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 32;
            style.normal.textColor = currentTimeRemaining <= 10f ? Color.red : Color.white;
            
            string timerText = $"Time: {GetFormattedTime()} ({CalculateStars()}★)";
            GUI.Label(new Rect(10, 10, 300, 50), timerText, style);
        }
    }
}








