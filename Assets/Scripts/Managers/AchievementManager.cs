using System;

using System.Collections.Generic;

using System.Linq;

using UnityEngine;

using FnGMafia.Core;

 

/// <summary>

/// Manager for achievement system with titles and portraits

/// </summary>

public class AchievementManager : Singleton<AchievementManager>

{

    private AchievementDatabase achievementDatabase;

    private Dictionary<string, PlayerAchievementProgress> playerProgress = new Dictionary<string, PlayerAchievementProgress>();

    private int totalAchievementPoints = 0;

    private string equippedTitle = "";

    private string equippedPortrait = "";

 

    protected override void Awake()

    {

        base.Awake();

        LoadAchievementDatabase();

        InitializeProgress();

    }

 

    private void LoadAchievementDatabase()

    {

        TextAsset achievementData = Resources.Load<TextAsset>("Config/achievements");

        if (achievementData != null)

        {

            achievementDatabase = JsonUtility.FromJson<AchievementDatabase>(achievementData.text);

            Debug.Log($"Loaded {achievementDatabase.achievements.Count} achievements");

        }

        else

        {

            Debug.LogError("Failed to load achievements.json");

            achievementDatabase = new AchievementDatabase();

        }

    }

 

    private void InitializeProgress()

    {

        foreach (var achievement in achievementDatabase.achievements)

        {

            if (!playerProgress.ContainsKey(achievement.achievementId))

            {

                playerProgress[achievement.achievementId] = new PlayerAchievementProgress

                {

                    achievementId = achievement.achievementId,

                    currentValue = 0,

                    isCompleted = false,

                    isClaimed = false,

                    completionDate = DateTime.MinValue

                };

            }

        }

    }

 

    /// <summary>

    /// Update achievement progress

    /// </summary>

    public void UpdateAchievementProgress(string criteriaType, int value = 1, string specificId = "")

    {

        var relevantAchievements = achievementDatabase.achievements

            .Where(a => a.criteria.criteriaType == criteriaType)

            .Where(a => string.IsNullOrEmpty(specificId) || a.criteria.specificId == specificId)

            .ToList();

 

        foreach (var achievement in relevantAchievements)

        {

            var progress = playerProgress[achievement.achievementId];

 

            if (progress.isCompleted)

                continue;

 

            // Check prerequisites

            if (!string.IsNullOrEmpty(achievement.prerequisiteAchievement))

            {

                var prereq = playerProgress[achievement.prerequisiteAchievement];

                if (!prereq.isCompleted)

                    continue;

            }

 

            // Update progress

            progress.currentValue += value;

 

            // Check completion

            if (progress.currentValue >= achievement.criteria.targetValue)

            {

                CompleteAchievement(achievement.achievementId);

            }

        }

    }

 

    /// <summary>

    /// Set achievement progress (for absolute values like level, power)

    /// </summary>

    public void SetAchievementProgress(string criteriaType, int value, string specificId = "")

    {

        var relevantAchievements = achievementDatabase.achievements

            .Where(a => a.criteria.criteriaType == criteriaType)

            .Where(a => string.IsNullOrEmpty(specificId) || a.criteria.specificId == specificId)

            .ToList();

 

        foreach (var achievement in relevantAchievements)

        {

            var progress = playerProgress[achievement.achievementId];

 

            if (progress.isCompleted)

                continue;

 

            // Check prerequisites

            if (!string.IsNullOrEmpty(achievement.prerequisiteAchievement))

            {

                var prereq = playerProgress[achievement.prerequisiteAchievement];

                if (!prereq.isCompleted)

                    continue;

            }

 

            // Set progress

            progress.currentValue = value;

 

            // Check completion

            if (progress.currentValue >= achievement.criteria.targetValue)

            {

                CompleteAchievement(achievement.achievementId);

            }

        }

    }

 

    /// <summary>

    /// Complete an achievement

    /// </summary>

    private void CompleteAchievement(string achievementId)

    {

        var progress = playerProgress[achievementId];

        if (progress.isCompleted)

            return;

 

        var achievement = achievementDatabase.achievements.Find(a => a.achievementId == achievementId);

        if (achievement == null)

            return;

 

        progress.isCompleted = true;

        progress.completionDate = DateTime.Now;

 

        Debug.Log($"🏆 Achievement Unlocked: {achievement.achievementName}");

 

        // Show notification (would trigger UI)

        ShowAchievementNotification(achievement);

    }

 

    /// <summary>

    /// Claim achievement rewards

    /// </summary>

    public bool ClaimAchievementReward(string achievementId)

    {

        var progress = playerProgress[achievementId];

        if (!progress.isCompleted)

        {

            Debug.Log("Achievement not completed");

            return false;

        }

 

        if (progress.isClaimed)

        {

            Debug.Log("Rewards already claimed");

            return false;

        }

 

        var achievement = achievementDatabase.achievements.Find(a => a.achievementId == achievementId);

        if (achievement == null)

            return false;

 

        // Give rewards

        foreach (var reward in achievement.rewards)

        {

            GiveReward(reward);

        }

 

        // Add achievement points

        totalAchievementPoints += achievement.achievementPoints;

 

        // Check milestone rewards

        CheckMilestoneRewards();

 

        progress.isClaimed = true;

        Debug.Log($"Claimed rewards for: {achievement.achievementName}");

 

        return true;

    }

 

