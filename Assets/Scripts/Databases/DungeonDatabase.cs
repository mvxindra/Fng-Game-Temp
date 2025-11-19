using System.Collections.Generic;
using UnityEngine;

public class DungeonDatabase : MonoBehaviour
{
    public static DungeonDatabase Instance { get; private set; }
    public List<DungeonConfig> dungeons = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        var json = Resources.Load<TextAsset>("Config/dungeons");
        if (json == null)
        {
            Debug.LogError("[DungeonDatabase] dungeons.json missing!");
            return;
        }

        try
        {
            // Parse as JSON array
            string wrapped = "{\"dungeons\":" + json.text + "}";
            DungeonList wrapper = JsonUtility.FromJson<DungeonList>(wrapped);
            
            dungeons.Clear();
            if (wrapper != null && wrapper.dungeons != null)
            {
                foreach (var d in wrapper.dungeons)
                {
                    dungeons.Add(d);
                }
            }
            
            Debug.Log($"[DungeonDatabase] Loaded {dungeons.Count} dungeons.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[DungeonDatabase] Failed to parse dungeons.json: {ex.Message}");
            Debug.LogError($"[DungeonDatabase] JSON content: {json.text}");
        }
    }

    public DungeonConfig Get(string id)
    {
        var d = dungeons.Find(x => x.id == id);
        if (d == null)
            Debug.LogWarning("[DungeonDatabase] Unknown dungeon: " + id);
        return d;
    }
}