using System;
using System.Collections.Generic;

[Serializable]
public class BannerRate
{
    public int rarity;
    public int weight;                // Weighted chance
    public List<string> featuredPool; // List of IDs for that rarity
}

[Serializable]
public class BannerPityConfig
{
    public bool enabled;
    public int count;                 // Rolls until pity triggers
    public int guaranteeRarity;       // Rarity given by pity
}

[Serializable]
public class GachaBanner
{
    public string id;                 // Example: "BANNER_WEAPON_1"
    public string type;               // "hero", "weapon", "hybrid"
    public string currency;           // "GEMS", "TICKETS"
    public int cost;                  // cost per pull
    public string start;              // ISO time string
    public string end;                // ISO time string
    public BannerPityConfig pity;     // pity configuration
    public List<BannerRate> rates;    // rarity tables
}

[Serializable]
public class BannerList
{
    public List<GachaBanner> banners;
}
