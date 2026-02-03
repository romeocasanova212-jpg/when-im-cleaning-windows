using UnityEngine;
using System;

namespace WhenImCleaningWindows.Monetization
{
    /// <summary>
    /// VIP tier definitions (3-tier subscription system).
    /// </summary>
    public enum VIPTier
    {
        None = 0,
        Bronze = 1,  // $4.99/mo
        Silver = 2,  // $9.99/mo
        Gold = 3     // $19.99/mo
    }
    
    /// <summary>
    /// VIP Manager - Handles subscription tiers and cumulative gem spending levels.
    /// Integrates with EnergySystem (unlimited energy) and CurrencyManager (reward multipliers).
    /// </summary>
    public class VIPManager : MonoBehaviour
    {
        public static VIPManager Instance { get; private set; }
        
        [Header("VIP Status")]
        [SerializeField] private VIPTier currentTier = VIPTier.None;
        [SerializeField] private int cumulativeVIPLevel = 0;
        [SerializeField] private float totalGemsSpent = 0f;
        
        [Header("Subscription Tracking")]
        [SerializeField] private DateTime subscriptionExpiry;
        [SerializeField] private bool isSubscriptionActive = false;
        
        // Events
        public static event Action<VIPTier> OnVIPTierChanged;
        public static event Action<int> OnVIPLevelUp;
        public static event Action OnVIPExpired;
        public static event Action<VIPTier> OnVIPRenewed;
        
        // Managers
        private EnergySystem energySystem;
        private CurrencyManager currencyManager;
        
        // Constants
        private const int GEMS_PER_VIP_LEVEL = 5000;  // 5,000 gems spent = 1 cumulative level
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadVIPData();
        }
        
        private void Start()
        {
            energySystem = EnergySystem.Instance;
            currencyManager = CurrencyManager.Instance;
            
            CheckSubscriptionExpiry();
        }
        
        /// <summary>
        /// Activate VIP subscription (called when player purchases).
        /// </summary>
        public void ActivateVIP(VIPTier tier, int durationDays = 30)
        {
            VIPTier oldTier = currentTier;
            currentTier = tier;
            isSubscriptionActive = true;
            subscriptionExpiry = DateTime.Now.AddDays(durationDays);
            
            SaveVIPData();
            
            OnVIPTierChanged?.Invoke(tier);
            
            if (tier > oldTier)
            {
                ShowTierUpgradePopup(tier);
            }
            
            Debug.Log($"VIP {tier} activated until {subscriptionExpiry.ToShortDateString()}");
        }
        
        /// <summary>
        /// Check if subscription has expired.
        /// </summary>
        private void CheckSubscriptionExpiry()
        {
            if (isSubscriptionActive && DateTime.Now > subscriptionExpiry)
            {
                DeactivateVIP();
            }
        }
        
        /// <summary>
        /// Deactivate VIP (subscription expired or cancelled).
        /// </summary>
        private void DeactivateVIP()
        {
            if (!isSubscriptionActive) return;
            
            currentTier = VIPTier.None;
            isSubscriptionActive = false;
            
            SaveVIPData();
            
            OnVIPExpired?.Invoke();
            OnVIPTierChanged?.Invoke(VIPTier.None);
            
            Debug.Log("VIP subscription expired");
        }
        
        /// <summary>
        /// Track gem spending (for cumulative VIP levels).
        /// Called by CurrencyManager.TrySpendGems().
        /// </summary>
        public void AddGemsSpent(int gems)
        {
            totalGemsSpent += gems;
            
            int oldLevel = cumulativeVIPLevel;
            cumulativeVIPLevel = Mathf.FloorToInt(totalGemsSpent / GEMS_PER_VIP_LEVEL);
            
            if (cumulativeVIPLevel > oldLevel)
            {
                OnVIPLevelUp?.Invoke(cumulativeVIPLevel);
                ShowCumulativeLevelUpPopup();
            }
            
            SaveVIPData();
        }
        
        /// <summary>
        /// Get current VIP tier.
        /// </summary>
        public VIPTier GetCurrentTier()
        {
            CheckSubscriptionExpiry();
            return currentTier;
        }
        
        /// <summary>
        /// Is VIP active?
        /// </summary>
        public bool IsVIPActive()
        {
            CheckSubscriptionExpiry();
            return isSubscriptionActive && currentTier != VIPTier.None;
        }
        
        /// <summary>
        /// Get cumulative VIP level (based on gem spending).
        /// </summary>
        public int GetCumulativeLevel()
        {
            return cumulativeVIPLevel;
        }
        
        /// <summary>
        /// Get reward multiplier based on VIP tier.
        /// </summary>
        public float GetRewardMultiplier()
        {
            switch (currentTier)
            {
                case VIPTier.Bronze: return 2.5f;
                case VIPTier.Silver: return 3.0f;
                case VIPTier.Gold:   return 4.0f;
                default:             return 1.0f;
            }
        }
        
        /// <summary>
        /// Get speed boost percentage.
        /// </summary>
        public float GetSpeedBoost()
        {
            switch (currentTier)
            {
                case VIPTier.Bronze: return 0.15f;  // +15%
                case VIPTier.Silver: return 0.25f;  // +25%
                case VIPTier.Gold:   return 0.40f;  // +40%
                default:             return 0f;
            }
        }
        
        /// <summary>
        /// Get idle coin rate multiplier.
        /// </summary>
        public float GetIdleCoinMultiplier()
        {
            if (IsVIPActive()) return 2.0f;  // 2× for any VIP tier
            return 1.0f;
        }
        
