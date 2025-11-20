using System;
using System.Collections.Generic;
using FngGame.DataModels;

[Serializable]
public class EnemyStatBlock
{
    public int atk;
    public int def;
    public int hp;
    public int spd;
}

[Serializable]
public class EnemyGrowthBlock
{
    public float atk;
    public float def;
    public float hp;
    public float spd;

    /// <summary>
    /// Apply rarity-based multiplier to enemy growth rates.
    /// Used to scale growth rates based on enemy rarity.
    /// </summary>
    public EnemyGrowthBlock ApplyRarityGrowthMultiplier(int rarityTier)
    {
        var multiplier = RarityGrowthManager.Instance.GetGrowthMultiplier(rarityTier);

        return new EnemyGrowthBlock
        {
            atk = atk * multiplier,
            def = def * multiplier,
            hp = hp * multiplier,
            spd = spd * multiplier
        };
    }

    /// <summary>
    /// Apply rarity-based multiplier with stat-specific modifiers.
    /// Allows for fine-tuned growth rate scaling per stat type.
    /// </summary>
    public EnemyGrowthBlock ApplyRarityGrowthMultiplierPerStat(int rarityTier)
    {
        return new EnemyGrowthBlock
        {
            atk = atk * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "atk"),
            def = def * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "def"),
            hp = hp * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "hp"),
            spd = spd * RarityGrowthManager.Instance.GetStatGrowthMultiplier(rarityTier, "spd")
        };
    }
}

[Serializable]
public class EnemyResistances
{
    public float physical = 0f;
    public float fire = 0f;
    public float ice = 0f;
    public float lightning = 0f;
    public float dark = 0f;
    public float holy = 0f;
}

[Serializable]
public class EnemyConfig
{
    public string id;
    public int rarity;
    public string role;
    public string faction;
    public EnemyStatBlock baseStats;
    public EnemyGrowthBlock growth;
    public List<string> equipSlots;
    public List<string> skills;
    public string captainSkill;
    public List<string> passiveAbilities;
    public EnemyResistances resistances;

    /// <summary>
    /// Get the effective growth stats with rarity-based multiplier applied.
    /// This returns the growth rates that should be used for stat calculations.
    /// </summary>
    public EnemyGrowthBlock GetEffectiveGrowth()
    {
        if (growth == null)
        {
            return new EnemyGrowthBlock();
        }

        return growth.ApplyRarityGrowthMultiplier(rarity);
    }

    /// <summary>
    /// Calculate stats at a specific level using rarity-modified growth rates.
    /// Enemy growth is additive (flat increase per level), unlike hero multiplicative growth.
    /// </summary>
    /// <param name="level">Target level</param>
    /// <returns>Stats at the specified level</returns>
    public EnemyStatBlock GetStatsAtLevel(int level)
    {
        if (baseStats == null || growth == null)
        {
            return new EnemyStatBlock();
        }

        var effectiveGrowth = GetEffectiveGrowth();
        int levelDiff = level - 1; // Enemies typically start at level 1

        return new EnemyStatBlock
        {
            atk = baseStats.atk + (int)(effectiveGrowth.atk * levelDiff),
            def = baseStats.def + (int)(effectiveGrowth.def * levelDiff),
            hp = baseStats.hp + (int)(effectiveGrowth.hp * levelDiff),
            spd = baseStats.spd + (int)(effectiveGrowth.spd * levelDiff)
        };
    }
}
