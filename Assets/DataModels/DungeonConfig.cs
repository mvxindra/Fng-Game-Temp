using System;
using System.Collections.Generic;

[Serializable]
public class DungeonFloorConfig
{
    public string id;                 // Floor ID ("F1", "F2", "F3")
    public List<string> enemies;      // Single-wave fallback OR enemy presets
    public List<string> loot;         // Reward IDs or DropTable IDs
    public int xp;                    // XP gained from floor
}

[Serializable]
public class DungeonConfig
{
    public string id;                 // Dungeon ID
    public string name;
    public int recommendedPower;
    public int staminaCost;
    public List<DungeonFloorConfig> floors;
}

[Serializable]
public class DungeonList
{
    public List<DungeonConfig> dungeons;
}
