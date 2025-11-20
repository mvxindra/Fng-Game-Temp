using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FnGMafia.Core;
using FnGMafia.Profile;

/// <summary>
/// Manages player profile, customization, and stats showcase
/// </summary>
public class PlayerProfileManager : Singleton<PlayerProfileManager>
{
    private ProfileSaveData profileData;

    // Customization databases
    private List<ProfileTitle> availableTitles = new List<ProfileTitle>();
    private List<ProfileFrame> availableFrames = new List<ProfileFrame>();
    private List<ProfileBackground> availableBackgrounds = new List<ProfileBackground>();

    protected override void Awake()
    {
        base.Awake();
        InitializeProfile();
        LoadCustomizationData();
    }

    private void InitializeProfile()
    {
        profileData = new ProfileSaveData
        {
            profileInfo = new PlayerProfileInfo
            {
                playerId = "player_" + Guid.NewGuid().ToString(),
                playerName = "Player",
                playerLevel = 1,
                experience = 0,
                experienceToNext = 100,
                accountCreatedDate = DateTime.Now,
                lastLoginDate = DateTime.Now,
                totalPlayTime = 0
            },
            customization = new ProfileCustomization
            {
                profileIcon = "default_icon",
                profileFrame = "default_frame",
                profileBackground = "default_bg",
                equippedTitle = "none",
                unlockedTitles = new List<string> { "beginner" },
                unlockedFrames = new List<string> { "default_frame" },
                unlockedBackgrounds = new List<string> { "default_bg" },
                statusMessage = "",
                bio = ""
            },
            stats = new ProfileStats
            {
                accountLevel = 1,
                vipLevel = 0
            },
            showcase = new ProfileShowcase
            {
                featuredHeroes = new List<ShowcaseHero>(),
                featuredAchievements = new List<string>(),
                hallOfFame = new List<HallOfFameEntry>(),
                recentActivity = new List<ProfileActivity>()
            }
        };
    }

    private void LoadCustomizationData()
    {
        // In real implementation, load from config files
        availableTitles = CreateDefaultTitles();
        availableFrames = CreateDefaultFrames();
        availableBackgrounds = CreateDefaultBackgrounds();

        Debug.Log("[PlayerProfileManager] Profile system initialized");
    }

    #region Profile Info

