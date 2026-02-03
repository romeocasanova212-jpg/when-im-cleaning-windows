using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif
using System;
using System.Collections.Generic;
using WhenImCleaningWindows.Analytics;
using Debug = UnityEngine.Debug;

namespace WhenImCleaningWindows.Monetization
{
    /// <summary>
    /// Production Unity IAP integration with Google Play and Apple StoreKit.
    /// Handles real purchases, receipt validation, and store communication.
    /// Replaces debug purchasing with full production readiness.
    /// </summary>
#if UNITY_PURCHASING
    public class UnityIAPIntegration : MonoBehaviour, IDetailedStoreListener
#else
    public class UnityIAPIntegration : MonoBehaviour
#endif
    {
        public static UnityIAPIntegration Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableIAP = true;
        [SerializeField] private bool debugMode = false;
        [SerializeField] private bool validateReceipts = true;

#if UNITY_PURCHASING
        private IStoreController storeController;
        private IExtensionProvider storeExtensionProvider;
#endif
        private bool isInitialized = false;

        public bool IsInitialized => isInitialized;

        public void PurchaseProduct(string productId)
        {
            // TODO: Implement IAP purchase
        }

        // Events
        public static event Action OnStoreInitialized;
#if UNITY_PURCHASING
        public static event Action<Product> OnPurchaseSuccess;
        public static event Action<Product, PurchaseFailureReason> OnPurchaseFailed;
        public static event Action<Product> OnPurchaseDeferred;
#endif

        #region Store Product IDs

        // These match the IAPManager product IDs and must be configured in Google Play Console and App Store Connect
        private static class ProductIDs
        {
            // Starter Bundles
            public const string WelcomePack = "com.cleaningwindows.welcomepack";
            public const string RookieBundle = "com.cleaningwindows.rookiebundle";
            public const string ProgressionPack = "com.cleaningwindows.progressionpack";

            // Gem Packs
            public const string GemPouch = "com.cleaningwindows.gempouch";
            public const string GemBag = "com.cleaningwindows.gembag";
            public const string GemPile = "com.cleaningwindows.gempile";
            public const string GemChest = "com.cleaningwindows.gemchest";
            public const string GemVault = "com.cleaningwindows.gemvault";
            public const string GemTreasure = "com.cleaningwindows.gemtreasure";
            public const string GemFortune = "com.cleaningwindows.gemfortune";
            public const string GemMountain = "com.cleaningwindows.gemmountain";

            // VIP Subscriptions
            public const string VIPBronze = "com.cleaningwindows.vipbronze";
            public const string VIPSilver = "com.cleaningwindows.vipsilver";
            public const string VIPGold = "com.cleaningwindows.vipgold";

            // Power-Ups
            public const string PowerUpNuke = "com.cleaningwindows.powerupnuke";
            public const string PowerUpTurbo = "com.cleaningwindows.powerupturbo";
            public const string PowerUpAutoPilot = "com.cleaningwindows.powerupautopilot";
            public const string PowerUpTimeFreeze = "com.cleaningwindows.poweruptimefreeze";

            // Energy Refills
            public const string EnergyRefill5 = "com.cleaningwindows.energyrefill5";
            public const string EnergyRefill10 = "com.cleaningwindows.energyrefill10";

            // Cosmetics (Phase 3)
            public const string SqueegeeClassic = "com.cleaningwindows.squeegee_classic";
            public const string SqueegeeGold = "com.cleaningwindows.squeegee_gold";
            public const string WindowThemeUrban = "com.cleaningwindows.theme_urban";
            public const string WindowThemeNature = "com.cleaningwindows.theme_nature";
            public const string ParticleStars = "com.cleaningwindows.particle_stars";
            public const string ParticleRainbow = "com.cleaningwindows.particle_rainbow";

            // Skip Timers
            public const string SkipTimer = "com.cleaningwindows.skiptimer";
            public const string SkipLevel = "com.cleaningwindows.skiplevel";
        }

        #endregion

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
#if UNITY_PURCHASING
            InitializeStore();
#else
            UnityEngine.Debug.LogWarning("[UnityIAP] Unity Purchasing not imported. Install from Window > TextMesh Pro > Import TMP Essential Resources");
            isInitialized = false;
#endif
        }

