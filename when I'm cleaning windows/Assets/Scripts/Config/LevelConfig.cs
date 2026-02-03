using UnityEngine;

namespace WhenImCleaningWindows.Config
{
    /// <summary>
    /// Level Configuration - ScriptableObject for level generation settings.
    /// Create via: Assets → Create → When I'm Cleaning Windows → Level Config
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "When I'm Cleaning Windows/Level Config", order = 2)]
    public class LevelConfig : ScriptableObject
    {
        [Header("World Progression")]
        [Tooltip("Number of floors per world")]
        public int floorsPerWorld = 100;
        
        [Tooltip("Number of rooms per floor")]
        public int roomsPerFloor = 10;
        
        [Tooltip("Total worlds available")]
        public int totalWorlds = 10;
        
        [Header("Difficulty Scaling")]
        [Tooltip("Starting timer (seconds) for World 1")]
        public float startingTimer = 120f;
        
        [Tooltip("Ending timer (seconds) for World 10")]
        public float endingTimer = 40f;
        
        [Tooltip("Starting hazard count for World 1")]
        public int startingHazardCount = 8;
        
        [Tooltip("Ending hazard count for World 10")]
        public int endingHazardCount = 25;
        
        [Header("Hazard Generation")]
        [Tooltip("Min hazard size multiplier")]
        [Range(0.1f, 2f)]
        public float minHazardSize = 0.5f;
        
        [Tooltip("Max hazard size multiplier")]
        [Range(0.5f, 3f)]
        public float maxHazardSize = 1.5f;
        
        [Tooltip("Min hazard complexity (0.0-1.0)")]
        [Range(0f, 1f)]
        public float minComplexity = 0.3f;
        
        [Tooltip("Max hazard complexity (0.0-1.0)")]
        [Range(0f, 1f)]
        public float maxComplexity = 0.8f;
        
        [Header("Regeneration")]
        [Tooltip("Enable hazard regeneration")]
        public bool enableRegeneration = true;
        
        [Tooltip("Regeneration rate (%/sec)")]
        [Range(0f, 10f)]
        public float regenerationRate = 3f;
        
        [Tooltip("Clean threshold to stop regeneration (%)")]
        [Range(50f, 100f)]
        public float regenStopThreshold = 80f;
        
        [Tooltip("Cellular automata neighbor threshold")]
        [Range(3, 8)]
        public int neighborThreshold = 4;
        
        [Header("Window Mesh")]
        [Tooltip("Window mesh subdivisions (width)")]
        [Range(8, 64)]
        public int meshSubdivisionsX = 32;
        
        [Tooltip("Window mesh subdivisions (height)")]
        [Range(8, 64)]
        public int meshSubdivisionsY = 32;
        
        [Tooltip("Window width (world units)")]
        [Range(5f, 20f)]
        public float windowWidth = 10f;
        
        [Tooltip("Window height (world units)")]
        [Range(5f, 20f)]
        public float windowHeight = 8f;
        
        [Header("AI Validation")]
        [Tooltip("Enable AI solvability check")]
        public bool enableAIValidation = true;
        
        [Tooltip("Minimum solvability percentage")]
        [Range(80f, 100f)]
        public float minSolvabilityPercent = 95f;
        
        [Tooltip("Max validation attempts")]
        [Range(1, 10)]
        public int maxValidationAttempts = 3;
        
        [Header("Procedural Generation")]
        [Tooltip("Perlin noise octaves")]
        [Range(1, 10)]
        public int perlinOctaves = 7;
        
        [Tooltip("Perlin noise frequency")]
        [Range(0.01f, 1f)]
        public float perlinFrequency = 0.1f;
        
        [Tooltip("Poisson disk sampling attempts")]
        [Range(10, 50)]
        public int poissonAttempts = 20;
        
        [Tooltip("Minimum distance between hazards")]
        [Range(0.5f, 3f)]
        public float minHazardDistance = 1f;
        
        [Header("Star Thresholds")]
        [Tooltip("1 star: Complete level")]
        public bool oneStar_Complete = true;
        
        [Tooltip("2 stars: X% time remaining")]
        [Range(0f, 100f)]
        public float twoStar_TimePercent = 33f;
        
        [Tooltip("3 stars: X% time remaining")]
        [Range(0f, 100f)]
        public float threeStar_TimePercent = 66f;
        
        [Header("World Themes")]
        [Tooltip("World theme names (10 entries)")]
        public string[] worldThemeNames = new string[]
        {
            "Suburban Homes",
            "Downtown Towers",
            "Historic District",
            "Industrial Zone",
            "Coastal Resort",
            "Urban Decay",
            "Mountain Chalet",
            "Cyberpunk City",
            "Space Station",
            "Formby Mansion"
        };
    }
}








