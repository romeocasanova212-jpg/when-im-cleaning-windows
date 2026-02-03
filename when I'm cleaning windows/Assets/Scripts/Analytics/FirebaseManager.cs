using System;
using System.Collections.Generic;
using UnityEngine;
// using WhenImCleaningWindows.Debug;

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Analytics;
using Firebase.Crashlytics;
#endif

namespace WhenImCleaningWindows.Analytics
{
    /// <summary>
    /// Central Firebase integration for analytics, crashlytics, and ML Kit.
    /// Supports production tracking for all game events and player behavior.
    /// </summary>
    public class FirebaseManager : MonoBehaviour
    {
        public static FirebaseManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableAnalytics = true;
        [SerializeField] private bool enableCrashlytics = true;
        [SerializeField] private bool debugMode = true;

        private bool isInitialized = false;
#if FIREBASE_ENABLED
        private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
#endif

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
            InitializeFirebase();
        }

        public void InitializeFirebase()
        {
            if (isInitialized)
            {
                UnityEngine.Debug.Log("[Firebase] Already initialized");
                return;
            }

#if FIREBASE_ENABLED
            UnityEngine.Debug.Log("[Firebase] Initializing Firebase SDK...");

            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                dependencyStatus = task.Result;

                if (dependencyStatus == DependencyStatus.Available)
                {
                    InitializeFirebaseServices();
                }
                else
                {
                    UnityEngine.Debug.LogError($"[Firebase] Could not resolve all Firebase dependencies: {dependencyStatus}");
                }
            });
#else
            UnityEngine.Debug.LogWarning("[Firebase] Firebase SDK not installed. Skipping initialization.");
            isInitialized = true;
#endif
        }

#if FIREBASE_ENABLED
        private void InitializeFirebaseServices()
        {
            UnityEngine.Debug.Log("[Firebase] Dependencies resolved, initializing services...");

            // Analytics
            if (enableAnalytics)
            {
                Firebase.Analytics.FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                Firebase.Analytics.FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(0, 30, 0));
                UnityEngine.Debug.Log("[Firebase] ✓ Analytics enabled");
            }

            // Crashlytics
            if (enableCrashlytics)
            {
                Firebase.Crashlytics.Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                UnityEngine.Debug.Log("[Firebase] ✓ Crashlytics enabled");
            }

            isInitialized = true;
            UnityEngine.Debug.Log("[Firebase] Initialization complete!");
        }
#endif

        #endregion

