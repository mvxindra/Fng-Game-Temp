using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manager for guild system including raids and co-op
/// </summary>
public class GuildManager : Singleton<GuildManager>
{
    private GuildDatabase guildDatabase;
    private Dictionary<string, Guild> guilds = new Dictionary<string, Guild>();
    private Dictionary<string, ActiveRaid> activeRaids = new Dictionary<string, ActiveRaid>();
    private Dictionary<string, ActiveCoopSession> activeCoopSessions = new Dictionary<string, ActiveCoopSession>();

    private string playerGuildId;

    protected override void Awake()
    {
        base.Awake();
        LoadGuildDatabase();
    }

    private void LoadGuildDatabase()
    {
        TextAsset guildData = Resources.Load<TextAsset>("Config/guild_system");
        if (guildData != null)
        {
            guildDatabase = JsonUtility.FromJson<GuildDatabase>(guildData.text);
            Debug.Log($"Loaded {guildDatabase.raidBosses.Count} raid bosses");
        }
        else
        {
            Debug.LogError("Failed to load guild_system.json");
            guildDatabase = new GuildDatabase();
        }
    }

    /// <summary>
    /// Create a new guild
    /// </summary>
    public Guild CreateGuild(string guildName, string guildMasterId, int creationCost)
    {
        // Check if player has enough resources
        if (!HasEnoughGold(creationCost))
        {
            Debug.Log("Not enough gold to create guild");
            return null;
        }

        string guildId = Guid.NewGuid().ToString();
        var guild = new Guild
        {
            guildId = guildId,
            guildName = guildName,
            guildLevel = 1,
            guildExp = 0,
            guildMasterId = guildMasterId,
            creationDate = DateTime.Now,
            members = new List<GuildMember>(),
            officers = new List<string>(),
            maxMembers = 30,
            guildGold = 0,
            totalDonations = 0,
            raidHistory = new List<RaidRecord>(),
            activeBuffs = new List<GuildBuff>(),
            settings = new GuildSettings
            {
                joinType = "open",
                minLevel = 1,
                autoKickInactiveDays = 30,
                dailyDonationLimit = 10000
            }
        };

        // Add guild master as first member
        guild.members.Add(new GuildMember
        {
            playerId = guildMasterId,
            playerName = "Guild Master", // TODO: Get from player data
            playerLevel = 1,
            joinDate = DateTime.Now,
            totalContribution = 0,
            weeklyContribution = 0,
            rank = "master",
            lastActive = DateTime.Now
        });

        guilds[guildId] = guild;
        playerGuildId = guildId;

        DeductGold(creationCost);
        Debug.Log($"Created guild {guildName}");
        return guild;
    }

    /// <summary>
    /// Join a guild
    /// </summary>
    public bool JoinGuild(string guildId, string playerId, string playerName, int playerLevel)
    {
        if (!guilds.ContainsKey(guildId))
        {
            Debug.Log("Guild not found");
            return false;
        }

        var guild = guilds[guildId];

        if (guild.members.Count >= guild.maxMembers)
        {
            Debug.Log("Guild is full");
            return false;
        }

        if (playerLevel < guild.settings.minLevel)
        {
            Debug.Log($"Player level too low (minimum {guild.settings.minLevel})");
            return false;
        }

        guild.members.Add(new GuildMember
        {
            playerId = playerId,
            playerName = playerName,
            playerLevel = playerLevel,
            joinDate = DateTime.Now,
            totalContribution = 0,
            weeklyContribution = 0,
            rank = "member",
            lastActive = DateTime.Now
        });

        playerGuildId = guildId;
        Debug.Log($"{playerName} joined guild {guild.guildName}");
        return true;
    }

    /// <summary>
    /// Start a guild raid
    /// </summary>
    public ActiveRaid StartRaid(string guildId, string raidBossId)
    {
        if (!guilds.ContainsKey(guildId))
        {
            Debug.LogError("Guild not found");
            return null;
        }

        var bossConfig = guildDatabase.raidBosses.Find(b => b.bossId == raidBossId);
        if (bossConfig == null)
        {
            Debug.LogError($"Raid boss {raidBossId} not found");
            return null;
        }

        var guild = guilds[guildId];

        // Check requirements
        if (guild.guildLevel < bossConfig.requirements.minGuildLevel)
        {
            Debug.Log($"Guild level too low (requires {bossConfig.requirements.minGuildLevel})");
            return null;
        }

        // Create active raid
        string raidId = Guid.NewGuid().ToString();
        var raid = new ActiveRaid
        {
            raidId = raidId,
            guildId = guildId,
            raidBossId = raidBossId,
            startTime = DateTime.Now,
            endTime = DateTime.Now.AddHours(bossConfig.duration),
            currentPhase = 1,
            bossCurrentHP = bossConfig.bossHP,
            bossMaxHP = bossConfig.bossHP,
            participantDamage = new Dictionary<string, int>(),
            isComplete = false
        };

        activeRaids[raidId] = raid;
        Debug.Log($"Started raid against {bossConfig.bossName}");
        return raid;
    }

