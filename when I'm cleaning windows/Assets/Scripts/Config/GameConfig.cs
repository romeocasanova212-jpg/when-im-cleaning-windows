using UnityEngine;

namespace WhenImCleaningWindows.Config
{
    /// <summary>
    /// Global Game Configuration - ScriptableObject for easy tweaking without code changes.
    /// Create via: Assets → Create → When I'm Cleaning Windows → Game Config
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "When I'm Cleaning Windows/Game Config", order = 1)]
    public class GameConfig : ScriptableObject
    {
        [Header("Performance")]
        [Tooltip("Target frame rate (60, 90, 120)")]
        public int targetFrameRate = 120;
        
        [Tooltip("Enable Burst compilation for procedural generation")]
        public bool useBurstCompilation = true;
        
        [Tooltip("Maximum time for procedural generation (ms)")]
        public float maxGenerationTime = 50f;
        
        [Header("Gameplay")]
        [Tooltip("Percentage of window that must be clean to complete level")]
        [Range(80f, 100f)]
        public float levelCompleteThreshold = 95f;
        
        [Tooltip("Base cleaning radius (world units)")]
        [Range(0.1f, 2f)]
        public float baseCleaningRadius = 0.5f;
        
        [Tooltip("Base cleaning power (0.0-1.0)")]
        [Range(0.01f, 1f)]
        public float baseCleaningPower = 0.3f;
        
        [Tooltip("Turbo multiplier for power-up")]
        [Range(1.5f, 5f)]
        public float turboMultiplier = 2.5f;
        
        [Header("Energy System")]
        [Tooltip("Maximum free energy (lives)")]
        public int maxFreeEnergy = 5;

        [Tooltip("Maximum VIP energy (overflow cap)")]
        public int maxVipEnergy = 10;
        
        [Tooltip("Energy regen time (seconds)")]
        public float energyRegenTime = 1200f; // 20 minutes
        
        [Tooltip("Lives per day for free players")]
        public int freeLivesPerDay = 72;
        
        [Header("Currency")]
        [Tooltip("Base coins per level (before star multiplier)")]
        public int baseCoinsPerLevel = 5;
        
        [Tooltip("Coins per star")]
        public int coinsPerStar = 5;
        
        [Tooltip("Gem drop chance on 3-star completion")]
        [Range(0f, 1f)]
        public float gemDropChance = 0.05f;
        
        [Tooltip("Min gems on drop")]
        public int minGemDrop = 2;
        
        [Tooltip("Max gems on drop")]
        public int maxGemDrop = 10;
        
        [Header("VIP System")]
        [Tooltip("Bronze tier coin multiplier")]
        public float bronzeCoinMultiplier = 2.5f;
        
        [Tooltip("Silver tier coin multiplier")]
        public float silverCoinMultiplier = 3f;
        
        [Tooltip("Gold tier coin multiplier")]
        public float goldCoinMultiplier = 4f;
        
        [Header("Audio")]
        [Tooltip("Master volume (0.0-1.0)")]
        [Range(0f, 1f)]
        public float masterVolume = 0.8f;
        
        [Tooltip("SFX volume (0.0-1.0)")]
        [Range(0f, 1f)]
        public float sfxVolume = 1f;
        
        [Tooltip("Music volume (0.0-1.0)")]
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;
        
        [Tooltip("Enable haptic feedback")]
        public bool enableHaptics = true;
        
        [Header("Visual")]
        [Tooltip("Enable VFX particles")]
        public bool enableVFX = true;
        
        [Tooltip("Particle quality (Low, Medium, High, Ultra)")]
        public ParticleQuality particleQuality = ParticleQuality.High;
        
        [Tooltip("Enable screen shake effects")]
        public bool enableScreenShake = true;
        
        [Header("Debug")]
        [Tooltip("Enable debug console (backtick key)")]
        public bool enableDebugConsole = true;
        
        [Tooltip("Show FPS counter")]
        public bool showFPS = false;
        
        [Tooltip("Enable development cheats")]
        public bool enableCheats = true;
    }
    
    public enum ParticleQuality
    {
        Low = 30,       // 30 particles max
        Medium = 100,   // 100 particles
        High = 300,     // 300 particles
        Ultra = 1000    // 1000 particles
    }
}








