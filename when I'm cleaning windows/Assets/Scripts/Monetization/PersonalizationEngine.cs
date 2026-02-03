using UnityEngine;
using System;
using System.Collections.Generic;

namespace WhenImCleaningWindows.Monetization
{
    /// <summary>
    /// Player profile for churn prediction.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        public int daysPlayed = 0;
        public int sessionsToday = 0;
        public int levelsCompleted = 0;
        public int deathsTotal = 0;
        public float avgSessionLength = 0f;  // minutes
        public float lifetimeSpend = 0f;
        public int daysSinceLastPurchase = 999;
        public int daysSinceInstall = 0;
        public DateTime lastSessionDate;
        public DateTime installDate;
        
        // Churn signals
        public bool hasSeenWelcomePack = false;
        public bool hasSeenD3Offer = false;
        public bool hasSeenD7Offer = false;
        public bool hasSeenD14Offer = false;
        public bool hasSeenD30Offer = false;
        
        // Engagement metrics
        public int consecutiveDeaths = 0;
        public float avgLevelRetries = 0f;
        public int hardLevelsSkipped = 0;
        public float lastChurnScore = 0f;
    }
    
    /// <summary>
    /// Offer trigger definition.
    /// </summary>
    [Serializable]
    public class OfferTrigger
    {
        public string offerId;
        public string productId;
        public int dayTrigger;  // D1, D3, D7, etc.
        public float churnThreshold;  // 0-1
        public float discountPercentage;  // 0-70%
        public string urgencyMessage;
        public float showRate;  // % of players who see it
    }
    
    /// <summary>
    /// ML Personalization Engine (2026 Matrix).
    /// Predicts churn and triggers dynamic offers at D1/D3/D7/D14/D30.
    /// Uses lightweight on-device ML (no Firebase dependency in prototype).
    /// </summary>
    public class PersonalizationEngine : MonoBehaviour
    {
        public static PersonalizationEngine Instance { get; private set; }
        
        [Header("Player Profile")]
        [SerializeField] private PlayerProfile profile = new PlayerProfile();
        
        [Header("ML Configuration")]
        [SerializeField] private bool enableML = true;
        [SerializeField] private bool enableDynamicOffers = true;
        [SerializeField] private float churnThreshold = 0.65f;  // Per GDD: "if churn_risk > 0.65"
        
        [Header("Offer Triggers")]
        [SerializeField] private List<OfferTrigger> triggers = new List<OfferTrigger>();
        
        // Events
        public static event Action<OfferTrigger> OnOfferTriggered;
        public static event Action<float> OnChurnScoreUpdated;
        
        // Managers
        private IAPManager iapManager;
        private VIPManager vipManager;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadPlayerProfile();
            InitializeTriggers();
        }
        
        private void Start()
        {
            iapManager = IAPManager.Instance;
            vipManager = VIPManager.Instance;
            
            UpdateDaysPlayed();
            CheckDailyOffers();
        }
        
        /// <summary>
        /// Initialize offer triggers (D1/D3/D7/D14/D30 matrix).
        /// </summary>
        private void InitializeTriggers()
        {
            triggers.Clear();
            
            // D1: Welcome Pack (85% show rate)
            triggers.Add(new OfferTrigger
            {
                offerId = "welcome_pack_d1",
                productId = "com.cleaningwindows.welcomepack",
                dayTrigger = 1,
                churnThreshold = 0f,  // Always show
                discountPercentage = 0f,
                urgencyMessage = "WELCOME! 100% BONUS!",
                showRate = 0.85f
            });
            
            // D3: Progression Wall (churn > 0.5)
            triggers.Add(new OfferTrigger
            {
                offerId = "progression_pack_d3",
                productId = "com.cleaningwindows.progressionpack",
                dayTrigger = 3,
                churnThreshold = 0.5f,
                discountPercentage = 30f,
                urgencyMessage = "Stuck? This will help! -30%",
                showRate = 0.65f
            });
            
            // D7: VIP Trial (churn > 0.4)
            triggers.Add(new OfferTrigger
            {
                offerId = "vip_trial_d7",
                productId = "com.cleaningwindows.rookiebundle",  // VIP 3-day trial included
                dayTrigger = 7,
                churnThreshold = 0.4f,
                discountPercentage = 50f,
                urgencyMessage = "Try VIP FREE for 3 days!",
                showRate = 0.50f
            });
            
            // D14: Win-Back (churn > 0.65)
            triggers.Add(new OfferTrigger
            {
                offerId = "winback_d14",
                productId = "com.cleaningwindows.gempile",
                dayTrigger = 14,
                churnThreshold = 0.65f,
                discountPercentage = 70f,
                urgencyMessage = "We miss you! Come back! -70%",
                showRate = 1.0f  // 100% show if churn risk high
            });
            
            // D30: Loyalty Reward (churn < 0.3, spend > £20)
            triggers.Add(new OfferTrigger
            {
                offerId = "loyalty_d30",
                productId = "com.cleaningwindows.gemchest",
                dayTrigger = 30,
                churnThreshold = 0f,  // Low churn = loyal
                discountPercentage = 40f,
                urgencyMessage = "Thank you! Exclusive reward -40%",
                showRate = 1.0f
            });
            
            Debug.Log($"Initialized {triggers.Count} offer triggers");
        }
        
        /// <summary>
        /// Calculate churn risk score (0-1).
        /// Lightweight ML model (no Firebase in prototype).
        /// </summary>
        public float CalculateChurnScore()
        {
            if (!enableML) return 0f;
            
            float score = 0f;
            
            // Factor 1: Session frequency (40% weight)
            float sessionScore = 0f;
            if (profile.sessionsToday == 0) sessionScore = 0.8f;
            else if (profile.sessionsToday < 2) sessionScore = 0.5f;
            else sessionScore = 0.1f;
            score += sessionScore * 0.4f;
            
            // Factor 2: Death rate (30% weight)
            float deathRate = profile.levelsCompleted > 0 ? (float)profile.deathsTotal / profile.levelsCompleted : 0f;
            float deathScore = Mathf.Clamp01(deathRate * 0.5f);  // >2 deaths per level = high churn
            score += deathScore * 0.3f;
            
            // Factor 3: Session length (20% weight)
            float lengthScore = 0f;
            if (profile.avgSessionLength < 5f) lengthScore = 0.7f;  // <5 min = high churn
            else if (profile.avgSessionLength < 15f) lengthScore = 0.3f;
            else lengthScore = 0f;
            score += lengthScore * 0.2f;
            
            // Factor 4: Consecutive deaths (10% weight)
            float frustrationScore = Mathf.Clamp01(profile.consecutiveDeaths / 5f);  // 5+ deaths = frustrated
            score += frustrationScore * 0.1f;
            
            // Clamp to 0-1
            score = Mathf.Clamp01(score);
            
            profile.lastChurnScore = score;
            OnChurnScoreUpdated?.Invoke(score);
            
            return score;
        }
        
        /// <summary>
        /// Check if any offers should trigger today.
        /// </summary>
        private void CheckDailyOffers()
        {
            int daysPlayed = profile.daysSinceInstall;
            float churnScore = CalculateChurnScore();
            
            foreach (OfferTrigger trigger in triggers)
            {
                // Check day trigger
                if (daysPlayed != trigger.dayTrigger) continue;
                
                // Check if already seen
                if (HasSeenOffer(trigger.offerId)) continue;
                
                // Check churn threshold
                if (churnScore < trigger.churnThreshold)
                {
                    // Special case: D30 loyalty (show to LOW churn + high spenders)
                    if (trigger.dayTrigger == 30)
                    {
                        if (profile.lifetimeSpend < 20f) continue;
                    }
                    else
                    {
                        continue;
                    }
                }
                
                // Check show rate (random roll)
                if (UnityEngine.Random.value > trigger.showRate) continue;
                
                // TRIGGER OFFER!
                TriggerOffer(trigger);
            }
        }
        
        /// <summary>
        /// Trigger an offer popup.
        /// </summary>
        private void TriggerOffer(OfferTrigger trigger)
        {
            MarkOfferSeen(trigger.offerId);
            
            OnOfferTriggered?.Invoke(trigger);
            
            Debug.Log($"OFFER TRIGGERED: {trigger.offerId} | {trigger.urgencyMessage}");
            
            // In production: Show UI popup with purchase button
            // OfferPopup.Show(trigger);
        }
        
        /// <summary>
        /// Mark offer as seen.
        /// </summary>
        private void MarkOfferSeen(string offerId)
        {
            switch (offerId)
            {
                case "welcome_pack_d1": profile.hasSeenWelcomePack = true; break;
                case "progression_pack_d3": profile.hasSeenD3Offer = true; break;
                case "vip_trial_d7": profile.hasSeenD7Offer = true; break;
                case "winback_d14": profile.hasSeenD14Offer = true; break;
                case "loyalty_d30": profile.hasSeenD30Offer = true; break;
            }
            SavePlayerProfile();
        }
        
        /// <summary>
        /// Has player seen this offer?
        /// </summary>
        private bool HasSeenOffer(string offerId)
        {
            switch (offerId)
            {
                case "welcome_pack_d1": return profile.hasSeenWelcomePack;
                case "progression_pack_d3": return profile.hasSeenD3Offer;
                case "vip_trial_d7": return profile.hasSeenD7Offer;
                case "winback_d14": return profile.hasSeenD14Offer;
                case "loyalty_d30": return profile.hasSeenD30Offer;
                default: return false;
            }
        }
        
        /// <summary>
        /// Update days played (call once per session).
        /// </summary>
        private void UpdateDaysPlayed()
        {
            DateTime now = DateTime.Now;
            
            // First install
            if (profile.installDate == DateTime.MinValue)
            {
                profile.installDate = now;
                profile.lastSessionDate = now;
                profile.daysSinceInstall = 0;
                profile.daysPlayed = 1;
            }
            else
            {
                // Calculate days since install
                TimeSpan timeSinceInstall = now - profile.installDate;
                profile.daysSinceInstall = (int)timeSinceInstall.TotalDays;
                
                // Check if new day
                if (now.Date > profile.lastSessionDate.Date)
                {
                    profile.daysPlayed++;
                    profile.sessionsToday = 0;
                }
            }
            
            profile.sessionsToday++;
            profile.lastSessionDate = now;
            
            SavePlayerProfile();
        }
        
        /// <summary>
        /// Track level completion (for churn model).
        /// </summary>
        public void OnLevelComplete(int stars, int retries)
        {
            profile.levelsCompleted++;
            profile.consecutiveDeaths = 0;
            
            // Update avg retries
            float totalRetries = profile.avgLevelRetries * (profile.levelsCompleted - 1) + retries;
            profile.avgLevelRetries = totalRetries / profile.levelsCompleted;
            
            SavePlayerProfile();
        }
        
        /// <summary>
        /// Track level failure (for churn model).
        /// </summary>
        public void OnLevelFailed()
        {
            profile.deathsTotal++;
            profile.consecutiveDeaths++;
            
            // Check if frustrated (5+ consecutive deaths = trigger emergency offer)
            if (profile.consecutiveDeaths >= 5)
            {
                TriggerEmergencyOffer();
            }
            
            SavePlayerProfile();
        }
        
        /// <summary>
        /// Trigger emergency offer (player frustrated).
        /// </summary>
        private void TriggerEmergencyOffer()
        {
            Debug.Log("EMERGENCY OFFER: Player frustrated! Offering power-up bundle");
            
            // In production: Show power-up bundle popup
            // OfferPopup.ShowEmergency("Struggling? Try this!", powerUpBundle);
        }
        
        /// <summary>
        /// Track session duration.
        /// </summary>
        public void OnSessionEnd(float sessionLengthMinutes)
        {
            // Update rolling average
            int totalSessions = profile.sessionsToday + (profile.daysPlayed - 1) * 3;  // Estimate
            float totalMinutes = profile.avgSessionLength * totalSessions;
            profile.avgSessionLength = (totalMinutes + sessionLengthMinutes) / (totalSessions + 1);
            
            SavePlayerProfile();
        }
        
        /// <summary>
        /// Get player profile (for display).
        /// </summary>
        public PlayerProfile GetProfile()
        {
            return profile;
        }
        
        /// <summary>
        /// Save player profile to PlayerPrefs.
        /// </summary>
        private void SavePlayerProfile()
        {
            PlayerPrefs.SetInt("Profile_DaysPlayed", profile.daysPlayed);
            PlayerPrefs.SetInt("Profile_SessionsToday", profile.sessionsToday);
            PlayerPrefs.SetInt("Profile_LevelsCompleted", profile.levelsCompleted);
            PlayerPrefs.SetInt("Profile_Deaths", profile.deathsTotal);
            PlayerPrefs.SetFloat("Profile_AvgSession", profile.avgSessionLength);
            PlayerPrefs.SetFloat("Profile_LifetimeSpend", profile.lifetimeSpend);
            PlayerPrefs.SetInt("Profile_DaysSinceInstall", profile.daysSinceInstall);
            PlayerPrefs.SetInt("Profile_ConsecutiveDeaths", profile.consecutiveDeaths);
            PlayerPrefs.SetFloat("Profile_AvgRetries", profile.avgLevelRetries);
            PlayerPrefs.SetString("Profile_InstallDate", profile.installDate.ToString());
            PlayerPrefs.SetString("Profile_LastSession", profile.lastSessionDate.ToString());
            
            // Offer flags
            PlayerPrefs.SetInt("Offer_WelcomePack", profile.hasSeenWelcomePack ? 1 : 0);
            PlayerPrefs.SetInt("Offer_D3", profile.hasSeenD3Offer ? 1 : 0);
            PlayerPrefs.SetInt("Offer_D7", profile.hasSeenD7Offer ? 1 : 0);
            PlayerPrefs.SetInt("Offer_D14", profile.hasSeenD14Offer ? 1 : 0);
            PlayerPrefs.SetInt("Offer_D30", profile.hasSeenD30Offer ? 1 : 0);
            
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load player profile from PlayerPrefs.
        /// </summary>
        private void LoadPlayerProfile()
        {
            profile.daysPlayed = PlayerPrefs.GetInt("Profile_DaysPlayed", 0);
            profile.sessionsToday = PlayerPrefs.GetInt("Profile_SessionsToday", 0);
            profile.levelsCompleted = PlayerPrefs.GetInt("Profile_LevelsCompleted", 0);
            profile.deathsTotal = PlayerPrefs.GetInt("Profile_Deaths", 0);
            profile.avgSessionLength = PlayerPrefs.GetFloat("Profile_AvgSession", 0f);
            profile.lifetimeSpend = PlayerPrefs.GetFloat("Profile_LifetimeSpend", 0f);
            profile.daysSinceInstall = PlayerPrefs.GetInt("Profile_DaysSinceInstall", 0);
            profile.consecutiveDeaths = PlayerPrefs.GetInt("Profile_ConsecutiveDeaths", 0);
            profile.avgLevelRetries = PlayerPrefs.GetFloat("Profile_AvgRetries", 0f);
            
            string installDateStr = PlayerPrefs.GetString("Profile_InstallDate", "");
            if (!string.IsNullOrEmpty(installDateStr) && DateTime.TryParse(installDateStr, out DateTime installDate))
            {
                profile.installDate = installDate;
            }
            
            string lastSessionStr = PlayerPrefs.GetString("Profile_LastSession", "");
            if (!string.IsNullOrEmpty(lastSessionStr) && DateTime.TryParse(lastSessionStr, out DateTime lastSession))
            {
                profile.lastSessionDate = lastSession;
            }
            
            // Offer flags
            profile.hasSeenWelcomePack = PlayerPrefs.GetInt("Offer_WelcomePack", 0) == 1;
            profile.hasSeenD3Offer = PlayerPrefs.GetInt("Offer_D3", 0) == 1;
            profile.hasSeenD7Offer = PlayerPrefs.GetInt("Offer_D7", 0) == 1;
            profile.hasSeenD14Offer = PlayerPrefs.GetInt("Offer_D14", 0) == 1;
            profile.hasSeenD30Offer = PlayerPrefs.GetInt("Offer_D30", 0) == 1;
            
            Debug.Log($"Profile loaded: Days {profile.daysPlayed}, Levels {profile.levelsCompleted}, Churn {profile.lastChurnScore:F2}");
        }
        
        // === DEBUG FUNCTIONS ===
        
        [ContextMenu("Calculate Churn Score")]
        public void DEBUG_CalculateChurn()
        {
            float score = CalculateChurnScore();
            Debug.Log($"Churn Score: {score:F2} (Threshold: {churnThreshold})");
        }
        
        [ContextMenu("Trigger D1 Welcome Pack")]
        public void DEBUG_TriggerD1()
        {
            profile.daysSinceInstall = 1;
            CheckDailyOffers();
        }
        
        [ContextMenu("Simulate 5 Consecutive Deaths")]
        public void DEBUG_SimulateFrustration()
        {
            for (int i = 0; i < 5; i++)
            {
                OnLevelFailed();
            }
        }
        
        [ContextMenu("Reset Player Profile")]
        public void DEBUG_ResetProfile()
        {
            profile = new PlayerProfile();
            SavePlayerProfile();
            Debug.Log("Profile reset");
        }
    }
}








