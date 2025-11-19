using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TalentTreeManager : Singleton<TalentTreeManager>
{
    private Dictionary<string, HeroTalentProgress> heroTalentProgress;
    private Dictionary<string, TalentTreeConfig> talentTrees;
    private Dictionary<string, TalentNodeConfig> talentNodes;

    protected override void Awake()
    {
        base.Awake();
        heroTalentProgress = new Dictionary<string, HeroTalentProgress>();
        talentTrees = new Dictionary<string, TalentTreeConfig>();
        talentNodes = new Dictionary<string, TalentNodeConfig>();
        LoadTalentTreeConfigs();
    }

    private void LoadTalentTreeConfigs()
    {
        // Load from Resources/Config/talent_trees.json
        TextAsset configFile = Resources.Load<TextAsset>("Config/talent_trees");
        if (configFile != null)
        {
            TalentTreeDatabase database = JsonUtility.FromJson<TalentTreeDatabase>(configFile.text);
            foreach (var tree in database.trees)
            {
                talentTrees[tree.treeId] = tree;

                // Index all nodes by ID
                foreach (var node in tree.nodes)
                {
                    talentNodes[node.nodeId] = node;
                }
            }
            Debug.Log($"Loaded {talentTrees.Count} talent trees with {talentNodes.Count} total nodes");
        }
        else
        {
            Debug.LogWarning("talent_trees.json not found in Resources/Config");
        }
    }

    // Initialize talent progress for a hero
    public void InitializeHeroTalents(string heroId, int heroLevel, int ascensionLevel)
    {
        if (!heroTalentProgress.ContainsKey(heroId))
        {
            HeroTalentProgress progress = new HeroTalentProgress();
            progress.heroId = heroId;

            // Calculate talent points based on level and ascension
            TalentTreeConfig tree = GetTalentTreeForHero(heroId);
            if (tree != null)
            {
                progress.totalTalentPoints = (heroLevel * tree.talentPointsPerLevel) +
                                            (ascensionLevel * tree.talentPointsPerAscension);
            }

            heroTalentProgress[heroId] = progress;
        }
    }

    // Grant talent points when hero levels up or ascends
    public void GrantTalentPoints(string heroId, int points)
    {
        if (!heroTalentProgress.ContainsKey(heroId))
        {
            InitializeHeroTalents(heroId, 1, 0);
        }

        heroTalentProgress[heroId].AddTalentPoints(points);
        Debug.Log($"Granted {points} talent points to {heroId}. Total: {heroTalentProgress[heroId].totalTalentPoints}");
    }

    // Unlock/upgrade a talent node
    public bool UnlockTalentNode(string heroId, string nodeId)
    {
        if (!heroTalentProgress.ContainsKey(heroId))
        {
            Debug.LogError($"No talent progress found for hero {heroId}");
            return false;
        }

        HeroTalentProgress progress = heroTalentProgress[heroId];
        TalentNodeConfig node = GetTalentNode(nodeId);

        if (node == null)
        {
            Debug.LogError($"Talent node {nodeId} not found");
            return false;
        }

        // Check if we can unlock/upgrade this node
        if (!CanUnlockNode(heroId, nodeId))
        {
            Debug.LogWarning($"Cannot unlock node {nodeId} for hero {heroId}");
            return false;
        }

        int currentRank = progress.GetNodeRank(nodeId);

        // Check if we have available points
        if (progress.GetAvailablePoints() <= 0)
        {
            Debug.LogWarning($"No available talent points for hero {heroId}");
            return false;
        }

        // Unlock the node
        if (currentRank == 0)
        {
            progress.unlockedNodes.Add(nodeId);
        }

        // Increase rank
        int newRank = currentRank + 1;
        progress.nodeRanks[nodeId] = newRank;
        progress.spentTalentPoints++;

        // Apply effects if this is a skill unlock or passive unlock
        ApplyNodeEffects(heroId, node, newRank);

        Debug.Log($"Unlocked/upgraded node {nodeId} to rank {newRank} for hero {heroId}");
        return true;
    }

    // Check if a node can be unlocked
    private bool CanUnlockNode(string heroId, string nodeId)
    {
        HeroTalentProgress progress = heroTalentProgress[heroId];
        TalentNodeConfig node = GetTalentNode(nodeId);

        if (node == null) return false;

        // Check if already at max rank
        int currentRank = progress.GetNodeRank(nodeId);
        if (currentRank >= node.maxRanks) return false;

        // Check talent point requirement
        if (progress.spentTalentPoints < node.requiredTalentPoints) return false;

        // Check ascension requirement
        // Note: Need to get hero's ascension level from HeroConfig
        // For now, we'll skip this check or you can integrate with your hero system

        // Check prerequisites
        if (node.prerequisiteNodes != null && node.prerequisiteNodes.Count > 0)
        {
            foreach (string prereqId in node.prerequisiteNodes)
            {
                if (!progress.IsNodeUnlocked(prereqId))
                {
                    return false;
                }
            }
        }

        return true;
    }

    // Apply the effects of a talent node
    private void ApplyNodeEffects(string heroId, TalentNodeConfig node, int rank)
    {
        HeroTalentProgress progress = heroTalentProgress[heroId];
        TalentEffectConfig effect = node.effect;

        if (effect == null) return;

        switch (effect.effectType)
        {
            case "skill_unlock":
                if (!progress.unlockedSkills.Contains(effect.skillId))
                {
                    progress.unlockedSkills.Add(effect.skillId);
                    Debug.Log($"Unlocked skill {effect.skillId} for hero {heroId}");
                }
                break;

            case "passive_ability":
                if (!progress.unlockedPassives.Contains(effect.passiveId))
                {
                    progress.unlockedPassives.Add(effect.passiveId);
                    Debug.Log($"Unlocked passive {effect.passiveId} for hero {heroId}");
                }
                break;

            case "stat_boost":
            case "skill_modification":
                // These are calculated dynamically when stats are computed
                break;
        }
    }

    // Calculate total stat bonuses from talents
    public Dictionary<string, float> GetTalentStatBonuses(string heroId)
    {
        Dictionary<string, float> bonuses = new Dictionary<string, float>();

        if (!heroTalentProgress.ContainsKey(heroId))
        {
            return bonuses;
        }

        HeroTalentProgress progress = heroTalentProgress[heroId];

        foreach (var nodeEntry in progress.nodeRanks)
        {
            string nodeId = nodeEntry.Key;
            int rank = nodeEntry.Value;
            TalentNodeConfig node = GetTalentNode(nodeId);

            if (node?.effect?.effectType == "stat_boost")
            {
                string statType = node.effect.statType;
                float value = node.effect.valuePerRank * rank;

                if (!bonuses.ContainsKey(statType))
                {
                    bonuses[statType] = 0;
                }

                bonuses[statType] += value;
            }
        }

        return bonuses;
    }

    // Get unlocked skills from talents
    public List<string> GetUnlockedTalentSkills(string heroId)
    {
        if (!heroTalentProgress.ContainsKey(heroId))
        {
            return new List<string>();
        }

        return heroTalentProgress[heroId].unlockedSkills;
    }

    // Get unlocked passives from talents
    public List<PassiveAbilityConfig> GetUnlockedTalentPassives(string heroId)
    {
        List<PassiveAbilityConfig> passives = new List<PassiveAbilityConfig>();

        if (!heroTalentProgress.ContainsKey(heroId))
        {
            return passives;
        }

        HeroTalentProgress progress = heroTalentProgress[heroId];

        foreach (string passiveId in progress.unlockedPassives)
        {
            // Find the node that grants this passive
            foreach (var nodeEntry in progress.nodeRanks)
            {
                TalentNodeConfig node = GetTalentNode(nodeEntry.Key);
                if (node?.effect?.passiveId == passiveId)
                {
                    passives.Add(node.effect.passiveAbility);
                    break;
                }
            }
        }

        return passives;
    }

    // Reset talents (with material cost in real implementation)
    public void ResetTalents(string heroId)
    {
        if (!heroTalentProgress.ContainsKey(heroId))
        {
            return;
        }

        HeroTalentProgress progress = heroTalentProgress[heroId];
        int refundedPoints = progress.spentTalentPoints;

        progress.nodeRanks.Clear();
        progress.unlockedNodes.Clear();
        progress.unlockedPassives.Clear();
        progress.unlockedSkills.Clear();
        progress.spentTalentPoints = 0;

        Debug.Log($"Reset talents for {heroId}, refunded {refundedPoints} points");
    }

    // Utility methods
    public TalentTreeConfig GetTalentTreeForHero(string heroId)
    {
        return talentTrees.Values.FirstOrDefault(tree => tree.heroId == heroId);
    }

    public TalentNodeConfig GetTalentNode(string nodeId)
    {
        return talentNodes.ContainsKey(nodeId) ? talentNodes[nodeId] : null;
    }

    public HeroTalentProgress GetHeroTalentProgress(string heroId)
    {
        return heroTalentProgress.ContainsKey(heroId) ? heroTalentProgress[heroId] : null;
    }
}

[System.Serializable]
public class TalentTreeDatabase
{
    public List<TalentTreeConfig> trees;
}
