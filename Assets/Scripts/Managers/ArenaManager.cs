using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager for Arena PvP system with rankings, seasons, and replays
/// </summary>
public class ArenaManager : Singleton<ArenaManager>
{
    private ArenaDatabase arenaDatabase;
    private Dictionary<string, PlayerArenaProfile> playerProfiles = new Dictionary<string, PlayerArenaProfile>();
    private Dictionary<string, BattleReplayData> replays = new Dictionary<string, BattleReplayData>();
    private ArenaSeasonConfig currentSeason;
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
            Debug.Log($"Loaded {arenaDatabase.rankTiers.Count} arena rank tiers");

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
                rating = 1000,
                highestRating = 1000,
                wins = 0,
                losses = 0,
                winStreak = 0,
                seasonWins = 0,
                seasonLosses = 0,
                defenseWins = 0,
                defenseLosses = 0,
                lastBattleTime = DateTime.MinValue,
                defenseTeam = new List<string>()
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
        int playerRating = playerProfile.rating;

        // Find players within rating range
        var opponents = playerProfiles.Values
            .Where(p => p.playerId != playerId)
            .Where(p => Math.Abs(p.rating - playerRating) <= 200)
            .OrderBy(p => Math.Abs(p.rating - playerRating))
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
        int ratingChange = CalculateRatingChange(attackerProfile.rating, defenderProfile.rating, battleResult.attackerWon);

        if (battleResult.attackerWon)
        {
            attackerProfile.rating += ratingChange;
            attackerProfile.wins++;
            attackerProfile.seasonWins++;
            attackerProfile.winStreak++;

            defenderProfile.rating -= ratingChange;
            defenderProfile.defenseLosses++;
        }
        else
        {
            attackerProfile.rating -= ratingChange;
            attackerProfile.losses++;
            attackerProfile.seasonLosses++;
            attackerProfile.winStreak = 0;

            defenderProfile.rating += ratingChange;
            defenderProfile.defenseWins++;
        }

        // Update highest rating
        if (attackerProfile.rating > attackerProfile.highestRating)
        {
            attackerProfile.highestRating = attackerProfile.rating;
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
            ratingChange = ratingChange,
            newRating = attackerProfile.rating,
            newRank = attackerProfile.currentRank,
            replayId = replayId,
            rewards = CalculateBattleRewards(battleResult.attackerWon, attackerProfile.currentRank)
        };
    }

    /// <summary>
    /// Set defense team
    /// </summary>
    public void SetDefenseTeam(string playerId, List<string> heroIds)
    {
        var profile = GetPlayerProfile(playerId);
        profile.defenseTeam = new List<string>(heroIds);
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
        foreach (var rewardTier in currentSeason.seasonRewards)
        {
            if (rank >= rewardTier.minRank && rank <= rewardTier.maxRank)
            {
                rewards.AddRange(rewardTier.rewards);
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
        foreach (var tier in arenaDatabase.rankTiers.OrderByDescending(t => t.minRating))
        {
            if (profile.rating >= tier.minRating)
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
                level = u.GetLevel(),
                power = u.GetAttack() + u.GetDefense()
            }).ToList(),
            defenderTeam = defenderTeam.Select(u => new ReplayHero
            {
                heroId = u.GetHeroId(),
                level = u.GetLevel(),
                power = u.GetAttack() + u.GetDefense()
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
            .OrderByDescending(p => p.rating)
            .ThenByDescending(p => p.wins)
            .Select((p, index) => new LeaderboardEntry
            {
                rank = index + 1,
                playerId = p.playerId,
                playerName = "Player", // TODO: Get from player data
                rating = p.rating,
                currentRank = p.currentRank,
                wins = p.wins,
                losses = p.losses,
                winRate = p.wins + p.losses > 0 ? (float)p.wins / (p.wins + p.losses) : 0
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
            var rankTier = arenaDatabase.rankTiers.Find(r => r.rankId == rank);
            if (rankTier != null)
            {
                int bonusTokens = (int)(10 * rankTier.rewardMultiplier);
                rewards.Add(new ArenaReward
                {
                    rewardType = "currency",
                    rewardId = "arena_tokens",
                    quantity = bonusTokens
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
            rating = targetRating + UnityEngine.Random.Range(-50, 50),
            highestRating = targetRating,
            wins = UnityEngine.Random.Range(10, 100),
            losses = UnityEngine.Random.Range(10, 100),
            defenseTeam = new List<string>()
        };
    }
}

/// <summary>
/// Result of arena battle
/// </summary>
[Serializable]
public class ArenaBattleResult
{
    public bool attackerWon;
    public int ratingChange;
    public int newRating;
    public string newRank;
    public string replayId;
    public List<ArenaReward> rewards;
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

/// <summary>
/// Database containing arena configs
/// </summary>
[Serializable]
public class ArenaDatabase
{
    public List<ArenaRankTier> rankTiers = new List<ArenaRankTier>();
    public List<ArenaSeasonConfig> seasons = new List<ArenaSeasonConfig>();
    public List<ArenaShopItem> shopItems = new List<ArenaShopItem>();
}
