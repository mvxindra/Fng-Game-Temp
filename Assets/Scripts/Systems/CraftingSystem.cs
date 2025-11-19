using UnityEngine;

public class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    public MaterialInventory materialInventory;
    public GearInventory gearInventory;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool CraftGear(string materialId, int materialCost, string gearId)
    {
        if (!materialInventory.Has(materialId, materialCost))
            return false;

        materialInventory.Remove(materialId, materialCost);
        gearInventory.AddGearById(gearId, 1);
        return true;
    }

    public bool ReforgeGearSubstats(string materialId, int materialCost, GearInstance gearInstance)
    {
        if (!materialInventory.Has(materialId, materialCost))
            return false;

        materialInventory.Remove(materialId, materialCost);

        // Example: just log; actual reroll logic depends on your design.
        Debug.Log($"[CraftingSystem] Reforged gear {gearInstance.instanceId}");
        return true;
    }
}

/// <summary>
/// Runtime gear instance with augmentation system for long-term progression
/// </summary>
public class GearInstance
{
    public string instanceId;
    public string configId;   // maps to GearConfig
    public int level;
    public int enhance;
    public int passiveLevel;

    // Weapon-specific upgrade system
    public int limitBreakLevel;      // Limit break level (0-5) - increases max level cap

    // Augmentation system - external upgrades that persist and scale with weekly/monthly content
    public int augmentTier;          // 0-10+ tiers of augmentation (main power progression)
    public int refinementLevel;      // Additional stat refinement (0-20)
    public int potentialLevel;       // Unlock potential for extra substats (0-5)
    public bool isAwakened;          // Awakened state for bonus effects

    // Track augmentation materials used (for display/progression tracking)
    public int weeklyMaterialsUsed;
    public int monthlyMaterialsUsed;
    public int seasonalMaterialsUsed;
}
