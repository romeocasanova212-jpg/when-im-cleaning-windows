using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using WhenImCleaningWindows.Core;
using WhenImCleaningWindows.Gameplay;
using WhenImCleaningWindows.Mechanics;
using WhenImCleaningWindows.Debugging;
using WhenImCleaningWindows.Visual;
using WhenImCleaningWindows.Monetization;
using Debug = UnityEngine.Debug;

namespace WhenImCleaningWindows.Editor
{
    /// <summary>
    /// Scene Setup Utility - Automatically creates the complete MainGame scene hierarchy.
    /// Use this to quickly set up a playable test scene with all necessary components.
    /// 
    /// USAGE:
    /// 1. Tools → When I'm Cleaning Windows → Setup MainGame Scene
    /// 2. Wait for hierarchy creation (~5 seconds)
    /// 3. Press Play to test gameplay
    /// </summary>
    public class SceneSetupUtility : EditorWindow
    {
        private bool createCamera = true;
        private bool createLighting = true;
        private bool createGameSystems = true;
        private bool createWindow = true;
        private bool createUI = true;
        private bool createDebugTools = true;
        
        private Vector2 scrollPosition;
        
        [MenuItem("Tools/When I'm Cleaning Windows/Setup MainGame Scene")]
        public static void ShowWindow()
        {
            SceneSetupUtility window = GetWindow<SceneSetupUtility>("Scene Setup");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        [MenuItem("Tools/When I'm Cleaning Windows/Setup Project (Full)")]
        public static void SetupProjectFull()
        {
            ConfigAssetCreator.CreateDefaultConfigAssets();
            PrefabCreator.CreateAllTestPrefabs();
            CreateMainGameSceneDefault();
        }

        public static void CreateMainGameSceneDefault()
        {
            SceneSetupUtility setup = CreateInstance<SceneSetupUtility>();
            setup.createCamera = true;
            setup.createLighting = true;
            setup.createGameSystems = true;
            setup.createWindow = true;
            setup.createUI = true;
            setup.createDebugTools = true;
            setup.CreateMainGameScene();
            DestroyImmediate(setup);
        }
        
        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            GUILayout.Label("MainGame Scene Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox(
                "This utility will create a complete playable scene with all necessary GameObjects and components.\n\n" +
                "Current scene will be saved before setup.",
                MessageType.Info
            );
            
            GUILayout.Space(10);
            
            GUILayout.Label("Components to Create:", EditorStyles.boldLabel);
            createCamera = EditorGUILayout.ToggleLeft("Camera (Main Camera)", createCamera);
            createLighting = EditorGUILayout.ToggleLeft("Lighting (Directional Light)", createLighting);
            createGameSystems = EditorGUILayout.ToggleLeft("Game Systems (_GameSystems + Bootstrapper)", createGameSystems);
            createWindow = EditorGUILayout.ToggleLeft("Window (WindowMesh + Hazards + Cleaning)", createWindow);
            createUI = EditorGUILayout.ToggleLeft("UI (Canvas + Event System)", createUI);
            createDebugTools = EditorGUILayout.ToggleLeft("Debug Tools (Test Manager + Input Debugger)", createDebugTools);
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Create MainGame Scene", GUILayout.Height(40)))
            {
                CreateMainGameScene();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Clear Current Scene (Destructive!)", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                    "Clear Scene?",
                    "This will delete all GameObjects in the current scene. Are you sure?",
                    "Yes, Clear",
                    "Cancel"))
                {
                    ClearCurrentScene();
                }
            }
            
            GUILayout.Space(20);
            
