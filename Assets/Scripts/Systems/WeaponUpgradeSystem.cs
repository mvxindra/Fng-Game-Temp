using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class WeaponPassiveAbility
{
    public string id;
    public string name;
    public string description;
    public int unlockLevel;        // Level when passive unlocks
    public WeaponPassiveEffect effect;
}

[Serializable]
public class WeaponPassiveEffect
{
    // Damage amplification
    public int damageMultiplierPercent;     // Extra damage on attacks
    public int critDamageBonus;             // Additional crit damage
    public int critRateBonus;               // Additional crit rate
    public int armorPenetrationPercent;     // Ignore % of enemy DEF

    // On-hit effects
    public int lifestealPercent;            // Heal % of damage dealt
    public int vampiricStrikePercent;       // Extra damage converted to HP
    public int bleedChancePercent;          // Chance to apply bleed
    public int bleedDamagePercent;          // Bleed damage per turn
    public int bleedDuration;               // Bleed duration in turns

    // Status effects
    public int burnChancePercent;           // Chance to apply burn
    public int burnDamagePercent;           // Burn damage per turn
    public int burnDuration;                // Burn duration in turns
    public int poisonChancePercent;         // Chance to apply poison
    public int poisonDamagePercent;         // Poison damage per turn
    public int poisonDuration;              // Poison duration in turns
    public int stunChancePercent;           // Chance to stun
    public int stunDuration;                // Stun duration
    public int freezeChancePercent;         // Chance to freeze
    public int freezeDuration;              // Freeze duration

    // Attack modifiers
    public int multiStrikeChance;           // Chance to attack twice
    public int maxMultiStrikes;             // Max number of multi-strikes
    public int chainAttackChance;           // Chance to hit another enemy
    public int chainDamagePercent;          // Chain attack damage %
    public int chainTargets;                // Number of chain targets
    public int cleavePercent;               // Damage to nearby enemies
    public bool cleaveAllEnemies;           // Cleave hits all enemies
    public int executeDamagePercent;        // Extra damage to low HP enemies
    public int executeThreshold;            // HP threshold for execute (e.g., 30%)

    // Counter and defensive
    public int counterAttackChance;         // Chance to counter when hit
    public int counterDamageMultiplier;     // Counter damage multiplier
    public int damageToShieldsPercent;      // Extra damage to shields

    // Skill cooldown & energy
    public int cooldownReductionPercent;    // Reduce all skill cooldowns
    public int firstStrikeDamage;           // Extra damage on battle start
    public int firstStrikeSpdBuff;          // SPD buff on first strike
    public int firstStrikeBuffDuration;     // First strike buff duration

    // Buffs on action
    public int onAttackSpdBuff;             // SPD buff when attacking
    public int onAttackBuffDuration;        // Attack buff duration
    public int onKillAtkBuff;               // ATK buff when killing
    public int onKillBuffDuration;          // Kill buff duration
    public int onKillHealPercent;           // Heal % of max HP on kill

    // Conditional damage
    public int lowHpDamageBonus;            // Extra damage when self HP is low
    public int lowHpThreshold;              // HP threshold for low HP bonus
    public int rageAtkPerLostHP;            // ATK% per 10% HP lost
    public int rageMaxBonus;                // Max rage bonus

    // Advanced effects
    public int trueDamagePercent;           // % of damage as true damage (ignores DEF)
    public int defensiveBreakChance;        // Chance to reduce enemy DEF
    public int defensiveBreakAmount;        // DEF reduction amount
    public int defensiveBreakDuration;      // Duration in turns
}

[Serializable]
public class WeaponLimitBreak
{
    public int breakLevel;                  // Limit break level (0-5)
    public int maxLevelIncrease;            // How much max level increases
    public List<MaterialCost> materials;    // Materials required
    public int goldCost;
    public WeaponPassiveAbility unlockedPassive; // Passive unlocked at this break
}

[Serializable]
public class MaterialCost
{
    public string materialId;
    public int amount;
}

[Serializable]
public class WeaponRarityConfig
{
    public int rarity;
    public int baseMaxLevel;
    public int expPerLevel;
    public List<WeaponLimitBreak> limitBreaks;
    public bool hasPassiveAbility;
}

[Serializable]
public class WeaponUpgradeConfig
{
    public List<WeaponRarityConfig> rarityConfigs;
    public List<WeaponPassiveAbility> passiveAbilities;
}

public class WeaponUpgradeSystem : MonoBehaviour
{
    public static WeaponUpgradeSystem Instance { get; private set; }

