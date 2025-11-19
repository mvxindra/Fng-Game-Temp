using UnityEngine;

public class DungeonManager : MonoBehaviour
{
    public static DungeonManager Instance { get; private set; }

    public DungeonConfig currentDungeon;
    public DungeonFloorConfig currentFloor;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SelectDungeon(string dungeonId)
    {
        currentDungeon = DungeonDatabase.Instance.Get(dungeonId);

        if (currentDungeon == null)
            Debug.LogError("[DungeonManager] Dungeon not found: " + dungeonId);
    }

    public void SelectFloor(string floorId)
    {
        if (currentDungeon == null)
        {
            Debug.LogError("[DungeonManager] No dungeon selected.");
            return;
        }

        currentFloor = currentDungeon.floors.Find(f => f.id == floorId);
        if (currentFloor == null)
            Debug.LogError("[DungeonManager] Floor not found: " + floorId);
    }
}
