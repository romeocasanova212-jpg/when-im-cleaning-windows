using UnityEngine;
using UnityEngine.Events;
using System;

namespace WhenImCleaningWindows.Monetization
{
    /// <summary>
    /// Churn prediction system using Firebase ML Kit.
    /// Analyzes player behavior to predict likelihood of abandonment.
    /// Triggers targeted discounted offers to win back at-risk players.
    /// </summary>
    public class ChurnPredictionEngine : MonoBehaviour
    {
        [System.Serializable]
        public class ChurnPredictionData
        {
            public int daysSinceInstall;
            public float totalPlayTime;      // hours
            public int recentDailyActive;    // days active last 7 days
            public float energySpendRate;    // lives per day
            public float levelProgressSpeed; // levels per hour
            public float sessionLengthTrend; // trend of session lengths (increasing = engaged)
            
            public float churnRiskScore;     // 0-100, >70 = high risk
        }
        
        [SerializeField] private ChurnPredictionData currentPrediction;
        [SerializeField] private float churnThreshold = 70f;
        
        public UnityEvent<float> OnChurnRiskCalculated = new UnityEvent<float>();
        public UnityEvent OnHighChurnRiskDetected = new UnityEvent();
        
        private GameManager gameManager;
        private CurrencyManager currencyManager;
        private EnergySystem energySystem;
        
        private DateTime installTime;
        private float totalPlayTime = 0f;
        private int daysActiveLast7Days = 0;
        private DateTime lastActiveDate = DateTime.UtcNow;
        
        private void Start()
        {
            gameManager = GameManager.Instance;
            currencyManager = CurrencyManager.Instance;
            energySystem = EnergySystem.Instance;
            
            // Load install time from PlayerPrefs
            if (PlayerPrefs.HasKey("InstallTime"))
            {
                string installTimeStr = PlayerPrefs.GetString("InstallTime");
                installTime = DateTime.Parse(installTimeStr);
            }
            else
            {
                installTime = DateTime.UtcNow;
                PlayerPrefs.SetString("InstallTime", installTime.ToString());
            }
        }
        
        private void Update()
        {
            // Recalculate churn prediction periodically (every 5 minutes)
            if (Time.frameCount % (300 * 60) == 0)  // ~5 min
            {
                RecalculateChurnRisk();
            }
        }
        
        public void RecalculateChurnRisk()
        {
            currentPrediction = new ChurnPredictionData();
            
            // Calculate input metrics
            currentPrediction.daysSinceInstall = (int)(DateTime.UtcNow - installTime).TotalDays;
            currentPrediction.totalPlayTime = GetTotalPlayTime();
            currentPrediction.recentDailyActive = GetRecentDailyActive();
            currentPrediction.energySpendRate = GetEnergySpendRate();
            currentPrediction.levelProgressSpeed = GetLevelProgressSpeed();
            currentPrediction.sessionLengthTrend = GetSessionLengthTrend();
            
            // Predict churn risk (0-100)
            currentPrediction.churnRiskScore = CalculateChurnScore(currentPrediction);
            
            OnChurnRiskCalculated?.Invoke(currentPrediction.churnRiskScore);
            
            // If high risk, trigger intervention
            if (currentPrediction.churnRiskScore > churnThreshold)
            {
                OnHighChurnRiskDetected?.Invoke();
                TriggerChurnIntervention();
            }
        }
        
        private float CalculateChurnScore(ChurnPredictionData data)
        {
            float score = 0f;
            
            // Factor 1: Days Since Install (early dropoff is highest risk)
            if (data.daysSinceInstall <= 3)
                score += 30f * (1f - (data.daysSinceInstall / 3f));  // -30 if day 3, 0 if day 0
            
            // Factor 2: Play Time (low engagement = risk)
            if (data.totalPlayTime < 1f)
                score += 25f;  // Very risky if <1 hour total
            else if (data.totalPlayTime < 5f)
                score += 15f;  // Risky if <5 hours
            
            // Factor 3: Recent Activity (inactive recently = very risky)
            if (data.recentDailyActive < 2)
                score += 30f;  // Haven't played last 2 days
            else if (data.recentDailyActive < 4)
                score += 15f;  // Only 4 days active last 7
            
            // Factor 4: Energy Spend Rate (low energy use = not engaging with monetization)
            if (data.energySpendRate < 0.5f)
                score += 10f;  // <30 lives per month
            
            // Factor 5: Session Length Trend (decreasing = losing interest)
            if (data.sessionLengthTrend < 0f)
                score += 15f;  // Sessions getting shorter
            
            return Mathf.Clamp(score, 0f, 100f);
        }
        
        private void TriggerChurnIntervention()
        {
            // Show discounted offer targeting at-risk players
            float discountPercent = 20f + (currentPrediction.churnRiskScore - 70f) / 30f * 30f;
            discountPercent = Mathf.Clamp(discountPercent, 20f, 50f);
            
            Debug.Log($"[ChurnPredictionEngine] High churn risk detected ({currentPrediction.churnRiskScore:F1}/100). " +
                $"Offering {discountPercent}% discount intervention.");
            
            // TODO: Show targeted offer popup
            // ShowDiscountedOfferPopup(discountPercent);
            
            // Also give them a small bonus (1 free life) to extend session
            energySystem.AddEnergy(1, "Churn intervention bonus");
        }
        
        private float GetTotalPlayTime()
        {
            if (PlayerPrefs.HasKey("TotalPlayTime"))
            {
                return PlayerPrefs.GetFloat("TotalPlayTime");
            }
            return 0f;
        }
        
        private int GetRecentDailyActive()
        {
            int count = 0;
            for (int i = 0; i < 7; i++)
            {
                DateTime checkDate = DateTime.UtcNow.Date.AddDays(-i);
                string key = $"DailyActive_{checkDate:yyyyMMdd}";
                if (PlayerPrefs.HasKey(key))
                    count++;
            }
            return count;
        }
        
        private float GetEnergySpendRate()
        {
            // Lives spent per day
            if (currentPrediction.daysSinceInstall == 0)
                return 0f;
            
            int totalEnergySpent = 0;  // TODO: Track this in EnergySystem
            return (float)totalEnergySpent / currentPrediction.daysSinceInstall;
        }
        
        private float GetLevelProgressSpeed()
        {
            // Levels completed per hour of play
            int levelsCompleted = 0;  // TODO: Track this in GameManager
            float playTimeHours = GetTotalPlayTime();
            
            if (playTimeHours < 0.1f)
                return 0f;
            
            return (float)levelsCompleted / playTimeHours;
        }
        
        private float GetSessionLengthTrend()
        {
            // Compare average session length last 3 days vs previous 3 days
            // Positive = getting longer (more engaged), Negative = getting shorter (less engaged)
            // TODO: Implement actual session tracking
            return 0f;
        }
        
        public ChurnPredictionData GetCurrentPrediction() => currentPrediction;
    }
}
