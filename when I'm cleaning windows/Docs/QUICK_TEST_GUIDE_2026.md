# Quick Start: Testing the 2026 Enhancement Systems

**Time Required**: ~15 minutes per system
**Prerequisites**: Unity 6.3 LTS open with the project loaded

---

## System Test Order

### ✅ Test 1: ComboSystem (5 minutes)

**What to Test**: Gesture chain multipliers and haptic feedback

**Steps**:
1. Open `Scene_GamePlay.unity` or create a test scene
2. Press Play
3. Add `ComboSystem` to the scene (Ctrl+Shift+C → ComboSystem)
4. Execute a level completion gesture
5. Console should show: `Combo: 1x | Multiplier: 1.00x`
6. Complete another level within 3 seconds
7. Console should show: `Combo: 2x | Multiplier: 2.00x`
8. Complete a 3rd level
9. Console should show: `Combo: 3x | Multiplier: 4.00x`
10. Wait 3+ seconds (timeout)
11. Console should show: `Combo reset! Final combo: 3x`

**Expected Results**:
- ✅ Combo counter increments
- ✅ Multiplier follows exponential curve (2^n)
- ✅ Timeout resets combo
- ✅ Haptic feedback on each gesture (if device supports it)

**Debug**:
```csharp
// If multiplier not updating, check:
var comboSystem = FindObjectOfType<ComboSystem>();
Debug.Log($"Current Combo: {comboSystem.GetCurrentCombo()}");
Debug.Log($"Multiplier: {comboSystem.GetScoreMultiplier()}");
```

---

### ✅ Test 2: DailyMissionSystem (5 minutes)

**What to Test**: Mission creation, progress tracking, persistence

**Steps**:
1. Open Inspector → Player Preferences → Clear all (to reset missions)
2. Add `DailyMissionSystem` to scene
3. Press Play
4. Console should show:
   ```
   DailyMissionSystem initialized with 3 daily missions
   Mission 1: Clean 5 levels (Progress: 0/5)
   Mission 2: Get 3 combos (Progress: 0/3)
   Mission 3: Spend 50 gems (Progress: 0/50)
   ```
5. Complete 1 level → Mission 1 progress: 1/5
6. Complete 4 more levels → Mission 1 progress: 5/5 (COMPLETE!)
7. Console shows: `Mission completed! Reward: 500 gems`
8. Stop Play, delete PlayerPrefs key "DailyMissions_ActiveMissions"
9. Press Play again
10. Missions should refresh (new timestamps)

**Expected Results**:
- ✅ 3 daily missions created on first run
- ✅ Progress updates as objectives met
- ✅ Completion triggers rewards
- ✅ Persistence survives Stop/Play cycle
- ✅ Daily reset at midnight UTC

**Debug**:
```csharp
// Check mission status
var missionSystem = FindObjectOfType<DailyMissionSystem>();
var missions = missionSystem.GetActiveMissions();
foreach (var mission in missions)
{
    Debug.Log($"{mission.missionId}: {mission.currentProgress}/{mission.targetValue}");
}
```

---

### ✅ Test 3: AnalyticsFramework (5 minutes)

**What to Test**: Event logging and Firebase integration

**Steps**:
1. Add `AnalyticsFramework` to scene
2. Press Play
3. Console should show:
   ```
   AnalyticsFramework initialized
   Firebase Analytics connected
   Session tracking enabled
   ```
4. Perform game actions:
   - Complete a level → `Event logged: LevelCompleted`
   - Achieve combo → `Event logged: ComboAchieved`
   - Purchase item → `Event logged: PurchaseSucceeded`
5. After 10 events, console shows:
   ```
   Sending batch of 10 events to Firebase
   Batch sent successfully
   ```
6. Open Firebase Console → Analytics → Events
7. Verify events appear in real-time dashboard (may take 30 seconds)

**Expected Results**:
- ✅ Events logged to memory buffer
- ✅ Batch sent to Firebase every 10 events or 5 minutes
- ✅ Events visible in Firebase Console after 1 minute
- ✅ Offline persistence (events queued if no connection)

**Debug**:
```csharp
// Check event buffer status
var analytics = FindObjectOfType<AnalyticsFramework>();
Debug.Log($"Queued events: {analytics.GetEventBufferSize()}/100");

// Manually log custom event
analytics.LogEvent("test_event", new { 
    test_value = 42, 
    timestamp = DateTime.Now 
});
```

---

### ✅ Test 4: ChurnPredictionEngine (3 minutes)

**What to Test**: Risk score calculation and intervention triggers

