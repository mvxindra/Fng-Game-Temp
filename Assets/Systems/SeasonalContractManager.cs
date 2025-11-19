using System.Collections.Generic;
using UnityEngine;

public class SeasonalContractManager : MonoBehaviour
{
    public static SeasonalContractManager Instance { get; private set; }

    public SeasonalContract activeSeason;

    public int currentXP;
    public HashSet<int> claimedFreeTiers = new();
    public HashSet<int> claimedPremiumTiers = new();

    public bool premiumUnlocked = false;

    public RewardDistributor rewardDistributor;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadSeason(string seasonId)
    {
        activeSeason = ContractDatabase.Instance.Get(seasonId);
        if (activeSeason == null)
            Debug.LogError("[SeasonalContractManager] Unknown season: " + seasonId);
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        Debug.Log($"[Season] XP +{amount} (total: {currentXP})");
    }

    public bool IsTierUnlocked(int tierIndex)
    {
        if (activeSeason == null || tierIndex >= activeSeason.tiers.Count) return false;
        return currentXP >= activeSeason.tiers[tierIndex].reqXP;
    }

    public void ClaimFreeReward(int tierIndex)
    {
        if (!IsTierUnlocked(tierIndex)) return;
        if (claimedFreeTiers.Contains(tierIndex)) return;

        var tier = activeSeason.tiers[tierIndex];
        rewardDistributor.GrantReward(tier.freeReward, "SEASON_FREE", activeSeason.id);

        claimedFreeTiers.Add(tierIndex);
    }

    public void ClaimPremiumReward(int tierIndex)
    {
        if (!premiumUnlocked) return;
        if (!IsTierUnlocked(tierIndex)) return;
        if (claimedPremiumTiers.Contains(tierIndex)) return;

        var tier = activeSeason.tiers[tierIndex];
        rewardDistributor.GrantReward(tier.premiumReward, "SEASON_PREMIUM", activeSeason.id);

        claimedPremiumTiers.Add(tierIndex);
    }

    public void CompleteMission(string missionId)
    {
        if (activeSeason == null) return;

        var mission = activeSeason.missions.Find(m => m.id == missionId);
        if (mission == null) return;

        AddXP(mission.xp);
        Debug.Log($"[Season] Mission '{missionId}' completed (+{mission.xp} XP)");
    }
}
