using System.Collections.Generic;
using UnityEngine;

public class SkillInstance
{
    public SkillConfig Config { get; private set; }
    public int CurrentCooldown { get; private set; }

    public SkillInstance(SkillConfig config)
    {
        Config = config;
        CurrentCooldown = 0;
    }

    public bool IsReady => CurrentCooldown <= 0;

    public void TickCooldown()
    {
        if (CurrentCooldown > 0)
            CurrentCooldown--;
    }

    public void PutOnCooldown()
    {
        CurrentCooldown = Config.cooldown;
    }

    public void Activate(CombatUnit caster, List<CombatUnit> targets, BattleContext context)
    {
        if (!IsReady) return;

        context.Log($"{caster.heroId} uses {Config.name}!");

        foreach (var eff in Config.effects)
        {
            SkillExecutor.ApplyEffect(eff, caster, targets, context);
        }

        PutOnCooldown();
    }
}