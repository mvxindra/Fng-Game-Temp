using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unified player profile system with customization and stats showcase
/// </summary>
namespace FnGMafia.Profile
{
    [Serializable]
    public class ProfileSaveData
    {
        public PlayerProfileInfo profileInfo = new PlayerProfileInfo();
        public ProfileCustomization customization = new ProfileCustomization();
        public ProfileStats stats = new ProfileStats();
        public ProfileShowcase showcase = new ProfileShowcase();
    }

    [Serializable]
    public class PlayerProfileInfo
    {
        public string playerId;
        public string playerName;
        public int playerLevel;
        public int experience;
        public int experienceToNext;
        public DateTime accountCreatedDate;
        public DateTime lastLoginDate;
        public int totalPlayTime;              // Minutes
        public string guildId;
        public string guildName;
        public string guildRole;
    }

    [Serializable]
    public class ProfileCustomization
    {
        // Visual customization
        public string profileIcon;             // Avatar/portrait
        public string profileFrame;            // Border frame
        public string profileBackground;       // Background image
        public string nameplate;               // Nameplate style

        // Titles
        public string equippedTitle;
        public List<string> unlockedTitles = new List<string>();

        // Frames
        public List<string> unlockedFrames = new List<string>();

        // Backgrounds
        public List<string> unlockedBackgrounds = new List<string>();

        // Status/Bio
        public string statusMessage;           // Custom status
        public string bio;                     // Profile bio

        // Privacy settings
        public ProfilePrivacy privacy = new ProfilePrivacy();
    }

    [Serializable]
    public class ProfilePrivacy
    {
        public bool showOnlineStatus = true;
        public bool allowProfileViews = true;
        public bool showHeroCollection = true;
        public bool showStats = true;
        public PrivacyLevel profileVisibility = PrivacyLevel.Everyone;
    }

    public enum PrivacyLevel
    {
        Everyone,
        FriendsOnly,
        GuildOnly,
        Private
    }

    [Serializable]
    public class ProfileStats
    {
        // Account stats
        public int accountLevel;
        public int vipLevel;
        public int totalAchievements;
        public int achievementPoints;

        // Combat stats
        public int totalBattles;
        public int battlesWon;
        public float winRate;
        public int highestDamage;
        public int longestCombo;

        // Hero collection
        public int totalHeroesOwned;
        public int totalHeroesMaxLevel;
        public int totalHeroesMaxStars;
        public int uniqueHeroes;

        // Progression stats
        public int highestTowerFloor;
        public int highestDungeonLevel;
        public string highestDungeonCleared;

        // PvP stats
        public int arenaRank;
        public string arenaTier;
        public int arenaHighestRank;
        public int arenaSeasonWins;

        // Guild stats
        public int guildContribution;
        public int guildRaidRank;
        public int guildBossKills;

        // World Boss stats
        public long worldBossHighestDamage;
        public int worldBossBestRank;
        public string worldBossKillCount;

        // General stats
        public int totalLogins;
        public int consecutiveLogins;
        public int friendCount;
        public int eventsCompleted;
    }

    [Serializable]
    public class ProfileShowcase
    {
        // Featured heroes (up to 3)
        public List<ShowcaseHero> featuredHeroes = new List<ShowcaseHero>();

        // Featured achievements (up to 5)
        public List<string> featuredAchievements = new List<string>();

        // Hall of Fame entries
        public List<HallOfFameEntry> hallOfFame = new List<HallOfFameEntry>();

        // Recent activity
        public List<ProfileActivity> recentActivity = new List<ProfileActivity>();
    }

    [Serializable]
    public class ShowcaseHero
    {
        public int slotIndex;                  // 0-2
        public string heroId;
        public string heroName;
        public int level;
        public int stars;
        public int power;
        public string primaryElement;

        // Display settings
        public string pose;                    // Idle, attack, victory
        public bool show3DModel;               // Use 3D or 2D sprite
        public string backgroundEffect;        // Particles/effects
    }

    [Serializable]
    public class HallOfFameEntry
    {
        public string entryId;
        public string category;                // "arena_champion", "world_boss_top10", etc.
        public string title;
        public string description;
        public DateTime achievedDate;
        public int rank;
        public string icon;
        public bool isPermanent;               // Some entries may expire
    }

    [Serializable]
    public class ProfileActivity
    {
        public string activityId;
        public ActivityType activityType;
        public string description;
        public DateTime timestamp;
        public string icon;
    }

    public enum ActivityType
    {
        HeroObtained,
        AchievementUnlocked,
        RankUp,
        BossDefeated,
        DungeonCleared,
        FriendAdded,
        EventCompleted
    }

    /// <summary>
    /// Profile title configuration
    /// </summary>
    [Serializable]
    public class ProfileTitle
    {
        public string titleId;
        public string titleText;
        public string titleColor;              // Hex color
        public string rarity;
        public bool isAnimated;
        public string effect;                  // Visual effect
        public UnlockSource unlockSource;
    }

    [Serializable]
    public class UnlockSource
    {
        public string sourceType;              // "achievement", "rank", "event", "purchase"
        public string sourceId;
        public string description;
    }

    /// <summary>
    /// Profile frame configuration
    /// </summary>
    [Serializable]
    public class ProfileFrame
    {
        public string frameId;
        public string frameName;
        public string frameSprite;
        public bool isAnimated;
        public string rarity;
        public UnlockSource unlockSource;
    }

    /// <summary>
    /// Profile background configuration
    /// </summary>
    [Serializable]
    public class ProfileBackground
    {
        public string backgroundId;
        public string backgroundName;
        public string backgroundImage;
        public bool hasParticles;
        public string particleEffect;
        public string rarity;
        public UnlockSource unlockSource;
    }

    /// <summary>
    /// Profile viewing/inspection
    /// </summary>
    [Serializable]
    public class ProfileViewData
    {
        public PlayerProfileInfo profileInfo;
        public ProfileCustomization customization;
        public ProfileStats stats;
        public ProfileShowcase showcase;
        public DateTime viewedAt;
        public bool canSendFriendRequest;
        public bool canInspectHeroes;
    }
}
