using UnityEngine;
using WhenImCleaningWindows.Core;
using WhenImCleaningWindows.Gameplay;
using WhenImCleaningWindows.Mechanics;
using WhenImCleaningWindows.Procedural;

namespace WhenImCleaningWindows.Debugging
{
    /// <summary>
    /// Level Test Manager - Scene testing utility with UI buttons to rapidly test gameplay.
    /// Provides instant level completion, hazard spawning, power-up testing, and debug controls.
    /// </summary>
    public class LevelTestManager : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private bool showTestUI = true;
        [SerializeField] private KeyCode toggleUIKey = KeyCode.F1;
        
        [Header("Quick Actions")]
        [SerializeField] private KeyCode instantCompleteKey = KeyCode.F2;
        [SerializeField] private KeyCode nukeKey = KeyCode.F3;
        [SerializeField] private KeyCode turboKey = KeyCode.F4;
        [SerializeField] private KeyCode autoPilotKey = KeyCode.F5;
        [SerializeField] private KeyCode resetWindowKey = KeyCode.F6;
        
        [Header("References")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WindowMeshController windowMesh;
        [SerializeField] private HazardRenderer hazardRenderer;
        [SerializeField] private CleaningController cleaningController;
        [SerializeField] private LevelGenerator levelGenerator;
        
        // UI State
        private Rect windowRect = new Rect(Screen.width - 320, 10, 300, 600);
        private Vector2 scrollPosition = Vector2.zero;
        private GUIStyle buttonStyle;
        private GUIStyle labelStyle;
        private GUIStyle boxStyle;
        
        // Test State
        private int testLevelNumber = 1;
        private float testCleanPercentage = 50f;
        private HazardType selectedHazardType = HazardType.BirdPoop;
        
        private void Start()
        {
            // Find references if not assigned
            if (gameManager == null) gameManager = GameManager.Instance;
            if (windowMesh == null) windowMesh = FindFirstObjectByType<WindowMeshController>();
            if (hazardRenderer == null) hazardRenderer = FindFirstObjectByType<HazardRenderer>();
            if (cleaningController == null) cleaningController = FindFirstObjectByType<CleaningController>();
            if (levelGenerator == null) levelGenerator = LevelGenerator.Instance;
            
            InitializeGUIStyles();
            
            Debug.Log("[LevelTestManager] Initialized - Press F1 to toggle test UI");
        }
        
        private void InitializeGUIStyles()
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.padding = new RectOffset(10, 10, 8, 8);
            buttonStyle.margin = new RectOffset(4, 4, 4, 4);
            
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 14;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = Color.white;
            
            boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.padding = new RectOffset(10, 10, 10, 10);
        }
        
        private void Update()
        {
            // Toggle UI
            if (Input.GetKeyDown(toggleUIKey))
            {
                showTestUI = !showTestUI;
                Debug.Log($"[LevelTestManager] Test UI: {showTestUI}");
            }
            
            if (!showTestUI) return;
            
            // Quick action hotkeys
            if (Input.GetKeyDown(instantCompleteKey))
            {
                InstantCompleteLevel();
            }
            
            if (Input.GetKeyDown(nukeKey))
            {
                ActivateNuke();
            }
            
            if (Input.GetKeyDown(turboKey))
            {
                ActivateTurbo();
            }
            
            if (Input.GetKeyDown(autoPilotKey))
            {
                ActivateAutoPilot();
            }
            
            if (Input.GetKeyDown(resetWindowKey))
            {
                ResetWindow();
            }
        }
        
        private void OnGUI()
        {
            if (!Application.isPlaying || !showTestUI) return;
            
            if (buttonStyle == null) InitializeGUIStyles();
            
            windowRect = GUILayout.Window(1, windowRect, DrawTestWindow, "LEVEL TEST MANAGER", boxStyle);
        }
        
        private void DrawTestWindow(int windowID)
        {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            
            // Level Controls
            GUILayout.Label("LEVEL CONTROLS", labelStyle);
            GUILayout.BeginVertical(boxStyle);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Level: {testLevelNumber}", GUILayout.Width(100));
            testLevelNumber = (int)GUILayout.HorizontalSlider(testLevelNumber, 1, 100);
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button($"Start Level {testLevelNumber}", buttonStyle))
            {
                StartTestLevel(testLevelNumber);
            }
            
