using System;
using System.Collections.Generic;

/// <summary>
/// Expanded skill system with Active/Passive/Ultimate types and synergies
/// </summary>
[Serializable]
public class ExpandedSkillConfig
{
    public string skillId;
    public string skillName;
    public string description;
    public string skillType;                    // "active", "passive", "ultimate"
    public string skillCategory;                // "damage", "support", "debuff", "buff", "heal"

    // Skill requirements
    public int unlockLevel;                     // Level required to unlock
    public int heroAscension;                   // Ascension tier required
    public string prerequisiteSkill;            // Skill that must be unlocked first

    // Skill properties
    public int baseCooldown;                    // Turns between uses
    public int energyCost;                      // Energy/mana cost
    public string targetType;                   // "single", "aoe", "all_enemies", "all_allies", "self"
    public int range;                           // Range in units

    // Damage/Healing
    public float damageMultiplier;              // % of ATK/Magic
    public string scalingStat;                  // "ATK", "MAGIC", "HP", "DEF"
    public float healMultiplier;                // % of stat for healing
    public bool canCrit;                        // Can this skill crit?
    public float critDamageBonus;               // Bonus crit damage

    // Effects
    public List<SkillEffectConfig> effects;

    // Synergies
    public List<SkillSynergy> synergies;

    // Upgrade system
    public SkillUpgradeConfig upgradeConfig;

    // Animation/VFX
    public string animationId;
    public string vfxId;
    public float castTime;                      // Seconds to cast
}

/// <summary>
/// Skill synergy conditions and bonuses
/// </summary>
[Serializable]
public class SkillSynergy
{
    public string synergyId;
    public string synergyName;
    public string description;

    // Condition
    public string conditionType;                // "enemy_status", "ally_buff", "combo", "hp_threshold"
    public string requiredStatus;               // Status enemy/ally must have
    public string requiredSkill;                // Skill that must be used before
    public float hpThreshold;                   // HP % threshold
    public int comboCount;                      // Number of hits in combo

    // Bonus
    public string bonusType;                    // "damage", "cooldown", "additional_effect", "guaranteed_crit"
    public float bonusMultiplier;               // Multiplier for bonus
    public string additionalEffect;             // Extra effect to apply
    public int bonusDuration;                   // Duration of bonus
}

/// <summary>
/// Skill upgrade configuration
/// </summary>
[Serializable]
public class SkillUpgradeConfig
{
    public int maxLevel;                        // Maximum skill level
    public List<SkillLevelBonus> levelBonuses;

    // Upgrade costs
    public UpgradeRequirements baseRequirements;
    public float costScaling;                   // Cost multiplier per level
}

/// <summary>
/// Bonuses per skill level
/// </summary>
[Serializable]
public class SkillLevelBonus
{
    public int level;
    public float damageIncrease;                // +% damage per level
    public float cooldownReduction;             // -turns cooldown
    public int energyCostReduction;             // -energy cost
    public string unlockedEffect;               // Effect unlocked at this level
    public float synergyBonus;                  // Synergy multiplier increase
}

/// <summary>
/// Requirements to upgrade skill
/// </summary>
[Serializable]
public class UpgradeRequirements
{
    public int heroDuplicates;                  // Number of hero dupes needed
    public int skillTomes;                      // Skill tomes needed
    public int gold;
    public List<MaterialRequirement> materials;
}

/// <summary>
/// Passive skill configuration
/// </summary>
[Serializable]
public class PassiveSkillConfig
{
    public string passiveId;
    public string passiveName;
    public string description;
    public string passiveType;                  // "always", "conditional", "trigger"

    // Trigger conditions
    public string triggerCondition;             // "on_attack", "on_damaged", "start_turn", "below_hp", "on_kill"
    public float triggerChance;                 // Chance to trigger (0-1)
    public float triggerThreshold;              // HP threshold or other value

    // Effects
    public List<PassiveEffectConfig> effects;
    public int effectDuration;                  // Duration in turns (0 = permanent)
    public int cooldown;                        // Cooldown between triggers
}

/// <summary>
/// Ultimate skill configuration
/// </summary>
[Serializable]
public class UltimateSkillConfig
{
    public string ultimateId;
    public string ultimateName;
    public string description;

    // Charge system
    public int maxCharge;                       // Max charge points
    public int chargePerTurn;                   // Charge gained per turn
    public int chargeOnHit;                     // Charge on hitting enemy
    public int chargeOnDamaged;                 // Charge when damaged
    public int chargeOnKill;                    // Charge on killing enemy

    // Ultimate effects
    public bool isTransformation;               // Does this transform the hero?
    public int transformDuration;               // Duration of transformation
    public List<SkillEffectConfig> effects;
    public List<UltimateBonus> transformBonuses;

    // Animation
    public string cutsceneId;                   // Cutscene animation
    public float ultimateDuration;              // Duration in seconds
}

/// <summary>
/// Bonuses during ultimate transformation
/// </summary>
[Serializable]
public class UltimateBonus
{
    public string statType;                     // "ATK", "DEF", "SPD", "HP"
    public float multiplier;                    // Stat multiplier
    public string specialAbility;               // Special ability gained
    public bool invulnerable;                   // Invulnerable during ultimate?
}

/// <summary>
/// Runtime skill instance with upgrade level
/// </summary>
[Serializable]
public class SkillInstance
{
    public string skillId;
    public int skillLevel;                      // Current upgrade level
    public int currentCooldown;                 // Remaining cooldown
    public bool isUnlocked;

    // For ultimates
    public int currentCharge;                   // Current ultimate charge
    public bool isUltimateReady;

    public SkillInstance(string id)
    {
        skillId = id;
        skillLevel = 1;
        currentCooldown = 0;
        isUnlocked = true;
        currentCharge = 0;
        isUltimateReady = false;
    }
}

/// <summary>
/// Hero skill loadout
/// </summary>
[Serializable]
public class HeroSkillLoadout
{
    public string heroId;
    public List<string> activeSkills;           // Up to 4 active skills
    public List<string> passiveSkills;          // Passive skills
    public string ultimateSkill;                // Ultimate skill

    // Skill levels
    public Dictionary<string, int> skillLevels;

    public HeroSkillLoadout()
    {
        activeSkills = new List<string>();
        passiveSkills = new List<string>();
        skillLevels = new Dictionary<string, int>();
    }
}

/// <summary>
/// Skill combo system
/// </summary>
[Serializable]
public class SkillCombo
{
    public string comboId;
    public string comboName;
    public string description;
    public List<string> requiredSkills;         // Skills that must be used in order
    public int comboWindow;                     // Turns to complete combo

    // Combo bonus
    public float comboDamageBonus;
    public string comboEffect;                  // Special effect when combo completes
    public List<SkillEffectConfig> bonusEffects;
}

/// <summary>
/// Database for expanded skills
/// </summary>
[Serializable]
public class ExpandedSkillDatabase
{
    public List<ExpandedSkillConfig> skills;
    public List<PassiveSkillConfig> passives;
    public List<UltimateSkillConfig> ultimates;
    public List<SkillCombo> combos;
}
