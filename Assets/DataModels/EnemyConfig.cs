using System;
using System.Collections.Generic;

[Serializable]
public class EnemyStatBlock
{
    public int atk;
    public int def;
    public int hp;
    public int spd;
}

[Serializable]
public class EnemyGrowthBlock
{
    public float atk;
    public float def;
    public float hp;
    public float spd;
}

[Serializable]
public class EnemyResistances
{
    public float physical = 0f;
    public float fire = 0f;
    public float ice = 0f;
    public float lightning = 0f;
    public float dark = 0f;
    public float holy = 0f;
}

[Serializable]
public class EnemyConfig
{
    public string id;
    public int rarity;
    public string role;
    public string faction;
    public EnemyStatBlock baseStats;
    public EnemyGrowthBlock growth;
    public List<string> equipSlots;
    public List<string> skills;
    public string captainSkill;
    public List<string> passiveAbilities;
    public EnemyResistances resistances;
}
