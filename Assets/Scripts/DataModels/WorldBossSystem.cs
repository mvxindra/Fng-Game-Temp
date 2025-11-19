using System;

using System.Collections.Generic;

 

/// <summary>

/// World Boss configuration with multi-phase mechanics

/// </summary>

[Serializable]

public class WorldBossConfig

{

    public string bossId;

    public string bossName;

    public string description;

    public string bossType;                         // "dragon", "demon", "titan", "elemental", "undead"

    public string primaryElement;

    public string icon;

    public string modelId;

 

    // Boss stats

    public long bossMaxHP;                          // Shared HP across all players

    public int bossAtk;

    public int bossDef;

    public int bossSpd;

    public float critResist;                        // Boss crit resistance

    public float statusResist;                      // Status effect resistance

 

    // Schedule

    public WorldBossSchedule schedule;

 

    // Phase system

    public List<BossPhase> phases;

 

    // Participation

    public WorldBossRequirements requirements;

    public int energyCost;

    public int maxAttemptsPerPlayer;

 

    // Rewards

    public WorldBossRewardPool rewardPool;

    public List<ExclusiveRaidAffix> exclusiveAffixes;

    public List<ExclusiveCosmeticDrop> exclusiveCosmetics;

 

    // Special mechanics

    public List<BossSpecialMechanic> specialMechanics;

}

 

/// <summary>

/// World boss schedule

/// </summary>

[Serializable]

public class WorldBossSchedule

{

    public List<DayOfWeek> activeDays;              // Days boss is available

    public TimeSpan spawnTime;                      // Time boss spawns

    public int durationMinutes;                     // How long boss is active

    public bool isPermanent;                        // Always available

}

 

/// <summary>

/// Boss phase configuration

/// </summary>

[Serializable]

public class BossPhase

{

    public int phaseNumber;

    public string phaseName;

    public float hpThresholdStart;                  // HP % when phase starts (1.0, 0.7, 0.4, 0.1)

    public float hpThresholdEnd;                    // HP % when phase ends

 

    // Phase transition

    public PhaseTransition transition;

 

    // Abilities gained/lost

    public List<string> gainedAbilities;            // New abilities in this phase

    public List<string> lostAbilities;              // Abilities removed

    public List<BossAbility> phaseAbilities;        // Detailed ability configs

 

    // Stat changes

    public PhaseStatModifiers statModifiers;

 

    // Summons

    public List<PhaseMinion> summonedMinions;

 

    // Buffs/Debuffs

    public List<PhaseBuff> phaseSelfBuffs;          // Boss gains these

    public List<PhaseDebuff> phasePlayerDebuffs;    // Players get these

 

    // Mechanics

    public PhaseMechanic mechanic;

}

 

/// <summary>

/// Phase transition effects

/// </summary>

[Serializable]

public class PhaseTransition

{

    public bool hasTransition;

    public string transitionType;                   // "enrage", "heal", "shield", "invulnerable", "transform"

    public int transitionDuration;                  // Turns or seconds

    public string animationEffect;

    public string announcement;                     // "The dragon enters a rage!"

 

    // Transition effects

    public bool fullHeal;                           // Boss heals to full

    public float healPercent;                       // Heal X% of max HP

    public bool invulnerable;                       // Cannot be damaged during transition

    public long shieldAmount;                       // Gains shield

    public bool clearDebuffs;                       // Clears all debuffs

}

 

/// <summary>

/// Boss ability

/// </summary>

[Serializable]

public class BossAbility

{

    public string abilityId;

    public string abilityName;

    public string description;

    public string abilityType;                      // "aoe", "single_target", "buff", "summon", "mechanic"

 

    // Damage

    public int baseDamage;

    public float atkScaling;

    public string damageType;                       // "physical", "magical", "true"

    public string element;

 

    // Targeting

    public string targetPattern;                    // "all", "random_3", "lowest_hp", "highest_atk"

 

    // Effects

    public List<AbilityStatusEffect> statusEffects;

    public List<AbilitySpecialEffect> specialEffects;

 

    // Cooldown

    public int cooldown;

    public int initialCooldown;                     // Delay before first use

 

    // Conditions

    public AbilityCondition useCondition;           // When boss uses this

}

 

/// <summary>

/// Ability status effect

/// </summary>

[Serializable]

public class AbilityStatusEffect

{

