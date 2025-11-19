using System.Collections.Generic;
using UnityEngine;

public class WeaponPassiveManager : MonoBehaviour
{
    public static WeaponPassiveManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Apply passive effects before an attack
    /// </summary>
    public void ApplyPreAttackPassives(CombatUnit attacker, CombatUnit target, BattleContext context, ref int baseDamage)
    {
        var passives = GetWeaponPassives(attacker);
        if (passives == null || passives.Count == 0) return;

        foreach (var passive in passives)
        {
            var effect = passive.effect;

            // Damage multiplier
            if (effect.damageMultiplierPercent > 0)
            {
                int bonus = Mathf.RoundToInt(baseDamage * effect.damageMultiplierPercent / 100f);
                baseDamage += bonus;
                context.Log($"{attacker.heroId}'s {passive.name}: +{bonus} damage");
            }

            // First strike bonus
            if (effect.firstStrikeDamage > 0 && context.TurnCount == 1 && !context.HasAttackedThisBattle(attacker))
            {
                int bonus = Mathf.RoundToInt(baseDamage * effect.firstStrikeDamage / 100f);
                baseDamage += bonus;
                context.Log($"{attacker.heroId}'s {passive.name}: First Strike +{bonus} damage!");

                // Apply SPD buff if available
                if (effect.firstStrikeSpdBuff > 0)
                {
                    attacker.AddStatus($"SPD_BUFF_{effect.firstStrikeSpdBuff}", effect.firstStrikeBuffDuration, context);
                }
            }

            // Execute bonus damage
            if (effect.executeDamagePercent > 0 && target.CurrentHP <= (target.BaseHP * effect.executeThreshold / 100))
            {
                int bonus = Mathf.RoundToInt(baseDamage * effect.executeDamagePercent / 100f);
                baseDamage += bonus;
                context.Log($"{attacker.heroId}'s {passive.name}: Execute! +{bonus} damage");
            }

            // Low HP self damage bonus
            if (effect.lowHpDamageBonus > 0 && attacker.CurrentHP <= (attacker.BaseHP * effect.lowHpThreshold / 100))
            {
                int bonus = Mathf.RoundToInt(baseDamage * effect.lowHpDamageBonus / 100f);
                baseDamage += bonus;
                context.Log($"{attacker.heroId}'s {passive.name}: Last Stand! +{bonus} damage");
            }

            // Rage (based on missing HP)
            if (effect.rageAtkPerLostHP > 0)
            {
                int hpLostPercent = 100 - (attacker.CurrentHP * 100 / attacker.BaseHP);
                int rageBonus = Mathf.Min((hpLostPercent / 10) * effect.rageAtkPerLostHP, effect.rageMaxBonus);
                if (rageBonus > 0)
                {
                    int bonus = Mathf.RoundToInt(baseDamage * rageBonus / 100f);
                    baseDamage += bonus;
                    context.Log($"{attacker.heroId}'s {passive.name}: Rage +{rageBonus}% (+{bonus} damage)");
                }
            }

            // Critical rate/damage bonuses (applied to stats, not damage directly)
            // These would be checked in the crit calculation step
        }
    }

    /// <summary>
    /// Apply passive effects during damage calculation
    /// </summary>
    public int ApplyDamageModifiers(CombatUnit attacker, CombatUnit target, int baseDamage, BattleContext context)
    {
        var passives = GetWeaponPassives(attacker);
        if (passives == null || passives.Count == 0) return baseDamage;

        int totalDamage = baseDamage;
        int trueDamage = 0;

        foreach (var passive in passives)
        {
            var effect = passive.effect;

            // Armor penetration (reduce effective DEF)
            if (effect.armorPenetrationPercent > 0)
            {
                int defReduction = Mathf.RoundToInt(target.GetCurrentDEF() * effect.armorPenetrationPercent / 100f);
                context.Log($"{attacker.heroId}'s {passive.name}: Penetrating {defReduction} DEF");
                // This would be applied in actual damage calculation
            }

            // True damage conversion
            if (effect.trueDamagePercent > 0)
            {
                int truePortion = Mathf.RoundToInt(baseDamage * effect.trueDamagePercent / 100f);
                trueDamage += truePortion;
                context.Log($"{attacker.heroId}'s {passive.name}: {truePortion} true damage");
            }

            // Shield break bonus
            if (effect.damageToShieldsPercent > 0 && HasShield(target))
            {
                int shieldBonus = Mathf.RoundToInt(totalDamage * effect.damageToShieldsPercent / 100f);
                totalDamage += shieldBonus;
                context.Log($"{attacker.heroId}'s {passive.name}: Barrier Breaker +{shieldBonus} damage");
            }
        }

        return totalDamage + trueDamage;
    }

