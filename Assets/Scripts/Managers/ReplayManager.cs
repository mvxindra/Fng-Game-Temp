using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FnGMafia.Core;
using FnGMafia.Replay;

/// <summary>
/// Manages battle replays, spectating, and formation copying
/// </summary>
public class ReplayManager : Singleton<ReplayManager>
{
    private ReplaySaveData replayData;
    private ReplayPlaybackState playbackState;

    // Featured/Top replays (loaded from server or config)
    private List<ExtendedBattleReplay> topArenaReplays = new List<ExtendedBattleReplay>();
    private List<ExtendedBattleReplay> topWorldBossReplays = new List<ExtendedBattleReplay>();

    // Collections
    private List<ReplayCollection> replayCollections = new List<ReplayCollection>();

    protected override void Awake()
    {
        base.Awake();
        InitializeReplaySystem();
    }

    private void InitializeReplaySystem()
    {
        replayData = new ReplaySaveData
        {
            savedReplays = new List<ExtendedBattleReplay>(),
            favoriteReplays = new List<string>(),
            replayViews = new Dictionary<string, int>(),
            maxStoredReplays = 50
        };

        playbackState = new ReplayPlaybackState
        {
            playbackSpeed = 1.0f,
            skipAnimations = false
        };

        LoadTopReplays();
    }

    private void LoadTopReplays()
    {
        // In real implementation, load from server
        // For now, log initialization
        Debug.Log("[ReplayManager] Replay system initialized");
    }

    #region Replay Recording

    /// <summary>
    /// Save a battle as a replay
    /// </summary>
    public string SaveBattleReplay(ExtendedBattleReplay replay)
    {
        if (replay.replayId == null)
        {
            replay.replayId = Guid.NewGuid().ToString();
        }

        replay.battleDate = DateTime.Now;

        // Check storage limit
        if (replayData.savedReplays.Count >= replayData.maxStoredReplays)
        {
            // Remove oldest non-favorite replay
            var oldestReplay = replayData.savedReplays
                .Where(r => !r.isFavorite)
                .OrderBy(r => r.battleDate)
                .FirstOrDefault();

            if (oldestReplay != null)
            {
                replayData.savedReplays.Remove(oldestReplay);
                Debug.Log($"[ReplayManager] Removed oldest replay to make space");
            }
        }

        replayData.savedReplays.Add(replay);
        Debug.Log($"[ReplayManager] Saved replay: {replay.replayId} ({replay.battleType})");

        return replay.replayId;
    }

    /// <summary>
    /// Record Arena battle replay
    /// </summary>
    public string RecordArenaBattle(List<ReplayHero> attackers, List<ReplayHero> defenders,
        List<ReplayTurn> turns, int winnerTeam)
    {
        ExtendedBattleReplay replay = new ExtendedBattleReplay
        {
            battleType = BattleType.Arena,
            attackerTeam = attackers,
            defenderTeam = defenders,
            turns = turns,
            winnerTeam = winnerTeam,
            playerId = "player_local",
            playerName = "Player",
            attackerFormation = CreateFormationFromHeroes(attackers),
            defenderFormation = CreateFormationFromHeroes(defenders),
            title = "Arena Battle"
        };

        return SaveBattleReplay(replay);
    }

    /// <summary>
    /// Record World Boss battle replay
    /// </summary>
    public string RecordWorldBossBattle(string bossId, List<ReplayHero> team,
        List<ReplayTurn> turns, long damageDealt, float timeElapsed, int rank)
    {
        ExtendedBattleReplay replay = new ExtendedBattleReplay
        {
            battleType = BattleType.WorldBoss,
            bossId = bossId,
            attackerTeam = team,
            turns = turns,
            damageDealt = damageDealt,
            timeElapsed = timeElapsed,
            rankAchieved = rank,
            playerId = "player_local",
            playerName = "Player",
            attackerFormation = CreateFormationFromHeroes(team),
            title = $"World Boss - {bossId}"
        };

        return SaveBattleReplay(replay);
    }

    /// <summary>
    /// Record Guild Raid replay
    /// </summary>
    public string RecordGuildRaid(string raidId, List<ReplayHero> team,
        List<ReplayTurn> turns, long damageDealt, int rank)
    {
        ExtendedBattleReplay replay = new ExtendedBattleReplay
        {
            battleType = BattleType.GuildRaid,
            bossId = raidId,
            attackerTeam = team,
            turns = turns,
            damageDealt = damageDealt,
            rankAchieved = rank,
            playerId = "player_local",
            playerName = "Player",
            attackerFormation = CreateFormationFromHeroes(team),
            title = $"Guild Raid - {raidId}"
        };

        return SaveBattleReplay(replay);
    }

