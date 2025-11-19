using System;

using System.Collections.Generic;

 

/// <summary>

/// Guild system with raids, donations, and co-op

/// </summary>

[Serializable]

public class Guild

{

    public string guildId;

    public string guildName;

    public string guildTag;                     // 3-4 letter tag

    public string guildDescription;

    public string guildIcon;

 

    // Settings

    public int guildLevel;

    public int guildExp;

    public int maxMembers;                      // Increases with level

    public string joinType;                     // "open", "approval", "invite_only"

    public int minLevelRequirement;

 

    // Members

    public string guildMaster;                  // Player ID

    public List<GuildMember> members;

    public List<string> officers;               // Player IDs with officer permissions

 

    // Resources

    public int guildGold;

    public Dictionary<string, int> guildResources;

 

    // Progression

    public Dictionary<string, int> guildBuffs; // Active guild-wide buffs

    public List<string> completedRaids;        // Raid IDs completed

    public int weeklyContribution;              // Total weekly contributions

 

    // Stats

    public DateTime createdDate;

    public int totalRaidsCompleted;

    public int totalDonations;

 

    public Guild()

    {

        members = new List<GuildMember>();

        officers = new List<string>();

        guildResources = new Dictionary<string, int>();

        guildBuffs = new Dictionary<string, int>();

        completedRaids = new List<string>();

    }

}

 

/// <summary>

/// Guild member information

/// </summary>

[Serializable]

public class GuildMember

{

    public string playerId;

    public string playerName;

    public int playerLevel;

    public string role;                         // "master", "officer", "member"

    public DateTime joinDate;

    public DateTime lastActive;

 

    // Contributions

    public int totalContribution;

    public int weeklyContribution;

    public int lifetimeContribution;

 

    // Stats

    public int raidsParticipated;

    public int donationsMade;

}

 

/// <summary>

/// Guild raid boss configuration

/// </summary>

[Serializable]

public class GuildRaidConfig

{

    public string raidId;

    public string raidName;

    public string bossName;

    public string description;

    public int recommendedLevel;

 

    // Boss stats

    public long bossMaxHP;                      // Very high HP for guild raids

    public int bossATK;

    public int bossDEF;

    public int bossSPD;

 

    // Phases

    public List<RaidPhase> phases;

 

    // Rewards

    public List<RaidReward> firstClearRewards;

    public List<RaidReward> completionRewards;

    public List<RaidReward> rankingRewards;     // Based on contribution

 

    // Duration

    public int durationHours;                   // How long raid is active

    public int maxAttemptsPerPlayer;            // Attempts per player

 

    // Requirements

    public int minGuildLevel;

    public int entryCost;                       // Guild gold cost to start

}

 

/// <summary>

/// Raid boss phase with special mechanics

/// </summary>

[Serializable]

public class RaidPhase

{

    public int phaseNumber;

    public float hpThreshold;                   // Trigger at X% HP

    public string phaseName;

    public string description;

 

    // Phase changes

    public List<string> gainedAbilities;        // New skills boss gains

    public List<string> summonedMinions;        // Enemies spawned

    public float damageMultiplier;              // Damage multiplier

    public float defenseMultiplier;             // Defense multiplier

    public List<string> statusImmunities;       // Immunities gained

 

    // Special mechanics

    public string mechanicType;                 // "enrage", "shield", "heal", "summon"

    public string mechanicDescription;

}

 

/// <summary>

/// Raid reward configuration

/// </summary>

[Serializable]

public class RaidReward

{

    public string rewardType;                   // "gear", "material", "currency"

    public string rewardId;

    public int quantity;

    public int minRank;                         // Minimum contribution rank

    public int maxRank;                         // Maximum contribution rank

}

 

/// <summary>

/// Active guild raid instance

/// </summary>

[Serializable]

public class ActiveGuildRaid

{

    public string raidInstanceId;

    public string raidId;

    public string guildId;

    public DateTime startTime;

    public DateTime endTime;

 

    // Progress

    public long currentBossHP;

    public long maxBossHP;

    public int currentPhase;

    public bool isCompleted;

 

    // Participation

    public List<RaidParticipant> participants;

    public Dictionary<string, long> damageDealt; // Player ID -> Damage

 

