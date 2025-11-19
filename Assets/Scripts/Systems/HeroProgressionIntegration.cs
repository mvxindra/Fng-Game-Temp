using UnityEngine;

using System.Collections.Generic;

using FnGMafia.Core;

 

/// <summary>

/// Integrates all hero progression systems with combat and hero systems

/// </summary>

public class HeroProgressionIntegration : Singleton<HeroProgressionIntegration>

{

    protected override void Awake()

    {

        base.Awake();

    }

 

    /// <summary>

    /// Apply all progression bonuses to a CombatUnit

    /// This should be called when initializing a combat unit

    /// </summary>

    public void ApplyProgressionToCombatUnit(CombatUnit unit, string heroId, int heroLevel = 1)

    {

        if (unit == null || string.IsNullOrEmpty(heroId))

        {

            return;

        }

 

        // 1. Apply Ascension bonuses

        ApplyAscensionBonuses(unit, heroId);

 

        // 2. Apply Talent Tree bonuses

        ApplyTalentBonuses(unit, heroId);

 

        // 3. Apply Elemental Affinity bonuses

        ApplyElementalBonuses(unit, heroId);

 

        // 4. Add unlocked skills from talents and ascension

        AddProgressionSkills(unit, heroId);

 

        // 5. Initialize hero progression if needed

        InitializeHeroProgression(heroId, heroLevel);

    }

 

    /// <summary>

    /// Apply ascension stat bonuses to combat unit

    /// </summary>

    private void ApplyAscensionBonuses(CombatUnit unit, string heroId)

    {

        if (HeroAscensionManager.Instance == null) return;

 

        AscensionStatBonus bonuses = HeroAscensionManager.Instance.GetAscensionStatBonuses(heroId);

        if (bonuses == null) return;

 

        // Apply flat bonuses

        unit.BaseATK += Mathf.RoundToInt(bonuses.atkBonus);

        unit.BaseDEF += Mathf.RoundToInt(bonuses.defBonus);

        unit.BaseHP += Mathf.RoundToInt(bonuses.hpBonus);

        unit.BaseSPD += Mathf.RoundToInt(bonuses.spdBonus);

 

        // Apply percentage bonuses

        unit.BaseATK = Mathf.RoundToInt(unit.BaseATK * (1f + bonuses.atkPercent));

        unit.BaseDEF = Mathf.RoundToInt(unit.BaseDEF * (1f + bonuses.defPercent));

        unit.BaseHP = Mathf.RoundToInt(unit.BaseHP * (1f + bonuses.hpPercent));

        unit.BaseSPD = Mathf.RoundToInt(unit.BaseSPD * (1f + bonuses.spdPercent));

 

        // Update current HP to match new max

        unit.CurrentHP = unit.BaseHP;

 

        Debug.Log($"Applied ascension bonuses to {heroId}: ATK+{bonuses.atkBonus}(+{bonuses.atkPercent * 100}%), DEF+{bonuses.defBonus}, HP+{bonuses.hpBonus}");

    }

 

    /// <summary>

    /// Apply talent tree stat bonuses to combat unit

    /// </summary>

    private void ApplyTalentBonuses(CombatUnit unit, string heroId)

    {

        if (TalentTreeManager.Instance == null) return;

 

        Dictionary<string, float> bonuses = TalentTreeManager.Instance.GetTalentStatBonuses(heroId);

        if (bonuses == null || bonuses.Count == 0) return;

 

        foreach (var bonus in bonuses)

        {

            string statType = bonus.Key;

            float value = bonus.Value;

 

            switch (statType)

            {

                case "ATK":

                    unit.BaseATK += Mathf.RoundToInt(value);

                    break;

                case "DEF":

                    unit.BaseDEF += Mathf.RoundToInt(value);

                    break;

                case "HP":

                    unit.BaseHP += Mathf.RoundToInt(value);

                    unit.CurrentHP = unit.BaseHP;

                    break;

                case "SPD":

                    unit.BaseSPD += Mathf.RoundToInt(value);

                    break;

            }

        }

 

        Debug.Log($"Applied talent bonuses to {heroId}");

    }

 

