using UnityEngine;

using System.Collections.Generic;

using System.Linq;

using FnGMafia.Core;

 

public class HeroAscensionManager : Singleton<HeroAscensionManager>

{

    private Dictionary<string, HeroAscensionProgress> heroAscensionProgress;

    private Dictionary<string, HeroAscensionConfig> ascensionConfigs;

 

    protected override void Awake()

    {

        base.Awake();

        heroAscensionProgress = new Dictionary<string, HeroAscensionProgress>();

        ascensionConfigs = new Dictionary<string, HeroAscensionConfig>();

        LoadAscensionConfigs();

    }

 

    private void LoadAscensionConfigs()

    {

        TextAsset configFile = Resources.Load<TextAsset>("Config/hero_ascension");

        if (configFile != null)

        {

            HeroAscensionDatabase database = JsonUtility.FromJson<HeroAscensionDatabase>(configFile.text);

            foreach (var config in database.ascensions)

            {

                ascensionConfigs[config.heroId] = config;

            }

            Debug.Log($"Loaded {ascensionConfigs.Count} hero ascension configurations");

        }

        else

        {

            Debug.LogWarning("hero_ascension.json not found in Resources/Config");

        }

    }

 

    // Initialize hero ascension progress

    public void InitializeHeroAscension(string heroId, int startingLevel = 1)

    {

        if (!heroAscensionProgress.ContainsKey(heroId))

        {

            HeroAscensionProgress progress = new HeroAscensionProgress

            {

                heroId = heroId,

                currentLevel = startingLevel,

                currentTier = 0,

                levelCap = 50 // Default starting cap

            };

            heroAscensionProgress[heroId] = progress;

        }

    }

 

    // Ascend a hero to the next tier

    public bool AscendHero(string heroId)

    {

        if (!heroAscensionProgress.ContainsKey(heroId))

        {

            InitializeHeroAscension(heroId);

        }

 

        HeroAscensionProgress progress = heroAscensionProgress[heroId];

        HeroAscensionConfig config = GetAscensionConfig(heroId);

 

        if (config == null)

        {

            Debug.LogError($"No ascension config found for hero {heroId}");

            return false;

        }

 

        int nextTier = progress.currentTier + 1;

 

        if (nextTier > config.maxAscensionTier)

        {

            Debug.LogWarning($"Hero {heroId} is already at max ascension tier");

            return false;

        }

 

        AscensionTierConfig tierConfig = config.tiers.Find(t => t.tier == nextTier);

 

        if (tierConfig == null)

        {

            Debug.LogError($"No tier config found for tier {nextTier}");

            return false;

        }

 

        // Check level requirement

        if (progress.currentLevel < tierConfig.requiredLevel)

        {

            Debug.LogWarning($"Hero {heroId} level {progress.currentLevel} is below required level {tierConfig.requiredLevel}");

            return false;

        }

 

        // Check materials (integrate with MaterialInventory)

        if (!HasRequiredMaterials(tierConfig.materials))

        {

            Debug.LogWarning($"Insufficient materials for ascension");

            return false;

        }

 

        // Check gold (integrate with PlayerWallet)

        if (!HasRequiredGold(tierConfig.goldCost))

        {

            Debug.LogWarning($"Insufficient gold for ascension");

            return false;

        }

 

        // Consume materials and gold

        ConsumeMaterials(tierConfig.materials);

        ConsumeGold(tierConfig.goldCost);

 

        // Perform ascension

        progress.currentTier = nextTier;

        progress.levelCap = tierConfig.newLevelCap;

 

        // Apply stat bonuses

        ApplyStatBonuses(progress, tierConfig.statBonuses);

 

        // Unlock skills

        if (tierConfig.unlockedSkills != null)

        {

            foreach (string skillId in tierConfig.unlockedSkills)

            {

                if (!progress.unlockedSkills.Contains(skillId))

                {

                    progress.unlockedSkills.Add(skillId);

                    Debug.Log($"Unlocked skill {skillId} for hero {heroId}");

                }

            }

        }

 

        // Unlock passives

        if (tierConfig.unlockedPassives != null)

        {

            foreach (string passiveId in tierConfig.unlockedPassives)

            {

                if (!progress.unlockedPassives.Contains(passiveId))

                {

                    progress.unlockedPassives.Add(passiveId);

                    Debug.Log($"Unlocked passive {passiveId} for hero {heroId}");

                }

            }

        }

 

        // Grant talent points

        if (tierConfig.talentPointsGranted > 0)

        {

            TalentTreeManager.Instance.GrantTalentPoints(heroId, tierConfig.talentPointsGranted);

        }

 

        // Apply special features

        if (tierConfig.specialFeature != null && tierConfig.specialConfig != null)

        {

            ApplySpecialFeature(progress, tierConfig.specialConfig);

        }

 

        Debug.Log($"Successfully ascended hero {heroId} to tier {nextTier}! New level cap: {tierConfig.newLevelCap}");

        return true;

    }

 

