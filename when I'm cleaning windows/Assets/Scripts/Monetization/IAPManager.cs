using UnityEngine;
using System;
using System.Collections.Generic;

namespace WhenImCleaningWindows.Monetization
{
    /// <summary>
    /// IAP Product definition for the shop.
    /// </summary>
    [Serializable]
    public class IAPProduct
    {
        public string productId;
        public string displayName;
        public ProductType type;
        public float priceUSD;
        public int gemsReward;
        public int coinsReward;
        public int energyReward;
        public string bonusDescription;
        public int bonusPercentage; // For display "150% VALUE!"
    }
    
    /// <summary>
    /// Product types for organization.
    /// </summary>
    public enum ProductType
    {
        StarterBundle,
        GemPack,
        Subscription,
        PowerUp,
        Skip,
        Cosmetic
    }
    
    /// <summary>
    /// IAP Manager with 28 SKUs following 2026 polished monetization matrix.
    /// Integrates with Unity IAP (placeholder - would use Unity Purchasing in production).
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }
        
        [Header("Shop Configuration")]
        [SerializeField] private List<IAPProduct> products = new List<IAPProduct>();
        [SerializeField] private bool enableDebugPurchases = true;
        
        // Events
        public static event Action<IAPProduct> OnPurchaseComplete;
        public static event Action<IAPProduct> OnPurchaseFailed;
        public static event Action<string> OnPurchaseStarted;
        
        // Managers
        private CurrencyManager currencyManager;
        private EnergySystem energySystem;
        private UnityIAPIntegration unityIAPIntegration;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeProducts();
        }
        
        private void Start()
        {
            currencyManager = CurrencyManager.Instance;
            energySystem = FindFirstObjectByType<EnergySystem>();
            unityIAPIntegration = UnityIAPIntegration.Instance;
            
            // Subscribe to Unity IAP events if available
            if (unityIAPIntegration != null)
            {
#if UNITY_PURCHASING
                UnityIAPIntegration.OnPurchaseSuccess += OnUnityIAPPurchaseSuccess;
#endif
                UnityEngine.Debug.Log("IAPManager connected to UnityIAPIntegration for production purchases");
            }
            else
            {
                UnityEngine.Debug.Log("IAPManager running in debug mode (no Unity Purchasing)");
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from Unity IAP events
#if UNITY_PURCHASING
            if (unityIAPIntegration != null)
            {
                UnityIAPIntegration.OnPurchaseSuccess -= OnUnityIAPPurchaseSuccess;
            }
#endif
        }
        
        /// <summary>
        /// Initialize all 28 IAP products (2026 polished shop).
        /// </summary>
        private void InitializeProducts()
        {
            products.Clear();
            
            // === STARTER BUNDLES (High CVR Value Bombs) ===
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.welcomepack",
                displayName = "Welcome Pack",
                type = ProductType.StarterBundle,
                priceUSD = 0.99f,
                gemsReward = 500,
                energyReward = 5,
                coinsReward = 1000,
                bonusDescription = "BEST VALUE! +100%",
                bonusPercentage = 100
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.rookiebundle",
                displayName = "Rookie Bundle",
                type = ProductType.StarterBundle,
                priceUSD = 2.99f,
                gemsReward = 1500,
                energyReward = 0,
                bonusDescription = "VIP 3-Day Trial + Turbo",
                bonusPercentage = 150
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.progressionpack",
                displayName = "Progression Pack",
                type = ProductType.StarterBundle,
                priceUSD = 4.99f,
                gemsReward = 3000,
                energyReward = 10,
                bonusDescription = "Drone Lv1 Included",
                bonusPercentage = 180
            });
            
            // === GEM PACKS (Currency) ===
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.gempouch",
                displayName = "Gem Pouch",
                type = ProductType.GemPack,
                priceUSD = 1.99f,
                gemsReward = 600,
                bonusPercentage = 20
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.gembag",
                displayName = "Gem Bag",
                type = ProductType.GemPack,
                priceUSD = 4.99f,
                gemsReward = 1700,
                bonusPercentage = 40
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.gempile",
                displayName = "Gem Pile",
                type = ProductType.GemPack,
                priceUSD = 9.99f,
                gemsReward = 4000,
                bonusPercentage = 60
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.gemchest",
                displayName = "Gem Chest",
                type = ProductType.GemPack,
                priceUSD = 19.99f,
                gemsReward = 12000,
                bonusPercentage = 100
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.gemvault",
                displayName = "Gem Vault",
                type = ProductType.GemPack,
                priceUSD = 49.99f,
                gemsReward = 35000,
                bonusPercentage = 150
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.gemtreasure",
                displayName = "Gem Treasure",
                type = ProductType.GemPack,
                priceUSD = 99.99f,
                gemsReward = 80000,
                bonusPercentage = 180
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.gemfortune",
                displayName = "Gem Fortune",
                type = ProductType.GemPack,
                priceUSD = 199.99f,
                gemsReward = 200000,
                bonusPercentage = 220
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.gemmountain",
                displayName = "Gem Mountain",
                type = ProductType.GemPack,
                priceUSD = 299.99f,
                gemsReward = 350000,
                bonusPercentage = 250
            });
            
            // === ENERGY REFILLS ===
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.energyrefill5",
                displayName = "Energy Refill (5)",
                type = ProductType.PowerUp,
                priceUSD = 0.99f,
                energyReward = 5,
                bonusDescription = "Quick Refill - One More Try!"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.energyrefill10",
                displayName = "Energy Refill (10)",
                type = ProductType.PowerUp,
                priceUSD = 1.99f,
                energyReward = 10,
                bonusDescription = "Full Refill + 1 Bonus"
            });
            
            // === POWER-UPS ===
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.powerupnuke",
                displayName = "Power-Up: Nuke",
                type = ProductType.PowerUp,
                priceUSD = 0.99f,
                bonusDescription = "Instant Clear 50% Window"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.powerupturbo",
                displayName = "Power-Up: Turbo",
                type = ProductType.PowerUp,
                priceUSD = 0.99f,
                bonusDescription = "2× Speed for 30 Levels"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.powerupautopilot",
                displayName = "Power-Up: Auto-Pilot",
                type = ProductType.PowerUp,
                priceUSD = 1.99f,
                bonusDescription = "AI Clears Level Perfectly"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.poweruptimefreeze",
                displayName = "Power-Up: Time Freeze",
                type = ProductType.PowerUp,
                priceUSD = 1.99f,
                bonusDescription = "Pause Timer 10 Seconds"
            });
            
            // === SKIPS ===
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.skiptimer",
                displayName = "Skip Timer",
                type = ProductType.Skip,
                priceUSD = 0.99f,
                bonusDescription = "Skip Waiting for Energy"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.skiplevel",
                displayName = "Skip Level",
                type = ProductType.Skip,
                priceUSD = 1.99f,
                bonusDescription = "Skip Current Level"
            });
            
            // === COSMETICS (Phase 3) ===
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.squeegee_classic",
                displayName = "Squeegee: Classic",
                type = ProductType.Cosmetic,
                priceUSD = 2.99f,
                bonusDescription = "Timeless Design"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.squeegee_gold",
                displayName = "Squeegee: Gold",
                type = ProductType.Cosmetic,
                priceUSD = 4.99f,
                bonusDescription = "Premium Gold Finish"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.theme_urban",
                displayName = "Window Theme: Urban",
                type = ProductType.Cosmetic,
                priceUSD = 1.99f,
                bonusDescription = "Modern City Vibes"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.theme_nature",
                displayName = "Window Theme: Nature",
                type = ProductType.Cosmetic,
                priceUSD = 1.99f,
                bonusDescription = "Serene Natural Beauty"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.particle_stars",
                displayName = "Particles: Stars",
                type = ProductType.Cosmetic,
                priceUSD = 0.99f,
                bonusDescription = "Sparkling Star Effects"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.particle_rainbow",
                displayName = "Particles: Rainbow",
                type = ProductType.Cosmetic,
                priceUSD = 0.99f,
                bonusDescription = "Colorful Rainbow Effects"
            });
            
            // === SUBSCRIPTIONS (Managed by VIPManager) ===
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.vipbronze",
                displayName = "VIP Bronze",
                type = ProductType.Subscription,
                priceUSD = 4.99f,
                bonusDescription = "∞ Energy, 2.5× Rewards, +15% Speed"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.vipsilver",
                displayName = "VIP Silver",
                type = ProductType.Subscription,
                priceUSD = 9.99f,
                bonusDescription = "∞ Energy, 3× Rewards, +25% Speed, Skins"
            });
            
            products.Add(new IAPProduct
            {
                productId = "com.cleaningwindows.vipgold",
                displayName = "VIP Gold",
                type = ProductType.Subscription,
                priceUSD = 19.99f,
                bonusDescription = "∞ Energy, 4× Rewards, +40% Speed, Auto-Complete"
            });
            
            UnityEngine.Debug.Log($"Initialized {products.Count} IAP products");
        }
        
        /// <summary>
        /// Purchase a product (delegates to Unity IAP when available, or simulates in debug).
        /// </summary>
        public void PurchaseProduct(string productId)
        {
            IAPProduct product = products.Find(p => p.productId == productId);
            
            if (product == null)
            {
                UnityEngine.Debug.LogError($"Product not found: {productId}");
                OnPurchaseFailed?.Invoke(null);
                return;
            }
            
            OnPurchaseStarted?.Invoke(product.displayName);
            
            // Delegate to Unity IAP if available
            if (unityIAPIntegration != null && unityIAPIntegration.IsInitialized)
            {
                unityIAPIntegration.PurchaseProduct(productId);
            }
            // Otherwise simulate purchase in debug mode
            else if (enableDebugPurchases)
            {
                UnityEngine.Debug.Log($"[DEBUG] Simulating purchase: {product.displayName}");
                ProcessPurchaseSuccess(product);
            }
            else
            {
                UnityEngine.Debug.LogWarning("Unity IAP not initialized and debug purchases disabled");
                OnPurchaseFailed?.Invoke(product);
            }
        }
        
        /// <summary>
        /// Handle Unity IAP purchase success callback.
        /// </summary>