**Steps**:
1. Add `ChurnPredictionEngine` to scene
2. Press Play
3. Let session run for 30+ seconds
4. In Editor, set PlayerPrefs values:
   ```
   PlayerPrefs.SetString("LastPlayDate", 
       DateTime.Now.AddDays(-5).ToString()); // 5 days inactive
   ```
5. Stop and Play again
6. End session (press Escape or wait 2 minutes)
7. Console should show:
   ```
   ChurnPredictionEngine - Calculating churn risk...
   Days inactive: 5 (Factor: 0.50)
   Lifetime spend: $0 (Factor: 1.0)
   Session trend: Good (Factor: 0.2)
   ---> Final Churn Risk: 0.73 [HIGH RISK]
   
   Action: Triggering intervention offer
   ```

**Expected Results**:
- ✅ Risk score calculated (0.0-1.0)
- ✅ Risk increases with inactivity
- ✅ Risk tier appropriate (Low/Medium/High/Critical)
- ✅ Intervention triggers automatically
- ✅ Offer engine notified (next test)

**Debug**:
```csharp
// Check risk calculation
var churnEngine = FindObjectOfType<ChurnPredictionEngine>();
float risk = churnEngine.GetChurnRisk();
Debug.Log($"Current churn risk: {risk:P2}");

// Force high risk (for testing)
churnEngine.SetChurnRisk(0.9f);
```

---

### ✅ Test 5: DynamicOfferEngine (3 minutes)

**What to Test**: Personalized offer generation and display

**Steps**:
1. Add `DynamicOfferEngine` to scene
2. Set ChurnPredictionEngine risk to >0.7 (see Test 4)
3. Press Play
4. ChurnPredictionEngine calculates risk
5. DynamicOfferEngine triggers:
   ```
   DynamicOfferEngine - Generating personalized offer
   Player segment: At-Risk
   Price elasticity: 0.5x (50% discount)
   Offer type: Energy Refill (High value for at-risk players)
   Final price: $2.49 (normally $4.99)
   
   Action: Displaying offer popup
   ```
6. Check if OfferPopupUI appears
7. Accept offer
8. Console shows:
   ```
   Event logged: PurchaseSucceeded
   AnalyticsFramework notified
   Player engagement improved
   ```

**Expected Results**:
- ✅ Offer generated based on segment
- ✅ Price adjusted per elasticity factor
- ✅ Offer displayed at right time
- ✅ Acceptance logged to analytics
- ✅ No offer spam (max 3 per session)

**Debug**:
```csharp
// Check player segment
var offerEngine = FindObjectOfType<DynamicOfferEngine>();
var segment = offerEngine.GetPlayerSegment();
Debug.Log($"Player segment: {segment}");

// Generate test offer
var offer = offerEngine.GenerateTestOffer();
Debug.Log($"Offer: {offer.name} at ${offer.price}");
```

---

## Integration Test Checklist

After testing all 5 systems individually, run this integration test:

### Full Game Session Test (10 minutes)

**Steps**:
1. Press Play with all 5 systems active
2. Complete 3 levels in sequence
3. Verify:
   - ✅ ComboSystem tracks combos (Console: `Combo: 1x, 2x, 3x`)
   - ✅ DailyMissionSystem updates progress
   - ✅ AnalyticsFramework logs events (Console: `Event logged:`)
4. Complete 5 more levels to trigger mission reward
5. Verify:
   - ✅ Mission completion reward granted
   - ✅ Reward logged to analytics
6. Wait for session to end (2+ minutes)
7. Verify:
   - ✅ ChurnPredictionEngine calculates risk
   - ✅ If risk > 0.7: DynamicOfferEngine shows offer
   - ✅ Offer displayed in OfferPopupUI
8. Accept or decline offer
9. Verify:
   - ✅ All events in analytics buffer
   - ✅ Console shows no errors

**Expected Results**:
- ✅ All systems working in harmony
- ✅ No error messages in Console
- ✅ 0 frame rate drops (Profile if needed)
- ✅ Memory stable (<100 MB)

---

## Performance Profiling

### CPU Profiling
1. Window → Analysis → Profiler
2. Press Play
3. In Profiler, select CPU
4. Complete several levels
5. Check frame time breakdown:
   - `ComboSystem`: Should be <0.1ms
   - `DailyMissionSystem`: <0.5ms
   - `AnalyticsFramework`: <0.1ms (batched)
   - `ChurnPredictionEngine`: <2ms (session end only)
   - `DynamicOfferEngine`: <1ms (offer request only)

