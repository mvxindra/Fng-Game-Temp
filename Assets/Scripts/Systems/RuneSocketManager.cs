using UnityEngine;

using System.Collections.Generic;

using System.Linq;

using FnGMafia.Core;

 

/// <summary>

/// Manages rune and socket system

/// </summary>

public class RuneSocketManager : Singleton<RuneSocketManager>

{

    private Dictionary<string, RuneConfig> runeConfigs;

    private Dictionary<string, LegendaryRuneConfig> legendaryRunes;

    private Dictionary<string, SocketConfig> socketConfigs;

    private Dictionary<string, RuneCraftingRecipe> craftingRecipes;

    private Dictionary<string, RuneUpgradeRecipe> upgradeRecipes;

 

    // Player's rune inventory

    private Dictionary<string, int> runeInventory;

 

    protected override void Awake()

    {

        base.Awake();

        runeConfigs = new Dictionary<string, RuneConfig>();

        legendaryRunes = new Dictionary<string, LegendaryRuneConfig>();

        socketConfigs = new Dictionary<string, SocketConfig>();

        craftingRecipes = new Dictionary<string, RuneCraftingRecipe>();

        upgradeRecipes = new Dictionary<string, RuneUpgradeRecipe>();

        runeInventory = new Dictionary<string, int>();

        LoadConfigs();

    }

 

    private void LoadConfigs()

    {

        TextAsset configFile = Resources.Load<TextAsset>("Config/runes_sockets");

        if (configFile != null)

        {

            GearSocketDatabase db = JsonUtility.FromJson<GearSocketDatabase>(configFile.text);

 

            foreach (var socket in db.socketTypes)

            {

                socketConfigs[socket.socketType] = socket;

            }

 

            foreach (var rune in db.runes)

            {

                runeConfigs[rune.runeId] = rune;

            }

 

            foreach (var legendary in db.legendaryRunes)

            {

                legendaryRunes[legendary.runeId] = legendary;

            }

 

            foreach (var recipe in db.craftingRecipes)

            {

                craftingRecipes[recipe.recipeId] = recipe;

            }

 

            foreach (var upgrade in db.upgradeRecipes)

            {

                upgradeRecipes[upgrade.baseRuneId] = upgrade;

            }

 

            Debug.Log($"Loaded {runeConfigs.Count} runes, {legendaryRunes.Count} legendary runes, {socketConfigs.Count} socket types");

        }

        else

        {

            Debug.LogWarning("runes_sockets.json not found");

        }

    }

 

    /// <summary>

    /// Socket a rune into gear

    /// </summary>

    public bool SocketRune(EnhancedGearInstance gear, int socketIndex, string runeId)

    {

        if (gear == null || socketIndex < 0 || socketIndex >= gear.sockets.Count)

        {

            Debug.LogWarning("Invalid gear or socket index");

            return false;

        }

 

        SocketInstance socket = gear.sockets[socketIndex];

 

        // Check if socket is unlocked

        if (!socket.isUnlocked)

        {

            Debug.LogWarning($"Socket {socketIndex} is not unlocked");

            return false;

        }

 

        // Check if socket is already occupied

        if (!string.IsNullOrEmpty(socket.socketedRuneId))

        {

            Debug.LogWarning($"Socket {socketIndex} already has a rune");

            return false;

        }

 

        // Check if player has the rune

        if (!HasRune(runeId, 1))

        {

            Debug.LogWarning($"Player doesn't have rune {runeId}");

            return false;

        }

 

        // Check if rune is compatible with socket

        RuneConfig runeConfig = GetRuneConfig(runeId);

        if (runeConfig == null)

        {

            Debug.LogWarning($"Rune config {runeId} not found");

            return false;

        }

 

        if (!IsRuneCompatible(runeConfig, socket.socketType))

        {

            Debug.LogWarning($"Rune {runeId} not compatible with {socket.socketType} socket");

            return false;

        }

 

        // Socket the rune

        socket.socketedRuneId = runeId;

        RemoveRune(runeId, 1);

 

        Debug.Log($"Socketed rune {runeId} into socket {socketIndex}");

        return true;

    }

 

    /// <summary>

    /// Unsocket a rune from gear

    /// </summary>

    public bool UnsocketRune(EnhancedGearInstance gear, int socketIndex, bool destroyRune = false)

    {

        if (gear == null || socketIndex < 0 || socketIndex >= gear.sockets.Count)

        {

            return false;

        }

 

        SocketInstance socket = gear.sockets[socketIndex];

 

        if (string.IsNullOrEmpty(socket.socketedRuneId))

        {

            Debug.LogWarning("Socket is empty");

            return false;

        }

 

        string runeId = socket.socketedRuneId;

        socket.socketedRuneId = null;

 

        if (!destroyRune)

        {

            AddRune(runeId, 1);

        }

 

        Debug.Log($"Unsocketed rune {runeId} from socket {socketIndex}");

        return true;

    }

 

    /// <summary>

    /// Unlock a socket on gear

    /// </summary>

    public bool UnlockSocket(EnhancedGearInstance gear, int socketIndex, List<MaterialRequirement> materials)

