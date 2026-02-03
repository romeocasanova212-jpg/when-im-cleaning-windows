using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace WhenImCleaningWindows.Procedural
{
    /// <summary>
    /// Cellular Automata system for regenerating hazards (frost, algae, sap, etc.).
    /// Rule: If sum of neighbors > 4, cell regenerates at 2.5%/sec when clean% < 80%.
    /// Burst-compiled for 4ms parallel performance on mid-tier devices.
    /// </summary>
    public class CellularAutomata : MonoBehaviour
    {
        [Header("CA Settings")]
        [SerializeField] private int gridSize = 256;
        [SerializeField] private float regenRate = 0.025f; // 2.5% per second
        [SerializeField] private int neighborThreshold = 4;
        [SerializeField] private float cleanPercentageThreshold = 80f;
        
        private NativeArray<float> currentGrid;
        private NativeArray<float> nextGrid;
        private float currentCleanPercentage = 100f;
        
        /// <summary>
        /// Initialize CA grid with initial hazard state.
        /// </summary>
        public void Initialize(NativeArray<float> initialState)
        {
            if (currentGrid.IsCreated) currentGrid.Dispose();
            if (nextGrid.IsCreated) nextGrid.Dispose();
            
            currentGrid = new NativeArray<float>(initialState, Allocator.Persistent);
            nextGrid = new NativeArray<float>(gridSize * gridSize, Allocator.Persistent);
        }
        
        /// <summary>
        /// Update CA state based on regeneration rules.
        /// </summary>
        public void UpdateStep(float deltaTime, float cleanPercentage)
        {
            if (!currentGrid.IsCreated) return;
            
            currentCleanPercentage = cleanPercentage;
            
            // Only regenerate when clean percentage < 80%
            if (cleanPercentage >= cleanPercentageThreshold)
            {
                return;
            }
            
            // Create and schedule CA job
            var job = new CellularAutomataJob
            {
                gridSize = gridSize,
                regenAmount = regenRate * deltaTime,
                neighborThreshold = neighborThreshold,
                currentGrid = currentGrid,
                nextGrid = nextGrid
            };
            
            JobHandle handle = job.Schedule(gridSize * gridSize, 64);
            handle.Complete();
            
            // Swap buffers
            var temp = currentGrid;
            currentGrid = nextGrid;
            nextGrid = temp;
        }
        
        /// <summary>
        /// Get current grid state.
        /// </summary>
        public NativeArray<float> GetCurrentGrid()
        {
            return currentGrid;
        }
        
        /// <summary>
        /// Set cell value (for player clearing hazards).
        /// </summary>
        public void SetCell(int x, int y, float value)
        {
            if (!currentGrid.IsCreated) return;
            if (x < 0 || x >= gridSize || y < 0 || y >= gridSize) return;
            
            int index = y * gridSize + x;
            currentGrid[index] = value;
        }
        
        /// <summary>
        /// Clean up native arrays.
        /// </summary>
        private void OnDestroy()
        {
            if (currentGrid.IsCreated) currentGrid.Dispose();
            if (nextGrid.IsCreated) nextGrid.Dispose();
        }
    }
    
    /// <summary>
    /// Burst-compiled job for parallel CA updates.
    /// </summary>
    [BurstCompile]
    public struct CellularAutomataJob : IJobParallelFor
    {
        [ReadOnly] public int gridSize;
        [ReadOnly] public float regenAmount;
        [ReadOnly] public int neighborThreshold;
        
        [ReadOnly] public NativeArray<float> currentGrid;
        [WriteOnly] public NativeArray<float> nextGrid;
        
        public void Execute(int index)
        {
            int x = index % gridSize;
            int y = index / gridSize;
            
            float currentValue = currentGrid[index];
            
            // Count neighbors with hazards (value > 0.5)
            int neighborSum = 0;
            
            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    if (dx == 0 && dy == 0) continue; // Skip self
                    
                    int nx = x + dx;
                    int ny = y + dy;
                    
                    // Check bounds
                    if (nx < 0 || nx >= gridSize || ny < 0 || ny >= gridSize) continue;
                    
                    int neighborIndex = ny * gridSize + nx;
                    if (currentGrid[neighborIndex] > 0.5f)
                    {
                        neighborSum++;
                    }
                }
            }
            
            // Apply regeneration rule: sum > threshold causes regen
            if (neighborSum > neighborThreshold)
            {
                // Regenerate at specified rate
                currentValue = math.min(1.0f, currentValue + regenAmount);
            }
            
            nextGrid[index] = currentValue;
        }
    }
}








