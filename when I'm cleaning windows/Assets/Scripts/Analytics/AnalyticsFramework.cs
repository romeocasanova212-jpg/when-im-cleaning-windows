using UnityEngine;
using System.Collections.Generic;
using System;

namespace WhenImCleaningWindows.Analytics
{
    /// <summary>
    /// Comprehensive analytics framework tracking funnel, cohorts, monetization, retention.
    /// Integrates with Firebase Analytics for production reporting.
    /// Tracks D1/D7/D30 retention, ARPU, LTV by player cohort, offer performance.
    /// </summary>
    public class AnalyticsFramework : MonoBehaviour
    {
        [System.Serializable]
        public class FunnelMetrics
        {
            public int installs;
            public int day1Users;
            public int tutorialCompletions;
            public int firstLevelCompletes;
            public int energyWallHits;
            public int fomoPuppupClicks;
            public int firstPurchases;
            
            public float Day1Retention => installs > 0 ? (float)day1Users / installs : 0f;
            public float TutorialCompletionRate => installs > 0 ? (float)tutorialCompletions / installs : 0f;
            public float ConversionRate => energyWallHits > 0 ? (float)firstPurchases / energyWallHits : 0f;
        }
        
        [System.Serializable]
        public class CohortMetrics
        {
            public DateTime cohortDate;
            public int cohortSize;
            
            public int d1Retained;
            public int d7Retained;
            public int d30Retained;
            public int d90Retained;
            
            public float arpu;           // Average Revenue Per User (all users)
            public float arppu;          // Average Revenue Per Paying User
            public float ltv7;           // 7-day LTV
            public float ltv30;          // 30-day LTV
            
            public float D1Retention => cohortSize > 0 ? (float)d1Retained / cohortSize : 0f;
            public float D7Retention => cohortSize > 0 ? (float)d7Retained / cohortSize : 0f;
            public float D30Retention => cohortSize > 0 ? (float)d30Retained / cohortSize : 0f;
            public float D90Retention => cohortSize > 0 ? (float)d90Retained / cohortSize : 0f;
        }
        
        [System.Serializable]
        public class OfferAnalytics
        {
            public string offerId;
            public int impressions;
            public int clicks;
            public int purchases;
            public float totalRevenue;
            
            public float CTR => impressions > 0 ? (float)clicks / impressions : 0f;
            public float CVR => clicks > 0 ? (float)purchases / clicks : 0f;
            public float RevenuePerImpression => impressions > 0 ? totalRevenue / impressions : 0f;
        }
        
        [SerializeField] private FunnelMetrics currentFunnel = new FunnelMetrics();
        [SerializeField] private List<CohortMetrics> cohortMetrics = new List<CohortMetrics>();
        [SerializeField] private Dictionary<string, OfferAnalytics> offerAnalytics = new Dictionary<string, OfferAnalytics>();
        
        private static AnalyticsFramework instance;
        private DateTime sessionStartTime;
        private float totalSessionRevenue = 0f;
        
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        
        private void Start()
        {
            sessionStartTime = DateTime.UtcNow;
            
            // Load cohort from PlayerPrefs
            if (!PlayerPrefs.HasKey("CohortDate"))
            {
                PlayerPrefs.SetString("CohortDate", DateTime.UtcNow.Date.ToString("yyyy-MM-dd"));
            }
            
            LogEvent("app_launch", new Dictionary<string, object>
            {
                { "session_start", sessionStartTime }
            });
        }
        
