using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FnGMafia.Core;
using FnGMafia.Chat;

/// <summary>
/// Manages chat cosmetics including emojis, stickers, badges, and effects
/// </summary>
public class ChatCosmeticManager : Singleton<ChatCosmeticManager>
{
    private ChatCosmeticSaveData cosmeticData;
    private ChatCosmeticDatabase cosmeticDatabase;

    protected override void Awake()
    {
        base.Awake();
        InitializeChatCosmetics();
        LoadCosmeticDatabase();
    }

    private void InitializeChatCosmetics()
    {
        cosmeticData = new ChatCosmeticSaveData
        {
            unlockedEmojis = new List<string> { "smile", "thumbs_up", "heart" }, // Default emojis
            unlockedStickers = new List<string>(),
            unlockedBadges = new List<string>(),
            unlockedChatBubbles = new List<string> { "default" },
            unlockedChatEffects = new List<string>(),
            equippedChatBubble = "default",
            favoriteEmojis = new List<string>(),
            equippedBadge = "none"
        };
    }

    private void LoadCosmeticDatabase()
    {
        TextAsset cosmeticData = Resources.Load<TextAsset>("Config/chat_cosmetics");
        if (cosmeticData != null)
        {
            cosmeticDatabase = JsonUtility.FromJson<ChatCosmeticDatabase>(cosmeticData.text);
            Debug.Log($"[ChatCosmeticManager] Loaded chat cosmetics database");
        }
        else
        {
            Debug.LogWarning("[ChatCosmeticManager] Creating default cosmetics database");
            cosmeticDatabase = CreateDefaultCosmeticDatabase();
        }
    }

    #region Unlock System

    /// <summary>
    /// Unlock emoji
    /// </summary>
    public bool UnlockEmoji(string emojiId)
    {
        if (cosmeticData.unlockedEmojis.Contains(emojiId))
        {
            Debug.Log($"[ChatCosmeticManager] Emoji {emojiId} already unlocked");
            return false;
        }

        var emoji = cosmeticDatabase.emojis.Find(e => e.emojiId == emojiId);
        if (emoji == null)
        {
            Debug.LogError($"[ChatCosmeticManager] Emoji {emojiId} not found");
            return false;
        }

        // Check unlock conditions
        if (!CheckUnlockCondition(emoji.unlockCondition))
        {
            Debug.Log($"[ChatCosmeticManager] Unlock conditions not met for {emojiId}");
            return false;
        }

        cosmeticData.unlockedEmojis.Add(emojiId);
        Debug.Log($"[ChatCosmeticManager] Unlocked emoji: {emoji.emojiName}");
        return true;
    }

    /// <summary>
    /// Unlock sticker
    /// </summary>
    public bool UnlockSticker(string stickerId)
    {
        if (cosmeticData.unlockedStickers.Contains(stickerId))
            return false;

        var sticker = cosmeticDatabase.stickers.Find(s => s.stickerId == stickerId);
        if (sticker == null) return false;

        if (!CheckUnlockCondition(sticker.unlockCondition))
            return false;

        cosmeticData.unlockedStickers.Add(stickerId);
        Debug.Log($"[ChatCosmeticManager] Unlocked sticker: {sticker.stickerName}");
        return true;
    }

    /// <summary>
    /// Unlock badge
    /// </summary>
    public bool UnlockBadge(string badgeId)
    {
        if (cosmeticData.unlockedBadges.Contains(badgeId))
            return false;

        var badge = cosmeticDatabase.badges.Find(b => b.badgeId == badgeId);
        if (badge == null) return false;

        if (!CheckUnlockCondition(badge.unlockCondition))
            return false;

        cosmeticData.unlockedBadges.Add(badgeId);
        Debug.Log($"[ChatCosmeticManager] Unlocked badge: {badge.badgeName}");
        return true;
    }

    /// <summary>
    /// Unlock chat bubble style
    /// </summary>
    public bool UnlockChatBubble(string bubbleId)
    {
        if (cosmeticData.unlockedChatBubbles.Contains(bubbleId))
            return false;

        var bubble = cosmeticDatabase.chatBubbles.Find(b => b.bubbleId == bubbleId);
        if (bubble == null) return false;

        if (!CheckUnlockCondition(bubble.unlockCondition))
            return false;

        cosmeticData.unlockedChatBubbles.Add(bubbleId);
        Debug.Log($"[ChatCosmeticManager] Unlocked chat bubble: {bubble.bubbleName}");
        return true;
    }

    /// <summary>
    /// Check if unlock condition is met
    /// </summary>
    private bool CheckUnlockCondition(UnlockCondition condition)
    {
        if (condition == null)
            return true;

        switch (condition.unlockType)
        {
            case UnlockType.Default:
                return true;

            case UnlockType.Achievement:
                // Check with AchievementManager
                if (AchievementManager.Instance != null)
                {
                    // Would check if achievement is completed
                    return true; // Placeholder
                }
                return false;

            case UnlockType.Shop:
                return condition.isPurchasable;

            case UnlockType.Rank:
                // Check player rank
                return true; // Placeholder

            default:
                return false;
        }
    }

    #endregion

    #region Equipment

    /// <summary>
    /// Equip chat bubble style
    /// </summary>
    public bool EquipChatBubble(string bubbleId)
    {
        if (!cosmeticData.unlockedChatBubbles.Contains(bubbleId))
        {
            Debug.Log($"[ChatCosmeticManager] Chat bubble {bubbleId} not unlocked");
            return false;
        }

        cosmeticData.equippedChatBubble = bubbleId;
        Debug.Log($"[ChatCosmeticManager] Equipped chat bubble: {bubbleId}");
        return true;
    }

