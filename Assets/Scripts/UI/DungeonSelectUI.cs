using System.Collections.Generic;
using UnityEngine;

public class DungeonSelectUI : MonoBehaviour
{
    public GameObject dungeonButtonPrefab;
    public Transform contentRoot;
    public DungeonDatabase dungeonDb;

    private void Start()
    {
        if (dungeonDb == null) dungeonDb = DungeonDatabase.Instance;
    }

    public void Populate(List<string> dungeonIds)
    {
        if (contentRoot == null || dungeonButtonPrefab == null || dungeonDb == null) return;

        foreach (Transform child in contentRoot)
            Destroy(child.gameObject);

        foreach (var id in dungeonIds)
        {
            var cfg = dungeonDb.Get(id);
            if (cfg == null) continue;

            var go = Instantiate(dungeonButtonPrefab, contentRoot);
            go.name = cfg.id;
            // You can add a component here to set label text & onClick.
        }
    }
}
