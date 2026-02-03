using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using System.Collections.Generic;

namespace WhenImCleaningWindows.Procedural
{
    /// <summary>
    /// Post-generation validation bot that tests if level is solvable.
    /// Uses greedy pathfinding to ensure 95% clean threshold is achievable.
    /// Target: 18ms pathfinding performance.
    /// </summary>
    public class AISolvabilityBot : MonoBehaviour
    {
        [Header("Validation Settings")]
        [SerializeField] private float targetCleanPercentage = 95f;
        [SerializeField] private float timeLimit = 90f; // Level time limit in seconds
        [SerializeField] private float clearRadius = 10f; // Simulated clear radius per action
        [SerializeField] private int maxIterations = 500;
        
        private int gridSize = 256;
        
        /// <summary>
        /// Test if level is solvable within time limit.
        /// Returns true if bot can reach 95% clean threshold.
        /// </summary>
        public bool ValidateLevel(NativeArray<float> levelGrid, int size)
        {
            gridSize = size;
            float startTime = Time.realtimeSinceStartup;
            
            // Create working copy
            NativeArray<float> workingGrid = new NativeArray<float>(levelGrid, Allocator.Temp);
            
            // Run greedy clearing simulation
            int iterations = 0;
            float cleanPercentage = CalculateCleanPercentage(workingGrid);
            
            while (cleanPercentage < targetCleanPercentage && iterations < maxIterations)
            {
                // Find dirtiest cluster using greedy search
                Vector2Int targetCell = FindDirtiestCluster(workingGrid);
                
                if (targetCell.x == -1)
                {
                    break; // No more dirty cells
                }
                
                // Simulate clearing action
                ClearArea(workingGrid, targetCell.x, targetCell.y, clearRadius);
                
                cleanPercentage = CalculateCleanPercentage(workingGrid);
                iterations++;
                
                // Check time limit (simulated)
                float elapsedTime = (Time.realtimeSinceStartup - startTime) * 1000f; // ms
                if (elapsedTime > 20f) // 20ms budget
                {
                    Debug.LogWarning("AI Solvability check exceeded time budget!");
                    break;
                }
            }
            
            workingGrid.Dispose();
            
            bool isSolvable = cleanPercentage >= targetCleanPercentage;
            
            if (!isSolvable)
            {
                Debug.LogWarning($"Level FAILED solvability check: {cleanPercentage:F1}% achievable (target: {targetCleanPercentage}%)");
            }
            
            return isSolvable;
        }
        
        /// <summary>
        /// Calculate percentage of clean cells in grid.
        /// </summary>
        private float CalculateCleanPercentage(NativeArray<float> grid)
        {
            int totalCells = grid.Length;
            int cleanCells = 0;
            
            for (int i = 0; i < totalCells; i++)
            {
                if (grid[i] < 0.1f) // Consider < 0.1 as "clean"
                {
                    cleanCells++;
                }
            }
            
            return (cleanCells / (float)totalCells) * 100f;
        }
        
        /// <summary>
        /// Find the dirtiest cluster using greedy search.
        /// Returns cell coordinates with highest density of dirt.
        /// </summary>
        private Vector2Int FindDirtiestCluster(NativeArray<float> grid)
        {
            float maxDirtiness = 0f;
            Vector2Int dirtiestCell = new Vector2Int(-1, -1);
            
            // Sample grid at intervals (optimization)
            int sampleStep = Mathf.Max(1, gridSize / 32); // Sample every N cells
            
            for (int y = 0; y < gridSize; y += sampleStep)
            {
                for (int x = 0; x < gridSize; x += sampleStep)
                {
                    int index = y * gridSize + x;
                    float localDirtiness = CalculateLocalDirtiness(grid, x, y);
                    
                    if (localDirtiness > maxDirtiness)
                    {
                        maxDirtiness = localDirtiness;
                        dirtiestCell = new Vector2Int(x, y);
                    }
                }
            }
            
            return dirtiestCell;
        }
        
        /// <summary>
        /// Calculate dirtiness in local area around cell.
        /// </summary>
        private float CalculateLocalDirtiness(NativeArray<float> grid, int centerX, int centerY)
        {
            float totalDirt = 0f;
            int sampleRadius = 5;
            int sampleCount = 0;
            
            for (int dy = -sampleRadius; dy <= sampleRadius; dy++)
            {
                for (int dx = -sampleRadius; dx <= sampleRadius; dx++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    
                    if (x < 0 || x >= gridSize || y < 0 || y >= gridSize) continue;
                    
                    int index = y * gridSize + x;
                    totalDirt += grid[index];
                    sampleCount++;
                }
            }
            
            return sampleCount > 0 ? totalDirt / sampleCount : 0f;
        }
        
        /// <summary>
        /// Simulate clearing area around target cell.
        /// </summary>
        private void ClearArea(NativeArray<float> grid, int centerX, int centerY, float radius)
        {
            int radiusInt = Mathf.CeilToInt(radius);
            
            for (int dy = -radiusInt; dy <= radiusInt; dy++)
            {
                for (int dx = -radiusInt; dx <= radiusInt; dx++)
                {
                    int x = centerX + dx;
                    int y = centerY + dy;
                    
                    if (x < 0 || x >= gridSize || y < 0 || y >= gridSize) continue;
                    
                    float distance = math.sqrt(dx * dx + dy * dy);
                    if (distance <= radius)
                    {
                        int index = y * gridSize + x;
                        
                        // Simulate clearing (falloff from center)
                        float clearAmount = 1f - (distance / radius);
                        grid[index] = math.max(0f, grid[index] - clearAmount);
                    }
                }
            }
        }
        
        /// <summary>
        /// Visualize solvability check results.
        /// </summary>
        public void VisualizeValidation(NativeArray<float> grid)
        {
            // This would be implemented to show debug visualization in editor
            Debug.Log($"Validation visualization for {gridSize}x{gridSize} grid");
        }
    }
}








