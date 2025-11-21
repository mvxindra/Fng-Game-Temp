using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FnGMafia.Core;
using FnGMafia.Shop;

/// <summary>
/// Black Market Manager - Handles rotating offers and purchases
/// Integrates with existing CurrencyManager and shop systems
/// </summary>
public class BlackMarketManager : Singleton<BlackMarketManager>
{
    private BlackMarketDatabase database;
    private List<BlackMarketOffer> currentOffers = new List<BlackMarketOffer>();
    private Dictionary<string, int> offerPurchases = new Dictionary<string, int>();

    private DateTime lastRefreshTime;
    private int dailyFreeRefreshesUsed = 0;
    private DateTime lastDailyReset;

    protected override void Awake()
    {
        base.Awake();
        LoadBlackMarketDatabase();
        lastRefreshTime = DateTime.MinValue;
        lastDailyReset = DateTime.Now.Date;
    }

    private void LoadBlackMarketDatabase()
    {
        TextAsset configFile = Resources.Load<TextAsset>("Config/black_market");
        if (configFile != null)
        {
            database = JsonUtility.FromJson<BlackMarketDatabase>(configFile.text);
            Debug.Log($"[BlackMarket] Loaded {database.offerPools.Count} offer pools");
        }
        else
        {
            Debug.LogWarning("[BlackMarket] Config not found, creating default");
            database = CreateDefaultBlackMarket();
        }
    }

    #region Refresh System

    /// <summary>
    /// Refresh Black Market offers (automatic or manual)
    /// </summary>
    public bool RefreshMarket(bool isManual = false)
    {
        DateTime now = DateTime.Now;

        // Check if auto-refresh is due
        if (!isManual)
        {
            TimeSpan timeSinceRefresh = now - lastRefreshTime;
            if (timeSinceRefresh.TotalHours < database.config.refreshIntervalHours && currentOffers.Count > 0)
            {
                Debug.Log($"[BlackMarket] Next auto-refresh in {database.config.refreshIntervalHours - timeSinceRefresh.TotalHours:F1} hours");
                return false;
            }
        }
        else
        {
            // Manual refresh - check limits
            ResetDailyLimits();

            if (dailyFreeRefreshesUsed < database.config.freeRefreshesPerDay)
            {
                dailyFreeRefreshesUsed++;
                Debug.Log($"[BlackMarket] Free refresh used ({dailyFreeRefreshesUsed}/{database.config.freeRefreshesPerDay})");
            }
            else
            {
                // Check if player can afford gem refresh
                if (CurrencyManager.Instance != null)
                {
                    if (!CurrencyManager.Instance.DeductCurrency("gems", database.config.refreshCostGems))
                    {
                        Debug.Log("[BlackMarket] Not enough gems for manual refresh");
                        return false;
                    }
                }
            }
        }

        // Generate new offers
        GenerateOffers();
        lastRefreshTime = now;
        Debug.Log($"[BlackMarket] Market refreshed with {currentOffers.Count} offers");
        return true;
    }

    /// <summary>
    /// Generate random offers based on player level
    /// </summary>
    private void GenerateOffers()
    {
        currentOffers.Clear();
        offerPurchases.Clear();

        // Get player level (would integrate with player system)
        int playerLevel = GetPlayerLevel();

        // Find appropriate offer pool
        var pool = database.offerPools
            .Where(p => playerLevel >= p.minPlayerLevel && playerLevel <= p.maxPlayerLevel)
            .OrderByDescending(p => p.minPlayerLevel)
            .FirstOrDefault();

        if (pool == null)
        {
            Debug.LogWarning("[BlackMarket] No suitable offer pool found");
            return;
        }

        // Generate offers
        int offersToGenerate = database.config.simultaneousOffers;
        var availableOffers = new List<BlackMarketOfferTemplate>(pool.offers);

        for (int i = 0; i < offersToGenerate && availableOffers.Count > 0; i++)
        {
            // Roll for each offer based on appear chance
            var weightedOffers = availableOffers.Where(o => UnityEngine.Random.value <= o.appearChance).ToList();

            if (weightedOffers.Count == 0)
            {
                weightedOffers = availableOffers; // Fallback
            }

            var template = weightedOffers[UnityEngine.Random.Range(0, weightedOffers.Count)];
            availableOffers.Remove(template); // Don't repeat same offer

            // Create offer from template
            BlackMarketOffer offer = CreateOfferFromTemplate(template);
            currentOffers.Add(offer);
        }

        // Mark one random offer as featured
        if (currentOffers.Count > 0)
        {
            int featuredIndex = UnityEngine.Random.Range(0, currentOffers.Count);
            currentOffers[featuredIndex].isFeatured = true;
            currentOffers[featuredIndex].discountPercent += 10f; // Extra 10% off featured
            currentOffers[featuredIndex].discountedPrice = Mathf.RoundToInt(
                currentOffers[featuredIndex].originalPrice * (1f - currentOffers[featuredIndex].discountPercent / 100f)
            );
        }
    }

