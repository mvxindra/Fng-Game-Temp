using System.Collections.Generic;
using UnityEngine;

public class SkillDatabase : MonoBehaviour
{
    public static SkillDatabase Instance { get; private set; }
    private Dictionary<string, SkillConfig> skills = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        var json = Resources.Load<TextAsset>("Config/skill_config");
        if (json == null)
        {
            Debug.LogError("[SkillDatabase] skill_config.json missing!");
            return;
        }

        // Parse as JSON array
        string wrapped = "{\"skills\":" + json.text + "}";
        SkillConfigList wrapper = JsonUtility.FromJson<SkillConfigList>(wrapped);
        
        skills.Clear();
        foreach (var s in wrapper.skills)
        {
            skills[s.id] = s;
        }
        
        Debug.Log($"[SkillDatabase] Loaded {skills.Count} skills.");
    }

    public SkillConfig GetSkill(string id)
    {
        if (skills.TryGetValue(id, out var s)) return s;
        Debug.LogWarning("[SkillDatabase] Missing skill: " + id);
        return null;
    }
}