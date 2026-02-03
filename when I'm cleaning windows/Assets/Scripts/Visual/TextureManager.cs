using UnityEngine;
using System.Collections.Generic;
using WhenImCleaningWindows.Mechanics;

namespace WhenImCleaningWindows.Visual
{
    /// <summary>
    /// Texture Manager for all hazard visuals, window frames, and backgrounds.
    /// Handles texture atlasing, procedural generation for missing assets, and theme management.
    /// </summary>
    public class TextureManager : MonoBehaviour
    {
        public static TextureManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool useProceduralFallbacks = true;
        [SerializeField] private int textureResolution = 512;

        [Header("Hazard Textures (24 types)")]
        [SerializeField] private Texture2D birdPoopTexture;
        [SerializeField] private Texture2D fliesTexture;
        [SerializeField] private Texture2D mudTexture;
        [SerializeField] private Texture2D oilTexture;
        [SerializeField] private Texture2D spiderwebTexture;
        [SerializeField] private Texture2D graffitiTexture;
        [SerializeField] private Texture2D stickersTexture;
        [SerializeField] private Texture2D tapeTexture;
        [SerializeField] private Texture2D waterMarksTexture;
        [SerializeField] private Texture2D rustTexture;
        [SerializeField] private Texture2D gumTexture;
        [SerializeField] private Texture2D paintTexture;
        [SerializeField] private Texture2D ashTexture;
        [SerializeField] private Texture2D moldTexture;
        [SerializeField] private Texture2D scratchesTexture;
        [SerializeField] private Texture2D dustTexture;
        [SerializeField] private Texture2D frostTexture;
        [SerializeField] private Texture2D algaeTexture;
        [SerializeField] private Texture2D treeSapTexture;
        [SerializeField] private Texture2D fogTexture;
        [SerializeField] private Texture2D nanoBotsTexture;
        [SerializeField] private Texture2D pollenTexture;
        [SerializeField] private Texture2D condensationTexture;
        [SerializeField] private Texture2D pollutionTexture;

        [Header("Window Frames (10 themes)")]
        [SerializeField] private Sprite[] suburbanFrames;
        [SerializeField] private Sprite[] downtownFrames;
        [SerializeField] private Sprite[] historicFrames;
        [SerializeField] private Sprite[] industrialFrames;
        [SerializeField] private Sprite[] coastalFrames;
        [SerializeField] private Sprite[] urbanDecayFrames;
        [SerializeField] private Sprite[] mountainFrames;
        [SerializeField] private Sprite[] cyberpunkFrames;
        [SerializeField] private Sprite[] spaceStationFrames;
        [SerializeField] private Sprite[] formbyMansionFrames;

        [Header("Backgrounds")]
        [SerializeField] private Sprite[] worldBackgrounds;

        private Dictionary<HazardType, Texture2D> hazardTextureCache;
        private Dictionary<int, Sprite[]> windowFrameCache;

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

            InitializeTextures();
        }

        private void InitializeTextures()
        {
            Debug.Log("[TextureManager] Initializing texture system...");

            hazardTextureCache = new Dictionary<HazardType, Texture2D>();
            windowFrameCache = new Dictionary<int, Sprite[]>();

            // Load or generate hazard textures
            LoadHazardTextures();

            // Load window frames
            LoadWindowFrames();

            Debug.Log($"[TextureManager] ✓ Texture system initialized ({hazardTextureCache.Count} hazards, {windowFrameCache.Count} window themes)");
        }

        #endregion

        #region Hazard Textures