    /// <summary>
    /// Create offer from template
    /// </summary>
    private BlackMarketOffer CreateOfferFromTemplate(BlackMarketOfferTemplate template)
    {
        int quantity = UnityEngine.Random.Range(template.quantityMin, template.quantityMax + 1);
        float discount = UnityEngine.Random.Range(template.discountMin, template.discountMax);

        BlackMarketOffer offer = new BlackMarketOffer
        {
            offerId = Guid.NewGuid().ToString(),
            itemId = template.itemId,
            itemName = template.itemName,
            itemType = template.itemType,
            quantity = quantity,
            currencyType = template.currencyType,
            originalPrice = template.basePrice * quantity,
            discountPercent = discount,
            rarity = template.rarity,
            appearChance = template.appearChance,
            requiredLevel = template.requiredLevel,
            stock = template.stock,
            purchaseLimit = template.purchaseLimit,
            playerPurchased = 0,
            expiryTime = lastRefreshTime.AddHours(database.config.refreshIntervalHours),
            isFeatured = false,
            isLimitedTime = discount > 50f // Deals over 50% are limited time
        };

        offer.discountedPrice = Mathf.RoundToInt(offer.originalPrice * (1f - discount / 100f));

        return offer;
    }

    #endregion

    #region Purchase

    /// <summary>
    /// Purchase Black Market offer
    /// </summary>
    public bool PurchaseOffer(string offerId)
    {
        var offer = currentOffers.Find(o => o.offerId == offerId);
        if (offer == null)
        {
            Debug.Log("[BlackMarket] Offer not found");
            return false;
        }

        // Check expiry
        if (DateTime.Now > offer.expiryTime)
        {
            Debug.Log("[BlackMarket] Offer expired");
            currentOffers.Remove(offer);
            return false;
        }

        // Check purchase limit
        int purchased = offerPurchases.ContainsKey(offerId) ? offerPurchases[offerId] : 0;
        if (purchased >= offer.purchaseLimit)
        {
            Debug.Log("[BlackMarket] Purchase limit reached");
            return false;
        }

        // Check stock
        if (offer.stock >= 0 && purchased >= offer.stock)
        {
            Debug.Log("[BlackMarket] Out of stock");
            return false;
        }

        // Deduct currency
        if (CurrencyManager.Instance != null)
        {
            if (!CurrencyManager.Instance.DeductCurrency(offer.currencyType, offer.discountedPrice))
            {
                return false;
            }
        }

        // Give item
        GiveItem(offer.itemType, offer.itemId, offer.quantity);

        // Track purchase
        if (!offerPurchases.ContainsKey(offerId))
            offerPurchases[offerId] = 0;
        offerPurchases[offerId]++;
        offer.playerPurchased++;

        Debug.Log($"[BlackMarket] Purchased {offer.quantity}x {offer.itemName} for {offer.discountedPrice} {offer.currencyType} ({offer.discountPercent}% off)");
        return true;
    }