    public string effectType;                       // "stun", "burn", "poison", "bleed", "atk_down", etc.

    public int duration;

    public float chance;

    public int damagePerTurn;

    public float statReduction;

}

 

/// <summary>

/// Ability special effect

/// </summary>

[Serializable]

public class AbilitySpecialEffect

{

    public string effectType;                       // "dispel", "steal_buff", "reduce_cooldown", "extra_turn"

    public int value;

    public string target;

}

 

/// <summary>

/// Ability use condition

/// </summary>

[Serializable]

public class AbilityCondition

{

    public string conditionType;                    // "hp_below", "turn_count", "player_count", "always"

    public float threshold;

    public int turnInterval;

}

 

/// <summary>

/// Phase stat modifiers

/// </summary>

[Serializable]

public class PhaseStatModifiers

{

    public float atkMultiplier;                     // 1.0 = normal, 2.0 = double ATK

    public float defMultiplier;

    public float spdMultiplier;

    public float critRateBonus;

    public float critDamageBonus;

    public float damageReduction;

    public float lifeSteal;

}

 

/// <summary>

/// Phase minion

/// </summary>

[Serializable]

public class PhaseMinion

{

    public string minionId;

    public string minionName;

    public int minionCount;

    public bool respawns;                           // Respawn when killed

    public int respawnTurns;                        // Turns until respawn

    public bool linkedToBoss;                       // Boss invulnerable while minions alive

    public long minionHP;

    public int minionAtk;

}

 

/// <summary>

/// Phase self buff (boss gains)

/// </summary>

[Serializable]

public class PhaseBuff

{

    public string buffId;

    public string buffName;

    public int duration;                            // -1 = permanent for phase

    public float atkBonus;

    public float defBonus;

    public float spdBonus;

    public float critRateBonus;

    public bool counterAttack;

    public bool reflect;

    public float reflectPercent;

}

 

/// <summary>

/// Phase player debuff (applied to all players)

/// </summary>

[Serializable]

public class PhaseDebuff

{

    public string debuffId;

    public string debuffName;

    public int duration;

    public float atkReduction;

    public float defReduction;

    public float healingReduction;

    public bool preventRevive;

    public int damageOverTime;

}

 

/// <summary>

/// Phase mechanic

/// </summary>

[Serializable]

public class PhaseMechanic

{

    public string mechanicType;                     // "enrage_timer", "dps_check", "shield_break", "add_priority", "survival"

    public string description;

    public int timerTurns;                          // Turns until wipe

    public long damageThreshold;                    // Damage needed to pass check

    public bool wipeOnFail;                         // Team wipe if fail mechanic

    public string mechanicReward;                   // Extra reward for passing

}

 

/// <summary>

/// Boss special mechanic (persistent)

/// </summary>

[Serializable]

public class BossSpecialMechanic

{

    public string mechanicId;

    public string mechanicName;

    public string description;

    public string mechanicType;                     // "rage_stack", "phase_skip", "parry", "counter_heal"

 

    // Rage stack example

    public bool stacksOnHit;

    public int maxStacks;

    public float damagePerStack;

 

    // Phase skip

    public bool canSkipPhase;

    public float skipThreshold;                     // If burst damage exceeds this, skip phase

 

    // Counter mechanics

    public bool countersOnBlock;

    public float counterDamageMultiplier;

}

 

/// <summary>

/// World boss requirements

/// </summary>

[Serializable]

public class WorldBossRequirements

{

    public int minPlayerLevel;

    public int minGuildLevel;

    public int recommendedPower;

    public string requiredQuest;                    // Quest that unlocks boss

    public List<string> requiredItems;              // Items needed to enter

}

 

/// <summary>

/// World boss reward pool

/// </summary>

[Serializable]

public class WorldBossRewardPool

{

    // Base rewards (everyone gets)

    public List<WorldBossReward> baseRewards;

 

    // Ranking rewards

    public List<RankingTierReward> personalRankingRewards;

    public List<RankingTierReward> guildRankingRewards;

 

    // Milestone rewards (damage thresholds)

    public List<DamageMilestoneReward> damageMilestones;

 

    // First clear rewards

    public List<WorldBossReward> firstClearRewards;

    public bool firstClearAvailable;

}

 

/// <summary>

/// World boss reward

/// </summary>

[Serializable]

public class WorldBossReward

{

