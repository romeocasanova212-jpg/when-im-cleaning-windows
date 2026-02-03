using System;
using System.Collections.Generic;
using UnityEngine;
using WhenImCleaningWindows.Monetization;

#if FIREBASE_ENABLED
using Firebase.RemoteConfig;
#endif

namespace WhenImCleaningWindows.Analytics
{
    /// <summary>
    /// Firebase Remote Config integration for live game balancing without app updates.
    /// Enables A/B testing and dynamic parameter tuning for energy, monetization, and difficulty.
    /// </summary>
    public class RemoteConfigManager : MonoBehaviour
    {
        public static RemoteConfigManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableRemoteConfig = true;
        [SerializeField] private bool debugMode = true;
        [SerializeField] private float fetchTimeoutSeconds = 10f;

        // Default values (fallback if Remote Config fails)
        private Dictionary<string, object> defaultValues = new Dictionary<string, object>()
        {
            // Energy System
            { "energy_max_free", 5 },
            { "energy_max_vip", 10 },
            { "energy_regen_minutes", 20 },
            { "energy_refill_gem_cost", 50 },

            // Currency
            { "coin_per_level_base", 10 },
            { "gem_drop_chance", 0.05f },
            { "vip_coin_multiplier_bronze", 2.5f },
            { "vip_coin_multiplier_silver", 3.0f },
            { "vip_coin_multiplier_gold", 4.0f },

            // IAP Pricing
            { "welcome_pack_price", 0.99f },
            { "welcome_pack_discount", 50 },
            { "vip_bronze_price", 4.99f },
            { "vip_silver_price", 9.99f },
            { "vip_gold_price", 19.99f },

            // Churn Prediction
            { "churn_threshold_high", 0.7f },
            { "churn_threshold_medium", 0.4f },
            { "offer_trigger_d1_enabled", true },
            { "offer_trigger_d3_enabled", true },
            { "offer_trigger_d7_enabled", true },

            // Difficulty Scaling
            { "timer_world1_start", 120 },
            { "timer_world1_end", 60 },
            { "hazard_count_min", 8 },
            { "hazard_count_max", 25 },
            { "regen_rate_min", 0.025f },
            { "regen_rate_max", 0.043f },

            // Feature Flags
            { "feature_vip_enabled", true },
            { "feature_powerups_enabled", true },
            { "feature_cosmetics_enabled", false }, // Phase 3
            { "feature_formby_mode_enabled", false } // Phase 4
        };

        private bool isInitialized = false;
        private DateTime lastFetchTime;

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
            InitializeRemoteConfig();
        }

        public void InitializeRemoteConfig()
        {
            if (isInitialized)
            {
                Debug.Log("[RemoteConfig] Already initialized");
                return;
            }

            Debug.Log("[RemoteConfig] Initializing Remote Config...");

#if FIREBASE_ENABLED
            // Set default values
            Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaultValues);

            // Fetch and activate config
            FetchRemoteConfig();
#else
            Debug.LogWarning("[RemoteConfig] Firebase not installed. Using default values only.");
            isInitialized = true;
#endif
        }

        public void FetchRemoteConfig()
        {
            if (!enableRemoteConfig)
            {
                Debug.Log("[RemoteConfig] Remote Config disabled");
                return;
            }

#if FIREBASE_ENABLED
            Debug.Log("[RemoteConfig] Fetching remote config...");

            var fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.FromSeconds(fetchTimeoutSeconds)
            );

            fetchTask.ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
                    
                    if (info.LastFetchStatus == Firebase.RemoteConfig.LastFetchStatus.Success)
                    {
                        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                            .ContinueWithOnMainThread(activateTask =>
                            {
                                Debug.Log($"[RemoteConfig] ✓ Config fetched and activated");
                                isInitialized = true;
                                lastFetchTime = DateTime.Now;
                                OnConfigUpdated();
                            });
                    }
                    else
                    {
                        Debug.LogWarning($"[RemoteConfig] Fetch failed: {info.LastFetchStatus}");
                    }
                }
                else
                {
                    Debug.LogError("[RemoteConfig] Fetch task failed");
                }
            });
#else
            Debug.LogWarning("[RemoteConfig] Firebase not installed. Config fetch skipped.");
