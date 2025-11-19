using System.Collections.Generic;
using UnityEngine;

public class GearInventory : MonoBehaviour
{
    private List<GearInstance> gears = new();

    public void AddGearById(string gearId, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            var inst = new GearInstance
            {
                instanceId = System.Guid.NewGuid().ToString(),
                configId = gearId,
                level = 1,
                enhance = 0,
                passiveLevel = 1
            };
            gears.Add(inst);
            Debug.Log($"[GearInventory] Added gear {gearId} (instance {inst.instanceId})");
        }
    }

    public IEnumerable<GearInstance> GetAll() => gears;
}
