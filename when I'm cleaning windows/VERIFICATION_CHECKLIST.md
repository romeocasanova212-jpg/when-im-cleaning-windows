# Quick Verification Checklist

## Pre-Play Checks (Complete These Before Pressing Play)

### ✅ Setup Completion
- [ ] Ran "Tools → When I'm Cleaning Windows → Setup Project (Full)"
- [ ] No errors in console during setup
- [ ] Assets/Resources/Config/ contains: GameConfig.asset, LevelConfig.asset, AudioConfig.asset
- [ ] Assets/Prefabs/ contains: WindowQuad.prefab, HazardQuad.prefab, CleaningParticle.prefab
- [ ] Assets/Scenes/ contains: MainGame.unity

### ✅ Scene Verification
- [ ] Open MainGame.unity scene
- [ ] Hierarchy shows all expected GameObjects:
  - Main Camera
  - Directional Light
  - _GameSystems (with Bootstrapper component)
  - Window (with WindowMeshController component)
  - Canvas (with 11 UI screens)
  - EventSystem
  - DebugTools (optional)

### ✅ Component Check
- [ ] MainHUDScreen has MainHUD component attached
- [ ] LevelCompleteScreen has LevelCompleteUI component attached
- [ ] ShopScreen has ShopUI component attached
- [ ] EnergyRefillScreen has EnergyUI component attached
- [ ] OfferPopupScreen has OfferPopupUI component attached
- [ ] Canvas has GraphicRaycaster attached

---

## Play Mode Checks (Execute These After Pressing Play)

### Phase 1: Initialization
- [ ] Console shows "[Bootstrapper] ✓ Initialization Complete" log
- [ ] Console shows "[GameManager] Game initialized" log
- [ ] No null reference errors in console
- [ ] No missing component warnings

### Phase 2: Main Menu
- [ ] MainMenuScreen is visible
- [ ] Contains at least: Play button, Shop button, Settings button
- [ ] Buttons are clickable (not greyed out)

### Phase 3: Level Start
- [ ] Click Play button → LevelSelectScreen appears
- [ ] Select Level 1 → Game enters level
- [ ] MainHUDScreen appears with:
  - [ ] Level number displayed (e.g., "Level 1")
  - [ ] Timer showing 120 seconds (02:00)
  - [ ] Clean percentage showing 0%
  - [ ] Gems, coins, energy visible
  - [ ] Buttons (Pause, Power-Up) visible

### Phase 4: Gameplay Interaction
- [ ] Drag/click on the window area → Clean percentage increases
- [ ] Clean percentage updates in real-time as you drag (0% → 50% → 95%+)
- [ ] Timer counts down smoothly
- [ ] No lag or stuttering when dragging

### Phase 5: Level Complete
- [ ] When clean percentage reaches 95%, level complete triggers automatically
- [ ] LevelCompleteScreen appears with:
  - [ ] Level number displayed
  - [ ] Star rating displayed (1-3 stars based on time remaining)
  - [ ] Time remaining shown
  - [ ] Clean percentage shown (should be 100%)
  - [ ] Coins earned displayed
  - [ ] Buttons: Next Level, Retry, Menu

### Phase 6: Navigation
- [ ] Click Next Level → Game loads level 2 (or loops to level 1 if at max)
- [ ] Click Menu → Returns to MainMenuScreen
- [ ] Click Retry → Restarts current level with clean percentage at 0%

### Phase 7: Energy System (Optional)
- [ ] Press Play multiple times until energy depletes (starts at 5)
- [ ] After energy depletes, Play button shows energy refill required
- [ ] Energy UI appears showing refill options (watch ad or spend gems)

---

## Console Log Expectations

You should see these logs appear in order on Play:

```
=== BOOTSTRAPPER: Initializing Game Systems ===
[FirebaseManager] (if Firebase is configured)
[RemoteConfigManager] 
[CloudSaveManager]
[EnergySystem] ✓
[CurrencyManager] ✓
[HazardSystem] ✓
[AudioManager] ✓
[TextureManager] ✓
[VFXManager] ✓
[AnimationManager] ✓
[IAPManager] ✓
[VIPManager] ✓
[PersonalizationEngine] ✓
[LevelGenerator] ✓
[GameManager] ✓
[UIManager] ✓
=== BOOTSTRAPPER: ✓ Initialization Complete (took X.XX seconds) ===
```

---

## Common Issues & Fixes

### Issue: "Canvas not found"
**Fix**: Make sure MainGame.unity scene is saved. The scene setup creates a Canvas automatically.

### Issue: UI elements not appearing
**Fix**: Check that Canvas render mode is set to "Screen Space - Overlay". The scene setup handles this automatically.

### Issue: MainHUD not showing clean percentage
**Fix**: This requires WindowMeshController to be in the scene. Scene setup creates this automatically as "Window" GameObject.

### Issue: Buttons not clickable
**Fix**: Verify EventSystem exists in scene hierarchy. Scene setup creates this automatically.

### Issue: Level doesn't complete at 95%
**Fix**: Check GameManager.cs has levelCompleteThreshold set to 95f (it should be in GameConfig or hardcoded default).

---

## Performance Targets

**FPS**: 60 fps target on device
**Memory**: <500MB for base scene
**UI Update Time**: <1ms per frame for MainHUD updates
**Clean Percentage Update**: <0.5ms per event broadcast

---

## Success Criteria

✅ All checks passed = Project is ready for development

If any check fails:
1. Check console for specific error messages
2. Verify all setup steps completed successfully
3. Restart Unity and try again
4. Check this document's "Common Issues & Fixes" section

---

**Last Updated**: February 1, 2026
**Version**: 1.0 - Final Automation Build