    /// <summary>
    /// Apply passive effects after dealing damage
    /// </summary>
    public void ApplyPostAttackPassives(CombatUnit attacker, CombatUnit target, int damageDealt, BattleContext context)
    {
        var passives = GetWeaponPassives(attacker);
        if (passives == null || passives.Count == 0) return;

        foreach (var passive in passives)
        {
            var effect = passive.effect;

            // Lifesteal
            if (effect.lifestealPercent > 0)
            {
                int healAmount = Mathf.RoundToInt(damageDealt * effect.lifestealPercent / 100f);
                attacker.Heal(healAmount, context);
                context.Log($"{attacker.heroId}'s {passive.name}: Lifesteal {healAmount} HP");
            }

            // Apply status effects (chance-based)
            TryApplyBleed(attacker, target, effect, context);
            TryApplyBurn(attacker, target, effect, context);
            TryApplyPoison(attacker, target, effect, context);
            TryApplyStun(attacker, target, effect, context);
            TryApplyFreeze(attacker, target, effect, context);
            TryApplyDefenseBreak(attacker, target, effect, context);

            // Multi-strike
            if (effect.multiStrikeChance > 0)
            {
                int roll = Random.Range(0, 100);
                if (roll < effect.multiStrikeChance)
                {
                    int strikes = effect.maxMultiStrikes > 0 ? effect.maxMultiStrikes - 1 : 1;
                    context.Log($"{attacker.heroId}'s {passive.name}: Multi-Strike! (+{strikes} attacks)");
                    // Trigger additional attacks
                    for (int i = 0; i < strikes; i++)
                    {
                        context.QueueExtraAttack(attacker, target);
                    }
                }
            }

            // Chain attack
            if (effect.chainAttackChance > 0)
            {
                int roll = Random.Range(0, 100);
                if (roll < effect.chainAttackChance)
                {
                    int chainTargets = effect.chainTargets > 0 ? effect.chainTargets : 1;
                    context.Log($"{attacker.heroId}'s {passive.name}: Chain Attack! Bouncing to {chainTargets} enemies");
                    // Queue chain attacks
                    for (int i = 0; i < chainTargets; i++)
                    {
                        var chainTarget = context.GetRandomEnemy(attacker.isEnemy);
                        if (chainTarget != null)
                        {
                            int chainDamage = Mathf.RoundToInt(damageDealt * effect.chainDamagePercent / 100f);
                            context.QueueChainAttack(attacker, chainTarget, chainDamage);
                        }
                    }
                }
            }

            // Cleave damage
            if (effect.cleavePercent > 0)
            {
                var targets = effect.cleaveAllEnemies ? context.GetAllEnemies(attacker.isEnemy) : context.GetAdjacentEnemies(target);
                foreach (var adjacentTarget in targets)
                {
                    if (adjacentTarget == target) continue;

                    int cleaveDamage = Mathf.RoundToInt(damageDealt * effect.cleavePercent / 100f);
                    adjacentTarget.TakeDamage(cleaveDamage, context);
                    context.Log($"{attacker.heroId}'s {passive.name}: Cleave {cleaveDamage} to {adjacentTarget.heroId}");
                }
            }

            // On-attack buffs
            if (effect.onAttackSpdBuff > 0)
            {
                attacker.AddStatus($"SPD_BUFF_{effect.onAttackSpdBuff}", effect.onAttackBuffDuration, context);
                context.Log($"{attacker.heroId}'s {passive.name}: Gained SPD buff!");
            }
        }
    }

    /// <summary>
    /// Apply passive effects when killing an enemy
    /// </summary>
    public void ApplyOnKillPassives(CombatUnit attacker, CombatUnit killedEnemy, BattleContext context)
    {
        var passives = GetWeaponPassives(attacker);
        if (passives == null || passives.Count == 0) return;

        foreach (var passive in passives)
        {
            var effect = passive.effect;

            // On-kill healing
            if (effect.onKillHealPercent > 0)
            {
                int healAmount = Mathf.RoundToInt(attacker.BaseHP * effect.onKillHealPercent / 100f);
                attacker.Heal(healAmount, context);
                context.Log($"{attacker.heroId}'s {passive.name}: Soul Siphon healed {healAmount} HP");
            }

            // On-kill ATK buff
            if (effect.onKillAtkBuff > 0)
            {
                attacker.AddStatus($"ATK_BUFF_{effect.onKillAtkBuff}", effect.onKillBuffDuration, context);
                context.Log($"{attacker.heroId}'s {passive.name}: Battle Fury activated! (+{effect.onKillAtkBuff}% ATK)");
            }
        }
    }

