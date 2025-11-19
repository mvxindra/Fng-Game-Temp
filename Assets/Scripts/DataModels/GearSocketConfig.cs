using System;

using System.Collections.Generic;

 

/// <summary>

/// Socket system for slotting runes and gems into gear

/// </summary>

[Serializable]

public class SocketConfig

{

    public string socketType;            // "red", "blue", "green", "yellow", "prismatic"

    public string socketName;            // Display name

    public string colorHex;              // Color for UI

    public string description;           // Socket description

}

 

/// <summary>

/// Rune/Gem that can be socketed into gear

/// </summary>

[Serializable]

public class RuneConfig

{

    public string runeId;                // Unique rune identifier

    public string runeName;              // Display name (e.g., "Ruby of Might")

    public string runeType;              // "red", "blue", "green", "yellow", "prismatic", "legendary"

    public int runeTier;                 // 1-5 (higher = more powerful)

    public string runeFamily;            // "might", "vitality", "speed", "cunning", etc.

 

    // Socket requirements

    public List<string> compatibleSockets; // Socket types this can fit into

 

    // Rune stats

    public RuneStatBonus stats;

 

    // Crafting

    public int materialCost;

    public List<MaterialRequirement> materials;

 

    // Upgrade path

    public string upgradeInto;           // Next tier rune ID

    public int upgradeCount;             // Number of same runes to upgrade

}

 

/// <summary>

/// Stats provided by a rune

/// </summary>

[Serializable]

public class RuneStatBonus

{

    // Primary stats

    public int flatAtk;

    public int flatDef;

    public int flatHp;

    public int flatSpd;

 

    public float percentAtk;

    public float percentDef;

    public float percentHp;

    public float percentSpd;

 

    // Combat stats

    public float critChance;

    public float critDamage;

    public float accuracy;

    public float resistance;

    public float lifeSteal;

    public float penetration;            // Armor/resistance penetration

 

    // Elemental stats

    public float elementalDamage;        // All elemental damage

    public float specificElementDamage;  // Specific element (defined by runeFamily)

    public float elementalResist;        // All elemental resistance

 

    // Special effects

    public string specialEffectId;

    public RuneSpecialEffect specialEffect;

}

 

/// <summary>

/// Special effects that can be on runes

/// </summary>

[Serializable]

public class RuneSpecialEffect

{

    public string effectType;            // "on_hit", "on_crit", "on_kill", "passive"

    public float procChance;             // Chance to trigger

 

    // Effect parameters

    public string statusEffect;          // Status to apply

    public int statusDuration;

    public float damageBonus;            // Extra damage

    public float healAmount;             // Heal amount

    public string summonId;              // Summon entity ID

}

 

/// <summary>

/// Legendary runes with unique effects

/// </summary>

[Serializable]

public class LegendaryRuneConfig

{

    public string runeId;

    public string runeName;

    public string legendaryEffect;       // Description of legendary effect

    public string loreText;              // Flavor text

 

    // Requirements

    public int requiredLevel;

    public string requiredClass;

 

    // Stats (typically very powerful)

    public RuneStatBonus stats;

 

    // Unique legendary ability

    public LegendaryRuneAbility ability;

}

 

/// <summary>

/// Unique ability from legendary rune

/// </summary>

[Serializable]

public class LegendaryRuneAbility

{

    public string abilityName;

    public string abilityDescription;

    public string abilityType;           // "aura", "proc", "transform", "summon"

 

    // Aura effects (affects party)

    public float auraRadius;

    public GearStatBlock auraStats;

 

    // Proc effects

    public string procTrigger;           // "on_attack", "on_damaged", "on_kill"

    public float procChance;

    public string procEffect;

 

    // Transform effects

    public string transformType;         // "berserk", "ghost", "elemental"

    public int transformDuration;

    public float transformStatMultiplier;

}

 

/// <summary>

/// Runtime socket instance on gear

/// </summary>

[Serializable]

public class SocketInstance

{

    public string socketType;            // Type of socket

    public string socketedRuneId;        // Rune currently in socket (null if empty)

    public bool isUnlocked;              // Socket must be unlocked to use

 

    public SocketInstance(string type)

    {

        socketType = type;

        socketedRuneId = null;

        isUnlocked = false;

    }

}

 

/// <summary>

/// Rune crafting recipe

/// </summary>

[Serializable]

public class RuneCraftingRecipe

{

    public string recipeId;

    public string resultRuneId;          // Rune produced

    public int resultQuantity;

 

    public List<MaterialRequirement> materials;

    public int goldCost;

    public int craftingTime;             // Seconds (0 = instant)

}

 

/// <summary>

/// Rune upgrade recipe (combine multiple of same rune)

/// </summary>

[Serializable]

public class RuneUpgradeRecipe

{

    public string baseRuneId;            // Starting rune

    public int requiredCount;            // Number needed

    public string resultRuneId;          // Upgraded rune

    public int goldCost;

    public float successChance;          // Chance of success (0-1)

}

 

/// <summary>

/// Database for socket and rune system

/// </summary>

[Serializable]

public class GearSocketDatabase

{

    public List<SocketConfig> socketTypes;

    public List<RuneConfig> runes;

    public List<LegendaryRuneConfig> legendaryRunes;

    public List<RuneCraftingRecipe> craftingRecipes;

    public List<RuneUpgradeRecipe> upgradeRecipes;

}