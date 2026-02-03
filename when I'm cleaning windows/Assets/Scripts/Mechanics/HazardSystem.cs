using UnityEngine;
using System.Collections.Generic;

namespace WhenImCleaningWindows.Mechanics
{
    /// <summary>
    /// Hazard System - Manages all 24 hazard types and their behaviors.
    /// Integrates with procedural generation and cellular automata for regenerating hazards.
    /// </summary>
    public class HazardSystem : MonoBehaviour
    {
        public static HazardSystem Instance { get; private set; }
        
        [Header("Hazard Database")]
        [SerializeField] private List<HazardProperties> hazardDatabase = new List<HazardProperties>();
        
        [Header("Current Level Hazards")]
        [SerializeField] private List<HazardType> activeHazards = new List<HazardType>();
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeHazardDatabase();
        }
        
        /// <summary>
        /// Initialize all 24 hazard type properties.
        /// </summary>
        private void InitializeHazardDatabase()
        {
            hazardDatabase.Clear();
            
            // === STATIC HAZARDS (16 types) ===
            
            // 1. Bird Poop (Classic, Tutorial)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.BirdPoop,
                displayName = "Bird Poop",
                primaryColor = new Color(0.9f, 0.9f, 0.85f),
                isRegenerating = false,
                cleanDifficulty = 1.0f,
                swipesRequired = 2,
                firstAppearanceWorld = 1,
                spawnWeight = 1.5f,
                cleanSFX = "event:/SFX/Clean/Splat",
                hasParticleEffect = true
            });
            
            // 2. Dead Flies
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.DeadFlies,
                displayName = "Dead Flies",
                primaryColor = new Color(0.2f, 0.2f, 0.2f),
                isRegenerating = false,
                cleanDifficulty = 0.8f,
                swipesRequired = 1,
                firstAppearanceWorld = 1,
                spawnWeight = 1.2f,
                cleanSFX = "event:/SFX/Clean/Sweep",
                ambientSFX = "event:/AMB/Flies_Buzzing",
                hasParticleEffect = true
            });
            
            // 3. Mud
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Mud,
                displayName = "Mud",
                primaryColor = new Color(0.4f, 0.3f, 0.2f),
                isRegenerating = false,
                cleanDifficulty = 1.2f,
                swipesRequired = 4,
                firstAppearanceWorld = 1,
                spawnWeight = 1.3f,
                cleanSFX = "event:/SFX/Clean/Squelch"
            });
            
            // 4. Oil Stain
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.OilStain,
                displayName = "Oil Stain",
                primaryColor = new Color(0.1f, 0.1f, 0.15f, 0.8f),
                isRegenerating = false,
                cleanDifficulty = 1.8f,
                swipesRequired = 6,
                firstAppearanceWorld = 2,
                spawnWeight = 0.8f,
                cleanSFX = "event:/SFX/Clean/Grease"
            });
            
            // 5. Spiderweb
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Spiderweb,
                displayName = "Spiderweb",
                primaryColor = new Color(0.9f, 0.9f, 0.9f, 0.4f),
                isRegenerating = false,
                cleanDifficulty = 0.6f,
                swipesRequired = 2,
                firstAppearanceWorld = 2,
                spawnWeight = 1.0f,
                cleanSFX = "event:/SFX/Clean/Tear",
                hasParticleEffect = true
            });
            
            // 6. Graffiti
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Graffiti,
                displayName = "Graffiti",
                primaryColor = new Color(0.9f, 0.2f, 0.3f),
                isRegenerating = false,
                cleanDifficulty = 2.0f,
                swipesRequired = 8,
                firstAppearanceWorld = 3,
                spawnWeight = 0.5f,
                cleanSFX = "event:/SFX/Clean/Scrub_Hard"
            });
            
            // 7. Stickers
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Stickers,
                displayName = "Stickers",
                primaryColor = new Color(1f, 0.8f, 0.2f),
                isRegenerating = false,
                cleanDifficulty = 1.5f,
                swipesRequired = 5,
                firstAppearanceWorld = 2,
                spawnWeight = 0.9f,
                cleanSFX = "event:/SFX/Clean/Peel",
                hasParticleEffect = true
            });
            
            // 8. Tape Residue
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.TapeResidue,
                displayName = "Tape Residue",
                primaryColor = new Color(0.8f, 0.8f, 0.7f, 0.6f),
                isRegenerating = false,
                cleanDifficulty = 1.4f,
                swipesRequired = 4,
                firstAppearanceWorld = 3,
                spawnWeight = 0.7f,
                cleanSFX = "event:/SFX/Clean/Sticky"
            });
            
            // 9. Water Marks
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.WaterMarks,
                displayName = "Water Marks",
                primaryColor = new Color(0.6f, 0.7f, 0.8f, 0.3f),
                isRegenerating = false,
                cleanDifficulty = 0.9f,
                swipesRequired = 3,
                firstAppearanceWorld = 1,
                spawnWeight = 1.4f,
                cleanSFX = "event:/SFX/Clean/Wipe"
            });
            
            // 10. Rust
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Rust,
                displayName = "Rust",
                primaryColor = new Color(0.7f, 0.3f, 0.1f),
                isRegenerating = false,
                cleanDifficulty = 2.5f,
                swipesRequired = 10,
                firstAppearanceWorld = 4,
                spawnWeight = 0.4f,
                cleanSFX = "event:/SFX/Clean/Grind",
                blocksGestures = true  // Requires power-up
            });
            
            // 11. Gum
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Gum,
                displayName = "Gum",
                primaryColor = new Color(1f, 0.4f, 0.6f),
                isRegenerating = false,
                cleanDifficulty = 1.6f,
                swipesRequired = 5,
                firstAppearanceWorld = 2,
                spawnWeight = 0.8f,
                cleanSFX = "event:/SFX/Clean/Stretch"
            });
            
            // 12. Paint
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Paint,
                displayName = "Paint",
                primaryColor = new Color(0.2f, 0.4f, 0.8f),
                isRegenerating = false,
                cleanDifficulty = 1.7f,
                swipesRequired = 6,
                firstAppearanceWorld = 3,
                spawnWeight = 0.6f,
                cleanSFX = "event:/SFX/Clean/Scrape"
            });
            
            // 13. Ash
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Ash,
                displayName = "Ash",
                primaryColor = new Color(0.3f, 0.3f, 0.3f, 0.7f),
                isRegenerating = false,
                cleanDifficulty = 0.7f,
                swipesRequired = 2,
                firstAppearanceWorld = 3,
                spawnWeight = 1.0f,
                cleanSFX = "event:/SFX/Clean/Blow",
                hasParticleEffect = true
            });
            
            // 14. Mold
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Mold,
                displayName = "Mold",
                primaryColor = new Color(0.2f, 0.5f, 0.2f),
                isRegenerating = false,
                cleanDifficulty = 1.9f,
                swipesRequired = 7,
                firstAppearanceWorld = 4,
                spawnWeight = 0.5f,
                cleanSFX = "event:/SFX/Clean/Scrub_Bio"
            });
            
            // 15. Scratches
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Scratches,
                displayName = "Scratches",
                primaryColor = new Color(0.8f, 0.8f, 0.8f, 0.5f),
                isRegenerating = false,
                cleanDifficulty = 3.0f,
                swipesRequired = 12,
                firstAppearanceWorld = 5,
                spawnWeight = 0.3f,
                cleanSFX = "event:/SFX/Clean/Polish",
                requiresSpecialTool = true  // Needs buffer power-up
            });
            
            // 16. Dust
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Dust,
                displayName = "Dust",
                primaryColor = new Color(0.7f, 0.7f, 0.6f, 0.4f),
                isRegenerating = false,
                cleanDifficulty = 0.5f,
                swipesRequired = 1,
                firstAppearanceWorld = 1,
                spawnWeight = 1.5f,
                cleanSFX = "event:/SFX/Clean/Dust",
                hasParticleEffect = true
            });
            
            // === REGENERATING HAZARDS (8 types) ===
            
            // 17. Frost (Cellular Automata)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Frost,
                displayName = "Frost",
                primaryColor = new Color(0.8f, 0.9f, 1f, 0.6f),
                isRegenerating = true,
                cleanDifficulty = 1.3f,
                swipesRequired = 4,
                regenRate = 0.025f,  // 2.5%/sec
                spreadRadius = 2.5f,
                firstAppearanceWorld = 4,
                spawnWeight = 0.7f,
                cleanSFX = "event:/SFX/Clean/Ice_Crack",
                ambientSFX = "event:/AMB/Frost_Crackling",
                hasParticleEffect = true
            });
            
            // 18. Algae (Organic Growth)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Algae,
                displayName = "Algae",
                primaryColor = new Color(0.2f, 0.6f, 0.3f),
                isRegenerating = true,
                cleanDifficulty = 1.5f,
                swipesRequired = 5,
                regenRate = 0.020f,  // 2%/sec
                spreadRadius = 2.0f,
                firstAppearanceWorld = 3,
                spawnWeight = 0.8f,
                cleanSFX = "event:/SFX/Clean/Slime"
            });
            
            // 19. Tree Sap (Sticky Spread)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.TreeSap,
                displayName = "Tree Sap",
                primaryColor = new Color(0.6f, 0.4f, 0.1f, 0.7f),
                isRegenerating = true,
                cleanDifficulty = 2.0f,
                swipesRequired = 7,
                regenRate = 0.015f,  // 1.5%/sec
                spreadRadius = 1.5f,
                firstAppearanceWorld = 5,
                spawnWeight = 0.5f,
                cleanSFX = "event:/SFX/Clean/Sticky_Thick"
            });
            
            // 20. Fog (Condenses Over Time)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Fog,
                displayName = "Fog",
                primaryColor = new Color(0.8f, 0.8f, 0.8f, 0.5f),
                isRegenerating = true,
                cleanDifficulty = 0.8f,
                swipesRequired = 2,
                regenRate = 0.025f,  // 2.5%/sec - gentle regen
                spreadRadius = 3.0f,
                firstAppearanceWorld = 2,
                spawnWeight = 1.0f,
                cleanSFX = "event:/SFX/Clean/Wipe_Fast",
                hasParticleEffect = true
            });
            
            // 21. Nano-Bots (Sci-Fi Self-Replicating)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.NanoBots,
                displayName = "Nano-Bots",
                primaryColor = new Color(0.2f, 0.8f, 1f),
                isRegenerating = true,
                cleanDifficulty = 2.2f,
                swipesRequired = 8,
                regenRate = 0.028f,  // 2.8%/sec
                spreadRadius = 2.5f,
                firstAppearanceWorld = 8,  // Late game
                spawnWeight = 0.4f,
                cleanSFX = "event:/SFX/Clean/Electric_Zap",
                ambientSFX = "event:/AMB/Nano_Hum",
                hasParticleEffect = true
            });
            
            // 22. Pollen (Seasonal Drift)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Pollen,
                displayName = "Pollen",
                primaryColor = new Color(1f, 0.9f, 0.3f, 0.6f),
                isRegenerating = true,
                cleanDifficulty = 0.6f,
                swipesRequired = 2,
                regenRate = 0.022f,  // 2.2%/sec
                spreadRadius = 2.8f,
                firstAppearanceWorld = 2,
                spawnWeight = 0.9f,
                cleanSFX = "event:/SFX/Clean/Blow_Light",
                hasParticleEffect = true
            });
            
            // 23. Condensation (Humidity-Based)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Condensation,
                displayName = "Condensation",
                primaryColor = new Color(0.7f, 0.8f, 0.9f, 0.4f),
                isRegenerating = true,
                cleanDifficulty = 0.7f,
                swipesRequired = 2,
                regenRate = 0.025f,  // 2.5%/sec
                spreadRadius = 2.2f,
                firstAppearanceWorld = 1,
                spawnWeight = 1.2f,
                cleanSFX = "event:/SFX/Clean/Squeegee",
                hasParticleEffect = true
            });
            
            // 24. Pollution (Smog Accumulation)
            hazardDatabase.Add(new HazardProperties
            {
                type = HazardType.Pollution,
                displayName = "Pollution",
                primaryColor = new Color(0.5f, 0.5f, 0.4f, 0.7f),
                isRegenerating = true,
                cleanDifficulty = 1.8f,
                swipesRequired = 6,
                regenRate = 0.018f,  // 1.8%/sec
                spreadRadius = 2.0f,
                firstAppearanceWorld = 6,
                spawnWeight = 0.6f,
                cleanSFX = "event:/SFX/Clean/Chemical_Spray"
            });
            
            Debug.Log($"Initialized {hazardDatabase.Count} hazard types");
        }
        
        /// <summary>
        /// Get hazard properties by type.
        /// </summary>
        public HazardProperties GetHazardProperties(HazardType type)
        {
            return hazardDatabase.Find(h => h.type == type);
        }
        
        /// <summary>
        /// Get all hazards available for a specific world.
        /// </summary>
        public List<HazardProperties> GetHazardsForWorld(int worldNumber)
        {
            return hazardDatabase.FindAll(h => h.firstAppearanceWorld <= worldNumber);
        }
        
        /// <summary>
        /// Get random hazards for level generation (weighted selection).
        /// </summary>
        public List<HazardType> SelectRandomHazards(int worldNumber, int hazardCount)
        {
            List<HazardProperties> availableHazards = GetHazardsForWorld(worldNumber);
            List<HazardType> selected = new List<HazardType>();
            
            // Calculate total weight
            float totalWeight = 0f;
            foreach (var hazard in availableHazards)
            {
                totalWeight += hazard.spawnWeight;
            }
            
            // Select hazards using weighted random
            for (int i = 0; i < hazardCount; i++)
            {
                float randomValue = Random.value * totalWeight;
                float cumulative = 0f;
                
                foreach (var hazard in availableHazards)
                {
                    cumulative += hazard.spawnWeight;
                    if (randomValue <= cumulative)
                    {
                        selected.Add(hazard.type);
                        break;
                    }
                }
            }
            
            return selected;
        }
        
        /// <summary>
        /// Get all regenerating hazards.
        /// </summary>
        public List<HazardProperties> GetRegeneratingHazards()
        {
            return hazardDatabase.FindAll(h => h.isRegenerating);
        }
        
        /// <summary>
        /// Get hazards that require special tools.
        /// </summary>
        public List<HazardProperties> GetSpecialToolHazards()
        {
            return hazardDatabase.FindAll(h => h.requiresSpecialTool);
        }
        
        /// <summary>
        /// Calculate total clean difficulty for a hazard list.
        /// </summary>
        public float CalculateLevelDifficulty(List<HazardType> hazards)
        {
            float totalDifficulty = 0f;
            foreach (var hazardType in hazards)
            {
                var props = GetHazardProperties(hazardType);
                if (props != null)
                {
                    totalDifficulty += props.cleanDifficulty;
                    if (props.isRegenerating) totalDifficulty += 0.5f;  // Bonus difficulty
                }
            }
            return totalDifficulty;
        }
        
        // === DEBUG FUNCTIONS ===
        
        [ContextMenu("List All Hazards")]
        public void DEBUG_ListHazards()
        {
            Debug.Log("=== ALL HAZARDS ===");
            foreach (var hazard in hazardDatabase)
            {
                string regen = hazard.isRegenerating ? " [REGEN]" : "";
                string special = hazard.requiresSpecialTool ? " [SPECIAL]" : "";
                Debug.Log($"{hazard.displayName} - World {hazard.firstAppearanceWorld}, Difficulty {hazard.cleanDifficulty}{regen}{special}");
            }
        }
        
        [ContextMenu("Test World 3 Hazards")]
        public void DEBUG_TestWorld3()
        {
            var hazards = SelectRandomHazards(3, 10);
            Debug.Log($"World 3 Random Hazards: {string.Join(", ", hazards)}");
        }
    }
}








