using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;
using System.Collections.Generic;
using WhenImCleaningWindows.Core;
using WhenImCleaningWindows.Mechanics;
using WhenImCleaningWindows.Procedural;
using WhenImCleaningWindows.Visual;
using WhenImCleaningWindows.Audio;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Gameplay
{
    /// <summary>
    /// Cleaning Controller - Main gameplay controller that integrates gestures, cleaning, and hazards.
    /// Handles player input and translates it into cleaning actions on the window.
    /// </summary>
    public class CleaningController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WindowMeshController windowMesh;
        [SerializeField] private HazardRenderer hazardRenderer;
        [SerializeField] private Camera mainCamera;

        [Header("Cleaning Configuration")]
        [SerializeField] private float swipeCleanRadius = 0.5f;
        [SerializeField] private float swipeCleanPower = 0.3f;
        [SerializeField] private float circleCleanRadius = 0.8f;
        [SerializeField] private float circleCleanPower = 0.5f;
        [SerializeField] private float squeegeeCleanRadius = 0.3f;
        [SerializeField] private float squeegeeCleanPower = 0.4f;

        [Header("Input Settings")]
        [SerializeField] private float minSwipeDistance = 0.1f;
        [SerializeField] private float circleDetectionRadius = 1f;
        [SerializeField] private int circlePointsRequired = 20;

        private GestureInput gestureInput;
        private Vector3 lastCleanPosition;
        private bool isCleaning = false;

        // Circle detection
        private Vector3 circleStartPos;
        private float circleAngleSum = 0f;
        private int circlePoints = 0;

        public bool IsActive { get; private set; }

        #region Initialization

        private void Awake()
        {
            if (windowMesh == null) windowMesh = GetComponent<WindowMeshController>();
            if (hazardRenderer == null) hazardRenderer = GetComponent<HazardRenderer>();
            if (mainCamera == null) mainCamera = Camera.main;

            ApplyConfig();

            // Initialize gesture input
            gestureInput = FindFirstObjectByType<GestureInput>();
            if (gestureInput == null)
            {
                GameObject gestureObj = new GameObject("GestureInput");
                gestureInput = gestureObj.AddComponent<GestureInput>();
            }

            // Enable Enhanced Touch
            EnhancedTouchSupport.Enable();
        }

        private void ApplyConfig()
        {
            GameConfig config = ConfigProvider.GameConfig;
            if (config == null) return;

            swipeCleanRadius = config.baseCleaningRadius;
            swipeCleanPower = config.baseCleaningPower;
        }

        private void OnEnable()
        {
            SubscribeToGestureEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromGestureEvents();
        }

        private void SubscribeToGestureEvents()
        {
            if (gestureInput != null)
            {
                gestureInput.OnSwipe += HandleSwipe;
                gestureInput.OnCircleScrub += HandleCircle;
                gestureInput.OnDoubleTap += HandleDoubleTap;
            }
        }

        private void UnsubscribeFromGestureEvents()
        {
            if (gestureInput != null)
            {
                gestureInput.OnSwipe -= HandleSwipe;
                gestureInput.OnCircleScrub -= HandleCircle;
                gestureInput.OnDoubleTap -= HandleDoubleTap;
            }
        }

        #endregion

        #region Update Loop

        private void Update()
        {
            if (!IsActive) return;

            // Handle continuous cleaning (drag/swipe)
            if (Touch.activeFingers.Count > 0)
            {
                var touch = Touch.activeFingers[0].currentTouch;

                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    ProcessContinuousCleaning(touch.screenPosition);
                }
                else if (touch.phase == TouchPhase.Began)
                {
                    StartCleaning(touch.screenPosition);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    EndCleaning();
                }
            }

            // Fallback to mouse input in editor
            #if UNITY_EDITOR || UNITY_STANDALONE
            HandleMouseInput();
            #endif
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartCleaning(Input.mousePosition);
            }
            else if (Input.GetMouseButton(0))
            {
                ProcessContinuousCleaning(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                EndCleaning();
            }
        }

        #endregion

        #region Cleaning Logic

        private void StartCleaning(Vector2 screenPosition)
        {
            Vector3 worldPosition = ScreenToWorldPosition(screenPosition);

            if (windowMesh.IsPointOnWindow(worldPosition))
            {
                isCleaning = true;
                lastCleanPosition = worldPosition;
                circleStartPos = worldPosition;
                circleAngleSum = 0f;
                circlePoints = 0;
            }
        }

        private void ProcessContinuousCleaning(Vector2 screenPosition)
        {
            if (!isCleaning) return;

            Vector3 worldPosition = ScreenToWorldPosition(screenPosition);

            if (!windowMesh.IsPointOnWindow(worldPosition)) return;

            // Calculate distance from last position
            float distance = Vector3.Distance(worldPosition, lastCleanPosition);

            if (distance >= minSwipeDistance)
            {
                // Perform cleaning along the path
                CleanPath(lastCleanPosition, worldPosition);

                // Detect circle motion
                DetectCircleMotion(worldPosition);

                lastCleanPosition = worldPosition;
            }
        }

        private void EndCleaning()
        {
            isCleaning = false;

            // Check if we completed a circle
            if (circlePoints >= circlePointsRequired && circleAngleSum >= Mathf.PI * 1.5f)
            {
                HandleCircle(circleStartPos, circleDetectionRadius);
            }
        }

        private void CleanPath(Vector3 startPos, Vector3 endPos)
        {
            // Sample points along the path
            int samples = Mathf.CeilToInt(Vector3.Distance(startPos, endPos) / (swipeCleanRadius * 0.5f));
            samples = Mathf.Max(samples, 2);

            for (int i = 0; i <= samples; i++)
            {
                float t = (float)i / samples;
                Vector3 position = Vector3.Lerp(startPos, endPos, t);
                CleanAtPosition(position, swipeCleanRadius, swipeCleanPower);
            }

            // Play VFX
            Vector3 direction = (endPos - startPos).normalized;
            VFXManager.Instance?.PlaySwipeSudsEffect(endPos, direction);
        }

        private void CleanAtPosition(Vector3 worldPosition, float radius, float power)
        {
            // Clean window mesh
            windowMesh.CleanArea(worldPosition, radius, power * Time.deltaTime);

            // Clean hazards
            hazardRenderer.CleanHazardArea(worldPosition, radius, power * Time.deltaTime);
        }

        private void DetectCircleMotion(Vector3 currentPos)
        {
            Vector3 fromCenter = currentPos - circleStartPos;
            float distanceFromCenter = fromCenter.magnitude;

            // Check if we're roughly at circle radius
            if (Mathf.Abs(distanceFromCenter - circleDetectionRadius) < circleDetectionRadius * 0.3f)
            {
                if (circlePoints > 0)
                {
                    Vector3 lastFromCenter = lastCleanPosition - circleStartPos;
                    float angle = Vector3.SignedAngle(lastFromCenter, fromCenter, Vector3.forward);
                    circleAngleSum += Mathf.Abs(angle * Mathf.Deg2Rad);
                }

                circlePoints++;
            }
        }

        #endregion

        #region Gesture Handlers

        private void HandleSwipe(Vector2 startPos, Vector2 endPos)
        {
            Vector3 worldStart = ScreenToWorldPosition(startPos);
            Vector3 worldEnd = ScreenToWorldPosition(endPos);
            Vector2 direction = (endPos - startPos).normalized;

            UnityEngine.Debug.Log($"[CleaningController] Swipe detected: {direction}");

            // Squeegee effect for long swipes
            if (Vector3.Distance(worldStart, worldEnd) > 2f)
            {
                VFXManager.Instance?.PlaySqueegeStreakEffect(worldStart, worldEnd);
            }
        }

        private void HandleCircle(Vector2 center, float radius)
        {
            Vector3 worldCenter = ScreenToWorldPosition(center);

            UnityEngine.Debug.Log($"[CleaningController] Circle detected at {worldCenter}");

            // Extra powerful circle clean
            CleanAtPosition(worldCenter, circleCleanRadius, circleCleanPower);
            VFXManager.Instance?.PlayCirclePolishEffect(worldCenter);
            AudioManager.Instance?.PlayCircleScrubSFX();
        }

        private void HandleDoubleTap(Vector2 position)
        {
            Vector3 worldPosition = ScreenToWorldPosition(position);

            UnityEngine.Debug.Log($"[CleaningController] Double tap at {worldPosition}");

            // Spray bottle effect
            VFXManager.Instance?.PlaySprayBottleEffect(worldPosition);
            AudioManager.Instance?.PlaySFX(AudioEventType.SFX_Spray_Water);
        }

        #endregion

        #region Utility

        private Vector3 ScreenToWorldPosition(Vector2 screenPosition)
        {
            if (mainCamera == null) mainCamera = Camera.main;

            Ray ray = mainCamera.ScreenPointToRay(screenPosition);
            Plane windowPlane = new Plane(Vector3.back, windowMesh.transform.position);

            if (windowPlane.Raycast(ray, out float distance))
            {
                return ray.GetPoint(distance);
            }

            return Vector3.zero;
        }

        #endregion

        #region Public API

        public void StartLevel(LevelData level)
        {
            UnityEngine.Debug.Log($"[CleaningController] Starting level {level.levelNumber}");

            // Reset window
            windowMesh.ResetWindow();

            // Spawn hazards - Convert HazardType list to Hazard list
            List<Hazard> hazardList = new List<Hazard>();
            foreach (var hazardType in level.hazards)
            {
                hazardList.Add(new Hazard
                {
                    type = hazardType,
                    position = new Vector2(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f)),
                    size = Random.Range(0.3f, 0.8f),
                    cleanDifficulty = Random.Range(0.5f, 1.0f),
                    regenRate = 0f
                });
            }
            hazardRenderer.SpawnHazards(hazardList);

            // Activate
            IsActive = true;
        }

        public void EndLevel()
        {
            UnityEngine.Debug.Log("[CleaningController] Ending level");

            IsActive = false;

            // Clear hazards
            hazardRenderer.ClearAllHazards();
        }

        public float GetCurrentCleanPercentage()
        {
            return windowMesh.CurrentCleanPercentage;
        }

        public void SetCleaningPower(float multiplier)
        {
            swipeCleanPower *= multiplier;
            circleCleanPower *= multiplier;
            squeegeeCleanPower *= multiplier;
        }

        public void ResetCleaningPower()
        {
            // Reset to default values (would load from config)
            swipeCleanPower = 0.3f;
            circleCleanPower = 0.5f;
            squeegeeCleanPower = 0.4f;
        }

        #endregion

        #region Power-Ups

        public void ActivateNuke()
        {
            UnityEngine.Debug.Log("[CleaningController] NUKE activated!");

            // Clean entire window instantly
            windowMesh.SetInitialDirtiness(0f);
            hazardRenderer.ClearAllHazards();

            VFXManager.Instance?.PlayNukeExplosionEffect(windowMesh.transform.position);
            AudioManager.Instance?.PlaySFX(AudioEventType.PowerUp_Nuke);
        }

        public void ActivateTurbo(float duration)
        {
            UnityEngine.Debug.Log($"[CleaningController] TURBO activated for {duration}s!");

            StartCoroutine(TurboCoroutine(duration));
        }

        private System.Collections.IEnumerator TurboCoroutine(float duration)
        {
            SetCleaningPower(2.5f);
            VFXManager.Instance?.PlayTurboTrailEffect(transform);

            yield return new WaitForSeconds(duration);

            ResetCleaningPower();
        }

        public void ActivateAutoPilot(float duration)
        {
            UnityEngine.Debug.Log($"[CleaningController] AUTO-PILOT activated for {duration}s!");

            StartCoroutine(AutoPilotCoroutine(duration));
        }

        private System.Collections.IEnumerator AutoPilotCoroutine(float duration)
        {
            VFXManager.Instance?.PlayAutoPilotGlowEffect(transform);

            float elapsed = 0f;

            while (elapsed < duration)
            {
                // Auto-clean random spots
                Vector2 randomPos = new Vector2(Random.value, Random.value);
                Vector3 worldPos = windowMesh.transform.position + new Vector3(
                    (randomPos.x - 0.5f) * windowMesh.WindowSize.x,
                    (randomPos.y - 0.5f) * windowMesh.WindowSize.y,
                    0f
                );

                CleanAtPosition(worldPos, swipeCleanRadius, swipeCleanPower * 5f);

                yield return new WaitForSeconds(0.1f);
                elapsed += 0.1f;
            }
        }

        #endregion

        #region Debug

        [ContextMenu("Test Start Level")]
        private void TestStartLevel()
        {
            // Generate test level
            LevelData testLevel = LevelGenerator.Instance?.GenerateLevel(1);
            if (testLevel != null)
            {
                StartLevel(testLevel);
            }
        }

        [ContextMenu("Test Nuke")]
        private void TestNuke()
        {
            ActivateNuke();
        }

        [ContextMenu("Test Turbo")]
        private void TestTurbo()
        {
            ActivateTurbo(5f);
        }

        #endregion
    }
}








