using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TargetSelector
{
    public static List<CombatUnit> GetTargets(
        string targetType,
        CombatUnit caster,
        List<CombatUnit> allies,
        List<CombatUnit> enemies)
    {
        switch (targetType)
        {
            case "Self":
                return new List<CombatUnit> { caster };

            case "SingleEnemy":
                return new List<CombatUnit> { enemies.FirstOrDefault(e => e.CurrentHP > 0) };

            case "SingleAlly":
                return new List<CombatUnit> { LowestHP(allies) };

            case "AllEnemies":
                return enemies.Where(e => e.CurrentHP > 0).ToList();

            case "AllAllies":
                return allies.Where(a => a.CurrentHP > 0).ToList();

            case "RandomEnemy":
                return new List<CombatUnit> { RandomAlive(enemies) };

            case "RandomAlly":
                return new List<CombatUnit> { RandomAlive(allies) };

            case "LowestHpEnemy":
                return new List<CombatUnit> { LowestHP(enemies) };

            default:
                Debug.LogWarning($"[TargetSelector] Unknown target type: {targetType}");
                return new List<CombatUnit>();
        }
    }

    private static CombatUnit LowestHP(List<CombatUnit> units)
    {
        return units
            .Where(u => u.CurrentHP > 0)
            .OrderBy(u => u.CurrentHP)
            .FirstOrDefault();
    }

    private static CombatUnit RandomAlive(List<CombatUnit> units)
    {
        var alive = units.Where(u => u.CurrentHP > 0).ToList();
        if (alive.Count == 0) return null;

        int index = Random.Range(0, alive.Count);
        return alive[index];
    }
}