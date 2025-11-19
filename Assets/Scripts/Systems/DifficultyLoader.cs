using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public enum Difficulty
{
    NORMAL,
    HARD,
    NIGHTMARE,
    REVERSE,
    REVERSE_HARD
}

public class DifficultyLoader : MonoBehaviour
{
    public static Difficulty CurrentDifficulty = Difficulty.NORMAL;

    [System.Serializable]
    public class WaveData
    {
        public List<string> enemyParty;
    }

    [System.Serializable]
    public class ActData
    {
        public string actName;
        public List<WaveData> waves;
    }

    [System.Serializable]
    public class ChapterData
    {
        public string chapterName;
        public List<ActData> acts;
    }

    [System.Serializable]
    public class StoryWaves
    {
        public string difficulty;
        public List<ChapterData> chapters;
    }

    public static StoryWaves LoadStoryWaves()
    {
        string fileName = "story_waves.json";
        switch (CurrentDifficulty)
        {
            case Difficulty.HARD:
            case Difficulty.REVERSE_HARD:
                fileName = "story_waves_hard.json";
                break;
            case Difficulty.NIGHTMARE:
                fileName = "story_waves_nightmare.json";
                break;
            case Difficulty.REVERSE:
                fileName = "story_waves.json";
                break;
        }

        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (!File.Exists(path))
        {
            Debug.LogError("Missing wave file: " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        var storyWaves = JsonConvert.DeserializeObject<StoryWaves>(json);

        ApplyDifficultyScaling(storyWaves);
        return storyWaves;
    }

    static void ApplyDifficultyScaling(StoryWaves storyWaves)
    {
        foreach (var chapter in storyWaves.chapters)
        {
            foreach (var act in chapter.acts)
            {
                foreach (var wave in act.waves)
                {
                    for (int i = 0; i < wave.enemyParty.Count; i++)
                    {
                        string baseId = wave.enemyParty[i];
                        switch (CurrentDifficulty)
                        {
                            case Difficulty.HARD:
                            case Difficulty.REVERSE_HARD:
                                wave.enemyParty[i] = baseId + "_ELITE";
                                break;
                            case Difficulty.NIGHTMARE:
                                wave.enemyParty[i] = baseId + "_NM";
                                break;
                        }
                    }
                }
            }
        }
    }

    public static float GetStatMultiplier()
    {
        switch (CurrentDifficulty)
        {
            case Difficulty.HARD:
            case Difficulty.REVERSE_HARD:
                return 1.5f;
            case Difficulty.NIGHTMARE:
                return 2.25f;
            default:
                return 1f;
        }
    }
}
