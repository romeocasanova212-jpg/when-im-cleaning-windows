using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhenImCleaningWindows.Core;
using WhenImCleaningWindows.Monetization;

namespace WhenImCleaningWindows.UI
{
    /// <summary>
    /// Main HUD - In-game overlay showing energy, coins, gems, timer, and clean percentage.
    /// </summary>
    public class MainHUD : MonoBehaviour
    {
        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI gemsText;
        [SerializeField] private TextMeshProUGUI coinsText;
        [SerializeField] private TextMeshProUGUI energyText;
        
        [Header("Level Info")]
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private TextMeshProUGUI worldNameText;
        
        [Header("Timer")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Image timerFillBar;
        [SerializeField] private Color normalTimerColor = Color.white;
        [SerializeField] private Color warningTimerColor = Color.yellow;
        [SerializeField] private Color dangerTimerColor = Color.red;
        [SerializeField] private float warningThreshold = 30f;  // 30 seconds
        [SerializeField] private float dangerThreshold = 10f;   // 10 seconds
        
        [Header("Clean Progress")]
        [SerializeField] private TextMeshProUGUI cleanPercentageText;
        [SerializeField] private Image cleanProgressBar;
        [SerializeField] private Color progressStartColor = Color.red;
        [SerializeField] private Color progressEndColor = Color.green;
        
        [Header("Buttons")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button powerUpButton;
        
        private GameManager gameManager;
        private CurrencyManager currencyManager;
        private EnergySystem energySystem;
        private Mechanics.TimerSystem timerSystem;
        private Gameplay.WindowMeshController windowMesh;
        
        private float currentCleanPercentage = 0f;
        
        private void Start()
        {
            // Defer initialization to next frame to ensure all singletons are available
            StartCoroutine(DeferredInitialization());
        }
        
        private System.Collections.IEnumerator DeferredInitialization()
        {
            // Wait one frame for all managers to initialize
            yield return null;
            
            AutoAssignReferences();
            
            gameManager = GameManager.Instance;
            currencyManager = CurrencyManager.Instance;
            energySystem = EnergySystem.Instance;
            timerSystem = FindFirstObjectByType<Mechanics.TimerSystem>();
            windowMesh = FindFirstObjectByType<Gameplay.WindowMeshController>();
            
            // Subscribe to events
            if (currencyManager != null)
            {
                CurrencyManager.OnGemsChanged += UpdateGems;
                CurrencyManager.OnCoinsChanged += UpdateCoins;
            }
            
            if (energySystem != null)
            {
                EnergySystem.OnEnergyChanged += UpdateEnergy;
            }
            
            if (gameManager != null)
            {
                GameManager.OnLevelStarted += OnLevelStarted;
            }
            
            if (timerSystem != null)
            {
                timerSystem.OnTimerUpdate.AddListener(UpdateTimer);
            }

            if (windowMesh != null)
            {
                Gameplay.WindowMeshController.OnCleanPercentageChanged += SetCleanPercentage;
            }
            
            // Setup buttons
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }
            
            if (powerUpButton != null)
            {
                powerUpButton.onClick.AddListener(OnPowerUpClicked);
            }
            
            // Initial update
            UpdateCurrencyDisplay();
            UpdateEnergy(energySystem?.GetCurrentEnergy() ?? 0);
        }
        
        private void AutoAssignReferences()
        {
            gemsText ??= FindChildComponent<TextMeshProUGUI>("GemsText");
            coinsText ??= FindChildComponent<TextMeshProUGUI>("CoinsText");
            energyText ??= FindChildComponent<TextMeshProUGUI>("EnergyText");

            levelNumberText ??= FindChildComponent<TextMeshProUGUI>("LevelNumberText");
            worldNameText ??= FindChildComponent<TextMeshProUGUI>("WorldNameText");

            timerText ??= FindChildComponent<TextMeshProUGUI>("TimerText");
            timerFillBar ??= FindChildComponent<Image>("TimerFillBar");

            cleanPercentageText ??= FindChildComponent<TextMeshProUGUI>("CleanPercentageText");
            cleanProgressBar ??= FindChildComponent<Image>("CleanProgressBar");

            pauseButton ??= FindChildComponent<Button>("PauseButton");
            powerUpButton ??= FindChildComponent<Button>("PowerUpButton");
        }

        private T FindChildComponent<T>(string name) where T : Component
        {
            Transform child = FindChildRecursive(transform, name);
            return child != null ? child.GetComponent<T>() : null;
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        private void Update()
        {
            // Update clean percentage display
            UpdateCleanProgress(currentCleanPercentage);
        }
        
        /// <summary>
        /// Update currency display.
        /// </summary>
        private void UpdateCurrencyDisplay()
        {
            if (currencyManager == null) return;
            
            if (gemsText != null)
            {
                gemsText.text = currencyManager.GetGems().ToString("N0");
            }
            
            if (coinsText != null)
            {
                coinsText.text = currencyManager.GetCoins().ToString("N0");
            }
        }
        
        private void UpdateGems(int gems)
        {
            if (gemsText != null)
            {
                gemsText.text = gems.ToString("N0");
                
                // Animate (in production)
                // StartCoroutine(AnimateNumber(gemsText));
            }
        }
        
        private void UpdateCoins(int coins)
        {
            if (coinsText != null)
            {
                coinsText.text = coins.ToString("N0");
            }
        }
        
        private void UpdateEnergy(int energy)
        {
            if (energyText != null)
            {
                bool isVIP = VIPManager.Instance?.IsVIPActive() ?? false;
                if (isVIP)
                {
                    energyText.text = "∞";
                }
                else
                {
                    int maxEnergy = energySystem?.GetMaxEnergy() ?? 5;
                    energyText.text = $"{energy}/{maxEnergy}";
                }
            }
        }
        
        /// <summary>
        /// Update timer display.
        /// </summary>
        private void UpdateTimer(float timeRemaining)
        {
            if (timerText == null) return;
            
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";
            
            // Color coding
            Color timerColor = normalTimerColor;
            if (timeRemaining <= dangerThreshold)
            {
                timerColor = dangerTimerColor;
            }
            else if (timeRemaining <= warningThreshold)
            {
                timerColor = warningTimerColor;
            }
            
            timerText.color = timerColor;
            
            // Fill bar
            if (timerFillBar != null)
            {
                float totalTime = timerSystem != null ? timerSystem.GetTimeLimit() : 120f;
                timerFillBar.fillAmount = Mathf.Clamp01(timeRemaining / totalTime);
                timerFillBar.color = timerColor;
            }
        }
        
        /// <summary>
        /// Update clean progress display.
        /// </summary>
        private void UpdateCleanProgress(float percentage)
        {
            if (cleanPercentageText != null)
            {
                cleanPercentageText.text = $"{percentage:F1}%";
            }
            
            if (cleanProgressBar != null)
            {
                cleanProgressBar.fillAmount = percentage / 100f;
                cleanProgressBar.color = Color.Lerp(progressStartColor, progressEndColor, percentage / 100f);
            }
        }
        
        /// <summary>
        /// Called when level starts.
        /// </summary>
        private void OnLevelStarted(Procedural.LevelData level)
        {
            if (levelNumberText != null)
            {
                levelNumberText.text = $"Level {level.levelNumber}";
            }
            
            if (worldNameText != null)
            {
                worldNameText.text = level.themeName;
            }
            
            currentCleanPercentage = 0f;
        }
        
        /// <summary>
        /// Set clean percentage (called by game logic).
        /// </summary>
        public void SetCleanPercentage(float percentage)
        {
            currentCleanPercentage = Mathf.Clamp(percentage, 0f, 100f);
        }
        
        /// <summary>
        /// Handle pause button.
        /// </summary>
        private void OnPauseClicked()
        {
            UIManager.Instance?.OnPauseButton();
        }
        
        /// <summary>
        /// Handle power-up button.
        /// </summary>
        private void OnPowerUpClicked()
        {
            // In production: Show power-up selection menu
            UnityEngine.Debug.Log("Power-up menu opened");
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (currencyManager != null)
            {
                CurrencyManager.OnGemsChanged -= UpdateGems;
                CurrencyManager.OnCoinsChanged -= UpdateCoins;
            }
            
            if (energySystem != null)
            {
                EnergySystem.OnEnergyChanged -= UpdateEnergy;
            }
            
            if (gameManager != null)
            {
                GameManager.OnLevelStarted -= OnLevelStarted;
            }
            
            if (timerSystem != null)
            {
                timerSystem.OnTimerUpdate.RemoveListener(UpdateTimer);
            }

            Gameplay.WindowMeshController.OnCleanPercentageChanged -= SetCleanPercentage;
        }
    }
}








