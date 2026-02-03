# Integration Guide: 2026 Enhancement Systems

## Overview
This guide explains how to integrate the 5 new advanced systems into the existing game architecture. All systems are designed to work seamlessly with the existing Bootstrapper and GameManager.

---

## System Dependency Map

```
Bootstrapper (Phase 0-3)
├── Phase 1: Core Systems
├── Phase 2: Gameplay Systems
│   ├── GameManager
│   ├── GestureInput
│   ├── TimerSystem
│   └── [NEW] ComboSystem ✨
├── Phase 3: UI Systems
│   ├── UIManager
│   ├── MainHUD
│   ├── ShopUI
│   ├── EnergyUI
│   └── [NEW] OfferPopupUI (modified)
└── Phase 4: Engagement Systems [NEW]
    ├── [NEW] DailyMissionSystem
    ├── [NEW] AnalyticsFramework
    ├── [NEW] ChurnPredictionEngine
    └── [NEW] DynamicOfferEngine
```

---

## Step-by-Step Integration

### Step 1: Add ComboSystem to Bootstrapper (Phase 2)

**Location**: `Bootstrapper.cs` - Add to Phase 2 Initialization

```csharp
// In Phase 2: Initialize Gameplay Systems
gameObject.AddComponent<ComboSystem>();
```

**Wiring**: ComboSystem listens to `GestureInput.OnGestureComplete` events

```csharp
// In ComboSystem.Start()
var gestureInput = FindObjectOfType<GestureInput>();
if (gestureInput != null)
{
    gestureInput.OnGestureComplete += OnGestureComplete;
}
```

**Configuration**:
- `COMBO_TIMEOUT`: 3f seconds (adjust for game feel)
- `COMBO_MULTIPLIER_BASE`: 2f (exponential growth per combo)

---

### Step 2: Add DailyMissionSystem to Bootstrapper (Phase 4)

**Location**: `Bootstrapper.cs` - Add to Phase 4 (NEW PHASE)

```csharp
// In Phase 4: Initialize Engagement Systems
gameObject.AddComponent<DailyMissionSystem>();
```

**Wiring**: DailyMissionSystem listens to game events

```csharp
// In DailyMissionSystem.Start()
var gameManager = FindObjectOfType<GameManager>();
if (gameManager != null)
{
    gameManager.OnLevelComplete += CheckMissionProgress;
}

var comboSystem = FindObjectOfType<ComboSystem>();
if (comboSystem != null)
{
    comboSystem.OnComboAchieved += CheckComboMission;
}
```

**Configuration**:
- Daily reset time: 00:00 UTC
- Mission refresh: Daily at midnight
- Reward tiers: Easy (100 coins), Medium (300 coins), Hard (500 gems)

**Database Setup** (PlayerPrefs):
```
key: "DailyMissions_ActiveMissions" → JSON serialized array
key: "DailyMissions_LastResetTime" → DateTime.Now.ToLongDateString()
```

---

### Step 3: Add AnalyticsFramework to Bootstrapper (Phase 4)

**Location**: `Bootstrapper.cs` - Add to Phase 4

```csharp
// In Phase 4: Initialize Engagement Systems
gameObject.AddComponent<AnalyticsFramework>();
```

**Wiring**: AnalyticsFramework listens to ALL major game events

```csharp
// In AnalyticsFramework.Start()
var gameManager = FindObjectOfType<GameManager>();
if (gameManager != null)
{
    gameManager.OnLevelStart += () => LogEvent(AnalyticsEvent.LevelStarted);
    gameManager.OnLevelComplete += () => LogEvent(AnalyticsEvent.LevelCompleted);
    gameManager.OnLevelFailed += () => LogEvent(AnalyticsEvent.LevelFailed);
}

var comboSystem = FindObjectOfType<ComboSystem>();
if (comboSystem != null)
{
    comboSystem.OnComboAchieved += (combo) => 
        LogEvent(AnalyticsEvent.ComboAchieved, new { comboCount = combo });
}

var iapManager = FindObjectOfType<IAPManager>();
if (iapManager != null)
{
    iapManager.OnPurchaseSucceeded += (sku) => 
        LogEvent(AnalyticsEvent.PurchaseSucceeded, new { skuId = sku });
}
```

**Configuration**:
```csharp
// In AnalyticsFramework config
public const int SESSION_LENGTH_THRESHOLD_MINUTES = 30; // Long session threshold
public const int FUNNEL_ANALYSIS_WINDOW_DAYS = 7;      // Weekly cohort analysis
public const bool ENABLE_BIGQUERY_EXPORT = false;      // Enable in production
```

