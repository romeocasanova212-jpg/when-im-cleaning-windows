# 🪟 When I'm Cleaning Windows

[![Unity](https://img.shields.io/badge/Unity-6.3_LTS-black?logo=unity)](https://unity.com)
[![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS-blue)](https://github.com/romeocasanova212-jpg/when-im-cleaning-windows)
[![License](https://img.shields.io/badge/License-Proprietary-red)](LICENSE)
[![Status](https://img.shields.io/badge/Status-Prototype-yellow)](https://github.com/romeocasanova212-jpg/when-im-cleaning-windows)

**Hyper-casual ASMR window cleaning game inspired by PlayStation 2's EyeToy: Play "Wishi Washi" minigame**

> *Rise from rookie washer to Glass God. Swipe suds, scrub hazards, and share your shine.* 🧽✨

---

## 📖 Overview

**When I'm Cleaning Windows** is a mobile-first action simulator that combines:
- 🎮 **Nostalgia** (PS2 EyeToy generation, now mobile-native)
- 🎧 **ASMR** (250+ satisfying sounds, haptic feedback)
- ♾️ **Infinite Content** (10,000+ procedurally-generated levels)
- 💎 **Fair Monetization** (Months to complete free, hours with IAP)
- 📱 **2026-Optimized** (120FPS OLED, NPU-accelerated ML, Snapdragon 4-8 Gen support)

### 🎯 Core Loop (90 Seconds)
```
Spend Life → Clean Windows (Swipe/Scrub/Spray) → 95% Clean → Shine! → Next Floor → Stars → Rewards
```

### 🌍 10 Worlds × 100 Levels
From grimy Back Alleys to zero-gravity Space Stations—each with unique hazards (bird poop, frost, acid rain, nano-bots, etc.)

---

## 🚀 Quick Start

### Prerequisites
- **Unity 6.3 LTS** (URP template)
- **Android Studio** (for Android builds)
- **Xcode 15+** (for iOS builds, macOS only)
- **Git** configured with GitHub credentials

### Clone & Setup
```bash
git clone https://github.com/romeocasanova212-jpg/when-im-cleaning-windows.git
cd when-im-cleaning-windows
```

### Open in Unity
1. Launch **Unity Hub**
2. Click **"Add"** → Select cloned folder
3. Open with **Unity 6.3 LTS**
4. Wait for package imports (~5 minutes first time)

### Play Prototype
1. Open scene: `Assets/Scenes/Prototype_World1.unity`
2. Press **Play** in Editor
3. Swipe with mouse (touch simulation)

---

## 📂 Project Structure

```
when-im-cleaning-windows/
├── Assets/
│   ├── Scenes/
│   │   └── Prototype_World1.unity          # Week 1 prototype scene
│   ├── Scripts/
│   │   ├── Procedural/
│   │   │   ├── PerlinNoise.cs             # Suds generation (Burst-compiled)
│   │   │   ├── PoissonDiskSampling.cs     # Hazard placement (O(1) spatial grid)
│   │   │   ├── CellularAutomata.cs        # Regeneration mechanics (3%/sec)
│   │   │   └── AISolvabilityBot.cs        # Post-gen validation (92% threshold)
│   │   ├── Mechanics/
│   │   │   ├── GestureInput.cs            # Enhanced Touch multi-touch
│   │   │   ├── SudsPhysics.cs             # PhysX deformable suds
│   │   │   └── TimerSystem.cs             # Level timer (90s → 35s scaling)
│   │   ├── Monetization/                  # (Week 2-4: Alpha)
│   │   └── UI/                            # (Week 1: Basic HUD)
│   ├── Prefabs/
│   │   └── WindowFloor.prefab             # Reusable floor template
│   ├── Audio/
│   │   └── ASMR_SFX/                      # 250+ squeegee/spray sounds
│   ├── Materials/
│   │   └── GlassPBR.mat                   # Ray-traced glass (URP)
│   └── Addressables/                      # (Week 5+: On-demand worlds)
├── Docs/
│   ├── GAME_DESIGN_DOCUMENT.md            # Full GDD (you're here!)
│   └── TECHNICAL_SPEC.md                  # Algorithm deep-dive
├── ProjectSettings/                        # Unity project config
├── README.md                               # This file
└── .gitignore                              # Unity .gitignore template
```

---

## 🛠️ Tech Stack (2026 Production-Ready)

### Core Engine
| Component | Technology | Purpose |
|-----------|-----------|---------|
| **Engine** | Unity 6.3 LTS | Mobile-first, ECS-ready |
| **Rendering** | Universal Render Pipeline (URP) | 120FPS OLED optimization |
| **Performance** | Burst Compiler + Job System | <80ms procedural gen |
| **Input** | Enhanced Touch Input System | Multi-touch gestures |
| **Audio** | FMOD Studio 2.02 | ASMR SFX + Formby sync |
| **Haptics** | Nice Vibrations | 0.0-1.0 intensity scaling |

### Procedural Generation
| Layer | Algorithm | Performance |
|-------|-----------|-------------|
| **1. Suds Base** | Perlin Noise (6 octaves) | 10ms GPU / 25ms CPU |
| **2. Hazards** | Poisson Disk Sampling (O(1) grid) | 0.5ms (10× faster) |
| **3. Regen** | Cellular Automata (sum >5) | 5ms Burst parallel |
| **4. Validation** | AI Greedy Bot (92% threshold) | 20ms pathfinding |

### Backend Services
- **Firebase**: Auth, Analytics, Crashlytics, ML Kit (churn prediction)
- **Unity Gaming Services**: Cloud Save, Economy (anti-cheat)
- **Unity IAP**: Cross-platform purchases (28 SKUs)
- **Ads**: Unity Ads + ironSource + AdMob (mediation)

### Target Devices (2026)
| Tier | Chipset | RAM | Gen Time | FPS |
|------|---------|-----|----------|-----|
| **High** | Snapdragon 8 Gen 3 | 12GB | 15ms | 120 |
| **Mid** | Snapdragon 6 Gen 1 | 6GB | 60ms | 90 |
| **Low** | Snapdragon 4 Gen 2 | 4GB | 80ms | 60 |

---

## 🎮 Gameplay Features

### Gestures (Multi-Touch)
- **Swipe/Drag**: Clear suds (PhysX deform)
- **Circle Scrub**: Remove stubborn hazards (precision 0-1 score)
- **Up-Flick**: Spray water (scatter beads)
- **Double-Tap**: Bucket power-up (+10s, nuke area)
- **Pull**: Stretch sap/goo (elastic resistance)
- **Gyro Tilt**: Zero-G debris navigation

### 25 Hazard Types
#### Static (17)
Bird Poop, Flies, Salts, Water Spots, Oil, Mud, Webs, Soot, Ash, Rain, Acid, Alkali, Blood, Pollution, Combos

#### Regenerating (8 - Cellular Automata)
Frost, Algae, Sap, Pollution, Fog, Nano-Bots, Blood, Pollen *(3%/sec spread when clean% <80%)*

### Progression
- **Squeegee HQ**: Isometric meta hub (grows with stars)
- **20+ Upgrades**: Glove Size, Turbo Speed, Idle Drones ($0.99-$14.99)
- **Battle Pass**: 50 tiers, $4.99/month
- **Daily Challenges**: 7-day streak = Exclusive Formby ukulele skin

---

## 💰 Monetization (Fair F2P)

### Core Model
- **70-95% IAP** (Energy gates, bundles, VIP sub)
- **5-30% Rewarded Ads**

### Energy System
- **5 Lives Max** (regen 1/30min = 48/day free)
- **Refill Options**: Watch ad (+1), $0.99 (5 lives), $2.99 (unlimited 1hr)

### IAP Shop (28 Items)
| Category | Example | Price |
|----------|---------|-------|
| **Currency** | Starter Pack | $0.99 (250 gems + 5 lives) |
| **Subscription** | VIP (∞ lives, 2× rewards) | $4.99/mo |
| **Boosters** | Nuke ×10 | $1.99 |
| **Skips** | Area Unlock | $9.99 |
| **Cosmetics** | Legend Bundle | $24.99 |

### Dynamic Pricing (Firebase ML)
- Churn prediction: "60% Off Gems – We Miss You!"
- Regional: ₹79 India, £3.99 UK (not $4.99 converted)

### Revenue Target
- **Year 1**: $1M+ (1M downloads × $1 ARPU)
- **Whale Contribution**: 1% users × $50 LTV = 50% of IAP revenue

---

## 📅 Roadmap

### ✅ Phase 1: Prototype (Week 1) - **CURRENT**
- [x] 1 World, 10 Levels, 4-7 Floors
- [x] Procedural: Perlin + PDS (Burst CPU, 256×256 grid)
- [x] 3 Gestures: Swipe, Scrub, Tap
- [x] 3 Hazards: Poop (static), Flies (static), Frost (regen)
- [x] Timer: 90s, 95% gate, 1-3 stars
- [x] 10 ASMR SFX + ukulele loop

### 🔲 Phase 2: Alpha (Weeks 2-4)
- [ ] Lives + IAP Shop (28 items)
- [ ] All 25 hazards + CA regen
- [ ] AI solvability bot
- [ ] Firebase ML integration
- [ ] 1,000 levels (Worlds 1-3)

### 🔲 Phase 3: Beta (Weeks 5-12)
- [ ] 10 Worlds (10,000 levels)
- [ ] Social: Leaderboards, TikTok export
- [ ] Battle Pass + VIP sub
- [ ] Live-ops events (weekly)

### 🔲 Phase 4: Launch (Q2 2026)
- [ ] ASO optimization
- [ ] Influencer campaigns (£10k)
- [ ] Soft launch (Canada, Australia, Philippines)
- [ ] Global release

---

## ���� Testing & Benchmarking

### Performance Targets
```bash
# Unity Profiler (Editor → Window → Analysis → Profiler)
- CPU Main Thread: <16ms (60 FPS minimum)
- GPU Frame Time: <8ms (120 FPS target)
- Procedural Gen: <80ms (mid-tier devices)
- Memory: <500MB (low-end devices)
```

### Test Devices (Priority)
1. **Samsung Galaxy A54** (Snapdragon 6 Gen 1, 6GB) - 25% market
2. **iPhone 15** (A16 Bionic, 6GB) - 20% market
3. **Xiaomi Redmi Note 12** (Snapdragon 4 Gen 2, 4GB) - 30% market

### Playtest Checklist
- [ ] Gestures feel responsive (<50ms input lag)
- [ ] ASMR sounds trigger on correct events
- [ ] Regen feels fair (not cheap)
- [ ] 10 non-devs: 80% say "enjoyable"
- [ ] No crashes on 10-minute session

---

## 🤝 Contributing

### Development Workflow
1. **Branch**: `git checkout -b feature/your-feature`
2. **Code**: Follow Unity C# style guide
3. **Test**: Profile on 3 device tiers
4. **Commit**: `git commit -m "feat: Add Circle Scrub gesture"`
5. **Push**: `git push origin feature/your-feature`
6. **PR**: Open pull request to `main` branch

### Code Standards
- **Burst-compile** all hot-path functions (`[BurstCompile]`)
- **NativeArray** for grid data (no managed allocations)
- **IJobParallelFor** for parallelizable loops
- **Profiler**: Every feature must hit <16ms frame budget

---

## 📄 Documentation

- **[Game Design Document](Docs/GAME_DESIGN_DOCUMENT.md)**: Full GDD (this file, but expanded)
- **[Technical Spec](Docs/TECHNICAL_SPEC.md)**: Algorithm math, Burst code samples
- **Unity Manual**: [Unity 6.3 LTS Docs](https://docs.unity3d.com/)
- **URP Guide**: [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)

---

## 🐛 Known Issues

- [ ] **Issue #1**: ComputeShader fallback not implemented (crashes on <OpenGL ES 3.1 devices)
- [ ] **Issue #2**: Regen CA sum >5 rule causes stable patterns in corners
- [ ] **Issue #3**: PDS spatial grid edge cases (hazards clip screen borders)

See [Issues](https://github.com/romeocasanova212-jpg/when-im-cleaning-windows/issues) for full tracker.

---

## 📜 License

**Proprietary** - All rights reserved © 2026 @romeocasanova212-jpg

This project is **not open-source**. Code is provided for portfolio review only.

---

## 🙏 Acknowledgments

- **EyeToy: Play** (2003, Sony London Studio) - Original "Wishi Washi" inspiration
- **George Formby** - "When I'm Cleaning Windows" (1936 song)
- **Ken Perlin** - Perlin Noise algorithm (1985)
- **Robert Bridson** - Fast Poisson Disk Sampling (2007)
- **John Conway** - Game of Life / Cellular Automata (1970)

---

## 📞 Contact

- **GitHub**: [@romeocasanova212-jpg](https://github.com/romeocasanova212-jpg)
- **Email**: romeo@brewandbrawl.co.uk
- **Project**: [when-im-cleaning-windows](https://github.com/romeocasanova212-jpg/when-im-cleaning-windows)

---

*Built with ❤️, ASMR, and algorithmic precision* 🪟✨🎶