    public ActiveGuildRaid()

    {

        participants = new List<RaidParticipant>();

        damageDealt = new Dictionary<string, long>();

    }

}

 

/// <summary>

/// Raid participant data

/// </summary>

[Serializable]

public class RaidParticipant

{

    public string playerId;

    public string playerName;

    public long totalDamage;

    public int attemptsUsed;

    public int maxAttempts;

    public DateTime lastAttempt;

    public bool hasClaimed;                     // Claimed rewards?

}

 

/// <summary>

/// Guild donation configuration

/// </summary>

[Serializable]

public class GuildDonationConfig

{

    public string donationId;

    public string donationName;

    public string description;

 

    // Cost

    public string currencyType;                 // "gold", "gems", "materials"

    public int cost;

    public string materialId;                   // If donating materials

 

    // Rewards

    public int guildExp;                        // EXP for guild

    public int contributionPoints;              // Points for player

    public int guildGold;                       // Gold for guild bank

 

    // Limits

    public int dailyLimit;                      // Max donations per day

}

 

/// <summary>

/// Guild shop configuration

/// </summary>

[Serializable]

public class GuildShopItem

{

    public string itemId;

    public string itemName;

    public string itemType;                     // "gear", "material", "buff"

    public int cost;                            // Contribution points

    public int stock;                           // Available stock

    public int refreshTime;                     // Hours to refresh

    public int requiredGuildLevel;              // Guild level needed

}

 

/// <summary>

/// Guild buff configuration

/// </summary>

[Serializable]

public class GuildBuff

{

    public string buffId;

    public string buffName;

    public string description;

    public int duration;                        // Hours

    public int cost;                            // Guild gold

 

    // Effects

    public float expBonus;                      // +% EXP

    public float goldBonus;                     // +% Gold

    public float dropRateBonus;                 // +% Drop rate

    public Dictionary<string, float> statBonus; // Stat bonuses

}

 

/// <summary>

/// Co-op dungeon configuration

/// </summary>

[Serializable]

public class CoopDungeonConfig

{

    public string dungeonId;

    public string dungeonName;

    public string description;

    public int playerCount;                     // Number of players (usually 2)

 

    // Difficulty

    public int recommendedLevel;

    public string difficulty;                   // "normal", "hard", "elite", "nightmare"

 

    // Enemies

    public List<CoopWave> waves;

 

    // Rewards

    public List<DungeonReward> rewards;

    public List<DungeonReward> bonusRewards;    // If both players survive

 

    // Entry

    public int energyCost;

    public int dailyLimit;

}

 

/// <summary>

/// Co-op dungeon wave

/// </summary>

[Serializable]

public class CoopWave

{

    public int waveNumber;

    public List<string> enemies;               // Enemy IDs

    public int[] enemyLevels;                  // Level for each enemy

    public string bossId;                      // Boss for this wave

    public bool isEliteWave;

}

 

/// <summary>

/// Dungeon reward

/// </summary>

[Serializable]

public class DungeonReward

{

    public string rewardType;

    public string rewardId;

    public int quantity;

    public float dropChance;                   // Chance to drop (0-1)

}

 

/// <summary>

/// Active co-op session

/// </summary>

[Serializable]

public class ActiveCoopSession

{

    public string sessionId;

    public string dungeonId;

    public List<string> playerIds;             // Participating players

    public DateTime startTime;

    public int currentWave;

    public bool isComplete;

    public bool allPlayersAlive;

 

    // Progress

    public Dictionary<string, int> playerDamage;

    public Dictionary<string, int> playerHealing;

    public Dictionary<string, int> playerKills;

 

    public ActiveCoopSession()

    {

        playerIds = new List<string>();

        playerDamage = new Dictionary<string, int>();

        playerHealing = new Dictionary<string, int>();

        playerKills = new Dictionary<string, int>();

    }

}

 

/// <summary>

/// Database for guild and co-op systems

/// </summary>

[Serializable]

public class GuildCoopDatabase

{

    public List<GuildRaidConfig> raids;

    public List<GuildDonationConfig> donations;

    public List<GuildShopItem> shopItems;

    public List<GuildBuff> buffs;

    public List<CoopDungeonConfig> coopDungeons;

}