---

### Step 4: Add ChurnPredictionEngine to Bootstrapper (Phase 4)

**Location**: `Bootstrapper.cs` - Add to Phase 4

```csharp
// In Phase 4: Initialize Engagement Systems
gameObject.AddComponent<ChurnPredictionEngine>();
```

**Wiring**: ChurnPredictionEngine updates on session end

```csharp
// In ChurnPredictionEngine.Start()
var gameManager = FindObjectOfType<GameManager>();
if (gameManager != null)
{
    gameManager.OnSessionEnd += CalculateChurnRisk;
}
```

**Configuration**:
```csharp
// In ChurnPredictionEngine config
public const float HIGH_RISK_THRESHOLD = 0.7f;  // 70% risk = high risk
public const float VERY_HIGH_RISK_THRESHOLD = 0.85f;  // 85% risk = very high risk

// Feature weights (must sum to 1.0)
public const float WEIGHT_INACTIVITY = 0.4f;
public const float WEIGHT_LOW_SPEND = 0.2f;
public const float WEIGHT_SESSION_LENGTH = 0.2f;
public const float WEIGHT_DIFFICULTY_WALL = 0.1f;
public const float WEIGHT_RETENTION_CURVE = 0.1f;
```

**Firebase ML Kit Integration**:
```csharp
// Requires AndroidManifest.xml permissions:
// <uses-permission android:name="android.permission.INTERNET" />
// <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />

// Initialize Firebase ML Kit in FirebaseManager:
var options = new Firebase.MLKit.FirebaseMLKitOptions();
Firebase.MLKit.FirebaseML.Initialize(options);
```

---

### Step 5: Add DynamicOfferEngine to Bootstrapper (Phase 4)

**Location**: `Bootstrapper.cs` - Add to Phase 4

```csharp
// In Phase 4: Initialize Engagement Systems
gameObject.AddComponent<DynamicOfferEngine>();
```

**Wiring**: DynamicOfferEngine feeds offers to OfferPopupUI

```csharp
// In DynamicOfferEngine.Start()
var churnEngine = FindObjectOfType<ChurnPredictionEngine>();
var iapManager = FindObjectOfType<IAPManager>();

// Request offer when churn risk is high
if (churnEngine.GetChurnRisk() > 0.7f)
{
    var offer = GeneratePersonalizedOffer(churnEngine.GetChurnRisk());
    iapManager.DisplayOfferPopup(offer);
}
```

**Configuration**:
```csharp
// In DynamicOfferEngine config
public const int MAX_OFFERS_PER_SESSION = 3;  // Never spam player
public const int MIN_MINUTES_BETWEEN_OFFERS = 5;  // 5 minute cooldown
public const float BASE_CONVERSION_RATE_SPENDER = 0.15f;  // 15% conversion target
public const float BASE_CONVERSION_RATE_GRINDER = 0.05f;  // 5% conversion target

// Price elasticity factors (per segment)
Dictionary<PlayerSegment, float> elasticityFactors = new()
{
    { PlayerSegment.Spender, 1.2f },      // Willing to pay 20% more
    { PlayerSegment.Grinder, 0.7f },      // Want 30% discount
    { PlayerSegment.Casual, 0.9f },       // Standard pricing
    { PlayerSegment.AtRisk, 0.5f }        // Heavy discounts to retain
};
```

---

## Event Flow Diagrams

### Daily Mission Completion Flow
```
Player Completes Level
    ↓
GameManager.OnLevelComplete event
    ↓
DailyMissionSystem.CheckMissionProgress()
    ↓
Update mission progress (clean X levels)
    ↓
Check if mission completed
    ↓
YES: Grant rewards → Update UI
NO: Continue tracking
```

### Churn-to-Offer Flow
```
Session Ends
    ↓
GameManager.OnSessionEnd event
    ↓
AnalyticsFramework logs session data
    ↓
ChurnPredictionEngine.CalculateChurnRisk()
    ↓
Risk Score Calculated (0-1.0)
    ↓
IF risk > 0.7:
    DynamicOfferEngine.GeneratePersonalizedOffer()
        ↓
        Segment player (Spender/Grinder/Casual/AtRisk)
        ↓
        Calculate price elasticity
        ↓
        Generate offer variant
        ↓
        OfferPopupUI.ShowOffer()
        ↓
        Log event to AnalyticsFramework
```

