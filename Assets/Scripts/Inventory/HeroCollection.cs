using System.Collections.Generic;
using UnityEngine;

public class HeroCollection : MonoBehaviour
{
    public static HeroCollection Instance { get; private set; }

    public HashSet<string> ownedHeroes = new();
    public Dictionary<string, int> heroShards = new();

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

    public void UnlockHero(string heroId)
    {
        if (ownedHeroes.Add(heroId))
        {
            Debug.Log($"[HeroCollection] Hero unlocked: {heroId}");
        }
        else
        {
            Debug.Log($"[HeroCollection] Hero {heroId} already owned. Maybe convert to shards here.");
        }
    }

    public void AddShards(string shardId, int amount)
    {
        if (!heroShards.ContainsKey(shardId))
            heroShards[shardId] = 0;

        heroShards[shardId] += amount;
        Debug.Log($"[HeroCollection] +{amount} shards for {shardId} (total {heroShards[shardId]})");
    }

    public bool IsHeroOwned(string heroId)
    {
        return ownedHeroes.Contains(heroId);
    }
}
