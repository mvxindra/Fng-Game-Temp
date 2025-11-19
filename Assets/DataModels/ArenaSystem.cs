using System;
using System.Collections.Generic;

/// <summary>
/// Arena/PvP system with leaderboards, seasons, and replays
/// </summary>
[Serializable]
public class ArenaConfig
{
    public int maxDailyBattles;                 // Free battles per day
    public int refreshCost;                     // Cost to refresh opponents
    public List<ArenaRank> ranks;               // All rank tiers
    public List<ArenaReward> seasonRewards;     // End of season rewards
}

/// <summary>
/// Arena rank tier
/// </summary>
[Serializable]
public class ArenaRank
{
    public string rankId;
    public string rankName;                     // "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Master", "Challenger"
    public int minRating;                       // Minimum rating for this rank
    public int maxRating;                       // Maximum rating
    public string rankIcon;                     // Icon path

    // Rewards
    public int dailyGems;                       // Daily gem reward
    public int dailyArenaTokens;                // Arena currency
    public List<string> unlockedFeatures;       // Features unlocked at this rank
}

/// <summary>
/// Player arena profile
/// </summary>
[Serializable]
public class PlayerArenaProfile
{
    public string playerId;
    public string playerName;
    public int playerLevel;

    // Rating
    public int currentRating;
    public int seasonHighRating;
    public int allTimeHighRating;
    public string currentRank;

    // Stats
    public int totalWins;
    public int totalLosses;
    public int winStreak;
    public int bestWinStreak;
    public int defensesWon;
    public int defensesLost;

    // Season stats
    public int seasonWins;
    public int seasonLosses;
    public int seasonRank;                      // Leaderboard position

    // Defense setup
    public ArenaDefenseSetup defenseTeam;

    // Today's battles
    public int battlesRemaining;
    public DateTime lastBattleTime;
    public List<string> recentOpponents;        // To avoid repeat battles
}

/// <summary>
/// Arena defense team setup
/// </summary>
[Serializable]
public class ArenaDefenseSetup
{
    public List<string> heroIds;                // Up to 5 heroes
    public Dictionary<string, string> heroGear; // Hero ID -> Gear loadout
    public string formation;                    // Formation type
    public DateTime lastModified;

    public ArenaDefenseSetup()
    {
        heroIds = new List<string>();
        heroGear = new Dictionary<string, string>();
    }
}

/// <summary>
/// Arena battle result
/// </summary>
[Serializable]
public class ArenaBattleResult
{
    public string battleId;
    public string attackerId;
    public string defenderId;
    public DateTime battleTime;

    // Outcome
    public bool attackerWon;
    public int turnsElapsed;
    public float battleDuration;                // Seconds

    // Rating changes
    public int attackerRatingChange;
    public int defenderRatingChange;
    public int attackerNewRating;
    public int defenderNewRating;

    // Rewards
    public List<ArenaReward> attackerRewards;

    // Battle data for replay
    public BattleReplayData replayData;
}

/// <summary>
/// Battle replay data
/// </summary>
[Serializable]
public class BattleReplayData
{
    public string replayId;
    public DateTime battleDate;

    // Teams
    public List<ReplayHero> attackerTeam;
    public List<ReplayHero> defenderTeam;

    // Turn-by-turn actions
    public List<ReplayTurn> turns;

    // Metadata
    public int winnerTeam;                      // 0 = attacker, 1 = defender
    public float battleSpeed;                   // Playback speed
}

/// <summary>
/// Hero snapshot for replay
/// </summary>
[Serializable]
public class ReplayHero
{
    public string heroId;
    public string heroName;
    public int level;
    public int rarity;
    public Dictionary<string, int> stats;       // All stats at battle time
    public List<string> skills;
    public List<string> gear;
}

/// <summary>
/// Single turn in replay
/// </summary>
[Serializable]
public class ReplayTurn
{
    public int turnNumber;
    public string actorHeroId;                  // Hero acting this turn
    public string actionType;                   // "attack", "skill", "ultimate", "item"
    public string targetHeroId;                 // Target of action
    public List<string> multiTargets;           // For AoE