### Analytics Event Collection
```
Every Game Event
    ↓
System logs: OnGestureComplete, OnLevelComplete, OnPurchaseSuccess, etc.
    ↓
AnalyticsFramework.LogEvent(eventType, customDimensions)
    ↓
Event queued in memory buffer (capacity: 100 events)
    ↓
Every 10 events OR every 5 minutes:
    Send batch to Firebase Analytics
        ↓
        Store in localStorage if offline
        ↓
        Sync when connectivity restored
```

---

## Testing Integration

### 1. Test ComboSystem
```
Steps:
1. Press Play in Editor
2. Execute swipe gesture → Combo count increases
3. Execute another swipe within 3 seconds → Combo multiplier (2x)
4. Execute third swipe within 3 seconds → Combo multiplier (4x)
5. Wait 3+ seconds → Combo resets to 0
6. Verify Console: "Combo multiplier: 2x, 4x, 8x"
```

### 2. Test DailyMissionSystem
```
Steps:
1. Press Play in Editor
2. Check Console for "DailyMissionSystem initialized with 3 daily missions"
3. Navigate to Missions UI
4. Complete a mission objective (e.g., clear 1 level)
5. Verify mission progress updates
6. Verify rewards displayed correctly
7. Check PlayerPrefs for persistence: "DailyMissions_ActiveMissions"
```

### 3. Test ChurnPredictionEngine
```
Steps:
1. Press Play in Editor
2. Check Console for "ChurnPredictionEngine initialized"
3. End session (press Escape or wait for timeout)
4. Console should log: "Player churn risk: X.XX"
5. Verify risk calculation considers:
   - Days inactive (PlayerPrefs "LastPlayDate")
   - Lifetime spend (CurrencyManager.totalGemsSpent)
   - Session length trends (AnalyticsFramework)
6. IF risk > 0.7, verify DynamicOfferEngine triggers
```

### 4. Test DynamicOfferEngine
```
Steps:
1. Set ChurnPredictionEngine risk to >0.7 (manual override in Inspector)
2. Press Play in Editor
3. Verify DynamicOfferEngine.GeneratePersonalizedOffer() called
4. Check Console for "Offer generated for segment: AtRisk"
5. Verify offer shows in OfferPopupUI
6. Verify price adjusted for segment
7. Accept offer and verify PurchaseSucceeded event logged
```

### 5. Test AnalyticsFramework
```
Steps:
1. Press Play in Editor
2. Check Console for "AnalyticsFramework initialized"
3. Execute game actions:
   - Start level → "Event logged: LevelStarted"
   - Complete level → "Event logged: LevelCompleted"
   - Purchase item → "Event logged: PurchaseSucceeded"
4. After 10 events, verify: "Sending batch of 10 events to Firebase"
5. Check Firebase Console → Analytics → Events dashboard
```

---

## Bootstrapper Phase Structure (Updated)

```csharp
public class Bootstrapper : MonoBehaviour
{
    public IEnumerator InitializeGame()
    {
        // Phase 0: System Setup
        yield return new WaitForSeconds(0.1f);  // Let Unity stabilize
        
        // Phase 1: Core Services
        gameObject.AddComponent<FirebaseManager>();     // Firebase init
        gameObject.AddComponent<CloudSaveManager>();    // Cloud persistence
        
        yield return new WaitForSeconds(0.1f);
        
        // Phase 2: Gameplay Systems
        gameObject.AddComponent<GameManager>();          // Game state machine
        gameObject.AddComponent<TimerSystem>();          // Level timer
        gameObject.AddComponent<GestureInput>();         // Input handling
        gameObject.AddComponent<ComboSystem>();          // [NEW] Combo tracking
        
        yield return new WaitForSeconds(0.1f);
        
        // Phase 3: UI Systems
        gameObject.AddComponent<UIManager>();            // UI management
        gameObject.AddComponent<MainHUD>();              // In-game overlay
        gameObject.AddComponent<ShopUI>();               // Shop display
        gameObject.AddComponent<EnergyUI>();             // Energy management
        gameObject.AddComponent<OfferPopupUI>();         // Dynamic offers
        
        yield return new WaitForSeconds(0.1f);
        
        // Phase 4: Engagement & Analytics Systems [NEW]
        gameObject.AddComponent<AnalyticsFramework>();   // Event tracking
        gameObject.AddComponent<DailyMissionSystem>();   // Daily challenges
        gameObject.AddComponent<ChurnPredictionEngine>(); // ML risk scoring
        gameObject.AddComponent<DynamicOfferEngine>();   // Personalized offers
        
        yield return new WaitForSeconds(0.1f);
        
        Debug.Log("✅ Game initialization complete! All systems online.");
        Debug.Log($"Total systems initialized: 13 core systems + 4 new engagement systems");
    }
}
```