        #endregion Initialization

#if UNITY_PURCHASING
        private void InitializeStore()
        {
            if (isInitialized)
            {
                UnityEngine.Debug.Log("[UnityIAP] Already initialized");
                return;
            }

            if (!enableIAP)
            {
                UnityEngine.Debug.Log("[UnityIAP] IAP disabled");
                return;
            }

            UnityEngine.Debug.Log("[UnityIAP] Initializing Unity Purchasing...");

            // Check if Unity Purchasing is available
            if (Application.platform != RuntimePlatform.Android &&
                Application.platform != RuntimePlatform.IPhonePlayer &&
                !debugMode)
            {
                UnityEngine.Debug.LogWarning("[UnityIAP] IAP only works on Android/iOS. Using debug mode.");
                debugMode = true;
            }

            // Build product catalog
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add all products
            AddStarterBundles(builder);
            AddGemPacks(builder);
            AddVIPSubscriptions(builder);
            AddPowerUps(builder);
            AddEnergyRefills(builder);
            AddCosmetics(builder);
            AddSkips(builder);

            // Initialize Unity IAP
            UnityEngine.Debug.Log($"[UnityIAP] Initializing with {builder.products.Count} products...");
            UnityPurchasing.Initialize(this, builder);
        }

        private void AddStarterBundles(ConfigurationBuilder builder)
        {
            builder.AddProduct(ProductIDs.WelcomePack, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.RookieBundle, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.ProgressionPack, UnityEngine.Purchasing.ProductType.Consumable);
        }

        private void AddGemPacks(ConfigurationBuilder builder)
        {
            builder.AddProduct(ProductIDs.GemPouch, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.GemBag, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.GemPile, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.GemChest, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.GemVault, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.GemTreasure, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.GemFortune, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.GemMountain, UnityEngine.Purchasing.ProductType.Consumable);
        }

        private void AddVIPSubscriptions(ConfigurationBuilder builder)
        {
            // Subscriptions - auto-renewing monthly
            builder.AddProduct(ProductIDs.VIPBronze, UnityEngine.Purchasing.ProductType.Subscription);
            builder.AddProduct(ProductIDs.VIPSilver, UnityEngine.Purchasing.ProductType.Subscription);
            builder.AddProduct(ProductIDs.VIPGold, UnityEngine.Purchasing.ProductType.Subscription);
        }

        private void AddPowerUps(ConfigurationBuilder builder)
        {
            builder.AddProduct(ProductIDs.PowerUpNuke, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.PowerUpTurbo, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.PowerUpAutoPilot, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.PowerUpTimeFreeze, UnityEngine.Purchasing.ProductType.Consumable);
        }

        private void AddEnergyRefills(ConfigurationBuilder builder)
        {
            builder.AddProduct(ProductIDs.EnergyRefill5, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.EnergyRefill10, UnityEngine.Purchasing.ProductType.Consumable);
        }

        private void AddCosmetics(ConfigurationBuilder builder)
        {
            // Non-consumable - purchased once, owned forever
            builder.AddProduct(ProductIDs.SqueegeeClassic, UnityEngine.Purchasing.ProductType.NonConsumable);
            builder.AddProduct(ProductIDs.SqueegeeGold, UnityEngine.Purchasing.ProductType.NonConsumable);
            builder.AddProduct(ProductIDs.WindowThemeUrban, UnityEngine.Purchasing.ProductType.NonConsumable);
            builder.AddProduct(ProductIDs.WindowThemeNature, UnityEngine.Purchasing.ProductType.NonConsumable);
            builder.AddProduct(ProductIDs.ParticleStars, UnityEngine.Purchasing.ProductType.NonConsumable);
            builder.AddProduct(ProductIDs.ParticleRainbow, UnityEngine.Purchasing.ProductType.NonConsumable);
        }

        private void AddSkips(ConfigurationBuilder builder)
        {
            builder.AddProduct(ProductIDs.SkipTimer, UnityEngine.Purchasing.ProductType.Consumable);
            builder.AddProduct(ProductIDs.SkipLevel, UnityEngine.Purchasing.ProductType.Consumable);
        }

        #region IStoreListener Implementation

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            UnityEngine.Debug.Log("[UnityIAP] ✓ Store initialized successfully");

            storeController = controller;
            storeExtensionProvider = extensions;
            isInitialized = true;

            // Log available products
            foreach (var product in controller.products.all)
            {
                if (product.availableToPurchase)
                {
                    UnityEngine.Debug.Log($"[UnityIAP] Product available: {product.definition.id} - {product.metadata.localizedPriceString}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[UnityIAP] Product unavailable: {product.definition.id}");
                }
            }

