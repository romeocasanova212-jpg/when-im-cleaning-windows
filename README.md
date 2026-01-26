# When I'm Cleaning Windows 🪟✨

> **Hyper-casual ASMR window cleaning game inspired by EyeToy: Play's "Wishi Washi"**  
> Unity 6.3 LTS URP | Procedural Generation | 2026 Mobile-Optimized | Android-First

---

## 🎮 Overview

**When I'm Cleaning Windows** is a satisfying, hyper-casual mobile game that transforms window cleaning into an addictive ASMR experience. Rise from rookie washer to "Glass God" by swiping away suds and hazards across procedurally-generated multi-floor buildings. Clean 95% before the timer runs out, unlock new worlds, and share your most satisfying shine moments on TikTok.

### Core Experience
- **1-3 minute sessions** optimized for mobile attention spans
- **120FPS smoothness** on OLED displays with haptic feedback
- **250+ ASMR sound effects** synchronized to your gestures
- **10,000+ unique levels** via procedural generation
- **Fair free-to-play** progression (months free, hours with IAP)

---

## 🎯 Game Features

### Mechanics
- **Multi-touch gestures**: Swipe, circle scrub, spray, pull, gyro tilt
- **25+ hazard types**: Bird poop, frost, sap, pollution, nano-bots, and more
- **Regenerating challenges**: Some hazards spread dynamically (Cellular Automata)
- **Physics-based suds**: Real-time deformation with PhysX

### Content
- **10 Worlds** × 10 Areas × 100 Levels = 10,000 levels
- **Procedural generation**: Perlin Noise + Poisson Disk Sampling + Cellular Automata
- **AI solvability validation**: Every level is beatable
- **Weekly events**: Themed challenges with limited-time rewards

### Progression
- **Squeegee HQ**: Build your diorama with earned stars
- **20+ upgrades**: Glove size, turbo speed, idle drones
- **Battle Pass**: Premium rewards for dedicated players
- **VIP Subscription**: $4.99/mo for unlimited lives and 2x rewards

---

## 🛠️ Tech Stack (2026-Ready)

### Engine & Graphics
- **Unity 6.3 LTS** with Universal Render Pipeline (URP)
- **120FPS target** with adaptive performance scaling
- **Ray-traced glass** (PBR materials with refractions)
- **Burst-compiled Jobs** for procedural generation
- **ComputeShader GPU acceleration** on high-end devices

### Performance Optimization
- **Hybrid CPU/GPU generation**: Auto-detects device capabilities
- **Adaptive grid resolution**: 256×256 (phones) to 512×512 (tablets)
- **RenderTexture pooling**: 95% memory reduction
- **Target: <80ms generation time** on Snapdragon 6 Gen 1

### Monetization & Analytics
- **Unity IAP**: 28 in-app purchase items
- **Firebase ML**: On-device churn prediction (NPU-accelerated)
- **Unity Analytics**: A/B testing for pricing and offers
- **Energy gates**: 5 lives, 1/30min regeneration

### Audio
- **FMOD integration**: Real-time audio sync to gameplay
- **Royalty-free ukulele**: George Formby-inspired soundtrack
- **Nice Vibrations**: Haptic feedback scaled to gesture intensity

---

## 📁 Repository Structure

```
when-im-cleaning-windows/
├── Assets/
│   ├── Scripts/
│   │   ├── Procedural/          # Perlin, PDS, CA, AI Bot
│   │   ├── Mechanics/           # Gestures, Suds, Timer
│   │   ├── Monetization/        # Lives, IAP, Firebase ML
│   │   └── UI/                  # HUD, VFX, Menus
│   ├── Prefabs/
│   ├── Audio/
│   │   ├── ASMR_SFX/           # 250+ sound effects
│   │   └── Music/
│   ├── Materials/               # PBR Glass shaders
│   └── Addressables/            # On-demand world downloads
├── Docs/
│   ├── GAME_DESIGN_DOCUMENT.md  # Full GDD with 2026 updates
│   └── TECHNICAL_SPEC.md        # Algorithm details & benchmarks
└── README.md
```

---

## 🚀 Development Roadmap

### Phase 1: Prototype (Week 1) ✅ **CURRENT**
- [x] Procedural generation pipeline (Perlin, PDS, basic CA)
- [x] Core gestures (swipe, circle scrub, double-tap)
- [x] Timer system & 95% win condition
- [x] 3 hazard types (Poop, Flies, Frost)
- [x] Basic UI & shine VFX
- [x] 10 ASMR sound effects
- [x] Performance: 80ms gen, 120FPS target

### Phase 2: Alpha (Weeks 2-4)
- [ ] Lives system + energy gates
- [ ] IAP shop (28 items)
- [ ] All 25 hazards with regeneration
- [ ] AI solvability bot (92% threshold)
- [ ] Firebase ML integration
- [ ] 1,000 levels (Worlds 1-3)

### Phase 3: Beta (Weeks 5-12)
- [ ] All 10 worlds (10,000 levels)
- [ ] Social features (leaderboards, sharing)
- [ ] Battle Pass & VIP subscription
- [ ] Live-ops event system
- [ ] TikTok export integration
- [ ] Closed beta testing

