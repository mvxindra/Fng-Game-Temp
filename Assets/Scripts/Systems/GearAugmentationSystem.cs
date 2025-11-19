using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AugmentMaterialCost
{
    public string materialId;
    public int amount;
}

[Serializable]
public class AugmentTierRequirement
{
    public int tier;
    public List<AugmentMaterialCost> materials;
    public int goldCost;
}

[Serializable]
public class AugmentScaling
{
    public int atkBonusPercent;      // % increase to base ATK
    public int defBonusPercent;      // % increase to base DEF
    public int hpBonusPercent;       // % increase to base HP
    public int allStatsBonusPercent; // % increase to all stats
}

[Serializable]
public class AugmentConfig
{
    public List<AugmentTierRequirement> tiers;
    public AugmentScaling scalingPerTier;
    public int maxTier;
}

[Serializable]
public class RefinementConfig
{
    public List<AugmentMaterialCost> materialsPerLevel;
    public int maxLevel;
    public int statIncreasePerLevel; // Flat stat increase per level
}

[Serializable]
public class PotentialConfig
{
    public List<AugmentMaterialCost> materialsPerLevel;
    public int maxLevel;
    public string description; // "Unlocks additional substat at level 1, 3, 5"
}

[Serializable]
public class AwakeningConfig
{
    public List<AugmentMaterialCost> materials;
    public int awakeningBonusPercent; // Additional % to all stats
    public string specialEffect; // Optional special effect
}

[Serializable]
public class GearAugmentationConfig
{
    public AugmentConfig augmentation;
    public RefinementConfig refinement;
    public PotentialConfig potential;
    public AwakeningConfig awakening;
}

public class GearAugmentationSystem : MonoBehaviour
{
    public static GearAugmentationSystem Instance { get; private set; }

