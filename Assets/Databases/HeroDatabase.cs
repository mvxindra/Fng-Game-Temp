using System.Collections.Generic;
using UnityEngine;

public class HeroDatabase : MonoBehaviour
{
    public static HeroDatabase Instance { get; private set; }

    private Dictionary<string, HeroConfig> heroes = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadHeroes();
    }

    private void LoadHeroes()
    {
        TextAsset json = Resources.Load<TextAsset>("Config/hero");
        if (json == null)
        {
            Debug.LogError("HeroDatabase ERROR: hero.json missing from Resources/Config/");
            return;
        }

        // Wrap array for JsonUtility
        string wrapped = "{\"heroes\":" + json.text + "}";
        HeroConfigList listWrapper = JsonUtility.FromJson<HeroConfigList>(wrapped);

        heroes.Clear();
        foreach (HeroConfig cfg in listWrapper.heroes)
        {
            if (!heroes.ContainsKey(cfg.id))
                heroes[cfg.id] = cfg;
            else
                Debug.LogWarning($"Duplicate hero ID detected: {cfg.id}");
        }

        Debug.Log($"HeroDatabase: Loaded {heroes.Count} heroes.");
    }

    public HeroConfig GetHero(string id)
    {
        if (heroes.TryGetValue(id, out var data))
            return data;

        Debug.LogWarning($"HeroDatabase: Unknown hero id: {id}");
        return null;
    }
}
