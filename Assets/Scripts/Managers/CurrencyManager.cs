using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager for multi-currency system and shops
/// </summary>
public class CurrencyManager : Singleton<CurrencyManager>
{
    private CurrencyDatabase currencyDatabase;
    private Dictionary<string, int> playerCurrencies = new Dictionary<string, int>();
    private Dictionary<string, int> dailyEarnings = new Dictionary<string, int>();
    private List<BlackMarketOffer> currentBlackMarketOffers = new List<BlackMarketOffer>();
    private DateTime lastBlackMarketRefresh;
    private int playerVipTier = 0;

    protected override void Awake()
    {
        base.Awake();
        LoadCurrencyDatabase();
        InitializeCurrencies();
    }

    private void LoadCurrencyDatabase()
    {
        TextAsset currencyData = Resources.Load<TextAsset>("Config/currency_system");
        if (currencyData != null)
        {
            currencyDatabase = JsonUtility.FromJson<CurrencyDatabase>(currencyData.text);
            Debug.Log($"Loaded {currencyDatabase.currencies.Count} currency types");
        }
        else
        {
            Debug.LogError("Failed to load currency_system.json");
            currencyDatabase = new CurrencyDatabase();
        }
    }

    private void InitializeCurrencies()
    {
        // Initialize all currencies to 0
        foreach (var currency in currencyDatabase.currencies)
        {
            playerCurrencies[currency.currencyId] = 0;
            dailyEarnings[currency.currencyId] = 0;
        }

        // Give starting amounts
        playerCurrencies["gold"] = 10000;
        playerCurrencies["gems"] = 500;

        lastBlackMarketRefresh = DateTime.MinValue;
    }

    /// <summary>
    /// Get currency amount
    /// </summary>
    public int GetCurrency(string currencyId)
    {
        return playerCurrencies.ContainsKey(currencyId) ? playerCurrencies[currencyId] : 0;
    }

    /// <summary>
    /// Add currency with caps and limits
    /// </summary>
    public bool AddCurrency(string currencyId, int amount, bool ignoreLimit = false)
    {
        var currency = currencyDatabase.currencies.Find(c => c.currencyId == currencyId);
        if (currency == null)
        {
            Debug.LogError($"Currency {currencyId} not found");
            return false;
        }

        if (!ignoreLimit)
        {
            // Check daily earning cap
            if (currency.dailyEarnCap > 0)
            {
                int currentDaily = dailyEarnings.ContainsKey(currencyId) ? dailyEarnings[currencyId] : 0;
                if (currentDaily >= currency.dailyEarnCap)
                {
                    Debug.Log($"Daily earning cap reached for {currencyId}");
                    return false;
                }

                // Cap the amount to daily limit
                int remaining = currency.dailyEarnCap - currentDaily;
                amount = Mathf.Min(amount, remaining);
            }

            // Apply soft cap reduction
            if (currency.softCapAmount > 0 && playerCurrencies[currencyId] >= currency.softCapAmount)
            {
                amount = (int)(amount * currency.softCapReduction);
                Debug.Log($"Soft cap applied to {currencyId}: {currency.softCapReduction * 100}% reduction");
            }
        }

        // Add currency
        if (!playerCurrencies.ContainsKey(currencyId))
        {
            playerCurrencies[currencyId] = 0;
        }
        playerCurrencies[currencyId] += amount;

        // Track daily earnings
        if (!dailyEarnings.ContainsKey(currencyId))
        {
            dailyEarnings[currencyId] = 0;
        }
        dailyEarnings[currencyId] += amount;

        Debug.Log($"Added {amount} {currencyId}. Total: {playerCurrencies[currencyId]}");
        return true;
    }

    /// <summary>
    /// Deduct currency
    /// </summary>
    public bool DeductCurrency(string currencyId, int amount)
    {
        if (!playerCurrencies.ContainsKey(currencyId) || playerCurrencies[currencyId] < amount)
        {
            Debug.Log($"Insufficient {currencyId}");
            return false;
        }

        playerCurrencies[currencyId] -= amount;
        Debug.Log($"Deducted {amount} {currencyId}. Remaining: {playerCurrencies[currencyId]}");
        return true;
    }

