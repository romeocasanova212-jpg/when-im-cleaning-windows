using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using WhenImCleaningWindows.Mechanics;
using WhenImCleaningWindows.Visual;

namespace WhenImCleaningWindows.Gameplay
{
    /// <summary>
    /// Hazard Renderer - Places and manages hazards on the window mesh.
    /// Handles hazard spawning, regeneration, and visual representation.
    /// </summary>
    public class HazardRenderer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WindowMeshController windowMesh;
        [SerializeField] private Transform hazardContainer;

        [Header("Hazard Prefab")]
        [SerializeField] private GameObject hazardPrefab; // Simple quad with texture

        [Header("Configuration")]
        [SerializeField] private float hazardScale = 0.5f;
        [SerializeField] private bool enableRegenerationVFX = true;

        private List<HazardInstance> activeHazards = new List<HazardInstance>();
        private Dictionary<HazardType, Texture2D> hazardTextures;

        private class HazardInstance
        {
            public GameObject gameObject;
            public HazardType type;
            public Vector3 position;
            public float size;
            public float cleanDifficulty;
            public float regenRate;
            public float currentDirtiness; // 0.0 = clean, 1.0 = fully dirty
            public bool isRegenerating;
        }

        #region Initialization

        private void Awake()
        {
            if (windowMesh == null)
            {
                windowMesh = GetComponent<WindowMeshController>();
            }

            if (hazardContainer == null)
            {
                hazardContainer = new GameObject("HazardContainer").transform;
                hazardContainer.SetParent(transform);
                hazardContainer.localPosition = Vector3.zero;
            }

            InitializeHazardTextures();
        }

        private void InitializeHazardTextures()
        {
            hazardTextures = new Dictionary<HazardType, Texture2D>();

            // Get textures from TextureManager
            if (TextureManager.Instance != null)
            {
                foreach (HazardType type in System.Enum.GetValues(typeof(HazardType)))
                {
                    Texture2D texture = TextureManager.Instance.GetHazardTexture(type);
                    if (texture != null)
                    {
                        hazardTextures[type] = texture;
                    }
                }

                UnityEngine.Debug.Log($"[HazardRenderer] Loaded {hazardTextures.Count} hazard textures");
            }
            else
            {
                UnityEngine.Debug.LogWarning("[HazardRenderer] TextureManager not available");
            }
        }

        #endregion

        #region Hazard Spawning

        public void SpawnHazards(List<Hazard> hazards)
        {
            ClearAllHazards();

            UnityEngine.Debug.Log($"[HazardRenderer] Spawning {hazards.Count} hazards...");

            foreach (Hazard hazard in hazards)
            {
                SpawnHazard(hazard);
            }

            UnityEngine.Debug.Log($"[HazardRenderer] ✓ Spawned {activeHazards.Count} hazards");
        }
        /// <summary>
        /// Spawn a single hazard (useful for testing/debugging).
        /// </summary>
        public void SpawnSingleHazard(HazardType type, Vector2 position, float size = 1f)
        {
            Hazard hazard = new Hazard
            {
                type = type,
                position = position,
                size = size,
                cleanDifficulty = 1f,
                regenRate = 0f
            };
            
            SpawnHazard(hazard);
            UnityEngine.Debug.Log($"[HazardRenderer] ✓ Spawned single hazard: {type} at {position}");
        }
        private void SpawnHazard(Hazard hazard)
        {
            // Create hazard GameObject
            GameObject hazardObj;

            if (hazardPrefab != null)
            {
                hazardObj = Instantiate(hazardPrefab, hazardContainer);
            }
            else
            {
                // Create simple quad
                hazardObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
                hazardObj.transform.SetParent(hazardContainer);
                Destroy(hazardObj.GetComponent<Collider>()); // Remove collider
            }

            hazardObj.name = $"Hazard_{hazard.type}_{activeHazards.Count}";

            // Position on window
            Vector3 worldPos = CalculateWorldPosition(hazard.position);
            hazardObj.transform.position = worldPos + Vector3.forward * -0.01f; // Slightly in front of window
            hazardObj.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
            hazardObj.transform.localScale = Vector3.one * hazard.size * hazardScale;

            // Apply texture
            MeshRenderer renderer = hazardObj.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Material mat = new Material(Shader.Find("Unlit/Transparent"));
                
                if (hazardTextures.TryGetValue(hazard.type, out Texture2D texture))
                {
                    mat.mainTexture = texture;
                }
                else
                {
                    // Fallback color based on type
                    mat.color = GetFallbackColor(hazard.type);
                }

                renderer.material = mat;
            }

