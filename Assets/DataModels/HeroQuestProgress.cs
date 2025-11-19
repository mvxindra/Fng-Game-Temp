using System;
using System.Collections.Generic;

[Serializable]
public class HeroQuestProgress
{
    public string heroId;
    public List<CompletedQuest> completedQuests;
    public List<ActiveQuest> activeQuests;
    public List<string> unlockedQuests;
    public Dictionary<string, DateTime> questCooldowns; // For repeatable quests

    public HeroQuestProgress()
    {
        completedQuests = new List<CompletedQuest>();
        activeQuests = new List<ActiveQuest>();
        unlockedQuests = new List<string>();
        questCooldowns = new Dictionary<string, DateTime>();
    }

    public bool IsQuestCompleted(string questId)
    {
        return completedQuests.Exists(q => q.questId == questId);
    }

    public bool IsQuestActive(string questId)
    {
        return activeQuests.Exists(q => q.questId == questId);
    }

    public bool IsQuestUnlocked(string questId)
    {
        return unlockedQuests.Contains(questId);
    }

    public ActiveQuest GetActiveQuest(string questId)
    {
        return activeQuests.Find(q => q.questId == questId);
    }
}

[Serializable]
public class ActiveQuest
{
    public string questId;
    public DateTime startTime;
    public List<ObjectiveProgress> objectiveProgress;

    public ActiveQuest()
    {
        objectiveProgress = new List<ObjectiveProgress>();
    }

    public bool IsComplete()
    {
        foreach (var obj in objectiveProgress)
        {
            if (!obj.isCompleted)
            {
                return false;
            }
        }
        return true;
    }
}

[Serializable]
public class ObjectiveProgress
{
    public string objectiveId;
    public int currentCount;
    public int targetCount;
    public bool isCompleted;

    public void IncrementProgress(int amount = 1)
    {
        currentCount += amount;
        if (currentCount >= targetCount)
        {
            currentCount = targetCount;
            isCompleted = true;
        }
    }
}

[Serializable]
public class CompletedQuest
{
    public string questId;
    public DateTime completionTime;
    public int timesCompleted;                    // For repeatable quests
}
