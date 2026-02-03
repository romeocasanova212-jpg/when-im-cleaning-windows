# Unity Editor Quick Start Guide

**Date**: January 28, 2026  
**Status**: Phase 2 Alpha Complete - Ready for Testing  
**Time Required**: ~5 minutes for first playable build

---

## 🎯 Quick Setup (Automated)

### Step 1: Open Project in Unity
1. Launch **Unity Hub**
2. Click **"Add"** → Select project folder
3. Open with **Unity 6.3 LTS**
4. Wait for initial import (~5 minutes first time)

### Step 2: Install Required Packages
Unity → Window → Package Manager → Install:
- **Input System** (for Enhanced Touch)
- **TextMeshPro** (for UI)
- *(Optional)* **DOTween** (for animations, has fallbacks)

### Step 3: Auto-Generate Scene
1. Unity Editor → **Tools** → **When I'm Cleaning Windows** → **Setup MainGame Scene**
2. Check all options (Camera, Lighting, Systems, Window, UI, Debug)
3. Click **"Create MainGame Scene"** button
4. Wait ~5 seconds for automatic generation
5. Scene saved to `Assets/Scenes/MainGame.unity`

### Step 4: Create Configuration Assets
1. Assets → **Create** → **When I'm Cleaning Windows** → **Game Config**
2. Assets → **Create** → **When I'm Cleaning Windows** → **Level Config**
3. Assets → **Create** → **When I'm Cleaning Windows** → **Audio Config**
4. *(Optional)* Tweak settings in Inspector

### Step 5: Generate Test Prefabs (Optional)
1. Unity Editor → **Tools** → **When I'm Cleaning Windows** → **Create Test Prefabs**
2. Instant generation of:
   - WindowQuad.prefab
   - HazardQuad.prefab
   - CleaningParticle.prefab
   - Materials (Clean, Dirty, Hazard, Frame)

### Step 6: Press Play!
1. Open `MainGame.unity` scene
2. Press **Play** button in Unity Editor
3. Use **mouse drag** to clean window (touch simulation)
4. Watch console for level start confirmation

---

## 🎮 Testing Controls

### Mouse Controls (Editor)
- **Left Click + Drag**: Clean window
- **Circle Motion**: Circle scrub gesture (270° to trigger)
- **Double-Click**: Double-tap gesture

