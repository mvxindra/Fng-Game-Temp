using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Comprehensive Friend System with support heroes, friend points, and social features
/// </summary>
namespace FnGMafia.Social
{
    [Serializable]
    public class FriendSaveData
    {
        public List<FriendEntry> friends = new List<FriendEntry>();
        public List<FriendRequest> pendingRequests = new List<FriendRequest>();
        public List<FriendRequest> sentRequests = new List<FriendRequest>();
        public int friendPoints;
        public List<SupportHeroRental> supportRentals = new List<SupportHeroRental>();
        public DateTime lastDailyReset;
        public List<string> blockedPlayers = new List<string>();
        public FriendSettings settings = new FriendSettings();
    }

    [Serializable]
    public class FriendEntry
    {
        public string playerId;
        public string playerName;
        public int playerLevel;
        public DateTime friendsSince;
        public DateTime lastOnline;
        public bool isOnline;
        public int friendPoints;           // Points earned from this friend
        public bool isFavorite;

        // Profile info
        public string profileIcon;
        public string profileFrame;
        public string title;
        public int totalPower;

        // Support hero
        public SupportHero supportHero;

        // Social stats
        public int timesHelped;            // Times you used their support hero
        public int timesHelpedYou;         // Times they used your support hero
        public DateTime lastInteraction;
    }

    [Serializable]
    public class SupportHero
    {
        public string heroId;
        public string heroName;
        public int level;
        public int stars;
        public int power;
        public string primaryElement;

        // Equipment snapshot (for display)
        public List<string> equippedGearIds = new List<string>();

        // Stats snapshot
        public int atk;
        public int def;
        public int hp;
        public int spd;
    }

    [Serializable]
    public class FriendRequest
    {
        public string requestId;
        public string fromPlayerId;
        public string fromPlayerName;
        public int fromPlayerLevel;
        public string profileIcon;
        public DateTime requestTime;
        public string message;             // Optional message with request
        public int mutualFriends;          // Number of mutual friends
    }

    [Serializable]
    public class SupportHeroRental
    {
        public string rentalId;
        public string friendId;
        public string heroId;
        public DateTime rentedAt;
        public bool wasUsed;
        public int friendPointsEarned;
    }

    [Serializable]
    public class FriendSettings
    {
        public bool acceptFriendRequests = true;
        public bool showOnlineStatus = true;
        public bool allowSupportHeroUse = true;
        public FriendRequestPrivacy requestPrivacy = FriendRequestPrivacy.Everyone;
    }

    public enum FriendRequestPrivacy
    {
        Everyone,
        MutualFriends,
        GuildMembers,
        None
    }

    [Serializable]
    public class FriendShopDatabase
    {
        public List<FriendShopItem> items = new List<FriendShopItem>();
        public List<FriendShopRefresh> refreshSchedule = new List<FriendShopRefresh>();
    }

    [Serializable]
    public class FriendShopItem
    {
        public string itemId;
        public string itemName;
        public string itemType;            // "material", "cosmetic", "shard", "hero"
        public string itemReference;       // ID of the actual item
        public int quantity;
        public int friendPointCost;
        public int stock;                  // -1 for unlimited
        public int dailyLimit;             // Max purchases per day, -1 for unlimited
        public int totalLimit;             // Total lifetime purchases, -1 for unlimited
        public int requiredFriendLevel;    // Unlock at certain friend count
        public string rarity;
        public string icon;
        public bool isFeatured;
    }

    [Serializable]
    public class FriendShopRefresh
    {
        public string refreshId;
        public int dayOfWeek;              // 0-6 for weekly rotation
        public List<string> featuredItems; // Item IDs featured this day
    }

    [Serializable]
    public class FriendPointsConfig
    {
        public int pointsForSupportUsed = 25;        // When friend uses your support hero
        public int pointsForUsingSupportin = 10;     // When you use friend's support hero
        public int pointsForDailyGift = 5;           // Send/receive daily gifts
        public int pointsForCoopComplete = 50;       // Complete co-op content together
        public int maxDailyPoints = 500;             // Daily cap
        public int maxStoredPoints = 10000;          // Maximum stored friend points
    }

    /// <summary>
    /// Friend recommendation system
    /// </summary>
    [Serializable]
    public class FriendRecommendation
    {
        public string playerId;
        public string playerName;
        public int playerLevel;
        public string profileIcon;
        public int mutualFriends;
        public int totalPower;
        public string recommendReason;     // "guild_member", "similar_level", "active_player"
        public float matchScore;           // 0-1 compatibility score
        public SupportHero supportHero;
    }

    /// <summary>
    /// Daily gift system
    /// </summary>
    [Serializable]
    public class DailyGift
    {
        public string giftId;
        public string fromPlayerId;
        public string fromPlayerName;
        public DateTime sentTime;
        public bool claimed;
        public List<GiftReward> rewards = new List<GiftReward>();
    }

    [Serializable]
    public class GiftReward
    {
        public string rewardType;          // "gold", "stamina", "friend_points"
        public string rewardId;
        public int quantity;
    }

    /// <summary>
    /// Co-op invitation
    /// </summary>
    [Serializable]
    public class CoopInvitation
    {
        public string inviteId;
        public string fromPlayerId;
        public string fromPlayerName;
        public string contentType;         // "dungeon", "raid", "world_boss"
        public string contentId;
        public DateTime inviteTime;
        public DateTime expiryTime;
        public int requiredPower;
    }
}