    #endregion

    #region Replay Retrieval

    /// <summary>
    /// Get replay by ID
    /// </summary>
    public ExtendedBattleReplay GetReplay(string replayId)
    {
        var replay = replayData.savedReplays.Find(r => r.replayId == replayId);

        if (replay != null)
        {
            // Increment view count
            if (!replayData.replayViews.ContainsKey(replayId))
                replayData.replayViews[replayId] = 0;

            replayData.replayViews[replayId]++;
            replay.viewCount = replayData.replayViews[replayId];
        }

        return replay;
    }

    /// <summary>
    /// Get all replays
    /// </summary>
    public List<ExtendedBattleReplay> GetAllReplays()
    {
        return new List<ExtendedBattleReplay>(replayData.savedReplays);
    }

    /// <summary>
    /// Get replays by type
    /// </summary>
    public List<ExtendedBattleReplay> GetReplaysByType(BattleType type)
    {
        return replayData.savedReplays.Where(r => r.battleType == type).ToList();
    }

    /// <summary>
    /// Get favorite replays
    /// </summary>
    public List<ExtendedBattleReplay> GetFavoriteReplays()
    {
        return replayData.savedReplays.Where(r => r.isFavorite).ToList();
    }

    /// <summary>
    /// Search replays with filters
    /// </summary>
    public List<ExtendedBattleReplay> SearchReplays(ReplayFilter filter)
    {
        var results = replayData.savedReplays.AsEnumerable();

        if (filter.battleType.HasValue)
            results = results.Where(r => r.battleType == filter.battleType.Value);

        if (filter.onlyTopReplays)
            results = results.Where(r => r.isTopReplay);

        if (filter.onlyFavorites)
            results = results.Where(r => r.isFavorite);

        if (!string.IsNullOrEmpty(filter.searchTerm))
        {
            results = results.Where(r =>
                r.title.Contains(filter.searchTerm) ||
                r.playerName.Contains(filter.searchTerm));
        }

        if (filter.fromDate.HasValue)
            results = results.Where(r => r.battleDate >= filter.fromDate.Value);

        if (filter.toDate.HasValue)
            results = results.Where(r => r.battleDate <= filter.toDate.Value);

        return results.ToList();
    }

    /// <summary>
    /// Get top replays for a battle type
    /// </summary>
    public List<ExtendedBattleReplay> GetTopReplays(BattleType type, int count = 10)
    {
        return replayData.savedReplays
            .Where(r => r.battleType == type && r.isTopReplay)
            .OrderByDescending(r => r.viewCount)
            .Take(count)
            .ToList();
    }

    #endregion

    #region Replay Management

    /// <summary>
    /// Toggle favorite on replay
    /// </summary>
    public bool ToggleFavorite(string replayId)
    {
        var replay = replayData.savedReplays.Find(r => r.replayId == replayId);
        if (replay == null) return false;

        replay.isFavorite = !replay.isFavorite;

        if (replay.isFavorite && !replayData.favoriteReplays.Contains(replayId))
            replayData.favoriteReplays.Add(replayId);
        else if (!replay.isFavorite)
            replayData.favoriteReplays.Remove(replayId);

        Debug.Log($"[ReplayManager] Replay {replayId} favorite: {replay.isFavorite}");
        return true;
    }

    /// <summary>
    /// Delete replay
    /// </summary>
    public bool DeleteReplay(string replayId)
    {
        var replay = replayData.savedReplays.Find(r => r.replayId == replayId);
        if (replay == null) return false;

        // Don't allow deleting top replays
        if (replay.isTopReplay)
        {
            Debug.Log("[ReplayManager] Cannot delete top replay");
            return false;
        }

        replayData.savedReplays.Remove(replay);
        replayData.favoriteReplays.Remove(replayId);
        replayData.replayViews.Remove(replayId);

        Debug.Log($"[ReplayManager] Deleted replay: {replayId}");
        return true;
    }

    /// <summary>
    /// Set replay title
    /// </summary>
    public bool SetReplayTitle(string replayId, string title)
    {
        var replay = replayData.savedReplays.Find(r => r.replayId == replayId);
        if (replay == null) return false;

        replay.title = title;
        Debug.Log($"[ReplayManager] Set replay title: {title}");
        return true;
    }

