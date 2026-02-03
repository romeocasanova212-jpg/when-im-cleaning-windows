using UnityEngine;
using System.Collections.Generic;
using WhenImCleaningWindows.Mechanics;
using WhenImCleaningWindows.Audio;

namespace WhenImCleaningWindows.Visual
{
    /// <summary>
    /// VFX Manager for all particle effects and visual feedback.
    /// Handles cleaning effects, power-ups, UI animations, and hazard-specific particles.
    /// </summary>
    public class VFXManager : MonoBehaviour
    {
        public static VFXManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool enableParticles = true;
        [SerializeField] private int maxParticlesPerEffect = 100;
        [SerializeField] private ParticleSystemQualitySettings qualitySettings;

        [Header("Cleaning VFX Prefabs")]
        [SerializeField] private ParticleSystem swipeSudsEffect;
        [SerializeField] private ParticleSystem circlePolishEffect;
        [SerializeField] private ParticleSystem squeegeeStreakEffect;
        [SerializeField] private ParticleSystem sprayBottleEffect;
        [SerializeField] private ParticleSystem perfectClearEffect;

        [Header("Hazard-Specific VFX")]
        [SerializeField] private ParticleSystem frostCrackleEffect;
        [SerializeField] private ParticleSystem nanoBotSparkEffect;
        [SerializeField] private ParticleSystem fogDissipateEffect;
        [SerializeField] private ParticleSystem pollenBurstEffect;

        [Header("Power-Up VFX")]
        [SerializeField] private ParticleSystem nukeExplosionEffect;
        [SerializeField] private ParticleSystem turboTrailEffect;
        [SerializeField] private ParticleSystem autoPilotGlowEffect;
        [SerializeField] private ParticleSystem timeFreezeEffect;

        [Header("UI VFX")]
        [SerializeField] private ParticleSystem starBurstEffect;
        [SerializeField] private ParticleSystem coinCollectEffect;
        [SerializeField] private ParticleSystem gemCollectEffect;
        [SerializeField] private ParticleSystem levelUpEffect;
        [SerializeField] private ParticleSystem energyRefillEffect;

        [Header("Ambient VFX")]
        [SerializeField] private ParticleSystem rainEffect;
        [SerializeField] private ParticleSystem snowEffect;
        [SerializeField] private ParticleSystem dustMotesEffect;
        [SerializeField] private ParticleSystem cyberpunkNeonEffect;

        private Dictionary<string, ParticleSystem> vfxCache;
        private Queue<ParticleSystem> particlePool;

        public enum ParticleSystemQualitySettings
        {
            Low,        // 30 particles max
            Medium,     // 100 particles max
            High,       // 300 particles max
            Ultra       // 1000 particles max
        }

        #region Initialization

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeVFX();
        }

        private void InitializeVFX()
        {
            Debug.Log("[VFXManager] Initializing VFX system...");

            vfxCache = new Dictionary<string, ParticleSystem>();
            particlePool = new Queue<ParticleSystem>();

            // Apply quality settings
            ApplyQualitySettings();

            // Initialize procedural fallbacks if prefabs are missing
            if (swipeSudsEffect == null) swipeSudsEffect = CreateProceduralSwipeEffect();
            if (starBurstEffect == null) starBurstEffect = CreateProceduralStarBurst();
            if (coinCollectEffect == null) coinCollectEffect = CreateProceduralCoinEffect();

            Debug.Log("[VFXManager] ✓ VFX system initialized");
        }

        private void ApplyQualitySettings()
        {
            switch (qualitySettings)
            {
                case ParticleSystemQualitySettings.Low:
                    maxParticlesPerEffect = 30;
                    break;
                case ParticleSystemQualitySettings.Medium:
                    maxParticlesPerEffect = 100;
                    break;
                case ParticleSystemQualitySettings.High:
                    maxParticlesPerEffect = 300;
                    break;
                case ParticleSystemQualitySettings.Ultra:
                    maxParticlesPerEffect = 1000;
                    break;
            }

            Debug.Log($"[VFXManager] Quality set to {qualitySettings} (max {maxParticlesPerEffect} particles)");
        }

