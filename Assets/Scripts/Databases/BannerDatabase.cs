using System.Collections.Generic;
using UnityEngine;

public class BannerDatabase : MonoBehaviour
{
    public static BannerDatabase Instance { get; private set; }
    private Dictionary<string, GachaBanner> banners = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        TextAsset json = Resources.Load<TextAsset>("Config/gacha_banner");
        if (json == null)
        {
            Debug.LogError("[BannerDatabase] gacha_banner.json missing!");
            return;
        }

        try
        {
            // Wrap array in object for JsonUtility
            string wrapped = "{\"banners\":" + json.text + "}";
            BannerList wrapper = JsonUtility.FromJson<BannerList>(wrapped);
            
            if (wrapper != null && wrapper.banners != null)
            {
                foreach (var b in wrapper.banners)
                {
                    banners[b.id] = b;
                }
            }
            
            Debug.Log($"[BannerDatabase] Loaded {banners.Count} banners.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BannerDatabase] Failed to parse gacha_banner.json: {ex.Message}");
            Debug.LogError($"[BannerDatabase] JSON content: {json.text}");
        }
    }

    public GachaBanner Get(string id)
    {
        if (banners.TryGetValue(id, out var b)) return b;
        Debug.LogWarning("[BannerDatabase] Missing banner: " + id);
        return null;
    }
}

