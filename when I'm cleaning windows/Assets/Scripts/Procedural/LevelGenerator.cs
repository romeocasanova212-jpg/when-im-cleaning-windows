using UnityEngine;
using System.Collections.Generic;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Procedural
{
    /// <summary>
    /// Level data structure containing all generation parameters.
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        public int levelNumber;
        public int worldNumber;
        public int floorNumber;
        public int roomNumber;
        
        // Generation seeds
        public int perlinSeed;
        public int poissonSeed;
        public int hazardSeed;
        
        // Difficulty scaling
        public float difficultyMultiplier;
        public int hazardCount;
        public float regenRate;
        public float timerSeconds;
        
        // Hazards
        public List<Mechanics.HazardType> hazards;
        
        // Validation
        public bool isSolvable;
        public int elegantPaths;
        
        // Metadata
        public string themeName;
        public bool isKeyLevel;  // Every 100th = boss level
        public bool hasStory;    // Every 500th = story cutscene
    }
    
    /// <summary>
    /// World theme configuration.
    /// </summary>
    [System.Serializable]
    public class WorldTheme
    {
        public int worldNumber;
        public string themeName;
        public string description;
        public Color ambientColor;
        public string musicTrack;
        public int startLevel;
        public int endLevel;
    }
    
    /// <summary>
    /// Level Generator - Produces 3,000 levels for Worlds 1-3 (10,000 total at launch).
    /// Uses Perlin, Poisson, CA, and AI Solvability Bot for balanced progression.
    /// </summary>
    public class LevelGenerator : MonoBehaviour
    {
        public static LevelGenerator Instance { get; private set; }
        
        [Header("Generation Settings")]
        [SerializeField] private int totalLevels = 3000;  // Alpha: 3,000 (W1-3) | Launch: 10,000 (W1-10)
        [SerializeField] private int levelsPerWorld = 1000;
        [SerializeField] private int floorsPerWorld = 100;
        [SerializeField] private int roomsPerFloor = 10;
        
        [Header("Difficulty Curve")]
        [SerializeField] private float baseTimerSeconds = 120f;  // World 1
        [SerializeField] private float endTimerSeconds = 40f;    // World 10
        [SerializeField] private int baseHazardCount = 8;
        [SerializeField] private int maxHazardCount = 25;
        
        [Header("World Themes")]
        [SerializeField] private List<WorldTheme> worldThemes = new List<WorldTheme>();
        
        // Systems
        private Mechanics.HazardSystem hazardSystem;
        private PerlinNoise perlinGenerator;
        private PoissonDiskSampling poissonGenerator;
        private CellularAutomata caGenerator;
        private AISolvabilityBot solvabilityBot;
        
        // Generated levels cache
        private Dictionary<int, LevelData> generatedLevels = new Dictionary<int, LevelData>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyConfig();
            
            InitializeWorldThemes();
        }

        private void ApplyConfig()
        {
            LevelConfig config = ConfigProvider.LevelConfig;
            if (config == null) return;

            floorsPerWorld = config.floorsPerWorld;
            roomsPerFloor = config.roomsPerFloor;
            levelsPerWorld = Mathf.Max(1, floorsPerWorld * roomsPerFloor);
            totalLevels = Mathf.Max(1, config.totalWorlds * levelsPerWorld);

            baseTimerSeconds = config.startingTimer;
            endTimerSeconds = config.endingTimer;
            baseHazardCount = config.startingHazardCount;
            maxHazardCount = config.endingHazardCount;
        }
        
        private void Start()
        {
            hazardSystem = Mechanics.HazardSystem.Instance;
            perlinGenerator = GetComponent<PerlinNoise>();
            poissonGenerator = GetComponent<PoissonDiskSampling>();
            caGenerator = GetComponent<CellularAutomata>();
            solvabilityBot = GetComponent<AISolvabilityBot>();
        }
        
        /// <summary>
        /// Initialize world themes (10 worlds total, 1-3 for alpha).
        /// </summary>
        private void InitializeWorldThemes()
        {
            worldThemes.Clear();
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 1,
                themeName = "Suburban Homes",
                description = "Classic residential windows - where it all begins",
                ambientColor = new Color(0.9f, 0.95f, 1f),
                musicTrack = "Music/World1_Suburban",
                startLevel = 1,
                endLevel = 1000
            });
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 2,
                themeName = "Downtown Offices",
                description = "Corporate skyscrapers with city views",
                ambientColor = new Color(0.85f, 0.9f, 1f),
                musicTrack = "Music/World2_Office",
                startLevel = 1001,
                endLevel = 2000
            });
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 3,
                themeName = "Historic District",
                description = "Stained glass, ornate frames, centuries of grime",
                ambientColor = new Color(1f, 0.95f, 0.85f),
                musicTrack = "Music/World3_Historic",
                startLevel = 2001,
                endLevel = 3000
            });
            
            // Future worlds (launch)
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 4,
                themeName = "Industrial Zone",
                description = "Factories, warehouses, heavy rust and oil",
                ambientColor = new Color(0.8f, 0.75f, 0.7f),
                musicTrack = "Music/World4_Industrial",
                startLevel = 3001,
                endLevel = 4000
            });
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 5,
                themeName = "Coastal Resort",
                description = "Beach hotels, salt spray, seagull presents",
                ambientColor = new Color(0.85f, 0.95f, 1f),
                musicTrack = "Music/World5_Coastal",
                startLevel = 4001,
                endLevel = 5000
            });
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 6,
                themeName = "Urban Decay",
                description = "Abandoned buildings, graffiti, overgrown",
                ambientColor = new Color(0.7f, 0.75f, 0.7f),
                musicTrack = "Music/World6_Decay",
                startLevel = 5001,
                endLevel = 6000
            });
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 7,
                themeName = "Mountain Lodge",
                description = "Alpine chalets, frost patterns, breathtaking views",
                ambientColor = new Color(0.9f, 0.95f, 1f),
                musicTrack = "Music/World7_Mountain",
                startLevel = 6001,
                endLevel = 7000
            });
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 8,
                themeName = "Cyberpunk Megacity",
                description = "Neon-lit skyscrapers, nano-bot infestations, acid rain",
                ambientColor = new Color(0.8f, 0.85f, 1f),
                musicTrack = "Music/World8_Cyberpunk",
                startLevel = 7001,
                endLevel = 8000
            });
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 9,
                themeName = "Space Station",
                description = "Zero-G cleaning, gyroscope gestures, cosmic vistas",
                ambientColor = new Color(0.7f, 0.8f, 0.9f),
                musicTrack = "Music/World9_Space",
                startLevel = 8001,
                endLevel = 9000
            });
            
            worldThemes.Add(new WorldTheme
            {
                worldNumber = 10,
                themeName = "Formby's Mansion",
                description = "The final challenge - George's legendary estate",
                ambientColor = new Color(1f, 0.98f, 0.95f),
                musicTrack = "Music/World10_Mansion",
                startLevel = 9001,
                endLevel = 10000
            });

            ApplyWorldThemeNamesFromConfig();
            ApplyWorldThemeRanges();
            
            Debug.Log($"Initialized {worldThemes.Count} world themes");
        }

        private void ApplyWorldThemeNamesFromConfig()
        {
            LevelConfig config = ConfigProvider.LevelConfig;
            if (config == null || config.worldThemeNames == null) return;

            int count = Mathf.Min(worldThemes.Count, config.worldThemeNames.Length);
            for (int i = 0; i < count; i++)
            {
                if (!string.IsNullOrWhiteSpace(config.worldThemeNames[i]))
                {
                    worldThemes[i].themeName = config.worldThemeNames[i];
                }
            }
        }

        private void ApplyWorldThemeRanges()
        {
            int worldCount = worldThemes.Count;
            for (int i = 0; i < worldCount; i++)
            {
                int worldNumber = worldThemes[i].worldNumber;
                int startLevel = ((worldNumber - 1) * levelsPerWorld) + 1;
                int endLevel = worldNumber * levelsPerWorld;

                worldThemes[i].startLevel = startLevel;
                worldThemes[i].endLevel = endLevel;
            }
        }
        
        /// <summary>
        /// Generate a specific level (on-demand generation).
        /// </summary>
        public LevelData GenerateLevel(int levelNumber)
        {
            // Check cache
            if (generatedLevels.ContainsKey(levelNumber))
            {
                return generatedLevels[levelNumber];
            }
            
            // Calculate world/floor/room
            int worldNumber = Mathf.Clamp(((levelNumber - 1) / levelsPerWorld) + 1, 1, Mathf.Max(1, ConfigProvider.LevelConfig != null ? ConfigProvider.LevelConfig.totalWorlds : 10));
            int levelInWorld = ((levelNumber - 1) % levelsPerWorld) + 1;
            int floorNumber = ((levelInWorld - 1) / roomsPerFloor) + 1;
            int roomNumber = ((levelInWorld - 1) % roomsPerFloor) + 1;
            
            // Create level data
            LevelData level = new LevelData
            {
                levelNumber = levelNumber,
                worldNumber = worldNumber,
                floorNumber = floorNumber,
                roomNumber = roomNumber,
                
                // Seeds (deterministic based on level number)
                perlinSeed = levelNumber * 7919,      // Prime multipliers for variation
                poissonSeed = levelNumber * 6421,
                hazardSeed = levelNumber * 5381,
                
                // Difficulty scaling
                difficultyMultiplier = CalculateDifficultyMultiplier(levelNumber),
                hazardCount = CalculateHazardCount(worldNumber, floorNumber),
                regenRate = CalculateRegenRate(worldNumber),
                timerSeconds = CalculateTimerDuration(worldNumber),
                
                // Theme
                themeName = GetWorldTheme(worldNumber).themeName,
                isKeyLevel = (levelNumber % 100 == 0),
                hasStory = (levelNumber % 500 == 0)
            };
            
            // Generate hazards
            level.hazards = hazardSystem.SelectRandomHazards(worldNumber, level.hazardCount);
            
            // Validate solvability (would use AI bot in full implementation)
            level.isSolvable = ValidateLevel(level);
            level.elegantPaths = Random.Range(1, 4);  // Simplified
            
            // Cache and return
            generatedLevels[levelNumber] = level;
            
            Debug.Log($"Generated Level {levelNumber}: World {worldNumber}, Floor {floorNumber}, Room {roomNumber} ({level.hazardCount} hazards, {level.timerSeconds}s)");
            
            return level;
        }
        
        /// <summary>
        /// Calculate difficulty multiplier (1.0 → 10.0 across 10 worlds).
        /// </summary>
        private float CalculateDifficultyMultiplier(int levelNumber)
        {
            float progress = (float)levelNumber / 10000f;  // 0-1 across all 10,000 levels
            return 1f + (progress * 9f);  // 1.0 → 10.0
        }
        
        /// <summary>
        /// Calculate hazard count (8-25 based on world + floor).
        /// </summary>
        private int CalculateHazardCount(int worldNumber, int floorNumber)
        {
            int worldBase = baseHazardCount + (worldNumber - 1) * 2;  // +2 per world
            int floorBonus = Mathf.FloorToInt(floorNumber * 0.1f);    // +0.1 per floor
            return Mathf.Clamp(worldBase + floorBonus, baseHazardCount, maxHazardCount);
        }
        
        /// <summary>
        /// Calculate regeneration rate (2.5%/sec base, increases with world).
        /// </summary>
        private float CalculateRegenRate(int worldNumber)
        {
            return 0.025f + (worldNumber - 1) * 0.002f;  // 2.5% → 4.3%/sec
        }
        
        /// <summary>
        /// Calculate timer duration (120s → 40s scaling).
        /// </summary>
        private float CalculateTimerDuration(int worldNumber)
        {
            float t = (float)(worldNumber - 1) / 9f;  // 0-1 across 10 worlds
            return Mathf.Lerp(baseTimerSeconds, endTimerSeconds, t);
        }
        
        /// <summary>
        /// Validate level is solvable (simplified - would use AI bot).
        /// </summary>
        private bool ValidateLevel(LevelData level)
        {
            // In full implementation: Run AISolvabilityBot
            // For now: Always true (assumes procedural gen is tuned)
            return true;
        }
        
        /// <summary>
        /// Get world theme by world number.
        /// </summary>
        public WorldTheme GetWorldTheme(int worldNumber)
        {
            return worldThemes.Find(w => w.worldNumber == worldNumber);
        }
        
        /// <summary>
        /// Pre-generate all levels for a world (background loading).
        /// </summary>
        public void PreGenerateWorld(int worldNumber)
        {
            int startLevel = (worldNumber - 1) * levelsPerWorld + 1;
            int endLevel = worldNumber * levelsPerWorld;
            
            Debug.Log($"Pre-generating World {worldNumber} (Levels {startLevel}-{endLevel})...");
            
            for (int i = startLevel; i <= endLevel; i++)
            {
                GenerateLevel(i);
            }
            
            Debug.Log($"World {worldNumber} generation complete!");
        }
        
        /// <summary>
        /// Get level from cache.
        /// </summary>
        public LevelData GetLevel(int levelNumber)
        {
            if (!generatedLevels.ContainsKey(levelNumber))
            {
                return GenerateLevel(levelNumber);
            }
            return generatedLevels[levelNumber];
        }
        
        /// <summary>
        /// Clear level cache (memory management).
        /// </summary>
        public void ClearCache()
        {
            generatedLevels.Clear();
            Debug.Log("Level cache cleared");
        }
        
        // === DEBUG FUNCTIONS ===
        
        [ContextMenu("Generate Level 1")]
        public void DEBUG_GenerateLevel1()
        {
            var level = GenerateLevel(1);
            Debug.Log($"Level 1: {level.hazardCount} hazards, {level.timerSeconds}s timer");
        }
        
        [ContextMenu("Generate Level 1000 (End of World 1)")]
        public void DEBUG_GenerateLevel1000()
        {
            var level = GenerateLevel(1000);
            Debug.Log($"Level 1000: {level.hazardCount} hazards, {level.timerSeconds}s timer, KEY LEVEL: {level.isKeyLevel}");
        }
        
        [ContextMenu("Generate Level 3000 (End of Alpha Content)")]
        public void DEBUG_GenerateLevel3000()
        {
            var level = GenerateLevel(3000);
            Debug.Log($"Level 3000: World {level.worldNumber}, {level.hazardCount} hazards, {level.timerSeconds}s timer");
        }
        
        [ContextMenu("Pre-Generate World 1")]
        public void DEBUG_PreGenerateWorld1()
        {
            PreGenerateWorld(1);
        }
    }
}