    // Results
    public int damageDealt;
    public int healingDone;
    public bool wasCrit;
    public List<string> appliedEffects;         // Status effects applied
    public List<string> triggeredPassives;      // Passives that triggered
}

/// <summary>
/// Arena season configuration
/// </summary>
[Serializable]
public class ArenaSeason
{
    public int seasonNumber;
    public string seasonName;
    public string seasonTheme;
    public DateTime startDate;
    public DateTime endDate;

    // Rules
    public List<string> bannedHeroes;           // Heroes not allowed
    public List<string> featuredHeroes;         // Heroes with bonuses
    public string specialRule;                  // Special season rule

    // Rewards
    public List<ArenaReward> rewards;
    public string exclusiveSkinId;              // Top players get this
}

/// <summary>
/// Arena reward
/// </summary>
[Serializable]
public class ArenaReward
{
    public int minRank;                         // Minimum leaderboard rank
    public int maxRank;                         // Maximum leaderboard rank
    public string minRankTier;                  // Or by rank tier

    // Rewards
    public int gold;
    public int gems;
    public int arenaTokens;
    public List<ItemReward> items;
    public string exclusiveItem;                // Special item for top ranks
}

/// <summary>
/// Arena leaderboard
/// </summary>
[Serializable]
public class ArenaLeaderboard
{
    public int seasonNumber;
    public DateTime lastUpdated;
    public List<LeaderboardEntry> entries;

    public ArenaLeaderboard()
    {
        entries = new List<LeaderboardEntry>();
    }
}

/// <summary>
/// Leaderboard entry
/// </summary>
[Serializable]
public class LeaderboardEntry
{
    public int rank;
    public string playerId;
    public string playerName;
    public int playerLevel;
    public string guildName;

    // Stats
    public int rating;
    public int wins;
    public int losses;
    public float winRate;

    // Display
    public string titleIcon;                    // Special icon/title
    public List<string> topHeroes;              // Most used heroes
}

/// <summary>
/// Arena matchmaking configuration
/// </summary>
[Serializable]
public class ArenaMatchmaking
{
    public int ratingRange;                     // ± rating for opponents
    public int levelRange;                      // ± level for opponents
    public int opponentCount;                   // Number of opponents to show
    public bool allowRevenge;                   // Can revenge recent attackers

    // Bots
    public bool allowBots;                      // Fill with bots if needed
    public int botMinRating;
    public int botMaxRating;
}

/// <summary>
/// Arena shop item
/// </summary>
[Serializable]
public class ArenaShopItem
{
    public string itemId;
    public string itemName;
    public string itemType;
    public int cost;                            // Arena tokens
    public int stock;                           // Stock per season
    public int stockRemaining;
    public int requiredRank;                    // Rank tier required
}

/// <summary>
/// Live Arena (Real-time PvP)
/// </summary>
[Serializable]
public class LiveArenaConfig
{
    public bool isEnabled;
    public int entryFee;                        // Gems to enter
    public int timerPerTurn;                    // Seconds per turn
    public List<LiveArenaReward> winRewards;
    public List<LiveArenaReward> lossRewards;
}

/// <summary>
/// Live arena reward
/// </summary>
[Serializable]
public class LiveArenaReward
{
    public string rewardType;
    public string rewardId;
    public int quantity;
}

/// <summary>
/// Active live arena match
/// </summary>
[Serializable]
public class LiveArenaMatch
{
    public string matchId;
    public string player1Id;
    public string player2Id;
    public DateTime startTime;

    // Match state
    public int currentTurn;
    public string currentPlayer;
    public int player1TimeRemaining;
    public int player2TimeRemaining;

    // Teams
    public List<string> player1Team;
    public List<string> player2Team;

    // Winner
    public string winnerId;
    public string winCondition;                 // "knockout", "timeout", "surrender"
}

/// <summary>
/// Database for arena system
/// </summary>
[Serializable]
public class ArenaDatabase
{
    public ArenaConfig config;
    public List<ArenaSeason> seasons;
    public List<ArenaShopItem> shopItems;
    public ArenaMatchmaking matchmaking;
    public LiveArenaConfig liveArena;
}
