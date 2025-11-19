using System;

using System.Collections.Generic;

 

/// <summary>

/// Enhanced gear instance with rarity, affixes, and sockets

/// Extends the existing GearInstance functionality

/// </summary>

[Serializable]

public class EnhancedGearInstance

{

    // Base properties (from existing GearInstance)

    public string instanceId;

    public string configId;              // Base gear config ID

    public int level;

    public int enhance;

    public int passiveLevel;

    public int limitBreakLevel;

    public int augmentTier;

    public int refinementLevel;

    public int potentialLevel;

    public bool isAwakened;

    public int weeklyMaterialsUsed;

    public int monthlyMaterialsUsed;

    public int seasonalMaterialsUsed;

 

    // === NEW: Enhanced Gear Properties ===

 

    // Rarity

    public int rarityTier;               // 1-7 (Common to Mythic)

    public string rarityId;              // "common", "rare", "unique", etc.

    public bool isUnique;                // Named unique gear

    public string uniqueId;              // If unique, reference to UniqueGearConfig

 

    // Affixes (random substats)

    public List<AffixInstance> affixes; // Random affixes rolled on this gear

    public int affixRerollCount;         // Track rerolls for cost scaling

 

    // Sockets

    public List<SocketInstance> sockets; // Socket slots on this gear

    public int unlockedSocketCount;      // Number of unlocked sockets

 

    // Set bonus

    public string namedSetId;            // If part of named set (e.g., "dragon_slayer_set")

 

    // Quality

    public float itemQuality;            // 0.5 - 1.5 (affects all stats)

    public int itemLevel;                // Item level (can differ from player level)

 

    // Identification (for unidentified items system)

    public bool isIdentified;            // False = stats hidden

    public int identifyCost;             // Cost to identify

 

    // Binding

    public bool isSoulbound;             // Can't be traded

    public string boundToHeroId;         // Hero this is bound to

 

    // Metadata

    public DateTime acquiredDate;

    public string acquiredFrom;          // "drop", "craft", "quest", "shop"

 

    public EnhancedGearInstance()

    {

        affixes = new List<AffixInstance>();

        sockets = new List<SocketInstance>();

        isIdentified = true;

        itemQuality = 1.0f;

    }

 

    /// <summary>

    /// Get total stat bonuses from all sources

    /// </summary>

    public GearStatBlock GetTotalStats()

    {

        GearStatBlock total = new GearStatBlock();

 

        // Base stats would come from GearConfig

        // Affixes

        foreach (var affix in affixes)

        {

            if (affix.appliedStats != null)

            {

                ApplyAffixStats(total, affix.appliedStats);

            }

        }

 

        // Socketed runes

        foreach (var socket in sockets)

        {

            if (!string.IsNullOrEmpty(socket.socketedRuneId))

            {

                // Get rune stats and apply

                // This would query RuneConfig from a manager

            }

        }

 

        return total;

    }

 

    private void ApplyAffixStats(GearStatBlock stats, AffixStatMod affix)

    {

        stats.atk += affix.flatAtk;

        stats.def += affix.flatDef;

        stats.hp += affix.flatHp;

        stats.spd += affix.flatSpd;

        // Percentages would be calculated when applying to hero

    }

 

    /// <summary>

    /// Check if gear has an empty socket

    /// </summary>

    public bool HasEmptySocket()

    {

        foreach (var socket in sockets)

        {

            if (socket.isUnlocked && string.IsNullOrEmpty(socket.socketedRuneId))

            {

                return true;

            }

        }

        return false;

    }

 

    /// <summary>

    /// Get total number of socketed runes

    /// </summary>

    public int GetSocketedRuneCount()

    {

        int count = 0;

        foreach (var socket in sockets)

        {

            if (!string.IsNullOrEmpty(socket.socketedRuneId))

            {

                count++;

            }

        }

        return count;

    }

}

 

/// <summary>

/// Gear generation parameters for random loot

/// </summary>

[Serializable]

public class GearGenerationParams

{

    public string gearConfigId;          // Base gear to generate from

    public int rarityTier;               // Rolled rarity

    public int itemLevel;                // Item level (affects power)

    public float qualityRoll;            // 0.5 - 1.5

 

    public int affixCount;               // Number of affixes to roll

    public List<string> guaranteedAffixes; // Affixes that must appear

 

    public int socketCount;              // Number of sockets to generate

    public List<string> socketTypes;     // Types of sockets

 

    public bool forceUnique;             // Force unique gear

    public string uniqueId;              // Specific unique to generate

 

    public string namedSetId;            // If generating set piece

}