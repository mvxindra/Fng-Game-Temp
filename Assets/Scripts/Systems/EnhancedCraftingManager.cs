using UnityEngine;

using System.Collections.Generic;

using System.Linq;

using FnGMafia.Core;

 

/// <summary>

/// Enhanced crafting system with recipes, transmutation, and salvage

/// </summary>

public class EnhancedCraftingManager : Singleton<EnhancedCraftingManager>

{

    private Dictionary<string, CraftingRecipeConfig> recipes;

    private Dictionary<string, TransmutationRecipe> transmutations;

    private Dictionary<string, ForgeUpgradeRecipe> forgeUpgrades;

    private Dictionary<string, MaterialConversionRecipe> conversions;

    private List<SalvageRecipe> salvageRecipes;

    private Dictionary<string, CraftingFacilityConfig> facilities;

 

    // Player progression

    private Dictionary<string, int> facilityLevels;

    private HashSet<string> discoveredRecipes;

 

    protected override void Awake()

    {

        base.Awake();

        recipes = new Dictionary<string, CraftingRecipeConfig>();

        transmutations = new Dictionary<string, TransmutationRecipe>();

        forgeUpgrades = new Dictionary<string, ForgeUpgradeRecipe>();

        conversions = new Dictionary<string, MaterialConversionRecipe>();

        salvageRecipes = new List<SalvageRecipe>();

        facilities = new Dictionary<string, CraftingFacilityConfig>();

        facilityLevels = new Dictionary<string, int>();

        discoveredRecipes = new HashSet<string>();

        LoadConfigs();

    }

 

    private void LoadConfigs()

    {

        TextAsset configFile = Resources.Load<TextAsset>("Config/crafting_recipes");

        if (configFile != null)

        {

            EnhancedCraftingDatabase db = JsonUtility.FromJson<EnhancedCraftingDatabase>(configFile.text);

 

            foreach (var recipe in db.recipes)

            {

                recipes[recipe.recipeId] = recipe;

            }

 

            foreach (var trans in db.transmutations)

            {

                transmutations[trans.minRarity + "_" + trans.gearSlot] = trans;

            }

 

            foreach (var upgrade in db.forgeUpgrades)

            {

                forgeUpgrades[upgrade.upgradeType] = upgrade;

            }

 

            foreach (var conversion in db.conversions)

            {

                conversions[conversion.recipeId] = conversion;

            }

 

            salvageRecipes = db.salvageRecipes;

 

            foreach (var facility in db.facilities)

            {

                facilities[facility.facilityId] = facility;

                facilityLevels[facility.facilityId] = 1; // Start at level 1

            }

 

            Debug.Log($"Loaded {recipes.Count} crafting recipes, {transmutations.Count} transmutations, {facilities.Count} facilities");

        }

        else

        {

            Debug.LogWarning("crafting_recipes.json not found");

        }

    }

 

    /// <summary>

    /// Craft gear using a recipe

    /// </summary>

    public EnhancedGearInstance CraftGear(string recipeId)

