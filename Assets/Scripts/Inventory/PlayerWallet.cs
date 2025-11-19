using UnityEngine;
using System;

public class PlayerWallet : MonoBehaviour
{
    public int gold;
    public int gems;

    [Header("Stamina Settings")]
    public int currentStamina = 100;
    public int maxStamina = 100;
    public float staminaRegenRate = 1f; // Stamina regenerated per minute

    private DateTime lastStaminaUpdate;

    public static PlayerWallet Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        lastStaminaUpdate = DateTime.Now;
    }

    private void Update()
    {
        RegenerateStamina();
    }

    private void RegenerateStamina()
    {
        if (currentStamina >= maxStamina) return;

        TimeSpan timeSinceLastUpdate = DateTime.Now - lastStaminaUpdate;
        float minutesElapsed = (float)timeSinceLastUpdate.TotalMinutes;

        if (minutesElapsed >= 1f)
        {
            int staminaToAdd = Mathf.FloorToInt(minutesElapsed * staminaRegenRate);
            if (staminaToAdd > 0)
            {
                currentStamina = Mathf.Min(currentStamina + staminaToAdd, maxStamina);
                lastStaminaUpdate = DateTime.Now;
                Debug.Log($"[PlayerWallet] Stamina regenerated: {staminaToAdd} (current: {currentStamina}/{maxStamina})");
            }
        }
    }

    public bool HasStamina(int amount)
    {
        return currentStamina >= amount;
    }

    public bool SpendStamina(int amount)
    {
        if (!HasStamina(amount))
        {
            Debug.LogWarning($"[PlayerWallet] Not enough stamina! Need {amount}, have {currentStamina}");
            return false;
        }

        currentStamina -= amount;
        Debug.Log($"[PlayerWallet] Stamina spent: {amount} (remaining: {currentStamina}/{maxStamina})");
        return true;
    }

    public void AddStamina(int amount)
    {
        currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        Debug.Log($"[PlayerWallet] Stamina restored: {amount} (current: {currentStamina}/{maxStamina})");
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"[PlayerWallet] GOLD +{amount} (total {gold})");
    }

    public bool DeductGold(int amount)
    {
        if (gold < amount)
        {
            Debug.LogWarning($"[PlayerWallet] Not enough gold! Need {amount}, have {gold}");
            return false;
        }

        gold -= amount;
        Debug.Log($"[PlayerWallet] GOLD -{amount} (remaining {gold})");
        return true;
    }

    public void AddGems(int amount)
    {
        gems += amount;
        Debug.Log($"[PlayerWallet] GEMS +{amount} (total {gems})");
    }

    public void AddKey(string keyId, int amount)
    {
        Debug.Log($"[PlayerWallet] KEY {keyId} +{amount}");
        // TODO: store per-key counters if needed
    }
}
