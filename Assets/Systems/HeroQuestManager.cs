using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class HeroQuestManager : Singleton<HeroQuestManager>
{
    private Dictionary<string, HeroQuestProgress> heroQuestProgress;
    private Dictionary<string, HeroQuestConfig> questConfigs;
    private Dictionary<string, OriginStoryConfig> originStories;

    protected override void Awake()
    {
        base.Awake();
        heroQuestProgress = new Dictionary<string, HeroQuestProgress>();
        questConfigs = new Dictionary<string, HeroQuestConfig>();
        originStories = new Dictionary<string, OriginStoryConfig>();
        LoadQuestConfigs();
    }

    private void LoadQuestConfigs()
    {
        // Load hero quests
        TextAsset configFile = Resources.Load<TextAsset>("Config/hero_quests");
        if (configFile != null)
        {
            HeroQuestDatabase database = JsonUtility.FromJson<HeroQuestDatabase>(configFile.text);
            foreach (var quest in database.quests)
            {
                questConfigs[quest.questId] = quest;
            }
            Debug.Log($"Loaded {questConfigs.Count} hero quests");
        }
        else
        {
            Debug.LogWarning("hero_quests.json not found in Resources/Config");
        }

        // Load origin stories
        TextAsset storiesFile = Resources.Load<TextAsset>("Config/origin_stories");
        if (storiesFile != null)
        {
            OriginStoryDatabase storyDb = JsonUtility.FromJson<OriginStoryDatabase>(storiesFile.text);
            foreach (var story in storyDb.stories)
            {
                originStories[story.heroId] = story;
            }
            Debug.Log($"Loaded {originStories.Count} origin stories");
        }
    }

    // Initialize hero quest progress
    public void InitializeHeroQuests(string heroId)
    {
        if (!heroQuestProgress.ContainsKey(heroId))
        {
            HeroQuestProgress progress = new HeroQuestProgress
            {
                heroId = heroId
            };

            // Unlock initial quests
            UnlockInitialQuests(heroId, progress);

            heroQuestProgress[heroId] = progress;
        }
    }

    private void UnlockInitialQuests(string heroId, HeroQuestProgress progress)
    {
        // Unlock origin quest if available
        if (originStories.ContainsKey(heroId))
        {
            OriginStoryConfig story = originStories[heroId];
            if (story.questIds != null && story.questIds.Count > 0)
            {
                // Unlock first origin quest
                progress.unlockedQuests.Add(story.questIds[0]);
            }
        }

        // Unlock any chapter 1 quests for this hero
        foreach (var quest in questConfigs.Values)
        {
            if (quest.heroId == heroId && quest.chapter == 1)
            {
                if (!progress.unlockedQuests.Contains(quest.questId))
                {
                    progress.unlockedQuests.Add(quest.questId);
                }
            }
        }
    }

    // Start a quest
    public bool StartQuest(string heroId, string questId)
    {
        if (!heroQuestProgress.ContainsKey(heroId))
        {
            InitializeHeroQuests(heroId);
        }

        HeroQuestProgress progress = heroQuestProgress[heroId];
        HeroQuestConfig quest = GetQuestConfig(questId);

        if (quest == null)
        {
            Debug.LogError($"Quest {questId} not found");
            return false;
        }

        // Check if quest is unlocked
        if (!progress.IsQuestUnlocked(questId))
        {
            Debug.LogWarning($"Quest {questId} is not unlocked for hero {heroId}");
            return false;
        }

        // Check if already completed (non-repeatable)
        if (progress.IsQuestCompleted(questId) && !quest.isRepeatable)
        {
            Debug.LogWarning($"Quest {questId} is already completed and not repeatable");
            return false;
        }

        // Check cooldown for repeatable quests
        if (quest.isRepeatable && progress.questCooldowns.ContainsKey(questId))
        {
            DateTime cooldownEnd = progress.questCooldowns[questId];
            if (DateTime.Now < cooldownEnd)
            {
                Debug.LogWarning($"Quest {questId} is on cooldown until {cooldownEnd}");
                return false;
            }
        }

        // Check requirements
        if (!MeetRequirements(heroId, quest.requirements))
        {
            Debug.LogWarning($"Hero {heroId} does not meet requirements for quest {questId}");
            return false;
        }

        // Check if already active
        if (progress.IsQuestActive(questId))
        {
            Debug.LogWarning($"Quest {questId} is already active");
            return false;
        }

        // Start the quest
        ActiveQuest activeQuest = new ActiveQuest
        {
            questId = questId,
            startTime = DateTime.Now
        };

        // Initialize objectives
        foreach (var objective in quest.objectives)
        {
            activeQuest.objectiveProgress.Add(new ObjectiveProgress
            {
                objectiveId = objective.objectiveId,
                currentCount = 0,
                targetCount = objective.targetCount,
                isCompleted = false
            });
        }

        progress.activeQuests.Add(activeQuest);
        Debug.Log($"Started quest {questId} for hero {heroId}");

        return true;
    }

    // Update quest progress
    public void UpdateQuestProgress(string heroId, string questId, string objectiveId, int amount = 1)
    {
        if (!heroQuestProgress.ContainsKey(heroId))
        {
            return;
        }

        HeroQuestProgress progress = heroQuestProgress[heroId];
        ActiveQuest activeQuest = progress.GetActiveQuest(questId);

        if (activeQuest == null)
        {
            return;
        }

        ObjectiveProgress objective = activeQuest.objectiveProgress.Find(o => o.objectiveId == objectiveId);
        if (objective != null && !objective.isCompleted)
        {
            objective.IncrementProgress(amount);
            Debug.Log($"Quest {questId} objective {objectiveId}: {objective.currentCount}/{objective.targetCount}");

            // Check if quest is complete
            if (activeQuest.IsComplete())
            {
                CompleteQuest(heroId, questId);
            }
        }
    }

    // Complete a quest
    private void CompleteQuest(string heroId, string questId)
    {
        HeroQuestProgress progress = heroQuestProgress[heroId];
        ActiveQuest activeQuest = progress.GetActiveQuest(questId);
        HeroQuestConfig quest = GetQuestConfig(questId);

        if (activeQuest == null || quest == null)
        {
            return;
        }

        // Remove from active quests
        progress.activeQuests.Remove(activeQuest);

        // Add to completed quests
        CompletedQuest completed = progress.completedQuests.Find(c => c.questId == questId);
        if (completed == null)
        {
            completed = new CompletedQuest
            {
                questId = questId,
                completionTime = DateTime.Now,
                timesCompleted = 1
            };
            progress.completedQuests.Add(completed);
        }
        else
        {
            completed.timesCompleted++;
            completed.completionTime = DateTime.Now;
        }

        // Set cooldown for repeatable quests
        if (quest.isRepeatable)
        {
            progress.questCooldowns[questId] = DateTime.Now.AddHours(quest.repeatCooldownHours);
        }

        // Grant rewards
        GrantQuestRewards(heroId, quest.rewards);

        // Unlock follow-up quests
        if (quest.unlocksQuests != null)
        {
            foreach (string unlockedQuestId in quest.unlocksQuests)
            {
                if (!progress.unlockedQuests.Contains(unlockedQuestId))
                {
                    progress.unlockedQuests.Add(unlockedQuestId);
                    Debug.Log($"Unlocked quest {unlockedQuestId}");
                }
            }
        }

        Debug.Log($"Completed quest {questId} for hero {heroId}!");
    }

    // Grant quest rewards
    private void GrantQuestRewards(string heroId, QuestRewards rewards)
    {
        if (rewards == null) return;

        // Grant gold
        if (rewards.gold > 0 && PlayerWallet.Instance != null)
        {
            PlayerWallet.Instance.AddGold(rewards.gold);
            Debug.Log($"Granted {rewards.gold} gold");
        }

        // Grant materials
        if (rewards.materials != null && MaterialInventory.Instance != null)
        {
            foreach (var material in rewards.materials)
            {
                MaterialInventory.Instance.AddMaterial(material.materialId, material.quantity);
                Debug.Log($"Granted {material.quantity}x {material.materialId}");
            }
        }

        // Grant talent points
        if (rewards.talentPoints > 0 && TalentTreeManager.Instance != null)
        {
            TalentTreeManager.Instance.GrantTalentPoints(heroId, rewards.talentPoints);
        }

        // Grant elemental mastery
        if (rewards.elementalMastery > 0 && ElementalAffinityManager.Instance != null)
        {
            ElementalAffinityManager.Instance.IncreaseElementalMastery(heroId, rewards.elementalMastery);
        }

        // Unlock skill
        if (!string.IsNullOrEmpty(rewards.unlockedSkillId))
        {
            Debug.Log($"Unlocked skill {rewards.unlockedSkillId} for hero {heroId}");
            // Could add to a hero skills list
        }

        // Grant exclusive gear
        if (rewards.exclusiveGear != null)
        {
            Debug.Log($"Granted exclusive gear: {rewards.exclusiveGear.gearName}");
            // Could add to inventory with special flag
        }
    }

    // Check if requirements are met
    private bool MeetRequirements(string heroId, List<QuestRequirement> requirements)
    {
        if (requirements == null || requirements.Count == 0)
        {
            return true;
        }

        foreach (var req in requirements)
        {
            switch (req.requirementType)
            {
                case "hero_level":
                    // Check hero level (integrate with hero system)
                    HeroAscensionProgress ascProgress = HeroAscensionManager.Instance?.GetHeroAscensionProgress(heroId);
                    if (ascProgress != null && ascProgress.currentLevel < req.requiredLevel)
                    {
                        return false;
                    }
                    break;

                case "ascension_tier":
                    ascProgress = HeroAscensionManager.Instance?.GetHeroAscensionProgress(heroId);
                    if (ascProgress != null && ascProgress.currentTier < req.requiredAscension)
                    {
                        return false;
                    }
                    break;

                case "quest_completed":
                    HeroQuestProgress progress = GetHeroQuestProgress(heroId);
                    if (progress != null && !progress.IsQuestCompleted(req.requiredQuestId))
                    {
                        return false;
                    }
                    break;

                case "hero_owned":
                    // Check if hero is owned (integrate with HeroCollection)
                    if (HeroCollection.Instance != null && !HeroCollection.Instance.IsHeroOwned(req.requiredHeroId))
                    {
                        return false;
                    }
                    break;
            }
        }

        return true;
    }

    // Track enemy defeats for quest objectives
    public void OnEnemyDefeated(string heroId, string enemyId)
    {
        if (!heroQuestProgress.ContainsKey(heroId))
        {
            return;
        }

        HeroQuestProgress progress = heroQuestProgress[heroId];

        foreach (var activeQuest in progress.activeQuests)
        {
            HeroQuestConfig quest = GetQuestConfig(activeQuest.questId);
            if (quest == null) continue;

            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == "defeat_enemies")
                {
                    // Check if it's the right enemy or any enemy
                    if (string.IsNullOrEmpty(objective.targetEnemyId) || objective.targetEnemyId == enemyId)
                    {
                        UpdateQuestProgress(heroId, activeQuest.questId, objective.objectiveId, 1);
                    }
                }
            }
        }
    }

    // Track skill usage
    public void OnSkillUsed(string heroId, string skillId)
    {
        if (!heroQuestProgress.ContainsKey(heroId))
        {
            return;
        }

        HeroQuestProgress progress = heroQuestProgress[heroId];

        foreach (var activeQuest in progress.activeQuests)
        {
            HeroQuestConfig quest = GetQuestConfig(activeQuest.questId);
            if (quest == null) continue;

            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == "use_skill" && objective.targetSkillId == skillId)
                {
                    UpdateQuestProgress(heroId, activeQuest.questId, objective.objectiveId, 1);
                }
            }
        }
    }

    // Track stage completion
    public void OnStageCompleted(string heroId, string stageId)
    {
        if (!heroQuestProgress.ContainsKey(heroId))
        {
            return;
        }

        HeroQuestProgress progress = heroQuestProgress[heroId];

        foreach (var activeQuest in progress.activeQuests)
        {
            HeroQuestConfig quest = GetQuestConfig(activeQuest.questId);
            if (quest == null) continue;

            foreach (var objective in quest.objectives)
            {
                if (objective.objectiveType == "complete_stage" && objective.targetStageId == stageId)
                {
                    UpdateQuestProgress(heroId, activeQuest.questId, objective.objectiveId, 1);
                }
            }
        }
    }

    // Utility methods
    public HeroQuestConfig GetQuestConfig(string questId)
    {
        return questConfigs.ContainsKey(questId) ? questConfigs[questId] : null;
    }

    public HeroQuestProgress GetHeroQuestProgress(string heroId)
    {
        return heroQuestProgress.ContainsKey(heroId) ? heroQuestProgress[heroId] : null;
    }

    public OriginStoryConfig GetOriginStory(string heroId)
    {
        return originStories.ContainsKey(heroId) ? originStories[heroId] : null;
    }

    public List<HeroQuestConfig> GetAvailableQuests(string heroId)
    {
        List<HeroQuestConfig> available = new List<HeroQuestConfig>();

        if (!heroQuestProgress.ContainsKey(heroId))
        {
            InitializeHeroQuests(heroId);
        }

        HeroQuestProgress progress = heroQuestProgress[heroId];

        foreach (string questId in progress.unlockedQuests)
        {
            if (!progress.IsQuestActive(questId) && !progress.IsQuestCompleted(questId))
            {
                HeroQuestConfig quest = GetQuestConfig(questId);
                if (quest != null)
                {
                    available.Add(quest);
                }
            }
        }

        return available;
    }
}

[System.Serializable]
public class HeroQuestDatabase
{
    public List<HeroQuestConfig> quests;
}

[System.Serializable]
public class OriginStoryDatabase
{
    public List<OriginStoryConfig> stories;
}
