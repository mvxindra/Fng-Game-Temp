using System;
using System.Collections.Generic;

/// <summary>
/// Achievement configuration
/// </summary>
[Serializable]
public class AchievementConfig
{
    public string achievementId;
    public string achievementName;
    public string description;
    public string category;                     // "progression", "combat", "collection", "social", "mastery"
    public string icon;
    public int tierLevel;                       // 1-5 (bronze, silver, gold, platinum, diamond)

    // Completion criteria
    public AchievementCriteria criteria;

    // Rewards
    public List<AchievementReward> rewards;
    public string unlockedTitle;                // Title unlocked (e.g., "The Collector")
    public string unlockedPortrait;             // Portrait frame ID
    public int achievementPoints;               // Points toward achievement score

    // Display
    public bool isHidden;                       // Hidden until unlocked
    public bool isSecret;                       // Secret achievement
    public string prerequisiteAchievement;      // Must complete this first
}

/// <summary>
/// Achievement criteria
/// </summary>
[Serializable]
public class AchievementCriteria
{
    public string criteriaType;                 // See below for types
    public int targetValue;                     // Target number/amount
    public string specificId;                   // Specific item/hero/stage ID if needed
    public int minRarity;                       // Minimum rarity requirement
    public string condition;                    // Additional condition (e.g., "without_death")
}

// Criteria types:
// - "hero_6star": Get a 6★ hero
// - "gear_15": Get gear to +15
// - "story_clear": Clear story stage
// - "total_power": Reach total team power
// - "hero_level": Get hero to level X
// - "collection_count": Own X heroes
// - "arena_rank": Reach arena rank
// - "guild_level": Guild reaches level X
// - "summon_count": Summon X heroes total
// - "enhancement_count": Enhance gear X times
// - "perfect_clear": Clear without deaths
// - "speed_clear": Clear in X seconds
// - "gold_earned": Earn X gold total
// - "consecutive_wins": Win X battles in a row

/// <summary>
/// Achievement reward
/// </summary>
[Serializable]
public class AchievementReward
{
    public string rewardType;
    public string rewardId;
    public int quantity;
}

/// <summary>
/// Player achievement progress
/// </summary>
[Serializable]
public class PlayerAchievementProgress
{
    public string achievementId;
    public int currentValue;
    public bool isCompleted;
    public bool isClaimed;
    public DateTime completionDate;
}

/// <summary>
/// Title configuration
/// </summary>
[Serializable]
public class TitleConfig
{
    public string titleId;
    public string titleText;
    public string description;
    public string rarity;                       // "common", "rare", "epic", "legendary"
    public TitleEffect titleEffect;             // Optional stat bonus
    public string unlockedBy;                   // Achievement ID that unlocks it
}

/// <summary>
/// Title effect (stat bonus when equipped)
/// </summary>
[Serializable]
public class TitleEffect
{
    public float bonusAtk;
    public float bonusDef;
    public float bonusHp;
    public float bonusAllStats;
    public float bonusExpGain;
    public float bonusGoldGain;
}

/// <summary>
/// Portrait frame configuration
/// </summary>
[Serializable]
public class PortraitFrameConfig
{
    public string frameId;
    public string frameName;
    public string description;
    public string rarity;                       // "common", "rare", "epic", "legendary"
    public string animationType;                // "static", "glow", "particle", "animated"
    public string unlockedBy;                   // Achievement ID
}

/// <summary>
/// Achievement database
/// </summary>
[Serializable]
public class AchievementDatabase
{
    public List<AchievementConfig> achievements = new List<AchievementConfig>();
    public List<TitleConfig> titles = new List<TitleConfig>();
    public List<PortraitFrameConfig> portraitFrames = new List<PortraitFrameConfig>();
    public AchievementRewardTiers rewardTiers = new AchievementRewardTiers();
}

/// <summary>
/// Achievement milestone rewards
/// </summary>
[Serializable]
public class AchievementRewardTiers
{
    public List<AchievementMilestone> milestones = new List<AchievementMilestone>();
}

/// <summary>
/// Achievement milestone (total points)
/// </summary>
[Serializable]
public class AchievementMilestone
{
    public int requiredPoints;
    public List<AchievementReward> rewards;
    public string unlockedTitle;
    public string unlockedPortrait;
}
