using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace WhenImCleaningWindows.Procedural
{
    /// <summary>
    /// Fast Poisson Disk Sampling for hazard placement using O(1) spatial grid.
    /// Based on Robert Bridson's algorithm (2007).
    /// Target: <0.5ms performance (10x faster than naive approaches).
    /// </summary>
    public class PoissonDiskSampling : MonoBehaviour
    {
        [Header("Sampling Settings")]
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private int rejectionSamples = 20;
        [SerializeField] private Vector2 sampleRegionSize = new Vector2(256, 256);
        
        /// <summary>
        /// Generate Poisson disk samples for hazard placement.
        /// </summary>
        public List<Vector2> GenerateSamples(int seed = 0)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)(seed == 0 ? 1 : seed));
            
            float cellSize = minDistance / math.sqrt(2);
            int gridWidth = Mathf.CeilToInt(sampleRegionSize.x / cellSize);
            int gridHeight = Mathf.CeilToInt(sampleRegionSize.y / cellSize);
            
            // Grid to store sample indices (O(1) lookup)
            int[,] grid = new int[gridWidth, gridHeight];
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    grid[x, y] = -1;
                }
            }
            
            List<Vector2> points = new List<Vector2>();
            List<Vector2> spawnPoints = new List<Vector2>();
            
            // Start with random initial point
            Vector2 initialPoint = new Vector2(
                random.NextFloat(0, sampleRegionSize.x),
                random.NextFloat(0, sampleRegionSize.y)
            );
            
            spawnPoints.Add(initialPoint);
            points.Add(initialPoint);
            
            int gridX = (int)(initialPoint.x / cellSize);
            int gridY = (int)(initialPoint.y / cellSize);
            grid[gridX, gridY] = 0;
            
            // Generate samples using active list
            while (spawnPoints.Count > 0)
            {
                int spawnIndex = random.NextInt(0, spawnPoints.Count);
                Vector2 spawnCenter = spawnPoints[spawnIndex];
                bool candidateAccepted = false;
                
                // Try rejection samples
                for (int i = 0; i < rejectionSamples; i++)
                {
                    float angle = random.NextFloat(0, math.PI * 2);
                    float radius = random.NextFloat(minDistance, 2 * minDistance);
                    
                    Vector2 candidate = spawnCenter + new Vector2(
                        math.cos(angle) * radius,
                        math.sin(angle) * radius
                    );
                    
                    if (IsValid(candidate, sampleRegionSize, cellSize, minDistance, points, grid, gridWidth, gridHeight))
                    {
                        points.Add(candidate);
                        spawnPoints.Add(candidate);
                        
                        int cX = (int)(candidate.x / cellSize);
                        int cY = (int)(candidate.y / cellSize);
                        grid[cX, cY] = points.Count - 1;
                        
                        candidateAccepted = true;
                        break;
                    }
                }
                
                if (!candidateAccepted)
                {
                    spawnPoints.RemoveAt(spawnIndex);
                }
            }
            
            return points;
        }
        
        /// <summary>
        /// Check if candidate point is valid (not too close to existing points).
        /// </summary>
        private bool IsValid(Vector2 candidate, Vector2 regionSize, float cellSize, float minDist,
            List<Vector2> points, int[,] grid, int gridWidth, int gridHeight)
        {
            // Check bounds
            if (candidate.x < 0 || candidate.x >= regionSize.x || candidate.y < 0 || candidate.y >= regionSize.y)
            {
                return false;
            }
            
            // Get grid cell
            int cellX = (int)(candidate.x / cellSize);
            int cellY = (int)(candidate.y / cellSize);
            
            // Check neighboring cells (O(1) spatial lookup)
            int searchStartX = Mathf.Max(0, cellX - 2);
            int searchEndX = Mathf.Min(cellX + 2, gridWidth - 1);
            int searchStartY = Mathf.Max(0, cellY - 2);
            int searchEndY = Mathf.Min(cellY + 2, gridHeight - 1);
            
            for (int x = searchStartX; x <= searchEndX; x++)
            {
                for (int y = searchStartY; y <= searchEndY; y++)
                {
                    int pointIndex = grid[x, y];
                    if (pointIndex != -1)
                    {
                        float sqrDist = (candidate - points[pointIndex]).sqrMagnitude;
                        if (sqrDist < minDist * minDist)
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// Visualize samples in Scene view for debugging.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            
            Gizmos.color = Color.red;
            var samples = GenerateSamples(12345);
            
            foreach (var sample in samples)
            {
                Gizmos.DrawSphere(new Vector3(sample.x, sample.y, 0), 2f);
            }
        }
    }
}








