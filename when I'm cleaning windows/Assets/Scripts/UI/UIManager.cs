using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace WhenImCleaningWindows.UI
{
    /// <summary>
    /// UI screen types.
    /// </summary>
    public enum UIScreen
    {
        None,
        MainMenu,
        LevelSelect,
        MainHUD,
        Shop,
        Settings,
        LevelComplete,
        LevelFailed,
        OfferPopup,
        EnergyRefill,
        VIPDashboard,
        Pause
    }
    
    /// <summary>
    /// UI Manager - Central controller for all UI screens and transitions.
    /// Manages screen stack, animations, and integration with game systems.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("Screen References")]
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private GameObject mainMenuScreen;
        [SerializeField] private GameObject levelSelectScreen;
        [SerializeField] private GameObject mainHUDScreen;
        [SerializeField] private GameObject shopScreen;
        [SerializeField] private GameObject settingsScreen;
        [SerializeField] private GameObject levelCompleteScreen;
        [SerializeField] private GameObject levelFailedScreen;
        [SerializeField] private GameObject offerPopupScreen;
        [SerializeField] private GameObject energyRefillScreen;
        [SerializeField] private GameObject vipDashboardScreen;
        [SerializeField] private GameObject pauseScreen;
        
        [Header("Transition Settings")]
        [SerializeField] private float transitionDuration = 0.3f;
        [SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // Events
        public static event Action<UIScreen> OnScreenChanged;
        public static event Action<UIScreen, UIScreen> OnScreenTransition;
        
        // State
        private UIScreen currentScreen = UIScreen.None;
        private Stack<UIScreen> screenStack = new Stack<UIScreen>();
        private Dictionary<UIScreen, GameObject> screenMap = new Dictionary<UIScreen, GameObject>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            AutoAssignScreens();
            InitializeScreenMap();
        }
        
        private void Start()
        {
            // Hide all screens initially
            HideAllScreens();
            
            // Show main menu
            ShowScreen(UIScreen.MainMenu, false);

            // Subscribe to game events
            WhenImCleaningWindows.Core.GameManager.OnLevelStarted += HandleLevelStarted;
            WhenImCleaningWindows.Core.GameManager.OnLevelCompleted += HandleLevelCompleted;
            WhenImCleaningWindows.Core.GameManager.OnLevelFailed += HandleLevelFailed;
            WhenImCleaningWindows.Core.GameManager.OnGameStateChanged += HandleGameStateChanged;
        }
        
        /// <summary>
        /// Initialize screen mapping.
        /// </summary>
        private void InitializeScreenMap()
        {
            screenMap[UIScreen.MainMenu] = mainMenuScreen;
            screenMap[UIScreen.LevelSelect] = levelSelectScreen;
            screenMap[UIScreen.MainHUD] = mainHUDScreen;
            screenMap[UIScreen.Shop] = shopScreen;
            screenMap[UIScreen.Settings] = settingsScreen;
            screenMap[UIScreen.LevelComplete] = levelCompleteScreen;
            screenMap[UIScreen.LevelFailed] = levelFailedScreen;
            screenMap[UIScreen.OfferPopup] = offerPopupScreen;
            screenMap[UIScreen.EnergyRefill] = energyRefillScreen;
            screenMap[UIScreen.VIPDashboard] = vipDashboardScreen;
            screenMap[UIScreen.Pause] = pauseScreen;
        }

        private void AutoAssignScreens()
        {
            if (mainCanvas == null)
            {
                mainCanvas = FindFirstObjectByType<Canvas>();
            }

            if (mainCanvas == null) return;

            mainMenuScreen ??= FindChildByName(mainCanvas.transform, "MainMenuScreen");
            levelSelectScreen ??= FindChildByName(mainCanvas.transform, "LevelSelectScreen");
            mainHUDScreen ??= FindChildByName(mainCanvas.transform, "MainHUDScreen");
            shopScreen ??= FindChildByName(mainCanvas.transform, "ShopScreen");
            settingsScreen ??= FindChildByName(mainCanvas.transform, "SettingsScreen");
            levelCompleteScreen ??= FindChildByName(mainCanvas.transform, "LevelCompleteScreen");
            levelFailedScreen ??= FindChildByName(mainCanvas.transform, "LevelFailedScreen");
            offerPopupScreen ??= FindChildByName(mainCanvas.transform, "OfferPopupScreen");
            energyRefillScreen ??= FindChildByName(mainCanvas.transform, "EnergyRefillScreen");
            vipDashboardScreen ??= FindChildByName(mainCanvas.transform, "VIPDashboardScreen");
            pauseScreen ??= FindChildByName(mainCanvas.transform, "PauseScreen");
        }

        private static GameObject FindChildByName(Transform root, string name)
        {
            Transform child = root.Find(name);
            return child != null ? child.gameObject : null;
        }
        
        /// <summary>
        /// Show a screen (with optional animation and stack management).
        /// </summary>
        public void ShowScreen(UIScreen screen, bool addToStack = true, bool animate = true)
        {
            if (screen == currentScreen) return;
            
            UIScreen previousScreen = currentScreen;
            
            // Hide current screen
            if (currentScreen != UIScreen.None && screenMap.ContainsKey(currentScreen))
            {
                if (animate)
                {
                    StartCoroutine(AnimateScreenOut(screenMap[currentScreen]));
                }
                else
                {
                    screenMap[currentScreen].SetActive(false);
                }
            }
            
            // Add to stack if requested
            if (addToStack && currentScreen != UIScreen.None)
            {
                screenStack.Push(currentScreen);
            }
            
            // Show new screen
            currentScreen = screen;
            
            if (screenMap.ContainsKey(screen))
            {
                if (animate)
                {
                    screenMap[screen].SetActive(true);
                    StartCoroutine(AnimateScreenIn(screenMap[screen]));
                }
                else
                {
                    screenMap[screen].SetActive(true);
                }
            }
            
            OnScreenChanged?.Invoke(screen);
            OnScreenTransition?.Invoke(previousScreen, screen);
            
            Debug.Log($"UI: {previousScreen} → {screen}");
        }
        
        /// <summary>
        /// Go back to previous screen in stack.
        /// </summary>
        public void GoBack()
        {
            if (screenStack.Count > 0)
            {
                UIScreen previousScreen = screenStack.Pop();
                ShowScreen(previousScreen, false, true);
            }
        }
        
        /// <summary>
        /// Clear screen stack.
        /// </summary>
        public void ClearStack()
        {
            screenStack.Clear();
        }
        
        /// <summary>
        /// Hide all screens.
        /// </summary>
        private void HideAllScreens()
        {
            foreach (var screen in screenMap.Values)
            {
                if (screen != null)
                {
                    screen.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Show popup overlay (doesn't replace current screen).
        /// </summary>
        public void ShowPopup(UIScreen popup)
        {
            if (screenMap.ContainsKey(popup))
            {
                screenMap[popup].SetActive(true);
                StartCoroutine(AnimateScreenIn(screenMap[popup]));
            }
        }
        
        /// <summary>
        /// Hide popup overlay.
        /// </summary>
        public void HidePopup(UIScreen popup)
        {
            if (screenMap.ContainsKey(popup))
            {
                StartCoroutine(AnimateScreenOut(screenMap[popup]));
            }
        }
        
        /// <summary>
        /// Animate screen in (fade + scale).
        /// </summary>
        private System.Collections.IEnumerator AnimateScreenIn(GameObject screen)
        {
            CanvasGroup canvasGroup = screen.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = screen.AddComponent<CanvasGroup>();
            }
            
            RectTransform rectTransform = screen.GetComponent<RectTransform>();
            
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = transitionCurve.Evaluate(elapsed / transitionDuration);
                
                canvasGroup.alpha = t;
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);
                }
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
            if (rectTransform != null)
            {
                rectTransform.localScale = Vector3.one;
            }
        }
        
        /// <summary>
        /// Animate screen out (fade + scale).
        /// </summary>
        private System.Collections.IEnumerator AnimateScreenOut(GameObject screen)
        {
            CanvasGroup canvasGroup = screen.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = screen.AddComponent<CanvasGroup>();
            }
            
            RectTransform rectTransform = screen.GetComponent<RectTransform>();
            
            float elapsed = 0f;
            while (elapsed < transitionDuration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - transitionCurve.Evaluate(elapsed / transitionDuration);
                
                canvasGroup.alpha = t;
                if (rectTransform != null)
                {
                    rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.9f, Vector3.one, t);
                }
                
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            screen.SetActive(false);
        }

        private void HandleLevelStarted(WhenImCleaningWindows.Procedural.LevelData _)
        {
            ShowScreen(UIScreen.MainHUD, true, true);
        }

        private void HandleLevelCompleted(WhenImCleaningWindows.Core.LevelResult _)
        {
            ShowScreen(UIScreen.LevelComplete, true, true);
        }

        private void HandleLevelFailed()
        {
            ShowScreen(UIScreen.LevelFailed, true, true);
        }

        private void HandleGameStateChanged(WhenImCleaningWindows.Core.GameState state)
        {
            if (state == WhenImCleaningWindows.Core.GameState.MainMenu)
            {
                ShowScreen(UIScreen.MainMenu, false, true);
            }
        }

        private void OnDestroy()
        {
            WhenImCleaningWindows.Core.GameManager.OnLevelStarted -= HandleLevelStarted;
            WhenImCleaningWindows.Core.GameManager.OnLevelCompleted -= HandleLevelCompleted;
            WhenImCleaningWindows.Core.GameManager.OnLevelFailed -= HandleLevelFailed;
            WhenImCleaningWindows.Core.GameManager.OnGameStateChanged -= HandleGameStateChanged;
        }
        
        /// <summary>
        /// Get current screen.
        /// </summary>
        public UIScreen GetCurrentScreen()
        {
            return currentScreen;
        }
        
        /// <summary>
        /// Get screen GameObject.
        /// </summary>
        public GameObject GetScreenObject(UIScreen screen)
        {
            return screenMap.ContainsKey(screen) ? screenMap[screen] : null;
        }
        
        // === BUTTON CALLBACKS ===
        
        public void OnPlayButton()
        {
            ShowScreen(UIScreen.LevelSelect);
        }
        
        public void OnShopButton()
        {
            ShowScreen(UIScreen.Shop);
        }
        
        public void OnSettingsButton()
        {
            ShowScreen(UIScreen.Settings);
        }
        
        public void OnBackButton()
        {
            GoBack();
        }
        
        public void OnPauseButton()
        {
            ShowPopup(UIScreen.Pause);
            Time.timeScale = 0f;
        }
        
        public void OnResumeButton()
        {
            HidePopup(UIScreen.Pause);
            Time.timeScale = 1f;
        }
        
        public void OnQuitToMenuButton()
        {
            Time.timeScale = 1f;
            ClearStack();
            ShowScreen(UIScreen.MainMenu, false);
        }
    }
}