    private WeaponUpgradeConfig config;
    public MaterialInventory materialInventory;
    public PlayerWallet playerWallet;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadConfig();
    }

    private void LoadConfig()
    {
        var json = Resources.Load<TextAsset>("Config/weapon_upgrade");
        if (json == null)
        {
            Debug.LogError("[WeaponUpgradeSystem] weapon_upgrade.json not found!");
            return;
        }

        config = JsonUtility.FromJson<WeaponUpgradeConfig>(json.text);
        Debug.Log("[WeaponUpgradeSystem] Loaded weapon upgrade config.");
    }

    public int GetMaxLevel(int rarity, int limitBreakLevel)
    {
        var rarityConfig = config.rarityConfigs.Find(r => r.rarity == rarity);
        if (rarityConfig == null) return 1;

        int maxLevel = rarityConfig.baseMaxLevel;

        // Add bonus levels from limit breaks
        for (int i = 0; i < limitBreakLevel && i < rarityConfig.limitBreaks.Count; i++)
        {
            maxLevel += rarityConfig.limitBreaks[i].maxLevelIncrease;
        }

        return maxLevel;
    }

    public bool LevelUpWeapon(GearInstance weapon, string expMaterialId, int amount)
    {
        var gearConfig = GearDatabase.Instance?.GetGear(weapon.configId);
        if (gearConfig == null || gearConfig.slot != "weapon")
        {
            Debug.LogWarning("[WeaponUpgradeSystem] Not a weapon!");
            return false;
        }

        // Check if already at max level
        int maxLevel = GetMaxLevel(gearConfig.rarity, weapon.limitBreakLevel);
        if (weapon.level >= maxLevel)
        {
            Debug.LogWarning("[WeaponUpgradeSystem] Weapon is already at max level!");
            return false;
        }

        // Check if player has materials
        if (!materialInventory.Has(expMaterialId, amount))
        {
            Debug.LogWarning($"[WeaponUpgradeSystem] Not enough {expMaterialId}!");
            return false;
        }

        // Consume materials
        materialInventory.Remove(expMaterialId, amount);

        // Add levels (simple version - each material = 1 level)
        int levelsToAdd = amount;
        weapon.level = Mathf.Min(weapon.level + levelsToAdd, maxLevel);

        Debug.Log($"[WeaponUpgradeSystem] Weapon leveled up to {weapon.level}/{maxLevel}!");

        return true;
    }

    public bool LimitBreakWeapon(GearInstance weapon)
    {
        var gearConfig = GearDatabase.Instance?.GetGear(weapon.configId);
        if (gearConfig == null || gearConfig.slot != "weapon")
        {
            Debug.LogWarning("[WeaponUpgradeSystem] Not a weapon!");
            return false;
        }

        var rarityConfig = config.rarityConfigs.Find(r => r.rarity == gearConfig.rarity);
        if (rarityConfig == null) return false;

        // Check if already at max limit break
        if (weapon.limitBreakLevel >= rarityConfig.limitBreaks.Count)
        {
            Debug.LogWarning("[WeaponUpgradeSystem] Weapon is already at max limit break!");
            return false;
        }

        var limitBreak = rarityConfig.limitBreaks[weapon.limitBreakLevel];

        // Check materials
        foreach (var mat in limitBreak.materials)
        {
            if (!materialInventory.Has(mat.materialId, mat.amount))
            {
                Debug.LogWarning($"[WeaponUpgradeSystem] Not enough {mat.materialId}. Need {mat.amount}");
                return false;
            }
        }

        // Check gold
        if (playerWallet.gold < limitBreak.goldCost)
        {
            Debug.LogWarning($"[WeaponUpgradeSystem] Not enough gold. Need {limitBreak.goldCost}");
            return false;
        }

        // Consume materials
        foreach (var mat in limitBreak.materials)
        {
            materialInventory.Remove(mat.materialId, mat.amount);
        }
        playerWallet.gold -= limitBreak.goldCost;

        // Apply limit break
        weapon.limitBreakLevel++;

        Debug.Log($"[WeaponUpgradeSystem] Weapon limit break level {weapon.limitBreakLevel}!");

        // Unlock passive if available
        if (limitBreak.unlockedPassive != null)
        {
            Debug.Log($"[WeaponUpgradeSystem] Unlocked passive: {limitBreak.unlockedPassive.name}!");
        }

        return true;
    }

    public List<WeaponPassiveAbility> GetActivePassives(GearInstance weapon)
    {
        var actives = new List<WeaponPassiveAbility>();
        var gearConfig = GearDatabase.Instance?.GetGear(weapon.configId);
        if (gearConfig == null || gearConfig.slot != "weapon") return actives;

        var rarityConfig = config.rarityConfigs.Find(r => r.rarity == gearConfig.rarity);
        if (rarityConfig == null || !rarityConfig.hasPassiveAbility) return actives;

        // Get passives from limit breaks
        for (int i = 0; i < weapon.limitBreakLevel && i < rarityConfig.limitBreaks.Count; i++)
        {
            var passive = rarityConfig.limitBreaks[i].unlockedPassive;
            if (passive != null && weapon.level >= passive.unlockLevel)
            {
                actives.Add(passive);
            }
        }

        return actives;
    }
}

// Extension to GearInstance for weapon-specific properties
public static class GearInstanceWeaponExtensions
{
    public static int limitBreakLevel = 0; // Added to GearInstance via extension concept
}