        private void LoadHazardTextures()
        {
            // Static Hazards
            RegisterHazardTexture(HazardType.BirdPoop, birdPoopTexture, Color.white, "Bird Poop");
            RegisterHazardTexture(HazardType.DeadFlies, fliesTexture, new Color(0.1f, 0.1f, 0.1f), "Flies");
            RegisterHazardTexture(HazardType.Mud, mudTexture, new Color(0.4f, 0.3f, 0.2f), "Mud");
            RegisterHazardTexture(HazardType.OilStain, oilTexture, new Color(0.1f, 0.1f, 0.1f, 0.8f), "Oil");
            RegisterHazardTexture(HazardType.Spiderweb, spiderwebTexture, new Color(0.9f, 0.9f, 0.9f, 0.3f), "Spiderweb");
            RegisterHazardTexture(HazardType.Graffiti, graffitiTexture, Color.magenta, "Graffiti");
            RegisterHazardTexture(HazardType.Stickers, stickersTexture, Color.yellow, "Stickers");
            RegisterHazardTexture(HazardType.TapeResidue, tapeTexture, new Color(0.9f, 0.9f, 0.7f), "Tape");
            RegisterHazardTexture(HazardType.WaterMarks, waterMarksTexture, new Color(0.7f, 0.7f, 0.8f, 0.5f), "Water Marks");
            RegisterHazardTexture(HazardType.Rust, rustTexture, new Color(0.6f, 0.3f, 0.1f), "Rust");
            RegisterHazardTexture(HazardType.Gum, gumTexture, Color.red, "Gum");
            RegisterHazardTexture(HazardType.Paint, paintTexture, Color.cyan, "Paint");
            RegisterHazardTexture(HazardType.Ash, ashTexture, new Color(0.3f, 0.3f, 0.3f), "Ash");
            RegisterHazardTexture(HazardType.Mold, moldTexture, new Color(0.2f, 0.5f, 0.2f), "Mold");
            RegisterHazardTexture(HazardType.Scratches, scratchesTexture, new Color(0.8f, 0.8f, 0.8f, 0.6f), "Scratches");
            RegisterHazardTexture(HazardType.Dust, dustTexture, new Color(0.7f, 0.7f, 0.6f, 0.4f), "Dust");

            // Regenerating Hazards
            RegisterHazardTexture(HazardType.Frost, frostTexture, new Color(0.8f, 0.9f, 1.0f, 0.6f), "Frost");
            RegisterHazardTexture(HazardType.Algae, algaeTexture, new Color(0.2f, 0.6f, 0.3f), "Algae");
            RegisterHazardTexture(HazardType.TreeSap, treeSapTexture, new Color(0.6f, 0.4f, 0.1f, 0.7f), "Tree Sap");
            RegisterHazardTexture(HazardType.Fog, fogTexture, new Color(0.9f, 0.9f, 0.9f, 0.3f), "Fog");
            RegisterHazardTexture(HazardType.NanoBots, nanoBotsTexture, new Color(0.3f, 0.6f, 1.0f, 0.5f), "Nano-Bots");
            RegisterHazardTexture(HazardType.Pollen, pollenTexture, new Color(1.0f, 0.9f, 0.3f, 0.4f), "Pollen");
            RegisterHazardTexture(HazardType.Condensation, condensationTexture, new Color(0.8f, 0.8f, 0.9f, 0.4f), "Condensation");
            RegisterHazardTexture(HazardType.Pollution, pollutionTexture, new Color(0.4f, 0.4f, 0.4f, 0.5f), "Pollution");

            Debug.Log($"[TextureManager] Loaded {hazardTextureCache.Count} hazard textures");
        }

        private void RegisterHazardTexture(HazardType type, Texture2D texture, Color fallbackColor, string name)
        {
            if (texture != null)
            {
                hazardTextureCache[type] = texture;
                Debug.Log($"[TextureManager] ✓ Loaded texture: {name}");
            }
            else if (useProceduralFallbacks)
            {
                // Generate procedural placeholder
                Texture2D proceduralTexture = GenerateProceduralTexture(fallbackColor, name);
                hazardTextureCache[type] = proceduralTexture;
                Debug.Log($"[TextureManager] Generated procedural texture: {name} ({fallbackColor})");
            }
            else
            {
                Debug.LogWarning($"[TextureManager] Missing texture: {name}");
            }
        }

        public Texture2D GetHazardTexture(HazardType type)
        {
            if (hazardTextureCache.TryGetValue(type, out Texture2D texture))
            {
                return texture;
            }

            Debug.LogWarning($"[TextureManager] Texture not found for {type}");
            return Texture2D.whiteTexture;
        }

        #endregion

        #region Procedural Generation

