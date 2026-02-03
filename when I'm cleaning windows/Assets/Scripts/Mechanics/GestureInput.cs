using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace WhenImCleaningWindows.Mechanics
{
    /// <summary>
    /// Enhanced Touch multi-touch gesture recognition system.
    /// Supports: Swipe/Drag, Circle Scrub, Up-Flick, Double-Tap, Pull, Gyro Tilt.
    /// Target: <50ms input lag for responsive ASMR feedback.
    /// </summary>
    public class GestureInput : MonoBehaviour
    {
        [Header("Gesture Settings")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float scrubCircleRadius = 30f;
        [SerializeField] private float flickAngleThreshold = 45f;
        [SerializeField] private float doubleTapMaxDelay = 0.3f;
        [SerializeField] private float pullMinDistance = 20f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;
        
        // Events
        public System.Action<Vector2, Vector2> OnSwipe;
        public System.Action<Vector2, float> OnCircleScrub;
        public System.Action<Vector2> OnFlick;
        public System.Action<Vector2> OnDoubleTap;
        public System.Action<Vector2, float> OnPull;
        
        private List<Vector2> currentTouchPath = new List<Vector2>();
        private float lastTapTime = 0f;
        private Vector2 lastTapPosition;
        private bool gyroEnabled = false;
        
        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            
            // Enable gyroscope if available
            if (SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
                gyroEnabled = true;
            }
        }
        
        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }
        
        private void Update()
        {
            ProcessTouches();
            ProcessGyro();
        }
        
        /// <summary>
        /// Process all active touches and recognize gestures.
        /// </summary>
        private void ProcessTouches()
        {
            var activeTouches = Touch.activeTouches;
            
            if (activeTouches.Count == 0)
            {
                // Clear path when no touches
                if (currentTouchPath.Count > 0)
                {
                    AnalyzeGesture();
                    currentTouchPath.Clear();
                }
                return;
            }
            
            foreach (var touch in activeTouches)
            {
                switch (touch.phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        HandleTouchBegan(touch);
                        break;
                        
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                        HandleTouchMoved(touch);
                        break;
                        
                    case UnityEngine.InputSystem.TouchPhase.Ended:
                        HandleTouchEnded(touch);
                        break;
                }
            }
        }
        
        /// <summary>
        /// Handle touch begin phase.
        /// </summary>
        private void HandleTouchBegan(Touch touch)
        {
            Vector2 screenPos = touch.screenPosition;
            currentTouchPath.Clear();
            currentTouchPath.Add(screenPos);
            
            // Check for double-tap
            if (Time.time - lastTapTime < doubleTapMaxDelay &&
                Vector2.Distance(screenPos, lastTapPosition) < 50f)
            {
                OnDoubleTap?.Invoke(ScreenToWorld(screenPos));
            }
            
            lastTapTime = Time.time;
            lastTapPosition = screenPos;
        }
        
        /// <summary>
        /// Handle touch move phase (drag/swipe).
        /// </summary>
        private void HandleTouchMoved(Touch touch)
        {
            Vector2 screenPos = touch.screenPosition;
            currentTouchPath.Add(screenPos);
            
            // Real-time swipe/drag feedback
            if (currentTouchPath.Count >= 2)
            {
                Vector2 lastPos = currentTouchPath[currentTouchPath.Count - 2];
                Vector2 delta = screenPos - lastPos;
                
                if (delta.magnitude > 5f)
                {
                    OnSwipe?.Invoke(ScreenToWorld(lastPos), ScreenToWorld(screenPos));
                }
            }
        }
        
        /// <summary>
        /// Handle touch end phase and analyze gesture.
        /// </summary>
        private void HandleTouchEnded(Touch touch)
        {
            Vector2 screenPos = touch.screenPosition;
            currentTouchPath.Add(screenPos);
            
            AnalyzeGesture();
            currentTouchPath.Clear();
        }
        
        /// <summary>
        /// Analyze completed touch path to recognize gestures.
        /// </summary>
        private void AnalyzeGesture()
        {
            if (currentTouchPath.Count < 3) return;
            
            // Check for circle scrub
            if (IsCircleGesture(currentTouchPath, out Vector2 center, out float radius))
            {
                float precision = CalculateCirclePrecision(currentTouchPath, center, radius);
                OnCircleScrub?.Invoke(ScreenToWorld(center), precision);
                return;
            }
            
            // Check for flick (up direction)
            Vector2 startPos = currentTouchPath[0];
            Vector2 endPos = currentTouchPath[currentTouchPath.Count - 1];
            Vector2 direction = endPos - startPos;
            
            if (direction.magnitude > swipeThreshold)
            {
                float angle = Vector2.SignedAngle(Vector2.up, direction);
                
                if (Mathf.Abs(angle) < flickAngleThreshold)
                {
                    OnFlick?.Invoke(ScreenToWorld(endPos));
                    return;
                }
            }
            
            // Check for pull gesture (sustained distance)
            if (direction.magnitude > pullMinDistance)
            {
                float pullStrength = Mathf.Clamp01(direction.magnitude / 200f);
                OnPull?.Invoke(ScreenToWorld(endPos), pullStrength);
            }
        }
        
        /// <summary>
        /// Check if touch path forms a circular pattern.
        /// </summary>
        private bool IsCircleGesture(List<Vector2> path, out Vector2 center, out float radius)
        {
            center = Vector2.zero;
            radius = 0f;
            
            if (path.Count < 8) return false;
            
            // Calculate centroid
            foreach (var point in path)
            {
                center += point;
            }
            center /= path.Count;
            
            // Calculate average radius
            float totalRadius = 0f;
            foreach (var point in path)
            {
                totalRadius += Vector2.Distance(point, center);
            }
            radius = totalRadius / path.Count;
            
            // Check if points are roughly circular (variance test)
            float variance = 0f;
            foreach (var point in path)
            {
                float dist = Vector2.Distance(point, center);
                variance += Mathf.Pow(dist - radius, 2);
            }
            variance /= path.Count;
            
            // Low variance = circular pattern
            return variance < radius * 0.3f && radius > scrubCircleRadius;
        }
        
        /// <summary>
        /// Calculate precision score (0-1) for circle gesture.
        /// </summary>
        private float CalculateCirclePrecision(List<Vector2> path, Vector2 center, float radius)
        {
            float totalDeviation = 0f;
            
            foreach (var point in path)
            {
                float dist = Vector2.Distance(point, center);
                totalDeviation += Mathf.Abs(dist - radius);
            }
            
            float avgDeviation = totalDeviation / path.Count;
            float precision = 1f - Mathf.Clamp01(avgDeviation / radius);
            
            return precision;
        }
        
        /// <summary>
        /// Process gyroscope input for zero-G levels.
        /// </summary>
        private void ProcessGyro()
        {
            if (!gyroEnabled) return;
            
            Vector3 gyroRotation = Input.gyro.rotationRate;
            
            // Gyro tilt can be used for special mechanics
            // Implementation depends on level requirements
        }
        
        /// <summary>
        /// Convert screen position to world position.
        /// </summary>
        private Vector2 ScreenToWorld(Vector2 screenPos)
        {
            Camera cam = Camera.main;
            if (cam == null) return screenPos;
            
            Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
            return new Vector2(worldPos.x, worldPos.y);
        }
        
        /// <summary>
        /// Debug visualization.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || currentTouchPath.Count < 2) return;
            
            Gizmos.color = Color.cyan;
            for (int i = 0; i < currentTouchPath.Count - 1; i++)
            {
                Vector2 start = ScreenToWorld(currentTouchPath[i]);
                Vector2 end = ScreenToWorld(currentTouchPath[i + 1]);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}








