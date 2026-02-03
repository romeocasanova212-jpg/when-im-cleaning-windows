using UnityEngine;
using WhenImCleaningWindows.Core;
using WhenImCleaningWindows.Monetization;
using WhenImCleaningWindows.Procedural;
using WhenImCleaningWindows.Mechanics;
using WhenImCleaningWindows.UI;
using WhenImCleaningWindows.Audio;
using WhenImCleaningWindows.Visual;
using WhenImCleaningWindows.Analytics;
using WhenImCleaningWindows.CloudSave;
using Debug = UnityEngine.Debug;

namespace WhenImCleaningWindows
{
    /// <summary>
    /// Bootstrapper - Initializes all game systems in the correct order.
    /// Attach this to a GameObject in the initial scene (e.g., "Bootstrapper" or "_GameSystems").
    /// This ensures all singleton managers are created and initialized before gameplay begins.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Initialization Settings")]
        [SerializeField] private bool autoInitialize = true;
        [SerializeField] private bool showDebugLogs = true;
        
        [Header("Prefab References (Drag from Project)")]
        [SerializeField] private GameObject energySystemPrefab;
        [SerializeField] private GameObject currencyManagerPrefab;
        [SerializeField] private GameObject iapManagerPrefab;
        [SerializeField] private GameObject unityIAPIntegrationPrefab;
        [SerializeField] private GameObject vipManagerPrefab;
        [SerializeField] private GameObject personalizationEnginePrefab;
        [SerializeField] private GameObject levelGeneratorPrefab;
        [SerializeField] private GameObject hazardSystemPrefab;
        [SerializeField] private GameObject gameManagerPrefab;
        [SerializeField] private GameObject uiManagerPrefab;
        [SerializeField] private GameObject audioManagerPrefab;
        [SerializeField] private GameObject firebaseManagerPrefab;
        [SerializeField] private GameObject remoteConfigManagerPrefab;
        [SerializeField] private GameObject cloudSaveManagerPrefab;
        [SerializeField] private GameObject textureManagerPrefab;
        [SerializeField] private GameObject vfxManagerPrefab;
        [SerializeField] private GameObject animationManagerPrefab;
        
        [Header("Status")]
        [SerializeField] private bool isInitialized = false;
        [SerializeField] private float initializationTime = 0f;
        
