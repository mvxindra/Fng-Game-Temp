using System.Collections.Generic;

public static class DungeonRewardRoller
{
    public static List<(string reward, int amount)> RollFloorRewards(DungeonFloorConfig floor)
    {
        var results = new List<(string reward, int amount)>();
        if (floor == null || floor.loot == null) return results;

        foreach (var rewardId in floor.loot)
        {
            // If it's a drop table
            var tableRewards = DropManager.Instance.RollTable(rewardId);
            if (tableRewards.Count > 0)
            {
                results.AddRange(tableRewards);
                continue;
            }

            // If it's a direct reward like "GOLD:2000"
            if (rewardId.Contains(":"))
            {
                var split = rewardId.Split(':');
                results.Add((split[0], int.Parse(split[1])));
            }
            else
            {
                results.Add((rewardId, 1));
            }
        }

        return results;
    }
}