    /// <summary>
    /// Equip badge
    /// </summary>
    public bool EquipBadge(string badgeId)
    {
        if (badgeId != "none" && !cosmeticData.unlockedBadges.Contains(badgeId))
        {
            Debug.Log($"[ChatCosmeticManager] Badge {badgeId} not unlocked");
            return false;
        }

        cosmeticData.equippedBadge = badgeId;
        Debug.Log($"[ChatCosmeticManager] Equipped badge: {badgeId}");
        return true;
    }

    /// <summary>
    /// Add emoji to favorites
    /// </summary>
    public bool AddFavoriteEmoji(string emojiId)
    {
        if (!cosmeticData.unlockedEmojis.Contains(emojiId))
            return false;

        if (cosmeticData.favoriteEmojis.Contains(emojiId))
            return false;

        if (cosmeticData.favoriteEmojis.Count >= 20) // Max 20 favorites
        {
            Debug.Log("[ChatCosmeticManager] Maximum favorite emojis reached");
            return false;
        }

        cosmeticData.favoriteEmojis.Add(emojiId);
        return true;
    }

    #endregion

    #region Achievement Integration

    /// <summary>
    /// Unlock cosmetics from achievement
    /// </summary>
    public void UnlockFromAchievement(string achievementId)
    {
        // Find all cosmetics unlocked by this achievement
        var emojis = cosmeticDatabase.emojis.Where(e =>
            e.unlockCondition?.unlockType == UnlockType.Achievement &&
            e.unlockCondition?.requiredId == achievementId);

        foreach (var emoji in emojis)
        {
            UnlockEmoji(emoji.emojiId);
        }

        var stickers = cosmeticDatabase.stickers.Where(s =>
            s.unlockCondition?.unlockType == UnlockType.Achievement &&
            s.unlockCondition?.requiredId == achievementId);

        foreach (var sticker in stickers)
        {
            UnlockSticker(sticker.stickerId);
        }

        var badges = cosmeticDatabase.badges.Where(b =>
            b.unlockCondition?.unlockType == UnlockType.Achievement &&
            b.unlockCondition?.requiredId == achievementId);

        foreach (var badge in badges)
        {
            UnlockBadge(badge.badgeId);
        }
    }

    /// <summary>
    /// Unlock cosmetics from event
    /// </summary>
    public void UnlockFromEvent(string eventId, List<string> rewardIds)
    {
        foreach (var rewardId in rewardIds)
        {
            // Try each cosmetic type
            UnlockEmoji(rewardId);
            UnlockSticker(rewardId);
            UnlockBadge(rewardId);
            UnlockChatBubble(rewardId);
        }
    }

    #endregion

    #region Getters

    /// <summary>
    /// Get all unlocked emojis
    /// </summary>
    public List<ChatEmoji> GetUnlockedEmojis()
    {
        return cosmeticDatabase.emojis
            .Where(e => cosmeticData.unlockedEmojis.Contains(e.emojiId))
            .ToList();
    }

    /// <summary>
    /// Get all unlocked stickers
    /// </summary>
    public List<ChatSticker> GetUnlockedStickers()
    {
        return cosmeticDatabase.stickers
            .Where(s => cosmeticData.unlockedStickers.Contains(s.stickerId))
            .ToList();
    }

    /// <summary>
    /// Get all unlocked badges
    /// </summary>
    public List<ChatBadge> GetUnlockedBadges()
    {
        return cosmeticDatabase.badges
            .Where(b => cosmeticData.unlockedBadges.Contains(b.badgeId))
            .ToList();
    }

    /// <summary>
    /// Get equipped cosmetics
    /// </summary>
    public (string chatBubble, string badge) GetEquippedCosmetics()
    {
        return (cosmeticData.equippedChatBubble, cosmeticData.equippedBadge);
    }

    #endregion

    #region Save/Load

    public ChatCosmeticSaveData GetSaveData()
    {
        return cosmeticData;
    }

    public void LoadSaveData(ChatCosmeticSaveData data)
    {
        cosmeticData = data;
        Debug.Log($"[ChatCosmeticManager] Loaded {cosmeticData.unlockedEmojis.Count} emojis");
    }

    #endregion

    #region Default Data

    private ChatCosmeticDatabase CreateDefaultCosmeticDatabase()
    {
        return new ChatCosmeticDatabase
        {
            emojis = new List<ChatEmoji>
            {
                new ChatEmoji { emojiId = "smile", emojiName = "Smile", icon = "emoji_smile", rarity = "common", category = "emotion" },
                new ChatEmoji { emojiId = "thumbs_up", emojiName = "Thumbs Up", icon = "emoji_thumbs", rarity = "common", category = "reaction" },
                new ChatEmoji { emojiId = "heart", emojiName = "Heart", icon = "emoji_heart", rarity = "common", category = "emotion" },
                new ChatEmoji { emojiId = "victory", emojiName = "Victory", icon = "emoji_victory", rarity = "rare", category = "reaction" },
                new ChatEmoji { emojiId = "sparkles", emojiName = "Sparkles", icon = "emoji_sparkles", isAnimated = true, rarity = "epic", category = "special" }
            },
            stickers = new List<ChatSticker>(),
            badges = new List<ChatBadge>(),
            chatBubbles = new List<ChatBubbleStyle>
            {
                new ChatBubbleStyle { bubbleId = "default", bubbleName = "Default", frameSprite = "bubble_default" }
            },
            chatEffects = new List<ChatEffect>()
        };
    }

    #endregion
}
