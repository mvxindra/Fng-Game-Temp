using UnityEngine;
using UnityEngine.UI;

public class SkillButtonPanel : MonoBehaviour
{
    public Button[] skillButtons;

    public void ShowSkills(CombatUnit unit)
    {
        gameObject.SetActive(true);

        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i >= unit.Skills.Count)
            {
                skillButtons[i].gameObject.SetActive(false);
                continue;
            }

            var s = unit.Skills[i];
            skillButtons[i].gameObject.SetActive(true);

            // Set button label
            skillButtons[i].GetComponentInChildren<Text>().text =
                $"{s.Config.name}\nCD:{s.CurrentCooldown}";

            skillButtons[i].interactable = s.IsReady;

            int index = i;
            skillButtons[i].onClick.RemoveAllListeners();
            skillButtons[i].onClick.AddListener(() =>
            {
                BattleController.Instance.UseSkill(unit, s);
            });
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
