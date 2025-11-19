using System;
using System.Collections.Generic;

[Serializable]
public class DropTable
{
    public string id;              // Table ID (e.g., "REWARD_NORMAL_T1")
    public List<DropEntry> loot;   // Weighted list of rewards
}

[Serializable]
public class DropTableList
{
    public List<DropTable> tables;
}
