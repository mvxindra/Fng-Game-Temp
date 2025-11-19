using System;
using System.Collections.Generic;

[Serializable]
public class HeroAscensionProgress
{
    public string heroId;
    public int currentTier;                       // Current ascension tier (0 = not ascended)
    public int currentLevel;                      // Current hero level
    public int levelCap;                          // Current level cap

    // Unlocked content
    public List<string> unlockedSkills;           // Skills unlocked through ascension
    public List<string> unlockedPassives;         // Passives unlocked through ascension
    public List<string> evolvedSkills;            // Skills that have evolved

    // Accumulated bonuses
    public AscensionStatBonus totalStatBonuses;

    // Special features
    public List<string> activeFeatures;           // Special features currently active
    public string secondaryElement;               // If dual element unlocked

    public HeroAscensionProgress()
    {
        currentTier = 0;
        currentLevel = 1;
        levelCap = 50; // Default cap
        unlockedSkills = new List<string>();
        unlockedPassives = new List<string>();
        evolvedSkills = new List<string>();
        totalStatBonuses = new AscensionStatBonus();
        activeFeatures = new List<string>();
    }

    public bool CanAscend(int requiredLevel)
    {
        return currentLevel >= requiredLevel;
    }

    public bool IsAtLevelCap()
    {
        return currentLevel >= levelCap;
    }
}
