using System.Collections.Generic;
using UnityEngine;

public class DungeonLoader : MonoBehaviour
{
    public static DungeonLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public EnemyParty LoadCurrentFloorParty()
    {
        var floor = DungeonManager.Instance.currentFloor;
        if (floor == null)
        {
            Debug.LogError("[DungeonLoader] No floor loaded.");
            return null;
        }

        // Single-wave fallback (Dungeon-only, not stage-based)
        var enemyList = new List<string>(floor.enemies);
        return new EnemyParty(enemyList);
    }
}
