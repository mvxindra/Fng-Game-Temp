using System;
using System.Collections.Generic;

[Serializable]
public class TalentNodeConfig
{
    public string nodeId;                    // Unique identifier
    public string nodeName;                  // Display name
    public string description;               // Effect description
    public int tier;                         // Tier level (1-5+)
    public string pathType;                  // "damage", "support", "tank", "hybrid"
    public int maxRanks;                     // Maximum points that can be invested

    // Requirements
    public List<string> prerequisiteNodes;   // Required nodes to unlock this
    public int requiredTalentPoints;         // Total points needed to unlock
    public int requiredAscensionLevel;       // Hero ascension level required

    // Effects (can stack with ranks)
    public TalentEffectConfig effect;

    // Position in tree (for UI)
    public int row;
    public int column;
}

[Serializable]
public class TalentEffectConfig
{
    public string effectType;                // "stat_boost", "skill_unlock", "passive_ability", "skill_modification"

    // Stat boost effects
    public string statType;                  // "ATK", "DEF", "HP", "CRIT", "CRIT_DMG", "SPD", "ACC", "RES"
    public float valuePerRank;               // Value added per rank (can be flat or percentage)
    public bool isPercentage;                // True for percentage, false for flat

    // Skill effects
    public string skillId;                   // Skill to unlock or modify
    public string modificationType;          // "cooldown_reduction", "damage_increase", "range_increase", "add_effect"
    public float modificationValue;

    // Passive ability
    public string passiveId;                 // ID of passive ability to unlock
    public PassiveAbilityConfig passiveAbility;

    // Status effect modifications
    public string statusId;                  // Status effect to apply
    public int statusDuration;
    public float statusChance;               // Chance to apply (0-1)
}

[Serializable]
public class PassiveAbilityConfig
{
    public string abilityId;
    public string abilityName;
    public string description;
    public string triggerType;               // "on_attack", "on_damaged", "on_kill", "start_of_turn", "low_hp"
    public float triggerChance;              // 0-1
    public List<PassiveEffectConfig> effects;
}

[Serializable]
public class PassiveEffectConfig
{
    public string effectType;                // "heal", "damage", "buff", "debuff", "summon", "counter"
    public string targetType;                // "self", "ally", "enemy", "all_enemies", "all_allies"
    public float value;
    public string scalingStat;               // Stat to scale from
    public float multiplier;
    public string statusId;                  // For buff/debuff effects
    public int duration;
}
