using System.Collections.Generic;
using UnityEngine;

public class MaterialInventory : MonoBehaviour
{
    public static MaterialInventory Instance { get; private set; }

    private Dictionary<string, int> mats = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Has(string matId, int amount)
    {
        return mats.TryGetValue(matId, out var current) && current >= amount;
    }

    public void Add(string matId, int amount)
    {
        if (!mats.ContainsKey(matId)) mats[matId] = 0;
        mats[matId] += amount;
        Debug.Log($"[MaterialInventory] {matId} +{amount} (total {mats[matId]})");
    }

    public void Remove(string matId, int amount)
    {
        if (!mats.ContainsKey(matId)) return;
        mats[matId] = Mathf.Max(0, mats[matId] - amount);
        Debug.Log($"[MaterialInventory] {matId} -{amount} (total {mats[matId]})");
    }
}