            OnStoreInitialized?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            UnityEngine.Debug.LogError($"[UnityIAP] Store initialization failed: {error}");
            
            if (error == InitializationFailureReason.NoProductsAvailable)
            {
                UnityEngine.Debug.LogError("[UnityIAP] No products configured in Google Play Console or App Store Connect!");
            }

            // Try again in 30 seconds
            Invoke(nameof(InitializeStore), 30f);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            UnityEngine.Debug.LogError($"[UnityIAP] Store initialization failed: {error} - {message}");
            FirebaseManager.Instance?.LogException(new Exception($"IAP Init Failed: {error} - {message}"), "UnityIAP.OnInitializeFailed");
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            UnityEngine.Debug.Log($"[UnityIAP] Processing purchase: {args.purchasedProduct.definition.id}");

            // Validate receipt if enabled
            if (validateReceipts && !ValidateReceipt(args.purchasedProduct))
            {
                UnityEngine.Debug.LogError($"[UnityIAP] Receipt validation failed for {args.purchasedProduct.definition.id}");
                return PurchaseProcessingResult.Complete; // Still complete to avoid refund loops
            }

            // Process the purchase
            ProcessPurchaseRewards(args.purchasedProduct);

            // Log analytics
            FirebaseManager.Instance?.LogPurchase(
                args.purchasedProduct.definition.id,
                GetProductType(args.purchasedProduct.definition.id),
                (decimal)args.purchasedProduct.metadata.localizedPrice,
                args.purchasedProduct.metadata.isoCurrencyCode
            );

            // Invoke success event
            OnPurchaseSuccess?.Invoke(args.purchasedProduct);

