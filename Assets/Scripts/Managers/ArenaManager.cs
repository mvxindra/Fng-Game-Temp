using System;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using FnGMafia.Core;

 

/// <summary>

/// Manager for Arena PvP system with rankings, seasons, and replays

/// </summary>

public class ArenaManager : Singleton<ArenaManager>

{

    private ArenaDatabase arenaDatabase;

    private Dictionary<string, PlayerArenaProfile> playerProfiles = new Dictionary<string, PlayerArenaProfile>();

    private Dictionary<string, BattleReplayData> replays = new Dictionary<string, BattleReplayData>();

    private ArenaSeason currentSeason;

    private List<LeaderboardEntry> leaderboard = new List<LeaderboardEntry>();

 

    private string currentPlayerId;

 

    protected override void Awake()

    {

        base.Awake();

        LoadArenaDatabase();

    }

 

    private void LoadArenaDatabase()

    {

        TextAsset arenaData = Resources.Load<TextAsset>("Config/arena_system");

        if (arenaData != null)

        {

            arenaDatabase = JsonUtility.FromJson<ArenaDatabase>(arenaData.text);

            Debug.Log($"Loaded {arenaDatabase.config.ranks.Count} arena rank tiers");

 

            // Load current season

            if (arenaDatabase.seasons.Count > 0)

            {

                currentSeason = arenaDatabase.seasons[arenaDatabase.seasons.Count - 1];

            }

        }

        else

        {

            Debug.LogError("Failed to load arena_system.json");

            arenaDatabase = new ArenaDatabase();

        }

    }

 

    /// <summary>

    /// Get or create player's arena profile

    /// </summary>

    public PlayerArenaProfile GetPlayerProfile(string playerId)

    {

        if (!playerProfiles.ContainsKey(playerId))

        {

            playerProfiles[playerId] = new PlayerArenaProfile

            {

                playerId = playerId,

                currentRank = "bronze_1",

                currentRating = 1000,

                allTimeHighRating = 1000,

                totalWins = 0,

                totalLosses = 0,

                winStreak = 0,

                seasonWins = 0,

                seasonLosses = 0,

                defensesWon = 0,

                defensesLost = 0,

                lastBattleTime = DateTime.MinValue,

                defenseTeam = new ArenaDefenseSetup()

            };

        }

        return playerProfiles[playerId];

    }

 

    /// <summary>

    /// Find opponents for matchmaking

    /// </summary>

    public List<PlayerArenaProfile> FindOpponents(string playerId, int count = 3)

    {

        var playerProfile = GetPlayerProfile(playerId);

        int playerRating = playerProfile.currentRating;



        // Find players within rating range

        var opponents = playerProfiles.Values

            .Where(p => p.playerId != playerId)

            .Where(p => Math.Abs(p.currentRating - playerRating) <= 200)

            .OrderBy(p => Math.Abs(p.currentRating - playerRating))

            .Take(count)

            .ToList();

 

        // If not enough opponents, add some AI opponents

        while (opponents.Count < count)

        {

            opponents.Add(GenerateAIOpponent(playerRating));

        }

 

        return opponents;

    }

 

    /// <summary>

    /// Start an arena battle

    /// </summary>

    public ArenaBattleResult StartBattle(string attackerId, string defenderId, List<CombatUnit> attackerTeam, List<CombatUnit> defenderTeam)

