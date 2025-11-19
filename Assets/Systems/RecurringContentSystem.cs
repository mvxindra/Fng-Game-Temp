using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ContentReward
{
    public string materialId;
    public int amount;
}

[Serializable]
public class RecurringMission
{
    public string id;
    public string name;
    public string description;
    public string type; // "weekly", "monthly", "seasonal"
    public int staminaCost;
    public int difficulty; // 1-5
    public List<string> enemyParty;
    public List<ContentReward> rewards;
    public int maxClearsPerPeriod; // How many times can complete per week/month/season
}

[Serializable]
public class RecurringContentConfig
{
    public List<RecurringMission> weeklyMissions;
    public List<RecurringMission> monthlyMissions;
    public List<RecurringMission> seasonalMissions;
}

[Serializable]
public class MissionProgress
{
    public string missionId;
    public int clearsThisPeriod;
    public DateTime lastReset;
}

public class RecurringContentSystem : MonoBehaviour
{
    public static RecurringContentSystem Instance { get; private set; }

    private RecurringContentConfig config;
    private Dictionary<string, MissionProgress> progress = new();

    public MaterialInventory materialInventory;
    public PlayerWallet playerWallet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadConfig();
    }

    private void LoadConfig()
    {
        var json = Resources.Load<TextAsset>("Config/recurring_content");
        if (json == null)
        {
            Debug.LogWarning("[RecurringContentSystem] recurring_content.json not found!");
            return;
        }

        config = JsonUtility.FromJson<RecurringContentConfig>(json.text);
        Debug.Log($"[RecurringContentSystem] Loaded {config.weeklyMissions?.Count ?? 0} weekly, " +
                  $"{config.monthlyMissions?.Count ?? 0} monthly, " +
                  $"{config.seasonalMissions?.Count ?? 0} seasonal missions.");
    }

    private void Update()
    {
        CheckAndResetMissions();
    }

    private void CheckAndResetMissions()
    {
        DateTime now = DateTime.Now;

        foreach (var kvp in progress)
        {
            var missionProgress = kvp.Value;
            var mission = GetMission(kvp.Key);
            if (mission == null) continue;

            bool shouldReset = false;

            switch (mission.type)
            {
                case "weekly":
                    // Reset if it's been more than 7 days
                    if ((now - missionProgress.lastReset).TotalDays >= 7)
                        shouldReset = true;
                    break;

                case "monthly":
                    // Reset if it's a new month
                    if (now.Month != missionProgress.lastReset.Month || now.Year != missionProgress.lastReset.Year)
                        shouldReset = true;
                    break;

                case "seasonal":
                    // Reset if it's been more than 90 days (3 months)
                    if ((now - missionProgress.lastReset).TotalDays >= 90)
                        shouldReset = true;
                    break;
            }

            if (shouldReset)
            {
                missionProgress.clearsThisPeriod = 0;
                missionProgress.lastReset = now;
                Debug.Log($"[RecurringContentSystem] {mission.type} mission '{mission.name}' has been reset.");
            }
        }
    }

    private RecurringMission GetMission(string missionId)
    {
        var mission = config.weeklyMissions?.Find(m => m.id == missionId);
        if (mission != null) return mission;

        mission = config.monthlyMissions?.Find(m => m.id == missionId);
        if (mission != null) return mission;

        mission = config.seasonalMissions?.Find(m => m.id == missionId);
        return mission;
    }

    public bool StartMission(string missionId)
    {
        var mission = GetMission(missionId);
        if (mission == null)
        {
            Debug.LogWarning($"[RecurringContentSystem] Mission {missionId} not found!");
            return false;
        }

        // Check progress
        if (!progress.ContainsKey(missionId))
        {
            progress[missionId] = new MissionProgress
            {
                missionId = missionId,
                clearsThisPeriod = 0,
                lastReset = DateTime.Now
            };
        }

        var missionProgress = progress[missionId];

        if (missionProgress.clearsThisPeriod >= mission.maxClearsPerPeriod)
        {
            Debug.LogWarning($"[RecurringContentSystem] Mission {mission.name} has reached max clears this period!");
            return false;
        }

        // Check stamina
        if (!playerWallet.SpendStamina(mission.staminaCost))
        {
            return false;
        }

        Debug.Log($"[RecurringContentSystem] Starting mission: {mission.name}");
        // TODO: Load battle with mission.enemyParty
        return true;
    }

    public bool CompleteMission(string missionId)
    {
        var mission = GetMission(missionId);
        if (mission == null) return false;

        if (!progress.ContainsKey(missionId))
        {
            Debug.LogError($"[RecurringContentSystem] Trying to complete mission that wasn't started!");
            return false;
        }

        var missionProgress = progress[missionId];

        // Award rewards
        foreach (var reward in mission.rewards)
        {
            materialInventory.Add(reward.materialId, reward.amount);
            Debug.Log($"[RecurringContentSystem] Rewarded {reward.amount}x {reward.materialId}");
        }

        missionProgress.clearsThisPeriod++;
        Debug.Log($"[RecurringContentSystem] Completed {mission.name} ({missionProgress.clearsThisPeriod}/{mission.maxClearsPerPeriod})");

        return true;
    }

    public List<RecurringMission> GetAvailableWeeklyMissions()
    {
        return config.weeklyMissions ?? new List<RecurringMission>();
    }

    public List<RecurringMission> GetAvailableMonthlyMissions()
    {
        return config.monthlyMissions ?? new List<RecurringMission>();
    }

    public List<RecurringMission> GetAvailableSeasonalMissions()
    {
        return config.seasonalMissions ?? new List<RecurringMission>();
    }

    public MissionProgress GetMissionProgress(string missionId)
    {
        progress.TryGetValue(missionId, out var p);
        return p;
    }
}
