using System.Collections.Generic;
using UnityEngine;

public class PartyManager : MonoBehaviour
{
    public static PartyManager Instance { get; private set; }

    public Party activeParty;        // The party used in combat
    public List<Party> savedParties; // Optional (team presets)

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetActiveParty(Party p)
    {
        activeParty = p;
    }
}
