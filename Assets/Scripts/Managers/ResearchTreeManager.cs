using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FnGMafia.Core;

/// <summary>
/// Manages account-wide Research Tree system
/// </summary>
public class ResearchTreeManager : Singleton<ResearchTreeManager>
{
    private ResearchSaveData researchData;
    private Dictionary<string, ResearchTree> researchTrees;
    private Dictionary<string, ResearchNode> researchNodes;

    protected override void Awake()
    {
        base.Awake();
        researchData = new ResearchSaveData();
        researchTrees = new Dictionary<string, ResearchTree>();
        researchNodes = new Dictionary<string, ResearchNode>();
        LoadResearchTreeConfigs();
    }

    private void LoadResearchTreeConfigs()
    {
        TextAsset configFile = Resources.Load<TextAsset>("Config/research_trees");
        if (configFile != null)
        {
            ResearchTreeDatabase database = JsonUtility.FromJson<ResearchTreeDatabase>(configFile.text);
            foreach (var tree in database.trees)
            {
                researchTrees[tree.treeId] = tree;

                // Index all nodes by ID
                foreach (var node in tree.nodes)
                {
                    researchNodes[node.nodeId] = node;
                }
            }
            Debug.Log($"[ResearchTreeManager] Loaded {researchTrees.Count} research trees with {researchNodes.Count} total nodes");
        }
        else
        {
            Debug.LogWarning("[ResearchTreeManager] research_trees.json not found, creating default");
            CreateDefaultResearchTrees();
        }
    }

    #region Research Operations

    /// <summary>
    /// Unlock research node
    /// </summary>
    public bool UnlockNode(string nodeId)
    {
        if (researchData.unlockedNodes.Contains(nodeId))
        {
            Debug.Log($"[ResearchTreeManager] Node {nodeId} already unlocked");
            return false;
        }

        var node = researchNodes.ContainsKey(nodeId) ? researchNodes[nodeId] : null;
        if (node == null)
        {
            Debug.LogError($"[ResearchTreeManager] Node {nodeId} not found");
            return false;
        }

        // Check prerequisites
        if (!CheckPrerequisites(node))
        {
            Debug.Log($"[ResearchTreeManager] Prerequisites not met for {nodeId}");
            return false;
        }

        // Check costs
        if (!CheckAndDeductCosts(node))
        {
            return false;
        }

        // If time-gated, start research process
        if (node.isTimeGated)
        {
            StartTimedResearch(nodeId, node.researchTimeHours);
            return true;
        }

        // Immediate unlock
        CompleteResearch(nodeId);
        return true;
    }

    /// <summary>
    /// Upgrade existing research node
    /// </summary>
    public bool UpgradeNode(string nodeId)
    {
        if (!researchData.unlockedNodes.Contains(nodeId))
        {
            Debug.Log($"[ResearchTreeManager] Node {nodeId} not unlocked yet");
            return false;
        }

        var node = researchNodes[nodeId];
        if (node.maxUpgradeLevel <= 1)
        {
            Debug.Log($"[ResearchTreeManager] Node {nodeId} cannot be upgraded");
            return false;
        }

        int currentLevel = researchData.nodeUpgradeLevels.ContainsKey(nodeId) ?
            researchData.nodeUpgradeLevels[nodeId] : 1;

        if (currentLevel >= node.maxUpgradeLevel)
        {
            Debug.Log($"[ResearchTreeManager] Node {nodeId} already at max level");
            return false;
        }

        // Check upgrade cost
        if (researchData.totalResearchPoints - researchData.spentResearchPoints < node.upgradePointCost)
        {
            Debug.Log("[ResearchTreeManager] Insufficient research points for upgrade");
            return false;
        }

        // Upgrade
        researchData.spentResearchPoints += node.upgradePointCost;
        researchData.nodeUpgradeLevels[nodeId] = currentLevel + 1;

        Debug.Log($"[ResearchTreeManager] Upgraded {node.nodeName} to level {currentLevel + 1}");
        return true;
    }

