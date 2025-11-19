using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chat cosmetics including emojis, stickers, animated badges, and chat effects
/// </summary>
namespace FnGMafia.Chat
{
    [Serializable]
    public class ChatCosmeticSaveData
    {
        public List<string> unlockedEmojis = new List<string>();
        public List<string> unlockedStickers = new List<string>();
        public List<string> unlockedBadges = new List<string>();
        public List<string> unlockedChatBubbles = new List<string>();
        public List<string> unlockedChatEffects = new List<string>();

        // Equipped cosmetics
        public string equippedChatBubble = "default";
        public List<string> favoriteEmojis = new List<string>();
        public string equippedBadge = "none";
    }

    [Serializable]
    public class ChatCosmeticDatabase
    {
        public List<ChatEmoji> emojis = new List<ChatEmoji>();
        public List<ChatSticker> stickers = new List<ChatSticker>();
        public List<ChatBadge> badges = new List<ChatBadge>();
        public List<ChatBubbleStyle> chatBubbles = new List<ChatBubbleStyle>();
        public List<ChatEffect> chatEffects = new List<ChatEffect>();
    }

    [Serializable]
    public class ChatEmoji
    {
        public string emojiId;
        public string emojiName;
        public string icon;                    // Sprite path
        public bool isAnimated;
        public string rarity;                  // common, rare, epic, legendary
        public UnlockCondition unlockCondition;
        public string category;                // "emotion", "reaction", "event", "special"
    }

    [Serializable]
    public class ChatSticker
    {
        public string stickerId;
        public string stickerName;
        public string spriteSheet;             // Animated sprite sheet
        public int frameCount;                 // For animations
        public float animationSpeed;
        public string rarity;
        public UnlockCondition unlockCondition;
        public string category;                // "hero", "funny", "seasonal", "limited"
        public bool isLimitedTime;
        public DateTime? availableUntil;
    }

    [Serializable]
    public class ChatBadge
    {
        public string badgeId;
        public string badgeName;
        public string description;
        public string icon;
        public bool isAnimated;
        public string animationType;           // "glow", "pulse", "sparkle", "rotate"
        public string rarity;
        public UnlockCondition unlockCondition;
        public BadgeType badgeType;
    }

    public enum BadgeType
    {
        Achievement,                           // From achievements
        Rank,                                  // Arena/PvP rank
        Event,                                 // Event participant
        VIP,                                   // VIP tier
        Seasonal,                              // Season-specific
        Special                                // Limited edition
    }

    [Serializable]
    public class ChatBubbleStyle
    {
        public string bubbleId;
        public string bubbleName;
        public string frameSprite;             // Border/frame sprite
        public string backgroundColor;         // Hex color
        public string textColor;               // Hex color
        public bool hasParticleEffect;
        public string particleEffect;          // Particle system prefab
        public string rarity;
        public UnlockCondition unlockCondition;
    }

    [Serializable]
    public class ChatEffect
    {
        public string effectId;
        public string effectName;
        public string description;
        public string effectType;              // "on_send", "on_receive", "ambient"
        public string visualEffect;            // Particle/animation prefab
        public string soundEffect;             // Audio clip
        public string rarity;
        public UnlockCondition unlockCondition;
    }

    [Serializable]
    public class UnlockCondition
    {
        public UnlockType unlockType;
        public string requiredId;              // Achievement ID, event ID, etc.
        public int requiredLevel;
        public int requiredCurrency;
        public string currencyType;            // "friend_points", "gems", "event_tokens"
        public bool isPurchasable;
        public int purchaseCost;
        public string purchaseCurrency;
    }

    public enum UnlockType
    {
        Default,                               // Always available
        Achievement,                           // Complete specific achievement
        Event,                                 // Event reward
        Shop,                                  // Purchase from shop
        Rank,                                  // Reach specific rank
        SeasonPass,                            // Season pass reward
        Limited,                               // Limited time offer
        Special                                // Special conditions
    }

    /// <summary>
    /// Chat message with cosmetics applied
    /// </summary>
    [Serializable]
    public class StyledChatMessage
    {
        public string messageId;
        public string senderId;
        public string senderName;
        public string messageText;
        public DateTime timestamp;

        // Applied cosmetics
        public string chatBubbleStyle;
        public List<string> emojisUsed = new List<string>();
        public string stickerAttached;
        public string senderBadge;
        public string chatEffect;

        // Reactions
        public Dictionary<string, int> reactions = new Dictionary<string, int>();  // emoji -> count
    }

    /// <summary>
    /// Emoji pack/collection
    /// </summary>
    [Serializable]
    public class EmojiPack
    {
        public string packId;
        public string packName;
        public string description;
        public List<string> emojiIds;
        public string packIcon;
        public UnlockCondition unlockCondition;
        public bool isComplete;                // Player has all emojis
    }
}
