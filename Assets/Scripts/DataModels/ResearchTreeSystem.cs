using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Research Tree system for account-wide permanent upgrades
/// </summary>
[Serializable]
public class ResearchSaveData
{
    public int totalResearchPoints;
    public int spentResearchPoints;
    public List<string> unlockedNodes = new List<string>();
    public Dictionary<string, int> nodeUpgradeLevels = new Dictionary<string, int>();
    public DateTime lastResearchCompleted;
    public List<ActiveResearch> activeResearches = new List<ActiveResearch>();
}

[Serializable]
public class ActiveResearch
{
    public string researchId;
    public string nodeId;
    public DateTime startTime;
    public float durationHours;
    public bool isComplete;
}

[Serializable]
public class ResearchTreeDatabase
{
    public List<ResearchTree> trees = new List<ResearchTree>();
}

[Serializable]
public class ResearchTree
{
    public string treeId;
    public string treeName;
    public string description;
    public string category;                   // "combat", "economy", "collection", "social"
    public string icon;
    public List<ResearchNode> nodes = new List<ResearchNode>();
}

[Serializable]
public class ResearchNode
{
    public string nodeId;
    public string nodeName;
    public string description;
    public ResearchNodeType nodeType;
    public int tier;                          // 1-5 progression tiers

    // Prerequisites
    public List<string> requiredNodes;        // Node IDs that must be unlocked first
    public int requiredAccountLevel;

    // Costs
    public int researchPointCost;
    public int goldCost;
    public Dictionary<string, int> materialCosts;  // material ID -> quantity

    // Time-gated research
    public bool isTimeGated;
    public float researchTimeHours;           // Time required to research

    // Upgrade levels
    public int maxUpgradeLevel;               // 1 for single unlock, >1 for upgradeable
    public int upgradePointCost;              // Cost per additional level

    // Effects
    public List<ResearchEffect> effects = new List<ResearchEffect>();

    // Visual
    public Vector2 nodePosition;              // Position in tree UI
    public string icon;
}

public enum ResearchNodeType
{
    StatBoost,                                // Permanent stat increase
    ResourceBonus,                            // Increase resource gains
    UnlockFeature,                            // Unlock new game feature
    QualityOfLife,                            // Convenience features
    Special                                   // Unique effects
}

[Serializable]
public class ResearchEffect
{
    public string effectType;                 // "stat_boost", "resource_bonus", "unlock", etc.
    public string targetType;                 // "all_heroes", "specific_element", "specific_type"
    public string target;                     // Specific target ID if applicable

    // Stat modifications
    public string statType;                   // "hp", "atk", "def", etc.
    public float flatBonus;
    public float percentBonus;

    // Resource bonuses
    public string resourceType;               // "gold", "exp", "drop_rate"
    public float bonusPercent;

    // Feature unlocks
    public string featureId;                  // Feature unlocked by this research

    // Per-level scaling
    public float bonusPerLevel;
}

/// <summary>
/// Example Research Effects:
/// - "+2% HP to all heroes" (stat_boost)
/// - "+10% Gold from battles" (resource_bonus)
/// - "+1 extra item from dungeon chests" (resource_bonus)
/// - "Unlock Auto-Repeat for dungeons" (unlock)
/// - "Increase friend list capacity by 10" (special)
/// - "+5% EXP gain" (resource_bonus)
/// - "Reduce summon cost by 5%" (resource_bonus)
/// </summary>

/// <summary>
/// Research categories with example nodes
/// </summary>
[Serializable]
public class ResearchCategory
{
    public string categoryId;
    public string categoryName;
    public string description;
    public List<string> treeIds;              // Research trees in this category
}

// Example Research Trees:
//
// COMBAT MASTERY TREE:
// - Tier 1: +2% HP to all heroes
// - Tier 2: +2% ATK to all heroes
// - Tier 3: +5% Crit Damage
// - Tier 4: +3% Skill Damage
// - Tier 5: Ultimate skills charge 5% faster
//
// ECONOMY TREE:
// - Tier 1: +10% Gold from battles
// - Tier 2: +5% Material drop rate
// - Tier 3: +1 extra chest drop chance
// - Tier 4: Reduce shop prices by 5%
// - Tier 5: +15% Friend Points earned
//
// COLLECTION TREE:
// - Tier 1: +5% EXP gain
// - Tier 2: Hero level cap +5
// - Tier 3: +1 Equipment slot (mods)
// - Tier 4: Talent tree capacity +10 points
// - Tier 5: Unlock Artifact system
//
// CONVENIENCE TREE:
// - Tier 1: Auto-battle speed +25%
// - Tier 2: Unlock Quick Complete for dungeons
// - Tier 3: +10 Friend list capacity
// - Tier 4: Auto-sell low quality items
// - Tier 5: Unlock Team Presets (save 5 teams)
