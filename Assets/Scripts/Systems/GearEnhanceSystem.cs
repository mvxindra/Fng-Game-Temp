using UnityEngine;

public class GearEnhanceSystem : MonoBehaviour
{
    public static GearEnhanceSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Returns a multiplier (1.0 = base) for main stat after enhance.
    /// Uses rarity to look up per-enhance percent from GearUpgradeSystem.
    /// </summary>
    public float GetMainStatMultiplier(GearConfig gear, int enhanceLevel)
    {
        if (gear == null) return 1f;

        var scaling = GearUpgradeSystem.Instance.GetEnhanceScaling(gear.rarity);
        if (scaling == null) return 1f;

        int clampedEnhance = Mathf.Clamp(enhanceLevel, 0, scaling.maxEnhance);
        float totalPercent = clampedEnhance * scaling.mainStatPercentPerEnhance;
        return 1f + (totalPercent / 100f);
    }

    /// <summary>
    /// Example: apply enhancement to mainAtk stat.
    /// You can extend this for magic, hp, etc.
    /// </summary>
    public int GetEnhancedMainStatValue(int baseValue, GearConfig gear, int enhanceLevel)
    {
        float mult = GetMainStatMultiplier(gear, enhanceLevel);
        return Mathf.FloorToInt(baseValue * mult);
    }
}
