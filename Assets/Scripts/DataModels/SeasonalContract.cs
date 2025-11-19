using System;
using System.Collections.Generic;

[Serializable]
public class SeasonalContract
{
    public string id;
    public string name;
    public string start;  // Date string
    public string end;    // Date string
    public int durationDays;
    public List<SeasonTier> tiers;
    public List<SeasonMission> missions;
}

[Serializable]
public class SeasonTier
{
    public int tierIndex;
    public int reqXP;
    public string freeReward;
    public string premiumReward;
}

[Serializable]
public class SeasonMission
{
    public string id;
    public string description;
    public int xp;
    public string type;  // "battle", "summon", "upgrade", etc.
    public int progressRequired;
}

[Serializable]
public class ContractList
{
    public List<SeasonalContract> contracts;
}