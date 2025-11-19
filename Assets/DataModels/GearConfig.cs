using System;
using System.Collections.Generic;

[Serializable]
public class GearStatBlock
{
    public int atk;
    public int def;
    public int hp;
    public int spd;
    public int crit;
    public int critDmg;
    public int res;
    public int acc;
    public int magic;
}

[Serializable]
public class GearPassiveEffect
{
    public int atkBuffPercent;
    public int defBuffPercent;
    public int spdBuff;
    public int durationTurns;

    public int hpDamagePercent;
    public int trueDamageAtkPercent;

    public int damageToDebuffedPercent;
    public int damageToShieldsPercent;
    public int damageToFastPercent;
    public int multiHitDamagePercent;

    public int evadeChancePercent;
    public int stunChancePercent;
    public int stunTurns;
    public int freezeChancePercent;
    public int freezeTurns;
    public int applyBurnTurns;
    public int applyPoisonTurns;
    public int applyShockTurns;

    public int cooldownReduction;
    public int triggerEvery;
    public int hpThreshold;
    public int speedThreshold;
    public int ignoreDefPercent;
}

[Serializable]
public class GearPassiveConfig
{
    public string name;
    public string description;
    public GearPassiveEffect effect;
}

[Serializable]
public class GearConfig
{
    public string id;
    public string slot;       // "weapon", "armor", "accessory"
    public int rarity;        // 3,4,5
    public GearStatBlock mainStat;
    public List<GearStatBlock> subStats;
    public string set;        // Set ID for set bonuses (e.g., "ASSAULT_SET", "TANK_SET")
    public GearPassiveConfig passive;  // optional
}

[Serializable]
public class GearConfigList
{
    public List<GearConfig> items;
}
