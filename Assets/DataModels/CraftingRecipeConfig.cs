using System;
using System.Collections.Generic;

/// <summary>
/// Enhanced crafting recipe system
/// </summary>
[Serializable]
public class CraftingRecipeConfig
{
    public string recipeId;              // Unique recipe identifier
    public string recipeName;            // Display name
    public string recipeCategory;        // "weapon", "armor", "accessory", "rune", "material"
    public string recipeType;            // "craft", "upgrade", "reforge", "transmute"

    // Requirements
    public int requiredCrafterLevel;     // Player crafter level required
    public string requiredFacility;      // "forge", "enchanter", "jeweler", "alchemist"
    public List<string> prerequisiteRecipes; // Recipes that must be learned first

    // Inputs
    public List<MaterialRequirement> materials;
    public int goldCost;
    public int energyCost;               // Stamina/energy cost

    // Output
    public string resultItemId;          // Item produced (gear, rune, etc.)
    public int resultQuantity;           // Number produced
    public int resultMinRarity;          // Minimum rarity for random gear
    public int resultMaxRarity;          // Maximum rarity for random gear

    // Crafting process
    public int craftingTime;             // Seconds (0 = instant)
    public float successChance;          // 1.0 = always succeed
    public int maxCrafts;                // Max crafts before recipe expires (-1 = unlimited)

    // Bonuses
    public bool canCriticalCraft;        // Can produce better version
    public float criticalChance;         // Chance to crit craft
    public CriticalCraftBonus critBonus; // Bonuses on crit

    // Unlocking
    public bool isDiscovered;            // Player knows this recipe
    public string unlockMethod;          // "default", "drop", "quest", "achievement"
}

/// <summary>
/// Bonus for critical crafts
/// </summary>
[Serializable]
public class CriticalCraftBonus
{
    public int bonusQuantity;            // Extra items produced
    public int bonusRarity;              // Increase rarity tier
    public float bonusQuality;           // Increase quality roll
    public bool guaranteeSocket;         // Guarantee a socket
    public bool guaranteeAffix;          // Guarantee extra affix
}

/// <summary>
/// Material requirement for crafting
/// </summary>
[Serializable]
public class CraftingMaterialRequirement
{
    public string materialId;
    public int quantity;
    public bool isOptional;              // Optional material for bonuses
    public string bonusEffect;           // Effect if optional material used
}

/// <summary>
/// Gear transmutation recipe (convert gear to materials)
/// </summary>
[Serializable]
public class TransmutationRecipe
{
    public int minRarity;                // Minimum gear rarity
    public int maxRarity;                // Maximum gear rarity
    public string gearSlot;              // "weapon", "armor", "accessory", "any"

    // Outputs (scaled by rarity)
    public List<MaterialReward> baseRewards;
    public float rarityMultiplier;       // Multiply rewards by rarity

    // Bonus rewards
    public float affixBonusPerAffix;     // Extra materials per affix
    public float socketBonusPerSocket;   // Extra materials per socket
    public List<MaterialReward> uniqueBonus; // Bonus for transmuting unique
}

/// <summary>
/// Forge upgrade recipe (improve gear quality)
/// </summary>
[Serializable]
public class ForgeUpgradeRecipe
{
    public string upgradeType;           // "quality", "sockets", "affixes", "rarity"
    public string recipeName;

    // Costs
    public List<MaterialRequirement> materials;
    public int goldCost;

    // Quality upgrade
    public float qualityIncrease;        // Increase quality by X

    // Socket upgrade
    public int socketsToAdd;             // Add X sockets
    public float socketUnlockChance;     // Chance to unlock socket

    // Affix upgrade
    public int affixesToAdd;             // Add X affixes
    public bool rerollAffixes;           // Reroll existing affixes
    public int minAffixTier;             // Minimum tier for new affixes

    // Rarity upgrade
    public bool canUpgradeRarity;        // Can increase rarity
    public float rarityUpgradeChance;    // Chance to increase rarity
}

/// <summary>
/// Material conversion recipe
/// </summary>
[Serializable]
public class MaterialConversionRecipe
{
    public string recipeId;
    public string recipeName;

    // Input materials
    public string inputMaterialId;
    public int inputQuantity;

    // Output materials
    public string outputMaterialId;
    public int outputQuantity;

    // Optional catalyst
    public string catalystMaterialId;    // Optional material to improve yield
    public int catalystQuantity;
    public float catalystBonus;          // Bonus multiplier if catalyst used

    public int goldCost;
}

/// <summary>
/// Salvage recipe (break down gear for materials)
/// </summary>
[Serializable]
public class SalvageRecipe
{
    public string gearSlot;              // "weapon", "armor", "accessory", "any"
    public int minRarity;
    public int maxRarity;

    // Base salvage yield
    public List<MaterialReward> baseYield;

    // Modifiers
    public float levelMultiplier;        // Multiply by gear level
    public float enhanceMultiplier;      // Multiply by enhance level
    public float qualityMultiplier;      // Multiply by quality

    // Socket salvage
    public bool extractRunes;            // Extract runes or destroy them
    public float runeExtractChance;      // Chance to extract rune intact

    // Affix salvage
    public List<MaterialReward> affixMaterials; // Materials per affix
}

/// <summary>
/// Crafting station/facility
/// </summary>
[Serializable]
public class CraftingFacilityConfig
{
    public string facilityId;            // "forge", "enchanter", "jeweler"
    public string facilityName;          // Display name
    public string description;           // Description
    public int maxLevel;                 // Max facility level

    // Level benefits
    public List<FacilityLevelBonus> levelBonuses;

    // Unlocking
    public int goldCost;
    public List<MaterialRequirement> materials;
}

/// <summary>
/// Bonuses per facility level
/// </summary>
[Serializable]
public class FacilityLevelBonus
{
    public int level;
    public float successChanceBonus;     // Increase success chance
    public float criticalChanceBonus;    // Increase crit craft chance
    public float costReduction;          // Reduce crafting costs
    public int unlockRecipeCount;        // Number of new recipes unlocked
    public List<string> unlockedRecipes; // Specific recipes unlocked
}

/// <summary>
/// Database for enhanced crafting system
/// </summary>
[Serializable]
public class EnhancedCraftingDatabase
{
    public List<CraftingRecipeConfig> recipes;
    public List<TransmutationRecipe> transmutations;
    public List<ForgeUpgradeRecipe> forgeUpgrades;
    public List<MaterialConversionRecipe> conversions;
    public List<SalvageRecipe> salvageRecipes;
    public List<CraftingFacilityConfig> facilities;
}