    {

        if (!recipes.ContainsKey(recipeId))

        {

            Debug.LogWarning($"Recipe {recipeId} not found");

            return null;

        }

 

        CraftingRecipeConfig recipe = recipes[recipeId];

 

        // Check if recipe is discovered

        if (!recipe.isDiscovered && !discoveredRecipes.Contains(recipeId))

        {

            Debug.LogWarning($"Recipe {recipeId} not discovered yet");

            return null;

        }

 

        // Check materials and gold

        if (!CheckMaterials(recipe.materials))

        {

            Debug.LogWarning("Insufficient materials");

            return null;

        }

 

        if (!CheckGold(recipe.goldCost))

        {

            Debug.LogWarning("Insufficient gold");

            return null;

        }

 

        // Check success chance

        float successChance = recipe.successChance;

        string facility = recipe.requiredFacility;

        if (!string.IsNullOrEmpty(facility) && facilityLevels.ContainsKey(facility))

        {

            // Apply facility bonus

            int level = facilityLevels[facility];

            CraftingFacilityConfig facilityConfig = facilities[facility];

            var levelBonus = facilityConfig.levelBonuses.Find(b => b.level == level);

            if (levelBonus != null)

            {

                successChance += levelBonus.successChanceBonus;

            }

        }

 

        // Roll for success

        float roll = Random.Range(0f, 1f);

        if (roll > successChance)

        {

            Debug.Log("Crafting failed!");

            ConsumeMaterials(recipe.materials);

            ConsumeGold(recipe.goldCost);

            return null;

        }

 

        // Roll for critical craft

        bool isCriticalCraft = false;

        if (recipe.canCriticalCraft)

        {

            float critRoll = Random.Range(0f, 1f);

            if (critRoll <= recipe.criticalChance)

            {

                isCriticalCraft = true;

                Debug.Log("Critical craft!");

            }

        }

 

        // Consume materials and gold

        ConsumeMaterials(recipe.materials);

        ConsumeGold(recipe.goldCost);

 

        // Generate gear

        int rarityTier = Random.Range(recipe.resultMinRarity, recipe.resultMaxRarity + 1);

        if (isCriticalCraft && recipe.critBonus != null)

        {

            rarityTier += recipe.critBonus.bonusRarity;

        }

 

        GearGenerationParams genParams = new GearGenerationParams

        {

            gearConfigId = recipe.resultItemId,

            rarityTier = rarityTier,

            itemLevel = 1, // Could be based on player level

            qualityRoll = Random.Range(0.8f, 1.2f),

            affixCount = Random.Range(2, 5),

            socketCount = Random.Range(1, 3)

        };

 

        if (isCriticalCraft && recipe.critBonus != null)

        {

            genParams.qualityRoll += recipe.critBonus.bonusQuality;

            if (recipe.critBonus.guaranteeSocket)

            {

                genParams.socketCount = Mathf.Max(genParams.socketCount, 2);

            }

            if (recipe.critBonus.guaranteeAffix)

            {

                genParams.affixCount++;

            }

        }

 

        EnhancedGearInstance gear = EnhancedGearManager.Instance.GenerateGear(genParams);

        gear.acquiredFrom = "craft";

 

        Debug.Log($"Crafted gear {gear.instanceId} with rarity tier {rarityTier}");

        return gear;

    }

 

    /// <summary>

    /// Transmute gear into materials

    /// </summary>

    public List<MaterialReward> TransmuteGear(EnhancedGearInstance gear)

    {

        GearConfig gearConfig = GearDatabase.Instance?.GetGear(gear.configId);

        if (gearConfig == null) return null;

 

        // Find appropriate transmutation recipe

        string key = gear.rarityTier + "_" + gearConfig.slot;

        TransmutationRecipe recipe = transmutations.ContainsKey(key) ? transmutations[key] : null;

 

        if (recipe == null)

        {

            // Try generic slot recipe

            key = gear.rarityTier + "_any";

            recipe = transmutations.ContainsKey(key) ? transmutations[key] : null;

        }

 

        if (recipe == null)

        {

            Debug.LogWarning("No transmutation recipe found");

            return null;

        }

 

        List<MaterialReward> rewards = new List<MaterialReward>();

 

        // Base rewards

        foreach (var reward in recipe.baseRewards)

        {

            int quantity = Mathf.RoundToInt(reward.quantity * recipe.rarityMultiplier * gear.rarityTier);

            rewards.Add(new MaterialReward { materialId = reward.materialId, quantity = quantity });

        }

 

        // Affix bonuses

        int affixCount = gear.affixes.Count;

        if (affixCount > 0)

        {

            foreach (var reward in recipe.baseRewards)

            {

                int bonusQuantity = Mathf.RoundToInt(reward.quantity * recipe.affixBonusPerAffix * affixCount);

                rewards.Add(new MaterialReward { materialId = reward.materialId, quantity = bonusQuantity });

            }

        }

 

        // Socket bonuses

        int socketCount = gear.sockets.Count;

        if (socketCount > 0)

        {

            foreach (var reward in recipe.baseRewards)

            {

                int bonusQuantity = Mathf.RoundToInt(reward.quantity * recipe.socketBonusPerSocket * socketCount);

                rewards.Add(new MaterialReward { materialId = reward.materialId, quantity = bonusQuantity });

            }

        }

 

        // Unique bonus

        if (gear.isUnique && recipe.uniqueBonus != null)

        {

            rewards.AddRange(recipe.uniqueBonus);

        }

 

        // Grant rewards

        GrantMaterialRewards(rewards);

 

        Debug.Log($"Transmuted gear {gear.instanceId} into {rewards.Count} material types");

        return rewards;

    }

 

    /// <summary>

    /// Salvage gear for materials

    /// </summary>

    public List<MaterialReward> SalvageGear(EnhancedGearInstance gear)