    /// <summary>
    /// Exchange one currency for another
    /// </summary>
    public bool ExchangeCurrency(string fromCurrency, string toCurrency, int amount)
    {
        var exchange = currencyDatabase.exchanges.Find(e =>
            e.fromCurrencyId == fromCurrency && e.toCurrencyId == toCurrency);

        if (exchange == null)
        {
            Debug.LogError($"No exchange rate found for {fromCurrency} to {toCurrency}");
            return false;
        }

        int cost = amount * exchange.exchangeRate;
        if (!DeductCurrency(fromCurrency, cost))
        {
            return false;
        }

        AddCurrency(toCurrency, amount, ignoreLimit: true);
        Debug.Log($"Exchanged {cost} {fromCurrency} for {amount} {toCurrency}");
        return true;
    }

    /// <summary>
    /// Refresh Black Market offers
    /// </summary>
    public void RefreshBlackMarket()
    {
        TimeSpan timeSinceRefresh = DateTime.Now - lastBlackMarketRefresh;
        int refreshHours = 6; // Refresh every 6 hours

        if (timeSinceRefresh.TotalHours < refreshHours && currentBlackMarketOffers.Count > 0)
        {
            Debug.Log($"Black Market refreshes in {refreshHours - timeSinceRefresh.TotalHours:F1} hours");
            return;
        }

        currentBlackMarketOffers.Clear();

        // Generate random offers
        foreach (var offerTemplate in currencyDatabase.blackMarket.offers)
        {
            if (UnityEngine.Random.value <= offerTemplate.appearChance)
            {
                var offer = new BlackMarketOffer
                {
                    offerId = Guid.NewGuid().ToString(),
                    itemType = offerTemplate.itemType,
                    itemId = offerTemplate.itemId,
                    quantity = offerTemplate.quantity,
                    originalPrice = offerTemplate.originalPrice,
                    discountedPrice = offerTemplate.discountedPrice,
                    currencyType = offerTemplate.currencyType,
                    stock = offerTemplate.stock,
                    purchaseLimit = offerTemplate.purchaseLimit,
                    purchaseCount = 0,
                    expiryTime = DateTime.Now.AddHours(refreshHours),
                    appearChance = offerTemplate.appearChance
                };

                currentBlackMarketOffers.Add(offer);
            }
        }

        lastBlackMarketRefresh = DateTime.Now;
        Debug.Log($"Black Market refreshed! {currentBlackMarketOffers.Count} offers available");
    }

    /// <summary>
    /// Get current Black Market offers
    /// </summary>
    public List<BlackMarketOffer> GetBlackMarketOffers()
    {
        if (currentBlackMarketOffers.Count == 0)
        {
            RefreshBlackMarket();
        }
        return currentBlackMarketOffers;
    }

    /// <summary>
    /// Purchase from Black Market
    /// </summary>
    public bool PurchaseBlackMarketOffer(string offerId, int quantity = 1)
    {
        var offer = currentBlackMarketOffers.Find(o => o.offerId == offerId);
        if (offer == null)
        {
            Debug.Log("Offer not found");
            return false;
        }

        // Check purchase limit
        if (offer.purchaseCount + quantity > offer.purchaseLimit)
        {
            Debug.Log("Purchase limit reached");
            return false;
        }

        // Check stock
        if (offer.stock >= 0 && quantity > offer.stock)
        {
            Debug.Log("Insufficient stock");
            return false;
        }

        // Check currency
        int totalCost = offer.discountedPrice * quantity;
        if (!DeductCurrency(offer.currencyType, totalCost))
        {
            return false;
        }

        // Give item
        GiveItem(offer.itemType, offer.itemId, offer.quantity * quantity);

        // Update offer
        offer.purchaseCount += quantity;
        if (offer.stock > 0)
        {
            offer.stock -= quantity;
        }

        Debug.Log($"Purchased {quantity}x {offer.itemId} from Black Market");
        return true;
    }

    /// <summary>
    /// Purchase from shop
    /// </summary>
    public bool PurchaseShopItem(string shopType, string itemId)
    {
        var shop = currencyDatabase.shops.Find(s => s.shopType == shopType);
        if (shop == null)
        {
            Debug.LogError($"Shop {shopType} not found");
            return false;
        }

        var item = shop.items.Find(i => i.itemId == itemId);
        if (item == null)
        {
            Debug.LogError($"Item {itemId} not found in {shopType} shop");
            return false;
        }

        // Check if item is unlocked
        if (!string.IsNullOrEmpty(item.unlockCondition))
        {
            // TODO: Check unlock conditions
        }

        // Check cost
        if (!DeductCurrency(item.currencyType, item.price))
        {
            return false;
        }

        // Give item
        GiveItem(item.itemType, item.itemId, item.quantity);

        Debug.Log($"Purchased {item.itemName} from {shopType} shop");
        return true;
    }