    // Apply stat bonuses from ascension

    private void ApplyStatBonuses(HeroAscensionProgress progress, AscensionStatBonus bonuses)

    {

        if (bonuses == null) return;

 

        progress.totalStatBonuses.atkBonus += bonuses.atkBonus;

        progress.totalStatBonuses.defBonus += bonuses.defBonus;

        progress.totalStatBonuses.hpBonus += bonuses.hpBonus;

        progress.totalStatBonuses.spdBonus += bonuses.spdBonus;

 

        progress.totalStatBonuses.atkPercent += bonuses.atkPercent;

        progress.totalStatBonuses.defPercent += bonuses.defPercent;

        progress.totalStatBonuses.hpPercent += bonuses.hpPercent;

        progress.totalStatBonuses.spdPercent += bonuses.spdPercent;

 

        progress.totalStatBonuses.critBonus += bonuses.critBonus;

        progress.totalStatBonuses.critDmgBonus += bonuses.critDmgBonus;

        progress.totalStatBonuses.accBonus += bonuses.accBonus;

        progress.totalStatBonuses.resBonus += bonuses.resBonus;

    }

 

    // Apply special features from ascension

    private void ApplySpecialFeature(HeroAscensionProgress progress, AscensionSpecialFeature feature)

    {

        switch (feature.featureType)

        {

            case "skill_evolution":

                if (!string.IsNullOrEmpty(feature.baseSkillId) && !string.IsNullOrEmpty(feature.evolvedSkillId))

                {

                    progress.evolvedSkills.Add(feature.evolvedSkillId);

                    progress.activeFeatures.Add("skill_evolution");

                    Debug.Log($"Evolved skill {feature.baseSkillId} to {feature.evolvedSkillId}");

                }

                break;

 

            case "dual_element":

                if (!string.IsNullOrEmpty(feature.secondaryElement))

                {

                    progress.secondaryElement = feature.secondaryElement;

                    progress.activeFeatures.Add("dual_element");

                    Debug.Log($"Unlocked secondary element: {feature.secondaryElement}");

                }

                break;

 

            case "stat_break":

                progress.activeFeatures.Add($"stat_break_{feature.statType}");

                Debug.Log($"Unlocked stat break for {feature.statType}");

                break;

 

            case "passive_upgrade":

                if (!string.IsNullOrEmpty(feature.upgradedPassiveId))

                {

                    progress.activeFeatures.Add($"passive_upgrade_{feature.passiveId}");

                    Debug.Log($"Upgraded passive {feature.passiveId} to {feature.upgradedPassiveId}");

                }

                break;

        }

    }

 

    // Level up a hero

    public bool LevelUpHero(string heroId, int levelsToGain = 1)

