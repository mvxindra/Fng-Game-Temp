using UnityEngine;

public class StatusEffectInstance
{
    public StatusConfig Config { get; private set; }
    public int RemainingTurns { get; private set; }
    public bool IsExpired => RemainingTurns <= 0;

    public StatusEffectInstance(StatusConfig config, int duration)
    {
        Config = config;
        RemainingTurns = duration;
    }

    public void Tick()
    {
        RemainingTurns--;
    }

    public float GetStatMultiplier()
    {
        return 1f + Config.percentDelta;
    }

    public int GetFlatDelta()
    {
        return Mathf.RoundToInt(Config.flatDelta);
    }
}
