using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace WhenImCleaningWindows.Debugging
{
    /// <summary>
    /// Input Debugger - Visualizes touch input, gesture detection, and cleaning paths.
    /// Attach to any GameObject in the scene to see real-time input debugging.
    /// </summary>
    public class InputDebugger : MonoBehaviour
    {
        [Header("Debug Visualization")]
        [SerializeField] private bool showTouchPoints = true;
        [SerializeField] private bool showCleaningPaths = true;
        [SerializeField] private bool showCircleDetection = true;
        [SerializeField] private bool showGestureInfo = true;
        [SerializeField] private bool showPerformanceMetrics = true;
        
        [Header("Visual Settings")]
        [SerializeField] private Color touchColor = Color.cyan;
        [SerializeField] private Color cleanPathColor = Color.green;
        [SerializeField] private Color circleProgressColor = Color.yellow;
        [SerializeField] private float touchPointRadius = 0.3f;
        [SerializeField] private int maxPathPoints = 100;
        
        [Header("Performance")]
        [SerializeField] private float updateInterval = 0.1f; // Update metrics every 100ms
        
        // Tracking
        private Vector3[] pathPoints;
        private int pathPointIndex = 0;
        private Vector3 lastTouchWorldPos;
        private float totalAngle = 0f;
        private Vector3 lastCirclePos;
        
        // Performance
        private float fpsUpdateTimer = 0f;
        private int frameCount = 0;
        private float currentFPS = 0f;
        private int touchCount = 0;
        
        // References
        private Camera mainCamera;
        private GUIStyle debugStyle;
        
        private void Awake()
        {
            pathPoints = new Vector3[maxPathPoints];
            mainCamera = Camera.main;
            
            // Initialize enhanced touch
            EnhancedTouchSupport.Enable();
        }
        
        private void OnDestroy()
        {
            EnhancedTouchSupport.Disable();
        }
        
        private void Start()
        {
            // Setup GUI style
            debugStyle = new GUIStyle();
            debugStyle.fontSize = 18;
            debugStyle.normal.textColor = Color.white;
            debugStyle.alignment = TextAnchor.UpperLeft;
            
            Debug.Log("[InputDebugger] Initialized - Touch visualization active");
        }
        
        private void Update()
        {
            UpdatePerformanceMetrics();
            TrackTouchInput();
        }
        
        private void UpdatePerformanceMetrics()
        {
            if (!showPerformanceMetrics) return;
            
            frameCount++;
            fpsUpdateTimer += Time.deltaTime;
            
            if (fpsUpdateTimer >= updateInterval)
            {
                currentFPS = frameCount / fpsUpdateTimer;
                frameCount = 0;
                fpsUpdateTimer = 0f;
            }
        }
        
        private void TrackTouchInput()
        {
            if (!showCleaningPaths && !showCircleDetection) return;
            
            touchCount = Touch.activeTouches.Count;
            
            if (touchCount > 0)
            {
                Touch primaryTouch = Touch.activeTouches[0];
                Vector2 screenPos = primaryTouch.screenPosition;
                Vector3 worldPos = ScreenToWorldPosition(screenPos);
                
                // Track path
                if (showCleaningPaths && primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
                {
                    pathPoints[pathPointIndex] = worldPos;
                    pathPointIndex = (pathPointIndex + 1) % maxPathPoints;
                }
                
                // Track circle detection
                if (showCircleDetection)
                {
                    if (lastCirclePos != Vector3.zero)
                    {
                        Vector3 prevDir = (lastCirclePos - worldPos).normalized;
                        Vector3 currentDir = (worldPos - lastCirclePos).normalized;
                        
                        float angle = Vector3.SignedAngle(prevDir, currentDir, Vector3.forward);
                        totalAngle += Mathf.Abs(angle);
                        
                        // Reset if touch ended
                        if (primaryTouch.phase == UnityEngine.InputSystem.TouchPhase.Ended)
                        {
                            totalAngle = 0f;
                        }
                    }
                    
                    lastCirclePos = worldPos;
                }
                
                lastTouchWorldPos = worldPos;
            }
            else
            {
                // Clear tracking when no touches
                lastCirclePos = Vector3.zero;
            }
        }
        
        private void OnDrawGizmos()
        {
            if (mainCamera == null) return;
            
            // Draw touch points
            if (showTouchPoints && Touch.activeTouches.Count > 0)
            {
                Gizmos.color = touchColor;
                
                foreach (Touch touch in Touch.activeTouches)
                {
                    Vector3 worldPos = ScreenToWorldPosition(touch.screenPosition);
                    Gizmos.DrawWireSphere(worldPos, touchPointRadius);
                    
                    // Draw touch ID label
                    Gizmos.DrawLine(worldPos, worldPos + Vector3.up * 0.5f);
                }
            }
            
            // Draw cleaning path
            if (showCleaningPaths && pathPointIndex > 0)
            {
                Gizmos.color = cleanPathColor;
                
                for (int i = 0; i < maxPathPoints - 1; i++)
                {
                    if (pathPoints[i] != Vector3.zero && pathPoints[i + 1] != Vector3.zero)
                    {
                        Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
                    }
                }
            }
            
            // Draw circle detection progress
            if (showCircleDetection && totalAngle > 0f)
            {
                Gizmos.color = circleProgressColor;
                
                // Draw arc showing progress toward circle (270° needed)
                float progress = Mathf.Clamp01(totalAngle / 270f);
                int segments = Mathf.CeilToInt(20 * progress);
                float radius = 1.5f;
                
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = (i / 20f) * 360f * Mathf.Deg2Rad;
                    float angle2 = ((i + 1) / 20f) * 360f * Mathf.Deg2Rad;
                    
                    Vector3 p1 = lastTouchWorldPos + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0f) * radius;
                    Vector3 p2 = lastTouchWorldPos + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0f) * radius;
                    
                    Gizmos.DrawLine(p1, p2);
                }
            }
        }
        
        private void OnGUI()
        {
            if (!Application.isPlaying || (!showGestureInfo && !showPerformanceMetrics)) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 600));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>INPUT DEBUGGER</b>", debugStyle);
            GUILayout.Space(10);
            
            if (showGestureInfo)
            {
                GUILayout.Label($"<b>Touch Input:</b>", debugStyle);
                GUILayout.Label($"  Active Touches: {touchCount}", debugStyle);
                
                if (Touch.activeTouches.Count > 0)
                {
                    Touch primaryTouch = Touch.activeTouches[0];
                    GUILayout.Label($"  Phase: {primaryTouch.phase}", debugStyle);
                    GUILayout.Label($"  Screen Pos: {primaryTouch.screenPosition}", debugStyle);
                    GUILayout.Label($"  World Pos: {lastTouchWorldPos}", debugStyle);
                    GUILayout.Label($"  Delta: {primaryTouch.delta}", debugStyle);
                }
                
                GUILayout.Space(10);
                
                if (showCircleDetection)
                {
                    GUILayout.Label($"<b>Circle Detection:</b>", debugStyle);
                    GUILayout.Label($"  Angle Progress: {totalAngle:F1}° / 270°", debugStyle);
                    GUILayout.Label($"  Progress: {Mathf.Clamp01(totalAngle / 270f) * 100f:F1}%", debugStyle);
                    
                    if (totalAngle >= 270f)
                    {
                        GUILayout.Label($"  <color=green>✓ CIRCLE DETECTED!</color>", debugStyle);
                    }
                }
                
                GUILayout.Space(10);
            }
            
            if (showPerformanceMetrics)
            {
                GUILayout.Label($"<b>Performance:</b>", debugStyle);
                GUILayout.Label($"  FPS: {currentFPS:F1}", debugStyle);
                GUILayout.Label($"  Frame Time: {Time.deltaTime * 1000f:F2}ms", debugStyle);
                GUILayout.Label($"  Path Points: {pathPointIndex}/{maxPathPoints}", debugStyle);
                
                GUILayout.Space(10);
                
                // Color code FPS
                string fpsColor = "white";
                if (currentFPS >= 120f) fpsColor = "lime";
                else if (currentFPS >= 60f) fpsColor = "green";
                else if (currentFPS >= 30f) fpsColor = "yellow";
                else fpsColor = "red";
                
                GUILayout.Label($"  <color={fpsColor}>FPS Status: {GetFPSStatus()}</color>", debugStyle);
            }
            
            GUILayout.Space(20);
            GUILayout.Label("<b>Debug Controls:</b>", debugStyle);
            GUILayout.Label("  T - Toggle Touch Points", debugStyle);
            GUILayout.Label("  P - Toggle Cleaning Paths", debugStyle);
            GUILayout.Label("  C - Toggle Circle Detection", debugStyle);
            GUILayout.Label("  G - Toggle Gesture Info", debugStyle);
            GUILayout.Label("  M - Toggle Performance Metrics", debugStyle);
            GUILayout.Label("  R - Reset Path Tracking", debugStyle);
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void LateUpdate()
        {
            // Keyboard shortcuts for debug toggles
            if (Input.GetKeyDown(KeyCode.T))
            {
                showTouchPoints = !showTouchPoints;
                Debug.Log($"[InputDebugger] Touch Points: {showTouchPoints}");
            }
            
            if (Input.GetKeyDown(KeyCode.P))
            {
                showCleaningPaths = !showCleaningPaths;
                Debug.Log($"[InputDebugger] Cleaning Paths: {showCleaningPaths}");
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                showCircleDetection = !showCircleDetection;
                Debug.Log($"[InputDebugger] Circle Detection: {showCircleDetection}");
            }
            
            if (Input.GetKeyDown(KeyCode.G))
            {
                showGestureInfo = !showGestureInfo;
                Debug.Log($"[InputDebugger] Gesture Info: {showGestureInfo}");
            }
            
            if (Input.GetKeyDown(KeyCode.M))
            {
                showPerformanceMetrics = !showPerformanceMetrics;
                Debug.Log($"[InputDebugger] Performance Metrics: {showPerformanceMetrics}");
            }
            
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetTracking();
                Debug.Log("[InputDebugger] Reset path tracking");
            }
        }
        
        private void ResetTracking()
        {
            pathPoints = new Vector3[maxPathPoints];
            pathPointIndex = 0;
            totalAngle = 0f;
            lastCirclePos = Vector3.zero;
        }
        
        private Vector3 ScreenToWorldPosition(Vector2 screenPos)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            worldPos.z = 0f;
            return worldPos;
        }
        
        private string GetFPSStatus()
        {
            if (currentFPS >= 120f) return "Excellent (120+ FPS)";
            if (currentFPS >= 60f) return "Good (60+ FPS)";
            if (currentFPS >= 30f) return "Acceptable (30+ FPS)";
            return "Poor (<30 FPS)";
        }
        
        // Public API for external debugging
        public void LogTouchEvent(string eventName, Vector2 position)
        {
            Debug.Log($"[InputDebugger] {eventName} at {position}");
        }
        
        public void LogGestureDetected(string gestureName)
        {
            Debug.Log($"[InputDebugger] <color=cyan>GESTURE DETECTED: {gestureName}</color>");
        }
        
        public void LogCleaningAction(Vector3 worldPos, float cleanRadius, float cleanPower)
        {
            Debug.Log($"[InputDebugger] Cleaning at {worldPos} (radius: {cleanRadius}, power: {cleanPower})");
        }
    }
}








