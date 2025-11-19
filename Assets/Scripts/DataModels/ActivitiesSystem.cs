using System;

using System.Collections.Generic;

 

/// <summary>

/// Weekly and monthly activities system

/// </summary>

[Serializable]

public class ActivitySchedule

{

    public List<DailyContract> dailyContracts;

    public List<WeeklyContract> weeklyContracts;

    public List<MonthlyContract> monthlyContracts;

    public List<Bounty> activeBounties;

    public List<LimitedEvent> activeEvents;

    public List<TimeLimitedDungeon> timeLimitedDungeons;

 

    public ActivitySchedule()

    {

        dailyContracts = new List<DailyContract>();

        weeklyContracts = new List<WeeklyContract>();

        monthlyContracts = new List<MonthlyContract>();

        activeBounties = new List<Bounty>();

        activeEvents = new List<LimitedEvent>();

        timeLimitedDungeons = new List<TimeLimitedDungeon>();

    }

}

 

/// <summary>

/// Daily contract configuration

/// </summary>

[Serializable]

public class DailyContract

{

    public string contractId;

    public string contractName;

    public string description;

    public string contractType;                 // "combat", "collection", "upgrade", "social"

 

    // Objectives

    public List<ContractObjective> objectives;

 

    // Rewards

    public List<ContractReward> rewards;

    public int contractPoints;                  // Points toward daily total

 

    // Status

    public bool isComplete;

    public bool isClaimed;

    public DateTime expiryTime;

}

 

/// <summary>

/// Weekly contract (same structure but lasts week)

/// </summary>

[Serializable]

public class WeeklyContract : DailyContract

{

    public int weekNumber;                      // Week of year

}

 

/// <summary>

/// Monthly contract (same structure but lasts month)

/// </summary>

[Serializable]

public class MonthlyContract : DailyContract

{

    public int monthNumber;                     // Month of year

}

 

/// <summary>

/// Contract objective

/// </summary>

[Serializable]

public class ContractObjective

{

    public string objectiveId;

    public string objectiveType;                // "defeat_enemies", "complete_stages", "collect_resources", "summon_heroes"

    public string description;

    public int targetCount;

    public int currentCount;

 

    // Specific parameters

    public string targetId;                     // Specific enemy, stage, resource, etc.

    public string targetCategory;               // Category instead of specific ID

    public int minRarity;                       // For summon objectives

}

 

/// <summary>

/// Contract reward

/// </summary>

[Serializable]

public class ContractReward

{

    public string rewardType;

    public string rewardId;

    public int quantity;

}

 

/// <summary>

/// Bounty system

/// </summary>

[Serializable]

public class Bounty

{

    public string bountyId;

    public string bountyName;

    public string description;

    public string difficulty;                   // "easy", "medium", "hard", "extreme"

 

    // Target

    public string targetType;                   // "boss", "elite", "specific_enemy"

    public string targetId;

    public int targetLevel;

 

    // Requirements

    public BountyRequirements requirements;

 

    // Rewards

    public List<BountyReward> rewards;

    public int bountyReputation;                // Reputation points

 

    // Time limit

    public DateTime availableUntil;

    public int attemptsAllowed;

    public int attemptsUsed;

 

    // Status

    public bool isComplete;

    public bool isClaimed;

}

 

/// <summary>

/// Bounty requirements

/// </summary>

[Serializable]

public class BountyRequirements

{

    public int minLevel;

    public int minReputation;                   // Bounty hunter reputation

    public string requiredQuest;                // Quest that unlocks this

    public int energyCost;

}

 

/// <summary>

/// Bounty reward

/// </summary>

[Serializable]

public class BountyReward

{

    public string rewardType;

    public string rewardId;

    public int quantity;

    public float dropChance;                    // Some rewards are RNG

    public bool isGuaranteed;                   // Guaranteed reward

}

 

/// <summary>

/// Limited-time event

/// </summary>

[Serializable]

public class LimitedEvent

{

    public string eventId;

    public string eventName;

    public string description;

    public string eventType;                    // "raid", "dungeon", "collection", "pvp", "celebration"

    public string theme;                        // Event theme (holiday, seasonal, etc.)

 

    // Duration

    public DateTime startTime;

    public DateTime endTime;

    public bool isActive;

 

    // Event mechanics

    public List<EventStage> stages;

    public EventCurrency eventCurrency;

    public EventShop eventShop;

    public EventMilestones milestones;

 

    // Leaderboard

    public bool hasLeaderboard;

    public List<EventLeaderboardReward> leaderboardRewards;

}

 

/// <summary>

/// Event stage

/// </summary>

[Serializable]

public class EventStage

{

    public string stageId;

    public string stageName;

    public int difficulty;

    public List<string> enemies;

    public string bossId;

    public int energyCost;

    public List<EventReward> rewards;

    public List<EventReward> firstClearRewards;

}

 

/// <summary>

/// Event currency

/// </summary>