    public string rewardType;                       // "currency", "material", "gear", "cosmetic"

    public string rewardId;

    public int minQuantity;

    public int maxQuantity;

    public float dropChance;

    public bool isGuaranteed;

}

 

/// <summary>

/// Ranking tier reward

/// </summary>

[Serializable]

public class RankingTierReward

{

    public int minRank;

    public int maxRank;

    public int raidCurrency;                        // Exclusive raid currency

    public List<WorldBossReward> rewards;

    public string exclusiveReward;                  // Top rank exclusive

    public float rewardMultiplier;                  // Multiplier on base rewards

}

 

/// <summary>

/// Damage milestone reward

/// </summary>

[Serializable]

public class DamageMilestoneReward

{

    public long damageThreshold;

    public List<WorldBossReward> rewards;

    public bool isOneTime;                          // Only first time reaching this

}

 

/// <summary>

/// Exclusive raid affix (only drops from raids)

/// </summary>

[Serializable]

public class ExclusiveRaidAffix

{

    public string affixId;

    public string affixName;

    public string description;

    public string rarity;                           // "legendary", "mythic"

    public float dropChance;

 

    // Stats

    public Dictionary<string, float> statBonuses;

 

    // Special effect

    public string specialEffect;                    // "raid_damage", "boss_damage", "guild_buff"

    public float effectValue;

}

 

/// <summary>

/// Exclusive cosmetic drop

/// </summary>

[Serializable]

public class ExclusiveCosmeticDrop

{

    public string cosmeticId;

    public string cosmeticName;

    public string cosmeticType;                     // "weapon_skin", "hero_skin", "effect", "title", "portrait"

    public string description;

    public string rarity;

    public float dropChance;

    public bool isGuaranteedAtRank;                 // Top rank always gets this

    public int guaranteedRank;

}

 

/// <summary>

/// Active world boss instance

/// </summary>

[Serializable]

public class ActiveWorldBoss

{

    public string instanceId;

    public string bossId;

    public DateTime spawnTime;

    public DateTime endTime;

    public int currentPhase;

 

    // HP tracking

    public long currentHP;

    public long maxHP;

    public long totalDamageDealt;

 

    // Participation

    public Dictionary<string, PlayerRaidStats> playerStats;         // playerId -> stats

    public Dictionary<string, GuildRaidStats> guildStats;           // guildId -> stats

    public int totalParticipants;

 

    // Status

    public bool isDefeated;

    public bool isExpired;

    public DateTime defeatedTime;

 

    // Phase history

    public List<PhaseTransitionLog> phaseHistory;

}

 

/// <summary>

/// Player raid stats

/// </summary>

[Serializable]

public class PlayerRaidStats

{

    public string playerId;

    public string playerName;

    public string guildId;

    public long totalDamage;

    public int attackCount;

    public int highestSingleHit;

    public long damageThisPhase;

    public int deathCount;

    public bool passedMechanics;

    public DateTime lastAttackTime;

}

 

/// <summary>

/// Guild raid stats

/// </summary>

[Serializable]

public class GuildRaidStats

{

    public string guildId;

    public string guildName;

    public long totalDamage;

    public int memberParticipants;

    public long averageDamagePerMember;

    public int topContributorDamage;

}

 

/// <summary>

/// Phase transition log

/// </summary>

[Serializable]

public class PhaseTransitionLog

{

    public int phaseNumber;

    public DateTime transitionTime;

    public long bossHPAtTransition;

    public string triggerPlayer;                    // Who triggered the phase change

    public int participantsAtTransition;

}

 

/// <summary>

/// Raid shop item

/// </summary>

[Serializable]

public class RaidShopItem

{

    public string itemId;

    public string itemName;

    public string itemType;

    public string description;

    public int raidCurrencyCost;

    public int stock;                               // -1 = unlimited

    public string refreshType;                      // "daily", "weekly", "monthly", "permanent"

    public bool isExclusive;                        // Raid shop exclusive

    public int requiredRaidClears;                  // Must have cleared X raids

}

 

/// <summary>

/// Raid leaderboard entry

/// </summary>

[Serializable]

public class RaidLeaderboardEntry

{

    public int rank;

    public string playerId;

    public string playerName;

    public string guildName;

    public long damage;

    public int attackCount;

    public string topTeamSnapshot;                  // Serialized team composition

}
