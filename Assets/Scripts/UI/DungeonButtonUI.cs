using TMPro;
using UnityEngine;

public class DungeonButtonUI : MonoBehaviour
{
    public TMP_Text label;
    public string dungeonId;

    public void OnClick()
    {
        // Select the dungeon
        DungeonManager.Instance.SelectDungeon(dungeonId);

        // Open the floor selection UI (handle inactive objects too)
        var floors = Resources.FindObjectsOfTypeAll<DungeonFloorUI>();
        if (floors != null && floors.Length > 0)
        {
            floors[0].gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[DungeonButtonUI] DungeonFloorUI not found in scene.");
        }
    }
}
