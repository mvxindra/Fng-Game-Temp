using UnityEngine;
using TMPro;

public class ContractUI : MonoBehaviour
{
    public TextMeshProUGUI seasonTitle;
    public TextMeshProUGUI seasonRange;

    public void ShowSeason(SeasonalContract contract)
    {
        if (contract == null) return;
        if (seasonTitle != null) seasonTitle.text = $"Season {contract.id}";
        if (seasonRange != null) seasonRange.text = $"{contract.start} – {contract.end}";
    }
}
