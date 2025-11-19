using System.Collections.Generic;
using UnityEngine;

public static class SkillExecutor
{
    public static void ApplyEffect(
        SkillEffectConfig eff,
        CombatUnit caster,
        List<CombatUnit> targets,
        BattleContext context)
    {
        switch (eff.type)
        {
            case "Damage":
                ApplyDamage(eff, caster, targets, context);
                break;

            case "Heal":
                ApplyHeal(eff, caster, targets, context);
                break;

            case "Buff":
                ApplyStatus(eff, caster, targets, context, true);
                break;

            case "Debuff":
                ApplyStatus(eff, caster, targets, context, false);
                break;

            default:
                Debug.LogWarning($"[SkillExecutor] Unknown effect type: {eff.type}");
                break;
        }
    }

    private static void ApplyDamage(
        SkillEffectConfig eff,
        CombatUnit caster,
        List<CombatUnit> targets,
        BattleContext context)
    {
        int casterAtk = caster.GetCurrentATK();

        foreach (var t in targets)
        {
            if (t == null || t.CurrentHP <= 0) continue;

            int targetDef = t.GetCurrentDEF();
            float raw = casterAtk * eff.multiplier;
            float mitigated = raw * (100f / (100f + targetDef));
            int dmg = Mathf.Max(1, Mathf.RoundToInt(mitigated));

            t.TakeDamage(dmg, context);
        }
    }

    private static void ApplyHeal(
        SkillEffectConfig eff,
        CombatUnit caster,
        List<CombatUnit> targets,
        BattleContext context)
    {
        int casterAtk = caster.GetCurrentATK();

        foreach (var t in targets)
        {
            if (t == null || t.CurrentHP <= 0) continue;

            float raw = casterAtk * eff.multiplier;
            int heal = Mathf.Max(1, Mathf.RoundToInt(raw));
            t.Heal(heal, context);
        }
    }

    private static void ApplyStatus(
        SkillEffectConfig eff,
        CombatUnit caster,
        List<CombatUnit> targets,
        BattleContext context,
        bool isBuff)
    {
        if (string.IsNullOrEmpty(eff.statusId) || eff.duration <= 0)
            return;

        foreach (var t in targets)
        {
            if (t == null || t.CurrentHP <= 0) continue;
            t.AddStatus(eff.statusId, eff.duration, context);
        }
    }
}