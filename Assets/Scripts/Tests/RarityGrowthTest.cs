using UnityEngine;
using FngGame.DataModels;

/// <summary>
/// Test script to verify rarity-based growth rate system.
/// Attach this to a GameObject in the scene and run to see debug output.
/// </summary>
public class RarityGrowthTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runOnStart = true;
    [SerializeField] private int[] testLevels = { 1, 10, 30, 60, 90, 120 };

    private void Start()
    {
        if (runOnStart)
        {
            RunTests();
        }
    }

    [ContextMenu("Run Rarity Growth Tests")]
    public void RunTests()
    {
        Debug.Log("=== RARITY GROWTH RATE TEST ===\n");

        // Test 1: Display rarity multipliers
        TestRarityMultipliers();

        Debug.Log("\n");

        // Test 2: Compare hero growth across rarities
        TestHeroGrowthComparison();

        Debug.Log("\n");

        // Test 3: Compare enemy growth across rarities
        TestEnemyGrowthComparison();

        Debug.Log("\n=== TEST COMPLETE ===");
    }

    private void TestRarityMultipliers()
    {
        Debug.Log("--- Rarity Growth Multipliers ---");

        for (int rarity = 1; rarity <= 7; rarity++)
        {
            var config = RarityGrowthManager.Instance.GetConfig(rarity);
            if (config != null)
            {
                Debug.Log($"Rarity {rarity} ({config.description}): {config.growthMultiplier}x growth rate");
            }
            else
            {
                float multiplier = RarityGrowthManager.Instance.GetGrowthMultiplier(rarity);
                Debug.Log($"Rarity {rarity}: {multiplier}x growth rate (using default)");
            }
        }
    }

    private void TestHeroGrowthComparison()
    {
        Debug.Log("--- Hero Growth Comparison ---");
        Debug.Log("Simulating a hero with base ATK=100, growth=1.2 (multiplicative) at different rarities\n");

        // Create test hero configs with different rarities
        for (int rarity = 1; rarity <= 5; rarity++)
        {
            var testHero = new HeroConfig
            {
                id = $"TEST_HERO_R{rarity}",
                rarity = rarity,
                baseStats = new Stats { atk = 100, def = 50, hp = 1000, spd = 100 },
                growth = new Stats { atk = 1, def = 1, hp = 10, spd = 1 }, // Growth per level
                baseLevel = 1
            };

            Debug.Log($"Rarity {rarity} Hero ({RarityGrowthManager.Instance.GetGrowthMultiplier(rarity)}x multiplier):");

            foreach (int level in testLevels)
            {
                var stats = testHero.GetStatsAtLevel(level);
                Debug.Log($"  Level {level}: ATK={stats.atk}, DEF={stats.def}, HP={stats.hp}, SPD={stats.spd}");
            }

            Debug.Log("");
        }
    }

    private void TestEnemyGrowthComparison()
    {
        Debug.Log("--- Enemy Growth Comparison ---");
        Debug.Log("Simulating an enemy with base ATK=50, growth=2 (additive) at different rarities\n");

        // Create test enemy configs with different rarities
        for (int rarity = 1; rarity <= 5; rarity++)
        {
            var testEnemy = new EnemyConfig
            {
                id = $"TEST_ENEMY_R{rarity}",
                rarity = rarity,
                baseStats = new EnemyStatBlock { atk = 50, def = 30, hp = 500, spd = 80 },
                growth = new EnemyGrowthBlock { atk = 2, def = 1, hp = 15, spd = 1 } // Flat growth per level
            };

            Debug.Log($"Rarity {rarity} Enemy ({RarityGrowthManager.Instance.GetGrowthMultiplier(rarity)}x multiplier):");

            foreach (int level in testLevels)
            {
                var stats = testEnemy.GetStatsAtLevel(level);
                Debug.Log($"  Level {level}: ATK={stats.atk}, DEF={stats.def}, HP={stats.hp}, SPD={stats.spd}");
            }

            Debug.Log("");
        }
    }

    [ContextMenu("Test CombatUnit Initialization")]
    public void TestCombatUnitInitialization()
    {
        Debug.Log("=== COMBAT UNIT INITIALIZATION TEST ===\n");

        // Create a test hero
        var testHero = new HeroConfig
        {
            id = "TEST_HERO_LEGENDARY",
            rarity = 5,
            baseStats = new Stats { atk = 150, def = 80, hp = 2000, spd = 120 },
            growth = new Stats { atk = 2, def = 1, hp = 20, spd = 1 },
            baseLevel = 1,
            skills = new System.Collections.Generic.List<string>()
        };

        // Test at different levels
        int[] testLevels = { 1, 30, 60, 120 };

        foreach (int level in testLevels)
        {
            // Create a temporary GameObject with CombatUnit
            GameObject tempUnit = new GameObject($"TestUnit_Level{level}");
            CombatUnit unit = tempUnit.AddComponent<CombatUnit>();

            // Initialize with rarity-based growth
            unit.InitializeFromHeroConfig(testHero, level);

            Debug.Log($"Level {level} Legendary Hero (Rarity 5):");
            Debug.Log($"  ATK: {unit.BaseATK}, DEF: {unit.BaseDEF}, HP: {unit.BaseHP}, SPD: {unit.BaseSPD}");

            // Clean up
            Destroy(tempUnit);
        }

        Debug.Log("\n=== TEST COMPLETE ===");
    }

    [ContextMenu("Compare Growth Curves")]
    public void CompareGrowthCurves()
    {
        Debug.Log("=== GROWTH CURVE COMPARISON ===\n");
        Debug.Log("Comparing Common (R1) vs Legendary (R5) hero growth from level 1 to 120 (max level)\n");

        var commonHero = new HeroConfig
        {
            id = "COMMON_HERO",
            rarity = 1,
            baseStats = new Stats { atk = 100, def = 50, hp = 1000, spd = 100 },
            growth = new Stats { atk = 2, def = 1, hp = 20, spd = 1 },
            baseLevel = 1
        };

        var legendaryHero = new HeroConfig
        {
            id = "LEGENDARY_HERO",
            rarity = 5,
            baseStats = new Stats { atk = 100, def = 50, hp = 1000, spd = 100 },
            growth = new Stats { atk = 2, def = 1, hp = 20, spd = 1 },
            baseLevel = 1
        };

        Debug.Log("Level | Common ATK | Legendary ATK | Difference");
        Debug.Log("------|------------|---------------|------------");

        for (int level = 1; level <= 120; level += 10)
        {
            var commonStats = commonHero.GetStatsAtLevel(level);
            var legendaryStats = legendaryHero.GetStatsAtLevel(level);
            int diff = legendaryStats.atk - commonStats.atk;

            Debug.Log($"{level,5} | {commonStats.atk,10} | {legendaryStats.atk,13} | {diff,10} (+{(float)diff / commonStats.atk * 100:F1}%)");
        }

        Debug.Log("\n=== COMPARISON COMPLETE ===");
    }
}
