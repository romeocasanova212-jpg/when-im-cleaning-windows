# Architecture Reference - Complete System Wiring

## System Overview

```
┌─────────────────────────────────────────────────────────────┐
│                     WHEN I'M CLEANING WINDOWS                │
│                  10 Worlds × 100 Levels/World = 1000 Levels │
└─────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│ BOOTSTRAPPER (5-Phase Initialization)                                    │
├──────────────────────────────────────────────────────────────────────────┤
│ Phase 0: Firebase (Remote Config, Cloud Save, Analytics)                │
│ Phase 1: Core Systems (Energy, Currency, Hazard, Audio, VFX, Animation) │
│ Phase 2: Monetization (IAP, VIP, Personalization)                       │
│ Phase 3: Generation (LevelGenerator, Procedural Mesh)                   │
│ Phase 4: UI (UIManager auto-discovery, auto-wiring)                     │
│ Phase 5: GameManager (depends on all others)                            │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│ CONFIG PROVIDER (Static Loader with Fallbacks)                          │
├──────────────────────────────────────────────────────────────────────────┤
│ GameConfig (50+ properties) → Loaded from Resources/Config               │
│ LevelConfig (difficulty, mesh, themes) → Loaded from Resources/Config   │
│ AudioConfig (ASMR, FMOD, haptics) → Loaded from Resources/Config        │
│                                                                           │
│ Pattern: All systems call ApplyConfig() in Awake()                       │
│ Safety: Each system has Mathf.Max/Mathf.Min validation + hardcoded      │
│         defaults as fallback if ConfigProvider returns null              │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│ CORE GAMEPLAY LOOP                                                       │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│  1. LEVEL INITIALIZATION                                                 │
│     ├─ GameManager.StartLevel(levelNumber)                               │
│     ├─ LevelGenerator generates mesh + hazards                           │
│     ├─ TimerSystem.SetWorldLevel() scales difficulty                     │
│     └─ WindowMeshController initializes clean tracking                   │
│                                                                           │
│  2. PLAYER INPUT (Gesture)                                               │
│     ├─ CleaningController receives touch input (swipe, scrub, etc)       │
│     ├─ Calculates cleaning area (baseCleaningRadius from config)         │
│     └─ Calls WindowMeshController.CleanArea()                           │
│                                                                           │
│  3. MESH UPDATE                                                          │
│     ├─ WindowMeshController updates vertex colors (dirty→clean)          │
│     ├─ Calculates CurrentCleanPercentage                                 │
│     ├─ Broadcasts OnCleanPercentageChanged event if change > 0.05%       │
│     └─ GameManager checks threshold (95% = level complete)               │
│                                                                           │
│  4. UI UPDATE (Real-Time)                                                │
│     ├─ MainHUD listens to OnCleanPercentageChanged                       │
│     ├─ Updates CleanPercentageText display                               │
│     └─ Updates CleanProgressBar visual feedback                          │
│                                                                           │
│  5. TIMER UPDATE (Count Down)                                            │
│     ├─ TimerSystem decrements every frame                                │
│     ├─ MainHUD.UpdateTimer() updates display + color (green→yellow→red) │
│     └─ When 0: GameManager.FailLevel()                                   │
│                                                                           │
│  6. LEVEL COMPLETE                                                       │
│     ├─ GameManager.CompleteLevel() when clean % >= 95%                   │
│     ├─ CalculateStars() using time remaining vs thresholds               │
│     ├─ AwardLevelRewards() distributes coins + gems                      │
│     ├─ Fires OnLevelCompleted event                                      │
│     └─ UIManager shows LevelCompleteScreen                               │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│ EVENT COMMUNICATION PIPELINE                                             │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│ GAMEPLAY EVENTS                                                          │
│   └─ GameManager.OnLevelStarted(LevelData) → UIManager.ShowMainHUD()    │
│   └─ GameManager.OnLevelCompleted(LevelResult) → UIManager.ShowComplete │
│   └─ GameManager.OnLevelFailed() → UIManager.ShowLevelFailed()          │
│   └─ GameManager.OnGameStateChanged(state) → UIManager.ShowMainMenu()   │
│                                                                           │
│ MESH EVENTS                                                              │
│   └─ WindowMeshController.OnCleanPercentageChanged(%) → MainHUD.SetCP() │
│                                                                           │
│ ECONOMY EVENTS                                                           │
│   └─ CurrencyManager.OnCoinsChanged(amount) → MainHUD.UpdateCoins()     │
│   └─ CurrencyManager.OnGemsChanged(amount) → MainHUD.UpdateGems()       │
│   └─ EnergySystem.OnEnergyChanged(current, max) → EnergyUI.UpdateEnergy │
│                                                                           │
│ MONETIZATION EVENTS                                                      │
│   └─ PersonalizationEngine.OnOfferTriggered() → OfferPopupUI.ShowOffer()│
│   └─ IAPManager.OnPurchaseComplete() → ShopUI.OnPurchaseComplete()      │
│                                                                           │
│ AUDIO EVENTS                                                             │
│   └─ CleaningController.OnCleanSound() → AudioManager.PlayCleanSFX()    │
│   └─ WindowMeshController.OnMeshUpdated() → AudioManager.PlayASMR()     │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│ UI SCREEN HIERARCHY                                                      │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│ Canvas                                                                    │
│  ├─ MainMenuScreen                                                       │
│  │  └─ TitleText, PlayButton, ShopButton, SettingsButton                │
│  │                                                                       │
│  ├─ LevelSelectScreen                                                    │
│  │  └─ [Level grid with selectable level buttons]                       │
│  │                                                                       │
│  ├─ MainHUDScreen (Active during level)                                 │
│  │  ├─ GemsText, CoinsText, EnergyText (top-left corner)               │
│  │  ├─ LevelNumberText, WorldNameText (center-top)                      │
│  │  ├─ TimerText, TimerFillBar (top-right)                              │
│  │  ├─ CleanPercentageText, CleanProgressBar (bottom-center)            │
│  │  └─ PauseButton, PowerUpButton (bottom-right)                        │
│  │                                                                       │
│  ├─ LevelCompleteScreen (Shown when clean % ≥ 95%)                      │
│  │  ├─ LevelNumberText, TitleText (PERFECT! / 3 STARS / etc)            │
│  │  ├─ StarObjects[3] (animated pop-in)                                 │
│  │  ├─ TimeRemainingText, CleanPercentageText                           │
│  │  ├─ CoinsEarnedText, GemsEarnedText                                  │
│  │  ├─ ElegantBadge (visual flourish)                                   │
│  │  └─ Buttons: NextLevelButton, RetryButton, MenuButton                │
│  │                                                                       │
│  ├─ LevelFailedScreen (Shown when timer reaches 0)                      │
│  │  └─ [Failed message, Retry, Menu buttons]                            │
│  │                                                                       │
│  ├─ ShopScreen (Monetization)                                           │
│  │  ├─ GemsCountText, CoinsCountText (top-left)                         │
│  │  ├─ TabButtons: StarterBundles, GemPacks, PowerUps, VIP, Cosmetics   │
│  │  ├─ ShopItemContainer (scrollable list of 28 SKUs)                   │
│  │  ├─ FeaturedOfferPanel (top-right, 220×100)                          │
│  │  └─ BackButton                                                        │
│  │                                                                       │
│  ├─ EnergyRefillScreen (Triggered when energy depletes)                 │
│  │  ├─ EnergyText (5/5 or ∞ for VIP)                                    │
│  │  ├─ RegenTimerText ("+1 energy in MM:SS")                            │
│  │  ├─ EnergyFillBar (visual representation)                            │
│  │  ├─ AddEnergyButton                                                   │
│  │  └─ RefillPopup                                                       │
│  │     ├─ RefillCostText (50 Gems)                                      │
│  │     ├─ RefillWithGemsButton                                           │
│  │     ├─ WatchAdButton                                                  │
│  │     └─ ClosePopupButton                                               │
│  │                                                                       │
│  ├─ OfferPopupScreen (D1/D3/D7 re-engagement)                           │
│  │  ├─ PopupPanel (360×420, semi-transparent overlay)                   │
│  │  ├─ TitleText (Welcome / Get Started / etc)                          │
│  │  ├─ UrgencyText (Limited time only!)                                 │
│  │  ├─ PriceText, OriginalPriceText (strikethrough)                     │
│  │  ├─ DiscountBadgeText (50% OFF!)                                     │
│  │  ├─ RewardsText (200 Gems + 50 Coins)                                │
│  │  ├─ ProductIcon (80×80 preview)                                      │
│  │  └─ Buttons: BuyButton, CloseButton                                  │
│  │                                                                       │
│  ├─ VIPDashboardScreen                                                  │
│  │  └─ [VIP status, perks, upgrade CTA]                                 │
│  │                                                                       │
│  ├─ SettingsScreen                                                      │
│  │  └─ [Audio, graphics, language, privacy settings]                    │
│  │                                                                       │
│  ├─ PauseScreen                                                         │
│  │  └─ [Resume, Settings, Quit to Menu buttons]                         │
│  │                                                                       │
│  └─ EventSystem (for UI input handling)                                 │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│ CONFIG WIRING DETAILS                                                    │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│ GAMEPLAY CONFIG (GameConfig.cs)                                         │
│   └─ maxFreeEnergy = 5                                                   │
│   └─ maxVipEnergy = 10                                                   │
│   └─ energyRegenTime = 1200f (20 minutes, yields 72 lives/day)           │
│   └─ baseCoinsPerLevel = 5                                               │
│   └─ coinsPerStar = 5 (bonus per star: 25 coins max)                    │
│   └─ gemDropChance = 0.05f (5% chance drop)                             │
│   └─ minMaxGemDrop = (1, 5)                                              │
│   └─ levelCompleteThreshold = 95%                                        │
│   └─ baseCleaningRadius = 1.0f                                           │
│   └─ baseCleaningPower = 0.8f                                            │
│                                                                           │
│ DIFFICULTY CONFIG (LevelConfig.cs)                                      │
│   └─ baseTimeLimit = 120f (World 1)                                      │
│   └─ minTimeLimit = 40f (World 10)                                       │
│   └─ startingHazardCount = 8 (World 1)                                   │
│   └─ endingHazardCount = 25 (World 10)                                   │
│   └─ twoStar_TimePercent = 33% (time threshold for 2 stars)              │
│   └─ threeStar_TimePercent = 66% (time threshold for 3 stars)            │
│   └─ meshSubdivisionsX = 32 (vertices width)                             │
│   └─ meshSubdivisionsY = 32 (vertices height)                            │
│                                                                           │
│ AUDIO CONFIG (AudioConfig.cs)                                           │
│   └─ enableBinauralAudio = true                                          │
│   └─ masterVolume = 0.8f                                                 │
│   └─ sfxVolume = 0.7f                                                    │
│   └─ useFMOD = true (when plugin installed)                              │
│   └─ audioSourcePoolSize = 30                                            │
│                                                                           │
│ APPLICATION: Each system reads from ConfigProvider in Awake()            │
│   EnergySystem.ApplyConfig()                                             │
│   CurrencyManager.ApplyConfig()                                          │
│   TimerSystem.ApplyConfig()                                              │
│   LevelGenerator.ApplyConfig()                                           │
│   WindowMeshController.ApplyConfig()                                     │
│   CleaningController.ApplyConfig()                                       │
│   AudioManager.ApplyConfig()                                             │
│   GameManager (uses config values when calculating rewards)              │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│ AUTO-WIRING MECHANISM                                                    │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│ PROBLEM SOLVED: 100+ UI references needed manual drag-and-drop          │
│                                                                           │
│ SOLUTION: Recursive component discovery by name                         │
│                                                                           │
│ IMPLEMENTATION:                                                          │
│   1. All UI elements created with standardized names during scene setup  │
│      └─ "GemsText", "CoinsText", "TimerFillBar", etc.                    │
│   2. All UI controllers implement AutoAssignReferences() in Start()      │
│   3. AutoAssignReferences() uses FindChildRecursive(name)                │
│   4. Null coalescing (??=) allows inspector override if needed           │
│                                                                           │
│ PATTERN (per file):                                                      │
│   private void AutoAssignReferences() {                                  │
│       gemsText ??= FindChildComponent<TextMeshProUGUI>("GemsText");      │
│       coinsText ??= FindChildComponent<TextMeshProUGUI>("CoinsText");    │
│       timerFillBar ??= FindChildComponent<Image>("TimerFillBar");        │
│       // ... repeat for all UI elements                                  │
│   }                                                                       │
│                                                                           │
│ FILES IMPLEMENTING AUTO-WIRING:                                         │
│   ✓ MainHUD (12+ components)                                             │
│   ✓ LevelCompleteUI (9 components)                                       │
│   ✓ EnergyUI (10 components)                                             │
│   ✓ OfferPopupUI (8 components)                                          │
│   ✓ ShopUI (15+ components)                                              │
│   ✓ UIManager (11 screen references)                                     │
│                                                                           │
│ RESULT: Zero manual inspector wiring required                            │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────────┐
│ PROCEDURAL FALLBACK SYSTEM                                              │
├──────────────────────────────────────────────────────────────────────────┤
│                                                                           │
│ PROBLEM: Missing prefabs/components crash on Play                       │
│                                                                           │
│ SOLUTION: Runtime creation of minimal UI elements                       │
│                                                                           │
│ IMPLEMENTATION:                                                          │
│   └─ SceneSetupUtility.cs creates 6 layout builders                     │
│     └─ CreateMainHUDLayout() → 13 UI elements                            │
│     └─ CreateLevelCompleteLayout() → 11 UI elements                      │
│     └─ CreateShopLayout() → 11 UI elements + fallback shop items         │
│     └─ etc.                                                              │
│                                                                           │
│   └─ Helper methods for runtime element creation:                        │
│     └─ CreateTMPText(parent, name, text, fontSize, anchors, size)       │
│     └─ CreateImage(parent, name, color, anchors, size)                   │
│     └─ CreateButton(parent, name, label, anchors, size)                  │
│     └─ CreateFallbackShopItem() → Complete shop item card               │
│                                                                           │
│ RESULT: Scene playable on first run, no missing components              │
│                                                                           │
└──────────────────────────────────────────────────────────────────────────┘
```

