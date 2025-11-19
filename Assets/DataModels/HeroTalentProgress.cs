using System;
using System.Collections.Generic;

[Serializable]
public class HeroTalentProgress
{
    public string heroId;                              // Hero this progress belongs to
    public int totalTalentPoints;                      // Total points available
    public int spentTalentPoints;                      // Points spent
    public Dictionary<string, int> nodeRanks;          // nodeId -> current rank
    public List<string> unlockedNodes;                 // List of unlocked node IDs
    public List<string> unlockedPassives;              // Passive abilities unlocked
    public List<string> unlockedSkills;                // Skills unlocked via talents

    public HeroTalentProgress()
    {
        nodeRanks = new Dictionary<string, int>();
        unlockedNodes = new List<string>();
        unlockedPassives = new List<string>();
        unlockedSkills = new List<string>();
    }

    // Helper methods
    public int GetNodeRank(string nodeId)
    {
        return nodeRanks.ContainsKey(nodeId) ? nodeRanks[nodeId] : 0;
    }

    public bool IsNodeUnlocked(string nodeId)
    {
        return unlockedNodes.Contains(nodeId);
    }

    public int GetAvailablePoints()
    {
        return totalTalentPoints - spentTalentPoints;
    }

    public void AddTalentPoints(int points)
    {
        totalTalentPoints += points;
    }
}