            GUILayout.Label("Quick Actions:", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Find Missing Components"))
            {
                FindMissingComponents();
            }
            
            if (GUILayout.Button("Validate Scene Setup"))
            {
                ValidateSceneSetup();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void CreateMainGameScene()
        {
            // Save current scene
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                if (!EditorUtility.DisplayDialog(
                    "Save Current Scene?",
                    "Current scene has unsaved changes. Save before creating new scene?",
                    "Save",
                    "Don't Save"))
                {
                    return;
                }
                EditorSceneManager.SaveOpenScenes();
            }
            
            Debug.Log("[SceneSetup] Creating MainGame scene...");
            
            // Create new scene or clear current
            Scene currentScene = EditorSceneManager.GetActiveScene();
            if (currentScene.name != "MainGame")
            {
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                newScene.name = "MainGame";
            }
            
            ClearCurrentScene();
            
            // Create scene hierarchy
            int objectsCreated = 0;
            
            if (createCamera)
            {
                objectsCreated += CreateCamera();
            }
            
            if (createLighting)
            {
                objectsCreated += CreateLighting();
            }
            
            if (createGameSystems)
            {
                objectsCreated += CreateGameSystems();
            }
            
            if (createWindow)
            {
                objectsCreated += CreateWindow();
            }
            
            if (createUI)
            {
                objectsCreated += CreateUI();
            }
            
            if (createDebugTools)
            {
                objectsCreated += CreateDebugTools();
            }
            
            // Save scene
            string scenePath = "Assets/Scenes/MainGame.unity";
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), scenePath);
            
            Debug.Log($"[SceneSetup] ✓ Scene created successfully! {objectsCreated} GameObjects created.");
            Debug.Log($"[SceneSetup] Scene saved to: {scenePath}");
            Debug.Log($"[SceneSetup] Press PLAY to test the game!");
            
