using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FnGMafia.Core;

/// <summary>
/// Manages artifact collection, equipment, and upgrades
/// </summary>
public class ArtifactManager : Singleton<ArtifactManager>
{
    private ArtifactSaveData artifactData;
    private Dictionary<string, ArtifactConfig> artifactConfigs;
    private Dictionary<string, ArtifactSet> artifactSets;

    // Equipment limits
    private const int MAX_ARTIFACTS_PER_HERO = 2;

    protected override void Awake()
    {
        base.Awake();
        artifactData = new ArtifactSaveData();
        artifactConfigs = new Dictionary<string, ArtifactConfig>();
        artifactSets = new Dictionary<string, ArtifactSet>();
        LoadArtifactDatabase();
    }

    private void LoadArtifactDatabase()
    {
        TextAsset configFile = Resources.Load<TextAsset>("Config/artifacts");
        if (configFile != null)
        {
            ArtifactDatabase database = JsonUtility.FromJson<ArtifactDatabase>(configFile.text);

            foreach (var artifact in database.artifacts)
            {
                artifactConfigs[artifact.artifactId] = artifact;
            }

            foreach (var set in database.artifactSets)
            {
                artifactSets[set.setId] = set;
            }

            Debug.Log($"[ArtifactManager] Loaded {artifactConfigs.Count} artifacts and {artifactSets.Count} sets");
        }
        else
        {
            Debug.LogWarning("[ArtifactManager] artifacts.json not found, creating defaults");
            CreateDefaultArtifacts();
        }
    }

    #region Artifact Acquisition

    /// <summary>
    /// Award artifact to player
    /// </summary>
    public string AwardArtifact(string artifactId)
    {
        if (!artifactConfigs.ContainsKey(artifactId))
        {
            Debug.LogError($"[ArtifactManager] Artifact {artifactId} not found");
            return null;
        }

        ArtifactInstance instance = new ArtifactInstance
        {
            instanceId = Guid.NewGuid().ToString(),
            artifactId = artifactId,
            upgradeLevel = 0,
            enhancementLevel = 0,
            obtainedDate = DateTime.Now,
            isLocked = false,
            isFavorite = false,
            randomStats = RollRandomStats(artifactId)
        };

        artifactData.ownedArtifacts.Add(instance);

        var config = artifactConfigs[artifactId];
        Debug.Log($"[ArtifactManager] Obtained artifact: {config.artifactName}");

        // Update collection progress if part of a set
        if (!string.IsNullOrEmpty(config.setId))
        {
            UpdateSetProgress(config.setId);
        }

        return instance.instanceId;
    }

    /// <summary>
    /// Roll random substats for artifact (if applicable)
    /// </summary>
    private Dictionary<string, float> RollRandomStats(string artifactId)
    {
        // In a real implementation, would roll random substats based on rarity
        // For now, return empty
        return new Dictionary<string, float>();
    }

    /// <summary>
    /// Update artifact set collection progress
    /// </summary>
    private void UpdateSetProgress(string setId)
    {
        int count = artifactData.ownedArtifacts.Count(a =>
        {
            var config = artifactConfigs[a.artifactId];
            return config.setId == setId;
        });

        artifactData.artifactCollectionProgress[setId] = count;
    }

    #endregion

    #region Equipment

