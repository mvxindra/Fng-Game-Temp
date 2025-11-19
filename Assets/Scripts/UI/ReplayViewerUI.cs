using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using FnGMafia.Replay;

/// <summary>
/// UI controller for viewing and managing battle replays
/// </summary>
public class ReplayViewerUI : MonoBehaviour
{
    [Header("Replay List")]
    public Transform replayListContent;
    public GameObject replayEntryPrefab;
    public TMP_Dropdown battleTypeFilter;
    public Toggle onlyFavoritesToggle;
    public Toggle onlyTopReplaysToggle;

    [Header("Replay Details")]
    public GameObject replayDetailsPanel;
    public TextMeshProUGUI replayTitleText;
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI battleDateText;
    public TextMeshProUGUI battleStatsText;
    public Button playButton;
    public Button favoriteButton;
    public Button deleteButton;
    public Button copyFormationButton;

    [Header("Playback Controls")]
    public GameObject playbackPanel;
    public Slider progressSlider;
    public TextMeshProUGUI turnCounterText;
    public Button pauseButton;
    public Button stopButton;
    public TMP_Dropdown speedDropdown;
    public Toggle skipAnimationsToggle;

    [Header("Formation View")]
    public Transform attackerFormationContent;
    public Transform defenderFormationContent;
    public GameObject heroSlotPrefab;
    public Button applyFormationButton;

    private List<GameObject> spawnedReplayEntries = new List<GameObject>();
    private ExtendedBattleReplay currentReplay;
    private TeamFormation selectedFormation;

    private void Start()
    {
        SetupButtonListeners();
        ShowReplayList();
    }

    private void SetupButtonListeners()
    {
        if (playButton != null)
            playButton.onClick.AddListener(OnPlayReplay);

        if (favoriteButton != null)
            favoriteButton.onClick.AddListener(OnToggleFavorite);

        if (deleteButton != null)
            deleteButton.onClick.AddListener(OnDeleteReplay);

        if (copyFormationButton != null)
            copyFormationButton.onClick.AddListener(OnCopyFormation);

        if (pauseButton != null)
            pauseButton.onClick.AddListener(OnPauseResume);

        if (stopButton != null)
            stopButton.onClick.AddListener(OnStopPlayback);

        if (applyFormationButton != null)
            applyFormationButton.onClick.AddListener(OnApplyFormation);

        if (speedDropdown != null)
            speedDropdown.onValueChanged.AddListener(OnSpeedChanged);

        if (battleTypeFilter != null)
            battleTypeFilter.onValueChanged.AddListener(OnFilterChanged);
    }

    #region Replay List

    public void ShowReplayList()
    {
        SetActivePanel("list");
        RefreshReplayList();
    }

    public void RefreshReplayList()
    {
        // Clear existing entries
        foreach (var entry in spawnedReplayEntries)
            Destroy(entry);
        spawnedReplayEntries.Clear();

        if (ReplayManager.Instance == null) return;

        // Apply filters
        List<ExtendedBattleReplay> replays = GetFilteredReplays();

        foreach (var replay in replays)
        {
            if (replayEntryPrefab != null && replayListContent != null)
            {
                GameObject entry = Instantiate(replayEntryPrefab, replayListContent);
                SetupReplayEntry(entry, replay);
                spawnedReplayEntries.Add(entry);
            }
        }

        Debug.Log($"[ReplayViewerUI] Displaying {replays.Count} replays");
    }

    private List<ExtendedBattleReplay> GetFilteredReplays()
    {
        ReplayFilter filter = new ReplayFilter();

        // Battle type filter
        if (battleTypeFilter != null && battleTypeFilter.value > 0)
        {
            filter.battleType = (BattleType)(battleTypeFilter.value - 1);
        }

        // Favorites filter
        if (onlyFavoritesToggle != null && onlyFavoritesToggle.isOn)
        {
            filter.onlyFavorites = true;
        }

        // Top replays filter
        if (onlyTopReplaysToggle != null && onlyTopReplaysToggle.isOn)
        {
            filter.onlyTopReplays = true;
        }

        return ReplayManager.Instance.SearchReplays(filter);
    }

    private void SetupReplayEntry(GameObject entry, ExtendedBattleReplay replay)
    {
        var titleText = entry.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
        if (titleText != null)
            titleText.text = replay.title;

        var typeText = entry.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
        if (typeText != null)
            typeText.text = replay.battleType.ToString();

        var dateText = entry.transform.Find("DateText")?.GetComponent<TextMeshProUGUI>();
        if (dateText != null)
            dateText.text = replay.battleDate.ToString("MM/dd/yyyy");

        var viewBtn = entry.transform.Find("ViewButton")?.GetComponent<Button>();
        if (viewBtn != null)
            viewBtn.onClick.AddListener(() => ShowReplayDetails(replay.replayId));

        var favoriteIcon = entry.transform.Find("FavoriteIcon")?.gameObject;
        if (favoriteIcon != null)
            favoriteIcon.SetActive(replay.isFavorite);
    }

    private void OnFilterChanged(int value)
    {
        RefreshReplayList();
    }

    #endregion

    #region Replay Details