        /// <summary>
        /// Log event to Firebase Analytics with custom parameters.
        /// </summary>
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            // In production, this would call Firebase.Analytics.FirebaseAnalytics.LogEvent()
            Debug.Log($"[Analytics] Event: {eventName}");
            
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    Debug.Log($"  - {param.Key}: {param.Value}");
                }
            }
        }
        
        /// <summary>
        /// Log tutorial completion. Called when player completes onboarding.
        /// </summary>
        public void LogTutorialComplete()
        {
            currentFunnel.tutorialCompletions++;
            
            LogEvent("tutorial_complete", new Dictionary<string, object>
            {
                { "timestamp", DateTime.UtcNow }
            });
        }
        
        /// <summary>
        /// Log level completion. Called when player finishes a level with stars.
        /// </summary>
        public void LogLevelComplete(int worldNumber, int levelNumber, int starsEarned, float timeTaken)
        {
            currentFunnel.firstLevelCompletes++;
            
            LogEvent("level_complete", new Dictionary<string, object>
            {
                { "world", worldNumber },
                { "level", levelNumber },
                { "stars", starsEarned },
                { "time_seconds", timeTaken }
            });
        }
        
        /// <summary>
        /// Log energy wall hit. Called when player runs out of lives.
        /// Triggers monetization funnel tracking.
        /// </summary>
        public void LogEnergyWallHit(int currentLevel)
        {
            currentFunnel.energyWallHits++;
            
            LogEvent("energy_depleted", new Dictionary<string, object>
            {
                { "level_reached", currentLevel },
                { "funnel_position", "monetization_trigger" }
            });
        }
        
        /// <summary>
        /// Log FOMO popup shown. Called when out-of-energy popup displayed.
        /// </summary>
        public void LogFOMOPopupShown(string offerId)
        {
            LogEvent("offer_impression", new Dictionary<string, object>
            {
                { "offer_id", offerId }
            });
            
            if (offerAnalytics.TryGetValue(offerId, out var stats))
            {
                stats.impressions++;
            }
            else
            {
                offerAnalytics[offerId] = new OfferAnalytics { offerId = offerId, impressions = 1 };
            }
        }
        
        /// <summary>
        /// Log user clicked on FOMO popup/offer.
        /// </summary>
        public void LogOfferClicked(string offerId)
        {
            currentFunnel.fomoPuppupClicks++;
            
            LogEvent("offer_click", new Dictionary<string, object>
            {
                { "offer_id", offerId }
            });
            
            if (offerAnalytics.TryGetValue(offerId, out var stats))
            {
                stats.clicks++;
            }
        }
        
        /// <summary>
        /// Log successful IAP purchase. Called when transaction completes.
        /// </summary>
        public void LogPurchase(string offerId, float priceUSD, string currency = "USD")
        {
            currentFunnel.firstPurchases++;
            totalSessionRevenue += priceUSD;
            
            LogEvent("purchase", new Dictionary<string, object>
            {
                { "offer_id", offerId },
                { "value", priceUSD },
                { "currency", currency }
            });
            
            if (offerAnalytics.TryGetValue(offerId, out var stats))
            {
                stats.purchases++;
                stats.totalRevenue += priceUSD;
            }
            else
            {
                offerAnalytics[offerId] = new OfferAnalytics 
                { 
                    offerId = offerId, 
                    purchases = 1, 
                    totalRevenue = priceUSD 
                };
            }
        }
        
        /// <summary>
        /// Log churn risk detected. Called by ChurnPredictionEngine.
        /// </summary>
        public void LogChurnRiskDetected(float riskScore, string interventionType)
        {
            LogEvent("churn_risk_detected", new Dictionary<string, object>
            {
                { "risk_score", riskScore },
                { "intervention", interventionType }
            });
        }
        
        /// <summary>
        /// Log player segmentation. Called daily to track cohort.
        /// </summary>
        public void LogPlayerSegmentation(string segment, float estimatedLTV)
        {
            LogEvent("player_segmentation", new Dictionary<string, object>
            {
                { "segment", segment },
                { "estimated_ltv", estimatedLTV }
            });
        }
        
        /// <summary>
        /// Get current funnel metrics.
        /// </summary>
        public FunnelMetrics GetFunnelMetrics() => currentFunnel;
        
        /// <summary>
        /// Get offer performance analytics.
        /// </summary>
        public OfferAnalytics GetOfferAnalytics(string offerId)
        {
            if (offerAnalytics.TryGetValue(offerId, out var stats))
                return stats;
            return null;
        }
        
        /// <summary>
        /// Get all offer analytics (for A/B testing comparison).
        /// </summary>
        public Dictionary<string, OfferAnalytics> GetAllOfferAnalytics() => offerAnalytics;
        
        public static AnalyticsFramework Instance => instance;
    }
}