        #endregion

        #region Cleaning VFX

        public void PlaySwipeSudsEffect(Vector3 position, Vector3 direction)
        {
            if (!enableParticles || swipeSudsEffect == null) return;

            ParticleSystem effect = Instantiate(swipeSudsEffect, position, Quaternion.LookRotation(direction));
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);

            AudioManager.Instance?.PlaySwipeSFX();
        }

        public void PlayCirclePolishEffect(Vector3 position)
        {
            if (!enableParticles || circlePolishEffect == null) return;

            ParticleSystem effect = Instantiate(circlePolishEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);

            AudioManager.Instance?.PlayCircleScrubSFX();
        }

        public void PlaySqueegeStreakEffect(Vector3 startPos, Vector3 endPos)
        {
            if (!enableParticles || squeegeeStreakEffect == null) return;

            Vector3 direction = (endPos - startPos).normalized;
            float distance = Vector3.Distance(startPos, endPos);

            ParticleSystem effect = Instantiate(squeegeeStreakEffect, startPos, Quaternion.LookRotation(direction));
            
            var main = effect.main;
            main.startLifetime = distance / 2f; // Scale lifetime to distance

            effect.Play();
            Destroy(effect.gameObject, main.duration);

            AudioManager.Instance?.PlaySqueegeeSFX();
        }

        public void PlaySprayBottleEffect(Vector3 position)
        {
            if (!enableParticles || sprayBottleEffect == null) return;

            ParticleSystem effect = Instantiate(sprayBottleEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        public void PlayPerfectClearEffect(Vector3 position)
        {
            if (!enableParticles || perfectClearEffect == null) return;

            ParticleSystem effect = Instantiate(perfectClearEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration + 0.5f);
        }

        #endregion

        #region Hazard-Specific VFX

        public void PlayFrostCrackleEffect(Vector3 position)
        {
            if (!enableParticles || frostCrackleEffect == null) return;

            ParticleSystem effect = Instantiate(frostCrackleEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }

        public void PlayNanoBotSparkEffect(Vector3 position)
        {
            if (!enableParticles || nanoBotSparkEffect == null) return;

            ParticleSystem effect = Instantiate(nanoBotSparkEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 1.5f);
        }

        public void PlayFogDissipateEffect(Vector3 position)
        {
            if (!enableParticles || fogDissipateEffect == null) return;

            ParticleSystem effect = Instantiate(fogDissipateEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 3f);
        }

        public void PlayPollenBurstEffect(Vector3 position)
        {
            if (!enableParticles || pollenBurstEffect == null) return;

            ParticleSystem effect = Instantiate(pollenBurstEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }

        #endregion

        #region Power-Up VFX

        public void PlayNukeExplosionEffect(Vector3 position)
        {
            if (!enableParticles || nukeExplosionEffect == null) return;

            ParticleSystem effect = Instantiate(nukeExplosionEffect, position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 3f);

            AudioManager.Instance?.PlaySFX(AudioEventType.PowerUp_Nuke);
        }

        public void PlayTurboTrailEffect(Transform followTransform)
        {
            if (!enableParticles || turboTrailEffect == null) return;

            ParticleSystem effect = Instantiate(turboTrailEffect, followTransform.position, Quaternion.identity);
            effect.transform.SetParent(followTransform);
            effect.Play();

            AudioManager.Instance?.PlaySFX(AudioEventType.PowerUp_Turbo);

            // Destroy after turbo duration (e.g., 10 seconds)
            Destroy(effect.gameObject, 10f);
        }

        public void PlayAutoPilotGlowEffect(Transform targetTransform)
        {
            if (!enableParticles || autoPilotGlowEffect == null) return;

            ParticleSystem effect = Instantiate(autoPilotGlowEffect, targetTransform.position, Quaternion.identity);
            effect.transform.SetParent(targetTransform);
            effect.Play();

            AudioManager.Instance?.PlaySFX(AudioEventType.PowerUp_AutoPilot);

            // Destroy after auto-pilot duration
            Destroy(effect.gameObject, 15f);
        }

        public void PlayTimeFreezeEffect()
        {
            if (!enableParticles || timeFreezeEffect == null) return;

            // Screen-space effect
            ParticleSystem effect = Instantiate(timeFreezeEffect, Camera.main.transform.position, Quaternion.identity);
            effect.transform.SetParent(Camera.main.transform);
            effect.Play();

            AudioManager.Instance?.PlaySFX(AudioEventType.PowerUp_TimeFreeze);

            Destroy(effect.gameObject, 5f);
        }

        #endregion

        #region UI VFX

        public void PlayStarBurstEffect(Vector3 screenPosition, int starIndex)
        {
            if (!enableParticles || starBurstEffect == null) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            ParticleSystem effect = Instantiate(starBurstEffect, worldPos, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 1.5f);

            AudioManager.Instance?.PlayStarPop();
        }

        public void PlayCoinCollectEffect(Vector3 screenPosition)
        {
            if (!enableParticles || coinCollectEffect == null) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            ParticleSystem effect = Instantiate(coinCollectEffect, worldPos, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 1f);

            AudioManager.Instance?.PlayCoinCollect();
        }

        public void PlayGemCollectEffect(Vector3 screenPosition)
        {
            if (!enableParticles || gemCollectEffect == null) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            ParticleSystem effect = Instantiate(gemCollectEffect, worldPos, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 1f);

            AudioManager.Instance?.PlayGemCollect();
        }

        public void PlayLevelUpEffect(Vector3 screenPosition)
        {
            if (!enableParticles || levelUpEffect == null) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            ParticleSystem effect = Instantiate(levelUpEffect, worldPos, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 2f);
        }

        public void PlayEnergyRefillEffect(Vector3 screenPosition)
        {
            if (!enableParticles || energyRefillEffect == null) return;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            ParticleSystem effect = Instantiate(energyRefillEffect, worldPos, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, 1.5f);
        }

        #endregion

        #region Ambient VFX

        public void SetAmbientRain(bool active)
        {
            if (rainEffect != null)
            {
                if (active) rainEffect.Play();
                else rainEffect.Stop();
            }
        }

        public void SetAmbientSnow(bool active)
        {
            if (snowEffect != null)
            {
                if (active) snowEffect.Play();
                else snowEffect.Stop();
            }
        }

        public void SetDustMotes(bool active)
        {
            if (dustMotesEffect != null)
            {
                if (active) dustMotesEffect.Play();
                else dustMotesEffect.Stop();
            }
        }

        public void SetCyberpunkNeon(bool active)
        {
            if (cyberpunkNeonEffect != null)
            {
                if (active) cyberpunkNeonEffect.Play();
                else cyberpunkNeonEffect.Stop();
            }
        }

        #endregion

        #region Procedural VFX Generation

        private ParticleSystem CreateProceduralSwipeEffect()
        {
            GameObject obj = new GameObject("Procedural_SwipeSuds");
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.startSize = 0.2f;
            main.startColor = new Color(1f, 1f, 1f, 0.5f);
            main.maxParticles = 50;

            var emission = ps.emission;
            emission.rateOverTime = 100f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;

            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.cyan, 1.0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            colorOverLifetime.color = gradient;

            Debug.Log("[VFXManager] Created procedural swipe effect");
            return ps;
        }

        private ParticleSystem CreateProceduralStarBurst()
        {
            GameObject obj = new GameObject("Procedural_StarBurst");
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 1f;
            main.startSpeed = 3f;
            main.startSize = 0.3f;
            main.startColor = Color.yellow;
            main.maxParticles = 30;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 30) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            Debug.Log("[VFXManager] Created procedural star burst effect");
            return ps;
        }

        private ParticleSystem CreateProceduralCoinEffect()
        {
            GameObject obj = new GameObject("Procedural_CoinCollect");
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.startLifetime = 0.8f;
            main.startSpeed = 2f;
            main.startSize = 0.15f;
            main.startColor = new Color(1f, 0.84f, 0f); // Gold
            main.maxParticles = 20;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 45f;

            Debug.Log("[VFXManager] Created procedural coin collect effect");
            return ps;
        }

        #endregion

        #region Helpers

        public void SetParticlesEnabled(bool enabled)
        {
            enableParticles = enabled;
            Debug.Log($"[VFXManager] Particles {(enabled ? "enabled" : "disabled")}");
        }

        public void SetQuality(ParticleSystemQualitySettings quality)
        {
            qualitySettings = quality;
            ApplyQualitySettings();
        }

        #endregion

        #region Debug Context Menu

        [ContextMenu("Test All VFX")]
        private void TestAllVFX()
        {
            Debug.Log("=== Testing VFX System ===");

            Vector3 testPos = Vector3.zero;

            PlaySwipeSudsEffect(testPos, Vector3.forward);
            PlayCirclePolishEffect(testPos);
            PlayPerfectClearEffect(testPos);
            PlayStarBurstEffect(new Vector3(Screen.width / 2, Screen.height / 2, 0), 1);
            PlayCoinCollectEffect(new Vector3(Screen.width / 2, Screen.height / 2, 0));

            Debug.Log("=== VFX Test Complete ===");
        }

        [ContextMenu("Print VFX Requirements")]
        public void PrintVFXRequirements()
        {
            Debug.Log("=== VFX PARTICLE SYSTEM REQUIREMENTS ===");
            Debug.Log("\n--- CLEANING VFX ---");
            Debug.Log("1. SwipeSuds - White/cyan particles following swipe direction");
            Debug.Log("2. CirclePolish - Circular particle burst with shine");
            Debug.Log("3. SqueegeStreak - Water droplet trail");
            Debug.Log("4. SprayBottle - Mist spray cone");
            Debug.Log("5. PerfectClear - Rainbow sparkle burst");

            Debug.Log("\n--- HAZARD-SPECIFIC VFX ---");
            Debug.Log("6. FrostCrackle - Ice crystal shatter");
            Debug.Log("7. NanoBotSpark - Blue electric sparks");
            Debug.Log("8. FogDissipate - Wispy cloud dispersal");
            Debug.Log("9. PollenBurst - Yellow particle cloud");

            Debug.Log("\n--- POWER-UP VFX ---");
            Debug.Log("10. NukeExplosion - Screen-wide shockwave");
            Debug.Log("11. TurboTrail - Speed motion blur");
            Debug.Log("12. AutoPilotGlow - Green holographic aura");
            Debug.Log("13. TimeFreeze - Blue time-stop ripple");

            Debug.Log("\n--- UI VFX ---");
            Debug.Log("14. StarBurst - Yellow star particle explosion");
            Debug.Log("15. CoinCollect - Gold coins flying to UI");
            Debug.Log("16. GemCollect - Purple gems flying to UI");
            Debug.Log("17. LevelUp - Confetti celebration");
            Debug.Log("18. EnergyRefill - Green health restore");

            Debug.Log("\n--- AMBIENT VFX ---");
            Debug.Log("19. Rain - Persistent rain drops");
            Debug.Log("20. Snow - Falling snowflakes");
            Debug.Log("21. DustMotes - Floating dust particles");
            Debug.Log("22. CyberpunkNeon - Glowing neon particles");

            Debug.Log("\n=== Total: 22 Particle Systems ===");
            Debug.Log("===================================");
        }

        #endregion
    }
}