        private Texture2D GenerateProceduralTexture(Color baseColor, string name)
        {
            Texture2D texture = new Texture2D(textureResolution, textureResolution, TextureFormat.RGBA32, true);
            texture.name = $"Procedural_{name}";

            Color[] pixels = new Color[textureResolution * textureResolution];

            // Simple noise-based generation
            for (int y = 0; y < textureResolution; y++)
            {
                for (int x = 0; x < textureResolution; x++)
                {
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    Color pixelColor = baseColor * noise;
                    pixelColor.a = baseColor.a;
                    pixels[y * textureResolution + x] = pixelColor;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        #endregion

        #region Window Frames

        private void LoadWindowFrames()
        {
            windowFrameCache[1] = suburbanFrames;
            windowFrameCache[2] = downtownFrames;
            windowFrameCache[3] = historicFrames;
            windowFrameCache[4] = industrialFrames;
            windowFrameCache[5] = coastalFrames;
            windowFrameCache[6] = urbanDecayFrames;
            windowFrameCache[7] = mountainFrames;
            windowFrameCache[8] = cyberpunkFrames;
            windowFrameCache[9] = spaceStationFrames;
            windowFrameCache[10] = formbyMansionFrames;

            Debug.Log($"[TextureManager] Loaded {windowFrameCache.Count} window frame themes");
        }

        public Sprite GetWindowFrame(int worldNumber, int variant = 0)
        {
            if (windowFrameCache.TryGetValue(worldNumber, out Sprite[] frames))
            {
                if (frames != null && frames.Length > 0)
                {
                    int index = Mathf.Clamp(variant, 0, frames.Length - 1);
                    return frames[index];
                }
            }

            Debug.LogWarning($"[TextureManager] Window frame not found for World {worldNumber}");
            return null;
        }

        public Sprite GetWorldBackground(int worldNumber)
        {
            if (worldBackgrounds != null && worldBackgrounds.Length > 0)
            {
                int index = Mathf.Clamp(worldNumber - 1, 0, worldBackgrounds.Length - 1);
                return worldBackgrounds[index];
            }

            return null;
        }

        #endregion

        #region Asset Requirements Documentation

        [ContextMenu("Print Asset Requirements")]
        public void PrintAssetRequirements()
        {
            Debug.Log("=== VISUAL ASSET REQUIREMENTS ===");
            Debug.Log("\n--- HAZARD TEXTURES (24 types, 512x512 PNG with alpha) ---");
            Debug.Log("Static Hazards:");
            Debug.Log("1. BirdPoop - White splatter with chunky texture");
            Debug.Log("2. Flies - Small black dots in cluster");
            Debug.Log("3. Mud - Brown smudge with grainy texture");
            Debug.Log("4. Oil - Dark translucent streak");
            Debug.Log("5. Spiderweb - Delicate white threads");
            Debug.Log("6. Graffiti - Colorful spray paint tags");
            Debug.Log("7. Stickers - Bright adhesive shapes");
            Debug.Log("8. Tape - Tan/yellow adhesive strips");
            Debug.Log("9. WaterMarks - Faint mineral deposits");
            Debug.Log("10. Rust - Orange-brown corrosion");
            Debug.Log("11. Gum - Pink/red sticky blob");
            Debug.Log("12. Paint - Drip marks in various colors");
            Debug.Log("13. Ash - Gray dusty coating");
            Debug.Log("14. Mold - Green fuzzy growth");
            Debug.Log("15. Scratches - Linear abrasions");
            Debug.Log("16. Dust - Light gray film");

            Debug.Log("\nRegenerating Hazards (animated):");
            Debug.Log("17. Frost - Icy crystals spreading");
            Debug.Log("18. Algae - Green slime growing");
            Debug.Log("19. TreeSap - Sticky amber drips");
            Debug.Log("20. Fog - Wispy condensation");
            Debug.Log("21. NanoBots - Glowing blue tech particles");
            Debug.Log("22. Pollen - Yellow dust cloud");
            Debug.Log("23. Condensation - Water droplets forming");
            Debug.Log("24. Pollution - Grimy smog layer");

            Debug.Log("\n--- WINDOW FRAMES (10 worlds, 3 variants each, 1920x1080 PNG) ---");
            Debug.Log("World 1: Suburban Homes - White vinyl frames, simple");
            Debug.Log("World 2: Downtown Offices - Modern aluminum, sleek");
            Debug.Log("World 3: Historic District - Ornate wooden frames");
            Debug.Log("World 4: Industrial Zone - Metal riveted frames");
            Debug.Log("World 5: Coastal Resort - Beach house style");
            Debug.Log("World 6: Urban Decay - Broken, graffitied frames");
            Debug.Log("World 7: Mountain Lodge - Rustic wood frames");
            Debug.Log("World 8: Cyberpunk Megacity - Neon-lit tech frames");
            Debug.Log("World 9: Space Station - Futuristic porthole frames");
            Debug.Log("World 10: Formby's Mansion - Elegant Victorian frames");

            Debug.Log("\n--- BACKGROUNDS (10 worlds, 1920x1080 PNG) ---");
            Debug.Log("Each world needs parallax layers:");
            Debug.Log("- Far background (sky/horizon)");
            Debug.Log("- Mid background (buildings/landscape)");
            Debug.Log("- Near background (street level)");

            Debug.Log("\n--- PARTICLE EFFECTS (see VFXManager) ---");
            Debug.Log("=================================");
        }

        #endregion

        #region Debug Context Menu

        [ContextMenu("Test Procedural Generation")]
        private void TestProceduralGeneration()
        {
            Debug.Log("=== Testing Procedural Texture Generation ===");

            Color[] testColors = new Color[]
            {
                Color.red,
                Color.green,
                Color.blue,
                new Color(0.5f, 0.5f, 0.5f, 0.5f)
            };

            foreach (Color color in testColors)
            {
                Texture2D test = GenerateProceduralTexture(color, $"Test_{color}");
                Debug.Log($"Generated procedural texture with color {color}");
            }

            Debug.Log("=== Procedural Test Complete ===");
        }

        #endregion
    }
}