            if (GUILayout.Button("Next Level", buttonStyle))
            {
                testLevelNumber++;
                StartTestLevel(testLevelNumber);
            }
            
            if (GUILayout.Button("Instant Complete (F2)", buttonStyle))
            {
                InstantCompleteLevel();
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10);
            
            // Window Controls
            GUILayout.Label("WINDOW CONTROLS", labelStyle);
            GUILayout.BeginVertical(boxStyle);
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Clean: {testCleanPercentage:F0}%", GUILayout.Width(100));
            testCleanPercentage = GUILayout.HorizontalSlider(testCleanPercentage, 0f, 100f);
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Set Clean %", buttonStyle))
            {
                SetWindowCleanPercentage(testCleanPercentage);
            }
            
            if (GUILayout.Button("100% Clean", buttonStyle))
            {
                SetWindowCleanPercentage(100f);
            }
            
            if (GUILayout.Button("100% Dirty", buttonStyle))
            {
                SetWindowCleanPercentage(0f);
            }
            
            if (GUILayout.Button("Reset Window (F6)", buttonStyle))
            {
                ResetWindow();
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10);
            
            // Power-Up Controls
            GUILayout.Label("POWER-UPS", labelStyle);
            GUILayout.BeginVertical(boxStyle);
            
            if (GUILayout.Button("Nuke (F3)", buttonStyle))
            {
                ActivateNuke();
            }
            
            if (GUILayout.Button("Turbo (F4)", buttonStyle))
            {
                ActivateTurbo();
            }
            
            if (GUILayout.Button("Auto-Pilot (F5)", buttonStyle))
            {
                ActivateAutoPilot();
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10);
            
            // Hazard Controls
            GUILayout.Label("HAZARD SPAWNING", labelStyle);
            GUILayout.BeginVertical(boxStyle);
            
