using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhenImCleaningWindows.Monetization;

namespace WhenImCleaningWindows.UI
{
    /// <summary>
    /// Offer Popup Controller - Shows dynamic offers triggered by PersonalizationEngine.
    /// Displays D1/D3/D7/D14/D30 offers with urgency messaging and discount percentages.
    /// </summary>
    public class OfferPopupUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject popupPanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI urgencyText;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private TextMeshProUGUI originalPriceText;
        [SerializeField] private TextMeshProUGUI discountBadgeText;
        [SerializeField] private TextMeshProUGUI rewardsText;
        [SerializeField] private Button buyButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Image productIcon;
        
        [Header("Animation")]
        [SerializeField] private float popDuration = 0.5f;
        [SerializeField] private AnimationCurve popCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private PersonalizationEngine personalizationEngine;
        private IAPManager iapManager;
        private OfferTrigger currentOffer;
        
        private void Start()
        {
            StartCoroutine(DeferredInitialization());
        }
        
        private System.Collections.IEnumerator DeferredInitialization()
        {
            yield return null;
            
            AutoAssignReferences();
            personalizationEngine = PersonalizationEngine.Instance;
            iapManager = IAPManager.Instance;
            
            // Subscribe to offer trigger event
            if (personalizationEngine != null)
            {
                PersonalizationEngine.OnOfferTriggered += ShowOffer;
            }
            
            // Setup buttons
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(OnBuyButtonClicked);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseOffer);
            }
            
            if (popupPanel != null)
            {
                popupPanel.SetActive(false);
            }
        }
        
        private void AutoAssignReferences()
        {
            popupPanel ??= FindChildObject("PopupPanel");
            titleText ??= FindChildComponent<TextMeshProUGUI>("TitleText");
            urgencyText ??= FindChildComponent<TextMeshProUGUI>("UrgencyText");
            priceText ??= FindChildComponent<TextMeshProUGUI>("PriceText");
            originalPriceText ??= FindChildComponent<TextMeshProUGUI>("OriginalPriceText");
            discountBadgeText ??= FindChildComponent<TextMeshProUGUI>("DiscountBadgeText");
            rewardsText ??= FindChildComponent<TextMeshProUGUI>("RewardsText");
            buyButton ??= FindChildComponent<Button>("BuyButton");
            closeButton ??= FindChildComponent<Button>("CloseButton");
            productIcon ??= FindChildComponent<Image>("ProductIcon");
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

        /// <summary>
        /// Show offer popup (triggered by PersonalizationEngine).
        /// </summary>
        private void ShowOffer(OfferTrigger offer)
        {
            if (popupPanel == null || iapManager == null) return;
            
            currentOffer = offer;
            
            // Get product details
            IAPProduct product = iapManager.GetProduct(offer.productId);
            if (product == null)
            {
                Debug.LogError($"Product not found: {offer.productId}");
                return;
            }
            
            // Calculate discounted price
            float originalPrice = product.priceUSD;
            float discountedPrice = originalPrice * (1f - offer.discountPercentage / 100f);
            
            // Update UI
            if (titleText != null)
            {
                titleText.text = GetOfferTitle(offer.dayTrigger);
            }
            
            if (urgencyText != null)
            {
                urgencyText.text = offer.urgencyMessage;
            }
            
            if (priceText != null)
            {
                priceText.text = $"${discountedPrice:F2}";
            }
            
            if (originalPriceText != null && offer.discountPercentage > 0)
            {
                originalPriceText.text = $"${originalPrice:F2}";
                originalPriceText.gameObject.SetActive(true);
            }
            else if (originalPriceText != null)
            {
                originalPriceText.gameObject.SetActive(false);
            }
            
            if (discountBadgeText != null && offer.discountPercentage > 0)
            {
                discountBadgeText.text = $"-{offer.discountPercentage:F0}%";
                discountBadgeText.gameObject.SetActive(true);
            }
            else if (discountBadgeText != null)
            {
                discountBadgeText.gameObject.SetActive(false);
            }
            
            if (rewardsText != null)
            {
                rewardsText.text = GetRewardsText(product);
            }
            
            // Show popup with animation
            popupPanel.SetActive(true);
            StartCoroutine(AnimatePopIn());
            
            Debug.Log($"Offer shown: {offer.offerId} ({offer.urgencyMessage})");
        }
        
        /// <summary>
        /// Get offer title based on day trigger.
        /// </summary>
        private string GetOfferTitle(int dayTrigger)
        {
            switch (dayTrigger)
            {
                case 1: return "WELCOME!";
                case 3: return "Special Offer!";
                case 7: return "Try VIP FREE!";
                case 14: return "We Miss You!";
                case 30: return "Thank You!";
                default: return "Limited Offer!";
            }
        }
        
        /// <summary>
        /// Get rewards text for display.
        /// </summary>
        private string GetRewardsText(IAPProduct product)
        {
            string rewards = "";
            
            if (product.gemsReward > 0)
            {
                rewards += $"{product.gemsReward} Gems\n";
            }
            
            if (product.coinsReward > 0)
            {
                rewards += $"{product.coinsReward} Coins\n";
            }
            
            if (product.energyReward > 0)
            {
                rewards += $"{product.energyReward} Energy\n";
            }
            
            if (!string.IsNullOrEmpty(product.bonusDescription))
            {
                rewards += product.bonusDescription;
            }
            
            return rewards.TrimEnd('\n');
        }
        
        /// <summary>
        /// Handle buy button click.
        /// </summary>
        private void OnBuyButtonClicked()
        {
            if (currentOffer == null || iapManager == null) return;
            
            iapManager.PurchaseProduct(currentOffer.productId);
            CloseOffer();
        }
        
        /// <summary>
        /// Close offer popup.
        /// </summary>
        private void CloseOffer()
        {
            if (popupPanel != null)
            {
                StartCoroutine(AnimatePopOut());
            }
        }
        
        /// <summary>
        /// Animate popup in (scale + fade).
        /// </summary>
        private System.Collections.IEnumerator AnimatePopIn()
        {
            RectTransform rectTransform = popupPanel.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = popupPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = popupPanel.AddComponent<CanvasGroup>();
            }
            
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float t = popCurve.Evaluate(elapsed / popDuration);
                
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                }
                canvasGroup.alpha = t;
                
                yield return null;
            }
            
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// Animate popup out (scale + fade).
        /// </summary>
        private System.Collections.IEnumerator AnimatePopOut()
        {
            RectTransform rectTransform = popupPanel.GetComponent<RectTransform>();
            CanvasGroup canvasGroup = popupPanel.GetComponent<CanvasGroup>();
            
            float elapsed = 0f;
            while (elapsed < popDuration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float t = 1f - (elapsed / (popDuration * 0.5f));
                
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
                }
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = t;
                }
                
                yield return null;
            }
            
            popupPanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (personalizationEngine != null)
            {
                PersonalizationEngine.OnOfferTriggered -= ShowOffer;
            }
        }
    }
}