    /// <summary>

    /// Apply elemental affinity stat bonuses

    /// </summary>

    private void ApplyElementalBonuses(CombatUnit unit, string heroId)

    {

        if (ElementalAffinityManager.Instance == null) return;

 

        HeroElementalAffinity affinity = ElementalAffinityManager.Instance.GetHeroAffinity(heroId);

        if (affinity == null || string.IsNullOrEmpty(affinity.primaryElement)) return;

 

        ElementalStats elementBonus = ElementalAffinityManager.Instance.GetElementalStatBonus(affinity.primaryElement);

        if (elementBonus == null) return;

 

        // Apply elemental stat bonuses

        unit.BaseATK = Mathf.RoundToInt(unit.BaseATK * (1f + elementBonus.atkBonus));

        unit.BaseDEF = Mathf.RoundToInt(unit.BaseDEF * (1f + elementBonus.defBonus));

        unit.BaseHP = Mathf.RoundToInt(unit.BaseHP * (1f + elementBonus.hpBonus));

        unit.BaseSPD = Mathf.RoundToInt(unit.BaseSPD * (1f + elementBonus.spdBonus));

        unit.CurrentHP = unit.BaseHP;

 

        Debug.Log($"Applied elemental bonuses ({affinity.primaryElement}) to {heroId}");

    }

 

    /// <summary>

    /// Add skills unlocked through progression systems

    /// </summary>

    private void AddProgressionSkills(CombatUnit unit, string heroId)

    {

        List<string> unlockedSkills = new List<string>();

 

        // Get skills from talent tree

        if (TalentTreeManager.Instance != null)

        {

            unlockedSkills.AddRange(TalentTreeManager.Instance.GetUnlockedTalentSkills(heroId));

        }

 

        // Get skills from ascension

        if (HeroAscensionManager.Instance != null)

        {

            unlockedSkills.AddRange(HeroAscensionManager.Instance.GetUnlockedAscensionSkills(heroId));

        }

 

        // Add skills to combat unit

        foreach (string skillId in unlockedSkills)

        {

            // Check if evolved

            string finalSkillId = skillId;

            if (HeroAscensionManager.Instance != null)

            {

                finalSkillId = HeroAscensionManager.Instance.GetEvolvedSkill(heroId, skillId);

            }

 

            // Check if already has this skill

            bool hasSkill = false;

            foreach (var skill in unit.Skills)

            {

                if (skill.Config.id == finalSkillId)

                {

                    hasSkill = true;

                    break;

                }

            }

 

            if (!hasSkill)

            {

                var skillConfig = SkillDatabase.Instance?.GetSkill(finalSkillId);

                if (skillConfig != null)

                {

                    unit.Skills.Add(new SkillInstance(skillConfig));

                    Debug.Log($"Added progression skill {finalSkillId} to {heroId}");

                }

            }

        }

    }

 

    /// <summary>

    /// Initialize all progression systems for a hero

    /// </summary>

    private void InitializeHeroProgression(string heroId, int heroLevel)

    {

        // Initialize talent tree

        if (TalentTreeManager.Instance != null)

        {

            TalentTreeManager.Instance.InitializeHeroTalents(heroId, heroLevel, 0);

        }

 

        // Initialize ascension

        if (HeroAscensionManager.Instance != null)

        {

            HeroAscensionManager.Instance.InitializeHeroAscension(heroId, heroLevel);

        }

 

        // Initialize quests

        if (HeroQuestManager.Instance != null)

        {

            HeroQuestManager.Instance.InitializeHeroQuests(heroId);

        }

    }

 

    /// <summary>

    /// Calculate damage with elemental multiplier

    /// Call this in damage calculation

    /// </summary>

    public int CalculateDamageWithElemental(CombatUnit attacker, CombatUnit defender, int baseDamage)

    {

        if (ElementalAffinityManager.Instance == null)

        {

            return baseDamage;

        }

 

        float elementalMultiplier = ElementalAffinityManager.Instance.GetElementalMultiplier(

            attacker.heroId,

            defender.heroId

        );

 

        return Mathf.RoundToInt(baseDamage * elementalMultiplier);

    }

 

