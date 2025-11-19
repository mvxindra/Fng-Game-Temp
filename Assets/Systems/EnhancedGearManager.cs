using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages enhanced gear generation with rarity, affixes, and sockets
/// </summary>
public class EnhancedGearManager : Singleton<EnhancedGearManager>
{
    private Dictionary<string, GearRarityConfig> rarityConfigs;
    private Dictionary<string, UniqueGearConfig> uniqueGear;
    private Dictionary<string, NamedGearSetConfig> namedSets;
    private Dictionary<string, GearAffixConfig> affixConfigs;
    private List<AffixPoolConfig> affixPools;
    private GearAffixDatabase affixDatabase;

    protected override void Awake()
    {
        base.Awake();
        rarityConfigs = new Dictionary<string, GearRarityConfig>();
        uniqueGear = new Dictionary<string, UniqueGearConfig>();
        namedSets = new Dictionary<string, NamedGearSetConfig>();
        affixConfigs = new Dictionary<string, GearAffixConfig>();
        affixPools = new List<AffixPoolConfig>();
        LoadConfigs();
    }

    private void LoadConfigs()
    {
        // Load rarity configs
        TextAsset rarityFile = Resources.Load<TextAsset>("Config/gear_rarity");
        if (rarityFile != null)
        {
            GearRarityDatabase rarityDb = JsonUtility.FromJson<GearRarityDatabase>(rarityFile.text);
            foreach (var rarity in rarityDb.rarities)
            {
                rarityConfigs[rarity.rarityId] = rarity;
            }
            foreach (var unique in rarityDb.uniqueGear)
            {
                uniqueGear[unique.uniqueId] = unique;
            }
            foreach (var set in rarityDb.namedSets)
            {
                namedSets[set.setId] = set;
            }
            Debug.Log($"Loaded {rarityConfigs.Count} rarity tiers, {uniqueGear.Count} unique items, {namedSets.Count} named sets");
        }

        // Load affix configs
        TextAsset affixFile = Resources.Load<TextAsset>("Config/gear_affixes");
        if (affixFile != null)
        {
            affixDatabase = JsonUtility.FromJson<GearAffixDatabase>(affixFile.text);
            foreach (var affix in affixDatabase.affixes)
            {
                affixConfigs[affix.affixId] = affix;
            }
            affixPools = affixDatabase.affixPools;
            Debug.Log($"Loaded {affixConfigs.Count} affixes");
        }
    }

    /// <summary>
    /// Generate random gear with affixes and sockets
    /// </summary>
    public EnhancedGearInstance GenerateGear(GearGenerationParams genParams)
    {
        EnhancedGearInstance gear = new EnhancedGearInstance
        {
            instanceId = System.Guid.NewGuid().ToString(),
            configId = genParams.gearConfigId,
            itemLevel = genParams.itemLevel,
            itemQuality = genParams.qualityRoll,
            rarityTier = genParams.rarityTier,
            acquiredDate = System.DateTime.Now,
            acquiredFrom = "drop"
        };

        // Set rarity
        GearRarityConfig rarity = GetRarityConfigByTier(genParams.rarityTier);
        if (rarity != null)
        {
            gear.rarityId = rarity.rarityId;
        }

        // Check if unique
        if (genParams.forceUnique && !string.IsNullOrEmpty(genParams.uniqueId))
        {
            gear.isUnique = true;
            gear.uniqueId = genParams.uniqueId;
        }

        // Set named set
        if (!string.IsNullOrEmpty(genParams.namedSetId))
        {
            gear.namedSetId = genParams.namedSetId;
        }

        // Generate affixes
        GenerateAffixes(gear, genParams);

        // Generate sockets
        GenerateSockets(gear, genParams);

        Debug.Log($"Generated {rarity?.displayName} gear with {gear.affixes.Count} affixes and {gear.sockets.Count} sockets");

        return gear;
    }

