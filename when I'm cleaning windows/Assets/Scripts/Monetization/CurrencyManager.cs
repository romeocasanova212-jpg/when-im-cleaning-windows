using UnityEngine;
using System;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Monetization
{
    /// <summary>
    /// Manages dual currency system: Coins (soft) and Gems (premium).
    /// 2026 Algorithm: 1 Gem ≈ 10 Coins conversion for value hierarchy.
    /// Coins: Earned gameplay (5-20 per level), spent on basic upgrades.
    /// Gems: Rare gameplay + IAP, spent on premium features.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        public static CurrencyManager Instance { get; private set; }
        
        [Header("Currency Settings")]
        [SerializeField] private bool enableDebugUI = true;
        
        // Events
        public static event Action<int> OnCoinsChanged;
        public static event Action<int> OnGemsChanged;
        public static event Action<string, int> OnCurrencyEarned;
        public static event Action<string, int> OnCurrencySpent;
        
        // Current balances
        private int coins = 0;
        private int gems = 0;

        // Configurable reward settings
        private int baseCoinsPerLevel = 5;
        private int coinsPerStar = 5;
        private float gemDropChance = 0.05f;
        private int minGemDrop = 2;
        private int maxGemDrop = 10;
        
        // Constants
        private const int GEM_TO_COIN_RATIO = 10; // 1 gem = 10 coins
        private const string COINS_KEY = "PlayerCoins";
        private const string GEMS_KEY = "PlayerGems";
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyConfig();
            
            LoadCurrency();
        }

        private void ApplyConfig()
        {
            GameConfig config = ConfigProvider.GameConfig;
            if (config == null) return;

            baseCoinsPerLevel = config.baseCoinsPerLevel;
            coinsPerStar = config.coinsPerStar;
            gemDropChance = config.gemDropChance;
            minGemDrop = config.minGemDrop;
            maxGemDrop = config.maxGemDrop;
        }
        
        /// <summary>
        /// Load currency from PlayerPrefs.
        /// </summary>
        private void LoadCurrency()
        {
            coins = PlayerPrefs.GetInt(COINS_KEY, 0);
            gems = PlayerPrefs.GetInt(GEMS_KEY, 0);
            
            OnCoinsChanged?.Invoke(coins);
            OnGemsChanged?.Invoke(gems);
        }
        
        /// <summary>
        /// Set coins balance directly (for loading from cloud save).
        /// </summary>
        public void SetCoins(int amount)
        {
            coins = amount;
            SaveCurrency();
            OnCoinsChanged?.Invoke(coins);
        }
        
        /// <summary>
        /// Set gems balance directly (for loading from cloud save).
        /// </summary>
        public void SetGems(int amount)
        {
            gems = amount;
            SaveCurrency();
            OnGemsChanged?.Invoke(gems);
        }
        
        /// <summary>
        /// Save currency to PlayerPrefs.
        /// </summary>
        private void SaveCurrency()
        {
            PlayerPrefs.SetInt(COINS_KEY, coins);
            PlayerPrefs.SetInt(GEMS_KEY, gems);
            PlayerPrefs.Save();
        }
        
        #region Coins (Soft Currency)
        
        /// <summary>
        /// Add coins (from level completion, ads, etc).
        /// </summary>
        public void AddCoins(int amount, string source = "Gameplay")
        {
            if (amount <= 0) return;
            
            coins += amount;
            SaveCurrency();
            
            OnCoinsChanged?.Invoke(coins);
            OnCurrencyEarned?.Invoke("Coins", amount);
            
            Debug.Log($"Coins added: +{amount} from {source}. Total: {coins}");
        }
        
        /// <summary>
        /// Try to spend coins (for upgrades, retries, etc).
        /// </summary>
        public bool TrySpendCoins(int amount, string purpose = "Purchase")
        {
            if (amount <= 0) return false;
            
            if (coins >= amount)
            {
                coins -= amount;
                SaveCurrency();
                
                OnCoinsChanged?.Invoke(coins);
                OnCurrencySpent?.Invoke("Coins", amount);
                
                Debug.Log($"Coins spent: -{amount} for {purpose}. Remaining: {coins}");
                return true;
            }
            
            Debug.LogWarning($"Insufficient coins: Need {amount}, have {coins}");
            return false;
        }
        
        /// <summary>
        /// Get current coin balance.
        /// </summary>
        public int GetCoins()
        {
            return coins;
        }
        
        /// <summary>
        /// Check if player has enough coins.
        /// </summary>
        public bool HasCoins(int amount)
        {
            return coins >= amount;
        }
        
        #endregion
        
        #region Gems (Premium Currency)
        
        /// <summary>
        /// Add gems (from IAP, rare gameplay rewards, daily login).
        /// </summary>
        public void AddGems(int amount, string source = "Purchase")
        {
            if (amount <= 0) return;
            
            gems += amount;
            SaveCurrency();
            
            OnGemsChanged?.Invoke(gems);
            OnCurrencyEarned?.Invoke("Gems", amount);
            
            Debug.Log($"Gems added: +{amount} from {source}. Total: {gems}");
        }
        
        /// <summary>
        /// Try to spend gems (for VIP, power-ups, skips).
        /// </summary>
        public bool TrySpendGems(int amount, string purpose = "Purchase")
        {
            if (amount <= 0) return false;
            
            if (gems >= amount)
            {
                gems -= amount;
                SaveCurrency();
                
                OnGemsChanged?.Invoke(gems);
                OnCurrencySpent?.Invoke("Gems", amount);
                
                Debug.Log($"Gems spent: -{amount} for {purpose}. Remaining: {gems}");
                return true;
            }
            
            Debug.LogWarning($"Insufficient gems: Need {amount}, have {gems}");
            return false;
        }
        
        /// <summary>
        /// Get current gem balance.
        /// </summary>
        public int GetGems()
        {
            return gems;
        }
        
        /// <summary>
        /// Check if player has enough gems.
        /// </summary>
        public bool HasGems(int amount)
        {
            return gems >= amount;
        }
        
        #endregion
        
        #region Currency Conversion
        
        /// <summary>
        /// Convert gems to coins (emergency option, not encouraged).
        /// </summary>
        public bool ConvertGemToCoins(int gemAmount)
        {
            if (gemAmount <= 0) return false;
            
            if (TrySpendGems(gemAmount, "Gem Conversion"))
            {
                int coinsToAdd = gemAmount * GEM_TO_COIN_RATIO;
                AddCoins(coinsToAdd, "Gem Conversion");
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get coin value of gems (for display).
        /// </summary>
        public int GetCoinValueOfGems(int gemAmount)
        {
            return gemAmount * GEM_TO_COIN_RATIO;
        }
        
        #endregion
        
        #region Level Rewards
        
        /// <summary>
        /// Award currency based on level completion (stars, time, elegance).
        /// </summary>
        public void AwardLevelRewards(int stars, float timeBonus, float eleganceBonus, bool vipActive)
        {
            // Base coins: configurable based on stars
            int baseCoins = baseCoinsPerLevel + (stars * coinsPerStar);
            
            // Time bonus: +0-5 coins
            int timeBonusCoins = Mathf.FloorToInt(timeBonus * 5);
            
            // Elegance bonus: +0-5 coins
            int eleganceBonusCoins = Mathf.FloorToInt(eleganceBonus * 5);
            
            int totalCoins = baseCoins + timeBonusCoins + eleganceBonusCoins;
            
            // VIP 2.5× multiplier
            if (vipActive)
            {
                totalCoins = Mathf.FloorToInt(totalCoins * 2.5f);
            }
            
            AddCoins(totalCoins, "Level Completion");
            
            // Rare gem drops (configurable chance on 3-star)
            if (stars == 3 && UnityEngine.Random.value < gemDropChance)
            {
                int gemReward = UnityEngine.Random.Range(minGemDrop, maxGemDrop + 1);
                AddGems(gemReward, "Perfect Level");
            }
        }
        
        #endregion
        
        #region Idle/Offline Earnings
        
        /// <summary>
        /// Award idle coins (Squeegee HQ generates 2 coins/min, VIP 4 coins/min).
        /// </summary>
        public void AwardIdleCoins(TimeSpan offlineTime, bool vipActive)
        {
            int coinsPerMinute = vipActive ? 4 : 2;
            int maxIdleMinutes = 360; // 6 hours cap
            
            int minutesOffline = Mathf.Min((int)offlineTime.TotalMinutes, maxIdleMinutes);
            int idleCoins = minutesOffline * coinsPerMinute;
            
            if (idleCoins > 0)
            {
                AddCoins(idleCoins, "Idle Generation");
            }
        }
        
        #endregion
        
        #region Debug
        
        /// <summary>
        /// Reset all currency (debug only).
        /// </summary>
        public void ResetCurrency()
        {
            coins = 0;
            gems = 0;
            SaveCurrency();
            
            OnCoinsChanged?.Invoke(coins);
            OnGemsChanged?.Invoke(gems);
            
            Debug.Log("Currency reset to 0");
        }
        
        /// <summary>
        /// Debug UI.
        /// </summary>
        private void OnGUI()
        {
            if (!Application.isPlaying || !enableDebugUI) return;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.yellow;
            
            GUI.Label(new Rect(10, 180, 400, 30), $"💰 Coins: {coins:N0}", style);
            GUI.Label(new Rect(10, 210, 400, 30), $"💎 Gems: {gems:N0}", style);
            
            // Debug buttons
            if (GUI.Button(new Rect(10, 250, 150, 40), "+100 Coins"))
            {
                AddCoins(100, "Debug");
            }
            
            if (GUI.Button(new Rect(170, 250, 150, 40), "+10 Gems"))
            {
                AddGems(10, "Debug");
            }
            
            if (GUI.Button(new Rect(330, 250, 150, 40), "Convert 1 Gem"))
            {
                ConvertGemToCoins(1);
            }
            
            if (GUI.Button(new Rect(490, 250, 150, 40), "Reset"))
            {
                ResetCurrency();
            }
        }
        
        #endregion
    }
}