---

## Performance Impact

### Memory Footprint (Per System)
| System | Approx. Memory | Notes |
|--------|---|---|
| ComboSystem | ~50 KB | Simple int/float tracking |
| DailyMissionSystem | ~200 KB | Stores 3-4 missions + rewards |
| AnalyticsFramework | ~500 KB | Event buffer (100 events max) |
| ChurnPredictionEngine | ~100 KB | Risk scoring + historical data |
| DynamicOfferEngine | ~300 KB | Offer templates + pricing variants |
| **Total New Systems** | **~1.15 MB** | ~15% of typical mobile game |

### CPU Impact (Per Frame)
| System | Per-Frame Cost | Trigger Frequency |
|--------|---|---|
| ComboSystem | <0.1ms | Gesture inputs only |
| DailyMissionSystem | <0.5ms | Level completion (once per 2-3 min) |
| AnalyticsFramework | <0.1ms | Batched (every 10 events or 5 min) |
| ChurnPredictionEngine | <2ms | Session end only (once per session) |
| DynamicOfferEngine | <1ms | Offer request only (few times per session) |
| **Total New Systems** | **<4ms per session** | Negligible gameplay impact |

---

## Configuration Tuning Guide

### For Engagement (More Retention)
```csharp
// ComboSystem: Make combos easier to maintain
COMBO_TIMEOUT = 4f;  // Increased from 3f

// DailyMissionSystem: Make missions easier
Daily missions: 2 missions instead of 3
Hard mission target: 5 levels instead of 10

// ChurnPredictionEngine: More aggressive intervention
HIGH_RISK_THRESHOLD = 0.6f;  // Trigger earlier (was 0.7f)
WEIGHT_INACTIVITY = 0.3f;    // Less aggressive on inactivity
```

### For Monetization (Higher ARPU)
```csharp
// DynamicOfferEngine: More aggressive pricing
elasticityFactors[PlayerSegment.Spender] = 1.5f;  // 50% higher pricing
MAX_OFFERS_PER_SESSION = 5;  // More offer opportunities

// ChurnPredictionEngine: More intervention offers
VERY_HIGH_RISK_THRESHOLD = 0.75f;  // Trigger more discounts

// OfferPopupUI: More aggressive display
OFFER_DISPLAY_COOLDOWN = 3 minutes  // More frequent offers
```

### For Data Collection (Better Analytics)
```csharp
// AnalyticsFramework: More granular tracking
SESSION_LENGTH_THRESHOLD_MINUTES = 15;  // More detailed segmentation
Enable detailed event logging:
  - Per-hazard performance
  - Per-gesture tracking
  - Per-world difficulty analysis
```

---

## Debugging Checklist

- [ ] All 5 systems initialize without errors
- [ ] ComboSystem receives gesture events
- [ ] DailyMissionSystem updates on level completion
- [ ] ChurnPredictionEngine calculates risk >0 (not always 0)
- [ ] DynamicOfferEngine triggers when risk is high
- [ ] AnalyticsFramework logs Firebase events
- [ ] No circular dependencies between systems
- [ ] No memory leaks on long play sessions (monitor with Profiler)
- [ ] Firebase rules allow read/write for all systems
- [ ] PlayerPrefs persistence works after app restart

---

## Deployment Checklist

- [ ] All systems have release builds tested
- [ ] #if UNITY_EDITOR guards removed from release code
- [ ] Firebase rules updated for production
- [ ] Analytics events configured in Firebase Console
- [ ] Churn prediction model trained (optional: requires ML Kit)
- [ ] Dynamic offers validated with real pricing data
- [ ] Load testing: Verified <50ms startup time
- [ ] Memory profiling: Verified <100 MB total
- [ ] Battery profiling: Verified <5% battery per hour
- [ ] Network profiling: Verified <5 MB per session (offline-capable)

---

## Next Steps (Phase 2: Weeks 2-4)

1. **Achievement System** (5 tiers × 10 achievements = 50 total)
2. **Battle Pass System** (100 tiers, 2-week seasons)
3. **Leaderboards & Social** (weekly global + friend challenges)
4. **Seasonal Events** (2-week themed events with exclusive rewards)
5. **Advanced Haptics** (haptic symphonies, tactile feedback loops)

All systems will follow the same integration pattern shown above.
