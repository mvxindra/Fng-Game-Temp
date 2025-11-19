using System.Collections.Generic;
using UnityEngine;

public class BattleInitializer : MonoBehaviour
{
    public Transform allySpawnRoot;
    public Transform enemySpawnRoot;
    public GameObject combatUnitPrefab;

    public List<CombatUnit> AllyUnits { get; private set; } = new();
    public List<CombatUnit> EnemyUnits { get; private set; } = new();

    public void InitializeStoryBattle(Party allyParty, EnemyParty enemyParty, Difficulty diff)
    {
        if (diff == Difficulty.REVERSE || diff == Difficulty.REVERSE_HARD)
        {
            // REVERSE MODE: Only hunters (no main character)
            allyParty.heroIds.RemoveAt(0); // Remove slot 0 (main character)
        }
        
        InitializeBattle(allyParty, enemyParty);
    }

    public void InitializeBattle(Party allyParty, EnemyParty enemyParty)
    {
        AllyUnits.Clear();
        EnemyUnits.Clear();

        // Allies
        foreach (string heroId in allyParty.heroIds)
        {
            if (string.IsNullOrEmpty(heroId)) continue;

            var heroCfg = HeroDatabase.Instance.GetHero(heroId);
            if (heroCfg == null)
            {
                Debug.LogError($"BattleInitializer: Hero not found: {heroId}");
                continue;
            }

            var go = Object.Instantiate(combatUnitPrefab, allySpawnRoot);
            var cu = go.GetComponent<CombatUnit>();
            cu.isEnemy = false;
            cu.InitializeFromHeroConfig(heroCfg);
            AllyUnits.Add(cu);
        }

        // Enemies
        foreach (string enemyId in enemyParty.enemyIds)
        {
            if (string.IsNullOrEmpty(enemyId)) continue;

            var enemyCfg = EnemyDatabase.Instance.GetEnemy(enemyId);
            if (enemyCfg == null)
            {
                Debug.LogError($"BattleInitializer: Enemy not found: {enemyId}");
                continue;
            }

            var go = Object.Instantiate(combatUnitPrefab, enemySpawnRoot);
            var cu = go.GetComponent<CombatUnit>();
            cu.isEnemy = true;
            cu.InitializeFromHeroConfig(enemyCfg);
            EnemyUnits.Add(cu);
        }
    }
}
