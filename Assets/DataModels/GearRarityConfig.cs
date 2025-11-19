using System;
using System.Collections.Generic;

/// <summary>
/// Enhanced gear rarity system with multiple tiers
/// </summary>
[Serializable]
public class GearRarityConfig
{
    public string rarityId;              // "common", "uncommon", "rare", "epic", "unique", "legendary", "mythic"
    public int rarityTier;               // 1-7 (higher = better)
    public string displayName;           // Display name
    public string colorHex;              // Color for UI (#FFFFFF)

    // Drop rates and costs
    public float dropWeight;             // Weight for random drops
    public int craftingCostMultiplier;   // Base cost multiplier

    // Stat scaling
    public float statMultiplier;         // Base stat scaling (1.0 = baseline)
    public int minAffixes;               // Minimum number of affixes
    public int maxAffixes;               // Maximum number of affixes

    // Sockets
    public int minSockets;               // Minimum sockets
    public int maxSockets;               // Maximum sockets

    // Set bonus eligibility
    public bool canHaveSetBonus;         // Can be part of set
    public bool canHaveUniquePassive;    // Can have unique passive

    // Enhancement limits
    public int maxEnhanceLevel;          // Max enhance level
    public int maxAugmentTier;           // Max augment tier
}

/// <summary>
/// Rarity tier enum for easy reference
/// </summary>
public enum GearRarity
{
    Common = 1,      // White - Basic gear
    Uncommon = 2,    // Green - Slightly better
    Rare = 3,        // Blue - Good gear (existing system)
    Epic = 4,        // Purple - Very good (existing system)
    Legendary = 5,   // Orange - Excellent (existing system)
    Unique = 6,      // Gold - Special named items with unique effects
    Mythic = 7       // Red/Rainbow - Extremely rare, game-changing
}

/// <summary>
/// Named unique gear with special properties
/// </summary>
[Serializable]
public class UniqueGearConfig
{
    public string uniqueId;              // Unique gear ID
    public string gearConfigId;          // Base gear config this extends
    public string uniqueName;            // Special name (e.g., "Sword of the Fallen King")
    public string loreText;              // Flavor text/lore

    // Requirements
    public int requiredLevel;            // Minimum hero level
    public string requiredClass;         // Required hero class/role
    public string requiredElement;       // Required element affinity

    // Guaranteed affixes
    public List<string> guaranteedAffixes;

    // Unique passive ability
    public string uniquePassiveId;       // Special passive only this item has
    public UniquePassiveConfig uniquePassive;

    // Set information
    public string setId;                 // If part of named set
    public bool isSoulbound;             // Can't be traded/transferred
}

/// <summary>
/// Unique passive abilities that only appear on unique/mythic items
/// </summary>
[Serializable]
public class UniquePassiveConfig
{
    public string passiveId;
    public string passiveName;
    public string description;

    // Trigger conditions
    public string triggerType;           // "on_attack", "on_damaged", "start_battle", "low_hp", "kill"
    public float triggerChance;          // Chance to trigger (0-1)

    // Effects
    public List<UniquePassiveEffect> effects;
}

/// <summary>
/// Effects for unique passives
/// </summary>
[Serializable]
public class UniquePassiveEffect
{
    public string effectType;            // "summon_entity", "transform", "revive", "duplicate_attack", "time_stop"
    public float value;
    public int duration;
    public string targetType;            // "self", "allies", "enemies", "all"

    // Special effect parameters
    public string summonEntityId;        // For summon effects
    public string transformInto;         // For transform effects
    public float reviveHpPercent;        // For revive effects
}

/// <summary>
/// Named gear sets with bonuses
/// </summary>
[Serializable]
public class NamedGearSetConfig
{
    public string setId;                 // e.g., "dragon_slayer_set"
    public string setName;               // Display name
    public string setDescription;        // Flavor text
    public string setTheme;              // "dragon", "undead", "elemental", etc.

    public List<string> setGearIds;      // All gear pieces in set

    // Set bonuses at different piece counts
    public SetBonus twoPieceBonus;
    public SetBonus fourPieceBonus;
    public SetBonus sixPieceBonus;       // For full set
}

/// <summary>
/// Set bonus effects
/// </summary>
[Serializable]
public class SetBonus
{
    public int requiredPieces;           // Number of pieces needed
    public string bonusName;             // Bonus name
    public string bonusDescription;      // Description

    // Stat bonuses
    public GearStatBlock statBonuses;    // Flat stat increases
    public GearStatBlock statPercentages;// Percentage increases

    // Special effects
    public string specialEffectId;       // ID of special effect
    public SpecialSetEffect specialEffect;
}

/// <summary>
/// Special effects for set bonuses
/// </summary>
[Serializable]
public class SpecialSetEffect
{
    public string effectType;            // "damage_aura", "reflect_damage", "life_steal", "immunity"
    public float effectValue;
    public string statusImmunity;        // Status to be immune to
    public int auraRange;                // Range for aura effects
}

/// <summary>
/// Database for rarity configurations
/// </summary>
[Serializable]
public class GearRarityDatabase
{
    public List<GearRarityConfig> rarities;
    public List<UniqueGearConfig> uniqueGear;
    public List<NamedGearSetConfig> namedSets;
}
