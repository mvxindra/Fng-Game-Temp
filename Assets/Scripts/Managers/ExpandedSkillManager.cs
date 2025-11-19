using System;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using FnGMafia.Core;

 

/// <summary>

/// Manager for expanded skill system with synergies and upgrades

/// </summary>

public class ExpandedSkillManager : Singleton<ExpandedSkillManager>

{

    private ExpandedSkillDatabase skillDatabase;

    private Dictionary<string, HeroSkillLoadout> heroSkillLoadouts = new Dictionary<string, HeroSkillLoadout>();

 

    protected override void Awake()

    {

        base.Awake();

        LoadSkillDatabase();

    }

 

    private void LoadSkillDatabase()

    {

        TextAsset skillData = Resources.Load<TextAsset>("Config/expanded_skills");

        if (skillData != null)

        {

            skillDatabase = JsonUtility.FromJson<ExpandedSkillDatabase>(skillData.text);

            Debug.Log($"Loaded {skillDatabase.skills.Count} skills");

        }

        else

        {

            Debug.LogError("Failed to load expanded_skills.json");

            skillDatabase = new ExpandedSkillDatabase();

        }

    }

 

    /// <summary>

    /// Get skill configuration by ID

    /// </summary>

    public ExpandedSkillConfig GetSkill(string skillId)

    {

        return skillDatabase.skills.Find(s => s.skillId == skillId);

    }

 

    /// <summary>

    /// Get hero's skill loadout

    /// </summary>

    public HeroSkillLoadout GetHeroSkillLoadout(string heroId)

    {

        if (!heroSkillLoadouts.ContainsKey(heroId))

        {

            // Initialize default loadout

            heroSkillLoadouts[heroId] = new HeroSkillLoadout

            {

                heroId = heroId,

                activeSkills = new List<string>(),

                passiveSkills = new List<string>(),

                ultimateSkill = "",

                skillLevels = new Dictionary<string, int>()

            };

        }

        return heroSkillLoadouts[heroId];

    }

 

    /// <summary>

    /// Upgrade a skill using materials

    /// </summary>

    public bool UpgradeSkill(string heroId, string skillId, bool useDuplicate = false)

    {

        var loadout = GetHeroSkillLoadout(heroId);

        var skill = GetSkill(skillId);

 

        if (skill == null)

        {

            Debug.LogError($"Skill {skillId} not found");

            return false;

        }

 

        int currentLevel = loadout.skillLevels.ContainsKey(skillId) ? loadout.skillLevels[skillId] : 1;

 

        if (currentLevel >= skill.upgradeConfig.maxLevel)

        {

            Debug.Log($"Skill {skillId} is already at max level");

            return false;

        }

 

        var upgradeConfig = skill.upgradeConfig;

 

        // Check materials

        if (useDuplicate)

        {

            // Use hero duplicate (check inventory elsewhere)

            if (!HasHeroDuplicate(heroId))

            {

                Debug.Log("No hero duplicate available");

                return false;

            }

        }

        else

        {

            // Use skill tomes

            if (!HasRequiredMaterials(upgradeConfig.baseRequirements.materials, currentLevel))

            {

                Debug.Log("Insufficient materials");

                return false;

            }

        }

 

        // Consume materials and upgrade

        if (useDuplicate)

        {

            ConsumeHeroDuplicate(heroId);

        }

        else

        {

            ConsumeMaterials(upgradeConfig.baseRequirements.materials, currentLevel);

        }

 

        loadout.skillLevels[skillId] = currentLevel + 1;

        Debug.Log($"Upgraded {skillId} to level {currentLevel + 1}");

        return true;

    }

 

    /// <summary>

    /// Calculate skill damage with synergies

    /// </summary>

    public float CalculateSkillDamage(ExpandedSkillConfig skill, CombatUnit caster, CombatUnit target, int skillLevel = 1)

    {

        // Use damageMultiplier from skill config

        float baseDamage = 0f;



        // Apply level scaling (use a default value per level)

        float levelBonus = 10f * (skillLevel - 1);

        baseDamage += levelBonus;



        // Apply caster stats

        baseDamage += caster.GetCurrentATK() * skill.damageMultiplier;

 

        // Apply synergies

        float synergyMultiplier = CalculateSynergyMultiplier(skill, caster, target);

 

        return baseDamage * synergyMultiplier;

    }

 

    /// <summary>

    /// Calculate synergy multiplier for skill

    /// </summary>

    public float CalculateSynergyMultiplier(ExpandedSkillConfig skill, CombatUnit caster, CombatUnit target)

    {

        float multiplier = 1.0f;

 

        foreach (var synergy in skill.synergies)

        {

            bool conditionMet = false;

 

            switch (synergy.conditionType)

            {

                case "enemy_status":

                    // TODO: Implement status effect checking

                    conditionMet = false;

                    break;



                case "ally_buff":

                    // TODO: Implement status effect checking

                    conditionMet = false;

                    break;

 

                case "hp_threshold":

                    float hpPercent = (float)target.GetCurrentHP() / target.GetMaxHP();

                    float threshold = float.Parse(synergy.requiredStatus);

                    conditionMet = hpPercent <= threshold;

                    break;

 

                case "combo":

                    // Check if required skill was used recently (tracked elsewhere)

                    conditionMet = WasSkillUsedRecently(synergy.requiredStatus);

                    break;

            }

 

            if (conditionMet)

            {

                switch (synergy.bonusType)

                {

                    case "damage":

                        multiplier *= (1.0f + synergy.bonusMultiplier);

                        break;

                    case "crit_chance":

                        // Handle separately in combat calculation

                        break;

                    case "additional_effect":

                        // Handle in effect application

                        break;

                }

            }

        }

 

        return multiplier;

    }

 

