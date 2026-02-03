using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WhenImCleaningWindows.Core;
using WhenImCleaningWindows.Monetization;

namespace WhenImCleaningWindows.UI
{
    /// <summary>
    /// Level Complete UI - Shows stars earned, coins/gems rewards, and next level button.
    /// </summary>
    public class LevelCompleteUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private GameObject[] starObjects = new GameObject[3];
        [SerializeField] private TextMeshProUGUI coinsEarnedText;
        [SerializeField] private TextMeshProUGUI gemsEarnedText;
        [SerializeField] private TextMeshProUGUI timeRemainingText;
        [SerializeField] private TextMeshProUGUI cleanPercentageText;
        [SerializeField] private GameObject elegantBadge;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button menuButton;
        
        [Header("Animation")]
        [SerializeField] private float starAnimationDelay = 0.3f;
        [SerializeField] private float starPopDuration = 0.5f;
        [SerializeField] private AnimationCurve starPopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private GameManager gameManager;
        private LevelResult currentResult;
        
        private void Start()
        {
            StartCoroutine(DeferredInitialization());
        }
        
        private System.Collections.IEnumerator DeferredInitialization()
        {
            yield return null;
            
            AutoAssignReferences();
            gameManager = GameManager.Instance;
            
            // Subscribe to level complete event
            if (gameManager != null)
            {
                GameManager.OnLevelCompleted += ShowResults;
            }
            
            // Setup buttons
            if (nextLevelButton != null)
            {
                nextLevelButton.onClick.AddListener(OnNextLevelClicked);
            }
            
            if (retryButton != null)
            {
                retryButton.onClick.AddListener(OnRetryClicked);
            }
            
            if (menuButton != null)
            {
                menuButton.onClick.AddListener(OnMenuClicked);
            }
        }
        
        private void AutoAssignReferences()
        {
            levelNumberText ??= FindChildComponent<TextMeshProUGUI>("LevelNumberText");
            titleText ??= FindChildComponent<TextMeshProUGUI>("TitleText");
            coinsEarnedText ??= FindChildComponent<TextMeshProUGUI>("CoinsEarnedText");
            gemsEarnedText ??= FindChildComponent<TextMeshProUGUI>("GemsEarnedText");
            timeRemainingText ??= FindChildComponent<TextMeshProUGUI>("TimeRemainingText");
            cleanPercentageText ??= FindChildComponent<TextMeshProUGUI>("CleanPercentageText");
            elegantBadge ??= FindChildObject("ElegantBadge");

            GameObject star1 = FindChildObject("Star1");
            GameObject star2 = FindChildObject("Star2");
            GameObject star3 = FindChildObject("Star3");
            if (starObjects == null || starObjects.Length != 3)
            {
                starObjects = new GameObject[3];
            }
            if (starObjects.Length >= 3)
            {
                starObjects[0] ??= star1;
                starObjects[1] ??= star2;
                starObjects[2] ??= star3;
            }

            nextLevelButton ??= FindChildComponent<Button>("NextLevelButton");
            retryButton ??= FindChildComponent<Button>("RetryButton");
            menuButton ??= FindChildComponent<Button>("MenuButton");
        }

        private T FindChildComponent<T>(string name) where T : Component
        {
            Transform child = FindChildRecursive(transform, name);
            return child != null ? child.GetComponent<T>() : null;
        }

        private GameObject FindChildObject(string name)
        {
            Transform child = FindChildRecursive(transform, name);
            return child != null ? child.gameObject : null;
        }

        private Transform FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>
        /// Show level results.
        /// </summary>
        private void ShowResults(LevelResult result)
        {
            currentResult = result;
            
            // Level number
            if (levelNumberText != null)
            {
                levelNumberText.text = $"Level {result.levelNumber}";
            }
            
            // Title based on stars
            if (titleText != null)
            {
                titleText.text = GetTitleText(result.stars);
            }
            
            // Stats
            if (timeRemainingText != null)
            {
                int minutes = Mathf.FloorToInt(result.timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(result.timeRemaining % 60f);
                timeRemainingText.text = $"Time: {minutes:00}:{seconds:00}";
            }
            
            if (cleanPercentageText != null)
            {
                cleanPercentageText.text = $"{result.cleanPercentage:F1}% Clean";
            }
            
            // Rewards
            if (coinsEarnedText != null)
            {
                coinsEarnedText.text = $"+{result.coinsEarned}";
            }
            
            if (gemsEarnedText != null && result.gemsEarned > 0)
            {
                gemsEarnedText.text = $"+{result.gemsEarned}";
                gemsEarnedText.gameObject.SetActive(true);
            }
            else if (gemsEarnedText != null)
            {
                gemsEarnedText.gameObject.SetActive(false);
            }
            
            // Elegant badge
            if (elegantBadge != null)
            {
                elegantBadge.SetActive(result.wasElegant);
            }
            
            // Animate stars
            StartCoroutine(AnimateStars(result.stars));
        }
        
        /// <summary>
        /// Get title text based on stars.
        /// </summary>
        private string GetTitleText(int stars)
        {
            switch (stars)
            {
                case 3: return "PERFECT!";
                case 2: return "WELL DONE!";
                case 1: return "CLEARED!";
                default: return "COMPLETED";
            }
        }
        
        /// <summary>
        /// Animate stars one by one.
        /// </summary>
        private System.Collections.IEnumerator AnimateStars(int starsEarned)
        {
            // Hide all stars initially
            foreach (var star in starObjects)
            {
                if (star != null)
                {
                    star.SetActive(false);
                    star.transform.localScale = Vector3.zero;
                }
            }
            
            // Animate each earned star
            for (int i = 0; i < starsEarned && i < starObjects.Length; i++)
            {
                if (starObjects[i] != null)
                {
                    starObjects[i].SetActive(true);
                    
                    // Pop animation
                    float elapsed = 0f;
                    while (elapsed < starPopDuration)
                    {
                        elapsed += Time.deltaTime;
                        float t = starPopCurve.Evaluate(elapsed / starPopDuration);
                        starObjects[i].transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 1.2f, t);
                        
                        yield return null;
                    }
                    
                    // Settle back to normal size
                    starObjects[i].transform.localScale = Vector3.one;
                    
                    // Play SFX (in production)
                    // AudioManager.PlaySFX("star_pop");
                    
                    // Wait before next star
                    yield return new WaitForSeconds(starAnimationDelay);
                }
            }
        }
        
        /// <summary>
        /// Handle next level button.
        /// </summary>
        private void OnNextLevelClicked()
        {
            if (gameManager != null)
            {
                gameManager.NextLevel();
            }
        }
        
        /// <summary>
        /// Handle retry button.
        /// </summary>
        private void OnRetryClicked()
        {
            if (gameManager != null)
            {
                gameManager.RetryLevel();
            }
        }
        
        /// <summary>
        /// Handle menu button.
        /// </summary>
        private void OnMenuClicked()
        {
            UIManager.Instance?.ShowScreen(UIScreen.MainMenu, false);
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (gameManager != null)
            {
                GameManager.OnLevelCompleted -= ShowResults;
            }
        }
    }
}








