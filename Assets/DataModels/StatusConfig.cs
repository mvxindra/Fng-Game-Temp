using System;

[Serializable]
public class StatusConfigList
{
    public StatusConfig[] statuses;
}

[Serializable]
public class StatusConfig
{
    public string id;
    public string name;
    public string description;

    public string stat;          // "ATK", "DEF", etc. for buffs/debuffs
    public float flatDelta;      // e.g. +10, -20
    public float percentDelta;   // e.g. +0.2, -0.2
    public bool isDebuff;

    public float periodicDamagePercentHp; // e.g. 0.08f = 8% HP per turn
}

