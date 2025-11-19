using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Artifact/Relic system for endgame equipment with unique powerful effects
/// </summary>
[Serializable]
public class ArtifactSaveData
{
    public List<ArtifactInstance> ownedArtifacts = new List<ArtifactInstance>();
    public Dictionary<string, List<string>> heroEquippedArtifacts = new Dictionary<string, List<string>>();  // heroId -> artifact instance IDs
    public Dictionary<string, int> artifactCollectionProgress = new Dictionary<string, int>(); // setId -> pieces owned
}

[Serializable]
public class ArtifactDatabase
{
    public List<ArtifactConfig> artifacts = new List<ArtifactConfig>();
    public List<ArtifactSet> artifactSets = new List<ArtifactSet>();
}

[Serializable]
public class ArtifactConfig
{
    public string artifactId;
    public string artifactName;
    public string description;
    public ArtifactType artifactType;
    public string rarity;                      // legendary, mythic, ancient
    public string setId;                       // Artifact set ID (optional)

    // Visual
    public string icon;
    public string model;                       // 3D model reference

    // Base effects
    public List<ArtifactEffect> baseEffects = new List<ArtifactEffect>();

    // Upgrade system
    public int maxUpgradeLevel;
    public List<ArtifactLevelBonus> levelBonuses = new List<ArtifactLevelBonus>();

    // Acquisition
    public ArtifactSource source;
    public int dropRate;                       // Per 10000
}

public enum ArtifactType
{
    Weapon,                                    // Offensive artifact
    Armor,                                     // Defensive artifact
    Accessory,                                 // Utility artifact
    Special                                    // Unique artifacts
}

[Serializable]
public class ArtifactEffect
{
    public string effectId;
    public string effectName;
    public string description;
    public EffectTrigger trigger;
    public EffectTarget target;

    // Effect parameters
    public string effectType;                  // "stat_boost", "on_death", "on_attack", etc.
    public Dictionary<string, float> parameters = new Dictionary<string, float>();

    // Examples:
    // - "First time this hero dies, revive with 30% HP"
    // - "If team has 3 Infected heroes, all get +10% ATK"
    // - "+15% damage to bosses"
    // - "Heal 5% HP every turn"
}

public enum EffectTrigger
{
    Passive,                                   // Always active
    OnBattleStart,
    OnTurnStart,
    OnAttack,
    OnHit,
    OnKill,
    OnDeath,
    OnCrit,
    OnSkillUse,
    OnUltimate
}

public enum EffectTarget
{
    Self,
    AllAllies,
    RandomAlly,
    AllEnemies,
    RandomEnemy,
    Conditional                                // Based on condition (like element match)
}

[Serializable]
public class ArtifactLevelBonus
{
    public int level;
    public string bonusType;                   // "stat_increase", "new_effect", "effect_upgrade"
    public Dictionary<string, float> bonusValues = new Dictionary<string, float>();
}

[Serializable]
public class ArtifactSource
{
    public string sourceType;                  // "world_boss", "raid", "dungeon", "event", "crafting"
    public string sourceId;                    // Specific boss/dungeon ID
    public string description;
}

/// <summary>
/// Artifact set bonuses
/// </summary>
[Serializable]
public class ArtifactSet
{
    public string setId;
    public string setName;
    public string description;
    public List<string> artifactIds;           // Artifacts in this set

    // Set bonuses (activated when X pieces equipped)
    public Dictionary<int, List<ArtifactEffect>> setBonuses = new Dictionary<int, List<ArtifactEffect>>();
    // Example: 2-piece bonus, 4-piece bonus, 6-piece bonus
}

/// <summary>
/// Player's artifact instance
/// </summary>
[Serializable]
public class ArtifactInstance
{
    public string instanceId;
    public string artifactId;
    public int upgradeLevel;
    public int enhancementLevel;               // Enhancement for stronger effects
    public DateTime obtainedDate;

    // Random rolls/substats (optional)
    public Dictionary<string, float> randomStats = new Dictionary<string, float>();

    // Lock/favorite status
    public bool isLocked;
    public bool isFavorite;
}

/// <summary>
/// Artifact upgrade materials
/// </summary>
[Serializable]
public class ArtifactUpgradeCost
{
    public int level;
    public int goldCost;
    public Dictionary<string, int> materialCosts = new Dictionary<string, int>();  // material ID -> quantity
    public float successRate;                  // Upgrade success chance (1.0 = 100%)
}

/// <summary>
/// Example Artifacts:
///
/// "Phoenix Feather" (Mythic Accessory)
/// - Base: First time this hero dies, revive with 30% HP
/// - Level 5: Revive with 50% HP
/// - Level 10: Revive with 50% HP and cleanse all debuffs
///
/// "Curse of the Infected" (Legendary Special)
/// - Base: If team has 2+ Infected heroes, all Infected get +8% ATK
/// - Level 5: If team has 3+ Infected heroes, all get +10% ATK
/// - Level 10: If team has 3+ Infected heroes, all get +15% ATK and +10% HP
///
/// "Dragon's Might" (Ancient Weapon)
/// - Base: +10% damage to all enemies
/// - Level 5: +15% damage, +5% crit rate
/// - Level 10: +20% damage, +10% crit rate, +25% crit damage
///
/// "Guardian's Oath" (Mythic Armor)
/// - Base: Take 10% less damage when HP > 50%
/// - Level 5: Take 15% less damage when HP > 50%
/// - Level 10: Take 20% less damage, reflect 10% of damage taken
///
/// "Void Crystal" (Ancient Accessory)
/// - Base: Skills ignore 10% of enemy DEF
/// - Level 5: Skills ignore 15% of enemy DEF
/// - Level 10: Skills ignore 20% of enemy DEF, +10% skill damage
///
/// "Eternal Flame Set" (4-piece set)
/// - 2-piece: +10% ATK
/// - 4-piece: On kill, gain +20% ATK for 2 turns (stacks 3 times)
/// </summary>
