using System.Collections.Generic;
using UnityEngine;

public class CombatUnit : MonoBehaviour
{
    public string heroId;
    public bool isEnemy;

    public int BaseATK;
    public int BaseDEF;
    public int BaseHP;
    public int BaseSPD;

    public int CurrentHP;
    public int Level = 1; // Unit level for stat calculations

    public List<SkillInstance> Skills { get; private set; } = new();
    public List<StatusEffectInstance> ActiveStatus { get; private set; } = new();

    // Equipment tracking for weapon passives
    public GearInstance equippedWeapon;
    public GearInstance equippedArmor;
    public GearInstance equippedAccessory1;
    public GearInstance equippedAccessory2;

    // Cached weapon passives for performance
    private List<WeaponPassiveAbility> cachedWeaponPassives;

    public void InitializeFromHeroConfig(HeroConfig heroCfg, int level = 1)
    {
        heroId = heroCfg.id;
        Level = level;
        isEnemy = false;

        // Calculate stats at the specified level using rarity-based growth
        var statsAtLevel = heroCfg.GetStatsAtLevel(level);
        BaseATK = statsAtLevel.atk;
        BaseDEF = statsAtLevel.def;
        BaseHP = statsAtLevel.hp;
        BaseSPD = statsAtLevel.spd;
        CurrentHP = BaseHP;

        Skills.Clear();
        if (heroCfg.skills != null)
        {
            foreach (var skillId in heroCfg.skills)
            {
                var cfg = SkillDatabase.Instance.GetSkill(skillId);
                if (cfg != null)
                {
                    Skills.Add(new SkillInstance(cfg));
                }
            }
        }
    }

    /// <summary>
    /// Initialize combat unit from an enemy config with rarity-based growth.
    /// </summary>
    public void InitializeFromEnemyConfig(EnemyConfig enemyCfg, int level = 1)
    {
        heroId = enemyCfg.id;
        Level = level;
        isEnemy = true;

        // Calculate stats at the specified level using rarity-based growth
        var statsAtLevel = enemyCfg.GetStatsAtLevel(level);
        BaseATK = statsAtLevel.atk;
        BaseDEF = statsAtLevel.def;
        BaseHP = statsAtLevel.hp;
        BaseSPD = statsAtLevel.spd;
        CurrentHP = BaseHP;

        Skills.Clear();
        if (enemyCfg.skills != null)
        {
            foreach (var skillId in enemyCfg.skills)
            {
                var cfg = SkillDatabase.Instance.GetSkill(skillId);
                if (cfg != null)
                {
                    Skills.Add(new SkillInstance(cfg));
                }
            }
        }
    }

    public void OnTurnStart(BattleContext context)
    {
        // Apply periodic damage from statuses
        foreach (var status in ActiveStatus)
        {
            if (status.Config.periodicDamagePercentHp > 0f)
            {
                int dmg = Mathf.RoundToInt(BaseHP * status.Config.periodicDamagePercentHp);
                TakeDamage(dmg, context);
                context.Log($"{heroId} suffers {dmg} from {status.Config.name}");
            }
        }

        // Remove expired (in case something ticked early)
        ActiveStatus.RemoveAll(s => s.IsExpired);
    }

    public void OnTurnEnd()
    {
        foreach (var s in ActiveStatus)
            s.Tick();

        ActiveStatus.RemoveAll(s => s.IsExpired);

        foreach (var skill in Skills)
            skill.TickCooldown();
    }

    public int GetCurrentATK()
    {
        float atk = BaseATK;
        foreach (var st in ActiveStatus)
        {
            if (st.Config.stat == "ATK")
            {
                atk += st.Config.flatDelta;
                atk += BaseATK * st.Config.percentDelta;
            }
        }
        return Mathf.Max(1, Mathf.RoundToInt(atk));
    }

    public int GetCurrentDEF()
    {
        float def = BaseDEF;
        foreach (var st in ActiveStatus)
        {
            if (st.Config.stat == "DEF")
            {
                def += st.Config.flatDelta;
                def += BaseDEF * st.Config.percentDelta;
            }
        }
        return Mathf.Max(1, Mathf.RoundToInt(def));
    }

    public void TakeDamage(int amount, BattleContext context)
    {
        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        context.Log($"{heroId} takes {amount} damage (HP: {CurrentHP}/{BaseHP})");

        if (CurrentHP == 0)
        {
            context.Log($"{heroId} is defeated.");
            // TODO: death handling / animation
        }
    }

    public void Heal(int amount, BattleContext context)
    {
        int old = CurrentHP;
        CurrentHP = Mathf.Min(BaseHP, CurrentHP + amount);
        context.Log($"{heroId} heals {CurrentHP - old} HP (HP: {CurrentHP}/{BaseHP})");
    }

    public void AddStatus(string statusId, int duration, BattleContext context)
    {
        var cfg = StatusDatabase.Instance.GetStatus(statusId);
        if (cfg == null) return;

        ActiveStatus.Add(new StatusEffectInstance(cfg, duration));
        context.Log($"{heroId} gains {cfg.name} for {duration} turns");
    }

    /// <summary>
    /// Get weapon passive abilities for this unit
    /// </summary>
    public List<WeaponPassiveAbility> GetWeaponPassives()
    {
        if (cachedWeaponPassives != null)
            return cachedWeaponPassives;

        if (equippedWeapon == null)
            return new List<WeaponPassiveAbility>();

        // Get passives from weapon upgrade system
        cachedWeaponPassives = WeaponUpgradeSystem.Instance?.GetActivePassives(equippedWeapon)
                               ?? new List<WeaponPassiveAbility>();

        return cachedWeaponPassives;
    }

    /// <summary>
    /// Refresh passive cache when equipment changes
    /// </summary>
    public void RefreshPassiveCache()
    {
        cachedWeaponPassives = null;
    }

    // Helper methods for compatibility
    public string GetHeroId() => heroId;
    public int GetAttack() => GetCurrentATK();
    public int GetDefense() => GetCurrentDEF();
    public int GetMaxHP() => BaseHP;
    public int GetCurrentHP() => CurrentHP;
    public int GetLevel() => Level;
}
