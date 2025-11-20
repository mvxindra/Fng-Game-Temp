using System;
using System.Collections.Generic;
using UnityEngine;

namespace FngGame.DataModels
{
    /// <summary>
    /// Configuration for rarity-based growth rate modifiers.
    /// Higher rarity units/heroes will have better stat growth rates.
    /// </summary>
    [Serializable]
    public class RarityGrowthConfig
    {
        public int rarityTier;                    // 1-7 (matching GearRarity tiers)
        public float growthMultiplier;            // Overall growth rate multiplier
        public string description;                // Description of this rarity tier

        // Optional: Per-stat growth multipliers for fine-tuning
        public float atkGrowthMultiplier = 1.0f;
        public float defGrowthMultiplier = 1.0f;
        public float hpGrowthMultiplier = 1.0f;
        public float spdGrowthMultiplier = 1.0f;
    }

    /// <summary>
    /// Manager for loading and accessing rarity growth configurations.
    /// </summary>
    public class RarityGrowthManager
    {
        private static RarityGrowthManager _instance;
        public static RarityGrowthManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RarityGrowthManager();
                    _instance.LoadConfigs();
                }
                return _instance;
            }
        }

        private Dictionary<int, RarityGrowthConfig> rarityConfigs = new Dictionary<int, RarityGrowthConfig>();

        /// <summary>
        /// Load rarity growth configurations from JSON file.
        /// </summary>
        public void LoadConfigs()
        {
            rarityConfigs.Clear();

            TextAsset configFile = Resources.Load<TextAsset>("Config/rarity_growth");
            if (configFile != null)
            {
                RarityGrowthConfigList configList = JsonUtility.FromJson<RarityGrowthConfigList>(configFile.text);

                if (configList != null && configList.configs != null)
                {
                    foreach (var config in configList.configs)
                    {
                        rarityConfigs[config.rarityTier] = config;
                    }
                    Debug.Log($"[RarityGrowthManager] Loaded {rarityConfigs.Count} rarity growth configs");
                }
                else
                {
                    Debug.LogWarning("[RarityGrowthManager] Config file is empty or invalid");
                    LoadDefaultConfigs();
                }
            }
            else
            {
                Debug.LogWarning("[RarityGrowthManager] rarity_growth.json not found, using default values");
                LoadDefaultConfigs();
            }
        }

        /// <summary>
        /// Load default rarity growth configurations if config file is missing.
        /// </summary>
        private void LoadDefaultConfigs()
        {
            // Default growth multipliers: Rarity 1 = 1.0x, Rarity 5 = 2.0x, Rarity 7 = 3.0x
            var defaultConfigs = new[]
            {
                new RarityGrowthConfig { rarityTier = 1, growthMultiplier = 1.0f, description = "Common" },
                new RarityGrowthConfig { rarityTier = 2, growthMultiplier = 1.2f, description = "Uncommon" },
                new RarityGrowthConfig { rarityTier = 3, growthMultiplier = 1.5f, description = "Rare" },
                new RarityGrowthConfig { rarityTier = 4, growthMultiplier = 1.8f, description = "Epic" },
                new RarityGrowthConfig { rarityTier = 5, growthMultiplier = 2.0f, description = "Legendary" },
                new RarityGrowthConfig { rarityTier = 6, growthMultiplier = 2.5f, description = "Unique" },
                new RarityGrowthConfig { rarityTier = 7, growthMultiplier = 3.0f, description = "Mythic" }
            };

            foreach (var config in defaultConfigs)
            {
                rarityConfigs[config.rarityTier] = config;
            }

            Debug.Log("[RarityGrowthManager] Loaded default rarity growth configs");
        }

        /// <summary>
        /// Get growth multiplier for a specific rarity tier.
        /// </summary>
        /// <param name="rarityTier">Rarity tier (1-7)</param>
        /// <returns>Growth multiplier (default 1.0 if not found)</returns>
        public float GetGrowthMultiplier(int rarityTier)
        {
            if (rarityConfigs.TryGetValue(rarityTier, out var config))
            {
                return config.growthMultiplier;
            }

            Debug.LogWarning($"[RarityGrowthManager] No config found for rarity tier {rarityTier}, using 1.0x multiplier");
            return 1.0f;
        }

        /// <summary>
        /// Get stat-specific growth multiplier for a rarity tier.
        /// </summary>
        public float GetStatGrowthMultiplier(int rarityTier, string statName)
        {
            if (!rarityConfigs.TryGetValue(rarityTier, out var config))
            {
                return 1.0f;
            }

            float baseMultiplier = config.growthMultiplier;
            float statMultiplier = 1.0f;

            switch (statName.ToLower())
            {
                case "atk":
                case "attack":
                    statMultiplier = config.atkGrowthMultiplier;
                    break;
                case "def":
                case "defense":
                    statMultiplier = config.defGrowthMultiplier;
                    break;
                case "hp":
                case "health":
                    statMultiplier = config.hpGrowthMultiplier;
                    break;
                case "spd":
                case "speed":
                    statMultiplier = config.spdGrowthMultiplier;
                    break;
            }

            return baseMultiplier * statMultiplier;
        }

        /// <summary>
        /// Get the full config for a rarity tier.
        /// </summary>
        public RarityGrowthConfig GetConfig(int rarityTier)
        {
            if (rarityConfigs.TryGetValue(rarityTier, out var config))
            {
                return config;
            }
            return null;
        }

        /// <summary>
        /// Apply rarity-based growth modifier to a stat value.
        /// </summary>
        /// <param name="baseGrowth">Base growth rate from hero/enemy config</param>
        /// <param name="rarityTier">Rarity tier (1-7)</param>
        /// <param name="statName">Optional stat name for stat-specific multipliers</param>
        /// <returns>Modified growth rate</returns>
        public float ApplyRarityGrowth(float baseGrowth, int rarityTier, string statName = null)
        {
            float multiplier = string.IsNullOrEmpty(statName)
                ? GetGrowthMultiplier(rarityTier)
                : GetStatGrowthMultiplier(rarityTier, statName);

            return baseGrowth * multiplier;
        }
    }

    /// <summary>
    /// Wrapper class for JSON deserialization.
    /// </summary>
    [Serializable]
    public class RarityGrowthConfigList
    {
        public List<RarityGrowthConfig> configs;
    }
}