### Memory Profiling
1. Window → Analysis → Profiler
2. Press Play
3. In Profiler, select Memory
4. Complete 10+ levels
5. Check memory allocations:
   - Total should stay <100 MB
   - No unusual spikes (indicates memory leak)
   - GC.Alloc should be minimal

### Network Profiling (Optional)
1. Window → Analysis → Profiler
2. Add "Network" module
3. Check Firebase event data:
   - Batch size: ~1 KB per 10 events
   - Send frequency: Every 10 events or 5 minutes
   - No unnecessary requests

---

## Troubleshooting Guide

### ComboSystem Not Incrementing
**Problem**: Combo stays at 0x
**Solution**: 
- Check GameManager is active: `FindObjectOfType<GameManager>() != null`
- Verify gesture events firing: Add logging to `GestureInput.OnGestureComplete`
- Check `IsLevelActive` property in GameManager

### DailyMissionSystem Not Persisting
**Problem**: Missions reset on app restart
**Solution**:
- Check PlayerPrefs key exists: `PlayerPrefs.HasKey("DailyMissions_ActiveMissions")`
- Verify CloudSave integration (if using)
- Check disk space (PlayerPrefs needs write access)

### ChurnPredictionEngine Showing 0% Risk
**Problem**: Risk always calculated as 0.0
**Solution**:
- Check player has gameplay history (at least 1 session)
- Verify PlayerPrefs data exists
- Check Firebase ML Kit is initialized
- Review risk calculation weights (must sum to 1.0)

### DynamicOfferEngine Not Triggering
**Problem**: Offers never appear
**Solution**:
- Verify ChurnPredictionEngine risk > 0.7
- Check OfferPopupUI is in scene
- Verify offer display cooldown not blocking (5 min)
- Check Firebase SDK initialized

### AnalyticsFramework Not Sending
**Problem**: Events queue but never send
**Solution**:
- Check Firebase authentication credentials
- Verify network connection
- Check Firebase project rules allow write
- Review event buffer capacity (100 events max)

---

## Expected Console Output (Full Test)

```
[Bootstrapper] Initializing game systems...

[ComboSystem] ComboSystem initialized and ready for gesture chains
[DailyMissionSystem] DailyMissionSystem initialized with 3 daily missions
[AnalyticsFramework] AnalyticsFramework initialized
[ChurnPredictionEngine] ChurnPredictionEngine initialized

[GameManager] Level started: Scene_Level_001
[AnalyticsFramework] Event logged: LevelStarted with customDimensions

[GestureInput] Swipe detected
[ComboSystem] Combo: 1x | Multiplier: 1.00x

[GameManager] Level completed! Stars earned: 3
[ComboSystem] Combo: 2x | Multiplier: 2.00x
[DailyMissionSystem] Mission 1 progress: 1/5 (Clean levels)
[AnalyticsFramework] Event logged: LevelCompleted with customDimensions

... (repeat for more levels)

[DailyMissionSystem] Mission 1 completed! Reward: 500 gems
[AnalyticsFramework] Event logged: MissionCompleted

[GameManager] Session ended after 240 seconds
[AnalyticsFramework] Sending batch of 10 events to Firebase
[AnalyticsFramework] Batch sent successfully

[ChurnPredictionEngine] Calculating churn risk...
[ChurnPredictionEngine] Player churn risk: 0.45 [LOW RISK]

✅ All systems operational
```

---

## Next Steps After Testing

Once all systems pass testing:

1. **Integration** (1 hour)
   - Add systems to Bootstrapper.cs Phase 4
   - Wire all event connections
   - Run Play mode for 5+ minutes

2. **Optimization** (30 minutes)
   - Profile and identify bottlenecks
   - Optimize any >2ms system
   - Validate memory usage

3. **Documentation** (30 minutes)
   - Update UNITY_EDITOR_QUICKSTART.md
   - Create tuning guide for designers
   - Add troubleshooting FAQ

4. **Production Build** (15 minutes)
   - Build for Android
   - Verify startup time <2 seconds
   - Test on real device

---

## Test Status Tracker

Mark off as you complete each test:

- [ ] ComboSystem test passed
- [ ] DailyMissionSystem test passed
- [ ] AnalyticsFramework test passed
- [ ] ChurnPredictionEngine test passed
- [ ] DynamicOfferEngine test passed
- [ ] Integration test passed
- [ ] CPU profiling completed
- [ ] Memory profiling completed
- [ ] Console output verified
- [ ] Ready for production build

---

**Estimated Total Time**: 45 minutes
**Difficulty**: Beginner-friendly (mostly verification)
**Support**: See INTEGRATION_GUIDE_2026.md for more details
