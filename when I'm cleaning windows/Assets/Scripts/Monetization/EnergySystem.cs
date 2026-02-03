using UnityEngine;
using System;
using System.Collections;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Monetization
{
    /// <summary>
    /// Energy system managing lives/hearts for level attempts.
    /// 2026 Polished: 1 life per 20 minutes = 72 lives/day free (up 50% from 30min).
    /// VIP subscribers can overflow to 10 lives (5 max for free players).
    /// </summary>
    public class EnergySystem : MonoBehaviour
    {
        public static EnergySystem Instance { get; private set; }
        
        [Header("Energy Settings (2026 Optimized)")]
        [SerializeField] private int maxEnergy = 5;
        [SerializeField] private int vipMaxEnergy = 10; // VIP overflow
        [SerializeField] private float energyRegenTimeMinutes = 20f; // 72 lives/day
        
        [Header("Debug")]
        [SerializeField] private bool showDebugUI = true;
        
        // Events
        public static event Action<int> OnEnergyChanged;
        public static event Action OnEnergyDepleted;
        public static event Action<int> OnEnergyRestored;
        
        // State
        private int currentEnergy;
        private DateTime lastEnergyUpdateTime;
        private bool isVIP = false;
        private Coroutine regenCoroutine;
        
        // Save keys
        private const string ENERGY_KEY = "CurrentEnergy";
        private const string LAST_UPDATE_KEY = "LastEnergyUpdate";
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            ApplyConfig();
            
            LoadEnergy();
            StartRegeneration();
        }

        private void ApplyConfig()
        {
            GameConfig config = ConfigProvider.GameConfig;
            if (config == null) return;

            maxEnergy = config.maxFreeEnergy;
            vipMaxEnergy = Mathf.Max(config.maxVipEnergy, maxEnergy);
            energyRegenTimeMinutes = Mathf.Max(1f, config.energyRegenTime / 60f);
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                SaveEnergy();
            }
            else
            {
                LoadEnergy();
                ProcessOfflineRegen();
            }
        }
        
        private void OnApplicationQuit()
        {
            SaveEnergy();
        }
        
        /// <summary>
        /// Initialize energy from save data or set to max.
        /// </summary>
        private void LoadEnergy()
        {
            currentEnergy = PlayerPrefs.GetInt(ENERGY_KEY, maxEnergy);
            
            string lastUpdateString = PlayerPrefs.GetString(LAST_UPDATE_KEY, DateTime.Now.ToString());
            if (DateTime.TryParse(lastUpdateString, out DateTime savedTime))
            {
                lastEnergyUpdateTime = savedTime;
            }
            else
            {
                lastEnergyUpdateTime = DateTime.Now;
            }
            
            // Check VIP status (would integrate with VIPManager)
            isVIP = PlayerPrefs.GetInt("VIP_Active", 0) == 1;
            
            OnEnergyChanged?.Invoke(currentEnergy);
        }
        
        /// <summary>
        /// Save current energy state to PlayerPrefs.
        /// </summary>
        private void SaveEnergy()
        {
            PlayerPrefs.SetInt(ENERGY_KEY, currentEnergy);
            PlayerPrefs.SetString(LAST_UPDATE_KEY, DateTime.Now.ToString());
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Process energy regeneration that occurred while app was closed.
        /// </summary>
        private void ProcessOfflineRegen()
        {
            TimeSpan timeSinceLastUpdate = DateTime.Now - lastEnergyUpdateTime;
            int energyToAdd = Mathf.FloorToInt((float)timeSinceLastUpdate.TotalMinutes / energyRegenTimeMinutes);
            
            if (energyToAdd > 0)
            {
                AddEnergy(energyToAdd, "Offline Regeneration");
                lastEnergyUpdateTime = DateTime.Now;
                SaveEnergy();
            }
        }
        
        /// <summary>
        /// Start energy regeneration coroutine.
        /// </summary>
        private void StartRegeneration()
        {
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
            }
            regenCoroutine = StartCoroutine(RegenerateEnergyCoroutine());
        }
        
        /// <summary>
        /// Regenerate energy over time (1 per 20 minutes).
        /// </summary>
        private IEnumerator RegenerateEnergyCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(60f); // Check every minute
                
                int maxCap = isVIP ? vipMaxEnergy : maxEnergy;
                
                if (currentEnergy < maxCap)
                {
                    TimeSpan timeSinceLastUpdate = DateTime.Now - lastEnergyUpdateTime;
                    
                    if (timeSinceLastUpdate.TotalMinutes >= energyRegenTimeMinutes)
                    {
                        AddEnergy(1, "Regeneration");
                        lastEnergyUpdateTime = DateTime.Now;
                        SaveEnergy();
                    }
                }
            }
        }
        
        /// <summary>
        /// Consume energy for level attempt.
        /// </summary>
        public bool TryConsumeEnergy()
        {
            if (isVIP)
            {
                // VIP has unlimited energy
                return true;
            }
            
            if (currentEnergy > 0)
            {
                currentEnergy--;
                SaveEnergy();
                OnEnergyChanged?.Invoke(currentEnergy);
                
                if (currentEnergy == 0)
                {
                    OnEnergyDepleted?.Invoke();
                }
                
                return true;
            }
            
            OnEnergyDepleted?.Invoke();
            return false;
        }
        
        /// <summary>
        /// Add energy (from IAP, ads, rewards).
        /// </summary>
        public void AddEnergy(int amount, string source = "Purchase")
        {
            if (isVIP && amount > 0)
            {
                // VIP gets unlimited, but we still track for UI
                currentEnergy = Mathf.Min(currentEnergy + amount, vipMaxEnergy);
            }
            else
            {
                currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
            }
            
            SaveEnergy();
            OnEnergyChanged?.Invoke(currentEnergy);
            OnEnergyRestored?.Invoke(amount);
            
            Debug.Log($"Energy added: +{amount} from {source}. Current: {currentEnergy}");
        }
        
        /// <summary>
        /// Refill energy to max (from IAP or rewards).
        /// </summary>
        public void RefillEnergy()
        {
            int maxCap = isVIP ? vipMaxEnergy : maxEnergy;
            int added = maxCap - currentEnergy;
            
            currentEnergy = maxCap;
            lastEnergyUpdateTime = DateTime.Now;
            
            SaveEnergy();
            OnEnergyChanged?.Invoke(currentEnergy);
            OnEnergyRestored?.Invoke(added);
            
            Debug.Log($"Energy refilled to {currentEnergy}");
        }
        
        /// <summary>
        /// Set VIP status (called by VIPManager).
        /// </summary>
        public void SetVIPStatus(bool vipActive)
        {
            isVIP = vipActive;
            
            if (isVIP && currentEnergy < vipMaxEnergy)
            {
                // VIP gets immediate boost to overflow cap
                currentEnergy = vipMaxEnergy;
                OnEnergyChanged?.Invoke(currentEnergy);
            }
        }
        
        /// <summary>
        /// Get current energy.
        /// </summary>
        public int GetCurrentEnergy()
        {
            return isVIP ? int.MaxValue : currentEnergy; // VIP = unlimited
        }
        
        /// <summary>
        /// Get max energy cap.
        /// </summary>
        public int GetMaxEnergy()
        {
            return isVIP ? vipMaxEnergy : maxEnergy;
        }
        
        /// <summary>
        /// Check if energy is full.
        /// </summary>
        public bool IsFull()
        {
            return isVIP || currentEnergy >= maxEnergy;
        }
        
        /// <summary>
        /// Get time until next energy regen.
        /// </summary>
        public TimeSpan GetTimeUntilNextRegen()
        {
            if (isVIP || IsFull())
            {
                return TimeSpan.Zero;
            }
            
            TimeSpan timeSinceLastUpdate = DateTime.Now - lastEnergyUpdateTime;
            TimeSpan regenInterval = TimeSpan.FromMinutes(energyRegenTimeMinutes);
            TimeSpan remaining = regenInterval - timeSinceLastUpdate;
            
            return remaining.TotalSeconds > 0 ? remaining : TimeSpan.Zero;
        }
        
        /// <summary>
        /// Get formatted time string for UI.
        /// </summary>
        public string GetFormattedTimeUntilRegen()
        {
            if (isVIP)
            {
                return "VIP ∞";
            }
            
            if (IsFull())
            {
                return "Full";
            }
            
            TimeSpan time = GetTimeUntilNextRegen();
            return $"{time.Minutes:00}:{time.Seconds:00}";
        }
        
        /// <summary>
        /// Debug UI.
        /// </summary>
        private void OnGUI()
        {
            if (!Application.isPlaying || !showDebugUI) return;
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 24;
            style.normal.textColor = Color.white;
            
            string vipText = isVIP ? " (VIP ∞)" : "";
            string energyText = $"❤️ Energy: {currentEnergy}/{GetMaxEnergy()}{vipText}";
            string regenText = $"Next: {GetFormattedTimeUntilRegen()}";
            
            GUI.Label(new Rect(10, 60, 400, 30), energyText, style);
            GUI.Label(new Rect(10, 90, 400, 30), regenText, style);
            
            // Debug buttons
            if (GUI.Button(new Rect(10, 130, 120, 40), "Use Energy"))
            {
                TryConsumeEnergy();
            }
            
            if (GUI.Button(new Rect(140, 130, 120, 40), "Add +1"))
            {
                AddEnergy(1, "Debug");
            }
            
            if (GUI.Button(new Rect(270, 130, 120, 40), "Refill"))
            {
                RefillEnergy();
            }
            
            if (GUI.Button(new Rect(400, 130, 120, 40), "Toggle VIP"))
            {
                SetVIPStatus(!isVIP);
            }
        }
    }
}








