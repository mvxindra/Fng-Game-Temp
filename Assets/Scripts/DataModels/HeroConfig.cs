using System;
using System.Collections.Generic;
using FngGame.DataModels;

[Serializable]
public class Stats
{
    public int atk;
    public int def;
    public int hp;
    public int spd;
    public int crit;
    public int critDmg;
    public int acc;
    public int res;

    /// <summary>
    /// Apply rarity-based multiplier to growth stats.
    /// Used to scale growth rates based on unit rarity.
    /// </summary>
    public Stats ApplyRarityGrowthMultiplier(int rarityTier)
    {
        var multiplier = RarityGrowthManager.Instance.GetGrowthMultiplier(rarityTier);

        return new Stats
        {
            atk = (int)(atk * multiplier),
            def = (int)(def * multiplier),
            hp = (int)(hp * multiplier),
            spd = (int)(spd * multiplier),
            crit = (int)(crit * multiplier),
            critDmg = (int)(critDmg * multiplier),
            acc = (int)(acc * multiplier),
            res = (int)(res * multiplier)
        };
    }

    /// <summary>
    /// Apply rarity-based multiplier with stat-specific modifiers.
    /// Allows for fine-tuned growth rate scaling per stat type.
    /// </summary>
    public Stats ApplyRarityGrowthMultiplierPerStat(int rarityTier)
    {
        return new Stats
        {
            atk = (int)(atk * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "atk")),
            def = (int)(def * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "def")),
            hp = (int)(hp * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "hp")),
            spd = (int)(spd * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "spd")),
            crit = (int)(crit * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "atk")),
            critDmg = (int)(critDmg * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "atk")),
            acc = (int)(acc * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "atk")),
            res = (int)(res * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "def"))
        };
    }
}

[Serializable]
public class HeroEquipment
{
    public string weapon;        // Weapon slot - gear instance ID
    public string armor;         // Armor slot - gear instance ID
    public string accessory1;    // Accessory slot 1 - gear instance ID
    public string accessory2;    // Accessory slot 2 - gear instance ID
    public string weaponMod;     // Weapon mod - mod ID (faction-specific bonus)
    public string armorMod;      // Armor mod - mod ID (faction-specific bonus)
    public string accessory1Mod; // Accessory 1 mod - mod ID (faction-specific bonus)
    public string accessory2Mod; // Accessory 2 mod - mod ID (faction-specific bonus)
}

[Serializable]
public class HeroConfig
{
    public string id;
    public int rarity;
    public string role;
    public string faction;

    public Stats baseStats;
    public Stats growth;

    public List<string> equipSlots;  // Legacy support - can still use for restrictions
    public HeroEquipment equippedGear; // New 4-slot equipment system
    public List<string> skills;

    public string captainSkill;   // optional
    public string primaryElement;     // Elemental affinity (fire, water, earth, etc.)

    public int baseLevel = 1;         // Starting level

    public int baseAscension = 0;     // Starting ascension tier

    /// <summary>
    /// Get the effective growth stats with rarity-based multiplier applied.
    /// This returns the growth rates that should be used for stat calculations.
    /// </summary>
    public Stats GetEffectiveGrowth()
    {
        if (growth == null)
        {
            return new Stats();
        }

        return growth.ApplyRarityGrowthMultiplier(rarity);
    }

    /// <summary>
    /// Calculate stats at a specific level using rarity-modified growth rates.
    /// </summary>
    /// <param name="level">Target level</param>
    /// <returns>Stats at the specified level</returns>
    public Stats GetStatsAtLevel(int level)
    {
        if (baseStats == null || growth == null)
        {
            return new Stats();
        }

        var effectiveGrowth = GetEffectiveGrowth();
        int levelDiff = level - baseLevel;

        return new Stats
        {
            atk = baseStats.atk + (int)(effectiveGrowth.atk * levelDiff),
            def = baseStats.def + (int)(effectiveGrowth.def * levelDiff),
            hp = baseStats.hp + (int)(effectiveGrowth.hp * levelDiff),
            spd = baseStats.spd + (int)(effectiveGrowth.spd * levelDiff),
            crit = baseStats.crit + (int)(effectiveGrowth.crit * levelDiff),
            critDmg = baseStats.critDmg + (int)(effectiveGrowth.critDmg * levelDiff),
            acc = baseStats.acc + (int)(effectiveGrowth.acc * levelDiff),
            res = baseStats.res + (int)(effectiveGrowth.res * levelDiff)
        };
    }
}

[Serializable]
public class HeroConfigList
{
    public List<HeroConfig> heroes;
}