#endif
        }

        private void OnConfigUpdated()
        {
            Debug.Log("[RemoteConfig] Configuration updated - notifying systems");
            
            // Notify managers of updated config
            if (EnergySystem.Instance != null)
            {
                // Refresh energy parameters
                Debug.Log("[RemoteConfig] Updated Energy System parameters");
            }

            if (PersonalizationEngine.Instance != null)
            {
                // Refresh churn thresholds
                Debug.Log("[RemoteConfig] Updated Personalization Engine parameters");
            }
        }

        #endregion

        #region Energy Parameters

        public int GetEnergyMaxFree()
        {
            return GetInt("energy_max_free", 5);
        }

        public int GetEnergyMaxVIP()
        {
            return GetInt("energy_max_vip", 10);
        }

        public int GetEnergyRegenMinutes()
        {
            return GetInt("energy_regen_minutes", 20);
        }

        public int GetEnergyRefillGemCost()
        {
            return GetInt("energy_refill_gem_cost", 50);
        }

        #endregion

        #region Currency Parameters

        public int GetCoinPerLevelBase()
        {
            return GetInt("coin_per_level_base", 10);
        }

        public float GetGemDropChance()
        {
            return GetFloat("gem_drop_chance", 0.05f);
        }

        public float GetVIPCoinMultiplier(VIPTier tier)
        {
            switch (tier)
            {
                case VIPTier.Bronze:
                    return GetFloat("vip_coin_multiplier_bronze", 2.5f);
                case VIPTier.Silver:
                    return GetFloat("vip_coin_multiplier_silver", 3.0f);
                case VIPTier.Gold:
                    return GetFloat("vip_coin_multiplier_gold", 4.0f);
                default:
                    return 1.0f;
            }
        }

        #endregion

        #region IAP Pricing

        public float GetWelcomePackPrice()
        {
            return GetFloat("welcome_pack_price", 0.99f);
        }

        public int GetWelcomePackDiscount()
        {
            return GetInt("welcome_pack_discount", 50);
        }

        public float GetVIPPrice(VIPTier tier)
        {
            switch (tier)
            {
                case VIPTier.Bronze:
                    return GetFloat("vip_bronze_price", 4.99f);
                case VIPTier.Silver:
                    return GetFloat("vip_silver_price", 9.99f);
                case VIPTier.Gold:
                    return GetFloat("vip_gold_price", 19.99f);
                default:
                    return 0f;
            }
        }

        #endregion

        #region Churn Prediction

        public float GetChurnThresholdHigh()
        {
            return GetFloat("churn_threshold_high", 0.7f);
        }

        public float GetChurnThresholdMedium()
        {
            return GetFloat("churn_threshold_medium", 0.4f);
        }

        public bool IsOfferTriggerEnabled(string dayKey)
        {
            return GetBool($"offer_trigger_{dayKey.ToLower()}_enabled", true);
        }

        #endregion

        #region Difficulty Scaling

        public int GetTimerWorldStart(int worldNumber)
        {
            return GetInt($"timer_world{worldNumber}_start", 120);
        }

        public int GetTimerWorldEnd(int worldNumber)
        {
            return GetInt($"timer_world{worldNumber}_end", 60);
        }

        public int GetHazardCountMin()
        {
            return GetInt("hazard_count_min", 8);
        }

        public int GetHazardCountMax()
        {
            return GetInt("hazard_count_max", 25);
        }

        public float GetRegenRateMin()
        {
            return GetFloat("regen_rate_min", 0.025f);
        }

        public float GetRegenRateMax()
        {
            return GetFloat("regen_rate_max", 0.043f);
        }

        #endregion

        #region Feature Flags

        public bool IsVIPEnabled()
        {
            return GetBool("feature_vip_enabled", true);
        }

        public bool ArePowerUpsEnabled()
        {
            return GetBool("feature_powerups_enabled", true);
        }

        public bool AreCosmeticsEnabled()
        {
            return GetBool("feature_cosmetics_enabled", false);
        }

        public bool IsFormbyModeEnabled()
        {
            return GetBool("feature_formby_mode_enabled", false);
        }

        #endregion

        #region Generic Getters

        private int GetInt(string key, int defaultValue)
        {
            if (!isInitialized || debugMode)
            {
                if (defaultValues.TryGetValue(key, out object value))
                {
                    return Convert.ToInt32(value);
                }
                return defaultValue;
            }

#if FIREBASE_ENABLED
            return (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
#else
            return defaultValue;
#endif
        }

        private float GetFloat(string key, float defaultValue)
        {
            if (!isInitialized)
            {
                if (defaultValues.TryGetValue(key, out object value))
                {
                    return Convert.ToInt32(value);
                }
                return defaultValue;
            }

#if FIREBASE_ENABLED
            return (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
#else
            return defaultValue;
#endif
        }

        private bool GetBool(string key, bool defaultValue)
        {
            if (!isInitialized)
            {
                if (defaultValues.TryGetValue(key, out object value))
                {
                    return Convert.ToBoolean(value);
                }
                return defaultValue;
            }

#if FIREBASE_ENABLED
            return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
#else
            return defaultValue;
#endif
        }

        private string GetString(string key, string defaultValue)
        {
            if (!isInitialized)
            {
                if (defaultValues.TryGetValue(key, out object value))
                {
                    return value.ToString();
                }
                return defaultValue;
            }

#if FIREBASE_ENABLED
            return Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
#else
            return defaultValue;
#endif
        }

        #endregion

        #region Debug Context Menu

        [ContextMenu("Force Fetch Config")]
        private void ForceFetchConfig()
        {
            Debug.Log("[RemoteConfig] Force fetching config...");
            FetchRemoteConfig();
        }

        [ContextMenu("Print All Config Values")]
        private void PrintAllConfigValues()
        {
            Debug.Log("=== Remote Config Values ===");
            Debug.Log($"Energy Max Free: {GetEnergyMaxFree()}");
            Debug.Log($"Energy Max VIP: {GetEnergyMaxVIP()}");
            Debug.Log($"Energy Regen Minutes: {GetEnergyRegenMinutes()}");
            Debug.Log($"Gem Drop Chance: {GetGemDropChance() * 100}%");
            Debug.Log($"Welcome Pack Price: ${GetWelcomePackPrice()}");
            Debug.Log($"Churn Threshold High: {GetChurnThresholdHigh()}");
            Debug.Log($"Churn Threshold Medium: {GetChurnThresholdMedium()}");
            Debug.Log($"VIP Enabled: {IsVIPEnabled()}");
            Debug.Log($"Power-Ups Enabled: {ArePowerUpsEnabled()}");
            Debug.Log($"Cosmetics Enabled: {AreCosmeticsEnabled()}");
            Debug.Log("============================");
        }

        #endregion
    }
}








