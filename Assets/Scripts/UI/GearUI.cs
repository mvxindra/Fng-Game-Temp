using UnityEngine;
using TMPro;

public class GearUI : MonoBehaviour
{
    public GearInventory gearInventory;
    public TextMeshProUGUI gearListLabel;

    public void Refresh()
    {
        if (gearInventory == null || gearListLabel == null) return;

        System.Text.StringBuilder sb = new();
        foreach (var g in gearInventory.GetAll())
        {
            sb.AppendLine($"{g.configId} Lvl {g.level} +{g.enhance}");
        }

        gearListLabel.text = sb.ToString();
    }
}
