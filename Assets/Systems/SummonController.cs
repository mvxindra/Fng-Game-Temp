using System;
using System.Collections.Generic;
using UnityEngine;

public class SummonController : MonoBehaviour
{
    [Serializable] public class Rate { public int rarity; public int weight; public List<string> featuredPool; }
    [Serializable] public class Banner { public string id; public int cost; public bool pityEnabled; public int pityCount; public int pityRarity; public List<Rate> rates; }

    public Banner activeBanner; // Load from config
    Dictionary<string,int> pityCounters = new(); // per banner id

    System.Random rng = new System.Random();

    public string RollOnce(string bannerId){
        var b = activeBanner; // swap for lookup by id
        if(!pityCounters.ContainsKey(bannerId)) pityCounters[bannerId]=0;
        pityCounters[bannerId]++;

        if(b.pityEnabled && pityCounters[bannerId]>=b.pityCount){
            pityCounters[bannerId]=0; // grant pity
            return PullFromRarity(b.pityRarity, b);
        }
        var bucket = new List<(int rarity,int cumulative)>();
        int sum=0; foreach(var r in b.rates){ sum+=r.weight; bucket.Add((r.rarity, sum)); }
        int roll = rng.Next(0,sum);
        foreach(var e in bucket){ if(roll<e.cumulative) return PullFromRarity(e.rarity, b); }
        return PullFromRarity(b.rates[^1].rarity, b);
    }

    public List<string> RollTen(string bannerId)
    {
        var results = new List<string>();
        for (int i = 0; i < 10; i++)
        {
            results.Add(RollOnce(bannerId));
        }
        return results;
    }

    string PullFromRarity(int rarity, Banner b){
        var pool = new List<string>();
        foreach(var r in b.rates) if(r.rarity==rarity){
            if(r.featuredPool!=null && r.featuredPool.Count>0) pool.AddRange(r.featuredPool);
            // else fallback to your global rarity pool from DB
        }
        if(pool.Count==0) pool.Add($"R{rarity}_GENERIC");
        return pool[rng.Next(pool.Count)];
    }
}
