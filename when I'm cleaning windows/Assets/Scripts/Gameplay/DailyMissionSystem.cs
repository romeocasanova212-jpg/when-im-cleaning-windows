using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace WhenImCleaningWindows.Gameplay
{
    /// <summary>
    /// Advanced daily mission system for engagement and addictiveness.
    /// 3 rotating missions per day (Tier 1/2/3) with progressive difficulty.
    /// Each completion rewards gems, energy, or battle pass XP.
    /// </summary>
    [System.Serializable]
    public class MissionData
    {
        public enum MissionTier { Tier1 = 0, Tier2 = 1, Tier3 = 2 }
        public enum MissionType { LevelsCleared, StarsEarned, PowerUpsUsed, SprayCount, Combos, SpecificWorld }
        
        public string missionId;
        public string title;
        public string description;
        public MissionTier tier;
        public MissionType type;
        public int targetValue;
        public int currentProgress;
        public bool completed;
        
        public int rewardGems;
        public int rewardCoins;
        public int rewardBattlePassXP;
        
        public DateTime resetTime;
    }
    
    public class DailyMissionSystem : MonoBehaviour
    {
        [SerializeField] private List<MissionData> activeMissions = new List<MissionData>();
        [SerializeField] private int completedToday = 0;
        
        public UnityEvent<MissionData> OnMissionProgress = new UnityEvent<MissionData>();
        public UnityEvent<MissionData> OnMissionComplete = new UnityEvent<MissionData>();
        public UnityEvent OnAllTierMissionsComplete = new UnityEvent();
        
        private const int MISSIONS_PER_DAY = 3;
        private const int MISSIONS_PER_TIER = 4;
        private DateTime lastResetTime;
        
        // Mission pools (customizable)
        private List<MissionData> tier1Missions = new List<MissionData>();
        private List<MissionData> tier2Missions = new List<MissionData>();
        private List<MissionData> tier3Missions = new List<MissionData>();
        
        private void Awake()
        {
            InitializeMissionPools();
        }
        
        private void Start()
        {
            CheckAndResetMissions();
        }
        
        private void InitializeMissionPools()
        {
            // Tier 1: Easy missions (5-10 minutes)
            tier1Missions.Add(new MissionData
            {
                title = "Clean 5 Levels",
                description = "Complete any 5 levels",
                tier = MissionData.MissionTier.Tier1,
                type = MissionData.MissionType.LevelsCleared,
                targetValue = 5,
                rewardGems = 100,
                rewardCoins = 0,
                rewardBattlePassXP = 0
            });
            
            tier1Missions.Add(new MissionData
            {
                title = "Score 2+ Stars",
                description = "Get 2+ stars on any level",
                tier = MissionData.MissionTier.Tier1,
                type = MissionData.MissionType.StarsEarned,
                targetValue = 2,
                rewardGems = 100,
                rewardCoins = 0,
                rewardBattlePassXP = 0
            });
            
            tier1Missions.Add(new MissionData
            {
                title = "Use 3 Power-Ups",
                description = "Deploy 3 power-ups across levels",
                tier = MissionData.MissionTier.Tier1,
                type = MissionData.MissionType.PowerUpsUsed,
                targetValue = 3,
                rewardGems = 100,
                rewardCoins = 0,
                rewardBattlePassXP = 0
            });
            
            tier1Missions.Add(new MissionData
            {
                title = "Spray 10 Times",
                description = "Use spray gesture 10 times total",
                tier = MissionData.MissionTier.Tier1,
                type = MissionData.MissionType.SprayCount,
                targetValue = 10,
                rewardGems = 100,
                rewardCoins = 0,
                rewardBattlePassXP = 0
            });
            
            // Tier 2: Medium missions (15-20 minutes)
            tier2Missions.Add(new MissionData
            {
                title = "5-Level Streak",
                description = "Complete 5 levels without failure",
                tier = MissionData.MissionTier.Tier2,
                type = MissionData.MissionType.Combos,
                targetValue = 5,
                rewardGems = 250,
                rewardCoins = 50,
                rewardBattlePassXP = 10
            });
            
            tier2Missions.Add(new MissionData
            {
                title = "Double Star Spree",
                description = "Get 3-star on 2 levels",
                tier = MissionData.MissionTier.Tier2,
                type = MissionData.MissionType.StarsEarned,
                targetValue = 6,  // 3 stars × 2 levels
                rewardGems = 250,
                rewardCoins = 50,
                rewardBattlePassXP = 10
            });
            
            tier2Missions.Add(new MissionData
            {
                title = "Speed Cleaner",
                description = "Complete any level in under 30 seconds",
                tier = MissionData.MissionTier.Tier2,
                type = MissionData.MissionType.LevelsCleared,
                targetValue = 1,
                rewardGems = 250,
                rewardCoins = 50,
                rewardBattlePassXP = 10
            });
            
            tier2Missions.Add(new MissionData
            {
                title = "World Explorer",
                description = "Complete 10 levels in World 1-2",
                tier = MissionData.MissionTier.Tier2,
                type = MissionData.MissionType.SpecificWorld,
                targetValue = 10,
                rewardGems = 250,
                rewardCoins = 50,
                rewardBattlePassXP = 10
            });
            
            // Tier 3: Hard missions (30-45 minutes)
            tier3Missions.Add(new MissionData
            {
                title = "Star Collector",
                description = "Earn 10+ stars across levels with 2+ stars each",
                tier = MissionData.MissionTier.Tier3,
                type = MissionData.MissionType.StarsEarned,
                targetValue = 10,
                rewardGems = 500,
                rewardCoins = 100,
                rewardBattlePassXP = 25
            });
            
            tier3Missions.Add(new MissionData
            {
                title = "Combo Kingpin",
                description = "Maintain a 10-combo (across multiple sessions)",
                tier = MissionData.MissionTier.Tier3,
                type = MissionData.MissionType.Combos,
                targetValue = 10,
                rewardGems = 500,
                rewardCoins = 100,
                rewardBattlePassXP = 25
            });
            
            tier3Missions.Add(new MissionData
            {
                title = "Three Star Master",
                description = "Get 3-star on 3 levels (can be any world)",
                tier = MissionData.MissionTier.Tier3,
                type = MissionData.MissionType.StarsEarned,
                targetValue = 9,  // 3 stars × 3 levels
                rewardGems = 500,
                rewardCoins = 100,
                rewardBattlePassXP = 25
            });
            
            tier3Missions.Add(new MissionData
            {
                title = "World 5 Conqueror",
                description = "Complete 5 levels in World 5+ with 2+ stars each",
                tier = MissionData.MissionTier.Tier3,
                type = MissionData.MissionType.SpecificWorld,
                targetValue = 5,
                rewardGems = 500,
                rewardCoins = 100,
                rewardBattlePassXP = 25
            });
        }
        
        public void CheckAndResetMissions()
        {
            if (DateTime.UtcNow.Date > lastResetTime.Date)
            {
                // Reset missions for new day
                activeMissions.Clear();
                completedToday = 0;
                
                // Pick random missions from each tier
                int tier1Idx = UnityEngine.Random.Range(0, tier1Missions.Count);
                int tier2Idx = UnityEngine.Random.Range(0, tier2Missions.Count);
                int tier3Idx = UnityEngine.Random.Range(0, tier3Missions.Count);
                
                var mission1 = tier1Missions[tier1Idx];
                var mission2 = tier2Missions[tier2Idx];
                var mission3 = tier3Missions[tier3Idx];
                
                mission1.missionId = $"tier1_{DateTime.UtcNow:yyyyMMdd}";
                mission2.missionId = $"tier2_{DateTime.UtcNow:yyyyMMdd}";
                mission3.missionId = $"tier3_{DateTime.UtcNow:yyyyMMdd}";
                
                mission1.currentProgress = 0;
                mission2.currentProgress = 0;
                mission3.currentProgress = 0;
                
                mission1.completed = false;
                mission2.completed = false;
                mission3.completed = false;
                
                activeMissions.Add(mission1);
                activeMissions.Add(mission2);
                activeMissions.Add(mission3);
                
                lastResetTime = DateTime.UtcNow;
                
                Debug.Log($"[DailyMissionSystem] Daily missions reset. Today's missions:\n" +
                    $"1. {mission1.title}\n2. {mission2.title}\n3. {mission3.title}");
            }
        }
        
        /// <summary>
        /// Called when player completes a level.
        /// Updates progress on all active missions.
        /// </summary>
        public void OnLevelComplete(int worldNumber, int levelNumber, int starsEarned, float completionTime)
        {
            foreach (var mission in activeMissions)
            {
                if (mission.completed) continue;
                
                switch (mission.type)
                {
                    case MissionData.MissionType.LevelsCleared:
                        mission.currentProgress++;
                        break;
                        
                    case MissionData.MissionType.StarsEarned:
                        mission.currentProgress += starsEarned;
                        break;
                        
                    case MissionData.MissionType.SpecificWorld:
                        if (worldNumber >= 5)  // Assumes Tier 3 specific world is World 5+
                            mission.currentProgress++;
                        break;
                }
                
                OnMissionProgress?.Invoke(mission);
                
                if (mission.currentProgress >= mission.targetValue)
                {
                    CompleteMission(mission);
                }
            }
        }
        
        public void OnSprayUsed()
        {
            foreach (var mission in activeMissions)
            {
                if (mission.completed || mission.type != MissionData.MissionType.SprayCount)
                    continue;
                
                mission.currentProgress++;
                OnMissionProgress?.Invoke(mission);
                
                if (mission.currentProgress >= mission.targetValue)
                {
                    CompleteMission(mission);
                }
            }
        }
        
        public void OnComboChange(int comboCount)
        {
            foreach (var mission in activeMissions)
            {
                if (mission.completed || mission.type != MissionData.MissionType.Combos)
                    continue;
                
                mission.currentProgress = Mathf.Max(mission.currentProgress, comboCount);
                OnMissionProgress?.Invoke(mission);
                
                if (mission.currentProgress >= mission.targetValue)
                {
                    CompleteMission(mission);
                }
            }
        }
        
        private void CompleteMission(MissionData mission)
        {
            if (mission.completed) return;
            
            mission.completed = true;
            completedToday++;
            
            OnMissionComplete?.Invoke(mission);
            
            // Award rewards
            CurrencyManager.Instance.AddGems(mission.rewardGems);
            CurrencyManager.Instance.AddCoins(mission.rewardCoins);
            
            // TODO: Award battle pass XP
            
            // Check if all missions in tier completed
            bool allTierComplete = activeMissions.FindAll(m => m.completed).Count >= 2;  // At least 2 of 3
            if (allTierComplete)
            {
                OnAllTierMissionsComplete?.Invoke();
            }
        }
        
        public List<MissionData> GetActiveMissions() => new List<MissionData>(activeMissions);
        public int GetCompletedMissionsToday() => completedToday;
    }
}
