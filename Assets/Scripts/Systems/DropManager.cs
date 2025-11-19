using System.Collections.Generic;
using UnityEngine;

public class DropManager : MonoBehaviour
{
    public static DropManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public List<(string reward, int amount)> RollTable(string tableId)
    {
        var results = new List<(string reward, int amount)>();
        var table = DropDatabase.Instance.Get(tableId);
        if (table == null || table.loot == null || table.loot.Count == 0)
            return results;

        // Weighted roll
        int totalWeight = 0;
        foreach (var e in table.loot)
            totalWeight += e.weight;

        int roll = Random.Range(0, totalWeight);
        int sum = 0;

        foreach (var entry in table.loot)
        {
            sum += entry.weight;
            if (roll < sum)
            {
                int amount = 1;
                // Parse "REWARD:amount"
                if (entry.reward.Contains(":"))
                {
                    string[] parts = entry.reward.Split(':');
                    amount = int.Parse(parts[1]);
                    results.Add((parts[0], amount));
                }
                else results.Add((entry.reward, 1));
                break;
            }
        }

        return results;
    }
}
