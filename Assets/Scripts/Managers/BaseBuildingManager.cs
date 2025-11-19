using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager for base building and HQ system
/// </summary>
public class BaseBuildingManager : Singleton<BaseBuildingManager>
{
    private BaseBuildingDatabase buildingDatabase;
    private PlayerBase playerBase;

    protected override void Awake()
    {
        base.Awake();
        LoadBuildingDatabase();
        InitializePlayerBase();
    }

    private void LoadBuildingDatabase()
    {
        TextAsset buildingData = Resources.Load<TextAsset>("Config/base_building");
        if (buildingData != null)
        {
            buildingDatabase = JsonUtility.FromJson<BaseBuildingDatabase>(buildingData.text);
            Debug.Log($"Loaded {buildingDatabase.buildingTypes.Count} building types");
        }
        else
        {
            Debug.LogError("Failed to load base_building.json");
            buildingDatabase = new BaseBuildingDatabase();
        }
    }

    private void InitializePlayerBase()
    {
        // Initialize with default base
        playerBase = new PlayerBase
        {
            baseId = Guid.NewGuid().ToString(),
            baseName = "My Base",
            baseLevel = 1,
            buildings = new List<BuildingInstance>(),
            resources = new Dictionary<string, int>
            {
                { "gold", 1000 },
                { "gems", 100 },
                { "wood", 500 },
                { "stone", 500 },
                { "iron", 200 }
            },
            completedResearch = new List<string>(),
            activeProduction = new List<ProductionJob>(),
            layoutPreset = "default"
        };

        // Add starting buildings
        AddBuilding("hq", 1, 1);
    }

    /// <summary>
    /// Get player's base
    /// </summary>
    public PlayerBase GetPlayerBase()
    {
        return playerBase;
    }

    /// <summary>
    /// Construct a new building
    /// </summary>
    public bool ConstructBuilding(string buildingTypeId, int gridX, int gridY)
    {
        var buildingConfig = buildingDatabase.buildingTypes.Find(b => b.buildingId == buildingTypeId);
        if (buildingConfig == null)
        {
            Debug.LogError($"Building type {buildingTypeId} not found");
            return false;
        }

        // Check if building already exists
        if (playerBase.buildings.Any(b => b.buildingTypeId == buildingTypeId && buildingConfig.maxCount == 1))
        {
            Debug.Log($"Building {buildingTypeId} already exists and can only have 1");
            return false;
        }

        // Count existing buildings of this type
        int existingCount = playerBase.buildings.Count(b => b.buildingTypeId == buildingTypeId);
        if (existingCount >= buildingConfig.maxCount)
        {
            Debug.Log($"Maximum number of {buildingTypeId} buildings reached ({buildingConfig.maxCount})");
            return false;
        }

        // Check requirements
        if (playerBase.baseLevel < buildingConfig.requiredBaseLevel)
        {
            Debug.Log($"Base level {buildingConfig.requiredBaseLevel} required");
            return false;
        }

        // Check resources
        if (!HasResources(buildingConfig.buildCost))
        {
            Debug.Log("Insufficient resources");
            return false;
        }

        // Deduct resources
        DeductResources(buildingConfig.buildCost);

        // Create building instance
        var building = new BuildingInstance
        {
            instanceId = Guid.NewGuid().ToString(),
            buildingTypeId = buildingTypeId,
            level = 1,
            gridX = gridX,
            gridY = gridY,
            constructionStartTime = DateTime.Now,
            constructionEndTime = DateTime.Now.AddSeconds(buildingConfig.buildTime),
            isConstructing = true,
            lastCollectionTime = DateTime.Now
        };

        playerBase.buildings.Add(building);
        Debug.Log($"Started construction of {buildingConfig.buildingName}");
        return true;
    }

