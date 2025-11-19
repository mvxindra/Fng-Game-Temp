using UnityEngine;

[CreateAssetMenu(fileName = "ChapterData", menuName = "SLA/Chapter")]
public class ChapterData : ScriptableObject
{
    public string chapterName; // e.g., "Hidden Chapter 5"
    public ActData[] acts; // Array of acts within this chapter
    public RewardData clearRewards; // EXP, Gold, etc.
}

[System.Serializable]
public class ActData
{
    public string actName; // e.g., "The Sudden Summons"
    public StageData[] stages; // Array of stages within this act
}

[System.Serializable]
public class StageData
{
    public string stageName; // e.g., "Stage 1"
    public Difficulty difficulty; // Normal, Hard, Reverse, etc.
    public int staminaCost; // Stamina required to play this stage
    public MissionData[] battleMissions; // 10-15 per stage
    public RewardData clearRewards; // EXP, Gold, etc.
}

[System.Serializable]
public class MissionData
{
    public string description; // "Clear 0/10"
    public int progressMax; // Maximum progress required to complete the mission
    public RewardData reward; // Reward for completing the mission
}

[System.Serializable]
public class RewardData
{
    public int experiencePoints; // Amount of experience points rewarded
    public int gold; // Amount of gold rewarded
    public string[] items; // List of item names rewarded
}