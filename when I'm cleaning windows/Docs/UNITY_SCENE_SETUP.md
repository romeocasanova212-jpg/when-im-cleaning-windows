# Unity Scene Setup Guide

**Project**: When I'm Cleaning Windows  
**Date**: January 28, 2026  
**Purpose**: Instructions for setting up Unity scenes to run the game

---

## 🎬 Quick Start (5 Minutes)

### Step 1: Create Main Scene

1. Open Unity 6.3 LTS
2. Create new scene: `File → New Scene → URP → Empty Template`
3. Save as: `Assets/Scenes/Main.unity`

### Step 2: Add Bootstrapper

1. Create empty GameObject: `GameObject → Create Empty`
2. Rename to: `_GameSystems`
3. Add component: `Bootstrapper` script
4. Check "Auto Initialize" in inspector
5. Check "Show Debug Logs" for testing

### Step 3: Add Debug Console (Optional)

1. Create Canvas: `GameObject → UI → Canvas`
2. Set Canvas Scaler: "Scale With Screen Size", Reference: 1920×1080
3. Create empty child: `DebugConsole`
4. Add component: `DebugConsole` script
5. Create UI Panel for console (or leave empty for code-only testing)

### Step 4: Run!

1. Press Play ▶️
2. Check Console for initialization logs
3. Press **`** (backtick) to open Debug Console (if UI set up)
4. Use debug commands to test systems

---

## 🏗️ Complete Scene Hierarchy

```
Main Scene
├── _GameSystems (Bootstrapper)
│   └── Bootstrapper component
│       • Auto Initialize: ✓
│       • Show Debug Logs: ✓
│
├── Canvas (UI Root)
│   ├── MainMenuScreen
│   ├── LevelSelectScreen
│   ├── MainHUDScreen
│   ├── ShopScreen
│   ├── SettingsScreen
│   ├── LevelCompleteScreen
│   ├── LevelFailedScreen
│   ├── OfferPopupScreen
│   ├── EnergyRefillScreen
│   ├── VIPDashboardScreen
│   ├── PauseScreen
│   └── DebugConsole
│       └── DebugConsole component
│
├── EventSystem (Auto-created with Canvas)
│
└── Main Camera
    • Clear Flags: Solid Color
    • Background: #1A1A1A (dark grey)
```

---

## 🎮 Manager GameObjects (Auto-Created by Bootstrapper)

The Bootstrapper automatically creates these at runtime:

- **EnergySystem** (72 lives/day, VIP unlimited)
- **CurrencyManager** (Gems + Coins)
- **IAPManager** (28 SKUs)
- **VIPManager** (3 tiers)
- **PersonalizationEngine** (ML churn)
- **LevelGenerator** (3,000 levels)
- **HazardSystem** (24 types)
- **GameManager** (State + progression)
- **AudioManager** (ASMR SFX)
- **UIManager** (If Canvas exists)

All are marked `DontDestroyOnLoad` and persist between scenes.

---

## 🖼️ Creating UI Screens (Optional)

For a fully functional UI, create these panels in the Canvas:

### 1. Main Menu Screen
```
Canvas/MainMenuScreen (Panel)
├── TitleText (TextMeshPro)
├── PlayButton (Button)
├── ShopButton (Button)
└── SettingsButton (Button)
```

### 2. Shop Screen
```
Canvas/ShopScreen (Panel)
├── TopBar
│   ├── GemsText (TextMeshPro)
│   └── CoinsText (TextMeshPro)
├── TabButtons
│   ├── StarterBundlesTab (Button)
│   ├── GemPacksTab (Button)
│   ├── PowerUpsTab (Button)
│   ├── VIPTab (Button)
│   └── CosmeticsTab (Button)
├── ShopItemContainer (Scroll View)
│   └── Content (Grid Layout Group)
└── FeaturedOfferPanel
    ├── FeaturedTitleText
    ├── FeaturedPriceText
    └── FeaturedBuyButton
```

### 3. Main HUD
```
Canvas/MainHUDScreen (Panel)
├── TopBar
│   ├── GemsText
│   ├── CoinsText
│   └── EnergyText
├── LevelInfo
│   ├── LevelNumberText
│   └── WorldNameText
├── Timer
│   ├── TimerText
│   └── TimerFillBar (Image)
├── CleanProgress
│   ├── CleanPercentageText
│   └── CleanProgressBar (Image)
└── Buttons
    ├── PauseButton
    └── PowerUpButton
```

### 4. Energy UI (Embedded in Main HUD)
```
TopBar/EnergyContainer
├── EnergyText (Current/Max)
├── RegenTimerText (Next regen countdown)
├── EnergyFillBar (Image)
├── VIPUnlimitedIcon (Image - hidden if not VIP)
└── AddEnergyButton (Opens refill popup)
```

### 5. Level Complete Screen
```
Canvas/LevelCompleteScreen (Panel)
├── LevelNumberText
├── TitleText ("PERFECT!" / "WELL DONE!" / "CLEARED!")
├── StarsContainer
│   ├── Star1 (Image)
│   ├── Star2 (Image)
│   └── Star3 (Image)
├── Stats
│   ├── TimeRemainingText
│   └── CleanPercentageText
├── Rewards
│   ├── CoinsEarnedText
│   └── GemsEarnedText (optional)
├── ElegantBadge (Image - shown if elegant)
└── Buttons
    ├── NextLevelButton
    ├── RetryButton
    └── MenuButton
