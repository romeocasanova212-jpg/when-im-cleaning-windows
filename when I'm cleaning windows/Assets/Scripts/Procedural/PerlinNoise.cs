using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace WhenImCleaningWindows.Procedural
{
    /// <summary>
    /// Burst-compiled Perlin Noise generator for suds base generation.
    /// Target: <10ms GPU / <20ms CPU on mid-tier devices (256x256 grid).
    /// Uses 7 octaves for organic suds patterns.
    /// </summary>
    public class PerlinNoise : MonoBehaviour
    {
        [Header("Generation Settings")]
        [SerializeField] private int gridSize = 256;
        [SerializeField] private int octaves = 7;
        [SerializeField] private float scale = 50f;
        [SerializeField] private float persistence = 0.5f;
        [SerializeField] private float lacunarity = 2f;
        
        private NativeArray<float> noiseMap;
        
        /// <summary>
        /// Generate Perlin noise map using Burst-compiled job system.
        /// </summary>
        public NativeArray<float> Generate(int seed = 0)
        {
            // Allocate native array for noise data
            noiseMap = new NativeArray<float>(gridSize * gridSize, Allocator.TempJob);
            
            // Create and schedule job
            var job = new PerlinNoiseJob
            {
                gridSize = gridSize,
                octaves = octaves,
                scale = scale,
                persistence = persistence,
                lacunarity = lacunarity,
                seed = seed,
                noiseMap = noiseMap
            };
            
            JobHandle handle = job.Schedule(gridSize * gridSize, 64);
            handle.Complete();
            
            return noiseMap;
        }
        
        /// <summary>
        /// Clean up native arrays.
        /// </summary>
        public void Dispose()
        {
            if (noiseMap.IsCreated)
            {
                noiseMap.Dispose();
            }
        }
        
        private void OnDestroy()
        {
            Dispose();
        }
    }
    
    /// <summary>
    /// Burst-compiled job for parallel Perlin noise generation.
    /// </summary>
    [BurstCompile]
    public struct PerlinNoiseJob : IJobParallelFor
    {
        [ReadOnly] public int gridSize;
        [ReadOnly] public int octaves;
        [ReadOnly] public float scale;
        [ReadOnly] public float persistence;
        [ReadOnly] public float lacunarity;
        [ReadOnly] public int seed;
        
        [WriteOnly] public NativeArray<float> noiseMap;
        
        public void Execute(int index)
        {
            int x = index % gridSize;
            int y = index / gridSize;
            
            float amplitude = 1f;
            float frequency = 1f;
            float noiseHeight = 0f;
            float maxValue = 0f;
            
            // Layer multiple octaves of Perlin noise
            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (x / scale) * frequency + seed;
                float sampleY = (y / scale) * frequency + seed;
                
                // Unity's noise function equivalent (using math.sin approximation)
                float perlinValue = GetPerlinValue(sampleX, sampleY);
                
                noiseHeight += perlinValue * amplitude;
                maxValue += amplitude;
                
                amplitude *= persistence;
                frequency *= lacunarity;
            }
            
            // Normalize to 0-1 range
            noiseMap[index] = noiseHeight / maxValue;
        }
        
        /// <summary>
        /// Custom Perlin-like noise function using math.sin for Burst compatibility.
        /// </summary>
        private float GetPerlinValue(float x, float y)
        {
            // Simple gradient noise approximation
            float2 p = new float2(x, y);
            float2 i = math.floor(p);
            float2 f = math.frac(p);
            
            // Smooth interpolation
            float2 u = f * f * (3.0f - 2.0f * f);
            
            // Hash-based gradient
            float a = Hash(i + new float2(0.0f, 0.0f));
            float b = Hash(i + new float2(1.0f, 0.0f));
            float c = Hash(i + new float2(0.0f, 1.0f));
            float d = Hash(i + new float2(1.0f, 1.0f));
            
            // Bilinear interpolation
            return math.lerp(math.lerp(a, b, u.x), math.lerp(c, d, u.x), u.y) * 2.0f - 1.0f;
        }
        
        /// <summary>
        /// Simple hash function for pseudo-random gradients.
        /// </summary>
        private float Hash(float2 p)
        {
            p = math.frac(p * new float2(123.34f, 456.21f));
            p += math.dot(p, p + 45.32f);
            return math.frac(p.x * p.y);
        }
    }
}