    /// <summary>
    /// Equip artifact to hero
    /// </summary>
    public bool EquipArtifact(string heroId, string instanceId)
    {
        var instance = artifactData.ownedArtifacts.Find(a => a.instanceId == instanceId);
        if (instance == null)
        {
            Debug.LogError($"[ArtifactManager] Artifact instance {instanceId} not found");
            return false;
        }

        // Initialize hero's artifact list if needed
        if (!artifactData.heroEquippedArtifacts.ContainsKey(heroId))
        {
            artifactData.heroEquippedArtifacts[heroId] = new List<string>();
        }

        var equippedArtifacts = artifactData.heroEquippedArtifacts[heroId];

        // Check if already equipped
        if (equippedArtifacts.Contains(instanceId))
        {
            Debug.Log($"[ArtifactManager] Artifact already equipped");
            return false;
        }

        // Check artifact limit
        if (equippedArtifacts.Count >= MAX_ARTIFACTS_PER_HERO)
        {
            Debug.Log($"[ArtifactManager] Maximum artifacts equipped ({MAX_ARTIFACTS_PER_HERO})");
            return false;
        }

        // Check if artifact is equipped on another hero
        foreach (var kvp in artifactData.heroEquippedArtifacts)
        {
            if (kvp.Value.Contains(instanceId) && kvp.Key != heroId)
            {
                // Auto-unequip from other hero
                kvp.Value.Remove(instanceId);
                Debug.Log($"[ArtifactManager] Auto-unequipped from another hero");
            }
        }

        // Equip
        equippedArtifacts.Add(instanceId);
        var config = artifactConfigs[instance.artifactId];
        Debug.Log($"[ArtifactManager] Equipped {config.artifactName} to hero {heroId}");

        return true;
    }

    /// <summary>
    /// Unequip artifact from hero
    /// </summary>
    public bool UnequipArtifact(string heroId, string instanceId)
    {
        if (!artifactData.heroEquippedArtifacts.ContainsKey(heroId))
            return false;

        var equippedArtifacts = artifactData.heroEquippedArtifacts[heroId];
        bool removed = equippedArtifacts.Remove(instanceId);

        if (removed)
        {
            Debug.Log($"[ArtifactManager] Unequipped artifact from hero {heroId}");
        }

        return removed;
    }

    /// <summary>
    /// Get hero's equipped artifacts
    /// </summary>
    public List<ArtifactInstance> GetEquippedArtifacts(string heroId)
    {
        if (!artifactData.heroEquippedArtifacts.ContainsKey(heroId))
            return new List<ArtifactInstance>();

        var instanceIds = artifactData.heroEquippedArtifacts[heroId];
        return artifactData.ownedArtifacts.Where(a => instanceIds.Contains(a.instanceId)).ToList();
    }

    #endregion

    #region Upgrade/Enhancement

    /// <summary>
    /// Upgrade artifact level
    /// </summary>
    public bool UpgradeArtifact(string instanceId)
    {
        var instance = artifactData.ownedArtifacts.Find(a => a.instanceId == instanceId);
        if (instance == null) return false;

        var config = artifactConfigs[instance.artifactId];

        if (instance.upgradeLevel >= config.maxUpgradeLevel)
        {
            Debug.Log("[ArtifactManager] Artifact already at max level");
            return false;
        }

        // In real implementation, check and deduct upgrade costs
        // For now, just upgrade
        instance.upgradeLevel++;
        Debug.Log($"[ArtifactManager] Upgraded {config.artifactName} to level {instance.upgradeLevel}");

        return true;
    }

    /// <summary>
    /// Enhance artifact (strengthen effects)
    /// </summary>
    public bool EnhanceArtifact(string instanceId)
    {
        var instance = artifactData.ownedArtifacts.Find(a => a.instanceId == instanceId);
        if (instance == null) return false;

        // Enhancement system (similar to gear enhancement)
        instance.enhancementLevel++;
        Debug.Log($"[ArtifactManager] Enhanced artifact to +{instance.enhancementLevel}");

        return true;
    }

    #endregion

    #region Set Bonuses

    /// <summary>
    /// Get active set bonuses for hero
    /// </summary>
    public List<ArtifactEffect> GetActiveSetBonuses(string heroId)
    {
        var equippedArtifacts = GetEquippedArtifacts(heroId);
        var activeBonuses = new List<ArtifactEffect>();

        // Count artifacts per set
        var setCount = new Dictionary<string, int>();
        foreach (var artifact in equippedArtifacts)
        {
            var config = artifactConfigs[artifact.artifactId];
            if (!string.IsNullOrEmpty(config.setId))
            {
                if (!setCount.ContainsKey(config.setId))
                    setCount[config.setId] = 0;
                setCount[config.setId]++;
            }
        }

        // Check which set bonuses are active
        foreach (var kvp in setCount)
        {
            if (artifactSets.ContainsKey(kvp.Key))
            {
                var set = artifactSets[kvp.Key];
                int piecesEquipped = kvp.Value;

                // Add bonuses for each threshold met
                foreach (var bonusEntry in set.setBonuses)
                {
                    if (piecesEquipped >= bonusEntry.Key)
                    {
                        activeBonuses.AddRange(bonusEntry.Value);
                    }
                }
            }
        }

        return activeBonuses;
    }