### Debug Hotkeys
- **` (Backtick)**: Toggle Debug Console
  - Type `help` for command list
  - Try: `level 10`, `nuke`, `gems 1000`

- **F1**: Toggle Level Test Manager UI
  - Instant level completion
  - Spawn specific hazards
  - Activate power-ups
  - Adjust clean percentage

- **F2**: Instant Complete Level
- **F3**: Activate Nuke Power-Up
- **F4**: Activate Turbo (2.5× speed)
- **F5**: Activate Auto-Pilot
- **F6**: Reset Window (100% dirty)

### Input Debugger (Visualization)
- **T**: Toggle touch point visualization
- **P**: Toggle cleaning path trails
- **C**: Toggle circle detection progress
- **G**: Toggle gesture info panel
- **M**: Toggle performance metrics (FPS)
- **R**: Reset path tracking

---

## 🧪 What to Test

### Core Gameplay Loop
1. **Level Start**
   - Energy consumed (5/5 → 4/5)
   - Window appears with hazards
   - Timer starts (120s for World 1)
   - Clean percentage shows 0%

2. **Cleaning Mechanics**
   - Mouse drag removes dirt
   - Continuous cleaning (no click spam needed)
   - Circle scrub detection (270° accumulated angle)
   - Hazard removal (visual feedback)

3. **Level Completion**
   - Reach 95% clean threshold
   - Level complete popup
   - Stars calculated (based on time remaining)
   - Coins awarded (5 + 5×stars + VIP multiplier)
   - Next level unlocked

4. **Energy System**
   - Energy depletes on level start
   - Refill popup when energy = 0
   - VIP unlimited energy (if activated)

### Power-Ups (via Debug Console)
1. **Nuke** (`F3` or `nuke` command)
   - Instant 100% clean
   - VFX explosion (if prefabs exist)
   - Audio feedback

2. **Turbo** (`F4` or `turbo` command)
   - 2.5× cleaning power
   - Lasts 10 seconds
   - Visual trail effect

3. **Auto-Pilot** (`F5` or `autopilot` command)
   - Automatic cleaning
   - Random spot selection
   - Lasts 10 seconds

### Hazard Types
Use Level Test Manager to spawn specific hazards:
- **Static**: Bird Poop, Flies, Salts, Water Spots, Oil, Mud, etc.
- **Regenerating**: Frost, Algae, Sap, Fog, Nano-Bots, Pollen, Blood, Pollution
  - Watch hazards spread when clean% < 80%

### Currency System
1. Complete levels → Earn coins
2. 3-star completion → 5% chance for gems
3. VIP multipliers (Bronze 2.5×, Silver 3×, Gold 4×)
4. Debug commands:
   - `gems 1000` - Add gems
   - `coins 5000` - Add coins

### Progression
1. Level unlocking (sequential)
2. World progression (100 floors × 10 rooms = 1000 levels/world)
3. Difficulty scaling (120s → 40s timer, 8 → 25 hazards)

---

## 🔍 Validation Checklist

Run these checks after setup:

### Scene Validation
1. Tools → When I'm Cleaning Windows → **Validate Scene Setup**
2. Check Console for ✓/✗ status:
   - ✓ Main Camera found
   - ✓ Bootstrapper found
   - ✓ WindowMeshController found
   - ✓ HazardRenderer found
   - ✓ CleaningController found
   - ✓ Canvas found
   - ✓ EventSystem found

### Performance Check
1. Press Play
2. Check FPS (InputDebugger shows this)
3. Target: 60+ FPS in Editor (120 FPS on device)
4. Console should show:
   - `[Bootstrapper] Initializing all systems...`
   - `[GameManager] All systems initialized`
   - `[WindowMeshController] Generated mesh: 1024 vertices`

### Gameplay Verification
1. Debug Console: `level 1`
2. Clean window with mouse
3. Watch clean percentage increase (MainHUD or console)
4. Verify level completes at 95%+
5. Check level complete screen appears
6. Confirm coins awarded
7. Next level unlocks

---

## 🐛 Troubleshooting

### Scene Setup Failed
**Symptom**: "Missing component" errors in Console  
**Fix**: Tools → When I'm Cleaning Windows → Find Missing Components

### Input Not Working
**Symptom**: Mouse drag doesn't clean  
**Fix 1**: Ensure Input System package installed  
**Fix 2**: Check CleaningController component exists on Window GameObject  
**Fix 3**: Camera must be tagged "MainCamera"

### No Window Visible
**Symptom**: Blank screen when playing  
**Fix 1**: Check Window GameObject is active  
**Fix 2**: Camera orthographic size = 5, position (0, 0, -10)  
**Fix 3**: WindowMeshController must call GenerateWindowMesh() on Start

### Performance Issues
**Symptom**: <30 FPS in Editor  
**Fix 1**: Close other Unity projects  
**Fix 2**: Reduce mesh subdivisions in LevelConfig (32 → 16)  
**Fix 3**: Disable VFX in GameConfig  
**Fix 4**: Lower particle quality to "Low"

### Hazards Not Spawning
**Symptom**: Clean window appears, no dirt  
**Fix**: Debug Console → `level 1` to force level restart  
Check Console for: `[HazardRenderer] Spawned X hazards`

### UI Not Responding
**Symptom**: Can't click buttons  
**Fix**: Ensure EventSystem exists in scene (auto-created by SceneSetupUtility)

---

## 📊 Expected Console Output

When working correctly, Console shows:
```
[Bootstrapper] Initializing all systems...
[Bootstrapper] Phase 0: Initializing Firebase...
[Bootstrapper] Phase 1: Initializing core systems...
[GameManager] All systems initialized
[WindowMeshController] Generated mesh: 1024 vertices
[LevelGenerator] Generated level 1: World 1, Floor 1, Room 1
[HazardRenderer] Spawned 8 hazards
[GameManager] Level 1 started! (8 hazards, 120s)
```

During gameplay:
```
[CleaningController] Cleaning at (2.34, 1.56) radius=0.5 power=0.3
[WindowMeshController] Clean percentage: 45.2%
[HazardRenderer] Hazard 3 cleaned (45% remaining)
```

On level complete:
```
[CleaningController] Level complete! 97.3% clean
[GameManager] Level 1 complete! 3 stars, 20 coins
```

---

## 🎯 Next Steps After Testing

1. **Firebase Integration**
   - Install Firebase Unity SDK
   - Configure Firebase Console project
   - Enable Analytics, Crashlytics, Remote Config, Cloud Storage

2. **Unity IAP Setup**
   - Install Unity IAP package
   - Configure Google Play Console
   - Configure App Store Connect
   - Test sandbox purchases

3. **FMOD Audio**
   - Install FMOD Studio 2.02 plugin
   - Import 250+ ASMR sound samples
   - Create FMOD events
   - Test binaural audio with headphones

4. **Visual Assets**
   - Create 24 hazard textures (512×512 PNG)
   - Create 10 window frame sprite sheets
   - Create 22 VFX particle prefabs
   - Replace procedural fallbacks

---

## 📚 Additional Resources

- **[PROJECT_STATUS.md](PROJECT_STATUS.md)** - Full implementation status
- **[GAME_DESIGN_DOCUMENT.md](GAME_DESIGN_DOCUMENT.md)** - Complete GDD v5.0
- **[UNITY_SCENE_SETUP.md](UNITY_SCENE_SETUP.md)** - Manual scene setup guide
- **[UNITY_IAP_SETUP.md](UNITY_IAP_SETUP.md)** - IAP configuration guide

---

**Questions?** Check Console logs first, then review [PROJECT_STATUS.md](PROJECT_STATUS.md) for troubleshooting.

**Ready to test!** Press Play and start cleaning! 🪟✨
