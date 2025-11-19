using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    private List<CombatUnit> turnOrder = new();
    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeTurns(List<CombatUnit> allies, List<CombatUnit> enemies)
    {
        turnOrder.Clear();
        turnOrder.AddRange(allies);
        turnOrder.AddRange(enemies);

        turnOrder.Sort((a, b) => b.BaseSPD.CompareTo(a.BaseSPD));

        currentIndex = 0;
    }

    public CombatUnit GetCurrentUnit()
    {
        if (turnOrder.Count == 0) return null;
        return turnOrder[currentIndex];
    }

    public void NextTurn()
    {
        if (turnOrder.Count == 0) return;

        currentIndex++;
        if (currentIndex >= turnOrder.Count)
            currentIndex = 0;
    }

    public void RemoveDead()
    {
        turnOrder.RemoveAll(u => u.CurrentHP <= 0);

        if (currentIndex >= turnOrder.Count)
            currentIndex = 0;
    }
}
