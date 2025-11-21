using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enhanced Black Market system with rotating rare items and special deals
/// Integrates with existing shop systems
/// </summary>
namespace FnGMafia.Shop
{
    [Serializable]
    public class BlackMarketConfig
    {
        public string marketId = "black_market";
        public string marketName = "Black Market";
        public int refreshIntervalHours = 6;              // Refresh every 6 hours
        public int freeRefreshesPerDay = 3;               // Free refreshes daily
        public int refreshCostGems = 50;                  // Cost to manually refresh
        public int simultaneousOffers = 6;                // Number of offers at once
        public int qualityTier = 0;                       // Increases with player level
    }

    [Serializable]
    public class BlackMarketOffer
    {
        public string offerId;
        public string itemId;
        public string itemName;
        public string itemType;                           // "gear", "material", "hero_shard", "artifact", "rune"
        public int quantity;

        // Pricing
        public string currencyType;                       // "gold", "gems", "arena_tokens", etc.
        public int originalPrice;
        public int discountedPrice;
        public float discountPercent;                     // e.g., 30% off

        // Limits
        public int stock;                                 // -1 = unlimited
        public int purchaseLimit;                         // Per offer appearance
        public int playerPurchased = 0;                   // Track player purchases

        // Rarity and appearance
        public string rarity;                             // "common", "rare", "epic", "legendary", "mythic"
        public float appearChance;                        // 0-1 probability
        public int requiredLevel;                         // Minimum player level
        public DateTime expiryTime;                       // When this offer expires

        // Visual
        public string icon;
        public bool isFeatured;                           // Highlighted deal
        public bool isLimitedTime;                        // Flash deal
    }

    [Serializable]
    public class BlackMarketOfferPool
    {
        public string poolId;
        public string poolName;
        public int minPlayerLevel;
        public int maxPlayerLevel;
        public List<BlackMarketOfferTemplate> offers = new List<BlackMarketOfferTemplate>();
    }

    [Serializable]
    public class BlackMarketOfferTemplate
    {
        public string itemId;
        public string itemName;
        public string itemType;
        public int quantityMin;
        public int quantityMax;

        // Pricing formula
        public string currencyType;
        public int basePrice;
        public float discountMin = 20f;                   // Minimum 20% off
        public float discountMax = 70f;                   // Maximum 70% off

        // Appearance
        public string rarity;
        public float appearChance;
        public int requiredLevel;
        public int stock = -1;
        public int purchaseLimit = 1;
    }

    /// <summary>
    /// Black Market Database - Pre-defined offer pools
    /// </summary>
    [Serializable]
    public class BlackMarketDatabase
    {
        public BlackMarketConfig config = new BlackMarketConfig();
        public List<BlackMarketOfferPool> offerPools = new List<BlackMarketOfferPool>();
    }

    /// <summary>
    /// Example Black Market Offers by Tier:
    ///
    /// EARLY GAME (Level 1-20):
    /// - Common gear with 30-50% discount
    /// - Basic materials (Enhancement Stones, Gold)
    /// - Hero XP potions
    ///
    /// MID GAME (Level 21-50):
    /// - Rare/Epic gear with 20-40% discount
    /// - Advanced materials (Ascension materials)
    /// - Hero shards (3-5 shards)
    /// - Low-tier runes
    ///
    /// LATE GAME (Level 51-80):
    /// - Epic/Legendary gear with 15-30% discount
    /// - Rare materials (Limit Break materials)
    /// - Hero shards (5-10 shards)
    /// - Mid-tier runes
    /// - Artifact fragments
    ///
    /// END GAME (Level 81-100):
    /// - Legendary/Mythic gear with 10-25% discount
    /// - Endgame materials (Awakening materials)
    /// - Hero shards (10-20 shards)
    /// - High-tier runes
    /// - Complete artifacts (rare)
    /// - Research points
    /// - Mythic keys
    /// </summary>
}