    /// <summary>
    /// Upgrade a building
    /// </summary>
    public bool UpgradeBuilding(string instanceId)
    {
        var building = playerBase.buildings.Find(b => b.instanceId == instanceId);
        if (building == null)
        {
            Debug.LogError("Building not found");
            return false;
        }

        var buildingConfig = buildingDatabase.buildingTypes.Find(b => b.buildingId == building.buildingTypeId);
        if (buildingConfig == null)
        {
            Debug.LogError("Building config not found");
            return false;
        }

        if (building.level >= buildingConfig.maxLevel)
        {
            Debug.Log("Building already at max level");
            return false;
        }

        // Get upgrade config
        var upgradeConfig = buildingConfig.upgrades.Find(u => u.level == building.level + 1);
        if (upgradeConfig == null)
        {
            Debug.LogError($"Upgrade config for level {building.level + 1} not found");
            return false;
        }

        // Check resources
        if (!HasResources(upgradeConfig.upgradeCost))
        {
            Debug.Log("Insufficient resources");
            return false;
        }

        // Deduct resources
        DeductResources(upgradeConfig.upgradeCost);

        // Start upgrade
        building.isConstructing = true;
        building.constructionStartTime = DateTime.Now;
        building.constructionEndTime = DateTime.Now.AddSeconds(upgradeConfig.upgradeTime);

        Debug.Log($"Started upgrade to level {building.level + 1}");
        return true;
    }

    /// <summary>
    /// Complete building construction or upgrade
    /// </summary>
    public void CompleteConstruction(string instanceId)
    {
        var building = playerBase.buildings.Find(b => b.instanceId == instanceId);
        if (building == null || !building.isConstructing)
            return;

        if (DateTime.Now >= building.constructionEndTime)
        {
            building.isConstructing = false;
            building.level++;
            Debug.Log($"Construction completed! Building is now level {building.level}");
        }
    }

    /// <summary>
    /// Collect resources from production building
    /// </summary>
    public Dictionary<string, int> CollectProduction(string instanceId)
    {
        var building = playerBase.buildings.Find(b => b.instanceId == instanceId);
        if (building == null)
        {
            Debug.LogError("Building not found");
            return new Dictionary<string, int>();
        }

        var buildingConfig = buildingDatabase.buildingTypes.Find(b => b.buildingId == building.buildingTypeId);
        if (buildingConfig == null || buildingConfig.production == null)
        {
            Debug.Log("Building does not produce resources");
            return new Dictionary<string, int>();
        }

        // Calculate production since last collection
        TimeSpan timeSinceCollection = DateTime.Now - building.lastCollectionTime;
        int productionCycles = (int)(timeSinceCollection.TotalSeconds / buildingConfig.production.productionInterval);

        if (productionCycles <= 0)
        {
            Debug.Log("No resources ready to collect");
            return new Dictionary<string, int>();
        }

        // Calculate production amount (based on building level)
        var upgradeConfig = buildingConfig.upgrades.Find(u => u.level == building.level);
        float productionMultiplier = upgradeConfig != null ? upgradeConfig.productionBonus : 1.0f;

        int baseAmount = buildingConfig.production.baseProduction;
        int totalAmount = (int)(baseAmount * productionMultiplier * productionCycles);

        // Cap at storage limit
        int storageLimit = buildingConfig.production.storageCapacity;
        totalAmount = Mathf.Min(totalAmount, storageLimit);

        // Add to player resources
        string resourceType = buildingConfig.production.resourceType;
        if (!playerBase.resources.ContainsKey(resourceType))
        {
            playerBase.resources[resourceType] = 0;
        }
        playerBase.resources[resourceType] += totalAmount;

        building.lastCollectionTime = DateTime.Now;

        Debug.Log($"Collected {totalAmount} {resourceType}");
        return new Dictionary<string, int> { { resourceType, totalAmount } };
    }

    /// <summary>
    /// Start research
    /// </summary>
    public bool StartResearch(string researchId)
    {
        var researchConfig = buildingDatabase.researchTree.Find(r => r.researchId == researchId);
        if (researchConfig == null)
        {
            Debug.LogError($"Research {researchId} not found");
            return false;
        }

        // Check if already completed
        if (playerBase.completedResearch.Contains(researchId))
        {
            Debug.Log("Research already completed");
            return false;
        }

        // Check prerequisites
        foreach (var prereq in researchConfig.prerequisiteResearch)
        {
            if (!playerBase.completedResearch.Contains(prereq))
            {
                Debug.Log($"Prerequisite research {prereq} not completed");
                return false;
            }
        }

        // Check required building
        if (!string.IsNullOrEmpty(researchConfig.requiredBuilding))
        {
            var building = playerBase.buildings.Find(b =>
                b.buildingTypeId == researchConfig.requiredBuilding &&
                b.level >= researchConfig.requiredBuildingLevel);

            if (building == null)
            {
                Debug.Log($"Required building {researchConfig.requiredBuilding} level {researchConfig.requiredBuildingLevel} not found");
                return false;
            }
        }

        // Check resources
        if (!HasResources(researchConfig.researchCost))
        {
            Debug.Log("Insufficient resources");
            return false;
        }

        // Deduct resources and start research
        DeductResources(researchConfig.researchCost);

        var production = new ProductionJob
        {
            jobId = Guid.NewGuid().ToString(),
            jobType = "research",
            itemId = researchId,
            startTime = DateTime.Now,
            endTime = DateTime.Now.AddSeconds(researchConfig.researchTime),
            isComplete = false
        };

        playerBase.activeProduction.Add(production);
        Debug.Log($"Started research: {researchConfig.researchName}");
        return true;
    }

