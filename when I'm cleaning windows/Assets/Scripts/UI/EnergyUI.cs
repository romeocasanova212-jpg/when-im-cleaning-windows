using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhenImCleaningWindows.Monetization;

namespace WhenImCleaningWindows.UI
{
    /// <summary>
    /// Energy UI Controller - Displays energy counter, regeneration timer, and refill options.
    /// </summary>
    public class EnergyUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI regenTimerText;
        [SerializeField] private Image energyFillBar;
        [SerializeField] private GameObject vipUnlimitedIcon;
        [SerializeField] private Button addEnergyButton;
        
        [Header("Refill Popup")]
        [SerializeField] private GameObject refillPopup;
        [SerializeField] private TextMeshProUGUI refillCostText;
        [SerializeField] private Button refillWithGemsButton;
        [SerializeField] private Button watchAdButton;
        [SerializeField] private Button closePopupButton;
        
        [Header("Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color lowEnergyColor = Color.red;
        [SerializeField] private int lowEnergyThreshold = 2;
        
        private EnergySystem energySystem;
        private VIPManager vipManager;
        private CurrencyManager currencyManager;
        
        private void Start()
        {
            StartCoroutine(DeferredInitialization());
        }
        
        private System.Collections.IEnumerator DeferredInitialization()
        {
            yield return null;
            
            AutoAssignReferences();
            energySystem = EnergySystem.Instance;
            vipManager = VIPManager.Instance;
            currencyManager = CurrencyManager.Instance;
            
            // Subscribe to energy events
            if (energySystem != null)
            {
                EnergySystem.OnEnergyChanged += UpdateEnergyDisplay;
                EnergySystem.OnEnergyDepleted += ShowRefillPopup;
            }
            
            // Setup buttons
            if (addEnergyButton != null)
            {
                addEnergyButton.onClick.AddListener(ShowRefillPopup);
            }
            
            if (refillWithGemsButton != null)
            {
                refillWithGemsButton.onClick.AddListener(RefillWithGems);
            }
            
            if (watchAdButton != null)
            {
                watchAdButton.onClick.AddListener(RefillWithAd);
            }
            
            if (closePopupButton != null)
            {
                closePopupButton.onClick.AddListener(CloseRefillPopup);
            }
            
            if (refillPopup != null)
            {
                refillPopup.SetActive(false);
            }
            
            UpdateEnergyDisplay(energySystem?.GetCurrentEnergy() ?? 0);
        }
        
        private void AutoAssignReferences()
        {
            energyText ??= FindChildComponent<TextMeshProUGUI>("EnergyText");
            regenTimerText ??= FindChildComponent<TextMeshProUGUI>("RegenTimerText");
            energyFillBar ??= FindChildComponent<Image>("EnergyFillBar");
            vipUnlimitedIcon ??= FindChildObject("VIPUnlimitedIcon");
            addEnergyButton ??= FindChildComponent<Button>("AddEnergyButton");

            refillPopup ??= FindChildObject("RefillPopup");
            refillCostText ??= FindChildComponent<TextMeshProUGUI>("RefillCostText");
            refillWithGemsButton ??= FindChildComponent<Button>("RefillWithGemsButton");
            watchAdButton ??= FindChildComponent<Button>("WatchAdButton");
            closePopupButton ??= FindChildComponent<Button>("ClosePopupButton");
        }

        private T FindChildComponent<T>(string name) where T : Component
        {
            Transform child = FindChildRecursive(transform, name);
            return child != null ? child.GetComponent<T>() : null;
        }

        private GameObject FindChildObject(string name)
        {
            Transform child = FindChildRecursive(transform, name);
            return child != null ? child.gameObject : null;
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
            UpdateRegenTimer();
        }
        
        /// <summary>
        /// Update energy display.
        /// </summary>
        private void UpdateEnergyDisplay(int currentEnergy)
        {
            if (energySystem == null) return;
            
            bool isVIP = vipManager?.IsVIPActive() ?? false;
            int maxEnergy = energySystem.GetMaxEnergy();
            
            // VIP unlimited display
            if (isVIP && vipUnlimitedIcon != null)
            {
                vipUnlimitedIcon.SetActive(true);
                if (energyText != null)
                {
                    energyText.text = "∞";
                    energyText.color = normalColor;
                }
            }
            else
            {
                if (vipUnlimitedIcon != null)
                {
                    vipUnlimitedIcon.SetActive(false);
                }
                
                if (energyText != null)
                {
                    energyText.text = $"{currentEnergy}/{maxEnergy}";
                    
                    // Color coding
                    if (currentEnergy <= lowEnergyThreshold)
                    {
                        energyText.color = lowEnergyColor;
                    }
                    else
                    {
                        energyText.color = normalColor;
                    }
                }
            }
            
            // Fill bar
            if (energyFillBar != null && !isVIP)
            {
                energyFillBar.fillAmount = (float)currentEnergy / maxEnergy;
            }
        }
        
        /// <summary>
        /// Update regeneration timer display.
        /// </summary>
        private void UpdateRegenTimer()
        {
            if (energySystem == null || regenTimerText == null) return;
            
            bool isVIP = vipManager?.IsVIPActive() ?? false;
            if (isVIP)
            {
                regenTimerText.text = "";
                return;
            }
            
            int currentEnergy = energySystem.GetCurrentEnergy();
            int maxEnergy = energySystem.GetMaxEnergy();
            
            // Only show timer if not at max
            if (currentEnergy < maxEnergy)
            {
                float secondsToNext = (float)energySystem.GetTimeUntilNextRegen().TotalSeconds;
                int minutes = Mathf.FloorToInt(secondsToNext / 60f);
                int seconds = Mathf.FloorToInt(secondsToNext % 60f);
                
                regenTimerText.text = $"+1 in {minutes:00}:{seconds:00}";
            }
            else
            {
                regenTimerText.text = "FULL";
            }
        }
        
        /// <summary>
        /// Show refill popup (called when out of energy or button pressed).
        /// </summary>
        private void ShowRefillPopup()
        {
            if (refillPopup == null) return;
            
            // Check if VIP (shouldn't happen, but safety)
            bool isVIP = vipManager?.IsVIPActive() ?? false;
            if (isVIP)
            {
                Debug.Log("VIP has unlimited energy!");
                return;
            }
            
            refillPopup.SetActive(true);
            
            // Update cost text (50 gems for full refill)
            if (refillCostText != null)
            {
                refillCostText.text = "50";
            }
            
            // Check if player has enough gems
            if (refillWithGemsButton != null && currencyManager != null)
            {
                bool canAfford = currencyManager.HasGems(50);
                refillWithGemsButton.interactable = canAfford;
            }
        }
        
        /// <summary>
        /// Close refill popup.
        /// </summary>
        private void CloseRefillPopup()
        {
            if (refillPopup != null)
            {
                refillPopup.SetActive(false);
            }
        }
        
        /// <summary>
        /// Refill energy with gems (50 gems = full refill).
        /// </summary>
        private void RefillWithGems()
        {
            if (currencyManager == null || energySystem == null) return;
            
            int cost = 50;
            
            if (currencyManager.TrySpendGems(cost, "Energy Refill"))
            {
                energySystem.RefillEnergy();
                CloseRefillPopup();
                Debug.Log("Energy refilled with gems!");
            }
            else
            {
                Debug.Log("Not enough gems!");
                // In production: Show "not enough gems" popup with shop link
            }
        }
        
        /// <summary>
        /// Refill energy with rewarded ad (+5 energy).
        /// </summary>
        private void RefillWithAd()
        {
            // In production: Show Unity Ads rewarded video
            // For now: Debug implementation
            
            if (energySystem != null)
            {
                energySystem.AddEnergy(5, "Rewarded Ad");
                CloseRefillPopup();
                Debug.Log("Energy refilled with ad!");
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (energySystem != null)
            {
                EnergySystem.OnEnergyChanged -= UpdateEnergyDisplay;
                EnergySystem.OnEnergyDepleted -= ShowRefillPopup;
            }
        }
    }
}