    /// <summary>

    /// Get talent tree passives for a hero

    /// </summary>

    public List<PassiveAbilityConfig> GetTalentPassives(string heroId)

    {

        if (TalentTreeManager.Instance == null)

        {

            return new List<PassiveAbilityConfig>();

        }

 

        return TalentTreeManager.Instance.GetUnlockedTalentPassives(heroId);

    }

 

    /// <summary>

    /// When hero levels up, grant talent points and check for auto-unlocks

    /// </summary>

    public void OnHeroLevelUp(string heroId, int newLevel)

    {

        // Level up in ascension system (which also grants talent points)

        if (HeroAscensionManager.Instance != null)

        {

            HeroAscensionManager.Instance.LevelUpHero(heroId, 1);

        }

 

        Debug.Log($"Hero {heroId} leveled up to {newLevel}");

    }

 

    /// <summary>

    /// When a battle ends, track quest progress

    /// </summary>

    public void OnBattleComplete(string heroId, string stageId, List<string> defeatedEnemies, bool victory)

    {

        if (!victory || HeroQuestManager.Instance == null)

        {

            return;

        }

 

        // Track stage completion

        HeroQuestManager.Instance.OnStageCompleted(heroId, stageId);

 

        // Track enemy defeats

        foreach (string enemyId in defeatedEnemies)

        {

            HeroQuestManager.Instance.OnEnemyDefeated(heroId, enemyId);

        }

    }

 

    /// <summary>

    /// When a skill is used, track for quests

    /// </summary>

    public void OnSkillUsed(string heroId, string skillId)

    {

        if (HeroQuestManager.Instance != null)

        {

            HeroQuestManager.Instance.OnSkillUsed(heroId, skillId);

        }

    }

 

    /// <summary>

    /// Get complete hero progression summary

    /// </summary>

    public HeroProgressionSummary GetProgressionSummary(string heroId)

    {

        HeroProgressionSummary summary = new HeroProgressionSummary

        {

            heroId = heroId

        };

 

        // Ascension info

        if (HeroAscensionManager.Instance != null)

        {

            var ascProgress = HeroAscensionManager.Instance.GetHeroAscensionProgress(heroId);

            if (ascProgress != null)

            {

                summary.ascensionTier = ascProgress.currentTier;

                summary.level = ascProgress.currentLevel;

                summary.levelCap = ascProgress.levelCap;

            }

        }

 

        // Talent info

        if (TalentTreeManager.Instance != null)

        {

            var talentProgress = TalentTreeManager.Instance.GetHeroTalentProgress(heroId);

            if (talentProgress != null)

            {

                summary.availableTalentPoints = talentProgress.GetAvailablePoints();

                summary.spentTalentPoints = talentProgress.spentTalentPoints;

            }

        }

 

        // Elemental info

        if (ElementalAffinityManager.Instance != null)

        {

            var affinity = ElementalAffinityManager.Instance.GetHeroAffinity(heroId);

            if (affinity != null)

            {

                summary.primaryElement = affinity.primaryElement;

                summary.secondaryElement = affinity.secondaryElement;

                summary.elementalMastery = affinity.elementalMastery;

            }

        }

 

        // Quest info

        if (HeroQuestManager.Instance != null)

        {

            var questProgress = HeroQuestManager.Instance.GetHeroQuestProgress(heroId);

            if (questProgress != null)

            {

                summary.completedQuestCount = questProgress.completedQuests.Count;

                summary.activeQuestCount = questProgress.activeQuests.Count;

            }

        }

 

        return summary;

    }

}

 

/// <summary>

/// Summary of all hero progression

/// </summary>

public class HeroProgressionSummary

{

    public string heroId;

 

    // Ascension

    public int ascensionTier;

    public int level;

    public int levelCap;

 

    // Talents

    public int availableTalentPoints;

    public int spentTalentPoints;

 

    // Elemental

    public string primaryElement;

    public string secondaryElement;

    public int elementalMastery;

 

    // Quests

    public int completedQuestCount;

    public int activeQuestCount;

}