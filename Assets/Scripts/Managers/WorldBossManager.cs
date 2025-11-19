using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager for World Boss raids with multi-phase mechanics
/// </summary>
public class WorldBossManager : Singleton<WorldBossManager>
{
    private WorldBossDatabase bossDatabase;
    private Dictionary<string, ActiveWorldBoss> activeRaids = new Dictionary<string, ActiveWorldBoss>();
    private Dictionary<string, int> playerAttemptsUsed = new Dictionary<string, int>();
    private int playerRaidCurrency = 0;

    protected override void Awake()
    {
        base.Awake();
        LoadWorldBossDatabase();
    }

    private void LoadWorldBossDatabase()
    {
        TextAsset bossData = Resources.Load<TextAsset>("Config/world_boss_raids");
        if (bossData != null)
        {
            bossDatabase = JsonUtility.FromJson<WorldBossDatabase>(bossData.text);
            Debug.Log($"Loaded {bossDatabase.worldBosses.Count} world bosses");
        }
        else
        {
            Debug.LogError("Failed to load world_boss_raids.json");
            bossDatabase = new WorldBossDatabase();
        }
    }

    /// <summary>
    /// Spawn a world boss
    /// </summary>
    public ActiveWorldBoss SpawnWorldBoss(string bossId)
    {
        var bossConfig = bossDatabase.worldBosses.Find(b => b.bossId == bossId);
        if (bossConfig == null)
        {
            Debug.LogError($"World boss {bossId} not found");
            return null;
        }

        string instanceId = Guid.NewGuid().ToString();
        var activeBoss = new ActiveWorldBoss
        {
            instanceId = instanceId,
            bossId = bossId,
            spawnTime = DateTime.Now,
            endTime = DateTime.Now.AddMinutes(bossConfig.schedule.durationMinutes),
            currentPhase = 1,
            currentHP = bossConfig.bossMaxHP,
            maxHP = bossConfig.bossMaxHP,
            totalDamageDealt = 0,
            playerStats = new Dictionary<string, PlayerRaidStats>(),
            guildStats = new Dictionary<string, GuildRaidStats>(),
            totalParticipants = 0,
            isDefeated = false,
            isExpired = false,
            phaseHistory = new List<PhaseTransitionLog>()
        };

        activeRaids[instanceId] = activeBoss;

        Debug.Log($"🐉 World Boss Spawned: {bossConfig.bossName}");
        Debug.Log($"   HP: {bossConfig.bossMaxHP:N0}");
        Debug.Log($"   Duration: {bossConfig.schedule.durationMinutes} minutes");

        return activeBoss;
    }

