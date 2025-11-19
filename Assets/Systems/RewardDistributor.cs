using System.Collections.Generic;
using UnityEngine;

public class RewardDistributor : MonoBehaviour
{
    public static RewardDistributor Instance { get; private set; }

    public PlayerWallet wallet;
    public MaterialInventory materialInventory;
    public GearInventory gearInventory;
    public HeroCollection heroCollection;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Grants a single reward with source tracking
    /// </summary>
    public void GrantReward(string rewardString, string source, string context)
    {
        if (string.IsNullOrEmpty(rewardString))
            return;

        // Parse reward string format: "TYPE:VALUE" or single item ID
        string[] parts = rewardString.Split(':');

        if (parts.Length == 2)
        {
            string rewardType = parts[0];
            
            if (int.TryParse(parts[1], out int amount))
            {
                switch (rewardType)
                {
                    case "GOLD":
                        wallet?.AddGold(amount);
                        Debug.Log($"[RewardDistributor] Granted {amount} GOLD from {source} ({context})");
                        break;

                    case "GEMS":
                        wallet?.AddGems(amount);
                        Debug.Log($"[RewardDistributor] Granted {amount} GEMS from {source} ({context})");
                        break;

                    case "KEY":
                        wallet?.AddKey(rewardType, amount);
                        Debug.Log($"[RewardDistributor] Granted {amount} {rewardType} from {source} ({context})");
                        break;

                    default:
                        // Check if it's a material
                        if (rewardType.StartsWith("MAT_"))
                        {
                            materialInventory?.Add(rewardType, amount);
                            Debug.Log($"[RewardDistributor] Granted {amount}x {rewardType} from {source} ({context})");
                        }
                        else if (rewardType.StartsWith("GEAR_"))
                        {
                            gearInventory?.AddGearById(rewardType, amount);
                            Debug.Log($"[RewardDistributor] Granted {amount}x {rewardType} from {source} ({context})");
                        }
                        else if (rewardType.StartsWith("SHARD_"))
                        {
                            heroCollection?.AddShards(rewardType, amount);
                            Debug.Log($"[RewardDistributor] Granted {amount}x {rewardType} from {source} ({context})");
                        }
                        else
                        {
                            Debug.LogWarning($"[RewardDistributor] Unknown reward type: {rewardType}");
                        }
                        break;
                }
            }
        }
        else
        {
            // Single item ID (gear, hero, etc.)
            if (rewardString.StartsWith("HERO_"))
            {
                heroCollection?.UnlockHero(rewardString);
                Debug.Log($"[RewardDistributor] Granted hero '{rewardString}' from {source} ({context})");
            }
            else if (rewardString.StartsWith("GEAR_"))
            {
                gearInventory?.AddGearById(rewardString, 1);
                Debug.Log($"[RewardDistributor] Granted gear '{rewardString}' from {source} ({context})");
            }
            else
            {
                Debug.Log($"[RewardDistributor] Granted item '{rewardString}' from {source} ({context})");
            }
        }
    }

    /// <summary>
    /// Distributes multiple rewards at once
    /// </summary>
    public void DistributeRewards(List<(string reward, int amount)> rewards, string source = "SYSTEM")
    {
        foreach (var (reward, amount) in rewards)
        {
            DistributeReward(reward, amount, source);
        }
    }

    /// <summary>
    /// Legacy method for distributing a single reward with amount
    /// </summary>
    public void DistributeReward(string rewardId, int amount, string source = "SYSTEM")
    {
        if (amount > 1)
        {
            GrantReward($"{rewardId}:{amount}", source, "batch");
        }
        else
        {
            GrantReward(rewardId, source, "single");
        }
    }
}