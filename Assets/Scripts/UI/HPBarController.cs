using UnityEngine;
using UnityEngine.UI;

public class HPBarController : MonoBehaviour
{
    public CombatUnit unit;
    public Image fill;

    private void Update()
    {
        if (unit == null || fill == null) return;
        if (unit.BaseHP <= 0) return;

        fill.fillAmount = Mathf.Clamp01((float)unit.CurrentHP / unit.BaseHP);
    }
}
