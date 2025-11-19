using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SetBonusEffect
{
    public int atkPercent;
    public int defPercent;
    public int hpPercent;
    public int spdPercent;
    public int critRate;
    public int critDmg;
    public int acc;
    public int res;
    public int damageBonus; // Flat damage increase
    public int healingBonus; // Healing effectiveness
    public string statusImmunity; // e.g., "BURN", "STUN"
}

[Serializable]
public class GearSetBonus
{
    public int piecesRequired; // Number of pieces needed for this bonus
    public string description;
    public SetBonusEffect effect;
}

[Serializable]
public class GearSetConfig
{
    public string setId;
    public string setName;
    public List<GearSetBonus> bonuses; // Can have 2-piece and 4-piece bonuses
}

[Serializable]
public class GearSetConfigList
{
    public List<GearSetConfig> sets;
}

public class GearSetBonusSystem : MonoBehaviour
{
    public static GearSetBonusSystem Instance { get; private set; }

    private Dictionary<string, GearSetConfig> _sets = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSets();
    }

    private void LoadSets()
    {
        var json = Resources.Load<TextAsset>("Config/gear_sets");
        if (json == null)
        {
            Debug.LogWarning("[GearSetBonusSystem] gear_sets.json not found. No set bonuses available.");
            return;
        }

        var list = JsonUtility.FromJson<GearSetConfigList>(json.text);
        _sets.Clear();

        foreach (var set in list.sets)
        {
            _sets[set.setId] = set;
        }

        Debug.Log($"[GearSetBonusSystem] Loaded {_sets.Count} gear sets.");
    }

    public GearSetConfig GetSet(string setId)
    {
        _sets.TryGetValue(setId, out var config);
        return config;
    }

    // Calculate active set bonuses from equipped gear
    public List<SetBonusEffect> CalculateSetBonuses(List<string> equippedGearSets)
    {
        var setBonuses = new List<SetBonusEffect>();
        var setCounts = new Dictionary<string, int>();

        // Count pieces of each set
        foreach (var setId in equippedGearSets)
        {
            if (string.IsNullOrEmpty(setId)) continue;

            if (!setCounts.ContainsKey(setId))
                setCounts[setId] = 0;

            setCounts[setId]++;
        }

        // Apply bonuses based on piece counts
        foreach (var kvp in setCounts)
        {
            var setConfig = GetSet(kvp.Key);
            if (setConfig == null) continue;

            int pieceCount = kvp.Value;

            // Apply all bonuses that meet the piece requirement
            foreach (var bonus in setConfig.bonuses)
            {
                if (pieceCount >= bonus.piecesRequired)
                {
                    setBonuses.Add(bonus.effect);
                    Debug.Log($"[GearSetBonusSystem] {setConfig.setName} {bonus.piecesRequired}-piece bonus active!");
                }
            }
        }

        return setBonuses;
    }
}