    /// <summary>
    /// Complete research
    /// </summary>
    public void CompleteResearch(string jobId)
    {
        var job = playerBase.activeProduction.Find(j => j.jobId == jobId && j.jobType == "research");
        if (job == null)
            return;

        if (DateTime.Now >= job.endTime)
        {
            job.isComplete = true;
            playerBase.completedResearch.Add(job.itemId);
            playerBase.activeProduction.Remove(job);

            var researchConfig = buildingDatabase.researchTree.Find(r => r.researchId == job.itemId);
            Debug.Log($"Research completed: {researchConfig?.researchName}");
        }
    }

    /// <summary>
    /// Apply building bonuses to hero
    /// </summary>
    public void ApplyBuildingBonuses(CombatUnit unit, string heroId)
    {
        // Training Hall bonus
        var trainingHall = playerBase.buildings.Find(b => b.buildingTypeId == "training_hall");
        if (trainingHall != null)
        {
            var config = buildingDatabase.buildingTypes.Find(b => b.buildingId == "training_hall");
            var upgrade = config?.upgrades.Find(u => u.level == trainingHall.level);
            if (upgrade != null)
            {
                float expBonus = upgrade.productionBonus;
                // Apply exp bonus (would be used in actual training logic)
            }
        }

        // Apply research bonuses
        foreach (var researchId in playerBase.completedResearch)
        {
            var research = buildingDatabase.researchTree.Find(r => r.researchId == researchId);
            if (research?.statBonus != null)
            {
                unit.ModifyAttack((int)research.statBonus.bonusAtk);
                unit.ModifyDefense((int)research.statBonus.bonusDef);
                unit.ModifyMaxHP((int)research.statBonus.bonusHp);
            }
        }
    }

    /// <summary>
    /// Get all buildings of a specific type
    /// </summary>
    public List<BuildingInstance> GetBuildingsByType(string buildingTypeId)
    {
        return playerBase.buildings.Where(b => b.buildingTypeId == buildingTypeId).ToList();
    }

    /// <summary>
    /// Get building by instance ID
    /// </summary>
    public BuildingInstance GetBuilding(string instanceId)
    {
        return playerBase.buildings.Find(b => b.instanceId == instanceId);
    }

    // Helper methods
    private bool HasResources(List<ResourceCost> costs)
    {
        foreach (var cost in costs)
        {
            if (!playerBase.resources.ContainsKey(cost.resourceType) ||
                playerBase.resources[cost.resourceType] < cost.amount)
            {
                return false;
            }
        }
        return true;
    }

    private void DeductResources(List<ResourceCost> costs)
    {
        foreach (var cost in costs)
        {
            playerBase.resources[cost.resourceType] -= cost.amount;
        }
    }

    private bool AddBuilding(string buildingTypeId, int gridX, int gridY)
    {
        var building = new BuildingInstance
        {
            instanceId = Guid.NewGuid().ToString(),
            buildingTypeId = buildingTypeId,
            level = 1,
            gridX = gridX,
            gridY = gridY,
            isConstructing = false,
            lastCollectionTime = DateTime.Now
        };

        playerBase.buildings.Add(building);
        return true;
    }
}

/// <summary>
/// Database containing building configs
/// </summary>
[Serializable]
public class BaseBuildingDatabase
{
    public List<BuildingTypeConfig> buildingTypes = new List<BuildingTypeConfig>();
    public List<ResearchConfig> researchTree = new List<ResearchConfig>();
    public List<LayoutPreset> layoutPresets = new List<LayoutPreset>();
}