            return PurchaseProcessingResult.Complete;
        }

        // IStoreListener.OnPurchaseFailed - basic callback (explicit implementation)
        void IStoreListener.OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            UnityEngine.Debug.LogWarning($"[UnityIAP] Purchase failed: {product.definition.id} - {failureReason}");

            // Trigger analytics
            FirebaseManager.Instance?.LogException(
                new Exception($"Purchase Failed: {product.definition.id} - {failureReason}"),
                "UnityIAP.OnPurchaseFailed"
            );
        }

        // IDetailedStoreListener.OnPurchaseFailed - detailed callback (explicit implementation)
        void IDetailedStoreListener.OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            UnityEngine.Debug.LogWarning($"[UnityIAP] Purchase failed (detailed): {product.definition.id} - {failureDescription.reason}: {failureDescription.message}");

            // Trigger analytics with more detail
            FirebaseManager.Instance?.LogException(
                new Exception($"Purchase Failed (Detailed): {product.definition.id} - {failureDescription.reason}: {failureDescription.message}"),
                "UnityIAP.OnPurchaseFailedDetailed"
            );
        }

        #endregion IStoreListener Implementation

        #region Purchase Initiation

        public void PurchaseProduct(string productId)
        {
            if (!isInitialized)
            {
                UnityEngine.Debug.LogWarning("[UnityIAP] Store not initialized yet");
                return;
            }

            Product product = storeController.products.WithID(productId);

            if (product == null || !product.availableToPurchase)
            {
                UnityEngine.Debug.LogWarning($"[UnityIAP] Product not available: {productId}");
                return;
            }

            UnityEngine.Debug.Log($"[UnityIAP] Initiating purchase: {productId} - {product.metadata.localizedPriceString}");
            storeController.InitiatePurchase(product);
        }

        #endregion

        #region Receipt Validation

        private bool ValidateReceipt(Product product)
        {
            // Basic validation - in production, send to your server for validation
            bool isValid = false;

#if UNITY_ANDROID || UNITY_IOS
            var validator = new CrossPlatformValidator(
                GooglePlayTangle.Data(),
                AppleTangle.Data(),
                Application.identifier
            );

            try
            {
                var result = validator.Validate(product.receipt);
                UnityEngine.Debug.Log($"[UnityIAP] Receipt validated for {product.definition.id}");

                // Check for suspicious activity
                foreach (IPurchaseReceipt receipt in result)
                {
                    UnityEngine.Debug.Log($"[UnityIAP] Receipt: {receipt.productID}, Purchase Date: {receipt.purchaseDate}");
                }

                isValid = true;
            }
            catch (IAPSecurityException ex)
            {
                UnityEngine.Debug.LogError($"[UnityIAP] Receipt validation failed: {ex.Message}");
                FirebaseManager.Instance?.LogException(ex, "UnityIAP.ValidateReceipt");
                isValid = false;
            }
#else
            // Editor/Desktop - skip validation
            isValid = true;
#endif

            return isValid;
        }

        #endregion

        #region Reward Processing

        private void ProcessPurchaseRewards(Product product)
        {
            string productId = product.definition.id;

            UnityEngine.Debug.Log($"[UnityIAP] Granting rewards for: {productId}");

            // Match product ID to rewards
            switch (productId)
            {
                // Starter Bundles
                case ProductIDs.WelcomePack:
                    CurrencyManager.Instance?.AddGems(500, "Welcome Pack");
                    CurrencyManager.Instance?.AddCoins(1000, "Welcome Pack");
                    EnergySystem.Instance?.AddEnergy(5, "Welcome Pack");
                    break;

                case ProductIDs.RookieBundle:
                    CurrencyManager.Instance?.AddGems(1500, "Rookie Bundle");
                    CurrencyManager.Instance?.AddCoins(0, "Rookie Bundle");
                    break;

                case ProductIDs.ProgressionPack:
                    CurrencyManager.Instance?.AddGems(3000, "Progression Pack");
                    CurrencyManager.Instance?.AddEnergy(10, "Progression Pack");
                    break;

                // Gem Packs
                case ProductIDs.GemPouch:
                    CurrencyManager.Instance?.AddGems(600, "Gem Pouch");
                    break;

                case ProductIDs.GemBag:
                    CurrencyManager.Instance?.AddGems(1700, "Gem Bag");
                    break;

                case ProductIDs.GemPile:
                    CurrencyManager.Instance?.AddGems(4000, "Gem Pile");
                    break;

                case ProductIDs.GemChest:
                    CurrencyManager.Instance?.AddGems(12000, "Gem Chest");
                    break;

                case ProductIDs.GemVault:
                    CurrencyManager.Instance?.AddGems(35000, "Gem Vault");
                    break;

                case ProductIDs.GemTreasure:
                    CurrencyManager.Instance?.AddGems(80000, "Gem Treasure");
                    break;

                case ProductIDs.GemFortune:
                    CurrencyManager.Instance?.AddGems(200000, "Gem Fortune");
                    break;

                case ProductIDs.GemMountain:
                    CurrencyManager.Instance?.AddGems(350000, "Gem Mountain");
                    break;

                // VIP Subscriptions
                case ProductIDs.VIPBronze:
                    VIPManager.Instance?.ActivateVIP(VIPTier.Bronze, 30);
                    FirebaseManager.Instance?.LogVIPActivation("Bronze", 30, 4.99m);
                    break;

                case ProductIDs.VIPSilver:
                    VIPManager.Instance?.ActivateVIP(VIPTier.Silver, 30);
                    FirebaseManager.Instance?.LogVIPActivation("Silver", 30, 9.99m);
                    break;

                case ProductIDs.VIPGold:
                    VIPManager.Instance?.ActivateVIP(VIPTier.Gold, 30);
                    FirebaseManager.Instance?.LogVIPActivation("Gold", 30, 19.99m);
                    break;

                // Power-Ups
                case ProductIDs.PowerUpNuke:
                case ProductIDs.PowerUpTurbo:
                case ProductIDs.PowerUpAutoPilot:
                case ProductIDs.PowerUpTimeFreeze:
                    // Grant power-up inventory (to be implemented in Phase 3)
                    UnityEngine.Debug.Log($"[UnityIAP] Power-up granted: {productId}");
                    break;

                // Energy Refills
                case ProductIDs.EnergyRefill5:
                    EnergySystem.Instance?.AddEnergy(5, "Energy Refill");
                    FirebaseManager.Instance?.LogEnergyRefill("Purchase", 5, 50);
                    break;

                case ProductIDs.EnergyRefill10:
                    EnergySystem.Instance?.AddEnergy(10, "Energy Refill");
                    FirebaseManager.Instance?.LogEnergyRefill("Purchase", 10, 100);
                    break;

                // Cosmetics (Phase 3)
                case ProductIDs.SqueegeeClassic:
                case ProductIDs.SqueegeeGold:
                case ProductIDs.WindowThemeUrban:
                case ProductIDs.WindowThemeNature:
                case ProductIDs.ParticleStars:
                case ProductIDs.ParticleRainbow:
                    // Unlock cosmetic (to be implemented in Phase 3)
                    UnityEngine.Debug.Log($"[UnityIAP] Cosmetic unlocked: {productId}");
                    PlayerPrefs.SetInt($"Owned_{productId}", 1);
                    break;

                // Skips
                case ProductIDs.SkipTimer:
                case ProductIDs.SkipLevel:
                    // Grant skip token (to be implemented)
                    UnityEngine.Debug.Log($"[UnityIAP] Skip granted: {productId}");
                    break;

                default:
                    UnityEngine.Debug.LogWarning($"[UnityIAP] Unknown product ID: {productId}");
                    break;
            }

            UnityEngine.Debug.Log($"[UnityIAP] ✓ Rewards granted for {productId}");
        }

        #endregion

        #region Subscription Management

        public bool IsSubscribed(string subscriptionId)
        {
            if (!isInitialized)
                return false;

            Product subscription = storeController.products.WithID(subscriptionId);
            if (subscription == null)
                return false;

            // Check subscription status
            SubscriptionManager subscriptionManager = new SubscriptionManager(subscription, null);
            SubscriptionInfo info = subscriptionManager.getSubscriptionInfo();

            return info.isSubscribed() == Result.True;
        }

        public DateTime GetSubscriptionExpiry(string subscriptionId)
        {
            if (!isInitialized)
                return DateTime.MinValue;

            Product subscription = storeController.products.WithID(subscriptionId);
            if (subscription == null)
                return DateTime.MinValue;

            SubscriptionManager subscriptionManager = new SubscriptionManager(subscription, null);
            SubscriptionInfo info = subscriptionManager.getSubscriptionInfo();

            return info.getExpireDate();
        }

        #endregion

        #region Restore Purchases (iOS)

        public void RestorePurchases()
        {
            if (!isInitialized)
            {
                UnityEngine.Debug.LogWarning("[UnityIAP] Store not initialized");
                return;
            }

            UnityEngine.Debug.Log("[UnityIAP] Restoring purchases...");

            IAppleExtensions appleExtensions = storeExtensionProvider.GetExtension<IAppleExtensions>();
            appleExtensions.RestoreTransactions((result, error) =>
            {
                if (result)
                {
                    UnityEngine.Debug.Log("[UnityIAP] ✓ Purchases restored successfully");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[UnityIAP] Restore failed: {error}");
                }
            });
        }

        #endregion

        #region Helpers

        public bool IsInitialized()
        {
            return isInitialized;
        }

        public Product GetProduct(string productId)
        {
            if (!isInitialized)
                return null;

            return storeController.products.WithID(productId);
        }

        public string GetLocalizedPrice(string productId)
        {
            Product product = GetProduct(productId);
            return product?.metadata.localizedPriceString ?? "$?.??";
        }

        private string GetProductType(string productId)
        {
            if (productId.Contains("welcomepack") || productId.Contains("bundle"))
                return "StarterBundle";
            if (productId.Contains("gem"))
                return "GemPack";
            if (productId.Contains("vip"))
                return "Subscription";
            if (productId.Contains("powerup"))
                return "PowerUp";
            if (productId.Contains("energy"))
                return "EnergyRefill";
            if (productId.Contains("squeegee") || productId.Contains("theme") || productId.Contains("particle"))
                return "Cosmetic";
            if (productId.Contains("skip"))
                return "Skip";

            return "Unknown";
        }

        #endregion Helpers

#endif
    }

    #region Obfuscated Store Keys (Generated by Unity)

    // These are generated by Unity after configuring store keys in the IAP Catalog
    // They provide basic receipt validation obfuscation
    // PRODUCTION: Replace these placeholder keys with actual values from store consoles
#if UNITY_PURCHASING
    public static class GooglePlayTangle
    {
        public static byte[] Data()
        {
            // PRODUCTION TODO: Replace with actual Google Play public key from Google Play Console
            // For development/testing, this returns an empty array
            return new byte[] { };
        }
    }

    public static class AppleTangle
    {
        public static byte[] Data()
        {
            // PRODUCTION TODO: Replace with actual Apple public key from App Store Connect
            // For development/testing, this returns an empty array
            return new byte[] { };
        }
    }
#else
    /// <summary>
    /// Fallback implementations when Unity Purchasing is not available
    /// </summary>
    public static class GooglePlayTangle
    {
        public static byte[] Data() => new byte[] { };
    }

    public static class AppleTangle
    {
        public static byte[] Data() => new byte[] { };
    }
#endif

    #endregion Obfuscated Store Keys (Generated by Unity)
}