    #endregion

    #region Management

    /// <summary>
    /// Lock/unlock artifact
    /// </summary>
    public bool ToggleLock(string instanceId)
    {
        var instance = artifactData.ownedArtifacts.Find(a => a.instanceId == instanceId);
        if (instance == null) return false;

        instance.isLocked = !instance.isLocked;
        return true;
    }

    /// <summary>
    /// Favorite/unfavorite artifact
    /// </summary>
    public bool ToggleFavorite(string instanceId)
    {
        var instance = artifactData.ownedArtifacts.Find(a => a.instanceId == instanceId);
        if (instance == null) return false;

        instance.isFavorite = !instance.isFavorite;
        return true;
    }

    /// <summary>
    /// Dismantle artifact for materials
    /// </summary>
    public bool DismantleArtifact(string instanceId)
    {
        var instance = artifactData.ownedArtifacts.Find(a => a.instanceId == instanceId);
        if (instance == null) return false;

        if (instance.isLocked)
        {
            Debug.Log("[ArtifactManager] Cannot dismantle locked artifact");
            return false;
        }

        // Check if equipped
        foreach (var kvp in artifactData.heroEquippedArtifacts)
        {
            if (kvp.Value.Contains(instanceId))
            {
                Debug.Log("[ArtifactManager] Cannot dismantle equipped artifact");
                return false;
            }
        }

        artifactData.ownedArtifacts.Remove(instance);
        Debug.Log("[ArtifactManager] Artifact dismantled");

        // In real implementation, award dismantle materials
        return true;
    }

    #endregion

    #region Getters

    /// <summary>
    /// Get all owned artifacts
    /// </summary>
    public List<ArtifactInstance> GetOwnedArtifacts()
    {
        return new List<ArtifactInstance>(artifactData.ownedArtifacts);
    }

    /// <summary>
    /// Get artifact config
    /// </summary>
    public ArtifactConfig GetArtifactConfig(string artifactId)
    {
        return artifactConfigs.ContainsKey(artifactId) ? artifactConfigs[artifactId] : null;
    }

    /// <summary>
    /// Get artifact instance
    /// </summary>
    public ArtifactInstance GetArtifactInstance(string instanceId)
    {
        return artifactData.ownedArtifacts.Find(a => a.instanceId == instanceId);
    }

    #endregion

    #region Save/Load

    public ArtifactSaveData GetSaveData()
    {
        return artifactData;
    }

    public void LoadSaveData(ArtifactSaveData data)
    {
        artifactData = data;
        Debug.Log($"[ArtifactManager] Loaded {artifactData.ownedArtifacts.Count} artifacts");
    }

    #endregion

    #region Default Data

    private void CreateDefaultArtifacts()
    {
        ArtifactConfig phoenixFeather = new ArtifactConfig
        {
            artifactId = "phoenix_feather",
            artifactName = "Phoenix Feather",
            description = "Grants resurrection",
            artifactType = ArtifactType.Accessory,
            rarity = "mythic",
            maxUpgradeLevel = 10,
            baseEffects = new List<ArtifactEffect>
            {
                new ArtifactEffect
                {
                    effectId = "revive_on_death",
                    effectName = "Phoenix Rebirth",
                    description = "First time this hero dies, revive with 30% HP",
                    trigger = EffectTrigger.OnDeath,
                    target = EffectTarget.Self,
                    effectType = "revive"
                }
            }
        };

        artifactConfigs[phoenixFeather.artifactId] = phoenixFeather;

        Debug.Log("[ArtifactManager] Created default artifacts");
    }

    #endregion
}
