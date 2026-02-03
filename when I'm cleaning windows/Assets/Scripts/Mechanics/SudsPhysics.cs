using UnityEngine;
using Unity.Mathematics;

namespace WhenImCleaningWindows.Mechanics
{
    /// <summary>
    /// PhysX-based deformable suds physics system.
    /// Simulates realistic suds behavior with clearing, spreading, and dripping.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class SudsPhysics : MonoBehaviour
    {
        [Header("Physics Settings")]
        [SerializeField] private int gridResolution = 256;
        [SerializeField] private float viscosity = 0.8f;
        [SerializeField] private float spreadRate = 0.1f;
        [SerializeField] private float dripGravity = 0.05f;
        [SerializeField] private float evaporationRate = 0.001f;
        
        [Header("Visual Settings")]
        [SerializeField] private Material sudsMaterial;
        [SerializeField] private float heightScale = 1f;
        
        private float[,] sudsGrid;
        private float[,] velocityGrid;
        private Mesh sudsMesh;
        private Vector3[] vertices;
        private int[] triangles;
        private Vector2[] uvs;
        
        private void Awake()
        {
            InitializeGrid();
            GenerateMesh();
        }
        
        /// <summary>
        /// Initialize suds simulation grid.
        /// </summary>
        private void InitializeGrid()
        {
            sudsGrid = new float[gridResolution, gridResolution];
            velocityGrid = new float[gridResolution, gridResolution];
            
            // Initialize with zero suds
            for (int y = 0; y < gridResolution; y++)
            {
                for (int x = 0; x < gridResolution; x++)
                {
                    sudsGrid[x, y] = 0f;
                    velocityGrid[x, y] = 0f;
                }
            }
        }
        
        /// <summary>
        /// Generate mesh for suds visualization.
        /// </summary>
        private void GenerateMesh()
        {
            sudsMesh = new Mesh();
            GetComponent<MeshFilter>().mesh = sudsMesh;
            
            // Create vertices
            vertices = new Vector3[(gridResolution + 1) * (gridResolution + 1)];
            uvs = new Vector2[vertices.Length];
            
            float cellSize = 1f / gridResolution;
            
            for (int y = 0; y <= gridResolution; y++)
            {
                for (int x = 0; x <= gridResolution; x++)
                {
                    int index = y * (gridResolution + 1) + x;
                    vertices[index] = new Vector3(x * cellSize, y * cellSize, 0);
                    uvs[index] = new Vector2(x / (float)gridResolution, y / (float)gridResolution);
                }
            }
            
            // Create triangles
            triangles = new int[gridResolution * gridResolution * 6];
            int triIndex = 0;
            
            for (int y = 0; y < gridResolution; y++)
            {
                for (int x = 0; x < gridResolution; x++)
                {
                    int vertIndex = y * (gridResolution + 1) + x;
                    
                    triangles[triIndex++] = vertIndex;
                    triangles[triIndex++] = vertIndex + gridResolution + 1;
                    triangles[triIndex++] = vertIndex + 1;
                    
                    triangles[triIndex++] = vertIndex + 1;
                    triangles[triIndex++] = vertIndex + gridResolution + 1;
                    triangles[triIndex++] = vertIndex + gridResolution + 2;
                }
            }
            
            sudsMesh.vertices = vertices;
            sudsMesh.triangles = triangles;
            sudsMesh.uv = uvs;
            sudsMesh.RecalculateNormals();
            
            if (sudsMaterial != null)
            {
                GetComponent<MeshRenderer>().material = sudsMaterial;
            }
        }
        
        /// <summary>
        /// Update suds physics simulation.
        /// </summary>
        private void Update()
        {
            UpdatePhysics(Time.deltaTime);
            UpdateMesh();
        }
        
        /// <summary>
        /// Update suds physics (spreading, dripping, evaporation).
        /// </summary>
        private void UpdatePhysics(float deltaTime)
        {
            // Temporary array for next frame
            float[,] nextSudsGrid = new float[gridResolution, gridResolution];
            
            for (int y = 0; y < gridResolution; y++)
            {
                for (int x = 0; x < gridResolution; x++)
                {
                    float currentSuds = sudsGrid[x, y];
                    
                    if (currentSuds < 0.01f)
                    {
                        nextSudsGrid[x, y] = 0f;
                        continue;
                    }
                    
                    // Evaporation
                    currentSuds -= evaporationRate * deltaTime;
                    
                    // Spreading to neighbors
                    float spreadAmount = currentSuds * spreadRate * deltaTime;
                    int neighborCount = 0;
                    
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            
                            int nx = x + dx;
                            int ny = y + dy;
                            
                            if (nx >= 0 && nx < gridResolution && ny >= 0 && ny < gridResolution)
                            {
                                neighborCount++;
                            }
                        }
                    }
                    
                    if (neighborCount > 0)
                    {
                        float spreadPerNeighbor = spreadAmount / neighborCount;
                        
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                
                                int nx = x + dx;
                                int ny = y + dy;
                                
                                if (nx >= 0 && nx < gridResolution && ny >= 0 && ny < gridResolution)
                                {
                                    nextSudsGrid[nx, ny] += spreadPerNeighbor;
                                }
                            }
                        }
                        
                        currentSuds -= spreadAmount;
                    }
                    