    {

        GearConfig gearConfig = GearDatabase.Instance?.GetGear(gear.configId);

        if (gearConfig == null) return null;

 

        // Find salvage recipe

        SalvageRecipe recipe = salvageRecipes.Find(r =>

            (r.gearSlot == gearConfig.slot || r.gearSlot == "any") &&

            gear.rarityTier >= r.minRarity &&

            gear.rarityTier <= r.maxRarity

        );

 

        if (recipe == null)

        {

            Debug.LogWarning("No salvage recipe found");

            return null;

        }

 

        List<MaterialReward> rewards = new List<MaterialReward>();

 

        // Base yield

        foreach (var reward in recipe.baseYield)

        {

            float multiplier = 1f;

            multiplier *= 1 + (gear.level * recipe.levelMultiplier);

            multiplier *= 1 + (gear.enhance * recipe.enhanceMultiplier);

            multiplier *= gear.itemQuality * recipe.qualityMultiplier;

 

            int quantity = Mathf.RoundToInt(reward.quantity * multiplier);

            rewards.Add(new MaterialReward { materialId = reward.materialId, quantity = quantity });

        }

 

        // Affix materials

        if (recipe.affixMaterials != null)

        {

            foreach (var affix in gear.affixes)

            {

                foreach (var reward in recipe.affixMaterials)

                {

                    rewards.Add(new MaterialReward { materialId = reward.materialId, quantity = reward.quantity });

                }

            }

        }

 

        // Extract runes

        if (recipe.extractRunes)

        {

            foreach (var socket in gear.sockets)

            {

                if (!string.IsNullOrEmpty(socket.socketedRuneId))

                {

                    float extractRoll = Random.Range(0f, 1f);

                    if (extractRoll <= recipe.runeExtractChance)

                    {

                        // Return rune to inventory

                        RuneSocketManager.Instance?.AddRune(socket.socketedRuneId, 1);

                        Debug.Log($"Extracted rune {socket.socketedRuneId}");

                    }

                }

            }

        }

 

        // Grant rewards

        GrantMaterialRewards(rewards);

 

        Debug.Log($"Salvaged gear {gear.instanceId} for {rewards.Count} material types");

        return rewards;

    }

 

    /// <summary>

    /// Upgrade gear using forge

    /// </summary>

    public bool ForgeUpgradeGear(EnhancedGearInstance gear, string upgradeType)

    {

        if (!forgeUpgrades.ContainsKey(upgradeType))

        {

            Debug.LogWarning($"Forge upgrade {upgradeType} not found");

            return false;

        }

 

        ForgeUpgradeRecipe recipe = forgeUpgrades[upgradeType];

 

        // Check materials

        if (!CheckMaterials(recipe.materials))

        {

            Debug.LogWarning("Insufficient materials");

            return false;

        }

 

        if (!CheckGold(recipe.goldCost))

        {

            Debug.LogWarning("Insufficient gold");

            return false;

        }

 

        ConsumeMaterials(recipe.materials);

        ConsumeGold(recipe.goldCost);

 

        switch (upgradeType)

        {

            case "quality":

                gear.itemQuality += recipe.qualityIncrease;

                Debug.Log($"Increased quality to {gear.itemQuality}");

                break;

 

            case "sockets":

                for (int i = 0; i < recipe.socketsToAdd; i++)

                {

                    gear.sockets.Add(new SocketInstance("prismatic"));

                }

                Debug.Log($"Added {recipe.socketsToAdd} sockets");

                break;

 

            case "affixes":

                if (recipe.rerollAffixes)

                {

                    EnhancedGearManager.Instance.RerollAffixes(gear, 0);

                }

                else

                {

                    // Add new affixes

                    for (int i = 0; i < recipe.affixesToAdd; i++)

                    {

                        // Generate new affix

                        // Would use EnhancedGearManager methods

                    }

                }

                break;

 

            case "rarity":

                if (recipe.canUpgradeRarity)

                {

                    float roll = Random.Range(0f, 1f);

                    if (roll <= recipe.rarityUpgradeChance)

                    {

                        gear.rarityTier++;

                        Debug.Log($"Upgraded rarity to tier {gear.rarityTier}");

                    }

                    else

                    {

                        Debug.Log("Rarity upgrade failed");

                    }

                }

                break;

        }

 

        return true;

    }

 

    /// <summary>

    /// Convert materials

    /// </summary>

