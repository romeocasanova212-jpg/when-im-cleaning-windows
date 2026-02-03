using UnityEngine;
using System.Collections.Generic;
using WhenImCleaningWindows.Procedural;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Gameplay
{
    /// <summary>
    /// Window Mesh Controller - Generates and manages the 3D window mesh for cleaning gameplay.
    /// Creates a subdivided quad mesh with vertex colors for dirt tracking.
    /// </summary>
    public class WindowMeshController : MonoBehaviour
    {
        public static event System.Action<float> OnCleanPercentageChanged;

        [Header("Window Configuration")]
        [SerializeField] private int gridResolution = 32; // 32x32 grid = 1024 vertices
        [SerializeField] private Vector2 windowSize = new Vector2(8f, 6f); // World space size
        [SerializeField] private Material windowMaterial;

        [Header("Dirt Tracking")]
        [SerializeField] private float initialDirtiness = 1.0f; // 100% dirty at start
        [SerializeField] private Color cleanColor = new Color(1f, 1f, 1f, 0f); // Transparent
        [SerializeField] private Color dirtyColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Dark opaque

        private Mesh windowMesh;
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Color[] vertexColors;
        private float[] vertexDirtiness; // 0.0 = clean, 1.0 = dirty
        private int vertexCount;
        private float lastReportedCleanPercentage = -1f;

        public float CurrentCleanPercentage { get; private set; }
        public Vector2 WindowSize => windowSize;

        #region Initialization

        private void Awake()
        {
            ApplyConfig();
            InitializeWindow();
        }

        private void ApplyConfig()
        {
            LevelConfig config = ConfigProvider.LevelConfig;
            if (config == null) return;

            gridResolution = Mathf.Clamp(config.meshSubdivisionsX, 8, 64);
            windowSize = new Vector2(config.windowWidth, config.windowHeight);
        }

        public void InitializeWindow()
        {
            Debug.Log("[WindowMesh] Initializing window mesh...");

            // Setup mesh components
            if (meshFilter == null)
            {
                meshFilter = gameObject.GetComponent<MeshFilter>();
                if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            if (meshRenderer == null)
            {
                meshRenderer = gameObject.GetComponent<MeshRenderer>();
                if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            // Generate mesh
            GenerateWindowMesh();

            // Apply material
            if (windowMaterial != null)
            {
                meshRenderer.material = windowMaterial;
            }
            else
            {
                // Create default material with vertex color support
                windowMaterial = new Material(Shader.Find("Standard"));
                windowMaterial.SetFloat("_Mode", 3); // Transparent mode
                windowMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                windowMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                windowMaterial.SetInt("_ZWrite", 0);
                windowMaterial.DisableKeyword("_ALPHATEST_ON");
                windowMaterial.EnableKeyword("_ALPHABLEND_ON");
                windowMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                windowMaterial.renderQueue = 3000;
                meshRenderer.material = windowMaterial;
            }

            // Initialize dirt
            SetInitialDirtiness(initialDirtiness);

            Debug.Log($"[WindowMesh] ✓ Window mesh initialized ({gridResolution}x{gridResolution}, {vertexCount} vertices)");
        }

        #endregion

        #region Mesh Generation

        private void GenerateWindowMesh()
        {
            windowMesh = new Mesh();
            windowMesh.name = "WindowMesh";

            vertexCount = (gridResolution + 1) * (gridResolution + 1);
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            vertexColors = new Color[vertexCount];
            vertexDirtiness = new float[vertexCount];

            // Generate vertices
            float xStep = windowSize.x / gridResolution;
            float yStep = windowSize.y / gridResolution;
            float xOffset = -windowSize.x * 0.5f;
            float yOffset = -windowSize.y * 0.5f;

            for (int y = 0; y <= gridResolution; y++)
            {
                for (int x = 0; x <= gridResolution; x++)
                {
                    int index = y * (gridResolution + 1) + x;

                    vertices[index] = new Vector3(
                        xOffset + x * xStep,
                        yOffset + y * yStep,
                        0f
                    );

                    uvs[index] = new Vector2(
                        (float)x / gridResolution,
                        (float)y / gridResolution
                    );

                    vertexColors[index] = Color.white;
                    vertexDirtiness[index] = 0f;
                }
            }

            // Generate triangles
            int[] triangles = new int[gridResolution * gridResolution * 6];
            int triIndex = 0;

            for (int y = 0; y < gridResolution; y++)
            {
                for (int x = 0; x < gridResolution; x++)
                {
                    int vertIndex = y * (gridResolution + 1) + x;

                    // Triangle 1
                    triangles[triIndex++] = vertIndex;
                    triangles[triIndex++] = vertIndex + gridResolution + 1;
                    triangles[triIndex++] = vertIndex + 1;

                    // Triangle 2
                    triangles[triIndex++] = vertIndex + 1;
                    triangles[triIndex++] = vertIndex + gridResolution + 1;
                    triangles[triIndex++] = vertIndex + gridResolution + 2;
                }
            }

            // Assign to mesh
            windowMesh.vertices = vertices;
            windowMesh.uv = uvs;
            windowMesh.triangles = triangles;
            windowMesh.colors = vertexColors;
            windowMesh.RecalculateNormals();
            windowMesh.RecalculateBounds();

            meshFilter.mesh = windowMesh;
        }

        #endregion

        #region Dirt Management

        public void SetInitialDirtiness(float dirtiness)
        {
            for (int i = 0; i < vertexCount; i++)
            {
                vertexDirtiness[i] = dirtiness;
                vertexColors[i] = Color.Lerp(cleanColor, dirtyColor, dirtiness);
            }

            UpdateMeshColors();
            CalculateCleanPercentage();
        }

        public void CleanArea(Vector3 worldPosition, float radius, float cleanAmount)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            // Find affected vertices
            Vector3[] vertices = windowMesh.vertices;
            bool anyChanged = false;

            for (int i = 0; i < vertexCount; i++)
            {
                float distance = Vector2.Distance(
                    new Vector2(vertices[i].x, vertices[i].y),
                    new Vector2(localPos.x, localPos.y)
                );

                if (distance <= radius)
                {
                    // Falloff based on distance
                    float falloff = 1f - (distance / radius);
                    float actualClean = cleanAmount * falloff;

                    vertexDirtiness[i] = Mathf.Max(0f, vertexDirtiness[i] - actualClean);
                    vertexColors[i] = Color.Lerp(cleanColor, dirtyColor, vertexDirtiness[i]);
                    anyChanged = true;
                }
            }

            if (anyChanged)
            {
                UpdateMeshColors();
                CalculateCleanPercentage();
            }
        }

        public void AddDirtArea(Vector3 worldPosition, float radius, float dirtAmount)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            Vector3[] vertices = windowMesh.vertices;
            bool anyChanged = false;

            for (int i = 0; i < vertexCount; i++)
            {
                float distance = Vector2.Distance(
                    new Vector2(vertices[i].x, vertices[i].y),
                    new Vector2(localPos.x, localPos.y)
                );

                if (distance <= radius)
                {
                    float falloff = 1f - (distance / radius);
                    float actualDirt = dirtAmount * falloff;

                    vertexDirtiness[i] = Mathf.Min(1f, vertexDirtiness[i] + actualDirt);
                    vertexColors[i] = Color.Lerp(cleanColor, dirtyColor, vertexDirtiness[i]);
                    anyChanged = true;
                }
            }

            if (anyChanged)
            {
                UpdateMeshColors();
                CalculateCleanPercentage();
            }
        }

        private void UpdateMeshColors()
        {
            windowMesh.colors = vertexColors;
        }

        private void CalculateCleanPercentage()
        {
            float totalDirt = 0f;

            for (int i = 0; i < vertexCount; i++)
            {
                totalDirt += vertexDirtiness[i];
            }

            float averageDirt = totalDirt / vertexCount;
            CurrentCleanPercentage = (1f - averageDirt) * 100f;

            if (Mathf.Abs(CurrentCleanPercentage - lastReportedCleanPercentage) > 0.05f)
            {
                lastReportedCleanPercentage = CurrentCleanPercentage;
                OnCleanPercentageChanged?.Invoke(CurrentCleanPercentage);
            }
        }

        #endregion

        #region Queries

        public float GetDirtinessAt(Vector3 worldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);
            return SampleDirtinessAtLocal(localPos);
        }

        private float SampleDirtinessAtLocal(Vector3 localPos)
        {
            // Find nearest vertex
            Vector3[] vertices = windowMesh.vertices;
            float minDistance = float.MaxValue;
            int nearestIndex = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                float distance = Vector2.Distance(
                    new Vector2(vertices[i].x, vertices[i].y),
                    new Vector2(localPos.x, localPos.y)
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = i;
                }
            }

            return vertexDirtiness[nearestIndex];
        }

        public bool IsPointOnWindow(Vector3 worldPosition)
        {
            Vector3 localPos = transform.InverseTransformPoint(worldPosition);

            return localPos.x >= -windowSize.x * 0.5f &&
                   localPos.x <= windowSize.x * 0.5f &&
                   localPos.y >= -windowSize.y * 0.5f &&
                   localPos.y <= windowSize.y * 0.5f;
        }

        #endregion

        #region Public API

        public void ResetWindow()
        {
            SetInitialDirtiness(initialDirtiness);
        }

        public void SetWindowSize(Vector2 newSize)
        {
            windowSize = newSize;
            GenerateWindowMesh();
            SetInitialDirtiness(initialDirtiness);
        }

        public void SetGridResolution(int resolution)
        {
            gridResolution = Mathf.Clamp(resolution, 8, 128);
            GenerateWindowMesh();
            SetInitialDirtiness(initialDirtiness);
        }

        #endregion

        #region Debug

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            // Draw window bounds
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            Gizmos.DrawWireCube(center, new Vector3(windowSize.x, windowSize.y, 0.1f));

            // Show clean percentage
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * (windowSize.y * 0.5f + 0.5f), 
                $"Clean: {CurrentCleanPercentage:F1}%");
            #endif
        }

        [ContextMenu("Test Clean Center")]
        private void TestCleanCenter()
        {
            CleanArea(transform.position, 1f, 0.5f);
        }

        [ContextMenu("Test Add Dirt Center")]
        private void TestAddDirtCenter()
        {
            AddDirtArea(transform.position, 1f, 0.3f);
        }

        [ContextMenu("Reset Window")]
        private void TestResetWindow()
        {
            ResetWindow();
        }

        #endregion
    }
}