    /// <summary>
    /// Attack world boss
    /// </summary>
    public WorldBossAttackResult AttackWorldBoss(string instanceId, string playerId, string playerName, string guildId, List<CombatUnit> team)
    {
        if (!activeRaids.ContainsKey(instanceId))
        {
            Debug.LogError("Raid instance not found");
            return null;
        }

        var raid = activeRaids[instanceId];
        var bossConfig = bossDatabase.worldBosses.Find(b => b.bossId == raid.bossId);

        // Check if raid is still active
        if (raid.isDefeated || raid.isExpired || DateTime.Now > raid.endTime)
        {
            if (!raid.isExpired && DateTime.Now > raid.endTime)
            {
                raid.isExpired = true;
                Debug.Log("⏰ World Boss raid time expired!");
            }
            return null;
        }

        // Check player attempts
        string attemptKey = $"{playerId}_{instanceId}";
        if (!playerAttemptsUsed.ContainsKey(attemptKey))
        {
            playerAttemptsUsed[attemptKey] = 0;
        }

        if (playerAttemptsUsed[attemptKey] >= bossConfig.maxAttemptsPerPlayer)
        {
            Debug.Log($"Max attempts reached ({bossConfig.maxAttemptsPerPlayer})");
            return null;
        }

        // Get current phase
        var currentPhase = GetCurrentPhase(bossConfig, raid);

        // Simulate combat with phase mechanics
        var damageResult = SimulateRaidCombat(team, bossConfig, currentPhase, raid);

        // Record player stats
        if (!raid.playerStats.ContainsKey(playerId))
        {
            raid.playerStats[playerId] = new PlayerRaidStats
            {
                playerId = playerId,
                playerName = playerName,
                guildId = guildId,
                totalDamage = 0,
                attackCount = 0,
                highestSingleHit = 0,
                damageThisPhase = 0,
                deathCount = 0,
                passedMechanics = true,
                lastAttackTime = DateTime.Now
            };
            raid.totalParticipants++;
        }

        var playerStats = raid.playerStats[playerId];
        playerStats.totalDamage += damageResult.totalDamage;
        playerStats.damageThisPhase += damageResult.totalDamage;
        playerStats.attackCount++;
        playerStats.highestSingleHit = Math.Max(playerStats.highestSingleHit, damageResult.highestHit);
        playerStats.deathCount += damageResult.deaths;
        playerStats.lastAttackTime = DateTime.Now;

        // Record guild stats
        if (!string.IsNullOrEmpty(guildId))
        {
            if (!raid.guildStats.ContainsKey(guildId))
            {
                raid.guildStats[guildId] = new GuildRaidStats
                {
                    guildId = guildId,
                    guildName = "Guild",  // TODO: Get from guild data
                    totalDamage = 0,
                    memberParticipants = 0,
                    averageDamagePerMember = 0,
                    topContributorDamage = 0
                };
            }

            var guildStats = raid.guildStats[guildId];
            guildStats.totalDamage += damageResult.totalDamage;
            guildStats.topContributorDamage = Math.Max(guildStats.topContributorDamage, (int)playerStats.totalDamage);

            // Recalculate guild stats
            var guildMembers = raid.playerStats.Values.Where(p => p.guildId == guildId).ToList();
            guildStats.memberParticipants = guildMembers.Count;
            guildStats.averageDamagePerMember = guildStats.totalDamage / Math.Max(1, guildStats.memberParticipants);
        }

        // Apply damage to boss
        raid.currentHP -= damageResult.totalDamage;
        raid.totalDamageDealt += damageResult.totalDamage;

        // Check phase transitions
        int newPhase = CheckPhaseTransition(raid, bossConfig, playerId);
        bool phaseChanged = newPhase != raid.currentPhase;

        // Check if boss defeated
        bool bossDefeated = raid.currentHP <= 0;
        if (bossDefeated)
        {
            raid.isDefeated = true;
            raid.currentHP = 0;
            raid.defeatedTime = DateTime.Now;
            CompleteBossRaid(instanceId);
        }

        // Increment attempts
        playerAttemptsUsed[attemptKey]++;

        // Create result
        var result = new WorldBossAttackResult
        {
            damageDealt = damageResult.totalDamage,
            highestHit = damageResult.highestHit,
            bossCurrentHP = raid.currentHP,
            bossMaxHP = raid.maxHP,
            currentPhase = raid.currentPhase,
            phaseChanged = phaseChanged,
            bossDefeated = bossDefeated,
            playerRank = GetPlayerRank(instanceId, playerId),
            guildRank = !string.IsNullOrEmpty(guildId) ? GetGuildRank(instanceId, guildId) : -1,
            attemptsRemaining = bossConfig.maxAttemptsPerPlayer - playerAttemptsUsed[attemptKey]
        };

        Debug.Log($"💥 {playerName} dealt {damageResult.totalDamage:N0} damage!");
        Debug.Log($"   Boss HP: {raid.currentHP:N0} / {raid.maxHP:N0} ({GetHPPercent(raid):F1}%)");
        Debug.Log($"   Rank: #{result.playerRank}");

        return result;
    }

    /// <summary>
    /// Get current phase based on HP
    /// </summary>
    private BossPhase GetCurrentPhase(WorldBossConfig boss, ActiveWorldBoss raid)
    {
        float hpPercent = (float)raid.currentHP / raid.maxHP;

        // Find appropriate phase
        foreach (var phase in boss.phases.OrderByDescending(p => p.hpThresholdStart))
        {
            if (hpPercent <= phase.hpThresholdStart)
            {
                return phase;
            }
        }

        return boss.phases[0]; // Default to phase 1
    }

