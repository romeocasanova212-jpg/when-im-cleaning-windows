using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

namespace WhenImCleaningWindows.Visual
{
    /// <summary>
    /// Animation Manager for all UI transitions, tweens, and visual feedback.
    /// Uses DOTween for smooth animations (fallback to coroutines if DOTween not installed).
    /// </summary>
    public class AnimationManager : MonoBehaviour
    {
        public static AnimationManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private bool useDOTween = true;
        [SerializeField] private float defaultDuration = 0.3f;
        [SerializeField] private Ease defaultEase = Ease.OutQuad;

        [Header("Animation Curves (Fallback)")]
        [SerializeField] private AnimationCurve easeInOutCurve;
        [SerializeField] private AnimationCurve elasticCurve;
        [SerializeField] private AnimationCurve bounceCurve;

        private Dictionary<Transform, Coroutine> activeAnimations;
        private bool isDOTweenAvailable = false;

        #region Initialization

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeAnimationSystem();
        }

        private void InitializeAnimationSystem()
        {
            UnityEngine.Debug.Log("[AnimationManager] Initializing animation system...");

            activeAnimations = new Dictionary<Transform, Coroutine>();

            // Check if DOTween is available
            #if DOTWEEN_ENABLED
            isDOTweenAvailable = true;
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            UnityEngine.Debug.Log("[AnimationManager] ✓ DOTween initialized");
            #else
            isDOTweenAvailable = false;
            UnityEngine.Debug.Log("[AnimationManager] DOTween not available - using coroutine fallbacks");
            #endif

            // Initialize default curves if not set
            if (easeInOutCurve == null || easeInOutCurve.length == 0)
            {
                easeInOutCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            }

            if (elasticCurve == null || elasticCurve.length == 0)
            {
                elasticCurve = CreateElasticCurve();
            }

            if (bounceCurve == null || bounceCurve.length == 0)
            {
                bounceCurve = CreateBounceCurve();
            }

            UnityEngine.Debug.Log("[AnimationManager] ✓ Animation system initialized");
        }

        #endregion

        #region Scale Animations

