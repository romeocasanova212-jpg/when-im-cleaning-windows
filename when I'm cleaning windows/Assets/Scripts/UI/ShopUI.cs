using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using WhenImCleaningWindows.Monetization;

namespace WhenImCleaningWindows.UI
{
    /// <summary>
    /// Shop UI Controller - Displays 28 SKU grid with tabs, VIP tiers, and purchase buttons.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform shopItemContainer;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private TextMeshProUGUI gemsCountText;
        [SerializeField] private TextMeshProUGUI coinsCountText;
        
        [Header("Tab Buttons")]
        [SerializeField] private Button starterBundlesTab;
        [SerializeField] private Button gemPacksTab;
        [SerializeField] private Button powerUpsTab;
        [SerializeField] private Button vipTab;
        [SerializeField] private Button cosmeticsTab;
        
        [Header("Featured Offer")]
        [SerializeField] private GameObject featuredOfferPanel;
        [SerializeField] private TextMeshProUGUI featuredTitleText;
        [SerializeField] private TextMeshProUGUI featuredPriceText;
        [SerializeField] private TextMeshProUGUI featuredBonusText;
        [SerializeField] private Button featuredBuyButton;
        
        private IAPManager iapManager;
        private CurrencyManager currencyManager;
        private VIPManager vipManager;
        private ProductType currentTab = ProductType.StarterBundle;
        
        private void Start()
        {
            StartCoroutine(DeferredInitialization());
        }
        
        private System.Collections.IEnumerator DeferredInitialization()
        {
            yield return null;
            
            AutoAssignReferences();
            iapManager = IAPManager.Instance;
            currencyManager = CurrencyManager.Instance;
            vipManager = VIPManager.Instance;
            
            // Subscribe to currency events
            if (currencyManager != null)
            {
                CurrencyManager.OnGemsChanged += UpdateCurrencyDisplay;
                CurrencyManager.OnCoinsChanged += UpdateCurrencyDisplay;
            }
            
            // Subscribe to purchase events
            if (iapManager != null)
            {
                IAPManager.OnPurchaseComplete += OnPurchaseComplete;
            }
            
            // Setup tab buttons
            if (starterBundlesTab != null) starterBundlesTab.onClick.AddListener(() => ShowTab(ProductType.StarterBundle));
            if (gemPacksTab != null) gemPacksTab.onClick.AddListener(() => ShowTab(ProductType.GemPack));
            if (powerUpsTab != null) powerUpsTab.onClick.AddListener(() => ShowTab(ProductType.PowerUp));
            if (vipTab != null) vipTab.onClick.AddListener(() => ShowTab(ProductType.Subscription));
            if (cosmeticsTab != null) cosmeticsTab.onClick.AddListener(() => ShowTab(ProductType.Cosmetic));
            
            UpdateCurrencyDisplay(0);
            ShowTab(ProductType.StarterBundle);
            ShowFeaturedOffer();
        }
        
        /// <summary>
        /// Update currency display.
        /// </summary>
        private void UpdateCurrencyDisplay(int _)
        {
            if (currencyManager == null) return;
            
            if (gemsCountText != null)
            {
                gemsCountText.text = currencyManager.GetGems().ToString("N0");
            }
            
            if (coinsCountText != null)
            {
                coinsCountText.text = currencyManager.GetCoins().ToString("N0");
            }
        }
        