[Serializable]

public class EventCurrency

{

    public string currencyId;

    public string currencyName;

    public string icon;

    public int playerAmount;                    // How much player has

}

 

/// <summary>

/// Event shop

/// </summary>

[Serializable]

public class EventShop

{

    public List<EventShopItem> items;

}

 

/// <summary>

/// Event shop item

/// </summary>

[Serializable]

public class EventShopItem

{

    public string itemId;

    public string itemName;

    public int cost;                            // Event currency cost

    public int stock;                           // Limited stock

    public int purchaseLimit;                   // Per player limit

}

 

/// <summary>

/// Event milestones (cumulative rewards)

/// </summary>

[Serializable]

public class EventMilestones

{

    public List<EventMilestone> milestones;

}

 

/// <summary>

/// Single event milestone

/// </summary>

[Serializable]

public class EventMilestone

{

    public int milestoneNumber;

    public string milestoneName;

    public int requiredPoints;                  // Points needed

    public List<EventReward> rewards;

    public bool isClaimed;

}

 

/// <summary>

/// Event reward

/// </summary>

[Serializable]

public class EventReward

{

    public string rewardType;

    public string rewardId;

    public int quantity;

}

 

/// <summary>

/// Event leaderboard reward

/// </summary>

[Serializable]

public class EventLeaderboardReward

{

    public int minRank;

    public int maxRank;

    public List<EventReward> rewards;

    public string exclusiveReward;              // Unique item for top ranks

}

 

/// <summary>

/// Time-limited dungeon

/// </summary>

[Serializable]

public class TimeLimitedDungeon

{

    public string dungeonId;

    public string dungeonName;

    public string description;

 

    // Schedule

    public List<DayOfWeek> availableDays;       // Days of week it's open

    public TimeSpan startTime;                  // Time of day it opens

    public TimeSpan endTime;                    // Time of day it closes

    public int durationMinutes;                 // How long it stays open

 

    // Difficulty tiers

    public List<DungeonTier> tiers;

 

    // Entry

    public int energyCost;

    public int dailyAttempts;

    public int keyRequired;                     // Special key item needed

 

    // Rewards

    public List<TimedDungeonReward> rewards;

    public List<TimedDungeonReward> bonusRewards; // For perfect clear

}

 

/// <summary>

/// Dungeon difficulty tier

/// </summary>

[Serializable]

public class DungeonTier

{

    public string tierId;

    public string tierName;                     // "Normal", "Hard", "Hell", "Nightmare"

    public int recommendedPower;

    public float rewardMultiplier;              // Multiplier on rewards

}

 

/// <summary>

/// Time-limited dungeon reward

/// </summary>

[Serializable]

public class TimedDungeonReward

{

    public string rewardType;

    public string rewardId;

    public int minQuantity;

    public int maxQuantity;

    public float dropChance;

}

 

/// <summary>

/// Login reward system

/// </summary>

[Serializable]

public class LoginRewardSystem

{

    public int consecutiveDays;

    public List<DailyLoginReward> rewards;

    public DateTime lastLoginDate;

    public bool hasClaimedToday;

}

 

/// <summary>

/// Daily login reward

/// </summary>

[Serializable]

public class DailyLoginReward

{

    public int day;

    public string rewardType;

    public string rewardId;

    public int quantity;

    public bool isClaimed;

    public bool isSpecial;                      // Day 7, 30, etc.

}

 

/// <summary>

/// Pass system (Battle Pass style)

/// </summary>

[Serializable]

public class SeasonPass

{

    public string passId;

    public string passName;

    public int season;

    public DateTime startDate;

    public DateTime endDate;

 

    // Progress

    public int currentLevel;

    public int currentExp;

    public int maxLevel;

 

    // Tiers

    public bool hasPremiumPass;                 // Did player buy premium?

    public List<PassTier> tiers;

}

 

/// <summary>

/// Pass tier rewards

/// </summary>

[Serializable]

public class PassTier

{

    public int tier;

    public int requiredExp;

 

    // Free rewards

    public List<PassReward> freeRewards;

 

    // Premium rewards

    public List<PassReward> premiumRewards;

 

    // Claimed status

    public bool freeRewardClaimed;

    public bool premiumRewardClaimed;

}

 

/// <summary>

/// Pass reward

/// </summary>

[Serializable]

public class PassReward

{

    public string rewardType;

    public string rewardId;

    public int quantity;

}

 

/// <summary>

/// Database for activities system

/// </summary>

[Serializable]

public class ActivitiesDatabase

{

    public List<DailyContract> dailyContractPool;

    public List<WeeklyContract> weeklyContractPool;

    public List<MonthlyContract> monthlyContractPool;

    public List<Bounty> bountyPool;

    public List<LimitedEvent> events;

    public List<TimeLimitedDungeon> timeLimitedDungeons;

    public LoginRewardSystem loginRewards;

    public SeasonPass currentSeasonPass;

}