    private GearAugmentationConfig config;
    public MaterialInventory materialInventory;
    public PlayerWallet playerWallet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadConfig();
    }

    private void LoadConfig()
    {
        var json = Resources.Load<TextAsset>("Config/gear_augmentation");
        if (json == null)
        {
            Debug.LogError("[GearAugmentationSystem] gear_augmentation.json not found!");
            return;
        }

        config = JsonUtility.FromJson<GearAugmentationConfig>(json.text);
        Debug.Log("[GearAugmentationSystem] Loaded augmentation config.");
    }

    /// <summary>
    /// Augment gear to next tier
    /// </summary>
    public bool AugmentGear(GearInstance gear)
    {
        if (config == null || config.augmentation == null) return false;
        if (gear.augmentTier >= config.augmentation.maxTier)
        {
            Debug.LogWarning($"[GearAugmentationSystem] Gear {gear.instanceId} is already at max augment tier.");
            return false;
        }

        var tierReq = config.augmentation.tiers.Find(t => t.tier == gear.augmentTier + 1);
        if (tierReq == null) return false;

        // Check materials and gold
        foreach (var mat in tierReq.materials)
        {
            if (!materialInventory.Has(mat.materialId, mat.amount))
            {
                Debug.LogWarning($"[GearAugmentationSystem] Not enough {mat.materialId}. Need {mat.amount}");
                return false;
            }
        }

        if (playerWallet.gold < tierReq.goldCost)
        {
            Debug.LogWarning($"[GearAugmentationSystem] Not enough gold. Need {tierReq.goldCost}");
            return false;
        }

        // Consume materials
        foreach (var mat in tierReq.materials)
        {
            materialInventory.Remove(mat.materialId, mat.amount);
        }
        playerWallet.gold -= tierReq.goldCost;

        // Upgrade tier
        gear.augmentTier++;
        Debug.Log($"[GearAugmentationSystem] Gear {gear.instanceId} augmented to tier {gear.augmentTier}!");

        return true;
    }

    /// <summary>
    /// Refine gear for additional flat stats
    /// </summary>
    public bool RefineGear(GearInstance gear)
    {
        if (config == null || config.refinement == null) return false;
        if (gear.refinementLevel >= config.refinement.maxLevel)
        {
            Debug.LogWarning($"[GearAugmentationSystem] Gear {gear.instanceId} is already at max refinement.");
            return false;
        }

        var materials = config.refinement.materialsPerLevel;

        // Check materials
        foreach (var mat in materials)
        {
            if (!materialInventory.Has(mat.materialId, mat.amount))
            {
                Debug.LogWarning($"[GearAugmentationSystem] Not enough {mat.materialId}. Need {mat.amount}");
                return false;
            }
        }

        // Consume materials
        foreach (var mat in materials)
        {
            materialInventory.Remove(mat.materialId, mat.amount);
            gear.weeklyMaterialsUsed += mat.amount; // Track weekly materials
        }

        gear.refinementLevel++;
        Debug.Log($"[GearAugmentationSystem] Gear {gear.instanceId} refined to level {gear.refinementLevel}!");

        return true;
    }

    /// <summary>
    /// Unlock potential for additional substats
    /// </summary>
    public bool UnlockPotential(GearInstance gear)
    {
        if (config == null || config.potential == null) return false;
        if (gear.potentialLevel >= config.potential.maxLevel)
        {
            Debug.LogWarning($"[GearAugmentationSystem] Gear {gear.instanceId} is already at max potential.");
            return false;
        }

        var materials = config.potential.materialsPerLevel;

        // Check materials
        foreach (var mat in materials)
        {
            if (!materialInventory.Has(mat.materialId, mat.amount))
            {
                Debug.LogWarning($"[GearAugmentationSystem] Not enough {mat.materialId}. Need {mat.amount}");
                return false;
            }
        }

        // Consume materials
        foreach (var mat in materials)
        {
            materialInventory.Remove(mat.materialId, mat.amount);
            gear.monthlyMaterialsUsed += mat.amount; // Track monthly materials
        }

        gear.potentialLevel++;
        Debug.Log($"[GearAugmentationSystem] Gear {gear.instanceId} potential unlocked to level {gear.potentialLevel}!");

        return true;
    }

    /// <summary>
    /// Awaken gear for significant power boost
    /// </summary>
    public bool AwakenGear(GearInstance gear)
    {
        if (config == null || config.awakening == null) return false;
        if (gear.isAwakened)
        {
            Debug.LogWarning($"[GearAugmentationSystem] Gear {gear.instanceId} is already awakened.");
            return false;
        }

        var materials = config.awakening.materials;

        // Check materials
        foreach (var mat in materials)
        {
            if (!materialInventory.Has(mat.materialId, mat.amount))
            {
                Debug.LogWarning($"[GearAugmentationSystem] Not enough {mat.materialId}. Need {mat.amount}");
                return false;
            }
        }

        // Consume materials
        foreach (var mat in materials)
        {
            materialInventory.Remove(mat.materialId, mat.amount);
            gear.seasonalMaterialsUsed += mat.amount; // Track seasonal materials
        }

        gear.isAwakened = true;
        Debug.Log($"[GearAugmentationSystem] Gear {gear.instanceId} has been awakened!");

        return true;
    }

    /// <summary>
    /// Calculate total stat bonus from augmentations
    /// </summary>
    public AugmentScaling GetTotalAugmentBonus(GearInstance gear)
    {
        if (config == null) return null;

        var totalBonus = new AugmentScaling();

        // Augment tier bonuses
        if (config.augmentation != null)
        {
            totalBonus.atkBonusPercent = gear.augmentTier * config.augmentation.scalingPerTier.atkBonusPercent;
            totalBonus.defBonusPercent = gear.augmentTier * config.augmentation.scalingPerTier.defBonusPercent;
            totalBonus.hpBonusPercent = gear.augmentTier * config.augmentation.scalingPerTier.hpBonusPercent;
            totalBonus.allStatsBonusPercent = gear.augmentTier * config.augmentation.scalingPerTier.allStatsBonusPercent;
        }

        // Awakening bonus
        if (gear.isAwakened && config.awakening != null)
        {
            totalBonus.allStatsBonusPercent += config.awakening.awakeningBonusPercent;
        }

        return totalBonus;
    }
}
