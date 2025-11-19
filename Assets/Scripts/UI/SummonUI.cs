using UnityEngine;
using TMPro;

public class SummonUI : MonoBehaviour
{
    public SummonController summonController;
    public TextMeshProUGUI lastResultLabel;

    public void OnSingleSummon()
    {
        if (summonController == null) return;
        string result = summonController.RollOnce(summonController.activeBanner.id);
        if (lastResultLabel != null)
            lastResultLabel.text = $"Last Pull: {result}";
    }

    public void OnTenSummon()
    {
        if (summonController == null) return;
        var results = summonController.RollTen(summonController.activeBanner.id);
        if (lastResultLabel != null)
            lastResultLabel.text = $"10x Pull: {string.Join(", ", results)}";
    }
}