    /// <summary>
    /// Update player name
    /// </summary>
    public bool UpdatePlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) || newName.Length < 3 || newName.Length > 16)
        {
            Debug.Log("[PlayerProfileManager] Invalid name length (3-16 characters)");
            return false;
        }

        profileData.profileInfo.playerName = newName;
        Debug.Log($"[PlayerProfileManager] Name updated to: {newName}");
        return true;
    }

    /// <summary>
    /// Update status message
    /// </summary>
    public bool UpdateStatusMessage(string status)
    {
        if (status.Length > 100)
        {
            Debug.Log("[PlayerProfileManager] Status message too long (max 100 characters)");
            return false;
        }

        profileData.customization.statusMessage = status;
        Debug.Log("[PlayerProfileManager] Status updated");
        return true;
    }

    /// <summary>
    /// Update bio
    /// </summary>
    public bool UpdateBio(string bio)
    {
        if (bio.Length > 500)
        {
            Debug.Log("[PlayerProfileManager] Bio too long (max 500 characters)");
            return false;
        }

        profileData.customization.bio = bio;
        Debug.Log("[PlayerProfileManager] Bio updated");
        return true;
    }

    #endregion

    #region Customization

    /// <summary>
    /// Equip title
    /// </summary>
    public bool EquipTitle(string titleId)
    {
        if (titleId != "none" && !profileData.customization.unlockedTitles.Contains(titleId))
        {
            Debug.Log($"[PlayerProfileManager] Title {titleId} not unlocked");
            return false;
        }

        profileData.customization.equippedTitle = titleId;
        Debug.Log($"[PlayerProfileManager] Equipped title: {titleId}");
        return true;
    }

    /// <summary>
    /// Equip frame
    /// </summary>
    public bool EquipFrame(string frameId)
    {
        if (!profileData.customization.unlockedFrames.Contains(frameId))
        {
            Debug.Log($"[PlayerProfileManager] Frame {frameId} not unlocked");
            return false;
        }

        profileData.customization.profileFrame = frameId;
        Debug.Log($"[PlayerProfileManager] Equipped frame: {frameId}");
        return true;
    }

    /// <summary>
    /// Equip background
    /// </summary>
    public bool EquipBackground(string backgroundId)
    {
        if (!profileData.customization.unlockedBackgrounds.Contains(backgroundId))
        {
            Debug.Log($"[PlayerProfileManager] Background {backgroundId} not unlocked");
            return false;
        }

        profileData.customization.profileBackground = backgroundId;
        Debug.Log($"[PlayerProfileManager] Equipped background: {backgroundId}");
        return true;
    }

    /// <summary>
    /// Unlock title
    /// </summary>
    public bool UnlockTitle(string titleId)
    {
        if (profileData.customization.unlockedTitles.Contains(titleId))
            return false;

        profileData.customization.unlockedTitles.Add(titleId);
        Debug.Log($"[PlayerProfileManager] Unlocked title: {titleId}");
        return true;
    }

    /// <summary>
    /// Unlock frame
    /// </summary>
    public bool UnlockFrame(string frameId)
    {
        if (profileData.customization.unlockedFrames.Contains(frameId))
            return false;

        profileData.customization.unlockedFrames.Add(frameId);
        Debug.Log($"[PlayerProfileManager] Unlocked frame: {frameId}");
        return true;
    }

    /// <summary>
    /// Unlock background
    /// </summary>
    public bool UnlockBackground(string backgroundId)
    {
        if (profileData.customization.unlockedBackgrounds.Contains(backgroundId))
            return false;

        profileData.customization.unlockedBackgrounds.Add(backgroundId);
        Debug.Log($"[PlayerProfileManager] Unlocked background: {backgroundId}");
        return true;
    }

    #endregion

    #region Showcase

    /// <summary>
    /// Set featured hero in showcase
    /// </summary>
    public bool SetFeaturedHero(int slotIndex, string heroId)
    {
        if (slotIndex < 0 || slotIndex > 2)
        {
            Debug.Log("[PlayerProfileManager] Invalid showcase slot (0-2)");
            return false;
        }

        // Remove existing hero in this slot
        profileData.showcase.featuredHeroes.RemoveAll(h => h.slotIndex == slotIndex);

        // Add new hero (in real implementation, load actual hero data)
        ShowcaseHero hero = new ShowcaseHero
        {
            slotIndex = slotIndex,
            heroId = heroId,
            heroName = "Hero Name", // Would load actual name
            level = 60,
            stars = 5,
            power = 10000,
            show3DModel = true
        };

        profileData.showcase.featuredHeroes.Add(hero);
        profileData.showcase.featuredHeroes = profileData.showcase.featuredHeroes.OrderBy(h => h.slotIndex).ToList();

        Debug.Log($"[PlayerProfileManager] Set featured hero in slot {slotIndex}");
        return true;
    }

    /// <summary>
    /// Add featured achievement
    /// </summary>
    public bool AddFeaturedAchievement(string achievementId)
    {
        if (profileData.showcase.featuredAchievements.Contains(achievementId))
            return false;

        if (profileData.showcase.featuredAchievements.Count >= 5)
        {
            Debug.Log("[PlayerProfileManager] Maximum featured achievements reached (5)");
            return false;
        }

        profileData.showcase.featuredAchievements.Add(achievementId);
        Debug.Log($"[PlayerProfileManager] Added featured achievement: {achievementId}");
        return true;
    }

    /// <summary>
    /// Add Hall of Fame entry
    /// </summary>
    public bool AddHallOfFameEntry(string category, string title, string description, int rank, bool isPermanent = false)
    {
        HallOfFameEntry entry = new HallOfFameEntry
        {
            entryId = Guid.NewGuid().ToString(),
            category = category,
            title = title,
            description = description,
            achievedDate = DateTime.Now,
            rank = rank,
            isPermanent = isPermanent
        };

        profileData.showcase.hallOfFame.Add(entry);
        Debug.Log($"[PlayerProfileManager] Added Hall of Fame: {title}");
        return true;
    }

    /// <summary>
    /// Log profile activity
    /// </summary>
    public void LogActivity(ActivityType type, string description)
    {
        ProfileActivity activity = new ProfileActivity
        {
            activityId = Guid.NewGuid().ToString(),
            activityType = type,
            description = description,
            timestamp = DateTime.Now
        };

        profileData.showcase.recentActivity.Insert(0, activity);

        // Keep only last 20 activities
        if (profileData.showcase.recentActivity.Count > 20)
        {
            profileData.showcase.recentActivity.RemoveRange(20, profileData.showcase.recentActivity.Count - 20);
        }
    }

    #endregion

    #region Stats Update

    /// <summary>
    /// Update combat stats
    /// </summary>
    public void UpdateCombatStats(bool won, int damage = 0)
    {
        profileData.stats.totalBattles++;

        if (won)
            profileData.stats.battlesWon++;

        profileData.stats.winRate = (float)profileData.stats.battlesWon / profileData.stats.totalBattles * 100f;

        if (damage > profileData.stats.highestDamage)
            profileData.stats.highestDamage = damage;
    }

    /// <summary>
    /// Update tower progress
    /// </summary>
    public void UpdateTowerProgress(int floor)
    {
        if (floor > profileData.stats.highestTowerFloor)
        {
            profileData.stats.highestTowerFloor = floor;
            LogActivity(ActivityType.DungeonCleared, $"Reached Tower Floor {floor}");
        }
    }

    /// <summary>
    /// Update arena stats
    /// </summary>
    public void UpdateArenaStats(int rank, string tier, bool won)
    {
        profileData.stats.arenaRank = rank;
        profileData.stats.arenaTier = tier;

        if (rank < profileData.stats.arenaHighestRank || profileData.stats.arenaHighestRank == 0)
        {
            profileData.stats.arenaHighestRank = rank;
            LogActivity(ActivityType.RankUp, $"Reached Arena Rank #{rank}");
        }

        if (won)
            profileData.stats.arenaSeasonWins++;
    }

    #endregion

    #region Getters

    /// <summary>
    /// Get profile for display
    /// </summary>
    public ProfileViewData GetProfileViewData()
    {
        return new ProfileViewData
        {
            profileInfo = profileData.profileInfo,
            customization = profileData.customization,
            stats = profileData.stats,
            showcase = profileData.showcase,
            viewedAt = DateTime.Now,
            canSendFriendRequest = true,
            canInspectHeroes = profileData.customization.privacy.showHeroCollection
        };
    }

    public ProfileSaveData GetSaveData()
    {
        return profileData;
    }

    public void LoadSaveData(ProfileSaveData data)
    {
        profileData = data;
        Debug.Log($"[PlayerProfileManager] Loaded profile for {profileData.profileInfo.playerName}");
    }

    #endregion

    #region Default Data

    private List<ProfileTitle> CreateDefaultTitles()
    {
        return new List<ProfileTitle>
        {
            new ProfileTitle { titleId = "beginner", titleText = "Beginner", titleColor = "#FFFFFF", rarity = "common" },
            new ProfileTitle { titleId = "veteran", titleText = "Veteran", titleColor = "#00FF00", rarity = "rare" },
            new ProfileTitle { titleId = "champion", titleText = "Champion", titleColor = "#FFD700", rarity = "epic" },
            new ProfileTitle { titleId = "legend", titleText = "Legend", titleColor = "#FF0000", rarity = "legendary", isAnimated = true }
        };
    }

    private List<ProfileFrame> CreateDefaultFrames()
    {
        return new List<ProfileFrame>
        {
            new ProfileFrame { frameId = "default_frame", frameName = "Default", frameSprite = "frame_default", rarity = "common" },
            new ProfileFrame { frameId = "gold_frame", frameName = "Golden", frameSprite = "frame_gold", rarity = "epic" }
        };
    }

    private List<ProfileBackground> CreateDefaultBackgrounds()
    {
        return new List<ProfileBackground>
        {
            new ProfileBackground { backgroundId = "default_bg", backgroundName = "Default", backgroundImage = "bg_default" }
        };
    }

    #endregion
}