        /// <summary>
        /// Show products for a specific tab.
        /// </summary>
        private void ShowTab(ProductType type)
        {
            currentTab = type;

            if (shopItemContainer == null || iapManager == null)
            {
                Debug.LogWarning("ShopUI: Missing container or IAPManager.");
                return;
            }
            
            // Clear existing items
            foreach (Transform child in shopItemContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Get products for this type
            List<IAPProduct> products = iapManager.GetProductsByType(type);
            
            // Create shop items
            foreach (var product in products)
            {
                CreateShopItem(product);
            }
            
            Debug.Log($"Shop: Showing {products.Count} products for {type}");
        }
        
        private void AutoAssignReferences()
        {
            shopItemContainer ??= FindChildTransform("ShopItemContainer");
            shopItemPrefab ??= FindChildObject("ShopItemTemplate");
            gemsCountText ??= FindChildComponent<TextMeshProUGUI>("GemsCountText");
            coinsCountText ??= FindChildComponent<TextMeshProUGUI>("CoinsCountText");

            starterBundlesTab ??= FindChildComponent<Button>("StarterBundlesTab");
            gemPacksTab ??= FindChildComponent<Button>("GemPacksTab");
            powerUpsTab ??= FindChildComponent<Button>("PowerUpsTab");
            vipTab ??= FindChildComponent<Button>("VIPTab");
            cosmeticsTab ??= FindChildComponent<Button>("CosmeticsTab");

            featuredOfferPanel ??= FindChildObject("FeaturedOfferPanel");
            featuredTitleText ??= FindChildComponent<TextMeshProUGUI>("FeaturedTitleText");
            featuredPriceText ??= FindChildComponent<TextMeshProUGUI>("FeaturedPriceText");
            featuredBonusText ??= FindChildComponent<TextMeshProUGUI>("FeaturedBonusText");
            featuredBuyButton ??= FindChildComponent<Button>("FeaturedBuyButton");
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

        private Transform FindChildTransform(string name)
        {
            return FindChildRecursive(transform, name);
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
        /// Create a shop item UI element.
        /// </summary>
        private void CreateShopItem(IAPProduct product)
        {
            if (shopItemContainer == null) return;

            GameObject item = shopItemPrefab != null
                ? Instantiate(shopItemPrefab, shopItemContainer)
                : CreateFallbackShopItem();
            
            // Setup UI elements (assuming prefab has these components)
            TextMeshProUGUI nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI priceText = item.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI bonusText = item.transform.Find("BonusText")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI descText = item.transform.Find("DescText")?.GetComponent<TextMeshProUGUI>();
            Button buyButton = item.transform.Find("BuyButton")?.GetComponent<Button>();
            
            if (nameText != null) nameText.text = product.displayName;
            if (priceText != null) priceText.text = $"£{product.priceUSD:F2}";
            if (bonusText != null && product.bonusPercentage > 0)
            {
                bonusText.text = $"+{product.bonusPercentage}%";
                bonusText.gameObject.SetActive(true);
            }
            if (descText != null && !string.IsNullOrEmpty(product.bonusDescription))
            {
                descText.text = product.bonusDescription;
            }
            
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(() => PurchaseProduct(product));
            }
        }

        private GameObject CreateFallbackShopItem()
        {
            GameObject root = new GameObject("ShopItem");
            root.transform.SetParent(shopItemContainer, false);

            RectTransform rt = root.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(300, 140);

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.3f);

            CreateTMP(root.transform, "NameText", "Item", 18, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10));
            CreateTMP(root.transform, "PriceText", "£0.99", 16, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10));
            CreateTMP(root.transform, "BonusText", "+0%", 14, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10, -10));
            CreateTMP(root.transform, "DescText", "", 12, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 10));

            GameObject buyButtonObj = CreateButton(root.transform, "BuyButton", "Buy", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-50, 20), new Vector2(80, 30));
            return root;
        }

        private TextMeshProUGUI CreateTMP(Transform parent, string name, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            return tmp;
        }

        private GameObject CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.6f, 0.9f, 0.9f);

            Button btn = go.AddComponent<Button>();

            TextMeshProUGUI tmp = CreateTMP(go.transform, "Label", label, 14, Vector2.zero, Vector2.one, Vector2.zero);
            tmp.alignment = TextAlignmentOptions.Center;

            return go;
        }
        
        /// <summary>
        /// Purchase a product.
        /// </summary>
        private void PurchaseProduct(IAPProduct product)
        {
            if (iapManager == null) return;
            
            iapManager.PurchaseProduct(product.productId);
            
            Debug.Log($"Shop: Purchasing {product.displayName} (£{product.priceUSD})");
        }
        
        /// <summary>
        /// Handle purchase complete.
        /// </summary>
        private void OnPurchaseComplete(IAPProduct product)
        {
            Debug.Log($"Shop: Purchase complete - {product.displayName}");
            
            // In production: Show success popup with rewards
            // For now: Just log
        }
        
        /// <summary>
        /// Show featured offer (Welcome Pack on D1).
        /// </summary>
        private void ShowFeaturedOffer()
        {
            if (featuredOfferPanel == null || iapManager == null) return;
            
            // Get Welcome Pack
            IAPProduct welcomePack = iapManager.GetProduct("com.cleaningwindows.welcomepack");
            
            if (welcomePack != null)
            {
                featuredOfferPanel.SetActive(true);
                
                if (featuredTitleText != null)
                {
                    featuredTitleText.text = welcomePack.displayName;
                }
                
                if (featuredPriceText != null)
                {
                    featuredPriceText.text = $"£{welcomePack.priceUSD:F2}";
                }
                
                if (featuredBonusText != null)
                {
                    featuredBonusText.text = welcomePack.bonusDescription;
                }
                
                if (featuredBuyButton != null)
                {
                    featuredBuyButton.onClick.RemoveAllListeners();
                    featuredBuyButton.onClick.AddListener(() => PurchaseProduct(welcomePack));
                }
            }
            else
            {
                featuredOfferPanel.SetActive(false);
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (currencyManager != null)
            {
                CurrencyManager.OnGemsChanged -= UpdateCurrencyDisplay;
                CurrencyManager.OnCoinsChanged -= UpdateCurrencyDisplay;
            }
            
            if (iapManager != null)
            {
                IAPManager.OnPurchaseComplete -= OnPurchaseComplete;
            }
        }
    }
}