    #endregion

    #region Formation Copy

    /// <summary>
    /// Copy formation from replay
    /// </summary>
    public TeamFormation CopyFormation(string replayId, bool useDefenderFormation = false)
    {
        var replay = GetReplay(replayId);
        if (replay == null)
        {
            Debug.Log("[ReplayManager] Replay not found");
            return null;
        }

        TeamFormation formation = useDefenderFormation ?
            replay.defenderFormation :
            replay.attackerFormation;

        if (formation == null)
        {
            Debug.Log("[ReplayManager] Formation not available");
            return null;
        }

        Debug.Log($"[ReplayManager] Copied formation: {formation.formationName}");
        return formation;
    }

    /// <summary>
    /// Apply formation to party
    /// </summary>
    public bool ApplyFormationToParty(TeamFormation formation)
    {
        if (formation == null || formation.heroes == null || formation.heroes.Count == 0)
        {
            Debug.Log("[ReplayManager] Invalid formation");
            return false;
        }

        // In real implementation, integrate with PartySystem
        Debug.Log($"[ReplayManager] Applied formation with {formation.heroes.Count} heroes");

        foreach (var hero in formation.heroes)
        {
            Debug.Log($"  Slot {hero.slotPosition}: {hero.heroName} (Lv.{hero.level})");
        }

        return true;
    }

    #endregion

    #region Playback Control

    /// <summary>
    /// Start replay playback
    /// </summary>
    public bool StartPlayback(string replayId)
    {
        var replay = GetReplay(replayId);
        if (replay == null) return false;

        playbackState.currentReplayId = replayId;
        playbackState.currentTurn = 0;
        playbackState.isPlaying = true;
        playbackState.isPaused = false;

        Debug.Log($"[ReplayManager] Started playback: {replay.title}");
        return true;
    }

    /// <summary>
    /// Pause playback
    /// </summary>
    public void PausePlayback()
    {
        playbackState.isPaused = true;
        Debug.Log("[ReplayManager] Playback paused");
    }

    /// <summary>
    /// Resume playback
    /// </summary>
    public void ResumePlayback()
    {
        playbackState.isPaused = false;
        Debug.Log("[ReplayManager] Playback resumed");
    }

    /// <summary>
    /// Stop playback
    /// </summary>
    public void StopPlayback()
    {
        playbackState.isPlaying = false;
        playbackState.currentReplayId = null;
        playbackState.currentTurn = 0;
        Debug.Log("[ReplayManager] Playback stopped");
    }

    /// <summary>
    /// Set playback speed
    /// </summary>
    public void SetPlaybackSpeed(float speed)
    {
        playbackState.playbackSpeed = Mathf.Clamp(speed, 0.5f, 3.0f);
        Debug.Log($"[ReplayManager] Playback speed: {playbackState.playbackSpeed}x");
    }

    /// <summary>
    /// Jump to specific turn
    /// </summary>
    public void JumpToTurn(int turnNumber)
    {
        playbackState.currentTurn = turnNumber;
        Debug.Log($"[ReplayManager] Jumped to turn {turnNumber}");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Create formation data from hero list
    /// </summary>
    private TeamFormation CreateFormationFromHeroes(List<ReplayHero> heroes)
    {
        TeamFormation formation = new TeamFormation
        {
            formationName = "Team Formation",
            heroes = new List<HeroFormationSlot>(),
            elements = new List<string>(),
            totalPower = 0
        };

        for (int i = 0; i < heroes.Count; i++)
        {
            var hero = heroes[i];
            HeroFormationSlot slot = new HeroFormationSlot
            {
                slotPosition = i,
                heroId = hero.heroId,
                heroName = hero.heroName,
                level = hero.level,
                stars = hero.rarity,
                equippedGearIds = hero.gear,
                stats = hero.stats,
                activeSkills = hero.skills
            };

            formation.heroes.Add(slot);
        }

        return formation;
    }

    #endregion

    #region Save/Load

    public ReplaySaveData GetSaveData()
    {
        return replayData;
    }

    public void LoadSaveData(ReplaySaveData data)
    {
        replayData = data;
        Debug.Log($"[ReplayManager] Loaded {replayData.savedReplays.Count} replays");
    }

    #endregion
}
