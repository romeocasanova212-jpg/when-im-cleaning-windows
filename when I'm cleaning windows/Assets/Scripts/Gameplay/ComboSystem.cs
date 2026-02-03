using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace WhenImCleaningWindows.Gameplay
{
    /// <summary>
    /// Advanced combo system for maximum addiction psychology.
    /// Tracks consecutive level completions without failure.
    /// Applies escalating multipliers: 1.2x (3), 1.5x (5), 2.0x (7), 3.0x (10+)
    /// </summary>
    public class ComboSystem : MonoBehaviour
    {
        [SerializeField] private int currentCombo = 0;
        [SerializeField] private int bestCombo = 0;
        
        private const int COMBO_MILESTONE_3 = 3;
        private const int COMBO_MILESTONE_5 = 5;
        private const int COMBO_MILESTONE_7 = 7;
        private const int COMBO_MILESTONE_10 = 10;
        
        private Dictionary<int, float> comboMultipliers = new Dictionary<int, float>
        {
            { 0, 1.0f },      // No combo
            { 3, 1.2f },      // 3-combo
            { 5, 1.5f },      // 5-combo
            { 7, 2.0f },      // 7-combo
            { 10, 3.0f }      // 10+ combo
        };
        
        public UnityEvent<int> OnComboChanged = new UnityEvent<int>();
        public UnityEvent<int> OnComboMilestone = new UnityEvent<int>();
        public UnityEvent OnComboReset = new UnityEvent();
        
        public int CurrentCombo => currentCombo;
        public int BestCombo => bestCombo;
        
        /// <summary>
        /// Called when level is successfully completed.
        /// Increments combo and triggers haptic feedback at milestones.
        /// </summary>
        public void OnLevelComplete(int starsEarned)
        {
            currentCombo++;
            
            if (currentCombo > bestCombo)
            {
                bestCombo = currentCombo;
            }
            
            OnComboChanged?.Invoke(currentCombo);
            
            // Trigger milestone callbacks
            if (currentCombo == COMBO_MILESTONE_3 || 
                currentCombo == COMBO_MILESTONE_5 || 
                currentCombo == COMBO_MILESTONE_7 || 
                currentCombo == COMBO_MILESTONE_10)
            {
                OnComboMilestone?.Invoke(currentCombo);
                TriggerMilestoneHaptics(currentCombo);
            }
        }
        
        /// <summary>
        /// Called when level fails. Resets combo to 0.
        /// Triggers "one more try" psychology.
        /// </summary>
        public void OnLevelFail()
        {
            if (currentCombo > 0)
            {
                currentCombo = 0;
                OnComboReset?.Invoke();
            }
        }
        
        /// <summary>
        /// Returns gem multiplier based on current combo.
        /// Example: 3-combo = 1.2x, 10-combo = 3.0x
        /// </summary>
        public float GetGemMultiplier()
        {
            if (currentCombo >= COMBO_MILESTONE_10)
                return comboMultipliers[COMBO_MILESTONE_10];
            else if (currentCombo >= COMBO_MILESTONE_7)
                return comboMultipliers[COMBO_MILESTONE_7];
            else if (currentCombo >= COMBO_MILESTONE_5)
                return comboMultipliers[COMBO_MILESTONE_5];
            else if (currentCombo >= COMBO_MILESTONE_3)
                return comboMultipliers[COMBO_MILESTONE_3];
            
            return 1.0f;
        }
        
        private void TriggerMilestoneHaptics(int milestone)
        {
            // Haptic patterns for each milestone
            switch (milestone)
            {
                case COMBO_MILESTONE_3:
                    Haptics.Pulse(0.4f, 150);  // Gentle pulse
                    break;
                case COMBO_MILESTONE_5:
                    Haptics.Pulse(0.6f, 250);  // Medium pulse
                    break;
                case COMBO_MILESTONE_7:
                    Haptics.Pulse(0.8f, 300);  // Strong pulse
                    break;
                case COMBO_MILESTONE_10:
                    Haptics.Resonance(0.8f, 1000);  // Long sustained
                    break;
            }
        }
    }
}