                    // Gravity drip (downward bias)
                    if (y > 0)
                    {
                        float dripAmount = currentSuds * dripGravity * deltaTime;
                        nextSudsGrid[x, y - 1] += dripAmount;
                        currentSuds -= dripAmount;
                    }
                    
                    nextSudsGrid[x, y] += currentSuds;
                }
            }
            
            // Apply viscosity smoothing
            for (int y = 0; y < gridResolution; y++)
            {
                for (int x = 0; x < gridResolution; x++)
                {
                    sudsGrid[x, y] = Mathf.Lerp(sudsGrid[x, y], nextSudsGrid[x, y], 1f - viscosity);
                }
            }
        }
        
        /// <summary>
        /// Update mesh vertices based on suds height.
        /// </summary>
        private void UpdateMesh()
        {
            for (int y = 0; y <= gridResolution; y++)
            {
                for (int x = 0; x <= gridResolution; x++)
                {
                    int index = y * (gridResolution + 1) + x;
                    
                    // Sample suds height (clamp to grid bounds)
                    int gridX = Mathf.Min(x, gridResolution - 1);
                    int gridY = Mathf.Min(y, gridResolution - 1);
                    float height = sudsGrid[gridX, gridY] * heightScale;
                    
                    vertices[index].z = height;
                }
            }
            
            sudsMesh.vertices = vertices;
            sudsMesh.RecalculateNormals();
        }
        
        /// <summary>
        /// Clear suds at specific position (player interaction).
        /// </summary>
        public void ClearSuds(Vector2 worldPosition, float radius, float amount)
        {
            int centerX = Mathf.RoundToInt(worldPosition.x * gridResolution);
            int centerY = Mathf.RoundToInt(worldPosition.y * gridResolution);
            int radiusInt = Mathf.CeilToInt(radius * gridResolution);
            
            for (int dy = -radiusInt; dy <= radiusInt; dy++)
            {
                for (int dx = -radiusInt; dx <= radiusInt; dx++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    
                    if (x < 0 || x >= gridResolution || y < 0 || y >= gridResolution) continue;
                    
                    float distance = math.sqrt(dx * dx + dy * dy);
                    if (distance <= radiusInt)
                    {
                        float falloff = 1f - (distance / radiusInt);
                        sudsGrid[x, y] = math.max(0f, sudsGrid[x, y] - amount * falloff);
                    }
                }
            }
        }
        
        /// <summary>
        /// Add suds at specific position (spray action).
        /// </summary>
        public void AddSuds(Vector2 worldPosition, float radius, float amount)
        {
            int centerX = Mathf.RoundToInt(worldPosition.x * gridResolution);
            int centerY = Mathf.RoundToInt(worldPosition.y * gridResolution);
            int radiusInt = Mathf.CeilToInt(radius * gridResolution);
            
            for (int dy = -radiusInt; dy <= radiusInt; dy++)
            {
                for (int dx = -radiusInt; dx <= radiusInt; dx++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    
                    if (x < 0 || x >= gridResolution || y < 0 || y >= gridResolution) continue;
                    
                    float distance = math.sqrt(dx * dx + dy * dy);
                    if (distance <= radiusInt)
                    {
                        float falloff = 1f - (distance / radiusInt);
                        sudsGrid[x, y] = math.min(1f, sudsGrid[x, y] + amount * falloff);
                    }
                }
            }
        }
        
        /// <summary>
        /// Get current suds amount at position.
        /// </summary>
        public float GetSudsAt(Vector2 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.x * gridResolution);
            int y = Mathf.RoundToInt(worldPosition.y * gridResolution);
            
            if (x < 0 || x >= gridResolution || y < 0 || y >= gridResolution) return 0f;
            
            return sudsGrid[x, y];
        }
    }
}








