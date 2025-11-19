using System;

using System.Collections.Generic;

 

[Serializable]

public class AscensionTierConfig

{

    public int tier;                              // Ascension tier (1-10+)

    public int requiredLevel;                     // Hero level required

    public int newLevelCap;                       // New max level after ascension

    public List<MaterialRequirement> materials;    // Materials needed

    public int goldCost;

 

    // Stat bonuses

    public AscensionStatBonus statBonuses;

 

    // Unlocks

    public List<string> unlockedSkills;           // New skills unlocked

    public List<string> unlockedPassives;         // Passive abilities unlocked

    public int talentPointsGranted;               // Bonus talent points

    public bool unlocksUltimateSkill;             // Special ultimate skill unlock

 

    // Special features

    public string specialFeature;                 // e.g., "skill_evolution", "stat_break", "dual_element"

    public AscensionSpecialFeature specialConfig;

}

 

[Serializable]

public class AscensionStatBonus

{

    public float atkBonus;          // Flat ATK increase

    public float defBonus;          // Flat DEF increase

    public float hpBonus;           // Flat HP increase

    public float spdBonus;          // Flat SPD increase

 

    public float atkPercent;        // % ATK increase

    public float defPercent;        // % DEF increase

    public float hpPercent;         // % HP increase

    public float spdPercent;        // % SPD increase

 

    public float critBonus;         // CRIT chance increase

    public float critDmgBonus;      // CRIT damage increase

    public float accBonus;          // Accuracy increase

    public float resBonus;          // Resistance increase

}

 

[Serializable]

public class AscensionSpecialFeature

{

    public string featureType;      // "skill_evolution", "stat_break", "dual_element", "passive_upgrade"

 

    // Skill evolution

    public string baseSkillId;      // Skill to evolve

    public string evolvedSkillId;   // Evolved version

 

    // Stat break (exceed normal caps)

    public string statType;         // Which stat can exceed cap

    public float newCap;            // New cap value

 

    // Dual element

    public string secondaryElement; // Adds second element affinity

 

    // Passive upgrade

    public string passiveId;

    public string upgradedPassiveId;

}

 

[Serializable]

public class MaterialRequirement

{

    public string materialId;

    public int quantity;

}

 

[Serializable]

public class HeroAscensionConfig

{

    public string heroId;

    public List<AscensionTierConfig> tiers;

    public int maxAscensionTier;

}