            EditorUtility.DisplayDialog(
                "Scene Setup Complete!",
                $"MainGame scene created with {objectsCreated} GameObjects.\n\n" +
                "Press Play to test the gameplay loop!\n\n" +
                "Tips:\n" +
                "- F1: Toggle Level Test Manager\n" +
                "- ` (backtick): Debug Console\n" +
                "- Drag mouse to clean window",
                "Got it!"
            );
        }
        
        private int CreateCamera()
        {
            GameObject cameraObj = new GameObject("Main Camera");
            Camera camera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
            cameraObj.tag = "MainCamera";
            
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.2f, 0.3f, 0.4f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            
            cameraObj.transform.position = new Vector3(0f, 0f, -10f);
            
            Debug.Log("[SceneSetup] Created Main Camera (orthographic, size=5)");
            return 1;
        }
        
        private int CreateLighting()
        {
            GameObject lightObj = new GameObject("Directional Light");
            Light light = lightObj.AddComponent<Light>();
            
            light.type = LightType.Directional;
            light.color = Color.white;
            light.intensity = 1f;
            light.shadows = LightShadows.Soft;
            
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            
            Debug.Log("[SceneSetup] Created Directional Light");
            return 1;
        }
        
        private int CreateGameSystems()
        {
            GameObject systemsObj = new GameObject("_GameSystems");
            
            // Add Bootstrapper (will initialize all other systems automatically)
            Bootstrapper bootstrapper = systemsObj.AddComponent<Bootstrapper>();
            // autoInitialize defaults to true, so all systems will auto-initialize
            
            // Add TimerSystem (needed by gameplay)
            systemsObj.AddComponent<TimerSystem>();
            
            Debug.Log("[SceneSetup] Created _GameSystems with Bootstrapper + TimerSystem");
            Debug.Log("[SceneSetup] Bootstrapper will auto-initialize all managers on Awake");
            return 1;
        }
        
        private int CreateWindow()
        {
            GameObject windowObj = new GameObject("Window");
            windowObj.transform.position = Vector3.zero;
            
            // Add WindowMeshController
            WindowMeshController windowMesh = windowObj.AddComponent<WindowMeshController>();
            
            // Add HazardRenderer
            HazardRenderer hazardRenderer = windowObj.AddComponent<HazardRenderer>();
            
            // Add CleaningController
            CleaningController cleaningController = windowObj.AddComponent<CleaningController>();
            
            // Add TextureManager
            TextureManager textureManager = windowObj.AddComponent<TextureManager>();
            
            // Connect references using serialized property names
            SerializedObject hazardSO = new SerializedObject(hazardRenderer);
            hazardSO.FindProperty("windowMesh").objectReferenceValue = windowMesh;
            hazardSO.ApplyModifiedProperties();
            
            SerializedObject cleaningSO = new SerializedObject(cleaningController);
            cleaningSO.FindProperty("windowMesh").objectReferenceValue = windowMesh;
            cleaningSO.FindProperty("hazardRenderer").objectReferenceValue = hazardRenderer;
            cleaningSO.ApplyModifiedProperties();
            
            // Wire TextureManager references to HazardRenderer
            SerializedObject textureSO = new SerializedObject(textureManager);
            textureSO.FindProperty("hazardRenderer").objectReferenceValue = hazardRenderer;
            textureSO.ApplyModifiedProperties();
            
            Debug.Log("[SceneSetup] Created Window with WindowMesh + HazardRenderer + CleaningController + TextureManager");
            return 1;
        }
        
        private int CreateUI()
        {
            int objectsCreated = 0;
            
            // Create Canvas
            GameObject canvasObj = new GameObject("Canvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            objectsCreated++;

            // Create basic UI screens
            GameObject mainMenuScreenObj = CreateUIScreen(canvas.transform, "MainMenuScreen");
            GameObject levelSelectScreenObj = CreateUIScreen(canvas.transform, "LevelSelectScreen");
            GameObject mainHUDScreenObj = CreateUIScreen(canvas.transform, "MainHUDScreen", typeof(WhenImCleaningWindows.UI.MainHUD));
            GameObject shopScreenObj = CreateUIScreen(canvas.transform, "ShopScreen", typeof(WhenImCleaningWindows.UI.ShopUI));
            GameObject settingsScreenObj = CreateUIScreen(canvas.transform, "SettingsScreen");
            GameObject levelCompleteScreenObj = CreateUIScreen(canvas.transform, "LevelCompleteScreen", typeof(WhenImCleaningWindows.UI.LevelCompleteUI));
            GameObject levelFailedScreenObj = CreateUIScreen(canvas.transform, "LevelFailedScreen");
            GameObject offerPopupScreenObj = CreateUIScreen(canvas.transform, "OfferPopupScreen", typeof(WhenImCleaningWindows.UI.OfferPopupUI));
            GameObject energyRefillScreenObj = CreateUIScreen(canvas.transform, "EnergyRefillScreen", typeof(WhenImCleaningWindows.UI.EnergyUI));
            GameObject vipDashboardScreenObj = CreateUIScreen(canvas.transform, "VIPDashboardScreen");
            GameObject pauseScreenObj = CreateUIScreen(canvas.transform, "PauseScreen");

            objectsCreated += 11;

            // Layout basic UI elements
            CreateMainMenuLayout(mainMenuScreenObj.transform);
            CreateMainHUDLayout(mainHUDScreenObj.transform);
            CreateLevelCompleteLayout(levelCompleteScreenObj.transform);
            CreateOfferPopupLayout(offerPopupScreenObj.transform);
            CreateEnergyRefillLayout(energyRefillScreenObj.transform);
            CreateShopLayout(shopScreenObj.transform);
            
            // Note: MainHUD will auto-assign references via AutoAssignReferences() in Start()
            // Note: ShopUI will find IAPManager via FindFirstObjectByType in Start()
            
            // Create EventSystem
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            objectsCreated++;
            
            Debug.Log("[SceneSetup] Created UI (Canvas + EventSystem, references auto-assigned at runtime)");
            return objectsCreated;
        }
        private GameObject CreateUIScreen(Transform parent, string name, System.Type optionalComponent = null)
        {
            GameObject screen = new GameObject(name);
            screen.transform.SetParent(parent);

            RectTransform rectTransform = screen.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            CanvasGroup canvasGroup = screen.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;

            if (optionalComponent != null)
            {
                screen.AddComponent(optionalComponent);
            }

            return screen;
        }

        private void CreateMainMenuLayout(Transform parent)
        {
            CreateTMPText(parent, "TitleText", "When I'm Cleaning Windows", 36, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -40), new Vector2(600, 60));
            CreateButton(parent, "PlayButton", "Play", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(200, 50));
            CreateButton(parent, "ShopButton", "Shop", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(200, 50));
            CreateButton(parent, "SettingsButton", "Settings", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(200, 50));
        }

        private void CreateMainHUDLayout(Transform parent)
        {
            CreateTMPText(parent, "GemsText", "0", 18, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60, -20), new Vector2(100, 30));
            CreateTMPText(parent, "CoinsText", "0", 18, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180, -20), new Vector2(100, 30));
            CreateTMPText(parent, "EnergyText", "5/5", 18, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300, -20), new Vector2(100, 30));

            CreateTMPText(parent, "LevelNumberText", "Level 1", 20, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(200, 30));
            CreateTMPText(parent, "WorldNameText", "World 1", 16, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(200, 30));

            CreateTMPText(parent, "TimerText", "02:00", 22, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80, -20), new Vector2(120, 30));
            CreateImage(parent, "TimerFillBar", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-80, -50), new Vector2(120, 8));

            CreateTMPText(parent, "CleanPercentageText", "0%", 22, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 40), new Vector2(120, 30));
            CreateImage(parent, "CleanProgressBar", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 20), new Vector2(200, 10));

            CreateButton(parent, "PauseButton", "Pause", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-60, 30), new Vector2(100, 40));
            CreateButton(parent, "PowerUpButton", "Power", new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-180, 30), new Vector2(100, 40));
        }

        private void CreateLevelCompleteLayout(Transform parent)
        {
            CreateTMPText(parent, "LevelNumberText", "Level 1", 24, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(200, 40));
            CreateTMPText(parent, "TitleText", "PERFECT!", 32, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -70), new Vector2(260, 50));

            GameObject starsContainer = new GameObject("StarsContainer");
            starsContainer.transform.SetParent(parent, false);
            RectTransform starsRt = starsContainer.AddComponent<RectTransform>();
            starsRt.anchorMin = new Vector2(0.5f, 0.5f);
            starsRt.anchorMax = new Vector2(0.5f, 0.5f);
            starsRt.anchoredPosition = new Vector2(0, 40);
            starsRt.sizeDelta = new Vector2(240, 60);

            CreateImage(starsContainer.transform, "Star1", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40, 0), new Vector2(50, 50));
            CreateImage(starsContainer.transform, "Star2", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(50, 50));
            CreateImage(starsContainer.transform, "Star3", new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-40, 0), new Vector2(50, 50));

            CreateTMPText(parent, "TimeRemainingText", "Time: 00:00", 16, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(200, 30));
            CreateTMPText(parent, "CleanPercentageText", "100% Clean", 16, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(200, 30));

            CreateTMPText(parent, "CoinsEarnedText", "+0", 18, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-60, 120), new Vector2(100, 30));
            CreateTMPText(parent, "GemsEarnedText", "+0", 18, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(60, 120), new Vector2(100, 30));

            CreateImage(parent, "ElegantBadge", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 90), new Vector2(120, 30));

            CreateButton(parent, "NextLevelButton", "Next", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(160, 40));
            CreateButton(parent, "RetryButton", "Retry", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120, 50), new Vector2(100, 40));
            CreateButton(parent, "MenuButton", "Menu", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(120, 50), new Vector2(100, 40));
        }

        private void CreateOfferPopupLayout(Transform parent)
        {
            GameObject panel = new GameObject("PopupPanel");
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(360, 420);
            panel.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.6f);

            CreateTMPText(panel.transform, "TitleText", "WELCOME!", 24, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(200, 40));
            CreateTMPText(panel.transform, "UrgencyText", "Limited time!", 16, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -60), new Vector2(200, 30));
            CreateTMPText(panel.transform, "PriceText", "$0.99", 20, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(160, 30));
            CreateTMPText(panel.transform, "OriginalPriceText", "$1.99", 14, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(160, 24));
            CreateTMPText(panel.transform, "DiscountBadgeText", "-50%", 14, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20, -20), new Vector2(60, 24));
            CreateTMPText(panel.transform, "RewardsText", "Rewards", 14, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(240, 60));
            CreateImage(panel.transform, "ProductIcon", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(80, 80));
            CreateButton(panel.transform, "BuyButton", "Buy", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(160, 40));
            CreateButton(panel.transform, "CloseButton", "Close", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 10), new Vector2(120, 32));
        }

        private void CreateEnergyRefillLayout(Transform parent)
        {
            CreateTMPText(parent, "EnergyText", "5/5", 18, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60, -20), new Vector2(100, 30));
            CreateTMPText(parent, "RegenTimerText", "+1 in 20:00", 14, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60, -50), new Vector2(140, 30));
            CreateImage(parent, "EnergyFillBar", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(200, -35), new Vector2(120, 8));
            CreateImage(parent, "VIPUnlimitedIcon", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(340, -25), new Vector2(24, 24));
            CreateButton(parent, "AddEnergyButton", "+", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(380, -25), new Vector2(30, 30));

            GameObject popup = new GameObject("RefillPopup");
            popup.transform.SetParent(parent, false);
            RectTransform rt = popup.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300, 220);
            popup.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);

            CreateTMPText(popup.transform, "RefillCostText", "50", 20, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(100, 30));
            CreateButton(popup.transform, "RefillWithGemsButton", "Use Gems", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(160, 36));
            CreateButton(popup.transform, "WatchAdButton", "Watch Ad", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(160, 36));
            CreateButton(popup.transform, "ClosePopupButton", "Close", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 10), new Vector2(120, 30));
        }

        private void CreateShopLayout(Transform parent)
        {
            CreateTMPText(parent, "GemsCountText", "0", 18, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60, -20), new Vector2(100, 30));
            CreateTMPText(parent, "CoinsCountText", "0", 18, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180, -20), new Vector2(100, 30));

            CreateButton(parent, "StarterBundlesTab", "Bundles", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60, -60), new Vector2(110, 30));
            CreateButton(parent, "GemPacksTab", "Gems", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180, -60), new Vector2(110, 30));
            CreateButton(parent, "PowerUpsTab", "Power", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(300, -60), new Vector2(110, 30));
            CreateButton(parent, "VIPTab", "VIP", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(420, -60), new Vector2(80, 30));
            CreateButton(parent, "CosmeticsTab", "Cosmetic", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(520, -60), new Vector2(110, 30));

            GameObject container = new GameObject("ShopItemContainer");
            container.transform.SetParent(parent, false);
            RectTransform rt = container.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(20, 20);
            rt.offsetMax = new Vector2(-20, -120);

            GameObject featured = new GameObject("FeaturedOfferPanel");
            featured.transform.SetParent(parent, false);
            RectTransform frt = featured.AddComponent<RectTransform>();
            frt.anchorMin = new Vector2(1f, 1f);
            frt.anchorMax = new Vector2(1f, 1f);
            frt.anchoredPosition = new Vector2(-120, -20);
            frt.sizeDelta = new Vector2(220, 100);
            featured.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);

            CreateTMPText(featured.transform, "FeaturedTitleText", "Welcome Pack", 14, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -10), new Vector2(200, 24));
            CreateTMPText(featured.transform, "FeaturedPriceText", "$0.99", 14, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -5), new Vector2(200, 24));
            CreateTMPText(featured.transform, "FeaturedBonusText", "Bonus", 12, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -30), new Vector2(200, 24));
            CreateButton(featured.transform, "FeaturedBuyButton", "Buy", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 10), new Vector2(100, 26));
        }

        private TextMeshProUGUI CreateTMPText(Transform parent, string name, string text, int fontSize, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }

        private Image CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            Image img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.5f);
            return img;
        }

        private Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
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

            CreateTMPText(go.transform, "Label", label, 14, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return btn;
        }
        
        private int CreateDebugTools()
        {
            int objectsCreated = 0;
            
            // Create DebugConsole
            GameObject debugConsoleObj = new GameObject("DebugConsole");
            debugConsoleObj.AddComponent<DebugConsole>();
            objectsCreated++;
            
            // Create LevelTestManager
            GameObject testManagerObj = new GameObject("LevelTestManager");
            testManagerObj.AddComponent<LevelTestManager>();
            objectsCreated++;
            
            // Create InputDebugger
            GameObject inputDebuggerObj = new GameObject("InputDebugger");
            inputDebuggerObj.AddComponent<InputDebugger>();
            objectsCreated++;
            
            Debug.Log("[SceneSetup] Created Debug Tools (Console + TestManager + InputDebugger)");
            return objectsCreated;
        }
        
        private void ClearCurrentScene()
        {
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int deletedCount = 0;
            
            foreach (GameObject obj in allObjects)
            {
                // Don't delete objects with HideFlags (EditorOnly, etc.)
                if (obj.hideFlags == HideFlags.None)
                {
                    DestroyImmediate(obj);
                    deletedCount++;
                }
            }
            
            Debug.Log($"[SceneSetup] Cleared scene ({deletedCount} objects deleted)");
        }
        
        private void FindMissingComponents()
        {
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int missingCount = 0;
            
            foreach (GameObject obj in allObjects)
            {
                Component[] components = obj.GetComponents<Component>();
                foreach (Component comp in components)
                {
                    if (comp == null)
                    {
                        Debug.LogWarning($"[SceneSetup] Missing component on: {obj.name}", obj);
                        missingCount++;
                    }
                }
            }
            
            if (missingCount == 0)
            {
                Debug.Log("[SceneSetup] ✓ No missing components found!");
            }
            else
            {
                Debug.LogWarning($"[SceneSetup] Found {missingCount} missing components!");
            }
        }
        
        private void ValidateSceneSetup()
        {
            Debug.Log("[SceneSetup] Validating scene setup...");
            
            bool isValid = true;
            
            // Check for Main Camera
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("[SceneSetup] ✗ Main Camera not found!");
                isValid = false;
            }
            else
            {
                Debug.Log("[SceneSetup] ✓ Main Camera found");
            }
            
            // Check for Bootstrapper
            Bootstrapper bootstrapper = FindFirstObjectByType<Bootstrapper>();
            if (bootstrapper == null)
            {
                Debug.LogError("[SceneSetup] ✗ Bootstrapper not found!");
                isValid = false;
            }
            else
            {
                Debug.Log("[SceneSetup] ✓ Bootstrapper found");
            }
            
            // Check for WindowMeshController
            WindowMeshController windowMesh = FindFirstObjectByType<WindowMeshController>();
            if (windowMesh == null)
            {
                Debug.LogError("[SceneSetup] ✗ WindowMeshController not found!");
                isValid = false;
            }
            else
            {
                Debug.Log("[SceneSetup] ✓ WindowMeshController found");
            }
            
            // Check for HazardRenderer
            HazardRenderer hazardRenderer = FindFirstObjectByType<HazardRenderer>();
            if (hazardRenderer == null)
            {
                Debug.LogError("[SceneSetup] ✗ HazardRenderer not found!");
                isValid = false;
            }
            else
            {
                Debug.Log("[SceneSetup] ✓ HazardRenderer found");
            }
            
            // Check for CleaningController
            CleaningController cleaningController = FindFirstObjectByType<CleaningController>();
            if (cleaningController == null)
            {
                Debug.LogError("[SceneSetup] ✗ CleaningController not found!");
                isValid = false;
            }
            else
            {
                Debug.Log("[SceneSetup] ✓ CleaningController found");
            }
            
            // Check for Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[SceneSetup] ⚠ Canvas not found (UI will not display)");
            }
            else
            {
                Debug.Log("[SceneSetup] ✓ Canvas found");
            }
            
            // Check for EventSystem
            UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                Debug.LogWarning("[SceneSetup] ⚠ EventSystem not found (UI input won't work)");
            }
            else
            {
                Debug.Log("[SceneSetup] ✓ EventSystem found");
            }
            
            if (isValid)
            {
                Debug.Log("[SceneSetup] ✓✓✓ Scene setup is VALID! Press Play to test.");
                EditorUtility.DisplayDialog(
                    "Scene Valid!",
                    "All critical components found.\n\nScene is ready for testing!",
                    "OK"
                );
            }
            else
            {
                Debug.LogError("[SceneSetup] ✗✗✗ Scene setup is INVALID! Check errors above.");
                EditorUtility.DisplayDialog(
                    "Scene Invalid",
                    "Critical components are missing.\n\nCheck Console for details.",
                    "OK"
                );
            }
        }
    }
}