    /// <summary>
    /// Purchase bundle
    /// </summary>
    public bool PurchaseBundle(string bundleId)
    {
        var bundle = currencyDatabase.bundles.Find(b => b.bundleId == bundleId);
        if (bundle == null)
        {
            Debug.LogError($"Bundle {bundleId} not found");
            return false;
        }

        // Check if available
        if (!bundle.isAvailable)
        {
            Debug.Log("Bundle not available");
            return false;
        }

        // Check purchase limit
        if (bundle.purchaseLimit > 0 && bundle.purchaseCount >= bundle.purchaseLimit)
        {
            Debug.Log("Bundle purchase limit reached");
            return false;
        }

        // Check cost (bundles use real money or premium currency)
        if (!DeductCurrency(bundle.currencyType, bundle.price))
        {
            return false;
        }

        // Give all items in bundle
        foreach (var item in bundle.items)
        {
            GiveItem(item.itemType, item.itemId, item.quantity);
        }

        // Give bonus items
        foreach (var bonus in bundle.bonusItems)
        {
            GiveItem(bonus.itemType, bonus.itemId, bonus.quantity);
        }

        bundle.purchaseCount++;
        Debug.Log($"Purchased bundle: {bundle.bundleName}");
        return true;
    }

    /// <summary>
    /// Get VIP tier benefits
    /// </summary>
    public VIPTierConfig GetVIPBenefits()
    {
        return currencyDatabase.vipTiers.Find(v => v.tier == playerVipTier);
    }

    /// <summary>
    /// Upgrade VIP tier
    /// </summary>
    public bool UpgradeVIP()
    {
        var nextTier = currencyDatabase.vipTiers.Find(v => v.tier == playerVipTier + 1);
        if (nextTier == null)
        {
            Debug.Log("Already at max VIP tier");
            return false;
        }

        // TODO: Check VIP points or purchase requirement
        playerVipTier++;
        Debug.Log($"Upgraded to VIP tier {playerVipTier}");
        return true;
    }

    /// <summary>
    /// Reset daily earnings (call once per day)
    /// </summary>
    public void ResetDailyEarnings()
    {
        dailyEarnings.Clear();
        foreach (var currency in currencyDatabase.currencies)
        {
            dailyEarnings[currency.currencyId] = 0;
        }
        Debug.Log("Daily earnings reset");
    }

    /// <summary>
    /// Get all currencies with amounts
    /// </summary>
    public Dictionary<string, int> GetAllCurrencies()
    {
        return new Dictionary<string, int>(playerCurrencies);
    }

    // Helper method
    private void GiveItem(string itemType, string itemId, int quantity)
    {
        switch (itemType)
        {
            case "currency":
                AddCurrency(itemId, quantity, ignoreLimit: true);
                break;
            case "hero":
                // TODO: Add hero to roster
                Debug.Log($"Received hero: {itemId}");
                break;
            case "gear":
                // TODO: Add gear to inventory
                Debug.Log($"Received gear: {itemId} x{quantity}");
                break;
            case "material":
                // TODO: Add material to inventory
                Debug.Log($"Received material: {itemId} x{quantity}");
                break;
            case "summon_ticket":
                // TODO: Add summon ticket
                Debug.Log($"Received summon ticket: {itemId} x{quantity}");
                break;
            default:
                Debug.LogWarning($"Unknown item type: {itemType}");
                break;
        }
    }
}

/// <summary>
/// Database containing currency configs
/// </summary>
[Serializable]
public class CurrencyDatabase
{
    public List<CurrencyConfig> currencies = new List<CurrencyConfig>();
    public List<CurrencyExchange> exchanges = new List<CurrencyExchange>();
    public BlackMarketConfig blackMarket = new BlackMarketConfig();
    public List<ShopConfig> shops = new List<ShopConfig>();
    public List<BundleConfig> bundles = new List<BundleConfig>();
    public List<VIPTierConfig> vipTiers = new List<VIPTierConfig>();
}
