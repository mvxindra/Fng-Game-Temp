using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Battle context that tracks battle state and provides utility methods for passives
/// </summary>
public class BattleContext
{
    public List<string> log = new();
    public int TurnCount { get; private set; }

    private HashSet<string> attackedThisBattle = new HashSet<string>();
    private Queue<PendingAction> pendingActions = new Queue<PendingAction>();

    public List<CombatUnit> AllyUnits { get; set; }
    public List<CombatUnit> EnemyUnits { get; set; }

    public void IncrementTurn()
    {
        TurnCount++;
        Log($"=== Turn {TurnCount} ===");
    }

    public void Log(string msg)
    {
        log.Add(msg);
        Debug.Log(msg);
    }

    public bool HasAttackedThisBattle(CombatUnit unit)
    {
        return attackedThisBattle.Contains(unit.heroId);
    }

    public void MarkAsAttacked(CombatUnit unit)
    {
        attackedThisBattle.Add(unit.heroId);
    }

    public void QueueExtraAttack(CombatUnit attacker, CombatUnit target)
    {
        pendingActions.Enqueue(new PendingAction
        {
            type = ActionType.ExtraAttack,
            source = attacker,
            target = target
        });
    }

    public void QueueCounterAttack(CombatUnit counter, CombatUnit target, int damage)
    {
        pendingActions.Enqueue(new PendingAction
        {
            type = ActionType.CounterAttack,
            source = counter,
            target = target,
            damageAmount = damage
        });
    }

    public void QueueChainAttack(CombatUnit attacker, CombatUnit target, int damage)
    {
        pendingActions.Enqueue(new PendingAction
        {
            type = ActionType.ChainAttack,
            source = attacker,
            target = target,
            damageAmount = damage
        });
    }

    public bool HasPendingActions()
    {
        return pendingActions.Count > 0;
    }

    public PendingAction GetNextAction()
    {
        if (pendingActions.Count > 0)
            return pendingActions.Dequeue();
        return null;
    }

    public CombatUnit GetRandomEnemy(bool fromEnemySide)
    {
        var targets = fromEnemySide ? AllyUnits : EnemyUnits;
        var alive = targets.FindAll(u => u.CurrentHP > 0);

        if (alive.Count == 0) return null;

        return alive[Random.Range(0, alive.Count)];
    }

    public List<CombatUnit> GetAllEnemies(bool fromEnemySide)
    {
        var targets = fromEnemySide ? AllyUnits : EnemyUnits;
        return targets.FindAll(u => u.CurrentHP > 0);
    }

    public List<CombatUnit> GetAdjacentEnemies(CombatUnit target)
    {
        // Simple implementation - in full version would check actual positioning
        var targets = target.isEnemy ? EnemyUnits : AllyUnits;
        return targets.FindAll(u => u.CurrentHP > 0 && u != target);
    }
}

public enum ActionType
{
    ExtraAttack,
    CounterAttack,
    ChainAttack
}

public class PendingAction
{
    public ActionType type;
    public CombatUnit source;
    public CombatUnit target;
    public int damageAmount;
}
