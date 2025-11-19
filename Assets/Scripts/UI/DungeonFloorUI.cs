using System.Collections.Generic;
using UnityEngine;

public class DungeonFloorUI : MonoBehaviour
{
    public GameObject floorButtonPrefab;
    public Transform contentRoot;

    public void Populate(DungeonConfig dungeon)
    {
        if (dungeon == null || floorButtonPrefab == null || contentRoot == null) return;

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var floor in dungeon.floors)
        {
            var go = Instantiate(floorButtonPrefab, contentRoot);
            go.name = $"{dungeon.id}_{floor.id}";
            // Add script to set label text & onClick to start floor battle.
        }
    }
}