    /// <summary>
    /// Attack raid boss
    /// </summary>
    public RaidAttackResult AttackRaidBoss(string raidId, string playerId, List<CombatUnit> team)
    {
        if (!activeRaids.ContainsKey(raidId))
        {
            Debug.LogError("Raid not found");
            return null;
        }

        var raid = activeRaids[raidId];
        var bossConfig = guildDatabase.raidBosses.Find(b => b.bossId == raid.raidBossId);

        if (raid.isComplete)
        {
            Debug.Log("Raid already complete");
            return null;
        }

        if (DateTime.Now > raid.endTime)
        {
            Debug.Log("Raid time expired");
            raid.isComplete = true;
            return null;
        }

        // Simulate combat and calculate damage
        int damageDealt = SimulateRaidCombat(team, bossConfig, raid.currentPhase);

        // Record damage
        if (!raid.participantDamage.ContainsKey(playerId))
        {
            raid.participantDamage[playerId] = 0;
        }
        raid.participantDamage[playerId] += damageDealt;

        // Apply damage to boss
        raid.bossCurrentHP -= damageDealt;

        // Check phase transitions
        CheckPhaseTransition(raid, bossConfig);

        // Check if boss defeated
        bool bossDefeated = raid.bossCurrentHP <= 0;
        if (bossDefeated)
        {
            raid.isComplete = true;
            raid.bossCurrentHP = 0;
            CompleteRaid(raidId);
        }

        return new RaidAttackResult
        {
            damageDealt = damageDealt,
            totalDamage = raid.participantDamage[playerId],
            bossCurrentHP = raid.bossCurrentHP,
            bossMaxHP = raid.bossMaxHP,
            currentPhase = raid.currentPhase,
            bossDefeated = bossDefeated
        };
    }

    /// <summary>
    /// Check and trigger phase transitions
    /// </summary>
    private void CheckPhaseTransition(ActiveRaid raid, RaidBossConfig bossConfig)
    {
        float hpPercent = (float)raid.bossCurrentHP / raid.bossMaxHP;

        foreach (var phase in bossConfig.phases)
        {
            if (phase.phaseNumber > raid.currentPhase && hpPercent <= phase.hpThreshold)
            {
                raid.currentPhase = phase.phaseNumber;
                Debug.Log($"Raid boss entered Phase {phase.phaseNumber}!");
                // TODO: Notify players, apply phase mechanics
            }
        }
    }

    /// <summary>
    /// Complete a raid and distribute rewards
    /// </summary>
    private void CompleteRaid(string raidId)
    {
        var raid = activeRaids[raidId];
        var guild = guilds[raid.guildId];
        var bossConfig = guildDatabase.raidBosses.Find(b => b.bossId == raid.raidBossId);

        // Sort participants by damage
        var rankings = raid.participantDamage.OrderByDescending(kvp => kvp.Value).ToList();

        // Distribute rewards based on contribution
        for (int i = 0; i < rankings.Count; i++)
        {
            string playerId = rankings[i].Key;
            int damage = rankings[i].Value;
            int rank = i + 1;

            // Calculate rewards (top contributors get bonus)
            var rewards = CalculateRaidRewards(bossConfig, damage, rank);
            DistributeRewards(playerId, rewards);
        }

        // Add guild exp and gold
        guild.guildExp += bossConfig.guildExpReward;
        guild.guildGold += bossConfig.guildGoldReward;

        // Record in history
        guild.raidHistory.Add(new RaidRecord
        {
            raidBossId = raid.raidBossId,
            completionDate = DateTime.Now,
            participants = raid.participantDamage.Count,
            totalDamage = raid.participantDamage.Values.Sum(),
            mvpPlayerId = rankings[0].Key
        });

        Debug.Log($"Raid completed! MVP: {rankings[0].Key} with {rankings[0].Value} damage");
    }