    {

        var attackerProfile = GetPlayerProfile(attackerId);

        var defenderProfile = GetPlayerProfile(defenderId);

 

        // Check for banned heroes in current season

        if (currentSeason != null && currentSeason.bannedHeroes.Count > 0)

        {

            foreach (var unit in attackerTeam)

            {

                if (currentSeason.bannedHeroes.Contains(unit.GetHeroId()))

                {

                    Debug.Log($"Hero {unit.GetHeroId()} is banned this season");

                    return null;

                }

            }

        }

 

        // Simulate battle

        var battleResult = SimulateBattle(attackerTeam, defenderTeam);

 

        // Record replay

        var replay = RecordBattleReplay(attackerId, defenderId, attackerTeam, defenderTeam, battleResult);

        string replayId = Guid.NewGuid().ToString();

        replays[replayId] = replay;

 

        // Update ratings

        int ratingChange = CalculateRatingChange(attackerProfile.currentRating, defenderProfile.currentRating, battleResult.attackerWon);



        if (battleResult.attackerWon)

        {

            attackerProfile.currentRating += ratingChange;

            attackerProfile.totalWins++;

            attackerProfile.seasonWins++;

            attackerProfile.winStreak++;



            defenderProfile.currentRating -= ratingChange;

            defenderProfile.defensesLost++;

        }

        else

        {

            attackerProfile.currentRating -= ratingChange;

            attackerProfile.totalLosses++;

            attackerProfile.seasonLosses++;

            attackerProfile.winStreak = 0;



            defenderProfile.currentRating += ratingChange;

            defenderProfile.defensesWon++;

        }



        // Update highest rating

        if (attackerProfile.currentRating > attackerProfile.allTimeHighRating)

        {

            attackerProfile.allTimeHighRating = attackerProfile.currentRating;

        }

 

        // Update rank

        UpdatePlayerRank(attackerProfile);

        UpdatePlayerRank(defenderProfile);

 

        attackerProfile.lastBattleTime = DateTime.Now;

 

        // Update leaderboard

        UpdateLeaderboard();

 

        Debug.Log($"Arena battle: {(battleResult.attackerWon ? "Victory" : "Defeat")}! Rating: {(battleResult.attackerWon ? "+" : "-")}{ratingChange}");

 

        return new ArenaBattleResult

        {

            attackerWon = battleResult.attackerWon,

            attackerRatingChange = ratingChange,

            attackerNewRating = attackerProfile.currentRating,

            defenderRatingChange = ratingChange,

            defenderNewRating = defenderProfile.currentRating,

            attackerRewards = CalculateBattleRewards(battleResult.attackerWon, attackerProfile.currentRank),

            replayData = replay

        };

    }

 

    /// <summary>

    /// Set defense team

    /// </summary>

    public void SetDefenseTeam(string playerId, List<string> heroIds)

    {

        var profile = GetPlayerProfile(playerId);

        profile.defenseTeam.heroIds = new List<string>(heroIds);

        profile.defenseTeam.lastModified = DateTime.Now;

        Debug.Log($"Defense team set for {playerId}");

    }

 

    /// <summary>

    /// Get battle replay

    /// </summary>

    public BattleReplayData GetReplay(string replayId)

    {

        return replays.ContainsKey(replayId) ? replays[replayId] : null;

    }

 

    /// <summary>

    /// Get current leaderboard

    /// </summary>

    public List<LeaderboardEntry> GetLeaderboard(int topCount = 100)

    {

        return leaderboard.Take(topCount).ToList();

    }

 

    /// <summary>

    /// Get player's leaderboard rank

    /// </summary>

    public int GetPlayerLeaderboardRank(string playerId)

    {

        var entry = leaderboard.FindIndex(e => e.playerId == playerId);

        return entry >= 0 ? entry + 1 : -1;

    }

 

    /// <summary>

    /// Claim season rewards

    /// </summary>

    public List<ArenaReward> ClaimSeasonRewards(string playerId)

    {

        if (currentSeason == null)

        {

            Debug.Log("No active season");

            return new List<ArenaReward>();

        }

 

        var profile = GetPlayerProfile(playerId);

        int rank = GetPlayerLeaderboardRank(playerId);

 

        var rewards = new List<ArenaReward>();

 

        // Find applicable reward tier

        foreach (var rewardTier in currentSeason.rewards)

        {

            if (rank >= rewardTier.minRank && rank <= rewardTier.maxRank)

            {

                rewards.Add(rewardTier);

                break;

            }

        }

 

        Debug.Log($"Claimed {rewards.Count} season rewards for rank {rank}");

        return rewards;

    }

 

    /// <summary>

    /// Update player rank based on rating

    /// </summary>

    private void UpdatePlayerRank(PlayerArenaProfile profile)

    {

        foreach (var tier in arenaDatabase.config.ranks.OrderByDescending(t => t.minRating))

        {

            if (profile.currentRating >= tier.minRating)

            {

                profile.currentRank = tier.rankId;

                return;

            }

        }

    }

 

    /// <summary>

    /// Calculate rating change using ELO-like system

    /// </summary>

    private int CalculateRatingChange(int attackerRating, int defenderRating, bool attackerWon)

    {

        const int K = 32; // K-factor for rating changes

        float expectedScore = 1.0f / (1.0f + Mathf.Pow(10, (defenderRating - attackerRating) / 400.0f));

        float actualScore = attackerWon ? 1.0f : 0.0f;

        int change = Mathf.RoundToInt(K * (actualScore - expectedScore));

        return Mathf.Abs(change);

    }

 

    /// <summary>

    /// Simulate arena battle

    /// </summary>

    private BattleSimulationResult SimulateBattle(List<CombatUnit> attackerTeam, List<CombatUnit> defenderTeam)

