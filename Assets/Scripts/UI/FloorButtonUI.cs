using TMPro;
using UnityEngine;

public class FloorButtonUI : MonoBehaviour
{
    public TMP_Text label;
    public string floorId;

    public void OnClick()
    {
        // Select the floor
        DungeonManager.Instance.SelectFloor(floorId);

        // Load enemy party for this floor
        var enemyParty = DungeonLoader.Instance.LoadCurrentFloorParty();

        // Load player party from PartyManager and start the battle
        var playerParty = PartyManager.Instance.activeParty;

        // Assign enemy party and start battle using the existing API
        if (BattleController.Instance != null)
        {
            BattleController.Instance.currentEnemyParty = enemyParty;
            BattleController.Instance.StartBattle();
        }
    }
}