            // Hazard type selector
            GUILayout.Label($"Type: {selectedHazardType}");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("◀", GUILayout.Width(40)))
            {
                selectedHazardType = (HazardType)(((int)selectedHazardType - 1 + 24) % 24);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("▶", GUILayout.Width(40)))
            {
                selectedHazardType = (HazardType)(((int)selectedHazardType + 1) % 24);
            }
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Spawn Test Hazard", buttonStyle))
            {
                SpawnTestHazard(selectedHazardType);
            }
            
            if (GUILayout.Button("Spawn All Hazards", buttonStyle))
            {
                SpawnAllHazardTypes();
            }
            
            if (GUILayout.Button("Clear All Hazards", buttonStyle))
            {
                ClearAllHazards();
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10);
            
            // Info Display
            GUILayout.Label("CURRENT STATE", labelStyle);
            GUILayout.BeginVertical(boxStyle);
            
            if (gameManager != null)
            {
                GUILayout.Label($"Game State: {gameManager.GetState()}");
                LevelData currentLvl = gameManager.GetCurrentLevel();
                GUILayout.Label($"Current Level: {currentLvl?.levelNumber ?? 0}");
            }
            
            if (cleaningController != null)
            {
                float currentClean = cleaningController.GetCurrentCleanPercentage();
                GUILayout.Label($"Window Clean: {currentClean:F1}%");
            }
            
            if (windowMesh != null)
            {
                GUILayout.Label($"Window Mesh: Active");
            }
            
            if (hazardRenderer != null)
            {
                GUILayout.Label($"Hazard Renderer: Active");
            }
            
            GUILayout.EndVertical();
            GUILayout.Space(10);
            
            // Hotkey Reference
            GUILayout.Label("HOTKEYS", labelStyle);
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label("F1 - Toggle Test UI");
            GUILayout.Label("F2 - Instant Complete");
            GUILayout.Label("F3 - Nuke Power-Up");
            GUILayout.Label("F4 - Turbo Power-Up");
            GUILayout.Label("F5 - Auto-Pilot Power-Up");
            GUILayout.Label("F6 - Reset Window");
            GUILayout.EndVertical();
            
            GUILayout.EndScrollView();
            
            // Make window draggable
            GUI.DragWindow();
        }
        
        // Test Actions
        private void StartTestLevel(int levelNumber)
        {
            if (gameManager != null)
            {
                gameManager.StartLevel(levelNumber);
                Debug.Log($"[LevelTestManager] Started test level {levelNumber}");
            }
            else
            {
                Debug.LogWarning("[LevelTestManager] GameManager not found");
            }
        }
        
        private void InstantCompleteLevel()
        {
            if (windowMesh != null)
            {
                // Clean entire window instantly
                windowMesh.SetInitialDirtiness(0f);
                Debug.Log("[LevelTestManager] Instant complete - window 100% clean");
                
                // Level will auto-complete in GameManager.Update when it detects 95%+ clean
            }
            else
            {
                Debug.LogWarning("[LevelTestManager] WindowMeshController not found");
            }
        }
        
        private void SetWindowCleanPercentage(float percentage)
        {
            if (windowMesh != null)
            {
                float dirtiness = 1f - (percentage / 100f);
                windowMesh.SetInitialDirtiness(dirtiness);
                Debug.Log($"[LevelTestManager] Set window clean to {percentage}%");
            }
        }
        
        private void ResetWindow()
        {
            if (windowMesh != null)
            {
                windowMesh.SetInitialDirtiness(1f);
                Debug.Log("[LevelTestManager] Reset window to 100% dirty");
            }
        }
        
        private void ActivateNuke()
        {
            if (cleaningController != null)
            {
                cleaningController.ActivateNuke();
                Debug.Log("[LevelTestManager] ✓ Activated Nuke power-up");
            }
            else
            {
                Debug.LogWarning("[LevelTestManager] CleaningController not found");
            }
        }
        
        private void ActivateTurbo()
        {
            if (cleaningController != null)
            {
                cleaningController.ActivateTurbo(10f); // 10 second turbo mode
                Debug.Log("[LevelTestManager] ✓ Activated Turbo power-up (10s)");
            }
            else
            {
                Debug.LogWarning("[LevelTestManager] CleaningController not found");
            }
        }
        
        private void ActivateAutoPilot()
        {
            if (cleaningController != null)
            {
                cleaningController.ActivateAutoPilot(15f); // 15 second auto-pilot
                Debug.Log("[LevelTestManager] ✓ Activated Auto-Pilot power-up (15s)");
            }
            else
            {
                Debug.LogWarning("[LevelTestManager] CleaningController not found");
            }
        }
        
        private void SpawnTestHazard(HazardType hazardType)
        {
            if (hazardRenderer == null)
            {
                Debug.LogWarning("[LevelTestManager] HazardRenderer not found");
                return;
            }
            
            // Create a test hazard position
            Vector2 testPosition = new Vector2(Random.Range(-4f, 4f), Random.Range(-3f, 3f));
            float testSize = Random.Range(0.5f, 1.5f);
            
            hazardRenderer.SpawnSingleHazard(hazardType, testPosition, testSize);
            Debug.Log($"[LevelTestManager] ✓ Spawned test hazard: {hazardType} at {testPosition}");
        }
        
        private void SpawnAllHazardTypes()
        {
            if (hazardRenderer == null)
            {
                Debug.LogWarning("[LevelTestManager] HazardRenderer not found");
                return;
            }
            
            Debug.Log("[LevelTestManager] Spawning all 24 hazard types...");
            
            // Spawn in a grid pattern
            int columns = 6;
            float spacing = 1.5f;
            float startX = -4f;
            float startY = 3f;
            
            for (int i = 0; i < 24; i++)
            {
                int row = i / columns;
                int col = i % columns;
                
                HazardType type = (HazardType)i;
                Vector2 position = new Vector2(startX + col * spacing, startY - row * spacing);
                
                hazardRenderer.SpawnSingleHazard(type, position, 0.8f);
            }
            
            Debug.Log("[LevelTestManager] ✓ Spawned all 24 hazard types in grid");
        }
        
        private void ClearAllHazards()
        {
            if (hazardRenderer != null)
            {
                hazardRenderer.ClearAllHazards();
                Debug.Log("[LevelTestManager] Cleared all hazards");
            }
            else
            {
                Debug.LogWarning("[LevelTestManager] HazardRenderer not found");
            }
        }
    }
}








