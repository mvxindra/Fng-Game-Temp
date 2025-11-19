using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GearLevelScaling
{
    public int maxLevel;
    public int atkPerLevel;
    public int magicPerLevel;
    public int hpPerLevel;
}

[Serializable]
public class GearEnhanceScaling
{
    public int maxEnhance;
    public int mainStatPercentPerEnhance;
}

[Serializable]
public class GearPassiveScaling
{
    public int maxPassiveLevel;
    public int perLevelBonusPercent;
}

[Serializable]
public class GearReforgeRule
{
    public int rerollSubstats;
}

[Serializable]
public class GearUpgradeConfigRoot
{
    public Dictionary<string, GearLevelScaling> levelScaling;
    public Dictionary<string, GearEnhanceScaling> enhanceScaling;
    public Dictionary<string, GearPassiveScaling> passiveScaling;
    public Dictionary<string, GearReforgeRule> reforge;
}

public class GearUpgradeSystem : MonoBehaviour
{
    public static GearUpgradeSystem Instance { get; private set; }

    private GearUpgradeConfigRoot config;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        var json = Resources.Load<TextAsset>("Config/gear_upgrade");
        if (json == null)
        {
            Debug.LogError("[GearUpgradeSystem] gear_upgrade.json missing!");
            return;
        }

        config = JsonUtility.FromJson<GearUpgradeConfigRoot>(json.text);
        Debug.Log("[GearUpgradeSystem] Loaded gear_upgrade config.");
    }

    public GearLevelScaling GetLevelScaling(int rarity)
    {
        if (config == null || config.levelScaling == null) return null;
        config.levelScaling.TryGetValue(rarity.ToString(), out var s);
        return s;
    }

    public GearEnhanceScaling GetEnhanceScaling(int rarity)
    {
        if (config == null || config.enhanceScaling == null) return null;
        config.enhanceScaling.TryGetValue(rarity.ToString(), out var s);
        return s;
    }

    public GearPassiveScaling GetPassiveScaling(int rarity)
    {
        if (config == null || config.passiveScaling == null) return null;
        config.passiveScaling.TryGetValue(rarity.ToString(), out var s);
        return s;
    }

    public GearReforgeRule GetReforgeRule(int rarity)
    {
        if (config == null || config.reforge == null) return null;
        config.reforge.TryGetValue(rarity.ToString(), out var r);
        return r;
    }
}