```

---

## 🔧 Prefab Setup (Optional but Recommended)

To avoid manually setting up UI each time:

1. Create all UI screens in Canvas
2. Drag each screen to `Assets/Prefabs/UI/`
3. Create prefabs for managers: `Assets/Prefabs/Managers/`
4. Assign prefabs to Bootstrapper inspector

**Prefabs to Create:**
- `EnergySystem.prefab`
- `CurrencyManager.prefab`
- `IAPManager.prefab`
- `VIPManager.prefab`
- `PersonalizationEngine.prefab`
- `LevelGenerator.prefab`
- `HazardSystem.prefab`
- `GameManager.prefab`
- `UIManager.prefab` (with full Canvas hierarchy)
- `AudioManager.prefab`

---

## 🧪 Testing Workflow

### Basic Test (No UI)
1. Add Bootstrapper to scene
2. Press Play
3. Check Console for "✓" initialization logs
4. Open **Window → Analysis → Profiler** to verify <16ms frame time
5. Test via code or Debug Console buttons

### Full Test (With UI)
1. Set up complete Canvas hierarchy
2. Assign UI screens to UIManager in Bootstrapper
3. Press Play
4. Click Play button on Main Menu
5. Select Level 1
6. (Gameplay placeholder - would need window mesh + gesture input)
7. Click Pause → Complete Level → See results

### Debug Console Commands
Press **`** (backtick) then click buttons:
- Add 5 Energy
- Add 1000 Gems
- Add 5000 Coins
- Activate VIP Bronze/Gold
- Start Level 1
- Complete Level (3★)
- Trigger D1 Offer
- Calculate Churn Score
- Unlock All Levels
- Print System Status

---

## 📱 Build Settings

### Android
1. `File → Build Settings`
2. Platform: Android
3. Texture Compression: ASTC
4. Minimum API Level: 24 (Android 7.0)
5. Target API Level: 34 (Android 14)
6. Scripting Backend: IL2CPP
7. Target Architectures: ARM64
8. Add scenes: `Main.unity`

### iOS
1. Platform: iOS
2. Target SDK: Device SDK
3. Target Minimum iOS Version: 13.0
4. Architecture: ARM64
5. Camera Usage Description: "For AR features" (if using camera)

---

## 🎯 Next Steps After Scene Setup

### Immediate (Can Test Now)
- ✅ Bootstrapper initializes all systems
- ✅ Debug Console provides quick testing
- ✅ Energy/Currency/VIP systems functional
- ✅ Level generation works (3,000 levels)
- ✅ Churn prediction active
- ✅ IAP shop defined (debug purchases)

### Requires Additional Work
- ⚠️ Window mesh + suds physics (needs SudsPhysics integration)
- ⚠️ Gesture input (needs Enhanced Touch Input setup)
- ⚠️ Hazard visuals (needs textures/materials)
- ⚠️ Audio SFX (needs FMOD plugin or AudioClips)
- ⚠️ Real IAP purchases (needs Unity IAP + Google/Apple setup)
- ⚠️ Firebase analytics (needs Firebase SDK)

---

## 🐛 Troubleshooting

### "Instance is null" errors
**Solution**: Ensure Bootstrapper runs first. Set Script Execution Order:
1. `Edit → Project Settings → Script Execution Order`
2. Add `Bootstrapper` with value `-100` (runs early)

### UI not showing
**Solution**: Check UIManager screen assignments. Verify Canvas Render Mode = "Screen Space - Overlay"

### Gestures not working
**Solution**: Add `Enhanced Touch Support` component to scene:
```csharp
EnhancedTouchSupport.Enable();
```

### Audio not playing
**Solution**: AudioManager requires AudioClips or FMOD banks. For now, it logs events only.

### Performance issues
**Solution**: Check Profiler. Ensure Burst Compiler enabled: `Jobs → Burst → Enable Compilation`

---

## 📚 Script References

All scripts auto-initialize via Bootstrapper. Access via static Instance properties:

```csharp
// Energy
EnergySystem.Instance.AddEnergy(5, "Test");

// Currency
CurrencyManager.Instance.AddGems(100, "Test");

// Game Flow
GameManager.Instance.StartLevel(1);
GameManager.Instance.CompleteLevel(100f, 50, true);

// VIP
VIPManager.Instance.ActivateVIP(VIPTier.Bronze, 30);

// Personalization
float churnScore = PersonalizationEngine.Instance.CalculateChurnScore();

// Level Gen
var level = LevelGenerator.Instance.GenerateLevel(100);

// UI
UIManager.Instance.ShowScreen(UIScreen.Shop);

// Audio
AudioManager.Instance.PlaySFX(AudioEventType.SFX_Swipe_Suds);
```

---

## ✅ Scene Checklist

Before pressing Play, verify:

- [ ] Bootstrapper GameObject exists
- [ ] Bootstrapper has "Auto Initialize" checked
- [ ] Canvas exists (if using UI)
- [ ] EventSystem exists (auto-created with Canvas)
- [ ] Camera background color set (not skybox)
- [ ] Script Execution Order set (Bootstrapper = -100)
- [ ] Burst Compiler enabled in Jobs menu
- [ ] Platform set (Android or iOS) in Build Settings

---

## 🚀 Ready to Play!

With these steps complete:
1. Press **Play** ▶️
2. Watch Console for initialization logs
3. Press **`** for Debug Console
4. Test all systems via debug buttons

**Estimated Setup Time**: 5-15 minutes (depending on UI complexity)

---

*For full documentation, see: [PROJECT_STATUS.md](PROJECT_STATUS.md)*