    /// <summary>
    /// Generate affixes for gear
    /// </summary>
    private void GenerateAffixes(EnhancedGearInstance gear, GearGenerationParams genParams)
    {
        GearConfig gearConfig = GearDatabase.Instance?.GetGear(gear.configId);
        if (gearConfig == null) return;

        // Add guaranteed affixes first
        if (genParams.guaranteedAffixes != null)
        {
            foreach (string affixId in genParams.guaranteedAffixes)
            {
                AffixInstance affix = CreateAffixInstance(affixId, gear.itemLevel);
                if (affix != null)
                {
                    gear.affixes.Add(affix);
                }
            }
        }

        // Generate random affixes
        int remainingAffixes = genParams.affixCount - gear.affixes.Count;
        for (int i = 0; i < remainingAffixes; i++)
        {
            AffixInstance affix = GenerateRandomAffix(gearConfig.slot, gear.rarityTier, gear.itemLevel);
            if (affix != null)
            {
                gear.affixes.Add(affix);
            }
        }
    }

    /// <summary>
    /// Generate random affix based on slot and rarity
    /// </summary>
    private AffixInstance GenerateRandomAffix(string slot, int rarityTier, int itemLevel)
    {
        // Find affix pool for this slot
        AffixPoolConfig pool = affixPools.Find(p => p.slot == slot && p.minRarity <= rarityTier);
        if (pool == null || pool.affixIds.Count == 0) return null;

        // Weighted random selection
        string selectedAffixId = WeightedRandom(pool.affixIds, pool.weights);
        return CreateAffixInstance(selectedAffixId, itemLevel);
    }

    /// <summary>
    /// Create an affix instance from config
    /// </summary>
    private AffixInstance CreateAffixInstance(string affixId, int itemLevel)
    {
        if (!affixConfigs.ContainsKey(affixId)) return null;

        GearAffixConfig config = affixConfigs[affixId];
        AffixInstance instance = new AffixInstance
        {
            affixId = affixId,
            affixTier = config.affixTier,
            rollQuality = Random.Range(0.7f, 1.3f) // 70% to 130% of base value
        };

        // Calculate applied stats with tier and quality
        instance.appliedStats = CalculateAffixStats(config.statMod, instance.affixTier, instance.rollQuality, itemLevel);

        return instance;
    }

    /// <summary>
    /// Calculate final affix stats with tier and quality modifiers
    /// </summary>
    private AffixStatMod CalculateAffixStats(AffixStatMod baseMod, int tier, float quality, int itemLevel)
    {
        AffixStatMod result = new AffixStatMod();
        float multiplier = tier * quality * (1 + itemLevel * 0.01f);

        // Apply multiplier to all stats
        result.flatAtk = Mathf.RoundToInt(baseMod.flatAtk * multiplier);
        result.flatDef = Mathf.RoundToInt(baseMod.flatDef * multiplier);
        result.flatHp = Mathf.RoundToInt(baseMod.flatHp * multiplier);
        result.flatSpd = Mathf.RoundToInt(baseMod.flatSpd * multiplier);

        result.percentAtk = baseMod.percentAtk * multiplier;
        result.percentDef = baseMod.percentDef * multiplier;
        result.percentHp = baseMod.percentHp * multiplier;
        result.percentSpd = baseMod.percentSpd * multiplier;

        result.critChance = baseMod.critChance * multiplier;
        result.critDamage = baseMod.critDamage * multiplier;
        result.lifeSteal = baseMod.lifeSteal * multiplier;
        result.cooldownReduction = baseMod.cooldownReduction * multiplier;

        // Copy other stats with multiplier...
        return result;
    }

    /// <summary>
    /// Generate sockets for gear
    /// </summary>
    private void GenerateSockets(EnhancedGearInstance gear, GearGenerationParams genParams)
    {
        for (int i = 0; i < genParams.socketCount; i++)
        {
            string socketType = "prismatic"; // Default
            if (genParams.socketTypes != null && i < genParams.socketTypes.Count)
            {
                socketType = genParams.socketTypes[i];
            }

            SocketInstance socket = new SocketInstance(socketType)
            {
                isUnlocked = i == 0 // First socket is unlocked by default
            };

            gear.sockets.Add(socket);
        }

        gear.unlockedSocketCount = 1;
    }

