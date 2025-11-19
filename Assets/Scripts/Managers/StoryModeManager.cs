using System;
using UnityEditor;
using UnityEngine;

public class StoryModeManager : MonoBehaviour
{
    public ChapterData[] chapters; // Array of all chapters in the story mode

    // Optional singleton accessor for convenience
    public static StoryModeManager Instance { get; private set; }

    private int currentChapterIndex = 0;
    private int currentActIndex = 0;
    private int currentStageIndex = 0;

    // Start the story mode from the first chapter
    public void StartStoryMode()
    {
        // ensure singleton
        if (Instance == null) Instance = this;
        currentChapterIndex = 0;
        StartChapter(currentChapterIndex);
    }

    // Simple static bridge so other systems can report progress without instance lookup
    public static void ReportProgress(int amount)
    {
        var mgr = Instance ?? UnityEngine.Object.FindFirstObjectByType<StoryModeManager>();
        if (mgr == null)
        {
            Debug.LogWarning($"[StoryModeManager] ReportProgress({amount}) called but no StoryModeManager found.");
            return;
        }

        // For now, just log the progress. Real implementation should update mission/chapter progress.
        Debug.Log($"[StoryModeManager] Progress reported: {amount}");
    }

    // Start a specific chapter
    public void StartChapter(int chapterIndex)
    {
        currentChapterIndex = chapterIndex;
        currentActIndex = 0;
        currentStageIndex = 0;

        Debug.Log($"Starting Chapter: {chapters[chapterIndex].chapterName}");
        StartAct(currentActIndex);
    }

    // Start a specific act within the current chapter
    public void StartAct(int actIndex)
    {
        currentActIndex = actIndex;
        currentStageIndex = 0;

        var currentAct = chapters[currentChapterIndex].acts[actIndex];
        Debug.Log($"Starting Act: {currentAct.actName}");
        StartStage(currentStageIndex);
    }

    // Start a specific stage within the current act
    public void StartStage(int stageIndex)
    {
        currentStageIndex = stageIndex;

        var currentStage = chapters[currentChapterIndex].acts[currentActIndex].stages[stageIndex];
        Debug.Log($"Starting Stage: {currentStage.stageName} - Difficulty: {currentStage.difficulty}");
        foreach (var mission in currentStage.battleMissions)
        {
            Debug.Log($"Mission: {mission.description} - Progress: 0/{mission.progressMax}");
        }
    }

    // Mark the current stage as completed and move to the next stage
    public void CompleteStage()
    {
        Debug.Log($"Stage {currentStageIndex + 1} completed!");
        currentStageIndex++;

        var currentAct = chapters[currentChapterIndex].acts[currentActIndex];
        if (currentStageIndex >= currentAct.stages.Length)
        {
            CompleteAct();
        }
        else
        {
            StartStage(currentStageIndex);
        }
    }

    // Mark the current act as completed and move to the next act
    public void CompleteAct()
    {
        Debug.Log($"Act {currentActIndex + 1} completed!");
        currentActIndex++;

        if (currentActIndex >= chapters[currentChapterIndex].acts.Length)
        {
            CompleteChapter();
        }
        else
        {
            StartAct(currentActIndex);
        }
    }

    // Mark the current chapter as completed and move to the next chapter
    public void CompleteChapter()
    {
        Debug.Log($"Chapter {currentChapterIndex + 1} completed!");
        currentChapterIndex++;

        if (currentChapterIndex >= chapters.Length)
        {
            Debug.Log("Story Mode Completed!");
        }
        else
        {
            StartChapter(currentChapterIndex);
        }
    }
}