    {

        if (gear == null || socketIndex < 0 || socketIndex >= gear.sockets.Count)

        {

            return false;

        }

 

        SocketInstance socket = gear.sockets[socketIndex];

 

        if (socket.isUnlocked)

        {

            Debug.LogWarning("Socket is already unlocked");

            return false;

        }

 

        // Check materials (would integrate with MaterialInventory)

        // For now, just unlock

        socket.isUnlocked = true;

        gear.unlockedSocketCount++;

 

        Debug.Log($"Unlocked socket {socketIndex} on gear {gear.instanceId}");

        return true;

    }

 

    /// <summary>

    /// Check if rune is compatible with socket type

    /// </summary>

    private bool IsRuneCompatible(RuneConfig rune, string socketType)

    {

        if (rune.compatibleSockets == null || rune.compatibleSockets.Count == 0)

        {

            return true; // Compatible with all if not specified

        }

 

        if (socketType == "prismatic")

        {

            return true; // Prismatic sockets accept any rune

        }

 

        return rune.compatibleSockets.Contains(socketType);

    }

 

    /// <summary>

    /// Craft a rune from materials

    /// </summary>

    public bool CraftRune(string recipeId)

    {

        if (!craftingRecipes.ContainsKey(recipeId))

        {

            Debug.LogWarning($"Recipe {recipeId} not found");

            return false;

        }

 

        RuneCraftingRecipe recipe = craftingRecipes[recipeId];

 

        // Check materials (integrate with MaterialInventory)

        // For now, just create the rune

 

        AddRune(recipe.resultRuneId, recipe.resultQuantity);

 

        Debug.Log($"Crafted {recipe.resultQuantity}x {recipe.resultRuneId}");

        return true;

    }

 

    /// <summary>

    /// Upgrade a rune to higher tier

    /// </summary>

    public bool UpgradeRune(string baseRuneId)

    {

        if (!upgradeRecipes.ContainsKey(baseRuneId))

        {

            Debug.LogWarning($"No upgrade recipe for {baseRuneId}");

            return false;

        }

 

        RuneUpgradeRecipe recipe = upgradeRecipes[baseRuneId];

 

        // Check if player has required runes

        if (!HasRune(baseRuneId, recipe.requiredCount))

        {

            Debug.LogWarning($"Need {recipe.requiredCount}x {baseRuneId} to upgrade");

            return false;

        }

 

        // Roll for success

        float roll = Random.Range(0f, 1f);

        if (roll > recipe.successChance)

        {

            Debug.Log("Upgrade failed!");

            RemoveRune(baseRuneId, recipe.requiredCount);

            return false;

        }

 

        // Success!

        RemoveRune(baseRuneId, recipe.requiredCount);

        AddRune(recipe.resultRuneId, 1);

 

        Debug.Log($"Successfully upgraded {baseRuneId} to {recipe.resultRuneId}!");

        return true;

    }

 

    /// <summary>

    /// Calculate total stat bonuses from all socketed runes

    /// </summary>

    public RuneStatBonus CalculateRuneBonuses(EnhancedGearInstance gear)

    {

        RuneStatBonus totalBonus = new RuneStatBonus();

 

        foreach (var socket in gear.sockets)

        {

            if (string.IsNullOrEmpty(socket.socketedRuneId)) continue;

 

            RuneConfig runeConfig = GetRuneConfig(socket.socketedRuneId);

            if (runeConfig != null && runeConfig.stats != null)

            {

                AddRuneStats(totalBonus, runeConfig.stats);

            }

        }

 

        return totalBonus;

    }

 

    private void AddRuneStats(RuneStatBonus total, RuneStatBonus add)

    {

        total.flatAtk += add.flatAtk;

        total.flatDef += add.flatDef;

        total.flatHp += add.flatHp;

        total.flatSpd += add.flatSpd;

 

        total.percentAtk += add.percentAtk;

        total.percentDef += add.percentDef;

        total.percentHp += add.percentHp;

        total.percentSpd += add.percentSpd;

 

        total.critChance += add.critChance;

        total.critDamage += add.critDamage;

        total.lifeSteal += add.lifeSteal;

        total.penetration += add.penetration;

 

        total.elementalDamage += add.elementalDamage;

        total.elementalResist += add.elementalResist;

    }

 

    // Rune inventory management

    public void AddRune(string runeId, int count)

    {

        if (!runeInventory.ContainsKey(runeId))

        {

            runeInventory[runeId] = 0;

        }

        runeInventory[runeId] += count;

        Debug.Log($"Added {count}x {runeId}. Total: {runeInventory[runeId]}");

    }

 

    public bool RemoveRune(string runeId, int count)

    {

        if (!HasRune(runeId, count))

        {

            return false;

        }

        runeInventory[runeId] -= count;

        return true;

    }

 

    public bool HasRune(string runeId, int count)

    {

        return runeInventory.ContainsKey(runeId) && runeInventory[runeId] >= count;

    }

 

    public int GetRuneCount(string runeId)

    {

        return runeInventory.ContainsKey(runeId) ? runeInventory[runeId] : 0;

    }

 

    // Utility methods

    public RuneConfig GetRuneConfig(string runeId)

    {

        return runeConfigs.ContainsKey(runeId) ? runeConfigs[runeId] : null;

    }

 

    public LegendaryRuneConfig GetLegendaryRuneConfig(string runeId)

    {

        return legendaryRunes.ContainsKey(runeId) ? legendaryRunes[runeId] : null;

    }

 

    public SocketConfig GetSocketConfig(string socketType)

    {

        return socketConfigs.ContainsKey(socketType) ? socketConfigs[socketType] : null;

    }

}