using System;
using System.Collections.Generic;

/// <summary>
/// Base building/HQ system for managing resources and boosting progression
/// </summary>
[Serializable]
public class PlayerBase
{
    public string playerId;
    public int baseLevel;                       // Overall base level
    public Dictionary<string, Building> buildings;
    public Dictionary<string, int> resources;   // Resource stockpiles

    // Production
    public DateTime lastCollectionTime;
    public Dictionary<string, float> productionRates; // Resource/hour

    public PlayerBase()
    {
        buildings = new Dictionary<string, Building>();
        resources = new Dictionary<string, int>();
        productionRates = new Dictionary<string, float>();
    }
}

/// <summary>
/// Individual building in base
/// </summary>
[Serializable]
public class Building
{
    public string buildingId;
    public string buildingType;                 // From BuildingConfig
    public int level;
    public int maxLevel;

    // Upgrade
    public bool isUpgrading;
    public DateTime upgradeStartTime;
    public DateTime upgradeCompleteTime;

    // Production
    public bool isProducing;
    public string producingResource;
    public float productionRate;
    public int storedProduction;                // Uncollected production

    // Status
    public bool isUnlocked;
    public DateTime unlockedDate;
}

/// <summary>
/// Building configuration
/// </summary>
[Serializable]
public class BuildingConfig
{
    public string buildingId;
    public string buildingName;
    public string buildingType;                 // "training_hall", "alchemy_lab", "forge", "vault", "barracks"
    public string description;
    public string category;                     // "production", "boost", "storage", "special"

    // Unlock requirements
    public int requiredBaseLevel;
    public int unlockCost;
    public List<MaterialRequirement> unlockMaterials;

    // Levels
    public int maxLevel;
    public List<BuildingLevelConfig> levels;

    // Production (if applicable)
    public string producedResource;
    public float baseProductionRate;            // Per hour
    public int storageCapacity;                 // Max stored production

    // Visual
    public string modelPath;
    public Vector2Int gridPosition;             // Position in base grid
    public Vector2Int gridSize;                 // Size on grid
}

/// <summary>
/// Building level configuration
/// </summary>
[Serializable]
public class BuildingLevelConfig
{
    public int level;

    // Upgrade cost
    public int goldCost;
    public List<MaterialRequirement> materials;
    public int upgradeDuration;                 // Seconds

    // Benefits at this level
    public float productionBonus;               // +% production
    public float efficiencyBonus;               // +% efficiency
    public int storageIncrease;                 // +storage capacity
    public List<BuildingBonus> bonuses;         // Special bonuses

    // Requirements
    public int requiredBaseLevel;
}

/// <summary>
/// Building bonus effects
/// </summary>
[Serializable]
public class BuildingBonus
{
    public string bonusType;                    // "exp_boost", "gold_boost", "training_speed", "craft_speed"
    public float bonusValue;                    // Bonus amount
    public string targetType;                   // "all", "hero", "gear", "specific"
    public string targetId;                     // Specific target if applicable
}

/// <summary>
/// Training Hall - Boost hero training
/// </summary>
[Serializable]
public class TrainingHallConfig : BuildingConfig
{
    public int maxSimultaneousTraining;         // Heroes that can train at once
    public float expBoostPerLevel;              // +% EXP per level
    public float skillExpBoost;                 // +% Skill EXP
    public int reducedTrainingTime;             // -% training time
}

/// <summary>
/// Alchemy Lab - Produce potions and materials
/// </summary>
[Serializable]
public class AlchemyLabConfig : BuildingConfig
{
    public List<AlchemyRecipe> recipes;
    public int maxSimultaneousCrafts;
    public float craftSpeedBonus;               // -% craft time per level
    public float successRateBonus;              // +% success rate
}

/// <summary>
/// Alchemy recipe
/// </summary>
[Serializable]
public class AlchemyRecipe
{
    public string recipeId;
    public string recipeName;
    public int requiredLabLevel;