    public bool ConvertMaterials(string recipeId, bool useCatalyst)

    {

        if (!conversions.ContainsKey(recipeId))

        {

            return false;

        }

 

        MaterialConversionRecipe recipe = conversions[recipeId];

 

        // Check materials

        if (MaterialInventory.Instance != null)

        {

            if (!MaterialInventory.Instance.Has(recipe.inputMaterialId, recipe.inputQuantity))

            {

                return false;

            }

 

            if (useCatalyst && !string.IsNullOrEmpty(recipe.catalystMaterialId))

            {

                if (!MaterialInventory.Instance.Has(recipe.catalystMaterialId, recipe.catalystQuantity))

                {

                    return false;

                }

            }

        }

 

        // Consume materials

        MaterialInventory.Instance?.Remove(recipe.inputMaterialId, recipe.inputQuantity);

        if (useCatalyst && !string.IsNullOrEmpty(recipe.catalystMaterialId))

        {

            MaterialInventory.Instance?.Remove(recipe.catalystMaterialId, recipe.catalystQuantity);

        }

 

        // Grant output

        int outputQuantity = recipe.outputQuantity;

        if (useCatalyst)

        {

            outputQuantity = Mathf.RoundToInt(outputQuantity * (1 + recipe.catalystBonus));

        }

 

        MaterialInventory.Instance?.Add(recipe.outputMaterialId, outputQuantity);

 

        Debug.Log($"Converted {recipe.inputQuantity}x {recipe.inputMaterialId} to {outputQuantity}x {recipe.outputMaterialId}");

        return true;

    }

 

    // Helper methods

    private bool CheckMaterials(List<MaterialRequirement> materials)

    {

        if (materials == null || MaterialInventory.Instance == null) return true;

 

        foreach (var mat in materials)

        {

            if (!MaterialInventory.Instance.Has(mat.materialId, mat.quantity))

            {

                return false;

            }

        }

        return true;

    }

 

    private bool CheckGold(int goldCost)

    {

        return PlayerWallet.Instance == null || PlayerWallet.Instance.gold >= goldCost;

    }

 

    private void ConsumeMaterials(List<MaterialRequirement> materials)

    {

        if (materials == null || MaterialInventory.Instance == null) return;

 

        foreach (var mat in materials)

        {

            MaterialInventory.Instance.Remove(mat.materialId, mat.quantity);

        }

    }

 

    private void ConsumeGold(int goldCost)

    {

        PlayerWallet.Instance?.DeductGold(goldCost);

    }

 

    private void GrantMaterialRewards(List<MaterialReward> rewards)

    {

        if (rewards == null || MaterialInventory.Instance == null) return;

 

        foreach (var reward in rewards)

        {

            MaterialInventory.Instance.Add(reward.materialId, reward.quantity);

        }

    }

 

    // Recipe discovery

    public void DiscoverRecipe(string recipeId)

    {

        discoveredRecipes.Add(recipeId);

        Debug.Log($"Discovered recipe {recipeId}");

    }

 

    public bool IsRecipeDiscovered(string recipeId)

    {

        return discoveredRecipes.Contains(recipeId);

    }

 

    // Facility management

    public int GetFacilityLevel(string facilityId)

    {

        return facilityLevels.ContainsKey(facilityId) ? facilityLevels[facilityId] : 0;

    }

 

    public bool UpgradeFacility(string facilityId)

    {

        if (!facilities.ContainsKey(facilityId)) return false;

 

        CraftingFacilityConfig facility = facilities[facilityId];

        int currentLevel = GetFacilityLevel(facilityId);

 

        if (currentLevel >= facility.maxLevel) return false;

 

        // Check costs

        if (!CheckGold(facility.goldCost)) return false;

        if (!CheckMaterials(facility.materials)) return false;

 

        // Upgrade

        ConsumeGold(facility.goldCost);

        ConsumeMaterials(facility.materials);

 

        facilityLevels[facilityId] = currentLevel + 1;

 

        // Unlock recipes at this level

        var levelBonus = facility.levelBonuses.Find(b => b.level == currentLevel + 1);

        if (levelBonus != null && levelBonus.unlockedRecipes != null)

        {

            foreach (string recipeId in levelBonus.unlockedRecipes)

            {

                DiscoverRecipe(recipeId);

            }

        }

 

        Debug.Log($"Upgraded {facilityId} to level {currentLevel + 1}");

        return true;

    }

}