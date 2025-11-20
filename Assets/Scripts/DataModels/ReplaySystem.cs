using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extended replay system for Arena, World Boss, Raids, and general battles
/// </summary>
namespace FnGMafia.Replay
{
    [Serializable]
    public class ReplaySaveData
    {
        public List<ExtendedBattleReplay> savedReplays = new List<ExtendedBattleReplay>();
        public List<string> favoriteReplays = new List<string>();
        public Dictionary<string, int> replayViews = new Dictionary<string, int>();
        public int maxStoredReplays = 50;
    }

    /// <summary>
    /// Extended replay data supporting different battle types
    /// </summary>
    [Serializable]
    public class ExtendedBattleReplay
    {
        public string replayId;
        public BattleType battleType;          // Arena, WorldBoss, Raid, Dungeon, Story
        public DateTime battleDate;
        public bool isTopReplay;               // Featured/top ranking replays
        public bool isFavorite;
        public int viewCount;

        // Player info
        public string playerId;
        public string playerName;
        public int playerLevel;

        // Teams
        public List<ReplayHero> attackerTeam;
        public List<ReplayHero> defenderTeam;

        // For World Boss/Raid
        public string bossId;
        public long damageDealt;
        public float timeElapsed;
        public int rankAchieved;

        // Turn-by-turn recording
        public List<ReplayTurn> turns;

        // Battle results
        public int winnerTeam;                 // 0 = attacker, 1 = defender
        public float battleSpeed;

        // Formation/Team comp data for copy feature
        public TeamFormation attackerFormation;
        public TeamFormation defenderFormation;

        // Metadata
        public string title;                   // Custom replay title
        public List<string> tags;              // Tags like "one_shot", "comeback", "f2p"
        public int totalPower;
        public string difficulty;
    }

    public enum BattleType
    {
        Arena,
        WorldBoss,
        GuildRaid,
        Dungeon,
        Story,
        CoOp,
        Trial
    }

    /// <summary>
    /// Team formation data for "Copy Formation" feature
    /// </summary>
    [Serializable]
    public class TeamFormation
    {
        public List<HeroFormationSlot> heroes = new List<HeroFormationSlot>();
        public string formationName;
        public int totalPower;
        public List<string> elements;          // Elements represented in team
        public string strategy;                // "offensive", "defensive", "balanced"
    }

    [Serializable]
    public class HeroFormationSlot
    {
        public int slotPosition;               // 0-4 for typical team
        public string heroId;
        public string heroName;
        public int level;
        public int stars;
        public string primaryElement;

        // Gear snapshot
        public List<string> equippedGearIds;
        public List<string> equippedArtifactIds;

        // Stats snapshot
        public Dictionary<string, int> stats;

        // Skills/Talents
        public List<string> activeSkills;
        public List<string> talentNodes;       // Unlocked talent nodes
    }

    /// <summary>
    /// Replay filters and search
    /// </summary>
    [Serializable]
    public class ReplayFilter
    {
        public BattleType? battleType;
        public int? minPower;
        public int? maxPower;
        public List<string> requiredHeroes;    // Filter by heroes used
        public List<string> elements;          // Filter by team element
        public DateTime? fromDate;
        public DateTime? toDate;
        public bool onlyTopReplays;
        public bool onlyFavorites;
        public string searchTerm;              // Search by title/player name
    }

    /// <summary>
    /// Replay collection for showcasing top plays
    /// </summary>
    [Serializable]
    public class ReplayCollection
    {
        public string collectionId;
        public string collectionName;
        public string description;
        public List<string> replayIds;
        public string collectionType;          // "weekly_top", "hall_of_fame", "tutorial"
        public DateTime startDate;
        public DateTime endDate;
    }

    /// <summary>
    /// Replay playback controls
    /// </summary>
    [Serializable]
    public class ReplayPlaybackState
    {
        public string currentReplayId;
        public int currentTurn;
        public bool isPlaying;
        public bool isPaused;
        public float playbackSpeed;            // 1x, 1.5x, 2x, 3x
        public bool skipAnimations;
    }

    /// <summary>
    /// Replay sharing/social features
    /// </summary>
    [Serializable]
    public class ReplayShare
    {
        public string replayId;
        public string sharedBy;
        public DateTime sharedDate;
        public string shareMessage;
        public List<string> sharedWith;        // Friend IDs or "guild", "public"
    }

    /// <summary>
    /// Leaderboard entry with replay
    /// </summary>
    [Serializable]
    public class LeaderboardReplayEntry
    {
        public int rank;
        public string playerId;
        public string playerName;
        public int score;                      // Arena rating, boss damage, etc.
        public string replayId;
        public bool hasReplay;
        public DateTime achievedDate;
    }
}