### Phase 4: Launch (Q2 2026)
- [ ] App Store Optimization (ASO)
- [ ] Influencer partnerships (£10k budget)
- [ ] Weekly content updates
- [ ] Regional pricing & localization

---

## 📊 Target Metrics (2026 Standards)

| Metric | Target | Industry Benchmark |
|--------|--------|-------------------|
| **D1 Retention** | 40% | 35% (hyper-casual) |
| **D7 Retention** | 25% | 18% (hyper-casual) |
| **D30 Retention** | 12% | 8% (hyper-casual) |
| **ARPU (Free)** | $1-5 | $2 (average) |
| **ARPU (Whale)** | $50+ | $75 (top 1%) |
| **Conversion Rate** | 3-5% | 2.5% (hyper-casual) |
| **Session Length** | 3-5 min | 4 min (average) |
| **Sessions/Day** | 3-5 | 4 (average) |

---

## 🎯 Core Game Loop

```
1. Spend Life (5 max, regen 1/30min)
      ↓
2. Timer Starts (90s → 35s scaling)
      ↓
3. Gesture to Clean (swipe suds, scrub hazards)
      ↓
4. Hit 95% Clean → Shine VFX + Next Floor
      ↓
5. Complete All Floors → Stars (1-3) + Rewards
      ↓
6. Upgrade Squeegee HQ / Buy Lives / Share Shine
```

---

## 🧠 Procedural Algorithm (High-Level)

### 4-Layer Pipeline
1. **Perlin Noise**: Generates suds base density (6 octaves, smooth gradients)
2. **Poisson Disk Sampling**: Places hazard blobs (O(1) spatial grid, 8px min distance)
3. **Cellular Automata**: Handles regeneration (sum >5 rule, 3%/sec spread rate)
4. **AI Solvability Bot**: Validates 100 swipes achieve 92% coverage

### Performance
- **GPU (High-end)**: 15ms via ComputeShader
- **CPU (Mid-tier)**: 60ms via Burst + Jobs
- **CPU (Low-end)**: 80ms single-threaded Burst

*See [TECHNICAL_SPEC.md](Docs/TECHNICAL_SPEC.md) for full math & code*

---

## 🎨 Art & Audio Direction

### Visual Style
- **Ray-traced PBR glass** with realistic refractions
- **Cartoon hazards** (exaggerated bird poop, comical flies)
- **Particle explosions** for shine moments (120FPS optimized)
- **Behind-glass scenes** reveal on completion (AR-ready)

### Audio Philosophy
- **ASMR-first**: Every swipe triggers satisfying squeegee sounds
- **Dynamic music**: Ukulele tempo syncs to cleaning speed
- **Haptic symphony**: Rumble intensity matches gesture score
- **Narrator taunts**: George Formby-inspired voiceover (optional)

---

## 💰 Monetization Model

### Free-to-Play Core
- **Energy gates**: 5 lives, 30-minute regeneration
- **Progression**: Months to max free, hours with IAP
- **No pay-to-win**: All levels beatable free

### IAP Structure (28 Items)
- **Lives**: Quick Refill $0.99, Unlimited Hour $2.99
- **Gems**: Starter $0.99, Daily Deal $1.99, Pro $9.99, Mega $99.99
- **VIP Sub**: $4.99/mo (unlimited lives, 2x rewards, ad-free)
- **Boosters**: Nuke, Turbo, Extra Time
- **Skips**: Floor, Area, World
- **Cosmetics**: Glove skins, theme packs

### Revenue Model
- **70-95% IAP** (whales + dolphins)
- **5-30% Rewarded Ads** (free users)
- **Target**: $1M+ Year 1 at 1M downloads

---

## 🔐 2026 Compliance

### Privacy (ATT/GDPR)
- **On-device ML**: Firebase churn prediction (no tracking)
- **Contextual offers**: Based on gameplay, not user data
- **Transparent permissions**: Push notifications on Day 3+

### Accessibility
- **One-handed mode**: Tap to auto-clean
- **Voice control**: "Clean top-left" commands
- **Colorblind modes**: Blue/yellow suds variants
- **Haptic-only feedback**: Vibration intensity = clean %

### Regional Compliance
- **EU spend caps**: $50/mo for unverified users
- **Regional pricing**: ₹79 India, £3.99 UK (not $4.99)
- **Age verification**: Required for unlimited spending

---

## 🤝 Contributing

This is a private development repository. For team members:

1. **Branching**: `feature/your-feature-name`
2. **Commits**: Use conventional commits (`feat:`, `fix:`, `docs:`)
3. **PRs**: Require 1 approval + CI pass
4. **Testing**: Profile on Snapdragon 6 Gen 1 minimum

---

## 📄 Documentation

- [**Game Design Document**](Docs/GAME_DESIGN_DOCUMENT.md) - Full GDD with 2026 updates
- [**Technical Specification**](Docs/TECHNICAL_SPEC.md) - Algorithm math & Burst code
- [**API Reference**](Docs/API.md) - Code documentation (coming soon)

---

## 📞 Contact

**Project Lead**: @romeocasanova212-jpg  
**Target Launch**: Q2 2026  
**Platform**: iOS/Android (Android-first)

---

## 📜 License

Proprietary - All Rights Reserved  
© 2026 When I'm Cleaning Windows

---

**Built with ❤️ and ASMR in mind** 🪟✨🎶