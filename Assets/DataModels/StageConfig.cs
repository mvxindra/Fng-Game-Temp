using System;
using System.Collections.Generic;

[Serializable]
public class StageConfig
{
    public string stageId;            // e.g. "STAGE_1_1"
    public string difficulty;         // "Normal", "Hard", "Nightmare"
    public bool isBossStage;          // Boss = always 3 waves
    public List<Wave> waves;          // Wave list
}

[Serializable]
public class StageWrapper
{
    public List<StageConfig> stages;
}
