using UnityEngine;

namespace WhenImCleaningWindows.Mechanics
{
    /// <summary>
    /// Represents a hazard instance with its properties.
    /// </summary>
    [System.Serializable]
    public struct Hazard
    {
        public HazardType type;
        public Vector2 position;
        public float size;
        public float cleanDifficulty;
        public float regenRate;
    }
    
    /// <summary>
    /// Hazard type enumeration (24 total types).
    /// </summary>
    public enum HazardType
    {
        // === STATIC HAZARDS (16 types) ===
        BirdPoop = 0,
        DeadFlies = 1,
        Mud = 2,
        OilStain = 3,
        Spiderweb = 4,
        Graffiti = 5,
        Stickers = 6,
        TapeResidue = 7,
        WaterMarks = 8,
        Rust = 9,
        Gum = 10,
        Paint = 11,
        Ash = 12,
        Mold = 13,
        Scratches = 14,
        Dust = 15,
        
        // === REGENERATING HAZARDS (8 types) ===
        Frost = 16,         // Spreads from edges
        Algae = 17,         // Organic growth
        TreeSap = 18,       // Sticky spread
        Fog = 19,           // Condenses over time
        NanoBots = 20,      // Sci-fi self-replicating
        Pollen = 21,        // Seasonal drift
        Condensation = 22,  // Humidity-based
        Pollution = 23      // Smog accumulation
    }
    
    /// <summary>
    /// Hazard properties and behavior configuration.
    /// </summary>
    [System.Serializable]
    public class HazardProperties
    {
        [Header("Visual")]
        public HazardType type;
        public string displayName;
        public Color primaryColor;
        public Sprite icon;
        public Material material;
        
        [Header("Gameplay")]
        public bool isRegenerating;
        public float cleanDifficulty = 1.0f;  // Multiplier for swipes needed (1.0 = normal, 2.0 = double)
        public int swipesRequired = 3;         // Base swipes to clean
        public float regenRate = 0.025f;       // For regenerating types (2.5%/sec)
        public float spreadRadius = 2.0f;      // For cellular automata
        
        [Header("World Appearance")]
        public int firstAppearanceWorld = 1;   // Which world introduces this hazard
        public float spawnWeight = 1.0f;       // Probability weight
        
        [Header("Audio")]
        public string cleanSFX;                // FMOD event path
        public string ambientSFX;              // Looping sound (flies buzzing, frost crackling)
        
        [Header("Special Mechanics")]
        public bool requiresSpecialTool = false;      // Needs power-up?
        public bool blocksGestures = false;           // Blocks swipes (rust, scratches)
        public bool hasParticleEffect = false;        // Visual polish
    }
}








