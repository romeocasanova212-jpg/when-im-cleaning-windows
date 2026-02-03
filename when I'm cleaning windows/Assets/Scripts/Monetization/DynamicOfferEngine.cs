using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace WhenImCleaningWindows.Monetization
{
    /// <summary>
    /// Dynamic offer system with A/B testing and player segmentation.
    /// Shows different offers to whales/dolphins/minnows/guppies based on LTV prediction.
    /// Implements FOMO countdown timers and purchase-probability optimization.
    /// </summary>
    [System.Serializable]
    public class OfferData
    {
        public string offerId;
        public enum OfferType { EnergyPack, Cosmetic, BattlePass, SeasonalBundle }
        public enum OfferTier { Tier1Free, Tier1Impulse, Tier2Daily, Tier3Milestone }
        
        public OfferType type;
        public OfferTier tier;
        public string title;
        public string description;
        
        // Pricing
        public float priceUSD;
        public int gemsIncluded;
        public int energyIncluded;
        public bool isSaleOffer;
        public float originalPrice;
        
        // Segmentation
        public enum PlayerSegment { Whale, Dolphin, Minnow, Guppy }
        public List<PlayerSegment> targetSegments;
        
        // Timing
        public DateTime offerStartTime;
        public DateTime offerEndTime;
        public int secondsRemaining;
        
        // Psychology
        public string psychologicalTrigger;  // "anchoring", "fomo", "scarcity", "loss_aversion"
        public int displayCount;
        public int clickCount;
        public int purchaseCount;
    }
    
    public class DynamicOfferEngine : MonoBehaviour
    {
        [SerializeField] private List<OfferData> activeOffers = new List<OfferData>();
        [SerializeField] private Dictionary<string, OfferData> allOffers = new Dictionary<string, OfferData>();
        
        public UnityEvent<OfferData> OnOfferGenerated = new UnityEvent<OfferData>();
        public UnityEvent<OfferData> OnOfferExpired = new UnityEvent<OfferData>();
        public UnityEvent<OfferData> OnOfferPurchased = new UnityEvent<OfferData>();
        
        private CurrencyManager currencyManager;
        private PersonalizationEngine personalizationEngine;
        
        private const int MAX_SIMULTANEOUS_OFFERS = 3;
        
        private void Start()
        {
            currencyManager = CurrencyManager.Instance;
            personalizationEngine = GetComponent<PersonalizationEngine>();
            
            InitializeOfferTemplates();
        }
        
        private void Update()
        {
            // Update countdown timers on active offers
            foreach (var offer in activeOffers)
            {
                offer.secondsRemaining = (int)(offer.offerEndTime - DateTime.UtcNow).TotalSeconds;
                if (offer.secondsRemaining <= 0)
                {
                    ExpireOffer(offer);
                }
            }
        }
        
        private void InitializeOfferTemplates()
        {
            // Template 1: Energy Pack (£0.99, Tier1 Impulse)
            allOffers.Add("energy_pack_5", new OfferData
            {
                offerId = "energy_pack_5",
                type = OfferData.OfferType.EnergyPack,
                tier = OfferData.OfferTier.Tier1Impulse,
                title = "5 Lives for £0.99",
                description = "Continue playing immediately",
                priceUSD = 0.99f,
                energyIncluded = 5,
                gemsIncluded = 0,
                targetSegments = new List<OfferData.PlayerSegment> 
                { 
                    OfferData.PlayerSegment.Minnow, 
                    OfferData.PlayerSegment.Guppy 
                },
                psychologicalTrigger = "fomo"
            });
            
            // Template 2: Energy Pack (£2.99, Tier1 Anchoring)
            allOffers.Add("energy_pack_15", new OfferData
            {
                offerId = "energy_pack_15",
                type = OfferData.OfferType.EnergyPack,
                tier = OfferData.OfferTier.Tier1Impulse,
                title = "15 Lives for £2.99",
                description = "3x the value",
                priceUSD = 2.99f,
                energyIncluded = 15,
                gemsIncluded = 0,
                targetSegments = new List<OfferData.PlayerSegment> 
                { 
                    OfferData.PlayerSegment.Dolphin, 
                    OfferData.PlayerSegment.Minnow 
                },
                psychologicalTrigger = "anchoring"
            });
            
            // Template 3: Energy Pack (£4.99, Whales only)
            allOffers.Add("energy_pack_50", new OfferData
            {
                offerId = "energy_pack_50",
                type = OfferData.OfferType.EnergyPack,
                tier = OfferData.OfferTier.Tier1Impulse,
                title = "50 Lives for £4.99",
                description = "Stock up and play",
                priceUSD = 4.99f,
                energyIncluded = 50,
                gemsIncluded = 500,  // Bonus gems for whales
                targetSegments = new List<OfferData.PlayerSegment> 
                { 
                    OfferData.PlayerSegment.Whale 
                },
                psychologicalTrigger = "anchoring"
            });
            
            // Template 4: Sale Offer (Monday special)
            allOffers.Add("monday_sale_energy_5", new OfferData
            {
                offerId = "monday_sale_energy_5",
                type = OfferData.OfferType.EnergyPack,
                tier = OfferData.OfferTier.Tier2Daily,
                title = "50% Off! £0.99 → £0.49",
                description = "Monday Special - Today Only",
                priceUSD = 0.49f,
                energyIncluded = 5,
                gemsIncluded = 0,
                isSaleOffer = true,
                originalPrice = 0.99f,
                targetSegments = new List<OfferData.PlayerSegment> 
                { 
                    OfferData.PlayerSegment.Minnow, 
                    OfferData.PlayerSegment.Guppy 
                },
                psychologicalTrigger = "scarcity"
            });
            
            // Template 5: Bonus Offer (buy X get Y)
            allOffers.Add("weekend_bonus_15", new OfferData
            {
                offerId = "weekend_bonus_15",
                type = OfferData.OfferType.EnergyPack,
                tier = OfferData.OfferTier.Tier2Daily,
                title = "Buy 15 Lives, Get 5 Free",
                description = "Weekend Bonus - Friday to Sunday",
                priceUSD = 2.99f,
                energyIncluded = 20,  // 15 + 5 free
                gemsIncluded = 0,
                targetSegments = new List<OfferData.PlayerSegment> 
                { 
                    OfferData.PlayerSegment.Dolphin, 
                    OfferData.PlayerSegment.Minnow 
                },
                psychologicalTrigger = "loss_aversion"
            });
            
            // Template 6: VIP Offer (Whales)
            allOffers.Add("vip_tier2_monthly", new OfferData
            {
                offerId = "vip_tier2_monthly",
                type = OfferData.OfferType.EnergyPack,
                tier = OfferData.OfferTier.Tier3Milestone,
                title = "VIP Tier 2: £14.99/month",
                description = "30 energy/day + 1.5x gems",
                priceUSD = 14.99f,
                energyIncluded = 900,  // 30/day × 30 days
                gemsIncluded = 0,
                targetSegments = new List<OfferData.PlayerSegment> 
                { 
                    OfferData.PlayerSegment.Whale,
                    OfferData.PlayerSegment.Dolphin
                },
                psychologicalTrigger = "anchoring"
            });
            
            // Template 7: Battle Pass
            allOffers.Add("battle_pass_standard", new OfferData
            {
                offerId = "battle_pass_standard",
                type = OfferData.OfferType.BattlePass,
                tier = OfferData.OfferTier.Tier2Daily,
                title = "Battle Pass: £9.99",
                description = "70 levels of rewards + cosmetics",
                priceUSD = 9.99f,
                gemsIncluded = 8000,  // Can earn gems back
                targetSegments = new List<OfferData.PlayerSegment> 
                { 
                    OfferData.PlayerSegment.Whale,
                    OfferData.PlayerSegment.Dolphin,
                    OfferData.PlayerSegment.Minnow
                },
                psychologicalTrigger = "anchoring"
            });
        }
        
        /// <summary>
        /// Generate offer for player based on their segment and current state.
        /// Called when energy depletes (FOMO trigger).
        /// </summary>
        public void GenerateEnergyDepletionOffer()
        {
            var segment = DeterminePlayerSegment();
            var offer = SelectOfferForSegment(segment, OfferData.OfferType.EnergyPack);
            
            if (offer != null)
            {
                DisplayOffer(offer);
            }
        }
        
        /// <summary>
        /// Generate daily limited-time offer (Monday/Friday/etc).
        /// </summary>
        public void GenerateDailyLimitedOffer()
        {
            int dayOfWeek = (int)DateTime.UtcNow.DayOfWeek;
            
            string offerId = dayOfWeek switch
            {
                1 => "monday_sale_energy_5",        // Monday
                5 => "weekend_bonus_15",             // Friday
                _ => "energy_pack_15"                // Other days
            };
            
            if (allOffers.TryGetValue(offerId, out var offer))
            {
                DisplayOffer(offer);
            }
        }
        
        /// <summary>
        /// Generate milestone offer (Level 50, 100, 500, etc).
        /// </summary>
        public void GenerateMilestoneOffer(int levelReached)
        {
            var segment = DeterminePlayerSegment();
            
            string offerId = levelReached switch
            {
                3 => "energy_pack_5",           // Day 3 offer
                50 => "energy_pack_15",         // Level 50
                500 => "vip_tier2_monthly",     // Level 500
                _ => null
            };
            
            if (offerId != null && allOffers.TryGetValue(offerId, out var offer))
            {
                // Check if offer is valid for this segment
                if (offer.targetSegments.Contains(segment))
                {
                    DisplayOffer(offer);
                }
            }
        }
        
        private void DisplayOffer(OfferData offer)
        {
            if (activeOffers.Count >= MAX_SIMULTANEOUS_OFFERS)
                return;
            
            offer.offerStartTime = DateTime.UtcNow;
            offer.offerEndTime = DateTime.UtcNow.AddSeconds(900);  // 15 min expiry
            offer.displayCount++;
            
            activeOffers.Add(offer);
            OnOfferGenerated?.Invoke(offer);
            
            Debug.Log($"[DynamicOfferEngine] Offer generated: {offer.title} ({offer.offerId})");
        }
        
        public void OnOfferClicked(OfferData offer)
        {
            offer.clickCount++;
            Debug.Log($"[DynamicOfferEngine] Offer clicked: {offer.title}. CTR: {(float)offer.clickCount / offer.displayCount:P}");
        }
        
        public void OnOfferPurchased(OfferData offer)
        {
            offer.purchaseCount++;
            OnOfferPurchased?.Invoke(offer);
            
            // Award purchased items
            if (offer.energyIncluded > 0)
                EnergySystem.Instance.AddEnergy(offer.energyIncluded, $"Purchased: {offer.title}");
            
            if (offer.gemsIncluded > 0)
                currencyManager.AddGems(offer.gemsIncluded);
            
            // Remove from active offers
            activeOffers.Remove(offer);
            
            Debug.Log($"[DynamicOfferEngine] Offer purchased: {offer.title}. Revenue: ${offer.priceUSD:F2}");
        }
        
        private void ExpireOffer(OfferData offer)
        {
            activeOffers.Remove(offer);
            OnOfferExpired?.Invoke(offer);
            
            Debug.Log($"[DynamicOfferEngine] Offer expired: {offer.title}");
        }
        
        private OfferData.PlayerSegment DeterminePlayerSegment()
        {
            float ltv = personalizationEngine.EstimateLTV();
            
            if (ltv > 100f)
                return OfferData.PlayerSegment.Whale;
            else if (ltv > 20f)
                return OfferData.PlayerSegment.Dolphin;
            else if (ltv > 2f)
                return OfferData.PlayerSegment.Minnow;
            else
                return OfferData.PlayerSegment.Guppy;
        }
        
        private OfferData SelectOfferForSegment(OfferData.PlayerSegment segment, OfferData.OfferType type)
        {
            // Find best offer for this segment and type
            var validOffers = new List<OfferData>();
            
            foreach (var offer in allOffers.Values)
            {
                if (offer.type == type && offer.targetSegments.Contains(segment))
                {
                    validOffers.Add(offer);
                }
            }
            
            if (validOffers.Count > 0)
                return validOffers[UnityEngine.Random.Range(0, validOffers.Count)];
            
            return null;
        }
        
        public List<OfferData> GetActiveOffers() => new List<OfferData>(activeOffers);
    }
}