        private static Bootstrapper instance;
        
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            if (autoInitialize)
            {
                InitializeSystems();
            }
        }
        
        /// <summary>
        /// Initialize all game systems in the correct dependency order.
        /// </summary>
        [ContextMenu("Initialize All Systems")]
        public void InitializeSystems()
        {
            if (isInitialized)
            {
                if (showDebugLogs) Debug.LogWarning("Bootstrapper: Already initialized!");
                return;
            }
            
            float startTime = Time.realtimeSinceStartup;
            
            if (showDebugLogs) Debug.Log("=== BOOTSTRAPPER: Initializing Game Systems ===");
            
            // Phase 0: Firebase Services (foundation for analytics)
            InitializeFirebase();
            InitializeRemoteConfig();
            InitializeCloudSave();
            
            // Phase 1: Core Systems (no dependencies)
            InitializeEnergySystem();
            InitializeCurrencyManager();
            InitializeHazardSystem();
            InitializeAudioManager();
            InitializeTextureManager();
            InitializeVFXManager();
            InitializeAnimationManager();
            
            // Phase 2: Managers (depend on Phase 1)
            InitializeIAPManager();
            InitializeUnityIAP();
            InitializeVIPManager();
            InitializePersonalizationEngine();
            
            // Phase 3: Generation (depends on Phase 1 & 2)
            InitializeLevelGenerator();
            
            // Phase 4: UI (depends on all previous)
            InitializeUIManager();
            
            // Phase 5: Game Controller (depends on everything)
            InitializeGameManager();
            
            initializationTime = Time.realtimeSinceStartup - startTime;
            isInitialized = true;
            
            if (showDebugLogs)
            {
                Debug.Log($"=== BOOTSTRAPPER: Initialization Complete ({initializationTime * 1000f:F2}ms) ===");
                PrintSystemStatus();
            }
        }
        
        /// <summary>
        /// Initialize Energy System.
        /// </summary>
        private void InitializeEnergySystem()
        {
            if (EnergySystem.Instance == null)
            {
                if (energySystemPrefab != null)
                {
                    Instantiate(energySystemPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("EnergySystem");
                    obj.AddComponent<EnergySystem>();
                }
                
                if (showDebugLogs) Debug.Log("✓ EnergySystem initialized");
            }
        }
        
        /// <summary>
        /// Initialize Currency Manager.
        /// </summary>
        private void InitializeCurrencyManager()
        {
            if (CurrencyManager.Instance == null)
            {
                if (currencyManagerPrefab != null)
                {
                    Instantiate(currencyManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("CurrencyManager");
                    obj.AddComponent<CurrencyManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ CurrencyManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize IAP Manager.
        /// </summary>
        private void InitializeIAPManager()
        {
            if (IAPManager.Instance == null)
            {
                if (iapManagerPrefab != null)
                {
                    Instantiate(iapManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("IAPManager");
                    obj.AddComponent<IAPManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ IAPManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize VIP Manager.
        /// </summary>
        private void InitializeVIPManager()
        {
            if (VIPManager.Instance == null)
            {
                if (vipManagerPrefab != null)
                {
                    Instantiate(vipManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("VIPManager");
                    obj.AddComponent<VIPManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ VIPManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize Personalization Engine.
        /// </summary>
        private void InitializePersonalizationEngine()
        {
            if (PersonalizationEngine.Instance == null)
            {
                if (personalizationEnginePrefab != null)
                {
                    Instantiate(personalizationEnginePrefab);
                }
                else
                {
                    GameObject obj = new GameObject("PersonalizationEngine");
                    obj.AddComponent<PersonalizationEngine>();
                }
                
                if (showDebugLogs) Debug.Log("✓ PersonalizationEngine initialized");
            }
        }
        
        /// <summary>
        /// Initialize Level Generator.
        /// </summary>
        private void InitializeLevelGenerator()
        {
            if (LevelGenerator.Instance == null)
            {
                if (levelGeneratorPrefab != null)
                {
                    Instantiate(levelGeneratorPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("LevelGenerator");
                    obj.AddComponent<LevelGenerator>();
                }
                
                if (showDebugLogs) Debug.Log("✓ LevelGenerator initialized");
            }
        }
        
        /// <summary>
        /// Initialize Hazard System.
        /// </summary>
        private void InitializeHazardSystem()
        {
            if (HazardSystem.Instance == null)
            {
                if (hazardSystemPrefab != null)
                {
                    Instantiate(hazardSystemPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("HazardSystem");
                    obj.AddComponent<HazardSystem>();
                }
                
                if (showDebugLogs) Debug.Log("✓ HazardSystem initialized");
            }
        }
        
        /// <summary>
        /// Initialize Game Manager.
        /// </summary>
        private void InitializeGameManager()
        {
            if (GameManager.Instance == null)
            {
                if (gameManagerPrefab != null)
                {
                    Instantiate(gameManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("GameManager");
                    obj.AddComponent<GameManager>();
                    obj.AddComponent<TimerSystem>();  // Timer system needed by GameManager
                }
                
                if (showDebugLogs) Debug.Log("✓ GameManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize UI Manager.
        /// </summary>
        private void InitializeUIManager()
        {
            if (UIManager.Instance == null)
            {
                if (uiManagerPrefab != null)
                {
                    Instantiate(uiManagerPrefab);
                }
                else
                {
                    Canvas canvas = FindFirstObjectByType<Canvas>();
                    if (canvas != null)
                    {
                        GameObject obj = new GameObject("UIManager");
                        obj.AddComponent<UIManager>();
                    }
                    else
                    {
                        // UI Manager requires Canvas setup - log warning
                        Debug.LogWarning("UIManager prefab not assigned and no Canvas found. Create UI Canvas manually.");
                    }
                }
                
                if (showDebugLogs) Debug.Log("✓ UIManager initialized (or skipped)");
            }
        }
        
        /// <summary>
        /// Initialize Audio Manager.
        /// </summary>
        private void InitializeAudioManager()
        {
            if (AudioManager.Instance == null)
            {
                if (audioManagerPrefab != null)
                {
                    Instantiate(audioManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("AudioManager");
                    obj.AddComponent<AudioManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ AudioManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize Firebase Analytics, Crashlytics, and ML Kit.
        /// </summary>
        private void InitializeFirebase()
        {
            if (FirebaseManager.Instance == null)
            {
                if (firebaseManagerPrefab != null)
                {
                    Instantiate(firebaseManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("FirebaseManager");
                    obj.AddComponent<FirebaseManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ FirebaseManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize Firebase Remote Config for live balancing.
        /// </summary>
        private void InitializeRemoteConfig()
        {
            if (RemoteConfigManager.Instance == null)
            {
                if (remoteConfigManagerPrefab != null)
                {
                    Instantiate(remoteConfigManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("RemoteConfigManager");
                    obj.AddComponent<RemoteConfigManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ RemoteConfigManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize Cloud Save for cross-device progression.
        /// </summary>
        private void InitializeCloudSave()
        {
            if (CloudSaveManager.Instance == null)
            {
                if (cloudSaveManagerPrefab != null)
                {
                    Instantiate(cloudSaveManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("CloudSaveManager");
                    obj.AddComponent<CloudSaveManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ CloudSaveManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize Unity IAP for real purchases.
        /// </summary>
        private void InitializeUnityIAP()
        {
            if (UnityIAPIntegration.Instance == null)
            {
                if (unityIAPIntegrationPrefab != null)
                {
                    Instantiate(unityIAPIntegrationPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("UnityIAPIntegration");
                    obj.AddComponent<UnityIAPIntegration>();
                }
                
                if (showDebugLogs) Debug.Log("✓ UnityIAPIntegration initialized");
            }
        }
        
        /// <summary>
        /// Initialize Texture Manager for hazard visuals and window frames.
        /// </summary>
        private void InitializeTextureManager()
        {
            if (TextureManager.Instance == null)
            {
                if (textureManagerPrefab != null)
                {
                    Instantiate(textureManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("TextureManager");
                    obj.AddComponent<TextureManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ TextureManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize VFX Manager for particle effects.
        /// </summary>
        private void InitializeVFXManager()
        {
            if (VFXManager.Instance == null)
            {
                if (vfxManagerPrefab != null)
                {
                    Instantiate(vfxManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("VFXManager");
                    obj.AddComponent<VFXManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ VFXManager initialized");
            }
        }
        
        /// <summary>
        /// Initialize Animation Manager for UI animations.
        /// </summary>
        private void InitializeAnimationManager()
        {
            if (AnimationManager.Instance == null)
            {
                if (animationManagerPrefab != null)
                {
                    Instantiate(animationManagerPrefab);
                }
                else
                {
                    GameObject obj = new GameObject("AnimationManager");
                    obj.AddComponent<AnimationManager>();
                }
                
                if (showDebugLogs) Debug.Log("✓ AnimationManager initialized");
            }
        }
        
        /// <summary>
        /// Print status of all systems.
        /// </summary>
        private void PrintSystemStatus()
        {
            if (showDebugLogs)
            {
                Debug.Log("\n=== SYSTEM STATUS ===");
                Debug.Log($"FirebaseManager: {(FirebaseManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"RemoteConfigManager: {(RemoteConfigManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"CloudSaveManager: {(CloudSaveManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"EnergySystem: {(EnergySystem.Instance != null ? "✓" : "✗")}");
                Debug.Log($"CurrencyManager: {(CurrencyManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"IAPManager: {(IAPManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"UnityIAPIntegration: {(UnityIAPIntegration.Instance != null ? "✓" : "✗")}");
                Debug.Log($"VIPManager: {(VIPManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"PersonalizationEngine: {(PersonalizationEngine.Instance != null ? "✓" : "✗")}");
                Debug.Log($"LevelGenerator: {(LevelGenerator.Instance != null ? "✓" : "✗")}");
                Debug.Log($"HazardSystem: {(HazardSystem.Instance != null ? "✓" : "✗")}");
                Debug.Log($"GameManager: {(GameManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"UIManager: {(UIManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"AudioManager: {(AudioManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"TextureManager: {(TextureManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"VFXManager: {(VFXManager.Instance != null ? "✓" : "✗")}");
                Debug.Log($"AnimationManager: {(AnimationManager.Instance != null ? "✓" : "✗")}");
                Debug.Log("====================\n");
            }
        }
        
        /// <summary>
        /// Get initialization status.
        /// </summary>
        public bool IsInitialized()
        {
            return isInitialized;
        }
        
        /// <summary>
        /// Get initialization time.
        /// </summary>
        public float GetInitializationTime()
        {
            return initializationTime;
        }
    }
}