    // Inputs
    public List<MaterialRequirement> ingredients;
    public int goldCost;

    // Output
    public string resultItemId;
    public int resultQuantity;
    public float successChance;

    // Time
    public int craftTime;                       // Seconds
}

/// <summary>
/// Vault - Increase resource storage
/// </summary>
[Serializable]
public class VaultConfig : BuildingConfig
{
    public int goldCapacity;
    public int gemsProtected;                   // Gems protected from raids
    public Dictionary<string, int> resourceCapacities;
}

/// <summary>
/// Barracks - Increase team size and unlock formations
/// </summary>
[Serializable]
public class BarracksConfig : BuildingConfig
{
    public int maxTeamSlots;                    // Max heroes in party
    public List<string> unlockedFormations;     // Formations unlocked
    public float teamStatBonus;                 // +% stats to all team members
}

/// <summary>
/// Mine - Produce gold over time
/// </summary>
[Serializable]
public class MineConfig : BuildingConfig
{
    public int goldPerHour;
    public int maxStorage;                      // Max gold before must collect
    public float productionBoostPerLevel;
}

/// <summary>
/// Monument - Prestige and permanent bonuses
/// </summary>
[Serializable]
public class MonumentConfig : BuildingConfig
{
    public List<MonumentBonus> permanentBonuses;
    public int prestigePointsGranted;
}

/// <summary>
/// Monument permanent bonus
/// </summary>
[Serializable]
public class MonumentBonus
{
    public int monumentLevel;
    public string bonusType;                    // "all_stats", "exp_gain", "drop_rate"
    public float bonusValue;
    public bool isPermanent;                    // Persists through prestige
}

/// <summary>
/// Workshop - Gear enhancement boost
/// </summary>
[Serializable]
public class WorkshopConfig : BuildingConfig
{
    public float enhanceSuccessBonus;           // +% success rate
    public float enhanceCostReduction;          // -% cost
    public int maxSimultaneousEnhances;
    public List<string> unlockedEnhanceTypes;   // Types of enhancements unlocked
}

/// <summary>
/// Library - Research tech tree
/// </summary>
[Serializable]
public class LibraryConfig : BuildingConfig
{
    public List<Research> availableResearch;
    public int maxSimultaneousResearch;
}

/// <summary>
/// Research configuration
/// </summary>
[Serializable]
public class Research
{
    public string researchId;
    public string researchName;
    public string description;
    public int requiredLibraryLevel;

    // Requirements
    public List<string> prerequisiteResearch;
    public int goldCost;
    public List<MaterialRequirement> materials;
    public int researchTime;                    // Seconds

    // Benefits
    public List<ResearchBonus> bonuses;
}

/// <summary>
/// Research bonus
/// </summary>
[Serializable]
public class ResearchBonus
{
    public string bonusType;
    public float bonusValue;
    public string description;
}

/// <summary>
/// Active research
/// </summary>
[Serializable]
public class ActiveResearch
{
    public string researchId;
    public DateTime startTime;
    public DateTime completeTime;
    public bool isComplete;
}

/// <summary>
/// Base layout preset
/// </summary>
[Serializable]
public class BaseLayoutPreset
{
    public string layoutId;
    public string layoutName;
    public Dictionary<string, Vector2Int> buildingPositions;
    public bool isUnlocked;
}

/// <summary>
/// Vector2Int for grid positions
/// </summary>
[Serializable]
public struct Vector2Int
{
    public int x;
    public int y;

    public Vector2Int(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

/// <summary>
/// Database for base building system
/// </summary>
[Serializable]
public class BaseBuildingDatabase
{
    public List<BuildingConfig> buildings;
    public List<TrainingHallConfig> trainingHalls;
    public List<AlchemyLabConfig> alchemyLabs;
    public List<Research> research;
    public List<BaseLayoutPreset> layoutPresets;
}