    {

        // Simplified battle simulation

        int attackerPower = attackerTeam.Sum(u => u.GetAttack() + u.GetDefense() + u.GetMaxHP() / 10);

        int defenderPower = defenderTeam.Sum(u => u.GetAttack() + u.GetDefense() + u.GetMaxHP() / 10);

 

        // Add some randomness (±10%)

        attackerPower = (int)(attackerPower * UnityEngine.Random.Range(0.9f, 1.1f));

        defenderPower = (int)(defenderPower * UnityEngine.Random.Range(0.9f, 1.1f));

 

        bool attackerWon = attackerPower > defenderPower;

        int turnCount = UnityEngine.Random.Range(5, 15);

 

        return new BattleSimulationResult

        {

            attackerWon = attackerWon,

            turnCount = turnCount

        };

    }

 

    /// <summary>

    /// Record battle replay

    /// </summary>

    private BattleReplayData RecordBattleReplay(string attackerId, string defenderId, List<CombatUnit> attackerTeam, List<CombatUnit> defenderTeam, BattleSimulationResult result)

    {

        var replay = new BattleReplayData

        {

            replayId = Guid.NewGuid().ToString(),

            attackerPlayerId = attackerId,

            defenderPlayerId = defenderId,

            battleDate = DateTime.Now,

            attackerWon = result.attackerWon,

            attackerTeam = attackerTeam.Select(u => new ReplayHero

            {

                heroId = u.GetHeroId(),

                heroName = u.GetHeroId(),

                level = u.GetLevel(),

                rarity = 1,

                stats = new Dictionary<string, int>

                {

                    { "ATK", u.GetAttack() },

                    { "DEF", u.GetDefense() },

                    { "HP", u.GetMaxHP() }

                },

                skills = new List<string>(),

                gear = new List<string>()

            }).ToList(),

            defenderTeam = defenderTeam.Select(u => new ReplayHero

            {

                heroId = u.GetHeroId(),

                heroName = u.GetHeroId(),

                level = u.GetLevel(),

                rarity = 1,

                stats = new Dictionary<string, int>

                {

                    { "ATK", u.GetAttack() },

                    { "DEF", u.GetDefense() },

                    { "HP", u.GetMaxHP() }

                },

                skills = new List<string>(),

                gear = new List<string>()

            }).ToList(),

            turns = new List<ReplayTurn>() // Simplified - would record actual turns in full implementation

        };

 

        return replay;

    }

 

    /// <summary>

    /// Update leaderboard rankings

    /// </summary>

    private void UpdateLeaderboard()

    {

        leaderboard = playerProfiles.Values

            .OrderByDescending(p => p.currentRating)

            .ThenByDescending(p => p.totalWins)

            .Select((p, index) => new LeaderboardEntry

            {

                rank = index + 1,

                playerId = p.playerId,

                playerName = "Player", // TODO: Get from player data

                rating = p.currentRating,

                wins = p.totalWins,

                losses = p.totalLosses,

                winRate = p.totalWins + p.totalLosses > 0 ? (float)p.totalWins / (p.totalWins + p.totalLosses) : 0

            })

            .ToList();

    }

 

    /// <summary>

    /// Calculate battle rewards

    /// </summary>

    private List<ArenaReward> CalculateBattleRewards(bool won, string rank)

    {

        var rewards = new List<ArenaReward>();

 

        if (won)

        {

            // Base rewards

            rewards.Add(new ArenaReward

            {

                rewardType = "currency",

                rewardId = "arena_tokens",

                quantity = 10

            });

 

            // Bonus based on rank

            var rankTier = arenaDatabase.config.ranks.Find(r => r.rankId == rank);

            if (rankTier != null)

            {

                int bonusTokens = rankTier.dailyArenaTokens;

                rewards.Add(new ArenaReward

                {

                    rewardType = "currency",

                    rewardId = "arena_tokens",

                    quantity = bonusTokens,

                    minRank = 1,

                    maxRank = 100

                });

            }

        }

 

        return rewards;

    }

 

    /// <summary>

    /// Generate AI opponent

    /// </summary>

    private PlayerArenaProfile GenerateAIOpponent(int targetRating)

    {

        string aiId = "ai_" + Guid.NewGuid().ToString();

        return new PlayerArenaProfile

        {

            playerId = aiId,

            currentRank = "bronze_1",

            currentRating = targetRating + UnityEngine.Random.Range(-50, 50),

            allTimeHighRating = targetRating,

            totalWins = UnityEngine.Random.Range(10, 100),

            totalLosses = UnityEngine.Random.Range(10, 100),

            defenseTeam = new ArenaDefenseSetup()

        };

    }

}

 


/// <summary>

/// Simplified battle simulation result

/// </summary>

[Serializable]

public class BattleSimulationResult

{

    public bool attackerWon;

    public int turnCount;

}

 