    /// <summary>
    /// Check for counter-attack when unit is hit
    /// </summary>
    public bool TryCounterAttack(CombatUnit defender, CombatUnit attacker, BattleContext context)
    {
        var passives = GetWeaponPassives(defender);
        if (passives == null || passives.Count == 0) return false;

        foreach (var passive in passives)
        {
            var effect = passive.effect;

            if (effect.counterAttackChance > 0)
            {
                int roll = Random.Range(0, 100);
                if (roll < effect.counterAttackChance)
                {
                    context.Log($"{defender.heroId}'s {passive.name}: Counter Attack!");

                    int counterDamage = defender.GetCurrentATK();
                    if (effect.counterDamageMultiplier > 0)
                    {
                        counterDamage = Mathf.RoundToInt(counterDamage * effect.counterDamageMultiplier / 100f);
                    }

                    context.QueueCounterAttack(defender, attacker, counterDamage);
                    return true;
                }
            }
        }

        return false;
    }

    // Helper methods for status application
    private void TryApplyBleed(CombatUnit attacker, CombatUnit target, WeaponPassiveEffect effect, BattleContext context)
    {
        if (effect.bleedChancePercent > 0 && Random.Range(0, 100) < effect.bleedChancePercent)
        {
            target.AddStatus("BLEED", effect.bleedDuration, context);
            context.Log($"{attacker.heroId} inflicted Bleed on {target.heroId}!");
        }
    }

    private void TryApplyBurn(CombatUnit attacker, CombatUnit target, WeaponPassiveEffect effect, BattleContext context)
    {
        if (effect.burnChancePercent > 0 && Random.Range(0, 100) < effect.burnChancePercent)
        {
            target.AddStatus("BURN", effect.burnDuration, context);
            context.Log($"{attacker.heroId} inflicted Burn on {target.heroId}!");
        }
    }

    private void TryApplyPoison(CombatUnit attacker, CombatUnit target, WeaponPassiveEffect effect, BattleContext context)
    {
        if (effect.poisonChancePercent > 0 && Random.Range(0, 100) < effect.poisonChancePercent)
        {
            target.AddStatus("POISON", effect.poisonDuration, context);
            context.Log($"{attacker.heroId} inflicted Poison on {target.heroId}!");
        }
    }

    private void TryApplyStun(CombatUnit attacker, CombatUnit target, WeaponPassiveEffect effect, BattleContext context)
    {
        if (effect.stunChancePercent > 0 && Random.Range(0, 100) < effect.stunChancePercent)
        {
            target.AddStatus("STUN", effect.stunDuration, context);
            context.Log($"{attacker.heroId} stunned {target.heroId}!");
        }
    }

    private void TryApplyFreeze(CombatUnit attacker, CombatUnit target, WeaponPassiveEffect effect, BattleContext context)
    {
        if (effect.freezeChancePercent > 0 && Random.Range(0, 100) < effect.freezeChancePercent)
        {
            target.AddStatus("FREEZE", effect.freezeDuration, context);
            context.Log($"{attacker.heroId} froze {target.heroId}!");
        }
    }

    private void TryApplyDefenseBreak(CombatUnit attacker, CombatUnit target, WeaponPassiveEffect effect, BattleContext context)
    {
        if (effect.defensiveBreakChance > 0 && Random.Range(0, 100) < effect.defensiveBreakChance)
        {
            target.AddStatus($"DEF_BREAK_{effect.defensiveBreakAmount}", effect.defensiveBreakDuration, context);
            context.Log($"{attacker.heroId} broke {target.heroId}'s defense! (-{effect.defensiveBreakAmount}% DEF)");
        }
    }

    private bool HasShield(CombatUnit unit)
    {
        // Check if unit has any shield status
        foreach (var status in unit.ActiveStatus)
        {
            if (status.Config.name.Contains("Shield") || status.Config.name.Contains("Barrier"))
                return true;
        }
        return false;
    }

    private List<WeaponPassiveAbility> GetWeaponPassives(CombatUnit unit)
    {
        return unit.GetWeaponPassives();
    }
}