    /// <summary>
    /// Check and trigger phase transitions
    /// </summary>
    private int CheckPhaseTransition(ActiveWorldBoss raid, WorldBossConfig boss, string triggerPlayer)
    {
        float hpPercent = (float)raid.currentHP / raid.maxHP;
        int oldPhase = raid.currentPhase;

        foreach (var phase in boss.phases.OrderBy(p => p.phaseNumber))
        {
            if (phase.phaseNumber > raid.currentPhase && hpPercent <= phase.hpThresholdStart)
            {
                // Phase transition!
                raid.currentPhase = phase.phaseNumber;

                // Log transition
                raid.phaseHistory.Add(new PhaseTransitionLog
                {
                    phaseNumber = phase.phaseNumber,
                    transitionTime = DateTime.Now,
                    bossHPAtTransition = raid.currentHP,
                    triggerPlayer = triggerPlayer,
                    participantsAtTransition = raid.totalParticipants
                });

                // Handle transition effects
                if (phase.transition != null && phase.transition.hasTransition)
                {
                    HandlePhaseTransition(raid, phase.transition);
                }

                // Reset phase damage for all players
                foreach (var playerStat in raid.playerStats.Values)
                {
                    playerStat.damageThisPhase = 0;
                }

                Debug.Log($"⚡ PHASE TRANSITION: {phase.phaseName}!");
                Debug.Log($"   {phase.transition?.announcement ?? "The boss grows stronger!"}");

                return phase.phaseNumber;
            }
        }

        return oldPhase;
    }

    /// <summary>
    /// Handle phase transition effects
    /// </summary>
    private void HandlePhaseTransition(ActiveWorldBoss raid, PhaseTransition transition)
    {
        if (transition.fullHeal)
        {
            raid.currentHP = raid.maxHP;
            Debug.Log("   💚 Boss fully healed!");
        }
        else if (transition.healPercent > 0)
        {
            long healAmount = (long)(raid.maxHP * transition.healPercent);
            raid.currentHP = Math.Min(raid.maxHP, raid.currentHP + healAmount);
            Debug.Log($"   💚 Boss healed {healAmount:N0} HP!");
        }

        if (transition.clearDebuffs)
        {
            Debug.Log("   ✨ Boss cleared all debuffs!");
        }

        if (transition.shieldAmount > 0)
        {
            Debug.Log($"   🛡️ Boss gained {transition.shieldAmount:N0} shield!");
        }
    }

    /// <summary>
    /// Simulate raid combat
    /// </summary>
    private RaidDamageResult SimulateRaidCombat(List<CombatUnit> team, WorldBossConfig boss, BossPhase phase, ActiveWorldBoss raid)
    {
        // Simplified combat simulation
        long totalDamage = 0;
        int highestHit = 0;
        int deaths = 0;

        // Calculate team power
        int teamPower = team.Sum(u => u.GetAttack() + u.GetDefense() + u.GetMaxHP() / 10);

        // Base damage (scaled by team power and phase modifiers)
        float phaseMultiplier = phase.statModifiers?.atkMultiplier ?? 1.0f;
        totalDamage = (long)(teamPower * UnityEngine.Random.Range(8f, 15f) * phaseMultiplier);

        // Apply boss defense
        float defReduction = 1.0f - (boss.bossDef / (boss.bossDef + 1000f));
        totalDamage = (long)(totalDamage * defReduction);

        // Highest hit simulation
        highestHit = (int)(totalDamage * UnityEngine.Random.Range(0.15f, 0.25f));

        // Death chance increases in later phases
        float deathChance = 0.1f * raid.currentPhase;
        deaths = team.Count(u => UnityEngine.Random.value < deathChance);

        return new RaidDamageResult
        {
            totalDamage = totalDamage,
            highestHit = highestHit,
            deaths = deaths
        };
    }

