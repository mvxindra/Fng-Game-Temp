using UnityEngine;
using TMPro;

public class FactionOpsUI : MonoBehaviour
{
    public FactionOperationManager manager;
    public TextMeshProUGUI scoreLabel;

    private void Update()
    {
        if (manager == null || scoreLabel == null) return;
        scoreLabel.text = $"Faction Score: {manager.factionScore}";
    }
}
