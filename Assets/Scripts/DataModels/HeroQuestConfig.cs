using System;

using System.Collections.Generic;

 

[Serializable]

public class HeroQuestConfig

{

    public string questId;                        // Unique quest identifier

    public string heroId;                         // Hero this quest belongs to

    public string questName;                      // Display name

    public string questType;                      // "origin", "character_story", "mastery", "awakening"

    public int chapter;                           // Quest chapter/sequence

    public string description;                    // Quest story text

    public string loreText;                       // Background lore

 

    // Requirements

    public List<QuestRequirement> requirements;

 

    // Objectives

    public List<QuestObjective> objectives;

 

    // Rewards

    public QuestRewards rewards;

 

    // Unlocks

    public List<string> unlocksQuests;            // Follow-up quests

    public bool isRepeatable;

    public int repeatCooldownHours;

}

 

[Serializable]

public class QuestRequirement

{

    public string requirementType;                // "hero_level", "ascension_tier", "hero_owned", "quest_completed"

    public int requiredLevel;

    public int requiredAscension;

    public string requiredHeroId;

    public string requiredQuestId;

}

 

[Serializable]

public class QuestObjective

{

    public string objectiveId;

    public string objectiveType;                  // "defeat_enemies", "complete_stage", "use_skill", "survive_turns", "collect_item"

    public string description;

    public int targetCount;                       // Number required

    public int currentCount;                      // Current progress

 

    // Specific parameters

    public string targetEnemyId;                  // Specific enemy to defeat

    public string targetStageId;                  // Stage to complete

    public string targetSkillId;                  // Skill to use

    public int survivalTurns;                     // Turns to survive

    public string itemId;                         // Item to collect

    public bool mustUseHero;                      // Must use quest hero

    public List<string> allowedHeroes;            // Allowed heroes in party

 

    // Battle conditions

    public string battleCondition;                // "no_deaths", "under_turns", "no_ultimate", "solo"

    public int maxTurns;

    public bool soloOnly;

    public bool noDeaths;

}

 

[Serializable]

public class QuestRewards

{

    public int gold;

    public int experiencePoints;                  // Hero EXP

    public List<ItemReward> items;

    public List<MaterialReward> materials;

 

    // Special rewards

    public string unlockedSkillId;                // Skill unlock

    public string unlockedPassiveId;              // Passive unlock

    public string unlockedTitleId;                // Cosmetic title

    public int talentPoints;

    public int elementalMastery;

    public QuestEquipmentReward exclusiveGear;    // Unique quest gear

}

 

[Serializable]

public class ItemReward

{

    public string itemId;

    public int quantity;

}

 

[Serializable]

public class MaterialReward

{

    public string materialId;

    public int quantity;

}

 

[Serializable]

public class QuestEquipmentReward

{

    public string gearId;

    public string gearName;

    public string description;

    public bool isSoulbound;                      // Can't be transferred to other heroes

}

 

[Serializable]

public class OriginStoryConfig

{

    public string heroId;

    public string originTitle;                    // e.g., "The Fallen Knight"

    public List<StoryChapter> chapters;

    public List<string> questIds;                 // All origin quests

}

 

[Serializable]

public class StoryChapter

{

    public int chapterNumber;

    public string chapterTitle;

    public string storyText;

    public List<string> dialogues;

    public string unlockQuestId;

}