    /// <summary>

    /// Check if skill combo condition is met

    /// </summary>

    public bool CheckComboCondition(SkillCombo combo, List<string> recentSkillsUsed)

    {

        if (recentSkillsUsed.Count < combo.requiredSkills.Count)

            return false;

 

        var lastSkills = recentSkillsUsed.TakeLast(combo.requiredSkills.Count).ToList();

 

        for (int i = 0; i < combo.requiredSkills.Count; i++)

        {

            if (lastSkills[i] != combo.requiredSkills[i])

                return false;

        }

 

        return true;

    }

 

    /// <summary>

    /// Get ultimate skill charge requirement

    /// </summary>

    public int GetUltimateChargeRequirement(string skillId)

    {

        var skill = GetSkill(skillId);

        if (skill == null || skill.skillType != "ultimate")

            return 100;



        // TODO: Add ultimateConfig to ExpandedSkillConfig or use a default value

        return 100;

    }

 

    /// <summary>

    /// Check if hero can use ultimate

    /// </summary>

    public bool CanUseUltimate(string heroId, int currentCharge)

    {

        var loadout = GetHeroSkillLoadout(heroId);

        if (string.IsNullOrEmpty(loadout.ultimateSkill))

            return false;



        int required = GetUltimateChargeRequirement(loadout.ultimateSkill);

        return currentCharge >= required;

    }

 

    /// <summary>

    /// Get all passive skill effects for a hero

    /// </summary>

    public List<ExpandedSkillConfig> GetActivePassiveSkills(string heroId)

    {

        var loadout = GetHeroSkillLoadout(heroId);

        var passives = new List<ExpandedSkillConfig>();

 

        foreach (var passiveId in loadout.passiveSkills)

        {

            var skill = GetSkill(passiveId);

            if (skill != null && skill.skillType == "passive")

            {

                passives.Add(skill);

            }

        }

 

        return passives;

    }

 

    /// <summary>

    /// Apply passive skill bonuses to combat unit

    /// </summary>

    public void ApplyPassiveBonuses(CombatUnit unit, string heroId)

    {

        var passives = GetActivePassiveSkills(heroId);

 

        foreach (var passive in passives)

        {

            int skillLevel = GetSkillLevel(heroId, passive.skillId);

 

            // Apply stat bonuses

            foreach (var effect in passive.effects)

            {

                float value = effect.multiplier;



                switch (effect.type)

                {

                    case "Buff":

                    case "Debuff":

                        // TODO: Implement stat modification in CombatUnit

                        Debug.Log($"Passive skill would apply {effect.type} with multiplier {value}");

                        break;

                    default:

                        // Other effects handled elsewhere

                        break;

                }

            }

        }

    }

 

    /// <summary>

    /// Get skill level for a hero

    /// </summary>

    public int GetSkillLevel(string heroId, string skillId)

    {

        var loadout = GetHeroSkillLoadout(heroId);

        return loadout.skillLevels.ContainsKey(skillId) ? loadout.skillLevels[skillId] : 1;

    }

 

    /// <summary>

    /// Equip a skill to hero's loadout

    /// </summary>

    public bool EquipSkill(string heroId, string skillId)

    {

        var loadout = GetHeroSkillLoadout(heroId);

        var skill = GetSkill(skillId);

 

        if (skill == null)

        {

            Debug.LogError($"Skill {skillId} not found");

            return false;

        }

 

        switch (skill.skillType)

        {

            case "active":

                if (loadout.activeSkills.Count >= 4)

                {

                    Debug.Log("Active skill slots full");

                    return false;

                }

                if (!loadout.activeSkills.Contains(skillId))

                {

                    loadout.activeSkills.Add(skillId);

                }

                break;

 

            case "passive":

                if (loadout.passiveSkills.Count >= 3)

                {

                    Debug.Log("Passive skill slots full");

                    return false;

                }

                if (!loadout.passiveSkills.Contains(skillId))

                {

                    loadout.passiveSkills.Add(skillId);

                }

                break;

 

            case "ultimate":

                loadout.ultimateSkill = skillId;

                break;

        }

 

        Debug.Log($"Equipped {skill.skillType} skill {skillId} to {heroId}");

        return true;

    }

 

    /// <summary>

    /// Unequip a skill from hero's loadout

    /// </summary>

    public bool UnequipSkill(string heroId, string skillId)

    {

        var loadout = GetHeroSkillLoadout(heroId);

 

        if (loadout.activeSkills.Contains(skillId))

        {

            loadout.activeSkills.Remove(skillId);

            return true;

        }

 

        if (loadout.passiveSkills.Contains(skillId))

        {

            loadout.passiveSkills.Remove(skillId);

            return true;

        }

 

        if (loadout.ultimateSkill == skillId)

        {

            loadout.ultimateSkill = "";

            return true;

        }

 

        return false;

    }

 

    // Helper methods (would connect to inventory/resource systems)

    private bool HasHeroDuplicate(string heroId)

    {

        // TODO: Check player inventory for hero duplicate

        return true; // Placeholder

    }

 

    private void ConsumeHeroDuplicate(string heroId)

    {

        // TODO: Remove duplicate from inventory

        Debug.Log($"Consumed duplicate of {heroId}");

    }

 

    private bool HasRequiredMaterials(List<MaterialRequirement> materials, int level)

    {

        // TODO: Check player resources

        return true; // Placeholder

    }

 

    private void ConsumeMaterials(List<MaterialRequirement> materials, int level)

    {

        // TODO: Deduct materials from player resources

        Debug.Log($"Consumed skill upgrade materials for level {level}");

    }

 

    private bool WasSkillUsedRecently(string skillId)

    {

        // TODO: Track recently used skills in combat

        return false; // Placeholder

    }

}

 