#if UNITY_PURCHASING
        private void OnUnityIAPPurchaseSuccess(UnityEngine.Purchasing.Product unityProduct)
        {
            // Map Unity IAP Product to our IAPProduct and process rewards
            IAPProduct product = products.Find(p => p.productId == unityProduct.definition.id);
            if (product != null)
            {
                ProcessPurchaseSuccess(product);
            }
            else
            {
                UnityEngine.Debug.LogError($"[IAPManager] Purchased product not found in catalog: {unityProduct.definition.id}");
            }
        }
#endif
        
        /// <summary>
        /// Process successful purchase (called by Unity IAP callback).
        /// </summary>
        public void ProcessPurchaseSuccess(IAPProduct product)
        {
            // Award currency
            if (product.gemsReward > 0)
            {
                currencyManager?.AddGems(product.gemsReward, product.displayName);
            }
            
            if (product.coinsReward > 0)
            {
                currencyManager?.AddCoins(product.coinsReward, product.displayName);
            }
            
            // Award energy
            if (product.energyReward > 0)
            {
                energySystem?.AddEnergy(product.energyReward, product.displayName);
            }
            
            // Handle special products
            switch (product.productId)
            {
                case "com.cleaningwindows.energyrefill10":
                    // Activate full energy refill
                    UnityEngine.Debug.Log("Energy refill activated!");
                    break;
                    
                case "com.cleaningwindows.vipbronze":
                case "com.cleaningwindows.vipsilver":
                case "com.cleaningwindows.vipgold":
                    // Activate VIP (handled by VIPManager)
                    break;
            }
            
            // Track purchase for analytics
            TrackPurchase(product);
            
            OnPurchaseComplete?.Invoke(product);
            
            UnityEngine.Debug.Log($"Purchase complete: {product.displayName} (£{product.priceUSD})");
        }
        
        /// <summary>
        /// Track purchase for analytics (Firebase, Unity Analytics).
        /// </summary>
        private void TrackPurchase(IAPProduct product)
        {
            // Firebase Analytics: LogPurchase event
            // Unity Analytics: Transaction event
            
            PlayerPrefs.SetFloat("LifetimeSpend", GetLifetimeSpend() + product.priceUSD);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Get lifetime spend (for whale detection).
        /// </summary>
        public float GetLifetimeSpend()
        {
            return PlayerPrefs.GetFloat("LifetimeSpend", 0f);
        }
        
        /// <summary>
        /// Get all products by type.
        /// </summary>
        public List<IAPProduct> GetProductsByType(ProductType type)
        {
            return products.FindAll(p => p.type == type);
        }
        
        /// <summary>
        /// Get product by ID.
        /// </summary>
        public IAPProduct GetProduct(string productId)
        {
            return products.Find(p => p.productId == productId);
        }
    }
}