    public void ShowReplayDetails(string replayId)
    {
        currentReplay = ReplayManager.Instance.GetReplay(replayId);
        if (currentReplay == null)
        {
            Debug.Log("[ReplayViewerUI] Replay not found");
            return;
        }

        SetActivePanel("details");

        // Update UI
        if (replayTitleText != null)
            replayTitleText.text = currentReplay.title;

        if (playerNameText != null)
            playerNameText.text = $"By: {currentReplay.playerName} (Lv.{currentReplay.playerLevel})";

        if (battleDateText != null)
            battleDateText.text = currentReplay.battleDate.ToString("MMM dd, yyyy HH:mm");

        if (battleStatsText != null)
        {
            string stats = $"Type: {currentReplay.battleType}\n";

            if (currentReplay.battleType == BattleType.Arena)
            {
                stats += $"Result: {(currentReplay.winnerTeam == 0 ? "Victory" : "Defeat")}\n";
            }
            else if (currentReplay.battleType == BattleType.WorldBoss || currentReplay.battleType == BattleType.GuildRaid)
            {
                stats += $"Damage: {currentReplay.damageDealt:N0}\n";
                stats += $"Rank: #{currentReplay.rankAchieved}\n";
                stats += $"Time: {currentReplay.timeElapsed:F1}s\n";
            }

            stats += $"Views: {currentReplay.viewCount}";
            battleStatsText.text = stats;
        }

        // Show formations
        DisplayFormation(currentReplay.attackerFormation, attackerFormationContent);
        if (currentReplay.defenderFormation != null)
            DisplayFormation(currentReplay.defenderFormation, defenderFormationContent);
    }

    private void DisplayFormation(TeamFormation formation, Transform container)
    {
        if (formation == null || container == null) return;

        // Clear existing
        foreach (Transform child in container)
            Destroy(child.gameObject);

        // Display heroes
        foreach (var hero in formation.heroes)
        {
            if (heroSlotPrefab != null)
            {
                GameObject slot = Instantiate(heroSlotPrefab, container);
                SetupHeroSlot(slot, hero);
            }
        }
    }

    private void SetupHeroSlot(GameObject slot, HeroFormationSlot hero)
    {
        var nameText = slot.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
            nameText.text = hero.heroName;

        var levelText = slot.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
        if (levelText != null)
            levelText.text = $"Lv.{hero.level}";

        var starsText = slot.transform.Find("StarsText")?.GetComponent<TextMeshProUGUI>();
        if (starsText != null)
            starsText.text = new string('★', hero.stars);
    }

    #endregion

    #region Playback Controls

    public void ShowPlayback()
    {
        SetActivePanel("playback");

        if (currentReplay != null)
        {
            ReplayManager.Instance.StartPlayback(currentReplay.replayId);
        }
    }

    private void OnPauseResume()
    {
        // Toggle pause/resume
        Debug.Log("[ReplayViewerUI] Pause/Resume playback");
        ReplayManager.Instance.PausePlayback();
    }

    private void OnStopPlayback()
    {
        ReplayManager.Instance.StopPlayback();
        ShowReplayDetails(currentReplay.replayId);
    }

    private void OnSpeedChanged(int value)
    {
        float[] speeds = { 1.0f, 1.5f, 2.0f, 3.0f };
        if (value >= 0 && value < speeds.Length)
        {
            ReplayManager.Instance.SetPlaybackSpeed(speeds[value]);
        }
    }

    #endregion

    #region Formation Copy

    private void OnCopyFormation()
    {
        if (currentReplay == null) return;

        // Default to attacker formation
        selectedFormation = ReplayManager.Instance.CopyFormation(currentReplay.replayId, false);

        if (selectedFormation != null)
        {
            Debug.Log($"[ReplayViewerUI] Formation copied: {selectedFormation.formationName}");

            if (applyFormationButton != null)
                applyFormationButton.gameObject.SetActive(true);
        }
    }

    private void OnApplyFormation()
    {
        if (selectedFormation == null) return;

        bool success = ReplayManager.Instance.ApplyFormationToParty(selectedFormation);

        if (success)
        {
            Debug.Log("[ReplayViewerUI] Formation applied to party!");
            // Show confirmation message
        }
    }

    public void CopyDefenderFormation()
    {
        if (currentReplay == null) return;

        selectedFormation = ReplayManager.Instance.CopyFormation(currentReplay.replayId, true);

        if (selectedFormation != null)
        {
            Debug.Log($"[ReplayViewerUI] Defender formation copied");
        }
    }

    #endregion

    #region Button Handlers

    private void OnPlayReplay()
    {
        if (currentReplay == null) return;
        ShowPlayback();
    }

    private void OnToggleFavorite()
    {
        if (currentReplay == null) return;

        ReplayManager.Instance.ToggleFavorite(currentReplay.replayId);
        currentReplay.isFavorite = !currentReplay.isFavorite;

        Debug.Log($"[ReplayViewerUI] Replay favorite: {currentReplay.isFavorite}");
    }

    private void OnDeleteReplay()
    {
        if (currentReplay == null) return;

        bool success = ReplayManager.Instance.DeleteReplay(currentReplay.replayId);

        if (success)
        {
            Debug.Log("[ReplayViewerUI] Replay deleted");
            ShowReplayList();
        }
    }

    #endregion

    #region Panel Management

    private void SetActivePanel(string panelName)
    {
        if (replayDetailsPanel != null)
            replayDetailsPanel.SetActive(panelName == "details");

        if (playbackPanel != null)
            playbackPanel.SetActive(panelName == "playback");

        // Replay list is the parent content, always visible in background
    }

    #endregion
}