        public void ScalePop(Transform target, float scale = 1.2f, float duration = 0.3f, System.Action onComplete = null)
        {
            if (target == null) return;

            Vector3 originalScale = target.localScale;
            Vector3 popScale = originalScale * scale;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                target.DOKill();
                target.DOScale(popScale, duration * 0.5f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() =>
                    {
                        target.DOScale(originalScale, duration * 0.5f)
                            .SetEase(Ease.InBack)
                            .OnComplete(() => onComplete?.Invoke());
                    });
                #endif
            }
            else
            {
                StopAnimationIfActive(target);
                Coroutine anim = StartCoroutine(ScalePopCoroutine(target, originalScale, popScale, duration, onComplete));
                activeAnimations[target] = anim;
            }
        }

        private IEnumerator ScalePopCoroutine(Transform target, Vector3 originalScale, Vector3 popScale, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            float halfDuration = duration * 0.5f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elasticCurve.Evaluate(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(originalScale, popScale, t);
                yield return null;
            }

            elapsed = 0f;

            // Scale down
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                float t = elasticCurve.Evaluate(elapsed / halfDuration);
                target.localScale = Vector3.Lerp(popScale, originalScale, t);
                yield return null;
            }

            target.localScale = originalScale;
            onComplete?.Invoke();
            activeAnimations.Remove(target);
        }

        public void ScalePulse(Transform target, float minScale = 0.9f, float maxScale = 1.1f, float duration = 1f)
        {
            if (target == null) return;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                target.DOKill();
                target.DOScale(maxScale, duration * 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                #endif
            }
            else
            {
                StopAnimationIfActive(target);
                Coroutine anim = StartCoroutine(ScalePulseCoroutine(target, minScale, maxScale, duration));
                activeAnimations[target] = anim;
            }
        }

        private IEnumerator ScalePulseCoroutine(Transform target, float minScale, float maxScale, float duration)
        {
            float halfDuration = duration * 0.5f;

            while (true)
            {
                float elapsed = 0f;

                // Scale up
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = easeInOutCurve.Evaluate(elapsed / halfDuration);
                    float scale = Mathf.Lerp(minScale, maxScale, t);
                    target.localScale = Vector3.one * scale;
                    yield return null;
                }

                elapsed = 0f;

                // Scale down
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = easeInOutCurve.Evaluate(elapsed / halfDuration);
                    float scale = Mathf.Lerp(maxScale, minScale, t);
                    target.localScale = Vector3.one * scale;
                    yield return null;
                }
            }
        }

        #endregion

        #region Fade Animations

        public void FadeIn(CanvasGroup canvasGroup, float duration = 0.3f, System.Action onComplete = null)
        {
            if (canvasGroup == null) return;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                canvasGroup.DOKill();
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, duration)
                    .SetEase(defaultEase)
                    .OnComplete(() => onComplete?.Invoke());
                #endif
            }
            else
            {
                StartCoroutine(FadeCoroutine(canvasGroup, 0f, 1f, duration, onComplete));
            }
        }

        public void FadeOut(CanvasGroup canvasGroup, float duration = 0.3f, System.Action onComplete = null)
        {
            if (canvasGroup == null) return;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                canvasGroup.DOKill();
                canvasGroup.DOFade(0f, duration)
                    .SetEase(defaultEase)
                    .OnComplete(() => onComplete?.Invoke());
                #endif
            }
            else
            {
                StartCoroutine(FadeCoroutine(canvasGroup, canvasGroup.alpha, 0f, duration, onComplete));
            }
        }

        private IEnumerator FadeCoroutine(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            canvasGroup.alpha = startAlpha;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = easeInOutCurve.Evaluate(elapsed / duration);
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                yield return null;
            }

            canvasGroup.alpha = endAlpha;
            onComplete?.Invoke();
        }

        #endregion

        #region Move Animations

        public void MoveToPosition(Transform target, Vector3 endPosition, float duration = 0.5f, System.Action onComplete = null)
        {
            if (target == null) return;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                target.DOKill();
                target.DOMove(endPosition, duration)
                    .SetEase(defaultEase)
                    .OnComplete(() => onComplete?.Invoke());
                #endif
            }
            else
            {
                StopAnimationIfActive(target);
                Coroutine anim = StartCoroutine(MoveCoroutine(target, target.position, endPosition, duration, onComplete));
                activeAnimations[target] = anim;
            }
        }

        public void MoveLocalToPosition(Transform target, Vector3 endPosition, float duration = 0.5f, System.Action onComplete = null)
        {
            if (target == null) return;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                target.DOKill();
                target.DOLocalMove(endPosition, duration)
                    .SetEase(defaultEase)
                    .OnComplete(() => onComplete?.Invoke());
                #endif
            }
            else
            {
                StopAnimationIfActive(target);
                Coroutine anim = StartCoroutine(MoveLocalCoroutine(target, target.localPosition, endPosition, duration, onComplete));
                activeAnimations[target] = anim;
            }
        }

        private IEnumerator MoveCoroutine(Transform target, Vector3 startPos, Vector3 endPos, float duration, System.Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = easeInOutCurve.Evaluate(elapsed / duration);
                target.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            target.position = endPos;
            onComplete?.Invoke();
            activeAnimations.Remove(target);
        }

        private IEnumerator MoveLocalCoroutine(Transform target, Vector3 startPos, Vector3 endPos, float duration, System.Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = easeInOutCurve.Evaluate(elapsed / duration);
                target.localPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            target.localPosition = endPos;
            onComplete?.Invoke();
            activeAnimations.Remove(target);
        }

        #endregion

        #region Rotation Animations

        public void RotateSpin(Transform target, float duration = 1f, int loops = -1)
        {
            if (target == null) return;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                target.DOKill();
                target.DORotate(new Vector3(0f, 0f, 360f), duration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
                    .SetLoops(loops, LoopType.Restart);
                #endif
            }
            else
            {
                StopAnimationIfActive(target);
                Coroutine anim = StartCoroutine(RotateSpinCoroutine(target, duration));
                activeAnimations[target] = anim;
            }
        }

        private IEnumerator RotateSpinCoroutine(Transform target, float duration)
        {
            while (true)
            {
                float elapsed = 0f;
                Quaternion startRotation = target.localRotation;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float angle = (elapsed / duration) * 360f;
                    target.localRotation = Quaternion.Euler(0f, 0f, angle);
                    yield return null;
                }
            }
        }

        #endregion

        #region Shake Animations

        public void ShakePosition(Transform target, float strength = 0.5f, float duration = 0.3f, int vibrato = 10)
        {
            if (target == null) return;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                target.DOKill();
                target.DOShakePosition(duration, strength, vibrato);
                #endif
            }
            else
            {
                StopAnimationIfActive(target);
                Coroutine anim = StartCoroutine(ShakeCoroutine(target, strength, duration));
                activeAnimations[target] = anim;
            }
        }

        private IEnumerator ShakeCoroutine(Transform target, float strength, float duration)
        {
            Vector3 originalPosition = target.localPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float x = Random.Range(-strength, strength);
                float y = Random.Range(-strength, strength);
                target.localPosition = originalPosition + new Vector3(x, y, 0f);
                yield return null;
            }

            target.localPosition = originalPosition;
            activeAnimations.Remove(target);
        }

        #endregion

        #region Bounce Animations

        public void BounceIn(Transform target, float duration = 0.5f, System.Action onComplete = null)
        {
            if (target == null) return;

            target.localScale = Vector3.zero;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                target.DOKill();
                target.DOScale(Vector3.one, duration)
                    .SetEase(Ease.OutBounce)
                    .OnComplete(() => onComplete?.Invoke());
                #endif
            }
            else
            {
                StopAnimationIfActive(target);
                Coroutine anim = StartCoroutine(BounceInCoroutine(target, duration, onComplete));
                activeAnimations[target] = anim;
            }
        }

        private IEnumerator BounceInCoroutine(Transform target, float duration, System.Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = bounceCurve.Evaluate(elapsed / duration);
                target.localScale = Vector3.one * t;
                yield return null;
            }

            target.localScale = Vector3.one;
            onComplete?.Invoke();
            activeAnimations.Remove(target);
        }

        #endregion

        #region Color Animations

        public void ColorFade(Image image, Color endColor, float duration = 0.3f, System.Action onComplete = null)
        {
            if (image == null) return;

            if (isDOTweenAvailable && useDOTween)
            {
                #if DOTWEEN_ENABLED
                image.DOKill();
                image.DOColor(endColor, duration)
                    .SetEase(defaultEase)
                    .OnComplete(() => onComplete?.Invoke());
                #endif
            }
            else
            {
                StartCoroutine(ColorFadeCoroutine(image, image.color, endColor, duration, onComplete));
            }
        }

        private IEnumerator ColorFadeCoroutine(Image image, Color startColor, Color endColor, float duration, System.Action onComplete)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = easeInOutCurve.Evaluate(elapsed / duration);
                image.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            image.color = endColor;
            onComplete?.Invoke();
        }

        #endregion

        #region Helpers

        private void StopAnimationIfActive(Transform target)
        {
            if (activeAnimations.TryGetValue(target, out Coroutine anim))
            {
                StopCoroutine(anim);
                activeAnimations.Remove(target);
            }
        }

        public void StopAllAnimations()
        {
            foreach (var kvp in activeAnimations)
            {
                if (kvp.Value != null)
                {
                    StopCoroutine(kvp.Value);
                }
            }
            activeAnimations.Clear();
        }

        private AnimationCurve CreateElasticCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.5f, 1.1f, 0f, 0f),
                new Keyframe(0.75f, 0.95f, 0f, 0f),
                new Keyframe(1f, 1f, 0f, 0f)
            );
        }

        private AnimationCurve CreateBounceCurve()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 0f),
                new Keyframe(0.5f, 1.2f, 0f, 0f),
                new Keyframe(0.75f, 0.9f, 0f, 0f),
                new Keyframe(0.9f, 1.05f, 0f, 0f),
                new Keyframe(1f, 1f, 0f, 0f)
            );
        }

        #endregion

        #region Debug Context Menu

        [ContextMenu("Test Animations")]
        private void TestAnimations()
        {
            UnityEngine.Debug.Log("=== Testing Animation System ===");
            UnityEngine.Debug.Log($"DOTween Available: {isDOTweenAvailable}");
            UnityEngine.Debug.Log($"Using DOTween: {useDOTween}");
            UnityEngine.Debug.Log($"Active Animations: {activeAnimations.Count}");
            UnityEngine.Debug.Log("=================================");
        }

        #endregion
    }
}