    /// <summary>
    /// Check if prerequisites are met
    /// </summary>
    private bool CheckPrerequisites(ResearchNode node)
    {
        // Check required nodes
        if (node.requiredNodes != null && node.requiredNodes.Count > 0)
        {
            foreach (var requiredNodeId in node.requiredNodes)
            {
                if (!researchData.unlockedNodes.Contains(requiredNodeId))
                {
                    Debug.Log($"[ResearchTreeManager] Required node {requiredNodeId} not unlocked");
                    return false;
                }
            }
        }

        // Check account level (would integrate with player level system)
        // For now, assume requirement is met

        return true;
    }

    /// <summary>
    /// Check costs and deduct if met
    /// </summary>
    private bool CheckAndDeductCosts(ResearchNode node)
    {
        // Check research points
        int availablePoints = researchData.totalResearchPoints - researchData.spentResearchPoints;
        if (availablePoints < node.researchPointCost)
        {
            Debug.Log($"[ResearchTreeManager] Insufficient research points. Need {node.researchPointCost}, have {availablePoints}");
            return false;
        }

        // Check gold (integrate with CurrencyManager)
        if (node.goldCost > 0 && CurrencyManager.Instance != null)
        {
            if (!CurrencyManager.Instance.DeductCurrency("gold", node.goldCost))
            {
                return false;
            }
        }

        // Deduct research points
        researchData.spentResearchPoints += node.researchPointCost;
        Debug.Log($"[ResearchTreeManager] Spent {node.researchPointCost} research points");

        return true;
    }

    /// <summary>
    /// Start timed research
    /// </summary>
    private void StartTimedResearch(string nodeId, float durationHours)
    {
        ActiveResearch research = new ActiveResearch
        {
            researchId = Guid.NewGuid().ToString(),
            nodeId = nodeId,
            startTime = DateTime.Now,
            durationHours = durationHours,
            isComplete = false
        };

        researchData.activeResearches.Add(research);
        Debug.Log($"[ResearchTreeManager] Started research for {nodeId}, completes in {durationHours}h");
    }

    /// <summary>
    /// Complete research unlock
    /// </summary>
    private void CompleteResearch(string nodeId)
    {
        researchData.unlockedNodes.Add(nodeId);
        researchData.nodeUpgradeLevels[nodeId] = 1;
        researchData.lastResearchCompleted = DateTime.Now;

        var node = researchNodes[nodeId];
        Debug.Log($"[ResearchTreeManager] Unlocked research: {node.nodeName}");

        // Apply research effects
        ApplyResearchEffects(node);
    }

    /// <summary>
    /// Apply research effects to game
    /// </summary>
    private void ApplyResearchEffects(ResearchNode node)
    {
        foreach (var effect in node.effects)
        {
            Debug.Log($"[ResearchTreeManager] Applied effect: {effect.effectType} - {effect.statType} {effect.percentBonus}%");

            // In real implementation, would integrate with game systems
            // For example:
            // - Register stat modifiers with combat system
            // - Update resource multipliers
            // - Unlock features in respective managers
        }
    }

    /// <summary>
    /// Check for completed timed researches
    /// </summary>
    public void UpdateTimedResearches()
    {
        var now = DateTime.Now;
        var completedResearches = researchData.activeResearches
            .Where(r => !r.isComplete && (now - r.startTime).TotalHours >= r.durationHours)
            .ToList();

        foreach (var research in completedResearches)
        {
            research.isComplete = true;
            CompleteResearch(research.nodeId);
            Debug.Log($"[ResearchTreeManager] Research completed: {research.nodeId}");
        }
    }

    /// <summary>
    /// Claim completed timed research
    /// </summary>
    public bool ClaimCompletedResearch(string researchId)
    {
        var research = researchData.activeResearches.Find(r => r.researchId == researchId);
        if (research == null || !research.isComplete) return false;

        researchData.activeResearches.Remove(research);
        Debug.Log($"[ResearchTreeManager] Claimed completed research");
        return true;
    }

    #endregion

    #region Research Points

    /// <summary>
    /// Add research points
    /// </summary>
    public void AddResearchPoints(int amount)
    {
        researchData.totalResearchPoints += amount;
        Debug.Log($"[ResearchTreeManager] +{amount} research points (Total: {researchData.totalResearchPoints})");
    }

    /// <summary>
    /// Get available research points
    /// </summary>
    public int GetAvailablePoints()
    {
        return researchData.totalResearchPoints - researchData.spentResearchPoints;
    }

    #endregion

    #region Getters

