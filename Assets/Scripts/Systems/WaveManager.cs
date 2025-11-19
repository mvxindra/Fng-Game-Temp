using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    private StageConfig currentStage;
    private int waveIndex = -1;

    public int TotalWaves => currentStage != null ? currentStage.waves.Count : 0;
    public int CurrentWave => waveIndex + 1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadStage(string stageId)
    {
        currentStage = WaveDatabase.Instance.Get(stageId);
        waveIndex = -1;
    }

    public EnemyParty NextWave()
    {
        if (currentStage == null) return null;

        waveIndex++;
        if (waveIndex >= currentStage.waves.Count)
            return null;

        Wave wave = currentStage.waves[waveIndex];
        return new EnemyParty(new List<string>(wave.enemyParty));
    }

    public bool HasMoreWaves()
    {
        if (currentStage == null) return false;
        return waveIndex + 1 < currentStage.waves.Count;
    }
}