    /// <summary>
    /// Complete boss raid and distribute rewards
    /// </summary>
    private void CompleteBossRaid(string instanceId)
    {
        var raid = activeRaids[instanceId];
        var boss = bossDatabase.worldBosses.Find(b => b.bossId == raid.bossId);

        Debug.Log($"🎉 World Boss Defeated!");
        Debug.Log($"   Total Damage: {raid.totalDamageDealt:N0}");
        Debug.Log($"   Participants: {raid.totalParticipants}");
        Debug.Log($"   Time: {(raid.defeatedTime - raid.spawnTime).TotalMinutes:F1} minutes");

        // Generate rankings
        var playerRankings = GeneratePlayerRankings(raid);
        var guildRankings = GenerateGuildRankings(raid);

        // Distribute rewards
        DistributeRaidRewards(raid, boss, playerRankings, guildRankings);
    }

    /// <summary>
    /// Generate player rankings
    /// </summary>
    private List<RaidLeaderboardEntry> GeneratePlayerRankings(ActiveWorldBoss raid)
    {
        return raid.playerStats.Values
            .OrderByDescending(p => p.totalDamage)
            .Select((p, index) => new RaidLeaderboardEntry
            {
                rank = index + 1,
                playerId = p.playerId,
                playerName = p.playerName,
                guildName = !string.IsNullOrEmpty(p.guildId) ? "Guild" : "No Guild",
                damage = p.totalDamage,
                attackCount = p.attackCount
            })
            .ToList();
    }

    /// <summary>
    /// Generate guild rankings
    /// </summary>
    private List<GuildLeaderboardEntry> GenerateGuildRankings(ActiveWorldBoss raid)
    {
        return raid.guildStats.Values
            .OrderByDescending(g => g.totalDamage)
            .Select((g, index) => new GuildLeaderboardEntry
            {
                rank = index + 1,
                guildId = g.guildId,
                guildName = g.guildName,
                totalDamage = g.totalDamage,
                memberParticipants = g.memberParticipants,
                averageDamage = g.averageDamagePerMember
            })
            .ToList();
    }

    /// <summary>
    /// Distribute raid rewards
    /// </summary>
    private void DistributeRaidRewards(ActiveWorldBoss raid, WorldBossConfig boss, List<RaidLeaderboardEntry> playerRankings, List<GuildLeaderboardEntry> guildRankings)
    {
        foreach (var entry in playerRankings)
        {
            var rewards = CalculatePlayerRewards(boss, entry.rank, entry.damage);
            GiveRewards(entry.playerId, rewards);

            Debug.Log($"Rank #{entry.rank}: {entry.playerName} - {entry.damage:N0} damage");
            Debug.Log($"  Rewards: {rewards.raidCurrency} raid currency + items");
        }

        foreach (var entry in guildRankings)
        {
            Debug.Log($"Guild Rank #{entry.rank}: {entry.guildName} - {entry.totalDamage:N0} total damage");
        }
    }

    /// <summary>
    /// Calculate rewards for player based on rank and damage
    /// </summary>
    private PlayerRaidRewards CalculatePlayerRewards(WorldBossConfig boss, int rank, long damage)
    {
        var rewards = new PlayerRaidRewards();

        // Base raid currency
        rewards.raidCurrency = 100;

        // Ranking bonus
        var rankingReward = boss.rewardPool.personalRankingRewards
            .FirstOrDefault(r => rank >= r.minRank && rank <= r.maxRank);

        if (rankingReward != null)
        {
            rewards.raidCurrency += rankingReward.raidCurrency;
            rewards.rewardMultiplier = rankingReward.rewardMultiplier;
            rewards.items.AddRange(rankingReward.rewards);
        }

        // Damage milestones
        foreach (var milestone in boss.rewardPool.damageMilestones)
        {
            if (damage >= milestone.damageThreshold)
            {
                rewards.items.AddRange(milestone.rewards);
            }
        }

        // Top 3 get exclusive rewards
        if (rank <= 3 && boss.exclusiveCosmetics.Count > 0)
        {
            var exclusive = boss.exclusiveCosmetics.FirstOrDefault(c => c.isGuaranteedAtRank && c.guaranteedRank >= rank);
            if (exclusive != null)
            {
                rewards.exclusiveCosmetic = exclusive.cosmeticId;
            }
        }

        return rewards;
    }

