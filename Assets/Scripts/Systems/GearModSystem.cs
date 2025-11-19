using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GearModEffect
{
    // Stat bonuses
    public int atkBonus;
    public int defBonus;
    public int hpBonus;
    public int spdBonus;
    public int critRateBonus;
    public int critDmgBonus;
    public int accBonus;
    public int resBonus;

    // Percentage bonuses
    public int atkPercentBonus;
    public int defPercentBonus;
    public int hpPercentBonus;
    public int spdPercentBonus;

    // Faction-specific bonuses
    public int factionDamageBonus;      // Extra damage vs specific faction
    public int factionDefenseBonus;     // Reduced damage from specific faction
    public string targetFaction;        // Which faction this affects

    // Combat effects
    public int damageMultiplier;        // General damage increase
    public int healingBonus;            // Healing effectiveness
    public int shieldStrength;          // Shield/barrier strength
    public int lifeSteal;               // Lifesteal %
    public int dodgeChance;             // Evasion %
    public int blockChance;             // Block chance %

    // Special effects
    public string statusImmunity;       // Immune to specific status
    public int cooldownReduction;       // Skill CDR
    public int energyGeneration;        // Extra energy per turn
    public string specialAbility;       // Unique ability description
}

[Serializable]
public class GearMod
{
    public string id;
    public string name;
    public string description;
    public int rarity;                  // 1-5 stars
    public string faction;              // Required faction to equip
    public string modType;              // "offensive", "defensive", "utility", "special"
    public GearModEffect effect;
    public List<string> compatibleSlots; // Which gear slots can use this mod
}

[Serializable]
public class GearModList
{
    public List<GearMod> mods;
}

public class GearModSystem : MonoBehaviour
{
    public static GearModSystem Instance { get; private set; }

    private Dictionary<string, GearMod> modDatabase = new();
    private Dictionary<string, List<GearMod>> modsByFaction = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadMods();
    }

    private void LoadMods()
    {
        var json = Resources.Load<TextAsset>("Config/gear_mods");
        if (json == null)
        {
            Debug.LogError("[GearModSystem] gear_mods.json not found!");
            return;
        }

        var modList = JsonUtility.FromJson<GearModList>(json.text);
        modDatabase.Clear();
        modsByFaction.Clear();

        foreach (var mod in modList.mods)
        {
            modDatabase[mod.id] = mod;

            if (!modsByFaction.ContainsKey(mod.faction))
                modsByFaction[mod.faction] = new List<GearMod>();

            modsByFaction[mod.faction].Add(mod);
        }

        Debug.Log($"[GearModSystem] Loaded {modDatabase.Count} mods across {modsByFaction.Count} factions.");
    }

    public GearMod GetMod(string modId)
    {
        modDatabase.TryGetValue(modId, out var mod);
        return mod;
    }

    public List<GearMod> GetModsForFaction(string faction)
    {
        modsByFaction.TryGetValue(faction, out var mods);
        return mods ?? new List<GearMod>();
    }

    public List<GearMod> GetModsByRarity(string faction, int rarity)
    {
        var factionMods = GetModsForFaction(faction);
        return factionMods.FindAll(m => m.rarity == rarity);
    }

    /// <summary>
    /// Check if a unit can equip a mod based on faction
    /// </summary>
    public bool CanEquipMod(string unitFaction, GearMod mod)
    {
        return mod.faction == unitFaction || mod.faction == "Universal";
    }

    /// <summary>
    /// Apply mod effects to base stats
    /// </summary>
    public void ApplyModToStats(GearMod mod, ref int atk, ref int def, ref int hp, ref int spd)
    {
        if (mod == null || mod.effect == null) return;

        // Flat bonuses
        atk += mod.effect.atkBonus;
        def += mod.effect.defBonus;
        hp += mod.effect.hpBonus;
        spd += mod.effect.spdBonus;

        // Percentage bonuses
        atk += Mathf.RoundToInt(atk * mod.effect.atkPercentBonus / 100f);
        def += Mathf.RoundToInt(def * mod.effect.defPercentBonus / 100f);
        hp += Mathf.RoundToInt(hp * mod.effect.hpPercentBonus / 100f);
        spd += Mathf.RoundToInt(spd * mod.effect.spdPercentBonus / 100f);
    }
}
