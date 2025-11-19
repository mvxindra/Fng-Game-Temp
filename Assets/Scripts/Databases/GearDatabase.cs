using System.Collections.Generic;
using UnityEngine;

public class GearDatabase : MonoBehaviour
{
    public static GearDatabase Instance { get; private set; }
    private Dictionary<string, GearConfig> gearById = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        var json = Resources.Load<TextAsset>("Config/gear");
        if (json == null)
        {
            Debug.LogError("[GearDatabase] gear.json missing!");
            return;
        }

        // Wrap array in object for JsonUtility
        string wrapped = "{\"items\":" + json.text + "}";
        GearConfigList wrapper = JsonUtility.FromJson<GearConfigList>(wrapped);
        foreach (var g in wrapper.items)
        {
            gearById[g.id] = g;
        }
        Debug.Log($"[GearDatabase] Loaded {gearById.Count} gear items.");
    }

    public GearConfig Get(string id)
    {
        if (gearById.TryGetValue(id, out var g)) return g;
        Debug.LogWarning("[GearDatabase] Missing gear id: " + id);
        return null;
    }

    public GearConfig GetGear(string id)
    {
        return Get(id);
    }
}