    /// <summary>
    /// Get player rank in current raid
    /// </summary>
    public int GetPlayerRank(string instanceId, string playerId)
    {
        if (!activeRaids.ContainsKey(instanceId))
            return -1;

        var raid = activeRaids[instanceId];
        var rankings = raid.playerStats.Values
            .OrderByDescending(p => p.totalDamage)
            .ToList();

        for (int i = 0; i < rankings.Count; i++)
        {
            if (rankings[i].playerId == playerId)
                return i + 1;
        }

        return -1;
    }

    /// <summary>
    /// Get guild rank in current raid
    /// </summary>
    public int GetGuildRank(string instanceId, string guildId)
    {
        if (!activeRaids.ContainsKey(instanceId))
            return -1;

        var raid = activeRaids[instanceId];
        var rankings = raid.guildStats.Values
            .OrderByDescending(g => g.totalDamage)
            .ToList();

        for (int i = 0; i < rankings.Count; i++)
        {
            if (rankings[i].guildId == guildId)
                return i + 1;
        }

        return -1;
    }

    /// <summary>
    /// Get leaderboard for raid
    /// </summary>
    public (List<RaidLeaderboardEntry> players, List<GuildLeaderboardEntry> guilds) GetRaidLeaderboard(string instanceId, int topCount = 100)
    {
        if (!activeRaids.ContainsKey(instanceId))
            return (new List<RaidLeaderboardEntry>(), new List<GuildLeaderboardEntry>());

        var raid = activeRaids[instanceId];
        var playerRankings = GeneratePlayerRankings(raid).Take(topCount).ToList();
        var guildRankings = GenerateGuildRankings(raid).Take(topCount).ToList();

        return (playerRankings, guildRankings);
    }

    /// <summary>
    /// Get raid currency balance
    /// </summary>
    public int GetRaidCurrency()
    {
        return playerRaidCurrency;
    }

    /// <summary>
    /// Add raid currency
    /// </summary>
    public void AddRaidCurrency(int amount)
    {
        playerRaidCurrency += amount;
    }

    // Helper methods
    private float GetHPPercent(ActiveWorldBoss raid)
    {
        return ((float)raid.currentHP / raid.maxHP) * 100f;
    }

    private void GiveRewards(string playerId, PlayerRaidRewards rewards)
    {
        playerRaidCurrency += rewards.raidCurrency;
        // TODO: Give other rewards
        Debug.Log($"  +{rewards.raidCurrency} raid currency");
    }
}

// Helper classes
[Serializable]
public class RaidDamageResult
{
    public long totalDamage;
    public int highestHit;
    public int deaths;
}

[Serializable]
public class WorldBossAttackResult
{
    public long damageDealt;
    public int highestHit;
    public long bossCurrentHP;
    public long bossMaxHP;
    public int currentPhase;
    public bool phaseChanged;
    public bool bossDefeated;
    public int playerRank;
    public int guildRank;
    public int attemptsRemaining;
}

[Serializable]
public class PlayerRaidRewards
{
    public int raidCurrency;
    public List<WorldBossReward> items = new List<WorldBossReward>();
    public float rewardMultiplier = 1.0f;
    public string exclusiveCosmetic;
}

[Serializable]
public class GuildLeaderboardEntry
{
    public int rank;
    public string guildId;
    public string guildName;
    public long totalDamage;
    public int memberParticipants;
    public long averageDamage;
}

[Serializable]
public class WorldBossDatabase
{
    public List<WorldBossConfig> worldBosses = new List<WorldBossConfig>();
    public List<RaidShopItem> raidShopItems = new List<RaidShopItem>();
}
