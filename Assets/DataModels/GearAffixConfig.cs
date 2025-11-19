using System;
using System.Collections.Generic;

/// <summary>
/// Gear affix system for random substats and modifiers
/// Affixes are randomly rolled when gear drops
/// </summary>
[Serializable]
public class GearAffixConfig
{
    public string affixId;               // Unique affix identifier
    public string affixName;             // Display name (e.g., "of the Titan")
    public string affixType;             // "prefix" or "suffix"
    public string affixCategory;         // "offensive", "defensive", "utility", "elemental"

    // Requirements
    public List<string> allowedSlots;    // "weapon", "armor", "accessory"
    public int minRarity;                // Minimum rarity tier

    // Stat modifications
    public AffixStatMod statMod;

    // Weighting for random generation
    public float dropWeight;             // Higher = more common
    public int affixTier;                // 1-5 (determines power level)
}

/// <summary>
/// Stat modification from an affix
/// </summary>
[Serializable]
public class AffixStatMod
{
    // Primary stats
    public int flatAtk;
    public int flatDef;
    public int flatHp;
    public int flatSpd;

    public float percentAtk;             // 0.05 = +5%
    public float percentDef;
    public float percentHp;
    public float percentSpd;

    // Combat stats
    public float critChance;             // +X% crit chance
    public float critDamage;             // +X% crit damage
    public float accuracy;               // +X% accuracy
    public float resistance;             // +X% resistance
    public float evasion;                // +X% evasion
    public float blockChance;            // +X% block chance

    // Damage modifiers
    public float damageMultiplier;       // +X% all damage
    public float physicalDamage;         // +X% physical damage
    public float magicDamage;            // +X% magic damage
    public float trueDamage;             // +X% true damage

    // Defensive modifiers
    public float damageReduction;        // +X% damage reduction
    public float physicalResist;         // +X% physical resistance
    public float magicResist;            // +X% magic resistance

    // Utility
    public float lifeSteal;              // +X% life steal
    public float cooldownReduction;      // +X% CDR
    public float expGain;                // +X% exp gain
    public float goldFind;               // +X% gold find
    public float dropRate;               // +X% item drop rate

    // Elemental damage
    public float fireDamage;
    public float waterDamage;
    public float earthDamage;
    public float lightningDamage;
    public float iceDamage;
    public float lightDamage;
    public float darkDamage;

    // Elemental resistance
    public float fireResist;
    public float waterResist;
    public float earthResist;
    public float lightningResist;
    public float iceResist;
    public float lightResist;
    public float darkResist;

    // Status effect modifiers
    public float stunDuration;           // +X% stun duration
    public float burnDamage;             // +X% burn damage
    public float poisonDamage;           // +X% poison damage
    public float freezeDuration;         // +X% freeze duration
}

/// <summary>
/// Affix pool for generation based on slot and rarity
/// </summary>
[Serializable]
public class AffixPoolConfig
{
    public string poolId;
    public string slot;                  // "weapon", "armor", "accessory"
    public int minRarity;
    public List<string> affixIds;        // Available affixes for this pool
    public List<float> weights;          // Weight for each affix
}

/// <summary>
/// Runtime affix instance on a gear
/// </summary>
[Serializable]
public class AffixInstance
{
    public string affixId;               // Reference to config
    public int affixTier;                // Rolled tier (affects power)
    public float rollQuality;            // 0.5 - 1.5 (roll quality multiplier)

    // Cached for performance
    public AffixStatMod appliedStats;    // Stats after tier and quality applied
}

/// <summary>
/// Affix generation rules
/// </summary>
[Serializable]
public class AffixGenerationRules
{
    public int minRarity;
    public int maxRarity;

    // Tier ranges for generation
    public int minAffixTier;
    public int maxAffixTier;

    // Roll quality ranges (multiplies final values)
    public float minRollQuality;         // 0.5 = 50% of base value
    public float maxRollQuality;         // 1.5 = 150% of base value

    // Category distribution
    public float offensiveWeight;        // Chance for offensive affixes
    public float defensiveWeight;        // Chance for defensive affixes
    public float utilityWeight;          // Chance for utility affixes
    public float elementalWeight;        // Chance for elemental affixes
}

/// <summary>
/// Database for affix system
/// </summary>
[Serializable]
public class GearAffixDatabase
{
    public List<GearAffixConfig> affixes;
    public List<AffixPoolConfig> affixPools;
    public AffixGenerationRules generationRules;
}
