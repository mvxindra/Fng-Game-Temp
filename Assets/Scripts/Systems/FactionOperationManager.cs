using UnityEngine;
using System.Collections.Generic;

public class FactionOperationManager : MonoBehaviour
{
    public class Node { public string id; public int pve; public int pvp; public int threshold; public bool completed; }
    public List<Node> nodes = new();
    public int factionScore;

    public void Contribute(string nodeId, int amount, bool pvp){
        var n = nodes.Find(x=>x.id==nodeId);
        if(n==null) return;
        if(pvp) n.pvp += amount; else n.pve += amount;
        if(n.pve+n.pvp>=n.threshold && !n.completed){ n.completed=true; factionScore+=n.threshold; /* Mail reward */ }
    }
}
