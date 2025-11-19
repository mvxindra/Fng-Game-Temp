using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FnGMafia.Core;
using FnGMafia.Social;

/// <summary>
/// Manages friend list, support heroes, friend points, and social features
/// </summary>
public class FriendManager : Singleton<FriendManager>
{
    private FriendSaveData friendData;
    private FriendShopDatabase friendShop;
    private FriendPointsConfig pointsConfig;

    // Friend limits
    private const int MAX_FRIENDS = 100;
    private const int MAX_PENDING_REQUESTS = 50;

    // Shop purchase tracking
    private Dictionary<string, int> dailyShopPurchases = new Dictionary<string, int>();
    private Dictionary<string, int> totalShopPurchases = new Dictionary<string, int>();

    protected override void Awake()
    {
        base.Awake();
        InitializeFriendSystem();
        LoadFriendShopDatabase();
        LoadPointsConfig();
    }

    private void InitializeFriendSystem()
    {
        friendData = new FriendSaveData
        {
            friends = new List<FriendEntry>(),
            pendingRequests = new List<FriendRequest>(),
            sentRequests = new List<FriendRequest>(),
            friendPoints = 0,
            supportRentals = new List<SupportHeroRental>(),
            lastDailyReset = DateTime.Now,
            blockedPlayers = new List<string>(),
            settings = new FriendSettings()
        };
    }

    private void LoadFriendShopDatabase()
    {
        TextAsset shopData = Resources.Load<TextAsset>("Config/friend_shop");
        if (shopData != null)
        {
            friendShop = JsonUtility.FromJson<FriendShopDatabase>(shopData.text);
            Debug.Log($"[FriendManager] Loaded {friendShop.items.Count} friend shop items");
        }
        else
        {
            Debug.LogWarning("[FriendManager] Friend shop config not found, creating default");
            friendShop = CreateDefaultFriendShop();
        }
    }

    private void LoadPointsConfig()
    {
        pointsConfig = new FriendPointsConfig();
        Debug.Log("[FriendManager] Friend points config loaded");
    }

    #region Friend Management

    /// <summary>
    /// Send friend request
    /// </summary>
    public bool SendFriendRequest(string targetPlayerId, string message = "")
    {
        // Check if already friends
        if (IsFriend(targetPlayerId))
        {
            Debug.Log("[FriendManager] Already friends with this player");
            return false;
        }

        // Check if request already sent
        if (friendData.sentRequests.Any(r => r.fromPlayerId == targetPlayerId))
        {
            Debug.Log("[FriendManager] Friend request already sent");
            return false;
        }

        // Check if at max sent requests
        if (friendData.sentRequests.Count >= MAX_PENDING_REQUESTS)
        {
            Debug.Log("[FriendManager] Too many pending requests");
            return false;
        }

        // Create friend request (In real implementation, this would be sent to server)
        FriendRequest request = new FriendRequest
        {
            requestId = Guid.NewGuid().ToString(),
            fromPlayerId = "player_local", // Would be actual player ID
            fromPlayerName = "Player",     // Would be actual player name
            fromPlayerLevel = 50,          // Would be actual level
            profileIcon = "default_icon",
            requestTime = DateTime.Now,
            message = message,
            mutualFriends = 0              // Calculate mutual friends
        };

        friendData.sentRequests.Add(request);
        Debug.Log($"[FriendManager] Sent friend request to {targetPlayerId}");
        return true;
    }

    /// <summary>
    /// Accept friend request
    /// </summary>
    public bool AcceptFriendRequest(string requestId)
    {
        var request = friendData.pendingRequests.Find(r => r.requestId == requestId);
        if (request == null)
        {
            Debug.Log("[FriendManager] Friend request not found");
            return false;
        }

        // Check friend limit
        if (friendData.friends.Count >= MAX_FRIENDS)
        {
            Debug.Log("[FriendManager] Friend list is full");
            return false;
        }

        // Create friend entry
        FriendEntry newFriend = new FriendEntry
        {
            playerId = request.fromPlayerId,
            playerName = request.fromPlayerName,
            playerLevel = request.fromPlayerLevel,
            friendsSince = DateTime.Now,
            lastOnline = DateTime.Now,
            isOnline = true,
            friendPoints = 0,
            isFavorite = false,
            profileIcon = request.profileIcon,
            timesHelped = 0,
            timesHelpedYou = 0,
            lastInteraction = DateTime.Now,
            supportHero = new SupportHero() // Would load actual support hero
        };

        friendData.friends.Add(newFriend);
        friendData.pendingRequests.Remove(request);

        Debug.Log($"[FriendManager] Accepted friend request from {request.fromPlayerName}");
        return true;
    }

    /// <summary>
    /// Reject friend request
    /// </summary>
    public bool RejectFriendRequest(string requestId)
    {
        var request = friendData.pendingRequests.Find(r => r.requestId == requestId);
        if (request == null) return false;

        friendData.pendingRequests.Remove(request);
        Debug.Log($"[FriendManager] Rejected friend request from {request.fromPlayerName}");
        return true;
    }

