using System;

using System.Collections.Generic;

 

/// <summary>

/// Multi-currency system

/// </summary>

[Serializable]

public class PlayerCurrencies

{

    // Basic currencies

    public int gold;

    public int gems;                            // Premium currency

 

    // Special currencies

    public int heroShards;                      // Universal hero shards

    public int soulCores;                       // For awakening/limit breaks

    public int premiumOrbs;                     // For premium summons

    public int arenaTokens;                     // From arena

    public int guildCoins;                      // From guild activities

    public int raidTickets;                     // Entry for raids

    public int dungeonKeys;                     // Entry for special dungeons

 

    // Event currencies

    public Dictionary<string, int> eventCurrencies; // Event-specific currencies

 

    // Soft caps and daily limits

    public Dictionary<string, int> dailyEarned;     // Track daily earnings

    public Dictionary<string, DateTime> lastReset;  // Last daily reset per currency

 

    public PlayerCurrencies()

    {

        eventCurrencies = new Dictionary<string, int>();

        dailyEarned = new Dictionary<string, int>();

        lastReset = new Dictionary<string, DateTime>();

    }

}

 

/// <summary>

/// Currency configuration

/// </summary>

[Serializable]

public class CurrencyConfig

{

    public string currencyId;

    public string currencyName;

    public string description;

    public string icon;

    public string currencyType;                 // "basic", "premium", "special", "event"

 

    // Caps

    public int maxAmount;                       // -1 for unlimited

    public int dailyCap;                        // Max earned per day (-1 for unlimited)

    public int softCap;                         // Amount before diminishing returns

 

    // Sources

    public List<string> obtainablFrom;          // Where this can be obtained

    public bool canPurchase;                    // Can buy with real money?

}

 

/// <summary>

/// Black Market - Rotating shop with random items

/// </summary>

[Serializable]

public class BlackMarket

{

    public string marketId;

    public DateTime lastRefresh;

    public DateTime nextRefresh;

    public int refreshInterval;                 // Hours between refreshes

 

    // Current offerings

    public List<BlackMarketOffer> currentOffers;

    public int manualRefreshCost;               // Gems to force refresh

    public int manualRefreshesUsed;

 

    public BlackMarket()

    {

        currentOffers = new List<BlackMarketOffer>();

    }

}

 

/// <summary>

/// Black market offer

/// </summary>

[Serializable]

public class BlackMarketOffer

{

    public string offerId;

    public string itemType;                     // "gear", "hero", "material", "currency"

    public string itemId;

    public int quantity;

 

    // Cost

    public string currencyType;                 // "gold", "gems", "black_market_tokens"

    public int originalPrice;

    public int discountedPrice;

    public int discountPercent;

 

    // Availability

    public int stock;                           // How many available

    public int purchaseLimit;                   // Max per player

    public DateTime expiryTime;

 

    // Rarity

    public string offerRarity;                  // "common", "rare", "epic", "legendary"

    public float appearChance;                  // Chance to appear in rotation

}

 

/// <summary>

/// Shop configuration

/// </summary>

[Serializable]

public class ShopConfig

{

    public string shopId;

    public string shopName;

    public string description;

    public string shopType;                     // "general", "arena", "guild", "event", "premium"

 

    // Items

    public List<ShopItem> items;

    public bool hasRandomRotation;

    public int rotationInterval;                // Hours

 

    // Requirements

    public int unlockLevel;

    public string requiredQuest;                // Quest that must be completed

}

 

/// <summary>

/// Shop item

/// </summary>

[Serializable]

public class ShopItem

{

    public string itemId;

    public string itemName;

    public string itemType;

    public int quantity;

 

    // Cost

    public List<CurrencyCost> costs;            // Can cost multiple currencies

 

    // Limits

    public int dailyStock;                      // Refreshes daily

    public int weeklyStock;                     // Refreshes weekly

    public int monthlyStock;                    // Refreshes monthly

    public int lifetimeStock;                   // Never refreshes

 

    // Requirements

    public int requiredLevel;

    public string requiredRank;                 // For arena shop

    public int requiredGuildLevel;              // For guild shop

}

 

/// <summary>

/// Currency cost

/// </summary>

[Serializable]

public class CurrencyCost

{

    public string currencyType;

    public int amount;

}

 

/// <summary>

/// Player shop progress (tracks purchases)

/// </summary>

[Serializable]

public class PlayerShopProgress

{

    public string shopId;

    public Dictionary<string, int> dailyPurchases;   // Item ID -> count

    public Dictionary<string, int> weeklyPurchases;

    public Dictionary<string, int> monthlyPurchases;

    public Dictionary<string, int> lifetimePurchases;

 

    public DateTime lastDailyReset;

    public DateTime lastWeeklyReset;

    public DateTime lastMonthlyReset;

 

    public PlayerShopProgress()

    {

        dailyPurchases = new Dictionary<string, int>();

        weeklyPurchases = new Dictionary<string, int>();

        monthlyPurchases = new Dictionary<string, int>();

        lifetimePurchases = new Dictionary<string, int>();

    }

}

 

/// <summary>

/// Bundle/Package configuration

/// </summary>

[Serializable]

public class BundleConfig

{

    public string bundleId;

    public string bundleName;

    public string description;

    public string bundleType;                   // "starter", "daily", "weekly", "limited"

 

    // Contents

    public List<BundleItem> items;

 

    // Cost

    public float realMoneyCost;                 // USD

    public string currency;                     // "USD", "EUR", etc.

 

    // Limits

    public int purchaseLimit;                   // Max purchases

    public DateTime startDate;

    public DateTime endDate;

    public bool isLimitedTime;

 

    // Display

    public string badgeText;                    // "BEST VALUE", "LIMITED", etc.

    public int sortOrder;                       // Display order

}

 

/// <summary>

/// Bundle item

/// </summary>

[Serializable]

public class BundleItem

{

    public string itemType;

    public string itemId;

    public int quantity;

    public bool isBonus;                        // Bonus item (shows as extra)

}

 

/// <summary>

/// VIP system configuration

/// </summary>

[Serializable]

public class VIPSystem

{

    public int vipLevel;

    public int vipPoints;

    public List<VIPTier> tiers;

}

 

/// <summary>

/// VIP tier

/// </summary>

[Serializable]

public class VIPTier

{

    public int tier;

    public string tierName;

    public int requiredPoints;

 

    // Benefits

    public float expBonus;

    public float goldBonus;

    public float dropRateBonus;

    public int dailyGems;

    public int extraDailyDungeons;

    public int extraArenaAttempts;

    public bool instantBuildingUpgrades;

    public List<string> exclusiveFeatures;

}

 

/// <summary>

/// Daily deal configuration

/// </summary>

[Serializable]

public class DailyDeal

{

    public string dealId;

    public string dealName;

    public BundleConfig deal;

    public DateTime resetTime;

    public bool isPurchased;

}

 

/// <summary>

/// Exchange rate configuration (for currency conversion)

/// </summary>

[Serializable]

public class CurrencyExchange

{

    public string fromCurrency;

    public string toCurrency;

    public float exchangeRate;                  // How much 'to' you get per 'from'

    public int dailyLimit;                      // Max exchanges per day

    public int exchangeFee;                     // Fee in gems

}

 

/// <summary>

/// Database for currency and shop systems

/// </summary>

[Serializable]

public class CurrencyShopDatabase

{

    public List<CurrencyConfig> currencies;

    public List<ShopConfig> shops;

    public List<BundleConfig> bundles;

    public List<BlackMarketOffer> blackMarketPool; // Pool of possible offers

    public List<CurrencyExchange> exchanges;

    public VIPSystem vipSystem;

}