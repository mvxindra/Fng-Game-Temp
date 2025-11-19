using System;
using System.Collections.Generic;

[Serializable]
public class Stats
{
    public int atk;
    public int def;
    public int hp;
    public int spd;
    public int crit;
    public int critDmg;
    public int acc;
    public int res;
}

[Serializable]
public class HeroEquipment
{
    public string weapon;        // Weapon slot - gear instance ID
    public string armor;         // Armor slot - gear instance ID
    public string accessory1;    // Accessory slot 1 - gear instance ID
    public string accessory2;    // Accessory slot 2 - gear instance ID
    public string weaponMod;     // Weapon mod - mod ID (faction-specific bonus)
    public string armorMod;      // Armor mod - mod ID (faction-specific bonus)
    public string accessory1Mod; // Accessory 1 mod - mod ID (faction-specific bonus)
    public string accessory2Mod; // Accessory 2 mod - mod ID (faction-specific bonus)
}

[Serializable]
public class HeroConfig
{
    public string id;
    public int rarity;
    public string role;
    public string faction;

    public Stats baseStats;
    public Stats growth;

    public List<string> equipSlots;  // Legacy support - can still use for restrictions
    public HeroEquipment equippedGear; // New 4-slot equipment system
    public List<string> skills;

    public string captainSkill;   // optional
    public string primaryElement;     // Elemental affinity (fire, water, earth, etc.)

    public int baseLevel = 1;         // Starting level

    public int baseAscension = 0;     // Starting ascension tier
}

[Serializable]
public class HeroConfigList
{
    public List<HeroConfig> heroes;
}