        /// <summary>
        /// Does VIP have unlimited energy?
        /// </summary>
        public bool HasUnlimitedEnergy()
        {
            return IsVIPActive();  // All tiers get unlimited
        }
        
        /// <summary>
        /// Get energy max overflow based on tier.
        /// </summary>
        public int GetEnergyMaxOverflow()
        {
            if (IsVIPActive()) return 10;  // VIP can bank 10 energy
            return 5;  // Free: 5 max
        }
        
        /// <summary>
        /// Get days remaining on subscription.
        /// </summary>
        public int GetDaysRemaining()
        {
            if (!isSubscriptionActive) return 0;
            
            TimeSpan remaining = subscriptionExpiry - DateTime.Now;
            return Mathf.Max(0, (int)remaining.TotalDays);
        }
        
        /// <summary>
        /// Get progress to next cumulative level (0-1).
        /// </summary>
        public float GetLevelProgress()
        {
            float gemsIntoCurrentLevel = totalGemsSpent - (cumulativeVIPLevel * GEMS_PER_VIP_LEVEL);
            return gemsIntoCurrentLevel / GEMS_PER_VIP_LEVEL;
        }
        
        /// <summary>
        /// Get gems needed for next cumulative level.
        /// </summary>
        public int GetGemsToNextLevel()
        {
            int gemsIntoCurrentLevel = (int)(totalGemsSpent - (cumulativeVIPLevel * GEMS_PER_VIP_LEVEL));
            return GEMS_PER_VIP_LEVEL - gemsIntoCurrentLevel;
        }
        
        /// <summary>
        /// Save VIP data to PlayerPrefs.
        /// </summary>
        private void SaveVIPData()
        {
            PlayerPrefs.SetInt("VIP_Tier", (int)currentTier);
            PlayerPrefs.SetInt("VIP_CumulativeLevel", cumulativeVIPLevel);
            PlayerPrefs.SetFloat("VIP_GemsSpent", totalGemsSpent);
            PlayerPrefs.SetString("VIP_Expiry", subscriptionExpiry.ToString());
            PlayerPrefs.SetInt("VIP_Active", isSubscriptionActive ? 1 : 0);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Load VIP data from PlayerPrefs.
        /// </summary>
        private void LoadVIPData()
        {
            currentTier = (VIPTier)PlayerPrefs.GetInt("VIP_Tier", 0);
            cumulativeVIPLevel = PlayerPrefs.GetInt("VIP_CumulativeLevel", 0);
            totalGemsSpent = PlayerPrefs.GetFloat("VIP_GemsSpent", 0f);
            isSubscriptionActive = PlayerPrefs.GetInt("VIP_Active", 0) == 1;
            
            string expiryString = PlayerPrefs.GetString("VIP_Expiry", DateTime.Now.ToString());
            if (DateTime.TryParse(expiryString, out DateTime parsedExpiry))
            {
                subscriptionExpiry = parsedExpiry;
            }
            else
            {
                subscriptionExpiry = DateTime.Now;
            }
            
            Debug.Log($"VIP loaded: Tier {currentTier}, Level {cumulativeVIPLevel}, Active: {isSubscriptionActive}");
        }
        
        /// <summary>
        /// Show tier upgrade popup (would trigger UI in production).
        /// </summary>
        private void ShowTierUpgradePopup(VIPTier tier)
        {
            string[] perks = GetTierPerks(tier);
            Debug.Log($"VIP {tier} UNLOCKED!\nPerks: {string.Join(", ", perks)}");
        }
        
        /// <summary>
        /// Show cumulative level up popup.
        /// </summary>
        private void ShowCumulativeLevelUpPopup()
        {
            Debug.Log($"VIP Level {cumulativeVIPLevel}! Unlocked permanent perk!");
        }
        
        /// <summary>
        /// Get tier perks for display.
        /// </summary>
        private string[] GetTierPerks(VIPTier tier)
        {
            switch (tier)
            {
                case VIPTier.Bronze:
                    return new[] { "∞ Energy Forever", "2.5× All Rewards", "+15% Speed", "VIP Chat Badge" };
                case VIPTier.Silver:
                    return new[] { "∞ Energy Forever", "3× All Rewards", "+25% Speed", "5 Exclusive Skins", "No Ads" };
                case VIPTier.Gold:
                    return new[] { "∞ Energy Forever", "4× All Rewards", "+40% Speed", "10 Skins + Effects", "Auto-Complete", "Priority Support" };
                default:
                    return new string[0];
            }
        }
        
        // === DEBUG FUNCTIONS ===
        
        [ContextMenu("Activate Bronze VIP (30 days)")]
        public void DEBUG_ActivateBronze()
        {
            ActivateVIP(VIPTier.Bronze, 30);
        }
        
        [ContextMenu("Activate Silver VIP (30 days)")]
        public void DEBUG_ActivateSilver()
        {
            ActivateVIP(VIPTier.Silver, 30);
        }
        
        [ContextMenu("Activate Gold VIP (30 days)")]
        public void DEBUG_ActivateGold()
        {
            ActivateVIP(VIPTier.Gold, 30);
        }
        
        [ContextMenu("Expire VIP")]
        public void DEBUG_ExpireVIP()
        {
            DeactivateVIP();
        }
        
        [ContextMenu("Add 5000 Gems Spent (Level Up)")]
        public void DEBUG_AddGemsSpent()
        {
            AddGemsSpent(5000);
        }
        
        public DateTime GetVIPExpiryDate() => DateTime.Now.AddDays(30);
        public int GetCumulativeGemsSpent() => 0;
    }
}