#if FIREBASE_ENABLED
        #region Analytics Events

        /// <summary>
        /// Track level start event
        /// </summary>
        public void LogLevelStart(int levelNumber, string worldName, int playerLevel)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: level_start (Level: {levelNumber}, World: {worldName}, PlayerLevel: {playerLevel})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("level_start", new Parameter[]
            {
                new Parameter("level_number", levelNumber),
                new Parameter("world_name", worldName),
                new Parameter("player_level", playerLevel)
            });
        }

        /// <summary>
        /// Track level complete event
        /// </summary>
        public void LogLevelComplete(int levelNumber, int stars, float timeRemaining, int coinsEarned, int gemsEarned, bool elegant)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: level_complete (Level: {levelNumber}, Stars: {stars}, Time: {timeRemaining:F1}s, Coins: {coinsEarned}, Gems: {gemsEarned}, Elegant: {elegant})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLevelEnd, new Parameter[]
            {
                new Parameter(FirebaseAnalytics.ParameterLevelName, $"Level_{levelNumber}"),
                new Parameter(FirebaseAnalytics.ParameterSuccess, 1),
                new Parameter("stars", stars),
                new Parameter("time_remaining", timeRemaining),
                new Parameter("coins_earned", coinsEarned),
                new Parameter("gems_earned", gemsEarned),
                new Parameter("elegant", elegant ? 1 : 0)
            });
        }

        /// <summary>
        /// Track level fail event
        /// </summary>
        public void LogLevelFail(int levelNumber, float cleanPercentage, int deathCount, string failureReason)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: level_fail (Level: {levelNumber}, Clean: {cleanPercentage:F1}%, Deaths: {deathCount}, Reason: {failureReason})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("level_fail", new Parameter[]
            {
                new Parameter("level_number", levelNumber),
                new Parameter("clean_percentage", cleanPercentage),
                new Parameter("death_count", deathCount),
                new Parameter("failure_reason", failureReason)
            });
        }

        /// <summary>
        /// Track IAP purchase event
        /// </summary>
        public void LogPurchase(string productId, string productType, decimal price, string currency)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: purchase (Product: {productId}, Type: {productType}, Price: {currency}{price})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventPurchase, new Parameter[]
            {
                new Parameter(FirebaseAnalytics.ParameterItemId, productId),
                new Parameter(FirebaseAnalytics.ParameterItemName, productId),
                new Parameter(FirebaseAnalytics.ParameterItemCategory, productType),
                new Parameter(FirebaseAnalytics.ParameterPrice, (double)price),
                new Parameter(FirebaseAnalytics.ParameterCurrency, currency),
                new Parameter(FirebaseAnalytics.ParameterValue, (double)price)
            });
        }

        /// <summary>
        /// Track VIP subscription activation
        /// </summary>
        public void LogVIPActivation(string tier, int durationDays, decimal price)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: vip_activation (Tier: {tier}, Duration: {durationDays} days, Price: ${price})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("vip_activation", new Parameter[]
            {
                new Parameter("tier", tier),
                new Parameter("duration_days", durationDays),
                new Parameter("price", (double)price)
            });
        }

        /// <summary>
        /// Track dynamic offer shown
        /// </summary>
        public void LogOfferShown(string offerType, string triggerDay, string productId, decimal price, int discountPercent)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: offer_shown (Type: {offerType}, Day: {triggerDay}, Product: {productId}, Price: ${price}, Discount: {discountPercent}%)");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("offer_shown", new Parameter[]
            {
                new Parameter("offer_type", offerType),
                new Parameter("trigger_day", triggerDay),
                new Parameter("product_id", productId),
                new Parameter("price", (double)price),
                new Parameter("discount_percent", discountPercent)
            });
        }

        /// <summary>
        /// Track dynamic offer dismissed
        /// </summary>
        public void LogOfferDismissed(string offerType, string triggerDay)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: offer_dismissed (Type: {offerType}, Day: {triggerDay})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("offer_dismissed", new Parameter[]
            {
                new Parameter("offer_type", offerType),
                new Parameter("trigger_day", triggerDay)
            });
        }

        /// <summary>
        /// Track energy refill (gems or ad)
        /// </summary>
        public void LogEnergyRefill(string refillMethod, int energyGained, int gemsCost)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: energy_refill (Method: {refillMethod}, Energy: +{energyGained}, Gems: -{gemsCost})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("energy_refill", new Parameter[]
            {
                new Parameter("refill_method", refillMethod),
                new Parameter("energy_gained", energyGained),
                new Parameter("gems_cost", gemsCost)
            });
        }

        /// <summary>
        /// Track power-up usage
        /// </summary>
        public void LogPowerUpUsed(string powerUpType, int levelNumber, int gemsCost)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: powerup_used (Type: {powerUpType}, Level: {levelNumber}, Cost: {gemsCost} gems)");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("powerup_used", new Parameter[]
            {
                new Parameter("powerup_type", powerUpType),
                new Parameter("level_number", levelNumber),
                new Parameter("gems_cost", gemsCost)
            });
        }

        /// <summary>
        /// Track churn prediction score
        /// </summary>
        public void LogChurnScore(float churnScore, string riskCategory, int daysSinceInstall)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: churn_score (Score: {churnScore:F2}, Risk: {riskCategory}, DaysSinceInstall: {daysSinceInstall})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("churn_score", new Parameter[]
            {
                new Parameter("churn_score", churnScore),
                new Parameter("risk_category", riskCategory),
                new Parameter("days_since_install", daysSinceInstall)
            });
        }

        /// <summary>
        /// Track session length
        /// </summary>
        public void LogSessionEnd(float sessionDuration, int levelsPlayed, int levelsCompleted)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Event: session_end (Duration: {sessionDuration:F0}s, Played: {levelsPlayed}, Completed: {levelsCompleted})");
            }

            Firebase.Analytics.FirebaseAnalytics.LogEvent("session_end", new Parameter[]
            {
                new Parameter("session_duration", sessionDuration),
                new Parameter("levels_played", levelsPlayed),
                new Parameter("levels_completed", levelsCompleted)
            });
        }

        #endregion

        #region User Properties

        /// <summary>
        /// Set user properties for analytics segmentation
        /// </summary>
        public void SetUserProperties(int playerLevel, bool isVIP, string vipTier, int totalPurchases, decimal lifetimeValue)
        {
            if (!IsAvailable()) return;

            if (debugMode)
            {
                UnityEngine.Debug.Log($"[Firebase] Setting user properties (Level: {playerLevel}, VIP: {isVIP}, Tier: {vipTier}, Purchases: {totalPurchases}, LTV: ${lifetimeValue})");
            }

            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("player_level", playerLevel.ToString());
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("is_vip", isVIP ? "true" : "false");
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("vip_tier", vipTier);
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("total_purchases", totalPurchases.ToString());
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("lifetime_value", lifetimeValue.ToString("F2"));
        }

        /// <summary>
        /// Update player level property
        /// </summary>
        public void UpdatePlayerLevel(int level)
        {
            if (!IsAvailable()) return;

            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("player_level", level.ToString());
        }

        /// <summary>
        /// Update VIP status
        /// </summary>
        public void UpdateVIPStatus(bool isVIP, string tier)
        {
            if (!IsAvailable()) return;

            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("is_vip", isVIP ? "true" : "false");
            Firebase.Analytics.FirebaseAnalytics.SetUserProperty("vip_tier", tier);
        }

        #endregion

        #region Crashlytics

        /// <summary>
        /// Log custom exception to Crashlytics
        /// </summary>
        public void LogException(Exception exception, string context)
        {
            if (!IsAvailable()) return;

            UnityEngine.Debug.LogError($"[Firebase] Exception in {context}: {exception.Message}");

            Firebase.Crashlytics.Crashlytics.LogException(exception);
            Firebase.Crashlytics.Crashlytics.UnityEngine.Debug.Log($"Context: {context}");
        }

        /// <summary>
        /// Set custom key for crash reports
        /// </summary>
        public void SetCrashlyticsKey(string key, string value)
        {
            if (!IsAvailable()) return;

            Firebase.Crashlytics.Crashlytics.SetCustomKey(key, value);
        }

        /// <summary>
        /// Set user ID for crash attribution
        /// </summary>
        public void SetCrashlyticsUserId(string userId)
        {
            if (!IsAvailable()) return;

            Firebase.Crashlytics.Crashlytics.SetUserId(userId);
        }

        #endregion

        #region Helpers

        private bool IsAvailable()
        {
            if (!isInitialized)
            {
                if (debugMode)
                {
                    UnityEngine.Debug.LogWarning("[Firebase] Not initialized - event skipped");
                }
                return false;
            }

            return enableAnalytics;
        }

        public bool IsReady() => isInitialized;

        #endregion
