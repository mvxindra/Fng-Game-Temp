using System.Collections.Generic;
using UnityEngine;

public class DropDatabase : MonoBehaviour
{
    public static DropDatabase Instance { get; private set; }
    private Dictionary<string, DropTable> tables = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        var json = Resources.Load<TextAsset>("Config/dungeon_loot");
        if (json == null)
        {
            Debug.LogError("[DropDatabase] dungeon_loot.json missing!");
            return;
        }

        // Parse as JSON array
        string wrapped = "{\"tables\":" + json.text + "}";
        DropTableList wrapper = JsonUtility.FromJson<DropTableList>(wrapped);
        
        tables.Clear();
        foreach (var t in wrapper.tables)
        {
            tables[t.id] = t;
        }
        
        Debug.Log($"[DropDatabase] Loaded {tables.Count} drop tables.");
    }

    public DropTable Get(string id)
    {
        if (tables.TryGetValue(id, out var t)) return t;
        Debug.LogWarning("[DropDatabase] Missing drop table: " + id);
        return null;
    }
}