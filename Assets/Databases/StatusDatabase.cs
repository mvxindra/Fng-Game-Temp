using System.Collections.Generic;
using UnityEngine;

public class StatusDatabase : MonoBehaviour
{
    public static StatusDatabase Instance { get; private set; }

    private readonly Dictionary<string, StatusConfig> _statusById = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadStatuses();
    }

    private void LoadStatuses()
    {
        TextAsset json = Resources.Load<TextAsset>("Config/status_config");
        if (json == null)
        {
            Debug.LogError("StatusDatabase: status_config.json not found in Resources/Config.");
            return;
        }

        string wrapped = "{\"statuses\":" + json.text + "}";
        StatusConfigList list = JsonUtility.FromJson<StatusConfigList>(wrapped);

        _statusById.Clear();
        foreach (var sc in list.statuses)
        {
            _statusById[sc.id] = sc;
        }

        Debug.Log($"StatusDatabase: Loaded {_statusById.Count} statuses.");
    }

    public StatusConfig GetStatus(string id)
    {
        if (_statusById.TryGetValue(id, out var cfg))
            return cfg;

        Debug.LogWarning($"StatusDatabase: Status ID not found: {id}");
        return null;
    }
}
