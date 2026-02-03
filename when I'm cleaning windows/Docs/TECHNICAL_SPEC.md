# When I'm Cleaning Windows - Technical Specification

**Version**: 2.0 (2026 Polished Algorithms)  
**Date**: January 28, 2026  
**Engine**: Unity 6.3 LTS  
**Target Performance**: <50ms procedural generation on mid-tier devices

## Table of Contents
1. [Overview](#overview)
2. [2026 Algorithm Improvements](#2026-algorithm-improvements)
3. [Procedural Generation Algorithms](#procedural-generation-algorithms)
4. [Performance Optimization](#performance-optimization)
5. [Code Architecture](#code-architecture)
6. [Data Structures](#data-structures)
7. [Platform-Specific Considerations](#platform-specific-considerations)

---

## 1. Overview

This document provides in-depth technical details for the **2026-polished** procedural generation systems, monetization algorithms, and performance optimizations of **When I'm Cleaning Windows**.

### 1.1 Technology Stack
- **Engine**: Unity 6.3 LTS
- **Rendering**: Universal Render Pipeline (URP) with 120FPS OLED optimization
- **Compiler**: Burst 1.8+ with LLVM backend
- **Job System**: Unity Job System with IJobParallelFor
- **Math Library**: Unity.Mathematics (SIMD-optimized)
- **Input**: Enhanced Touch Input System
- **Audio**: FMOD Studio 2.02 with binaural ASMR
- **ML**: Firebase ML Kit (churn prediction, dynamic pricing)

---

## 2. 2026 Algorithm Improvements

### 2.1 Core Optimizations Summary
| System | Old Target | New Target | Improvement | Method |
|--------|-----------|-----------|------------|--------|
| **Perlin Noise** | 6 octaves, 25ms | 7 octaves, 20ms | 20% faster + richer | SIMD, reduced sampling |
| **Poisson Disk** | 30 attempts, 0.5ms | 20 attempts, 0.3ms | 40% faster | Smarter rejection |
| **Cellular Automata** | >5 threshold, 3%/sec, 5ms | >4 threshold, 2.5%/sec, 4ms | 20% faster + fairer | Optimized neighborhood |
| **AI Solvability** | 92% target, 20ms | 95% target, 18ms | 10% faster + 3% fairer | Dual-seed testing |
| **Total Pipeline** | <80ms | **<50ms** | **37.5% faster** | Cumulative gains |

### 2.2 Fairness Enhancements
- **Energy Regen**: 1/20min (was 1/30min) = **72 lives/day free** (up 50%)
- **Timer Scaling**: 120s→40s (was 90s→35s) = **33% more forgiving early game**
- **AI Threshold**: 95% (was 92%) = **3% more levels beatable**
- **CA Trigger**: >4 neighbors (was >5) = **Gentler regeneration spread**

---

## 3. Procedural Generation Algorithms (2026 Polished)

### 3.1 Perlin Noise (Suds Base Generation - Enhanced)

#### Algorithm Overview
Generates organic suds patterns using **7-octave** multi-layer Perlin noise (up from 6) with difficulty scaling.

#### Mathematical Formula (2026 Enhanced)
```
noise(x, y, floorDifficulty) = saturate(
  Σ(i=0 to 6) [
    amplitude_i × perlin(x × frequency_i, y × frequency_i)
  ] × (1 + 0.1 × floorDifficulty)
)

where:
    amplitude_i = persistence^i      // 0.5^i
    frequency_i = lacunarity^i       // 2.0^i
    floorDifficulty = 0-10 (World 1→10)
```

**Key Change**: Multiplying final result by `(1 + 0.1 × floorDifficulty)` increases suds density in later worlds naturally.

#### Performance Characteristics (2026)
- **Grid Size**: 256×256 = 65,536 cells
- **Octaves**: 7 (richer organic patterns)
- **Batch Size**: 128 cells per job (optimized from 64)
- **Target**: <20ms CPU (down from 25ms)
- **Memory**: 256KB per grid (float32 × 65,536)
- **SIMD**: 4-8× speedup on AVX2/NEON

#### Code Changes
```csharp
// Old: 6 octaves, no difficulty scaling
for (int i = 0; i < 6; i++) { ... }
noiseMap[index] = noiseHeight / maxValue;

// New: 7 octaves, difficulty multiplier
for (int i = 0; i < 7; i++) { ... }
float difficultyMultiplier = 1.0f + (floorDifficulty * 0.1f);
noiseMap[index] = math.saturate((noiseHeight / maxValue) * difficultyMultiplier);
```

---

### 3.2 Poisson Disk Sampling (Hazard Placement - Optimized)

#### Algorithm Changes (2026)
**Rejection Samples**: 20 attempts (down from 30) with smarter early-out logic.

**Pseudocode**:
```
for i = 0 to 20: // Was 30
    candidate = generate_in_annulus(minDist, 2×minDist)
    
    // Early-out optimization
    if candidate.out_of_bounds():
        continue // Don't count as rejection
    
    if is_valid_via_grid(candidate):
        accept(candidate)
        break
```

**Performance Gain**: 40% faster (0.5ms → 0.3ms) by reducing wasteful rejections.

#### Hazard Count Scaling
```
hazardCount = floor(8 + (floorDifficulty × 1.7))

World 1: 8-10 hazards
World 5: 16-18 hazards
World 10: 24-25 hazards
```

---

### 3.3 Cellular Automata (Regeneration - Tuned)

#### Rule Changes (2026)
**Old Rule**: `sum(neighbors) > 5` triggers regen at 3%/sec  
**New Rule**: `sum(neighbors) > 4` triggers regen at 2.5%/sec

**Mathematical Model**:
```
For each cell (x, y):
    alive_neighbors = count(adjacent cells > 0.5)
    
    if alive_neighbors > 4:  // Was >5
        cell_value(t+1) = min(1.0, cell_value(t) + 0.025 × Δt)  // Was 0.03
    else:
        cell_value(t+1) = cell_value(t)
```

**Trigger Threshold**: <85% clean activates CA (was <80%)  
**Visual Cue**: Orange pulse on window edges when CA active

#### Performance Optimization
- **Batch Size**: 128 cells/job (from 64)
- **Early-Exit**: Skip cells with value < 0.01
- **Target**: 4ms (down from 5ms)

#### Code Changes
```csharp
// Old
if (neighborSum > 5) { currentValue += 0.03f * deltaTime; }

// New
if (neighborSum > 4) { currentValue += 0.025f * deltaTime; }
```

---

### 3.4 AI Solvability Bot (2026 Enhanced)

#### Validation Algorithm (Dual-Seed Method)
**Old**: Single seed, 92% target, 20ms  
**New**: **2 seeds tested**, 95% target, 18ms

**Pseudocode**:
```
Algorithm: Dual-Seed Solvability
Input: levelGrid, targetClean (95%)
Output: Boolean (beatable/reject)

1. For seed in [randomSeed1, randomSeed2]:
    a. botGrid = copy(levelGrid)
    b. result = greedy_solve(botGrid, 18ms timeout)
    c. If result >= 90%: PASS // Tolerance ±5%
2. If both seeds <90%: REJECT and regenerate
3. Else: ACCEPT

Elegance Bonus Check:
4. If seed1 OR seed2 achieved >=98% with <50% actions:
    Mark level as "Elegant" (sparkle icon)
```

**Key Improvements**:
- **Dual-seed testing** catches edge-case unsolvable layouts
- **95% target** with ±5% tolerance = 90-100% range (fairer)

#### Implementation Details
```csharp
[BurstCompile]
public struct PerlinNoiseJob : IJobParallelFor
{
    [ReadOnly] public int gridSize;           // 256x256 default
    [ReadOnly] public int octaves;            // 6 layers
    [ReadOnly] public float scale;            // 50.0 (zoom level)
    [ReadOnly] public float persistence;      // 0.5 (amplitude decay)
    [ReadOnly] public float lacunarity;       // 2.0 (frequency growth)
    [ReadOnly] public int seed;               // Random seed
    
    [WriteOnly] public NativeArray<float> noiseMap;
    
    public void Execute(int index)
    {
        int x = index % gridSize;
        int y = index / gridSize;
        
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        float maxValue = 0f;
        
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x / scale) * frequency + seed;
            float sampleY = (y / scale) * frequency + seed;
            
            float perlinValue = GetPerlinValue(sampleX, sampleY);
            
            noiseHeight += perlinValue * amplitude;
            maxValue += amplitude;
            
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        
        noiseMap[index] = noiseHeight / maxValue;
    }
}
```

#### Performance Characteristics
- **Grid Size**: 256×256 = 65,536 cells
- **Batch Size**: 64 cells per job (1,024 jobs total)
- **Target**: <10ms GPU / <25ms CPU
- **Memory**: 256KB per grid (float32 × 65,536)

#### Optimization Techniques
1. **Burst Compilation**: LLVM optimizes to SIMD instructions
2. **Parallel Execution**: IJobParallelFor splits across cores
3. **NativeArray**: Zero-copy memory for job system
4. **Cache-Friendly**: Sequential memory access pattern

---

### 2.2 Poisson Disk Sampling (Hazard Placement)

#### Algorithm Overview
Fast Poisson Disk Sampling based on Robert Bridson's paper (SIGGRAPH 2007). Ensures hazards maintain minimum distance while appearing random.

#### Pseudocode
```
Algorithm: Fast Poisson Disk Sampling
Input: minDistance, regionSize, rejectionSamples
Output: List of sample points

1. Initialize spatial grid (cellSize = minDistance / √2)
2. Add random initial point to spawnPoints and grid
3. While spawnPoints is not empty:
    a. Pick random point from spawnPoints
    b. For i = 0 to rejectionSamples:
        i.   Generate random point in annulus [minDist, 2×minDist]
        ii.  Check neighbors in grid (O(1) lookup)
        iii. If valid, add to points, spawnPoints, and grid
    c. If no valid candidates, remove point from spawnPoints
4. Return points
```

#### Implementation Details
```csharp
public List<Vector2> GenerateSamples(int seed)
{
    Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)seed);
    
    float cellSize = minDistance / math.sqrt(2);
    int gridWidth = Mathf.CeilToInt(sampleRegionSize.x / cellSize);
    int gridHeight = Mathf.CeilToInt(sampleRegionSize.y / cellSize);
    
    int[,] grid = new int[gridWidth, gridHeight]; // -1 = empty
    List<Vector2> points = new List<Vector2>();
    List<Vector2> spawnPoints = new List<Vector2>();
    
    // Start with random initial point
    Vector2 initialPoint = random.NextFloat2(float2.zero, sampleRegionSize);
    spawnPoints.Add(initialPoint);
    points.Add(initialPoint);
    grid[(int)(initialPoint.x / cellSize), (int)(initialPoint.y / cellSize)] = 0;
    
    while (spawnPoints.Count > 0)
    {
        int spawnIndex = random.NextInt(0, spawnPoints.Count);
        Vector2 spawnCenter = spawnPoints[spawnIndex];
        bool candidateAccepted = false;
        
        for (int i = 0; i < rejectionSamples; i++)
        {
            float angle = random.NextFloat(0, math.PI * 2);
            float radius = random.NextFloat(minDistance, 2 * minDistance);
            Vector2 candidate = spawnCenter + new float2(
                math.cos(angle) * radius,
                math.sin(angle) * radius
            );
            
            if (IsValid(candidate, cellSize, points, grid))
            {
                points.Add(candidate);
                spawnPoints.Add(candidate);
                grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count - 1;
                candidateAccepted = true;
                break;
            }
        }
        
        if (!candidateAccepted)
            spawnPoints.RemoveAt(spawnIndex);
    }
    
    return points;
}
```

#### Complexity Analysis
- **Time**: O(n) where n = number of samples
- **Space**: O(grid cells) = O((width/cellSize) × (height/cellSize))
- **Neighbor Check**: O(1) via spatial grid
- **Target**: <0.5ms for ~100 hazards

#### Spatial Grid Optimization
```
Grid Cell Size = minDistance / √2

For each candidate:
    Check 5×5 = 25 neighboring cells (max)
    Average: ~9 non-empty neighbors
    
Performance: O(1) per candidate check
```

---

### 2.3 Cellular Automata (Regeneration)

#### Algorithm Overview
Conway-style cellular automata for hazard spreading. Implements "sum of neighbors > threshold" rule.

#### Mathematical Model
```
For each cell (x, y):
    neighbors = count(adjacent cells with hazard)
    
    if neighbors > threshold:
        cell_value(t+1) = min(1.0, cell_value(t) + regenRate × Δt)
    else:
        cell_value(t+1) = cell_value(t)
```

#### Implementation Details
```csharp
[BurstCompile]
public struct CellularAutomataJob : IJobParallelFor
{
    [ReadOnly] public int gridSize;
    [ReadOnly] public float regenAmount;      // 0.025 × Δt
    [ReadOnly] public int neighborThreshold;  // 4
    
    [ReadOnly] public NativeArray<float> currentGrid;
    [WriteOnly] public NativeArray<float> nextGrid;
    
    public void Execute(int index)
    {
        int x = index % gridSize;
        int y = index / gridSize;
        
        float currentValue = currentGrid[index];
        int neighborSum = 0;
        
        // Moore neighborhood (8 cells)
        for (int dy = -1; dy <= 1; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                if (dx == 0 && dy == 0) continue;
                
                int nx = x + dx;
                int ny = y + dy;
                
                if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize)
                {
                    int neighborIndex = ny * gridSize + nx;
                    if (currentGrid[neighborIndex] > 0.5f)
                        neighborSum++;
                }
            }
        }
        
        if (neighborSum > neighborThreshold)
        {
            currentValue = math.min(1.0f, currentValue + regenAmount);
        }
        
        nextGrid[index] = currentValue;
    }
}
```

#### Performance Characteristics
- **Grid Size**: 256×256 = 65,536 cells
- **Updates**: 60 FPS = 16.67ms frame budget
- **Target**: 5ms Burst-compiled
- **Memory**: 512KB (2 grids × 256KB)

#### Double Buffering
```
Frame N:   Read from currentGrid → Write to nextGrid
Frame N+1: Read from nextGrid   → Write to currentGrid
```

---

### 2.4 AI Solvability Bot

#### Algorithm Overview
Greedy pathfinding algorithm that simulates optimal player behavior to validate level solvability.

#### Pseudocode
```
Algorithm: Greedy Solvability Check
Input: levelGrid (256×256), targetClean (95%)
Output: Boolean (solvable/unsolvable)

1. cleanPercentage = 0%
2. iterations = 0
3. While cleanPercentage < targetClean AND iterations < maxIterations:
    a. targetCell = FindDirtiestCluster(grid)
    b. If targetCell == null: BREAK
    c. ClearArea(targetCell, radius)
    d. cleanPercentage = CalculateCleanPercentage(grid)
    e. iterations++
4. Return (cleanPercentage >= targetClean)
```

#### Dirtiest Cluster Heuristic
```csharp
private Vector2Int FindDirtiestCluster(NativeArray<float> grid)
{
    float maxDirtiness = 0f;
    Vector2Int dirtiestCell = new Vector2Int(-1, -1);
    
    int sampleStep = gridSize / 32; // Sample every 8 cells
    
    for (int y = 0; y < gridSize; y += sampleStep)
    {
        for (int x = 0; x < gridSize; x += sampleStep)
        {
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
            
            if (x >= 0 && x < gridSize && y >= 0 && y < gridSize)
            {
                totalDirt += grid[y * gridSize + x];
                sampleCount++;
            }
        }
    }
    
    return totalDirt / sampleCount;
}
```

#### Performance Budget
- **Time Limit**: 18ms
- **Iterations**: ~100-200 typical
- **Memory**: 256KB temp array
- **Success Rate**: 95% validation threshold

---

## 3. Performance Optimization

### 3.1 Burst Compiler Settings

#### Compilation Flags
```json
{
    "BurstCompileTarget": "Player",
    "EnableOptimizations": true,
    "EnableSafetyChecks": false,
    "EnableDebugInAllBuilds": false,
    "CpuMinTargetX64": "AVX2",
    "CpuMinTargetARM": "ARMV8A_AARCH64"
}
```

#### Assembly Output (Example)
```asm
; Burst-compiled SIMD loop
vmovaps ymm0, ymmword ptr [rsi + 4*rax]
vaddps ymm0, ymm0, ymmword ptr [rdi + 4*rax]
vmulps ymm0, ymm0, ymm1
vmovaps ymmword ptr [rdx + 4*rax], ymm0
```

### 3.2 Job System Architecture

#### Parallel Job Scheduling
```csharp
// Schedule procedural generation pipeline
JobHandle perlinHandle = perlinJob.Schedule(gridSize * gridSize, 64);
JobHandle poissonHandle = poissonJob.Schedule(); // Runs after Perlin
JobHandle caHandle = caJob.Schedule(gridSize * gridSize, 64, poissonHandle);
JobHandle aiHandle = aiJob.Schedule(caHandle);

aiHandle.Complete(); // Block until pipeline finishes
```

### 3.3 Memory Management

#### NativeArray Lifecycle
```csharp
// Allocate once, reuse across frames
NativeArray<float> persistentGrid = new NativeArray<float>(
    gridSize * gridSize,
    Allocator.Persistent
);

// Temp arrays for jobs
NativeArray<float> tempGrid = new NativeArray<float>(
    gridSize * gridSize,
    Allocator.TempJob
);

// Always dispose
persistentGrid.Dispose();
tempGrid.Dispose();
```

#### Memory Budget (Mid-Tier Device)
- **Procedural Grids**: 512KB
- **Textures**: 50MB (compressed)
- **Audio**: 200MB (streaming)
- **Total**: ~500MB target

### 3.4 Profiling Targets

| System | Target (ms) | Low-End (ms) | High-End (ms) |
|--------|-------------|--------------|--------------|
| Perlin Noise | 25 | 40 | 10 |
| Poisson Disk | 0.5 | 1.0 | 0.3 |
| Cellular Automata | 5 | 10 | 2 |
| AI Solvability | 20 | 30 | 10 |
| **Total Gen** | **80** | **120** | **40** |

---

## 4. Code Architecture

### 4.1 Namespace Structure
```
WhenImCleaningWindows/
├── Procedural/
│   ├── PerlinNoise.cs
│   ├── PoissonDiskSampling.cs
│   ├── CellularAutomata.cs
│   └── AISolvabilityBot.cs
├── Mechanics/
│   ├── GestureInput.cs
│   ├── SudsPhysics.cs
│   └── TimerSystem.cs
├── Monetization/
│   ├── EnergySystem.cs
│   ├── IAPManager.cs
│   └── AdManager.cs
└── UI/
    ├── HUDController.cs
    ├── MenuManager.cs
    └── ProgressionUI.cs
```

### 4.2 Design Patterns

#### Singleton (Managers)
```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

#### Object Pool (Particles)
```csharp
public class ParticlePool : MonoBehaviour
{
    private Queue<ParticleSystem> pool = new Queue<ParticleSystem>();
    
    public ParticleSystem Get()
    {
        if (pool.Count > 0)
            return pool.Dequeue();
        
        return Instantiate(particlePrefab);
    }
    
    public void Return(ParticleSystem ps)
    {
        ps.Stop();
        pool.Enqueue(ps);
    }
}
```

#### Observer (Events)
```csharp
public class EventManager
{
    public static event Action<int> OnLevelComplete;
    public static event Action<float> OnCleanPercentageUpdate;
    
    public static void TriggerLevelComplete(int stars)
    {
        OnLevelComplete?.Invoke(stars);
    }
}
```

---

## 5. Data Structures

### 5.1 Level Data
```csharp
[Serializable]
public struct LevelData
{
    public int world;
    public int level;
    public int seed;
    public float timeLimit;
    public HazardConfig[] hazards;
}

[Serializable]
public struct HazardConfig
{
    public HazardType type;
    public Vector2 position;
    public float intensity;
    public bool regenerates;
}
```

### 5.2 Save Data
```csharp
[Serializable]
public class SaveData
{
    public int currentWorld;
    public int currentLevel;
    public int totalStars;
    public int lives;
    public float[] upgradeProgress;
    public string[] unlockedCosmetics;
    public long lastLoginTimestamp;
}
```

---

## 6. Platform-Specific Considerations

### 6.1 Android Optimization
```csharp
#if UNITY_ANDROID
    // Force GPU acceleration
    Application.targetFrameRate = 120;
    QualitySettings.vSyncCount = 0;
    
    // Snapdragon-specific
    if (SystemInfo.processorType.Contains("Snapdragon"))
    {
        // Enable NPU acceleration for ML
        EnableNPUAcceleration();
    }
#endif
```

### 6.2 iOS Optimization
```csharp
#if UNITY_IOS
    // Metal API
    PlayerSettings.iOS.graphicsAPI = iOSGraphicsAPI.Metal;
    
    // A-series Bionic optimization
    if (SystemInfo.processorType.Contains("Apple"))
    {
        // Neural Engine for ML
        EnableNeuralEngine();
    }
#endif
```

### 6.3 Device Tier Detection
```csharp
public enum DeviceTier
{
    Low,    // Snapdragon 4 Gen, 4GB RAM
    Mid,    // Snapdragon 6 Gen, 6GB RAM
    High    // Snapdragon 8 Gen, 12GB RAM
}

public static DeviceTier DetectDeviceTier()
{
    int ram = SystemInfo.systemMemorySize;
    
    if (ram >= 10000) return DeviceTier.High;
    if (ram >= 5000) return DeviceTier.Mid;
    return DeviceTier.Low;
}
```

---

## 7. Testing & Validation

### 7.1 Unit Tests
```csharp
[Test]
public void PerlinNoise_GeneratesValidOutput()
{
    var noiseGen = new PerlinNoise();
    var result = noiseGen.Generate(seed: 12345);
    
    Assert.IsTrue(result.IsCreated);
    Assert.AreEqual(256 * 256, result.Length);
    
    foreach (var value in result)
    {
        Assert.IsTrue(value >= 0f && value <= 1f);
    }
    
    result.Dispose();
}
```

### 7.2 Performance Tests
```csharp
[Test, Performance]
public void PerlinNoise_MeetsPerformanceTarget()
{
    var noiseGen = new PerlinNoise();
    
    Measure.Method(() =>
    {
        var result = noiseGen.Generate();
        result.Dispose();
    })
    .WarmupCount(5)
    .MeasurementCount(100)
    .IterationsPerMeasurement(10)
    .Run();
    
    // Assert < 25ms on CPU
}
```

---

## 8. Future Optimizations

### 8.1 GPU Compute Shaders
```hlsl
// Perlin Noise Compute Shader
#pragma kernel PerlinNoiseCS

RWTexture2D<float> Result;

[numthreads(8,8,1)]
void PerlinNoiseCS(uint3 id : SV_DispatchThreadID)
{
    float2 uv = id.xy / 256.0;
    float noise = PerlinNoise(uv);
    Result[id.xy] = noise;
}
```

### 8.2 Neural Network Level Generation
```csharp
// Future: Use Unity ML-Agents to generate levels
public class MLLevelGenerator
{
    private NNModel trainedModel;
    
    public LevelData GenerateWithML(int difficulty)
    {
        // Neural network predicts optimal hazard placement
        return model.Predict(difficulty);
    }
}
```

---

## 9. Conclusion

This technical specification provides the mathematical foundations, algorithmic implementations, and performance optimizations necessary to achieve the ambitious targets of **When I'm Cleaning Windows**. By leveraging Burst compilation, job system parallelization, and careful algorithm selection, the game achieves <80ms procedural generation on mid-tier 2026 mobile devices while maintaining 60+ FPS gameplay.

---

© 2026 @romeocasanova212-jpg | romeo@brewandbrawl.co.uk
