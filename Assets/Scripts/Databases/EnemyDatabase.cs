using System.Collections.Generic;
using UnityEngine;

public class EnemyDatabase : MonoBehaviour
{
    public static EnemyDatabase Instance { get; private set; }

    private Dictionary<string, HeroConfig> _enemies = new();

    private void Awake()
    {
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadEnemies();
    }

    private void LoadEnemies()
    {
        TextAsset json = Resources.Load<TextAsset>("Config/enemy");
        if (json == null)
        {
            Debug.LogError("EnemyDatabase: enemy.json not found in Resources/Config.");
            return;
        }

        // Assuming enemy.json is a flat array of HeroConfig-like objects
        string wrapped = "{\"heroes\":" + json.text + "}";
        var wrapper = JsonUtility.FromJson<HeroConfigList>(wrapped);
        _enemies.Clear();

        foreach (var e in wrapper.heroes)
        {
            _enemies[e.id] = e;
        }
    }

    public HeroConfig GetEnemy(string id)
    {
        if (_enemies.TryGetValue(id, out var cfg))
            return cfg;

        Debug.LogWarning($"EnemyDatabase: Unknown enemy id {id}");
        return null;
    }
}
