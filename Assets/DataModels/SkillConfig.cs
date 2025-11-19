using System;
using System.Collections.Generic;

[Serializable]
public class SkillConfig
{
    public string id;
    public string name;
    public string description;
    public int cooldown;
    public string targetType;  // "SingleEnemy", "AllEnemies", "AllAllies", etc.
    public List<SkillEffectConfig> effects;
}

[Serializable]
public class SkillEffectConfig
{
    public string type;  // "Damage", "Heal", "Buff", "Debuff"
    public string scalingStat;  // "ATK", "MAGIC", etc.
    public float multiplier;
    public bool isAoE;
    public string statusId;
    public int duration;
}

public enum SkillEffectType
{
    Damage,
    Heal,
    Buff,
    Debuff
}

[Serializable]
public class SkillConfigList
{
    public List<SkillConfig> skills;
}