    /// <summary>
    /// Get all research trees
    /// </summary>
    public List<ResearchTree> GetAllTrees()
    {
        return new List<ResearchTree>(researchTrees.Values);
    }

    /// <summary>
    /// Get research tree by ID
    /// </summary>
    public ResearchTree GetTree(string treeId)
    {
        return researchTrees.ContainsKey(treeId) ? researchTrees[treeId] : null;
    }

    /// <summary>
    /// Check if node is unlocked
    /// </summary>
    public bool IsNodeUnlocked(string nodeId)
    {
        return researchData.unlockedNodes.Contains(nodeId);
    }

    /// <summary>
    /// Get node level
    /// </summary>
    public int GetNodeLevel(string nodeId)
    {
        if (!IsNodeUnlocked(nodeId)) return 0;
        return researchData.nodeUpgradeLevels.ContainsKey(nodeId) ? researchData.nodeUpgradeLevels[nodeId] : 1;
    }

    /// <summary>
    /// Get active researches
    /// </summary>
    public List<ActiveResearch> GetActiveResearches()
    {
        return new List<ActiveResearch>(researchData.activeResearches);
    }

    /// <summary>
    /// Calculate total stat bonus from research
    /// </summary>
    public float GetStatBonus(string statType)
    {
        float totalBonus = 0f;

        foreach (var nodeId in researchData.unlockedNodes)
        {
            var node = researchNodes[nodeId];
            int nodeLevel = GetNodeLevel(nodeId);

            foreach (var effect in node.effects)
            {
                if (effect.statType == statType)
                {
                    totalBonus += effect.percentBonus + (effect.bonusPerLevel * (nodeLevel - 1));
                }
            }
        }

        return totalBonus;
    }

    #endregion

    #region Save/Load

    public ResearchSaveData GetSaveData()
    {
        return researchData;
    }

    public void LoadSaveData(ResearchSaveData data)
    {
        researchData = data;
        Debug.Log($"[ResearchTreeManager] Loaded {researchData.unlockedNodes.Count} unlocked research nodes");

        // Reapply all research effects
        foreach (var nodeId in researchData.unlockedNodes)
        {
            if (researchNodes.ContainsKey(nodeId))
            {
                ApplyResearchEffects(researchNodes[nodeId]);
            }
        }
    }

    #endregion

    #region Default Data

    private void CreateDefaultResearchTrees()
    {
        // Create a sample combat mastery tree
        ResearchTree combatTree = new ResearchTree
        {
            treeId = "combat_mastery",
            treeName = "Combat Mastery",
            description = "Permanent combat bonuses for all heroes",
            category = "combat",
            nodes = new List<ResearchNode>
            {
                new ResearchNode
                {
                    nodeId = "combat_hp_1",
                    nodeName = "Vitality I",
                    description = "+2% HP to all heroes",
                    nodeType = ResearchNodeType.StatBoost,
                    tier = 1,
                    researchPointCost = 1,
                    goldCost = 1000,
                    maxUpgradeLevel = 5,
                    upgradePointCost = 1,
                    effects = new List<ResearchEffect>
                    {
                        new ResearchEffect
                        {
                            effectType = "stat_boost",
                            targetType = "all_heroes",
                            statType = "hp",
                            percentBonus = 2f,
                            bonusPerLevel = 1f
                        }
                    }
                },
                new ResearchNode
                {
                    nodeId = "combat_atk_1",
                    nodeName = "Strength I",
                    description = "+2% ATK to all heroes",
                    nodeType = ResearchNodeType.StatBoost,
                    tier = 1,
                    researchPointCost = 1,
                    goldCost = 1000,
                    maxUpgradeLevel = 5,
                    upgradePointCost = 1,
                    effects = new List<ResearchEffect>
                    {
                        new ResearchEffect
                        {
                            effectType = "stat_boost",
                            targetType = "all_heroes",
                            statType = "atk",
                            percentBonus = 2f,
                            bonusPerLevel = 1f
                        }
                    }
                }
            }
        };

        researchTrees[combatTree.treeId] = combatTree;
        foreach (var node in combatTree.nodes)
        {
            researchNodes[node.nodeId] = node;
        }

        Debug.Log("[ResearchTreeManager] Created default research trees");
    }

    #endregion
}
