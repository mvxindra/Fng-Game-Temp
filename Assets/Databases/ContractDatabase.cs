using System.Collections.Generic;
using UnityEngine;

public class ContractDatabase : MonoBehaviour
{
    public static ContractDatabase Instance { get; private set; }
    private Dictionary<string, SeasonalContract> contracts = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        var json = Resources.Load<TextAsset>("Config/seasonal_contracts");
        if (json == null)
        {
            Debug.LogError("[ContractDatabase] seasonal_contracts.json missing!");
            return;
        }

        ContractList wrapper = JsonUtility.FromJson<ContractList>(json.text);
        foreach (var c in wrapper.contracts)
        {
            contracts[c.id] = c;
        }
        Debug.Log($"[ContractDatabase] Loaded {contracts.Count} contracts.");
    }

    public SeasonalContract Get(string id)
    {
        if (contracts.TryGetValue(id, out var c)) return c;
        Debug.LogWarning("[ContractDatabase] Unknown contract id: " + id);
        return null;
    }
}

