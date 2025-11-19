using System.Collections.Generic;
using UnityEngine;

public class WaveDatabase : MonoBehaviour
{
    public static WaveDatabase Instance { get; private set; }
    private Dictionary<string, StageConfig> stages = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    private void Load()
    {
        LoadWaveFile("Config/waves");         // dungeon waves
        LoadWaveFile("Config/story_waves");   // story mode waves
    }

    private void LoadWaveFile(string resourcePath)
    {
        var json = Resources.Load<TextAsset>(resourcePath);
        if (json == null)
        {
            Debug.LogWarning("[WaveDatabase] Missing file: " + resourcePath);
            return;
        }

        try
        {
            // Remove JSON comments before parsing (JsonUtility doesn't support them)
            string cleanJson = RemoveJsonComments(json.text);

            // Wrap array in object for JsonUtility
            string wrapped = "{\"stages\":" + cleanJson + "}";
            StageWrapper wrapper = JsonUtility.FromJson<StageWrapper>(wrapped);

            if (wrapper != null && wrapper.stages != null)
            {
                foreach (var s in wrapper.stages)
                {
                    stages[s.stageId] = s;
                }
            }

            Debug.Log($"[WaveDatabase] Loaded {wrapper.stages.Count} from {resourcePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[WaveDatabase] Failed to parse {resourcePath}: {ex.Message}");
        }
    }

    private string RemoveJsonComments(string json)
    {
        // Remove single-line comments (// ...)
        var lines = json.Split('\n');
        var result = new System.Text.StringBuilder();
        
        foreach (var line in lines)
        {
            int commentIndex = line.IndexOf("//");
            if (commentIndex >= 0)
            {
                result.AppendLine(line.Substring(0, commentIndex));
            }
            else
            {
                result.AppendLine(line);
            }
        }
        
        return result.ToString();
    }

    public StageConfig Get(string id)
    {
        if (stages.TryGetValue(id, out var s)) return s;
        Debug.LogWarning("[WaveDatabase] Missing stage config: " + id);
        return null;
    }
}