    /// <summary>
    /// Remove friend
    /// </summary>
    public bool RemoveFriend(string playerId)
    {
        var friend = friendData.friends.Find(f => f.playerId == playerId);
        if (friend == null) return false;

        friendData.friends.Remove(friend);
        Debug.Log($"[FriendManager] Removed friend: {friend.playerName}");
        return true;
    }

    /// <summary>
    /// Toggle favorite friend
    /// </summary>
    public bool ToggleFavorite(string playerId)
    {
        var friend = friendData.friends.Find(f => f.playerId == playerId);
        if (friend == null) return false;

        friend.isFavorite = !friend.isFavorite;
        Debug.Log($"[FriendManager] {friend.playerName} favorite: {friend.isFavorite}");
        return true;
    }

    /// <summary>
    /// Block player
    /// </summary>
    public bool BlockPlayer(string playerId)
    {
        if (friendData.blockedPlayers.Contains(playerId)) return false;

        // Remove from friends if they are friends
        RemoveFriend(playerId);

        friendData.blockedPlayers.Add(playerId);
        Debug.Log($"[FriendManager] Blocked player: {playerId}");
        return true;
    }

    #endregion

    #region Support Hero System

    /// <summary>
    /// Rent support hero from friend (once per day per friend)
    /// </summary>
    public SupportHero RentSupportHero(string friendId)
    {
        var friend = friendData.friends.Find(f => f.playerId == friendId);
        if (friend == null)
        {
            Debug.Log("[FriendManager] Friend not found");
            return null;
        }

        // Check if already rented today
        var todayRental = friendData.supportRentals.Find(r =>
            r.friendId == friendId &&
            r.rentedAt.Date == DateTime.Now.Date);

        if (todayRental != null)
        {
            Debug.Log("[FriendManager] Already rented support hero from this friend today");
            return null;
        }

        // Create rental record
        SupportHeroRental rental = new SupportHeroRental
        {
            rentalId = Guid.NewGuid().ToString(),
            friendId = friendId,
            heroId = friend.supportHero.heroId,
            rentedAt = DateTime.Now,
            wasUsed = false,
            friendPointsEarned = 0
        };

        friendData.supportRentals.Add(rental);
        friend.lastInteraction = DateTime.Now;

        Debug.Log($"[FriendManager] Rented {friend.supportHero.heroName} from {friend.playerName}");
        return friend.supportHero;
    }

    /// <summary>
    /// Complete battle with rented support hero
    /// </summary>
    public void CompleteBattleWithSupport(string rentalId, bool victory)
    {
        var rental = friendData.supportRentals.Find(r => r.rentalId == rentalId);
        if (rental == null || rental.wasUsed) return;

        rental.wasUsed = true;

        if (victory)
        {
            // Award friend points
            int points = pointsConfig.pointsForUsingSupportin;
            AddFriendPoints(points);
            rental.friendPointsEarned = points;

            // Update friend stats
            var friend = friendData.friends.Find(f => f.playerId == rental.friendId);
            if (friend != null)
            {
                friend.timesHelped++;
                friend.friendPoints += pointsConfig.pointsForSupportUsed;
                Debug.Log($"[FriendManager] {friend.playerName} earned {pointsConfig.pointsForSupportUsed} friend points");
            }
        }
    }

    /// <summary>
    /// Set your support hero for friends to use
    /// </summary>
    public bool SetSupportHero(string heroId)
    {
        // In real implementation, would load actual hero data
        // For now, create a sample support hero
        Debug.Log($"[FriendManager] Support hero set to: {heroId}");
        return true;
    }

    #endregion

    #region Friend Points

    /// <summary>
    /// Add friend points
    /// </summary>
    public void AddFriendPoints(int amount)
    {
        // Check daily cap
        // In real implementation, track daily earnings

        friendData.friendPoints = Mathf.Min(
            friendData.friendPoints + amount,
            pointsConfig.maxStoredPoints
        );

        Debug.Log($"[FriendManager] +{amount} friend points (Total: {friendData.friendPoints})");
    }

    /// <summary>
    /// Spend friend points
    /// </summary>
    public bool SpendFriendPoints(int amount)
    {
        if (friendData.friendPoints < amount)
        {
            Debug.Log("[FriendManager] Insufficient friend points");
            return false;
        }

        friendData.friendPoints -= amount;
        Debug.Log($"[FriendManager] Spent {amount} friend points (Remaining: {friendData.friendPoints})");
        return true;
    }

    /// <summary>
    /// Get current friend points
    /// </summary>
    public int GetFriendPoints()
    {
        return friendData.friendPoints;
    }

    #endregion

    #region Friend Shop