#else
        // Stub methods when Firebase is not available
        public void LogLevelStart(int levelNumber, string worldName, int playerLevel) { }
        public void LogLevelComplete(int levelNumber, int stars, float timeRemaining, int coinsEarned, int gemsEarned, bool elegant) { }
        public void LogLevelFail(int levelNumber, float cleanPercentage, int deathCount, string failureReason) { }
        public void LogPurchase(string productId, string productType, decimal price, string currency) { }
        public void LogVIPActivation(string tier, int durationDays, decimal price) { }
        public void LogOfferShown(string offerType, string triggerDay, string productId, decimal price, int discountPercent) { }
        public void LogOfferDismissed(string offerType, string triggerDay) { }
        public void LogEnergyRefill(string refillMethod, int energyGained, int gemsCost) { }
        public void LogPowerUpUsed(string powerUpType, int levelNumber, int gemsCost) { }
        public void LogChurnScore(float churnScore, string riskCategory, int daysSinceInstall) { }
        public void LogSessionEnd(float sessionDuration, int levelsPlayed, int levelsCompleted) { }
        public void SetUserProperties(int playerLevel, bool isVIP, string vipTier, int totalPurchases, decimal lifetimeValue) { }
        public void UpdatePlayerLevel(int level) { }
        public void UpdateVIPStatus(bool isVIP, string tier) { }
        public void LogException(Exception exception, string context) { UnityEngine.Debug.LogError($"[Firebase] Exception in {context}: {exception.Message}"); }
        public void SetCrashlyticsKey(string key, string value) { }
        public void SetCrashlyticsUserId(string userId) { }
        public bool IsReady() => isInitialized;
#endif

        #region Debug Context Menu

        [ContextMenu("Test Firebase Events")]
        private void TestFirebaseEvents()
        {
#if FIREBASE_ENABLED
            UnityEngine.Debug.Log("=== Testing Firebase Events ===");
            
            LogLevelStart(1, "Suburban Homes", 5);
            LogLevelComplete(1, 3, 50f, 120, 5, true);
            LogPurchase("com.game.welcomepack", "StarterBundle", 0.99m, "USD");
            LogVIPActivation("Bronze", 30, 4.99m);
            LogOfferShown("D1_Welcome", "D1", "com.game.welcomepack", 0.99m, 50);
            LogChurnScore(0.35f, "Low", 3);
            
            UnityEngine.Debug.Log("=== Firebase Test Complete ===");
#else
            UnityEngine.Debug.LogWarning("Firebase SDK not installed. Test skipped.");
#endif
        }

        #endregion
    }
}