---

## Key Design Patterns

### 1. Singleton + DontDestroyOnLoad
All manager systems use this pattern for persistent state across scenes.

### 2. Static Events for Decoupled Communication
Systems don't reference each other directly. They communicate via public static events.

### 3. ApplyConfig() Pattern
Each system reads from ConfigProvider in Awake() before other initialization.

### 4. Recursive Component Discovery
UI controllers find child components by name instead of manual serialization.

### 5. Procedural Layout Generation
Scene setup creates complete UI hierarchy at runtime, no manual scene design needed.

---

## Performance Considerations

- **Memory**: Scene uses ~100-200MB base (expandable with assets)
- **GC**: Events allocated once at startup, no allocations in gameplay loop
- **Physics**: No physics, pure mesh vertex manipulation for performance
- **Audio**: 30-source pool for ASMR + SFX (configurable)
- **Rendering**: Orthographic 2D, 32×32 mesh subdivision default (configurable)

---

## Security Considerations

- **PlayerPrefs**: Used for local progression (energy, currency, level unlocks)
- **Encryption**: PlayerPrefs on mobile automatically encrypted by OS
- **Cloud Save**: Optional Firebase integration for cross-device sync
- **IAP Validation**: Receipt validation via iTunes Connect / Google Play
- **Analytics**: Privacy-first analytics with no personal data collection

---

## Extensibility Points

### Adding New Config Values
1. Add property to GameConfig.cs / LevelConfig.cs
2. Call `ConfigProvider.GameConfig.propertyName` in any system
3. Inspector exposes value for live editing

### Adding New UI Screens
1. Create screen GameObject in Canvas with RectTransform
2. Add controller component (inherit from MonoBehaviour)
3. Implement AutoAssignReferences() method
4. Add screen enum to UIScreen enum
5. Wire in UIManager.ShowScreen() logic

### Adding New Gameplay Systems
1. Create singleton MonoBehaviour inheriting from required base
2. Implement ApplyConfig() method
3. Add initialization to Bootstrapper in appropriate phase
4. Broadcast events for inter-system communication

### Adding New Audio Events
1. Define event name in AudioConfig.cs
2. Register FMOD event path
3. Call AudioManager.PlayEvent(eventName) from gameplay logic

---

**Last Updated**: February 1, 2026  
**Architecture Version**: 1.0 - Full Automation Complete
