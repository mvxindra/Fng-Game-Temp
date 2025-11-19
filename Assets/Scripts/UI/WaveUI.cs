using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    public TextMeshProUGUI label;

    public void UpdateWave(int current, int total)
    {
        if (label == null) return;
        label.text = $"Wave {current}/{total}";
    }
}