    /// <summary>

    /// Check if milestone rewards are available

    /// </summary>

    private void CheckMilestoneRewards()

    {

        foreach (var milestone in achievementDatabase.rewardTiers.milestones)

        {

            if (totalAchievementPoints >= milestone.requiredPoints)

            {

                // TODO: Track which milestones have been claimed

                Debug.Log($"Milestone reached: {milestone.requiredPoints} points!");

            }

        }

    }

 

    /// <summary>

    /// Equip a title

    /// </summary>

    public bool EquipTitle(string titleId)

    {

        var title = achievementDatabase.titles.Find(t => t.titleId == titleId);

        if (title == null)

        {

            Debug.LogError($"Title {titleId} not found");

            return false;

        }

 

        // Check if unlocked

        if (!string.IsNullOrEmpty(title.unlockedBy))

        {

            var achievement = playerProgress[title.unlockedBy];

            if (!achievement.isCompleted)

            {

                Debug.Log("Title not unlocked");

                return false;

            }

        }

 

        equippedTitle = titleId;

        Debug.Log($"Equipped title: {title.titleText}");

        return true;

    }

 

    /// <summary>

    /// Equip a portrait frame

    /// </summary>

    public bool EquipPortrait(string frameId)

    {

        var frame = achievementDatabase.portraitFrames.Find(f => f.frameId == frameId);

        if (frame == null)

        {

            Debug.LogError($"Portrait frame {frameId} not found");

            return false;

        }

 

        // Check if unlocked

        if (!string.IsNullOrEmpty(frame.unlockedBy))

        {

            var achievement = playerProgress[frame.unlockedBy];

            if (!achievement.isCompleted)

            {

                Debug.Log("Portrait frame not unlocked");

                return false;

            }

        }

 

        equippedPortrait = frameId;

        Debug.Log($"Equipped portrait: {frame.frameName}");

        return true;

    }

 

    /// <summary>

    /// Get equipped title bonuses

    /// </summary>

    public TitleEffect GetTitleBonuses()

    {

        if (string.IsNullOrEmpty(equippedTitle))

            return null;

 

        var title = achievementDatabase.titles.Find(t => t.titleId == equippedTitle);

        return title?.titleEffect;

    }

 

    /// <summary>

    /// Get all unlocked titles

    /// </summary>

    public List<TitleConfig> GetUnlockedTitles()

    {

        var unlockedTitles = new List<TitleConfig>();

 

        foreach (var title in achievementDatabase.titles)

        {

            if (string.IsNullOrEmpty(title.unlockedBy))

            {

                unlockedTitles.Add(title);

                continue;

            }

 

            if (playerProgress.ContainsKey(title.unlockedBy) &&

                playerProgress[title.unlockedBy].isCompleted)

            {

                unlockedTitles.Add(title);

            }

        }

 

        return unlockedTitles;

    }

 

    /// <summary>

    /// Get all unlocked portrait frames

    /// </summary>

    public List<PortraitFrameConfig> GetUnlockedPortraits()

    {

        var unlockedFrames = new List<PortraitFrameConfig>();

 

        foreach (var frame in achievementDatabase.portraitFrames)

        {

            if (string.IsNullOrEmpty(frame.unlockedBy))

            {

                unlockedFrames.Add(frame);

                continue;

            }

 

            if (playerProgress.ContainsKey(frame.unlockedBy) &&

                playerProgress[frame.unlockedBy].isCompleted)

            {

                unlockedFrames.Add(frame);

            }

        }

 

        return unlockedFrames;

    }

 

    /// <summary>

    /// Get achievement progress

    /// </summary>

    public PlayerAchievementProgress GetAchievementProgress(string achievementId)

    {

        return playerProgress.ContainsKey(achievementId) ? playerProgress[achievementId] : null;

    }

 

    /// <summary>

    /// Get all achievements by category

    /// </summary>

    public List<AchievementConfig> GetAchievementsByCategory(string category)

    {

        return achievementDatabase.achievements

            .Where(a => a.category == category)

            .Where(a => !a.isHidden || playerProgress[a.achievementId].isCompleted)

            .ToList();

    }

 

    /// <summary>

    /// Get total achievement stats

    /// </summary>

    public (int completed, int total, int points) GetAchievementStats()

    {

        int completed = playerProgress.Values.Count(p => p.isCompleted);

        int total = achievementDatabase.achievements.Count;

        return (completed, total, totalAchievementPoints);

    }

 

    // Helper methods

    private void ShowAchievementNotification(AchievementConfig achievement)

    {

        // TODO: Trigger UI notification

        Debug.Log($"🏆 {achievement.achievementName}");

        Debug.Log($"   {achievement.description}");

        if (!string.IsNullOrEmpty(achievement.unlockedTitle))

        {

            Debug.Log($"   Title unlocked: {achievement.unlockedTitle}");

        }

        if (!string.IsNullOrEmpty(achievement.unlockedPortrait))

        {

            Debug.Log($"   Portrait unlocked: {achievement.unlockedPortrait}");

        }

    }

 

    private void GiveReward(AchievementReward reward)

    {

        Debug.Log($"Received: {reward.quantity}x {reward.rewardId}");

        // TODO: Add to player inventory based on rewardType

    }

}