    /// <summary>
    /// Roll random rarity based on drop weights
    /// </summary>
    public int RollRandomRarity()
    {
        if (rarityConfigs.Count == 0) return 3; // Default to rare

        List<string> rarityIds = rarityConfigs.Keys.ToList();
        List<float> weights = rarityConfigs.Values.Select(r => r.dropWeight).ToList();

        string selectedRarity = WeightedRandom(rarityIds, weights);
        return rarityConfigs[selectedRarity].rarityTier;
    }

    /// <summary>
    /// Weighted random selection
    /// </summary>
    private T WeightedRandom<T>(List<T> items, List<float> weights)
    {
        if (items.Count == 0 || items.Count != weights.Count) return default(T);

        float totalWeight = weights.Sum();
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;

        for (int i = 0; i < items.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue <= cumulativeWeight)
            {
                return items[i];
            }
        }

        return items[items.Count - 1];
    }

    /// <summary>
    /// Reroll affixes on gear
    /// </summary>
    public bool RerollAffixes(EnhancedGearInstance gear, int materialCost)
    {
        GearConfig gearConfig = GearDatabase.Instance?.GetGear(gear.configId);
        if (gearConfig == null) return false;

        // Clear existing affixes
        gear.affixes.Clear();

        // Generate new affixes
        GearRarityConfig rarity = GetRarityConfigByTier(gear.rarityTier);
        if (rarity == null) return false;

        int affixCount = Random.Range(rarity.minAffixes, rarity.maxAffixes + 1);
        for (int i = 0; i < affixCount; i++)
        {
            AffixInstance affix = GenerateRandomAffix(gearConfig.slot, gear.rarityTier, gear.itemLevel);
            if (affix != null)
            {
                gear.affixes.Add(affix);
            }
        }

        gear.affixRerollCount++;
        Debug.Log($"Rerolled affixes for gear {gear.instanceId}. New affixes: {gear.affixes.Count}");
        return true;
    }

    /// <summary>
    /// Get rarity config by tier
    /// </summary>
    public GearRarityConfig GetRarityConfigByTier(int tier)
    {
        return rarityConfigs.Values.FirstOrDefault(r => r.rarityTier == tier);
    }

    /// <summary>
    /// Get rarity config by ID
    /// </summary>
    public GearRarityConfig GetRarityConfig(string rarityId)
    {
        return rarityConfigs.ContainsKey(rarityId) ? rarityConfigs[rarityId] : null;
    }

    /// <summary>
    /// Get unique gear config
    /// </summary>
    public UniqueGearConfig GetUniqueGear(string uniqueId)
    {
        return uniqueGear.ContainsKey(uniqueId) ? uniqueGear[uniqueId] : null;
    }

    /// <summary>
    /// Get named set config
    /// </summary>
    public NamedGearSetConfig GetNamedSet(string setId)
    {
        return namedSets.ContainsKey(setId) ? namedSets[setId] : null;
    }

    /// <summary>
    /// Calculate set bonuses for equipped gear
    /// </summary>
    public List<SetBonus> CalculateSetBonuses(List<EnhancedGearInstance> equippedGear)
    {
        List<SetBonus> activeBonuses = new List<SetBonus>();

        // Count pieces per set
        Dictionary<string, int> setCount = new Dictionary<string, int>();
        foreach (var gear in equippedGear)
        {
            if (!string.IsNullOrEmpty(gear.namedSetId))
            {
                if (!setCount.ContainsKey(gear.namedSetId))
                {
                    setCount[gear.namedSetId] = 0;
                }
                setCount[gear.namedSetId]++;
            }
        }

        // Check which bonuses are active
        foreach (var kvp in setCount)
        {
            NamedGearSetConfig setConfig = GetNamedSet(kvp.Key);
            if (setConfig == null) continue;

            int pieceCount = kvp.Value;

            if (pieceCount >= 2 && setConfig.twoPieceBonus != null)
            {
                activeBonuses.Add(setConfig.twoPieceBonus);
            }
            if (pieceCount >= 4 && setConfig.fourPieceBonus != null)
            {
                activeBonuses.Add(setConfig.fourPieceBonus);
            }
            if (pieceCount >= 6 && setConfig.sixPieceBonus != null)
            {
                activeBonuses.Add(setConfig.sixPieceBonus);
            }
        }

        return activeBonuses;
    }
}