    {

        if (!heroAscensionProgress.ContainsKey(heroId))

        {

            InitializeHeroAscension(heroId);

        }

 

        HeroAscensionProgress progress = heroAscensionProgress[heroId];

 

        if (progress.IsAtLevelCap())

        {

            Debug.LogWarning($"Hero {heroId} is at level cap {progress.levelCap}. Ascend to increase cap.");

            return false;

        }

 

        int newLevel = Mathf.Min(progress.currentLevel + levelsToGain, progress.levelCap);

        int actualLevels = newLevel - progress.currentLevel;

        progress.currentLevel = newLevel;

 

        // Grant talent points for each level

        if (actualLevels > 0)

        {

            TalentTreeManager talentManager = TalentTreeManager.Instance;

            if (talentManager != null)

            {

                TalentTreeConfig tree = talentManager.GetTalentTreeForHero(heroId);

                if (tree != null)

                {

                    int talentPoints = actualLevels * tree.talentPointsPerLevel;

                    talentManager.GrantTalentPoints(heroId, talentPoints);

                }

            }

        }

 

        Debug.Log($"Hero {heroId} leveled up to {newLevel}");

        return true;

    }

 

    // Get total stat bonuses for a hero

    public AscensionStatBonus GetAscensionStatBonuses(string heroId)

    {

        if (!heroAscensionProgress.ContainsKey(heroId))

        {

            return new AscensionStatBonus();

        }

 

        return heroAscensionProgress[heroId].totalStatBonuses;

    }

 

    // Get unlocked skills from ascension

    public List<string> GetUnlockedAscensionSkills(string heroId)

    {

        if (!heroAscensionProgress.ContainsKey(heroId))

        {

            return new List<string>();

        }

 

        return heroAscensionProgress[heroId].unlockedSkills;

    }

 

    // Check if skill has evolved

    public string GetEvolvedSkill(string heroId, string baseSkillId)

    {

        if (!heroAscensionProgress.ContainsKey(heroId))

        {

            return baseSkillId;

        }

 

        HeroAscensionProgress progress = heroAscensionProgress[heroId];

        HeroAscensionConfig config = GetAscensionConfig(heroId);

 

        if (config == null) return baseSkillId;

 

        // Check all tiers for skill evolution

        foreach (var tier in config.tiers)

        {

            if (tier.tier > progress.currentTier) break;

 

            if (tier.specialConfig?.featureType == "skill_evolution" &&

                tier.specialConfig.baseSkillId == baseSkillId)

            {

                return tier.specialConfig.evolvedSkillId;

            }

        }

 

        return baseSkillId;

    }

 

    // Material and gold checking (to be integrated with actual inventory systems)

    private bool HasRequiredMaterials(List<MaterialRequirement> materials)

    {

        // TODO: Integrate with MaterialInventory

        // For now, return true

        return true;

    }

 

    private bool HasRequiredGold(int goldCost)

    {

        // TODO: Integrate with PlayerWallet

        // For now, return true

        return true;

    }

 

    private void ConsumeMaterials(List<MaterialRequirement> materials)

    {

        // TODO: Integrate with MaterialInventory

        if (MaterialInventory.Instance != null)

        {

            foreach (var req in materials)

            {

                MaterialInventory.Instance.Remove(req.materialId, req.quantity);

            }

        }

    }

 

    private void ConsumeGold(int goldCost)

    {

        // TODO: Integrate with PlayerWallet

        if (PlayerWallet.Instance != null)

        {

            PlayerWallet.Instance.DeductGold(goldCost);

        }

    }

 

    // Utility methods

    public HeroAscensionConfig GetAscensionConfig(string heroId)

    {

        return ascensionConfigs.ContainsKey(heroId) ? ascensionConfigs[heroId] : null;

    }

 

    public HeroAscensionProgress GetHeroAscensionProgress(string heroId)

    {

        return heroAscensionProgress.ContainsKey(heroId) ? heroAscensionProgress[heroId] : null;

    }

}

 

[System.Serializable]

public class HeroAscensionDatabase

{

    public List<HeroAscensionConfig> ascensions;

}