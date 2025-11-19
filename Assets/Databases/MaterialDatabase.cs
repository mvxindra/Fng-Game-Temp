using System.Collections.Generic;
using UnityEngine;

public class MaterialDatabase : MonoBehaviour
{
    public static MaterialDatabase Instance { get; private set; }
    private Dictionary<string, MaterialConfig> mats = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        var json = Resources.Load<TextAsset>("Config/materials");
        if (json == null)
        {
            Debug.LogError("[MaterialDatabase] materials.json missing!");
            return;
        }

        MaterialList wrapper = JsonUtility.FromJson<MaterialList>(json.text);
        foreach (var m in wrapper.materials)
        {
            mats[m.id] = m;
        }
        Debug.Log($"[MaterialDatabase] Loaded {mats.Count} materials.");
    }

    public MaterialConfig Get(string id)
    {
        if (mats.TryGetValue(id, out var m)) return m;
        Debug.LogWarning("[MaterialDatabase] Unknown material: " + id);
        return null;
    }
}
