using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using WhenImCleaningWindows.Core;
using WhenImCleaningWindows.Monetization;

namespace WhenImCleaningWindows.Debugging
{
    /// <summary>
    /// Debug Console - In-game testing tool with quick access to common debug commands.
    /// Press ` (backtick) or ~ (tilde) to toggle.
    /// </summary>
    public class DebugConsole : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject consolePanel;
        [SerializeField] private TextMeshProUGUI consoleOutput;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Transform buttonContainer;
        [SerializeField] private GameObject buttonPrefab;
        
        [Header("Settings")]
        [SerializeField] private KeyCode toggleKey = KeyCode.BackQuote;
        [SerializeField] private int maxLogLines = 50;
        [SerializeField] private bool showOnStart = false;
        
        private List<string> logHistory = new List<string>();
        private bool isVisible = false;
        
        private void Start()
        {
            if (consolePanel != null)
            {
                consolePanel.SetActive(showOnStart);
                isVisible = showOnStart;
            }
            
            CreateDebugButtons();
            Logger.Log("Debug Console Ready. Press ` to toggle.");
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                ToggleConsole();
            }
        }
        
        /// <summary>
        /// Toggle console visibility.
        /// </summary>
        public void ToggleConsole()
        {
            isVisible = !isVisible;
            if (consolePanel != null)
            {
                consolePanel.SetActive(isVisible);
            }
        }
        
        /// <summary>
        /// Log message to console.
        /// </summary>
        public void Log(string message)
        {
            logHistory.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");
            
            // Trim old logs
            if (logHistory.Count > maxLogLines)
            {
                logHistory.RemoveAt(0);
            }
            
            UpdateConsoleOutput();
        }
        
        /// <summary>
        /// Add log message to history.
        /// </summary>
        private void AddLog(string message)
        {
            logHistory.Add($"[{System.DateTime.Now:HH:mm:ss}] {message}");
            
            if (logHistory.Count > maxLogLines)
            {
                logHistory.RemoveAt(0);
            }
            
            UpdateConsoleOutput();
        }
        
        /// <summary>
        /// Update console output text.
        /// </summary>
        private void UpdateConsoleOutput()
        {
            if (consoleOutput != null)
            {
                consoleOutput.text = string.Join("\n", logHistory);
                
                // Scroll to bottom
                if (scrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    scrollRect.verticalNormalizedPosition = 0f;
                }
            }
        }
        
        /// <summary>
        /// Create debug command buttons.
        /// </summary>
        private void CreateDebugButtons()
        {
            if (buttonContainer == null || buttonPrefab == null) return;
            
            // Energy commands
            CreateButton("Add 5 Energy", () => {
                EnergySystem.Instance?.AddEnergy(5, "Debug");
                Logger.Log("Added 5 energy");
            });
            
            CreateButton("Refill Energy", () => {
                EnergySystem.Instance?.RefillEnergy();
                Logger.Log("Energy refilled");
            });
            
            // Currency commands
            CreateButton("Add 1000 Gems", () => {
                CurrencyManager.Instance?.AddGems(1000, "Debug");
                Logger.Log("Added 1000 gems");
            });
            
            CreateButton("Add 5000 Coins", () => {
                CurrencyManager.Instance?.AddCoins(5000, "Debug");
                Logger.Log("Added 5000 coins");
            });
            
            // VIP commands
            CreateButton("Activate VIP Bronze", () => {
                VIPManager.Instance?.ActivateVIP(VIPTier.Bronze, 30);
                Logger.Log("VIP Bronze activated (30 days)");
            });
            
            CreateButton("Activate VIP Gold", () => {
                VIPManager.Instance?.ActivateVIP(VIPTier.Gold, 30);
                Logger.Log("VIP Gold activated (30 days)");
            });
            
            // Level commands
            CreateButton("Start Level 1", () => {
                GameManager.Instance?.StartLevel(1);
                Logger.Log("Started Level 1");
            });
            
            CreateButton("Complete Level (3★)", () => {
                GameManager.Instance?.CompleteLevel(100f, 50, true);
                Logger.Log("Level completed with 3 stars");
            });
            
            CreateButton("Unlock All Levels", () => {
                var progress = GameManager.Instance?.GetProgress();
                if (progress != null)
                {
                    progress.highestLevelUnlocked = 10000;
                    Logger.Log("All 10,000 levels unlocked");
                }
            });
            
            // Personalization commands
            CreateButton("Trigger D1 Offer", () => {
                PersonalizationEngine.Instance?.DEBUG_TriggerD1();
                Logger.Log("D1 Welcome Pack triggered");
            });
            
            CreateButton("Simulate Frustration", () => {
                PersonalizationEngine.Instance?.DEBUG_SimulateFrustration();
                Logger.Log("Simulated 5 consecutive deaths");
            });
            
            CreateButton("Calculate Churn", () => {
                PersonalizationEngine.Instance?.DEBUG_CalculateChurn();
                Logger.Log("Churn score calculated");
            });
            
            // Level generation commands
            CreateButton("Generate Level 1000", () => {
                var level = Procedural.LevelGenerator.Instance?.GenerateLevel(1000);
                if (level != null)
                {
                    Logger.Log($"Generated Level 1000: World {level.worldNumber}, {level.hazardCount} hazards");
                }
            });
            
            CreateButton("Pre-Gen World 1", () => {
                Procedural.LevelGenerator.Instance?.PreGenerateWorld(1);
                Logger.Log("Pre-generating World 1 (1,000 levels)...");
            });
            
            // System commands
            CreateButton("Clear PlayerPrefs", () => {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                Logger.Log("PlayerPrefs cleared! Restart required.");
            });
            
            CreateButton("Print System Status", () => {
                PrintSystemStatus();
            });
        }
        
        /// <summary>
        /// Create a debug button.
        /// </summary>
        private void CreateButton(string label, System.Action action)
        {
            if (buttonPrefab == null || buttonContainer == null) return;
            
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = label;
            }
            
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => action());
            }
        }
        
        /// <summary>
        /// Print system status to console.
        /// </summary>
        private void PrintSystemStatus()
        {
            Logger.Log("=== SYSTEM STATUS ===");
            
            // Energy
            var energy = EnergySystem.Instance;
            if (energy != null)
            {
                Logger.Log($"Energy: {energy.GetCurrentEnergy()}/{energy.GetMaxEnergy()}");
            }
            
            // Currency
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                Logger.Log($"Gems: {currency.GetGems()}, Coins: {currency.GetCoins()}");
            }
            
            // VIP
            var vip = VIPManager.Instance;
            if (vip != null)
            {
                Logger.Log($"VIP: {vip.GetCurrentTier()} (Level {vip.GetCumulativeLevel()})");
            }
            
            // Progress
            var progress = GameManager.Instance?.GetProgress();
            if (progress != null)
            {
                Logger.Log($"Level: {progress.currentLevel}/{progress.highestLevelUnlocked}, Stars: {progress.totalStars}");
            }
            
            // Churn
            var personalization = PersonalizationEngine.Instance;
            if (personalization != null)
            {
                var profile = personalization.GetProfile();
                Logger.Log($"Churn Score: {profile.lastChurnScore:F2}, Days: {profile.daysPlayed}");
            }
            
            Logger.Log("====================");
        }
        
        // === PUBLIC API ===
        
        public static void DebugLog(string message)
        {
            var console = FindFirstObjectByType<DebugConsole>();
            if (console != null)
            {
                console.AddLog(message);
            }
        }
    }
}