            // Create instance
            HazardInstance instance = new HazardInstance
            {
                gameObject = hazardObj,
                type = hazard.type,
                position = hazard.position,
                size = hazard.size,
                cleanDifficulty = hazard.cleanDifficulty,
                regenRate = hazard.regenRate,
                currentDirtiness = 1.0f,
                isRegenerating = hazard.regenRate > 0f
            };

            activeHazards.Add(instance);

            // Apply initial dirt to window mesh
            ApplyHazardDirtToWindow(instance);
        }

        private Vector3 CalculateWorldPosition(Vector2 normalizedPos)
        {
            Vector2 windowSize = windowMesh.WindowSize;
            float x = (normalizedPos.x - 0.5f) * windowSize.x;
            float y = (normalizedPos.y - 0.5f) * windowSize.y;
            return windowMesh.transform.position + new Vector3(x, y, 0f);
        }

        private void ApplyHazardDirtToWindow(HazardInstance hazard)
        {
            Vector3 worldPos = CalculateWorldPosition(hazard.position);
            float radius = hazard.size * hazardScale * 0.5f;
            windowMesh.AddDirtArea(worldPos, radius, hazard.currentDirtiness * hazard.cleanDifficulty);
        }

        #endregion

        #region Hazard Cleaning

        public void CleanHazardArea(Vector3 worldPosition, float radius, float cleanPower)
        {
            foreach (HazardInstance hazard in activeHazards)
            {
                Vector3 hazardWorldPos = hazard.gameObject.transform.position;
                float distance = Vector2.Distance(
                    new Vector2(worldPosition.x, worldPosition.y),
                    new Vector2(hazardWorldPos.x, hazardWorldPos.y)
                );

                if (distance <= radius + hazard.size * hazardScale * 0.5f)
                {
                    // Clean this hazard
                    float falloff = 1f - Mathf.Clamp01(distance / radius);
                    float actualClean = cleanPower * falloff / hazard.cleanDifficulty;

                    hazard.currentDirtiness = Mathf.Max(0f, hazard.currentDirtiness - actualClean);

                    // Update visual
                    UpdateHazardVisual(hazard);

                    // If fully cleaned and not regenerating, remove
                    if (hazard.currentDirtiness <= 0.01f && !hazard.isRegenerating)
                    {
                        RemoveHazard(hazard);
                        return;
                    }
                }
            }
        }

        private void UpdateHazardVisual(HazardInstance hazard)
        {
            MeshRenderer renderer = hazard.gameObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                Color color = renderer.material.color;
                color.a = hazard.currentDirtiness;
                renderer.material.color = color;
            }
        }

        private void RemoveHazard(HazardInstance hazard)
        {
            // Play removal VFX
            if (VFXManager.Instance != null)
            {
                VFXManager.Instance.PlayPerfectClearEffect(hazard.gameObject.transform.position);
            }

            // Remove from list and destroy
            activeHazards.Remove(hazard);
            Destroy(hazard.gameObject);
        }

        #endregion

        #region Regeneration

        private void Update()
        {
            if (activeHazards.Count == 0) return;

            float deltaTime = Time.deltaTime;

            foreach (HazardInstance hazard in activeHazards)
            {
                if (hazard.isRegenerating && hazard.currentDirtiness < 1.0f)
                {
                    // Regenerate
                    hazard.currentDirtiness = Mathf.Min(1.0f, hazard.currentDirtiness + hazard.regenRate * deltaTime);

                    // Update visual
                    UpdateHazardVisual(hazard);

                    // Re-apply dirt to window
                    ApplyHazardDirtToWindow(hazard);

                    // Play regeneration VFX occasionally
                    if (enableRegenerationVFX && Random.value < 0.01f)
                    {
                        PlayRegenerationVFX(hazard);
                    }
                }
            }
        }

        private void PlayRegenerationVFX(HazardInstance hazard)
        {
            if (VFXManager.Instance == null) return;

            Vector3 pos = hazard.gameObject.transform.position;

            switch (hazard.type)
            {
                case HazardType.Frost:
                    VFXManager.Instance.PlayFrostCrackleEffect(pos);
                    break;
                case HazardType.NanoBots:
                    VFXManager.Instance.PlayNanoBotSparkEffect(pos);
                    break;
                case HazardType.Fog:
                    VFXManager.Instance.PlayFogDissipateEffect(pos);
                    break;
                case HazardType.Pollen:
                    VFXManager.Instance.PlayPollenBurstEffect(pos);
                    break;
            }
        }

        #endregion

        #region Cleanup

        public void ClearAllHazards()
        {
            foreach (HazardInstance hazard in activeHazards)
            {
                if (hazard.gameObject != null)
                {
                    Destroy(hazard.gameObject);
                }
            }

            activeHazards.Clear();
        }

        #endregion

        #region Helpers

        private Color GetFallbackColor(HazardType type)
        {
            switch (type)
            {
                case HazardType.BirdPoop: return Color.white;
                case HazardType.DeadFlies: return Color.black;
                case HazardType.Mud: return new Color(0.4f, 0.3f, 0.2f);
                case HazardType.OilStain: return new Color(0.1f, 0.1f, 0.1f);
                case HazardType.Spiderweb: return new Color(0.9f, 0.9f, 0.9f, 0.5f);
                case HazardType.Graffiti: return Color.magenta;
                case HazardType.Stickers: return Color.yellow;
                case HazardType.TapeResidue: return new Color(0.9f, 0.9f, 0.7f);
                case HazardType.WaterMarks: return new Color(0.7f, 0.7f, 0.8f, 0.5f);
                case HazardType.Rust: return new Color(0.6f, 0.3f, 0.1f);
                case HazardType.Gum: return Color.red;
                case HazardType.Paint: return Color.cyan;
                case HazardType.Ash: return new Color(0.3f, 0.3f, 0.3f);
                case HazardType.Mold: return new Color(0.2f, 0.5f, 0.2f);
                case HazardType.Scratches: return new Color(0.8f, 0.8f, 0.8f, 0.6f);
                case HazardType.Dust: return new Color(0.7f, 0.7f, 0.6f, 0.4f);
                case HazardType.Frost: return new Color(0.8f, 0.9f, 1.0f, 0.6f);
                case HazardType.Algae: return new Color(0.2f, 0.6f, 0.3f);
                case HazardType.TreeSap: return new Color(0.6f, 0.4f, 0.1f, 0.7f);
                case HazardType.Fog: return new Color(0.9f, 0.9f, 0.9f, 0.3f);
                case HazardType.NanoBots: return new Color(0.3f, 0.6f, 1.0f, 0.5f);
                case HazardType.Pollen: return new Color(1.0f, 0.9f, 0.3f, 0.4f);
                case HazardType.Condensation: return new Color(0.8f, 0.8f, 0.9f, 0.4f);
                case HazardType.Pollution: return new Color(0.4f, 0.4f, 0.4f, 0.5f);
                default: return Color.gray;
            }
        }

        public int GetActiveHazardCount()
        {
            return activeHazards.Count;
        }

        public float GetTotalHazardDirtiness()
        {
            float total = 0f;
            foreach (HazardInstance hazard in activeHazards)
            {
                total += hazard.currentDirtiness;
            }
            return activeHazards.Count > 0 ? total / activeHazards.Count : 0f;
        }

        #endregion

        #region Debug

        [ContextMenu("Test Spawn Random Hazards")]
        private void TestSpawnRandomHazards()
        {
            List<Hazard> testHazards = new List<Hazard>();

            for (int i = 0; i < 10; i++)
            {
                testHazards.Add(new Hazard
                {
                    type = (HazardType)Random.Range(0, 24),
                    position = new Vector2(Random.value, Random.value),
                    size = Random.Range(0.5f, 1.5f),
                    cleanDifficulty = Random.Range(0.5f, 2.0f),
                    regenRate = Random.value < 0.3f ? Random.Range(0.01f, 0.05f) : 0f
                });
            }

            SpawnHazards(testHazards);
        }

        #endregion
    }
}