    /// <summary>
    /// Donate to guild
    /// </summary>
    public bool DonateToGuild(string playerId, int goldAmount)
    {
        if (string.IsNullOrEmpty(playerGuildId))
        {
            Debug.Log("Not in a guild");
            return false;
        }

        var guild = guilds[playerGuildId];
        var member = guild.members.Find(m => m.playerId == playerId);

        if (member == null)
        {
            Debug.LogError("Player not in guild");
            return false;
        }

        if (!HasEnoughGold(goldAmount))
        {
            Debug.Log("Not enough gold");
            return false;
        }

        // Deduct gold and add to guild
        DeductGold(goldAmount);
        guild.guildGold += goldAmount;
        guild.totalDonations += goldAmount;
        member.totalContribution += goldAmount;
        member.weeklyContribution += goldAmount;

        Debug.Log($"{member.playerName} donated {goldAmount} gold to guild");
        return true;
    }

    /// <summary>
    /// Start co-op dungeon session
    /// </summary>
    public ActiveCoopSession StartCoopDungeon(string dungeonId, string player1Id, string player2Id)
    {
        var dungeonConfig = guildDatabase.coopDungeons.Find(d => d.dungeonId == dungeonId);
        if (dungeonConfig == null)
        {
            Debug.LogError($"Co-op dungeon {dungeonId} not found");
            return null;
        }

        string sessionId = Guid.NewGuid().ToString();
        var session = new ActiveCoopSession
        {
            sessionId = sessionId,
            dungeonId = dungeonId,
            player1Id = player1Id,
            player2Id = player2Id,
            currentWave = 1,
            isComplete = false,
            rewardsEarned = new List<DungeonReward>()
        };

        activeCoopSessions[sessionId] = session;
        Debug.Log($"Started co-op dungeon {dungeonConfig.dungeonName}");
        return session;
    }

    /// <summary>
    /// Get player's guild
    /// </summary>
    public Guild GetPlayerGuild()
    {
        if (string.IsNullOrEmpty(playerGuildId))
            return null;

        return guilds.ContainsKey(playerGuildId) ? guilds[playerGuildId] : null;
    }

    /// <summary>
    /// Get all available raid bosses
    /// </summary>
    public List<RaidBossConfig> GetAvailableRaidBosses(int guildLevel)
    {
        return guildDatabase.raidBosses
            .Where(b => b.requirements.minGuildLevel <= guildLevel)
            .ToList();
    }

    // Helper methods
    private int SimulateRaidCombat(List<CombatUnit> team, RaidBossConfig boss, int phase)
    {
        // Simplified combat simulation
        int totalDamage = 0;
        foreach (var unit in team)
        {
            totalDamage += unit.GetAttack() * 10; // Simplified calculation
        }
        return totalDamage;
    }

    private List<RaidRewardItem> CalculateRaidRewards(RaidBossConfig boss, int damage, int rank)
    {
        var rewards = new List<RaidRewardItem>();

        foreach (var reward in boss.rewards)
        {
            float multiplier = rank <= 3 ? 1.5f : 1.0f; // Top 3 get bonus
            int quantity = (int)(reward.quantity * multiplier);

            rewards.Add(new RaidRewardItem
            {
                rewardType = reward.rewardType,
                rewardId = reward.rewardId,
                quantity = quantity
            });
        }

        return rewards;
    }

    private void DistributeRewards(string playerId, List<RaidRewardItem> rewards)
    {
        // TODO: Add rewards to player inventory
        Debug.Log($"Distributed {rewards.Count} rewards to {playerId}");
    }

    private bool HasEnoughGold(int amount)
    {
        // TODO: Check player resources
        return true; // Placeholder
    }

    private void DeductGold(int amount)
    {
        // TODO: Deduct from player resources
        Debug.Log($"Deducted {amount} gold");
    }
}

/// <summary>
/// Result of a raid attack
/// </summary>
[Serializable]
public class RaidAttackResult
{
    public int damageDealt;
    public int totalDamage;
    public int bossCurrentHP;
    public int bossMaxHP;
    public int currentPhase;
    public bool bossDefeated;
}

/// <summary>
/// Database containing guild configs
/// </summary>
[Serializable]
public class GuildDatabase
{
    public List<RaidBossConfig> raidBosses = new List<RaidBossConfig>();
    public List<CoopDungeonConfig> coopDungeons = new List<CoopDungeonConfig>();
    public List<GuildShopItem> guildShopItems = new List<GuildShopItem>();
    public List<GuildBuffConfig> guildBuffs = new List<GuildBuffConfig>();
}
