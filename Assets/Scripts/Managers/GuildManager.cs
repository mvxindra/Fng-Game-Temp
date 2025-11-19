using System;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using FnGMafia.Core;

 

/// <summary>

/// Manager for guild system including raids and co-op

/// </summary>

public class GuildManager : Singleton<GuildManager>

{

    private GuildDatabase guildDatabase;

    private Dictionary<string, Guild> guilds = new Dictionary<string, Guild>();

    private Dictionary<string, ActiveGuildRaid> activeRaids = new Dictionary<string, ActiveGuildRaid>();

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

            guildMaster = guildMasterId,

            createdDate = DateTime.Now,

            members = new List<GuildMember>(),

            officers = new List<string>(),

            maxMembers = 30,

            guildGold = 0,

            totalDonations = 0,

            joinType = "open",

            minLevelRequirement = 1

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

            role = "master",

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

 

        if (playerLevel < guild.minLevelRequirement)

        {

            Debug.Log($"Player level too low (minimum {guild.minLevelRequirement})");

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

            role = "member",

            lastActive = DateTime.Now

        });

 

        playerGuildId = guildId;

        Debug.Log($"{playerName} joined guild {guild.guildName}");

        return true;

    }

 

    /// <summary>

    /// Start a guild raid

    /// </summary>

    public ActiveGuildRaid StartRaid(string guildId, string raidBossId)

    {

        if (!guilds.ContainsKey(guildId))

        {

            Debug.LogError("Guild not found");

            return null;

        }

 

        var bossConfig = guildDatabase.raidBosses.Find(b => b.raidId == raidBossId);

        if (bossConfig == null)

        {

            Debug.LogError($"Raid boss {raidBossId} not found");

            return null;

        }

 

        var guild = guilds[guildId];

 

        // Check requirements

        if (guild.guildLevel < bossConfig.minGuildLevel)

        {

            Debug.Log($"Guild level too low (requires {bossConfig.minGuildLevel})");

            return null;

        }

 

        // Create active raid

        string raidId = Guid.NewGuid().ToString();

        var raid = new ActiveGuildRaid

        {

            raidInstanceId = raidId,

            raidId = raidBossId,

            guildId = guildId,

            startTime = DateTime.Now,

            endTime = DateTime.Now.AddHours(bossConfig.durationHours),

            currentPhase = 1,

            currentBossHP = bossConfig.bossMaxHP,

            maxBossHP = bossConfig.bossMaxHP,

            isCompleted = false

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

        var bossConfig = guildDatabase.raidBosses.Find(b => b.raidId == raid.raidId);

 

        if (raid.isCompleted)

        {

            Debug.Log("Raid already complete");

            return null;

        }



        if (DateTime.Now > raid.endTime)

        {

            Debug.Log("Raid time expired");

            raid.isCompleted = true;

            return null;

        }

 

        // Simulate combat and calculate damage

        int damageDealt = SimulateRaidCombat(team, bossConfig, raid.currentPhase);

 

        // Record damage

        if (!raid.damageDealt.ContainsKey(playerId))

        {

            raid.damageDealt[playerId] = 0;

        }

        raid.damageDealt[playerId] += damageDealt;



        // Apply damage to boss

        raid.currentBossHP -= damageDealt;



        // Check phase transitions

        CheckPhaseTransition(raid, bossConfig);



        // Check if boss defeated

        bool bossDefeated = raid.currentBossHP <= 0;

        if (bossDefeated)

        {

            raid.isCompleted = true;

            raid.currentBossHP = 0;

            CompleteRaid(raidId);

        }



        return new RaidAttackResult

        {

            damageDealt = damageDealt,

            totalDamage = (int)raid.damageDealt[playerId],

            bossCurrentHP = (int)raid.currentBossHP,

            bossMaxHP = (int)raid.maxBossHP,

            currentPhase = raid.currentPhase,

            bossDefeated = bossDefeated

        };

    }

 

    /// <summary>

    /// Check and trigger phase transitions

    /// </summary>

    private void CheckPhaseTransition(ActiveGuildRaid raid, GuildRaidConfig bossConfig)

    {

        float hpPercent = (float)raid.currentBossHP / raid.maxBossHP;

 

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

        var bossConfig = guildDatabase.raidBosses.Find(b => b.raidId == raid.raidId);

 

        // Sort participants by damage

        var rankings = raid.damageDealt.OrderByDescending(kvp => kvp.Value).ToList();



        // Distribute rewards based on contribution

        for (int i = 0; i < rankings.Count; i++)

        {

            string playerId = rankings[i].Key;

            long damage = rankings[i].Value;

            int rank = i + 1;



            // Calculate rewards (top contributors get bonus)

            var rewards = CalculateRaidRewards(bossConfig, (int)damage, rank);

            DistributeRewards(playerId, rewards);

        }



        // Add guild exp and gold (using fixed values since properties don't exist)

        guild.guildExp += 1000;

        guild.guildGold += 500;



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

            playerIds = new List<string> { player1Id, player2Id },

            startTime = DateTime.Now,

            currentWave = 1,

            isComplete = false,

            allPlayersAlive = true

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

    public List<GuildRaidConfig> GetAvailableRaidBosses(int guildLevel)

    {

        return guildDatabase.raidBosses

            .Where(b => b.minGuildLevel <= guildLevel)

            .ToList();

    }

 

    // Helper methods

    private int SimulateRaidCombat(List<CombatUnit> team, GuildRaidConfig boss, int phase)

    {

        // Simplified combat simulation

        int totalDamage = 0;

        foreach (var unit in team)

        {

            totalDamage += unit.GetAttack() * 10; // Simplified calculation

        }

        return totalDamage;

    }

 

    private List<RaidReward> CalculateRaidRewards(GuildRaidConfig boss, int damage, int rank)

    {

        var rewards = new List<RaidReward>();



        // Use completion rewards as base

        foreach (var reward in boss.completionRewards)

        {

            float multiplier = rank <= 3 ? 1.5f : 1.0f; // Top 3 get bonus

            int quantity = (int)(reward.quantity * multiplier);



            rewards.Add(new RaidReward

            {

                rewardType = reward.rewardType,

                rewardId = reward.rewardId,

                quantity = quantity,

                minRank = reward.minRank,

                maxRank = reward.maxRank

            });

        }



        return rewards;

    }

 

    private void DistributeRewards(string playerId, List<RaidReward> rewards)

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

    public List<GuildRaidConfig> raidBosses = new List<GuildRaidConfig>();

    public List<CoopDungeonConfig> coopDungeons = new List<CoopDungeonConfig>();

    public List<GuildShopItem> guildShopItems = new List<GuildShopItem>();

    public List<GuildBuff> guildBuffs = new List<GuildBuff>();

}