    /// <summary>
    /// Purchase from friend shop
    /// </summary>
    public bool PurchaseFriendShopItem(string itemId)
    {
        var item = friendShop.items.Find(i => i.itemId == itemId);
        if (item == null)
        {
            Debug.Log("[FriendManager] Item not found in friend shop");
            return false;
        }

        // Check friend level requirement
        if (friendData.friends.Count < item.requiredFriendLevel)
        {
            Debug.Log($"[FriendManager] Requires {item.requiredFriendLevel} friends");
            return false;
        }

        // Check daily limit
        if (item.dailyLimit > 0)
        {
            int dailyPurchased = dailyShopPurchases.ContainsKey(itemId) ? dailyShopPurchases[itemId] : 0;
            if (dailyPurchased >= item.dailyLimit)
            {
                Debug.Log("[FriendManager] Daily purchase limit reached");
                return false;
            }
        }

        // Check total limit
        if (item.totalLimit > 0)
        {
            int totalPurchased = totalShopPurchases.ContainsKey(itemId) ? totalShopPurchases[itemId] : 0;
            if (totalPurchased >= item.totalLimit)
            {
                Debug.Log("[FriendManager] Total purchase limit reached");
                return false;
            }
        }

        // Check stock
        if (item.stock == 0)
        {
            Debug.Log("[FriendManager] Item out of stock");
            return false;
        }

        // Check cost
        if (!SpendFriendPoints(item.friendPointCost))
        {
            return false;
        }

        // Give item (integrate with inventory system)
        GiveShopItem(item);

        // Update purchase tracking
        if (!dailyShopPurchases.ContainsKey(itemId))
            dailyShopPurchases[itemId] = 0;
        dailyShopPurchases[itemId]++;

        if (!totalShopPurchases.ContainsKey(itemId))
            totalShopPurchases[itemId] = 0;
        totalShopPurchases[itemId]++;

        // Update stock
        if (item.stock > 0)
            item.stock--;

        Debug.Log($"[FriendManager] Purchased {item.itemName} from friend shop");
        return true;
    }

    private void GiveShopItem(FriendShopItem item)
    {
        // Integrate with actual inventory systems
        Debug.Log($"[FriendManager] Received {item.quantity}x {item.itemName}");

        // Example integration:
        // - MaterialInventory for materials
        // - HeroCollection for hero shards
        // - CosmeticManager for cosmetics
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Check if player is friend
    /// </summary>
    public bool IsFriend(string playerId)
    {
        return friendData.friends.Any(f => f.playerId == playerId);
    }

    /// <summary>
    /// Get friend list
    /// </summary>
    public List<FriendEntry> GetFriends()
    {
        return new List<FriendEntry>(friendData.friends);
    }

    /// <summary>
    /// Get favorite friends
    /// </summary>
    public List<FriendEntry> GetFavoriteFriends()
    {
        return friendData.friends.Where(f => f.isFavorite).ToList();
    }

    /// <summary>
    /// Get online friends
    /// </summary>
    public List<FriendEntry> GetOnlineFriends()
    {
        return friendData.friends.Where(f => f.isOnline).ToList();
    }

    /// <summary>
    /// Get pending friend requests
    /// </summary>
    public List<FriendRequest> GetPendingRequests()
    {
        return new List<FriendRequest>(friendData.pendingRequests);
    }

    /// <summary>
    /// Get friend shop items
    /// </summary>
    public List<FriendShopItem> GetFriendShopItems()
    {
        return new List<FriendShopItem>(friendShop.items);
    }

    /// <summary>
    /// Reset daily limits
    /// </summary>
    public void ResetDaily()
    {
        DateTime now = DateTime.Now;
        if ((now - friendData.lastDailyReset).TotalDays >= 1)
        {
            friendData.supportRentals.Clear();
            dailyShopPurchases.Clear();
            friendData.lastDailyReset = now;
            Debug.Log("[FriendManager] Daily reset complete");
        }
    }

    #endregion

    #region Save/Load

    public FriendSaveData GetSaveData()
    {
        return friendData;
    }

    public void LoadSaveData(FriendSaveData data)
    {
        friendData = data;
        Debug.Log($"[FriendManager] Loaded {friendData.friends.Count} friends");
    }

    #endregion

    #region Default Data

    private FriendShopDatabase CreateDefaultFriendShop()
    {
        return new FriendShopDatabase
        {
            items = new List<FriendShopItem>
            {
                new FriendShopItem
                {
                    itemId = "fp_gold_1000",
                    itemName = "1000 Gold",
                    itemType = "currency",
                    itemReference = "gold",
                    quantity = 1000,
                    friendPointCost = 100,
                    stock = -1,
                    dailyLimit = 10,
                    totalLimit = -1,
                    requiredFriendLevel = 0,
                    rarity = "common"
                },
                new FriendShopItem
                {
                    itemId = "fp_hero_shard",
                    itemName = "Universal Hero Shard",
                    itemType = "shard",
                    itemReference = "universal_shard",
                    quantity = 1,
                    friendPointCost = 250,
                    stock = 10,
                    dailyLimit = 2,
                    totalLimit = -1,
                    requiredFriendLevel = 5,
                    rarity = "rare"
                }
            },
            refreshSchedule = new List<FriendShopRefresh>()
        };
    }

    #endregion
}