    private void GiveItem(string itemType, string itemId, int quantity)
    {
        // Integrate with inventory systems
        Debug.Log($"[BlackMarket] Awarded {quantity}x {itemId} ({itemType})");

        switch (itemType)
        {
            case "currency":
                if (CurrencyManager.Instance != null)
                    CurrencyManager.Instance.AddCurrency(itemId, quantity, ignoreLimit: true);
                break;

            case "material":
                // Add to MaterialInventory
                break;

            case "gear":
                // Add to GearInventory
                break;

            case "hero_shard":
                // Add to HeroCollection
                break;

            case "artifact":
                if (ArtifactManager.Instance != null)
                    ArtifactManager.Instance.AwardArtifact(itemId);
                break;

            case "research_points":
                if (ResearchTreeManager.Instance != null)
                    ResearchTreeManager.Instance.AddResearchPoints(quantity);
                break;

            default:
                Debug.LogWarning($"[BlackMarket] Unknown item type: {itemType}");
                break;
        }
    }

    #endregion

    #region Helpers

    private void ResetDailyLimits()
    {
        DateTime now = DateTime.Now;
        if (now.Date > lastDailyReset.Date)
        {
            dailyFreeRefreshesUsed = 0;
            lastDailyReset = now.Date;
            Debug.Log("[BlackMarket] Daily limits reset");
        }
    }

    private int GetPlayerLevel()
    {
        // Integrate with player system
        // For now, return a sample value
        return PlayerProfileManager.Instance != null ?
            PlayerProfileManager.Instance.GetSaveData().profileInfo.playerLevel : 1;
    }

    public List<BlackMarketOffer> GetCurrentOffers()
    {
        RefreshMarket(false); // Auto-refresh if needed
        return new List<BlackMarketOffer>(currentOffers);
    }

    public int GetFreeRefreshesRemaining()
    {
        ResetDailyLimits();
        return database.config.freeRefreshesPerDay - dailyFreeRefreshesUsed;
    }

    public DateTime GetNextAutoRefreshTime()
    {
        return lastRefreshTime.AddHours(database.config.refreshIntervalHours);
    }

    #endregion

    #region Default Data

    private BlackMarketDatabase CreateDefaultBlackMarket()
    {
        BlackMarketDatabase db = new BlackMarketDatabase();

        // Early game pool (Level 1-20)
        BlackMarketOfferPool earlyPool = new BlackMarketOfferPool
        {
            poolId = "early_game",
            poolName = "Early Game Offers",
            minPlayerLevel = 1,
            maxPlayerLevel = 20,
            offers = new List<BlackMarketOfferTemplate>
            {
                new BlackMarketOfferTemplate
                {
                    itemId = "gold_small", itemName = "Gold Pouch", itemType = "currency",
                    quantityMin = 5000, quantityMax = 10000,
                    currencyType = "gems", basePrice = 50,
                    discountMin = 30f, discountMax = 50f,
                    rarity = "common", appearChance = 0.8f, requiredLevel = 1,
                    stock = -1, purchaseLimit = 3
                },
                new BlackMarketOfferTemplate
                {
                    itemId = "enhancement_stone_t1", itemName = "Enhancement Stone", itemType = "material",
                    quantityMin = 5, quantityMax = 15,
                    currencyType = "gold", basePrice = 200,
                    discountMin = 25f, discountMax = 45f,
                    rarity = "common", appearChance = 0.7f, requiredLevel = 5,
                    stock = -1, purchaseLimit = 5
                },
                new BlackMarketOfferTemplate
                {
                    itemId = "GEAR_RARE_SWORD", itemName = "Rare Sword", itemType = "gear",
                    quantityMin = 1, quantityMax = 1,
                    currencyType = "gold", basePrice = 5000,
                    discountMin = 30f, discountMax = 60f,
                    rarity = "rare", appearChance = 0.4f, requiredLevel = 10,
                    stock = 1, purchaseLimit = 1
                }
            }
        };

        db.offerPools.Add(earlyPool);

        Debug.Log("[BlackMarket] Created default database");
        return db;
    }

    #endregion
}
