using System;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using FnGMafia.Core;

 

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

            Debug.Log($"Loaded {buildingDatabase.buildings.Count} building types");

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

            playerId = "player_1",

            baseLevel = 1,

            buildings = new Dictionary<string, Building>(),

            resources = new Dictionary<string, int>

            {

                { "gold", 1000 },

                { "gems", 100 },

                { "wood", 500 },

                { "stone", 500 },

                { "iron", 200 }

            }

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

        var buildingConfig = buildingDatabase.buildings.Find(b => b.buildingId == buildingTypeId);

        if (buildingConfig == null)

        {

            Debug.LogError($"Building type {buildingTypeId} not found");

            return false;

        }

 

        // Check if building already exists

        if (playerBase.buildings.Any(b => b.Value.buildingType == buildingTypeId && buildingConfig.maxLevel == 1))

        {

            Debug.Log($"Building {buildingTypeId} already exists and can only have 1");

            return false;

        }

 

        // Count existing buildings of this type

        int existingCount = playerBase.buildings.Count(b => b.Value.buildingType == buildingTypeId);

        if (existingCount >= buildingConfig.maxLevel)

        {

            Debug.Log($"Maximum number of {buildingTypeId} buildings reached ({buildingConfig.maxLevel})");

            return false;

        }

 

        // Check requirements

        if (playerBase.baseLevel < buildingConfig.requiredBaseLevel)

        {

            Debug.Log($"Base level {buildingConfig.requiredBaseLevel} required");

            return false;

        }



        // Check resources

        if (!HasResources(buildingConfig.unlockMaterials))

        {

            Debug.Log("Insufficient resources");

            return false;

        }

 

        // Deduct resources

        DeductResources(buildingConfig.unlockMaterials);



        // Create building instance

        var building = new Building

        {

            buildingId = Guid.NewGuid().ToString(),

            buildingType = buildingTypeId,

            level = 1,

            maxLevel = buildingConfig.maxLevel,

            isUpgrading = true,

            upgradeStartTime = DateTime.Now,

            upgradeCompleteTime = DateTime.Now.AddSeconds(buildingConfig.unlockCost),

            isUnlocked = true,

            unlockedDate = DateTime.Now

        };



        playerBase.buildings.Add(building.buildingId, building);

        Debug.Log($"Started construction of {buildingConfig.buildingName}");

        return true;

    }

 

    /// <summary>

    /// Upgrade a building

    /// </summary>

    public bool UpgradeBuilding(string instanceId)

    {

        var building = playerBase.buildings.ContainsKey(instanceId) ? playerBase.buildings[instanceId] : null;

        if (building == null)

        {

            Debug.LogError("Building not found");

            return false;

        }



        var buildingConfig = buildingDatabase.buildings.Find(b => b.buildingId == building.buildingType);

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

        var upgradeConfig = buildingConfig.levels.Find(u => u.level == building.level + 1);

        if (upgradeConfig == null)

        {

            Debug.LogError($"Upgrade config for level {building.level + 1} not found");

            return false;

        }

 

        // Check resources

        if (!HasResources(upgradeConfig.materials))

        {

            Debug.Log("Insufficient resources");

            return false;

        }



        // Deduct resources

        DeductResources(upgradeConfig.materials);



        // Start upgrade

        building.isUpgrading = true;

        building.upgradeStartTime = DateTime.Now;

        building.upgradeCompleteTime = DateTime.Now.AddSeconds(upgradeConfig.upgradeDuration);

 

        Debug.Log($"Started upgrade to level {building.level + 1}");

        return true;

    }

 

    /// <summary>

    /// Complete building construction or upgrade

    /// </summary>

    public void CompleteConstruction(string instanceId)

    {

        var building = playerBase.buildings.ContainsKey(instanceId) ? playerBase.buildings[instanceId] : null;

        if (building == null || !building.isUpgrading)

            return;



        if (DateTime.Now >= building.upgradeCompleteTime)

        {

            building.isUpgrading = false;

            building.level++;

            Debug.Log($"Construction completed! Building is now level {building.level}");

        }

    }

 

    /// <summary>

    /// Collect resources from production building

    /// </summary>

    public Dictionary<string, int> CollectProduction(string instanceId)

    {

        var building = playerBase.buildings.ContainsKey(instanceId) ? playerBase.buildings[instanceId] : null;

        if (building == null)

        {

            Debug.LogError("Building not found");

            return new Dictionary<string, int>();

        }

 

        var buildingConfig = buildingDatabase.buildings.Find(b => b.buildingId == building.buildingType);

        if (buildingConfig == null || buildingConfig.producedResource == null)

        {

            Debug.Log("Building does not produce resources");

            return new Dictionary<string, int>();

        }

 

        // Use stored production

        int totalAmount = building.storedProduction;



        if (totalAmount <= 0)

        {

            Debug.Log("No resources ready to collect");

            return new Dictionary<string, int>();

        }



        // Add to player resources

        string resourceType = buildingConfig.producedResource;

        if (!playerBase.resources.ContainsKey(resourceType))

        {

            playerBase.resources[resourceType] = 0;

        }

        playerBase.resources[resourceType] += totalAmount;



        building.storedProduction = 0;

 

        Debug.Log($"Collected {totalAmount} {resourceType}");

        return new Dictionary<string, int> { { resourceType, totalAmount } };

    }

 

    /// <summary>

    /// Start research

    /// </summary>

    public bool StartResearch(string researchId)

    {

        var researchConfig = buildingDatabase.research.Find(r => r.researchId == researchId);

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

        if (!string.IsNullOrEmpty(researchConfig.requiredLibraryLevel.ToString()))

        {

            var building = playerBase.buildings.Values.FirstOrDefault(b =>

                b.buildingType == "library" &&

                b.level >= researchConfig.requiredLibraryLevel);

 

            if (building == null)

            {

                Debug.Log($"Required library level {researchConfig.requiredLibraryLevel} not found");

                return false;

            }

        }



        // Check resources

        if (!HasResources(researchConfig.materials))

        {

            Debug.Log("Insufficient resources");

            return false;

        }

 

        // Deduct resources and start research

        DeductResources(researchConfig.materials);

 

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

 

            var researchConfig = buildingDatabase.research.Find(r => r.researchId == job.itemId);

            Debug.Log($"Research completed: {researchConfig?.researchName}");

        }

    }

 

    /// <summary>

    /// Apply building bonuses to hero

    /// </summary>

    public void ApplyBuildingBonuses(CombatUnit unit, string heroId)

    {

        // Training Hall bonus

        var trainingHall = playerBase.buildings.Values.FirstOrDefault(b => b.buildingType == "training_hall");

        if (trainingHall != null)

        {

            var config = buildingDatabase.buildings.Find(b => b.buildingId == "training_hall");

            var upgrade = config?.levels.Find(u => u.level == trainingHall.level);

            if (upgrade != null)

            {

                float expBonus = upgrade.productionBonus;

                // Apply exp bonus (would be used in actual training logic)

            }

        }

 

        // Apply research bonuses

        foreach (var researchId in playerBase.completedResearch)

        {

            var research = buildingDatabase.research.Find(r => r.researchId == researchId);

            if (research?.bonuses != null)

            {

                foreach (var bonus in research.bonuses)

                {

                    // Apply research bonuses

                    // Note: Research bonuses don't have specific stat modifiers in the data model

                    Debug.Log($"Applied research bonus: {bonus.bonusType} = {bonus.bonusValue}");

                }

            }

        }

    }

 

    /// <summary>

    /// Get all buildings of a specific type

    /// </summary>

    public List<Building> GetBuildingsByType(string buildingTypeId)

    {

        return playerBase.buildings.Values.Where(b => b.buildingType == buildingTypeId).ToList();

    }

 

    /// <summary>

    /// Get building by instance ID

    /// </summary>

    public Building GetBuilding(string buildingId)

    {

        return playerBase.buildings.ContainsKey(buildingId) ? playerBase.buildings[buildingId] : null;

    }

 

    // Helper methods

    private bool HasResources(List<MaterialRequirement> costs)

    {

        foreach (var cost in costs)

        {

            if (!playerBase.resources.ContainsKey(cost.materialId) ||

                playerBase.resources[cost.materialId] < cost.quantity)

            {

                return false;

            }

        }

        return true;

    }

 

    private void DeductResources(List<MaterialRequirement> costs)

    {

        foreach (var cost in costs)

        {

            playerBase.resources[cost.materialId] -= cost.quantity;

        }

    }

 

    private bool AddBuilding(string buildingTypeId, int gridX, int gridY)

    {

        var buildingConfig = buildingDatabase.buildings.Find(b => b.buildingId == buildingTypeId);

        var building = new Building

        {

            buildingId = Guid.NewGuid().ToString(),

            buildingType = buildingTypeId,

            level = 1,

            maxLevel = buildingConfig?.maxLevel ?? 10,

            isUpgrading = false,

            isUnlocked = true,

            unlockedDate = DateTime.Now

        };



        playerBase.buildings.Add(building.buildingId, building);

        return true;

    }

}

 

